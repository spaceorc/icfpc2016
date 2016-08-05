using lib;
using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class PointProjectionSolver
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



        public class Path
        {
            public List<Edge<EdgeInfo,Vector>> edges;
            public Rational length;
            public override string ToString()
            {
                return ((double)length).ToString() + " : " +
                    edges
                    .Select(z => z.Data.segment.Start).StrJoin(" ") + " " + edges[edges.Count - 1].To.ToString() ;
            }
        }


        Graph<EdgeInfo,Vector> Graph;

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
                list.Add(new Path { edges = new List<Edge<EdgeInfo,Vector>> { e }, length = e.Data.length});

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

        public void SeparateTo4(Path path)
        {
            var i = 0;
            var queue = new Queue<Edge<EdgeInfo, Vector>>();
            var one = new Rational(1, 1);
            Rational sum;
            while ((sum = GetRationalEnumerableSum(queue.Select(x => x.Data.length))) != one)
                if (sum < one)
                    queue.Enqueue(path.edges[i++]);
                else
                    queue.Dequeue();

            var rIndex = 1;
            var result = new List<List<Edge<EdgeInfo, Vector>>>();
            result.Add(queue.ToList());
            while ((sum = GetRationalEnumerableSum(result[rIndex].Select(x=>x.Data.length))) < one)
                result[rIndex].Add(path.edges[i++]);
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

            
            Graph = new Graph<EdgeInfo,Vector>(vectors.Count);
            for (int i = 0; i < vectors.Count; i++)
                Graph[i].Data = vectors[i];

            foreach (var seg in spec.Segments)
            {
                if (!Arithmetic.IsSquare(seg.QuadratOfLength)) continue;

                var length = Arithmetic.Sqrt(seg.QuadratOfLength);

                var e = Graph.Connect(vectors.IndexOf(seg.Start), vectors.IndexOf(seg.End));
                e.Data = new EdgeInfo { length = length, segment = seg };

                e = Graph.Connect(vectors.IndexOf(seg.End), vectors.IndexOf(seg.Start));
                e.Data = new EdgeInfo { length = length, segment = new Segment(seg.End, seg.Start) };
     }

        }
    }
}
