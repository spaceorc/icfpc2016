using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			new Bashkort_Solver().PostSolutionsForSameProblems();
		}

		static void Main(string[] args)
		{
			if (args.Contains("--yura"))
			{
				ShowIntro("SolveWithProjectionSolverRunner");

				for (var iteration = 0;; iteration++)
				{
					if (iteration > 0 || args.Contains("-d"))
						DownloadNewProblems();

					Console.WriteLine("Solving...");
					repo.GetAllNotSolvedPerfectly()
						.OrderBy(EstimateDifficulty)
						.AsParallel().WithDegreeOfParallelism(8)
						.ForAll(problemSpec =>
						{
							Console.WriteLine($"Solving {problemSpec.id}...");
							SolveWithProjectionSolverRunner(problemSpec);
						});

					Console.WriteLine("Waiting 1 minute...");
					Thread.Sleep(TimeSpan.FromMinutes(1));
				}
			}
			else if (args.Contains("--convex"))
			{
				ShowIntro("ConvexPolygonSolver");
				var newProblems = DownloadNewProblems();
				ConvexPolygonSolver.SolveAll(newProblems);
				ConvexPolygonSolver.SolveAllNotSolvedPerfectly();
			}
			else
			{
				ShowIntro("DumbSolver");
				var newProblems = DownloadNewProblems();
				Console.Out.WriteLine($"newProblems.Count: {newProblems.Count}");
				var noSolutionProblems = repo.GetAllNotSolvedPerfectly().Where(x => repo.FindResponse(x.id) == null).Skip(15).ToList();
				Console.Out.WriteLine($"noSolutionProblems.Count: {noSolutionProblems.Count}");
				var sw = Stopwatch.StartNew();
				for (var i = 0; i < noSolutionProblems.Count; i++)
				{
					var problem = noSolutionProblems[i];
					Console.Write($"{sw.Elapsed:c} Problem {problem.id:0000} ({i:0000}/{noSolutionProblems.Count:0000}) ");
					var solution = TryGetInitialSolution(problem);
					if (solution != null)
						ProblemsSender.Post(solution, problem.id);
					Console.WriteLine();
				}
			}
		}

		private static SolutionSpec TryGetInitialSolution(ProblemSpec problem, TimeSpan? timeout = null)
		{
			timeout = timeout ?? TimeSpan.FromSeconds(10);
			SolutionSpec initialSolution = null;
			var t = new Thread(() => { initialSolution = new ImperfectSolver().SolveMovingAndRotatingInitialSquare(problem); })
			{ IsBackground = true };
			t.Start();
			if (!t.Join(timeout.Value))
			{
				t.Abort();
				t.Join();
				Console.Write($"Failed to get initial solution in {timeout}! Skipping");
			}
			return initialSolution;
		}

		private static void ShowIntro(string algorithmName)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Out.WriteLine($"Running Autosolver using {algorithmName}");
			Console.ResetColor();
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
			var thread = new Thread(() =>
			{
				try
				{
					var solution = UltraSolver.AutoSolve(problemSpec);
					if (solution == null || solution.Size() > 5000 || !solution.AreFacetsValid())
					{
						return;
					}
					Console.WriteLine(" posting... ");
					var ps = ProblemsSender.Post(solution, problemSpec.id);
					Console.Write($" perfect score: {ps}");
				}
				catch(Exception e)
				{
					if(e is ThreadAbortException)
						return;
					Console.WriteLine($"Exception in ProjectionSolverRunner: {e}");
				}
			})
			{ IsBackground = true };
			thread.Start();

			if (!thread.Join(TimeSpan.FromSeconds(25)))
			{
				thread.Abort();
				thread.Join();
			}
		}
	}
}
