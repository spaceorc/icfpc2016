using System.Collections.Generic;
using System.Linq;
using lib;

namespace SquareConstructor
{
    public class PolygonGraph
    {
        public Node[] Nodes { get; }

        public PolygonGraph(Polygon[] polygons)
        {
            Nodes = polygons.Select(p => new Node(p)).ToArray();

            foreach (var currentNode in Nodes)
            {
                foreach (var neightbourNode in Nodes)
                {
                    if (currentNode.Equals(neightbourNode))
                        continue;
                    if (currentNode.Polygon.GetCommonSegment(neightbourNode.Polygon) != null)
                        currentNode.Neightbours.Add(neightbourNode);
                }
            }
        }

        public class Node
        {
            public Polygon Polygon { get; private set; }
            public List<Node> Neightbours { get; private set; }

            public Node(Polygon polygon)
            {
                Polygon = polygon;
                Neightbours = new List<Node>();
            }

            public Node[] GetNeightbours(Segment segment)
            {
                return Neightbours.Where(n => PolygonExtensions.HasSegment(n.Polygon, segment)).ToArray();
            }
        }
    }
}