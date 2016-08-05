using System;
using System.IO;
using System.Linq;
using System.Text;

namespace lib
{
	public class Polygon
	{
		public readonly Point[] Vertices;

		public Polygon(params Point[] vertices)
		{
			Vertices = vertices;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Vertices.Length.ToString());
			sb.Append(Vertices.StrJoin(Environment.NewLine));
			return sb.ToString();
		}

		public static Polygon Parse(StringReader reader)
		{
			var vCount = int.Parse(reader.ReadLine()?? "0");
			var ps = Enumerable.Range(0, vCount)
				.Select(i => reader.ReadLine())
				.Select(Point.Parse)
				.ToArray();
			return new Polygon(ps);
		}

		public Polygon Move(Rational shiftX, Rational shiftY)
		{
			return new Polygon(Vertices.Select(p => new Point(p.X + shiftX, p.Y +shiftY)).ToArray());
		}
	}
}