using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public class D4Problem
	{
		public static SolutionSpec D4(Rational a, Rational b, Rational c, Rational d)
		{
			var src = $"1/2,1/2 {1 - b},0 1/2,0 0,0 0,1/2 0,1 1/2,1 {1 - a},1 1,1 1,{1 - a} 1,1/2 1,{b} 1,0 {c},{1 - c} {d},{d}".ToPoints();
			Console.WriteLine(src.Length);
			var dst = src.ToArray();
			Console.WriteLine(dst.Length);
			Action<int, int, int> r = (who, m1, m2) => dst[who] = dst[who].Reflect(dst[m1], dst[m2]);
			r(3, 2, 14);
			dst[4] = dst[2];
			r(13, 0, 14); r(13, 0, 4);
			r(5, 0, 14); r(5, 0, 4); r(5, 4, 13);
			dst[6] = dst[2];
			r(7, 0, 14); r(7, 0, 4); r(7, 0, 13);
			r(12, 0, 1);
			r(11, 0, 1); r(11, 0, 12);
			r(10, 0, 1); r(10, 0, 12); r(10, 0, 11);
			r(9, 0, 1); r(9, 0, 12); r(9, 0, 11); r(9, 0, 10);
			r(8, 0, 1); r(8, 0, 12); r(8, 0, 11); r(8, 0, 10); r(8, 0, 9);
			var facets = new Facet[]
			{
				new Facet(0, 1,2,14),
				new Facet(0, 14,4),
				new Facet(0, 4,13),
				new Facet(0, 13,6,7),
				new Facet(0, 7,8),
				new Facet(0, 8,9),
				new Facet(0, 9,10),
				new Facet(0, 10,11),
				new Facet(0, 11,12),
				new Facet(0, 12,1),
				new Facet(3,4,14),
				new Facet(3,2,14),
				new Facet(4,13,5),
				new Facet(6,13,5),
			};
			return new SolutionSpec(src, facets, dst);
		}

		[Test, RequiresThread(ApartmentState.STA)]
		public void PrintD4()
		{
			var sol = D4(new Rational(9, 20), new Rational(19, 40), new Rational(12, 40), new Rational(13, 40));

			sol.AreFacetsValid(1).Should().BeTrue();

			Console.WriteLine(sol.Pack());
			sol.CreateVisualizerForm(1).ShowDialog();
		}

	}

	public class RandomFolder
	{
		public SolutionSpec MakeFinalFolds(SolutionSpec sol, Rational k)
		{
			return MakeFinalFolds(sol, k, sol.DestPoints);
		}

		public SolutionSpec MakeFinalFolds(SolutionSpec sol, Rational k, params int[] pointIndices)
		{
			return MakeFinalFolds(sol, k, pointIndices.Select(i => sol.DestPoints[i]));
		}

		private static SolutionSpec MakeFinalFolds(SolutionSpec sol, Rational k, IEnumerable<Vector> points)
		{
			//var starts = segments.ToLookup(s => s.Start);
			//var ends = segments.ToLookup(s => s.End);
			var count = 0;
			foreach (var p in points)
			{
				if (count == 3) break;
				var segs = sol.GetAllDestSegments().Where(s => s.Start.Equals(p) || s.End.Equals(p));
				foreach (var segment in segs)
				{
					var otherPoint = segment.Start.Equals(p) ? segment.End : segment.Start;
					var direction = otherPoint - p;
					var near = p + direction * k;
					var mirror = new Segment(near, near + new Vector(direction.Y, -direction.X));
					var sol2 = sol.Fold(mirror);
					//Console.WriteLine($"Increase = {sol2.Facets.Length - sol.Facets.Length}");
					if (sol2.Facets.Length == sol.Facets.Length + 2)
					{
						sol = sol2.Fold(mirror.Move(direction * k));
						count++;
						//Console.WriteLine($"win at {p}");
						break;
					}
				}
			}
			return sol;
		}

		[Test]
		public void DoSomething_WhenSomething()
		{
			var sol = D4Problem.D4("9/20", "19/40", "12/40", "13/40");
			//Console.WriteLine(Enumerable.Range(100, 100).AsParallel().Min(k => MakeFinalFolds(sol, new Rational(1, k), 3, 5, 8, 12).Pack().Size()));
			//return;
			var sol2 = MakeFinalFolds(sol, new Rational(1, 120), 3, 5, 8, 12).Pack();
			sol2.CreateVisualizerForm().ShowDialog();
			sol2.CreateVisualizerForm().ShowDialog();
			//Console.WriteLine(sol2.ToString());
			//var sol3 = MakeFinalFolds(sol, 999).Pack();
			////sol2.CreateVisualizerForm(true).ShowDialog();
			////Console.WriteLine(sol3.ToString());
			//Console.WriteLine(sol2.Size());
			//Console.WriteLine(sol3.Size());
			//Console.WriteLine(sol2.ToString());
		}
		[Test, Explicit]
		[TestCase(10, "9/20", "19/40", "12/40", "13/40", "1/120")]
		[TestCase(11, "9/20", "19/40", "12/40", "13/40", "1/116")]
		[TestCase(12, "9/20", "19/40", "13/40", "12/40", "1/115")]
		[TestCase(13, "9/20", "19/40", "14/40", "12/40", "1/114")]
		[TestCase(14, "17/40", "18/40", "14/40", "12/40", "1/113")]
		[TestCase(15, "8/20", "18/40", "14/40", "12/40", "1/112")]
		[TestCase(16, "7/20", "18/40", "14/40", "12/40", "1/111")]
		[TestCase(17, "17/40", "19/40", "12/40", "13/40", "1/120")]
		[TestCase(18, "17/40", "19/40", "12/40", "13/40", "1/116")]
		[TestCase(19, "17/40", "19/40", "13/40", "12/40", "1/115")]
		[TestCase(20, "17/40", "19/40", "14/40", "12/40", "1/114")]
		[TestCase(21, "17/40", "18/40", "14/40", "12/40", "1/113")]
		[TestCase(22, "17/40", "18/40", "14/40", "12/40", "1/112")]
		[TestCase(23, "17/40", "18/40", "14/40", "12/40", "1/111")]
		[TestCase(24, "16/40", "18/40", "14/40", "12/40", "1/111")]
		public void PostD4(int hour, string a, string b, string c, string d, string k)
		{
			var sol = MakeFinalFolds(D4Problem.D4(a, b, c, d), new Rational(1, 120), 3, 5, 8, 12).Pack();
			Console.WriteLine(sol.Facets.Length);
			Console.WriteLine(sol.Size());
			double time = (new DateTime(2016, 08, 07, hour, 0, 0) - new DateTime(1970, 1, 1)).TotalSeconds;
			Console.WriteLine();
			Console.WriteLine(sol);
			var ans = new ApiClient().PostProblem((int)time, sol);
			File.WriteAllText(Path.Combine(Paths.ProblemsDir(), $"{time}.problem.txt"), sol + "\r\n\r\n" + ans);
			Console.WriteLine(ans);
		}
	}
}