# 實作架構與步驟

## 1. 實作架構

依「外觀集中、行為不動」拆成五個模組；箭頭為相依方向。

```text
MyCAM / CncUI 的畫面 ──> CommonUI（Theme、Controls、Forms、Layout、Language）──> LanguageManager
主視窗 StartupForm ──> MainLayout（MyCAM/Layout）
Tools/UICheck（可持續檢查）、Tests/（本議題檢查）──> 建置輸出（以反射驅動，不被產品參考）
```

| 模組 | 內容 | 對應規格 |
| --- | --- | --- |
| `CommonUI.Theme` | `ThemeColors`、`ThemeFonts`、`ThemeMetrics`、`TextRole`、`ThemeText`、`ThemedToolStripRenderer`、`RoundDrawing` | SPEC-9 |
| `CommonUI.Controls` | `SectionPanel`、`ThemedButton`（`ButtonKind`、`ButtonIndicator`、`CommonButtonText`）、`ThemedLabel`、`DebouncedNumericUpDown`（`ValueRejectedEventArgs`）、`MultiSelectTreeView`、`ThemedDataGridView`、`RemovableValueRow`、`ColorSwatch`（`SwatchShape`）、`LoadingBar`、`RoundedProgressBar` | SPEC-5、SPEC-10、SPEC-15 |
| `CommonUI.Forms` | `DialogBase`、`PanelBase`、`MainWindowBase` | SPEC-8、SPEC-11 |
| `CommonUI.Layout` | `SectionStack`、`DialogPlacement`、`ResponsiveLayout` | SPEC-12、SPEC-13 |
| `CommonUI.Language` | `LanguageApplier`、`CommonTexts`（`Language/CommonUI_<語系>.xml`）；依賴 `LanguageManager` 新增的 `HasString` | SPEC-14 |
| `MyCAM` 主視窗 | `MainLayout`（純計算區域）、`MainLayoutRegions`、`PathLayerColor`（圖層資料色單一定義）、`MyApp.Initialize()`（字族） | SPEC-6、SPEC-12 |
| 畫面 | 31 個對話框、4 個嵌入式面板、主視窗改用上述基底與元件；語系 XML 與 60 個工具列圖示（`MyCAM/Resource/IconSource` 產生） | SPEC-1–SPEC-7 |
| `Tools/UICheck` | 靜態檢查、執行時版面、數值框操作、主視窗檢查與輔助工具（`DesignerDiff.ps1`、`FixTabOrder.ps1`） | SPEC-17 |
| `Tests/`（本資料夾） | 行為比對、本議題程式碼風格、主視窗區域基準 | SPEC-17 |

設計上的取捨：

1. 外觀基底插在繼承鏈最底層、行為基底原封保留，之後統一行為時只動行為基底，外觀層沿用（SPEC-8）。
2. 對話框不用 `TableLayoutPanel` 等自動排版：規格固定為單一解析度與兩個語系，固定座標在 Designer 中所見即所得，
   對齊與間距由檢查保證（SPEC-12）。
3. 主視窗的區域比例屬於外觀，由 `MainLayout` 一處計算；計算與套用分開，計算結果可單獨驗證，Designer 也能寫入
   計算結果（SPEC-12）。
4. 嵌入式面板與對話框共用卡片規格，只有邊距與唯讀數值列不同：面板放在主視窗內，數值列用輸入列的間距會壓縮 CAM 區（SPEC-7）。

## 2. 實作步驟

