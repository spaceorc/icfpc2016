using DataScience;
using lib;
using lib.Api;
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

namespace lib.ProjectionSolver
{
	class Program
	{


        static void ShowRibbonStat(IEnumerable<int> tasks)
        {

            foreach (var e in tasks)
                Console.WriteLine($"{e}\t{RibbonIndicator.GetRibbonWidth(new ProblemsRepo().Get(e))}");
            Console.ReadKey();
        }

        static void Main(string[] args)
		{
            var goodTasks = new[] { 1, 2, 3, 4, 5, 6, 7, 11, 12, 13, 14, 15, 16, 17, 38, 39, 40, 41, 46, 1131, 1903 };
	        foreach (var task in goodTasks)
	        {
				Console.WriteLine($"[ {task} ]");
		        UltraSolver.SolveAndSend(task, wait: false);
	        }
	        Console.ReadKey();

	        var nonRibbons = new[] { 4248 , 4008, 4033, 1490, 4206};

            var ribbons = new[] { 5157, 5208, 5161 };

            //Arithmetic.RationalTriangulate(
            //    new Segment(new Vector(0,0), new Vector(3, 3)),
            //    new Segment(new Vector(3, 3), new Vector(2, 6)),
            //    new Vector(0, 0),
            //    new Vector(2, 6));

            //ShowRibbonStat(ribbons.Concat(nonRibbons));

            //foreach (var e in goodTasks)

           // DrawProblem(5161);
            UltraSolver.SolveAndShow(5161);
//UltraSolver.SolveAndSend(41);

            
			
//
			//SolveTask(5811,new Rational(1,37)); return;
            //SolveTask(21, 1);return;

            ///DrawPathGraph(49);return;
            //SolveAndSend(1763);// return; //че за упаковка

            //  SolveAndSend(40);return;
            //	SolveAndSendStrip(5811, new Rational(1,37));


            //что не так с 42?
            //    foreach (var e in goodTasks) SolveAndSend(e,false);Console.ReadKey();
        
           // foreach (var e in goodTasks) SolveTask(e);
            //NewMain();return;

		}


        class PathStat
        {
            public List<PPath> pathes = new List<PPath>();
        }

        static void SolveWithUltrasolver(int taskNumber)
        {
            UltraSolver.SolveAndShow(taskNumber);
        }

        static void SolveTask(int taskNumber, Rational otherSide)
        {
            var spec = new ProblemsRepo().Get(taskNumber);
            var solver = SolverMaker.CreateSolver(spec);
            solver = SolverMaker.Solve(solver, otherSide);
            if (solver == null) return;
            SolverMaker.Visualize(solver);
        }


        static void DrawProblem(int task)
        {
            var spec = new ProblemsRepo().Get(task);
            var graph = Pathfinder.BuildGraph(spec);
            var viz = new GraphVisualizer<EdgeInfo, NodeInfo>();
            viz.GetX = z => z.Data.Location.X;
            viz.GetY = z => z.Data.Location.Y;
              //viz.NodeCaption = z => z.Data.Location.ToString() + " ("+z.NodeNumber.ToString()+")";
            viz.NodeCaption = z => z.NodeNumber.ToString();

            //viz.EdgeCaption = z => z.Data.length.ToString();

            viz.Window(600, graph);

        }

        static void DrawPathGraph(int task)
        {


            var fname = string.Format("...\\..\\..\\problems\\{0:D3}.spec.txt", task);
            var spec = ProblemSpec.Parse(File.ReadAllText(fname));
            var r = Pathfinder.BuildGraph(spec);

            var lens = r.Edges.Select(z => z.Data.length).OrderBy(z => z).ToList();

            var matrix = new PathStat[r.NodesCount, r.NodesCount];

            foreach (var e in Pathfinder.FindAllPathes(r, 1, 0.7))
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
                    if (matrix[i, j] != null)
                        gr.NonDirectedConnect(i, j, matrix[i, j]);

            var viz = new GraphVisualizer<PathStat, NodeInfo>();
            viz.GetX = z => (double)z.Data.Location.X;
            viz.GetY = z => (double)z.Data.Location.Y;

            Func<PathStat, string> stat = z => z.pathes.Count().ToString(); ;

            viz.EdgeCaption = z => stat(z.Data);
            viz.NodeCaption = z => z.Data.Location.ToString();


            viz.Window(500, gr);
        }

        //static void SolveAndSend(int id, bool wait = true)
        //{
        //    Console.WriteLine(id + ":" + " " + SolveAndSendInternal(id));
        //    if (wait) Console.ReadKey();
        //    return;
        //}

        //static void SolveAndSendStrip(int id, Rational otherSide)
        //{
        //    Console.WriteLine(id + ":" + " " + SolveAndSendStripInternal(id, otherSide));
        //    Console.ReadKey();
        //    return;
        //}


        //public static double SolveAndSendStripInternal(int id, Rational otherSide)
        //{
        //    var repo = new ProblemsRepo();
        //    var problemSpec = repo.Get(id);
        //    var solver = SolverMaker.Solve(SolverMaker.CreateSolver(problemSpec), otherSide, 0);

        //    if (solver == null) return 0;

        //    var cycleFinder = new CycleFinder<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(
        //       solver.Projection,
        //       n => n.Data.Projection);

        //    var cycles = cycleFinder.GetCycles();
        //    var reflectedCycles = CycleReflector.GetUnribbonedCycles(cycles);
        //    var spec = SolutionSpecBuilder.GetSolutionsByCycles(reflectedCycles);

        //    spec = spec.Pack();
        //    if (spec == null) return 0;
        //    return ProblemsSender.Post(problemSpec.id, spec);
        //}

        //public static double SolveAndSendInternal(int id)
        //{
        //    var problemSpec = new ProblemsRepo().Get(id);
        //    var spec = ProjectionSolverRunner.Solve(problemSpec);
        //    if (spec == null)
        //        return 0;
        //    return ProblemsSender.Post(problemSpec.id, spec);
        //}

    }
}