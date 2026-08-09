using System;
using System.Runtime.InteropServices;

namespace WinYotuTubeMusic.Native;

public static class Subclass
{
    private const int WM_COMMAND = 0x0111;
    private const int THBN_CLICKED = 0x1800;

    public static readonly uint WM_TaskbarButtonCreated = RegisterWindowMessage("TaskbarButtonCreated");

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint RegisterWindowMessage(string lpString);

    public delegate IntPtr SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData);

    [DllImport("Comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

    [DllImport("Comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RemoveWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass);

    [DllImport("Comctl32.dll", SetLastError = true)]
    public static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    private static SUBCLASSPROC? _subclassProc;
    private static Action<uint>? _onTaskbarButtonClicked;
    private static Action? _onTaskbarButtonCreated;

    public static void Register(IntPtr hWnd, Action<uint> onTaskbarButtonClicked, Action onTaskbarButtonCreated)
    {
        _onTaskbarButtonClicked = onTaskbarButtonClicked;
        _onTaskbarButtonCreated = onTaskbarButtonCreated;
        _subclassProc = WndProc;
        SetWindowSubclass(hWnd, _subclassProc, 1001, IntPtr.Zero);
    }

    private static IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData)
    {
        if (WM_TaskbarButtonCreated != 0 && uMsg == WM_TaskbarButtonCreated)
        {
            _onTaskbarButtonCreated?.Invoke();
        }
        else if (uMsg == WM_COMMAND)
        {
            int hiword = (int)((wParam.ToInt64() >> 16) & 0xFFFF);
            int loword = (int)(wParam.ToInt64() & 0xFFFF);

            if (hiword == THBN_CLICKED)
            {
                _onTaskbarButtonClicked?.Invoke((uint)loword);
                return IntPtr.Zero;
            }
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }
}
