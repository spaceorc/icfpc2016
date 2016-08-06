using System;
using System.Linq;
using NUnit.Framework;

namespace lib
{
	public static class ConvexPolygonSolver
	{
		public static SolutionSpec Solve(Polygon poly, SolutionSpec initialSolution)
		{
			if (poly.GetSignedSquare() < 0)
				throw new InvalidOperationException("poly.GetSignedSquare() < 0");

			var solution = initialSolution ?? SolutionSpec.CreateTrivial(x => x);
			while (true)
			{
				var foldsCount = 0;
				foreach (var segment in poly.Segments)
				{
					var s = solution.Fold(segment);
					if (s != solution)
						foldsCount++;
					solution = s;
				}
				if (foldsCount == 0)
					return solution;
			}
		}
	}

	[TestFixture]
	public class ConvexPolygonSolver_Should
	{
		[Test]
		public void Solve()
		{
			var problem = new ProblemsRepo().Get(11);
			var poly = problem.Polygons.Single();
			var solution = ConvexPolygonSolver.Solve(poly, null);
			solution.CreateVisualizerForm(false).ShowDialog();
		}
	}
}