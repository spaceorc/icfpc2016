using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public static class SolutionPacker
	{
		public static SolutionSpec Pack(this SolutionSpec source)
		{
			return source.RemoveBadFacetsVertices().PackFacetNumbers();
		}

		private class Node
		{
			public Facet ResultFacet;
			public bool Rotated;
			public Node MergedInto;

			public Node GetMergedInto()
			{
				return MergedInto != null ? MergedInto.GetMergedInto() : this;
			}

			public readonly List<Facet> Siblings = new List<Facet>();

			public override string ToString()
			{
				return $"ResultFacet: {ResultFacet}, Rotated: {Rotated}, MergedInto: {MergedInto}, Siblings: {string.Join("|", Siblings)}";
			}
		}

		public static SolutionSpec Normalize(this SolutionSpec source)
		{
			return source.DoNormalize().RemoveBadFacetsVertices();
		}

		private static SolutionSpec DoNormalize(this SolutionSpec source)
		{
			var edgeToFacets = new Dictionary<Tuple<int,int>, List<Facet>>();
			foreach (var facet in source.Facets)
			{
				for (int i = 0; i < facet.Vertices.Length; i++)
				{
					var vertex = facet.Vertices[i];
					var nextVertex = facet.Vertices[(i + 1)% facet.Vertices.Length];
					var edge = Tuple.Create(vertex < nextVertex ? vertex : nextVertex, vertex < nextVertex ? nextVertex : vertex);
					List<Facet> list;
					if (!edgeToFacets.TryGetValue(edge, out list))
					{
						list = new List<Facet>();
						edgeToFacets.Add(edge, list);
					}
					list.Add(facet);
				}
			}
			var graph = new Dictionary<Facet, Node>();
			foreach (var kvp in edgeToFacets)
			{
				var facets = kvp.Value;
				foreach (var facet in facets)
				{
					Node node;
					if (!graph.TryGetValue(facet, out node))
					{
						var srcPoly = new Polygon(facet.Vertices.Select(v => source.SourcePoints[v]).ToArray());
						var dstPoly = new Polygon(facet.Vertices.Select(v => source.DestPoints[v]).ToArray());
						var positiveOnSource = srcPoly.GetSignedSquare() > 0;
						var positiveOnDest = dstPoly.GetSignedSquare() > 0;
						node = new Node {ResultFacet = facet, Rotated = positiveOnSource != positiveOnDest};
						graph.Add(facet, node);
					}
					node.Siblings.AddRange(facets.Where(f => f != facet));
				}
			}
			foreach (var kvp in graph)
			{
				var node = kvp.Value;
				if (node.MergedInto != null)
					continue;

				foreach (var siblingKey in node.Siblings.ToArray())
				{
					var siblingNode = graph[siblingKey].GetMergedInto();
					if (siblingNode == node)
						continue;

					if (node.Rotated == siblingNode.Rotated)
					{
						var newResultFacet = JoinFacets(node.ResultFacet, siblingNode.ResultFacet);
						if (newResultFacet != null)
						{
							siblingNode.MergedInto = node;
							node.Siblings.AddRange(siblingNode.Siblings);
							node.ResultFacet = newResultFacet;
						}
					}
				}
			}

			return new SolutionSpec(source.SourcePoints, graph.Values.Where(x => x.MergedInto == null).Select(n => n.ResultFacet).ToArray(), source.DestPoints);
		}

		public static Facet JoinFacets(Facet f1, Facet f2)
		{
			var f1Map = f1.Vertices.Select((v, i) => new { v, i }).ToDictionary(x => x.v, x => x.i);
			var f2Vertices = f2.Vertices.ToList();
			var f2Map = f2Vertices.Select((v, i) => new { v, i }).ToDictionary(x => x.v, x => x.i);
			var commonVertices1 = new List<bool>();
			var commonVertices2 = new List<bool>();
			var resultVertices = new List<int>();
			var commonCount1 = 0;
			var commonCount2 = 0;
			int[] commonOrder2 = null;
			bool hasCommonToUncommon = false;
			for (var i = 0; i < f1.Vertices.Length; i++)
			{
				var isCommon = f2Map.ContainsKey(f1.Vertices[i]);
				var nextIsCommon = f2Map.ContainsKey(f1.Vertices[(i + 1) % f1.Vertices.Length]);
				if (isCommon)
					commonCount1++;
				commonVertices1.Add(isCommon);
				if (isCommon && !nextIsCommon)
				{
					if (hasCommonToUncommon)
						return null;
					hasCommonToUncommon = true;
				}
			}

			hasCommonToUncommon = false;
			for (var i = 0; i < f2Vertices.Count; i++)
			{
				var isCommon = f1Map.ContainsKey(f2Vertices[i]);
				var nextIsCommon = f1Map.ContainsKey(f2Vertices[(i + 1)% f2Vertices.Count]);
				if (isCommon)
					commonCount2++;
				commonVertices2.Add(isCommon);
				if (isCommon && nextIsCommon)
					commonOrder2 = new[] { f2Vertices[i], f2Vertices[(i + 1) % f2Vertices.Count] };
				if (isCommon && !nextIsCommon)
				{
					if (hasCommonToUncommon)
						return null;
					hasCommonToUncommon = true;
				}
			}

			if (commonCount2 == f2Vertices.Count)
			{
				var skip = commonVertices1[0];
				for (var i = 0; i < f1.Vertices.Length; i++)
				{
					if (!commonVertices1[(i + 1)%f1.Vertices.Length])
						skip = false;
					if (!skip)
					{
						resultVertices.Add(f1.Vertices[i]);
						if (commonVertices1[(i + 1) % f1.Vertices.Length])
						{
							resultVertices.Add(f1.Vertices[(i + 1) % f1.Vertices.Length]);
							skip = true;
						}
					}
				}
				return new Facet(resultVertices.ToArray());
			}

			if (f1.Vertices[(f1Map[commonOrder2[0]] + 1)%f1.Vertices.Length] == f1.Vertices[f1Map[commonOrder2[1]]])
			{
				f2Vertices.Reverse();
				commonVertices2.Reverse();
				f2Map = f2Vertices.Select((v, i) => new { v, i }).ToDictionary(x => x.v, x => x.i);
			}

			if (commonCount1 == f1.Vertices.Length)
			{
				var skip = commonVertices2[0];
				for (var i = 0; i < f2Vertices.Count; i++)
				{
					if (!commonVertices2[(i + 1) % f2Vertices.Count])
						skip = false;
					if (!skip)
					{
						resultVertices.Add(f2Vertices[i]);
						if (commonVertices2[(i + 1) % f2Vertices.Count])
						{
							resultVertices.Add(f2Vertices[(i + 1) % f2Vertices.Count]);
							skip = true;
						}
					}
				}
				return new Facet(resultVertices.ToArray());
			}


			var start1 = -1;
			for (var i = 0; i < f1.Vertices.Length; i++)
			{
				if (!f2Map.ContainsKey(f1.Vertices[i]))
				{
					start1 = i;
					break;
				}
			}

			int start2 = -1;
			var used = new HashSet<int>();
			for (var i = start1; i < start1 + f1.Vertices.Length; i++)
			{
				var vertex = f1.Vertices[i % f1.Vertices.Length];
				if (!used.Add(vertex))
					throw new InvalidOperationException($"!used.Add({vertex})");
				resultVertices.Add(vertex);
				int s2;
				if (f2Map.TryGetValue(vertex, out s2))
				{
					start2 = s2 + 1;
					break;
				}
			}
			if (start2 < 0)
				throw new InvalidOperationException("start2 < 0");
			int restart1 = -1;
			for (var i = start2; i < start2 + f2Vertices.Count; i++)
			{
				var vertex = f2Vertices[i % f2Vertices.Count];
				if (!used.Add(vertex))
					throw new InvalidOperationException($"!used.Add({vertex})");
				resultVertices.Add(vertex);
				int s1;
				if (f1Map.TryGetValue(vertex, out s1))
				{
					restart1 = s1 + 1;
					break;
				}
			}
			if (restart1 < 0)
				throw new InvalidOperationException("restart1 < 0");
			for (var i = restart1; i < start1 + f1.Vertices.Length; i++)
			{
				var vertex = f1.Vertices[i % f1.Vertices.Length];
				if (!used.Add(vertex))
					break;
				resultVertices.Add(vertex);
			}
			return new Facet(resultVertices.ToArray());
		}

		public static SolutionSpec RemoveBadFacetsVertices(this SolutionSpec source)
		{
			var sourcePoints = source.SourcePoints.ToList();
			var destPoints = source.DestPoints.ToList();
			var facets = source.Facets.ToList();
			var sourcePointsMap = sourcePoints.Select((p, i) => new { p, i }).ToDictionary(x => x.p, x => x.i);
			var usedVertices = new HashSet<int>();
			for (var i = 0; i < facets.Count; i++)
			{
				var polygon = new Polygon(facets[i].Vertices.Select(v => sourcePoints[v]).ToArray());
				var convexBoundary = polygon.RemoveExtraVertices();
				var newVertices = convexBoundary.Vertices.Select(v => sourcePointsMap[v]).ToArray();
				facets[i] = new Facet(newVertices);
				usedVertices.UnionWith(newVertices);
			}
			var newIndexesMap = new Dictionary<int, int>();
			for (var origIndex = 0; origIndex < sourcePoints.Count; origIndex++)
			{
				if (usedVertices.Contains(origIndex))
					newIndexesMap[origIndex] = newIndexesMap.Count;
			}
			for (var i = sourcePoints.Count - 1; i >= 0; i--)
			{
				if (!usedVertices.Contains(i))
				{
					sourcePoints.RemoveAt(i);
					destPoints.RemoveAt(i);
				}
			}
			foreach (var facet in facets)
			{
				for (int i = 0; i < facet.Vertices.Length; i++)
					facet.Vertices[i] = newIndexesMap[facet.Vertices[i]];
			}
			
			return new SolutionSpec(sourcePoints.ToArray(), facets.ToArray(), destPoints.ToArray());
		}

		public static SolutionSpec PackFacetNumbers(this SolutionSpec source)
		{
			var rates = new Dictionary<int, int>();
			foreach (var facet in source.Facets)
			{
				foreach (var vertex in facet.Vertices)
				{
					int rate;
					rates.TryGetValue(vertex, out rate);
					rates[vertex] = rate + 1;
				}
			}
			var orderedVertexes = rates.Select(r => new { vertex = r.Key, rate = r.Value }).OrderByDescending(r => r.rate).Select(r => r.vertex).ToList();
			var vertexesMap = orderedVertexes.Select((v, i) => new { oldVertex = v, newVertex = i }).ToDictionary(x => x.oldVertex, x => x.newVertex);

			var sourcePoints = orderedVertexes.Select(v => source.SourcePoints[v]).ToArray();
			var facets = source.Facets.Select(f => new Facet(f.Vertices.Select(v => vertexesMap[v]).ToArray())).ToArray();
			var destPoints = orderedVertexes.Select(v => source.DestPoints[v]).ToArray();
			return new SolutionSpec(sourcePoints, facets, destPoints);
		}
	}

	[TestFixture]
	public class SolutionPacker_Should
	{
		[Test]
		public void PackFacetNumbers_EmptySolution()
		{
			var origSolution = SolutionSpec.CreateTrivial();
			var result = origSolution.PackFacetNumbers();
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(origSolution.Facets.Select(FacetToString));
			result.DestPoints.Should().Equal(origSolution.DestPoints);
		}

		[Test]
		public void PackFacetNumbers_FoldedSolution()
		{
			var origSolution = SolutionSpec.CreateTrivial().Fold("0,0 1,1");
			var result = origSolution.PackFacetNumbers();
			var expectedSourcePoints = "0,0|1,1|0,1|1,0";
			var expectedDestPoints = "0,0|1,1|0,1|0,1";
			var expectedFacets = "0 1 2|0 3 1";
			result.SourcePoints.Should().Equal(expectedSourcePoints.Split('|').Select(Vector.Parse).ToArray());
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedFacets.Split('|'));
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
		}

		[Test]
		public void PackFacetNumbers_TwiceFoldedSolution()
		{
			var origSolution = SolutionSpec.CreateTrivial().Fold("1/2,0 1/2,1").Fold("1/2,1/2 0,1/2");
			var result = origSolution.PackFacetNumbers();
			var expectedSourcePoints = "1/2,1/2|1/2,0|0,1/2|1/2,1|1,1/2|0,0|0,1|1,0|1,1";
			var expectedDestPoints = "1/2,1/2|1/2,0|0,1/2|1/2,0|0,1/2|0,0|0,0|0,0|0,0";
			var expectedFacets = "5 1 0 2|0 3 6 2|1 7 4 0|4 8 3 0";
			result.SourcePoints.Should().Equal(expectedSourcePoints.Split('|').Select(Vector.Parse).ToArray());
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedFacets.Split('|'));
		}

		[Test]
		public void RemoveBadFacetsVertices_EmptySolution()
		{
			var origSolution = SolutionSpec.CreateTrivial();
			var result = origSolution.RemoveBadFacetsVertices();
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(origSolution.Facets.Select(FacetToString));
			result.DestPoints.Should().Equal(origSolution.DestPoints);
		}

		[TestCase(
			"0,0|1/2,0|1,0|1,1|0,1", "0,8|1/2,8|1,8|1,9|0,9", "0 1 2 3 4",
			"0,0|1,0|1,1|0,1", "0,8|1,8|1,9|0,9", "0 1 2 3")]
		[TestCase(
			"0,0|1/2,0|1,0|0,1/2|1/2,1/2|1,1/2|0,1|1,1", "0,8|1/2,8|1,8|0,17/2|1/2,17/2|1,17/2|0,9|1,9", "0 1 4 3|1 2 5 4|3 4 5 7 6",
			"0,0|1/2,0|1,0|0,1/2|1/2,1/2|1,1/2|0,1|1,1", "0,8|1/2,8|1,8|0,17/2|1/2,17/2|1,17/2|0,9|1,9", "0 1 4 3|1 2 5 4|3 5 7 6")]
		[TestCase(
			"0,0|1,0|1,1/3|1/3,1/3|1/3,1|0,1|1,1", "0,0|1,0|1,1/3|1/3,1/3|1/3,1|0,1|1/3,1/3", "0 1 2 3 4 5|6 2 4|3 2 4",
			"0,0|1,0|1,1/3|1/3,1/3|1/3,1|0,1|1,1", "0,0|1,0|1,1/3|1/3,1/3|1/3,1|0,1|1/3,1/3", "0 1 2 3 4 5|6 2 4|3 2 4")]
		public void RemoveBadFacetsVertices_Removes(string sourcePoints, string destPoints, string facets, string expectedSourcePoints, string expectedDestPoints, string expectedFacets)
		{
			var origSolution = new SolutionSpec(
				sourcePoints.Split('|').Select(Vector.Parse).ToArray(),
				facets.Split('|').Select(f => new Facet(f.Split(' ').Select(int.Parse).ToArray())).ToArray(),
				destPoints.Split('|').Select(Vector.Parse).ToArray());
			var expectedSolution = new SolutionSpec(
				expectedSourcePoints.Split('|').Select(Vector.Parse).ToArray(),
				expectedFacets.Split('|').Select(f => new Facet(f.Split(' ').Select(int.Parse).ToArray())).ToArray(),
				expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
			var result = origSolution.RemoveBadFacetsVertices();
			result.SourcePoints.Should().Equal(expectedSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedSolution.Facets.Select(FacetToString));
			result.DestPoints.Should().Equal(expectedSolution.DestPoints);
		}

		[TestCase("1 2 3 4", "2 3 6 7", "1 2 7 6 3 4")]
		[TestCase("1 2 3 4", "7 6 3 2", "1 2 7 6 3 4")]
		[TestCase("1 2 4 3", "2 3 4", "1 2 3")]
		[TestCase("1 2 4 3", "4 3 2", "1 2 3")]
		[TestCase("2 3 4", "1 2 4 3", "1 2 3")]
		[TestCase("2 3 4", "3 4 2 1", "1 2 3")]
		[TestCase("0 1 2", "2 3 0", "1 2 3 0")]
		public void JoinFacets(string f1, string f2, string expectedFacet)
		{
			var facet1 = new Facet(f1.Split(' ').Select(int.Parse).ToArray());
			var facet2 = new Facet(f2.Split(' ').Select(int.Parse).ToArray());
			FacetToString(SolutionPacker.JoinFacets(facet1, facet2)).Should().Be(expectedFacet);
		}

		[TestCase("1 2 3 4 5 6 7 8 9 10", "5 6 9 10")]
		[TestCase("1 2 3 4 5 6 7 8 9 10", "5 6 11 9 10 12")]
		public void JoinFacets_Null(string f1, string f2)
		{
			var facet1 = new Facet(f1.Split(' ').Select(int.Parse).ToArray());
			var facet2 = new Facet(f2.Split(' ').Select(int.Parse).ToArray());
			SolutionPacker.JoinFacets(facet1, facet2).Should().Be(null);
		}

		[Test]
		public void Normalize_EmptySolution()
		{
			var origSolution = SolutionSpec.CreateTrivial();
			var result = origSolution.Normalize();
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(origSolution.Facets.Select(FacetToString));
			result.DestPoints.Should().Equal(origSolution.DestPoints);
		}

		[TestCase(
			"0,0|1,0|1,1|0,1", "0,0|1,0|1,1|0,1", "0 1 2|2 3 0",
			"0,0|1,0|1,1|0,1", "0,0|1,0|1,1|0,1", "1 2 3 0")]
		[TestCase(
			"0,0|1,0|1,1|0,1", "0,0|0,1|1,1|0,1", "0 1 2|2 3 0",
			"0,0|1,0|1,1|0,1", "0,0|0,1|1,1|0,1", "0 1 2|2 3 0")]
		[TestCase(
			"0,0|1/2,0|1,0|0,1/2|1/2,1/2|1,1/2|0,1|1/2,1|1,1", "0,0|1/2,0|1,0|0,1/2|1/2,1/2|1,1/2|0,1|1/2,1|1,1", "0 1 4 3|4 5 2 1|7 8 5 4|4 7 6 3",
			"0,0|1,0|0,1|1,1", "0,0|1,0|0,1|1,1", "3 1 0 2")]
		[TestCase(
			"0,0|1,0|1,1/3|1/3,1/3|1/3,1|0,1|1,1", "0,0|1,0|1,1/3|1/3,1/3|1/3,1|0,1|1/3,1/3", "0 1 2 3 4 5|6 2 4|3 2 4",
			"0,0|1,0|1,1/3|1/3,1|0,1|1,1", "0,0|1,0|1,1/3|1/3,1|0,1|1/3,1/3", "0 1 2 3 4|5 2 3")]
		public void Normalize(string sourcePoints, string destPoints, string facets, string expectedSourcePoints, string expectedDestPoints, string expectedFacets)
		{
			var origSolution = new SolutionSpec(
				sourcePoints.Split('|').Select(Vector.Parse).ToArray(),
				facets.Split('|').Select(f => new Facet(f.Split(' ').Select(int.Parse).ToArray())).ToArray(),
				destPoints.Split('|').Select(Vector.Parse).ToArray());
			var expectedSolution = new SolutionSpec(
				expectedSourcePoints.Split('|').Select(Vector.Parse).ToArray(),
				expectedFacets.Split('|').Select(f => new Facet(f.Split(' ').Select(int.Parse).ToArray())).ToArray(),
				expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
			var result = origSolution.Normalize();
			result.SourcePoints.Should().Equal(expectedSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedSolution.Facets.Select(FacetToString));
			result.DestPoints.Should().Equal(expectedSolution.DestPoints);
		}

		private object FacetToString(Facet arg)
		{
			return string.Join(" ", arg.Vertices);
		}
	}
}