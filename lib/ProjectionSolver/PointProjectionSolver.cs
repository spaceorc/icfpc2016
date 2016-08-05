using lib;
using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class EdgeInfo
    {
        public Segment segment;
        public Rational length;
        public override string ToString()
        {
            return segment.ToString();
        }
    }

    public class NodeInfo
    {
        public Vector Location;
        public List<Vector> Projections = new List<Vector>();

        public override string ToString()
        {
            return new string('*', Projections.Count);
        }
    }

    public class PPath
    {
        public List<Edge<EdgeInfo, NodeInfo>> edges;
        public Edge<EdgeInfo,NodeInfo> LastEdge { get { return edges[edges.Count - 1]; } }
        public Rational length;
        public override string ToString()
        {
            return ((double)length).ToString() + " : " +
                edges
                .Select(z => z.Data.segment.Start).StrJoin(" ") + " " + edges[edges.Count - 1].To.Data.Location.ToString();
        }
    }

    public class PointProjectionSolver
    {

        #region data classes 

       
       
        public class ProjectedNodeInfo
        {
            public Node<EdgeInfo, NodeInfo> Original;
            public Vector Projection;
        }

        public class ProjectedEdgeInfo
        {
            public bool IsLate;
            internal EdgeInfo Segment;
        }

   

       
        #endregion


        public Graph<ProjectedEdgeInfo, ProjectedNodeInfo> Projection;

        public Graph<EdgeInfo,NodeInfo> Graph;

#region Cycle 
        public IEnumerable<PPath> Extend(PPath path)
        {
            var output = new List<PPath>();
            var last = path.edges[path.edges.Count - 1].To;
            var edges = last.IncidentEdges.ToList();
            foreach(var e in edges)
            {
                var result = new PPath();
                result.edges = path.edges.ToList();
                result.edges.Add(e);
                result.length = path.length + e.Data.length;
                yield return result;
            }
        }

        List<PPath> TruncateBadPaths(List<PPath> p)
        {
            return p.OrderByDescending(z => VerticesIn(z)).Take(10000).ToList();
        }

        public IEnumerable<PPath> Recursive(PPath path, Rational length)
        {
            if (path.length == 1)
            {
                yield return path;
                yield break;
            }
            if (path.length > 1)
                yield break;
            

            var last = path.edges[path.edges.Count - 1];
            var edges = last.To.IncidentEdges.OrderByDescending(z=>z.Data.length).ToList();
            var bad = edges.Where(z => z.To == last.From).First();
            edges.Remove(bad);
            edges.Add(bad);
            foreach(var e in edges)
            {
                var p = new PPath();
                p.edges = path.edges.ToList();
                p.edges.Add(e);
                p.length = path.length + e.Data.length;
                foreach (var c in Recursive(p,length))
                    yield return c;
            }
        }

        int depthRejected = 0;

        public IEnumerable<PPath> DepthAlgorithm(int nodeIndex, Rational length)
        {
            var edges = Graph[nodeIndex].IncidentEdges.ToList();
            foreach (var e in edges)
            {
                var p = new PPath { edges = new[] { e }.ToList(), length = e.Data.length };
                foreach (var c in Recursive(p,length))
                    yield return c;
            }

        }

        public IEnumerable<PPath> GetStartPath()
        {
          var startNode=  Graph.Edges.OrderByDescending(z => z.Data.length).First().From;
            var edges = startNode.IncidentEdges.ToList();
            foreach(var e in edges)
                yield return new PPath { edges = new List<Edge<EdgeInfo, NodeInfo>> { e }, length = e.Data.length };

        }

        public IEnumerable<PPath> Algorithm()
        {
           
            var list = new List<PPath>();
            foreach(var e in GetStartPath())
                list.Add(e);

            while(list.Count!=0)
            {
                var avg = list.Average(z => (double)z.length);
                list = TruncateBadPaths(list);

                var otherList = new List<PPath>();
                foreach (var a in list)
                    foreach (var r in Extend(a))
                        otherList.Add(r);

                list.Clear();
                foreach(var e in otherList)
                {
                    if (e.length == 4) yield return e;
                    else if (e.length > 4) continue;
                    else list.Add(e);
                }

            }
        }

        private static Rational GetRationalEnumerableSum(IEnumerable<Rational> source)
        {
            Rational res = 0;
            foreach (var e in source)
                res += e;
            return res;
        }
        #endregion

        public IEnumerable<List<int>> SeparateTo4(PPath path)
        {
            int n = path.edges.Count;
            var matrix = new Rational[n+1, n+1];
            for (int i = 0; i <= n; i++)
            {
                matrix[i, i] = 0;
                for (int j = i + 1; j <= n; j++)
                {
                    matrix[i, j] = matrix[i, j-1] + path.edges[j-1].Data.length;
                }
            }

            for (int potentialStart=0;potentialStart<n;potentialStart++)
            {
                var separation = new List<int>();
                int t = potentialStart;
                bool ok = true;
                separation.Add(t);
                for (int k=0;k<3;k++)
                {

                    var end = Enumerable.Range(0, n + 1).Where(z => matrix[t, z] == 1).ToList();
                    if (end.Count == 0)
                    {
                        ok = false;
                        break;
                    }
                    t = end[0];
                    separation.Add(t);
                }
                if (ok) yield return separation;
            }
        }

        public List<List<Edge<EdgeInfo,NodeInfo>>> Reorder(PPath path, List<int> separation)
        {
            var result = new List<List<Edge<EdgeInfo, NodeInfo>>>();
            separation.Add(separation[0]+path.edges.Count);
            for (int i=0;i<separation.Count-1;i++)
            {
                var current = new List<Edge<EdgeInfo, NodeInfo>>();
                for (int j=separation[i];j<separation[i+1];j++)
                {
                    current.Add(path.edges[j%path.edges.Count]);
                }
                result.Add(current);
            }
            return result;
        }

        public IEnumerable<List<List<Edge<EdgeInfo,NodeInfo>>>> GetReorderings(PPath path)
        {
            foreach (var e in SeparateTo4(path))
                yield return Reorder(path, e);
        }


        public bool TryProject(List<List<Edge<EdgeInfo, NodeInfo>>> parts)
        {
            var corners = new[] { new Vector(0, 0), new Vector(0, 1), new Vector(1, 1), new Vector(1, 0) };
            var direction = new[] { new Vector(0, 1), new Vector(1, 0), new Vector(0, -1), new Vector(-1, 0) };

            var list = new List<Tuple<EdgeInfo,ProjectedNodeInfo>>();

            for (int i=0;i<4;i++)
            {
                Rational len = 0;
                for (int k=0;k<parts[i].Count;k++)
                {
                    var location = corners[i] + direction[i] * len;
                    var node = parts[i][k].From;
                    list.Add(Tuple.Create(parts[i][k].Data,new ProjectedNodeInfo { Original = node, Projection = location }));
                    len += parts[i][k].Data.length;
                }
            }


            Projection = new Graph<ProjectedEdgeInfo, ProjectedNodeInfo>(list.Count);
            for (int i=0;i<list.Count;i++)
            {
                Projection[i].Data = list[i].Item2;
                var e = Projection.DirectedConnect(i, (i + 1)%Projection.NodesCount );
                e.Data = new ProjectedEdgeInfo { IsLate = false, Segment=list[i].Item1 };
            }

            return true;
        }

        bool SegmentOrMirror(Segment a, Segment b)
        {
            if (a.Start.Equals(b.Start) && a.End.Equals(b.End)) return true;
            if (a.Start.Equals(b.End) && a.End.Equals(b.Start)) return true;
            return false;

        }

        public IEnumerable<Segment> UnusedSegments()
        {
            foreach(var e in Segments)
            {
                if (Projection.Edges.Any(z => SegmentOrMirror(z.Data.Segment.segment,e))) continue;
                yield return e;
            }
        }

        public int AddAdditionalEdges(List<Segment> segments)
        {
            //for(int i=0;i<Graph.NodesCount;i++)
            //    for (int j=i+1;j<Graph.NodesCount;j++)
            //    {
            //        if (!Graph[i].Data.IsProjected || !Graph[j].Data.IsProjected)
            //            continue;

            //        var p1 = Graph[i].Data.Projection;
            //        var p2 = Graph[j].Data.Projection;
            //        var dx = p1.X - p2.X;
            //        var dy = p1.Y - p2.Y;
            //        var length = Math.Sqrt((double)(dx * dx + dy * dy));
            //        if (segments.Any(z=>Math.Abs(z.IrrationalLength-length)<1e-06))
            //        {
            //            var e=Graph.Connect(i, j);
            //            e.Data = new EdgeInfo { addedEdge = true };
            //        }
            //    }

            int used = 0;

            foreach(var s in segments)
            {
                var starts = Projection.Nodes.Where(z => z.Data.Original.Data.Location.Equals(s.Start)).ToList();
                var ends = Projection.Nodes.Where(z => z.Data.Original.Data.Location.Equals(s.End)).ToList();
                foreach(var start in starts)
                    foreach(var end in ends)
                    {
                        var len = Arithmetic.IrrationalDistance(start.Data.Projection, end.Data.Projection);
                        if (Math.Abs(len-Arithmetic.IrrationalDistance(s.Start,s.End))<1e-5)
                        {
                            var e = Projection.DirectedConnect(start.NodeNumber, end.NodeNumber);
                            e.Data = new ProjectedEdgeInfo { IsLate = true };
                            used++;
                        }
                    }
            }

            return used;
        }

        public bool IsCircular(PPath path)
        {
            return path.edges[0].From == path.edges[path.edges.Count - 1].To;
        }

        public int VerticesIn(PPath path)
        {
            return path
                .edges
                .SelectMany(z => new[] { z.Data.segment.Start, z.Data.segment.End })
                .Distinct()
                .Count();
        }

        #region initialization
        public List<Segment> Segments = new List<Segment>();
        public List<Vector> vectors;


        void GenerateSegments(ProblemSpec spec)
        {

            Segments = spec.Segments.ToList();

            vectors = spec
                   .Segments
                   .SelectMany(z => new[] { z.Start, z.End })
                   .Distinct()
                   .ToList();
            for (int i = 0; i < Segments.Count; i++)
                for (int j = i + 1; j < Segments.Count; j++)
                {
                    var intersect = Arithmetic.GetIntersection(Segments[i], Segments[j]);
                    if (!intersect.HasValue) continue;
                    var v = intersect.Value;
                    v = new Vector(v.X.Reduce(), v.Y.Reduce());
                    if (!vectors.Contains(v))
                        vectors.Add(v);
                }


            while (true)
            {
                var segments = new List<Segment>();
                foreach (var e in Segments)
                {
                    var inter = vectors.Where(z => !z.Equals(e.Start) && !z.Equals(e.End) && Arithmetic.PointInSegment(z, e)).ToList();
                    if (inter.Count == 0)
                        segments.Add(e);
                    else
                    {
                        segments.Add(new Segment(e.Start, inter[0]));
                        segments.Add(new Segment(inter[0], e.End));
                    }
                }
                if (segments.Count == Segments.Count)
                    break;
                Segments = segments;
            }


        }

        public PointProjectionSolver(ProblemSpec spec)
        {



            GenerateSegments(spec);

            Graph = new Graph<EdgeInfo, NodeInfo>(vectors.Count);
            for (int i = 0; i < vectors.Count; i++)
                Graph[i].Data = new NodeInfo { Location = vectors[i] };

            

        
            




            int edges = 0;
            foreach (var seg in Segments)
            {
                if (!Arithmetic.IsSquare(seg.QuadratOfLength)) continue;

                var length = Arithmetic.Sqrt(seg.QuadratOfLength);

                var e = Graph.DirectedConnect(vectors.IndexOf(seg.Start), vectors.IndexOf(seg.End));
                e.Data = new EdgeInfo { length = length, segment = seg };

                e = Graph.DirectedConnect(vectors.IndexOf(seg.End), vectors.IndexOf(seg.Start));
                e.Data = new EdgeInfo { length = length, segment = new Segment(seg.End, seg.Start) };
                edges++;
            }



        }
        #endregion
    }
}
