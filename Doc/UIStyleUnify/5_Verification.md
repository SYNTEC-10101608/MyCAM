# 5. 測試驗證

最後一次執行：2026-11-20，Debug 建置（`.\Build.bat Debug` 0 個錯誤），Designer 協作方式（3.24）之後。

| 類別 | 位置 | 入口 | 結果 |
| --- | --- | --- | --- |
| A. 可持續使用：規範與風格檢查 | `Tools/UICheck/` | `Tools\UICheck\UICheck.bat runtime UICheckScreens` | 全部通過 |
| B. 本議題限定：功能正確性與相容性 | `Doc/UIStyleUnify/Tests/` | `Doc\UIStyleUnify\Tests\RunIssueChecks.bat` | 全部通過 |
| C. 人工確認 | — | — | 部分待確認（5.3） |

兩類測試的分法：檢查「規範本身」、之後改畫面仍有意義的屬於 A，隨規範維護；檢查「本議題的改動」（與改動前比對、
本議題寫入的程式碼）的屬於 B，基準固定在分支起點，合併後仍可重現，但不再隨開發更新。

## 5.1 A 類：可持續使用的檢查（`Tools/UICheck`）

### 內容與結果

| 腳本 | 規則 | 檢查對象 | 結果 |
| --- | --- | --- | --- |
| `CheckStatic.ps1`（+ `DesignerParser.ps1`、`StaticExceptions.txt`） | S1–S25 | 34 個 Designer 檔（31 個對話框、3 個 CNC 面板）與主視窗；畫面程式碼；`CommonUI`；所有語系 XML；規範第 9 節 | 0 個問題 |
| `CheckRuntimeLayout.ps1`（+ `LayoutRunner.cs`） | T1–T7 | `MyCAM.exe`、`CncUI.dll` 中所有對話框與嵌入式面板，精靈與多狀態畫面逐一狀態；zh-TW、zh-CN | 各 42 個畫面狀態，0 個問題 |
| `CaptureMainWindow.ps1`（+ `MainWindowRunner.cs`） | P1–P6 | 主視窗 zh-TW / zh-CN × CAD / CAM / CNC 頁面 × 橫式 1920×1040 / 直式 1080×1880，共 12 組；置中檢查 `Show` 開啟的 3 個與 `ShowDialog` 開啟的 2 個對話框；另以 Designer 的客戶區 1920×1009 開加工頁，比對 10 個區域與 Designer（P6） | 13 組 0 個問題 |

規則的內容見規範第 8 節與各腳本開頭的註解。輔助檔案：

| 檔案 | 用途 |
| --- | --- |
| `UICheck.bat` | 入口。無參數：靜態檢查；`runtime [截圖資料夾]`：另跑執行時版面與主視窗（需先建置 Debug） |
| `HiddenDesktopRun.cs` | 讓 runner 在隱藏桌面執行，不佔用螢幕；兩個 `.ps1` 加 `-Visible` 改回一般桌面 |
| `SyncLanguage.ps1` | 執行時檢查前，把建置輸出中缺少或過期的語系檔從 `Language/` 複製過去 |
| `FixTabOrder.ps1` | 依版面重編 `TabIndex`（修正 S10 的工具，不是檢查） |
| `DesignerDiff.ps1` | Designer 檔的語意差異，供提交前與 review 使用（報告，不是檢查）。以 87ec4ce0 → 510f7a2b 驗證：`ToolStripContainer` 換成 `Panel` 列為控制項增刪與父容器變更，工具列按鈕重新量測的大小列為屬性變化，resx 為 `formatting only` |
| `StaticExceptions.txt` | 資料色登錄、承載單一遠端內容的視窗、已決議的例外，每行附理由 |

### 涵蓋範圍

