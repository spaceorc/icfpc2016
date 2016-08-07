using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.ProjectionSolver
{
    public class WayFinder
    {
        public Graph<EdgeInfo, NodeInfo> Graph;

        List<Rational> desiredLength;
        Rational maxLength;

        List<PPath>[,] currentPathes;

        public Dictionary<Rational, Dictionary<int, List<PPath>>> Result = new Dictionary<Rational, Dictionary<int, List<PPath>>>();

        List<PPath>[,] CreateMatrix()
        {
            var ps = new List<PPath>[Graph.NodesCount, Graph.NodesCount];
            for (int i = 0; i < Graph.NodesCount; i++)
                for (int j = 0; j < Graph.NodesCount; j++)
                    ps[i, j] = new List<PPath>();
            return ps;
        }

        public WayFinder(Graph<EdgeInfo, NodeInfo> Graph, List<Rational> desiredLength)
        {
            currentPathes = CreateMatrix();
            for (int i = 0; i < Graph.NodesCount; i++)
                currentPathes[i, i].Add(new PPath { edges = new List<Edge<EdgeInfo, NodeInfo>>(), length = 0 });

            this.desiredLength = desiredLength.OrderBy(z => z).ToList();
            maxLength = desiredLength.Max();
        }

        void RegisterOutput(PPath path)
        {
            foreach(var len in desiredLength)
                if (path.length==len)
                {
                    MakeMetrics(path);

                    if (!Result.ContainsKey(len)) Result[len] = new Dictionary<int, List<PPath>>();
                    var dict = Result[len];
                    var begin = path.FirstEdge.From.NodeNumber;
                    if (!dict.ContainsKey(begin)) dict[begin] = new List<PPath>();
                    dict[begin].Add(path);
                }
        }

        void MakeMetrics(PPath path)
        {
            path.originality1 = (double)path.edges.AllNodes().Distinct().Count();
            path.originality1 /= path.edges.Count;

            path.straightness = 0.0;
            for (int i=0;i<path.edges.Count-1;i++)
            {
                var first = path.edges[i].Data.segment.Direction;
                var second = path.edges[i + 1].Data.segment.Direction;
                var scalar = first.X * second.X + first.Y + second.Y;
                double cos = (double)scalar / (first.Length * second.Length);
                cos = (1 - cos) / 2;
                path.straightness += cos;
            }
            path.straightness /= path.edges.Count;

            path.metric = path.originality1 + path.straightness;
        }

        void Scoring()
        {
            var comparer = new Comparison<PPath>((a, b) => a.originality1.CompareTo(b.originality1));

            foreach (var len in Result.Keys)
                foreach(var begin in Result[len].Keys)
                    Result[len][begin].Sort(comparer);
        }

        public void MakeIteration()
        {
            var newPathes = CreateMatrix();
            for (int begin=0;begin<Graph.NodesCount;begin++)
                for (int intermediate=0;intermediate<Graph.NodesCount;intermediate++)
                    foreach(var currentPath in currentPathes[begin,intermediate])
                        foreach(var edge in Graph[intermediate].IncidentEdges)
                        {
                            var end = edge.To;
                            var path = new PPath { edges = currentPath.edges.ToList(), length = currentPath.length };
                            path.edges.Add(edge);
                            path.length += edge.Data.length;
                            if (path.length > maxLength) continue;
                            newPathes[begin, end.NodeNumber].Add(path);
                            RegisterOutput(path);
                        }

            Scoring();
        }
    }
}
