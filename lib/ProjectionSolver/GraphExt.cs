using System.Collections.Generic;
using System.Linq;
using lib;
using lib.Graphs;
using NUnit.Framework;

namespace Runner
{
	[TestFixture]
	public class GraphExt_Should
	{
		[Test]
		public void DoSomething_WhenSomething()
		{
			var problemsRepo = new ProblemsRepo();
			var problemSpec = problemsRepo.Get(15);
			problemSpec.CreateVisualizerForm().ShowDialog();
			var solutionSpec = GraphExt.Solve(problemSpec);
			solutionSpec.CreateVisualizerForm().ShowDialog();
		}
	}

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

		private static Rational GetVectorProd(GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> startNode, Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> x)
		{
			Vector a;
			Vector bX;
			if (startNode.FromFrom)
			{
				a = startNode.Edge.From.Data.Projection - startNode.Edge.To.Data.Projection;
				if (x.From.Data.Projection.Equals(startNode.Edge.To.Data.Projection))
					bX = x.To.Data.Projection - x.From.Data.Projection;
				else
					bX = x.From.Data.Projection - x.To.Data.Projection;
			}
			else
			{
				a = startNode.Edge.To.Data.Projection - startNode.Edge.From.Data.Projection;
				if (x.From.Data.Projection.Equals(startNode.Edge.From.Data.Projection))
					bX = x.To.Data.Projection - x.From.Data.Projection;
				else
					bX = x.From.Data.Projection - x.To.Data.Projection;
			}
			return a.VectorProdLength(bX);
		}

		public static SolutionSpec Solve(ProblemSpec problemSpec)
		{
			var solver = SolverMaker.Solve(SolverMaker.CreateSolver(problemSpec));
			var graphExt = new GraphExt(solver.Projection);
			return graphExt.GetSolution();
		}

		public GraphExt(Graph<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> graph)
		{
			foreach (var edge in graph.Edges)
			{
				edge.From.EnsureIncidentEdge(edge);
				edge.To.EnsureIncidentEdge(edge);
			}

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
			var res = new List<List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>>();
			foreach (var node in nodes)
			{
				if (!used.Contains(node))
				{
					var n = node;
					var cycle = new List<GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo>>();
					while (n != null && used.Add(n))
					{
						cycle.Add(n);
						n = n.Next;
					}
					if (n == node)
						res.Add(cycle);
				}
			}
			return res;
		}

		public SolutionSpec GetSolution()
		{
			var cycles = GetCycles();
			var sourcePoints = cycles
				.SelectMany(c => c.Select(n => n.Edge.From.Data.Projection).Concat(c.Select(n => n.Edge.To.Data.Projection))).Distinct().ToArray();

			var sourcePointIndices = sourcePoints.Select((x, i) => new { x, i }).ToDictionary(x => x.x, x => x.i);
			var facets = cycles.Select(c => new Facet(c.Select(e => sourcePointIndices[e.FromFrom ? e.Edge.From.Data.Projection : e.Edge.To.Data.Projection]).ToArray())).ToArray();
			var originalPointsInfo = cycles.SelectMany(c => c).Select(e => new
			{
				vector = e.FromFrom ? e.Edge.From.Data.Original.Data.Location : e.Edge.To.Data.Original.Data.Location,
				index = sourcePointIndices[e.FromFrom ? e.Edge.From.Data.Projection : e.Edge.To.Data.Projection]
			}).ToArray();
			var originalPoints = new Vector[sourcePoints.Length];
			foreach (var info in originalPointsInfo)
				originalPoints[info.index] = info.vector;
			return new SolutionSpec(sourcePoints, facets, originalPoints);
		}

		private void FindCycles(GNode<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> startNode)
		{
			while (!startNode.InCycle)
			{
				startNode.InCycle = true;
				var commonPoint = startNode.FromFrom ? startNode.Edge.To : startNode.Edge.From;
				var edges = commonPoint.IncidentEdges.Where(e => !e.Equals(startNode.Edge)).ToList();
				Edge<PointProjectionSolver.ProjectedEdgeInfo, PointProjectionSolver.ProjectedNodeInfo> nextEdge = null;
				foreach (var edge in edges)
				{
					var checkGoodProd = GetVectorProd(startNode, edge);
					if (checkGoodProd <= 0)
					{
						if (nextEdge == null)
						{
							nextEdge = edge;
						}
						else
						{
							var n = nextEdge.From == commonPoint ? nextEdge.To.Data.Projection - nextEdge.From.Data.Projection : nextEdge.From.Data.Projection - nextEdge.To.Data.Projection;
							var e = edge.From == commonPoint ? edge.To.Data.Projection - edge.From.Data.Projection : edge.From.Data.Projection - edge.To.Data.Projection;
							var candidateProd = e.VectorProdLength(n);
							if (candidateProd < 0)
								nextEdge = edge;
						}
					}
				}
				if (nextEdge == null)
					return;
				var nextFromFrom = nextEdge.From.Data.Projection.Equals(commonPoint.Data.Projection);
				startNode.Next = referenceMap[nextEdge].First(x => x.FromFrom == nextFromFrom);
				startNode = startNode.Next;
			}
		}
	}
}