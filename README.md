# YouTube Music for Windows (WinYouTubeMusic)

[English](README.md) | [繁體中文](README.zh-TW.md)

A lightweight, native **YouTube Music** desktop application designed for Windows 11 & 10. Built with **.NET 10** and **WPF WebView2**, it deeply integrates with the Windows ecosystem, featuring System Media Transport Controls (SMTC), Taskbar Jump Lists with pinning, and native DWM album art previews.

---

## 🌟 Key Features

1. **Taskbar Jump List & Pinning**
   - **Dual Categories**:
     - 📋 **Playlists**: Automatically tracks recently played or browsed playlists, albums, and artist radios.
     - 🎵 **Recent Tracks**: Keeps a history of recently played tracks while preserving playlist queue context (`&list=...`).
   - **Native Windows Pinning**:
     - Hover over any item in the Jump List and click the pin icon to pin your favorite playlists or songs to the top **Pinned** section.
   - **Seamless Single-Instance Playback**:
     - Clicking a Jump List item forwards the URL via a Named Pipe IPC to the active window and brings it to the front without opening duplicate processes.

2. **DWM Iconic Thumbnail (Album Art Hover Preview)**
   - Utilizes the native Desktop Window Manager (`DwmSetIconicThumbnail`) API.
   - Hovering over the taskbar icon displays a crisp, full-sized album artwork preview instead of a scaled-down web page preview.
   - **Taskbar Thumbnail Toolbar**: Integrated media control buttons below the thumbnail:
     - `⏮ Previous Track`
     - `▶ / ⏸ Play / Pause` (dynamically updates based on playback state)
     - `⏭ Next Track`

3. **Windows 11 System Media Transport Controls (SMTC)**
   - Full **AppUserModelID (AUMID)** registration ensures the Windows Quick Settings media panel (`Win + A`) and volume flyouts cleanly identify the app as **"YouTube Music"** with its official icon (no more "Unknown application").
   - 🎵 Real-time **Track Title** & 🎤 **Artist Name** display.
   - 🖼️ High-resolution **Album Artwork** downloading and rendering.
   - ⏯️ Full hardware media keys support (Play, Pause, Next, Previous).

4. **Single-Instance Management & Persistent Session**
   - Uses system-level `Mutex` and asynchronous `NamedPipe` IPC to ensure only one instance handles WebView2 user profile data.
   - Prevents profile lock errors (`0x800700AA`) and keeps Google / YouTube Music login sessions and cookies persistent and secure.

5. **Modern Windows 11 UI & State Persistence**
   - **Window Size & Position Memory**: Automatically persists window coordinates, dimensions, and maximized state across sessions with multi-monitor disconnection protection.
   - **Immersive Dark Mode**: Windows 11 native dark window frame and title bar styling.
   - **Autoplay Optimization**: Pre-configured with `--autoplay-policy=no-user-gesture-required` for instant playback when launching tracks or playlists from shortcuts.

---

## 💻 System Requirements

- **Operating System**: Windows 11 / Windows 10 (x64 / ARM64)
- **Runtime**: .NET 10.0 Windows Desktop Runtime
- **Web Runtime**: Microsoft Edge WebView2 Runtime (pre-installed on Windows 11)

---

## 🚀 Build & Run

Open PowerShell in the project root directory:

```powershell
# Build project
dotnet build

# Run application
dotnet run

# Publish Release build
dotnet publish -c Release -r win-x64 --no-self-contained -o ./publish
```

---

## 📄 License

MIT License.
