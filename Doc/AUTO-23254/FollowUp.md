# 後續處理

## 1. 範圍外的後續議題

本議題只統一外觀；以下屬於行為、既有問題或其他範疇。

### 1.1 對話框行為

延後原因：改變事件機制、開啟方式或操作流程，屬於行為；需逐一確認各 Action 的操作規格。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-1 | 行為基底統一 | 三種行為基底（`EditDialogBase<T>`、`FormBase`、直接繼承 `DialogBase`）的事件機制（預覽 / 確定 / 取消 / 重設）不一致 | 訂一個行為基底與事件流程，各對話框改用；**必須沿用本議題的外觀層（`DialogBase` 與 `CommonUI` 元件）** |
| TODO-2 | 開啟方式統一 | 多數以 `Show( owner )` 非強制回應開啟，`GlassDXFImportDlg`、`ConnectDlg` 以 `ShowDialog`；開啟入口有 `FormBase.Show`、`DialogShowHelper.ShowCentered`、直接 `Form.Show` 三種 | 與 TODO-1 一起訂出唯一入口；強制回應與否逐一確認 |
| TODO-3 | `DialogShowHelper.SetFormConfig` 的重複設定 | 執行時仍設定 `ShowInTaskbar`、`MaximizeBox`、`MinimizeBox`，與 `DialogBase` 固定的設定重複 | 確認 `DialogShowHelper` 開啟的對話框都繼承 `DialogBase` 後刪除 |
| TODO-4 | 補「取消」按鈕 | 部分對話框只能按 X 取消；有些 Action 可能刻意不讓使用者取消 | 逐一確認各 Action 後補上 |
| TODO-5 | 精靈的步驟流程元件 | 三個精靈的版面已統一，但步驟列舉、上一步 / 下一步、依步驟顯示的按鈕、步驟標題的狀態文字、分流仍各寫一份 | 在 `CommonUI` 做步驟流程元件，以參數選擇展開式或切換式 |

### 1.2 輸入控制項

延後原因：改變輸入行為或選項的操作方式，需逐一驗證。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-6 | 可為空的數值框 | `CNCThreePointDlg` 工件點 9 欄與誤差容忍欄以空白代表「未輸入」，工件點可「擷取」也必須可手動輸入，維持 `TextBox`（`StaticExceptions.txt` 兩行例外） | `CommonUI.Controls` 新增可為空的數值框（空白時以 `decimal?` 回傳 null），另訂空白時上下鍵 / 滾輪的起始值；需連 CNC 實測；完成後移除例外與指引中的特例 |
| TODO-7 | 下拉選單改為單選鈕 | 8 個對話框共 12 個 `ComboBox`：`AddPathDlg`（`m_cmbDiscreteMode`）、`AutoSortPathDlg`（`m_cmbSortMethod`、`m_cmbCylinderAxis`、`m_cmbExtrusionDir`、`m_cmbRotationDir`）、`LockMSAxisDlg`（`m_cmbMachiningDir`）、`ManualTransformDlg`（`m_cmbMethod`、`m_cmbAxisPlane`）、`MeasureDlg`（`m_cmbMethod`）、`PathEditDlg`（`m_cmbRefCoordType`）、`PatternSettingDlg`（`m_cmbPathType`）、`ToolVectorDlg`（`m_cmbInterpolateType`） | 1. 選項多的先確認版面放得下。2. 執行時變動選項的改為切換 `RadioButton` 的 `Visible` / `Enabled`。3. 改寫以 `SelectedIndex` 寫的邏輯並逐一驗證。4. 完成後從白名單與指引的特例移除 `ComboBox` |

### 1.3 主視窗

延後原因：版面行為或流程重構，不屬於外觀統一；TODO-11 為既有問題。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-8 | 主視窗行為重構 | `StartupForm.cs` 約 2000 行，混雜編輯器切換、工具列顯示 / 隱藏、CNC 面板管理 | 依職責拆分 |
| TODO-9 | 橫式加工頁的警報 / 日誌切頁 | 訊息區是樹下方的一欄時，日誌與警報上下分割（40% / 60%），空間都很小 | 改為切頁：位置不變，同時只顯示一個並佔滿該欄（2026-11-22 使用者訂的規格）。另需決定新警報是否自動切頁、頁籤是否顯示警報數、沒有警報時是否只顯示日誌；切頁元件放在 `CommonUI`；與 TODO-8 一起做 |
| TODO-10 | 切換語系後的字族 | 字型只在建立時套用：切換語系後，CNC 面板的本文與 `ThemedLabel` 依角色的字型停在舊字族，卡片標題是新字族；已開啟的對話框停在舊語系。語系不會在同一環境頻繁切換，影響小 | `MainWindowBase.ApplyThemeFont()` 連帶遞迴重設子控制項的主題字型 |
| TODO-11 | 直式主視窗 CAM 工具列溢出 | 既有問題，與本議題無關 | 另開 bug |

