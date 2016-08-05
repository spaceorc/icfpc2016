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
        public static BigInteger Square(BigInteger n)
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

        public static bool IsSquare(BigInteger n)
        {
            var sq = Square(n);
            return n == sq * sq;
        }

        public static bool IsSquare(Rational r)
        {
            r=r.Reduce();
            return IsSquare(r.Numerator) && IsSquare(r.Denomerator);
        }
    }
}
