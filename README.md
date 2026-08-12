# YouTube Music for Windows 11 (WinYouTubeMusic)

專為 Windows 11 設計的 **YouTube Music** 原生桌面應用程式，結合高效能 .NET 10 與 WPF WebView2 技術，並具備深度 Windows 11 工作列多媒體控制與系統媒體卡片（SMTC）整合。

---

## 🌟 核心特色 (Features)

1. **工作列預覽縮圖多媒體控制按鈕（MPC-HC 風格）**
   - 將滑鼠游標懸停於工作列圖示時，視窗預覽縮圖下方會顯示高畫質向量控制按鈕：
     - `⏮` **上一首 (Previous)**
     - `▶ / ⏸` **播放 / 暫停 (Play/Pause)**（動態切換播放與暫停圖示）
     - `⏭` **下一首 (Next)**

2. **專輯封面預覽裁切 (Album Artwork Thumbnail Clip)**
   - 懸停工作列時，預覽視窗會自動精準裁切並呈現在頂部區域顯示的當前播放曲目高畫質專輯封面。

3. **Windows 11 系統媒體傳輸控制 (System Media Transport Controls - SMTC)**
   - 整合 Windows 11 工作列右下角控制中心與快捷彈出視窗（`Win + A` 或音量鍵）：
     - 🎵 即時顯示**歌曲名稱 (Title)**
     - 🎤 即時顯示**歌手名稱 (Artist)**
     - 🖼️ 即時下載並顯示**專輯封面 (Album Artwork)**
     - ⏯️ 支援硬體鍵盤多媒體按鈕（Play, Pause, Next, Previous）控音與切歌。

4. **現代化 Windows 11 視覺介面**
   - **Immersive Dark Mode**：Windows 11 原生深色標題列與介面風格。
   - **登入狀態持久化**：自動記憶 Google / YouTube Music 帳號登入與播放偏好設定。

---

## 💻 系統需求 (System Requirements)

- **作業系統**：Windows 11 / Windows 10 (x64 / ARM64)
- **執行階段**：.NET 10.0 Windows Desktop Runtime
- **瀏覽器元件**：Microsoft Edge WebView2 Runtime（Windows 11 已預裝）

---

## 🚀 建置 (Build)

在專案根目錄開啟 Powershell 或 Terminal 執行：

```powershell
# 建置專案
dotnet build

# 執行專案
dotnet run
```

---

## 📄 授權說明 (License)

MIT License.
