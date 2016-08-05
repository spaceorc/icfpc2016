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

	internal class GraphExt
	{
		private readonly List<GNode<EdgeInfo, Vector>> nodes;
		private readonly Dictionary<Edge<EdgeInfo, Vector>, List<GNode<EdgeInfo, Vector>>> referenceMap;

		private class EdgesComparer : IComparer<Edge<EdgeInfo, Vector>>
		{
			public int Compare(Edge<EdgeInfo, Vector> x, Edge<EdgeInfo, Vector> y)
			{
				var res = x.From.Data.X * y.To.Data.Y - x.From.Data.Y * y.To.Data.X;
				if (res > 0)
					return 1;
				if (res == 0)
					return 0;
				return -1;
			}
		}

		public GraphExt(Graph<EdgeInfo, Vector> graph)
		{
			referenceMap = new Dictionary<Edge<EdgeInfo, Vector>, List<GNode<EdgeInfo, Vector>>>();
			nodes = new List<GNode<EdgeInfo, Vector>>();
			foreach (var edge in graph.Edges)
			{
				nodes.Add(new GNode<EdgeInfo, Vector>(edge, false));
				nodes.Add(new GNode<EdgeInfo, Vector>(edge, true));
				var l = nodes.Count - 1;
				referenceMap[edge] = new List<GNode<EdgeInfo, Vector>> { nodes[l], nodes[l - 1] };
			}
		}

		public List<List<GNode<EdgeInfo, Vector>>> GetCycles()
		{
			foreach (var node in nodes)
				FindCycles(node);
			var used = new HashSet<GNode<EdgeInfo, Vector>>();
			var n = nodes[0];
			var res = new List<List<GNode<EdgeInfo, Vector>>>();
			while (used.Count < nodes.Count)
			{
				if (used.Contains(n))
					continue;
				used.Add(n);
				var cycle = new List<GNode<EdgeInfo, Vector>>();
				while (!used.Contains(n.Next))
				{
					cycle.Add(n);
					n = n.Next;
				}
				res.Add(cycle);
			}
			return res;
		}

		private void FindCycles(GNode<EdgeInfo, Vector> startNode)
		{
			while (true)
			{
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