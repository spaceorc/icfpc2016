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
		public void SubmitSolutions(int problemId)
		{
			ImperfectSolver solver = new ImperfectSolver();
			Console.WriteLine($"problemId = {problemId}");
			var repo = new ProblemsRepo();
			var spec = repo.Get(problemId);
			var solution = solver.SolveMovingInitialSquare(spec);
			var res = apiClient.PostSolution(problemId, solution);
			Console.WriteLine(res);
			repo.PutSolution(problemId, solution);
			repo.PutResponse(problemId, res);
			Thread.Sleep(1000);
		}

		[TestCase(5)]
		public void TestMove(int problemId)
		{
			Console.WriteLine($"problemId = {problemId}");
			var spec = new ProblemsRepo().Get(problemId);
			var score = EvaluateProblem(spec);
			Console.WriteLine($"Score = {score}");
		}

		[Test]
		public void EvaluateAll()
		{
			var problems = new ProblemsRepo().GetAll();
			double sum1 = 0;
			double sum2 = 0;
			foreach (var p in problems)
			{
				//Console.WriteLine("Problem: " + p.id);
				var s1 = solver.SolveMovingInitialSquare(p);
				var s2 = solver.SolveMovingFoldedSquare(p);
				//Console.WriteLine(s2);
				var score1 = Eval(p, s1);
				var score2 = Eval(p, s2);
				sum1 += score1;
				sum2 += Math.Max(score1, score2);
				if (score2 > score1)
					Console.WriteLine(apiClient.PostSolution(p.id, s2));
				Thread.Sleep(800);
			}
			Console.WriteLine(sum1);
			Console.WriteLine(sum2);
		}

		static ImperfectSolver solver = new ImperfectSolver();
		private static ApiClient apiClient = new ApiClient();

		private static double Eval(ProblemSpec p, SolutionSpec s)
		{
			return SolutionEvaluator.EvaluateX(p, s, 100);
		}
		private static double EvaluateProblem(ProblemSpec spec)
		{
			//var solution = solver.SolveMovingInitialSquare(spec);
			var solution = solver.SolveMovingAndRotatingInitialSquare(spec, 10);
			var score = SolutionEvaluator.EvaluateX(spec, solution, 100);
			return score;
		}
	}
}