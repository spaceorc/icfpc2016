using System;
using System.Collections.Generic;
using System.Linq;
using lib;

namespace SquareConstructor
{
	public static class GeometryExtensions
	{
		public static Vector? GetIntersectionWithLine(this Segment segment, Segment line)
		{
			var A1 = segment.End - segment.Start;
			var B1 = segment.Start;

			var A2 = line.End - line.Start;
			var B2 = line.Start;

			var denominator = A1.Y*A2.X - A2.Y*A1.X;

			if (denominator == 0)
				return null;

			var t2 = ((B2.Y - B1.Y) * A1.X + (B1.X - B2.X) * A1.Y) / denominator;

			var point = A2 * t2 + B2;

			if (IsBetween(segment.Start.X, point.X, segment.End.X) && IsBetween(segment.Start.Y, point.Y, segment.End.Y))
				return point;

			return null;
		}

		public static Vector? GetIntersection(this Segment segment, Segment intersector)
		{
			var point = segment.GetIntersectionWithLine(intersector);
			if (point.HasValue)
			{
				if (IsBetween(intersector.Start.X, point.Value.X, intersector.End.X) && IsBetween(intersector.Start.Y, point.Value.Y, intersector.End.Y))
					return point;
			}
			return null;
		}

		private static bool IsBetween(Rational a, Rational x, Rational b)
		{
			return (a - x)*(b - x) <= 0;
		}

		public static double GetAngleMeasure(Vector vec1, Vector vec2)
		{
			var vectorAngleMeasure = 1 + vec1.ScalarProd(vec2)/Math.Sqrt(vec1.Length2*vec2.Length2);
			if (vec1.X*vec2.Y - vec1.Y*vec2.X < 0)
				vectorAngleMeasure = 4 - vectorAngleMeasure;
			return vectorAngleMeasure;
		}

		public static Rational GetSin(Vector vec1, Vector vec2)
		{
			if(vec1.Length2 != vec2.Length2)
				throw new Exception("vectors must be equal");
			return (vec1.X*vec2.Y - vec1.Y*vec2.X)/vec1.Length2;
		}

		public static Rational GetCos(Vector vec1, Vector vec2)
		{
			if (vec1.Length2 != vec2.Length2)
				throw new Exception("vectors must be equal");
			return (vec1.X * vec2.X + vec1.Y * vec2.Y)/vec1.Length2;
		}

		public static Rational? GetXIntersect(this Segment segment, int y)
		{
			var A = segment.End - segment.Start;
			var B = segment.Start;

			if (A.Y == 0)
				return null;

			var intersection = A.X*(y - B.Y)/A.Y + B.X;
			if (IsBetween(segment.Start.X, intersection, segment.End.X))
				return intersection;
			return null;
		}

		public static Rational? GetYIntersect(this Segment segment, int x)
		{
			var A = segment.End - segment.Start;
			var B = segment.Start;

			if (A.X == 0)
				return null;

			var intersection = A.Y * (x - B.X) / A.X + B.Y;

			if (IsBetween(segment.Start.Y, intersection, segment.End.Y))
				return intersection;
			return null;
		}
	}
}
