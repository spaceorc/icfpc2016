using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SquareConstructor;

namespace lib
{
	public static class SolutionSpecExt
	{
		public static SolutionSpec Fold(this SolutionSpec origSpec, Segment segment)
		{
			var facetsToFold = new HashSet<Facet>();
			for (int i = 0; i < origSpec.DestPoints.Length; i++)
			{
				if (GetDestPointRelativePosition(origSpec.DestPoints[i], segment) == PointRelativePosition.Outside)
					facetsToFold.UnionWith(origSpec.GetFacetsWithPoint(i));
			}

			if (!facetsToFold.Any())
				return origSpec;

			var newSourcePoints = new List<Vector>(origSpec.SourcePoints);
			var srcPointsMap = newSourcePoints.Select((x, i) => new { V = x, Index = i }).ToDictionary(x => x.V, x => x.Index);
			var newDestPoints = new List<Vector>(origSpec.DestPoints);
			var newFacets = new List<Facet>();
			foreach (var facet in origSpec.Facets)
			{
				if (!facetsToFold.Contains(facet))
					newFacets.Add(facet);
				else
				{
					if (facet.Vertices.All(v => GetDestPointRelativePosition(origSpec.DestPoints[v], segment) != PointRelativePosition.Inside))
					{
						newFacets.Add(facet);
						foreach (var vertex in facet.Vertices)
							newDestPoints[vertex] = origSpec.DestPoints[vertex].Reflect(segment);
					}
					else
					{
						int prevVertex = -1;
						var prevRelativePos = default (PointRelativePosition);
						var innerFacetVertices = new List<int>();
						var outerFacetVertices = new List<int>();
						foreach (var vertex in facet.Vertices)
						{
							var relativePos = GetDestPointRelativePosition(origSpec.DestPoints[vertex], segment);
							if (prevVertex == -1)
							{
								switch (relativePos)
								{
									case PointRelativePosition.Inside:
										innerFacetVertices.Add(vertex);
										break;
									case PointRelativePosition.Outside:
										outerFacetVertices.Add(vertex);
										newDestPoints[vertex] = origSpec.DestPoints[vertex].Reflect(segment);
										break;
									case PointRelativePosition.Boundary:
										innerFacetVertices.Add(vertex);
										outerFacetVertices.Add(vertex);
										break;
									default:
										throw new ArgumentOutOfRangeException();
								}
							}
							else
							{
								var destEdge = new Segment(origSpec.DestPoints[prevVertex], origSpec.DestPoints[vertex]);
								var srcEdge = new Segment(origSpec.SourcePoints[prevVertex], origSpec.SourcePoints[vertex]);
								switch (prevRelativePos)
								{
									case PointRelativePosition.Inside:
										switch (relativePos)
										{
											case PointRelativePosition.Inside:
												innerFacetVertices.Add(vertex);
												break;
											case PointRelativePosition.Outside:
												var intersectionDestPoint = destEdge.GetIntersectionWithLine(segment).Value;
												int intersectionVertex;
												var intersectionSrcPoint = GetSrcPoint(srcEdge, destEdge, intersectionDestPoint);
												if (!srcPointsMap.TryGetValue(intersectionSrcPoint, out intersectionVertex))
												{
													intersectionVertex = newDestPoints.Count;
													newDestPoints.Add(intersectionDestPoint);
													newSourcePoints.Add(intersectionSrcPoint);
													srcPointsMap.Add(intersectionSrcPoint, intersectionVertex);
												}
												innerFacetVertices.Add(intersectionVertex);
												outerFacetVertices.Add(intersectionVertex);
												outerFacetVertices.Add(vertex);
												newDestPoints[vertex] = origSpec.DestPoints[vertex].Reflect(segment);
												break;
											case PointRelativePosition.Boundary:
												innerFacetVertices.Add(vertex);
												outerFacetVertices.Add(vertex);
												break;
											default:
												throw new ArgumentOutOfRangeException();
										}
										break;
									case PointRelativePosition.Outside:
										switch (relativePos)
										{
											case PointRelativePosition.Inside:
												var intersectionDestPoint = destEdge.GetIntersectionWithLine(segment).Value;
												int intersectionVertex;
												var intersectionSrcPoint = GetSrcPoint(srcEdge, destEdge, intersectionDestPoint);
												if (!srcPointsMap.TryGetValue(intersectionSrcPoint, out intersectionVertex))
												{
													intersectionVertex = newDestPoints.Count;
													newDestPoints.Add(intersectionDestPoint);
													newSourcePoints.Add(intersectionSrcPoint);
													srcPointsMap.Add(intersectionSrcPoint, intersectionVertex);
												}
												innerFacetVertices.Add(intersectionVertex);
												innerFacetVertices.Add(vertex);
												outerFacetVertices.Add(intersectionVertex);
												break;
											case PointRelativePosition.Outside:
												outerFacetVertices.Add(vertex);
												newDestPoints[vertex] = origSpec.DestPoints[vertex].Reflect(segment);
												break;
											case PointRelativePosition.Boundary:
												innerFacetVertices.Add(vertex);
												outerFacetVertices.Add(vertex);
												break;
											default:
												throw new ArgumentOutOfRangeException();
										}
										break;
									case PointRelativePosition.Boundary:
										switch (relativePos)
										{
											case PointRelativePosition.Inside:
												innerFacetVertices.Add(vertex);
												break;
											case PointRelativePosition.Outside:
												outerFacetVertices.Add(vertex);
												newDestPoints[vertex] = origSpec.DestPoints[vertex].Reflect(segment);
												break;
											case PointRelativePosition.Boundary:
												throw new ArgumentOutOfRangeException("WTF! Переход из Boundary в Boundary");
											default:
												throw new ArgumentOutOfRangeException();
										}
										break;
									default:
										throw new ArgumentOutOfRangeException();
								}
							}
							prevVertex = vertex;
							prevRelativePos = relativePos;
						}

						// last edge
						{
							var vertex = facet.Vertices[0];
							var relativePos = GetDestPointRelativePosition(origSpec.DestPoints[vertex], segment);
							var destEdge = new Segment(origSpec.DestPoints[prevVertex], origSpec.DestPoints[vertex]);
							var srcEdge = new Segment(origSpec.SourcePoints[prevVertex], origSpec.SourcePoints[vertex]);
							switch (prevRelativePos)
							{
								case PointRelativePosition.Inside:
									switch (relativePos)
									{
										case PointRelativePosition.Inside:
											break;
										case PointRelativePosition.Outside:
											var intersectionDestPoint = destEdge.GetIntersectionWithLine(segment).Value;
											int intersectionVertex;
											var intersectionSrcPoint = GetSrcPoint(srcEdge, destEdge, intersectionDestPoint);
											if (!srcPointsMap.TryGetValue(intersectionSrcPoint, out intersectionVertex))
											{
												intersectionVertex = newDestPoints.Count;
												newDestPoints.Add(intersectionDestPoint);
												newSourcePoints.Add(intersectionSrcPoint);
												srcPointsMap.Add(intersectionSrcPoint, intersectionVertex);
											}
											innerFacetVertices.Add(intersectionVertex);
											outerFacetVertices.Add(intersectionVertex);
											break;
										case PointRelativePosition.Boundary:
											break;
										default:
											throw new ArgumentOutOfRangeException();
									}
									break;
								case PointRelativePosition.Outside:
									switch (relativePos)
									{
										case PointRelativePosition.Inside:
											var intersectionDestPoint = destEdge.GetIntersectionWithLine(segment).Value;
											int intersectionVertex;
											var intersectionSrcPoint = GetSrcPoint(srcEdge, destEdge, intersectionDestPoint);
											if (!srcPointsMap.TryGetValue(intersectionSrcPoint, out intersectionVertex))
											{
												intersectionVertex = newDestPoints.Count;
												newDestPoints.Add(intersectionDestPoint);
												newSourcePoints.Add(intersectionSrcPoint);
												srcPointsMap.Add(intersectionSrcPoint, intersectionVertex);
											}
											innerFacetVertices.Add(intersectionVertex);
											outerFacetVertices.Add(intersectionVertex);
											break;
										case PointRelativePosition.Outside:
											break;
										case PointRelativePosition.Boundary:
											break;
										default:
											throw new ArgumentOutOfRangeException();
									}
									break;
								case PointRelativePosition.Boundary:
									break;
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
						
						newFacets.Add(new Facet(innerFacetVertices.ToArray()));
						newFacets.Add(new Facet(outerFacetVertices.ToArray()));
					}
				}
			}
			return new SolutionSpec(newSourcePoints.ToArray(), newFacets.ToArray(), newDestPoints.ToArray());
		}

		private static Vector GetSrcPoint(Segment srcEdge, Segment destEdge, Vector destPoint)
		{
			var k = destEdge.ToVector().ScalarProd(destPoint - destEdge.Start)/destEdge.QuadratOfLength;
			var srcPoint = srcEdge.Start + srcEdge.ToVector()*k;
			return srcPoint;
		}

		private static PointRelativePosition GetDestPointRelativePosition(Vector destPoint, Segment segment)
		{
			var pointVector = destPoint - segment.Start;
			var vectorProdLength = pointVector.VectorProdLength(segment.ToVector());
			return vectorProdLength > 0 ? PointRelativePosition.Outside : vectorProdLength < 0 ? PointRelativePosition.Inside : PointRelativePosition.Boundary;
		}

		public static IEnumerable<Facet> GetFacetsWithPoint(this SolutionSpec spec, int vertex)
		{
			return spec.Facets.Where(f => f.Vertices.Contains(vertex));
		}

		private enum PointRelativePosition
		{
			Inside,
			Outside,
			Boundary
		}
	}

	[TestFixture]
	public class SolutionSpecExt_Should
	{
		[TestCase("0,0 1,0")]
		[TestCase("0,-1/2 1,-1/2")]
		[TestCase("1/2,2 -2,1/2")]
		public void Fold_Nothing(string segment)
		{
			var origSolution = SolutionSpec.CreateTrivial(x => x);
			var result = origSolution.Fold(segment);
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Should().Equal(origSolution.Facets);
			result.DestPoints.Should().Equal(origSolution.DestPoints);
		}

		[TestCase("1,0 0,0", "0,0|1,0|1,-1|0,-1")]
		[TestCase("1,1 1,0", "2,0|1,0|1,1|2,1")]
		[TestCase("0,1 1,1", "0,2|1,2|1,1|0,1")]
		[TestCase("0,0 0,1", "0,0|-1,0|-1,1|0,1")]
		public void Fold_ByBoundary(string segment, string expectedDestPoints)
		{
			var origSolution = SolutionSpec.CreateTrivial(x => x);
			var result = origSolution.Fold(segment);
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Should().Equal(origSolution.Facets);
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
		}

		[TestCase("0,0 1,1", "0,0|0,1|1,1|0,1", "0 1 2|0 2 3")]
		[TestCase("1,1 0,0", "0,0|1,0|1,1|1,0", "0 1 2|0 2 3")]
		[TestCase("1,0 0,1", "0,0|1,0|0,0|0,1", "0 1 3|1 2 3")]
		[TestCase("0,1 1,0", "1,1|1,0|1,1|0,1", "0 1 3|1 2 3")]
		public void Fold_ByDiagonal(string segment, string expectedDestPoints, string expectedFacets)
		{
			var origSolution = SolutionSpec.CreateTrivial(x => x);
			var result = origSolution.Fold(segment);
			result.SourcePoints.Should().Equal(origSolution.SourcePoints);
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedFacets.Split('|'));
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
		}

		[TestCase("0,1/2 1,1/2", "0,0|1,0|1,1|0,1|1,1/2|0,1/2", "0,1|1,1|1,1|0,1|1,1/2|0,1/2", "0 1 4 5|4 2 3 5")]
		[TestCase("1,1/2 0,1/2", "0,0|1,0|1,1|0,1|1,1/2|0,1/2", "0,0|1,0|1,0|0,0|1,1/2|0,1/2", "0 1 4 5|4 2 3 5")]
		[TestCase("1/2,0 1/2,1", "0,0|1,0|1,1|0,1|1/2,0|1/2,1", "0,0|0,0|0,1|0,1|1/2,0|1/2,1", "0 4 5 3|4 1 2 5")]
		[TestCase("1/2,1 1/2,0", "0,0|1,0|1,1|0,1|1/2,0|1/2,1", "1,0|1,0|1,1|1,1|1/2,0|1/2,1", "0 4 5 3|4 1 2 5")]
		public void Fold_ByMiddleLine(string segment, string expectedSrcPoints, string expectedDestPoints, string expectedFacets)
		{
			var origSolution = SolutionSpec.CreateTrivial(x => x);
			var result = origSolution.Fold(segment);
			result.SourcePoints.Should().Equal(expectedSrcPoints.Split('|').Select(Vector.Parse).ToArray());
			result.Facets.Select(FacetToString).ToArray().Should().BeEquivalentTo(expectedFacets.Split('|'));
			result.DestPoints.Should().Equal(expectedDestPoints.Split('|').Select(Vector.Parse).ToArray());
		}

		[Test]
		[Explicit]
		public void Fold_Demo()
		{
			var origSolution = SolutionSpec.CreateTrivial(x => x);
			var result = origSolution.Fold("1,3/4 0,1/4");
			result = result.Fold("1,1 1/4,0");
			result.CreateVisualizerForm(true).ShowDialog();
		}

		private object FacetToString(Facet arg)
		{
			return string.Join(" ", arg.Vertices);
		}
	}
}