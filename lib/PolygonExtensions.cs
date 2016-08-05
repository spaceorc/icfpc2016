using System.Linq;

namespace lib
{
    public static class PolygonExtensions
    {
        public static bool HasSegment(this Polygon polygon, Segment segment)
        {
            return polygon.Segments.Any(segment.Equals);
        }

        public static Segment GetCommonSegment(this Polygon polygon, Polygon thatPolygon)
        {
            foreach (var thisSegment in polygon.Segments)
            {
                foreach (var thatSegment in thatPolygon.Segments)
                {
                    if (thisSegment.Equals(thatSegment))
                        return thisSegment;
                }
            }
            return null;
        }
    }
}