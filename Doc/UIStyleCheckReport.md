# UI 風格統一：驗收報告

> 議題：UI 風格盤點、優化、統一（分支 `AUTO-TBD_f_UIStyleUnify`）
> 規範：`.github/instructions/ui-style.instructions.md`；需求與決議：[UIStyleNotes.md](UIStyleNotes.md)
> 執行日期：2026-11-16（Review 第二輪收尾後），Debug 建置

## 1. 檢查腳本結果

| 腳本 | 指令 | 結果 |
| --- | --- | --- |
| 靜態檢查 S1–S23 | `Tools\UICheck\CheckStatic.ps1` | 34 個 Designer 檔與主視窗，0 個問題 |
| 行為不變比對 B1–B8 | `Tools\UICheck\CompareBehavior.ps1`（與 `master`，先套用 `RenameMap.txt`） | 0 個差異（已決議的差異 `BehaviorExceptions.txt`） |
| 執行時版面 T1–T7 | `Tools\UICheck\CheckRuntimeLayout.ps1` | zh-TW、zh-CN 各 42 個畫面狀態，0 個問題 |
| 主視窗 P1–P5 | `Tools\UICheck\CaptureMainWindow.ps1` | zh-TW / zh-CN × CAD / CAM / CNC 頁面 × 橫式 1920×1040 / 直式 1080×1880，共 12 組，0 個問題 |

一次執行全部：`Tools\UICheck\UICheck.bat runtime <截圖資料夾>`。執行後建置輸出中不留檢查程式。

## 2. 涵蓋範圍

| 對象 | 檢查 |
| --- | --- |
| 對話框 31 個（含 `CNCOffsetDialog`、`LoadingBarDlg`） | 靜態全部規則；執行時開啟後檢查。精靈逐一檢查每個狀態：`AddPathDlg` 4 步、`DesignSupportBracketDlg` 3 頁、`CNCThreePointDlg` 4 個狀態 |
| 嵌入式面板 `CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel` | 以 Designer 尺寸承載檢查；`CNCPanel` 另檢查連線後按鈕可用的狀態；主視窗中以實際大小再檢查（P4） |
| 主視窗 `StartupForm` | 控制項型別與外觀屬性（S19）、欄位前綴（S9）、字族（P3）、選單與工具列語系文字（P5）、截圖 |
| 對話框置中 | `Show` 開啟的 3 個、`ShowDialog` 開啟的 `ConnectDlg`、`GlassDXFImportDlg`，橫式與直式 |
| 語系 | 每個對話框 / 面板呼叫 `ApplyLanguage`（S14）、控制項顯示語系檔文字（T7、P5）、四語系 ID 一致（S6）、沒有孤兒 ID（S13）；執行前自動同步建置輸出的語系檔 |
| 產品程式 | 產品專案不含檢查用的程式（規範第 8 節）；控制項類別只在 `CommonUI`（S15）；`CommonUI` 公開型別都列在元件目錄（S21） |

未由腳本開啟：`CNCDlg`（承載 VNC 遠端畫面）。

## 3. 截圖

| 項目 | 位置 |
| --- | --- |
| 對話框與 CNC 面板 × zh-TW / zh-CN（多狀態的畫面為 `<畫面>_<n>.png`） | `UICheckScreens\<語系>\` |
| 主視窗 × zh-TW / zh-CN × CAD / CAM / CNC × 橫式 / 直式 | `UICheckScreens\MainWindow_<語系>_<頁面>_<Landscape\|Portrait>.png` |

`UICheckScreens/` 不進版控，以 `UICheck.bat runtime UICheckScreens` 重新產生。

- 對話框截圖使用 `DrawToBitmap`；主視窗截圖中紅色虛線框是對話框置中的區域，框內疊上置中後的 `AddPathDlg`。
- 截圖在隱藏桌面執行，不影響使用者操作；3D 檢視區是空白的（OpenGL 不經 `PrintWindow` 輸出）。
- CNC 頁面是未連線狀態；`CNCPanel` 連線後的按鈕外觀由執行時檢查的連線狀態截圖呈現。

## 4. 需要人工確認的項目

1. Visual Studio Designer：本輪改動的 `DesignSupportBracketDlg`（X / Y 支架位置兩張卡片）、`CNCPanel`（加工鈕種類）開啟確認。
2. 實機操作：數值框取代 TextBox 的對話框（`AxisTransformDlg`、`AddPathDlg`、`PatternSettingDlg`、`ThreePtTransformDlg`、`AutoSortPathDlg`）的輸入與預覽。
3. CNC 實際連線：`CNCPanel` / `CNCInfoPanel` / `CNCAlarmPanel` 的數值、燈號、警報清單；直式時 `CNCDlg`（VNC）定位到 CNC 區域。
