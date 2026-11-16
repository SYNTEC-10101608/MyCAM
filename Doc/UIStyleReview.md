# UI 風格統一：Reviewer 初步盤點

> 日期：2026-11-16。範圍：分支 `AUTO-TBD_f_UIStyleUnify`（`master..87b6ddff`，26 個 commit）。
> 對照文件：`Doc/UIStyleDraft.md` v0.5、`Doc/UIStyleNotes.md`、`.github/instructions/ui-style.instructions.md`（以下簡稱「規範」）。
> 本檔只記錄審查發現與建議；決議仍寫回 `UIStyleNotes.md`。

## 0. 先確認的事實

| 項目 | 結果 |
| --- | --- |
| `Build.bat Debug` | 成功，0 個錯誤 |
| `Tools\UICheck\UICheck.bat`（靜態 + 行為比對） | 34 個 Designer 檔 0 問題；與 `master` 0 差異（與報告一致） |
| `UICheck.bat runtime` | 本次未重跑（會開隱藏桌面跑 12 張主視窗截圖） |

整體評價：基礎建設（`CommonUI.Theme` / `DialogBase` / `PanelBase` / `SectionPanel` / `ThemedButton`）的方向與草案一致，
對話框遷移的完成度高，Designer 檔幾乎沒有殘留外觀屬性。主要問題集中在三處：
**控制項命名縮寫完全沒有收斂**、**CNC 嵌入式面板仍是另一套實作**、**檢查腳本「通過」的涵蓋面比報告寫的窄**。

---

## 1. 文件與實作是否一致（含控制項縮寫）

### 1.1 控制項欄位縮寫：嚴重分裂，且名稱與型別不符（建議本議題處理）

C# 規範要求 `m_` + 控制項縮寫，但沒有縮寫對照表；本議題又把「控制項名稱 = 語系 ID」變成契約，名稱變成要長期維護的東西。
目前同一型別的縮寫（以 Designer 檔統計）：

| 型別 | 使用中的縮寫（個數） |
| --- | --- |
| `SectionPanel` | `m_sec`(34)、`m_grp`(13)、`m_gbx`(9)、`m_pnl`(7)、`m_gb`(5)、`m_gpb`(1) |
| `DebouncedNumericUpDown` | `m_txb`(32)、`m_tbx`(18)、`m_nud`(9)、`m_dbtn`(2)、無前綴 `tbxMachine*`(9)、`m*`(11) |
| `Label`（唯讀座標） | `m_tbx`(9, CalibrationREFDlg)、`m_txt`(9, CNCThreePointDlg)、`tbxCAD*`(9) |
| `Panel`（無外觀分組） | `m_pnl`(22)、`m_flpnl`(5)、`m_gbx`(3)、`m_tlp`(2)、`m_panel`(4)、`m_pan`(6) |
| `RadioButton` | `m_rbtn`(28)、`m_rdo`(5)、`m_rb`(2) |
| `ComboBox` / `CheckBox` | `m_cmb` / `m_cbx`；`m_chk` / `m_chb` |

問題不只是「不統一」：遷移後**名稱說的型別是錯的**（`m_txb*` 是數值框、`m_tbx*` / `m_txt*` 是 Label、`m_gbx*` / `m_flpnl*` / `m_tlp*` 是 Panel、
`m_pnl*` 是卡片、`m_dbtn*` 是數值框）。這些都是本議題改了型別的控制項，不屬於「與本次修改無關的既有程式碼」。

建議：
1. 規範第 3 節白名單表加一欄「欄位縮寫」，例如 `SectionPanel` → `m_sec`、`ThemedButton` → `m_btn`、`ThemedLabel` / `Label` → `m_lbl`、
   `DebouncedNumericUpDown` → `m_nud`、`Panel` → `m_pnl`、`RadioButton` → `m_rdo`、`CheckBox` → `m_chk`、`ComboBox` → `m_cmb`、`TextBox` → `m_txt`。
