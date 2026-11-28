# 規格

規則的最終版本寫在專案指引 `.github/project/coding-ui-style.md`（以下稱「指引」）；本文件說明每項規格做了什麼、
對應哪個需求（[Goal.md](Goal.md)），數值與 API 以程式為準。

## 1. 操作規格

使用者看得到的變化。

| 編號 | 規格 | 需求 |
| --- | --- | --- |
| SPEC-1 | **視覺主題**：所有畫面改為同一套淺色主題（色票、字型、圓角按鈕、白底卡片與標題列）；畫面建立時依介面語系套用字族（zh-TW 正黑、zh-CN 雅黑、其他 Segoe UI）。執行中換語系時，已建立的 CNC 面板可能暫時混用新舊字族；重新建立面板或重啟程式後才完整套用（DEC-21、TODO-10）。已開對話框不即時換語系是既有行為，不在此處變更 | REQ-1 |
| SPEC-2 | **對話框外框與位置**：所有對話框為工具視窗，不可拖曳改大小、不出現在工作列與 Alt+Tab；開啟時置中，橫式在整個主視窗、直式在上半部 CAM 區域；只改高度時位置不動 | REQ-1 |
| SPEC-3 | **對話框版面**：區塊一律是卡片，卡片垂直排列等寬；按鈕列主要動作在最右、取消在其左、輔助動作靠左；確定鍵統一為「確定」（有明確動作的按鈕保留原文字）；精靈分展開式與切換式兩型 | REQ-1 |
| SPEC-4 | **按鈕種類**：對話框以藍色實心表示完成這一步的動作、紅框紅字表示可能遺失資料的動作；CNC 加工鈕開始 / 暫停與繼續 / 停止為淡綠 / 淡橘 / 淡紅；停用一律灰色 | REQ-1 |
| SPEC-5 | **數值輸入**：所有數值欄位為同一種數值框，接受依目前文化設定的十進位數字，不接受 `e`／`E` 指數記號。非法或超出範圍的輸入（含清空）回復最新合法值，不得送入預覽或確定；Enter 遇到被拒絕的輸入時不提交。滾輪與上下鍵一次一個增量並停在上下限；只顯示、從不輸入的座標改為文字標籤 | REQ-1、REQ-4 |
| SPEC-6 | **主視窗**：工具列圖示統一為透明背景（功能工具列 40×40、其他 20×20），新增類圖示右下角加「+」；區域之間與 CNC 卡片之間一律相距 8；橫式訊息區橫跨底部時日誌在左、警報在右，訊息區是一欄時上下排列；加工頁監控卡片與 3D 畫布左右對齊，上緣與右側操作欄最後一張卡片對齊 | REQ-1 |
| SPEC-7 | **CNC 面板**：區塊標題改為卡片標題；面板隨主視窗大小縮放時，卡片間距與標題列不縮放 | REQ-1 |

## 2. 實作規格

