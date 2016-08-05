using System.Linq;

namespace lib
{
	public static class VectorExtensions
	{
		public static Vector Reflect(this Vector p, Vector a, Vector b)
		{
			return p.Reflect(new Segment(a, b));
		}
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

		public static Vector GetCenter(this Vector[] ps)
		{
			var minX = ps.Select(v => v.X).Min();
			var minY = ps.Select(v => v.Y).Min();
			var maxX = ps.Select(v => v.X).Max();
			var maxY = ps.Select(v => v.Y).Max();
			return new Vector((minX + maxX) / 2, (minY + maxY) / 2);
		}
		public static Vector[] Rotate(this Vector[] ps, Rational x)
		{
			return ps.Select(p => p.Rotate(ps.GetCenter(), x)).ToArray();
		}
		public static Vector[] Move(this Vector[] ps, Vector shift)
		{
			return ps.Select(p => p + shift).ToArray();
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