using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using lib;
using lib.Api;

namespace SquareConstructor
{
	class Program
	{
		private static readonly ApiClient client = new ApiClient();
		private static readonly ProblemsRepo repo = new ProblemsRepo();

		static void Main2(string[] args)
		{
			var problem = File.ReadAllText("../../../problems/031.spec.txt");
			var spec = ProblemSpec.Parse(problem);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form1 = new PolygonsAndSegmentsForm();
			form1.SetData(spec.Polygons, spec.Segments);
			Task.Run(() => Application.Run(form1));

			var solver = new ConstructorSolver(spec);
			var solution = SolutionPacker.Pack(solver.Work());

			var form = new PolygonsAndSegmentsForm();
			form.SetData(solution.Polygons, new Segment[0]);
			Application.Run(form);
		}

		static void Main(string[] args)
		{
			while (true)
			{
				if (args.Length > 0 && args[0] == "-d")
					DownloadNewProblems();

				Console.WriteLine("Solving...");
				foreach (var problemSpec in repo.GetAllNotSolvedPerfectly().OrderBy(EstimateDifficulty).Skip(100))
				{
					if(DateTime.Now.Hour > 5)
						return;
					Console.Write($"Solving {problemSpec.id}...");
					Solve(problemSpec);
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
			double smallSegCount = problem.Segments.Count(s => s.IrrationalLength < 1d / 8);
			double blackPoints = problem.Points.Count(p => !ratSegments.Any(s => s.IsEndpoint(p)));
			return ratSegCount / 10 + smallSegCount / 3 + blackPoints;
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

		private static void Solve(ProblemSpec problemSpec)
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
							var solution = SolutionPacker.Pack(new ConstructorSolver(problemSpec).Work());
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
