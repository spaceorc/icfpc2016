using System.Collections.Generic;
using System.Linq;
using lib.ProjectionSolver;

namespace lib
{
    public class RibbonIndicator
    {
        public static Rational? GetRibbonWidth(ProblemSpec spec)
        {
            var solver = SolverMaker.CreateSolver(spec);
            return GetRibbonWidth(solver);
        }

        private static Rational? GetRibbonWidth(PointProjectionSolver solver)
        {
            var gist = RibbonGist(solver);
            return Indicate(gist);
        }

        public static Rational? Indicate(KeyValuePair<Rational, double>[] gist)
        {
            var g1 = gist[0];
            var g2 = gist[1];
            if (g1.Key.Numerator == 1 &&
                g1.Key.Denomerator > 4)
            {
                if (2 * g1.Value > 3 * g2.Value && g1.Value > 2)
                {
                    return g1.Key;
                }
            }
            return null;
        }

        public static KeyValuePair<Rational, double>[] RibbonGist(PointProjectionSolver solver)
        {
            var result = new Dictionary<Rational, double>();
            var rationalSegments = solver.AllSegments
                .Where(s => Arithmetic.IsSquare((Rational) s.QuadratOfLength))
                .ToArray();
            var rationalVectors = new HashSet<Vector>();
            foreach (var segment in rationalSegments)
            {
                rationalVectors.Add(segment.Start);
                rationalVectors.Add(segment.End);
            }
            foreach (var vector in rationalVectors)
            {
                foreach (var segment in rationalSegments)
                {
                    var d = segment.Distance2To(vector);
                    if (d == 0)
                        continue;
                    d = d.Reduce();
                    //if (d.Numerator != 1)
                    //    continue;

                    if (!result.ContainsKey(d))
                        result[d] = 0;
                    result[d] += segment.IrrationalLength;
                }
            }
            return result.OrderByDescending(p => p.Value).Take(10).ToArray();
        }
    }
}