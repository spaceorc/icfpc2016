using System;
using System.Collections.Generic;
using System.Threading;
using lib;
using lib.Api;
using System.Linq;

namespace AutoSolver
{
	class Program
	{
		private static readonly ApiClient client = new ApiClient();
		private static readonly ProblemsRepo repo = new ProblemsRepo();

		static void Main2(string[] args)
		{
			var newProblems = DownloadNewProblems();
			ConvexPolygonSolver.SolveAll(newProblems);
			ConvexPolygonSolver.SolveAllNotSolvedPerfectly();
		}

		static void Main(string[] args)
		{
			while (true)
			{
				if (args.Length > 0 && args[0] == "-d")
					DownloadNewProblems();

				Console.WriteLine("Solving...");
				repo.GetAllNotSolvedPerfectly()
					.OrderBy(EstimateDifficulty)
					.AsParallel()
					.ForAll(problemSpec =>
					{
						Console.WriteLine($"Solving {problemSpec.id}...");
						SolveWithProjectionSolverRunner(problemSpec);
					});

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

		private static List<ProblemSpec> DownloadNewProblems()
		{
			Console.WriteLine("Downloading new problems...");
			var snapshot = client.GetLastSnapshot();
			var newProblems = new List<ProblemSpec>();
			foreach (var problem in snapshot.Problems)
			{
				if (repo.Find(problem.Id) == null)
				{
					var problemSpec = client.GetBlob(problem.SpecHash);
					repo.Put(problem.Id, problemSpec);
					newProblems.Add(repo.Get(problem.Id));
					Console.WriteLine($"Downloaded problem {problem.Id}");
				}
			}
			return newProblems;
		}

		private static void SolveWithProjectionSolverRunner(ProblemSpec problemSpec)
		{
			var originalities = new[] { 0.5 };
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
								ps = ProblemsSender.Post(solution, problemSpec.id);
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

			solutionFoundEvent.WaitOne(TimeSpan.FromSeconds(15));

			foreach (var t in threads)
				if (t.IsAlive)
				{
					t.Abort();
					t.Join();
				}
		}
	}
}
