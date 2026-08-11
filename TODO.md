# WinYouTubeMusic 待辦事項與未來優化規劃 (TODO List)

## 📌 優先推進行項目 (Priority Items)

### 1. 單一實例與多視窗共用 Session / Cookie 機制 (Single-Instance & Multi-Window Cookie Sharing)

* **問題背景 (Background)**：
  Chromium (WebView2) 引擎為了保護資料庫結構不損壞，規定同一個 Profile 資料夾 (`%LOCALAPPDATA%\WinYouTubeMusic\WebViewData`) 只能被單一主程序 (Master Process) 獨佔鎖定。當開啟第二個程式實例 (`WinYouTubeMusic.exe #2`) 時，第二個實例因遭遇 `0x800700AA (資源正在使用中)` 檔案鎖定，會切換至備援檔，導致第二個實例無法共用第一個實例的 Google 登入 Cookie。

* **最佳解決方案 (Proposed Solution)**：
  1. **單一實例檢測與喚醒 (Single-Instance Mutex Activation)**：
     - 使用 Win32 `Mutex` (或 `EventWaitHandle`) 檢測是否有已在執行的主程式實例。
     - 當使用者雙擊開啟第二個 `.exe` 實例時，第二實例會發送 Win32 通訊訊息（如 `WM_COPYDATA` 或 `SetForegroundWindow`），自動喚醒並將已有登入狀態的主視窗拉至最前階置頂。
  2. **同程序多視窗共享環境 (Shared Environment Multi-Window)**：
     - 若使用者需要在桌面同時開立多個 YouTube Music 視窗，改為在**同一個程序 (Process)** 內實作多視窗。
     - 所有視窗共享同一個 `CoreWebView2Environment` 實例，確保所有開立的視窗都能 100% 即時共用登入狀態與播放 Session。

---

## 💡 未來擴充規劃 (Future Enhancements)

- [ ] **全域快捷鍵 (Global Hotkeys)**：支援在背景或遊戲中直接透過快捷鍵切換曲目與暫停。
- [ ] **系統托盤最小化 (Minimize to System Tray)**：關閉或最小化視窗時常駐於 Windows 11 右下角托盤區背景播放。
