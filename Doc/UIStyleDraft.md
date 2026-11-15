# UI 風格盤點與重構草案（v0.5）

> 議題：UI 風格盤點、優化、統一（分支 `AUTO-TBD_f_UIStyleUnify`）
> 需求、決議與後續項目記錄在 [UIStyleNotes.md](UIStyleNotes.md)，本檔只放盤點結果與設計。
>
> 版本紀錄
> - v0.2：對話框外觀合併為一類、數值輸入盤點、沿用舊分支的風格與圖示、Designer 如實呈現。
> - v0.3：補充外框的定義；只顯示座標的欄位改為文字標籤；語系 ID 以控制項名稱自動套用。
> - v0.5：補上 `MainLayout` 的計算內容；衍生對話框不寫外觀程式；所有對話框不可改大小、不出現在工作列與 Alt+Tab；確認不用 `TableLayoutPanel`。
> - v0.4：範圍限定為外觀（行為一律不動）；對話框基底差異與外框影響盤點；置中納入外觀；共用控制項全部移到 `CommonUI`；版面改為 Designer 固定座標；區塊標題盤點；驗收腳本。

**範圍原則**：本議題只做外觀統一。每一項改動都要先判斷是外觀還是行為，行為差異一律不動、列入後續處理。
判斷標準：改動後，使用者能做的操作、操作的順序與結果、視窗是否強制回應，都必須和改動前完全一樣。

---

## 1. 盤點結果

### 1.1 UI 分類

| 類別 | 說明 | 成員 |
| --- | --- | --- |
| A. 主視窗殼層 | 選單、功能工具列、樹、3D 檢視、日誌、圖層列、視角列；橫 / 直式版面 | `StartupForm`、`Layout/MainLayout` |
| B. 嵌入式面板 | 嵌在主視窗某個區域，不是獨立視窗 | `CNCPanel`、`CNCInfoPanel`、`CNCAlarmPanel`、`VNCUserControl`、日誌面板、樹 |
| C. 對話框 | 所有自訂的獨立小視窗，外觀與編排完全一致 | 31 個，見 1.2 |
| D. 系統與提示 | 進度視窗；`MessageBox`、開檔 / 存檔本次不調整 | `LoadingBarDlg` |
| E. 共用控制項 | 可重用的元件 | 見 1.6 |

授權視窗（`FrmLicenseActivation`、`FrmLicenseValidationFailed`）不納入本次範圍。

### 1.2 對話框基底：行為差異還是外觀差異

現在三種基底混用。逐一比對它們各自提供的東西：

| 基底 | 提供的東西 | 性質 |
| --- | --- | --- |
| `EditDialogBase<T>` | `Preview` / `Confirm` / `Cancel` / `Reset` 事件；關閉視窗（非確定）時觸發 `Cancel` | **行為** |
| `FormBase` | 覆寫 `Show( owner )`：設 `ShowInTaskbar = false`、`MaximizeBox` / `MinimizeBox = false`，然後置中在主視窗 | 置中：**外觀**（已決議）；工作列 / 最小化：外框，見 1.3 |
| `Form`（直接繼承） | 無 | — |

| 目前基底 | 對話框 |
| --- | --- |
| `EditDialogBase<T>` | `AutoSortPathDlg`、`CalibrationREFDlg`、`ContourEditDlg`、`DesignSupportBracketDlg`、`EntryAndExitDlg`、`ExtendedGCodeDlg`、`FlyCutDlg`、`LeadDlg`、`LockMSAxisDlg`、`ManualTransformDlg`、`MeasureDlg`、`MicroJointDlg`、`OverCutDlg`、`PathCompensateDlg`、`PathEditDlg`、`PatternSettingDlg`、`RCornerDlg`、`SewPartDlg`、`StartPointDlg`、`ThreePtTransformDlg`、`ToolVectorDlg`、`TraverseDlg` |
| `FormBase` | `AxisTransformDlg`、`GlassDXFImportDlg`、`OrderDialog` |
| `Form` | `AddPathDlg`、`ConnectDlg`、`CNCDlg`、`CNCThreePointDlg`、`CNCOffsetDialog` |

