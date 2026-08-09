using System;
using System.Runtime.InteropServices;

namespace WinYotuTubeMusic.Native;

public enum THUMBBUTTONFLAGS : uint
{
    THBF_ENABLED = 0x00000000,
    THBF_DISABLED = 0x00000001,
    THBF_DISMISSONCLICK = 0x00000002,
    THBF_NOBACKGROUND = 0x00000004,
    THBF_HIDDEN = 0x00000008,
    THBF_NONINTERACTIVE = 0x00000010
}

public enum THUMBBUTTONMASK : uint
{
    THB_BITMAP = 0x00000001,
    THB_ICON = 0x00000002,
    THB_TOOLTIP = 0x00000004,
    THB_FLAGS = 0x00000008
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct THUMBBUTTON
{
    public THUMBBUTTONMASK dwMask;
    public uint iId;
    public uint iBitmap;
    public IntPtr hIcon;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szTip;
    public THUMBBUTTONFLAGS dwFlags;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

[ComImport]
[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITaskbarList3
{
    // ITaskbarList
    [PreserveSig] int HrInit();
    [PreserveSig] int AddTab(IntPtr hwnd);
    [PreserveSig] int DeleteTab(IntPtr hwnd);
    [PreserveSig] int ActivateTab(IntPtr hwnd);
    [PreserveSig] int SetSetActiveAlt(IntPtr hwnd);

    // ITaskbarList2
    [PreserveSig] int MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

    // ITaskbarList3
    [PreserveSig] int SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
    [PreserveSig] int SetProgressState(IntPtr hwnd, int tbpFlags);
    [PreserveSig] int RegisterTab(IntPtr hwnd, IntPtr hwndMDI);
    [PreserveSig] int UnregisterTab(IntPtr hwnd);
    [PreserveSig] int SetTabOrder(IntPtr hwnd, IntPtr hwndInsertBefore);
    [PreserveSig] int SetTabActive(IntPtr hwnd, IntPtr hwndMDI, uint dwReserved);
    [PreserveSig] int ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] THUMBBUTTON[] pButton);
    [PreserveSig] int ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] THUMBBUTTON[] pButton);
    [PreserveSig] int ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
    [PreserveSig] int SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
    [PreserveSig] int SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
    [PreserveSig] int SetThumbnailClip(IntPtr hwnd, ref RECT prcClip);
}

[ComImport]
[Guid("56FDF344-FD6D-11d0-958A-006097C51013")]
[ClassInterface(ClassInterfaceType.None)]
public class TaskbarList { }
