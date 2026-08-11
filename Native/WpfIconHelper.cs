using System.Windows;
using System.Windows.Media;

namespace WinYouTubeMusic.Native
{
    public static class WpfIconHelper
    {
        public static DrawingImage CreatePreviousIcon()
        {
            var group = new DrawingGroup();
            using (var dc = group.Open())
            {
                var brush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                brush.Freeze();

                // Left bar
                dc.DrawRectangle(brush, null, new Rect(4, 5, 3, 14));

                // Left triangle
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = new Point(19, 5) };
                figure.Segments.Add(new LineSegment(new Point(9, 12), true));
                figure.Segments.Add(new LineSegment(new Point(19, 19), true));
                figure.IsClosed = true;
                geometry.Figures.Add(figure);
                geometry.Freeze();

                dc.DrawGeometry(brush, null, geometry);
            }
            group.Freeze();
            return new DrawingImage(group);
        }

        public static DrawingImage CreatePlayIcon(bool isPlaying)
        {
            var group = new DrawingGroup();
            using (var dc = group.Open())
            {
                var brush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                brush.Freeze();

                if (isPlaying)
                {
                    // Pause: Two vertical bars
                    dc.DrawRectangle(brush, null, new Rect(6, 5, 4, 14));
                    dc.DrawRectangle(brush, null, new Rect(14, 5, 4, 14));
                }
                else
                {
                    // Play: Triangle facing right
                    var geometry = new PathGeometry();
                    var figure = new PathFigure { StartPoint = new Point(7, 5) };
                    figure.Segments.Add(new LineSegment(new Point(18, 12), true));
                    figure.Segments.Add(new LineSegment(new Point(7, 19), true));
                    figure.IsClosed = true;
                    geometry.Figures.Add(figure);
                    geometry.Freeze();

                    dc.DrawGeometry(brush, null, geometry);
                }
            }
            group.Freeze();
            return new DrawingImage(group);
        }

        public static DrawingImage CreateNextIcon()
        {
            var group = new DrawingGroup();
            using (var dc = group.Open())
            {
                var brush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                brush.Freeze();

                // Right triangle
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = new Point(5, 5) };
                figure.Segments.Add(new LineSegment(new Point(15, 12), true));
                figure.Segments.Add(new LineSegment(new Point(5, 19), true));
                figure.IsClosed = true;
                geometry.Figures.Add(figure);
                geometry.Freeze();

                dc.DrawGeometry(brush, null, geometry);

                // Right bar
                dc.DrawRectangle(brush, null, new Rect(17, 5, 3, 14));
            }
            group.Freeze();
            return new DrawingImage(group);
        }
    }
}
