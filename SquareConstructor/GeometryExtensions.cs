using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace SquareConstructor
{
	static class GeometryExtensions
	{
		public static Vector? GetIntersection(this Segment segment, Segment intersector)
		{
			var A1 = segment.End - segment.Start;
			var B1 = segment.Start;

			var A2 = intersector.End - intersector.Start;
			var B2 = intersector.Start;

			var denominator = (A1.Y*A2.X - A2.Y*A1.X);

			if (denominator == 0)
				return null;

			var t2 = ((B2.Y - B1.Y)* A1.X + (B1.X - B2.X) * A1.Y) / denominator;

			var point = A2 * t2 + B2;

			if (IsBetween(segment.Start.X, point.X, segment.End.X) && IsBetween(segment.Start.Y, point.Y, segment.End.Y) && IsBetween(intersector.Start.X, point.X, intersector.End.X) && IsBetween(intersector.Start.Y, point.Y, intersector.End.Y))
				return point;

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
	}
}
