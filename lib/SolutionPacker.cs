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
			return source.PackFacetNumbers().RemoveBadFacetsVertices();
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
				var convexBoundary = polygon.GetConvexBoundary();
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

		private object FacetToString(Facet arg)
		{
			return string.Join(" ", arg.Vertices);
		}
	}
}