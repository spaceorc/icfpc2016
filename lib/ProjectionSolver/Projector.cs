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
        public List<Segment> Segments;
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
        internal List<SegmentFamily> SegmentsFamily;

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
            var dict = Graph.Nodes.ToDictionary(z => z, z => new List<NodeProjection>());
            foreach (var e in AllNodeProjections)
                dict[e.Original].Add(e);
            return dict;
        }

        public Dictionary<Segment,List<EdgeProjection>> GetEdgeFunction()
        {
            var dict = AllSegments.ToDictionary(z => z, z => new List<EdgeProjection>());
            foreach (var e in AllEdgeProjections)
                foreach(var ee in e.Segments)
                    dict[ee].Add(e);
            return dict;
        }

        

        public bool IsCompleteProjection()
        {
            var goodNodes = AllNodeProjections.Select(z => z.Original).Distinct().Count();
            if (goodNodes != Graph.NodesCount) return false;
            var goodEdges = AllEdgeProjections.SelectMany(z => z.Segments).Distinct().Count();
            if (goodEdges != AllSegments.Count) return false;
            return true;
        }
    }



    public static class Projector
    {
        public static Projection CreateProjection(List<SegmentFamily> families, List<Segment> allSegments, Graph<EdgeInfo, NodeInfo> graph)
        {
            var p = new Projection();
            p.AllSegments = allSegments.ToList();
            p.Graph = graph;
            p.SegmentsFamily = families.ToList();
            return p;
        }


        public static ProjectionStage CreateInitialProjection(List<PPath> square, Projection p)
        {
            ProjectionStage stage = new ProjectionStage();

            var corners = new[] { new Vector(0, 0), new Vector(0, 1), new Vector(1, 1), new Vector(1, 0) };
            var direction = new[] { new Vector(0, 1), new Vector(1, 0), new Vector(0, -1), new Vector(-1, 0) };


            for (int i = 0; i < 4; i++)
            {
                Rational len = 0;
                for (int k = 0; k < square[i].edges.Count; k++)
                {
                    var location = corners[i] + direction[i] * len;
                    var node = square[i].edges[k].From;

                    var pr = new NodeProjection { Original = node, Projection = location };
                    stage.Nodes.Add(pr);
                   len += square[i].edges[k].Data.length;
                }
            }

            int ptr = 0;
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
                    var begin = stage.Nodes[ptr];
                    var end = stage.Nodes[(ptr + 1) % stage.Nodes.Count];
                    ptr++;

                    stage.Edges.Add(new EdgeProjection { begin = begin, end = end, Segments = new List<Segment> { e.Data.segment } });
                }
            return stage;
        }


        class EdgeProjector
        {
            public Dictionary<Node<EdgeInfo, NodeInfo>,List<NodeProjection>> nodesMap;
            public Dictionary<Segment, List<EdgeProjection>> edgesMap;

            public EdgeProjection TryInsertFamily(SegmentFamily family, int startIndex, int count)
            {
                var segments = family.Segments.Skip(startIndex).Take(count).ToList();
                if (segments.All(z => edgesMap[z].Count != 0)) return null;
                var startP = family.Points[startIndex];
                var start = nodesMap.Keys.Where(z => z.Data.Location.Equals(startP)).FirstOrDefault();
                if (start == null) return null;
                var endP = family.Points[startIndex + count];
                var end = nodesMap.Keys.Where(z => z.Data.Location.Equals(endP)).FirstOrDefault();
                if (end == null) return null;
                foreach(var s in nodesMap[start])
                    foreach(var f in nodesMap[end])
                    {
                        var length = new Segment(s.Projection, f.Projection).QuadratOfLength;
                        if (length != new Segment(startP,endP).QuadratOfLength) continue;
                        var result = new EdgeProjection { begin = s, end = f, Segments = segments };
                        return result;
                    }
                return null;
            }
        }

       

        public static ProjectionStage AddVeryGoodEdges(Projection p)
        {

            var stage = new ProjectionStage();
            var ep = new EdgeProjector();
            ep.edgesMap = p.GetEdgeFunction();
            ep.nodesMap = p.GetNodeFunction();
            

            foreach(var f in p.SegmentsFamily)
            {
                for (int size=f.Segments.Length;size>=1;size--)
                    for (int start=0;start<=f.Segments.Length-size;start++)
                    {
                        var res = ep.TryInsertFamily(f, start, size);
                        if (res!=null)
                        {
                            stage.Edges.Add(res);
                            return stage;
                        }
                    }
            }
            return null;
        }

        
    }


 
    
}
