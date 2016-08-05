using System.Collections.Generic;
using System.Linq;
using lib;
using lib.Graphs;

namespace Runner
{
	internal class GNode<TEdge, TNode>
	{
		public readonly Edge<TEdge, TNode> Edge;
		public bool InCycle;
		public GNode<TEdge, TNode> Next;
		public readonly bool FromFrom;

		public GNode(Edge<TEdge, TNode> edge, bool direction)
		{
			Edge = edge;
			FromFrom = direction;
		}
	}

	public class GraphExt
	{
		private readonly List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>> nodes;
		private readonly Dictionary<Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>, List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>> referenceMap;

		private class EdgesComparer : IComparer<Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>
		{
			public int Compare(Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> x, Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> y)
			{
				var res = x.From.Data.Projection.X * y.To.Data.Projection.Y - x.From.Data.Projection.Y * y.To.Data.Projection.X;
				if (res > 0)
					return 1;
				if (res == 0)
					return 0;
				return -1;
			}
		}

		public static SolutionSpec Solve(ProblemSpec problemSpec)
		{
			var solver = new PointProjectionSolver(problemSpec);

			var result = solver.Algorithm();

			result = result
				.Where(z => z.edges[0].From == z.edges[z.edges.Count - 1].To)
				.ToList();

			var resIndex = -1;

			for (int i = 0; i < result.Count; i++)
			{
				var r = solver.TryProject(result[i]);
				if (!r) continue;

				var unused = solver.UnusedSegments().ToList();

				solver.AddAdditionalEdges(unused);
				break;
			}

			var graphExt = new GraphExt(solver.Projection);
			return graphExt.GetSolution();
		}

		public GraphExt(Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> graph)
		{
			referenceMap = new Dictionary<Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>, List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>>();
			nodes = new List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>();
			foreach (var edge in graph.Edges)
			{
				nodes.Add(new GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(edge, false));
				nodes.Add(new GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>(edge, true));
				var l = nodes.Count - 1;
				referenceMap[edge] = new List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>> { nodes[l], nodes[l - 1] };
			}
		}

		private List<List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>> GetCycles()
		{
			foreach (var node in nodes)
				FindCycles(node);
			var used = new HashSet<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>();
			var n = nodes[0];
			var res = new List<List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>>();
			while (used.Count < nodes.Count)
			{
				if (used.Contains(n))
					continue;
				used.Add(n);
				var cycle = new List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>();
				while (!used.Contains(n.Next))
				{
					cycle.Add(n);
					n = n.Next;
				}
				res.Add(cycle);
			}
			return res;
		}

		public SolutionSpec GetSolution()
		{
			var cycles = GetCycles();
			var sourcePoints = cycles.SelectMany(c => c).Select(n => n.Edge.From.Data.Projection).Distinct().ToArray();
			var sourcePointIndices = sourcePoints.Select((x, i) => new { x, i }).ToDictionary(x => x.x, x => x.i);
			var facets = cycles.Select(c => new Facet(c.Select(e => sourcePointIndices[e.Edge.From.Data.Projection]).ToArray())).ToArray();
			var originalPointsInfo = cycles.SelectMany(c => c).Select(n => new
			{
				vector = n.Edge.From.Data.Original.Data.Location,
				index = sourcePointIndices[n.Edge.From.Data.Projection]
			}).ToArray();
			var originalPoints = new Vector[sourcePoints.Length];
			foreach (var info in originalPointsInfo)
				originalPoints[info.index] = info.vector;
			return  new SolutionSpec(sourcePoints, facets, originalPoints);
		}

		private void FindCycles(GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> startNode)
		{
			while (true)
			{
				// ReSharper disable once PossibleNullReferenceException
				if (startNode.InCycle)
					return;
				startNode.InCycle = true;

				if (startNode.FromFrom)
				{
					var nextEdge = startNode.Edge.From.IncidentEdges.Where(e => !e.Equals(startNode.Edge)).OrderBy(x => x, new EdgesComparer()).First();
					startNode.Next = referenceMap[nextEdge].FirstOrDefault(x => x.FromFrom);
				}
				else
				{
					var nextEdge = startNode.Edge.To.IncidentEdges.Where(e => !e.Equals(startNode.Edge)).OrderBy(x => x, new EdgesComparer()).First();
					startNode.Next = referenceMap[nextEdge].FirstOrDefault(x => !x.FromFrom);
				}
				startNode = startNode.Next;
			}
		}
	}
}