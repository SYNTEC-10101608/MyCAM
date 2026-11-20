# 3. 實作紀錄

本章記錄最終的施工結果，節次與 [2_Architecture.md](2_Architecture.md) 對應。施工過程中改過又撤回的做法
（例如 CNC 面板標題的 `TextRole.SectionTitle`、語系改在 `OnLoad` 套用）只記在 [4_Discussion.md](4_Discussion.md)。

3.1～3.13 依架構主題整理；3.14 起為逐檔異動明細（對分支起點 `a4b3826d`），供審查時逐檔對照。

## 3.1 施工階段

| 階段 | Commit | 內容 |
| --- | --- | --- |
| 盤點與草案 | `514fb23f` | 盤點結果、設計草案 v0.5、決議紀錄 |
| P0 規範 | `95243f00` | `.github/instructions/ui-style.instructions.md` 定稿，加入指引目錄與 `CLAUDE.md` |
| P1 基礎建設 | `889469c3` | `CommonUI` 主題 token、`DialogBase`、共用元件、三個控制項移入、圖示、檢查腳本、示範對話框 |
| P2 主視窗 | `ac20a093` | `MainWindowBase`、工具列繪製、移除寫死顏色與 Magenta 透明色 |
| P3 對話框 | `2f0d2e6c` ～ `eb4386ce`（10 個） | 31 個對話框分八批遷移 |
| P4 面板 | `91e1f205`、`0cacef2d` | CNC 面板改 `PanelBase` 與外觀統一、進度條；刪除 `GroupPanel` / `PanelButton` |
| P5 驗收 | `0691d5a9`、`46d176fb`、`b44be8c2` | 驗收報告、直式主視窗截圖、對話框置中檢查、主視窗 CAD / CAM / CNC 頁面截圖 |
| 使用者回饋 | `489815c5` ～ `87b6ddff` | CNC 面板按鈕對齊、`ToolVectorDlg` 區塊合併、隱藏桌面執行、CNC 面板標題改卡片標題 |
| Review 第一輪 | `933030d4` ～ `af642ace` | 字型初始化移到 `MyApp`、控制項全域改名、CNC 面板改用 `ThemeMetrics`、檢查補強 |
| Review 第二輪 | `972eb5c2` ～ `e6c733a0` | 按鈕種類分兩組、語系改在建構子套用、元件目錄、檢查補強、最終盤點 |
| 收官 | 本次提交 | `CommonUI` 五類重整、XML 文件註解、行長修正、測試分類、議題報告整併 |

## 3.2 外觀與行為分層（對應 2.2）

1. 新增 `CommonUI.Forms.DialogBase`，插在 `Form` 與 `FormBase` 之間；原本直接繼承 `Form` 的 `AddPathDlg`、`ConnectDlg`、
   `CNCDlg`、`CNCThreePointDlg`、`CNCOffsetDialog` 與 `LoadingBarDlg` 改繼承 `DialogBase`。`FormBase` 的置中交給 `DialogBase`，
   `Show( owner )` 的行為不變；`EditDialogBase<T>` 不動。
2. `DialogBase` 固定外框 `FixedToolWindow`、`AutoScaleMode = None`、字型、底色，並把這些屬性標成不序列化；
   Designer 檔中的 `FormBorderStyle`、`StartPosition`、`MaximizeBox`、`MinimizeBox`、`ShowIcon`、`ShowInTaskbar`、
   `AutoScaleMode`、`AutoScaleDimensions`、`Font`、`BackColor`、`ForeColor` 全部移除。
3. 新增 `PanelBase`（`CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`、`VNCUserControl`）與 `MainWindowBase`（`StartupForm`）。
4. `Show` / `ShowDialog` 的呼叫點一律未改。

## 3.3 主題（對應 2.3）

1. `CommonUI.Theme`：`ThemeColors`（含 CNC 加工鈕的框線與淡色底 token）、`ThemeFonts`、`ThemeMetrics`、`TextRole`、`ThemeText`、
   `ThemedToolStripRenderer`、`RoundDrawing`。
2. `MyApp.Initialize()`：讀設定、套用介面語系、`ThemeFonts.Initialize`；`Program.Main` 在建立 `StartupForm` 之前呼叫一次。
   修正以 zh-CN 啟動時主視窗字族錯誤；`ToolStrip` 不繼承父容器字型，由 `MainWindowBase` 另外設定。
3. 約 180 處寫死顏色移除；資料色改為具名常數並登錄於 `Tools/UICheck/StaticExceptions.txt`
   （`DesignSupportBracketDlg` X / Y 位置色、`CNCInfoPanel` 五軸色）。
4. 圖示：60 個工具列圖示以 `MyCAM/Resource/IconSource/RenderIcons.ps1` 產生透明背景 PNG（檔名不變），
   功能工具列 40×40，選單、圖層列、視角列、右鍵選單 20×20。

## 3.4 Designer 如實呈現（對應 2.4）

1. 所有畫面 `InitializeComponent()` 之後不再設定字型、顏色、位置、大小（靜態檢查 S11）；原本在程式中調整尺寸與位置的
   `DesignSupportBracketDlg` 改用 `SectionStack.ShowPage`，`CNCThreePointDlg` 手算的 `ClientSize` 改用 `SectionStack`。
2. 狀態文字改為 `ThemedLabel.Role`；非標籤控制項以 `ThemeText.SetRole`。
3. 使用者已在 Visual Studio Designer 開啟確認（含繼承泛型基底 `EditDialogBase<T>` 的 22 個對話框，不需非泛型中介類別）。

## 3.5 `CommonUI`（對應 2.5）

| 資料夾 | 新增 / 移入 |
| --- | --- |
| `Theme` | 見 3.3 |
| `Controls` | 新增 `SectionPanel`（取代 `GroupBox`、`GroupPanel`、區塊標題 `Label`）、`ThemedButton`（取代 `Button`、`PanelButton`）、`ButtonKind`、`ButtonIndicator`、`CommonButtonText`、`ThemedLabel`、`ThemedDataGridView`；移入 `DebouncedNumericUpDown`、`ValueRejectedEventArgs`、`MultiSelectTreeView`（邏輯不變）；`SupportPositionRow` 通用化為 `RemovableValueRow`（範圍、小數位數、單位、刪除文字由呼叫端傳入）；`LoadingBar`、`RoundedProgressBar` 改用 token |
| `Forms` | `DialogBase`、`PanelBase`、`MainWindowBase` |
| `Layout` | `SectionStack`、`DialogPlacement`；`ResponsiveLayout` 由 `CncUI/Helper` 移入並公開 |
| `Language` | `LanguageApplier`、`CommonTexts`（`Language/CommonUI_<語系>.xml`） |

1. 刪除：`CommonUI/Controls/GroupPanel.cs`、`PanelButton.cs`、`MyCAM/Editor/Dialog/DebouncedNumericUpDown.cs`、
   `SupportPositionRow.cs`、`MyCAM/Editor/MultiSelectTreeView.cs`、`CncUI/Helper/ResponsiveLayout.cs`。
