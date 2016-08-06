using System;
using System.Threading;
using lib;
using Newtonsoft.Json.Linq;
using Runner;

namespace AutoSolver
{
	class Program
	{
		private static ApiClient client;
		private static ProblemsRepo repo;

		static void Main(string[] args)
		{
			repo = new ProblemsRepo();
			client = new ApiClient();

			while (true)
			{
				Console.WriteLine("Downloading new problems...");
				var snapshot = client.GetLastSnapshot();
				foreach (var problem in snapshot.Problems)
				{
					if (repo.Find(problem.Id) == null)
					{
						var problemSpec = client.GetBlob(problem.SpecHash);
						Thread.Sleep(1000);
						repo.Put(problem.Id, problemSpec);
						Console.WriteLine($"Downloaded problem {problem.Id}");
					}
				}

				Console.WriteLine("Solving...");
				var problemSpecs = repo.GetAll();
				var imperfectSolver = new ImperfectSolver();
				foreach (var problemSpec in problemSpecs)
				{
					var response = repo.FindResponse(problemSpec.id);
					if (response == null)
					{
						Console.Write($"Solving {problemSpec.id}...");
						var solutionSpec = imperfectSolver.SolveMovingInitialSquare(problemSpec);
						var score = Post(problemSpec, solutionSpec);
						Console.Write($" imperfect score: {score}");

						if (score != 1.0)
						{
							var t = new Thread(() =>
							{
								var spec = GraphExt.Solve(problemSpec);
								var ps = Post(problemSpec, spec);
								Console.Write($" perfect score: {ps}");
							}) { IsBackground = true };
							t.Start();
							if (!t.Join(TimeSpan.FromSeconds(30)))
							{
								t.Abort();
								t.Join();
							}
						}
						Console.WriteLine();
					}
				}

				Console.WriteLine("Waiting 1 minute...");
				Thread.Sleep(TimeSpan.FromMinutes(1));
			}
		}

		private static double Post(ProblemSpec problemSpec, SolutionSpec solutionSpec)
		{
			try
			{
				var response = client.PostSolution(problemSpec.id, solutionSpec);
				repo.PutResponse(problemSpec.id, response);
				repo.PutSolution(problemSpec.id, solutionSpec);
				return JObject.Parse(response)["resemblance"].Value<double>();
			}
			catch (Exception e)
			{
				Console.WriteLine();
				Console.WriteLine(e);
				return 0;
			}
			finally
			{
				Thread.Sleep(1000);
			}
		}
	}
}
