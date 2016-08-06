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

		public static bool IsConvexViaVectorProd(this Polygon poly)
		{
			for (int i = 0; i < poly.Segments.Length; i++)
			{
				var thisEdge = poly.Segments[i];
				var nextEdge = poly.Segments[(i + 1)%poly.Segments.Length];
				var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
				if (prod < 0)
					return false;
			}
			return true;
		}
	}

	[TestFixture, Explicit]
	public class ConvexPolygonSolver_Should
	{
		[Test]
		public void Solve()
		{
			var problemId = 2225;
			var problemsRepo = new ProblemsRepo();
			var problem = problemsRepo.Get(problemId);
			var poly = problem.Polygons.Single();

			foreach (var x in Enumerable.Range(9, 1).Select(x => new Rational(x, 131)))
				foreach (var y in Enumerable.Range(8, 1).Select(y => new Rational(y, 245)))
				{
					var shift = new Vector(x, y);
					var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
					var solution = ConvexPolygonSolver.Solve(poly, initialSolution);
					var solutionSize = solution.ToString().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
					if (solutionSize <= 5080)
					{
						var packedSolution = solution.Pack();
						var packedSolutionSize = packedSolution.ToString().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
						Console.WriteLine($"{shift}: {solutionSize}; packed: {packedSolutionSize}");
					}
					//solution.CreateVisualizerForm(true).ShowDialog();
				}


			//solution.CreateVisualizerForm(false).ShowDialog();


/*
			problemsRepo.PutSolution(problemId, solution);

			var response = new ApiClient().PostSolution(problemId, solution);
			problemsRepo.PutResponse(problemId, response);*/
		}

		public static void SolveAll()
		{
			var apiClient = new ApiClient();
			var problemsRepo = new ProblemsRepo();
			foreach (var problem in problemsRepo.GetAll().Where(x => GetProblemResemblance(x.id) < 1.0))
			{
				if (problem.Polygons.Length == 1 && problem.Polygons.Single().IsConvexViaVectorProd())
				{
					Console.Write($"Problem {problem.id} is convex! Solvnig...");
					SolutionSpec initialSolution = null;
					var t = new Thread(() =>
					{
						initialSolution = new ImperfectSolver().SolveMovingAndRotatingInitialSquare(problem);
					})
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