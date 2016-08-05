﻿using System;
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
    }
}
