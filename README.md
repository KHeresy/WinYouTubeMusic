# YouTube Music for Windows 11 (WinYouTubeMusic)

專為 Windows 11 / 10 設計的 **YouTube Music** 原生桌面應用程式，結合高效能 .NET 10 與 WPF WebView2 技術，具備深度 Windows 系統媒體控制卡片（SMTC）、工作列右鍵跳躍清單（Jump List）與 DWM 原生縮圖整合。

---

## 🌟 核心特色 (Features)

1. **工作列右鍵跳躍清單與曲目/清單釘選 (Taskbar Jump List & Pinning)**
   - **雙分類支援**：
     - 📋 **「播放清單」**：自動記錄最近播放或瀏覽的播放清單、專輯、歌手電台。
     - 🎵 **「最近播放」**：自動記錄最近播放的單曲歷史（保留播放隊列參數）。
   - **Windows 原生釘選 (Pinning)**：
     - 懸停於右鍵選單項目點擊圖釘圖示，即可將喜愛的清單或曲目固定在最上方的「**已釘選**」分類，永不被新曲目洗掉。
   - **單一實例 IPC 無縫播放**：
     - 點擊 Jump List 項目時，若程式已在執行，會透過 Named Pipe 直接通知主視窗跳轉播放並喚醒置前，不會開啟重複視窗。

2. **工作列懸停專輯封面縮圖 (DWM Iconic Thumbnail)**
   - 使用 Windows 桌面視窗管理員（DWM）原生的 `DwmSetIconicThumbnail` 技術。
   - 滑鼠游標懸停於工作列圖示時，直接以高畫質呈現當前播放曲目的專輯封面大圖，取代傳統模糊的視窗縮圖。
   - **工作列多媒體控制按鈕**：縮圖下方具備 `⏮ 上一首`、`▶/⏸ 播放/暫停`、`⏭ 下一首` 向量控制按鈕。

3. **Windows 系統媒體傳輸控制 (System Media Transport Controls - SMTC)**
   - 完整註冊 **AppUserModelID (AUMID)**，在 Windows 快捷控制中心（`Win + A`）與音量彈出視窗中正確顯示應用程式名稱「**YouTube Music**」與專屬圖示，不再顯示「未知的應用程式」。
   - 🎵 即時顯示**歌曲名稱 (Title)**、🎤 **歌手名稱 (Artist)**、🖼️ **高畫質封面 (Album Art)**。
   - ⏯️ 完整支援鍵盤硬體多媒體鍵（Play, Pause, Next, Previous）控制。

4. **單一實例管理 (Single Instance Management)**
   - 使用系統級 `Mutex` 與 `NamedPipeServerStream` / `NamedPipeClientStream`。
   - 解決 Chromium WebView2 設定檔鎖定問題，確保 Google 帳號登入狀態與 Cookie 持久穩定。

5. **現代化 Windows 11 視覺介面**
   - **Immersive Dark Mode**：Windows 11 原生深色標題列與介面風格。
   - **自動播放最佳化**：內建 `--autoplay-policy=no-user-gesture-required`，點選清單或曲目即刻自動播放。

---

## 💻 系統需求 (System Requirements)

- **作業系統**：Windows 11 / Windows 10 (x64 / ARM64)
- **執行階段**：.NET 10.0 Windows Desktop Runtime
- **瀏覽器元件**：Microsoft Edge WebView2 Runtime（Windows 11 已預裝）

---

## 🚀 建置與執行 (Build & Run)

在專案根目錄開啟 PowerShell 執行：

```powershell
# 建置專案
dotnet build

# 執行專案
dotnet run
```

---

## 📄 授權說明 (License)

MIT License.
