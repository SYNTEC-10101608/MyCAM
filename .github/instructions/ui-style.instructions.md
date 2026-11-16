---
applyTo: "**/*.cs"
---

# UI 風格

適用於 `MyCAM`、`CncUI`、`CommonUI` 的所有畫面（主視窗、嵌入式面板、對話框、進度視窗）。
另須遵守 [通用規則](coding-style-general.instructions.md) 與 [C#](coding-style-csharp.instructions.md)。
設計緣由與盤點結果見 `Doc/UIStyleDraft.md`、`Doc/UIStyleNotes.md`。

不在本規範範圍：`MessageBox`、開檔 / 存檔對話框、授權視窗（`FALicenseChecker`）。

## 1. 總則

- **外觀與行為分開**：外觀（字型、顏色、外框、間距、位置、圖示）只來自 `CommonUI` 的主題與元件；
  行為（事件、預覽 / 確定 / 取消、`Show` / `ShowDialog`、顯示 / 隱藏的時機）由各畫面與其行為基底負責。
- 只改外觀時，使用者能做的操作、操作順序與結果、視窗是否強制回應，都必須與改動前相同。
- **Designer 如實呈現**：在 Designer 看到的就是執行時的樣子。外觀設定寫在基底與元件的建構子裡，
  不序列化進 `.Designer.cs`；衍生畫面在 `InitializeComponent()` 之後**不寫任何外觀程式**。
- 規格固定：1920×1080、縮放 100%（96 DPI）、淺色單一主題；驗收語系 zh-TW、zh-CN。
  不處理 DPI 縮放，不計算 `DeviceDpi / 96`。
- 所有控制項定義在 `CommonUI`；`MyCAM`、`CncUI` 不自行定義控制項。

## 2. 主題 token（`CommonUI.Theme`）

程式中不寫 `Color.FromArgb`、`Color.<名稱>`、`SystemColors.*`、`new Font( ... )`，一律取用 token。
資料色（圖層色、3D 物件色、CNC 座標軸點）不是主題色，定義為具名常數，只能用在色塊、圖例、3D 物件。

### 2.1 色票 `ThemeColors`

| Token | 色值 | 用途 |
| --- | --- | --- |
| `WINDOW` | `#F1F3F6` | 表單、面板底色 |
| `SURFACE` | `#FFFFFF` | 卡片、輸入框、按鈕 |
| `SURFACE_ALT` | `#F8F9FB` | 表頭、唯讀欄位、停用按鈕 |
| `BORDER` | `#D6DCE4` | 卡片框線 |
| `BORDER_STRONG` | `#BDC6D1` | 按鈕、輸入框框線 |
| `DIVIDER` | `#E4E8EE` | 表格格線、分隔線、進度條軌道 |
| `TEXT` | `#1E2936` | 內文 |
| `TEXT_SECONDARY` | `#5E6A79` | 單位、提示 |
| `TEXT_DISABLED` | `#A0A9B5` | 停用文字 |
| `ACCENT` / `ACCENT_HOVER` / `ACCENT_PRESSED` | `#0067AD` / `#005A99` / `#004C82` | 主要按鈕、選中、卡片標題短線 |
| `TEXT_ON_ACCENT` | `#FFFFFF` | 強調色底上的文字 |
| `ACCENT_TINT` / `ACCENT_TINT_STRONG` | `#E8F1F9` / `#CFE2F3` | 選取列 / 工具執行中 |
| `HOVER` / `PRESSED` | `#ECF0F5` / `#DFE5EC` | 白底控件的滑過 / 按下 |
| `ERROR` / `WARNING` / `SUCCESS` | `#C42B1C` / `#A86200` / `#107C41` | 狀態 |

### 2.2 字型 `ThemeFonts` 與文字角色 `TextRole`

- 字族依介面語系：zh-TW `Microsoft JhengHei UI`、zh-CN `Microsoft YaHei UI`、其他 `Segoe UI`。
  `Program.Main` 在建立任何畫面前呼叫 `ThemeFonts.Initialize( 語系 )`；Designer 中固定顯示 zh-TW 字型。

| `TextRole` | 字型 | 顏色 | 用途 |
| --- | --- | --- | --- |
| `Body` | 9pt | `TEXT` | 內文（預設） |
| `Emphasis` | 9pt 粗體 | `TEXT` | 軸名、需要強調的欄位 |
| `Title` | 10.5pt | `TEXT` | 主視窗模式頁籤 |
| `Value` | Consolas 10pt | `TEXT` | 持續變動的機台數值 |
| `Secondary` | 9pt | `TEXT_SECONDARY` | 單位、提示 |
| `Error` / `Warning` / `Success` | 9pt | 對應狀態色 | 狀態文字 |
| `SectionTitle` | 9pt 粗體 + 左側強調短線 | `TEXT` | 無法使用卡片標題的區塊標題（只限 `ThemedLabel`，見 4.5） |

- 靜態的文字角色：使用 `ThemedLabel`，在 Designer 設定 `Role`。
- 依狀態改變的文字角色：執行時改 `ThemedLabel.Role`；非標籤控制項呼叫 `ThemeText.SetRole( role, control )`。

### 2.3 尺寸 `ThemeMetrics`（px）

| 常數 | 值 | 說明 |
| --- | --- | --- |
| `DIALOG_MARGIN` | 12 | 對話框四周邊距 |
| `BUTTON_BAR_GAP` | 16 | 內容底部到按鈕列頂端 |
| `SECTION_GAP` | 8 | 卡片與卡片之間 |
| `SECTION_PADDING` | 10 | 卡片左右下內距；無標題卡片的上內距 |
| `SECTION_TITLE_HEIGHT` | 32 | 卡片標題列高 |
| `SECTION_CONTENT_TOP` | 36 | 有標題卡片的第一列頂端 |
| `INPUT_HEIGHT` | 23 | 輸入框、下拉選單高 |
| `ROW_PITCH` | 30 | 輸入列間距（兩列頂端距離） |
| `CONTROL_GAP` | 6 | 同列控件之間（標籤、輸入、單位） |
| `LABEL_OFFSET` | 4 | 標籤相對於列頂端的下移量 |
| `CHECK_OFFSET` | 2 | `CheckBox` / `RadioButton` 相對於列頂端的下移量 |
| `BUTTON_HEIGHT` / `BUTTON_MIN_WIDTH` / `BUTTON_GAP` | 28 / 80 / 8 | 按鈕 |

## 3. 控制項白名單

`.Designer.cs` 與程式中建立的控制項只能是下列型別。

| 用途 | 型別 |
| --- | --- |
| 對話框基底 | `CommonUI.Forms.DialogBase`（直接或經 `FormBase` / `EditDialogBase<T>`） |
| 嵌入式面板基底 | `CommonUI.Forms.PanelBase`（取代直接繼承 `UserControl`） |
| 卡片 / 區塊 | `CommonUI.Controls.SectionPanel` |
| 按鈕 | `CommonUI.Controls.ThemedButton`（`Kind`：`Normal` / `Primary` / `Danger`；CNC 用 `Indicator`、`Active`） |
| 非內文的文字 | `CommonUI.Controls.ThemedLabel` |
| 數值輸入 | `CommonUI.Controls.DebouncedNumericUpDown` |
| 可多選的樹 | `CommonUI.Controls.MultiSelectTreeView` |
| 可增刪數值列 | `CommonUI.Controls.RemovableValueRow`（執行時建立） |
| 進度 | `CommonUI.Controls.LoadingBar`、`RoundedProgressBar` |
| 唯讀清單 | `CommonUI.Controls.ThemedDataGridView`（以 `CellRole` 表示儲存格文字角色） |
| 原生（外觀由父容器繼承） | `Label`、`CheckBox`、`RadioButton`、`ComboBox`、`CheckedListBox`、`TrackBar`、`PictureBox`、`DataGridView` |
| 原生（只限非數值文字） | `TextBox`（IP、路徑、G-code、量測結果等） |
| 原生（無外觀的分組） | `Panel`（只用來把 `RadioButton` 分組或承載執行時的區段，不設底色與框線） |
| 主視窗 | `ToolStrip`、`MenuStrip`、`ToolStripContainer`、`ContextMenuStrip`、`TableLayoutPanel`（只限 `MainLayout` 管理的區域） |

不可使用：`GroupBox`、`Button`、`NumericUpDown`、`FlowLayoutPanel`、對話框與嵌入式面板內的 `TableLayoutPanel`、
當數值輸入用的 `TextBox`、只顯示座標的 `TextBox`（改用 `Label`）。

## 4. 對話框

### 4.1 繼承

```
Form
└─ DialogBase                外觀：外框、字型、底色、自動縮放關閉、置中、語系文字
   ├─ FormBase               行為：Show( owner )
   │  └─ EditDialogBase<T>   行為：Preview / Confirm / Cancel / Reset
   └─ 其他對話框
```

- 所有對話框（含 `CncUI` 的對話框、`LoadingBarDlg`）都繼承 `DialogBase`。
- `DialogBase` 只管外觀，不加入任何行為；行為的統一在 `FormBase` / `EditDialogBase<T>` 這一層處理。

### 4.2 `DialogBase` 固定的設定

| 項目 | 值 |
| --- | --- |
| 外框 | `FixedToolWindow`：不可改大小、不出現在工作列與 Alt+Tab、無最小化 / 最大化鈕、不顯示圖示 |
| `AutoScaleMode` | `None`（Designer 上的座標就是執行時的像素） |
| 字型 / 底色 / 文字色 | `TextRole.Body` / `WINDOW` / `TEXT` |
| 位置 | `OnLoad` 時置中（4.3）；`Show` 與 `ShowDialog` 都適用 |
| 語系文字 | `OnLoad` 時自動套用（第 7 節） |

這些屬性在 `DialogBase` 標成不序列化；`.Designer.cs` 不可出現 `FormBorderStyle`、`StartPosition`、`MaximizeBox`、
`MinimizeBox`、`ShowIcon`、`ShowInTaskbar`、`AutoScaleMode`、`AutoScaleDimensions`、`Font`、`BackColor`、`ForeColor`。

### 4.3 置中

| 主視窗 | 置中位置 |
| --- | --- |
| 橫式（寬 ≥ 高） | 整個主視窗的中央 |
| 直式（高 > 寬） | 上半部 CAM 區域的中央（主視窗頂端到 CNC 承載區頂端），不論 CNC 面板是否顯示 |

- 由 `DialogPlacement.GetCenterLocation( form )` 統一計算，超出螢幕工作區時往內收；CAM 區域由主視窗註冊的
  `DialogPlacement.AreaProvider` 提供。
- 執行時改變大小後要重新置中的對話框，呼叫 `DialogPlacement.MoveToCenter( this )`，不自行計算座標。
- 例外：`CNCDlg` 在直式時定位到 CNC 區域。

### 4.4 版面（Designer 固定座標）

不使用 `TableLayoutPanel`、`FlowLayoutPanel`、`Dock`、`Anchor` 排版（唯一例外：只承載單一內容的視窗，
如 `LoadingBarDlg` 的進度條、`CNCDlg` 的遠端畫面，可用 `Dock = Fill`）；座標依下列規則：

- 第一張卡片在 `(12, 12)`；卡片垂直排列，間距 8；同一對話框的卡片等寬。
- 卡片內：左側標籤 `x = 10`；同一卡片的輸入欄左緣對齊；單位標籤緊接輸入欄右側 6。
- 卡片內的動作按鈕（只作用於該卡片，或須隨卡片一起停用）放在列上，高 23。
- RadioButton 群組、會切換 `Enabled` / `Visible` 的區域，用無外觀的 `Panel` 包住；`Panel` 內的列以其頂端為 0 起算。
- 列：有標題卡片第一列 `y = 36`、無標題 `y = 10`，之後每列 `+30`；標籤 `+4`、`CheckBox` / `RadioButton` `+2`。
- 卡片高 = 最後一列頂端 + 23 + 10。
- 按鈕列：在最後一張卡片下方 16，高 28、寬至少 80、間距 8，距底邊 12。
  主要動作（確定）在最右，取消在其左；輔助動作（重設、預覽、另存…）靠左，從 `x = 12` 開始。
  按鈕的數量與功能不因統一而改變。
- `ClientSize` = 內容寬 + 左右邊距 12；高 = 按鈕列底 + 12。
- 欄寬以 zh-TW、zh-CN 中較長的字串設計。

確定鍵的文字統一為「確定」（共用語系字串，第 7 節）；按鈕的功能不變。

### 4.5 區塊標題

- 語意上是「畫面區塊標題」的文字，一律是 `SectionPanel.Text`，不另放標題 `Label`、不用粗體或放大的 `Label`。
- 例外：版面依設計座標等比縮放的嵌入式面板（`CNCPanel`、`CNCInfoPanel`，`ResponsiveLayout`），標題是卡片內的
  標籤，改用 `ThemedLabel.Role = SectionTitle`，外觀與卡片標題相同（強調短線 + 粗體）。
- 只是需要強調的文字（軸名、欄位）用 `ThemedLabel` 的 `Emphasis`；持續變動的機台數值用 `Value`。

### 4.6 執行時才決定的外觀

| 情況 | 做法 |
| --- | --- |
| 狀態文字變色 | `ThemedLabel.Role = TextRole.Error` 等，不直接寫顏色 |
| 顯示 / 隱藏區段後重新排列 | `SectionStack.Arrange( ... )` + `SectionStack.PlaceButtonBar( ... )`，不在對話框內手算座標 |
| 精靈的目前步驟 | `SectionPanel.Highlighted = true`（標題列加強調底色） |
| 動態增刪的數值列 | `RemovableValueRow` |
| 改變大小後重新置中 | `DialogPlacement.MoveToCenter( this )` |

## 5. 數值輸入

- 所有數值輸入一律是 `DebouncedNumericUpDown`，以 `Minimum` / `Maximum` / `DecimalPlaces` / `Increment`
  表達原本的範圍；不在程式中 `double.TryParse` 文字框。
- 只顯示座標、從不輸入的欄位用 `Label`（`AutoSize = false`、寬同輸入欄）。

## 6. 主視窗與嵌入式面板

- 主視窗繼承 `CommonUI.Forms.MainWindowBase`（字型、底色、關閉自動縮放、全域工具列繪製）；`StartupForm.Designer.cs`
  不寫字型與底色。唯一例外是圖層列色塊與「加工 / 不加工」圖例，屬資料色，留在 Designer。
- 主視窗的區域配置由 `MainLayout` 計算，維持既有的橫 / 直切換與比例；只調整純視覺的邊距。
- 選單與工具列使用 `ThemedToolStripRenderer`（由 `MainWindowBase` 設為 `ToolStripManager.Renderer`）；
  工具列「執行中」的底色取自 `ACCENT_TINT_STRONG`，取消時設回 `Color.Empty`，設定的時機不變。
- 樹與日誌的文字色：一般 `TEXT`、隱藏 / 不加工 `TEXT_DISABLED`、錯誤 `ERROR`、警告 `WARNING`。
- 截圖：`Tools\UICheck\CaptureMainWindow.ps1 -OutFile <png> -Width <寬> -Height <高> [-Language <語系>] [-Mode CAD|CAM|CNC]`。
  `-Mode` 以點選選單切換到 CAM 或加工（CNC）頁面後再截圖，預設為啟動後的 CAD 頁面。
  橫 / 直式只依主視窗外框的長寬比（`Height > Width`）判斷，與系統顯示方向無關；直式（1080×1880）在橫式螢幕上
  以超出螢幕的視窗大小截圖，不需旋轉螢幕。腳本同時檢查對話框是否置中在 4.3 的區域。
- 圖示：`ImageScalingSize` 功能工具列 40×40，選單、圖層列、視角列、右鍵選單 20×20；
  圖示為透明背景 PNG，由 `MyCAM/Resource/IconSource/RenderIcons.ps1` 產生，不使用 Magenta 透明色。
- 嵌入式面板的大小由主視窗決定，可用 `Anchor` / `Dock` 延展內容（對話框仍不可）。
- 嵌入式面板（`CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`）以 `SectionPanel` 分區、`ThemedButton` 當按鈕，
  與對話框使用同一套 token 與規則。

## 7. 多語系

- 控制項自己使用的文字、且一個 ID 只對應這個控制項：以**控制項名稱**當 ID，`DialogBase` 在 `OnLoad` 時自動套用
  （走訪所有子控制項，ID 不存在時保留 Designer 上的文字）。
- 表單標題：以**類別名稱**當 ID，同樣自動套用。
- 多個控制項共用的文字、執行時才決定的文字：在程式中明確呼叫 `TryGetString`，且該 ID **不可**等於任何控制項名稱
  （避免被自動套用覆蓋）。
- 共用的按鈕文字（確定、取消、重設、預覽、關閉）：在 Designer 設定 `ThemedButton.CommonText`，
  執行時一律取自 `Language/CommonUI_<語系>.xml`（優先於對話框自己的 ID）。
- 只套用「標題類」控制項（`Label`、按鈕、`CheckBox`、`RadioButton`、`SectionPanel`）；輸入框的內容不受影響。
- Designer 上的文字寫 zh-TW；四個語系的 XML 同步維護。
- 嵌入式面板（`PanelBase`）同樣不手寫逐一套用：呼叫 `LanguageApplier.Apply( this, languageManager )`。

## 8. 檢查腳本（`Tools/UICheck`）

| 指令 | 內容 |
| --- | --- |
| `Tools\UICheck\UICheck.bat` | 靜態檢查（`CheckStatic.ps1`）+ 行為不變比對（`CompareBehavior.ps1`，與 `master` 比較） |
| `Tools\UICheck\UICheck.bat runtime [截圖資料夾]` | 另外以 Debug 建置結果開啟所有對話框與 CNC 面板，zh-TW / zh-CN 各跑一次（`CheckRuntimeLayout.ps1`）；並以 CAD / CAM / CNC 頁面 × 橫 / 直式截取主視窗、檢查對話框置中（`CaptureMainWindow.ps1`） |

- 執行時檢查與主視窗截圖在**隱藏桌面**執行（`HiddenDesktopRun.cs`），視窗不會出現在螢幕上、不搶焦點，
  執行期間可照常操作電腦；隱藏桌面上 3D 檢視區（OpenGL）是空白的。要看到視窗時，對 `CheckRuntimeLayout.ps1` /
  `CaptureMainWindow.ps1` 加 `-Visible`。
- 經確認的例外列在 `StaticExceptions.txt` / `BehaviorExceptions.txt`，每行寫明理由；行為例外必須是已決議的項目。
- 修改畫面後必須跑 `UICheck.bat`，全部通過才可提交；新增畫面時一併確認執行時版面檢查。
