using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WinYouTubeMusic.Native;

public static class IconHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static IntPtr CreateIconFromBitmap(Bitmap bmp)
    {
        return bmp.GetHicon();
    }

    public static void SafeDestroyIcon(IntPtr hIcon)
    {
        if (hIcon != IntPtr.Zero)
        {
            DestroyIcon(hIcon);
        }
    }
}
