using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.ProjectionSolver
{
    public class SegmentFamilySubset
    {
        public SegmentFamily family;
        public int start;
        public int count;
        public IEnumerable<Segment> Insides { get { return family.Segments.Skip(start).Take(count); } }
        public Rational QuadratLength { get; private set; }

        public SegmentFamilySubset(SegmentFamily family, int start, int count)
        {
            this.family = family;
            this.start = start;
            this.count = count;
            QuadratLength = new Segment(Begin, End).QuadratOfLength;
            Segment = new Segment(Begin, End);
        }



        public Vector Begin
        {
            get { return family.Points[start]; }
        }

        public Vector End
        {
            get { return family.Points[start + count]; }
        }

        public Segment Segment { get; internal set; }
    }
}
