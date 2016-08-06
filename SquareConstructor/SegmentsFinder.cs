using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace SquareConstructor
{
	public static class PolygonFinder
	{
		public static List<Polygon> GetRealPolygons(ProblemSpec problem)
		{
			var segments = GetRealSegments(problem);
			segments.AddRange(segments.ToArray().Select(Reverse));

			Dictionary<Vector, List<Segment>> outerSegments = segments.GroupBy(segment => segment.Start).ToDictionary(group => group.Key, group => group.ToList());
			HashSet<Tuple<Vector, Vector>> usedSegments = new HashSet<Tuple<Vector, Vector>>();
			List<Polygon> polygons = new List<Polygon>();
			
			var holePolygons = problem.Polygons.Select(polygon => new HashSet<Vector>(polygon.Vertices)).ToList();
			
			foreach (var segment in segments)
			{
				if(usedSegments.Contains(Tuple.Create(segment.Start, segment.End)))
					continue;
				var points = GeneratePolygon(segment, outerSegments, usedSegments).ToArray();
				if (holePolygons.Any(p => points.Count(p.Contains) == p.Count))
					continue;
				polygons.Add(new Polygon(points));
			}
			return polygons;
		}

		private static IEnumerable<Vector> GeneratePolygon(Segment startSegment, Dictionary<Vector, List<Segment>> outerSegments, HashSet<Tuple<Vector, Vector>> usedSegments)
		{
			var segment = startSegment;
			usedSegments.Add(Tuple.Create(segment.Start, segment.End));
			yield return segment.Start;
			while (!segment.End.Equals(startSegment.Start))
			{
				var nextCands = outerSegments[segment.End];
				double min = 5;
				Segment best = null;

				foreach (var cand in nextCands)
				{
					var candMeasure = GeometryExtensions.GetAngleMeasure(segment.End - segment.Start, cand.End - cand.Start);
					if (candMeasure > 1e-7 && candMeasure < min)
					{
						min = candMeasure;
						best = cand;
					}
				}
				segment = best;
				yield return segment.Start;
				usedSegments.Add(Tuple.Create(segment.Start, segment.End));
			}
		}

		private static Segment Reverse(Segment segment)
		{
			return new Segment(segment.End, segment.Start);
		}

		public static List<Segment> GetRealSegments(ProblemSpec problem)
		{
			return problem.Segments.SelectMany(segment =>
			{
				var points = problem.Segments
					.Where(seg => !seg.Equals(segment))
					.Select(intersector => intersector.GetIntersection(segment))
					.Where(point => point != null)
					.Select(point => point.Value)
					.OrderBy(point =>
					{
						if (segment.Start.X == segment.End.X)
							return point.Y - segment.Start.Y;
						return point.X - segment.Start.X;
					})
					.Distinct()
					.ToArray();

				return points.Take(points.Length - 1).Select((point, i) => new Segment(point, points[i + 1])).ToList();
			}).ToList();
		}
	}
}