**結論**：`EditDialogBase<T>` 與其他兩者的差別是行為，本議題不動；`FormBase` 與 `Form` 的差別只有外觀（置中、外框），可以統一。

**做法**：新增只管外觀的 `DialogBase : Form`，放在繼承鏈最底層：

```
Form
└─ DialogBase            ← 新增：外框、字型、顏色、語系自動套用、置中
   ├─ FormBase           ← 保留，Show( owner ) 的行為不變（只是把置中交給 DialogBase）
   │  └─ EditDialogBase<T>  ← 保留，事件機制不變
   └─ （原本直接繼承 Form 的 5 個對話框改繼承 DialogBase）
```

- 每個對話框維持原本的行為基底，只是外觀全部來自 `DialogBase`。
- `Show` / `ShowDialog` 的呼叫點一律不動。
- 置中改在 `DialogBase.OnLoad` 處理，`Show` 與 `ShowDialog` 都會經過，所以不用改呼叫點也能統一。
- 之後做行為統一（後續處理項目）時，只動 `FormBase` / `EditDialogBase<T>` 這一層，外觀層原封沿用。

### 1.3 外框（`FormBorderStyle`）對行為的影響

目前的分布：

| 現況 | 對話框 |
| --- | --- |
| `FixedToolWindow` | 20 個（含 `CNCDlg`） |
| `FixedDialog` | `AddPathDlg`、`CNCOffsetDialog`、`CNCThreePointDlg`、`DesignSupportBracketDlg`、`LockMSAxisDlg` |
| `FixedSingle` | `AutoSortPathDlg`、`ToolVectorDlg` |
| 未設定（預設 `Sizable`，可拖曳改大小） | `ConnectDlg`、`FlyCutDlg`、`StartPointDlg` |

改成工具視窗外框會帶來的差異：

| 差異 | 影響的對話框 | 性質 | 處理 |
| --- | --- | --- | --- |
| 標題列變細、不顯示圖示 | 非 `FixedToolWindow` 的 10 個 | 外觀 | 統一 |
| 不能再拖曳改大小 | `ConnectDlg`、`FlyCutDlg`、`StartPointDlg` | **行為** | 決議統一：所有對話框都不可改大小 |
| 不再出現在工作列與 Alt+Tab | `AddPathDlg`、`ConnectDlg`（目前沒有設 `ShowInTaskbar = false`，也沒經過 `FormBase.Show`） | **行為** | 決議統一：所有對話框都不出現在工作列與 Alt+Tab |
| 最小化 / 最大化鈕 | 全部已經關閉（`FormBase.Show` 或 Designer 設定） | 無差異 | — |
| 視窗外框尺寸變小 | 全部 | 外觀 | Designer 存的是 `ClientSize`，內容區不變 |

`CNCDlg` 在直式時會定位到 CNC 區域（`PositionInArea`），這是它承載 VNC 畫面的用途，維持原樣，不套用一般的置中規則。

### 1.4 外觀分裂

