using System;
using lib;

namespace SquareConstructor
{
	public static class GeometryExtensions
	{
		public static Vector GetProjectionOntoLine(this Vector point, Segment line)
		{
			var lineVector = line.ToVector();
			var perp = new Vector(-lineVector.Y, lineVector.X);
			var projectionOntoLine = new Segment(point, point + perp).GetLineIntersectionWithLine(line).Value;
			return projectionOntoLine;
		}

		public static Vector? GetIntersectionWithLine(this Segment segment, Segment line)
		{
			var point = GetLineIntersectionWithLine(segment, line);
			if (point.HasValue)
			{
				if (IsBetween(segment.Start.X, point.Value.X, segment.End.X) && IsBetween(segment.Start.Y, point.Value.Y, segment.End.Y))
					return point;
			}
			return null;
		}

		public static Vector? GetLineIntersectionWithLine(this Segment thisLine, Segment otherLine)
		{
			var A1 = thisLine.End - thisLine.Start;
			var B1 = thisLine.Start;

			var A2 = otherLine.End - otherLine.Start;
			var B2 = otherLine.Start;

			var denominator = A1.Y*A2.X - A2.Y*A1.X;

			if (denominator == 0)
				return null;

			var t2 = ((B2.Y - B1.Y) * A1.X + (B1.X - B2.X) * A1.Y) / denominator;
			return A2 * t2 + B2;
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
