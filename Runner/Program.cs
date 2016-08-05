﻿using lib;
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


        static void Main(string[] args)
        {
            var goodTasks = new[] { 11, 12, 13, 14, 15, 16, 38, 39, 40, 41, 42 };
            var badTasks = new[] { 16 };

            var allTasks = Enumerable.Range(43, 100);


            foreach (var task in goodTasks)
            {
                var fname = string.Format("...\\..\\..\\problems\\{0:D3}.spec.txt", task);
                var spec = ProblemSpec.Parse(File.ReadAllText(fname));
                spec = spec.MoveToOrigin();


                var solver = new PointProjectionSolver(spec);

                //PaintSolver(spec,solver);

                var result = solver.Algorithm().Where(z => z.edges[0].From == z.edges[z.edges.Count - 1].To);


                var reorderings = result.SelectMany(z => solver.GetReorderings(z));


                bool ok = false;

                foreach(var reordering in reorderings)
                {
                    var r = solver.TryProject(reordering);
                    if (!r) continue;

                    var unused = solver.UnusedSegments().ToList();

                    var used = solver.AddAdditionalEdges(unused);
                    if (used < unused.Count) continue;
                    ok = true;
                    var wnd = new Form() { ClientSize = new Size(800, 600) };

                    wnd.Paint += (s, a) =>
                    {
                        var g = a.Graphics;
                        int size = 200;
                        foreach (var e in solver.Projection.Edges)
                        {
                            var color = Color.Black;
                            if (e.Data.IsLate) color = Color.Orange;

                            g.DrawLine(new Pen(color, 1),
                                e.From.Data.Projection.X.AsFloat() * size,
                                e.From.Data.Projection.Y.AsFloat() * size,
                                e.To.Data.Projection.X.AsFloat() * size,
                                e.To.Data.Projection.Y.AsFloat() * size
                                );

                            g.FillEllipse(Brushes.Red,
                                e.From.Data.Projection.X.AsFloat() * size - 3,
                                e.From.Data.Projection.Y.AsFloat() * size - 3,
                                6, 6);
                            g.FillEllipse(Brushes.Red,
                                e.To.Data.Projection.X.AsFloat() * size - 3,
                                e.To.Data.Projection.Y.AsFloat() * size - 3,
                                6, 6);

                        }

                    };

                    wnd.Text = fname;

                    Application.Run(wnd);
                    break;
                }

                if (!ok) MessageBox.Show("No solution for " + fname);
            }


           



            
                
            

        }
    }
}