| 項目 | 現況 |
| --- | --- |
| 字型 | 9 種組合：Arial 9 / 9.75 / 11.25 / 12 / 14.25、Microsoft Sans Serif 9、Microsoft JhengHei UI 9、新細明體 9 / 12。多數對話框沒指定字型，吃系統預設。Arial 沒有中文字，中文會由系統替代字型顯示。 |
| 縮放基準 | `AutoScaleDimensions` 有 9 種；33 個用 `Font` 縮放、4 個用 `None`。執行時的尺寸取決於當初設計時的字型，與 Designer 看到的不同。 |
| 群組容器 | 原生 `GroupBox` 25 個、自繪 `GroupPanel` 10 個（只在 CNC 相關畫面）。 |
| 按鈕 | 原生 `Button` 84 個、自繪 `PanelButton` 51 個（只在 CNC）。確定鍵的文字有 OK / Confirm / Sure / Done / Apply 五種，位置也不一致。 |
| 顏色 | 約 180 處寫死顏色，分散在 32 個檔案；強調藍就有 `(0,137,195)`、`(0,142,203)` 兩種。 |
| 工具列 | `ImageScalingSize` 有 20 / 24 / 40 三種；圖示以 Magenta 當透明色。 |
| 置中 | 三套做法：`FormBase.Show` → `DialogShowHelper` → `MyApp.CalculateDialogCenterLocation`；`LoadingBarDlg` 自己算；Designer 的 `StartPosition = CenterParent`（只對 `ShowDialog` 有效，`AddPathDlg` 用 `Show` 開啟，實際上沒有置中）。直式的判斷是看**螢幕**是否為直式，再把 Y 乘 0.5，與 `MainLayout` 以「主視窗高 > 寬」判斷不一致。 |
| DPI | `CNCPanel`、`CNCInfoPanel`、`GroupPanel`、`PanelButton` 自己計算 `DeviceDpi / 96`，其他畫面沒有處理。 |

### 1.5 主視窗：外觀與行為的切分

| 外觀（本議題處理） | 行為（本議題不動） |
| --- | --- |
| 選單、工具列的繪製（顏色、間距、選中 / 執行中狀態的樣子） | 工具列按鈕何時顯示 / 隱藏 / 啟用 |
| 工具列圖示與 `ImageScalingSize` | 編輯器切換、CAD / CAM / 模擬模式流程 |
| 工具列「執行中」的底色：**只換顏色來源**（改用主題 token），設定的時機不變 | 何時標示為執行中 |
| 各區域的邊框、間距、底色；樹與日誌的字型顏色 | CNC 面板的建立、關閉、何時顯示 |
| `MainLayout` 中純視覺的間距值 | `MainLayout` 決定哪些區域顯示、橫 / 直切換條件、各區比例 |
| | 拆分 `StartupForm.cs`（後續處理項目） |

**`MainLayout` 目前計算的內容**：主視窗大小改變、CNC 連線 / 斷線、進入 / 離開加工模式、警報出現 / 消失時重新計算。

| 項目 | 橫式（寬 ≥ 高） | 直式（高 > 寬） |
| --- | --- | --- |
| 判斷 | 主視窗寬 ≥ 高 | 主視窗高 > 寬 |
| 樹的欄寬 | 扣掉 CNC 操作面板後剩餘寬度的 22% | 15% |
| CNC 操作面板（`CNCPanel`） | 放在最右欄，欄寬 = 面板偏好寬度 × 1.1；只在面板已建立時顯示 | 移到下方 CNC 區的右側 45% |
| CNC 監控面板（`CNCInfoPanel`） | 放在底部列右側 | 移到下方 CNC 區左側 55% 的上方 80% |
| 底部列高 | 有 CNC 面板：取「監控面板偏好高度」與「主視窗高 25%」的較大者；沒有：一般 15%、加工模式 25% | 6% |
| 下方 CNC 區 | 不存在 | 固定佔 40%（不論 CNC 面板是否顯示） |
| 日誌 / 警報 | 有警報時，日誌佔訊息區上方 60%、警報佔下方 40% | 警報放在 CNC 區左側下方 20% |
| 其他 | 設定「加工」選單項目的啟用狀態 | 同左 |

實作方式：改寫主視窗 `TableLayoutPanel` 的欄 / 列比例與跨欄，並把 CNC 面板在兩個承載區之間搬移；直式 CNC 區內以 `SetBounds` 計算座標。
這些全部屬於版面切換的行為，本議題不動；只會調整承載區的邊距（`Margin`）這類純視覺的值，並讓 `MainLayout` 提供 CAM 區域的範圍給置中使用（3.4）。

### 1.6 共用控制項

