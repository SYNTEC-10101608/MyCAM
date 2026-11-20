# 2. 架構規劃

本章記錄最終的規劃方向與理由，每一節對應 [1_Goal.md](1_Goal.md) 的目標；具體規則以
`.github/instructions/ui-style.instructions.md`（以下稱「規範」）為準，本章不重複列出數值。

| 目標 | 對應章節 |
| --- | --- |
| 外顯風格一致 | 2.3 主題、2.6 版面、2.7 置中、2.9 按鈕種類、2.10 數值與座標欄位 |
| 底層實作一致 | 2.2 外觀與行為分層、2.4 Designer 如實呈現、2.5 `CommonUI`、2.8 多語系、2.11 命名 |
| 分類必要、分類內沒有例外 | 2.1 UI 分類、2.5 `CommonUI` 五類、2.13 保留的規則分支 |
| 行為沒有相容性異動、產品程式不留測試碼 | 2.2、2.12 檢查機制 |

## 2.1 UI 分類

依「畫面如何被承載」分成三類，每類一個外觀基底：

| 類別 | 承載方式 | 外觀基底 | 成員 |
| --- | --- | --- | --- |
| 主視窗 | 應用程式殼層，區域由 `MainLayout` 配置 | `MainWindowBase` | `StartupForm` |
| 嵌入式面板 | 嵌在主視窗的某個區域，大小由主視窗決定 | `PanelBase` | `CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`、`VNCUserControl` |
| 對話框 | 獨立的小視窗，大小由自己決定 | `DialogBase` | 31 個（含進度視窗 `LoadingBarDlg`、`CncUI` 的 `CNCOffsetDialog`） |

為什麼只分這三類：三者在「大小由誰決定、怎麼排版、語系何時套用」上確實不同，無法用同一個基底；
除此之外沒有其他外觀上的差別，所以不再細分。

1. 參數編輯、互動選取、精靈、一般對話框的差別是**操作邏輯**，不是外觀，外觀上同屬「對話框」一類，控制項與編排一律相同。
2. 進度視窗原本自成一類，盤點後它只是「承載單一內容的對話框」，併入對話框。
3. 卡片（`SectionPanel`）、按鈕、數值框等元件不受畫面類別影響：對話框與嵌入式面板的卡片規格、實作、規範完全相同。

## 2.2 外觀與行為分層

```
Form
└─ DialogBase                外觀：外框、字型、底色、自動縮放關閉、置中、語系文字
   ├─ FormBase               行為：Show( owner )
   │  └─ EditDialogBase<T>   行為：Preview / Confirm / Cancel / Reset
   └─ 其他對話框

UserControl
└─ PanelBase                 外觀：字型、底色、自動縮放關閉、語系文字

Form
└─ MainWindowBase            外觀：字型、底色、工具列字型與繪製、語系文字（含選單與工具列）
```

1. 盤點時對話框混用三種基底（`EditDialogBase<T>` 22 個、`FormBase` 3 個、直接繼承 `Form` 5 個）。
   `EditDialogBase<T>` 與其他兩者的差別是行為（事件機制），`FormBase` 與 `Form` 的差別只有外觀（置中、外框）。
2. 因此在繼承鏈最底層新增只管外觀的 `DialogBase`：所有對話框共用同一個外觀層，原本的行為基底原封保留。
   之後做行為統一時，只動 `FormBase` / `EditDialogBase<T>`，外觀層沿用。
3. 外框統一為工具視窗帶來的行為差異（不可改大小、不出現在工作列與 Alt+Tab）經盤點確認應該統一，一併納入。

## 2.3 主題

1. 色票、字型、尺寸集中為 `CommonUI.Theme` 的 token；程式中不寫顏色與字型。
2. 文字以「角色」（`TextRole`：內文、強調、標題、數值、次要、狀態）表達，而不是直接指定字型與顏色。
3. 字族依介面語系（zh-TW 正黑、zh-CN 雅黑），在建立任何畫面前由 `MyApp.Initialize()` 決定。
4. 資料色（圖層色、3D 物件色、CNC 座標軸點）不是主題色，定義為具名常數並登錄，只用在色塊、圖例、3D 物件。
5. 外觀設計（色票、字型、尺寸、圖示）沿用舊分支 `AUTO-TBD_f_UIStyleImprove`；實作不沿用，因為舊分支只做到外觀一致，沒有收斂實作。

## 2.4 Designer 如實呈現

1. 外觀屬性在**基底與元件的建構子**中設定，並標成不序列化：Designer 開啟畫面時會建立基底與元件的實體，
   所以看得到外觀；存檔時又不會把字型、顏色寫進各畫面的 `.Designer.cs`。
2. 原生 `CheckBox`、`RadioButton` 的字型與顏色繼承父容器，不需另外處理。文字標籤一律是 `ThemedLabel`，
   預設的 `Body` 角色同樣繼承父容器；只有一種標籤，就不必判斷何時用原生、何時用元件。
