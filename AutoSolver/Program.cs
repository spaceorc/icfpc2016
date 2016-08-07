using System;
using System.Threading;
using lib;
using lib.Api;
using System.Linq;

namespace AutoSolver
{
	class Program
	{
		private static ApiClient client;
		private static ProblemsRepo repo;

		static void Main2(string[] args)
		{
			ConvexPolygonSolver.SolveAllNotSolvedPerfectly();
		}

		static void Main(string[] args)
		{
			repo = new ProblemsRepo();
			client = new ApiClient();

			while (true)
			{
				//DownloadNewProblems();

				Console.WriteLine("Solving...");
				foreach (var problemSpec in repo.GetAllNotSolvedPerfectly().OrderBy(EstimateDifficulty))
				{
					Console.Write($"Solving {problemSpec.id}...");
					SolveWithProjectionSolverRunner(problemSpec);
					Console.WriteLine();
				}

				Console.WriteLine("Waiting 1 minute...");
				Thread.Sleep(TimeSpan.FromMinutes(1));
			}
		}

		private static double EstimateDifficulty(ProblemSpec problem)
		{
			var ratSegments = problem.Segments.Where(s => Arithmetic.IsSquare(s.QuadratOfLength)).ToList();
			double ratSegCount = ratSegments.Count;
			double smallSegCount = problem.Segments.Count(s => s.IrrationalLength < 1d/8);
			double blackPoints = problem.Points.Count(p => !ratSegments.Any(s => s.IsEndpoint(p)));
			return ratSegCount /10 + smallSegCount / 3 + blackPoints;
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
							var solution = UltraSolver.AutoSolve(problemSpec);
							if (solution == null || solution.Size() > 5000 || !solution.AreFacetsValid())
								return;
							double ps;
							lock (mutex)
							{
								Console.WriteLine(" posting... ");
								ps = ProblemsSender.Post(problemSpec, solution);
								Console.Write($" perfect score: {ps}");
							}
							if (ps == 1.0)
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

			solutionFoundEvent.WaitOne(TimeSpan.FromSeconds(10));

			foreach (var t in threads)
				if (t.IsAlive)
				{
					t.Abort();
					t.Join();
				}
		}
	}
}
