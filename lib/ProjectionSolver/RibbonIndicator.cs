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
            if (gist.Length < 2) return null;

            var g1 = gist[0];
            var g2 = gist[1];
            if (g1.Key.Numerator != 1 || g1.Key.Denomerator < 4) return null;

            if (g1.Value < 2) return null;

            var strength = g1.Value / g2.Value;

            if (strength < 1.2) return null;

            return g1.Key;
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
                    if (!Arithmetic.IsSquare(d))
                        continue;

                    d = Arithmetic.Sqrt(d);
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