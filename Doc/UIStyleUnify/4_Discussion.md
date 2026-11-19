# 4. 討論紀錄

本章是本議題所有要求、待決問題與決議的歷史紀錄，依日期排列，只追加不改寫。
被後來的決議取代的項目保留原文，並標註取代它的決議；最終結果以 [2_Architecture.md](2_Architecture.md)、
[3_Implementation.md](3_Implementation.md) 與規範為準。

## 4.1 工作方式

每一輪依「盤點 → 草案與待決問題（附建議）→ 逐題定稿 → 施工 → 檢查」進行；新的要求或決議記在本章並標日期。

| 日期 | 階段 | 文件（已整併進本報告） |
| --- | --- | --- |
| 2026-11-15 | 盤點與設計草案 v0.2～v0.5、P0 定稿 | 原 `Doc/UIStyleDraft.md`、`Doc/UIStyleNotes.md` |
| 2026-11-16 | P1～P5 施工與驗收、使用者回饋 | 原 `Doc/UIStyleCheckReport.md`、`Doc/UIStyleHandoff.md` |
| 2026-11-16 | Reviewer 盤點、第一輪與第二輪決議、最終盤點 | 原 `Doc/UIStyleReview.md` |
| 2026-11-17 | 收官：文件與測試整理、最終盤點 | 本報告 |

草案版本：v0.2 對話框外觀合併為一類、數值輸入盤點、沿用舊分支的風格與圖示、Designer 如實呈現；
v0.3 外框的定義、只顯示座標的欄位改為標籤、語系 ID 以控制項名稱自動套用；v0.4 範圍限定為外觀、對話框基底與外框盤點、
置中納入外觀、共用控制項移到 `CommonUI`、Designer 固定座標、區塊標題盤點、驗收腳本；v0.5 補 `MainLayout` 的計算內容、
衍生對話框不寫外觀程式、所有對話框不可改大小且不出現在工作列、確認不用 `TableLayoutPanel`。

## 4.2 盤點時的現況（2026-11-15）

決議的依據，記錄施工前的狀態。

| 項目 | 現況 |
| --- | --- |
| 對話框基底 | `EditDialogBase<T>` 22 個、`FormBase` 3 個（`AxisTransformDlg`、`GlassDXFImportDlg`、`OrderDialog`）、直接繼承 `Form` 5 個（`AddPathDlg`、`ConnectDlg`、`CNCDlg`、`CNCThreePointDlg`、`CNCOffsetDialog`） |
| 外框 | `FixedToolWindow` 20 個；`FixedDialog` 5 個；`FixedSingle` 2 個；未設定（可拖曳改大小）`ConnectDlg`、`FlyCutDlg`、`StartPointDlg`。`AddPathDlg`、`ConnectDlg` 會出現在工作列 |
| 字型 | 9 種組合（Arial 9 / 9.75 / 11.25 / 12 / 14.25、Microsoft Sans Serif 9、Microsoft JhengHei UI 9、新細明體 9 / 12），多數吃系統預設；Arial 沒有中文字 |
| 縮放基準 | `AutoScaleDimensions` 9 種；33 個用 `Font` 縮放、4 個用 `None` |
| 容器與按鈕 | `GroupBox` 25 個、`GroupPanel` 10 個（只在 CNC）；`Button` 84 個、`PanelButton` 51 個（只在 CNC）；確定鍵文字 OK / Confirm / Sure / Done / Apply 五種 |
| 顏色 | 約 180 處寫死顏色，分散在 32 個檔案；強調藍有兩種 |
| 工具列 | `ImageScalingSize` 20 / 24 / 40 三種；圖示以 Magenta 當透明色 |
| 置中 | 三套做法；`AddPathDlg` 以 `Show` 開啟、`CenterParent` 無效；直式判斷看螢幕方向並把 Y 乘 0.5，與 `MainLayout` 不一致 |
| DPI | `CNCPanel`、`CNCInfoPanel`、`GroupPanel`、`PanelButton` 自算 `DeviceDpi / 96` |
| 數值輸入 | 防抖數值框 47 個；原生 `NumericUpDown` 8 個、當數值用的 `TextBox` 36 個；只顯示座標的 `TextBox` 27 個 |
| 語系 | 27 個畫面手寫 `ApplyResourcesToUI()`，合計 438 行，其中 143 行的 ID 不等於控制項名稱 |
| 區塊標題 | `GroupBox.Text` 25 個、放大字 `Label`（`LeadDlg`）、粗體 `Label`（`DesignSupportBracketDlg`、`CNCThreePointDlg`、`AddPathDlg`、CNC 面板） |
| 共用控制項 | `DebouncedNumericUpDown`、`MultiSelectTreeView`、`SupportPositionRow` 在 `MyCAM`；`GroupPanel`、`PanelButton` 在 `CommonUI` |

