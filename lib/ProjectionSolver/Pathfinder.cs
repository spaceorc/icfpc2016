using lib;
using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public static class Pathfinder
    {
      
        public static IEnumerable<PPath> RecursiveDepthSearch(Graph<EdgeInfo, NodeInfo> Graph, PPath path, Rational length)
        {
            if (path.length == length)
            {
                yield return path;
                yield break;
            }
            if (path.length > 1)
                yield break;


            var last = path.edges[path.edges.Count - 1];
            var edges = last.To.IncidentEdges.OrderByDescending(z => z.Data.length).ToList();
            var bad = edges.Where(z => z.To == last.From).First();
            edges.Remove(bad);
            edges.Add(bad);
            foreach (var e in edges)
            {
                var p = new PPath();
                p.edges = path.edges.ToList();
                p.edges.Add(e);
                p.length = path.length + e.Data.length;
                foreach (var c in RecursiveDepthSearch(Graph, p, length))
                    yield return c;
            }
        }

        public static IEnumerable<PPath> DepthSeatch(Graph<EdgeInfo,NodeInfo> Graph, int startNode, Rational length)
        {
            var edges = Graph[startNode].IncidentEdges.ToList();
            foreach (var e in edges)
            {
                var p = new PPath { edges = new[] { e }.ToList(), length = e.Data.length };
                foreach (var c in RecursiveDepthSearch(Graph, p, length))
                    yield return c;
            }
        }

        public static IEnumerable<PPath> FindAllPathes(Graph<EdgeInfo,NodeInfo> Graph, Rational length, double originalityBorder=0)
        {
            for (int i = 0; i < Graph.NodesCount; i++)
                foreach (var e in DepthSeatch(Graph, i, length))
                {
                    var difVertices = e.edges.AllNodes().Distinct().Count();
                    e.originality = (difVertices) / e.edges.Count;
                    if (e.originality > originalityBorder)
                        yield return e;
                }
        }

        static IEnumerable<List<PPath>> FindAllCyclesRecursive(List<PPath> cycle, Dictionary<int,List<PPath>> map)
        {
            if (cycle.Count==4)
            {
                if (cycle[0].FirstEdge.From == cycle[3].LastEdge.To)
                    yield return cycle;
                yield break;
            }
            foreach(var e in map[cycle[cycle.Count-1].LastEdge.To.NodeNumber])
            {
                var c = cycle.ToList();
                c.Add(e);
                foreach (var r in FindAllCyclesRecursive(c, map))
                    yield return r;
            }
        }

        public static IEnumerable<List<PPath>> FindAllCycles(List<PPath> pathes)
        {
            var map = pathes.GroupBy(z => z.FirstEdge.From.NodeNumber).ToDictionary(z => z.Key, z => z.ToList());
            foreach (var e in map.Keys)
                foreach (var s in map[e])
                    foreach (var c in FindAllCyclesRecursive(new List<PPath> { s }, map))
                    {
                        yield return c;
                    }
        }

        #region

        public static Graph<EdgeInfo, NodeInfo> BuildGraph(ProblemSpec spec)
        {
            var r = MakeSegmentsWithIntersections(spec.Segments);
            return BuildGraph(r.Item1, r.Item2);
        }

        public static Graph<EdgeInfo,NodeInfo> BuildGraph(List<Segment> Segments, List<Vector> vectors)
        {
            var Graph = new Graph<EdgeInfo, NodeInfo>(vectors.Count);
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
            return Graph;
        }

        public static IEnumerable<Segment> GenerateAllSmallSegments(IEnumerable<SegmentFamily> families)
        {
            foreach (var f in families)
                for (int i = 0; i < f.Points.Length - 1; i++)
                    yield return new Segment(f.Points[i], f.Points[i + 1]);
        }

        public static Tuple<List<SegmentFamily>,List<Vector>> MakeSegmentsWithIntersections(IEnumerable<Segment> __segments)
        {
            var Segments = __segments.ToList();

            var vectors = Segments
                   .SelectMany(z => new[] { z.Start, z.End })
                   .Distinct()
                   .ToList();

            for (int i = 0; i < Segments.Count; i++)
                for (int j = i + 1; j < Segments.Count; j++)
                {
                    var intersect = Arithmetic.GetIntersection(Segments[i], Segments[j]);
                    if (!intersect.HasValue) continue;
                    var v = intersect.Value;
                    if (!vectors.Contains(v))
                        vectors.Add(v);
                }

            var result = new List<SegmentFamily>();

            foreach(var e in Segments)
            {
                var points = vectors
                    .Where(z => Arithmetic.PointInSegment(z, e))
                    .OrderBy(z => new Segment(e.Start, z).QuadratOfLength)
                    .ToArray();
                result.Add(new SegmentFamily(points));
            }

            return Tuple.Create(result, vectors);

        }
        #endregion
    }
}