2. `LanguageManager` 新增 `HasString`（不記錄缺少 ID 的查詢），供 `LanguageApplier` 判斷 ID 是否存在。
3. 收官時的整理：`SectionStack`、`DialogPlacement`、`ResponsiveLayout` 移到 `Layout`，`LanguageApplier`、`CommonTexts` 移到 `Language`，
   `RoundDrawing` 移到 `Theme`，資料夾與 namespace 一致；`ValueRejectedEventArgs` 從 `DebouncedNumericUpDown.cs` 拆成獨立檔案；
   `CommonUI` 所有公開型別、方法、屬性、事件補 XML 文件註解（token 常數由規範第 2 節的表格說明）。

## 3.6 版面（對應 2.6）

1. 對話框中的約 20 個 `TableLayoutPanel` 與所有 `FlowLayoutPanel`、`Dock`、`Anchor` 改為 Designer 固定座標，依規範 4.4 的格線；
   `TabIndex` 依版面重排（`Tools/UICheck/FixTabOrder.ps1`）。
2. 精靈（規範 4.7 的兩型，3.23）：`CNCThreePointDlg` 3 步為展開式，以 `SectionPanel.Highlighted` 表示目前步驟；
   `AddPathDlg` 4 步為切換式（固定大小），步驟卡片以 `SectionStack.Arrange` 擇一顯示；
   `DesignSupportBracketDlg` 3 頁為切換式（換頁），以 `SectionStack.ShowPage` 換頁，X / Y 支架位置為兩張並排卡片。
3. 使用者回饋調整：`ToolVectorDlg` 三個轉換區塊合併為「控制點移動」一張卡片（高度 705 → 564）；
   CNC 面板「參數設置」與倍率按鈕對齊。
4. 嵌入式面板：卡片間距、內距、第一列改用 `ThemeMetrics`，新增 `PANEL_MARGIN`、`INFO_ROW_HEIGHT`、`INFO_ROW_PITCH`；
   移除所有 `DeviceDpi` 計算；設計尺寸 `CNCPanel` 288×756、`CNCInfoPanel` 736×240；
   區塊標題一律改為 `SectionPanel.Text`，`ResponsiveLayout` 保持標題列固定高度、標題列內的控制項固定靠右。
5. 主視窗：`MainLayout` 的區域配置與比例不變，只新增 `GetDialogArea` 提供 CAM 區域給置中使用（之後改為純計算，3.22）；
   主視窗 Designer 移除寫死的底色與字型（見 3.15）。

## 3.7 置中（對應 2.7）

1. `DialogPlacement.GetCenterLocation` / `MoveToCenter`，CAM 區域由 `StartupForm` 註冊 `AreaProvider`。
2. 刪除原本的三套做法（`MyApp.CalculateDialogCenterLocation`、`LoadingBarDlg` 自算、Designer 的 `CenterParent`）。
3. `DialogShowHelper` 改呼叫 `DialogPlacement`；`CNCDlg` 直式定位到 CNC 區域維持原樣。

## 3.8 多語系（對應 2.8）

1. 30 個對話框在 `InitializeComponent()` 後呼叫 `ApplyLanguage()`；嵌入式面板與主視窗在套用語系的方法中最先呼叫
   `ApplyLanguage( languageManager )`。`StartupForm` 原本約 140 行的逐項套用、`CNCAlarmPanel` 的手寫欄位標題改為一次呼叫。
2. 語系 ID 改為控制項名稱，所有語系 XML 同步修改；與控制項同名、但其實由程式組成的 ID 刪除或改名（`ToolVectorDlg` 軸名、
   `LockMSAxisDlg` 格式字串）；刪除 9 個沒有控制項也沒被使用的 ID；主視窗 9 個不符命名的 ID 改為項目名。
3. 語系檔名與類別名不一致者改名：`LockABDlg` → `LockMSAxisDlg`、`CNCThreePointDialog` → `CNCThreePointDlg`。
4. 修正 `ExtendedGCodeDlg` 的 `IS_START_POINT` / `IS_END_POINT` / `IS_MIX_DATA` 與程式使用的 ID 不一致（簡中一直顯示繁中預設字）。
5. 確定鍵統一為「確定」（`CommonText`）；「應用」「生成路徑」「儲存 / 生成」等有明確動作的按鈕保留原文字。

## 3.9 按鈕種類（對應 2.9）

1. `ButtonKind`：`Normal`、`Primary`、`Danger`、`Start`、`Hold`、`Stop`；`CNCPanel` 的開始 / 暫停 / 繼續 / 停止套用 CNC 種類
   （淡色底、同色框線與文字）。
2. 依規則調整 `FlyCutDlg`：「移除」改 `Danger`、「新增」改 `Normal`。

## 3.10 數值與座標欄位（對應 2.10）

1. 改為 `DebouncedNumericUpDown`：`AxisTransformDlg` 原生數值框 6 個與步距 2 個、`AutoSortPathDlg` 2 個、`AddPathDlg` 3 個、
   `PatternSettingDlg` 12 個、`ThreePtTransformDlg` 機械座標 9 個；範圍與小數位數對應原本的驗證。
2. 改為 `Label`：`ThreePtTransformDlg` CAD 座標 9 個、`CNCThreePointDlg` 圖面點 9 個、`CalibrationREFDlg` 參考點 9 個
   （逐一確認只顯示、從不輸入）。
3. `CNCThreePointDlg` 工件點 9 欄與誤差容忍欄維持 `TextBox`（已決議，見 2.13）。

## 3.11 命名（對應 2.11）

1. 控制項欄位依型別前綴全域改名 304 筆（`Doc/UIStyleUnify/Tests/RenameMap.txt`），事件方法、`.resx`、所有語系 ID 一併改；
   順帶修正拼字（`Legnth`、`Tolernece`、`SlowDonw`）。

## 3.12 檢查機制（對應 2.12）

| 位置 | 檔案 |
| --- | --- |
| `Tools/UICheck`（可持續使用） | `UICheck.bat`、`CheckStatic.ps1`、`DesignerParser.ps1`、`StaticExceptions.txt`、`CheckRuntimeLayout.ps1`、`LayoutRunner.cs`、`CaptureMainWindow.ps1`、`MainWindowRunner.cs`、`HiddenDesktopRun.cs`、`SyncLanguage.ps1`、`FixTabOrder.ps1` |
| `Doc/UIStyleUnify/Tests`（本議題限定） | `RunIssueChecks.bat`、`CompareBehavior.ps1`、`RenameMap.txt`、`BehaviorExceptions.txt`、`CheckBranchRules.ps1` |

1. 收官時把行為比對三個檔案從 `Tools/UICheck` 移到議題資料夾，比對基準由 `master` 改為固定的分支起點 `a4b3826d`；
   `UICheck.bat` 只留可持續使用的檢查。新增 `CheckBranchRules.ps1`（I1–I9）與入口 `RunIssueChecks.bat`。
2. 靜態檢查 S21 擴充為同時檢查 `CommonUI` 的資料夾、namespace 與元件目錄小節一致。
3. 產品程式盤點：本分支新增到產品專案的成員都由產品程式使用，沒有只給檢查用的方法、屬性、旗標或分支；
   runner 執行結束即從建置輸出刪除（I9 檢查）。

