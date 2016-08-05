using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public static class SolutionEvaluator
	{
		public static double Evaluate(ProblemSpec problemSpec, SolutionSpec solutionSpec, int dpi)
		{
			var allPoints = problemSpec.Polygons.SelectMany(x => x.Vertices).Concat(solutionSpec.DestPoints).ToList();
			var minX = allPoints.Min(x => x.X);
			var minY = allPoints.Min(x => x.Y);
			var maxX = allPoints.Max(x => x.X);
			var maxY = allPoints.Max(x => x.Y);

			var positivePolygons = problemSpec.Polygons.Where(p => p.GetSignedSquare() >= 0).ToList();
			var negativePolygons = problemSpec.Polygons.Where(p => p.GetSignedSquare() < 0).ToList();
			var solutionPolygons = solutionSpec.Facets.Select(x => new Polygon(x.Vertices.Select(v => solutionSpec.DestPoints[v]).ToArray())).ToList();

			var deltaX = (maxX - minX) / dpi;
			var deltaY = (maxY - minY) / dpi;
			int intersection = 0;
			int union = 0;
			for (var x = minX; x < maxX; x += deltaX)
				for (var y = minY; y < maxY; y += deltaY)
				{
					var p = new Point(x, y);
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
	}

	[TestFixture]
	public class SolutionEvaluator_Should
	{
		[TestCase("0,0 1,0 1,1 0,1", "0,0 1,0 1,1 0,1", 1.0, 0)]
		[TestCase("0,0 1,0 1,1 0,1|10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 0.5, 0.03)]
		[TestCase("10,0 11,0 11,1 10,1", "0,0 1,0 1,1 0,1", 0.0, 0)]
		public void Evaluate(string problem, string solution, double expectedResult, double precision)
		{
			var problemPolygons = problem.Split('|').Select(x => new Polygon(x.Split(' ').Select(Point.Parse).ToArray())).ToArray();
			var problemSpec = new ProblemSpec(problemPolygons, new Segment[0]);

			var solutionPolygons = solution.Split('|').Select(x => new Polygon(x.Split(' ').Select(Point.Parse).ToArray())).ToArray();
			var solutionPoints = solutionPolygons.SelectMany(x => x.Vertices).Distinct().ToArray();
			var pointToIndex = solutionPoints.Select((p, i) => new { p, i }).ToDictionary(x => x.p, x => x.i);
			var facets = solutionPolygons.Select(x => new Facet(x.Vertices.Select(v => pointToIndex[v]).ToArray())).ToArray();
			var solutionSpec = new SolutionSpec(solutionPoints, facets, solutionPoints);

			var evaluation = SolutionEvaluator.Evaluate(problemSpec, solutionSpec, 100);
			evaluation.Should().BeApproximately(expectedResult, precision);
		}
	}
}