using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
    public static class Arithmetic
    {

        /// <summary>
        /// Возвращает наибольшее число, меньше или равное корню из n
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static BigInteger Sqrt(BigInteger n)
        {
            if (n == BigInteger.Zero) return BigInteger.Zero;
            var left = BigInteger.One;
            var right = n;
            while(right-left>1)
            {
                var m = (right + left) / 2;
                var t = m * m;
                if (t <= n) left = m;
                else right = m;
            }
            return left;
        }

        public static Rational Sqrt(Rational r)
        {
            r = r.Reduce();
            return new Rational(Sqrt(r.Numerator), Sqrt(r.Denomerator));
        }

        public static bool IsSquare(BigInteger n)
        {
            var sq = Sqrt(n);
            return n == sq * sq;
        }

        public static bool IsSquare(Rational r)
        {
            r=r.Reduce();
            return IsSquare(r.Numerator) && IsSquare(r.Denomerator);
        }

        public static double IrrationalDistance(Vector a, Vector b)
        {
            var dx = (double)(a.X - b.X);
            var dy = (double)(a.Y - b.Y);
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static bool PointInSegment(Vector a, Segment b)
        {
            if ((a - b.Start).VectorProdLength(b.End - b.Start) != 0) return false;
            if ((b.End - b.Start).ScalarProd(a - b.Start) < 0) return false;
            if ((b.Start - b.End).ScalarProd(a - b.End) < 0) return false;
            return true;

        }

        public static Vector? GetIntersection(this Segment segment, Segment intersector)
        {
            var A1 = segment.End - segment.Start;
            var B1 = segment.Start;

            var A2 = intersector.End - intersector.Start;
            var B2 = intersector.Start;

            var denominator = (A1.Y * A2.X - A2.Y * A1.X);

            if (denominator == 0)
                return null;

            var t2 = ((B2.Y - B1.Y) * A1.X + (B1.X - B2.X) * A1.Y) / denominator;

            var point = A2 * t2 + B2;

            if (IsBetween(segment.Start.X, point.X, segment.End.X) && IsBetween(segment.Start.Y, point.Y, segment.End.Y) && IsBetween(intersector.Start.X, point.X, intersector.End.X) && IsBetween(intersector.Start.Y, point.Y, intersector.End.Y))
                return point;

            return null;
        }

        private static bool IsBetween(Rational a, Rational x, Rational b)
        {
            return (a - x) * (b - x) <= 0;
        }
    }
}