## 3.13 收官時修正的通用規範問題

最終盤點以 `CheckBranchRules.ps1` 檢查本議題寫入的程式碼，修正：

1. 本議題新增或改動的 C# 行超過 120 字元：`CommonUI` 10 處、`MyCAM` / `CncUI` 約 40 處（多為控制項改名後變長的既有行）、
   檢查 runner 16 處，全部斷行（只換行，不改邏輯）。Designer 檔為 Visual Studio 產生，不在此列。
2. `StartupForm.cs` 一處 `this.m_TreeView` 改為 `m_TreeView`。
3. 內容重寫的 `LockMSAxisDlg_<語系>.xml` 補 UTF-8 BOM；`DesignerParser.ps1`、`BehaviorExceptions.txt` 補檔尾換行。

## 3.14 逐檔明細：對話框

所有對話框共同的改動，下表不再重複：

1. Designer 檔移除 `FormBorderStyle`、`StartPosition`、`MaximizeBox`、`MinimizeBox`、`ShowIcon`、`ShowInTaskbar`、
   `AutoScaleMode`、`AutoScaleDimensions`、`Font`、`BackColor`、`ForeColor`；座標依規範 4.4 重排，`TabIndex` 依版面重編。
2. `Button` → `ThemedButton`、`GroupBox` / `GroupPanel` → `SectionPanel`、單位與提示 `Label` → `ThemedLabel`（`Role` 設在 Designer）。
3. 控制項欄位依規範第 3 節改名（`RenameMap.txt`），事件方法與語系 ID 跟著改名。
4. 手寫的 `ApplyResourcesToUI()`（逐一 `TryGetString`）刪除，改為建構子中 `InitializeComponent()` 後呼叫 `ApplyLanguage()`；
   只為了取表單標題而存在的 `m_LanguageManager` 欄位一併刪除。
5. 確定 / 取消 / 重設鈕改設 `CommonText`，語系檔中對應的按鈕 ID 刪除（文字改取自 `CommonUI_<語系>.xml`）。

「尺寸」為 `ClientSize`（分支起點 → 現在）。「程式」只列改名以外的實質改動。

