using System;
using System.Diagnostics.Contracts;
using System.Linq;

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
			return X * p2.X + Y * p2.Y;
		}

		public Rational VectorProdLength(Vector p2)
		{
			return X * p2.Y - p2.X * Y;
		}

		[Pure]
		public Vector Move(Rational shiftX, Rational shiftY)
		{
			return new Vector(X + shiftX, Y + shiftY);
		}

		public double Length => Math.Sqrt(X * X + Y * Y);
		public Rational Length2 => X * X + Y * Y;

	}
}