using System.Windows;
using System.Windows.Media;

namespace WinYotuTubeMusic.Native;

public static class WpfIconHelper
{
    public static ImageSource CreatePlayIcon(bool isPlaying)
    {
        DrawingGroup group = new DrawingGroup();
        using (DrawingContext dc = group.Open())
        {
            // Canvas bounds
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 24, 24));

            Brush brush = Brushes.White;
            if (isPlaying)
            {
                // Pause Icon: two vertical bars
                dc.DrawRectangle(brush, null, new Rect(6, 4, 4, 16));
                dc.DrawRectangle(brush, null, new Rect(14, 4, 4, 16));
            }
            else
            {
                // Play Icon: triangle
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(new Point(7, 4), true, true);
                    ctx.LineTo(new Point(19, 12), true, false);
                    ctx.LineTo(new Point(7, 20), true, false);
                }
                dc.DrawGeometry(brush, null, geometry);
            }
        }
        return new DrawingImage(group);
    }

    public static ImageSource CreatePreviousIcon()
    {
        DrawingGroup group = new DrawingGroup();
        using (DrawingContext dc = group.Open())
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 24, 24));

            Brush brush = Brushes.White;
            dc.DrawRectangle(brush, null, new Rect(4, 4, 3, 16));

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(19, 4), true, true);
                ctx.LineTo(new Point(9, 12), true, false);
                ctx.LineTo(new Point(19, 20), true, false);
            }
            dc.DrawGeometry(brush, null, geometry);
        }
        return new DrawingImage(group);
    }

    public static ImageSource CreateNextIcon()
    {
        DrawingGroup group = new DrawingGroup();
        using (DrawingContext dc = group.Open())
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 24, 24));

            Brush brush = Brushes.White;
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(5, 4), true, true);
                ctx.LineTo(new Point(15, 12), true, false);
                ctx.LineTo(new Point(5, 20), true, false);
            }
            dc.DrawGeometry(brush, null, geometry);
            dc.DrawRectangle(brush, null, new Rect(16, 4, 3, 16));
        }
        return new DrawingImage(group);
    }
}
