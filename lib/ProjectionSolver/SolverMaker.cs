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

namespace lib.ProjectionSolver
{
    public class SolverMaker
    {



        public static PointProjectionSolver CreateSolver(ProblemSpec spec)
        {
            //            spec = spec.MoveToOrigin();

            var solver = new PointProjectionSolver { spec = spec };
            var r = Pathfinder.MakeSegmentsWithIntersections(spec.Segments);
            solver.vectors = r.Item2;
            solver.SegmentFamilies = r.Item1;
            solver.AllSegments = r.Item1.SelectMany(z => z.Segments).ToList();
            solver.Graph = Pathfinder.BuildGraph(solver.AllSegments, solver.vectors);
            return solver;
        }




        static Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>
            GenerateOutGraph(Projection proj, bool bidirectional)
        {
            var nodes = proj.AllNodeProjections.ToList();
            var g = new Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(nodes.Count);
            for (int i=0;i<nodes.Count;i++)
            {
                g[i].Data = new PointProjectionSolver.ProjectedNodeInfo
                {
                    Original = nodes[i].Original,
                    Projection = nodes[i].Projection
                };
            }
            var edges = proj.AllEdgeProjections.ToList();
            foreach(var e in edges)
            {
                var from = nodes.IndexOf(e.begin);
                var to = nodes.IndexOf(e.end);
                if (bidirectional)
                    g.NonDirectedConnect(from, to, new PointProjectionSolver.ProjectedEdgeInfo());
                else
                    g.DirectedConnect(from, to);
            }

            return g;
        }

        public static void Visualize(PointProjectionSolver solver, Projection p=null, string name="")
        {
            if (p == null) p = solver.ProjectionScheme;
            var gr = GenerateOutGraph(p, true);
            var viz = new GraphVisualizer<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>();
            viz.GetX = z => z.Data.Projection.X;
            viz.GetY = z => z.Data.Projection.Y;
            viz.NodeCaption = z => z.Data.Original.Data.Location.ToString();
            viz.Window(500, gr, name);
        }


        public static bool EvaluateProjection(PointProjectionSolver solver, Projection pr)
        {
            if (!pr.IsCompleteProjection()) return false;
            solver.ProjectionScheme = pr;
            solver.Projection = GenerateOutGraph(solver.ProjectionScheme, false);
            var solution = SolutionSpecBuilder.BuildSolutionByGraph(solver.Projection);
            return solution.AreFacetsValid(pr.SideY*pr.SideX);
        }


        static PointProjectionSolver TrySquashPoint(PointProjectionSolver solver, Projection pr, ProjectionStage stage)
        {
            pr.Stages.Push(stage);
            var res = TryHordEdges(solver, pr);
            if (res == null)
                pr.Stages.Pop();
            return res;
        }

        static PointProjectionSolver TryHordEdges(PointProjectionSolver solver, Projection pr)
        {
            while (true)
            {
                //Visualize(solver, pr);
                if (EvaluateProjection(solver, pr)) return solver;
                var hordEdgeStage = Projector.AddVeryGoodEdges(pr);
                if (hordEdgeStage != null)
                {
                    pr.Stages.Push(hordEdgeStage);
                    continue;
                }
                else
                    break;
            }
            return null;
        }

        static PointProjectionSolver TryCycle(PointProjectionSolver solver, List<PPath> cycle)
        {
            var pr = new Projection(solver.Graph, solver.AllSegments, solver.SegmentFamilies, cycle[0].length, cycle[1].length);
            pr.Stages.Push(Projector.CreateInitialProjection(cycle, pr));

            //Visualize(solver, pr, cycleCounter.ToString());

            var res=TryHordEdges(solver, pr);
            if (res != null) return res;

            var squashes = Projector.FindSquashPoint(pr);
            foreach (var sq in squashes)
            {
                var o = TrySquashPoint(solver, pr, sq);
                if (o != null) return o;
            }
            return null;
        }

        static int cycleCounter = 0;


        public static PointProjectionSolver Solve(PointProjectionSolver solver, Rational otherSide, double originality=0)
        {
            var pathes = Pathfinder.FindAllPathes(solver.Graph, 1, originality);
            var ps1 = pathes.ToList();

            var ps2 = otherSide == 1 ? ps1.ToList() : Pathfinder.FindAllPathes(solver.Graph, otherSide, originality).ToList();

            var cycles = Pathfinder.FindAllCycles(ps1, ps2).ToList();

            //var cs = cycles.ToList();

            cycleCounter = -1;

            foreach (var c in cycles.Skip(1))
            {
                cycleCounter++;
                var res = TryCycle(solver, c);
                if (res != null) return res;
            }
            return null;
        }
    }
}
