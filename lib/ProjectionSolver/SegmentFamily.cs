using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib

{
    public class SegmentFamily
    {
        public readonly Vector[] Points;
        public readonly Segment[] Segments;
        public SegmentFamily(Vector[] points)
        {
            this.Points = points;
            Segments = new Segment[points.Length - 1];
            for (int i = 0; i < points.Length - 1; i++)
                Segments[i] = new Segment(points[i], points[i + 1]);
        }
    }
}