| 控制項 | 位置 | 用途 | 相依 | 處理 |
| --- | --- | --- | --- | --- |
| `GroupPanel` | `CommonUI` | 有框線的卡片容器 | — | 重寫為 `SectionPanel`，取代 `GroupBox` |
| `PanelButton` | `CommonUI` | CNC 面板的按鈕，有燈號 | — | 併入 `ThemedButton` |
| `RoundedProgressBar`、`LoadingBar` | `CommonUI` | 進度條 | — | 改用主題 token |
| `DebouncedNumericUpDown` | `MyCAM/Editor/Dialog` | 防抖數值框 | 只依賴 WinForms | **移到 `CommonUI`** |
| `MultiSelectTreeView` | `MyCAM/Editor` | 可多選的樹（主視窗左側） | 只依賴 WinForms | **移到 `CommonUI`**；選取邏輯原樣搬移，不修改 |
| `SupportPositionRow` | `MyCAM/Editor/Dialog` | `DesignSupportBracketDlg` 裡可增刪的「支撐位置」列：序號 + 數值框 + 單位 mm + 刪除鈕，執行時依使用者按「新增 X / Y 位置」動態建立 | 只依賴 WinForms | **改成通用的「可增刪數值列」移到 `CommonUI`**（序號、單位、刪除文字由呼叫端給） |

規則：`MyCAM`、`CncUI` 不再自己定義控制項，所有控制項都在 `CommonUI`。三者都只依賴 WinForms，搬移只改命名空間，不改行為。

### 1.7 數值輸入盤點

目前用防抖數值框的有 47 個。以下**不是**防抖數值框的輸入，全部要改：

| 對話框 | 控制項 | 現況 |
| --- | --- | --- |
| `AxisTransformDlg` | `m_nudX/Y/Z/Rx/Ry/Rz` | 原生 `NumericUpDown` ×6 |
| `AxisTransformDlg` | `m_txbStepDistance`、`m_txbStepAngle` | `TextBox` ×2 |
| `AutoSortPathDlg` | `m_nudExtrusionBandwidth`、`m_nudRotationBandwidth` | 原生 `NumericUpDown` ×2 |
| `AddPathDlg` | `m_txbMaxLineLength`、`m_txbMaxChordError`、`m_txbD1Threshold` | `TextBox` ×3 |
| `PatternSettingDlg` | 圓、跑道、多邊形、矩形的尺寸與角度 | `TextBox` ×12 |
| `ThreePtTransformDlg` | `tbxMachineX1`…`Z3` | `TextBox` ×9 |
| `CNCThreePointDlg` | `m_txtWorkpiece1X`…`3Z`、`m_txtTolerance` | `TextBox` ×10 |

注意：`TextBox` 換成數值框會改變「輸入非數字時」的反應（原本是程式 `TryParse` 失敗後提示，數值框是直接擋掉或還原）。這是輸入元件本身的差異，已決議統一；施工時逐一確認原本的錯誤提示與範圍，用數值框的 `Minimum` / `Maximum` 對應。

以下 27 個只**顯示**點選座標的 `TextBox` 改為文字標籤；施工前逐一確認語意（包含執行時是否會切成可編輯）：

| 對話框 | 控制項 |
| --- | --- |
| `ThreePtTransformDlg` | `tbxCADX1`…`Z3`（停用） ×9 |
| `CNCThreePointDlg` | `m_txtDrawing1X`…`3Z`（唯讀） ×9 |
| `CalibrationREFDlg` | `m_tbxPnt1X`…`3Z`（唯讀） ×9 |

不是數值、維持 `TextBox` 的：`ConnectDlg.m_txbIP`、`MeasureDlg.m_txtResult`、`ExtendedGCodeDlg.m_txtGCode`、`DesignSupportBracketDlg.m_txbSavePath`。

### 1.8 多語系

每個畫面都有手寫的 `ApplyResourcesToUI()`（27 個，合計 438 行），逐一把控制項文字換成語系字串；其中 143 行的 ID 不等於控制項名稱。

