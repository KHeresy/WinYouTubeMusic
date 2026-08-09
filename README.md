# YouTube Music for Windows 11 (WinYotuTubeMusic)

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
   - **頁面導覽與釘選列**：頂部整合上一頁 (`◀`)、下一頁 (`▶`)、重新整理 (`🔄`) 與視窗置頂 (`📌`) 功能。
   - **登入狀態持久化**：自動記憶 Google / YouTube Music 帳號登入與播放偏好設定。

5. **強固性與偵錯支援**
   - **自動備援 Session 避鎖**：當背景舊進程鎖定資料庫時，自動切換至備援 Session Profile 避開 `0x800700AA` 錯誤。
   - **工作目錄日誌 (app.log)**：即時將執行紀錄寫入工作目錄 `app.log`，方便進行診斷與偵錯。

---

## 💻 系統需求 (System Requirements)

- **作業系統**：Windows 11 / Windows 10 (x64 / ARM64)
- **執行階段**：.NET 10.0 Windows Desktop Runtime
- **瀏覽器元件**：Microsoft Edge WebView2 Runtime（Windows 11 已預裝）

---

## 🚀 執行與建置 (Build & Run)

### 執行應用程式
直接開啟建置完成的執行檔：
```cmd
D:\Program\WinYotuTubeMusic\bin\Debug\net10.0-windows10.0.19041.0\WinYotuTubeMusic.exe
```

### 原始碼建置
在專案根目錄開啟 Powershell 或 Terminal 執行：
```powershell
# 建置專案
dotnet build

# 執行專案
dotnet run
```

---

## 📁 專案架構 (Project Structure)

```text
D:\Program\WinYotuTubeMusic\
├── App.xaml / App.xaml.cs                 # WPF 應用程式入口
├── MainWindow.xaml / MainWindow.xaml.cs   # 主視窗介面與 WebView2 控制 logic
├── WinYotuTubeMusic.csproj                # .NET 10 專案檔
├── app.manifest                            # Windows DPI & 相容性清單
├── Native/
│   ├── TaskbarList.cs                     # Win32 ITaskbarList3 COM 介面定義
│   └── WpfIconHelper.cs                   # 工作列縮圖 DrawingImage 向量圖形產生器
├── Services/
│   └── SmtcService.cs                     # Windows 11 SMTC 系統媒體傳輸控制服務
├── Scripts/
│   └── youtube_music_inject.js            # 注入 YouTube Music 網頁的 DOM 監聽腳本
├── app.log                                # 執行階段偵錯日誌檔案
└── README.md                              # 專案說明文件
```

---

## 📄 授權說明 (License)

MIT License.
