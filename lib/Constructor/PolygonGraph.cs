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
	                
	                foreach (var segment in currentNode.Polygon.GetCommonSegments(neightbourNode.Polygon))
	                {
						if(!currentNode.Neightbours.ContainsKey(segment))
							currentNode.Neightbours[segment] = new List<Node>();
						currentNode.Neightbours[segment].Add(neightbourNode);
					}
                }
            }
        }

        public class Node
        {
            public Polygon Polygon { get; private set; }
            public Dictionary<Segment, List<Node>> Neightbours { get; private set; }

            public Node(Polygon polygon)
            {
                Polygon = polygon;
                Neightbours = new Dictionary<Segment, List<Node>>();
            }

            public List<Node> GetNeightbours(Segment segment)
            {
                return Neightbours[segment];
            }
        }
    }
}