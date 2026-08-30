using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace WinYouTubeMusic.Services
{
    public class WindowSettings
    {
        public double Left { get; set; } = double.NaN;
        public double Top { get; set; } = double.NaN;
        public double Width { get; set; } = 1280;
        public double Height { get; set; } = 768;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WindowState WindowState { get; set; } = WindowState.Normal;
    }

    public class AppSettings
    {
        public WindowSettings Window { get; set; } = new WindowSettings();
    }

    public static class SettingsService
    {
        private static readonly object _lock = new object();
        private static AppSettings? _currentSettings;

        private static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinYouTubeMusic",
            "settings.json");

        public static AppSettings Current
        {
            get
            {
                if (_currentSettings == null)
                {
                    lock (_lock)
                    {
                        _currentSettings ??= LoadSettings();
                    }
                }
                return _currentSettings;
            }
        }

        public static AppSettings LoadSettings()
        {
            try
            {
                string path = SettingsFilePath;
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch { }

            return new AppSettings();
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                lock (_lock)
                {
                    _currentSettings = settings;
                    string path = SettingsFilePath;
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(path, json);
                }
            }
            catch { }
        }

        public static void RestoreWindowPlacement(Window window)
        {
            var settings = Current.Window;

            // Check if we have previously saved valid coordinates
            if (double.IsNaN(settings.Left) || double.IsNaN(settings.Top) ||
                settings.Width <= 0 || settings.Height <= 0)
            {
                return;
            }

            // Ensure dimensions respect minimum bounds
            double width = Math.Max(window.MinWidth > 0 ? window.MinWidth : 600, settings.Width);
            double height = Math.Max(window.MinHeight > 0 ? window.MinHeight : 400, settings.Height);

            // Validate if the coordinates intersect with the current virtual screen
            var virtualScreenRect = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            // Title bar region of the restored window
            var targetTitleBarRect = new Rect(settings.Left, settings.Top, width, Math.Min(height, 50));
            var intersection = Rect.Intersect(virtualScreenRect, targetTitleBarRect);

            // If the window title bar is not at least partially visible on screen, reset to center
            if (intersection.IsEmpty || intersection.Width < 50 || intersection.Height < 20)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Width = width;
                window.Height = height;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = settings.Left;
                window.Top = settings.Top;
                window.Width = width;
                window.Height = height;
            }

            if (settings.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        public static void SaveWindowPlacement(Window window)
        {
            var settings = Current;
            var winSettings = settings.Window;

            if (window.WindowState == WindowState.Normal)
            {
                if (!double.IsNaN(window.Left) && !double.IsInfinity(window.Left))
                {
                    winSettings.Left = window.Left;
                }
                if (!double.IsNaN(window.Top) && !double.IsInfinity(window.Top))
                {
                    winSettings.Top = window.Top;
                }

                double w = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
                double h = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
                if (!double.IsNaN(w) && !double.IsInfinity(w) && w > 0)
                {
                    winSettings.Width = w;
                }
                if (!double.IsNaN(h) && !double.IsInfinity(h) && h > 0)
                {
                    winSettings.Height = h;
                }

                winSettings.WindowState = WindowState.Normal;
            }
            else if (window.WindowState == WindowState.Maximized)
            {
                Rect restoreBounds = window.RestoreBounds;
                if (!restoreBounds.IsEmpty &&
                    !double.IsNaN(restoreBounds.Left) && !double.IsNaN(restoreBounds.Top) &&
                    restoreBounds.Width > 0 && restoreBounds.Height > 0)
                {
                    winSettings.Left = restoreBounds.Left;
                    winSettings.Top = restoreBounds.Top;
                    winSettings.Width = restoreBounds.Width;
                    winSettings.Height = restoreBounds.Height;
                }
                winSettings.WindowState = WindowState.Maximized;
            }
            else if (window.WindowState == WindowState.Minimized)
            {
                Rect restoreBounds = window.RestoreBounds;
                if (!restoreBounds.IsEmpty &&
                    !double.IsNaN(restoreBounds.Left) && !double.IsNaN(restoreBounds.Top) &&
                    restoreBounds.Width > 0 && restoreBounds.Height > 0)
                {
                    winSettings.Left = restoreBounds.Left;
                    winSettings.Top = restoreBounds.Top;
                    winSettings.Width = restoreBounds.Width;
                    winSettings.Height = restoreBounds.Height;
                }
                // Do not persist Minimized state; keep Normal or previous state
                if (winSettings.WindowState == WindowState.Minimized)
                {
                    winSettings.WindowState = WindowState.Normal;
                }
            }

            SaveSettings(settings);
        }
    }
}
