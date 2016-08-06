using System;
using System.Collections.Generic;
using System.Linq;

namespace lib
{
	public class ImperfectSolver
	{
		private static readonly Vector[] initialSquare = "0,0 1,0 1,1 0,1".ToPoints();

		public SolutionSpec SolveMovingInitialSquare(ProblemSpec problem)
		{
			return new SolutionSpec(initialSquare, new[] { new Facet(0, 1, 2, 3) }, MoveSquare(initialSquare, problem));
		}

		public SolutionSpec SolveMovingAndRotatingInitialSquare(ProblemSpec problem, int dpi = 10)
		{
			var shift = GetShift(problem);
			var ts = from dx in Range(0, 1, dpi)
					 from dy in Range(0, 1, dpi)
					 let finalShift = shift.Move(dx, dy)
					 from x in Range(0, 1, dpi * 2)
					 select (Func<Vector, Vector>)(v => v.Rotate(x) + finalShift);
			var transform =
				ts.Select(t => Tuple.Create(t, SolutionEvaluator.EvaluateX(problem, SolutionSpec.CreateTrivial(t), dpi * 2)))
					.OrderByDescending(t => t.Item2)
					.FirstOrDefault()?.Item1;
			return SolutionSpec.CreateTrivial(transform);
		}

		public SolutionSpec SolveMovingFoldedSquare(ProblemSpec problem)
		{
			var shift = GetShift(problem);
			var half = "0,0 0,1 1,1 1,0 0,1/2 1,1/2".ToPoints();
			var dest = "0,0 0,0 1,0 1,0 0,1/2 1,1/2".ToPoints().Select(v => v + shift).ToArray();
			var facets = new[] { new Facet(0, 4, 5, 3), new Facet(4, 1, 2, 5) };
			return new SolutionSpec(half, facets, dest);
		}

		private IEnumerable<Rational> Range(Rational start, Rational end, int steps)
		{
			var d = (end - start)/steps;
			return Enumerable.Range(0, steps).Select(n => start + n*d);
		}

		private Vector[] MoveSquare(Vector[] square, ProblemSpec problem)
		{
			var shift = GetShift(problem);
			return square.Select(p => p + shift).ToArray();
		}

		private static Vector GetShift(ProblemSpec problem)
		{
			var vs = problem.Polygons.SelectMany(p => p.Vertices).ToList();
			var minX = vs.Select(p => p.X).Min();
			var minY = vs.Select(p => p.Y).Min();
			var shift = new Vector(minX, minY);
			return shift;
		}
	}
}