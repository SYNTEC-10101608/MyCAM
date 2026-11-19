---
applyTo: "**/*.cs"
---

# UI 風格

適用於 `MyCAM`、`CncUI`、`CommonUI` 的所有畫面（主視窗、嵌入式面板、對話框、進度視窗）。
另須遵守 [通用規則](coding-style-general.instructions.md) 與 [C#](coding-style-csharp.instructions.md)。
本規範由議題 AUTO-23254（UI 風格統一）制定。

不在本規範範圍：`MessageBox`、開檔 / 存檔對話框、授權視窗（`FALicenseChecker`）。

## 1. 總則

- **外觀與行為分開**：外觀（字型、顏色、外框、間距、位置、圖示）只來自 `CommonUI` 的主題與元件；
  行為（事件、預覽 / 確定 / 取消、`Show` / `ShowDialog`、顯示 / 隱藏的時機）由各畫面與其行為基底負責。
- 只改外觀時，使用者能做的操作、操作順序與結果、視窗是否強制回應，都必須與改動前相同。
- **Designer 如實呈現**：在 Designer 看到的就是執行時的樣子。外觀設定寫在基底與元件的建構子裡，
  不序列化進 `.Designer.cs`；衍生畫面在 `InitializeComponent()` 之後**不寫任何外觀程式**。
- 規格固定：1920×1080、縮放 100%（96 DPI）、淺色單一主題；驗收語系 zh-TW、zh-CN。
  不處理 DPI 縮放，不計算 `DeviceDpi / 96`。
- 所有控制項與畫面共用的工具定義在 `CommonUI`；`MyCAM`、`CncUI` 不自行定義控制項。
  每個元件的用途列在第 9 節；有相同或類似用途的需求，一律使用並維護該元件，不另做一份。

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
| `CNC_START` / `CNC_HOLD` / `CNC_STOP` | `#2E8452` / `#C26E0A` / `#C2413B` | CNC 加工鈕的框線與文字（3.1） |
| `CNC_START_TINT` / `CNC_HOLD_TINT` / `CNC_STOP_TINT` | `#E5F3EA` / `#FDF0DE` / `#FBE6E4` | CNC 加工鈕底色，各有 `_HOVER` / `_PRESSED` |

### 2.2 字型 `ThemeFonts` 與文字角色 `TextRole`

- 字族依介面語系：zh-TW `Microsoft JhengHei UI`、zh-CN `Microsoft YaHei UI`、其他 `Segoe UI`。
  `Program.Main` 在建立任何畫面前呼叫 `MyApp.Initialize()`（讀設定、設語系、`ThemeFonts.Initialize`）；
  Designer 中固定顯示 zh-TW 字型。`ToolStrip` 不繼承父容器字型，由 `MainWindowBase` 另外設定。

| `TextRole` | 字型 | 顏色 | 用途 |
| --- | --- | --- | --- |
| `Body` | 9pt | `TEXT` | 內文（預設） |
| `Emphasis` | 9pt 粗體 | `TEXT` | 軸名、需要強調的欄位 |
| `Title` | 10.5pt | `TEXT` | 主視窗模式頁籤 |
| `Value` | Consolas 10pt | `TEXT` | 持續變動的機台數值 |
| `Secondary` | 9pt | `TEXT_SECONDARY` | 單位、提示 |
| `Error` / `Warning` / `Success` | 9pt | 對應狀態色 | 狀態文字 |
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
| `PANEL_MARGIN` | 8 | 嵌入式面板四周邊距 |
| `INFO_ROW_HEIGHT` / `INFO_ROW_PITCH` | 22 / 26 | 嵌入式面板唯讀數值列的高與間距 |
| `BUTTON_HEIGHT` / `BUTTON_MIN_WIDTH` / `BUTTON_GAP` | 28 / 80 / 8 | 按鈕 |

## 3. 控制項白名單

`.Designer.cs` 與程式中建立的控制項只能是下列型別；欄位名稱為「前綴 + PascalCase 名稱」，一個型別只用一個前綴。

| 用途 | 型別 | 欄位前綴 |
| --- | --- | --- |
| 對話框基底 | `CommonUI.Forms.DialogBase`（直接或經 `FormBase` / `EditDialogBase<T>`） | — |
| 嵌入式面板基底 | `CommonUI.Forms.PanelBase`（不直接繼承 `UserControl`） | — |
| 卡片 / 區塊 | `CommonUI.Controls.SectionPanel` | `m_pnl` |
| 按鈕 | `CommonUI.Controls.ThemedButton`（`Kind` 見 3.1；CNC 另用 `Indicator`、`Active`） | `m_btn` |
| 非內文的文字 | `CommonUI.Controls.ThemedLabel` | `m_lbl` |
| 數值輸入 | `CommonUI.Controls.DebouncedNumericUpDown` | `m_num` |
| 可多選的樹 | `CommonUI.Controls.MultiSelectTreeView` | `m_tv` |
| 可增刪數值列 | `CommonUI.Controls.RemovableValueRow`（執行時建立） | — |
| 進度 | `CommonUI.Controls.LoadingBar`、`RoundedProgressBar` | `m_prg` |
| 唯讀清單 | `CommonUI.Controls.ThemedDataGridView`（以 `CellRole` 表示儲存格文字角色） | `m_dgv` |
| 原生（外觀由父容器繼承） | `Label` | `m_lbl` |
| | `CheckBox` / `RadioButton` / `ComboBox` / `CheckedListBox` | `m_chk` / `m_rdo` / `m_cmb` / `m_clb` |
| | `TrackBar` / `PictureBox` / `DataGridView`（欄 `DataGridViewTextBoxColumn`） | `m_trb` / `m_pic` / `m_dgv`（`m_col`） |
| 原生（只限非數值文字） | `TextBox`（IP、路徑、G-code、量測結果等） | `m_txt` |
| 原生（無外觀的分組） | `Panel`（只用來把 `RadioButton` 分組或承載執行時的區段，不設底色與框線） | `m_pnl` |
| 原生（元件） | `ToolTip` / `Timer` | `m_ttp` / `m_tmr` |
| 主視窗 | `MenuStrip` / `ToolStrip` / `ToolStripContainer` / `ContextMenuStrip` | `m_ms` / `m_ts` / `m_tsc` / `m_cms` |
| | `ToolStripButton` / `ToolStripMenuItem` / `ToolStripSeparator` | `m_tsb` / `m_tsmi` / `m_tss` |
| | `TableLayoutPanel`（只限 `MainLayout` 管理的區域） | `m_pnl` |

- 同一個值的標題與顯示值並列時，標題用一般名稱、顯示值加 `Value`（`m_lblPnt1X` 顯示「X」，`m_lblPnt1XValue` 顯示座標）。
- 前綴只用於畫面自己擁有的控制項；從其他類別傳入的參考（如各 Action 的 `m_TreeView`）依一般物件命名。

不可使用：`GroupBox`、`Button`、`NumericUpDown`、`FlowLayoutPanel`、對話框與嵌入式面板內的 `TableLayoutPanel`、
當數值輸入用的 `TextBox`、只顯示座標的 `TextBox`（改用 `Label`）。

### 3.1 按鈕種類 `ButtonKind`

CAM（對話框）與 CNC（嵌入式面板）對嚴重性的分級不同，各用自己的一組。停用時一律灰色，不分種類。

| 範圍 | `Kind` | 外觀 | 用於 |
| --- | --- | --- | --- |
| 共用 | `Normal` | 白底 | 其他動作：取消、重設、瀏覽、只把輸入歸零的「清除」、CNC 功能鈕 |
| 對話框 | `Primary` | 藍色實心 | 讓這個對話框或這一步結束、有確定語意的動作（確定、生成、下一步）；每個容器最多一個，沒有就不設 |
| 對話框 | `Danger` | 白底紅框紅字 | 可能造成資料遺失的動作：刪除、移除、清除已存在的資料 |
| CNC 面板 | `Start` | 淡綠底、綠框綠字 | 開始加工 |
| CNC 面板 | `Hold` | 淡橘底、橘框橘字 | 暫停、繼續 |
| CNC 面板 | `Stop` | 淡紅底、紅框紅字 | 停止 |

CNC 功能鈕的啟用狀態以 `Active`（藍色實心）或 `Indicator`（燈號）表示。

## 4. 對話框

### 4.1 繼承

```
Form
└─ DialogBase                外觀：外框、字型、底色、自動縮放關閉、置中、語系文字套用（ApplyLanguage）
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
| 語系文字 | 對話框在 `InitializeComponent()` 之後呼叫 `ApplyLanguage()`（第 7 節） |

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
  內容對稱的兩張卡片可左右並排（例：X / Y 方向的清單），兩張等寬、間距 8，合計寬度同其他卡片。
- 卡片內：左側標籤 `x = 10`；同一卡片的輸入欄左緣對齊；單位標籤緊接輸入欄右側 6。
- 卡片內的動作按鈕（只作用於該卡片，或須隨卡片一起停用）放在列上，高 23。
- RadioButton 群組、會切換 `Enabled` / `Visible` 的區域，用無外觀的 `Panel` 包住；`Panel` 內的列以其頂端為 0 起算。
- 列：有標題卡片第一列 `y = 36`、無標題 `y = 10`，之後每列 `+30`；標籤 `+4`、`CheckBox` / `RadioButton` `+2`。
- 卡片高 = 最後一列頂端 + 23 + 10。同一位置輪流顯示的步驟卡片（由 `SectionStack.Arrange` 擇一顯示）取其中最高者，
  切換步驟時下方卡片不跳動；並排的兩張卡片同高。
- 精靈換頁（4.6 `FitToPage`）：每一頁是放在 `(12, 12)` 的無外觀 `Panel`，頁內以頁的左上角為 `(0, 0)`、
  依本節規則排列卡片與按鈕列（按鈕列底即頁底），對話框大小在換頁時配合該頁。
- 按鈕列：在最後一張卡片下方 16，高 28、寬至少 80、間距 8，距底邊 12。
  主要動作（確定）在最右，取消在其左；輔助動作（重設、預覽、另存…）靠左，從 `x = 12` 開始。
  按鈕的數量與功能不因統一而改變。
- `ClientSize` = 內容寬 + 左右邊距 12；高 = 按鈕列底 + 12。
- 欄寬以 zh-TW、zh-CN 中較長的字串設計。
- Tab 順序依版面：同一容器內由上到下、同一列由左到右（`Tools\UICheck\FixTabOrder.ps1` 依座標重編 `TabIndex`）。
- 對話框程式不設定控制項的字型、顏色、位置與大小；執行時的版面變化只透過 4.6 的方法。

確定鍵的文字統一為「確定」（共用語系字串，第 7 節）；按鈕的功能不變。

### 4.5 區塊標題

- 語意上是「畫面區塊標題」的文字，一律是 `SectionPanel.Text`，不另放標題 `Label`、不用粗體或放大的 `Label`。
- 卡片的規格與實作不因所在的畫面類別（對話框、嵌入式面板）而不同，沒有例外；靜態檢查 S8 會檢出卡片標題列內
  充當標題的粗體標籤。
- 標題列右側可放只作用於該卡片的動作控制項（按鈕、`CheckBox`、狀態文字）：按鈕高 23、`y = 5`，`CheckBox` `y = 5`，
  標籤高 24、`y = 4`，皆垂直置中於 32 高的標題列；右緣對齊卡片內容的右緣。
- 嵌入式面板以 `ResponsiveLayout` 等比縮放時，有標題的卡片保持標題列高度與標題列內的控制項不縮放，只縮放
  標題列下方的內容；卡片內容第一列同樣從 `y = 36` 起。
- 只是需要強調的文字（軸名、欄位）用 `ThemedLabel` 的 `Emphasis`；持續變動的機台數值用 `Value`。

### 4.6 執行時才決定的外觀

| 情況 | 做法 |
| --- | --- |
| 狀態文字變色 | `ThemedLabel.Role = TextRole.Error` 等，不直接寫顏色 |
| 顯示 / 隱藏區段後重新排列 | `SectionStack.Arrange( ... )` + `SectionStack.PlaceButtonBar( ... )`，不在對話框內手算座標 |
| 精靈的目前步驟 | `SectionPanel.Highlighted = true`（標題列加強調底色） |
| 動態增刪的數值列 | `RemovableValueRow` |
| 精靈切換步驟頁（各頁是一個 `Panel`，放在 `(12, 12)`） | `SectionStack.FitToPage( this, page )` |
| 改變大小後重新置中 | `DialogPlacement.MoveToCenter( this )` |

## 5. 數值輸入

- 所有數值輸入一律是 `DebouncedNumericUpDown`，以 `Minimum` / `Maximum` / `DecimalPlaces` / `Increment`
  表達欄位的範圍；不在程式中 `double.TryParse` 文字框。
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

### 嵌入式面板

- 嵌入式面板（`CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`）以 `SectionPanel` 分區、`ThemedButton` 當按鈕，
  與對話框使用同一套 token、卡片與標題列規則（4.4、4.5），差別只有下列幾點。
- 邊距 `PANEL_MARGIN`（8），卡片間距 `SECTION_GAP`（8）；第一張卡片在 `(8, 8)`。
- 唯讀數值列（座標、高度、加工狀態）高 `INFO_ROW_HEIGHT`、間距 `INFO_ROW_PITCH`，第一列同樣從 `y = 36` 起；
  可輸入的列仍用 `ROW_PITCH`。
- Designer 尺寸就是面板的偏好尺寸（`GetPreferredSize` 回傳 Designer 尺寸）；欄寬須讓 zh-TW、zh-CN 文字在
  這個尺寸下完整顯示。
- 面板的大小由主視窗決定，內容以 `ResponsiveLayout` 依 Designer 座標等比縮放，或以 `Anchor` / `Dock` 延展
  （對話框仍不可）；卡片標題列的高度與其中的控制項不縮放，固定靠右。
- 直式時 `CNCInfoPanel` 的卡片改為垂直排列，排列只用本節與 2.3 的 token 計算。
- 不讀 `DeviceDpi`，不做 DPI 換算（第 1 節的規格固定 96 DPI）。

## 7. 多語系

- 控制項自己使用的文字、且一個 ID 只對應這個控制項：以**名稱**當 ID，由基底的 `ApplyLanguage` 走訪套用，
  ID 不存在時保留 Designer 上的文字。不逐一手寫套用。

  | 畫面 | 呼叫 | 時機 |
  | --- | --- | --- |
  | 對話框 | `ApplyLanguage()`（`DialogBase`） | 建構子的 `InitializeComponent()` 之後立即呼叫一次 |
  | 嵌入式面板 | `ApplyLanguage( languageManager )`（`PanelBase`） | 套用語系的方法中最先呼叫；該方法在建構子與切換語系時執行 |
  | 主視窗 | `ApplyLanguage( languageManager )`（`MainWindowBase`） | 同上 |

- 套用對象：標題類控制項（`Label`、按鈕、`CheckBox`、`RadioButton`、`SectionPanel`）、清單欄位標題（以欄名）、
  選單與工具列項目（以項目名；提示文字的 ID 為 `<項目名>_ToolTip`，含右鍵選單）。輸入框的內容不受影響。
  嵌入式面板是另一個畫面，各自套用自己的語系檔。
- 表單標題：以**類別名稱**當 ID，對話框由同一個呼叫套用。
- 多個控制項共用的文字、執行時才決定的文字：在程式中明確呼叫 `TryGetString`，且該 ID **不可**等於任何控制項名稱
  （避免與控制項名稱的 ID 混淆）。
- 共用的按鈕文字（確定、取消、重設、預覽、關閉）：在 Designer 設定 `ThemedButton.CommonText`，
  執行時一律取自 `Language/CommonUI_<語系>.xml`（優先於對話框自己的 ID）。
- 執行時才決定的文字在 `ApplyLanguage` 之後設定。
- Designer 上的文字寫 zh-TW；四個語系的 XML 同步維護。

## 8. 檢查腳本（`Tools/UICheck`）

本節的檢查是規範的一部分，持續維護；維護方式見 8.1。

| 指令 | 內容 |
| --- | --- |
| `Tools\UICheck\UICheck.bat` | 靜態檢查（`CheckStatic.ps1`，只讀原始碼，不需建置） |
| `Tools\UICheck\UICheck.bat runtime [截圖資料夾]` | 另外以 Debug 建置結果開啟所有對話框與 CNC 面板，zh-TW / zh-CN 各跑一次（`CheckRuntimeLayout.ps1`）；並以 CAD / CAM / CNC 頁面 × 橫 / 直式截取主視窗、檢查對話框置中（`CaptureMainWindow.ps1`） |

- 執行時檢查與主視窗截圖在**隱藏桌面**執行，視窗不會出現在螢幕上、不搶焦點；要看到視窗時加 `-Visible`。

| 規則 | 內容 |
| --- | --- |
| S1–S8 | 白名單、外觀屬性不序列化、token 取用、座標規則、區塊標題等（見 `CheckStatic.ps1` 開頭） |
| S9 | 控制項欄位前綴符合第 3 節（含主視窗） |
| S10 | `TabIndex` 依版面順序（不符時執行 `FixTabOrder.ps1`） |
| S11 | 對話框程式中設定控制項的字型、顏色、位置或大小 |
| S12 | 使用 `DeviceDpi` |
| S13 | 語系檔中形如控制項名稱、卻沒有對應控制項也沒被程式使用的 ID |
| S14 | 對話框在 `InitializeComponent()` 之後立即呼叫 `ApplyLanguage()`；有語系檔的嵌入式面板呼叫 `ApplyLanguage( languageManager )` |
| S15 | `CommonUI` 以外定義了控制項類別 |
| S16 | `Start` / `Hold` / `Stop` 用在 CNC 面板以外；同一容器有兩個以上 `Primary` |
| S17 | 按鈕列順序：輔助動作從左邊距、其餘從右邊距以 8 相接；`Primary` 最右、取消 / 關閉緊鄰其左、重設 / 預覽在左 |
| S18 | 卡片內各列第一個輸入欄左緣對齊；單位標籤（名稱以 `Unit` 結尾）在輸入欄右側 6 |
| S19 | 主視窗 Designer 的控制項型別與外觀屬性（圖層色塊、圖例為例外） |
| S20 | `static readonly Color` 常數必須登錄為資料色（`StaticExceptions.txt`） |
| S21 | `CommonUI` 的公開型別都列在第 9 節、所屬資料夾的小節；資料夾與 namespace 一致 |
| S22 | 卡片標題列內的控制項位置（4.5）：按鈕高 23、`y = 5`，`CheckBox` `y = 5`，標籤高 24、`y = 4`，最右者對齊內容右緣 |
| S23 | 程式中建立白名單以外的控制項（`Button`、`GroupBox`、`NumericUpDown`、`TextBox`…） |
| T1–T7 | 文字截斷、超出容器、重疊等；T6 檢查 `Load` 前後版面與 Designer 相同；T7 檢查控制項顯示語系檔的文字。精靈與多狀態畫面依 `LayoutRunner.cs` 的 `STATE_MAP` 逐一檢查每個狀態 |
| P1–P5 | 對話框置中（含 `ShowDialog` 開啟的）、CAM 區域、主視窗字族、主視窗中 CNC 面板以實際大小的文字與版面、選單與工具列項目顯示語系檔的文字 |

- 修改畫面後必須跑 `UICheck.bat`，全部通過才可提交；新增畫面或改動版面時，另跑 `UICheck.bat runtime`。
- 新增精靈或多狀態的畫面時，把各狀態加到 `STATE_MAP`。
- 共用 / 執行時的標題文字、`SectionStack` 排列的卡片、精靈的頁，由檢查自行辨識，不列例外。
  `StaticExceptions.txt` 只登錄資料色、承載單一遠端內容的視窗與已決議的例外，每行寫明理由。
- 產品程式（`MyCAM`、`CncUI`、`CommonUI` 等）不放為了檢查而寫的程式：不加只給檢查用的方法、屬性、旗標或分支。
  檢查需要的狀態，在檢查程式中呼叫產品本來就有的方法觸發（例：`STATE_MAP` 以 `SetCommandsEnabled` 呈現 CNC 連線後的按鈕），
  或建立實際的環境（例：連線 CNC 模擬器）。檢查程式只在執行期間放在建置輸出，結束即刪除。

### 8.1 維護

| 檔案 | 用途 |
| --- | --- |
| `UICheck.bat` | 入口。無參數：靜態檢查；`runtime [截圖資料夾]`：另跑執行時版面與主視窗（需先建置 Debug） |
| `CheckStatic.ps1`、`DesignerParser.ps1` | 靜態檢查 S1–S23 與 Designer 檔解析 |
| `CheckRuntimeLayout.ps1`、`LayoutRunner.cs` | 執行時檢查 T1–T7 |
| `CaptureMainWindow.ps1`、`MainWindowRunner.cs` | 主視窗截圖與 P1–P5 |
| `HiddenDesktopRun.cs` | 讓 runner 在隱藏桌面執行；兩個 `.ps1` 加 `-Visible` 改回一般桌面 |
| `SyncLanguage.ps1` | 執行時檢查前，把建置輸出中缺少或過期的語系檔從 `Language/` 複製過去 |
| `FixTabOrder.ps1` | 依版面重編 `TabIndex`（修正 S10 的工具，不是檢查） |
| `StaticExceptions.txt` | 資料色登錄、承載單一遠端內容的視窗、已決議的例外 |

- **規則與規範同步**：本規範的規則改了，`CheckStatic.ps1` / `LayoutRunner.cs` / `MainWindowRunner.cs` 的對應規則要一起改，
  並更新本節的規則表與腳本開頭的註解。新增規則時，故意做出違規的程式確認能檢出（反向測試），再還原。
- **畫面資料夾**：靜態檢查只掃 `CheckStatic.ps1` 的 `$SCREEN_DIRS`（Designer 檔）與 `$CODE_SCOPE`（程式碼）；
  `FixTabOrder.ps1` 也有一份 `$SCREEN_DIRS`。畫面放到新的資料夾時，兩處都要加。
- **執行時檢查自動涵蓋新畫面**：runner 以反射找出 `MyCAM.exe`、`CncUI.dll` 中所有對話框與嵌入式面板
  （`SKIP_TYPE_LIST` 除外）。建構子需要參數的畫面，runner 依參數型別產生預設值；無法開啟的畫面算失敗，不會被略過。
- **多狀態畫面**：`STATE_MAP` 的格式為「方法名 參數…」，以反射呼叫產品既有的方法。
  產品中被 `STATE_MAP` 呼叫的方法改名或改參數時，`STATE_MAP` 要一起改，否則該畫面會開啟失敗。
- **置中檢查的對話框**：`CaptureMainWindow.ps1` 的 `-Dialogs`（`Show`）與 `-ModalDialogs`（`ShowDialog`）各列幾個代表；
  開啟方式改變時同步調整。
- **例外**：`StaticExceptions.txt` 格式為「規則 類別 控制項 理由」，理由寫明決議出處（議題編號與日期）。
  新增資料色常數時登錄 S20。
- **元件目錄**：`CommonUI` 新增公開型別時，放進所屬類別的資料夾，並在第 9 節該類別的小節加一列（S21）；
  新增類別時，同時新增資料夾、namespace 與小節（小節標題寫 `` （`CommonUI.<類別>`） ``）。
- **語系檔**：執行時檢查讀的是建置輸出的語系檔；`SyncLanguage.ps1` 會先同步，平常開發改了 `Language/` 仍要跑 `CopyRef.bat`。
- **runner 的編譯**：兩個 `.ps1` 以 Visual Studio 的 Roslyn `csc` 把 runner 編譯到 `MyCAM\bin\<組態>`
  （才能以與 `MyCAM.exe` 相同的方式找到語系檔與相依組件），執行結束即刪除；
  runner 參考的組件改名或新增相依時，更新 `.ps1` 中的 `/r:` 清單。
- **截圖**：輸出到指定的截圖資料夾，對話框為 `<語系>\<畫面>[_<狀態>].png`，主視窗為
  `MainWindow_<語系>_<頁面>_<Landscape|Portrait>.png`（紅色虛線框為對話框置中的區域）；截圖資料夾不進版控。
- **已知限制**：對話框截圖用 `DrawToBitmap`，執行時才填入的文字顯示的是 Designer 文字；隱藏桌面上 3D 檢視區（OpenGL）
  是空白的，會出現 `wglMakeCurrent` 錯誤訊息，可忽略；`CNCDlg` 需連線，不由腳本開啟。
  嵌入式面板的卡片座標沒有靜態檢查（由 T1–T4、P4 在實際大小下檢查）；執行時才填入的文字在 zh-CN 下的寬度、
  CNC 連線中的狀態，需要假資料或實際連線確認。

## 9. CommonUI 元件目錄

畫面需要下列用途時，一律使用對應的元件；元件不夠用時擴充元件本身（加屬性或參數），不在畫面裡另做一份。

`CommonUI` 依下列五類分資料夾，資料夾即 namespace（`CommonUI\<類別>` 為 `CommonUI.<類別>`），一個類別一個小節。
新增元件時放進所屬類別的資料夾，並在該小節加一列（靜態檢查 S21）。

| 類別 | 內容 |
| --- | --- |
| `Theme` | 主題：色票、字型、尺寸 token，文字角色，元件共用的繪製 |
| `Controls` | 控制項，與只用於控制項屬性的列舉、事件參數 |
| `Forms` | 畫面的外觀基底 |
| `Layout` | 執行時的版面計算 |
| `Language` | 語系文字的套用與共用文字 |

### 9.1 主題（`CommonUI.Theme`）

| 元件 | 用途 | 使用方式 |
| --- | --- | --- |
| `ThemeColors` / `ThemeFonts` / `ThemeMetrics` | 色票、字型、尺寸 token（第 2 節） | 畫面不寫顏色、字型、魔術數字 |
| `TextRole` / `ThemeText` | 文字角色與其字型、顏色；`ThemeText.SetRole` 給非標籤控制項 | 見 2.2 |
| `ThemedToolStripRenderer` | 選單與工具列的繪製 | 由 `MainWindowBase` 設定，畫面不直接使用 |
| `RoundDrawing` | 圓角路徑繪製，供元件內部使用 | 畫面不直接使用 |

### 9.2 控制項（`CommonUI.Controls`）

| 元件 | 用途 | 使用方式 |
| --- | --- | --- |
| `SectionPanel` | 卡片 / 區塊；`Text` 是區塊標題，`Highlighted` 表示精靈目前步驟 | 版面規則見 4.4、4.5 |
| `ThemedButton` | 唯一的按鈕；外觀由 `Kind` 決定（3.1），共用文字用 `CommonText`，CNC 用 `Indicator` / `Active` | 不自訂按鈕顏色 |
| `ButtonKind` / `ButtonIndicator` / `CommonButtonText` | `ThemedButton` 的按鈕種類（3.1）、CNC 按鈕燈號、共用按鈕文字 | 在 Designer 設定 |
| `ThemedLabel` | 需要文字角色的標籤（單位、提示、強調、狀態、機台數值） | 在 Designer 設 `Role`；依狀態改變時改 `Role` |
| `DebouncedNumericUpDown` | 所有數值輸入；以 `Minimum` / `Maximum` / `DecimalPlaces` / `Increment` 表達範圍，連續輸入時延遲送出 `DebouncedValueChanged`；輸入被拒時送出 `ValueRejected`（參數 `ValueRejectedEventArgs`） | 不用 `TextBox` 輸入數值 |
| `MultiSelectTreeView` | 可多選的樹 | — |
| `ThemedDataGridView` | 唯讀清單；`CellRole` 表示儲存格文字角色 | — |
| `RemovableValueRow` | 執行時增刪的數值列（序號 + 數值框 + 單位 + 刪除）；範圍、小數位數、單位、刪除文字由呼叫端傳入 | 以 `RemovableValueRow.Arrange` 排列 |
| `LoadingBar` / `RoundedProgressBar` | 進度顯示（`LoadingBar` 含標題與階段文字，內部使用 `RoundedProgressBar`） | 進度視窗為 `LoadingBarDlg` |

### 9.3 基底（`CommonUI.Forms`）

| 元件 | 用途 | 使用方式 |
| --- | --- | --- |
| `DialogBase` | 所有對話框的外觀層：外框、字型、底色、置中、語系文字 | 直接或經 `FormBase` / `EditDialogBase<T>` 繼承；建構子在 `InitializeComponent()` 後呼叫 `ApplyLanguage()` |
| `PanelBase` | 嵌入式面板的外觀層：字型、底色、關閉自動縮放、語系文字 | 取代直接繼承 `UserControl`；套用語系時呼叫 `ApplyLanguage( languageManager )` |
| `MainWindowBase` | 主視窗的外觀層：字型、底色、工具列字型與繪製、語系文字（含選單與工具列項目） | 只給 `StartupForm`；套用語系時呼叫 `ApplyLanguage( languageManager )` |

### 9.4 版面（`CommonUI.Layout`）

| 元件 | 用途 | 使用方式 |
| --- | --- | --- |
| `SectionStack` | 對話框執行時的版面變化：`Arrange`（隱藏區塊後補位）、`PlaceButtonBar`、`FitToPage`（精靈換頁） | 對話框內不手算座標 |
| `DialogPlacement` | 對話框置中（4.3）；`MoveToCenter` 重新置中，`AreaProvider` 由主視窗註冊 | 不自行計算位置 |
| `ResponsiveLayout` | 嵌入式面板依 Designer 座標等比縮放，標題列不縮放 | 只用於嵌入式面板 |

### 9.5 語系（`CommonUI.Language`）

| 元件 | 用途 | 使用方式 |
| --- | --- | --- |
| `LanguageApplier` | 以名稱為 ID 套用語系文字：標題類控制項、清單欄位、選單與工具列項目（`ApplyToItems`） | 由三個基底的 `ApplyLanguage` 呼叫，畫面不直接使用 |
| `CommonTexts` | 共用按鈕文字（確定、取消、重設、預覽、關閉），取自 `Language/CommonUI_<語系>.xml` | 按鈕設 `ThemedButton.CommonText`，畫面不直接使用 |