改法（已決議）：

- 控制項自己的文字、且 ID 只對應這一個控制項：以控制項名稱當 ID，由 `DialogBase` 在建構完成後自動走訪套用。
- 表單標題：以類別名稱當 ID，同樣自動套用。
- 多個控制項共用的文字、執行時才決定的文字：仍在程式中明確呼叫 `TryGetString`。
- 現有 ID 與控制項名稱不一致的，改 ID 讓它等於控制項名稱，四個語系的 XML 同步修改；不考慮客製語系檔的舊 ID 相容性。

### 1.9 區塊標題盤點

語意上是「畫面區塊的標題」、但實作方式各不相同的元素：

| 實作方式 | 位置 |
| --- | --- |
| `GroupBox.Text`（25 個） | `AddPathDlg`（`m_grpStep1`、`m_grpStep3`）、`AxisTransformDlg`（平移、旋轉）、`CalibrationREFDlg`（參考點 1–3）、`ToolVectorDlg`（控制點參數）、`FlyCutDlg`（飛切參數）、`LockMSAxisDlg`（鎖軸切割、轉台方向調整、指定加工方向）、`OrderDialog`（排序模式）、`ExtendedGCodeDlg`、`TraverseDlg`（空移參數）、`ContourEditDlg` 等 |
| 加大字的 `Label` | `LeadDlg`（`m_lblLeadIn`「進入引線」、`label3`「引出線」，12pt） |
| 粗體 `Label` | `DesignSupportBracketDlg`（`m_lblSection*` ×6）、`CNCThreePointDlg`（`m_lblDrawingTitle`、`m_lblWorkpieceTitle`、`m_lbl*Step` ×3）、`AddPathDlg`（`m_lblStepDescription`） |
| CNC 面板的粗體 `Label` | `CNCPanel`（`m_lbl*Title` ×5）、`CNCInfoPanel`（`m_lbl*Title`、`m_lblHeightGroupTitle`）、`CNCAlarmPanel`（`m_lblTitle`） |

**不是**區塊標題、只是字比較大的：`PatternSettingDlg` 的欄位標籤（11.25pt）、`AxisTransformDlg` 的 X/Y/Z 軸名（14.25pt）、`CNCOffsetDialog` 與 `CNCInfoPanel` 的軸名與數值。這些改為「強調」與「數值」文字角色。

處理（沿用舊分支做法）：區塊標題一律是卡片（`SectionPanel`）的 `Text`，不另放標題 `Label`。精靈的步驟說明（`m_lblStepDescription`、`m_lbl*Step`）施工時再逐一確認是區塊標題還是說明文字。

---

## 2. 目標風格（沿用舊分支的外觀設計）

淺灰底上放白色卡片，只用一種鋼藍強調色；容器直角、按鈕小圓角；不用漸層與陰影。

### 2.1 色票

| Token | 色值 | 用途 |
| --- | --- | --- |
| `WINDOW` | `#F1F3F6` | 表單底色 |
| `SURFACE` | `#FFFFFF` | 卡片、輸入框、按鈕 |
| `SURFACE_ALT` | `#F8F9FB` | 表頭、唯讀欄位、停用按鈕 |
| `BORDER` / `BORDER_STRONG` | `#D6DCE4` / `#BDC6D1` | 卡片框線 / 按鈕框線 |
| `DIVIDER` | `#E4E8EE` | 表格格線、進度條軌道 |
| `TEXT` / `TEXT_SECONDARY` / `TEXT_DISABLED` | `#1E2936` / `#5E6A79` / `#A0A9B5` | 文字 |
| `ACCENT`（hover / pressed） | `#0067AD`（`#005A99` / `#004C82`） | 主要按鈕、選中、卡片標題短線 |
| `ACCENT_TINT` / `ACCENT_TINT_STRONG` | `#E8F1F9` / `#CFE2F3` | 選取列、工具執行中 |
| `HOVER` / `PRESSED` | `#ECF0F5` / `#DFE5EC` | 白底控件的滑過 / 按下 |
| `ERROR` / `WARNING` / `SUCCESS` | `#C42B1C` / `#A86200` / `#107C41` | 狀態 |

