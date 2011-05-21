using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace StraightGenCylinder
{
    static class ImageWriter
    {
        public static void Store(IEnumerable<Point> points, int width, int height, string fileName)
        {
            var canvas = new Canvas { Background = Brushes.White };
            canvas.Width = width;
            canvas.Height = height;
            canvas.Children.Add(new Polyline { Stroke = Brushes.Black, StrokeThickness = 1, Points = new PointCollection(points) });
            canvas.Measure(new Size(width, height));
            canvas.Arrange(new Rect(0, 0, canvas.DesiredSize.Width, canvas.DesiredSize.Height));
            RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);
            Store(canvas, fileName);
        }

        public static void Store(FrameworkElement fwElement, string fileName)
        {
            var width = (int)fwElement.ActualWidth;
            var height = (int)fwElement.ActualHeight;
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(fwElement);

            var bwImage = new FormatConvertedBitmap(rtb, PixelFormats.BlackWhite, null, 0);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bwImage));
            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
    }
}
