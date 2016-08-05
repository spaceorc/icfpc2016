﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.Graphs
{
    public class Edge<TEdge,TNode>
    {
        public TEdge Data { get; set; }

        public override string ToString()
        {
            return Data.ToString();
        }

        public readonly Node<TEdge,TNode> From;
        public readonly Node<TEdge, TNode> To;
        public Edge(Node<TEdge, TNode> first, Node<TEdge, TNode> second)
        {
            this.From = first;
            this.To = second;
        }
        public bool IsIncident(Node<TEdge, TNode> node)
        {
            return From == node || To == node;
        }
        public Node<TEdge, TNode> OtherNode(Node<TEdge, TNode> node)
        {
            if (!IsIncident(node)) throw new ArgumentException();
            if (From == node) return To;
            return From;
        }
    }

    public class Node<TEdge,TNode>
    {
        readonly List<Edge<TEdge, TNode>> edges = new List<Edge<TEdge, TNode>>();
        public readonly int NodeNumber;

        public TNode Data;
        public override string ToString()
        {
            return Data.ToString();
        }

        public Node(int number)
        {
            NodeNumber = number;
        }

        public IEnumerable<Node<TEdge, TNode>> IncidentNodes
        {
            get
            {
                return edges.Select(z => z.OtherNode(this));
            }
        }
        public IEnumerable<Edge<TEdge, TNode>> IncidentEdges
        {
            get
            {
                foreach (var e in edges) yield return e;
            }
        }
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
    }

    public class Graph<TEdge, TNode>
    {
        private Node<TEdge, TNode>[] nodes;

        public Graph(int nodesCount)
        {
            nodes = Enumerable.Range(0, nodesCount).Select(z => new Node<TEdge, TNode>(z)).ToArray();
        }

        public int Length { get { return nodes.Length; } }

        public Node<TEdge, TNode> this[int index] { get { return nodes[index]; } }


        public int NodesCount {  get { return nodes.Length; } }

        public IEnumerable<Node<TEdge, TNode>> Nodes
        {
            get
            {
                foreach (var node in nodes) yield return node;
            }
        }

        public Edge<TEdge, TNode> Connect(int index1, int index2)
        {
            return Node<TEdge, TNode>.Connect(nodes[index1], nodes[index2], this);
        }

        public void Delete(Edge<TEdge, TNode> edge)
        {
            Node<TEdge, TNode>.Disconnect(edge);
        }

        public IEnumerable<Edge<TEdge, TNode>> Edges
        {
            get
            {
                return nodes.SelectMany(z => z.IncidentEdges).Distinct();
            }
        }

    }
}