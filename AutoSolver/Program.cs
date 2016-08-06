using System;
using System.Collections.Generic;
using System.Threading;
using lib;
using Newtonsoft.Json.Linq;
using lib.Api;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSolver
{
	class Program
	{
		private static ApiClient client;
		private static ProblemsRepo repo;

		static void Main(string[] args)
		{
			ConvexPolygonSolver.SolveAll();
		}

		static void Main2(string[] args)
		{
			repo = new ProblemsRepo();
			client = new ApiClient();

			while (true)
			{
				DownloadNewProblems();

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
						var score = ProblemsSender.Post(problemSpec, solutionSpec);
						Console.Write($" imperfect score: {score}");

						if (score != 1.0)
							SolveWithProjectionSolverRunner(problemSpec);
						Console.WriteLine();
					}
				}

				Console.WriteLine("Waiting 1 minute...");
				Thread.Sleep(TimeSpan.FromMinutes(1));
			}
		}

		private static void DownloadNewProblems()
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
		}

		private static void SolveWithProjectionSolverRunner(ProblemSpec problemSpec)
		{
			var originalities = new[] { 0, 0.3, 0.95 };
			var mutex = new object();
			var solutionFoundEvent = new ManualResetEvent(false);
			var threads = originalities
				.Select(coeff =>
				{
					var thread = new Thread(() =>
					{
						try
						{
							var spec = ProjectionSolverRunner.Solve(problemSpec);
							if (spec == null)
								return;
							double ps;
							lock (mutex)
							{
								ps = ProblemsSender.Post(problemSpec, spec);
								Console.Write($" perfect score: {ps}");
							}
							if(ps == 1.0)
								solutionFoundEvent.Set();
						}
						catch (Exception e)
						{
							if (e is ThreadAbortException)
								return;
							Console.WriteLine($"Exception in ProjectionSolverRunner: {e}");
						}
					})
					{ IsBackground = true };
					thread.Start();
					return thread;
				})
				.ToArray();

			solutionFoundEvent.WaitOne(TimeSpan.FromSeconds(30));

			foreach(var t in threads)
				if(t.IsAlive)
				{
					t.Abort();
					t.Join();
				}
		}
	}
}