`MainLayout` 的計算（橫 / 直切換、樹欄寬 22% / 15%、CNC 面板位置、底部列高、直式 CNC 區 40%、日誌 / 警報 60% / 40%）
全部屬於版面行為，本議題不動。

## 4.3 要求與決議

| 日期 | 項目 | 內容 |
| --- | --- | --- |
| 2026-11-15 | 目標 | 對現有 UI 做完整盤點，訂出分類規劃與風格，再全面重構。現有 UI 只作為操作規格的參考；在操作語意不變的前提下，實作可以盡量重構。除了外觀一致，實作方式也要集中收斂。每一類 UI 要訂出建構規則：靜態時處理哪些事、動態時處理哪些事；能抽象共用的就共用，真的不行的再討論是否有必要分裂。 |
| 2026-11-15 | 規格限制 | 直式與橫式人機的解析度固定為 1920×1080、縮放 100%；實際維護的語系只有 zh-TW、zh-CN；淺色、現代、不失穩重，只做一套固定主題。 |
| 2026-11-15 | 對話框分類 | 參數編輯、互動選取、精靈、一般對話框的差別是操作邏輯，不是外觀。外觀上所有對話框歸為同一類，控制項與容器編排一律統一。 |
| 2026-11-15 | 數值輸入 | 所有數值輸入一律使用 `DebouncedNumericUpDown`，原生 `NumericUpDown` 與當數值用的 `TextBox` 都要改掉。 |
| 2026-11-15 | 版面建構 | 保留 Designer。Designer 看到的外觀必須如實反映執行時的樣子。 |
| 2026-11-15 | 字型 | 好做的話依語系切換字型。 |
| 2026-11-15 | `MessageBox` / 開檔對話框 | 本次不調整外觀。 |
| 2026-11-15 | 授權視窗 | 本次不納入。 |
| 2026-11-15 | 舊分支 `AUTO-TBD_f_UIStyleImprove` | 外觀風格（色票、字型、尺寸）與已設計好的圖示可直接沿用；實作不參考，因為當時只做到外觀統一，沒有做到實作收斂。 |
| 2026-11-15 | 只顯示座標的欄位 | `ThreePtTransformDlg`、`CNCThreePointDlg`、`CalibrationREFDlg` 中只顯示 3D 點選座標的 27 個 `TextBox` 改為標籤。施工前要先逐一盤點語意，確認從頭到尾都不會被輸入（包含執行時是否會切成可編輯）。 |
| 2026-11-15 | 語系 ID | 控制項自己使用的文字、且一個 ID 只對應一個控制項時，以控制項名稱當 ID，由基底自動套用。多個控制項共用、執行時才決定的文字，仍在程式中明確取字串。 |
| 2026-11-15 | 客製語系檔相容性 | 現階段不考慮；語系 ID 改名造成 `Custom/Language` 舊 ID 失效可以接受。 |
| 2026-11-15 | 範圍：只做外觀 | 凡是行為差異（對話框基底的事件機制、`Show` / `ShowDialog`、主視窗的狀態與流程邏輯等）一律不動，列入後續處理。每一項改動都要先判斷是外觀還是行為。 |
| 2026-11-15 | 對話框置中 | 置中只影響視覺位置、不影響操作規格，視為外觀，本議題統一處理。 |
| 2026-11-15 | 置中位置 | 橫式置中在整個主視窗；直式置中在上半部 CAM 區域的中間，因為 CAM 編程操作大多用不到下半部。 |
| 2026-11-15 | 外框 | 只影響外觀的部分才在本議題處理；會影響行為的要先確認。 |
| 2026-11-15 | 外框帶來的行為差異 | 盤點後確認應該統一：所有對話框都不可拖曳改大小，也都不出現在工作列與 Alt+Tab。 |
| 2026-11-15 | 版面容器 | 對話框與嵌入式面板不使用 `TableLayoutPanel`，改用 Designer 固定座標；主視窗由 `MainLayout` 計算的動態區域維持原樣。 |
| 2026-11-15 | Designer 與衍生類別 | 外觀邏輯全部收斂在基底與共用元件；`InitializeComponent` 之後不應再寫任何外觀邏輯。 |
| 2026-11-15 | 共用控制項 | `MyCAM` 內自己定義的控制項都要評估移到 `CommonUI`。 |
| 2026-11-15 | 區塊標題 | 語意上是「畫面區塊標題」的元素要盤點並統一處理，可參考舊分支的做法（改為卡片標題）。 |
| 2026-11-15 | 驗收腳本 | P5 驗收要比照舊分支建立檢查腳本，驗證最終結果是否符合設計原則。 |
| 2026-11-15 | P0 定稿 | 草案 v0.5 定稿為規範。施工時補訂：有標題卡片第一列 `y = 36`、標籤下移 4、`CheckBox` / `RadioButton` 下移 2；確定鍵文字統一為「確定」；工具列圖示分 40×40 與 20×20 兩種。 |
| 2026-11-16 | 施工中訂定的細節 | 1. 卡片內的動作按鈕高 23；只作用於該卡片、或須隨卡片一起停用的按鈕留在卡片內。2. RadioButton 群組與會切換 `Enabled` / `Visible` 的容器保留原名稱，改為無外觀 `Panel`。3. 共用文字、格式文字、與其他類別共用的 ID 留在程式中取字串；與控制項同名、但由程式組成的 ID 刪除或改名。4. 精靈的步驟標籤改為步驟卡片標題，目前步驟以 `SectionPanel.Highlighted` 表示。5. 嵌入式面板可用 `Anchor` / `Dock`。6. 唯讀清單用 `ThemedDataGridView`。 |
| 2026-11-16 | 「應用」「生成路徑」等按鈕 | 確定鍵統一為「確定」只套用在語意為「確定 / 完成」的按鈕；有明確動作的按鈕保留原文字。 |
| 2026-11-16 | 直式人機的驗證方式 | 橫 / 直式只依主視窗外框長寬比判斷，與系統顯示方向無關；不旋轉螢幕，改在橫式螢幕上把主視窗開到 1080×1880 截圖並檢查置中。 |
| 2026-11-16 | 語系修正 | `ExtendedGCodeDlg` 的三個 ID 改為程式使用的 ID；語系檔名改為與類別名一致（`LockMSAxisDlg`、`CNCThreePointDlg`）。 |
| 2026-11-16 | 主視窗截圖補頁面 | 使用者要求橫式與直式截圖除了 CAD 頁面，也要有 CAM 與 CNC（加工）頁面。`CaptureMainWindow.ps1` 加 `-Mode`，`UICheck.bat runtime` 改為語系 × 頁面 × 橫直式共 12 張。 |
| 2026-11-16 | CNC 面板按鈕對齊 | 使用者看截圖後提出：「參數設置」對齊第三欄按鈕；五個倍率按鈕撐滿到與下方按鈕同寬。 |
| 2026-11-16 | `ToolVectorDlg` 區塊合併 | 使用者要求：三個轉換區塊都屬於「控制點的移動調整」，合為一張卡片「控制點移動」；「起終點切換」移到「至起點 / 至終點」下方；插值方案圖例併入「單點姿態資訊」。起終點切換列以無外觀 `Panel` 包住，在原本的時機設定 `Enabled`，維持行為。附帶差異：沒有選取點時圖例顯示停用色。 |
| 2026-11-16 | 檢查在隱藏桌面執行 | 使用者要求截圖時視窗不要跳到前景。經驗證（截圖逐像素相同、使用者確認不受影響）定為固定機制；不採用 Windows 虛擬桌面。已知差異：主視窗 3D 檢視區為空白。 |
| 2026-11-16 | CNC 面板區塊標題加強調短線 | 新增 `TextRole.SectionTitle`，以標籤畫出與卡片標題相同的短線。**同日撤回，由下一列取代。** |
| 2026-11-16 | 卡片規格不因所在 UI 類別而分裂 | 使用者指出上一列與「能集中的地方盡量集中」衝突：卡片不受畫面類別影響。CNC 面板區塊標題一律改為 `SectionPanel.Text`，撤回 `TextRole.SectionTitle`；`ResponsiveLayout` 保持標題列固定 32 高；標題列右側可放只作用於該卡片的控制項；座標區的「程式座標 ⇄」移到標題列右側；縮放不設下限；靜態檢查禁止在標題列放粗體標籤。 |
| 2026-11-16 | Designer 驗證、圖例停用色、直式工具列 | 使用者在 Visual Studio Designer 確認顯示無問題（含泛型基底的對話框）。`ToolVectorDlg` 圖例停用色：接受。直式主視窗 CAM 工具列溢出：既有問題，與本議題無關。 |
| 2026-11-16 | 版面決定時機 | 確認理解：對話框的位置與比例在 Designer 決定，執行時唯一的變化是 `SectionStack`；`CNCPanel` 一份設計版面等比縮放；`CNCInfoPanel` 是唯一依螢幕方向改排列的畫面；`CNCAlarmPanel` 以 `Dock` / `Anchor` 拉伸。 |
| 2026-11-16 | 規範文件只寫最終版本 | 規範只描述最終決議，不寫演變、撤回或例外的過程；過程只記在討論紀錄。 |
| 2026-11-16 | 控制項欄位縮寫 | 全域統一：白名單加「欄位前綴」欄，一個型別一個前綴，所有不符的欄位改名，四語系 ID 同步改；靜態檢查依型別檢查前綴。改名後做整體驗證（建置、以改名對照表轉換後行為比對、語系 ID 一一對應）。 |
| 2026-11-16 | CNC 面板版面 | 本議題處理：卡片間距、內距、第一列改用 `ThemeMetrics`，移除 `DeviceDpi`；保留 `ResponsiveLayout` 與 `CNCInfoPanel` 直式改排列；規範寫明嵌入式面板的版面規則。 |
| 2026-11-16 | 檢查補強（第一輪） | `.cs` 的事件訂閱與 `Enabled` / `Visible` 賦值比對、`StartupForm` 納入、`TabIndex`、`InitializeComponent` 之後的外觀賦值、`DeviceDpi`、建構失敗算失敗、`Load` 前後版面比對。`TabIndex` 依新版面重排，視為外觀調整。 |
| 2026-11-16 | 字型初始化位置 | 使用者要求放在 `MyApp`：`MyApp.Initialize()`（讀設定、套用語系、`ThemeFonts.Initialize`），`Program.Main` 在建立 `StartupForm` 之前呼叫。修正 zh-CN 啟動時主視窗字族錯誤。 |
| 2026-11-16 | 第一輪施工結果 | 1. 欄位前綴定案：卡片、`Panel`、`TableLayoutPanel` 用 `m_pnl`，數值框 `m_num`；改名 304 筆，順帶修正拼字、刪除 9 個無用 ID。標題與顯示值並列時顯示值加 `Value`。2. 新增 `PANEL_MARGIN` 8、`INFO_ROW_HEIGHT` 22、`INFO_ROW_PITCH` 26；面板 Designer 尺寸即偏好尺寸（`CNCPanel` 288×756、`CNCInfoPanel` 736×240，原本 246 寬在設計尺寸下文字被截斷）。3. `ToolStrip` 字型由 `MainWindowBase` 設定。4. 檢查新增 S9–S13、B8、T6、P3、P4 與 `FixTabOrder.ps1`；`DesignSupportBracketDlg` 換頁改用 `SectionStack.FitToPage`。 |
| 2026-11-16 | 按鈕顏色分兩套 | CNC 對嚴重性與行為的分級和 CAM 不同，不用同一套。CAM：可能造成資料遺失的用紅色（`Danger`），讓這一步結束、有確定語意的用藍色（`Primary`）。CNC：功能啟用狀態維持藍色；加工控制開始綠、暫停與繼續琥珀、停止紅；停用一律灰色。 |
| 2026-11-16 | 自訂控件一律在 `CommonUI` | 使用者要求：所有自訂控件放在 `CommonUI`，規範寫明每個控件的用途，之後有類似需求一律使用、維護該控件。`RemovableValueRow` 的範圍與小數位數由呼叫端傳入；`ResponsiveLayout` 移入 `CommonUI`；規範新增元件目錄；靜態檢查禁止 `CommonUI` 以外定義控制項類別。 |
| 2026-11-16 | 語系套用改回建構子 | 先前因 `DialogBase` 建構子早於衍生類別的 `InitializeComponent()` 而改在 `OnLoad` 套用，代價是時機比改動前晚。決議：`DialogBase.ApplyLanguage()` 由每個對話框在 `InitializeComponent()` 之後呼叫，時機與改動前相同；與嵌入式面板一致。 |
| 2026-11-16 | 第二輪檢查補強範圍 | 使用者要求：盤點所有可用腳本處理的檢查缺口，直接補上。 |
| 2026-11-16 | 第二輪施工結果 | 1. `ButtonKind` 新增 `Start` / `Hold` / `Stop`；`FlyCutDlg`「移除」改 `Danger`、「新增」改 `Normal`。2. 30 個對話框呼叫 `ApplyLanguage()`，`DialogBase.OnLoad` 只做置中。3. `RemovableValueRow` 參數化。4. `ResponsiveLayout` 移到 `CommonUI`。5. 規範新增按鈕種類與元件目錄。6. 檢查新增 S14–S21、T7、精靈與 CNC 面板多狀態、執行前同步語系檔、`ShowDialog` 開啟的對話框置中；以反向測試確認 S14、S16、T7 能檢出問題。 |
| 2026-11-16 | CNC 加工鈕顏色減淡 | 使用者看截圖：沿用狀態文字色當按鈕底色太深；暫停 / 繼續偏咖啡色，期望偏橘。改為獨立的按鈕底色 token。**由下一列取代。** |
| 2026-11-16 | CNC 加工鈕改為淡色底 | 使用者認為實心底色仍太深。改為淡色底 + 同色框線與文字（與 CAM `Danger` 同類樣式）：開始淡綠 / 綠、暫停與繼續淡橘 / 橘、停止淡紅 / 紅。 |
| 2026-11-16 | 產品程式不留檢查用的程式 | 使用者要求：部署的程式不可有為了檢查而寫的程式碼；檢查需要的狀態寫在檢查程式裡，或建立實際環境觸發。盤點：產品專案新增的成員都由產品程式使用；runner 原本留在建置輸出，改為執行結束即刪除。規範寫入此規則。 |
| 2026-11-16 | 最終盤點（第二輪收尾） | 使用者要求：依全部討論盤點程式與文件，確認符合共識與規範；分類必須是必要的分類，分類內沒有例外；測試涵蓋全部範疇，產品程式不留測試用程式。處理：語系套用收斂為三個基底的 `ApplyLanguage`（`LanguageApplier` 擴充清單欄位與選單 / 工具列項目）；靜態檢查例外由約 50 行減為 15 行，改由檢查自行辨識執行時文字、`SectionStack` 卡片、精靈的頁；規範補步驟卡片同高、精靈每頁的版面、語系套用表；新增 S22、S23、P5。 |
| 2026-11-16 | 支撐架位置拆成兩張卡片 | 「支架位置」拆成「X 方向支架位置」「Y 方向支架位置」左右並排（上下排會超過螢幕高度）；「重設位置」移到該頁按鈕列左側。規範加「內容對稱的兩張卡片可左右並排」。 |
| 2026-11-16 | 對話框寬度 | 不訂級距，風格一致即可，寬度依內容決定。 |
| 2026-11-16 | 卡片內小標 | 不允許：語意是區塊就拆成卡片。 |
| 2026-11-17 | 收官的要求 | 使用者要求：進行 Review 與收官。產出文件整理為「專案指引」（最終共識與規範）與「議題研發報告」（目標、架構規劃、實作紀錄、討論紀錄、測試驗證、後續事項）；測試程式分為「本議題限定」（功能正確性與相容性）與「未來可持續使用」（規範與風格檢查，並在測試驗證文件說明維護注意事項）；對程式與文件做最終盤點，確認施工範圍符合共識與規範，測試涵蓋本議題全部範疇，產品程式不留測試用程式。 |
| 2026-11-17 | 報告形式 | 議題資料夾 `Doc/UIStyleUnify/`，一章一檔；原本五份 `Doc/UIStyle*.md` 整併後刪除。 |
| 2026-11-17 | 本議題限定測試的位置 | 放在議題資料夾 `Doc/UIStyleUnify/Tests/`；比對基準固定為分支起點 `a4b3826d`，合併後仍可重現；`Tools/UICheck` 只留可持續使用的檢查。 |
| 2026-11-17 | `CommonUI` 分類 | 盤點發現資料夾 / namespace（`Theme`、`Controls`、`Forms`）與元件目錄的分節（基底、控制項、版面與文字工具）不一致，例如 `SectionStack` 在 `Controls`、`DialogPlacement` / `LanguageApplier` 在 `Forms`。決議兩邊統一為五類：`Theme`、`Controls`、`Forms`、`Layout`、`Language`；搬檔並改 namespace，不改行為；S21 檢查三者一致。 |
| 2026-11-17 | XML 文件註解 | `CommonUI` 公開成員約 40 處缺少 XML 文件註解，多數在搬入的既有元件。決議 `CommonUI` 全部補齊（只加註解，不改程式），因為元件目錄要求之後一律使用並維護這些元件。 |
| 2026-11-17 | 本議題寫入的程式碼須符合通用規範 | 最終盤點發現本議題新增或改動的 C# 行約 60 處超過 120 字元、一處 `this.`、`ValueRejectedEventArgs` 與 `DebouncedNumericUpDown` 同檔。全部修正，並新增本議題限定的 `CheckBranchRules.ps1` 檢查。行長規則依 `.editorconfig`（`max_line_length` 只設在 `*.cs`）；Designer 檔為 Visual Studio 產生，不檢查行規則；既有程式碼原本就不符合的不在範圍。 |

