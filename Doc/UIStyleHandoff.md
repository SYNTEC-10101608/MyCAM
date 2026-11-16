# UI 風格統一：階段交接

> 撰寫日期：2026-11-16。本檔是給下一個工作階段的接續說明；需求與決議仍以 [UIStyleNotes.md](UIStyleNotes.md) 為唯一紀錄來源。

## 1. 工作環境

| 項目 | 值 |
| --- | --- |
| 分支 | `AUTO-TBD_f_UIStyleUnify`（議題編號尚未決定，`TBD` 待補後改名） |
| Worktree | `D:\Decrypt\MyCAM\MyCAM\.claude\worktrees\ui-style-unify` |
| 主 checkout | `D:\Decrypt\MyCAM\MyCAM`（`master`） |
| 分支基底 | `master` = `origin/master` = `a4b3826d`（施工期間 master 未前進） |
| 推送狀態 | **尚未推送**到任何 remote（`origin` GitLab / `github` 個人），推送前要先問使用者推到哪裡 |
| 未進版控的資料夾 | `UICheckScreens/`（截圖，內含 `.gitignore` = `*`） |
| 建置 | `.\Build.bat Debug`（PowerShell，只建 C#）；不建 FCLTool；`Language/` 有變動，執行前重跑 `CopyRef.bat` |
| 舊分支 | `AUTO-TBD_f_UIStyleImprove`：只沿用色票、字型、尺寸與圖示，實作不參考 |

同一個 repo 另有 worktree `unify-path-craft-edit`（`AUTO-TBD_f_UnifyPathCraftEdit`），與本議題無關。

## 2. 完成程度

草案 v0.5 的 P0～P5 全部完成，分 18 個 commit（master 之後）：

| 階段 | Commit | 內容 |
| --- | --- | --- |
| 草案 | `514fb23f` | 盤點草案與決議紀錄 |
| P0 | `95243f00` | 規範定稿 `.github/instructions/ui-style.instructions.md`，加入指引目錄與 `CLAUDE.md` |
| P1 | `889469c3` | `CommonUI` 主題 token、基底、共用元件、三個控制項移入、圖示、檢查腳本、示範對話框 |
| P2 | `ac20a093` | 主視窗：`MainWindowBase`、工具列繪製、移除寫死顏色與 Magenta 透明色 |
| P3 | `2f0d2e6c`～`eb4386ce`（10 個） | 31 個對話框遷移，分批提交 |
| P4 | `91e1f205`、`0cacef2d` | CNC 面板改 `PanelBase`、外觀統一；進度條；刪除 `GroupPanel` / `PanelButton` |
| P5 | `0691d5a9`、`46d176fb` | 驗收報告；直式主視窗截圖與對話框置中檢查 |

之後依 Reviewer 盤點（`Doc/UIStyleReview.md`）進行兩輪討論與施工：控制項欄位依型別前綴改名、CNC 面板改用
`ThemeMetrics`、字型初始化移到 `MyApp`、按鈕種類分 CAM / CNC 兩組、語系套用統一由三個基底的 `ApplyLanguage`
處理（含主視窗選單與工具列）、`ResponsiveLayout` 移入 `CommonUI`、規範新增元件目錄，檢查擴充到 S1–S23、
B1–B8、T1–T7、P1–P5。決議與施工結果見 `UIStyleNotes.md` 第 3 節。

最後一次 `Tools\UICheck\UICheck.bat runtime UICheckScreens` 的結果見 `UIStyleCheckReport.md`：全部 0 個問題。

## 3. 產出的資料與用法

### 3.1 文件

| 檔案 | 意義 | 怎麼用 |
| --- | --- | --- |
| `.github/instructions/ui-style.instructions.md` | **團隊規範**（定稿），AI 工具也會讀 | 之後新增 / 修改畫面一律依此；規則有變時改這份 |
| `Doc/UIStyleDraft.md` | 設計草案 v0.5：盤點結果、設計緣由、分期計畫 | 想知道「為什麼這樣定」時查；不再當規範用 |
| `Doc/UIStyleNotes.md` | 需求、決議、後續項目、待決問題（都標日期） | 有新要求或決議時追加；第 4 節是範圍外的後續工作，第 5 節是待決問題 |
| `Doc/UIStyleCheckReport.md` | 驗收報告 | 檢查結果、涵蓋範圍、截圖位置、需要人工確認的項目 |
| `Doc/UIStyleReview.md` | Reviewer 盤點與兩輪待決問題 | 查 Review 提出的問題與處理結果 |
| `Doc/UIStyleHandoff.md` | 本檔 | 接續工作時先讀 |

### 3.2 程式

| 位置 | 內容 |
| --- | --- |
| `CommonUI/` | 所有元件，用途與使用方式見規範第 9 節「CommonUI 元件目錄」 |
| `Language/CommonUI_*.xml` | 共用按鈕文字（確定、取消、重設、預覽、關閉） |
| `MyCAM/Resource/*.png`、`IconSource/` | 新圖示與產生腳本 `RenderIcons.ps1` |

