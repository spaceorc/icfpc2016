using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class NewAlgorithm
    {
        PointProjectionSolver solver;
        public Dictionary<int, List<PPath>> outPathes;

        public IEnumerable<List<PPath>> Combine(List<PPath> path)
        {
            var ed = path[path.Count - 1].edges;
            foreach (var e in outPathes[ed[ed.Count - 1].To.NodeNumber])
            {
                var r = path.ToList();
                r.Add(e);
                if (r.Count==4)
                {
                    if (e.LastEdge.To == path[0].edges[0].From)
                        yield return r;
                }
                else foreach (var y in Combine(r))
                        yield return y;
            }
        }

        public void Build(PointProjectionSolver solver)
        {
            outPathes = new Dictionary<int, List<PPath>>();
            this.solver = solver;
            for (int i = 0; i < solver.Graph.NodesCount; i++)
            {
                outPathes[i] = solver.DepthAlgorithm(i, 1).ToList();
            }
        }

        public static List<List<PPath>> GetAll(PointProjectionSolver solver)
        {

            var res = new List<List<PPath>>();
            var alg = new NewAlgorithm();
            alg.Build(solver);
            foreach (var e in alg.outPathes.SelectMany(z => z.Value))
            {
                foreach (var r in alg.Combine(new[] { e }.ToList()))
                {
                    res.Add(r);
                }
            }
            return res;
        }


    }
}
