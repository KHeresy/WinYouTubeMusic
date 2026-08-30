# WinYouTubeMusic 待辦事項與未來優化規劃 (TODO List)

## 📌 已完成項目 (Completed)

- [x] **SMTC 與 AppUserModelID 整合**：解決 Windows 快捷控制中心（`Win + A`）顯示「未知的應用程式」問題，正確顯示應用程式名稱與圖示。
- [x] **DWM 原生自訂縮圖 (Iconic Thumbnail)**：懸停工作列時直接顯示高畫質專輯封面大圖。
- [x] **單一實例與 IPC 喚醒 (Single-Instance Mutex & Named Pipe IPC)**：使用 Mutex 與 Named Pipe 避免多開衝突，確保 Cookie 正常運作並支援外部參數喚醒主視窗。
- [x] **工作列右鍵跳躍清單 (Taskbar Jump List)**：支援「播放清單」與「最近播放」雙分類，並支援 Windows 原生釘選（Pin）與即時點選播放。
- [x] **視窗大小、位置與狀態記憶 (Window Placement Persistence)**：關閉與啟動時自動記錄並還原視窗座標、大小與最大化狀態，並具備多螢幕拔除防呆安全置中機制。

---

## 💡 未來擴充規劃 (Future Enhancements)

- [ ] **全域快捷鍵 (Global Hotkeys)**：支援在背景或全螢幕遊戲中直接透過自訂組合鍵切換曲目、暫停或調整音量。
- [ ] **系統托盤最小化 (Minimize to System Tray)**：關閉或最小化視窗時常駐於 Windows 11 右下角系統匣托盤區背景播放。
- [ ] **桌面歌詞顯示 (Desktop Lyrics)**：支援浮動桌面歌詞顯示。
