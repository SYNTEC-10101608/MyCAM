# UI 風格

新增或修改畫面（`*.Designer.cs`、`*.resx`、畫面的 `*.cs`）與 `CommonUI` 時讀本文件，與團隊的
[通用規則](../team/coding-general.md)、[C#](../team/coding-csharp.md) 一起遵守。
適用於 `MyCAM`、`CncUI`、`CommonUI` 的所有畫面（主視窗、嵌入式面板、對話框）。
不在範圍：`MessageBox`、開檔 / 存檔對話框、授權視窗（`FALicenseChecker`）。本規範由議題 AUTO-23254（UI 風格統一）制定。

本文件只寫原則與共識的特例；token 的數值、元件的用法、檢查規則的細節以程式與其註解為準。

## 1. 原則

1. **外觀與行為分開**：外觀（字型、顏色、外框、間距、位置、圖示）只來自 `CommonUI` 的主題與元件；
   行為（事件、預覽 / 確定 / 取消、開啟方式、顯示的時機）由各畫面與行為基底負責。改外觀不改變行為，改行為不改變外觀。
2. **Designer 如實呈現**：外觀設定在基底與元件的建構子，不寫進 `.Designer.cs`；Designer 看到的就是執行時的樣子。
   畫面程式不設定控制項的字型、顏色、位置與大小，執行時的版面變化只透過 `CommonUI.Layout` 與 `MainLayout`。
3. **共用的東西只有一份**：控制項與畫面共用的工具都在 `CommonUI`，依 `Theme`、`Controls`、`Forms`、`Layout`、
   `Language` 分資料夾（資料夾即 namespace）；`MyCAM`、`CncUI` 不定義控制項。以沿用既有元件為主，
   元件不夠用時擴充元件本身；需要新增元件時，先討論必要性。
4. **規格固定**：1920×1080、縮放 100%（96 DPI）、淺色單一主題。版面以 96 DPI 的像素設計，不做 DPI 縮放。
   驗收語系為 zh-TW、zh-CN，其他語系的 XML 同步維護。橫 / 直式只依主視窗外框的長寬比判斷（高 > 寬為直式）。

## 2. 主題

1. 顏色、字型、尺寸只取 `ThemeColors`、`ThemeFonts`、`ThemeMetrics` 的 token，程式中不寫色值、字型與版面數字。
2. 文字以角色表達（`TextRole`：內文、強調、機台數值、次要、狀態），不直接指定字型與顏色；依狀態改變時改角色。
3. 字族依介面語系，在建立任何畫面之前決定；Designer 固定顯示 zh-TW 字型。
4. 資料色（圖層、3D 物件、CNC 座標軸點）不是主題色：定義為具名常數並登錄（第 8 節），只用於色塊、圖例與 3D 物件。

## 3. 畫面分類與基底

| 類別 | 外觀基底 | 大小由誰決定 | 版面 |
| --- | --- | --- | --- |
| 主視窗 | `MainWindowBase` | 使用者 | 區域由 `MainLayout` 計算 |
| 嵌入式面板 | `PanelBase` | 主視窗 | Designer 版面，以 `ResponsiveLayout` 縮放 |
| 對話框 | `DialogBase` | 自己 | Designer 固定座標 |

1. 三類只差在上表三項；卡片、按鈕、數值框等元件在三類中規格與實作相同。
2. 外觀基底只有外觀，行為基底（`FormBase`、`EditDialogBase<T>`）只有行為，疊在 `DialogBase` 之上。
3. 對話框的外框統一為工具視窗：不可改大小、不出現在工作列。

## 4. 控制項與命名

畫面只使用下表的型別；新增型別需先討論，並同步修改檢查的白名單。

| 用途 | 型別 | 欄位前綴 |
| --- | --- | --- |
| 卡片 / 區塊、無外觀容器 | `SectionPanel`、`Panel` | `m_pnl` |
| 按鈕 | `ThemedButton` | `m_btn` |
| 文字標籤 | `ThemedLabel` | `m_lbl` |
| 數值輸入 | `DebouncedNumericUpDown` | `m_num` |
| 非數值文字輸入（IP、路徑、G-code 等） | `TextBox` | `m_txt` |
| 選項 | `CheckBox` / `RadioButton` / `ComboBox` / `CheckedListBox` | `m_chk` / `m_rdo` / `m_cmb` / `m_clb` |
| 樹 | `MultiSelectTreeView` | `m_tv` |
| 唯讀清單與欄 | `ThemedDataGridView`、`DataGridViewTextBoxColumn` | `m_dgv`、`m_col` |
| 進度 | `LoadingBar`、`RoundedProgressBar` | `m_prg` |
| 資料色的色塊 / 圓點 | `ColorSwatch` | `m_swt` |
| 其他 | `TrackBar` / `PictureBox` / `ToolTip` / `Timer` | `m_trb` / `m_pic` / `m_ttp` / `m_tmr` |
| 主視窗的選單與工具列 | `MenuStrip` / `ToolStrip` / `ToolStripButton` / `ToolStripMenuItem` / `ToolStripSeparator` | `m_ms` / `m_ts` / `m_tsb` / `m_tsmi` / `m_tss` |

1. 欄位名稱為「前綴 + PascalCase 名稱」，底線只用於分隔語意；控制項名稱就是語系 ID，屬於長期維護的契約。
   前綴只用於畫面自己擁有的控制項，從其他類別傳入的參考依一般物件命名。
2. 所有數值輸入一律是 `DebouncedNumericUpDown`，以範圍屬性表達欄位限制；只顯示、從不輸入的值用 `ThemedLabel`。
3. 畫面中有語意的區塊是 `SectionPanel`，區塊標題就是它的 `Text`，不另放標題標籤。
4. `Panel` 只當無外觀容器：同一容器內多組 `RadioButton` 的分組、一起切換 `Enabled` / `Visible` 的區域、精靈的頁、
   主視窗的區域。
5. 按鈕種類依 `ButtonKind`：對話框用 `Normal` / `Primary` / `Danger`，`Primary` 每個容器最多一個；
   `Start` / `Hold` / `Stop` 只用於 CNC 面板。

## 5. 對話框

1. 以 Designer 的固定座標排版，依 `ThemeMetrics` 的格線：卡片由上往下等寬排列，內容對稱的兩張卡片可左右並排；
   卡片內以固定列距排列，輸入欄左緣對齊。不使用 `TableLayoutPanel`、`FlowLayoutPanel`、`Dock`、`Anchor`。
2. 按鈕列在內容下方：主要動作在最右，取消在其左，輔助動作（重設、預覽…）靠左。
3. 欄寬以 zh-TW、zh-CN 中較長的字串設計；Tab 順序依版面由上到下、由左到右。
4. 開啟時由 `DialogPlacement` 置中：橫式在整個主視窗，直式在上半部 CAM 區域；開啟對話框的程式不自行計算位置。
5. 執行時的版面變化只透過 `SectionStack`：隱藏卡片後補位、移動按鈕列、精靈換頁。只改高度時位置不動；
   Designer 呈現全部卡片都顯示的狀態。
6. 精靈依「後面的步驟是否要對照前面步驟的內容」分兩型：
   1. 展開式：所有步驟同時顯示，以 `SectionPanel.Highlighted` 標示目前步驟。
   2. 切換式：只顯示目前步驟；各步驟大小相近時共用位置與按鈕列，差異大時每步一頁。
   3. 每一步是一張卡片，以區塊標題說明，不另放步驟標籤。

## 6. 主視窗與嵌入式面板

1. 主視窗的區域配置只由 `MainLayout` 計算與套用，各區是無外觀的 `Panel`；畫面程式不自行調整區域大小。
2. 主視窗的 Designer 呈現橫式加工頁（CNC 面板都顯示）的計算結果；改了比例、間距或 CNC 面板尺寸時同步更新 Designer。
3. 相鄰區域相距 `SECTION_GAP`；CNC 面板的卡片就是所在區域的可見邊緣，面板自己的邊距不疊加到間距上。
4. 嵌入式面板與對話框共用 token 與卡片規則，差別只有面板邊距與唯讀數值列的高度與間距（`ThemeMetrics`）。
   Designer 尺寸就是面板的偏好尺寸，執行時以 `ResponsiveLayout` 縮放：卡片之間與標題列不縮放，只縮放卡片內容。
5. 選單與工具列的字型與繪製由 `MainWindowBase` 負責；圖示依 [圖示設計](coding-icon-design.md)。

## 7. 多語系

1. 控制項自己的文字：控制項名稱就是 ID，表單標題的 ID 是類別名稱，由基底的 `ApplyLanguage` 套用，不逐一手寫：
   1. 對話框：建構子的 `InitializeComponent()` 之後立即呼叫。
   2. 嵌入式面板與主視窗：套用語系的方法中最先呼叫（建構時與切換語系時）。
2. 多個控制項共用、或執行時才決定的文字：在程式中以 `TryGetString` 取得，ID 不可等於任何控制項名稱，
   並在 `ApplyLanguage` 之後設定。
3. 共用的按鈕文字（確定、取消、重設、預覽、關閉）以 `ThemedButton.CommonText` 指定，集中在 `CommonUI` 的語系檔。
4. Designer 上的文字寫 zh-TW；`Language/` 中所有語系的 XML 同步維護。

## 8. 共識的特例

以下是已討論決議的特例，適用於該情況的所有畫面；靜態檢查的例外登錄在 `Tools/UICheck/StaticExceptions.txt`，
每行寫明理由與決議的議題編號、日期。

| 情況 | 處理 |
| --- | --- |
| 承載單一內容的視窗（`CNCDlg`、`LoadingBarDlg`、`VNCUserControl`） | 內容 `Dock = Fill`；`CNCDlg` 大小隨遠端畫面，直式時定位到 CNC 區域 |
| `CNCInfoPanel` 直式 | 卡片改為上下排列，是唯一依方向改排列的畫面 |
| 資料色 | 具名常數，登錄為 `DataColor` |
| `CNCThreePointDlg` 的工件點與誤差欄 | 維持 `TextBox`：空白代表「尚未輸入」；可為空的數值框完成後移除 |
| `ComboBox` | 暫時保留，之後改為 `RadioButton` |
| `ToolTip` | 只用於長度不固定、可能被截斷的執行時文字 |

## 9. Designer 檔

1. `.Designer.cs` 可以用 Designer 或直接改程式碼；`InitializeComponent` 只寫 Designer 讀得懂的寫法，
   需要流程控制、lambda 等的行為寫在建構子的 `InitializeComponent()` 之後。
2. Designer 存出的格式不審查、不整理；review 時看 `Tools/UICheck/DesignerDiff.ps1` 列出的語意差異。
3. 畫面的 `.resx` 只有設計時資訊；圖片引用專案資源檔（`Properties.Resources`），不設 `Localizable`。

## 10. 檢查（`Tools/UICheck`）

| 指令 | 內容 |
| --- | --- |
| `Tools\UICheck\UICheck.bat` | 靜態檢查：只讀原始碼，不需建置 |
| `Tools\UICheck\UICheck.bat runtime [截圖資料夾]` | 另以 Debug 建置結果開啟所有畫面，在 zh-TW、zh-CN 下檢查版面、數值框操作與主視窗 |

1. 何時必須通過見 [process-review.md](process-review.md) 的門檻；修改畫面的過程中可隨時執行。
2. 規則依檢查對象分四組，規則名稱與內容寫在各檔開頭：靜態（`CheckStatic.ps1`）、執行時版面（`LayoutRunner.cs`）、
   數值框操作（`ControlRunner.cs`）、主視窗（`MainWindowRunner.cs`）。
3. 規範改了，對應的檢查一起改；新增規則時故意做出違規，確認能檢出。
4. 產品程式不放為了檢查而寫的程式；檢查需要的狀態以反射呼叫產品既有的方法觸發（精靈與多狀態畫面登錄在
   `LayoutRunner.cs` 的 `STATE_MAP`）。
