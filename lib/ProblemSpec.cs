using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace lib
{
	public class ProblemSpec
	{
		public int id;
		public readonly Polygon[] Polygons;
		public Segment[] Segments;

		public IEnumerable<Vector> Points => Segments.SelectMany(s => new[] { s.Start, s.End });

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

		public Vector MinXY()
		{
			var vs = Polygons.SelectMany(p => p.Vertices).ToList();
			var minX = vs.Select(p => p.X).Min();
			var minY = vs.Select(p => p.Y).Min();
			return new Vector(minX, minY);
		}

		public Vector MaxXY()
		{
			var vs = Polygons.SelectMany(p => p.Vertices).ToList();
			var maxX = vs.Select(p => p.X).Max();
			var maxY = vs.Select(p => p.Y).Max();
			return new Vector(maxX, maxY);
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

		public static ProblemSpec Parse(string input, int id = 0)
		{
			var r = new StringReader(input);
			var pCount = int.Parse(r.ReadLine() ?? "0");
			var ps = Enumerable.Range(0, pCount)
				.Select(i => Polygon.Parse(r)).ToArray();
			var sCount = int.Parse(r.ReadLine() ?? "0");
			var ss = Enumerable.Range(0, sCount)
				.Select(i => r.ReadLine())
				.Select(Segment.Parse).ToArray();
			return new ProblemSpec(ps, ss) { id = id };
		}
	}
}