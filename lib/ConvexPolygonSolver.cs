using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace lib
{
	public static class ConvexPolygonSolver
	{
		public static SolutionSpec Solve(Polygon poly, SolutionSpec initialSolution)
		{
			if (poly.GetSignedSquare() < 0)
				throw new InvalidOperationException("poly.GetSignedSquare() < 0");

			var solution = initialSolution ?? SolutionSpec.CreateTrivial(x => x);
			while (true)
			{
				var foldsCount = 0;
				foreach (var segment in poly.Segments)
				{
					var s = solution.Fold(segment);
					if (s != solution)
						foldsCount++;
					solution = s;
				}
				if (foldsCount == 0)
					return solution;
			}
		}
			}

	[TestFixture, Explicit]
	public class ConvexPolygonSolver_Should
	{
		//[TestCase(2414)]
		//[TestCase(2225)]
		[TestCase(2267)]

		//[TestCase(2668)]
		//[TestCase(2777)]
		//[TestCase(2966)]
		//[TestCase(3180)]
		public void Solve(int problemId)
		{
			var problemsRepo = new ProblemsRepo();
			var problem = problemsRepo.Get(problemId);
			var poly = problem.Polygons.Single();
			var apiClient = new ApiClient();
			var dx = (int)problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.X.Denomerator).Max();
			var dy = (int)problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.Y.Denomerator).Max();
			foreach (var x in Enumerable.Range(0, dx).Select(x => new Rational(x, dx)))
				foreach (var y in Enumerable.Range(0, dy).Select(y => new Rational(y, dy)))
				{
					var shift = new Vector(x, y);
					//var shift = new Vector(0, 0);
					var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
					var solution = ConvexPolygonSolver.Solve(poly, initialSolution);
					var packedSolution = solution.Pack();
					var packedSolutionSize = packedSolution.ToString().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
					var solutionSize = solution.ToString().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
					if (packedSolutionSize <= 5000)
					{
						Console.WriteLine($"{shift}: {solutionSize}; packed: {packedSolutionSize}");

						try
						{
							var response = apiClient.PostSolution(problem.id, packedSolution);
							var resemblance = GetResemblance(response);
							if (resemblance > GetProblemResemblance(problem.id))
							{
								problemsRepo.PutSolution(problem.id, packedSolution);
								problemsRepo.PutResponse(problem.id, response);
								Console.Write("Solution improved! ");
								return;
							}
							Console.WriteLine(resemblance);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}
				}
					//solution.CreateVisualizerForm(true).ShowDialog();
				}
*/

			//solution.CreateVisualizerForm(false).ShowDialog();


/*
			problemsRepo.PutSolution(problemId, solution);

			var response = new ApiClient().PostSolution(problemId, solution);
			problemsRepo.PutResponse(problemId, response);*/
		}

		private static SolutionSpec Solve2225(Vector shift, Polygon poly)
		{
			var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
			return ConvexPolygonSolver.Solve(poly, initialSolution);
		}

		public static void SolveAll()
		{
			var apiClient = new ApiClient();
			var problemsRepo = new ProblemsRepo();
			foreach (var problem in problemsRepo.GetAll().Where(x => GetProblemResemblance(x.id) < 1.0)
				.Where(p => new[] {2267,2414,2668,2777, 2966,3180,}.Contains(p.id)))
			{
				if (problem.Polygons.Length == 1 && problem.Polygons.Single().IsConvex())
				{
					Console.Write($"Problem {problem.id} is convex! Solvnig...");
					SolutionSpec initialSolution = null;
					var t = new Thread(() => { initialSolution = new ImperfectSolver().SolveMovingAndRotatingInitialSquare(problem); })
					{ IsBackground = true };
					t.Start();
					if (!t.Join(TimeSpan.FromSeconds(10)))
					{
						t.Abort();
						t.Join();
						Console.WriteLine("ImperfectSolver sucks! Skipping");
						continue;
					}
					var solution = ConvexPolygonSolver.Solve(problem.Polygons[0], initialSolution).Pack();
					try
					{
						var response = apiClient.PostSolution(problem.id, solution);
						var resemblance = GetResemblance(response);
						if (resemblance > GetProblemResemblance(problem.id))
						{
							problemsRepo.PutSolution(problem.id, solution);
							problemsRepo.PutResponse(problem.id, response);
							Console.Write("Solution improved! ");
						}
						Console.WriteLine(resemblance);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}
		}

		private static double GetProblemResemblance(int problemId)
		{
			var response = new ProblemsRepo().FindResponse(problemId);
			return response == null ? 0.0 : GetResemblance(response);
		}

		private static double GetResemblance(string response)
		{
			return JObject.Parse(response)["resemblance"].Value<double>();
		}
	}
}