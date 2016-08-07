using lib;
using lib.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.ProjectionSolver
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


    public class ProjectionCurrentState
    {
        public Dictionary<Node<EdgeInfo, NodeInfo>, List<NodeProjection>> nodesMap;
        public Dictionary<Segment, List<EdgeProjection>> edgesMap;


        public Node<EdgeInfo, NodeInfo> GetNode(Vector p)
        {
            return nodesMap.Keys.Where(z => z.Data.Location.Equals(p)).FirstOrDefault();
        }

        public bool SegmentIsCovered(SegmentFamilySubset subs)
        {
            return subs.Insides.All(z => edgesMap[z].Count != 0);
        }

        public bool IsMapped(Node<EdgeInfo, NodeInfo> node)
        {
            return node != null && nodesMap[node].Count != 0;
        }
    }

    public partial class Projection
    {
        public readonly Graph<EdgeInfo, NodeInfo> Graph;
        public readonly List<Segment> AllSegments;
        public readonly Stack<ProjectionStage> Stages = new Stack<ProjectionStage>();
        public readonly List<SegmentFamily> SegmentsFamily;
        public readonly Rational SideX;
        public readonly Rational SideY;

        public Projection(Graph<EdgeInfo, NodeInfo> Graph, List<Segment> AllSegments, List<SegmentFamily> SegmentsFamily, Rational SideX, Rational SideY)
        {
            this.Graph = Graph;
            this.AllSegments = AllSegments;
            this.SegmentsFamily = SegmentsFamily;
            this.SideX = SideX;
            this.SideY = SideY;
        }


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

        public Dictionary<Node<EdgeInfo, NodeInfo>, List<NodeProjection>> GetNodeFunction()
        {
            var dict = Graph.Nodes.ToDictionary(z => z, z => new List<NodeProjection>());
            foreach (var e in AllNodeProjections)
                dict[e.Original].Add(e);
            return dict;
        }

        public Dictionary<Segment, List<EdgeProjection>> GetEdgeFunction()
        {
            var dict = AllSegments.ToDictionary(z => z, z => new List<EdgeProjection>());
            foreach (var e in AllEdgeProjections)
                foreach (var ee in e.Segments)
                    dict[ee].Add(e);
            return dict;
        }

        public ProjectionCurrentState GetCurrentState()
        {
            var state = new ProjectionCurrentState();
            state.edgesMap = GetEdgeFunction();
            state.nodesMap = GetNodeFunction();
            return state;
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

   


}