| 編號 | 規格 | 需求 |
| --- | --- | --- |
| SPEC-8 | **外觀與行為分層**：三類畫面各有一個只管外觀的基底（`MainWindowBase`、`PanelBase`、`DialogBase`）；行為基底 `FormBase`、`EditDialogBase<T>` 疊在 `DialogBase` 之上、原封保留。`Show` / `ShowDialog` 呼叫點不變 | REQ-2、REQ-4 |
| SPEC-9 | **主題 token**：`CommonUI.Theme` 的 `ThemeColors`、`ThemeFonts`、`ThemeMetrics` 是顏色、字型、尺寸的唯一來源；文字以 `TextRole` 表達；字族在建立任何畫面前由 `MyApp.Initialize()` 決定；資料色為具名常數並登錄 | REQ-1、REQ-2 |
| SPEC-10 | **`CommonUI` 唯一元件來源**：控制項與共用工具依 `Theme`、`Controls`、`Forms`、`Layout`、`Language` 五類分資料夾，資料夾即 namespace；`MyCAM`、`CncUI` 原有的自訂控制項移入或以新元件取代後刪除 | REQ-2、REQ-3 |
| SPEC-11 | **Designer 如實呈現**：外觀屬性在基底與元件建構子設定並標為不序列化；所有畫面 `AutoScaleMode = None`；`InitializeComponent()` 之後不寫外觀程式 | REQ-5 |
| SPEC-12 | **版面機制**：對話框為 Designer 固定座標，執行時只透過 `SectionStack`（補位、按鈕列、換頁）；嵌入式面板以 `ResponsiveLayout` 縮放；主視窗以 `MainLayout` 純計算區域（`Calculate` 只算尺寸、`Update` 套用），不使用 `TableLayoutPanel`，Designer 寫入橫式加工頁的計算結果 | REQ-2、REQ-5 |
| SPEC-13 | **置中**：`DialogPlacement` 一處計算，`DialogBase.OnLoad` 與 `FormBase.Show` 呼叫；CAM 區域由主視窗註冊 | REQ-2 |
| SPEC-14 | **多語系**：控制項名稱即 ID、表單標題 ID 為類別名稱，由三個基底的 `ApplyLanguage` 經 `LanguageApplier` 套用，取代各畫面手寫的套用程式；本語系未定義的 ID 保留 Designer 文字、不記錄缺 ID（`LanguageManager` 新增 `HasString` 查詢）；共用按鈕文字集中於 `CommonUI_<語系>.xml` | REQ-2 |
| SPEC-15 | **控制項白名單與命名**：畫面只用白名單型別，欄位名稱為「型別前綴 + PascalCase」，全域改名（309 筆），語系 ID 同步 | REQ-2、REQ-3 |
| SPEC-16 | **特例**：承載單一內容的視窗、`CNCInfoPanel` 直式改排列、資料色、`CNCThreePointDlg` 可為空的欄位、`ComboBox` 暫留、`ToolTip` 的用途，各有定義並登錄（指引「共識的特例」） | REQ-3 |
| SPEC-17 | **檢查機制**：可持續使用的檢查放在 `Tools/UICheck`，規則以名稱識別；本議題限定的檢查放在 `Tests/`，基準固定為 rebase 目標 `bd0fa270`（DEC-12）。檢查需要的狀態以反射呼叫產品既有方法觸發，runner 執行結束即從建置輸出刪除 | REQ-6、REQ-7 |
| SPEC-18 | **專案指引**：`project/coding-ui-style.md` 只寫原則與共識的特例，`project/coding-icon-design.md` 寫圖示的產生方式，皆依時機讀取、不常駐；數值、API 與規則細節由程式與註解說明。程式註解不引用文件內容或章節，出處只寫議題編號 | REQ-7 |
| SPEC-19 | **併入 `master`**：rebase 到 `bd0fa270`；`master` 的修正（AUTO-23220 主從軸鎖定、AUTO-23240 三點校正圖面點、AUTO-23275 四軸控制器隱藏第二旋轉軸、應用程式圖示）保留原行為，套用本議題的控制項與外觀；表單圖示改由 `Properties.Resources` 指定，`ApplicationIcon` 指向實際檔案位置。指引依新分類移到 `project/`、議題文件依 `process-docs.md` 重整（DEC-12） | REQ-4、REQ-7 |
| SPEC-20 | **規格螢幕的截圖環境**：執行時檢查（`UICheck.bat runtime`）在 1920×1080、96 DPI、有工作列的螢幕上進行，不更動既有的螢幕：已有符合的螢幕就使用，否則以虛擬螢幕驅動（Parsec VDD）在檢查期間加入虛擬副螢幕，縮放固定 100%，檢查結束即移除；機台以 `MYCAM_UICHECK_UNATTENDED` 標記為終端機（`1`）或一般工作電腦（`0`），未標記時 `runtime` 不執行並說明設定方式；終端機不加入虛擬副螢幕而中止，改以常駐的虛擬主螢幕當規格螢幕；同一台電腦上用到規格螢幕的檢查一次只執行一項；指定的螢幕找不到或視窗不在該螢幕上判為失敗；橫式主視窗依產品行為最大化，直式在同一螢幕上以高大於寬的視窗呈現（`MainLayout` 只看長寬比）；對話框與 CNC 面板也在該螢幕上開啟；終端機的主視窗在可見桌面截圖（DEC-13、DEC-16、DEC-18） | REQ-8 |

