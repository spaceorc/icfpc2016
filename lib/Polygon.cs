using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

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
			var vCount = int.Parse(reader.ReadLine() ?? "0");
			var ps = Enumerable.Range(0, vCount)
				.Select(i => reader.ReadLine())
				.Select(Point.Parse)
				.ToArray();
			return new Polygon(ps);
		}

		public Polygon Move(Rational shiftX, Rational shiftY)
		{
			return new Polygon(Vertices.Select(p => new Point(p.X + shiftX, p.Y + shiftY)).ToArray());
		}

		public Polygon Reflect(Segment mirror)
		{
			return new Polygon(Vertices.Select(v => v.Reflect(mirror)).ToArray());
		}

		public Rational GetUnsignedSquare()
		{
			var s = GetSignedSquare();
			return s > 0 ? s : -s;
		}

		public Rational GetSignedSquare()
		{
			Rational sum = 0;
			for (int i = 0; i < Vertices.Length; i++)
			{
				var p1 = Vertices[i];
				var p2 = Vertices[(i + 1) % Vertices.Length];
				sum += (p1.X - p2.X)*(p1.Y + p2.Y)/2;
				Console.WriteLine(sum);
			}
			return sum;
		}
	}

	[TestFixture]
	public class Polygon_Should
	{
		[TestCase("0,0 1,0 1,1 0,1", "1")]
		[TestCase("0,0 1,0 1,1", "1/2")]
		[TestCase("0,0 1000000000000000000,0 1000000000000000000,1000000000000000000", "500000000000000000000000000000000000")]
		[TestCase("0,0 0,1 1,1 1,0", "-1")]
		public void CalcSquare(string poly, string expectedSquare)
		{
			var polygon = new Polygon(poly.Split(' ').Select(Point.Parse).ToArray());
			var s = polygon.GetSignedSquare();
			s.Should().Be(Rational.Parse(expectedSquare));
		}
	}
}