| 對象 | 檢查 |
| --- | --- |
| 對話框 31 個 | 靜態全部規則；執行時開啟後檢查。精靈逐一狀態：`AddPathDlg` 4 步、`DesignSupportBracketDlg` 3 頁、`CNCThreePointDlg` 4 個狀態 |
| 嵌入式面板 `CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel` | 以 Designer 尺寸承載檢查；`CNCPanel` 另檢查連線後加工鈕可用的狀態；主視窗中以實際大小再檢查（P4） |
| 主視窗 `StartupForm` | 控制項型別與外觀屬性（S19）、欄位前綴（S9）、字族（P3）、選單與工具列語系文字（P5）、Designer 的區域與執行時一致（P6）、截圖 |
| 對話框置中 | `Show` 開啟的 `AddPathDlg`、`OverCutDlg`、`StartPointDlg`，`ShowDialog` 開啟的 `ConnectDlg`、`GlassDXFImportDlg`，橫式與直式 |
| 語系 | 每個畫面呼叫 `ApplyLanguage`（S14）、控制項顯示語系檔文字（T7、P5）、各語系 ID 一致（S6）、沒有孤兒 ID（S13） |
| `CommonUI` | 控制項類別只在 `CommonUI`（S15）；資料夾、namespace、元件目錄小節一致（S21） |
| 產品程式 | 不含檢查用的程式（規範第 8 節；本議題另以 B 類 I9 檢查） |

未由腳本開啟：`CNCDlg`（承載 VNC 遠端畫面，需連線）。

### 截圖

