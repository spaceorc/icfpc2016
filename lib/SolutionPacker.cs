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
		public void Pack_EmptySolution()
		{
			var origSolution = SolutionSpec.CreateTrivial();
			var result = origSolution.Pack();
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(origSolution.Facets.Select(FacetToString));
			result.DestPoints.Should().Equal(origSolution.DestPoints);
		}

		[Test]
		public void Pack_FoldedSolution()
		{
			var origSolution = SolutionSpec.CreateTrivial().Fold("0,0 1,1");
			var result = origSolution.Pack();
			var expectedSourcePoints = "0,0|1,1|0,1|1,0";
			var expectedDestPoints = "0,0|1,1|0,1|0,1";
			var expectedFacets = "0 1 2|0 3 1";
			result.SourcePoints.Should().Equal(expectedSourcePoints.Split('|').Select(Vector.Parse).ToArray());
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedFacets.Split('|'));
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
		}

		[Test]
		public void Pack_TwiceFoldedSolution()
		{
			var origSolution = SolutionSpec.CreateTrivial().Fold("1/2,0 1/2,1").Fold("1/2,1/2 0,1/2");
			var result = origSolution.Pack();
			var expectedSourcePoints = "1/2,1/2|1/2,0|0,1/2|1/2,1|1,1/2|0,0|0,1|1,0|1,1";
			var expectedDestPoints = "1/2,1/2|1/2,0|0,1/2|1/2,0|0,1/2|0,0|0,0|0,0|0,0";
			var expectedFacets = "5 1 0 2|0 3 6 2|1 7 4 0|4 8 3 0";
			result.SourcePoints.Should().Equal(expectedSourcePoints.Split('|').Select(Vector.Parse).ToArray());
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedFacets.Split('|'));
		}

		private object FacetToString(Facet arg)
		{
			return string.Join(" ", arg.Vertices);
		}
	}
}