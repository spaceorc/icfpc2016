using System;
using System.Diagnostics.Contracts;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public struct Point
	{
		public readonly Rational X, Y;

		public Point(Rational x, Rational y)
		{
			X = x;
			Y = y;
		}
		public static Point Parse(string s)
		{
			var parts = s.Split(',');
			if (parts.Length != 2) throw new FormatException(s);
			return new Point(Rational.Parse(parts[0]), Rational.Parse(parts[1]));
		}
		#region value semantics
		public bool Equals(Point other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Point && Equals((Point)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}


		public override string ToString()
		{
			return $"{X},{Y}";
		}
		#endregion
		public static implicit operator Point(string s)
		{
			return Parse(s);
		}
		public static Point operator -(Point a, Point b)
		{
			return new Point(a.X - b.X, a.Y - b.Y);
		}
		public static Point operator -(Point a)
		{
			return new Point(-a.X, -a.Y);
		}

		public static Point operator +(Point a, Point b)
		{
			return new Point(a.X + b.X, a.Y + b.Y);
		}
		public static Point operator *(Point a, Rational k)
		{
			return new Point(a.X * k, a.Y * k);
		}
		public static Point operator /(Point a, Rational k)
		{
			return new Point(a.X / k, a.Y / k);
		}
		public static Point operator *(Rational k, Point a)
		{
			return new Point(a.X * k, a.Y * k);
		}
		public Rational ScalarProd(Point p2)
		{
			return X*p2.X + Y*p2.Y;
		}

		[Pure]
		public Point Move(Rational shiftX, Rational shiftY)
		{
			return new Point(X + shiftX, Y + shiftY);
		}
		
		public double Length => Math.Sqrt(X * X + Y * Y);
		public Rational Length2 => X * X + Y * Y;

	}

	public static class PointExtensions
	{
		public static Point Reflect(this Point p, Segment mirror)
		{
			var b = mirror.End - mirror.Start;
			var a = p - mirror.Start;
			var k = a.ScalarProd(b)*2/b.Length2;
			return mirror.Start + b*k-a;
		}
	}

	[TestFixture]
	public class Point_Should
	{
		[TestCase("0,0 1,1", "0,1", "1,0")]
		[TestCase("0,0 1,0", "0,1", "0,-1")]
		[TestCase("10,10 11,10", "10,11", "10,9")]
		public void BeMirrored(string segment, string point, string expectedPoint)
		{
			Segment s = segment;
			Point p = point;
			var p2 = Point.Parse(expectedPoint);
			p.Reflect(s).Should().Be(p2);
			p2.Reflect(s).Should().Be(p);
		}
	}
}