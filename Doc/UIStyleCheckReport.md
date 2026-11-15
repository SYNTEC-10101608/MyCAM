# UI 風格統一：驗收報告（P5）

> 議題：UI 風格盤點、優化、統一（分支 `AUTO-TBD_f_UIStyleUnify`）
> 規範：`.github/instructions/ui-style.instructions.md`；需求與決議：[UIStyleNotes.md](UIStyleNotes.md)
> 執行日期：2026-11-16，Debug 建置

## 1. 檢查腳本結果

| 腳本 | 指令 | 結果 |
| --- | --- | --- |
| 靜態檢查 | `Tools\UICheck\CheckStatic.ps1` | 34 個 Designer 檔，0 個問題（經確認的例外 `StaticExceptions.txt`） |
| 行為不變比對 | `Tools\UICheck\CompareBehavior.ps1`（與 `master`） | 0 個差異（已決議的差異 `BehaviorExceptions.txt`） |
| 執行時版面 | `Tools\UICheck\CheckRuntimeLayout.ps1` | zh-TW 34 個畫面 0 問題；zh-CN 34 個畫面 0 問題 |

一次執行全部：`Tools\UICheck\UICheck.bat runtime <截圖資料夾>`。

## 2. 涵蓋範圍

- 對話框 31 個（含 `CncUI` 的 `CNCOffsetDialog`、進度視窗 `LoadingBarDlg`）
- 嵌入式面板：`CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`（以典型大小承載後檢查）
- 未由腳本開啟：`StartupForm`（以 `CaptureMainWindow.ps1` 截圖）、`CNCDlg`（承載 VNC 畫面）

## 3. 截圖

| 項目 | 位置 | 狀態 |
| --- | --- | --- |
| 所有對話框與 CNC 面板 × zh-TW / zh-CN | `UICheckScreens\<語系>\<畫面>.png`（不進版控，可用 `UICheck.bat runtime UICheckScreens` 重新產生） | 已產生 |
| 主視窗橫式 zh-TW | `UICheckScreens\MainWindow_zh-TW_Landscape.png` | 已產生 |
| 主視窗直式、主視窗 zh-CN | — | **待人工確認**：本機螢幕為橫式，視窗高度無法超過螢幕；zh-CN 需切換設定檔語系後重新啟動 |

腳本產生的截圖使用 `DrawToBitmap`，執行時才填入的文字（座標值、步驟說明等）顯示的是 Designer 上的預設文字。

## 4. 需要人工確認的項目

1. Visual Studio Designer：任選一個繼承 `EditDialogBase<T>` 的對話框（例如 `OverCutDlg`）開啟，確認畫面與執行時一致；另開 `StartPointDlg`、`CNCPanel` 各一。
2. 直式人機：對話框是否置中在上半部 CAM 區域；`CNCDlg` 是否仍定位到 CNC 區域。
3. 實機操作：數值框取代 TextBox 的對話框（`AxisTransformDlg`、`AddPathDlg`、`PatternSettingDlg`、`ThreePtTransformDlg`、`AutoSortPathDlg`）的輸入與預覽。
4. CNC 連線狀態下的 `CNCPanel` / `CNCInfoPanel` / `CNCAlarmPanel` 外觀與燈號。