3. 衍生畫面在 `InitializeComponent()` 之後**不寫任何外觀程式**。執行時的外觀變化只透過共用機制：
   文字角色（`ThemedLabel.Role`）、版面工具（`SectionStack`）、元件（`RemovableValueRow`）、置中（`DialogPlacement`）。
4. 所有畫面關閉自動縮放（`AutoScaleMode = None`），Designer 上的座標就是執行時的像素。
5. Designer 中固定顯示 zh-TW 字型與文字；zh-CN 的字串長度由執行時檢查驗證。

## 2.5 `CommonUI`：唯一的元件來源，依五類分資料夾

所有控制項與畫面共用的工具都在 `CommonUI`，`MyCAM`、`CncUI` 不自行定義控制項。
原本散在 `MyCAM` 的 `DebouncedNumericUpDown`、`MultiSelectTreeView`、`SupportPositionRow`（通用化為 `RemovableValueRow`），
與 `CncUI` 的 `ResponsiveLayout` 都移入；`GroupPanel`、`PanelButton` 以新元件取代後刪除。

`CommonUI` 依職責分為五類，資料夾即 namespace，規範第 9 節的元件目錄也依同樣五類分節，由靜態檢查 S21 確保三者一致：

| 類別 | 職責 | 元件 |
| --- | --- | --- |
| `Theme` | 主題 token、文字角色、元件共用的繪製 | `ThemeColors`、`ThemeFonts`、`ThemeMetrics`、`TextRole`、`ThemeText`、`ThemedToolStripRenderer`、`RoundDrawing` |
| `Controls` | 控制項與只用於控制項屬性的型別 | `SectionPanel`、`ThemedButton`、`ButtonKind`、`ButtonIndicator`、`CommonButtonText`、`ThemedLabel`、`DebouncedNumericUpDown`、`ValueRejectedEventArgs`、`MultiSelectTreeView`、`ThemedDataGridView`、`RemovableValueRow`、`LoadingBar`、`RoundedProgressBar` |
| `Forms` | 三類畫面的外觀基底 | `DialogBase`、`PanelBase`、`MainWindowBase` |
| `Layout` | 執行時的版面計算 | `SectionStack`、`DialogPlacement`、`ResponsiveLayout` |
| `Language` | 語系文字的套用與共用文字 | `LanguageApplier`、`CommonTexts` |

元件不夠用時擴充元件本身（加屬性或參數），不在畫面裡另做一份。

## 2.6 版面

| 畫面 | 版面在何時決定 | 執行時的變化 |
| --- | --- | --- |
| 對話框 | Designer 固定座標，依規範 4.4 的格線 | 只有 `SectionStack`：部分卡片隱藏時其餘依序補位、精靈換頁（`ShowPage`） |
| `CNCPanel` | Designer 一份設計版面（即偏好尺寸） | 依主視窗給的大小以 `ResponsiveLayout` 等比縮放，橫、直式排列相同 |
| `CNCInfoPanel` | Designer 為橫式版面 | 直式時卡片改為上下排列（只用 token 計算）後再縮放 |
| `CNCAlarmPanel` | Designer | 以 `Dock` / `Anchor` 跟著可用空間延展，字型與列高不變 |
| `VNCUserControl` | 遠端畫面 | 大小隨 CNC 遠端畫面的解析度（承載單一遠端內容，見 2.13） |
| 主視窗 | Designer 為橫式加工頁的計算結果（客戶區 1920×1009） | `MainLayout.Calculate` 依背景大小與狀態算出各區矩形，`Update` 套用；橫 / 直切換與比例不變 |

1. 對話框不用 `TableLayoutPanel` / `FlowLayoutPanel` / `Dock` / `Anchor`：規格固定為單一解析度與兩個語系，
   固定座標在 Designer 中所見即所得，對齊與間距由規則與檢查保證。
2. 主視窗也不用 `TableLayoutPanel`：區域比例分配屬於外觀，由 `MainLayout` 一處計算。計算與套用分開
   （`Calculate` 只處理尺寸，不碰控制項），Designer 寫入計算在橫式加工頁的結果，Designer 即呈現實際的區域分布；
   P6 檢查 Designer 與計算結果一致。原本由 `TableLayoutPanel` 決定的捨入（每個百分比各自無條件捨去）照原樣保留，
   所以改動前後各區座標逐像素相同。
3. 嵌入式面板與對話框使用同一套 token 與卡片規則；只有邊距（8）、唯讀數值列的高與間距（22 / 26）不同，
   因為面板放在主視窗內，數值列若用輸入列的 30 間距，下方面板變高會壓縮 CAM 區。
4. 縮放時卡片標題列（32 高）與其中的控制項不縮放，只縮放標題列下方的內容。

## 2.7 置中

