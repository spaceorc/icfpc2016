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

	}

	public static class PointExtensions
	{
		public static Point Reflect(this Point p, Segment mirror)
		{
			var b = mirror.End - mirror.Start;
			var a = p - mirror.Start;
			return new Point(0, 0);
			//var side = mirror.End - mirror.Start;
			//var n = new Point(-side.Y, side.X).Normalize();
			//return p + n * 2;
		}
	}

	[TestFixture]
	public class Point_Should
	{
		[Test]
		public void BeMirrored()
		{
			Segment s = "0,0 1,0";
			Point p = "1,1";
			p.Reflect(s).Should().Be(Point.Parse("1,-1"));
		}
	}
}