using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class Painter_Utils
	{
		[Test]
		public void PaintProblems()
		{
			var painter = new Painter();
			var ps =
				from i in Enumerable.Range(1, int.MaxValue)
				let filename = i.ToString("000") + ".spec.txt"
				let filepath = Path.Combine(Paths.ProblemsDir(), filename)
				select filepath;
			foreach (var path in ps.TakeWhile(File.Exists))
			{
				Console.WriteLine($"writing {path}");
				var content = File.ReadAllText(path);
				var spec = ProblemSpec.Parse(content);
				var bmp = MakeBitmap(painter, spec);
				bmp.Save(path+ ".bmp");
			}
		}

		[TestCase(15)]
		[Explicit]
		public void PaintOne(int index)
		{
			var painter = new Painter();
			var filename = index.ToString("000") + ".spec.txt";
			var content = File.ReadAllText(Path.Combine(Paths.ProblemsDir(), filename));
			var spec = ProblemSpec.Parse(content).MoveToOrigin();
			Console.WriteLine(spec.ToString());
			var bmp = MakeBitmap(painter, spec);
			var file = Path.Combine(Paths.ProblemsDir(), filename + ".bmp");
			bmp.Save(file);
			Process.Start(file);
		}

		private static Bitmap MakeBitmap(Painter painter, ProblemSpec problemSpec)
		{
			var size = 300;
			var bitmap = new Bitmap(size, size);
			painter.Paint(Graphics.FromImage(bitmap), size, problemSpec);
			return bitmap;
		}
	}
}