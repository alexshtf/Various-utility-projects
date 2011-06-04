using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace StraightGenCylinder
{
    class NaivePointsSearchStructure
    {
        private readonly IList<Point> points;

        public NaivePointsSearchStructure(IList<Point> points)
        {
            this.points = points;
        }

        public int[] PointsInRect(Rect rect)
        {
            var query = from i in Enumerable.Range(0, points.Count)
                        where rect.Contains(points[i])
                        select i;

            return query.ToArray();
        }
    }
}