## 4.4 待決問題與結論

| 日期 | 問題 | 結論 |
| --- | --- | --- |
| 2026-11-16 | `CNCThreePointDlg` 工件點 9 欄與誤差容忍欄是否改為數值框 | 這 10 欄以空白表示「尚未輸入」，程式依此判斷工件點是否完整。使用者先提出工件點應唯讀；查證後工件點可按「擷取」讀 CNC 校正位置，也可以手動輸入。盤點其他改成數值框的欄位，空白在改動前一律視為不合法，沒有「空白有意義」的語意，只有這個對話框有。**決議**：工件點不可改唯讀（使用者有時需要手動填入），本議題維持 `TextBox`；可為空的數值框列為後續議題。 |
| 2026-11-16 | CNC 面板內的區塊標題是否改為卡片標題 | 面板依設計座標等比縮放，程式也以標題 Label 的位置排版。**決議**：改為卡片標題，`ResponsiveLayout` 保留標題列固定高度（見 4.3「卡片規格不因所在 UI 類別而分裂」）。 |
| 2026-11-16 | 泛型基底在 Designer 的表現 | 22 個對話框繼承 `EditDialogBase<T>`。**已確認** Designer 顯示正常，不需中介類別。 |

## 4.5 Reviewer 盤點（2026-11-16）