### 1.4 既有缺陷

延後原因：分支起點即存在的行為，本議題不改行為。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-12 | `CalibrationREFDlg` KeyDown 重複訂閱 | Designer 與 `Init()` 都訂閱，按 Delete 觸發兩次 `DeleteKeyDown` | 刪除其中一處訂閱，確認 Delete 只刪一個點 |

### 1.5 檢查與驗證

延後原因：需要假資料、實際連線環境或新的檢查方式，成本高於本議題的範圍。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-13 | 執行時才填入的文字 | 約 40 個控制項的文字在執行時才填入，zh-CN 下是否截斷未自動檢查 | 以假資料或實際連線填入後檢查 |
| TODO-14 | CNC 面板的連線狀態 | 以產品方法 `SetCommandsEnabled` 呈現連線後的按鈕；`CNCInfoPanel`、`CNCAlarmPanel` 未實際連線檢查 | 若有新代 PC 模擬器或固定機台，runner 加選用參數（例 `-CncIp`）實際連線後檢查；需先確認環境 |
| TODO-15 | 嵌入式面板的卡片座標 | 沒有靜態規則，由執行時的 `TextClipped`、`TextOverflow`、`TitleOverflow`、`ControlOverlap`、`PanelInWindow` 在實際大小下檢查 | 評估是否需要等同 `DialogGrid` 的靜態規則 |
| TODO-16 | 控制項名稱的 PascalCase | `FieldPrefix` 只檢查「前綴 + 大寫或數字開頭」，不檢查完整的 PascalCase；底線可用於分隔語意（2026-11-23 決議） | 需要時再收緊規則 |
| TODO-17 | 行為比對的做法 | 本議題的行為比對只比對宣告（事件、屬性、狀態賦值） | 之後的議題需要行為比對時，以 `Tests/CompareBehavior.ps1` 為範本，在該議題另建並指定自己的基準 |

### 1.6 程式碼規範

延後原因：本議題只修正自己寫入的部分，不為了套用規範而改動無關的既有程式碼。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-18 | 既有程式碼的通用規範 | 大量超過 120 字元的行、一檔多個頂層型別、語系 XML 沒有 BOM 或檔尾換行 | 以 `Tests/CheckBranchRules.ps1` 為範本做成可持續使用的檢查，再分批整理 |

### 1.7 技術評估

延後原因：需重寫所有畫面與檢查，使用者尚未決定是否進行。

| TODO | 項目 | 現況 | 建議處理方式 |
| --- | --- | --- | --- |
| TODO-19 | 評估改用 WPF | Designer 重新產生程式碼、手排座標、DPI 縮放等問題在 WPF 大多不存在；OCCT 3D 檢視與 VNC 遠端畫面要以 `WindowsFormsHost` 或 D3D interop 承載，是最大風險 | 另開評估議題，以一個對話框加上 3D 檢視與 VNC 的承載做概念驗證再決定；本議題的 token 與規範可沿用 |

### 1.8 待現場回饋

已決議接受的外觀統一帶來的細節差異，現場有問題再處理，不預先開單。

| TODO | 項目 | 現況 | 回饋時的處理方式 |
| --- | --- | --- | --- |
| TODO-20 | 數值框取代 `TextBox` 的細節 | 可用上下鍵 / 滾輪；小數位數固定（`AddPathDlg` 弦高誤差 4 位，其餘 3 位）；`ThreePtTransformDlg` 機械座標不會再空白 | 調整該數值框的範圍或小數位數 |
| TODO-21 | 唯讀座標無法複製 | 三個對話框的唯讀座標改為標籤後不能選取複製 | 評估提供複製按鈕 |
| TODO-22 | 「新增路徑」的「進階參數」卡片偏大 | 卡片替錯誤訊息預留固定空間，分支起點也預留 | 2026-11-22 決議先不改 |

### 1.9 併入 `master` 時發現

#### TODO-23 `master` 的應用程式圖示路徑