| 項目 | 位置 |
| --- | --- |
| 對話框與 CNC 面板 × zh-TW / zh-CN（多狀態為 `<畫面>_<n>.png`） | `UICheckScreens\<語系>\` |
| 主視窗 × zh-TW / zh-CN × CAD / CAM / CNC × 橫 / 直式 | `UICheckScreens\MainWindow_<語系>_<頁面>_<Landscape\|Portrait>.png` |
| 主視窗 × Designer 的客戶區（P6） | `UICheckScreens\MainWindow_Designer.png` |

`UICheckScreens/` 不進版控（資料夾內 `.gitignore` 為 `*`），以 `UICheck.bat runtime UICheckScreens` 重新產生。
主視窗截圖中紅色虛線框是對話框置中的區域，框內疊上置中後的 `AddPathDlg`。

### 反向測試

確認規則真的能檢出問題：S14、S16、T7（2026-11-16）、S21 的資料夾與 namespace 一致（2026-11-17，把 `RoundDrawing`
的 namespace 暫時改錯，S21 報錯，還原後通過）。S1、S9、S23 的原生 `Label`（2026-11-19，把 `OverCutDlg` 的標籤暫時改回原生 `Label`、在程式中加 `new Label()`，三條規則都報錯，還原後通過）。S19 與 P6（2026-11-19，把 `m_pnlBottom` 暫時改回 `TableLayoutPanel`、
`m_pnlViewer` 的 Designer 寬度改為 1200，S19 與 P6 都報錯，P6 並列出應寫入的值，還原後通過）。S9、S19 的 `ToolStripContainer`（2026-11-20，把 `m_pnlToolBar` 暫時改回 `ToolStripContainer`，兩條規則都報錯，還原後通過）。S24（2026-11-20，在 `LeadDlg.Designer.cs` 的 `InitializeComponent` 暫時加入 `if` 區塊、lambda 事件、類別常數、`this.ApplyStyle()`、沒有 `this.` 的欄位、字串插補，六項都報錯，還原後通過）。S25（2026-11-21，在 `LeadDlg.Designer.cs` 暫時加入 `ComponentResourceManager`、在 `LeadDlg.resx` 與 `StartupForm.resx` 暫時加入 `<data>` 資源，三項都報錯，還原後通過）。

### 維護注意事項

1. **何時跑**：修改畫面後跑 `UICheck.bat`，全部通過才提交；新增畫面或改動版面時另跑 `UICheck.bat runtime`（先建置 Debug）。
2. **規則與規範同步**：規範（`.github/instructions/ui-style.instructions.md`）的規則改了，`CheckStatic.ps1` / `LayoutRunner.cs`
   / `MainWindowRunner.cs` 的對應規則要一起改，並更新規範第 8 節的規則表與腳本開頭的註解。新增規則時以反向測試確認能檢出問題。
3. **畫面資料夾**：靜態檢查只掃 `CheckStatic.ps1` 的 `$SCREEN_DIRS`（Designer 檔）與 `$CODE_SCOPE`（程式碼）；
   `FixTabOrder.ps1` 也有一份 `$SCREEN_DIRS`。畫面放到新的資料夾時，兩處都要加。
4. **執行時檢查自動涵蓋新畫面**：runner 以反射找出 `MyCAM.exe`、`CncUI.dll` 中所有對話框與嵌入式面板
   （`SKIP_TYPE_LIST` 除外）。建構子需要參數的畫面，runner 依參數型別產生預設值；無法開啟的畫面算失敗，不會被略過。
5. **多狀態畫面**：新增精靈或依狀態改版面的畫面，把各狀態加到 `LayoutRunner.cs` 的 `STATE_MAP`
   （「方法名 參數…」，以反射呼叫產品既有的方法）。**產品中被 `STATE_MAP` 呼叫的方法改名或改參數時，`STATE_MAP` 要一起改**，
   否則該畫面會開啟失敗。不可為了檢查在產品程式加方法。
6. **置中檢查的對話框**：`CaptureMainWindow.ps1` 的 `-Dialogs`（`Show`）與 `-ModalDialogs`（`ShowDialog`）各列幾個代表；
   開啟方式改變時同步調整。
7. **例外**：`StaticExceptions.txt` 只登錄資料色、承載單一遠端內容的視窗與已決議的例外，格式「規則 類別 控制項 理由」，
   理由要寫明決議出處。共用 / 執行時的標題文字、`SectionStack` 排列的卡片、精靈的頁由檢查自行辨識，不要加例外。
   新增資料色常數時登錄 S20。
8. **元件目錄**：`CommonUI` 新增公開型別時，放進所屬類別的資料夾，並在規範第 9 節該類別的小節加一列（S21）；
   新增類別時，同時新增資料夾、namespace 與小節（小節標題寫 `` （`CommonUI.<類別>`） ``）。
9. **語系檔**：執行時檢查讀的是建置輸出的語系檔；`SyncLanguage.ps1` 會先同步，平常開發改了 `Language/` 仍要跑 `CopyRef.bat`。
10. **runner 的編譯**：兩個 `.ps1` 以 Visual Studio 的 Roslyn `csc` 把 runner 編譯到 `MyCAM\bin\<組態>`（才能以與 `MyCAM.exe`
    相同的方式找到語系檔與相依組件），執行結束即刪除；runner 參考的組件改名或新增相依時，更新 `.ps1` 中的 `/r:` 清單。
11. **已知限制**：對話框截圖用 `DrawToBitmap`，執行時才填入的文字顯示的是 Designer 文字；隱藏桌面上 3D 檢視區（OpenGL）
    是空白的，執行時會出現 `wglMakeCurrent` 錯誤訊息，可忽略；疊在主視窗上的對話框外框是未啟用的樣子。
12. **檢查涵蓋不到的部分**（列於 [6_FollowUp.md](6_FollowUp.md)）：面板卡片的靜態座標規則、執行時才填入的文字在 zh-CN 下的寬度、
    CNC 連線中的狀態。

## 5.2 B 類：本議題限定的檢查（`Doc/UIStyleUnify/Tests`）

### 內容與結果

| 腳本 | 規則 | 檢查什麼 | 結果 |
| --- | --- | --- | --- |
| `CompareBehavior.ps1`（+ `RenameMap.txt`、`BehaviorExceptions.txt`） | B1–B8 | 與分支起點 `a4b3826d` 比對：Designer 的事件訂閱、RadioButton 群組、非外觀屬性、表單行為屬性、程式使用的控制項仍存在、型別只做外觀替換、全 repo 的 `Show` / `ShowDialog` 呼叫點、改動過的 `.cs` 中事件訂閱與 `Enabled` / `Visible` / `ReadOnly` / `Checked` 賦值。控制項改名先以 `RenameMap.txt`（304 筆）換算 | 0 個差異 |
| `CheckBranchRules.ps1` | I1–I9 | 本議題新增或改動的檔案符合通用與 C# 規範：編碼與 BOM、檔尾換行、行尾空白、C# 行長 120、tab 縮排、不用 `this.`、註解為英文且不用 `/* */`、產品專案一檔一個頂層型別；產品程式不提到檢查程式、建置輸出沒有殘留 runner | 335 個檔案，0 個問題 |
| `CompareMainLayout.ps1`（+ `MainLayoutBaseline.txt`） | — | 主視窗各承載區的座標與移除 `TableLayoutPanel` 前的基準逐像素相同：CAD / CAM / CNC 頁面 × 橫式 1920×1040、1366×768 × 直式 1080×1880、900×1600，共 12 個情境（zh-TW；需先建置 Debug） | 152 行，0 個差異 |
| `RunIssueChecks.bat` | — | 依序執行上面三個腳本 | 全部通過 |

`BehaviorExceptions.txt` 列出已決議的行為差異（數值框取代、唯讀座標改標籤、區塊標題 `Label` 併入卡片、步驟標籤改卡片標題、
主視窗直式 CNC 承載區的 `SizeChanged` 處理移除、從不顯示的第二、三層工具列容器刪除、功能工具列容器改為 `Panel`、
精靈換頁移入 `SectionStack.ShowPage`），每行附決議出處。

### 判斷原則

1. **行為不變（B 類的核心）**：外觀可以變；事件、行為屬性、開啟方式、程式中的狀態切換必須與分支起點相同，
   差異只能是 `BehaviorExceptions.txt` 中已決議的項目。`TabIndex` 依版面重排是已決議的外觀調整，不比對，由 A 類 S10 檢查。
2. **只檢查本議題寫入的部分**：`CheckBranchRules.ps1` 的整檔規則只在「分支起點的版本符合、現在不符合」或新檔時報；
   行規則只看新增的行。既有程式碼原本就不符合的不在範圍（通用規範：不為了套用規範而改動與本次修改無關的既有程式碼）。
3. 行長規則依 `.editorconfig`：`max_line_length = 120` 只設在 `*.cs`，所以 I4 只檢查 C#。
4. Designer 檔由 Visual Studio 產生（例如 `// ` 行尾空白、部分縮排），不套用行規則。

