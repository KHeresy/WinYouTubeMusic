using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WinYotuTubeMusic.Native;

public static class IconHelper
{
    public static IntPtr CreatePlayIcon(bool isPlaying)
    {
        using Bitmap bmp = new Bitmap(24, 24);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush brush = new SolidBrush(Color.White);

        if (isPlaying)
        {
            g.FillRectangle(brush, 6, 4, 4, 16);
            g.FillRectangle(brush, 14, 4, 4, 16);
        }
        else
        {
            PointF[] points = new PointF[]
            {
                new PointF(7, 4),
                new PointF(19, 12),
                new PointF(7, 20)
            };
            g.FillPolygon(brush, points);
        }

        return bmp.GetHicon();
    }

    public static IntPtr CreatePreviousIcon()
    {
        using Bitmap bmp = new Bitmap(24, 24);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush brush = new SolidBrush(Color.White);
        g.FillRectangle(brush, 4, 4, 3, 16);

        PointF[] points = new PointF[]
        {
            new PointF(19, 4),
            new PointF(9, 12),
            new PointF(19, 20)
        };
        g.FillPolygon(brush, points);

        return bmp.GetHicon();
    }

    public static IntPtr CreateNextIcon()
    {
        using Bitmap bmp = new Bitmap(24, 24);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush brush = new SolidBrush(Color.White);
        PointF[] points = new PointF[]
        {
            new PointF(5, 4),
            new PointF(15, 12),
            new PointF(5, 20)
        };
        g.FillPolygon(brush, points);
        g.FillRectangle(brush, 16, 4, 3, 16);

        return bmp.GetHicon();
    }
}
