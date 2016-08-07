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
				var spec = ProjectionSolverRunner.Solve(problemSpec);
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

		public static double Post(int problemId, SolutionSpec solutionSpec)
		{
			return Post(solutionSpec, problemId);
		}

		public static double Post(SolutionSpec solutionSpec, int problemId)
		{
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
				Console.Out.Write($" solution size limit exceeded {solutionSize} ");
				return 0;
			}

			return DoPost(problemId, solutionSpec);
		}

		private static double DoPost(int problemId, SolutionSpec solutionSpec)
		{
			var client = new ApiClient();
			try
			{
				var oldResemblance = repo.GetProblemResemblance(problemId);
				var response = client.PostSolution(problemId, solutionSpec);
				var resemblance = repo.GetResemblance(response);
				if (resemblance >= oldResemblance)
				{
					repo.PutResponse(problemId, response);
					repo.PutSolution(problemId, solutionSpec);
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