### 維護注意事項

1. **何時跑**：本議題合併前；`master` 前進後 merge / rebase 再跑一次。合併之後不需要例行執行，只在需要重現本議題的驗證時使用。
2. **基準固定為 `a4b3826d`**：兩個腳本都以 `-Base` 參數指定，預設為分支起點，合併後仍可重現。不要改成 `master`。
   `CompareMainLayout.ps1` 的基準是 `MainLayoutBaseline.txt`（改動前以 `-Update` 記錄一次），不要重新記錄。
3. **相依**：`CompareBehavior.ps1` 使用 `Tools/UICheck/DesignerParser.ps1`；`DesignerParser.ps1` 改動時，重跑一次確認仍可執行。
4. **不再增修**：之後的開發不在 `RenameMap.txt`、`BehaviorExceptions.txt` 加項目；新的議題需要行為比對時，
   以本資料夾的腳本為範本，在該議題的資料夾另建一份並指定自己的基準。

## 5.3 C 類：人工確認

| # | 項目 | 狀態 |
| --- | --- | --- |
| 1 | Visual Studio Designer 開啟所有畫面，外觀與執行時一致（含泛型基底 `EditDialogBase<T>` 的對話框） | 2026-11-16 已確認 |
| 2 | Designer 開啟第二輪之後改動的畫面：`DesignSupportBracketDlg`（X / Y 支架位置兩張卡片）、`CNCPanel`（加工鈕種類）；以及收官時 namespace 變動後的任一對話框與 CNC 面板（確認 Designer 仍能載入 `CommonUI`）；`StartupForm` 的區域分布與功能工具列區（2026-11-20 改為 `Panel` 後） | 待確認 |
| 3 | 實機操作數值框取代 `TextBox` 的對話框（`AxisTransformDlg`、`AddPathDlg`、`PatternSettingDlg`、`ThreePtTransformDlg`、`AutoSortPathDlg`）的輸入與預覽 | 待確認 |
| 4 | CNC 實際連線：`CNCPanel` / `CNCInfoPanel` / `CNCAlarmPanel` 的數值、燈號、警報清單；直式時 `CNCDlg`（VNC）定位到 CNC 區域 | 待確認 |

在 worktree 用 Visual Studio 開方案前先卸載 `FCLTool`（`.vs/` 不會帶入 worktree），並先建置 `CommonUI`，Designer 才會載入最新的元件。
