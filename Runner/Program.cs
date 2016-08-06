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




        class PathStat
        {
            public List<PPath> pathes = new List<PPath>();
        }


        static void SolveTask(int taskNumber)
        {
            var spec = new ProblemsRepo().Get(taskNumber);
            var solver = SolverMaker.CreateSolver(spec);
            solver = SolverMaker.Solve(solver);
            if (solver == null) return;
            SolverMaker.Visualize(solver);
        }


        static void NewMain()
        {
            var task = 42;
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

            Func<PathStat, string> stat = z => z.pathes.Count().ToString(); ;

            viz.EdgeCaption = z=>stat(z.Data);


            viz.Window(500, gr);
        }




		static void Main(string[] args)
		{
       //     NewMain();return;

            var goodTasks = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 15, 16, 38, 39, 40, 41, 42, 46 };

            SolveTask(43); return;

            foreach (var e in goodTasks) SolveTask(e);
            //NewMain();return;

		}
	}
}