## 3. 相容性異動

以下項目都已決議接受，不需要修改程式；合併時寫入 Merge Request 說明，部署到有客製檔案的現場時告知。

### 輸入

| # | 異動 | 既有資料的處理 |
| --- | --- | --- |
| 1 | **語系 ID 改名與刪除**：ID 改為控制項名稱、控制項欄位依前綴改名，並刪除無用 ID；以 zh-TW 計，30 個語系檔共 173 個舊 ID 不再使用。客製語系檔（`Custom/Language`）以 ID 逐筆覆蓋標準語系，舊 ID 的客製文字失效，畫面改顯示標準文字，不會出錯 | 不再支援舊 ID（2026-11-15 決議）。有客製語系檔的現場，依 `git diff bd0fa270 <合併 commit> -- Language/` 的新舊 ID 對照改寫客製檔 |
| 2 | **語系檔改名**：`LockABDlg_<語系>.xml` → `LockMSAxisDlg_<語系>.xml`、`CNCThreePointDialog_<語系>.xml` → `CNCThreePointDlg_<語系>.xml` | 舊檔名不再讀取；客製檔改為新檔名 |
| 3 | **共用按鈕文字改為全域**：26 個設定 `CommonText` 的按鈕（確定、取消、重設、預覽、關閉）一律讀 `CommonUI_<語系>.xml` 的 `Confirm`、`Cancel`、`Reset`、`Preview`、`Close`，不再讀畫面自己的 ID | 要保留的客製文字改寫到 `Custom/Language/CommonUI_<語系>.xml`；同一種按鈕的文字全畫面一致，不能再依畫面不同 |
| 4 | 機構、後處理、專案等其他設定與資料檔 | 無 |

### 輸出

無。NC 程式、專案檔與其他產出檔案的內容不變。

### 操作外觀（非資料相容性，合併說明時一併告知現場）

1. 對話框不可改大小、不出現在工作列（SPEC-2）。
2. 數值欄位改為數值框的細節差異：可用上下鍵與滾輪、小數位數固定、非法輸入回復合法值；原本是 `TextBox` 的欄位多了上下限，未另設範圍的為 ±99999（SPEC-5，DEC-20）。
   原本使用 `double.TryParse`、可接受指數記號的欄位不再接受該格式：新增路徑的 D1 門檻、弦差與最大線長；圖形設定的圓、矩形、跑道、多邊形共 12 個尺寸／圓角／角度欄位；三點轉換的 9 個機械座標。鍵入或貼上時請改用等值小數，例如以 `100` 取代 `1e2`；已核准的上下限及既有幾何條件不變（DEC-21）。
3. 只顯示的座標改為標籤後不能選取複製（SPEC-5）。
4. 橫式主視窗的警報在一般狀態位於日誌右側（SPEC-6）。

### 併入 `master` 的部分（SPEC-19）

無相容性異動：`master` 各議題的行為不變；應用程式與主視窗圖示與 `master` 相同，只改指定方式。

## 4. REQ 對照

| REQ | SPEC |
| --- | --- |
| REQ-1 | SPEC-1～SPEC-7、SPEC-9 |
| REQ-2 | SPEC-8～SPEC-10、SPEC-12～SPEC-15 |
| REQ-3 | SPEC-10、SPEC-15、SPEC-16 |
| REQ-4 | SPEC-5、SPEC-8、SPEC-19 |
| REQ-5 | SPEC-11、SPEC-12 |
| REQ-6 | SPEC-17 |
| REQ-7 | SPEC-17～SPEC-19 |
| REQ-8 | SPEC-20 |
