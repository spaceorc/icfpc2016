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

        static Rational getRational(Random rnd, int complexity)
        {
            var a = rnd.Next(complexity);
            var b = rnd.Next(complexity);
            return new Rational(Math.Min(a, b), Math.Max(a, b));
        }

        static Rational[] getRandomWeights(Random rnd, int complexity, int count)
        {
           
            var weigts = Enumerable
                .Range(0, count)
                .Select(z => Tuple.Create(rnd.Next(complexity) + 1, rnd.Next(complexity) + 1))
                .Select(z => new Rational(Math.Min(z.Item1, z.Item2), Math.Max(z.Item1, z.Item2)))
                .ToArray();
            return weigts;
        }

        static Rational[] getSolvableWeights(Random rnd, int complexity, int count)
        {
            List<Rational> weights = new List<Rational>();
            Rational sum = new Rational(1, 1);
            int t = 4;
            for (int i=0;i<count;i++)
            {
                Rational c = default(Rational);
                var cc = rnd.Next(t) + 1;
                while (true)
                {
                    c = getRational(rnd, complexity);
                    c = new Rational(c.Numerator*cc , c.Denomerator * count);
                    if (c < sum) break;
                }
                weights.Add(new Rational(c.Numerator / cc, c.Denomerator));
                sum = sum - c;
            }
            weights.Add(sum);
            return weights.ToArray();
        }

        static void Main(string[] args)
        {
           

            var equation = new DiofantEquation(getRandomWeights(new Random(1),40,100));

            int seed = 1;
            while (equation.Solve(seed++) == null) Console.WriteLine("\n\n\n\n\n");
            
                
        }
    }
}