資料色（圖層色、3D 物件色、CNC 座標軸點）不是主題色，只能用在色塊、圖例、3D 物件上。

### 2.2 字型

- 字族依介面語系：zh-TW 用 `Microsoft JhengHei UI`，zh-CN 用 `Microsoft YaHei UI`，其他語系用 `Segoe UI`。
- 文字角色：內文 9pt、強調 9pt 粗體（卡片標題、軸名）、標題 10.5pt（主視窗模式頁籤）、數值 Consolas 10pt（持續變動的機台數值）、次要（單位、提示，次要文字色）。

### 2.3 尺寸（100%、96 DPI，單位 px）

| 項目 | 值 |
| --- | --- |
| 對話框邊距 / 內容與按鈕列間距 | 12 / 16 |
| 卡片間距 / 卡片內距 | 8 / 10 |
| 卡片標題列高 | 32 |
| 輸入框高 / 輸入列間距（兩列頂端距離） | 23 / 30 |
| 控件間距 | 6 |
| 按鈕高 / 最小寬 / 間距 | 28 / 80 / 8 |

### 2.4 圖示

沿用舊分支重繪的 60 個工具列圖示（24×24 網格，以 `IconSource/RenderIcons.ps1` 產生，檔名與現有圖示相同）。工具列統一 `ImageScalingSize`，不再用 Magenta 當透明色。

---

## 3. 重構方向

### 3.1 Designer 如實呈現

**字型與顏色在什麼時候、用什麼方式處理**：在**基底表單與自訂控制項的建構子**裡設定。

- WinForms Designer 開啟一個對話框時，會實際建立它的基底類別（`DialogBase`）與畫面上每個自訂控制項（`SectionPanel`、`ThemedButton`、數值框…）的實體，所以建構子裡設定的字型、顏色、外框，**在 Designer 編輯時就看得到**，與執行時相同。
- 原生的 `Label`、`CheckBox`、`RadioButton` 的字型與顏色是「環境屬性」，沒設定就繼承父容器，所以只要表單與卡片設定好，它們在 Designer 上也會正確顯示。
- 為了讓 Designer 存檔時不把這些值抄進各對話框的 `.Designer.cs`，基底與自訂控制項會把這些屬性標成「不序列化」。結果是 Designer 檔裡不會出現字型與顏色，但畫面上看得到。
- 語系字型：Designer 中一律顯示 zh-TW 字型；執行時依語系選字型。
- 前提：`CommonUI` 要先建置，Designer 才會載入最新的外觀；改了主題後要重新建置再開 Designer。

**對衍生對話框的意義**：外觀邏輯全部收斂在 `DialogBase` 與 `CommonUI` 的元件。衍生的對話框只要依規範在 Designer 排版，Designer 看到的就是執行時的樣子，`InitializeComponent` 之後**不寫任何外觀程式**（不設字型、顏色、座標、尺寸）。

Designer 看不到、但不屬於「衍生類別補寫外觀」的只有以下幾種，都由共用機制處理：

| 情況 | 處理方式 |
| --- | --- |
| 語系文字 | `DialogBase` 自動套用；Designer 上顯示寫在 Designer 的 zh-TW 文字 |
| zh-CN 字型 | 執行時依語系切換；Designer 固定顯示 zh-TW 字型 |
| 依狀態改變的外觀（例如狀態文字變紅、停用變灰） | 呼叫主題 API 指定文字角色，例如 `SetTextRole( TextRole.Error, m_lblStatus )`；不直接寫顏色 |
| 執行時顯示 / 隱藏區段、動態增刪列 | 呼叫共用的版面工具與共用元件，不在對話框內手算座標 |