範圍：`master..87b6ddff`（26 個 commit）。建置成功；靜態 0 問題、行為比對 0 差異。整體評價：基礎建設方向與草案一致、
對話框遷移完成度高；主要問題在「控制項命名縮寫沒有收斂」「CNC 嵌入式面板仍是另一套實作」「檢查涵蓋面比報告寫的窄」。

### 發現

| # | 發現 | 處理 |
| --- | --- | --- |
| 1.1 | 控制項欄位縮寫嚴重分裂，且遷移後名稱說的型別是錯的（`m_txb*` 是數值框、`m_tbx*` 是 Label、`m_gbx*` 是 Panel…） | 第一輪：全域改名 304 筆、S9 |
| D1 | 規範禁止 `DeviceDpi`，`CNCInfoPanel`、`CNCPanel`、`StartupForm` 仍在算 | 第一輪：移除、S12 |
| D2 | `ThemeFonts.Initialize` 實際在 `StartupForm` 建構子，晚於 `MainWindowBase` 設字型 | 第一輪：`MyApp.Initialize()` |
| D3 | 面板縮放時標題列控制項的 X / 寬仍被縮放 | 第一輪：標題列內控制項不縮放、固定靠右 |
| D4 | CNC 面板卡片內距、間距、第一列與對話框不同 | 第一輪：改用 `ThemeMetrics` |
| D5 | `DesignSupportBracketDlg` 自己算 `ClientSize` | 第一輪：`SectionStack.FitToPage` |
| D6 | `RemovableValueRow` 範圍寫死 | 第二輪：參數化 |
| D7、D8、D9、D10 | 例外理由過期、`TextBox` 白名單過寬、交接文件建置指令、驗收報告未同步 | 第一輪 / 第二輪直接修正 |
| D11 | 面板版面決定時機只寫在紀錄，規範沒寫 | 第一輪：規範第 6 節 |
| D12 | `CNCPanel.Designer.cs` 全檔空白縮排（master 即如此） | 施工中重寫該檔時已改為 tab 縮排（2026-11-17 確認） |
| 2.1 | zh-CN 啟動時主視窗字族不對 | 第一輪：D2 修正、P3 檢查字族 |
| 2.2 | CNC 面板仍是另一套版面實作 | 第一輪：Q2 |
| 2.3 | `Danger` / `Primary` 何時用、對話框寬度、卡片內小標沒有規則 | 第二輪：R2-1～R2-4 |
| V1～V5 | 行為比對不比對 `.cs`、不含 `StartupForm`、不含 `TabIndex`、語系時機、刪除容器的 `Enabled` / `Visible` 承接 | 第一輪補 V1～V3（B8、`StartupForm`、`TabIndex` 依版面重排）；V4 隨語系改回建構子消失；V5 列後續 |
| S-a～S-g | 靜態檢查缺口：外觀賦值、面板卡片、按鈕順序與對齊、主視窗 Designer、具名顏色繞過、`DeviceDpi`、命名 | S-a、S-f、S-g 第一輪（S11、S12、S9）；S-c、S-d、S-e 第二輪（S17、S18、S19、S20）；S-b 列後續 |
| R-a～R-g | 執行時檢查缺口：建構失敗只印 SKIP、`Load` 前後比較、多狀態、面板直式、執行時文字、主視窗只驗置中、置中路徑 | R-a、R-b、R-f 第一輪（建構失敗算失敗、T6、P3 / P4）；R-c、R-d、R-g 第二輪（`STATE_MAP`、P1 含 `ShowDialog`）；R-e 列後續 |

