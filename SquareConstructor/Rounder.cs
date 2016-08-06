using System;
using System.Collections.Generic;
using System.Linq;
using lib;

namespace SquareConstructor
{
    public class Rounder
    {
        public static IEnumerable<Polygon[]> FindRounds(Polygon initialPolygon,
            Vector roundPoint,
            Dictionary<Segment, List<Polygon>> neightbourhood)
        {
            Segment firstSegment = null;
            var lastSegment = GetAnotherSegmentWithPoint(initialPolygon, firstSegment, roundPoint);
            var lastPoint = GetAnotherPointOfSegment(lastSegment, roundPoint);

            var neightbours = GetNeightbours(initialPolygon, firstSegment, neightbourhood);
            foreach (var neightbour in neightbours)
            {
                var nSegment = GetAnotherSegmentWithPoint(neightbour, firstSegment, roundPoint);
                var nPoint = GetAnotherPointOfSegment(nSegment, roundPoint);
                if (nPoint.Equals(lastPoint))
                    return null;//резутат
                //if (nPoint.Equals())
                //если точка совпадает с последней - это хорошо
                //если точка недокуржилась - рекусрия
                //если точка перекружилась - стоп
            }
            return null;
            //помнить про луч
        }

        private static Segment GetAnotherSegmentWithPoint(Polygon polygon, Segment segment, Vector point)
        {
            return polygon.Segments
                .FirstOrDefault(s => (s.Start.Equals(point) || s.End.Equals(point))
                                     && !s.Equals(segment));
        }

        private static Vector GetAnotherPointOfSegment(Segment segment, Vector point)
        {
            return segment.Start.Equals(point) ? segment.End : segment.Start;
        }

        private static Vector Get90Point(Vector roundPoint, Vector zeroPoint, bool clookwise)
        {
            throw new NotImplementedException();
        }

        private static Vector Get180Point(Vector roundPoint, Vector zeroPoint)
        {
            throw new NotImplementedException();
        }

        private static Polygon[] GetNeightbours(Polygon polygon, Segment segment, Dictionary<Segment, List<Polygon>> neightbourhood)
        {
            return null;
            //flip
            //contact
        }
    }
}
