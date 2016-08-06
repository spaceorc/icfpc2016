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
        public SegmentFamily(Vector[] points)
        {
            this.Points = points;
        }
    }
}
