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
using WinYouTubeMusic.Native;
using WinYouTubeMusic.Services;

namespace WinYouTubeMusic
{
    public partial class MainWindow : Window
    {
        private IntPtr _hwnd;
        private SmtcService? _smtcService;
        private bool _isPlaying = false;
        private static bool _enableLog = false;

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly object _artworkLock = new object();
        private System.Drawing.Bitmap? _currentArtworkBitmap;
        private string _currentArtworkUrl = "";

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbmp, uint dwSITFlags);

        [DllImport("dwmapi.dll")]
        private static extern int DwmInvalidateIconicBitmaps(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        private const int DWMWA_FORCE_ICONIC_REPRESENTATION = 7;
        private const int DWMWA_HAS_ICONIC_BITMAP = 10;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;

        public MainWindow()
        {
            InitializeComponent();

            // Check command line args to enable log (default is OFF)
            ParseCommandLineArgs();

            InitTaskbarThumbIcons();

            this.Loaded += MainWindow_Loaded;
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

            // Enable DWM Custom Iconic Thumbnail (Album Artwork on Taskbar Hover)
            int forceIconic = 1;
            int hasIconicBitmap = 1;
            DwmSetWindowAttribute(_hwnd, DWMWA_FORCE_ICONIC_REPRESENTATION, ref forceIconic, sizeof(int));
            DwmSetWindowAttribute(_hwnd, DWMWA_HAS_ICONIC_BITMAP, ref hasIconicBitmap, sizeof(int));

            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            source?.AddHook(WndProc);

            // Initialize SMTC
            try
            {
                _smtcService = new SmtcService(_hwnd, SendCommandToWeb);
            }
            catch (Exception ex)
            {
                LogWebView("SMTC init error: " + ex.Message);
            }

            // Initialize WebView2 & Navigate
            _ = InitializeWebViewAsync();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DWMSENDICONICTHUMBNAIL)
            {
                int maxWidth = (int)(((long)lParam >> 16) & 0xFFFF);
                int maxHeight = (int)((long)lParam & 0xFFFF);

                IntPtr hBmp = CreateThumbnailHBitmap(maxWidth, maxHeight);
                if (hBmp != IntPtr.Zero)
                {
                    DwmSetIconicThumbnail(hwnd, hBmp, 0);
                    DeleteObject(hBmp);
                }
                handled = true;
                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        private IntPtr CreateThumbnailHBitmap(int maxWidth, int maxHeight)
        {
            try
            {
                if (maxWidth <= 0 || maxHeight <= 0) return IntPtr.Zero;

                System.Drawing.Bitmap? source = null;
                lock (_artworkLock)
                {
                    if (_currentArtworkBitmap != null)
                    {
                        source = (System.Drawing.Bitmap)_currentArtworkBitmap.Clone();
                    }
                }

                source ??= GetDefaultCoverBitmap();

                using (source)
                {
                    double scale = Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height);
                    int destWidth = Math.Max(1, (int)(source.Width * scale));
                    int destHeight = Math.Max(1, (int)(source.Height * scale));

                    using var targetBitmap = new System.Drawing.Bitmap(destWidth, destHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (var g = System.Drawing.Graphics.FromImage(targetBitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.Clear(System.Drawing.Color.FromArgb(15, 15, 15));
                        g.DrawImage(source, 0, 0, destWidth, destHeight);
                    }

                    return targetBitmap.GetHbitmap();
                }
            }
            catch (Exception ex)
            {
                LogWebView("CreateThumbnailHBitmap error: " + ex.Message);
                return IntPtr.Zero;
            }
        }

        private System.Drawing.Bitmap GetDefaultCoverBitmap()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ytmusic.ico");
                if (File.Exists(iconPath))
                {
                    using var icon = new System.Drawing.Icon(iconPath, 256, 256);
                    return icon.ToBitmap();
                }
            }
            catch { }

            var bmp = new System.Drawing.Bitmap(200, 200, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.FromArgb(15, 15, 15));
            }
            return bmp;
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

        public void BringWindowToFront()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    if (_hwnd != IntPtr.Zero)
                    {
                        ShowWindow(_hwnd, SW_RESTORE);
                        SetForegroundWindow(_hwnd);
                    }
                    Activate();
                    Focus();
                }
                catch { }
            });
        }

        public void NavigateToUrl(string url)
        {
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                        (uri.Host.Contains("youtube.com") || uri.Host.Contains("youtu.be")))
                    {
                        LogWebView("Navigating to URL via JumpList/IPC: " + url);
                        if (YtmWebView.CoreWebView2 != null)
                        {
                            YtmWebView.CoreWebView2.Navigate(url);
                        }
                        else
                        {
                            YtmWebView.Source = uri;
                        }

                        BringWindowToFront();
                    }
                }
                catch (Exception ex)
                {
                    LogWebView("NavigateToUrl error: " + ex.Message);
                }
            });
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinYouTubeMusic", "WebViewData");
                Directory.CreateDirectory(userFolder);

                var options = new CoreWebView2EnvironmentOptions();
                options.AdditionalBrowserArguments = "--disable-gpu --disable-gpu-compositing --disable-features=HardwareMediaKeyHandling --autoplay-policy=no-user-gesture-required";

                CoreWebView2Environment? env = null;
                int retries = 3;

                while (retries > 0)
                {
                    try
                    {
                        LogWebView("Creating CoreWebView2Environment at: " + userFolder);
                        env = await CoreWebView2Environment.CreateAsync(userDataFolder: userFolder, options: options);
                        await YtmWebView.EnsureCoreWebView2Async(env);
                        break;
                    }
                    catch (COMException comEx) when ((uint)comEx.HResult == 0x800700AA)
                    {
                        retries--;
                        LogWebView($"Primary WebViewData folder locked (0x800700AA). Retrying... ({retries} left)");
                        if (retries > 0)
                        {
                            await Task.Delay(500);
                        }
                        else
                        {
                            string persistentFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinYouTubeMusic", "ProfileData");
                            Directory.CreateDirectory(persistentFolder);
                            LogWebView("Switching to persistent profile: " + persistentFolder);
                            env = await CoreWebView2Environment.CreateAsync(userDataFolder: persistentFolder, options: options);
                            await YtmWebView.EnsureCoreWebView2Async(env);
                        }
                    }
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

                string initialUrl = App.InitialUrl ?? "https://music.youtube.com/";
                LogWebView($"Setting YtmWebView.Source = {initialUrl} ...");
                YtmWebView.Source = new Uri(initialUrl);
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

                    string url = "";
                    if (root.TryGetProperty("url", out var urlEl))
                    {
                        url = urlEl.GetString() ?? "";
                    }
                    if (string.IsNullOrWhiteSpace(url) && YtmWebView.Source != null)
                    {
                        url = YtmWebView.Source.ToString();
                    }

                    _isPlaying = isPlaying;

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        Title = $"{title} - {artist} | YouTube Music";
                    }
                    else
                    {
                        Title = "YouTube Music";
                    }

                    if (_smtcService != null)
                    {
                        await _smtcService.UpdateMetadataAsync(title, artist, artwork, isPlaying);
                    }

                    UpdateTaskbarPlayIcon(isPlaying);

                    // Add to JumpList recent tracks
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url))
                    {
                        JumpListService.AddTrack(title, artist, url);
                    }

                    // Add to JumpList playlists if playlist info is present
                    if (root.TryGetProperty("playlistTitle", out var pTitleEl) &&
                        root.TryGetProperty("playlistUrl", out var pUrlEl))
                    {
                        string pTitle = pTitleEl.GetString() ?? "";
                        string pUrl = pUrlEl.GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(pTitle) && !string.IsNullOrWhiteSpace(pUrl))
                        {
                            JumpListService.AddPlaylist(pTitle, "播放清單", pUrl);
                        }
                    }

                    // Update DWM Taskbar iconic thumbnail with the downloaded artwork
                    if (!string.IsNullOrWhiteSpace(artwork) && artwork != _currentArtworkUrl)
                    {
                        _currentArtworkUrl = artwork;
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                byte[] bytes = await _httpClient.GetByteArrayAsync(artwork);
                                using var ms = new MemoryStream(bytes);
                                var bmp = new System.Drawing.Bitmap(ms);
                                lock (_artworkLock)
                                {
                                    _currentArtworkBitmap?.Dispose();
                                    _currentArtworkBitmap = bmp;
                                }
                                Dispatcher.Invoke(() =>
                                {
                                    if (_hwnd != IntPtr.Zero)
                                    {
                                        DwmInvalidateIconicBitmaps(_hwnd);
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                LogWebView("Download artwork error: " + ex.Message);
                            }
                        });
                    }
                }
                else if (root.TryGetProperty("type", out var typeDisc) && typeDisc.GetString() == "playlistDiscovered")
                {
                    string pTitle = root.GetProperty("playlistTitle").GetString() ?? "";
                    string pUrl = root.GetProperty("playlistUrl").GetString() ?? "";
                    if (!string.IsNullOrWhiteSpace(pTitle) && !string.IsNullOrWhiteSpace(pUrl))
                    {
                        JumpListService.AddPlaylist(pTitle, "播放清單", pUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWebView("WebMessage parse error: " + ex.Message);
            }
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

    function getTrackAndPlaylistInfo() {
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
        if (!artwork) { const el = document.querySelector('.image.style-scope.ytmusic-player-bar') || document.querySelector('ytmusic-player-bar img'); artwork = el ? el.src : ''; }
        const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
        if (btn) { const l = btn.getAttribute('title') || btn.getAttribute('aria-label') || ''; isPlaying = l.toLowerCase().includes('pause') || l.includes('暫停'); }
        else { const v = document.querySelector('video'); if (v) isPlaying = !v.paused; }

        let videoId = '';
        let playlistId = '';
        let playlistTitle = '';

        try {
            const player = document.getElementById('movie_player') || (document.querySelector('ytmusic-player') && document.querySelector('ytmusic-player').player_);
            if (player) {
                if (typeof player.getVideoData === 'function') {
                    const data = player.getVideoData();
                    if (data) {
                        videoId = data.video_id || '';
                        if (data.list) playlistId = data.list;
                    }
                }
                if (!playlistId && typeof player.getPlaylistId === 'function') {
                    playlistId = player.getPlaylistId() || '';
                }
            }
        } catch (e) {}

        if (!playlistId && window.location.search) {
            const match = window.location.search.match(/[?&]list=([^&]+)/);
            if (match) playlistId = match[1];
        }

        try {
            const queueHeader = document.querySelector('ytmusic-player-page #header .title') ||
                                document.querySelector('ytmusic-queue-header-renderer .title') ||
                                document.querySelector('.queue-title');
            if (queueHeader && queueHeader.innerText) {
                playlistTitle = queueHeader.innerText.trim();
            }
        } catch (e) {}

        if (!playlistTitle && album) {
            playlistTitle = album.trim();
        }

        if (!playlistTitle) {
            try {
                const albumLink = document.querySelector('ytmusic-player-bar .subtitle a[href*=""list=""]') ||
                                  document.querySelector('ytmusic-player-bar .subtitle a[href*=""browse/""]') ||
                                  document.querySelector('ytmusic-player-bar .byline a[href*=""browse/""]');
                if (albumLink && albumLink.innerText) {
                    playlistTitle = albumLink.innerText.trim();
                }
            } catch (e) {}
        }

        let trackUrl = '';
        if (videoId) {
            trackUrl = 'https://music.youtube.com/watch?v=' + videoId + (playlistId ? '&list=' + playlistId : '');
        } else if (window.location.href && window.location.href.includes('watch?v=')) {
            trackUrl = window.location.href;
        }

        let playlistUrl = '';
        if (playlistId) {
            playlistUrl = 'https://music.youtube.com/watch?list=' + playlistId;
        }

        return {
            type: 'trackChange',
            title: title.trim(),
            artist: artist.trim(),
            album: album.trim(),
            artwork: artwork,
            isPlaying: isPlaying,
            url: trackUrl,
            playlistId: playlistId,
            playlistTitle: playlistTitle,
            playlistUrl: playlistUrl
        };
    }

    function checkPlaylistPage() {
        try {
            if (window.location.pathname.includes('/playlist') && window.location.search.includes('list=')) {
                const listMatch = window.location.search.match(/[?&]list=([^&]+)/);
                const pId = listMatch ? listMatch[1] : '';
                const titleEl = document.querySelector('ytmusic-responsive-header-renderer .title') ||
                                document.querySelector('ytmusic-header-renderer .title') ||
                                document.querySelector('h1.title') ||
                                document.querySelector('.title.ytmusic-detail-header-renderer');
                const pTitle = titleEl ? titleEl.innerText.trim() : '';
                if (pId && pTitle && window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        type: 'playlistDiscovered',
                        playlistId: pId,
                        playlistTitle: pTitle,
                        playlistUrl: 'https://music.youtube.com/watch?list=' + pId
                    });
                }
            }
        } catch (e) {}
    }

    let lastData = '';
    setInterval(function() {
        const info = getTrackAndPlaylistInfo();
        const str = JSON.stringify(info);
        if (str !== lastData) {
            lastData = str;
            if (window.chrome && window.chrome.webview) { window.chrome.webview.postMessage(info); }
        }
        checkPlaylistPage();
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