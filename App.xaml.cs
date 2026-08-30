using System;
using System.Windows;
using WinYouTubeMusic.Native;
using WinYouTubeMusic.Services;

namespace WinYouTubeMusic
{
    public partial class App : Application
    {
        public static string? InitialUrl { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Parse URL from command line if any
            string? url = ParseUrlFromArgs(Environment.GetCommandLineArgs()) ?? ParseUrlFromArgs(e.Args);

            if (!SingleInstance.TryAcquire(out bool isPrimary))
            {
                // Another instance is already running. Forward URL synchronously and exit immediately without creating any UI.
                SingleInstance.SendToPrimaryInstance(url ?? "ACTIVATE");
                Shutdown(0);
                return;
            }

            InitialUrl = url;

            // 1. Initialize App Identity (AUMID, Registry, Start Menu shortcut)
            AppIdentityHelper.Initialize();

            // 2. Initialize JumpList
            JumpListService.Initialize();

            // 3. Start IPC Server for incoming secondary instances (e.g. from JumpList clicks)
            SingleInstance.StartIpcServer(receivedUrl =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (MainWindow is MainWindow mainWindow)
                    {
                        if (receivedUrl != "ACTIVATE" && !string.IsNullOrWhiteSpace(receivedUrl))
                        {
                            mainWindow.NavigateToUrl(receivedUrl);
                        }
                        else
                        {
                            mainWindow.BringWindowToFront();
                        }
                    }
                });
            });

            base.OnStartup(e);

            // 4. Create and show MainWindow manually for primary instance
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SingleInstance.Release();
            base.OnExit(e);
        }

        private static string? ParseUrlFromArgs(string[] args)
        {
            if (args == null) return null;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if ((arg.Equals("--url", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-url", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("--play-url", StringComparison.OrdinalIgnoreCase)) &&
                    i + 1 < args.Length)
                {
                    return args[i + 1].Trim('\"', '\'');
                }

                if (arg.StartsWith("--url=", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("-url=", StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Substring(arg.IndexOf('=') + 1).Trim('\"', '\'');
                }

                if (arg.StartsWith("https://music.youtube.com", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("https://www.youtube.com", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("https://youtu.be", StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Trim('\"', '\'');
                }
            }
            return null;
        }
    }
}
