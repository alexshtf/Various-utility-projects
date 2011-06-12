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
            var sourceTarget = GetSourceTarget(l1, l2, filteredPoints);
            var path = GetShortestPath(filteredPoints, sourceTarget.Item1, sourceTarget.Item2);

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

        private static Tuple<int, int> GetSourceTarget(Point[] l1, Point[] l2, Point[] filteredPoints)
        {
            var sourceMidpoint = WpfUtils.Lerp(l1[0], l2[0], 0.5);
            var targetMidpoint = WpfUtils.Lerp(l1.Last(), l2.Last(), 0.5);
            var indices = Enumerable.Range(0,filteredPoints.Length);

            var source = indices.OrderBy(i => (sourceMidpoint - filteredPoints[i]).LengthSquared).First();
            var target = indices.OrderBy(i => (targetMidpoint - filteredPoints[i]).LengthSquared).First();
            return Tuple.Create(source, target);
        }

        private static Point[] GetShortestPath(Point[] points, int source, int target)
        {
            Func<int, int, double> weight = (x, y) => (points[x] - points[y]).Length;
            var graph = GetPointsGraph(points);
            var path = Dijkstra.Compute(graph, weight, source, target);
            var result = path.Select(i => points[i]).ToArray();
            return result;
        }

        private static IEnumerable<Tuple<int, int>> GetPointsGraph(Point[] points)
        {
            var searchStructure = new NaivePointsSearchStructure(points);
            return from i in Enumerable.Range(0, points.Length)
                   let pnt = points[i]
                   from j in FindNearPoints(pnt, searchStructure.PointsInRect, distance: 3.0)
                   where i != j
                   select Tuple.Create(i, j);
        }

        private static IEnumerable<int> FindNearPoints(Point pnt, Func<Rect, int[]> pointsInRect, double distance)
        {
            var result = new int[0];
            while (result.Length < 2)
            {
                var rect = new Rect(pnt - new Vector(distance, distance), pnt + new Vector(distance, distance));
                result = pointsInRect(rect);
                distance = distance * 1.5;
            }

            return result;
        }
    }
}
