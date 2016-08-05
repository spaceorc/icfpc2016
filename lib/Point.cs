using System;
using System.Diagnostics.Contracts;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public struct Vector
	{
		public readonly Rational X, Y;

		public Vector(Rational x, Rational y)
		{
			X = x;
			Y = y;
		}
		public static Vector Parse(string s)
		{
			var parts = s.Split(',');
			if (parts.Length != 2) throw new FormatException(s);
			return new Vector(Rational.Parse(parts[0]), Rational.Parse(parts[1]));
		}
		#region value semantics
		public bool Equals(Vector other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Vector && Equals((Vector)obj);
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
		public static implicit operator Vector(string s)
		{
			return Parse(s);
		}
		public static Vector operator -(Vector a, Vector b)
		{
			return new Vector(a.X - b.X, a.Y - b.Y);
		}
		public static Vector operator -(Vector a)
		{
			return new Vector(-a.X, -a.Y);
		}

		public static Vector operator +(Vector a, Vector b)
		{
			return new Vector(a.X + b.X, a.Y + b.Y);
		}
		public static Vector operator *(Vector a, Rational k)
		{
			return new Vector(a.X * k, a.Y * k);
		}
		public static Vector operator /(Vector a, Rational k)
		{
			return new Vector(a.X / k, a.Y / k);
		}
		public static Vector operator *(Rational k, Vector a)
		{
			return new Vector(a.X * k, a.Y * k);
		}
		public Rational ScalarProd(Vector p2)
		{
			return X*p2.X + Y*p2.Y;
		}

		[Pure]
		public Vector Move(Rational shiftX, Rational shiftY)
		{
			return new Vector(X + shiftX, Y + shiftY);
		}
		
		public double Length => Math.Sqrt(X * X + Y * Y);
		public Rational Length2 => X * X + Y * Y;

	}

	public static class PointExtensions
	{
		public static Vector Reflect(this Vector p, Segment mirror)
		{
			var b = mirror.End - mirror.Start;
			var a = p - mirror.Start;
			var k = a.ScalarProd(b)*2/b.Length2;
			return mirror.Start + b*k-a;
		}

		public static Point[] ToPoints(this string points)
		{
			return points.Split(' ').Select(Point.Parse).ToArray();
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
			Vector p = point;
			var p2 = Vector.Parse(expectedPoint);
			p.Reflect(s).Should().Be(p2);
			p2.Reflect(s).Should().Be(p);
		}

		[TestCase("0,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Inside)]
		[TestCase("0,0", "-1,1 1,1 1,-1 -1,-1", ExpectedResult = PointToPolygonPositionType.Inside)]
		[TestCase("0,1/100000", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Inside)]
		[TestCase("2,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Outside)]
		[TestCase("-1,-1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
		[TestCase("-1,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
		[TestCase("-1,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
		[TestCase("-1,1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
		[TestCase("-1,-1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
		[TestCase("0,-1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
		public PointToPolygonPositionType BeInValidPositionToPolygon(string point, string polygonDef)
		{
			Point p = point;
			var polygon = new Polygon(polygonDef.Split(' ').Select(Point.Parse).ToArray());
			return p.GetPositionToPolygon(polygon);
		}
	}
}