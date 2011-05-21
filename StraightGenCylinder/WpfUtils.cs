using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows;

namespace StraightGenCylinder
{
    static class WpfUtils
    {
        /// <summary>
        /// Calculates linear interpolation between two points (1 - t) * p1 + t * p2
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="t">Interpolation factor</param>
        /// <returns>The resulting interpolated point</returns>
        [Pure]
        public static Point Lerp(Point p1, Point p2, double t)
        {
            var vec = t * (p2 - p1);
            return p1 + vec;
        }

        /// <summary>
        /// Computes the distance from the given point to the given line.
        /// </summary>
        /// <param name="pnt">The point to compute the distance for</param>
        /// <param name="p">First point defining the line</param>
        /// <param name="q">Second point defining the line</param>
        /// <returns>The distance from <paramref name="pnt"/> to the line defined by <paramref name="p"/> and <paramref name="q"/>.</returns>
        public static double DistanceToLine(this Point pnt, Point p, Point q)
        {
            return (pnt - pnt.ProjectOnLine(p, q)).Length;
        }

        public static Point ProjectOnLine(this Point pnt, Tuple<Point, Vector> line)
        {
            return pnt.ProjectOnLine(line.Item1, line.Item2);
        }

        public static Point ProjectOnLine(this Point pnt, Point p, Vector v)
        {
            return pnt.ProjectOnLine(p, p + v);
        }

        /// <summary>
        /// Projects a point on a line passing through p and q
        /// </summary>
        /// <param name="pnt">The point to be projected</param>
        /// <param name="p">First point defining the line</param>
        /// <param name="q">Second point defining the line</param>
        /// <returns>The point <paramref name="pnt"/> projected on the specified line segment.</returns>
        public static Point ProjectOnLine(this Point pnt, Point p, Point q)
        {
            var v = q - p;
            var u = pnt - p;

            var t = (u * v) / v.LengthSquared;
            var candidate = p + t * v;

            return candidate;
        }

        /// <summary>
        /// Projects a point on a segment.
        /// </summary>
        /// <param name="pnt">The point to project</param>
        /// <param name="segStart">Starting segment point</param>
        /// <param name="segEnd">Ending segment point</param>
        /// <returns>The point on the segment that minimizes the distance to pnt.</returns>
        [Pure]
        public static Point ProjectOnSegment(this Point pnt, Point segStart, Point segEnd)
        {
            Contract.Ensures((pnt - Contract.Result<Point>()).LengthSquared <= (pnt - segStart).LengthSquared);
            Contract.Ensures((pnt - Contract.Result<Point>()).LengthSquared <= (pnt - segEnd).LengthSquared);

            var v = segEnd - segStart;
            if (v.LengthSquared <= double.Epsilon) // segment is of length almost zero. Therefore any point is valid.
                return segStart;

            var u = pnt - segStart;
            var t = (u * v) / (v * v);

            if (t < 0)           // to the "left" of segStart
                return segStart;
            else if (t > 1)      // to the "right" of segEnd
                return segEnd;
            else                 // between segStart and segEnd
            {
                // the point segStart + t * v is still considered a candidate because of a numerical error that can occur
                // in the computation of "t". So we still need to choose the point with minimal distance to "pnt".
                // We do it to ensure the contract above is correct - that is, we find the closest point to "pnt" on the segment.
                var candidate = segStart + t * v;

                var potentialResults = new Tuple<Point, double>[]
                {
                    Tuple.Create(candidate, (candidate - pnt).LengthSquared),
                    Tuple.Create(segStart, (segStart - pnt).LengthSquared),
                    Tuple.Create(segEnd, (segEnd - pnt).LengthSquared),
                };

                return potentialResults.Minimizer(x => x.Item2).Item1;
            }
        }

        /// <summary>
        /// Calculates the minimum distance from a point to any point on a line segment.
        /// </summary>
        /// <param name="pnt">The point to calculate distance from.</param>
        /// <param name="segStart">The starting segment point</param>
        /// <param name="segEnd">Ending segment point.</param>
        /// <returns>The minimum distance between <paramref name="point"/> and any point on the segment between
        /// <paramref name="segStart"/> and <paramref name="segEnd"/>.</returns>
        [Pure]
        public static double DistanceFromSegment(this Point pnt, Point segStart, Point segEnd)
        {
            Contract.Ensures(Contract.Result<double>() >= 0);

            return (pnt - pnt.ProjectOnSegment(segStart, segEnd)).Length;
        }
    }
}