| 對話框 | 基底 | 尺寸 | 控制項替換 | 按鈕種類 | 程式 |
| --- | --- | --- | --- | --- | --- |
| `AddPathDlg` | `Form` → `DialogBase` | 400×237 → 380×475 | `GroupBox` 3 → `SectionPanel` 5（步驟 1、3、步驟說明、選項、進階）；數值 `TextBox` 3 → `DebouncedNumericUpDown`（範圍見 3.10） | 確定 `Primary` | 步驟與進階區塊改以 `SectionStack.Arrange` / `PlaceButtonBar` 排列，刪除手算 `Location`、`ClientSize` 與常數 `EDGE_MARGIN`、`ADVANCED_GROUP_GAP`；新增 `GetStepSection`；步驟說明卡片 `m_pnlStepHint` 與說明文字同時顯示；`ValidatePositiveTextBox` 參數型別 `TextBox` → `Control`，驗證邏輯不變 |
| `AutoSortPathDlg` | `EditDialogBase<SortParams>` | 265×520 → 320×545 | `NumericUpDown` 2 → `DebouncedNumericUpDown`；加 `SectionPanel` 1 | 確定 `Primary`、`CommonText` | 路徑提示色 `Color.Red` / `Green` → `Role = Error` / `Success`；下拉選項與共用文字移到 `InitLocalizedTexts`；表單標題 ID `FormTitle` → `AutoSortPathDlg` |
| `AxisTransformDlg` | `FormBase` | 585×328 → 320×394 | `GroupBox` 2 → `SectionPanel`；`NumericUpDown` 6、步距 `TextBox` 2 → `DebouncedNumericUpDown` | 確定 `Primary`；確定 / 取消 / 重設 `CommonText` | 只有改名與斷行；步距的 `Leave` / `KeyDown` 仍解析文字 |
| `CalibrationREFDlg` | `EditDialogBase<CalibrationData>` | 557×266 → 434×291 | `GroupBox` 3 → `SectionPanel`；唯讀座標 `TextBox` 9 → `Label`（`m_lblPnt*Value`）；共用的 X / Y / Z 標題改為每點各自的 `Label` | 確定 `Primary`、`CommonText`；三個刪除鈕與全部刪除 `Danger` | 三個刪除鈕共用的文字移到 `InitLocalizedTexts`；讀寫座標改讀寫 `Label.Text`，判斷與解析不變 |
| `CNCDlg` | `Form` → `DialogBase` | 1178×843（不變） | — | — | `ApplyResourcesToUI` 改名為 `InitAspectRatio`（只留長寬比設定）；直式定位改呼叫 `DialogPlacement.MoveToCenter`；遠端畫面 `Dock = Fill` 保留（規範 4.4 例外） |
| `CNCThreePointDlg` | `Form` → `DialogBase` | 484×576 → 484×508 | `GroupPanel` 2 → `SectionPanel` 4（圖面、選項、工件、計算）；`PanelButton` 9 → `ThemedButton`；圖面點 `TextBox` 9 → `Label`；工件點與誤差 `TextBox` 10 保留（例外，見 2.13） | 計算 `Primary` | 步驟標籤改為卡片標題（`SetStepAppearance` 改設 `SectionPanel.Text` / `Highlighted`）；說明與誤差文字色改 `Role`；顯示 / 隱藏工件與計算卡片改用 `SectionStack`，刪除手算 `ClientSize`；刪除 4 個寫死顏色常數與 `DrawingPoints` / `WorkpiecePoints` 兩個標題字串；語系檔名 `CNCThreePointDialog` → `CNCThreePointDlg` |
| `ConnectDlg` | `Form` → `DialogBase` | 335×103 → 320×141 | `Button` → `ThemedButton`；加 `SectionPanel` 1；提示改 `ThemedLabel` | 確定 `Primary`、`CommonText` | 提示色 `Color.Red` → `Role = Error`；表單標題 ID `FormTitle` → `ConnectDlg`；`m_lblTips_Default` 改為以控制項名稱 `m_lblTips` 為 ID |
| `ContourEditDlg` | `EditDialogBase<object>` | 521×443 → 440×333 | `GroupBox` → `SectionPanel`；`TableLayoutPanel` 2、`FlowLayoutPanel` 4 → 固定座標與無外觀 `Panel` 2 | 儲存 `Primary`、移除 `Danger` | 只有改名 |
| `DesignSupportBracketDlg` | `EditDialogBase<double>` | 執行時決定 → 584×967（Designer 上三頁並列） | 標題 `Label` → `SectionPanel` 9；`SupportPositionRow` → `RemovableValueRow` | 下一步 / 生成 / 儲存 DXF 各頁一個 `Primary` | 換頁與重新置中改 `SectionStack.ShowPage`（3.23）；位置列排列改 `RemovableValueRow.Arrange`；新增位置範圍常數（-99999～99999、1 位、mm）；X / Y 位置色改具名資料色 `X_POSITION_COLOR` / `Y_POSITION_COLOR`；三頁 `Panel` 改名加 `Page` 後綴 |
| `EntryAndExitDlg` | `EditDialogBase<EntryAndExitData>` | 460×186 → 320×171 | `TableLayoutPanel` → 固定座標；加 `SectionPanel` 1 | 確定 `Primary`、`CommonText` | 只有改名 |
| `ExtendedGCodeDlg` | `EditDialogBase<object>` | 607×389 → 500×358 | `TableLayoutPanel` 1、`FlowLayoutPanel` 4 → 無外觀 `Panel` 2 | 完成 `Primary`、`CommonText`；刪除 `Danger` | 只有改名；語系檔的 `IS_START_POINT` / `IS_END_POINT` / `IS_MIX_DATA` 改為程式實際使用的 `IsStartPOINT` / `IsEndPOINT` / `IsMixData`（3.8） |
| `FlyCutDlg` | `EditDialogBase<object>` | 535×251 → 380×248 | `GroupBox` → `SectionPanel` 2；`TableLayoutPanel` 2、`FlowLayoutPanel` 5 → 無外觀 `Panel` 3 | 移除 `Danger`、新增 `Normal` | 只有改名 |
| `GlassDXFImportDlg` | `FormBase` | 379×201 → 320×141 | 加 `SectionPanel` 1 | 確定 `Primary`；確定 / 取消 `CommonText` | 只有改名；`ShowDialog` 不變 |
| `LeadDlg` | `EditDialogBase<LeadData>` | 307×420 → 320×385 | 引入 / 引出標題 `Label` → `SectionPanel` 標題（共 3 張卡片）；單位改 `ThemedLabel` 6 | 確定 `Primary`、`CommonText` | 引入與引出共用的「線長」文字移到 `InitLocalizedTexts` |
| `LockMSAxisDlg` | `EditDialogBase<object>` | 392×465 → 440×458 | `GroupBox` 6 → `SectionPanel` 4 + 無外觀 `Panel` 4；`TableLayoutPanel` 2 → 固定座標 | 確定 `Primary`；確定 / 取消 `CommonText` | 角度框文字色 `ASSIGN_Color` / `AUTO_Color` → `ThemeText.SetRole( Body / Secondary )`；含軸名的文字改用 `Format_*` 格式字串 ID（`InitLocalizedTexts`）；語系檔名 `LockABDlg` → `LockMSAxisDlg` |
| `ManualTransformDlg` | `EditDialogBase<ETrsfConstraintType>` | 310×220 → 320×261 | 加 `SectionPanel` 1；狀態改 `ThemedLabel` | 確定 `Primary`、`CommonText` | 移動狀態色 `Green` / `Gray` → `Role = Success` / `Secondary`；`m_lblAxisPlane` 的執行時文字 ID 改為 `m_lblAxisPlane_Point`（避免與控制項同名） |
| `MeasureDlg` | `EditDialogBase<EMeasureType>` | 310×220 → 320×234 | 加 `SectionPanel` 1；量測結果 `TextBox` 保留（非數值文字） | — | 只有語系改呼叫 `ApplyLanguage()` |
| `MicroJointDlg` | `EditDialogBase<object>` | 318×113 → 320×111 | 加 `SectionPanel` 1 | 「完成」鈕（欄位名 `m_btnCancel`）`Primary`、`CommonText = Confirm`；刪除 / 全部刪除 `Danger` | 只有語系改呼叫 `ApplyLanguage()` |
| `OrderDialog` | `FormBase` | 304×313 → 320×274 | `GroupBox` 2 → `SectionPanel` | 套用鈕 `Primary`、`CommonText = Confirm` | `ApplyResourcesToUI` 中的 `UpdateUIByMode()` 與建構子呼叫的 `InitializeDialog` 重複，刪除後仍在 `InitializeDialog` 呼叫一次 |
| `OverCutDlg` | `EditDialogBase<double>` | 353×118 → 320×111 | 加 `SectionPanel` 1 | 確定 `Primary`、`CommonText` | 只有語系 |
| `PathCompensateDlg` | `EditDialogBase<double>` | 488×159 → 320×141 | `TableLayoutPanel` 2、`FlowLayoutPanel` 1 → 固定座標；加 `SectionPanel` 1 | 確定 `Primary`；確定 / 重設 `CommonText` | 只有語系 |
| `PathEditDlg` | `EditDialogBase<PathEditData>` | 501×264 → 360×227 | `GroupBox` → `SectionPanel`（`m_pnlMove`） | 確定 `Primary`；確定 / 重設 `CommonText` | 參考座標下拉選項移到 `InitRefCoordTypeItems` |
| `PatternSettingDlg` | `EditDialogBase<IStdPatternGeomData>` | 434×458 → 380×282 | 數值 `TextBox` 12 → `DebouncedNumericUpDown`（3 位）；加 `SectionPanel` 2；`label1` → `m_lblPathType` | 確定 `Primary`、`CommonText` | 各圖形面板共用的標籤文字與路徑類型移到 `InitLocalizedTexts`；`Leave` / `Enter` 的驗證只斷行 |
| `RCornerDlg` | `EditDialogBase<object>` | 302×145 → 320×111 | 加 `SectionPanel` 1 | 完成 `Primary`、`CommonText`；刪除 `Danger` | 只有語系 |
| `SewPartDlg` | `EditDialogBase<double>` | 387×122 → 320×111 | `TableLayoutPanel` → 固定座標；加 `SectionPanel` 1 | 確定 `Primary`、`CommonText` | 只有語系 |
| `StartPointDlg` | `EditDialogBase<object>` | 395×293 → 380×247 | `TableLayoutPanel` 3、`FlowLayoutPanel` 1 → 無外觀 `Panel` 1；加 `SectionPanel` 1 | 套用 `Primary` | 只有語系 |
| `ThreePtTransformDlg` | `EditDialogBase<TransParam>` | 485×385 → 400×334 | `TableLayoutPanel` → `SectionPanel` 2；CAD 座標 `TextBox` 9 → `Label`；機械座標 `TextBox` 9 → `DebouncedNumericUpDown`；新增三個點的標題 `m_lblMachinePoint1～3` | 確定 `Primary`、`CommonText` | 原本未加前綴的 `tbx*`、`btnOK`、`label1～3` 依規範改名；確定鈕的解析與「座標無效」提示不變 |
| `ToolVectorDlg` | `EditDialogBase<EToolVecInterpolateType>` | 449×395 → 476×564 | `GroupBox` 5 → `SectionPanel` 4（三個轉換區塊合併為「控制點移動」，3.6）；`TableLayoutPanel` → 固定座標；圖例 `GroupBox` → 無外觀 `Panel` 的色塊（資料色例外） | 移除 / 清除 `Danger` | 起點 / 終點切換列移出 `m_pnlIndexParam`，與其同時停用 / 啟用（`m_pnlStartEndSwitch`）；欄位 `m` → `m_lblInterpolateType` |
| `TraverseDlg` | `EditDialogBase<TraverseData>` | 388×289 → 380×299 | `GroupBox` → `SectionPanel` 3；`TableLayoutPanel` 6、`FlowLayoutPanel` 1 → 無外觀 `Panel` 5 | 確定 `Primary`、`CommonText` | 只有語系 |
| `CNCOffsetDialog`（`CncUI`） | `Form` → `DialogBase` | 300×232 → 320×231 | `PanelButton` 6 → `ThemedButton`；`Panel` → `SectionPanel`；`Label` 10 → `ThemedLabel` | 清除 `Danger` | 新增 `ApplyLanguage()`；按鈕陣列型別改 `ThemedButton` |
| `LoadingBarDlg` | `Form` → `DialogBase` | 480×140（程式建立） | — | — | 刪除自設的外框屬性與 `OnLoad` 自算置中（改由 `DialogBase`）；進度條 `Dock = Fill` 保留 |

