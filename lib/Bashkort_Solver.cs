using System;
using System.Collections.Generic;
using System.Linq;
using lib.Api;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	[Explicit]
	/*3560, "1077/9709", "1077/9709", "16/9709", "16/9709", False
3852, "9999077/69994655", "9999077/69994655", "1116/69994655", "1116/69994655", False
3854, "479887358674887/2399436793374547", "479887358674887/2399436793374547", "112/2399436793374547", "112/2399436793374547", False
3965, "997/9981", "997/9981", "11/9981", "11/9981", True
4008, "11173797/89390393", "11173797/89390393", "17/89390393", "17/89390393", True
4010, "199191999799187/1195151998796239", "199191999799187/1195151998796239", "1117/1195151998796239", "1117/1195151998796239", True
4229, "1077/9709", "1077/9709", "16/9709", "16/9709", False
4234, "997/9981", "997/9981", "11/9981", "11/9981", True
4236, "1077/9709", "1077/9709", "16/9709", "16/9709", False
4237, "997/9981", "997/9981", "11/9981", "11/9981", True
4239, "1077/9709", "1077/9709", "16/9709", "16/9709", False
4242, "997/9981", "997/9981", "11/9981", "11/9981", True
5195, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5197, "997/9981", "997/9981", "11/9981", "11/9981", True
5199, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5203, "997/9981", "997/9981", "11/9981", "11/9981", True
5204, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5705, "997/9981", "997/9981", "11/9981", "11/9981", True
5724, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5725, "997/9981", "997/9981", "11/9981", "11/9981", True
5726, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5987, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5988, "1077/9709", "1077/9709", "16/9709", "16/9709", False
5990, "1077/9709", "1077/9709", "16/9709", "16/9709", False
6100, "1077/9709", "1077/9709", "16/9709", "16/9709", False
6101, "1077/9709", "1077/9709", "16/9709", "16/9709", False*/
	public class Bashkort_Solver
	{
		[Test]
		[Explicit]
		public void DoSomething_WhenSomething()
		{
			var repo = new ProblemsRepo();
			var problemSpec = repo.Get(3854);

			var denominator = (Rational)"2399436793374547";
			var v887 = "479887358674887"/denominator;
			var v112 = "112"/denominator;
			var rate = 5;

//			Console.Out.WriteLine(v887 * rate + v112);

			var sourcePoints = new List<Vector>();
			var destPoints = new List<Vector>();
			var facets = new List<Facet>();

			for (int iX = 0; iX <= rate; iX++)
				for (int iY = 0; iY <= rate; iY++)
				{
					sourcePoints.Add(new Vector(iX * v887, iY * v887));
					if ((iX + iY)%2 == 0)
					{
						destPoints.Add(new Vector(v887, v887));
					}
					else if (iX%2 == 0)
					{
						destPoints.Add(new Vector(0, v887));
					}
					else
					{
						destPoints.Add(new Vector(v887, 0));
					}
				}

			for (int iX = 0; iX < rate; iX++)
				for (int iY = 0; iY < rate; iY++)
				{
					if ((iX + iY)%2 == 0)
					{
						facets.Add(new Facet(iX*(rate + 1) + iY, iX*(rate + 1) + iY + 1, iX*(rate + 1) + iY + rate + 1));
						facets.Add(new Facet(iX*(rate + 1) + iY + 1, iX*(rate + 1) + iY + rate + 1, iX*(rate + 1) + iY + rate + 2));
					}
					else
					{
						facets.Add(new Facet(iX * (rate + 1) + iY, iX * (rate + 1) + iY + 1, iX * (rate + 1) + iY + rate + 2));
						facets.Add(new Facet(iX * (rate + 1) + iY, iX * (rate + 1) + iY + rate + 2, iX * (rate + 1) + iY + rate + 1));
					}
				}

			for (int iX = 0; iX <= rate; iX++)
			{
				sourcePoints.Add(new Vector(iX * v887, 1));
				destPoints.Add(new Vector(iX%2 == 0 ? 0 : v887, v887 - v112));
			}
			for (int iY = 0; iY <= rate; iY++)
			{
				sourcePoints.Add(new Vector(1, iY * v887));
				destPoints.Add(new Vector(v887 - v112, iY%2 == 0 ? 0 : v887));
			}

			for (int iX = 0; iX < rate; iX++)
			{
				var start = (rate + 1)*(rate + 1);
				facets.Add(new Facet(start + iX, start + iX + 1, (iX + 2) * ( rate+1 ) - 1, (iX + 1) * ( rate+1 ) - 1));
			}

			for (int iY = 0; iY < rate; iY++)
			{
				var start = (rate + 1)*(rate + 2);
				var start2 = (rate + 1) * rate;
				facets.Add(new Facet(start + iY, start + iY + 1, start2 + iY + 1, start2 + iY));
			}

			sourcePoints.Add(new Vector(1, 1));
			destPoints.Add(new Vector(v887 - v112, v887 - v112));
			facets.Add(new Facet((rate + 1) * (rate + 1) - 1, (rate + 1) * (rate + 2) - 1, sourcePoints.Count - 1, sourcePoints.Count - 2));
			
			var solution = new SolutionSpec(sourcePoints.ToArray(), facets.ToArray(), destPoints.ToArray());
			Console.Out.WriteLine($"size: {solution.Size()}; packed: {solution.Pack().Size()}");
			Console.Out.WriteLine($"facets: {solution.Facets.Length}; sourcePoints: {solution.SourcePoints.Length}; destPoints: {solution.DestPoints.Length}");

			//solution.CreateVisualizerForm().ShowDialog();


			var post = ProblemsSender.Post(solution, problemSpec.id);
			Console.Out.WriteLine(post);


			//			var v775 = (Rational)"479887358674775";
//			var rational = v887*5 + 112;

//			var rational = (denominator - numerator)/112;
//			var rational = v775/112;

//			rational.Reduce();
//			Console.Out.WriteLine($"{rational.Numerator}/{rational.Denomerator}");
//			Console.Out.WriteLine(2399436793374547);
			//Console.Out.WriteLine((double)(Rational)"479887358674775/2399436793374547");
		}

		//[TestCase(2414)]
		//[TestCase(2225)]
		//[TestCase(2267)]

//		[TestCase(2668)]
//		[TestCase(2777)]
//		[TestCase(2966)]
//		[TestCase(3180)]
//		[TestCase(3404)]
		[TestCase(3854)]

		public void Solve(int problemId)
		{
			var problemsRepo = new ProblemsRepo();
			var problem = problemsRepo.Get(problemId);
			var poly = problem.Polygons.Single();
//			var dx = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.X.Denomerator).Max();
//			var dy = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.Y.Denomerator).Max();
//			foreach (var x in Enumerable.Range(0, dx).Select(x => new Rational(x, dx)))
//				foreach (var y in Enumerable.Range(0, dy).Select(y => new Rational(y, dy)))
			{
//					var shift = new Vector(x, y);
				var shift = new Vector(0, 0);
				var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
				var solution = ConvexPolygonSolver.Solve(poly.GetConvexBoundary(), initialSolution);
				var packedSolution = solution.Pack();
				var packedSolutionSize = packedSolution.Size();
				var solutionSize = solution.Size();
				Console.WriteLine($"{shift}: {solutionSize}; packed: {packedSolutionSize}");
				if (packedSolutionSize <= 5000)
				{
					ProblemsSender.Post(packedSolution, problemId, false);
//						return;
				}
			}
		}
	}
}