2. 本議題改過型別的控制項一律改名，語系 XML 四語系同步改 ID（命名即 ID，改名成本現在最低）。
3. 靜態檢查新增 S9：依型別檢查前綴。

### 1.2 規範與實作不符的條目

| # | 規範 / 文件寫的 | 實作 | 建議 |
| --- | --- | --- | --- |
| D1 | 規範 1、草案 3.6：不處理 DPI，**移除** `DeviceDpi / 96` | `CNCInfoPanel.cs:84,118,195,196`、`CNCPanel.cs:102,128`、`StartupForm.cs:290` 仍在算 | 移除，或在規範明列例外 |
| D2 | 規範 2.2：`Program.Main` 在建立畫面前呼叫 `ThemeFonts.Initialize` | 實際在 `StartupForm` 建構子內呼叫，**晚於** `MainWindowBase` 建構子設定 `Font`（見 2.1 的 bug） | 移到 `Program.Main`，文件與實作一致 |
| D3 | 規範 4.5：嵌入式面板縮放時「標題列內的控制項**不縮放**」 | `ResponsiveLayout.Apply` 只固定標題列控制項的 Y / 高，X 與寬仍乘 `scaleX` | 實作改為不縮放，或文件改為「只保持高度」 |
| D4 | 規範 4.5：嵌入式面板卡片內容第一列 `y = 36`；卡片與對話框「規格與實作不因畫面類別而不同，沒有例外」 | `CNCPanel` 卡片內左距 4（對話框 10）；`CNCInfoPanel` 卡片間距 4（`SECTION_GAP` 8）、第一列 y=34、列距 26；直式由 `CreateVerticalLayout` 以字型高 + padding 自算列高 | 見 2.2；至少在規範寫清楚面板適用哪些尺寸規則 |
| D5 | 規範 4.6：顯示 / 隱藏區段後用 `SectionStack`，不在對話框內手算座標 | `DesignSupportBracketDlg.ShowPanel()` 自己算 `ClientSize = activePanel.Right + activePanel.Left ...` | 改用 `SectionStack`（或給它一個「只顯示其中一個面板」的 API） |
| D6 | 草案 1.6：可增刪數值列「通用化，序號、單位、刪除文字由呼叫端給」 | `RemovableValueRow` 的範圍 / 小數位（±99999、1 位）寫死在元件內（與 master 的 `SupportPositionRow` 相同，行為沒變） | 範圍改由建構參數給，才算通用 |
| D7 | `StaticExceptions.txt` 的 `CNCThreePointDlg m_txtWorkpiece*` 理由寫 "pending decision" | 2026-11-16 已決議維持 TextBox、列後續議題 | 理由改為已決議並指向 Notes 第 4 節 |
| D8 | `CheckStatic.ps1` 的 `ALLOWED_TEXTBOX` 含 `GlassDXFImportDlg.m_txbFilePath` | 該控制項已不存在（只有數值框） | 刪除，避免白名單過寬 |
| D9 | `UIStyleHandoff.md` 1、6 節：`cmd /c Build.bat Debug` | 個人指引明訂不要用 `cmd /c`（找不到檔案） | 改為 `.\Build.bat Debug` |
| D10 | `UIStyleCheckReport.md` 4 節第 1、5 項仍列為待確認 | Notes 2026-11-16 已記錄 Designer 確認無誤、直式工具列溢出為既有問題 | 報告同步更新 |
| D11 | 規範 4.4 「對話框不用 `Dock`」的唯一例外列了 `LoadingBarDlg`、`CNCDlg` | `CNCInfoPanel` 直式是「唯一依方向改排列的畫面」只寫在 Notes，規範沒寫 | 規範第 6 節補上面板版面決定時機（Notes 2026-11-16 那一列） |
| D12 | C / C# 通用規則：縮排用 tab | `CncUI/Panel/CNCPanel.Designer.cs` 全檔 4 空白縮排（master 就是如此，但本議題改了 480 行） | 既然整檔重寫，順便轉 tab |

