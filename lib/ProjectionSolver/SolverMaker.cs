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
            solver.AllSegments = r.Item1;
            solver.Graph = Pathfinder.BuildGraph(solver.AllSegments, solver.vectors);
            return solver;
        }


        static PointProjectionSolver GenerateOutGraph(PointProjectionSolver solver, Projection proj)
        {


            var nodes = proj.AllNodeProjections.ToList();
            solver.Projection = new Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(nodes.Count);
            for (int i=0;i<nodes.Count;i++)
            {
                solver.Projection[i].Data = new PointProjectionSolver.ProjectedNodeInfo
                {
                    Original = nodes[i].Original,
                    Projection = nodes[i].Projection
                };
            }

            foreach(var e in proj.AllEdgeProjections)
            {
                var from = nodes.IndexOf(e.begin);
                var to = nodes.IndexOf(e.end);
                var r =solver.Projection.DirectedConnect(from, to);
                r.Data = new PointProjectionSolver.ProjectedEdgeInfo();
            }

            return solver;
        }

        public static PointProjectionSolver Solve(PointProjectionSolver solver)
        {
            var pathes = Pathfinder.FindAllPathes(solver.Graph, 1);
            var cycles = Pathfinder.FindAllCycles(pathes.ToList());


            foreach(var c in cycles)
            {
                var pr = Projector.CreateProjection(solver.AllSegments, solver.Graph);
                pr.Stages.Push(Projector.CreateInitialProjection(c, pr));

                while(true)
                {
                    if (pr.IsCompleteProjection())
                        return GenerateOutGraph(solver, pr);
                    var st = Projector.AddVeryGoodEdges(pr);
                    if (st == null) break;
                }
            }
            return null;
        }
    }
}