`FormBase` 改繼承 `DialogBase`（原本繼承 `Form`），只保留 `Show( owner )` 的行為。

## 3.15 逐檔明細：主視窗與嵌入式面板

| 檔案 | 改動 |
| --- | --- |
| `StartupForm.cs` | 改繼承 `MainWindowBase`；設定與語系初始化移到 `MyApp.Initialize()`；註冊 `DialogPlacement.AreaProvider`；約 140 行逐項套用語系改為 `ApplyLanguage( m_LanguageManager )`，只保留圖層提示的格式化文字；切換語系時重新 `ThemeFonts.Initialize` 與 `ApplyThemeFont()`；工具列「執行中」底色 `FromArgb( 233, 180, 159 )` → `ACCENT_TINT_STRONG`、預設 `SystemColors.Control` → `Color.Empty`；樹的 `ForeColor = Black` 刪除（由 `MultiSelectTreeView` 給）；`CNCPanel.GetPreferredSizeForDpi` → `CNCPanel.PREFERRED_SIZE`；逐項套用時當預設文字的 6 個常數（`SET_OFFSET`、`PROCESS_SETTINGS`、`NEED_PROCESS` 等）刪除，ID 不存在時改保留 Designer 上的文字；`this.m_TreeView` → `m_TreeView` |
| `StartupForm.Designer.cs` | 移除 9 處寫死底色、3 處字型、`AutoScaleMode`、68 處 `ImageTransparentColor`（Magenta）；右鍵選單圖示 24×24 → 20×20；控制項改名（`m_pan*` → `m_pnl*`、`m_tlp*` → `m_pnl*`、`m_btn*` 工具列鈕 → `m_tsb*`、`contextMenuStrip1` → `m_cmsMain` 等）；圖層色塊與加工圖例保留（資料色） |
| `StartupForm.resx` | `contextMenuStrip1` → `m_cmsMain` |
| `CNCPanel.cs` / `.Designer.cs` | 改繼承 `PanelBase`；`GroupPanel` 5 → `SectionPanel`、`PanelButton` 35 → `ThemedButton`（開始 `Start`、暫停 / 繼續 `Hold`、停止 `Stop`）、標題 `Label` 5 → 卡片標題；刪除 `DeviceDpi` 換算與 `OnDpiChangedAfterParent`；偏好尺寸 240×620（乘 DPI）→ `PREFERRED_SIZE` 288×756；內距改 `PANEL_MARGIN`；新增 `ApplyLanguage( Language )` |
| `CNCInfoPanel.cs` / `.Designer.cs` | 改繼承 `PanelBase`；`GroupPanel` 3 → `SectionPanel`、`PanelButton` → `ThemedButton`、標題 `Label` 2 → 卡片標題；尺寸 672×236 → 736×240，偏好尺寸改回傳 Designer 尺寸；刪除 `DeviceDpi` 換算；直式垂直排列改用 `PANEL_MARGIN` / `SECTION_GAP` / `INFO_ROW_HEIGHT` / `INFO_ROW_PITCH` / `CONTROL_GAP`，標題列內控制項不參與排列；加工狀態色 7 處 → `TextRole`；五軸色改具名資料色 `X/Y/Z/B/C_AXIS_COLOR`；新增 `ApplyLanguage( Language )` |
| `CNCAlarmPanel.cs` / `.Designer.cs` | 改繼承 `PanelBase`；`DataGridView` → `ThemedDataGridView`，目前 / 歷史警報色改 `CellRole = Error / Secondary`；標題 `Label` → `m_pnlAlarm` 卡片標題；清單位置改用 `SECTION_PADDING` / `CONTROL_GAP`；手寫欄位標題常數改由 `ApplyLanguage( Language )` 以欄名套用 |
| `VNCUserControl.cs` / `.Designer.cs` | 改繼承 `PanelBase`；Designer 移除 `AutoScaleMode` / `AutoScaleDimensions` |
| `MainLayout.cs` | 新增 `GetDialogArea`（直式時為 CNC 承載區以上的 CAM 區域）；改為純計算、不用 `TableLayoutPanel`（3.22） |

## 3.16 逐檔明細：其他產品程式

| 檔案 | 改動 |
| --- | --- |
| `MyCAM/Program.cs` | 建立 `StartupForm` 前呼叫 `MyApp.Initialize()` |
| `MyCAM/MyApp.cs` | `InitAppSetting()` 改為 `Initialize()`，並加入套用語系與 `ThemeFonts.Initialize`；刪除 `CalculateDialogCenterLocation`、`GetFormSizeInfo`、`GetMainFormScreen`、`FromSizeInfo` 與 `MAIN_FORM_NOT_INITIALIZED`（由 `DialogPlacement` 取代） |
| `MyCAM/Helper/FormShow/DialogShowHelper.cs` | `ShowCentered` 改用 `DialogPlacement.GetCenterLocation`；刪除 `MoveToCenter`、`EnsureDialogOnTop`（分支起點已無呼叫端） |
| `MyCAM/LogManager/LogHandler.cs` | 日誌色：錯誤 `FromArgb( 253, 99, 132 )` → `ERROR`、警告 `YellowGreen` → `WARNING`、一般 `Black` → `TEXT` |
| `MyCAM/Editor/ViewManager.cs` | 不加工節點色 `Gray` / `Black` → `TEXT_DISABLED` / `TEXT` |
| `MyCAM/Editor/Action/SelectObjectAction.cs` | 隱藏物件節點色 `LightGray` / `Black` → `TEXT_DISABLED` / `TEXT` |
| `ManualOrderAction.cs`、`SelectPathAction.cs`、`SimuEditor.cs` | 只加 `using CommonUI.Controls`（`MultiSelectTreeView` 移入 `CommonUI`） |
| `MyCAM/MyCAM.csproj` | 移除 `DebouncedNumericUpDown.cs`、`SupportPositionRow.cs`、`MultiSelectTreeView.cs` |
| `CncUI/CncUI.csproj` | 移除 `Helper\ResponsiveLayout.cs` |
| `LanguageManager/LanguageManager.cs` | 新增 `HasString`（3.5） |

## 3.17 逐檔明細：移入 `CommonUI` 的既有元件

與原檔比對，除 namespace 與 XML 文件註解外的差異：

