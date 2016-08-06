using System;
using System.Linq;
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

		public static bool IsConvex(this Polygon poly)
		{
			for (int i = 0; i < poly.Segments.Length; i++)
			{
				var thisEdge = poly.Segments[i];
				var nextEdge = poly.Segments[(i+1)%poly.Segments.Length];
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

			foreach (var x in Enumerable.Range(0, 131).Select(x => new Rational(x, 131)))
			foreach (var y in Enumerable.Range(0, 245).Select(y => new Rational(y, 245)))
				{
					var shift = new Vector(x, y);
					var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
					var solution = ConvexPolygonSolver.Solve(poly, initialSolution);
					var solutionSize = solution.ToString().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
					if (solutionSize <= 5080)
						Console.WriteLine($"{shift}: {solutionSize}");
					//solution.CreateVisualizerForm(true).ShowDialog();
				}


			//solution.CreateVisualizerForm(false).ShowDialog();


/*
			problemsRepo.PutSolution(problemId, solution);

			var response = new ApiClient().PostSolution(problemId, solution);
			problemsRepo.PutResponse(problemId, response);*/
		}

		[Test]
		[Explicit]
		public void SolveAll()
		{
			var apiClient = new ApiClient();
			var problemsRepo = new ProblemsRepo();
			foreach (var problem in problemsRepo.GetAll().Where(ProblemIsNotSolved))
			{
				if (problem.Polygons.Length == 1 && problem.Polygons.Single().IsConvex())
				{
					Console.Out.Write($"Solving convex problem: {problem.id}...");
					var solution = ConvexPolygonSolver.Solve(problem.Polygons[0], null);
					try
					{
						var response = apiClient.PostSolution(problem.id, solution);
						problemsRepo.PutSolution(problem.id, solution);
						problemsRepo.PutResponse(problem.id, response);
						Console.Out.WriteLine(GetResemblance(response));
					}
					catch (Exception e)
					{
						Console.Out.WriteLine(e);
					}
				}
			}
		}

		private static bool ProblemIsNotSolved(ProblemSpec problem)
		{
			var response = new ProblemsRepo().FindResponse(problem.id);
			return response == null || GetResemblance(response) < 1.0;
		}

		private static double GetResemblance(string response)
		{
			return JObject.Parse(response)["resemblance"].Value<double>();
		}
	}