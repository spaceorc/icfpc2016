using DataScience;
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
		static void PaintSolver(ProblemSpec spec, PointProjectionSolver solver)
		{
			spec = new ProblemSpec(spec.Polygons, solver.Segments.ToArray());
			var wnd = new Form();
			wnd.Paint += (s, a) => { new Painter().Paint(a.Graphics, 500, spec); };
			Application.Run(wnd);
		}




        class PathStat
        {
            public List<PPath> pathes = new List<PPath>();
        }


        static void NewMain()
        {
            var task = 43;
            var fname = string.Format("...\\..\\..\\problems\\{0:D3}.spec.txt", task);
            var spec = ProblemSpec.Parse(File.ReadAllText(fname));
            var r = Pathfinder.BuildGraph(spec);
            var matrix = new PathStat[r.NodesCount, r.NodesCount];

            foreach (var e in Pathfinder.FindAllPathes(r, 1))
            {
                var i = e.FirstEdge.From.NodeNumber;
                var j = e.LastEdge.To.NodeNumber;
                if (matrix[i, j] == null) matrix[i, j] = new PathStat();
                matrix[i, j].pathes.Add(e);
            }
                

            var gr = new Graph<PathStat, NodeInfo>(r.NodesCount);
            for (int i = 0; i < gr.NodesCount; i++)
                gr[i].Data = r[i].Data;

            for (int i = 0; i < gr.NodesCount; i++)
                for (int j = 0; j < gr.NodesCount; j++)
                    if (matrix[i, j]!=null)
                        gr.NonDirectedConnect(i, j, matrix[i, j]);

            var viz = new GraphVisualizer<PathStat, NodeInfo>();
            viz.GetX = z => (double)z.Data.Location.X;
            viz.GetY = z => (double)z.Data.Location.Y;

            Func<PathStat, string> stat = z => z.pathes.Select(x => (double)(x.edges.AllNodes().Distinct().Count()) / x.edges.Count).Average().ToString();

            viz.EdgeCaption = z=>stat(z.Data);


            viz.Window(500, gr);
        }




		static void Main(string[] args)
		{
            NewMain();return;
			var goodTasks = new[] { 1,2,3,4,5,6,7,8, 11, 12, 13, 14, 15, 16, 38, 39, 40, 41, 42, 46 };
            var nonTrivial = new[] { 11, 12, 13, 14, 15, 16, 38, 39, 40, 41, 42, 46 };

            var badTasks = new[] { 43 };

			var allTasks = Enumerable.Range(1093, 100);


			foreach (var task in badTasks)
			{
                var fname = string.Format("...\\..\\..\\problems\\{0:D3}.spec.txt",task);
                var spec = ProblemSpec.Parse(File.ReadAllText(fname));


                var solver = SolverMaker.CreateSolver(spec);
                
            //    PaintSolver(spec, solver);

                solver = SolverMaker.Solve(solver);
				if (solver == null)
				{
					MessageBox.Show("No solution for " + fname);
                    continue;
				}


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
							e.From.Data.Projection.X.AsFloat()*size,
							e.From.Data.Projection.Y.AsFloat()*size,
							e.To.Data.Projection.X.AsFloat()*size,
							e.To.Data.Projection.Y.AsFloat()*size
							);

						g.FillEllipse(Brushes.Red,
							e.From.Data.Projection.X.AsFloat()*size - 3,
							e.From.Data.Projection.Y.AsFloat()*size - 3,
							6, 6);
						g.FillEllipse(Brushes.Red,
							e.To.Data.Projection.X.AsFloat()*size - 3,
							e.To.Data.Projection.Y.AsFloat()*size - 3,
							6, 6);
					}
				};

				wnd.Text = fname;

				Application.Run(wnd);
			}
		}
	}
}