| 元件 | 原位置 | 差異 |
| --- | --- | --- |
| `DebouncedNumericUpDown` | `MyCAM/Editor/Dialog` | 新增 `Font`、`ForeColor` 覆寫並標成不序列化（外觀由父容器繼承）；`ValueRejectedEventArgs` 拆成獨立檔；其餘邏輯不變 |
| `MultiSelectTreeView` | `MyCAM/Editor` | 建構子設 `BorderStyle.FixedSingle`、`ForeColor = TEXT`；選取底色 `SystemColors.Highlight` → `ACCENT_TINT_STRONG`；旗標節點 `new Font( Bold )` / `Red` → `ThemeFonts.Emphasis` / `ERROR`；選取邏輯不變 |
| `SupportPositionRow` → `RemovableValueRow` | `MyCAM/Editor/Dialog` | 範圍、小數位數、單位、刪除文字、序號色改由建構子傳入；新增靜態 `Arrange` 排列多列 |
| `ResponsiveLayout` | `CncUI/Helper` | 改為 `public`；標題列高度與標題列內控制項不縮放 |
| `LoadingBar` | `CommonUI/Controls` | 進度色 `FromArgb( 0, 142, 203 )` → `ACCENT` |
| `RoundedProgressBar` | `CommonUI/Controls` | 預設色 `Yellow` → `ACCENT`；軌道與外框 `White` → `DIVIDER` |
| `RoundDrawing` | `CommonUI/Controls` → `CommonUI/Theme` | 只有 namespace 與文件註解 |

刪除：`CommonUI/Controls/GroupPanel.cs`、`PanelButton.cs`（由 `SectionPanel`、`ThemedButton` 取代）。

## 3.18 逐檔明細：語系檔

除了 `RenameMap.txt` 的控制項改名（ID 跟著改）與共用按鈕 ID 的刪除（3.14）之外，ID 的異動如下（各語系相同）：

| 語系檔 | 刪除 | 新增 | 理由 |
| --- | --- | --- | --- |
| `CommonUI_<語系>` | — | 新檔：確定、取消、重設、預覽、關閉 | 共用按鈕文字 |
| `AutoSortPathDlg`、`ConnectDlg`、`LockMSAxisDlg` | `FormTitle` | 類別名稱 | 表單標題以類別名稱為 ID |
| `ConnectDlg` | `m_lblTips_Default` | `m_lblTips` | 以控制項名稱為 ID |
| `CNCPanel` | `m_lbl*Title` 5 | `m_pnlAuxiliary`、`m_pnlJog`、`m_pnlHome`、`m_pnlStrategy`、`m_pnlMachining` | 標題改為卡片標題 |
| `CNCInfoPanel` | `m_lblHeightGroupTitle`、`m_lblMachiningTitle` | `m_pnlCoordinates`、`m_pnlHeight`、`m_pnlMachining` | 同上 |
| `CNCThreePointDlg` | `DrawingPoints`、`WorkpiecePoints` | — | 步驟標題即卡片標題；檔名由 `CNCThreePointDialog` 改名 |
| `CalibrationREFDlg` | `m_lblX`、`m_lblY`、`m_lblZ` | — | 共用標題拆成各點的 `m_lblPnt*X/Y/Z`；X / Y / Z 各語系相同，不設 ID |
| `DesignSupportBracketDlg` | `m_lblSection*` 6、`m_lblX/YPositionsTitle`、`m_btnImportParams` | `m_pnlBase`、`m_pnlBottomPinSize`、`m_pnlBracketCount`、`m_pnlBracketHeight`、`m_pnlCrossFit`、`m_pnlXBracket`、`m_pnlYBracket` | 標題改為卡片標題；`m_btnImportParams` 沒有對應控制項 |
| `ExtendedGCodeDlg` | `IS_START_POINT`、`IS_END_POINT`、`IS_MIX_DATA`、`m_lblInsertPosition` | `IsStartPOINT`、`IsEndPOINT`、`IsMixData`、`m_lblInsertType` | 改為程式實際使用的 ID（修正簡中顯示繁中預設字）；標籤 ID 對應控制項名稱 |
| `GlassDXFImportDlg` | `m_lblNote` | — | 沒有對應控制項 |
| `LeadDlg` | `m_lblLeadIn`、`m_lblLeadOut` | `m_pnlLeadIn`、`m_pnlLeadOut` | 標題改為卡片標題 |
| `LockMSAxisDlg` | `m_chkLockMaster`、`m_chkLockSlave`、`m_lblMasterAngle`、`m_lblSlaveAngle`、`m_gbxRotaryDirection` | `Format_LockMaster`、`Format_LockSlave`、`Format_MasterAngle`、`Format_SlaveAngle`、`Format_RotaryDirection` | 含軸名的格式字串不可與控制項同名；檔名由 `LockABDlg` 改名 |
| `ManualTransformDlg` | `m_lblAxisPlane` | `m_lblAxisPlane_Point` | 執行時文字不可與控制項同名 |
| `OrderDialog` | `m_gbxContent_Sequential`、`m_gbxContent_Swap` | `m_pnlContent_Sequential`、`m_pnlContent_Swap` | 執行時文字跟著卡片改名 |
| `PathEditDlg` | — | `m_pnlMove` | 新卡片的標題 |
| `ThreePtTransformDlg` | — | `m_lblMachinePoint1～3` | 新增的點標題 |
| `ToolVectorDlg` | `m`、`m_gbxColorLegend`、`m_gpbCtrlPnt`、`m_gpbPnt`、`m_gpbStartPnt`、`m_lblLegendMaster`、`m_lblLegendSlave`、`m_lblMaster`、`m_lblSlave`、`m_lblOnePntToolVecAdjust`、`m_lblSetBtn`、`m_lblToolDirQuickSetBtn` | `m_lblCtrlPnt`、`m_lblPnt`、`m_lblStartPnt`、`m_lblInterpolateType`、`m_pnlMovePnt`、`m_pnlQuickSet`、`m_pnlSetting`、`m_pnlToolVecInfo` | 三個轉換區塊合併、標題改卡片標題；軸名由程式組成的 ID 刪除 |
| `StartupForm` | 不符命名的 ID 與 `m_btn*_ToolTip` 等 27 個 | 對應的 `m_tsb*` / `m_tsmi*` 項目名與 `<項目名>_ToolTip` 共 20 個 | 選單與工具列以項目名為 ID（含 `SlowDonw` → `SlowDown` 拼字修正）；沒有對應項目的 ID（如 `m_tsAddPathForHat`、`m_tsbSelPath_Manual`）刪除 |

## 3.19 逐檔明細：資源

1. `MyCAM/Resource/*.png`：60 個工具列與選單圖示重繪為透明背景 PNG，檔名不變。
2. 新增 `MyCAM/Resource/IconSource/IconDefinitions.ps1`（圖示的向量定義）與 `RenderIcons.ps1`（產生 PNG）。

## 3.20 逐檔明細：指引與文件