| STEP | 步驟 | 內容 | SPEC | TEST |
| --- | --- | --- | --- | --- |
| STEP-1 | 盤點與規範定稿 | 現況盤點、設計草案、專案指引初版 | SPEC-18 | — |
| STEP-2 | 基礎建設 | `CommonUI` 主題 token、外觀基底、共用元件、三個控制項移入、圖示產生腳本、檢查腳本 | SPEC-9～SPEC-11、SPEC-17 | TEST-1、TEST-2、TEST-5 |
| STEP-3 | 主視窗外觀 | `MainWindowBase`、工具列繪製、移除寫死顏色與 Magenta 透明色、字族初始化移到 `MyApp`；`LogHandler` 在執行時以 `ThemedLabel`／`TextRole` 建立日誌標籤 | SPEC-1、SPEC-6、SPEC-9 | TEST-2（程式外觀規則）、TEST-9（主視窗，不驗日誌標籤的角色字型） |
| STEP-4 | 對話框遷移 | 31 個對話框分八批：改繼承 `DialogBase`、卡片與固定座標、數值框、語系自動套用 | SPEC-1～SPEC-5、SPEC-8、SPEC-11～SPEC-14 | TEST-3、TEST-4、TEST-6、TEST-11、TEST-15、TEST-16 |
| STEP-5 | 嵌入式面板 | CNC 面板改 `PanelBase`、卡片標題、`ThemeMetrics`、移除 `DeviceDpi`；刪除 `GroupPanel`、`PanelButton` | SPEC-7、SPEC-10、SPEC-12 | TEST-6、TEST-9、TEST-17 |
| STEP-6 | 全域命名 | 控制項欄位依型別前綴改名 309 筆，事件方法、`.resx`、語系 ID 同步（對照表 `Tests/RenameMap.txt`） | SPEC-15 | TEST-1、TEST-4、TEST-11 |
| STEP-7 | 主視窗版面 | `MainLayout` 改純計算、移除 `TableLayoutPanel` 與多餘的工具列容器、Designer 寫入計算結果 | SPEC-6、SPEC-12 | TEST-10、TEST-14 |
| STEP-8 | 規範與檢查補強 | `CommonUI` 五類重整、XML 文件註解、Designer 協作（`DesignerDiff.ps1`、`DesignerReadable`）；資料色元件 `ColorSwatch`；`ToolVecInterpolateColor` 讓 `ToolVectorDlg` 圖例與 `ToolVecEditRender` 的 3D 路徑共用色表；`DataColor.ToScreenColor` 將 OCCT 色轉成 sRGB，刀向圖例與 `PathLayerColor` 共用 | SPEC-9～SPEC-11、SPEC-16、SPEC-17 | TEST-1、TEST-2、TEST-5、TEST-13（規則與 Designer，不驗色表一致性或轉換色值） |
| STEP-9 | 整體 review 修正 | 數值框的回復與通知、行為比對防線、runner 清理（Round 1–4） | SPEC-5、SPEC-17 | TEST-7、TEST-11 |
| STEP-10 | 人工確認的修正 | 新增類圖示補「+」、區域與 CNC 卡片間距統一、橫式警報左右並排、監控卡片對齊 3D 畫布 | SPEC-6、SPEC-7 | TEST-14、TEST-17 |
| STEP-11 | 文件與檢查整理 | 專案指引精簡、檢查規則改以名稱識別、程式註解不引用文件、議題文件依新分類重整 | SPEC-17、SPEC-18 | TEST-12 |
| STEP-12 | 併入 `master` 與依新規範調整 | rebase 到 `bd0fa270` 並解衝突（DEC-12）；主視窗圖示改由專案資源檔指定、修正 `ApplicationIcon` 路徑；指引移到 `project/`、專案指引索引與專案規範；`IconSource` 腳本補 BOM；議題檢查基準改為 `bd0fa270`；議題文件依 `process-docs.md` 重整 | SPEC-19 | TEST-11、TEST-12、TEST-19 |
| STEP-13 | 規格螢幕的截圖環境 | `VirtualDisplayRun.cs`；主視窗 runner 的最大化模式；`CaptureMainWindow.ps1 -Screen`、`CheckRuntimeLayout.ps1` 在規格螢幕上執行；`STATE_MAP` 依 `master` 的新簽章與四軸狀態；`project/tool-uicheck.md`（DEC-13、DEC-14） | SPEC-20 | TEST-6、TEST-18 |
| STEP-14 | 數值格式與語系限制 | `DebouncedNumericUpDown` 在 Enter／Leave 事件送達畫面前拒絕非法文字；`AddPathDlg` 的 D1 即時回饋與進階欄位驗證使用同一解析與範圍。`ControlRunner` 新增 `NumCommitFormat`；記錄已建立 CNC 面板的換字族限制，README 補 UICheck 操作入口與寫入警告（DEC-21） | SPEC-1、SPEC-5、SPEC-17 | TEST-7、TEST-9、TEST-16 |
| STEP-15 | 主從軸鎖定 Designer 載入 | 曾以非泛型直接基底避開載入錯誤，已由 STEP-17 取代 | SPEC-8、SPEC-11、SPEC-19 | TEST-1、TEST-6、TEST-15、TEST-19 |
| STEP-16 | 整合 GitLab `master` 的輪廓編輯 | `ContourEditDlg` 保留 AUTO-23284 的控制點按鈕狀態及總偏移更新，沿用議題的主題與語系套用；Designer 移除舊提示並收合按鈕區上方空行，四個標準語系移除該 ID。行為比對與分支規則固定到新基準 | SPEC-17、SPEC-19 | TEST-6、TEST-11、TEST-12、TEST-19 |
| STEP-17 | 統一編輯對話框繼承 | `LockMSAxisDlg` 恢復直接繼承 `EditDialogBase<object>`，與 `ContourEditDlg`、`MicroJointDlg` 一致；移除中介類別、專案引用與 `ScreenBase` 的對應分支。不改版面、編輯事件或 CNC 行為 | SPEC-8、SPEC-11、SPEC-19 | TEST-1、TEST-6、TEST-15、TEST-19 |

每一步的程式、測試、文件分開提交；逐檔異動以 `git diff af5a2e8d` 與 `Tools/UICheck/DesignerDiff.ps1 -Base af5a2e8d -Head HEAD`
查閱。STEP-1～STEP-11 的 commit 在新規範之前，標題沒有 `(STEP-n)`，依上表的內容人工對照（DEC-12）；STEP-12 之後依新規範標示。

## 3. 異動範圍

