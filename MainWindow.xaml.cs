using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using WinYotuTubeMusic.Native;
using WinYotuTubeMusic.Services;

namespace WinYotuTubeMusic
{
    public partial class MainWindow : Window
    {
        private IntPtr _hwnd;
        private SmtcService? _smtcService;
        private bool _isPlaying = false;
        private bool _isAlwaysOnTop = false;
        private static bool _enableLog = false;
        private readonly HttpClient _httpClient = new HttpClient();

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public MainWindow()
        {
            InitializeComponent();

            // Check command line args to enable log (default is OFF)
            ParseCommandLineArgs();

            InitTaskbarThumbIcons();

            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += (s, e) => UpdateTaskbarThumbnailClip();
        }

        private void ParseCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg.Equals("--enable-log", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-log", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--log", StringComparison.OrdinalIgnoreCase))
                {
                    _enableLog = true;
                    break;
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;

            // Apply Windows 11 Dark Mode Frame
            int useDarkMode = 1;
            DwmSetWindowAttribute(_hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));

            // Initialize SMTC
            try
            {
                _smtcService = new SmtcService(_hwnd, SendCommandToWeb);
            }
            catch (Exception ex)
            {
                LogWebView("SMTC init error: " + ex.Message);
            }

            UpdateTaskbarThumbnailClip();

