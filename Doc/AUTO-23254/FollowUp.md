# 5. 後續處理

## 5.1 合併前

| # | 事項 | 做法 | 負責 |
| --- | --- | --- | --- |
| 1 | 同步 `master` | `git fetch origin` 後 merge / rebase `master`，重新建置，跑 `Tools\UICheck\UICheck.bat runtime` 與 `Doc\AUTO-23254\Tests\RunIssueChecks.bat`。已知 `master` 前進兩個 commit（`8db7be81`、`7829c338`），只改開發環境檔案，與本議題的檔案沒有交集 | AI |
| 2 | 推送 | 推送前確認 remote 與分支 | 使用者決定，AI 執行 |
| 3 | Merge Request 說明 | release note，並附 [2_Spec.md](2_Spec.md) 2.3 的相容性異動 | AI 起草，使用者確認 |
| 4 | 開議題單 | 依 5.3 的清單 | 使用者 |

## 5.2 範圍外的後續議題

本議題只統一外觀；以下屬於行為、既有問題或其他範疇。

### 5.2.1 對話框行為

延後原因：改變事件機制、開啟方式或操作流程，屬於行為；需逐一確認各 Action 的操作規格。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | 行為基底統一 | 三種行為基底（`EditDialogBase<T>`、`FormBase`、直接繼承 `DialogBase`）的事件機制（預覽 / 確定 / 取消 / 重設）不一致 | 訂一個行為基底與事件流程，各對話框改用；**必須沿用本議題的外觀層（`DialogBase` 與 `CommonUI` 元件）** |
| 2 | 開啟方式統一 | 多數以 `Show( owner )` 非強制回應開啟，`GlassDXFImportDlg`、`ConnectDlg` 以 `ShowDialog`；開啟入口有 `FormBase.Show`、`DialogShowHelper.ShowCentered`、直接 `Form.Show` 三種 | 與第 1 項一起訂出唯一入口；強制回應與否逐一確認 |
| 3 | `DialogShowHelper.SetFormConfig` 的重複設定 | 執行時仍設定 `ShowInTaskbar`、`MaximizeBox`、`MinimizeBox`，與 `DialogBase` 固定的設定重複 | 確認 `DialogShowHelper` 開啟的對話框都繼承 `DialogBase` 後刪除 |
| 4 | 補「取消」按鈕 | 部分對話框只能按 X 取消；有些 Action 可能刻意不讓使用者取消 | 逐一確認各 Action 後補上 |
| 5 | 精靈的步驟流程元件 | 三個精靈的版面已統一，但步驟列舉、上一步 / 下一步、依步驟顯示的按鈕、步驟標題的狀態文字、分流仍各寫一份 | 在 `CommonUI` 做步驟流程元件，以參數選擇展開式或切換式 |

### 5.2.2 輸入控制項

延後原因：改變輸入行為或選項的操作方式，需逐一驗證。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | 可為空的數值框 | `CNCThreePointDlg` 工件點 9 欄與誤差容忍欄以空白代表「未輸入」，工件點可「擷取」也必須可手動輸入，維持 `TextBox`（`StaticExceptions.txt` 兩行例外） | `CommonUI.Controls` 新增可為空的數值框（空白時以 `decimal?` 回傳 null），另訂空白時上下鍵 / 滾輪的起始值；需連 CNC 實測；完成後移除例外與指引中的特例 |
| 2 | 下拉選單改為單選鈕 | 8 個對話框共 12 個 `ComboBox`：`AddPathDlg`（`m_cmbDiscreteMode`）、`AutoSortPathDlg`（`m_cmbSortMethod`、`m_cmbCylinderAxis`、`m_cmbExtrusionDir`、`m_cmbRotationDir`）、`LockMSAxisDlg`（`m_cmbMachiningDir`）、`ManualTransformDlg`（`m_cmbMethod`、`m_cmbAxisPlane`）、`MeasureDlg`（`m_cmbMethod`）、`PathEditDlg`（`m_cmbRefCoordType`）、`PatternSettingDlg`（`m_cmbPathType`）、`ToolVectorDlg`（`m_cmbInterpolateType`） | 1. 選項多的先確認版面放得下。2. 執行時變動選項的改為切換 `RadioButton` 的 `Visible` / `Enabled`。3. 改寫以 `SelectedIndex` 寫的邏輯並逐一驗證。4. 完成後從白名單與指引的特例移除 `ComboBox` |

### 5.2.3 主視窗

延後原因：版面行為或流程重構，不屬於外觀統一；第 4 項為既有問題。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | 主視窗行為重構 | `StartupForm.cs` 約 2000 行，混雜編輯器切換、工具列顯示 / 隱藏、CNC 面板管理 | 依職責拆分 |
| 2 | 橫式加工頁的警報 / 日誌切頁 | 訊息區是樹下方的一欄時，日誌與警報上下分割（40% / 60%），空間都很小 | 改為切頁：位置不變，同時只顯示一個並佔滿該欄（2026-11-22 使用者訂的規格）。另需決定新警報是否自動切頁、頁籤是否顯示警報數、沒有警報時是否只顯示日誌；切頁元件放在 `CommonUI`；與第 1 項一起做 |
| 3 | 切換語系後的字族 | 字型只在建立時套用：切換語系後，CNC 面板的本文與 `ThemedLabel` 依角色的字型停在舊字族，卡片標題是新字族；已開啟的對話框停在舊語系。語系不會在同一環境頻繁切換，影響小 | `MainWindowBase.ApplyThemeFont()` 連帶遞迴重設子控制項的主題字型 |
| 4 | 直式主視窗 CAM 工具列溢出 | 既有問題，與本議題無關 | 另開 bug |

