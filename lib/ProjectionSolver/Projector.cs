using lib;
using lib.Graphs;
using lib.ProjectionSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace lib.ProjectionSolver
{

  


    public static class Projector
    {



        public static ProjectionStage CreateInitialProjection(List<PPath> square, Projection p)
        {
            ProjectionStage stage = new ProjectionStage();

            var corners = new[] { new Vector(0, 0), new Vector(p.SideX, 0), new Vector(p.SideX, p.SideY), new Vector(0, p.SideY) };
            var direction = new[] { new Vector(1, 0), new Vector(0, 1), new Vector(-1, 0), new Vector(0, -1) };


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







        
        public static List<EdgeProjection> TryInsertFamily(SegmentFamilySubset subset, ProjectionCurrentState state)
        {



            if (state.SegmentIsCovered(subset)) return null;
            var start = state.GetNode(subset.Begin);
            if (start == null) return null;
            var end = state.GetNode(subset.End);
            if (end == null) return null;

            var result = new List<EdgeProjection>();

            foreach (var s in state.nodesMap[start])
                foreach (var f in state.nodesMap[end])
                {
                    var length = new Segment(s.Projection, f.Projection).QuadratOfLength;
                    if (length != new Segment(subset.Begin, subset.End).QuadratOfLength) continue;
                    result.Add(new EdgeProjection { begin = s, end = f, Segments = subset.Insides.ToList() });
                }
            return result;
        }




        public static IEnumerable<SegmentFamilySubset> GetAllPossibleSegments(List<SegmentFamily> family)
        {
            var maxLength = family.Max(z => z.Segments.Length) + 1;

            for (int size=1;size<maxLength;size++)
                foreach (var f in family)
                    for (int start = 0; start <= f.Segments.Length - size; start++)
                        yield return new SegmentFamilySubset(f, start, size);
        }

        public static ProjectionStage AddVeryGoodEdges(Projection p)
        {

            var stage = new ProjectionStage();

            var ep = p.GetCurrentState();

            foreach (var subs in GetAllPossibleSegments(p.SegmentsFamily))
            {
                var startNode = ep.GetNode(subs.Begin)?.NodeNumber;
                var endNode = ep.GetNode(subs.End)?.NodeNumber;

                //if ((startNode == 9 && endNode == 8) || (startNode == 8 && endNode == 9)) Console.Write("!");


                var res = TryInsertFamily(subs, ep);
                if (res != null && res.Count!=0)
                {
                    stage.Edges.AddRange(res);
                  //  foreach (var t in res) Console.WriteLine($"{t.begin.Original.NodeNumber} {t.end.Original.NodeNumber}");
                    return stage;
                }
            }
            return null;
        }


        public class AdjoinedSegmentFamilySubset
        {
            public SegmentFamilySubset family;
            public Node<EdgeInfo, NodeInfo> ProjectedNode;
            public Node<EdgeInfo, NodeInfo> NonProjectedNode;

        }

        public static IEnumerable<int[]> GetCounting(int[] sizes)
        {
            var result = new int[sizes.Length];
            while(true)
            {
                bool ok = false;
                for(int position=0;position<sizes.Length;position++)
                {
                    result[position]++;
                    if (result[position]>=sizes[position])
                    {
                        result[position] = 0;
                        continue;
                    }
                    else
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok) break;
                yield return result;
            }
        }


        public static IEnumerable<ProjectionStage> TrySquashPoint(ProjectionCurrentState state, List<AdjoinedSegmentFamilySubset> edges)
        {
            var sizes = edges.Select(z => state.nodesMap[z.ProjectedNode].Count).ToArray();
            foreach (var p in GetCounting(sizes))
            {
                var proj = new NodeProjection[sizes.Length];
                for (int i = 0; i < sizes.Length; i++)
                    proj[i] = state.nodesMap[edges[i].ProjectedNode][p[i]];
                var vars = Arithmetic.RationalTriangulate(edges[0].family.Segment, edges[1].family.Segment, proj[0].Projection, proj[1].Projection);
                if (vars == null) continue;
                foreach(var v in vars)
                {
                    if (v.X < 0 || v.X > 1 || v.Y < 0 || v.Y > 1) continue;

                    var stage = new ProjectionStage();
                    var sq= new NodeProjection { Original = edges[0].NonProjectedNode, Projection = v };
                    stage.Nodes.Add(sq);
                    bool ok = true;
                    for (int k=0;k<sizes.Length;k++)
                    {
                        var len = new Segment(v, proj[k].Projection);
                        if (len.QuadratOfLength != edges[k].family.Segment.QuadratOfLength)
                        {
                            ok = false;
                            break;
                        }
                        stage.Edges.Add(new EdgeProjection { begin = sq, end = proj[k], Segments = edges[k].family.Insides.ToList() });
                    }
                    if (ok)
                        yield return stage;
                }
            }
        }

        public static IEnumerable<ProjectionStage> FindSquashPoint(Projection p)
        {
            var state = p.GetCurrentState();
            var store = new Dictionary<Node<EdgeInfo,NodeInfo>, List<AdjoinedSegmentFamilySubset>>();
            foreach(var e in GetAllPossibleSegments(p.SegmentsFamily))
            {
                var start = state.GetNode(e.Begin);
                var end = state.GetNode(e.End);

                var startFound = state.IsMapped(start);
                var endFound = state.IsMapped(end);

                if (startFound && endFound) continue;
                if (!startFound && !endFound) continue;

                var a = new AdjoinedSegmentFamilySubset {  family = e };
                a.ProjectedNode = startFound ? start : end;
                a.NonProjectedNode = startFound ? end : start;
                if (!store.ContainsKey(a.NonProjectedNode)) store[a.NonProjectedNode] = new List<AdjoinedSegmentFamilySubset>();
                store[a.NonProjectedNode].Add(a);
            }

            store = store.Where(z => z.Value.Count >= 2).ToDictionary(z => z.Key, z => z.Value);

            foreach (var e in store)
                foreach (var st in TrySquashPoint(state, e.Value))
                    yield return st;
        }
    }
}