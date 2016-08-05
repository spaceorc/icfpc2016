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

        public int PoolSize = 10;

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

        void Iteration()
        {
            foreach(var e in solutions.Keys.ToList())
            {
                var c = Clone(e);
                Mutate(c);
                solutions[c] = Evaluate(c);
            }
            Clean();
        }

        public int[] Solve(int seed)
        {
            solutions = new Dictionary<int[], double>();
            rnd = new Random(seed);

            for (int i=0;i<PoolSize;i++)
            {
                var s = Generate();
                solutions[s] = Evaluate(s);
            }

            while(true)
            {
                if (solutions.First().Value == 0)
                    return solutions.First().Key;
                Iteration();
                Console.WriteLine(solutions.Select(z => z.Value.ToString()).StrJoin(" "));
            }
        }

        public bool Check(int[] solution)
        {
            var sum = GetSumForSolution(solution);
            return sum == total;
        }
    }
}