1. `.github/instructions/ui-style.instructions.md`：新增，規範定稿。
2. `.github/copilot-instructions.md`、`CLAUDE.md`：指引目錄加入 UI 風格一列與 `@` 引用。
3. `Doc/UIStyleUnify/`：本議題報告（README 與第 1～6 章）與議題限定測試 `Tests/`；施工期間的
   `Doc/UIStyle{Draft,Notes,Review,CheckReport,Handoff}.md` 已整併進本報告後刪除。
4. `.github/instructions/project-overview.instructions.md`：多語系一節不再寫死語系數量，改為「`Language/` 中所有語系」。

## 3.21 第三輪 Review 的調整（2026-11-19）

1. 文字標籤只留 `ThemedLabel`（4.7 決議）：
   1. 28 個對話框的 `.Designer.cs` 中原生 `Label` 201 個改為 `ThemedLabel`（只換型別，`Role` 為預設的 `Body`，外觀不變）；
      本章前面寫「→ `Label`」的唯讀座標欄位，現在也是 `ThemedLabel`。
   2. `CommonUI`：`LoadingBar` 的兩個標籤、`RemovableValueRow` 的序號標籤改為 `ThemedLabel`；`ThemedLabel` 的類別說明改為「唯一的文字標籤」。
   3. `MyCAM/LogManager/LogHandler.cs`：日誌列改為 `ThemedLabel`，錯誤 / 警告 / 提示以 `Role = Error / Warning / Body`
      表示，刪除三個顏色欄位。
2. 圖層色單一定義（4.7 決議）：
   1. 新增 `MyCAM/Editor/PathLayerColor.cs`：圖層色清單（OCCT 色名，索引 0 為不加工、1～10 為圖層）、3D 路徑色、
      工具列色塊色（以 sRGB 換算）與色塊文字色（依對比度在 `TEXT` 與 `TEXT_ON_ACCENT` 間選擇）。
   2. `ViewManager.cs`：刪除 `TECH_Layer_Color_List`，路徑色改由 `PathLayerColor.GetPathColor` 取得；3D 顏色不變。
   3. `StartupForm.Designer.cs`：刪除 `m_tsbLayer1～10` 的 `BackColor` / `ForeColor`；`StartupForm.cs` 新增
      `InitLayerSwatches()`，建構時從 `PathLayerColor` 套用。工具列色塊因此與 3D 路徑同色（原本例如 `Orange` 對
      `DARKORANGE2`、`Sienna` 對 `BROWN`、`Green` 對 `GREEN3`）。
   4. `StaticExceptions.txt`：S19 例外由 `m_tsbLayer*` 縮小為 `m_tsbLayerNone`（「不加工」圖例）。
3. 檢查腳本 `CheckStatic.ps1`：
   1. 白名單與前綴表刪除原生 `Label`、`DataGridView`；S23 另檢出程式中 `new Label` / `new DataGridView`。
      反向測試：把 `OverCutDlg` 的標籤暫時改回原生 `Label`，S1、S9 報錯；在程式中加 `new Label()`，S23 報錯；還原後通過。
   2. 語系清單改為從 `Language/` 的檔名自動取得，新增語系時 S6 自動涵蓋。
4. 規範（`ui-style.instructions.md`）：
   1. 新增 3.2 容器的選用（`PanelBase` / `SectionPanel` / `Panel` / `TableLayoutPanel`）、4.7 精靈的三種形式（3.23 改為兩型）。
   2. 4.1 補 `FormBase` / `EditDialogBase<T>` 的行為說明；4.2 拆為「基底自動處理」與「撰寫時須遵守」；
      4.3 寫明置中包含在開啟流程中，以及執行時只改高度與換頁兩種情況的處理。
   3. 確定鍵改寫為共用文字依語系使用慣用詞；`RoundedProgressBar` 寫全名；第 6 節圖層色改由 `PathLayerColor` 定義。
   4. 刪除以「改動前 / 維持既有 / 不變」描述的句子（屬本議題施工的參考，不是規範），改寫為規則本身。

## 3.22 主視窗移除 `TableLayoutPanel`（2026-11-19）

依 4.7 決議與 4.8 的規劃施工。

1. 建立基準：`MainWindowRunner.cs` 新增 `[region]` 輸出（各承載區在主視窗客戶區的座標）；改動前以
   `Doc/UIStyleUnify/Tests/CompareMainLayout.ps1 -Update` 記錄 CAD / CAM / CNC 頁面 × 橫式 1920×1040、1366×768 ×
   直式 1080×1880、900×1600，共 12 個情境，存為 `MainLayoutBaseline.txt`。
2. `MyCAM/Layout/MainLayout.cs`：
   1. 新增 `Calculate`（只處理尺寸、不碰控制項），回傳新類別 `MainLayoutRegions`（各區矩形，座標相對於所在容器）。
   2. `Update` 依計算結果設定各區 `Bounds`、`Visible` 與所屬容器；不再使用列 / 欄樣式與跨欄，CNC 承載區也不再 `Dock = Fill`。
   3. 比例改為具名常數（樹欄 22% / 15%、CNC 欄放大 1.1、底部列 15% / 25% / 至少 1/4、直式 54% / 6% / 其餘、
      直式 CNC 區左側 55%、警報 20%、橫式日誌 40%），區域間距為 `HOST_MARGIN`（3，原本 `Panel` 的預設 `Margin`）。
   4. 捨入照 `TableLayoutPanel` 原本的結果：每個百分比各自無條件捨去，直式 CNC 區取剩餘高度。
   5. 刪除直式 CNC 承載區的 `SizeChanged` 處理：大小只由 `Update` 設定，同一次呼叫中已排好其中的面板
      （`BehaviorExceptions.txt` 登錄 B8）。
3. `StartupForm.Designer.cs`：
   1. `m_pnlBackGround`、`m_pnlBottom` 由 `TableLayoutPanel` 改為 `Panel`；刪除列 / 欄樣式、跨欄、各承載區的
      `Dock = Fill` 與 `Margin`，以及 `m_pnlLog` 殘留的跨欄設定。
   2. 客戶區 1596×1055 → 1920×1009；各承載區寫成 `Calculate` 在橫式加工頁的結果。直式 CNC 承載區與其警報區大小為 0。
   3. 刪除第二、三層工具列容器（`m_tscLevel2Container`、`m_tscLevel3Container`）：兩個都是空的，每個頁面只用一層工具列，
      執行時從不顯示；刪除後 Designer 中背景從工具列下方開始，與執行時相同。`StartupForm.cs` 一併整理：
      1. `m_UIStatusDic` 由「狀態 → 工具列清單」改為「狀態 → 工具列」（每個清單原本都只有一項）。
      2. `RefreshToolStripLayout` 只在第一層容器中顯示該狀態的工具列；刪除逐層切換容器顯示的迴圈與 `m_ToolStripLevelList`。
      3. 刪除 `ReLayOutUIZorder`：只在容器顯示數量改變時呼叫，而顯示數量從不改變（建構時主視窗尚未顯示，
         其中的 `Visible` 讀值都是 false，不會呼叫 `SetChildIndex`），所以從未調整過 z-order。
      4. `DefaultUISetting` 刪除隱藏第二、三層容器的兩行。
   4. `StartupForm.cs` 建立 `MainLayout` 時多傳入樹與 3D 檢視的承載區。
