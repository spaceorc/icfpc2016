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

            var spec = ProblemSpec.Parse(File.ReadAllText("...\\..\\..\\problems\\011.spec.txt"));
            spec = spec.MoveToOrigin();

            var q = spec.Segments.Where(z => Arithmetic.IsSquare(z.QuadratOfLength)).ToList();


            var solver = new PointProjectionSolver(spec);

            var result = solver.Algorithm();

            result = result
                .Where(z => z.edges[0].From == z.edges[z.edges.Count - 1].To)
                .ToList();



            var wnd = new Form() { ClientSize = new Size(800, 600) };

            wnd.Paint += (s, a) =>
              {
                  var g = a.Graphics;
                  var p = new Painter();
                  p.Paint(g, 600, spec);
                  Color[] cs = new[] { Color.Blue, Color.Cyan, Color.Orange, Color.Green, Color.Magenta };

                  for (int i=0;i<q.Count;i++)
                  {
                      p.PaintSegment(g, cs[i % cs.Length], q[i]);
                  }
              };




            
                
            

        }
    }
}
