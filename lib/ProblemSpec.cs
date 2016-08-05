using System;
using System.IO;
using System.Linq;
using System.Text;

namespace lib
{
	public class ProblemSpec
	{
		public readonly Polygon[] Polygons;
		public  Segment[] Segments;

		public ProblemSpec(Polygon[] polygons, Segment[] segments)
		{
			Polygons = polygons;
			Segments = segments;
		}

		public ProblemSpec MoveToOrigin()
		{
			var vs = Polygons.SelectMany(p => p.Vertices).ToList();
			var minX = vs.Select(p => p.X).Min();
			var minY = vs.Select(p => p.Y).Min();
			return new ProblemSpec(
				Polygons.Select(p => p.Move(-minX, -minY)).ToArray(),
				Segments.Select(s => s.Move(-minX, -minY)).ToArray()
				);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Polygons.Length.ToString());
			sb.AppendLine(Polygons.StrJoin(Environment.NewLine));
			sb.AppendLine(Segments.Length.ToString());
			sb.Append(Segments.StrJoin(Environment.NewLine));
			return sb.ToString();
		}

		public static ProblemSpec Parse(string input)
		{
			var r = new StringReader(input);
			var pCount = int.Parse(r.ReadLine() ?? "0");
			var ps = Enumerable.Range(0, pCount)
				.Select(i => Polygon.Parse(r)).ToArray();
			var sCount = int.Parse(r.ReadLine() ?? "0");
			var ss = Enumerable.Range(0, sCount)
				.Select(i => r.ReadLine())
				.Select(Segment.Parse).ToArray();
			return new ProblemSpec(ps, ss);
		}
	}
}