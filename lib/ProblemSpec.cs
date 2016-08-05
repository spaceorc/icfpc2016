using System;
using System.IO;
using System.Linq;
using System.Text;

namespace lib
{
	public class ProblemSpec
	{
		public readonly Polygon[] Polygons;
		public readonly Segment[] Segments;

		public ProblemSpec(Polygon[] polygons, Segment[] segments)
		{
			Polygons = polygons;
			Segments = segments;
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