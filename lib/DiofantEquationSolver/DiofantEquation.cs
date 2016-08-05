using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace lib.DiofantEquationSolver
{
    public class DiofantEquation
    {
        Random rnd;
        readonly BigInteger total;
        readonly BigInteger[] weights;
        readonly BigInteger Sum;

  
        public int PoolSize = 25;

        public DiofantEquation(params Rational[] areas)
        {
            total = areas[0].Denomerator;
            foreach (var e in areas.Skip(1))
                total = Rational.LCM(total, e.Denomerator);
            weights = new BigInteger[areas.Length];
            for (int i = 0; i < areas.Length; i++)
            {
                weights[i] = areas[i].Numerator * total / areas[i].Denomerator;
                Sum += weights[i];
            }
        }

        public double Evaluate(int[] solution)
        {
            BigInteger sum = GetSumForSolution(solution);

            sum -= total;
            if (sum < 0) sum = -sum;
            return (double)sum / (double)total;
        }

        private BigInteger GetSumForSolution(int[] solution)
        {
            BigInteger sum = 0;
            for (int i = 0; i < weights.Length; i++)
                sum += weights[i] * solution[i];
            return sum;
        }

        Dictionary<int[], double> solutions;


        int[] Generate()
        {

            var approx = (int)Math.Round((double)Sum / (double)total);
            if (approx <= 1) approx = 2;
            var s = new int[weights.Length];
            for (int i=0;i<weights.Length;i++)
            {
                s[i] = rnd.Next(approx);
            }
            return s;
        }


        int[] Cross(int[] a, int[] b)
        {
            var r = new int[a.Length];
            var p = rnd.Next(a.Length);
            for (int i=0;i<r.Length;i++)
            {
                r[i] = i < p ? a[i] : b[i];
            }
            return r;
        }

        void Mutate(int[] solution)
        {
            var p = rnd.Next(solution.Length);
            if (solution[p]==0)
            {
                solution[p]++;
                return;
            }
            solution[p] += rnd.Next(2) > 0 ? -1 : 1;
        }

        int[] Clone(int[] solution)
        {
            var r = new int[solution.Length];
            for (int i = 0; i < r.Length; i++)
                r[i] = solution[i];
            return r;
        }

        void Clean()
        {
            solutions = solutions
                .OrderBy(z => z.Value)
                .Take(PoolSize)
                .ToDictionary(z => z.Key, z => z.Value);
        }

        void AddSolution(int[] solution)
        {
            solutions[solution] = Evaluate(solution);
        }

        void Iteration()
        {

            var old = solutions.Keys.ToList();

            for (int i=0;i<PoolSize;i++)
                AddSolution(Cross(old[rnd.Next(old.Count)], old[rnd.Next(old.Count)]));


            foreach(var e in old)
            {
                var c = Clone(e);
                Mutate(c);
                AddSolution(c);
            }

            for (int i = 0; i < PoolSize / 4; i++)
                AddSolution(Generate());
            Clean();
        }

        public int[] Solve(int seed, int iterationsLimit=int.MaxValue)
        {
            solutions = new Dictionary<int[], double>();
            rnd = new Random(seed);

            for (int i=0;i<PoolSize;i++)
                AddSolution(Generate());

            for (int i=0;i<iterationsLimit;i++)
            {
                if (solutions.First().Value == 0)
                    return solutions.First().Key;
                Iteration();
                Console.WriteLine(solutions.Take(10).Select(z => z.Value.ToString("0.000")).StrJoin(" "));
            }
            return null;
        }

        public bool Check(int[] solution)
        {
            var sum = GetSumForSolution(solution);
            return sum == total;
        }
    }
}
