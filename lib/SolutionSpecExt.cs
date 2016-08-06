using System;
using System.Collections.Generic;
using System.Linq;
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
								if (relativePos != PointRelativePosition.Inside)
									outerFacetVertices.Add(vertex);
								if (relativePos != PointRelativePosition.Outside)
									innerFacetVertices.Add(vertex);
								prevVertex = vertex;
								prevRelativePos = relativePos;
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
												var destPoint = destEdge.GetIntersectionWithLine(segment).Value;
												int intersectionVertex;
												var srcPoint = GetSrcPoint(srcEdge, destEdge, destPoint);
												if (!srcPointsMap.TryGetValue(srcPoint, out intersectionVertex))
												{
													intersectionVertex = newDestPoints.Count;
													newDestPoints.Add(destPoint);
													newSourcePoints.Add(srcPoint);
													srcPointsMap.Add(srcPoint, intersectionVertex);
												}
												innerFacetVertices.Add(intersectionVertex);
												outerFacetVertices.Add(intersectionVertex);
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
												var destPoint = destEdge.GetIntersectionWithLine(segment).Value;
												int intersectionVertex;
												var srcPoint = GetSrcPoint(srcEdge, destEdge, destPoint);
												if (!srcPointsMap.TryGetValue(srcPoint, out intersectionVertex))
												{
													intersectionVertex = newDestPoints.Count;
													newDestPoints.Add(destPoint);
													newSourcePoints.Add(srcPoint);
													srcPointsMap.Add(srcPoint, intersectionVertex);
												}
												innerFacetVertices.Add(intersectionVertex);
												outerFacetVertices.Add(intersectionVertex);
												break;
											case PointRelativePosition.Outside:
												outerFacetVertices.Add(vertex);
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
								newFacets.Add(new Facet(innerFacetVertices.ToArray()));
								newFacets.Add(new Facet(outerFacetVertices.ToArray()));
							}
						}
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
}