---

## 2. 實作結果是否真的統一

### 2.1 疑似 bug：zh-CN 啟動時主視窗字族不對（需實測）

`MainWindowBase` 的建構子先於 `StartupForm` 建構子本體執行，當時 `ThemeFonts` 還是預設 zh-TW 字族；
`StartupForm` 之後才呼叫 `ThemeFonts.Initialize( zh-CN )`，但沒有再呼叫 `ApplyThemeFont()`（只有切換語系選單時才呼叫）。
結果：以 zh-CN 啟動時，主視窗與繼承字型的樹、日誌等仍是 `Microsoft JhengHei UI`；之後開的對話框才是 `Microsoft YaHei UI`。
主視窗截圖檢查（P1 / P2）只檢查置中，不檢查字族，所以抓不到。修法：D2（移到 `Program.Main`）。

### 2.2 CNC 嵌入式面板仍是另一套版面實作

對話框收斂到「Designer 固定座標 + `SectionStack`」，但 CNC 面板還是：`ResponsiveLayout`（放在 `CncUI/Helper`，namespace 卻是 `CncUI.Controls`）
等比縮放 + `CNCInfoPanel` 兩百行左右的手算座標（`UpdateResponsiveLayout`、`CreateVerticalLayout`、`AlignVerticalColumns`）+ 自己的常數
（`VERTICAL_CARD_GAP = 4` 等，不是 `ThemeMetrics`）+ 寫死的偏好尺寸（672×236、240×620）。
這正是 Notes 2026-11-16「卡片規格不因所在 UI 類別而分裂」想消除的分裂，目前只統一了「卡片標題」，沒統一卡片內距、間距、列距。
要不要在本議題處理是範圍問題（牽涉面板版面行為），見第 4 節待決問題 Q2。

### 2.3 規則沒定義、各對話框各自決定的地方

| 項目 | 現況 | 建議 |
| --- | --- | --- |
| `ButtonKind.Danger` 何時用 | 「刪除」多為 Danger；「清除」在 `PathCompensateDlg` / `PathEditDlg` 是 Normal，「全部清除」在 `CNCOffsetDialog` 是 Danger；`FlyCutDlg`「移除」是 Normal | 規範定義：不可復原的刪除 / 清除類用 Danger |
| `Primary` 給誰 | `FlyCutDlg` 沒有確定鍵，Primary 給「新增」；`AddPathDlg` 給「生成路徑」 | 規範定義：Primary 只給「結束這個對話框的主要動作」，沒有就不設 |
| 對話框寬度 | 320 / 360 / 380 / 400 / 434 / 440 / 476 / 484 / 500 / 584，共 10 種 | 是否訂寬度級距（例如 320 / 380 / 440 / 500）見 Q3 |
| 卡片內的次標題 | `DesignSupportBracketDlg` 的 `m_lblX/YPositionsTitle` 用 `Emphasis` 當小標；S8 只檢查標題列範圍，抓不到 | 規範明訂：卡片內小標用 `Emphasis` 是否允許 |

### 2.4 有確認是統一的

- 對話框全部經 `DialogBase`（含 `LoadingBarDlg`、`CncUI` 的 `CNCOffsetDialog`）；Designer 檔沒有字型、外框、縮放屬性。
- 寫死顏色只剩資料色具名常數（`CNCInfoPanel` 軸色、`DesignSupportBracketDlg` X/Y 色）與 Designer 的圖層色、圖例色塊（均有例外說明）。
- 單位標籤全部是 `ThemedLabel` + `Secondary`；按鈕列高 28、寬 ≥ 80、間距 8、距底 12 全部符合。
- 置中只剩 `DialogPlacement` 一處計算；`MyApp.CalculateDialogCenterLocation` 已刪除且無殘留呼叫。
- `GroupPanel`、`PanelButton`、`SupportPositionRow` 已刪除；`MyCAM` / `CncUI` 沒有自訂控制項類別。

