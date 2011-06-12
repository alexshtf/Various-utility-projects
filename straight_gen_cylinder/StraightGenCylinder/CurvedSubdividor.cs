using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    class SubdivisionResult
    {
        public Point[] MiddlePoints { get; set; }
        public Vector[] Normals { get; set; }
        public Point[] ProjectedPositiveSide { get; set; }
        public Point[] ProjectedNegativeSide { get; set; }
    }

    static class CurvedSubdividor
    {
        public static SubdivisionResult Subdivide(Point[] l1, Point[] l2)
        {
            // create a polygon from l1, l2: we assume here that l1 and l2 have the same direction
            var polygon = l1.Concat(l2.Reverse()).ToArray();

            var filteredPoints = TwoLinesMedialAxis.Compute(l1, l2, polygon, proximityDistance: 3.0);

            // connect the extreme points with a long path (dijkstra algorithm)
            var path = PointsToPolylineConverter.Convert(filteredPoints);

            // smooth the path
            var smoothed = SmoothPath(path);

            // create the result
            return new SubdivisionResult
            {
                MiddlePoints = smoothed.Item1,
                Normals = smoothed.Item2,
            };
        }

        private static Tuple<Point[], Vector[]> SmoothPath(Point[] path)
        {
            var xs = path.Select(pnt => pnt.X).ToArray();
            var ys = path.Select(pnt => pnt.Y).ToArray();

            var optimalFit = LSPointsIntervalsFitter.FitOptimalIntervals(xs, ys);
            var xInterval = optimalFit.Item1;
            var yInterval = optimalFit.Item2;
            var breakIndices = optimalFit.Item3;

            var smoothedPoints = new Point[path.Length];
            var smoothedNormals = new Vector[path.Length];
            for (int t = 0; t < path.Length; ++t)
            {
                var sx = IntervalsSampler.SampleIntervals(xInterval, t, breakIndices);
                var sy = IntervalsSampler.SampleIntervals(yInterval, t, breakIndices);

                smoothedPoints[t] = new Point(sx.Value, sy.Value);
                smoothedNormals[t] = new Vector(-sy.Derivative, sx.Derivative); // perpendicular to the tangent vector (x, y) => (-y, x)
                smoothedNormals[t].Normalize();
            }

            return Tuple.Create(smoothedPoints, smoothedNormals);
        }
    }
}
