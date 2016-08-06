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
        public Edge<EdgeInfo, NodeInfo> LastEdge { get { return edges[edges.Count - 1]; } }
        public Edge<EdgeInfo, NodeInfo> FirstEdge { get { return edges[0]; } }
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
            [Obsolete]
            public bool IsLate;
            [Obsolete]
            internal EdgeInfo Segment;
        }

   

       
        #endregion

        public Graph<ProjectedEdgeInfo, ProjectedNodeInfo> Projection;
        public ProblemSpec spec;
        public List<Segment> AllSegments;
        public List<Vector> vectors;
        public Graph<EdgeInfo, NodeInfo> Graph;
    }
}
