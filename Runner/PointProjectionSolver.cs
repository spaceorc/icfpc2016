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
	public class PointProjectionSolver
    {


        public class NodeInfo
        {
            public Vector Location;
            public List<Vector> Projections = new List<Vector>();

            public override string ToString()
            {
                return new string('*', Projections.Count);
            }
        }

        public class ProjectedNodeInfo
        {
            public Node<EdgeInfo, NodeInfo> Original;
            public Vector Projection;
        }

        public class ProjectedEdgeInfo
        {
            public bool IsLate;
        }

        public Graph<ProjectedEdgeInfo,ProjectedNodeInfo> Projection;


        public class Path
        {
            public List<Edge<EdgeInfo, NodeInfo>> edges;
            public Rational length;
            public override string ToString()
            {
                return ((double)length).ToString() + " : " +
                    edges
                    .Select(z => z.Data.segment.Start).StrJoin(" ") + " " + edges[edges.Count - 1].To.Data.Location.ToString() ;
            }
        }


        public Graph<EdgeInfo,NodeInfo> Graph;

        public IEnumerable<Path> Extend(Path path)
        {
            var output = new List<Path>();
            var last = path.edges[path.edges.Count - 1].To;
            var edges = last.IncidentEdges.ToList();
            foreach(var e in edges)
            {
                var result = new Path();
                result.edges = path.edges.ToList();
                result.edges.Add(e);
                result.length = path.length + e.Data.length;
                yield return result;
            }
        }

        public List<Path> Algorithm()
        {
            var result = new List<Path>();

            var list = new List<Path>();
            var startNode = Graph[0];
            var edges = startNode.IncidentEdges.ToList();
            foreach(var e in startNode.IncidentEdges)
                list.Add(new Path { edges = new[] { e }.ToList(), length = e.Data.length});

            while(list.Count!=0)
            {
                var avg = list.Average(z => (double)z.length);

                var otherList = new List<Path>();
                foreach (var a in list)
                    foreach (var r in Extend(a))
                        otherList.Add(r);

                list.Clear();
                foreach(var e in otherList)
                {
                    if (e.length == 4) result.Add(e);
                    else if (e.length > 4) continue;
                    else list.Add(e);
                }

            }

            return result
                .Where(z => IsCircular(z))
                .OrderByDescending(z => VerticesIn(z))
                .ToList();
        }

        private static Rational GetRationalEnumerableSum(IEnumerable<Rational> source)
        {
            Rational res = 0;
            foreach (var e in source)
                res += e;
            return res;
        }

        public List<int> SeparateTo4(Path path)
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
                if (ok) return separation;
            }
            return null;
        }

        public List<List<Edge<EdgeInfo,NodeInfo>>> Reorder(Path path, List<int> separation)
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


        public bool TryProject(Path path)
        {
            var separation = SeparateTo4(path);
            if (separation == null) return false;
            var parts = Reorder(path, separation);
            var corners = new[] { new Vector(0, 0), new Vector(0, 1), new Vector(1, 1), new Vector(1, 0) };
            var direction = new[] { new Vector(0, 1), new Vector(1, 0), new Vector(0, -1), new Vector(-1, 0) };

            var list = new List<ProjectedNodeInfo>();

            for (int i=0;i<4;i++)
            {
                Rational len = 0;
                for (int k=0;k<parts[i].Count;k++)
                {
                    var location = corners[i] + direction[i] * len;
                    var node = parts[i][k].From;
                    list.Add(new ProjectedNodeInfo { Original = node, Projection = location });
                    len += parts[i][k].Data.length;
                }
            }


            Projection = new Graph<ProjectedEdgeInfo, ProjectedNodeInfo>(list.Count);
            for (int i=0;i<list.Count;i++)
            {
                Projection[i].Data = list[i];
                var e = Projection.Connect(i, (i + 1)%Projection.NodesCount );
                e.Data = new ProjectedEdgeInfo { IsLate = false };
            }

            return true;
        }

        public void AddAdditionalEdges(List<Segment> segments)
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
                            var e = Projection.Connect(start.NodeNumber, end.NodeNumber);
                            e.Data = new ProjectedEdgeInfo { IsLate = true };
                        }
                    }
            }
        }

        public bool IsCircular(Path path)
        {
            return path.edges[0].From == path.edges[path.edges.Count - 1].To;
        }

        public int VerticesIn(Path path)
        {
            return path
                .edges
                .SelectMany(z => new[] { z.Data.segment.Start, z.Data.segment.End })
                .Distinct()
                .Count();
        }


        public PointProjectionSolver(ProblemSpec spec)
        {
            var vectors = spec
               .Segments
               .SelectMany(z => new[] { z.Start, z.End })
               .Distinct()
               .ToList();




            Graph = new Graph<EdgeInfo, NodeInfo>(vectors.Count);
            for (int i = 0; i < vectors.Count; i++)
                Graph[i].Data = new NodeInfo { Location = vectors[i] };

            var initialSegments = spec.Segments.ToList();

            while (true)
            {
                var segments = new List<Segment>();
                foreach (var e in initialSegments)
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
                if (segments.Count == initialSegments.Count)
                    break;
                initialSegments = segments;
            }

        
            




            int edges = 0;
            foreach (var seg in initialSegments)
            {
                if (!Arithmetic.IsSquare(seg.QuadratOfLength)) continue;

                var length = Arithmetic.Sqrt(seg.QuadratOfLength);

                var e = Graph.Connect(vectors.IndexOf(seg.Start), vectors.IndexOf(seg.End));
                e.Data = new EdgeInfo { length = length, segment = seg };

                e = Graph.Connect(vectors.IndexOf(seg.End), vectors.IndexOf(seg.Start));
                e.Data = new EdgeInfo { length = length, segment = new Segment(seg.End, seg.Start) };
                edges++;
            }



        }
    }
}
