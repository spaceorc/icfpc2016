using System;
using System.Net.Http;
using System.Threading;

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
				var spec = UltraSolver.AutoSolve(problemSpec);
				res = Post(spec, problemSpec.id);
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

		public static double Post(SolutionSpec solutionSpec, int problemId, bool pack = true, bool storeSolutionWithSameResemblance = false)
		{
			if (pack)
				solutionSpec = solutionSpec.Pack();

			var existingSolution = repo.FindSolution(problemId);
			if (existingSolution == solutionSpec.ToString())
			{
				var resemblance = repo.GetProblemResemblance(problemId);
				Console.Out.Write($" solution is the same! current score: {resemblance} ");
				return resemblance;
			}

			var solutionSize = solutionSpec.Size();
			if (solutionSize > 5000)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Out.Write($" solution size limit exceeded {solutionSize} ");
				Console.ResetColor();
				return 0;
			}

			return DoPost(problemId, solutionSpec, storeSolutionWithSameResemblance);
		}

		private static double DoPost(int problemSpecId, SolutionSpec solutionSpec, bool storeSolutionWithSameResemblance)
		{
			var client = new ApiClient();
			try
			{
				var oldResemblance = repo.GetProblemResemblance(problemSpecId);
				var response = client.PostSolution(problemSpecId, solutionSpec);
				var resemblance = repo.GetResemblance(response);
				if (resemblance >= oldResemblance)
				{
					if (resemblance > oldResemblance || storeSolutionWithSameResemblance)
					{
						repo.PutResponse(problemSpecId, response);
						repo.PutSolution(problemSpecId, solutionSpec);
					}
					if (resemblance > oldResemblance)
					{
						Console.ForegroundColor = resemblance >= 1.0 ? ConsoleColor.Green : ConsoleColor.White;
						Console.Out.Write($" solution for problem {problemSpecId} improved! new score: {resemblance} ");
						Console.ResetColor();
					}
					else
						Console.Out.Write($" just solution for problem {problemSpecId} changed! old = new score: {resemblance} ");
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