### 3.3 檢查腳本（`Tools/UICheck`）

| 檔案 | 用途 |
| --- | --- |
| `UICheck.bat` | 入口。無參數：靜態檢查 + 行為比對；`runtime [截圖資料夾]`：另跑執行時版面與主視窗檢查（需先建置 Debug） |
| `CheckStatic.ps1` + `DesignerParser.ps1` | 靜態檢查 S1～S23（規則列表見規範第 8 節與腳本開頭） |
| `CompareBehavior.ps1` + `RenameMap.txt` | 與 `master` 比對行為面差異 B1～B8；控制項改名以對照表換算 |
| `CheckRuntimeLayout.ps1` + `LayoutRunner.cs` | 開啟所有對話框與 CNC 面板（精靈依 `STATE_MAP` 各狀態），檢查 T1～T7，可輸出截圖 |
| `CaptureMainWindow.ps1` + `MainWindowRunner.cs` | 主視窗截圖與 P1～P5。`-Mode CAD\|CAM\|CNC` 切換頁面；直式在橫式螢幕上以 1080×1880 開窗 |
| `HiddenDesktopRun.cs` | 讓 runner 在隱藏桌面執行，不佔用螢幕；`-Visible` 改回一般桌面 |
| `SyncLanguage.ps1` | 執行時檢查前把過期的語系檔複製到建置輸出 |
| `FixTabOrder.ps1` | 依版面重編 `TabIndex`（S10） |
| `StaticExceptions.txt` / `BehaviorExceptions.txt` | 資料色登錄、單一遠端內容的視窗、已決議的例外與行為差異，每行附理由 |

Runner 由腳本以 Roslyn `csc` 編譯到 `MyCAM\bin\<組態>` 執行，結束即刪除，不進方案也不留在建置輸出。

## 4. 還沒處理的事

### 4.1 需要使用者協助確認

見 `UIStyleCheckReport.md` 第 4 節（Designer 開啟本輪改動的畫面、數值框實機操作、CNC 實際連線）。

### 4.2 待決問題（`UIStyleNotes.md` 第 5 節）

1. ~~`CNCThreePointDlg` 工作點座標 9 欄與誤差容忍欄~~：2026-11-16 已決議維持 TextBox、不改唯讀；可為空的數值框列為後續議題。
2. ~~CNC 面板內的區塊標題是否改為卡片標題~~：2026-11-16 已改為 `SectionPanel.Text`，`ResponsiveLayout` 保留標題列固定高度。
3. ~~泛型基底在 Designer 的表現~~：2026-11-16 已確認正常。

### 4.3 收尾

- 決定議題編號後，分支 `AUTO-TBD_...` 改名，並補 `UIStyleNotes.md` 開頭的議題編號。
- 決定推送的 remote 與分支。
- 合併前若 `master` 已前進，先 rebase / merge，再跑一次 `UICheck.bat runtime`（行為比對以 `master` 為基準）。

### 4.4 範圍外的後續項目（`UIStyleNotes.md` 第 4 節）

編輯工具對話框補「取消」、對話框行為模式統一、`Show` / `ShowDialog` 統一、橫式警報 / 日誌面板切換、主視窗行為重構、`CalibrationREFDlg` KeyDown 重複訂閱、數值框細節差異、唯讀座標無法複製、CNC 面板語系切換後字族未更新、支援未輸入的數值框（三點校正）、檢查涵蓋範圍的缺口、
執行時檢查發現語系檔過期（已由 `SyncLanguage.ps1` 處理）。這些都不在本議題施工範圍，另開議題處理。

## 5. 截圖與檢查的已知限制

- 對話框截圖用 `DrawToBitmap`，執行時才填入的文字顯示的是 Designer 預設文字。
- 主視窗截圖的 3D 檢視區是空白的（OpenGL 不經 `PrintWindow` 輸出），執行時會出現 `wglMakeCurrent` 錯誤訊息，可忽略。
- 疊在主視窗上的對話框外框是未啟用的樣子，右下黑邊是陰影區。

## 6. 下一個工作階段的開場 prompt

```text
接續 MyCAM「UI 風格盤點、優化、統一」議題。
- 分支 AUTO-TBD_f_UIStyleUnify，worktree D:\Decrypt\MyCAM\MyCAM\.claude\worktrees\ui-style-unify，不要另開分支或 worktree。
- 先讀 Doc/UIStyleHandoff.md（交接）、Doc/UIStyleNotes.md（需求 / 決議 / 待決問題）、.github/instructions/ui-style.instructions.md（規範）。
- P0～P5 已完成且尚未推送。本階段要處理：<在此填入，例如 Designer 驗證結果、待決問題的決議、推送目標>。
- 原則：只改外觀不改行為；Designer 如實呈現；改 C# 後跑 .\Build.bat Debug，不建 FCLTool；改畫面後跑 Tools\UICheck\UICheck.bat（必要時加 runtime），全部通過才提交。
- Commit 訊息用繁體中文；推送前先問我 remote 與分支；新的要求或決議寫進 Doc/UIStyleNotes.md 並標日期。
```