現在有些對話框在程式中寫死顏色或自己調整位置（例如 `CNCThreePointDlg`、`ManualTransformDlg`、`LockMSAxisDlg`、`AutoSortPathDlg` 寫死顏色，`DesignSupportBracketDlg` 自己改尺寸與位置），遷移時逐一判斷：靜態外觀移回 Designer 與基底，依狀態變化的改走上表的共用機制。

**待驗證**：22 個對話框繼承泛型基底 `EditDialogBase<T>`。WinForms Designer 對泛型基底的支援有限，P1 會先用一個這類對話框確認你的 Visual Studio 能正常開啟並顯示 `DialogBase` 的外觀；若不行，再加一層非泛型的中介類別。

其他規則：

1. **關閉自動縮放**：所有表單 `AutoScaleMode = None`，Designer 上的座標就是執行時的像素。
2. **Designer 檔不寫字型、顏色、外框**：只設定結構、位置、尺寸、文字與行為屬性。
3. **不在執行時走訪控制項套主題**（Designer 看不到這一步）。唯一的例外是語系文字，它在 Designer 上顯示 zh-TW 的預設文字。
4. zh-CN 的字串長度無法在 Designer 看到，所以欄寬以兩語系中較長者設計，並由驗收腳本檢查。

### 3.2 版面：Designer 固定座標

**決議方向：對話框與嵌入式面板一律在 Designer 用固定座標排版，不使用 `TableLayoutPanel`。**

理由：

- 規格固定為 1920×1080、100%、兩個語系，不需要隨字型或解析度自動調整的版面。
- 固定座標在 Designer 中拖曳即所見即所得；`TableLayoutPanel` 在 Designer 中難以編輯（巢狀、列欄比例），且自動尺寸與實際顯示容易不一致。
- 對齊與間距的一致性改由規則（2.3）與驗收腳本保證，不靠容器自動排列。

例外：

- 主視窗的區域配置由 `MainLayout` 以 `TableLayoutPanel` 動態計算，維持不變（屬於橫 / 直切換的行為）。
- 執行時依狀態顯示 / 隱藏區段的對話框，用共用的版面工具重新計算座標，不在各對話框手算。
- 現有對話框中的 `TableLayoutPanel`（約 20 個）改為固定座標。

### 3.3 對話框外觀層 `DialogBase`

| 靜態（建構時） | 動態（執行時） |
| --- | --- |
| 外框：一律 `FixedToolWindow`（不可改大小、不出現在工作列與 Alt+Tab）；`ShowInTaskbar = false`；字型、底色；`AutoScaleMode = None` | `OnLoad`：置中（3.4）；語系文字自動套用 |
| 底部按鈕列、卡片、數值框都來自 `CommonUI` | 不處理預覽、確定、取消等行為（由原本的 `FormBase` / `EditDialogBase<T>` 負責） |

- 各對話框的 Designer 檔不再設定 `FormBorderStyle`、`StartPosition`、`MaximizeBox`、`MinimizeBox`、`ShowIcon`。
- `LoadingBarDlg` 改繼承 `DialogBase`，移除自己的置中邏輯；`ControlBox = false` 與「禁止關閉」是行為，維持不變。

### 3.4 置中

| 主視窗 | 置中位置 |
| --- | --- |
| 橫式（寬 ≥ 高） | 整個主視窗的中央 |
| 直式（高 > 寬） | 上半部 CAM 區域的中央：主視窗頂端到 CNC 承載區（`m_panCNCVertical`）頂端之間。不論 CNC 面板是否已顯示。 |

- 直式的判斷改為「主視窗高 > 寬」，與 `MainLayout` 一致；不再看螢幕方向、不再把 Y 乘 0.5。
- 置中位置由 `MainLayout` 提供 CAM 區域的範圍，由 `DialogBase` 統一計算，超出螢幕工作區時往內收。
- 自己會改變大小再重新置中的對話框（`DesignSupportBracketDlg`）改呼叫同一個計算。
- 例外：`CNCDlg` 在直式時定位到 CNC 區域，維持原樣。

