using lib;
using lib.Graphs;
using Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{

    public class NodeProjection
    {
        public Node<EdgeInfo, NodeInfo> Original;
        public Vector Projection;
    }

    public class EdgeProjection
    {
        public NodeProjection begin;
        public NodeProjection end;
        public Segment Segment;
    }

    public class ProjectionStage
    {
        public List<NodeProjection> Nodes = new List<NodeProjection>();
        public List<EdgeProjection> Edges = new List<EdgeProjection>();
    }

     public class Projection
    {
        public Graph<EdgeInfo, NodeInfo> Graph;
        public List<Segment> AllSegments;
        public Stack<ProjectionStage> Stages = new Stack<ProjectionStage>();

        public IEnumerable<NodeProjection> AllNodeProjections
        {
            get
            {
                return Stages.SelectMany(z => z.Nodes);
            }
        }

        public IEnumerable<EdgeProjection> AllEdgeProjections
        {
            get
            {
                return Stages.SelectMany(z => z.Edges);
            }
        }

        public Dictionary<Node<EdgeInfo,NodeInfo>,List<NodeProjection>> GetNodeFunction()
        {
            var dict = Graph.Nodes.ToDictionary(z => z, z => new List<NodeProjection>);
            foreach (var e in AllNodeProjections)
                dict[e.Original].Add(e);
            return dict;
        }

        public Dictionary<Segment,List<EdgeProjection>> GetEdgeFunction()
        {
            var dict = AllSegments.ToDictionary(z => z, z => new List<EdgeProjection>());
            foreach (var e in AllEdgeProjections)
                dict[e.Segment].Add(e);
            return dict;
        }

        

        public bool IsCompleteProjection()
        {
            var goodNodes = AllNodeProjections.Select(z => z.Original).Distinct().Count();
            if (goodNodes != Graph.NodesCount) return false;
            var goodEdges = AllEdgeProjections.Select(z => z.Segment).Distinct().Count();
            if (goodEdges != AllSegments.Count) return false;
            return true;
        }
    }



    public static class Projector
    {
        public static Projection CreateProjection(List<Segment> allSegments, Graph<EdgeInfo, NodeInfo> graph)
        {
            var p = new Projection();
            p.AllSegments = allSegments.ToList();
            p.Graph = graph;
            return p;
        }


        public static ProjectionStage CreateInitialProjection(List<PPath> square, Projection p)
        {
            ProjectionStage stage = new ProjectionStage();

            var corners = new[] { new Vector(0, 0), new Vector(0, 1), new Vector(1, 1), new Vector(1, 0) };
            var direction = new[] { new Vector(0, 1), new Vector(1, 0), new Vector(0, -1), new Vector(-1, 0) };


            var forward = new Dictionary<Node<EdgeInfo, NodeInfo>, NodeProjection>();

            for (int i = 0; i < 4; i++)
            {
                Rational len = 0;
                for (int k = 0; k < square[i].edges.Count; k++)
                {
                    var location = corners[i] + direction[i] * len;
                    var node = square[i].edges[k].From;

                    var pr = new NodeProjection { Original = node, Projection = location };
                    stage.Nodes.Add(pr);
                    forward[node] = pr;
                    len += square[i].edges[k].Data.length;
                }
            }

            foreach (var s in square)
                foreach (var e in s.edges)
                {
                    Segment seg = e.Data.segment;
                    if (!p.AllSegments.Contains(seg))
                    {
                        seg = new Segment(e.Data.segment.End, e.Data.segment.Start);
                        if (!p.AllSegments.Contains(seg))
                            throw new Exception();
                    }
                    stage.Edges.Add(new EdgeProjection { begin = forward[e.From], end = forward[e.To], Segment = e.Data.segment });
                }
            return stage;
        }
        

        public static ProjectionStage AddVeryGoodEdges(Projection p)
        {
            var stage = new ProjectionStage();
            var nodesMap = p.GetNodeFunction();
            var edgesMap = p.GetEdgeFunction();
            
            foreach(var e in edgesMap.Keys)
            {
                if (edgesMap[e].Count != 0) continue;
                var start = nodesMap.Keys.Where(z => z.Data.Location.Equals(e.Start)).FirstOrDefault();
                if (start == null) continue;
                var end = nodesMap.Keys.Where(z => z.Data.Location.Equals(e.End)).FirstOrDefault();
                if (end == null) continue;

                foreach(var s in nodesMap[start])
                    foreach(var f in nodesMap[end])
                    {
                        var length = new Segment(s.Projection, f.Projection).QuadratOfLength;
                        if (length != e.QuadratOfLength) continue;
                        stage.Edges.Add(new EdgeProjection { begin = s, end = f, Segment = e });
                        return stage;
                    }

            }
            return null;
        }

        
    }


 
    
}
