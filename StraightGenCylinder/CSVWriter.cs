using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Globalization;

namespace StraightGenCylinder
{
    static class CSVWriter
    {
        public static void Write(IEnumerable<Point> points, string fileName)
        {
            Write(new IEnumerable<Point>[] { points }, fileName);
        }

        public static void Write(IList<IEnumerable<Point>> pointsLists, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                for (int i = 0; i < pointsLists.Count; ++i)
                {
                    WritePoints(pointsLists[i], writer);
                    if (i < pointsLists.Count - 1)
                        WriteSeperator(writer);
                }
            }
        }

        private static void WritePoints(IEnumerable<Point> points, StreamWriter writer)
        {
            foreach (var pnt in points)
                writer.WriteLine("{0}, {1}", pnt.X, pnt.Y);
        }

        private static void WriteSeperator(StreamWriter writer)
        {
            var ouput = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", double.NaN, double.NaN);
            writer.WriteLine(ouput);
        }
    }
}
