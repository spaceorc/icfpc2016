using lib;
using lib.DiofantEquationSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var rnd = new Random(100);
            var weigts = Enumerable
                .Range(0, 100)
                .Select(z => Tuple.Create(rnd.Next(100) + 1, rnd.Next(100) + 1))
                .Select(z => new Rational(Math.Min(z.Item1, z.Item2), Math.Max(z.Item1, z.Item2)))
                .ToArray();
            var equation = new DiofantEquation(weigts);
            equation.Solve(1);
                
        }
    }
}
