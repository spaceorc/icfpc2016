using System;

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
	}
}