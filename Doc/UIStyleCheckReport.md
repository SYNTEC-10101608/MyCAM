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
| 主視窗與對話框置中 | `Tools\UICheck\CaptureMainWindow.ps1` | zh-TW / zh-CN × CAD / CAM / CNC 頁面 × 橫式 1920×1040 / 直式 1080×1880，共 12 張，0 問題（直式的置中區域為上方 CAM 區 1080×1179） |

一次執行全部：`Tools\UICheck\UICheck.bat runtime <截圖資料夾>`。

## 2. 涵蓋範圍

- 對話框 31 個（含 `CncUI` 的 `CNCOffsetDialog`、進度視窗 `LoadingBarDlg`）
- 嵌入式面板：`CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`（以典型大小承載後檢查）
- 未由腳本開啟：`StartupForm`（以 `CaptureMainWindow.ps1` 截圖）、`CNCDlg`（承載 VNC 畫面）

## 3. 截圖

| 項目 | 位置 | 狀態 |
| --- | --- | --- |
| 所有對話框與 CNC 面板 × zh-TW / zh-CN | `UICheckScreens\<語系>\<畫面>.png`（不進版控，可用 `UICheck.bat runtime UICheckScreens` 重新產生） | 已產生 |
| 主視窗 × zh-TW / zh-CN × CAD / CAM / CNC × 橫式 / 直式 | `UICheckScreens\MainWindow_<語系>_<CAD\|CAM\|CNC>_<Landscape\|Portrait>.png` | 已產生（2026-11-16 補 CAM、CNC 頁面） |

- 對話框截圖使用 `DrawToBitmap`，執行時才填入的文字（座標值、步驟說明等）顯示的是 Designer 上的預設文字。
- 主視窗截圖：紅色虛線框是對話框置中的區域，框內疊上置中後的 `AddPathDlg`。語系只在記憶體中切換，不改設定檔。
  直式是在橫式螢幕上把主視窗開到 1080×1880（橫 / 直式只看外框比例），超出螢幕的部分仍會繪出。
- 頁面：CAD 是啟動後的預設頁；CAM、CNC 是調整視窗大小後，以點選選單「CAM」「加工」切換（與使用者操作相同）。
  CNC 頁面是**未連線**狀態的面板（數值為「—」、按鈕停用），連線後的外觀仍需實機確認。
- 截圖的限制：3D 檢視區是空白的（OpenGL 畫面不經 `PrintWindow` 輸出）；疊上的對話框外框是截圖時未啟用的樣子，
  右下的黑邊是視窗陰影區，不是實際外觀。

## 4. 需要人工確認的項目

1. Visual Studio Designer：任選一個繼承 `EditDialogBase<T>` 的對話框（例如 `OverCutDlg`）開啟，確認畫面與執行時一致；另開 `StartPointDlg`、`CNCPanel` 各一。
2. 直式人機：對話框置中已由腳本確認；`CNCDlg`（需 VNC 連線）是否仍定位到 CNC 區域，以及 CNC 連線時直式下方的面板。
3. 實機操作：數值框取代 TextBox 的對話框（`AxisTransformDlg`、`AddPathDlg`、`PatternSettingDlg`、`ThreePtTransformDlg`、`AutoSortPathDlg`）的輸入與預覽。
4. CNC 連線狀態下的 `CNCPanel` / `CNCInfoPanel` / `CNCAlarmPanel` 外觀與燈號（未連線的外觀已有 CNC 頁面截圖）。
5. 直式 CAM 頁面（寬 1080）的功能工具列放不下，最後幾個按鈕（位置補償、尺寸補償、輪廓編輯、量測）收進右側溢出箭頭。
   圖示尺寸 40×40 與 `master` 相同；需與 `master` 直式實機比對，確認是既有現象還是字型 / 文字寬度改變造成。
