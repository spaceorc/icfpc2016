using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class ImperfectSolver_Should
	{
		private static int[] ids = Enumerable.Range(1, 101).ToArray();

		[TestCaseSource(nameof(ids))]
		[Explicit]
		public void TestMove(int problemId)
		{
			ImperfectSolver solver = new ImperfectSolver();
			Console.WriteLine($"problemId = {problemId}");
			var repo = new ProblemsRepo();
			var spec = repo.Get(problemId);
			var solution = solver.SolveMovingInitialSquare(spec);
			var res = new ApiClient().PostSolution(problemId, solution);
			Console.WriteLine(res);
			repo.PutResponse(problemId, res);
			Thread.Sleep(1000);
		}
	}
}