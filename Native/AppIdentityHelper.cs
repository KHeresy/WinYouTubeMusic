using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace WinYouTubeMusic.Native;

public static class AppIdentityHelper
{
    public const string AppId = "KHeresy.WinYouTubeMusic";
    public const string AppDisplayName = "YouTube Music";

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

    public static void Initialize()
    {
        try
        {
            // 1. Set explicit AppUserModelID for the current process
            SetCurrentProcessExplicitAppUserModelID(AppId);

            // 2. Register HKCU registry entry for AppUserModelId
            RegisterRegistry();

            // 3. Create or update Start Menu shortcut with AUMID
            EnsureStartMenuShortcut();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppIdentityHelper] Error: {ex.Message}");
        }
    }

    private static void RegisterRegistry()
    {
        try
        {
            string exePath = Environment.ProcessPath ?? "";
            using var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppUserModelId\{AppId}");
            key.SetValue("DisplayName", AppDisplayName, RegistryValueKind.String);
            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                key.SetValue("IconUri", exePath, RegistryValueKind.String);
            }
            key.SetValue("ShowInSettings", 1, RegistryValueKind.DWord);
        }
        catch
        {
        }
    }

    private static void EnsureStartMenuShortcut()
    {
        try
        {
            string exePath = Environment.ProcessPath ?? "";
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return;

            string programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            if (!Directory.Exists(programsFolder))
            {
                Directory.CreateDirectory(programsFolder);
            }

            string shortcutPath = Path.Combine(programsFolder, "YouTube Music.lnk");

            var shellLink = (IShellLinkW)new CShellLink();
            shellLink.SetPath(exePath);
            shellLink.SetWorkingDirectory(Path.GetDirectoryName(exePath) ?? "");
            shellLink.SetDescription("YouTube Music Desktop");
            shellLink.SetIconLocation(exePath, 0);

            var propertyStore = (IPropertyStore)shellLink;
            var key = new PROPERTYKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5); // PKEY_AppUserModel_ID
            var pv = PROPVARIANT.FromString(AppId);
            try
            {
                propertyStore.SetValue(ref key, ref pv);
                propertyStore.Commit();
            }
            finally
            {
                pv.Dispose();
            }

            var persistFile = (IPersistFile)shellLink;
            persistFile.Save(shortcutPath, true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppIdentityHelper] Shortcut creation error: {ex.Message}");
        }
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    [ClassInterface(ClassInterfaceType.None)]
    private class CShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ppszFileName);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
    private interface IPropertyStore
    {
        void GetCount(out uint cProps);
        void GetAt(uint iProp, out PROPERTYKEY pkey);
        void GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        void SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        void Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;

        public PROPERTYKEY(Guid guid, uint id)
        {
            fmtid = guid;
            pid = id;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PROPVARIANT : IDisposable
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(2)] public ushort wReserved1;
        [FieldOffset(4)] public ushort wReserved2;
        [FieldOffset(6)] public ushort wReserved3;
        [FieldOffset(8)] public IntPtr pwszVal;

        public const ushort VT_LPWSTR = 31;

        public static PROPVARIANT FromString(string val)
        {
            var pv = new PROPVARIANT();
            pv.vt = VT_LPWSTR;
            pv.pwszVal = Marshal.StringToCoTaskMemUni(val);
            return pv;
        }

        public void Dispose()
        {
            if (pwszVal != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pwszVal);
                pwszVal = IntPtr.Zero;
            }
            vt = 0;
        }
    }
}
