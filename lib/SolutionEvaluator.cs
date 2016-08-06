using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public static class SolutionEvaluator
	{
		public static double Evaluate(ProblemSpec problemSpec, SolutionSpec solutionSpec, int dpi)
		{
			var problemPolygons = problemSpec.Polygons;
			var destPoints = solutionSpec.DestPoints.ToList();
			var allPoints = problemPolygons.SelectMany(x => x.Vertices).Concat(destPoints).ToList();
			var minX = allPoints.Min(x => x.X);
			var minY = allPoints.Min(x => x.Y);
			var maxX = allPoints.Max(x => x.X);
			var maxY = allPoints.Max(x => x.Y);

			var positivePolygons = problemPolygons.Where(p => p.GetSignedSquare() >= 0).ToList();
			var negativePolygons = problemPolygons.Where(p => p.GetSignedSquare() < 0).ToList();
			var solutionPolygons = solutionSpec.Facets.Select(x => new Polygon(x.Vertices.Select(v => destPoints[v]).ToArray())).ToList();

			var deltaX = (maxX - minX) / dpi;
			var deltaY = (maxY - minY) / dpi;
			int intersection = 0;
			int union = 0;
			for (var x = minX; x < maxX; x += deltaX)
				for (var y = minY; y < maxY; y += deltaY)
				{
					var p = new Vector(x, y);
					var inNegative = negativePolygons.Any(poly => p.GetPositionToPolygon(poly) == PointToPolygonPositionType.Inside);
					var inPositive = positivePolygons.Any(poly => p.GetPositionToPolygon(poly) != PointToPolygonPositionType.Outside);
					var inProblem = inPositive && !inNegative;
					var inSolution = solutionPolygons.Any(poly => p.GetPositionToPolygon(poly) != PointToPolygonPositionType.Outside);
					if (inProblem && inSolution)
						intersection++;
					if (inProblem || inSolution)
						union++;
				}
			return (double) intersection/union;
		}

		public static double EvaluateX(ProblemSpec problemSpec, SolutionSpec solutionSpec, int dpi)
		{
			var problemPolygons = problemSpec.Polygons.Select(x => new PolygonX(x)).ToList();
			var destPoints = solutionSpec.DestPoints.Select(x => new PointX(x)).ToList();
			var allPoints = problemPolygons.SelectMany(x => x.Vertices).Concat(destPoints).ToList();
			var minX = allPoints.Min(x => x.X);
			var minY = allPoints.Min(x => x.Y);
			var maxX = allPoints.Max(x => x.X);
			var maxY = allPoints.Max(x => x.Y);

			var positivePolygons = problemPolygons.Where(p => p.GetSignedSquare() >= 0).ToList();
			var negativePolygons = problemPolygons.Where(p => p.GetSignedSquare() < 0).ToList();
			var solutionPolygons = solutionSpec.Facets.Select(x => new PolygonX(x.Vertices.Select(v => destPoints[v]).ToArray())).ToList();

			var deltaX = (maxX - minX) / dpi;
			var deltaY = (maxY - minY) / dpi;
			int intersection = 0;
			int union = 0;
			for (var x = minX; x < maxX; x += deltaX)
				for (var y = minY; y < maxY; y += deltaY)
				{
					var p = new PointX(x, y);
					var inNegative = negativePolygons.Any(poly => p.GetPositionToPolygon(poly) == PointToPolygonPositionType.Inside);
					var inPositive = positivePolygons.Any(poly => p.GetPositionToPolygon(poly) != PointToPolygonPositionType.Outside);
					var inProblem = inPositive && !inNegative;
					var inSolution = solutionPolygons.Any(poly => p.GetPositionToPolygon(poly) != PointToPolygonPositionType.Outside);
					if (inProblem && inSolution)
						intersection++;
					if (inProblem || inSolution)
						union++;
				}
			return union == 0 ? 0 : (double) intersection/union;
		}

		private struct PointX
		{
			public readonly double X, Y;

			public PointX(Vector other)
				: this(other.X, other.Y)
			{
			}

			public PointX(double x, double y)
			{
				X = x;
				Y = y;
			}
			public static PointX operator -(PointX a, PointX b)
			{
				return new PointX(a.X - b.X, a.Y - b.Y);
			}
			public double Length2 => X * X + Y * Y;
		}

		private class SegmentX
		{
			public readonly PointX Start, End;

			public SegmentX(PointX start, PointX end)
			{
				Start = start;
				End = end;
			}
		}

		private class PolygonX
		{
			public readonly PointX[] Vertices;

			public PolygonX(Polygon src)
				: this(src.Vertices.Select(s => new PointX(s)).ToArray())
			{
			}

			public PolygonX(params PointX[] vertices)
			{
				Vertices = vertices;
			}

			public double GetSignedSquare()
			{
				double sum = 0;
				for (int i = 0; i < Vertices.Length; i++)
				{
					var p1 = Vertices[i];
					var p2 = Vertices[(i + 1) % Vertices.Length];
					sum += (p1.X - p2.X) * (p1.Y + p2.Y) / 2;
				}
				return sum;
			}
		}

		private static PointToPolygonPositionType GetPositionToPolygon(this PointX p, PolygonX polygon)
		{
			var parity = true;
			for (var i = 0; i < polygon.Vertices.Length; i++)
			{
				var v1 = polygon.Vertices[i];
				var v2 = polygon.Vertices[(i + 1) % polygon.Vertices.Length];
				var segment = new SegmentX(v1, v2);
				switch (ClassifyEdgeX(p, segment))
				{
					case EdgeType.TOUCHING:
						return PointToPolygonPositionType.Boundary;
					case EdgeType.CROSSING:
						parity = !parity;
						break;
				}
			}
			return parity ? PointToPolygonPositionType.Outside : PointToPolygonPositionType.Inside;
		}

		private enum EdgeType
		{
			CROSSING,
			INESSENTIAL,
			TOUCHING
		}

		private static EdgeType ClassifyEdgeX(PointX a, SegmentX e)
		{
			var v = e.Start;
			var w = e.End;
			switch (a.ClassifyX(e))
			{
				case PointClassification.LEFT:
					return ((v.Y < a.Y) && (a.Y <= w.Y)) ? EdgeType.CROSSING : EdgeType.INESSENTIAL;
				case PointClassification.RIGHT:
					return ((w.Y < a.Y) && (a.Y <= v.Y)) ? EdgeType.CROSSING : EdgeType.INESSENTIAL;
				case PointClassification.BETWEEN:
				case PointClassification.ORIGIN:
				case PointClassification.DESTINATION:
					return EdgeType.TOUCHING;
				default:
					return EdgeType.INESSENTIAL;
			}
		}

		private enum PointClassification
		{
			LEFT,
			RIGHT,
			BEYOND,
			BEHIND,
			BETWEEN,
			ORIGIN,
			DESTINATION
		};

		private static PointClassification ClassifyX(this PointX p, SegmentX s)
		{
			var a = s.End - s.Start;
			var b = p - s.Start;
			double sa = a.X * b.Y - b.X * a.Y;
			if (sa > 0.0)
				return PointClassification.LEFT;
			if (sa < 0.0)
				return PointClassification.RIGHT;
			if ((a.X * b.X < 0.0) || (a.Y * b.Y < 0.0))
				return PointClassification.BEHIND;
			if (a.Length2 < b.Length2)
				return PointClassification.BEYOND;
			if (s.Start.Equals(p))
				return PointClassification.ORIGIN;
			if (s.End.Equals(p))
				return PointClassification.DESTINATION;
			return PointClassification.BETWEEN;
		}
	}

	[TestFixture]
	public class SolutionEvaluator_Should
	{
		[TestCase("0,0 1,0 1,1 0,1", "0,0 1,0 1,1 0,1", 1.0, 0)]
		[TestCase("0,0 1,0 1,1 0,1|10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 0.5, 0.05)]
		[TestCase("0,0 1,0 1,5/4 0,5/4|10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 1/2.25, 0.05)]
		[TestCase("10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 0.0, 0)]
		public void Evaluate(string problem, string solution, double expectedResult, double precision)
		{
			var problemPolygons = problem.Split('|').Select(x => new Polygon(x.Split(' ').Select(Vector.Parse).ToArray())).ToArray();
			var problemSpec = new ProblemSpec(problemPolygons, new Segment[0]);

			var solutionPolygons = solution.Split('|').Select(x => new Polygon(x.Split(' ').Select(Vector.Parse).ToArray())).ToArray();
			var solutionPoints = solutionPolygons.SelectMany(x => x.Vertices).Distinct().ToArray();
			var pointToIndex = solutionPoints.Select((p, i) => new { p, i }).ToDictionary(x => x.p, x => x.i);
			var facets = solutionPolygons.Select(x => new Facet(x.Vertices.Select(v => pointToIndex[v]).ToArray())).ToArray();
			var solutionSpec = new SolutionSpec(solutionPoints, facets, solutionPoints);

			var evaluation = SolutionEvaluator.Evaluate(problemSpec, solutionSpec, 100);
			Console.Out.WriteLine(evaluation);
			evaluation.Should().BeApproximately(expectedResult, precision);
		}

		[TestCase("0,0 1,0 1,1 0,1", "0,0 1,0 1,1 0,1", 1.0, 0)]
		[TestCase("0,0 1,0 1,1 0,1|10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 0.5, 0.05)]
		[TestCase("0,0 1,0 1,5/4 0,5/4|10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 1/2.25, 0.05)]
		[TestCase("10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 0.0, 0)]
		public void EvaluateX(string problem, string solution, double expectedResult, double precision)
		{
			var problemPolygons = problem.Split('|').Select(x => new Polygon(x.Split(' ').Select(Vector.Parse).ToArray())).ToArray();
			var problemSpec = new ProblemSpec(problemPolygons, new Segment[0]);

			var solutionPolygons = solution.Split('|').Select(x => new Polygon(x.Split(' ').Select(Vector.Parse).ToArray())).ToArray();
			var solutionPoints = solutionPolygons.SelectMany(x => x.Vertices).Distinct().ToArray();
			var pointToIndex = solutionPoints.Select((p, i) => new { p, i }).ToDictionary(x => x.p, x => x.i);
			var facets = solutionPolygons.Select(x => new Facet(x.Vertices.Select(v => pointToIndex[v]).ToArray())).ToArray();
			var solutionSpec = new SolutionSpec(solutionPoints, facets, solutionPoints);

			var evaluation = SolutionEvaluator.EvaluateX(problemSpec, solutionSpec, 100);
			Console.Out.WriteLine(evaluation);
			evaluation.Should().BeApproximately(expectedResult, precision);
		}
	}
}