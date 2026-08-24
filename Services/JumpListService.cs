using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Shell;

namespace WinYouTubeMusic.Services;

public class RecentTrackItem
{
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string Url { get; set; } = "";
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}

public class RecentPlaylistItem
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "播放清單";
    public string Url { get; set; } = "";
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}

public class RecentStorageData
{
    public List<RecentTrackItem> Tracks { get; set; } = new List<RecentTrackItem>();
    public List<RecentPlaylistItem> Playlists { get; set; } = new List<RecentPlaylistItem>();
}

public static class JumpListService
{
    private static readonly object _lock = new object();
    private static readonly List<RecentTrackItem> _recentTracks = new List<RecentTrackItem>();
    private static readonly List<RecentPlaylistItem> _recentPlaylists = new List<RecentPlaylistItem>();
    private const int MaxTracks = 10;
    private const int MaxPlaylists = 5;

    private static string StorageFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinYouTubeMusic",
        "recent_tracks.json");

    public static void Initialize()
    {
        lock (_lock)
        {
            LoadFromDisk();
        }
        ApplyJumpList();
    }

    private static bool IsPlayableUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return (url.Contains("watch?v=", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("watch?list=", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("playlist?list=", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("youtu.be/", StringComparison.OrdinalIgnoreCase));
    }

    public static void AddTrack(string title, string artist, string url)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url)) return;
        if (!IsPlayableUrl(url)) return;

        bool changed = false;
        lock (_lock)
        {
            var existing = _recentTracks.FirstOrDefault(t =>
                string.Equals(t.Url, url, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(t.Title, title, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(t.Artist, artist, StringComparison.OrdinalIgnoreCase)));

            if (existing != null)
            {
                _recentTracks.Remove(existing);
                existing.Title = title;
                existing.Artist = artist;
                existing.Url = url;
                existing.PlayedAt = DateTime.UtcNow;
                _recentTracks.Insert(0, existing);
                changed = true;
            }
            else
            {
                _recentTracks.Insert(0, new RecentTrackItem
                {
                    Title = title,
                    Artist = artist,
                    Url = url,
                    PlayedAt = DateTime.UtcNow
                });
                changed = true;
            }

            while (_recentTracks.Count > MaxTracks)
            {
                _recentTracks.RemoveAt(_recentTracks.Count - 1);
            }

            if (changed)
            {
                SaveToDisk();
            }
        }

        if (changed)
        {
            ApplyJumpList();
        }
    }

    public static void AddPlaylist(string title, string subtitle, string url)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url)) return;
        if (!IsPlayableUrl(url)) return;

        if (title.Equals("YouTube Music", StringComparison.OrdinalIgnoreCase) ||
            title.Equals("YouTube", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool changed = false;
        lock (_lock)
        {
            var existing = _recentPlaylists.FirstOrDefault(p =>
                string.Equals(p.Url, url, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                _recentPlaylists.Remove(existing);
                existing.Title = title;
                existing.Subtitle = subtitle;
                existing.Url = url;
                existing.PlayedAt = DateTime.UtcNow;
                _recentPlaylists.Insert(0, existing);
                changed = true;
            }
            else
            {
                _recentPlaylists.Insert(0, new RecentPlaylistItem
                {
                    Title = title,
                    Subtitle = subtitle,
                    Url = url,
                    PlayedAt = DateTime.UtcNow
                });
                changed = true;
            }

            while (_recentPlaylists.Count > MaxPlaylists)
            {
                _recentPlaylists.RemoveAt(_recentPlaylists.Count - 1);
            }

            if (changed)
            {
                SaveToDisk();
            }
        }

        if (changed)
        {
            ApplyJumpList();
        }
    }

    private static void LoadFromDisk()
    {
        try
        {
            string path = StorageFilePath;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                // Try deserializing new composite format first
                try
                {
                    var data = JsonSerializer.Deserialize<RecentStorageData>(json);
                    if (data != null && (data.Tracks.Count > 0 || data.Playlists.Count > 0))
                    {
                        _recentTracks.Clear();
                        _recentTracks.AddRange(data.Tracks.Where(t => IsPlayableUrl(t.Url)).Take(MaxTracks));
                        _recentPlaylists.Clear();
                        _recentPlaylists.AddRange(data.Playlists.Where(p => IsPlayableUrl(p.Url)).Take(MaxPlaylists));
                        return;
                    }
                }
                catch { }

                // Fallback to legacy track list format
                var items = JsonSerializer.Deserialize<List<RecentTrackItem>>(json);
                if (items != null)
                {
                    _recentTracks.Clear();
                    _recentTracks.AddRange(items.Where(t => IsPlayableUrl(t.Url)).Take(MaxTracks));
                }
            }
        }
        catch { }
    }

    private static void SaveToDisk()
    {
        try
        {
            string path = StorageFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var data = new RecentStorageData
            {
                Tracks = _recentTracks,
                Playlists = _recentPlaylists
            };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { }
    }

    private static void ApplyJumpList()
    {
        if (Application.Current == null) return;

        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                string exePath = Environment.ProcessPath ?? "";
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return;

                var jumpList = new JumpList
                {
                    ShowFrequentCategory = false,
                    ShowRecentCategory = false
                };

                List<RecentPlaylistItem> playlistsCopy;
                List<RecentTrackItem> tracksCopy;
                lock (_lock)
                {
                    playlistsCopy = new List<RecentPlaylistItem>(_recentPlaylists);
                    tracksCopy = new List<RecentTrackItem>(_recentTracks);
                }

                // 1. Playlists Category (shows at top before Recent Tracks)
                foreach (var playlist in playlistsCopy)
                {
                    string displayTitle = playlist.Title;
                    if (displayTitle.Length > 60)
                    {
                        displayTitle = displayTitle.Substring(0, 57) + "...";
                    }

                    var task = new JumpTask
                    {
                        Title = displayTitle,
                        Description = $"播放清單：{playlist.Title}",
                        ApplicationPath = exePath,
                        Arguments = $"--url \"{playlist.Url}\"",
                        CustomCategory = "播放清單",
                        IconResourcePath = exePath,
                        IconResourceIndex = 0
                    };

                    jumpList.JumpItems.Add(task);
                }

                // 2. Recent Tracks Category
                foreach (var track in tracksCopy)
                {
                    string displayTitle = !string.IsNullOrWhiteSpace(track.Artist)
                        ? $"{track.Title} - {track.Artist}"
                        : track.Title;

                    if (displayTitle.Length > 60)
                    {
                        displayTitle = displayTitle.Substring(0, 57) + "...";
                    }

                    var task = new JumpTask
                    {
                        Title = displayTitle,
                        Description = $"播放 {track.Title}",
                        ApplicationPath = exePath,
                        Arguments = $"--url \"{track.Url}\"",
                        CustomCategory = "最近播放",
                        IconResourcePath = exePath,
                        IconResourceIndex = 0
                    };

                    jumpList.JumpItems.Add(task);
                }

                JumpList.SetJumpList(Application.Current, jumpList);
                jumpList.Apply();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JumpListService] Apply error: {ex.Message}");
            }
        });
    }
}