### 第一輪待決問題與決議

| # | 問題 | 建議 | 決議 |
| --- | --- | --- | --- |
| Q1 | 控制項縮寫是否在本議題統一並改名 | 本議題處理（名稱即語系 ID，之後改成本更高） | 依建議；panel 類一律 `m_pnl`、數值框 `m_num` |
| Q2 | CNC 面板版面統一到什麼程度 | 尺寸改用 `ThemeMetrics`、移除 `DeviceDpi`，保留 `ResponsiveLayout` | 依建議 |
| Q3 | 是否訂對話框寬度級距 | 建議 320 / 380 / 440 / 500 四級 | 第二輪決議不訂 |
| Q4 | 檢查缺口要補到哪裡 | 至少 V1、V2、V3、S-a、S-f、R-a、R-b | 依建議 |
| Q5 | 主視窗字族 | 本議題修並加檢查 | 依建議，初始化放在 `MyApp` |

### 第二輪待決問題與決議

| # | 問題 | 建議 | 決議 |
| --- | --- | --- | --- |
| R2-1 | `Danger` 何時用 | 刪除或移除已存在的資料用 `Danger`；只把輸入歸零的「清除」用 `Normal` | CAM / CNC 分兩組；CAM 的 `Danger` 給可能遺失資料的動作 |
| R2-2 | `Primary` 給誰 | 只給完成這個對話框（或這一步）的動作，每個容器最多一個 | 依建議 |
| R2-3 | 對話框寬度級距 | 訂四級並調整 5 個對話框 | 不訂級距，依內容決定 |
| R2-4 | 卡片內小標是否允許 | 不允許，拆成卡片 | 依建議，`DesignSupportBracketDlg` 拆成兩張並排卡片 |
| R2-5 | `RemovableValueRow` 範圍寫死 | 改為建構參數 | 依建議，並擴大為「自訂控件一律在 `CommonUI` 並寫明用途」 |
| C2-1 | 執行時讀到 `bin` 裡過期的語系檔仍通過 | 執行前同步 | 依建議（`SyncLanguage.ps1`） |
| C2-2 | 多狀態畫面只檢查一個狀態 | `STATE_MAP` | 依建議 |
| C2-3 | 按鈕列順序與卡片內對齊 | S17、S18 | 依建議 |
| C2-4 | `StartupForm.Designer.cs` 不在 S1 / S2 | S19 | 依建議 |
| C2-5 | 具名顏色常數可繞過 | 資料色登錄（S20） | 依建議 |
| C2-6 | 置中只驗同一種開啟方式 | 列後續 | 改為本議題補（P1 含 `ShowDialog`） |
| C2-7 | 語系在 `OnLoad` 套用，`Show` 前讀 `Text` 會拿到 zh-TW | 人工盤點 | 語系改回建構子套用，不需盤點 |
| C2-8～C2-10 | 刪除容器的承接、面板卡片靜態規則、執行時文字截斷 | 列後續 | 列後續（[6_FollowUp.md](6_FollowUp.md)） |