1. 目標：`master` 在乾淨的 checkout 可以建置。
2. 完成條件：`MyCAM.csproj` 的 `ApplicationIcon` 與 `Content` 指向 `Resource\AppIcon.ico`，乾淨 checkout 的 `Build.bat Debug` 成功。
3. 延後原因：屬於 `master`（AUTO-23275）的問題；本分支已修正，但 `master` 不應等本議題合併（DEC-14）。
4. 建議做法：另開 bug，只改 `MyCAM.csproj` 的兩處路徑；本議題合併時以本分支的修改為準。

### 1.10 客戶現場的螢幕

#### TODO-24 客戶螢幕不是 1920×1080、100% 時的支援

1. 目標：客戶現場的螢幕不符規格時，產品仍可正常操作。
2. 完成條件：盤點客戶現場的螢幕配置，訂出支援的解析度與縮放並寫入規格，`UICheck` 在這些配置下也能檢查。
3. 延後原因：產品部署與規格的變更，不是截圖環境（DEC-16）。虛擬螢幕本身無法解決，因為操作員看不到它。
4. 建議做法：
   1. 長期：先盤點，再讓產品適應客戶的螢幕（版面縮小或捲動、DPI 縮放）。
   2. 盤點之前的先行策略：1920×1080 但縮放不是 100% 的，改為 100%；解析度低於 1920×1080 的，安裝 VirtualDrivers VDD 並與實體螢幕設為同步顯示（畫面會縮小、略糊，但可操作，開發用的終端機已以此方式運作；手動步驟見 [Appendix-VirtualDisplay.md](Appendix-VirtualDisplay.md)）；解析度高於 1920×1080 的維持現狀，盤點時確認。
   3. 先行策略用於客戶機台前先驗證：觸控螢幕在同步顯示加縮放後點擊位置是否一致、MyCAM 的 3D 畫面在客戶的電腦上是否順暢、客戶 IT 是否接受安裝第三方驅動（需要系統管理員權限）。

### 1.11 截圖環境

#### TODO-25 在一般工作電腦實測 Parsec 虛擬副螢幕

1. 目標：確認沒有規格螢幕的一般工作電腦能以 Parsec 虛擬副螢幕完成 `UICheck.bat runtime`。
2. 完成條件：在沒有規格螢幕（如 1366×768 或縮放 125%）、未設終端機標記的電腦上執行 `UICheck.bat runtime` 全部通過，執行後既有螢幕維持原狀、虛擬螢幕已移除；結果補入 `TestPlan.md`。
3. 延後原因：開發用的終端機已有規格螢幕，走不到這條路（`TestPlan.md` 第 8 節第 6 項，DEC-16）。
4. 建議做法：由第一位在這類電腦上執行 UICheck 的同仁實測；失敗時依 `project/tool-uicheck.md` 第 2.4 節的狀況表處理，並回報。

## 2. 議題清單

依第 1 節整理，可直接開單；分支類型 `f` 為功能、`b` 為 bug。

| # | 議題 | 類型 | 內容 | 相依與注意事項 |
| --- | --- | --- | --- | --- |
| 1 | 對話框行為統一 | f | TODO-1～TODO-5 | 項目彼此相關，一起規劃；沿用本議題的外觀層 |
| 2 | 可為空的數值框 | f | TODO-6 | 需連 CNC 實測 |
| 3 | 下拉選單改為單選鈕 | f | TODO-7 | 與 TODO-6 無相依，可先做 |
| 4 | 主視窗行為重構 | f | TODO-8、TODO-9 | 警報 / 日誌切頁依附在重構後的結構上 |
| 5 | 切換語系後字族不一致 | b | TODO-10 | 影響小，可排低優先 |
| 6 | 直式主視窗 CAM 工具列溢出 | b | TODO-11 | — |
| 7 | `CalibrationREFDlg` Delete 觸發兩次 | b | TODO-12 | — |
| 8 | UI 檢查補強 | f | TODO-13～TODO-16 | TODO-14 需先確認模擬器或機台環境 |
| 9 | 既有程式碼通用規範整理 | f | TODO-18 | 先做檢查再分批整理，避免一次大量異動 |
| 10 | 評估改用 WPF | f | TODO-19 | 使用者決定是否進行後再開單 |
| 11 | `master` 應用程式圖示路徑錯誤 | b | TODO-23 | 不需等本議題合併 |
| 12 | 客戶螢幕不符規格的支援 | f | TODO-24 | 先盤點客戶現場；先行策略用於客戶機台前先驗證 |

TODO-17 是給之後議題的做法說明，TODO-20～TODO-22 等現場回饋，兩者不開單；TODO-25 由同仁執行 UICheck 時順帶實測，不開單。
