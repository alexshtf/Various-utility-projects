using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    class KDTreeSearchStructure2D : IPointsSearchStructure2D
    {
        private const int LEAF_THRESHOLD = 10;

        private readonly TreeNode root;

        public KDTreeSearchStructure2D(IList<Point> points)
        {
            Contract.Requires(points != null);

            root = RecursiveBuildTree(new Random(), points, Enumerable.Range(0, points.Count).ToArray(), isXAxis: true);
        }

        public int[] PointsInRect(Rect rect)
        {
            var result = RecursiveGetPoints(rect);
            return result.ToArray();
        }

        #region private points search methods

        private IEnumerable<int> RecursiveGetPoints(Rect rect)
        {
            return RecursiveGetPoints(rect, root);
        }

        private IEnumerable<int> RecursiveGetPoints(Rect rect, TreeNode root)
        {
            var childrenPoints =
                from child in EmptyIfNull(root.Children)
                where child.BoundingBox.IntersectsWith(rect)
                from item in RecursiveGetPoints(rect, child)
                select item;

            return childrenPoints.Concat(EmptyIfNull(root.PointIndices));
        }

        #endregion

        #region private tree construction methods

        private Rect GetBoundingRect(IEnumerable<Point> points)
        {
            var x = points.Select(p => p.X);
            var y = points.Select(p => p.Y);

            return new Rect(new Point(x.Min(), y.Min()), new Point(x.Max(), y.Max()));
        }

        private TreeNode RecursiveBuildTree(Random random, IList<Point> points, IList<int> indices, bool isXAxis)
        {
            var bbox = GetBoundingRect(ElementsAt(points, indices));

            if (points.Count < LEAF_THRESHOLD)
                return new TreeNode { Children = null, BoundingBox = bbox, PointIndices = indices.ToArray() };
            else
            {
                Func<Point, double> valueExtractor = 
                    isXAxis ? (Func<Point, double>)(pnt => pnt.X) 
                            : (Func<Point, double>)(p => p.Y);

                var pivotIndex = indices[random.Next(indices.Count)];
                var pivotPoint = points[pivotIndex];
                var pivot = valueExtractor(pivotPoint);

                var leftSubIndices = from i in indices
                                     where valueExtractor(points[i]) < pivot
                                     select i;
                var rightSubIndices = from i in indices
                                      where valueExtractor(points[i]) > pivot
                                      select i;

                var middleSubIndices = Enumerable.Range(0, points.Count).Except(leftSubIndices).Except(rightSubIndices);
                return new TreeNode
                {
                    Children = new TreeNode[] 
                    { 
                        RecursiveBuildTree(random, points, leftSubIndices.ToArray(), !isXAxis),
                        RecursiveBuildTree(random, points, rightSubIndices.ToArray(), !isXAxis),
                    },
                    PointIndices = ElementsAt(indices, middleSubIndices),
                    BoundingBox = bbox,
                };
            }
        }


        private static IEnumerable<T> EmptyIfNull<T>(T[] items)
        {
            if (items == null)
                return Enumerable.Empty<T>();
            else
                return items;
        }

        private static T[] ElementsAt<T>(IList<T> items, IEnumerable<int> indices)
        {
            var query = from idx in indices select items[idx];
            return query.ToArray();
        }

        #endregion

        #region TreeNode class

        private class TreeNode
        {
            public int[] PointIndices { get; set; }
            public Rect BoundingBox { get; set; }

            public TreeNode[] Children { get; set; }
        }

        #endregion
    }
}
