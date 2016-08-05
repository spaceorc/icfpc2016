using System.Linq;

namespace lib
{
	public class ImperfectSolver
	{
		private readonly Vector[] initialSquare = "0,0 1,0 1,1 0,1".ToPoints();

		public SolutionSpec SolveMovingInitialSquare(ProblemSpec problem)
		{
			return new SolutionSpec(initialSquare, new[] { new Facet(0, 1, 2, 3) }, MoveSquare(initialSquare, problem));
		}

		private Vector[] MoveSquare(Vector[] square, ProblemSpec problem)
		{
			var vs = problem.Polygons.SelectMany(p => p.Vertices).ToList();
			var minX = vs.Select(p => p.X).Min();
			var minY = vs.Select(p => p.Y).Min();
			return square.Select(p => p + new Vector(minX, minY)).ToArray();
		}
	}
}