4. 檢查：
   1. `CheckStatic.ps1`：主視窗白名單與前綴表刪除 `TableLayoutPanel`（S19）。
   2. `MainWindowRunner.cs` / `CaptureMainWindow.ps1` 新增 P6（`-DesignerLayout`）：以 Designer 的客戶區大小開加工頁，
      各承載區的 `Bounds` 必須等於 Designer 的 `Location` / `Size`，不符時列出應寫入的值；`UICheck.bat runtime` 加跑一次。
   3. 新增本議題限定的 `CompareMainLayout.ps1`，加入 `RunIssueChecks.bat`。
5. 規範：第 3 節不可使用加入 `TableLayoutPanel`（主視窗也不用）、3.2 刪除 `TableLayoutPanel` 列、
   第 6 節第 3 點改寫為 `MainLayout` 的計算與 Designer 規則、第 8 節加 P6。

## 3.23 功能工具列容器與精靈兩型（2026-11-20）

依 4.9 決議施工。

1. 功能工具列容器：
   1. `StartupForm.Designer.cs`：`m_tscLevel1Container`（`ToolStripContainer`）改為無外觀的 `Panel` `m_pnlToolBar`，
      位置與大小不變（`Dock = Top`、1920×75），五條功能工具列照舊 `Dock = Fill`。原本 Designer 中上方空的工具列
      停靠區佔 25 px、工具列只剩 50 px，執行時停靠區高度為 0；改為 `Panel` 後 Designer 與執行時相同。
   2. `StartupForm.cs`：`RefreshToolStripLayout` 改為只走訪 `m_pnlToolBar` 中的工具列；刪除走訪四個停靠區與內容區的
      `GetAllToolStrips` / `FindContainerAllToolStrips`，以及多餘的 `m_tscLevel1Container.Visible = true`（容器從未隱藏）。
   3. 使用者以 Visual Studio Designer 開啟並儲存過 `StartupForm`，`StartupForm.Designer.cs` 的宣告順序、`m_tsLayer` 等
      控制項的大小與 `StartupForm.resx` 的元件匣位置隨之重新產生；縮排改回 tab，其餘保留 Designer 的輸出。
2. 精靈：
   1. `CommonUI.Layout.SectionStack` 新增 `ShowPage( form, page, pageArray )`：切換期間隱藏對話框、隱藏其他頁、
      對話框大小配合該頁、顯示該頁並重新置中；`FitToPage` 改為內部方法。
   2. `DesignSupportBracketDlg.ShowPanel` 改為呼叫 `SectionStack.ShowPage`，步驟與原本的程式相同，只是移到 `CommonUI`。
3. 檢查：
   1. `CheckStatic.ps1`：主視窗白名單與前綴表刪除 `ToolStripContainer`（S19、S9）；換頁精靈的辨識改為 `SectionStack.ShowPage(`。
   2. `BehaviorExceptions.txt` 登錄 `m_tscLevel1Container` 的型別替換（B6）、`ShowPage` 中的 `Show()`（B7）與頁的顯示切換（B8）。
4. 規範：
   1. 第 3 節白名單刪除 `ToolStripContainer`、列入不可使用；3.2 `Panel` 用途加入功能工具列區；第 6 節第 3 點加功能工具列區。
   2. 4.7 改寫為展開式 / 切換式兩型，切換式再依各步驟大小分固定大小與換頁，並列出所有精靈共通的規則；
      4.3、4.4 第 6、7 點、4.6、9.4 改用兩型的名稱與 `ShowPage`。
5. 後續議題（6.2）：步驟流程元件。

## 3.24 Designer 的協作方式（2026-11-20）

依 4.10 決議施工。

1. `Tools/UICheck/DesignerParser.ps1`：
   1. `InitializeComponent` 的範圍改以大括號配對找出（略過字串），不再依賴 tab 縮排；空白縮排的檔案也能正確解析。
   2. 新增 `Get-DesignerStatements`：逐一取出陳述式並正規化空白（`Size( 54, 23 )` 與 `Size(54, 23)` 相同）。
2. `Tools/UICheck/DesignerDiff.ps1`（新增）：比對兩個版本（預設工作目錄對 `HEAD`）的 `.Designer.cs` / `.resx`，
   列出控制項（增刪、型別、父容器、子項順序）、屬性、事件、其他陳述式與 resx 資源的增減；忽略縮排、宣告順序、
   版面暫停呼叫（`SuspendLayout` 等）、Designer 一律寫出的 `Name`、resx 的設計時資訊；只有格式不同的檔案標示
   `formatting only`。是報告，不是檢查。
3. `Tools/UICheck/CheckStatic.ps1`：新增 S24，`InitializeComponent` 只能有 Designer 讀得懂的寫法。檢出：
   不以 `this.` / `((` 開頭的陳述式（流程控制、沒有 `this.` 的欄位或呼叫）、lambda / 匿名方法 / null 運算子、
   字串插補、類別常數、呼叫畫面自己的方法；Designer 自己宣告的區域變數（儲存格樣式、資源）允許。
4. 規範：
   1. 專案概觀「WinForms Designer」改寫為四個階段（撰寫、檢視、微調、review）與三條原則。
   2. UI 規範第 8 節規則表加 S24、第 5 點（Designer 檔格式不審查）；8.1 加 `DesignerDiff.ps1`。
5. `StartupForm.Designer.cs` / `.resx`：保留使用者實測時 Designer 存出的內容（`AddRange` 續行為空白、
   設定選單項目寬度 158、resx 重複的 `TrayLocation`）。
6. 後續議題（6.2）：評估改用 WPF。

## 3.25 規範 review 第一批（2026-11-20）

決議見 4.11。

1. 移除未使用的右鍵選單：
   1. `MyCAM/StartupForm.Designer.cs` / `.resx`：刪除 `m_cmsMain`（空的、沒有掛到任何控制項）與其 `TrayLocation`。
   2. `CommonUI/Forms/MainWindowBase.cs`：刪除右鍵選單的語系套用與字型設定。
   3. `Tools/UICheck/CheckStatic.ps1`：白名單與前綴表移除 `ContextMenuStrip`；`MainWindowRunner.cs` 收集項目時不再走訪右鍵選單。
2. 新增 `.github/instructions/icon-design.instructions.md`（圖示設計），登錄於 `copilot-instructions.md` 指引目錄與 `CLAUDE.md`。
3. `MyCAM/Resource/IconSource/RenderIcons.ps1`：色票註解改為「圖示繪圖色，`a`、`r` 等於 `ACCENT`、`ERROR`」。
4. UI 規範：
   1. 2.2 第 1 點改寫選單與工具列字型的說明。
   2. 第 3 節白名單移除 `ContextMenuStrip`，`ToolTip` 註明只用於可能被截斷的執行時文字。
   3. 4.6 補上 `SectionStack.Arrange` 的 Designer 規劃。
   4. 6.7 改為引用圖示設計指引；第 7 節移除右鍵選單。
