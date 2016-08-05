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


		static void Main(string[] args)
		{
			var goodTasks = new[] { 1,2,3,4,5,6,7,8, 11, 12, 13, 14, 15, 16, 38, 39, 40, 41, 42, 46 };
			var badTasks = new[] { 16 };

			var allTasks = Enumerable.Range(46, 100);


			foreach (var task in allTasks)
			{
				var fname = string.Format("...\\..\\..\\problems\\{0:D3}.spec.txt", task);
				var spec = ProblemSpec.Parse(File.ReadAllText(fname));

                var solver = SolverMaker.CreateSolver(spec);

                PaintSolver(spec, solver);

                solver = SolverMaker.Solve(solver);
				if (solver == null)
				{
					MessageBox.Show("No solution for " + fname);
					return;
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