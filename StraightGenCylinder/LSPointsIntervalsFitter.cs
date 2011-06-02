using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    class LSPointsIntervalsFitter
    {
        public static Tuple<double[][], double[][], int[]> FitOptimalIntervals(double[] xs, double[] ys, double threshold = double.NaN)
        {
            Contract.Requires(xs.Length > 0);
            Contract.Requires(xs.Length == ys.Length);
            Contract.Requires(threshold > 0 || double.IsNaN(threshold));

            if (double.IsNaN(threshold))
            {
                var xStd = Math.Sqrt(xs.Variance());
                var yStr = Math.Sqrt(ys.Variance());
                threshold = Math.Max(xStd, yStr) / 20;
            }

            return EnumerateFits(xs, ys, threshold).Last(); // fits sequence improves until the last (best) fit is reached.
        }

        private static IEnumerable<Tuple<double[][], double[][], int[]>> EnumerateFits(double[] xs, double[] ys, double threshold)
        {
            Contract.Requires(xs.Length > 0);
            Contract.Requires(xs.Length == ys.Length);
            Contract.Requires(threshold > 0);

            int n = xs.Length;
            var breakIndices = new List<int> { 3 };

            while (breakIndices.Last() < n) // while we haven't reached our optimal division yet
            {
                var breakIndicesArray = breakIndices.ToArray();
                var xIntervals = LSIntervalsFitter.FitIntervals(xs, breakIndicesArray);
                var yIntervals = LSIntervalsFitter.FitIntervals(ys, breakIndicesArray);
                yield return Tuple.Create(xIntervals, yIntervals, breakIndicesArray);

                var xLastIntervalMSE = LastIntervalMSE(xIntervals, xs, breakIndices);
                var yLastIntervalMSE = LastIntervalMSE(yIntervals, ys, breakIndices);

                if (xLastIntervalMSE < threshold && yLastIntervalMSE < threshold) // our current interval is still good enough
                    breakIndices[breakIndices.Count - 1] = breakIndices.Last() + 1;
                else // we need a new interval to approximate well enough
                {
                    var nextIntervalBreak = breakIndices.Last() + 3;
                    if (nextIntervalBreak >= n)
                        breakIndices[breakIndices.Count - 1] = breakIndices.Last() + 1;
                    else
                        breakIndices.Add(nextIntervalBreak);
                }
            }
        }

        private static double LastIntervalMSE(double[][] intervals, double[] values, IList<int> breakIndices)
        {
            var intervalEnd = breakIndices.Last();
            var intervalStart = breakIndices.Count == 1 ? 0 : breakIndices.Take(breakIndices.Count - 1).Last();
            var pointsCount = intervalEnd - intervalStart + 1;

            var lastIntervalCoefficients = intervals.Last();
            var query =
                from t in Enumerable.Range(intervalStart, pointsCount)
                let realValue = values[t]
                let predictedValue = // 3rd degree polynomial
                    lastIntervalCoefficients[0] +
                    lastIntervalCoefficients[1] * t +
                    lastIntervalCoefficients[2] * t * t +
                    lastIntervalCoefficients[3] * t * t * t
                let residual = realValue - predictedValue
                select residual * residual;

            return query.Average();
        }
    }
}