---

## 3. 檢查腳本是否足以驗證所有目標

「0 個問題」的可信度取決於涵蓋面。以下是目前**驗證不到**的部分：

### 3.1 行為不變比對（`CompareBehavior.ps1`）——缺口最大

| # | 缺口 | 影響 |
| --- | --- | --- |
| V1 | 只比對 Designer 檔；**`.cs` 程式碼的變動完全不比對**（除了 `Show` / `ShowDialog` 呼叫點）。本分支改了 30 多個 `.cs`（`CNCThreePointDlg.cs` 64 行、`DesignSupportBracketDlg.cs` 89 行、`CNCPanel.cs`、`CNCInfoPanel.cs`、`StartupForm.cs`…） | 程式碼中的事件訂閱、`Enabled` / `Visible` 切換、驗證邏輯被改也不會報 |
| V2 | **`StartupForm.Designer.cs` 不在比對範圍**（`SCREEN_DIRS` 沒有它），但它改了 518 行 | 主視窗的工具列項目、事件、`Visible` / `Enabled` 沒有被驗證 |
| V3 | `TabIndex` 不在 `BEHAVIOR_PROPS`；版面重排很容易改變 Tab 順序 | 鍵盤操作順序屬於操作規格 |
| V4 | 語系套用時機由建構子改到 `OnLoad`：若有程式在 `Show` 之前讀控制項 `Text`，會讀到 zh-TW 預設字 | 沒有任何檢查 |
| V5 | 被移除的容器（`GroupBox` → 刪除）只要程式沒引用就不報；容器原本的 `Enabled` / `Visible` 是否由新容器承接沒有驗證 | Notes 中 `ToolVectorDlg` 起終點切換列就是這種案例，靠人工發現 |

建議：V1 至少加「`.cs` 的 `+=` / `-=` 事件訂閱、`.Enabled =` / `.Visible =` 賦值」的新舊比對；V2 把 `StartupForm` 納入；V3 把 `TabIndex` 納入
（重排後的變動列入例外並逐一確認）。

### 3.2 靜態檢查（`CheckStatic.ps1`）

| # | 缺口 |
| --- | --- |
| S-a | 「`InitializeComponent()` 之後不寫外觀程式」這條核心規則**沒有檢查**：S4 只抓 `Color.*` / `new Font`，不抓 `.Font =`、`.ForeColor =`、`Location` / `Size` / `SetBounds` 等賦值（D5 的 `DesignSupportBracketDlg` 就沒被抓到） |
| S-b | S5 版面規則只檢查對話框；嵌入式面板的卡片規格完全不檢查（D4 因此通過） |
| S-c | S5 沒檢查：按鈕順序（確定最右、取消在其左、輔助從 x=12）、`Kind`、卡片內標籤 x=10、輸入欄左緣對齊、單位距輸入欄 6 |
| S-d | `StartupForm.Designer.cs` 不在 S1 / S2 範圍 |
| S-e | S4 只要寫成 `static readonly Color XXX =` 就放行，任何顏色都能用「具名常數」繞過，沒有區分是否為資料色 |
| S-f | `DeviceDpi` 的使用（規範明文禁止）沒有檢查（D1 因此通過） |
| S-g | 命名縮寫沒有檢查（1.1） |

### 3.3 執行時檢查（`LayoutRunner` / `MainWindowRunner`）