            // Initialize WebView2 & Navigate
            _ = InitializeWebViewAsync();
        }

        private void InitTaskbarThumbIcons()
        {
            try
            {
                BtnPrevTaskbar.ImageSource = WpfIconHelper.CreatePreviousIcon();
                BtnPlayTaskbar.ImageSource = WpfIconHelper.CreatePlayIcon(false);
                BtnNextTaskbar.ImageSource = WpfIconHelper.CreateNextIcon();
                LogWebView("InitTaskbarThumbIcons completed.");
            }
            catch (Exception ex)
            {
                LogWebView("InitTaskbarThumbIcons error: " + ex.Message);
            }
        }

        private void UpdateTaskbarPlayIcon(bool isPlaying)
        {
            try
            {
                BtnPlayTaskbar.ImageSource = WpfIconHelper.CreatePlayIcon(isPlaying);
                BtnPlayTaskbar.Description = isPlaying ? "暫停 (Pause)" : "播放 (Play)";
            }
            catch (Exception ex)
            {
                LogWebView("UpdateTaskbarPlayIcon error: " + ex.Message);
            }
        }

        private void UpdateTaskbarThumbnailClip()
        {
            try
            {
                if (AlbumCoverBorder.ActualWidth > 0 && AlbumCoverBorder.ActualHeight > 0)
                {
                    Point pos = AlbumCoverBorder.TransformToAncestor(this).Transform(new Point(0, 0));
                    TaskbarInfo.ThumbnailClipMargin = new Thickness(
                        pos.X,
                        pos.Y,
                        Math.Max(0, this.ActualWidth - (pos.X + AlbumCoverBorder.ActualWidth)),
                        Math.Max(0, this.ActualHeight - (pos.Y + AlbumCoverBorder.ActualHeight))
                    );
                }
            }
            catch (Exception ex)
            {
                LogWebView("UpdateTaskbarThumbnailClip error: " + ex.Message);
            }
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinYouTubeMusic", "WebViewData");
                Directory.CreateDirectory(userFolder);

                var options = new CoreWebView2EnvironmentOptions();
                options.AdditionalBrowserArguments = "--disable-gpu --disable-gpu-compositing";

                CoreWebView2Environment env;
                try
                {
                    LogWebView("Creating CoreWebView2Environment at: " + userFolder);
                    env = await CoreWebView2Environment.CreateAsync(userDataFolder: userFolder, options: options);
                    await YtmWebView.EnsureCoreWebView2Async(env);
                }
                catch (COMException comEx) when ((uint)comEx.HResult == 0x800700AA)
                {
                    LogWebView("Primary WebViewData folder locked (0x800700AA). Switching to fallback session profile...");
                    string fallbackFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinYouTubeMusic", $"Session_{Environment.ProcessId}");
                    Directory.CreateDirectory(fallbackFolder);
                    env = await CoreWebView2Environment.CreateAsync(userDataFolder: fallbackFolder, options: options);
                    await YtmWebView.EnsureCoreWebView2Async(env);
                }

                LogWebView("CoreWebView2 ensured successfully!");

                YtmWebView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(255, 15, 15, 15);
                YtmWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                YtmWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                YtmWebView.CoreWebView2.Settings.IsScriptEnabled = true;
                YtmWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                YtmWebView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36";

                YtmWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                YtmWebView.NavigationCompleted += YtmWebView_NavigationCompleted;
                YtmWebView.CoreWebView2.ProcessFailed += YtmWebView_ProcessFailed;

                string jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "youtube_music_inject.js");
                string scriptContent = File.Exists(jsPath) ? File.ReadAllText(jsPath) : GetEmbeddedScriptFallback();

                await YtmWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(scriptContent);

                LogWebView("Setting YtmWebView.Source = https://music.youtube.com/ ...");
                YtmWebView.Source = new Uri("https://music.youtube.com/");
            }
            catch (Exception ex)
            {
                LogWebView("InitializeWebViewAsync EXCEPTION: " + ex);
            }
        }

        private void YtmWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            LogWebView($"Navigation completed. IsSuccess={e.IsSuccess}, WebErrorStatus={e.WebErrorStatus}, Uri={YtmWebView.Source}");
            YtmWebView.Visibility = Visibility.Visible;
            UpdateTaskbarThumbnailClip();
        }

        private void YtmWebView_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            LogWebView($"WebView ProcessFailed: ProcessFailedKind={e.ProcessFailedKind}, Reason={e.Reason}");
        }

        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                string json = args.WebMessageAsJson;
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "trackChange")
                {
                    string title = root.GetProperty("title").GetString() ?? "";
                    string artist = root.GetProperty("artist").GetString() ?? "";
                    string artwork = root.GetProperty("artwork").GetString() ?? "";
                    bool isPlaying = root.GetProperty("isPlaying").GetBoolean();

                    _isPlaying = isPlaying;

                    TxtSongTitle.Text = !string.IsNullOrWhiteSpace(title) ? title : "YouTube Music";
                    TxtArtistName.Text = !string.IsNullOrWhiteSpace(artist) ? artist : "尚未播放曲目";

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        Title = $"{title} - {artist} | YouTube Music";
                    }
                    else
                    {
                        Title = "YouTube Music";
                    }

                    if (!string.IsNullOrWhiteSpace(artwork))
                    {
                        _ = LoadAlbumArtworkAsync(artwork);
                    }

                    if (_smtcService != null)
                    {
                        await _smtcService.UpdateMetadataAsync(title, artist, artwork, isPlaying);
                    }

                    UpdateTaskbarPlayIcon(isPlaying);
                    UpdateTaskbarThumbnailClip();
                }
            }
            catch (Exception ex)
            {
                LogWebView("WebMessage parse error: " + ex.Message);
            }
        }

        private async Task LoadAlbumArtworkAsync(string url)
        {
            try
            {
                byte[] data = await _httpClient.GetByteArrayAsync(url);
                using MemoryStream ms = new MemoryStream(data);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                Dispatcher.Invoke(() =>
                {
                    AlbumCoverImage.Source = bitmap;
                    PlaceholderIcon.Visibility = Visibility.Collapsed;
                    UpdateTaskbarThumbnailClip();
                });
            }
            catch { }
        }

        public void SendCommandToWeb(string cmd)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                if (YtmWebView.CoreWebView2 != null)
                {
                    await YtmWebView.ExecuteScriptAsync($"window.__ytmCommand && window.__ytmCommand('{cmd}');");
                }
            });
        }

        private void BtnPrevTaskbar_Click(object sender, EventArgs e)
        {
            LogWebView("BtnPrevTaskbar_Click triggered");
            SendCommandToWeb("previous");
        }

        private void BtnPlayTaskbar_Click(object sender, EventArgs e)
        {
            LogWebView("BtnPlayTaskbar_Click triggered");
            SendCommandToWeb("playPause");
        }

        private void BtnNextTaskbar_Click(object sender, EventArgs e)
        {
            LogWebView("BtnNextTaskbar_Click triggered");
            SendCommandToWeb("next");
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (YtmWebView.CoreWebView2 != null && YtmWebView.CanGoBack)
                {
                    YtmWebView.GoBack();
                }
            }
            catch (Exception ex)
            {
                LogWebView("BtnBack_Click error: " + ex.Message);
            }
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (YtmWebView.CoreWebView2 != null && YtmWebView.CanGoForward)
                {
                    YtmWebView.GoForward();
                }
            }
            catch (Exception ex)
            {
                LogWebView("BtnForward_Click error: " + ex.Message);
            }
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (YtmWebView.CoreWebView2 != null)
                {
                    YtmWebView.Reload();
                }
                else
                {
                    _ = InitializeWebViewAsync();
                }
            }
            catch (Exception ex)
            {
                LogWebView("BtnReload_Click error: " + ex.Message);
            }
        }

        private void BtnAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            _isAlwaysOnTop = !_isAlwaysOnTop;
            Topmost = _isAlwaysOnTop;
        }

        private void LogWebView(string msg)
        {
            if (!_enableLog) return;

            try
            {
                string logLine = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}\n";
                string defaultLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinYouTubeMusic", "app.log");
                Directory.CreateDirectory(Path.GetDirectoryName(defaultLogPath)!);
                File.AppendAllText(defaultLogPath, logLine);
            }
            catch { }
        }

        private string GetEmbeddedScriptFallback()
        {
            return @"
(function () {
    if (window.__ytmInjected) return;
    window.__ytmInjected = true;
    function getTrackInfo() {
        let title = ''; let artist = ''; let album = ''; let artwork = ''; let isPlaying = false;
        if (navigator.mediaSession && navigator.mediaSession.metadata) {
            title = navigator.mediaSession.metadata.title || '';
            artist = navigator.mediaSession.metadata.artist || '';
            album = navigator.mediaSession.metadata.album || '';
            if (navigator.mediaSession.metadata.artwork && navigator.mediaSession.metadata.artwork.length > 0) {
                artwork = navigator.mediaSession.metadata.artwork[navigator.mediaSession.metadata.artwork.length - 1].src || '';
            }
        }
        if (!title) { const el = document.querySelector('.title.style-scope.ytmusic-player-bar'); title = el ? el.innerText : ''; }
        if (!artist) { const el = document.querySelector('.byline.style-scope.ytmusic-player-bar'); artist = el ? el.innerText : ''; }
        if (!artwork) { const el = document.querySelector('.image.style-scope.ytmusic-player-bar'); artwork = el ? el.src : ''; }
        const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
        if (btn) { const l = btn.getAttribute('title') || btn.getAttribute('aria-label') || ''; isPlaying = l.toLowerCase().includes('pause') || l.includes('暫停'); }
        else { const v = document.querySelector('video'); if (v) isPlaying = !v.paused; }
        return { type: 'trackChange', title: title.trim(), artist: artist.trim(), album: album.trim(), artwork: artwork, isPlaying: isPlaying };
    }
    let lastData = '';
    setInterval(function() {
        const info = getTrackInfo();
        const str = JSON.stringify(info);
        if (str !== lastData) {
            lastData = str;
            if (window.chrome && window.chrome.webview) { window.chrome.webview.postMessage(info); }
        }
    }, 500);
    window.__ytmCommand = function (cmd) {
        if (cmd === 'playPause') {
            const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
            if (btn) btn.click();
            else { const v = document.querySelector('video'); if (v) { v.paused ? v.play() : v.pause(); } }
        } else if (cmd === 'next') {
            const btn = document.querySelector('.next-button'); if (btn) btn.click();
        } else if (cmd === 'previous') {
            const btn = document.querySelector('.previous-button'); if (btn) btn.click();
        }
    };
})();";
        }
    }
}