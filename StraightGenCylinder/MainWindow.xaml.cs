using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics.Contracts;

namespace StraightGenCylinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Polyline currentStroke;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            currentStroke = new Polyline { Stroke = Brushes.Black, StrokeThickness = 1 };
            currentStroke.Points.Add(e.GetPosition(canvas));
            canvas.Children.Add(currentStroke);
            canvas.CaptureMouse();
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentStroke = null;
            canvas.ReleaseMouseCapture();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentStroke != null)
                currentStroke.Points.Add(e.GetPosition(canvas));
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void computeStraight_Click(object sender, RoutedEventArgs e)
        {
            var polylines = canvas.Children.OfType<Polyline>();
            if (polylines.Count() != 2)
            {
                MessageBox.Show("Must have 2 polylines");
                return;
            }

            var l1 = polylines.ElementAt(0).Points.ToArray();
            var l2 = polylines.ElementAt(1).Points.ToArray();

            var subdivision = SilhouettesSubdividor.Subdivide(l1, l2);

            var n = subdivision.Item1.Length;
            Contract.Assume(subdivision.Item1.Length == subdivision.Item2.Length);

            for (int i = 0; i < n; ++i)
            {
                var p1 = subdivision.Item1[i];
                var p2 = subdivision.Item2[i];
                var line = CreateLine(p1, p2);
                canvas.Children.Add(line);
            }
        }

        private FrameworkElement CreateLine(Point p1, Point p2)
        {
            var lineGeometry = new LineGeometry(p1, p2);
            var p1Ellipse = new EllipseGeometry(p1, 3, 3);
            var p2Ellipse = new EllipseGeometry(p2, 3, 3);

            var geometryGroup = new GeometryGroup
            {
                Children = 
                {
                    lineGeometry,
                    p1Ellipse,
                    p2Ellipse,
                }
            };

            var path = new Path();
            path.Data = geometryGroup;
            path.Stroke = Brushes.CornflowerBlue;
            path.StrokeThickness = 1;

            return path;
        }

        private void store_Click(object sender, RoutedEventArgs e)
        {
            var polylines = canvas.Children.OfType<Polyline>().ToArray();
            ImageWriter.Store(polylines[0].Points, 512, 512, "curve1.png");
            ImageWriter.Store(polylines[1].Points, 512, 512, "curve2.png");
            ImageWriter.Store(canvas, "output.png");

            CSVWriter.Write(polylines[0].Points, "curve1.csv");
            CSVWriter.Write(polylines[1].Points, "curve2.csv");
        }

        private void computeCurved_Click(object sender, RoutedEventArgs e)
        {
            var polylines = canvas.Children.OfType<Polyline>();
            if (polylines.Count() != 2)
            {
                MessageBox.Show("Must have 2 polylines");
                return;
            }

            var l1 = polylines.ElementAt(0).Points.ToArray();
            var l2 = polylines.ElementAt(1).Points.ToArray();

            var subdivisionResult = CurvedSubdividor.Subdivide(l1, l2);
            canvas.Children.Add(VisualizePoints(subdivisionResult.Item1));
            //canvas.Children.Add(VisualizeMatch(subdivisionResult.Item2, subdivisionResult.Item3));
        }

        private UIElement VisualizeMatch(Point[] left, Point[] right)
        {
            Contract.Requires(left.Length == right.Length);
            int n = left.Length;

            var geometry = new GeometryGroup();
            for (int i = 0; i < n; ++i)
            {
                var l = left[i];
                var r = right[i];
                geometry.Children.Add(new EllipseGeometry(l, 2, 2));
                geometry.Children.Add(new EllipseGeometry(r, 2, 2));
                geometry.Children.Add(new LineGeometry(l, r));
            }
            geometry.Freeze();

            return new Path
            {
                Stroke = Brushes.Blue,
                Fill = new SolidColorBrush { Color = Colors.Blue, Opacity = 0.2 },
                Data = geometry,
            };
        }

        private UIElement VisualizePolyline(Point[] points)
        {
            throw new NotImplementedException();
        }

        private UIElement VisualizePoints(Point[] point)
        {
            var path = new Path();
            path.Stroke = Brushes.Blue;
            path.StrokeThickness = 1;
            path.Fill = new SolidColorBrush { Color = Colors.Blue, Opacity = 0.2 };

            var geometry = new GeometryGroup();
            foreach (var pnt in point.Skip(1).Take(point.Length - 2))
                geometry.Children.Add(new EllipseGeometry(pnt, 2, 2));

            geometry.Children.Add(new EllipseGeometry(point[0], 5, 5));
            geometry.Children.Add(new EllipseGeometry(point[point.Length - 1], 5, 5));

            geometry.Freeze();
            path.Data = geometry;

            return path;
        }
    }
}
