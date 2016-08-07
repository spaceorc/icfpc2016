using System.Collections.Generic;
using System.Linq;
using lib.Graphs;

namespace lib
{
    public class SolutionSpecBuilder
    {
        public static SolutionSpec BuildSolutionByGraph(
            Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> graph)
        {
            var cycleFinder = new CycleFinder<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(
                graph,
                n => n.Data.Projection);
            return BuildSolutionByCycles(cycleFinder.GetCycles());
        }

        public static SolutionSpec BuildSolutionByRibbonGraph(Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> graph)
        {
            var cycleFinder = new CycleFinder<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(
                graph,
                n => n.Data.Projection);
            var cycles = cycleFinder.GetCycles();

            var reflectedCycles = CycleReflector.GetUnribbonedCycles(cycles);
            return SolutionSpecBuilder.BuildSolutionByCycles(reflectedCycles);
        }

        public static SolutionSpec BuildSolutionByCycles(List<List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>> cycles)
        {
            var sourcePoints = cycles
                .SelectMany(c => c
                    .Select(n => n.Edge.From.Data.Projection)
                    .Concat(c.Select(n => n.Edge.To.Data.Projection)))
                .Distinct()
                .ToArray();
            var sourcePointIndices = sourcePoints
                .Select((x, i) => new { x, i })
                .ToDictionary(x => x.x, x => x.i);
            var facets = cycles
                .Select(c => new Facet(c.Select(e => sourcePointIndices[e.FromFrom ? e.Edge.From.Data.Projection : e.Edge.To.Data.Projection])
                    .ToArray()))
                .ToArray();
            var originalPointsInfo = cycles
                .SelectMany(c => c)
                .Select(e => new
                {
                    vector = e.FromFrom ? e.Edge.From.Data.Original.Data.Location : e.Edge.To.Data.Original.Data.Location,
                    index = sourcePointIndices[e.FromFrom ? e.Edge.From.Data.Projection : e.Edge.To.Data.Projection]
                }).ToArray();
            var originalPoints = new Vector[sourcePoints.Length];
            foreach (var info in originalPointsInfo)
                originalPoints[info.index] = info.vector;
            return new SolutionSpec(sourcePoints, facets, originalPoints);
        }
    }
}