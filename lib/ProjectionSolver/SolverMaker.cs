using lib;
using lib.DiofantEquationSolver;
using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Runner
{
    public class SolverMaker
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
        

        static void PaintSolver(ProblemSpec spec, PointProjectionSolver solver)
        {
            spec = new ProblemSpec(spec.Polygons, solver.Segments.ToArray());
            var wnd = new Form();
            wnd.Paint += (s, a) =>
              {
                  new Painter().Paint(a.Graphics, 200, spec);
              };
            Application.Run(wnd);
        }

        public static PointProjectionSolver CreateSolver(ProblemSpec spec)
        {
//            spec = spec.MoveToOrigin();

            var solver = new PointProjectionSolver(spec);
            return solver;
        }


        static IEnumerable<List<List<Edge<EdgeInfo,NodeInfo>>>> TraditionalReorderings(PointProjectionSolver solver)
        {
            var pathes = solver.Algorithm();

            var result = pathes.Where(z => z.edges[0].From == z.edges[z.edges.Count - 1].To);


            var reorderings = result.SelectMany(z => solver.GetReorderings(z));

            return reorderings;
        }

        static IEnumerable<List<List<Edge<EdgeInfo, NodeInfo>>>> NewReorderings(PointProjectionSolver solver)
        {
            return NewAlgorithm.GetAll(solver).Select(z=>z.Select(x=>x.edges).ToList());
            
        }

        public static PointProjectionSolver Solve(PointProjectionSolver solver)
        {


            //PaintSolver(spec,solver);
               var reorderings = TraditionalReorderings(solver);
            //var reorderings = NewReorderings(solver);
            

            bool ok = false;

            var best = solver.Projection;
            best = null;
            double bestQuality = -10;

            foreach(var reordering in reorderings)
            {
                var r = solver.TryProject(reordering);
                if (!r) continue;

                var unused = solver.UnusedSegments().ToList();

                var used = solver.AddAdditionalEdges(unused);
//                Console.WriteLine($"{used}/{unused.Count}");

                double quality = (double)used / unused.Count;
                if (quality > bestQuality)
                {
                    bestQuality = quality;
                    best = solver.Projection;
                    if (used == unused.Count) return solver ;
                }
            }

            solver.Projection = best;
	        

	        return solver;
        }
    }
}
