# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-08-30

### Added
- **Window Size, Position & State Persistence**:
  - Automatically remembers window coordinates (`Left`, `Top`), dimensions (`Width`, `Height`), and maximized state across application restarts.
  - Multi-monitor safety protection: automatically repositions and centers window on the primary display if disconnected external monitors cause the window to be off-screen.
  - Gracefully restores from maximized state and avoids starting minimized.

### Fixed
- **Secondary Instance Jump List Flash Fix**:
  - Removed implicit WPF `StartupUri` and made secondary instance IPC forwarding completely synchronous before process exit.
  - Secondary instances launched from Jump List / Pinned items now exit in milliseconds without creating any window or initializing UI.

## [0.1.0] - 2026-08-24

### Added
- **Native Taskbar Jump List & Pinning**:
  - Dual categories: **Playlists** (recent playlists, albums, and radios) and **Recent Tracks**.
  - Windows native item pinning support to keep favorite playlists and tracks permanently at the top.
  - Seamless single-instance IPC: Clicking any Jump List item navigates the running instance and starts playback immediately.
- **DWM Iconic Thumbnail & Taskbar Toolbar**:
  - Real-time high-resolution album artwork thumbnail preview on taskbar hover via `DwmSetIconicThumbnail`.
  - Taskbar thumbnail toolbar with high-DPI vector icons for `Previous`, `Play / Pause` (dynamic state), and `Next`.
- **Windows System Media Transport Controls (SMTC)**:
  - System-level media flyout and Quick Settings (`Win + A`) integration.
  - Full `AppUserModelID (AUMID)` registration (`KHeresy.WinYouTubeMusic`) with Start Menu shortcut and registry association to cleanly display "YouTube Music" and official app icon.
  - Real-time track metadata synchronization (Title, Artist, Album Artwork).
  - Full hardware keyboard media keys support (Play/Pause, Next, Previous).
- **Single-Instance Application Management**:
  - System `Mutex` and asynchronous `NamedPipe` communication preventing profile locking issues (`0x800700AA`) and multi-process conflicts.
  - Ensures persistent Google/YouTube Music login cookies and sessions.
- **Modern Windows 11 Design & Playback Optimization**:
  - Immersive dark mode window frame and styling.
  - `--autoplay-policy=no-user-gesture-required` for instant playback on startup and navigation.
- **CI/CD Automation**:
  - GitHub Actions workflow for automated building, packaging, and releasing on tag pushes (`v*`).
