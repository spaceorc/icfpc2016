using lib.ProjectionSolver;
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
				var spec = new ConstructorSolver(problemSpec).Work(); //TODO pack в 41 строке убран, т.к. работает криво. Убрать комментарий, если нужно
				res = Post(spec, problemSpec.Id);
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


			
		public static double Post(ProblemSpec problemSpec, SolutionSpec solutionSpec)
		{
			return Post(solutionSpec, problemId);
		}

		public static double Post(SolutionSpec solutionSpec, int problemId)
		{
			//solutionSpec = solutionSpec.Pack();

			var existingSolution = repo.FindSolution(problemSpec.id);
			if (existingSolution == solutionSpec.ToString())
			{
				var resemblance = repo.GetProblemResemblance(problemSpec.id);
				Console.Out.Write($" solution is the same! current score: {resemblance} ");
				return resemblance;
			}

			var solutionSize = solutionSpec.Size();
			if (solutionSize > 5000)
			{
				Console.Out.Write($" solution size limit exceeded {solutionSize} ");
				return 0;
			}

			return DoPost(problemSpec, solutionSpec);
		}

		private static double DoPost(ProblemSpec problemSpec, SolutionSpec solutionSpec)
		{
			var client = new ApiClient();
			try
			{
				var oldResemblance = repo.GetProblemResemblance(problemSpec.id);
				var response = client.PostSolution(problemSpec.id, solutionSpec);
				var resemblance = repo.GetResemblance(response);
				if (resemblance >= oldResemblance)
				{
					repo.PutResponse(problemSpec.id, response);
					repo.PutSolution(problemSpec.id, solutionSpec);
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