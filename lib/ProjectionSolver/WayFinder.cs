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
        public int startNode;
        public Rational TargetLength;
        Dictionary<int, List<PPath>> currentPathes = new Dictionary<int, List<PPath>>();
        Dictionary<int, Queue<PPath>> readyPathes = new Dictionary<int, Queue<PPath>>();
        
    }
}