| 對象 | 異動 |
| --- | --- |
| 對話框 31 個 | 改繼承 `DialogBase`（直接或經行為基底）；`.Designer.cs` 移除外觀屬性；`GroupBox` → `SectionPanel`、`Button` → `ThemedButton`、`Label` → `ThemedLabel`；約 20 個 `TableLayoutPanel` 與所有 `FlowLayoutPanel`、`Dock`、`Anchor` 改為固定座標；`TabIndex` 依版面重排；建構子呼叫 `ApplyLanguage()`，刪除手寫套用程式 |
| 數值欄位 | 改為 `DebouncedNumericUpDown`：`AxisTransformDlg` 8 個、`AutoSortPathDlg` 2 個、`AddPathDlg` 3 個、`PatternSettingDlg` 12 個、`ThreePtTransformDlg` 9 個；只顯示的座標改為 `ThemedLabel`：三個對話框共 27 個 |
| 嵌入式面板 4 個 | 改繼承 `PanelBase`；區塊標題改卡片標題；設計尺寸 `CNCPanel` 288×756、`CNCInfoPanel` 736×240；軸點改 `ColorSwatch` |
| 主視窗 | 改繼承 `MainWindowBase`；`MainLayout` 純計算；刪除第二、三層工具列容器與未使用的右鍵選單；功能工具列容器改為 `Panel`；圖層色改由 `PathLayerColor` 單一定義 |
| 語系 | 所有語系 XML 依新 ID 修改；兩個語系檔改名；新增 `CommonUI_<語系>.xml` |
| 資源 | 60 個工具列圖示重新產生為透明背景 PNG（檔名不變），新增「加工 / 不加工」2 個 |
| 刪除 | `GroupPanel`、`PanelButton`、`MyCAM` 內的 `DebouncedNumericUpDown`、`SupportPositionRow`、`MultiSelectTreeView`、`CncUI/Helper/ResponsiveLayout`；`TextRole.Title` |
| 指引 | 新增 `project/coding-ui-style.md`、`project/coding-icon-design.md`；修改專案概觀的專案指引索引、`project/coding-architecture.md`（`CommonUI`）、`project/coding-general.md`（語系措辭、Designer）、`project/process-review.md`（門檻加入 `UICheck`）；新增 `project/tool-uicheck.md`（UICheck 指令與截圖環境）；團隊概觀協作原則第 2 項補第 4 點；`team/process-ai-review.md` 第 2 節第 3 項補 Copilot 的模型一律由施工方以腳本執行。理由：UI 規範依新分類屬依時機讀取的專案程式規範，不常駐；截圖環境屬使用工具時的做法；兩項團隊規範補充是本議題執行時發現的誤用：把工具權限全自動當成決策不必先問、請人在 IDE 中啟動 Copilot 而無法自動確認結果（DEC-12～DEC-14、DEC-16）。第 13 回合整理：`project/coding-general.md` 的多語系只留語系檔與取字串，畫面規則連到 `coding-ui-style.md`；`coding-ui-style.md` 特例表不寫後續計畫；`tool-uicheck.md` 的安裝步驟移到 `Tools/UICheck/README.md`、`runtime` 連到建置依賴（DEC-20） |
| `LanguageManager` | 新增公開查詢 `HasString`：`LanguageApplier` 自動套用時辨識本語系未定義的 ID，保留 Designer 文字且不記錄缺 ID（SPEC-14） |
| 檢查工具 | `Tools/UICheck` 新增 `VirtualDisplayRun.cs`、`README.md`（常駐虛擬螢幕的安裝步驟）；`MainWindowRunner.cs`、`LayoutRunner.cs`、`CaptureMainWindow.ps1`、`CheckRuntimeLayout.ps1`、`UICheck.bat` 改在規格螢幕上執行 |

## 4. SPEC 對照

| SPEC | STEP |
| --- | --- |
| SPEC-1 | STEP-3、STEP-4、STEP-14 |
| SPEC-2～SPEC-4 | STEP-4 |
| SPEC-5 | STEP-4、STEP-9、STEP-14 |
| SPEC-6 | STEP-3、STEP-7、STEP-10 |
| SPEC-7 | STEP-5、STEP-10 |
| SPEC-8 | STEP-4、STEP-15、STEP-17 |
| SPEC-9 | STEP-2、STEP-3、STEP-8 |
| SPEC-10 | STEP-2、STEP-5、STEP-8 |
| SPEC-11 | STEP-2、STEP-4、STEP-8、STEP-15、STEP-17 |
| SPEC-12 | STEP-4、STEP-5、STEP-7 |
| SPEC-13、SPEC-14 | STEP-4 |
| SPEC-15 | STEP-6 |
| SPEC-16 | STEP-8 |
| SPEC-17 | STEP-2、STEP-8、STEP-9、STEP-11、STEP-14、STEP-16 |
| SPEC-18 | STEP-1、STEP-11 |
| SPEC-19 | STEP-12、STEP-15、STEP-16、STEP-17 |
| SPEC-20 | STEP-13 |
