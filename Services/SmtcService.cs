using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.Streams;

namespace WinYouTubeMusic.Services;

public class SmtcService
{
    private readonly SystemMediaTransportControls _smtc;
    private readonly Action<string> _commandHandler;
    private readonly HttpClient _httpClient;

    public SmtcService(IntPtr hwnd, Action<string> commandHandler)
    {
        _commandHandler = commandHandler;
        _httpClient = new HttpClient();

        // Get SMTC instance bound to window handle
        _smtc = SystemMediaTransportControlsInterop.GetForWindow(hwnd);
        _smtc.IsEnabled = true;
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsNextEnabled = true;
        _smtc.IsPreviousEnabled = true;

        _smtc.ButtonPressed += Smtc_ButtonPressed;
    }

    private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        switch (args.Button)
        {
            case SystemMediaTransportControlsButton.Play:
            case SystemMediaTransportControlsButton.Pause:
                _commandHandler("playPause");
                break;
            case SystemMediaTransportControlsButton.Next:
                _commandHandler("next");
                break;
            case SystemMediaTransportControlsButton.Previous:
                _commandHandler("previous");
                break;
        }
    }

    public async Task UpdateMetadataAsync(string title, string artist, string artworkUrl, bool isPlaying)
    {
        try
        {
            _smtc.PlaybackStatus = isPlaying ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;

            var updater = _smtc.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Title = title;
            updater.MusicProperties.Artist = artist;

            if (!string.IsNullOrWhiteSpace(artworkUrl))
            {
                try
                {
                    byte[] imageBytes = await _httpClient.GetByteArrayAsync(artworkUrl);
                    var stream = new InMemoryRandomAccessStream();
                    using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(imageBytes);
                        await writer.StoreAsync();
                    }
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromStream(stream);
                }
                catch { }
            }

            updater.Update();
        }
        catch { }
    }
}
