# 3. 測試規劃

測試依「維護週期 × 時機 × 執行者」分為五類（3.1～3.5），每類再依測試目的細分為測試項目 `T1`…。
規則以名稱識別，內容寫在各檢查檔的開頭；本章只寫每項測試的目的、涵蓋的規則與結果。

## 3.0 總覽與最終結果

最後一次執行：2026-11-23，Debug 建置（`.\Build.bat Debug` 0 個錯誤），文件與檢查整理後的工作目錄。

| 類 | 維護週期 | 時機 | 執行者 | 入口 | 測試項目 | 結果 |
| --- | --- | --- | --- | --- | --- | --- |
| 3.1 | 長期 | 靜態 | 自動 | `Tools\UICheck\UICheck.bat` | T1–T5 | 34 個 Designer 檔，0 個問題 |
| 3.2 | 長期 | 動態 | 自動 | `Tools\UICheck\UICheck.bat runtime [截圖資料夾]` | T6–T10 | 版面 zh-TW、zh-CN 各 42 個畫面狀態、數值框 46 個案例、主視窗 12 組截圖與 Designer 10 個區域，皆 0 個問題 |
| 3.3 | 議題限定 | 靜態 | 自動 | `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | T11–T13 | 行為比對 0 個差異；程式碼風格 341 個檔案 0 個問題 |
| 3.4 | 議題限定 | 動態 | 自動 | 同上 | T14 | 12 個情境、152 行，0 個差異 |
| 3.5 | 議題限定 | — | 人工 | — | T15–T17 | 2026-11-23 使用者確認全部完成（版本 `c4e6dd64`） |

## 3.1 長期維護 × 靜態 × 自動（`CheckStatic.ps1`）

只讀原始碼，不需建置。檢查對象：31 個對話框與 3 個 CNC 面板的 Designer 檔、主視窗、畫面程式碼、`CommonUI`、所有語系 XML。

| 編號 | 目的 | 規則 | 對應 |
| --- | --- | --- | --- |
| T1 | 控制項與命名統一 | `ControlWhitelist`、`CodeControlType`、`ControlInCommonUI`、`CommonUINamespace`、`FieldPrefix`、`ScreenBase` | R2、R3、S10、S15 |
| T2 | 外觀只來自主題與基底 | `DesignerAppearance`、`MainWindowDesigner`、`ThemeToken`、`DataColor`、`CodeAppearance`、`NoDpiScaling` | R1、R2、R5、S9、S11 |
| T3 | 對話框版面依格線 | `DialogGrid`、`RowAlignment`、`SectionTitle`、`TitleBarControl`、`ButtonKind`、`ButtonBarOrder`、`TabOrder` | R1、S3、S4、S12 |
| T4 | 多語系依名稱套用 | `LanguageIdSync`、`LanguageManualApply`、`LanguageOrphanId`、`LanguageApplyCall` | R2、S14 |
| T5 | Designer 能正常開啟 | `DesignerReadable`、`ResxResource` | R5、S11 |

## 3.2 長期維護 × 動態 × 自動（runner）

以 Debug 建置結果在隱藏桌面開啟畫面，zh-TW、zh-CN 各一次；runner 執行結束即從建置輸出刪除。

| 編號 | 目的 | 檢查檔 / 規則 | 對象 | 對應 |
| --- | --- | --- | --- | --- |
| T6 | 文字與控制項在實際字型下不截斷、不重疊、不超出，開啟後版面等於 Designer，文字取自語系檔 | `LayoutRunner.cs`：`TextClipped`、`TextOverflow`、`TitleOverflow`、`ControlOverlap`、`ControlOutside`、`LoadLayout`、`LanguageText` | 所有對話框與 CNC 面板；精靈與多狀態畫面逐一狀態（`STATE_MAP`） | R1、R5、S3、S14 |
| T7 | 數值框的操作約定 | `ControlRunner.cs`：`NumRejectInput`、`NumDebounce`、`NumStep`、`NumSilentRestore` | `DebouncedNumericUpDown`（不開視窗） | R1、R4、S5 |
| T8 | 對話框置中 | `MainWindowRunner.cs`：`DialogCenter`、`PortraitCamArea` | `Show` 開啟的 3 個與 `ShowDialog` 開啟的 2 個對話框，橫式與直式 | R1、S2、S13 |
| T9 | 主視窗的字族、語系文字與 CNC 面板 | `MainWindowRunner.cs`：`MainFontFamily`、`MenuLanguageText`、`PanelInWindow` | 主視窗 CAD / CAM / CNC 頁面 × 橫式 1920×1040 / 直式 1080×1880 | R1、S6、S7、S14 |
| T10 | 主視窗 Designer 如實呈現 | `MainWindowRunner.cs`：`MainDesignerRegion` | 以 Designer 的客戶區 1920×1009 開加工頁，比對 10 個區域 | R5、S12 |

截圖：對話框與 CNC 面板 `<語系>\<畫面>[_<狀態>].png`、主視窗 `MainWindow_<語系>_<頁面>_<Landscape|Portrait>.png`
（紅色虛線框為置中區域）、`MainWindow_Designer.png`；截圖資料夾不進版控。

## 3.3 議題限定 × 靜態 × 自動

基準固定為分支起點 `a4b3826d`，合併後仍可重現。

| 編號 | 目的 | 檢查檔 / 規則 | 對應 |
| --- | --- | --- | --- |
| T11 | 操作行為不變：事件、行為屬性、開啟方式、程式中的狀態切換與分支起點相同，差異只能是已決議的項目 | `CompareBehavior.ps1`：`SameEvents`、`SameRadioGroups`、`SameProperties`、`SameFormProps`、`UsedControls`、`TypeReplace`、`ShowCalls`、`CodeState`（改名以 `RenameMap.txt` 換算，已決議的差異在 `BehaviorExceptions.txt`） | R4、S8 |
| T12 | 本議題寫入的程式碼符合通用規範 | `CheckBranchRules.ps1`：`FileEncoding`、`FinalNewline`、`TrailingSpace`、`LineLength`、`TabIndent`、`NoThisAccess`、`CommentStyle`、`OneTypePerFile` | R7 |
| T13 | 產品程式不留檢查用的程式 | `CheckBranchRules.ps1`：`NoCheckCode` | R6、S17 |

判斷原則：

1. `SameProperties` 在型別改變時比對生效值（沒寫的屬性以反射讀預設值，本議題刪除的舊型別從分支起點原始碼編譯），因此需要先建置 Debug。
2. 行為比對讀的是宣告（屬性、事件、狀態賦值），不是操作結果；共用控制項的操作由 T7 檢查，各畫面的實機操作由 T16 確認。
3. 程式碼風格只檢查本議題寫入的部分：整檔規則在「分支起點的版本符合、現在不符合」或新檔時報，行規則只看新增的行；
   行長只檢查 C#（`.editorconfig`），Designer 檔不套用行規則。

## 3.4 議題限定 × 動態 × 自動

| 編號 | 目的 | 檢查檔 | 對應 |
| --- | --- | --- | --- |
| T14 | 主視窗各區域的位置與基準逐像素相同 | `CompareMainLayout.ps1` + `MainLayoutBaseline.txt`：CAD / CAM / CNC 頁面 × 橫式 1920×1040、1366×768 × 直式 1080×1880、900×1600，共 12 個情境 | R4、S6、S12 |

基準記錄過兩次：移除 `TableLayoutPanel` 前記錄，改寫 `MainLayout` 後逐像素相同；2026-11-22 統一間距是有意的版面調整，
改記錄為新版面。之後的通過只表示符合新版基準。

## 3.5 議題限定 × 人工

| 編號 | 目的 | 內容 | 結果 |
| --- | --- | --- | --- |
| T15 | Designer 能開啟且與執行時一致 | 以 Visual Studio Designer 開啟所有畫面（含泛型基底的對話框、`StartupForm`、三個 CNC 面板） | 2026-11-23 確認 |
| T16 | 數值框實機操作 | `AxisTransformDlg`、`AddPathDlg`、`PatternSettingDlg`、`ThreePtTransformDlg`、`AutoSortPathDlg` 的輸入與預覽 | 2026-11-23 確認 |
| T17 | CNC 實際連線 | 三個 CNC 面板的數值、燈號、警報清單；直式 `CNCDlg`（VNC）定位到 CNC 區域；卡片間距、監控卡片與 3D 畫布對齊、警報位置 | 2026-11-23 確認 |

## 3.6 反向測試

新增或修改規則時，故意做出違規確認能檢出，再還原。本議題做過的反向測試：

| 對象 | 做法 |
| --- | --- |
| 語系與按鈕規則 | 拿掉 `ApplyLanguage()`、放兩個 `Primary`、語系 ID 與控制項不符，`LanguageApplyCall`、`ButtonKind`、`LanguageText` 報出 |
| 控制項型別 | 標籤改回原生 `Label`、程式中 `new Label()`，`ControlWhitelist`、`FieldPrefix`、`CodeControlType` 報出 |
| 主視窗 | 改回 `TableLayoutPanel` / `ToolStripContainer`、改 Designer 寬度，`MainWindowDesigner`、`FieldPrefix`、`MainDesignerRegion` 報出並列出應寫入的值 |
| Designer 可讀性 | 在 `InitializeComponent` 加 `if`、lambda、類別常數、自身方法、非 `this.` 陳述式、字串插補，以及 `ComponentResourceManager`、`<data>` 資源，`DesignerReadable`、`ResxResource` 全數報出 |
| `CommonUI` 分類 | namespace 暫時改錯，`CommonUINamespace` 報出 |
| 數值框 | 換回各輪修正前的控制項，`NumRejectInput`、`NumDebounce`、`NumStep` 報出與審查實驗相同的錯誤；拿掉計時器的 `Stop()` 或把間隔減 50 ms，`NumDebounce` 報出 |
| 行為比對 | 加入 `ReadOnly = true`、放寬允許的替換、反轉 fixture 的預期，`SameProperties` 報出 |
| runner 清理 | 讓 runner 編譯失敗，三支腳本都沒有在建置輸出留下 runner |

## 3.7 已知限制

1. 對話框截圖用 `DrawToBitmap`，執行時才填入的文字顯示的是 Designer 文字；zh-CN 下的寬度未自動檢查（後續 5.2.5）。
2. 隱藏桌面上 3D 檢視區（OpenGL）是空白的，會出現 `wglMakeCurrent` 訊息，可忽略。
3. `CNCDlg` 需連線，不由腳本開啟；CNC 連線中的狀態由 T17 人工確認。
4. 嵌入式面板的卡片座標沒有靜態規則，由 T6、T9 在實際大小下檢查。
