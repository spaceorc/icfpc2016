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

		private BigInteger GCD(BigInteger a, BigInteger b)
		{
			BigInteger c;
			while (!a.IsZero)
			{
				c = a;
				a = b%a;
				b = c;
			}
			return b;
		}

		public Rational Reduce()
		{
			if (Numerator == 0) return new Rational(0, 1);
			var gcd = GCD(Numerator, Denomerator);
			var n = Numerator / gcd;
			var d = Denomerator / gcd;
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
		public static Rational operator -(Rational r1, Rational r2)
		{
			return new Rational(
				r1.Numerator * r2.Denomerator - r2.Numerator * r1.Denomerator,
				r1.Denomerator * r2.Denomerator
				).Reduce();
		}

		public static Rational operator *(Rational a, Rational b)
		{
			return new Rational(a.Numerator * b.Numerator, a.Denomerator * b.Denomerator);
		}

		public static Rational operator /(Rational a, Rational b)
		{
			return new Rational(a.Numerator * b.Denomerator, a.Denomerator * b.Numerator);
		}


		public static Rational operator +(Rational r1, int n2)
		{
			return r1 + new Rational(n2, 1);
		}
		public static Rational operator -(Rational r)
		{
			return new Rational(-r.Numerator, r.Denomerator);
		}
		public static implicit operator Rational(int r)
		{
			return new Rational(r, 1);
		}

		public static Rational operator +(int n2, Rational r1)
		{
			return r1 + n2;
		}

		public static implicit operator double(Rational r1)
		{
			return (double) r1.Numerator/(double) r1.Denomerator;
		}
		public static implicit operator float(Rational r1)
		{
			return (float)((double)r1.Numerator / (double)r1.Denomerator);
		}

		#endregion

		#region Сравнение

		public int CompareTo(object obj)
		{
			if (obj == null) throw new Exception();
			if (!(obj is Rational)) throw new Exception();
			var r = (Rational)obj;
			return (Numerator * r.Denomerator).CompareTo(Denomerator * r.Numerator);
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