using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Graphs
{
	public class Edge<TEdge, TNode>
	{
		public TEdge Data { get; set; }

		public override string ToString()
		{
			return $"{From}->{To}";
		}

		public readonly Node<TEdge, TNode> From;
		public readonly Node<TEdge, TNode> To;

		public Edge(Node<TEdge, TNode> first, Node<TEdge, TNode> second)
		{
			From = first;
			To = second;
		}

		private bool IsIncident(Node<TEdge, TNode> node) => From == node || To == node;

		public Node<TEdge, TNode> OtherNode(Node<TEdge, TNode> node)
		{
			if (!IsIncident(node)) throw new ArgumentException();
			return From == node ? To : From;
		}
	}

	public class Node<TEdge, TNode>
	{
		private readonly List<Edge<TEdge, TNode>> edges = new List<Edge<TEdge, TNode>>();
		public readonly int NodeNumber;

		public TNode Data;

		public Node(int number)
		{
			NodeNumber = number;
		}

		public IEnumerable<Node<TEdge, TNode>> IncidentNodes => edges.Select(z => z.OtherNode(this));

		public IEnumerable<Edge<TEdge, TNode>> IncidentEdges => edges;

		public static Edge<TEdge, TNode> Connect(Node<TEdge, TNode> node1, Node<TEdge, TNode> node2, Graph<TEdge, TNode> graph)
		{
			if (!graph.Nodes.Contains(node1) || !graph.Nodes.Contains(node2)) throw new ArgumentException();
			var edge = new Edge<TEdge, TNode>(node1, node2);
			node1.edges.Add(edge);
			return edge;
		}

		public static void Disconnect(Edge<TEdge, TNode> edge)
		{
			edge.From.edges.Remove(edge);
			edge.To.edges.Remove(edge);
		}

		public void EnsureIncidentEdge(Edge<TEdge, TNode> edge)
		{
			if (!edges.Contains(edge))
				edges.Add(edge);
		}

		public override string ToString()
		{
			return $"{NodeNumber}";
		}
	}

	public class Graph<TEdge, TNode>
	{
		private readonly List<Node<TEdge, TNode>> nodes;

		public Node<TEdge, TNode> AddNode()
		{
			var c = nodes.Count;
			var node = new Node<TEdge, TNode>(c);
			nodes.Add(node);
			return node;
		}

		public Graph(int nodesCount)
		{
			nodes = Enumerable.Range(0, nodesCount).Select(z => new Node<TEdge, TNode>(z)).ToList();
		}

		public Node<TEdge, TNode> this[int index] => nodes[index];

		public int NodesCount => nodes.Count;

		public IEnumerable<Node<TEdge, TNode>> Nodes => nodes;

		public Edge<TEdge, TNode> DirectedConnect(int index1, int index2)
		{
			return Node<TEdge, TNode>.Connect(nodes[index1], nodes[index2], this);
		}

		public void NonDirectedConnect(int index1, int index2, TEdge edge)
		{
			var e = DirectedConnect(index1, index2);
			e.Data = edge;
			e = DirectedConnect(index2, index1);
			e.Data = edge;
		}

		public void Delete(Edge<TEdge, TNode> edge)
		{
			Node<TEdge, TNode>.Disconnect(edge);
		}

		public IEnumerable<Edge<TEdge, TNode>> Edges => nodes.SelectMany(z => z.IncidentEdges).Distinct();
	}
}