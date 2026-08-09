using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace WinYotuTubeMusic.Services;

public class SmtcService
{
    private readonly SystemMediaTransportControls _smtc;
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly Action<string> _sendCommand;

    public SmtcService(IntPtr hwnd, Action<string> sendCommand)
    {
        _sendCommand = sendCommand;

        // Get SMTC for window
        _smtc = SystemMediaTransportControlsInterop.GetForWindow(hwnd);
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsNextEnabled = true;
        _smtc.IsPreviousEnabled = true;
        _smtc.IsEnabled = true;

        _smtc.ButtonPressed += Smtc_ButtonPressed;
    }

    private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        switch (args.Button)
        {
            case SystemMediaTransportControlsButton.Play:
            case SystemMediaTransportControlsButton.Pause:
                _sendCommand("playPause");
                break;
            case SystemMediaTransportControlsButton.Next:
                _sendCommand("next");
                break;
            case SystemMediaTransportControlsButton.Previous:
                _sendCommand("previous");
                break;
        }
    }

    public async Task UpdateMetadataAsync(string title, string artist, string artworkUrl, bool isPlaying)
    {
        _smtc.PlaybackStatus = isPlaying ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;

        var updater = _smtc.DisplayUpdater;
        updater.Type = MediaPlaybackType.Music;
        updater.MusicProperties.Title = title;
        updater.MusicProperties.Artist = artist;

        if (!string.IsNullOrEmpty(artworkUrl))
        {
            try
            {
                byte[] bytes = await _httpClient.GetByteArrayAsync(artworkUrl);
                InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));
                writer.WriteBytes(bytes);
                await writer.StoreAsync();
                await writer.FlushAsync();

                updater.Thumbnail = RandomAccessStreamReference.CreateFromStream(stream);
            }
            catch
            {
                // Fallback if image download fails
            }
        }

        updater.Update();
    }
}