## 4.6 施工中的發現

| 日期 | 發現 | 處理 |
| --- | --- | --- |
| 2026-11-16 | zh-CN 截圖中 CNC 面板顯示繁中文字，執行時檢查卻全部通過。原因是改名後沒重跑 `CopyRef.bat`，建置輸出的語系檔仍是舊 ID（ID 找不到時保留 Designer 文字）；`Language/` 內的 zh-CN 檔本身正確，不是簡中檔寫了繁體字 | 執行時檢查前以 `SyncLanguage.ps1` 自動同步 |
| 2026-11-16 | `CalibrationREFDlg` 的 KeyDown 在 Designer 與 `Init()` 重複訂閱，按 Delete 觸發兩次 | 既有行為，列後續 |
| 2026-11-16 | 數值框取代 `TextBox` 的細節差異：可用上下鍵 / 滾輪；小數位數固定；`ThreePtTransformDlg` 機械座標不會再出現空白 | 屬已決議的統一，記在 `BehaviorExceptions.txt` 與後續事項 |
| 2026-11-17 | `master` 在分支起點之後前進兩個 commit（`8db7be81`、`7829c338`），只改開發環境檔案（`.claude/settings.json`、`project-overview.instructions.md`、`.gitignore`、`.worktreeinclude`、`CopyRef.bat`），與本議題的檔案沒有交集 | 合併前 merge / rebase 後重跑檢查（[6_FollowUp.md](6_FollowUp.md)） |
