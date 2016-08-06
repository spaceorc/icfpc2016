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



        public static PointProjectionSolver CreateSolver(ProblemSpec spec)
        {
            //            spec = spec.MoveToOrigin();

            var solver = new PointProjectionSolver { spec = spec };
            var r = Pathfinder.MakeSegmentsWithIntersections(spec.Segments);
            solver.vectors = r.Item2;
            solver.AllSegments = Pathfinder.GenerateAllSmallSegments(r.Item1);
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

        public static void Visualize(PointProjectionSolver solver)
        {
            var gr = GenerateOutGraph(solver.ProjectionScheme, true);
            var viz = new GraphVisualizer<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>();
            viz.GetX = z => z.Data.Projection.X;
            viz.GetY = z => z.Data.Projection.Y;
            viz.Window(500, gr);
        }


        public static PointProjectionSolver Solve(PointProjectionSolver solver)
        {
            var pathes = Pathfinder.FindAllPathes(solver.Graph, 1, 0.9);
            var ps = pathes.ToList();
            var cycles = Pathfinder.FindAllCycles(ps);

            var cs = cycles.ToList();

            foreach(var c in cs)
            {
                var pr = Projector.CreateProjection(solver.AllSegments, solver.Graph);
                pr.Stages.Push(Projector.CreateInitialProjection(c, pr));
              //  Visualize(solver, pr);
                while(true)
                {
                    if (pr.IsCompleteProjection())
                    {
                        solver.ProjectionScheme = pr;
                        solver.Projection=GenerateOutGraph(solver.ProjectionScheme, false);
                        return solver;
                    }
                     
                    var st = Projector.AddVeryGoodEdges(pr);
                    if (st == null) break;
                    pr.Stages.Push(st);
             //       Visualize(solver, pr);

                }
            }
            return null;
        }
    }
}
