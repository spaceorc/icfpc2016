using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using lib.Api;
using NUnit.Framework;
using SquareConstructor;

namespace lib
{
	public static class ConvexPolygonSolver
	{
		private static readonly ProblemsRepo problemsRepo = new ProblemsRepo();

		public static void SolveAllNotSolvedPerfectly()
		{
			var snapshotJson = problemsRepo.GetSnapshot(new ApiClient());
			var problemToRank = snapshotJson.Problems.ToDictionary(p => p.Id, p => p.ExpectedScore());
			var allNotSolvedPerfectly = problemsRepo.GetAllNotSolvedPerfectly().OrderByDescending(x => problemToRank[x.id]);
			SolveAll(allNotSolvedPerfectly.ToList());
		}

		public static void SolveAll(List<ProblemSpec> problems)
		{
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < problems.Count; i++)
			{
				var problem = problems[i];
				Console.Write($"{sw.Elapsed:c} Problem {problem.id:0000} ({i:0000}/{problems.Count:0000}) ");
				var solution = TrySolveSingleProblem(problem);
				if (solution != null)
					ProblemsSender.Post(solution, problem.id, storeSolutionWithSameResemblance: true);
				else
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Out.Write($" no solution found ");
					Console.ResetColor();
				}
				Console.WriteLine();
			}
		}

		public static SolutionSpec TrySolveSingleProblem(ProblemSpec problem)
		{
			Polygon convexPolygon;
			var positivePolygon = problem.Polygons.Single(x => x.GetSignedSquare() > 0);
			if (positivePolygon.IsConvex())
			{
				convexPolygon = positivePolygon;
				Console.Write("CONVEX ");
			}
			else
			{
				convexPolygon = positivePolygon.GetConvexBoundary();
				Console.Write("NOT_CONVEX ");
			}
			return TrySolveInOneShot(problem, convexPolygon);
		}

		public static SolutionSpec TrySolveInBestShot(ProblemSpec problem, Polygon convexPolygon)
		{
			var solution = EnumerateInitialSolutions(convexPolygon)
				.Take(20)
				.Select(x => Solve(convexPolygon, x, TimeSpan.FromSeconds(1)))
				.OrderByDescending(x => SolutionEvaluator.EvaluateX(problem, x, dpi: 200))
				.FirstOrDefault();
			return solution;
		}

		private static IEnumerable<SolutionSpec> EnumerateInitialSolutions(Polygon convexPolygon)
		{
			var rationalEdges = convexPolygon.Segments.Where(x => Arithmetic.IsSquare(x.QuadratOfLength)).OrderByDescending(x => x.QuadratOfLength);
			foreach (var rationalEdge in rationalEdges)
			{
				yield return GetInitialSolutionAtTheMiddleOfRationalEdge(convexPolygon, rationalEdge);
			}
		}

		public static SolutionSpec TrySolveInOneShot(ProblemSpec problem, Polygon convexPolygon)
		{
			SolutionSpec solution = null;
			var initialSolution = TryGetInitialSolution(problem, convexPolygon);
			if (initialSolution != null)
				solution = Solve(convexPolygon, initialSolution);
			return solution;
		}

		public static SolutionSpec Solve(Polygon poly, SolutionSpec initialSolution, TimeSpan? timeout = null)
		{
			if (poly.GetSignedSquare() < 0)
				throw new InvalidOperationException("poly.GetSignedSquare() < 0");
			timeout = timeout ?? TimeSpan.FromSeconds(20);
			var sw = Stopwatch.StartNew();
			var solution = initialSolution ?? SolutionSpec.CreateTrivial(x => x);
			do
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
			} while (sw.Elapsed < timeout);
			Console.Write($"Solution folding failed to complete in: {timeout.Value} ");
			return solution;
		}

		public static SolutionSpec TryGetInitialSolution(ProblemSpec problem, Polygon problemPolygon, TimeSpan? timeout = null)
		{
			timeout = timeout ?? TimeSpan.FromSeconds(10);
			SolutionSpec initialSolution = null;
			var t = new Thread(() => { initialSolution = GetInitialSolutionByLongestRationalEdge(problemPolygon) ?? new ImperfectSolver().SolveMovingAndRotatingInitialSquare(problem); })
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

		private static SolutionSpec GetInitialSolutionByLongestRationalEdge(Polygon problemPolygon)
		{
			var longestRationalEdge = problemPolygon.Segments.Where(x => Arithmetic.IsSquare(x.QuadratOfLength)).OrderBy(x => x.QuadratOfLength).LastOrDefault();
			if (longestRationalEdge == null)
				return null;
			return GetInitialSolutionAtTheMiddleOfRationalEdge(problemPolygon, longestRationalEdge);
		}

		private static SolutionSpec GetInitialSolutionAtTheMiddleOfRationalEdge(Polygon problemPolygon, Segment rationalEdge)
		{
			var initialSolutionAlongRationalEdge = GetInitialSolutionAlongRationalEdge(rationalEdge);
			var projections = problemPolygon.Vertices.Select(x => x.GetProjectionOntoLine(rationalEdge)).ToList();
			var minX = projections.Min(p => p.X);
			var minY = projections.Min(p => p.Y);
			var maxX = projections.Max(p => p.X);
			var maxY = projections.Max(p => p.Y);
			var projectionsCenter = new Vector((maxX + minX)/2, (maxY + minY)/2);
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
				if (b.ScalarProd(a) > 0)
					return initialSolution;
				return initialSolution.Reflect(rationalEdge);
			}
			var bisect = new Segment(rationalEdge.Start, a + b + rationalEdge.Start);
			return initialSolution.Reflect(bisect).Reflect(rationalEdge);
		}
	}

	[TestFixture]
	public class ConvexPolygonSolver_Should
	{
		//[TestCase(2414)]
		//[TestCase(2225)]
		//[TestCase(2267)]

		[TestCase(2668)]
		[TestCase(2777)]
		[TestCase(2966)]
		[TestCase(3180)]
		[TestCase(3404)]

		[Explicit]
		public void Solve(int problemId)
		{
			var problemsRepo = new ProblemsRepo();
			var problem = problemsRepo.Get(problemId);
			var poly = problem.Polygons.Single();
			var dx = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.X.Denomerator).Max();
			var dy = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.Y.Denomerator).Max();
			foreach (var x in Enumerable.Range(0, dx).Select(x => new Rational(x, dx)))
				foreach (var y in Enumerable.Range(0, dy).Select(y => new Rational(y, dy)))
				{
					var shift = new Vector(x, y);
//					var shift = new Vector(0, 0);
					var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
					var solution = ConvexPolygonSolver.Solve(poly, initialSolution);
					var packedSolution = solution.Pack();
					var packedSolutionSize = packedSolution.Size();
					var solutionSize = solution.Size();
					if (packedSolutionSize <= 5000)
					{
						Console.WriteLine($"{shift}: {solutionSize}; packed: {packedSolutionSize}");
						ProblemsSender.Post(packedSolution, problemId, false);
						return;
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