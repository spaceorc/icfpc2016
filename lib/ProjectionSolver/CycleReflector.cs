using System;
using System.Collections.Generic;
using System.Linq;
using lib.Graphs;
using PEdge = lib.PointProjectionSolver.ProjectedEdgeInfo;
using PNode = lib.PointProjectionSolver.ProjectedNodeInfo;

namespace lib
{
    public class CycleReflector
    {
        public static List<List<GNode<PEdge, PNode>>> GetUnribbonedCycles(List<List<GNode<PEdge, PNode>>> ribbonCycles)
        {
            var result = new List<List<GNode<PEdge, PNode>>>();
            var ribbonHeight = GetRibbonHeight(ribbonCycles);
            Rational shift = 0;
            while (true)
            {
                if (shift + ribbonHeight <= 1)
                {
                    result.AddRange(GetShiftedCycles(ribbonCycles, shift));
                }

                if (shift + ribbonHeight + ribbonHeight <= 1)
                {
                    result.AddRange(GetReflectedAndShiftedCycles(ribbonCycles, ribbonHeight, shift));
                }

                shift += ribbonHeight + ribbonHeight;
                if (shift >= 1)
                    break;
            }
            return result;
        }

        private static Rational GetRibbonHeight(List<List<GNode<PEdge, PNode>>> cycles)
        {
            Rational result = 0;
            foreach (var cycle in cycles)
            {
                foreach (var gNode in cycle)
                {
                    var value1 = gNode.From.Data.Projection.Y;
                    if (value1 > result)
                        result = value1;

                    var value2 = gNode.To.Data.Projection.Y;
                    if (value2 > result)
                        result = value2;
                }
            }
            return result;
        }

        public static List<List<GNode<PEdge, PNode>>> GetShiftedCycles(List<List<GNode<PEdge, PNode>>> cycles, Rational shiftH)
        {
            return CloneCyclesWithUpdateProjection(cycles, v => ShiftY(v, shiftH));
        }

        public static List<List<GNode<PEdge, PNode>>> GetReflectedAndShiftedCycles(List<List<GNode<PEdge, PNode>>> cycles,
            Rational reflectionH, Rational shiftH)
        {
            return CloneCyclesWithUpdateProjection(cycles, v => ShiftY(ReflectY(v, reflectionH), shiftH));
        }

        private static Vector ShiftY(Vector v, Rational shift)
        {
            return new Vector(v.X, v.Y + shift);
        }

        private static Vector ReflectY(Vector v, Rational height)
        {
            return new Vector(v.X, height - v.Y + height);
        }

        private static List<List<GNode<PEdge, PNode>>> CloneCyclesWithUpdateProjection(List<List<GNode<PEdge, PNode>>> cycles,
            Func<Vector, Vector> updateProjection)
        {
            var result = CloneCycles(cycles);
            foreach (var cycle in result)
            {
                foreach (var gNode in cycle)
                {
                    gNode.From.Data.Projection = updateProjection(gNode.From.Data.Projection);
                    gNode.To.Data.Projection = updateProjection(gNode.To.Data.Projection);
                }
            }
            return result;
        }

        private static List<List<GNode<PEdge, PNode>>> CloneCycles(List<List<GNode<PEdge, PNode>>> cycles)
        {
            return cycles.Select(CloneCycle).ToList();
        }

        private static List<GNode<PEdge, PNode>> CloneCycle(List<GNode<PEdge, PNode>> cycle)
        {
            return cycle.Select(CloneGNode).ToList();
        }

        private static GNode<PEdge, PNode> CloneGNode(GNode<PEdge, PNode> gNode)
        {
            return new GNode<PEdge, PNode>(CloneEdge(gNode.Edge), gNode.FromFrom);
        }

        private static Edge<PEdge, PNode> CloneEdge(Edge<PEdge, PNode> edge)
        {
            return new Edge<PEdge, PNode>(CloneNode(edge.From), CloneNode(edge.To));
        }

        private static Node<PEdge, PNode> CloneNode(Node<PEdge, PNode> node)
        {
            var projection = node.Data.Projection;
            var newProjection = new Vector(projection.X, projection.Y);
            var newData = new PNode
            {
                Projection = newProjection,
                Original = node.Data.Original
            };
            return new Node<PEdge, PNode>(node.NodeNumber) { Data = newData };
        }
    }
}