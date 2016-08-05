using System.Linq;

namespace lib
{
	public static class VectorExtensions
	{
		public static Vector Reflect(this Vector p, Segment mirror)
		{
			var b = mirror.End - mirror.Start;
			var a = p - mirror.Start;
			var k = a.ScalarProd(b)*2/b.Length2;
			return mirror.Start + b*k-a;
		}

		public static Vector[] ToPoints(this string points)
		{
			return points.Split(' ').Select(Vector.Parse).ToArray();
		}

		public static Vector Rotate(this Vector p, Vector other, Rational x)
		{
			return (p - other).Rotate(x) + other;
		}
		public static Vector Rotate(this Vector p, Rational x)
		{
			Segment s1 = new Segment(new Vector(0, 0), new Vector(new Rational(1, 2), new Rational(1, 2)));
			Segment s2 = x < 0
				? new Segment(new Vector(0, 0), s1.End + new Vector(x, 0))
				: new Segment(new Vector(0, 0), s1.End - new Vector(0, x));
			return p.Reflect(s1).Reflect(s2);
		}
	}
}