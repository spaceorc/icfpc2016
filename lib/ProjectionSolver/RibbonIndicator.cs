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

        public static Rational? GetRibbonWidth(PointProjectionSolver solver)
        {
            double percent;
            var pointGist = PointGist(solver);
            var parallelGist = ParallelGist(solver, out percent);
            return Indicate(pointGist, parallelGist, percent);
        }

        public static Rational? Indicate(KeyValuePair<Rational, double>[] pointGist, KeyValuePair<Rational,
            double>[] parallelGist, double percent)
        {
            //var strength = GetStrength(pointGist, parallelGist, percent);
            //if (!strength.HasValue)
            //    return null;

            //if (strength < 1.0)
            //    return null;

            if (pointGist.Length == 0)
                return null;
            var pp1 = pointGist.Where(p => p.Key.Numerator == 1);
            if (pp1.Any())
                return pp1.First().Key;
            if (parallelGist.Any())
                return parallelGist.First().Key;
            return pointGist.First().Key;
        }

        public static double? GetStrength(KeyValuePair<Rational, double>[] pointGist,
            KeyValuePair<Rational, double>[] parallelGist, double percent)
        {
            if (pointGist.Length < 2)
                return null;

            var g1 = pointGist[0];
            var g2 = pointGist[1];

            if (g1.Key.Numerator != 1 || g1.Key.Denomerator < 4)
                return 0.0;

            if (percent < 0.5)
                return 0.0;

            var pointStrength = g1.Value / g2.Value;
            var parallelStrength = parallelGist.Length > 1 ? g1.Value/g1.Value : g1.Value;
            return (pointStrength + parallelStrength)/2;
        }

        public static KeyValuePair<Rational, double>[] PointGist(PointProjectionSolver solver)
        {
            var result = new Dictionary<Rational, double>();
            var segments = GetSegments(solver);
            var vectors = GetVectors(solver, segments);
            foreach (var vector in vectors)
            {
                foreach (var segment in segments)
                {
                    var d = segment.Distance2To(vector);
                    if (d == 0 || d == 1)
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
            return result.OrderByDescending(p => p.Value).Take(3).ToArray();
        }

        public static KeyValuePair<Rational, double>[] ParallelGist(PointProjectionSolver solver, out double hasParallelFactor)
        {
            var result = new Dictionary<Rational, double>();
            var segments = GetSegments(solver);
            var hasParallelCount = 0;
            foreach (var s1 in segments)
            {
                bool hasParallel = false;
                foreach (var s2 in segments)
                {
                    var sd = Arithmetic.InDistance2(s1.Start, s2);
                    var ed = Arithmetic.InDistance2(s1.End, s2);

                    if (sd.HasValue && ed.HasValue)
                    {
                        if (sd.Value == ed.Value)
                        {
                            var d = sd.Value;
                            if (d == 0 || d == 1)
                                continue;
                            if (!Arithmetic.IsSquare(d))
                                continue;

                            d = Arithmetic.Sqrt(d);
                            d = d.Reduce();
                            if (!result.ContainsKey(d))
                                result[d] = 0;
                            result[d]++;
                            if (!hasParallel)
                            {
                                hasParallelCount++;
                                hasParallel = true;
                            }
                        }
                    }
                }
            }
            hasParallelFactor = ((double)hasParallelCount)/segments.Length;
            return result.OrderByDescending(p => p.Value).Take(3).ToArray();
        }

        private static HashSet<Vector> GetVectors(PointProjectionSolver solver, Segment[] segments)
        {
            var vectors = new HashSet<Vector>();
            foreach (var segment in segments)
            {
                vectors.Add(segment.Start);
                vectors.Add(segment.End);
            }
            return vectors;
        }

        private static Segment[] GetSegments(PointProjectionSolver solver)
        {
            return solver.spec.Segments;
            //return solver.AllSegments
            //    .Where(s => Arithmetic.IsSquare(s.QuadratOfLength))
            //    .ToArray();
        }
    }
}