### 5.2.4 既有缺陷

延後原因：分支起點即存在的行為，本議題不改行為。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | `CalibrationREFDlg` KeyDown 重複訂閱 | Designer 與 `Init()` 都訂閱，按 Delete 觸發兩次 `DeleteKeyDown` | 刪除其中一處訂閱，確認 Delete 只刪一個點 |

### 5.2.5 檢查與驗證

延後原因：需要假資料、實際連線環境或新的檢查方式，成本高於本議題的範圍。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | 執行時才填入的文字 | 約 40 個控制項的文字在執行時才填入，zh-CN 下是否截斷未自動檢查 | 以假資料或實際連線填入後檢查 |
| 2 | CNC 面板的連線狀態 | 以產品方法 `SetCommandsEnabled` 呈現連線後的按鈕；`CNCInfoPanel`、`CNCAlarmPanel` 未實際連線檢查 | 若有新代 PC 模擬器或固定機台，runner 加選用參數（例 `-CncIp`）實際連線後檢查；需先確認環境 |
| 3 | 嵌入式面板的卡片座標 | 沒有靜態規則，由執行時的 `TextClipped`、`TextOverflow`、`TitleOverflow`、`ControlOverlap`、`PanelInWindow` 在實際大小下檢查 | 評估是否需要等同 `DialogGrid` 的靜態規則 |
| 4 | 控制項名稱的 PascalCase | `FieldPrefix` 只檢查「前綴 + 大寫或數字開頭」，不檢查完整的 PascalCase；底線可用於分隔語意（2026-11-23 決議） | 需要時再收緊規則 |
| 5 | 行為比對的做法 | 本議題的行為比對只比對宣告（事件、屬性、狀態賦值） | 之後的議題需要行為比對時，以 `Tests/CompareBehavior.ps1` 為範本，在該議題另建並指定自己的基準 |

### 5.2.6 程式碼規範

延後原因：本議題只修正自己寫入的部分，不為了套用規範而改動無關的既有程式碼。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | 既有程式碼的通用規範 | 大量超過 120 字元的行、一檔多個頂層型別、語系 XML 沒有 BOM 或檔尾換行 | 以 `Tests/CheckBranchRules.ps1` 為範本做成可持續使用的檢查，再分批整理 |

### 5.2.7 技術評估

延後原因：需重寫所有畫面與檢查，使用者尚未決定是否進行。

| # | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| 1 | 評估改用 WPF | Designer 重新產生程式碼、手排座標、DPI 縮放等問題在 WPF 大多不存在；OCCT 3D 檢視與 VNC 遠端畫面要以 `WindowsFormsHost` 或 D3D interop 承載，是最大風險 | 另開評估議題，以一個對話框加上 3D 檢視與 VNC 的承載做概念驗證再決定；本議題的 token 與規範可沿用 |

### 5.2.8 待現場回饋

已決議接受的外觀統一帶來的細節差異，現場有問題再處理，不預先開單。

| # | 項目 | 現況 | 回饋時的處理方式 |
| --- | --- | --- | --- |
| 1 | 數值框取代 `TextBox` 的細節 | 可用上下鍵 / 滾輪；小數位數固定（`AddPathDlg` 弦高誤差 4 位，其餘 3 位）；`ThreePtTransformDlg` 機械座標不會再空白 | 調整該數值框的範圍或小數位數 |
| 2 | 唯讀座標無法複製 | 三個對話框的唯讀座標改為標籤後不能選取複製 | 評估提供複製按鈕 |
| 3 | 「新增路徑」的「進階參數」卡片偏大 | 卡片替錯誤訊息預留固定空間，分支起點也預留 | 2026-11-22 決議先不改 |

## 5.3 議題清單

依 5.2 整理，可直接開單；分支類型 `f` 為功能、`b` 為 bug。

| # | 議題 | 類型 | 內容 | 相依與注意事項 |
| --- | --- | --- | --- | --- |
| 1 | 對話框行為統一 | f | 5.2.1 全部 5 項 | 項目彼此相關，一起規劃；沿用本議題的外觀層 |
| 2 | 可為空的數值框 | f | 5.2.2 第 1 項 | 需連 CNC 實測 |
| 3 | 下拉選單改為單選鈕 | f | 5.2.2 第 2 項 | 與第 2 項無相依，可先做 |
| 4 | 主視窗行為重構 | f | 5.2.3 第 1、2 項 | 警報 / 日誌切頁依附在重構後的結構上 |
| 5 | 切換語系後字族不一致 | b | 5.2.3 第 3 項 | 影響小，可排低優先 |
| 6 | 直式主視窗 CAM 工具列溢出 | b | 5.2.3 第 4 項 | — |
| 7 | `CalibrationREFDlg` Delete 觸發兩次 | b | 5.2.4 第 1 項 | — |
| 8 | UI 檢查補強 | f | 5.2.5 第 1～4 項 | 第 2 項需先確認模擬器或機台環境 |
| 9 | 既有程式碼通用規範整理 | f | 5.2.6 第 1 項 | 先做檢查再分批整理，避免一次大量異動 |
| 10 | 評估改用 WPF | f | 5.2.7 第 1 項 | 使用者決定是否進行後再開單 |

5.2.5 第 5 項是給之後議題的做法說明，5.2.8 等現場回饋，兩者不開單。
