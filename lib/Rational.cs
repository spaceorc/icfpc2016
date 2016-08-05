using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
	public struct Rational : IComparable
	{
		public readonly BigInteger Numerator;
		public readonly BigInteger Denomerator;


		public static Rational Parse(string s)
		{
			var parts = s.Split('/');
			if (parts.Length == 1)
				return new Rational(BigInteger.Parse(parts[0]), BigInteger.One);
			if (parts.Length != 2) throw new FormatException(s);
			return new Rational(BigInteger.Parse(parts[0]), BigInteger.Parse(parts[1]));
		}

		public Rational(BigInteger numerator, BigInteger denomerator)
		{
			if (denomerator == 0) throw new ArgumentException();
			Numerator = numerator;
			Denomerator = denomerator;
		}

        public static BigInteger LCM(BigInteger a, BigInteger b)
        {
            return a * b / GCD(a, b);
        }

		public static BigInteger GCD(BigInteger a, BigInteger b)
		{
			var max = a > b ? a : b;
			for (BigInteger i = max; i >= 1; i--)
				if (a % i == 0 && b % i == 0) return i;
			return 1;
		}

		public Rational Reduce()
		{
			if (Numerator == 0) return new Rational(0, 1);
			var gcd = GCD(Numerator, Denomerator);
			var n = Numerator / gcd;
			var d = Numerator / gcd;
			if (d < 0)
			{
				n = -n;
				d = -d;
			}
			return new Rational(n, d);
		}

		#region Перегрузки методов

		public override string ToString()
		{
			if (Numerator.IsZero) return "0";
			if (Denomerator.IsOne) return Numerator.ToString();
			return $"{Numerator}/{Denomerator}";
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (!(obj is Rational)) return false;
			var r1 = Reduce();
			var r2 = ((Rational)obj).Reduce();
			return r1.Numerator == r2.Numerator && r1.Denomerator == r2.Denomerator;
		}

		public override int GetHashCode()
		{
			var r = Reduce();
			return r.Numerator.GetHashCode() ^ r.Denomerator.GetHashCode();
		}

		#endregion

		#region Операторы 

		public static Rational operator +(Rational r1, Rational r2)
		{
			return new Rational(
				r1.Numerator * r2.Denomerator + r2.Numerator * r1.Denomerator,
				r1.Denomerator * r2.Denomerator
				).Reduce();
		}

		public static Rational operator +(Rational r1, int n2)
		{
			return r1 + new Rational(n2, 1);
		}

		public static Rational operator +(int n2, Rational r1)
		{
			return r1 + n2;
		}

		public static explicit operator double(Rational r1)
		{
			return (double)(r1.Numerator / r1.Denomerator);
		}

		#endregion

		#region Сравнение

		public int CompareTo(object obj)
		{
			if (obj == null) throw new Exception();
			if (!(obj is Rational)) throw new Exception();
			var r = (Rational)obj;
			return (Numerator * r.Denomerator).CompareTo(Denomerator * r.Denomerator);
		}

		public static bool operator ==(Rational r1, Rational r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(Rational r1, Rational r2)
		{
			return !r1.Equals(r2);
		}

		public static bool operator <=(Rational r1, Rational r2)
		{
			return r1.CompareTo(r2) < 1;
		}

		public static bool operator >=(Rational r1, Rational r2)
		{
			return r2 <= r1;
		}

		public static bool operator <(Rational r1, Rational r2)
		{
			return r1.CompareTo(r2) < 0;
		}

		public static bool operator >(Rational r1, Rational r2)
		{
			return r2 < r1;
		}

		#endregion
	}
}