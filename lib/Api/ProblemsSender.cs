using System;
using System.Net.Http;
using System.Threading;
using SquareConstructor;

namespace lib.Api
{
	public class ProblemsSender
	{
		private static readonly ProblemsRepo repo = new ProblemsRepo();

		public static double TrySolveAndSend(ProblemSpec problemSpec)
		{
			var res = 0.0;
			var t = new Thread(() =>
			{
				//var spec = ProjectionSolverRunner.Solve(problemSpec);
				var spec = new ConstructorSolver(problemSpec).Work();
				res = Post(spec, problemSpec.id, pack: false); //TODO pack работает криво (?). Убрать комментарий, если нужно
			})
			{ IsBackground = true };
			t.Start();
			if (!t.Join(5000))
			{
				t.Abort();
				t.Join();
			}
			return res;
		}

		public static double Post(SolutionSpec solutionSpec, int problemSpecId, bool pack = true)
		{
			if (pack)
				solutionSpec = solutionSpec.Pack();

			var existingSolution = repo.FindSolution(problemSpecId);
			if (existingSolution == solutionSpec.ToString())
			{
				var resemblance = repo.GetProblemResemblance(problemSpecId);
				Console.Out.Write($" solution is the same! current score: {resemblance} ");
				return resemblance;
			}

			var solutionSize = solutionSpec.Size();
			if (solutionSize > 5000)
			{
				Console.Out.Write($" solution size limit exceeded {solutionSize} ");
				return 0;
			}

			return DoPost(problemSpecId, solutionSpec);
		}

		private static double DoPost(int problemSpecId, SolutionSpec solutionSpec)
		{
			var client = new ApiClient();
			try
			{
				var oldResemblance = repo.GetProblemResemblance(problemSpecId);
				var response = client.PostSolution(problemSpecId, solutionSpec);
				var resemblance = repo.GetResemblance(response);
				if (resemblance >= oldResemblance)
				{
					repo.PutResponse(problemSpecId, response);
					repo.PutSolution(problemSpecId, solutionSpec);
					Console.Out.Write($" solution improved! new score: {resemblance} ");
				}
				return resemblance;
			}
			catch (Exception e)
			{
				if (e is ThreadAbortException)
					return 0;
				if (e is HttpRequestException)
					return 0;
				Console.WriteLine(e);
				return 0;
			}
		}
	}
}