| # | 缺口 |
| --- | --- |
| R-a | 畫面建構失敗時只印 `SKIP`、**不計入問題、結束碼仍是 0**。「34 個畫面 0 問題」不能保證 34 個都真的檢查到 |
| R-b | 「Designer 如實呈現」沒有自動驗證：沒有比較 `Load` 前後的 `ClientSize` / 控制項位置是否被衍生類別改動 |
| R-c | 每個畫面只檢查一個狀態：`AddPathDlg` 只看一個步驟、`DesignSupportBracketDlg` 只看三個面板之一、`CNCThreePointDlg` 只看初始步驟；用 `SectionStack` 改變版面的狀態沒被檢查 |
| R-d | CNC 面板只以一個尺寸承載；`CNCInfoPanel` 直式版面（`CreateVerticalLayout`）、`CNCPanel` 直式寬度下的縮放都沒檢查 |
| R-e | 執行時才填入的文字（S6 / S7 例外清單上約 40 個控制項）在 zh-CN 下是否超出，無法由 `DrawToBitmap` 截圖看出 |
| R-f | 主視窗只檢查置中；字族（2.1）、工具列繪製、樹與日誌色都沒驗證 |
| R-g | 置中只驗 3 個對話框，且都走同一條開啟路徑；`ShowDialog` 開啟的（`GlassDXFImportDlg`、`ConnectDlg`）、經 `FormBase.Show` 再次顯示的路徑沒覆蓋 |

建議優先：R-a（SKIP 改為失敗，或至少列出並要求例外清單）、R-b（`Load` 前後比較）、R-c / R-d（為多狀態畫面加 prepare 清單、面板加直式尺寸）。

---

## 4. 待決問題（附建議）

| # | 問題 | 建議 | 另一個選項 |
| --- | --- | --- | --- |
| Q1 | 控制項縮寫（1.1）是否在本議題統一並改名 | **本議題處理**：名稱即語系 ID，之後改成本更高；改名不影響行為 | 只訂規範與靜態檢查，既有名稱列後續議題 |
| Q2 | CNC 面板版面（2.2、D4）要統一到什麼程度 | 本議題只把尺寸改用 `ThemeMetrics`（卡片間距 8、內距 10、第一列 36）並移除 `DeviceDpi`；`ResponsiveLayout` 縮放機制保留，規範寫明面板的例外 | 列後續議題，規範明列 CNC 面板尚未符合的項目 |
| Q3 | 是否訂對話框寬度級距 | 不強制，但規範建議 320 / 380 / 440 / 500 四級，新畫面優先選用 | 維持依內容決定 |
| Q4 | 檢查腳本缺口（第 3 節）要補到哪裡才算驗收 | 本議題至少補 V1、V2、V3、S-a、S-f、R-a、R-b；其餘列後續 | 全部列後續，報告中明寫涵蓋範圍限制 |
| Q5 | 2.1 主視窗字族問題 | 本議題修（D2），並在 `MainWindowRunner` 加字族檢查 | — |

## 5. 第一輪決議與處理結果（2026-11-16）

| 項目 | 決議 | 結果 |
| --- | --- | --- |
| 控制項縮寫 | 本議題全域改名；panel 類一律 `m_pnl`、數值框 `m_num` | 304 筆改名（`Tools/UICheck/RenameMap.txt`），套用對照表後與 `master` 行為比對 0 差異；靜態檢查 S9 |
| CNC 面板版面 | 本議題處理，依建議 | 改用 `ThemeMetrics`（新增 `PANEL_MARGIN`、`INFO_ROW_HEIGHT`、`INFO_ROW_PITCH`），移除 `DeviceDpi`；設計尺寸 `CNCPanel` 288×756、`CNCInfoPanel` 736×240；規範第 6 節「嵌入式面板」 |
| 檢查腳本缺口 | 依建議補；`TabIndex` 依版面重排 | S9–S13、B8、T6、P3、P4、`FixTabOrder.ps1` |
| 主視窗字族 | 初始化放在 `MyApp.Initialize()`，`Program.Main` 先呼叫 | 另修正 `ToolStrip` 字型；P3 檢查字族 |

驗證：`UICheck.bat runtime` 靜態 0、行為 0、zh-TW / zh-CN 各 33 畫面 0、主視窗 12 張截圖 0（含 P4）。
