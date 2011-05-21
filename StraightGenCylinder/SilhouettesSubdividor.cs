using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace StraightGenCylinder
{
    static class SilhouettesSubdividor
    {
        public static Tuple<Point[], Point[], Point[]> Subdivide(Point[] l1, Point[] l2)
        {
            var allPoints = l1.Concat(l2).ToArray();
            var pcaLine = PCALine.Compute(allPoints);

            var minPoint = allPoints.Minimizer(p => ProjectedLinePosition(p, pcaLine.Item1, pcaLine.Item2)).ProjectOnLine(pcaLine);
            var maxPoint = allPoints.Minimizer(p => -ProjectedLinePosition(p, pcaLine.Item1, pcaLine.Item2)).ProjectOnLine(pcaLine);

            var samples = SampleSegment(minPoint, maxPoint);

            var points1 = new List<Point>();
            var points2 = new List<Point>();
            var centers = new List<Point>();
            var perp = new Vector(-pcaLine.Item2.Y, pcaLine.Item2.X);
            for (int i = 0; i < samples.Length; ++i)
            {
                var p1 = DirectionalProject(samples[i], perp, l1);
                var p2 = DirectionalProject(samples[i], perp, l2);
                if (p1 != null && p2 != null)
                {
                    points1.Add(p1.Value);
                    points2.Add(p2.Value);
                    centers.Add(samples[i]);
                }
            }

            return Tuple.Create(points1.ToArray(), points2.ToArray(), centers.ToArray());
        }

        private static Point? DirectionalProject(Point p, Vector v, Point[] polyline)
        {
            foreach (var pair in polyline.SeqPairs())
            {
                var segStart = pair.Item1;
                var segEnd = pair.Item2;
                Point result;
                if (LineIntersectSegment(p, v, segStart, segEnd, out result))
                    return result;
            }

            return null;
        }

        private static bool LineIntersectSegment(Point p, Vector v, Point segStart, Point segEnd, out Point result)
        {
            var a11 = v.X;
            var a12 = segStart.X - segEnd.X;
            var a21 = v.Y;
            var a22 = segStart.Y - segEnd.Y;
            var b1 = segStart.X - p.X;
            var b2 = segStart.Y - p.Y;

            var solution = LinearSolve(a11, a12, a21, a22, b1, b2);
            if (solution != null)
            {
                var beta = solution.Value.Y;
                result = segStart + beta * (segEnd - segStart);
                if (beta < 0 || beta > 1)
                    return false;
                else
                    return true;

            }
            else
            {
                result = default(Point);
                return false;
            }
        }

        private static Point? LinearSolve(double a11, double a12, double a21, double a22, double b1, double b2)
        {
            var det = a11 * a22 - a12 * a21;
            if (det != 0)
            {
                var x = -(a12 * b2 - a22 * b1) / det;
                var y = (a11 * b2 - a21 * b1) / det;
                return new Point(x, y);
            }
            else
                return null;
        }

        private static Point[] SampleSegment(Point pMin, Point pMax, int count = 50)
        {
            var v = pMax - pMin;
            return
                (from i in Enumerable.Range(0, count)
                 let t = i / (double)(count - 1)
                 select pMin + t * v).ToArray();
        }

        private static double ProjectedLinePosition(Point p, Point l0, Vector v)
        {
            var u = p - l0;
            var t = (u * v) / v.LengthSquared;
            return t;
        }
    }
}