1. 由 `DialogPlacement` 一處計算，`DialogBase.OnLoad` 呼叫，所以 `Show` 與 `ShowDialog` 不必改呼叫點就一致。
2. 橫式置中在整個主視窗；直式置中在上半部 CAM 區域（CAM 編程操作用不到下半部 CNC 區），CAM 區域由主視窗註冊提供。
3. 直式的判斷與 `MainLayout` 相同，看主視窗是否高 > 寬，不看螢幕方向。

## 2.8 多語系

1. 控制項自己使用、且一個 ID 只對應它的文字：以**控制項名稱**當 ID，由基底的 `ApplyLanguage` 走訪套用；
   表單標題以類別名稱當 ID。原本 27 個畫面各自手寫的套用程式（約 440 行）全部移除。
2. 套用時機與改動前相同：對話框在建構子的 `InitializeComponent()` 之後；嵌入式面板與主視窗在建構子與切換語系時。
   三類畫面各自的 `ApplyLanguage` 都呼叫同一個 `LanguageApplier`。
3. 多個控制項共用、或執行時才決定的文字，在程式中明確取字串，且 ID 不可等於任何控制項名稱。
4. 共用按鈕文字（確定、取消、重設、預覽、關閉）放在 `Language/CommonUI_<語系>.xml`，以 `ThemedButton.CommonText` 指定。

## 2.9 按鈕種類

CAM 對話框與 CNC 面板對「嚴重性」的分級不同，各用一組按鈕種類，不共用同一套語意：

| 範圍 | 種類 | 語意 |
| --- | --- | --- |
| 共用 | `Normal` | 其他動作 |
| 對話框 | `Primary` / `Danger` | 結束這一步、有確定語意的動作（每個容器最多一個）/ 可能造成資料遺失的動作 |
| CNC 面板 | `Start` / `Hold` / `Stop` | 加工控制：開始、暫停與繼續、停止；功能的啟用狀態以 `Active` / `Indicator` 表示 |

停用時一律灰色。按鈕的數量與功能不因統一而改變。

## 2.10 數值與座標欄位

1. 所有數值輸入一律是 `DebouncedNumericUpDown`，以範圍屬性表達欄位限制，取代原生 `NumericUpDown` 與當數值用的 `TextBox`。
2. 只顯示點選座標、從不輸入的欄位改為 `ThemedLabel`（施工前逐一確認語意：從頭到尾不會被輸入，也不會在執行時切成可編輯）。

## 2.11 命名

1. 控制項欄位名稱 = 型別前綴 + 名稱，一個型別一個前綴（規範第 3 節白名單）。
   因為「控制項名稱即語系 ID」，名稱成為要長期維護的契約，所以全域統一，不限本議題改過型別的控制項。
2. 前綴只用於畫面自己擁有的控制項；從其他類別傳入的參考依一般物件命名。

## 2.12 檢查機制

檢查程式依「之後是否還有意義」分兩類：

| 類別 | 位置 | 檢查什麼 | 生命週期 |
| --- | --- | --- | --- |
| 可持續使用 | `Tools/UICheck` | 規範本身：靜態規則（S1–S24）、執行時版面（T1–T7）、主視窗（P1–P6） | 規範的一部分，隨規範維護 |
| 本議題限定 | `Doc/UIStyleUnify/Tests` | 本議題的改動：與分支起點比對行為不變（B1–B8）、本議題寫入的程式碼符合通用規範、產品程式沒有檢查用的程式（I1–I9）、主視窗區域與移除 `TableLayoutPanel` 前的基準相同 | 基準固定為分支起點，合併後仍可重現，但不再隨開發更新 |

1. 執行時檢查以 Roslyn 編譯 runner 到建置輸出、在隱藏桌面執行，結束即刪除；檢查需要的狀態以反射呼叫產品既有的方法觸發，
   產品程式不為檢查增加任何成員或分支。
2. 共用 / 執行時的標題文字、`SectionStack` 排列的卡片、精靈的頁由檢查自行辨識，不以例外清單處理。

## 2.13 保留的規則分支

以下是規範中明訂的處理方式差異。每一項都是一個有定義的情況，適用於該情況的所有畫面，不是個別畫面的例外：

| 情況 | 處理 | 理由 |
| --- | --- | --- |
| 承載單一遠端內容 / 單一內容的視窗（`CNCDlg`、`LoadingBarDlg`） | 內容可 `Dock = Fill`；`CNCDlg` 直式時定位到 CNC 區域、大小隨遠端畫面 | 內容大小不由 Designer 決定；`CNCDlg` 的用途就是顯示 CNC 的遠端畫面 |
| 資料色 | 具名常數並登錄 | 表達資料（圖層、軸），不是主題 |
| `CNCInfoPanel` 直式改排列 | 唯一依方向改排列的畫面 | 橫式的一列卡片在直式寬度下放不下 |

唯一的個別例外是 `CNCThreePointDlg` 的工件點 9 欄與誤差容忍欄維持 `TextBox`：空白代表「尚未輸入」且有意義，
數值框無法保持空白。已決議維持，以「可為空的數值框」列為後續議題（[6_FollowUp.md](6_FollowUp.md)），完成後移除此例外。
