using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SquareConstructor;

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

		public static void SolveAll()
		{
			var apiClient = new ApiClient();
			var problemsRepo = new ProblemsRepo();
			foreach (var problem in problemsRepo.GetAll().Where(x => GetProblemResemblance(x.id) < 1.0))
			{
				if (problem.Polygons.Length == 1 && problem.Polygons.Single().IsConvex())
				{
					Console.Write($"Problem {problem.id} is convex! Solvnig...");
					var solution = TrySolve(problem);
					if (solution == null)
						continue;
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

		public static SolutionSpec TrySolve(ProblemSpec problem)
		{
			if (problem.Polygons.Length > 1 || !problem.Polygons.Single().IsConvex())
				return null;

			SolutionSpec initialSolution = null;
			var problemPolygon = problem.Polygons[0];
			var t = new Thread(() =>
			{
				initialSolution = GetInitialSolutionAlongRationalEdge(problemPolygon) ?? new ImperfectSolver().SolveMovingAndRotatingInitialSquare(problem);
			})
			{ IsBackground = true };
			t.Start();
			if (!t.Join(TimeSpan.FromSeconds(10)))
			{
				t.Abort();
				t.Join();
				Console.WriteLine("ImperfectSolver sucks! Skipping");
				return null;
			}
			return Solve(problemPolygon, initialSolution);
		}

		private static SolutionSpec GetInitialSolutionAlongRationalEdge(Polygon problemPolygon)
		{
			var longestRationalEdge = problemPolygon.Segments.Where(x => Arithmetic.IsSquare(x.QuadratOfLength)).OrderBy(x => x.QuadratOfLength).LastOrDefault();
			if (longestRationalEdge == null)
				return null;
			var initialSolutionAlongRationalEdge = GetInitialSolutionAlongRationalEdge(longestRationalEdge);
			var projections = problemPolygon.Vertices.Select(x => x.GetProjectionOntoLine(longestRationalEdge)).ToList();
			var minX = projections.Min(p => p.X);
			var minY = projections.Min(p => p.Y);
			var maxX = projections.Max(p => p.X);
			var maxY = projections.Max(p => p.Y);
			var projectionsCenter = new Vector((maxX +minX)/2, (maxY + minY) / 2);
			var squareEdgeCenter = (initialSolutionAlongRationalEdge.DestPoints[0] + initialSolutionAlongRationalEdge.DestPoints[1])/2;
			var shift = projectionsCenter - squareEdgeCenter;
			return initialSolutionAlongRationalEdge.Shift(shift);
		}

		public static SolutionSpec GetInitialSolutionAlongRationalEdge(Segment rationalEdge)
		{
			var initialSolution = SolutionSpec.CreateTrivial(x => x + rationalEdge.Start);
			var edgeLen = new Rational(Arithmetic.Sqrt(rationalEdge.QuadratOfLength.Numerator), Arithmetic.Sqrt(rationalEdge.QuadratOfLength.Denomerator));
			var a = rationalEdge.ToVector()/edgeLen;
			var b = Vector.Parse("1,0");
			if (b.VectorProdLength(a) == 0)
			{
				if (b.ScalarProd(a)> 0)
					return initialSolution;
				return initialSolution.Reflect(rationalEdge);
			}
			var bisect = new Segment(rationalEdge.Start, a + b + rationalEdge.Start);
			return initialSolution.Reflect(bisect).Reflect(rationalEdge);
		}

		public static double GetProblemResemblance(int problemId)
		{
			var response = new ProblemsRepo().FindResponse(problemId);
			return response == null ? 0.0 : GetResemblance(response);
		}

		public static double GetResemblance(string response)
		{
			return JObject.Parse(response)["resemblance"].Value<double>();
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
			var dx = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.X.Denomerator).Max();
			var dy = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.Y.Denomerator).Max();
			foreach (var x in Enumerable.Range(0, dx).Select(x => new Rational(x, dx)))
				foreach (var y in Enumerable.Range(0, dy).Select(y => new Rational(y, dy)))
				{
					var shift = new Vector(x, y);
					//var shift = new Vector(0, 0);
					var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
					var solution = ConvexPolygonSolver.Solve(poly, initialSolution);
					var packedSolution = solution.Pack();
					var packedSolutionSize = packedSolution.Size();
					var solutionSize = solution.Size();
					if (packedSolutionSize <= 5000)
					{
						Console.WriteLine($"{shift}: {solutionSize}; packed: {packedSolutionSize}");

						try
						{
							var response = apiClient.PostSolution(problem.id, packedSolution);
							var resemblance = ConvexPolygonSolver.GetResemblance(response);
							if (resemblance > ConvexPolygonSolver.GetProblemResemblance(problem.id))
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
				}
		}

		[Test]
		public void GetInitialSolutionAlongRationalEdge_6()
		{
			var solution = ConvexPolygonSolver.GetInitialSolutionAlongRationalEdge("15/29,-6/29 35/29,15/29");
			//solution.CreateVisualizerForm(true).ShowDialog();
			solution.DestPoints.Should().Equal("15/29,-6/29|35/29,15/29|14/29,35/29|-6/29,14/29".Split('|').Select(Vector.Parse).ToArray());
		}

		[Test]
		public void GetInitialSolutionAlongRationalEdge_82()
		{
			var solution = ConvexPolygonSolver.GetInitialSolutionAlongRationalEdge("41/68,58/71 27/68,58/71");
			//solution.CreateVisualizerForm(true).ShowDialog();
			solution.DestPoints.Should().Equal("41/68,58/71|109/68,58/71|109/68,-13/71|41/68,-13/71".Split('|').Select(Vector.Parse).ToArray());
		}

		[Test]
		public void GetInitialSolutionAlongRationalEdge()
		{
			var solution = ConvexPolygonSolver.GetInitialSolutionAlongRationalEdge("1,2 1,3");
			//solution.CreateVisualizerForm(true).ShowDialog();
			solution.DestPoints.Should().Equal("1,2|1,3|0,3|0,2".Split('|').Select(Vector.Parse).ToArray());
		}
	}
}