### 3.5 共用元件（`CommonUI`）

| 元件 | 取代 | 說明 |
| --- | --- | --- |
| `SectionPanel` | `GroupBox`、`GroupPanel`、區塊標題 `Label` | 直角白底卡片；標題畫在卡片上 |
| `ThemedButton` | 原生 `Button`、`PanelButton` | 主要 / 一般 / 危險三種樣式；CNC 的燈號功能保留為屬性 |
| `DialogButtonBar` 排版規則 | 各對話框自己擺的確定 / 取消 / 重設 | 主要按鈕靠右、輔助按鈕靠左；按鈕的數量與功能不變，只統一位置、尺寸、樣式 |
| `DebouncedNumericUpDown` | 1.7 的所有數值輸入 | 從 `MyCAM` 移入 |
| `MultiSelectTreeView` | — | 從 `MyCAM` 移入，邏輯不變 |
| 可增刪數值列 | `SupportPositionRow` | 從 `MyCAM` 移入並通用化 |
| `ThemedToolStripRenderer` | 預設工具列繪製 | 主視窗選單、工具列共用 |

確定鍵的文字（OK / Confirm / Sure / Done / Apply）統一用詞屬於外觀，改為共用語系字串；按鈕的功能不變。

### 3.6 其他規則

- 程式中不再寫 `Color.FromArgb`、`new Font(...)`，一律取用主題 token；資料色例外，定義為具名常數。
- 以 96 DPI 設計、不處理 DPI 縮放，移除各控制項自己計算 `DeviceDpi / 96` 的程式碼。

---

## 4. 執行階段

| 階段 | 內容 | 產出 |
| --- | --- | --- |
| P0 | 本草案定稿 | `.github/instructions/ui-style.instructions.md` |
| P1 | 基礎建設：主題 token、`DialogBase`、共用元件（含三個控制項移入 `CommonUI`）、圖示替換、檢查腳本的靜態部分 | 一個示範對話框，在 Designer 與執行時比對 |
| P2 | 主視窗殼層的外觀（1.5 左欄） | |
| P3 | 對話框逐一遷移：外框、版面、區塊標題、數值輸入、語系自動化 | 分批 commit，每批跑檢查腳本 |
| P4 | CNC 面板、進度視窗 | |
| P5 | 驗收：檢查腳本全部通過；所有畫面在 zh-TW / zh-CN × 橫 / 直式截圖 | 檢查報告 + 截圖清單 |

### 4.1 驗收腳本（比照舊分支 `Tools/UICheck`，依本草案重寫）

| 腳本 | 時機 | 檢查內容 |
| --- | --- | --- |
| 靜態檢查 | 不需建置，讀原始碼 | 控制項型別只來自 `CommonUI` 與白名單；`AutoScaleMode = None`；Designer 檔沒有字型、顏色、外框設定；所有對話框繼承 `DialogBase`；程式中沒有 `Color.FromArgb` / `new Font`（資料色除外）；數值輸入都是防抖數值框；版面符合 2.3（邊距、卡片內距、列距、按鈕列）；語系 ID 在 zh-TW / zh-CN XML 都存在 |
| 行為不變檢查 | 與改動前的版本比對 | 事件掛接（`+=`）不變；RadioButton 群組成員不變；非外觀屬性（數值範圍、選項、`Checked`、`Enabled`、`Visible`、`AcceptButton` / `CancelButton`…）不變；程式有用到的控制項沒有被刪；`Show` / `ShowDialog` 呼叫點不變 |
| 執行時版面檢查 | 建置 Debug 後，實際開啟每個對話框與 CNC 面板，zh-TW / zh-CN 各跑一次 | 文字超出控制項；自動尺寸的文字撞到右側控制項或超出容器；卡片標題超出卡片；同層控制項重疊；控制項超出容器；並輸出截圖 |

---

## 5. 待討論

目前沒有待決問題。
