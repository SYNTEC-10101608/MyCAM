# 13. Round 3 Review 回覆

## 13.1 回覆資訊與結論

| 項目 | 內容 |
| --- | --- |
| 回覆對象 | [12_ReviewRound3.md](12_ReviewRound3.md)（審查版本 `b75f03dd`，快照於 `88f71649` 提交，不修改） |
| 回覆日期 | 2026-11-22 |
| 修正版本 | `e5c38e8d`（本文件在其後的 commit；`review-reply-done.txt` 不提交） |
| 決議紀錄 | [4_Discussion.md](4_Discussion.md) 4.17 |
| 實作紀錄 | [3_Implementation.md](3_Implementation.md) 3.30 |

**3 個問題與文件收尾建議查證後全部屬實，全部照 review 的建議方向處理。**
人工驗收（5.3 第 2～4 項）仍未完成，三項都是合併前必做，本分支仍不是可合併狀態。

## 13.2 逐項回覆

| ID | 查證 | 處理 | 驗證 |
| --- | --- | --- | --- |
| R3-01 | 屬實；以 12.6 的步驟重現 4 個序列，另有 1.23456 輸入 `2000` 按下鍵變 999 | 修正控制項：上下鍵先走拒絕 / 驗證流程，再從最新合法值加減 | N3 新案例；反向測試重現 review 結果 |
| R3-02 | 屬實；實驗確認目前沒有藏著的差異，缺口只在已刪除的舊型別 | 修正比對：型別不同就比對生效值；已刪除的舊型別從分支起點的原始碼編譯 | `RunIssueChecks.bat` 0 個差異；9 個 fixture；允許清單注入實測 |
| R3-03 | 屬實 | 補時間門檻：最後一次變化後間隔內沒有通知 | 拿掉 `Stop()` 的反向測試 |
| 12.5 文件 | 屬實 | 5 章首段與結果表寫明驗證版本 | 文件 |

## 13.3 各項說明

1. **R3-01：上下鍵先處理尚未驗證的輸入**
   1. 原因：`UpButton` / `DownButton` 只檢查 `ReadOnly` 就交給原生流程；原生流程先自行解析編輯中的文字並夾到上下限，
      我們的拒絕流程沒有機會執行。鍵盤的上下方向鍵走同一個入口。
   2. 修正：原生加減之前先呼叫 `ValidateEditText`。被拒的文字依拒絕流程回復（不改值、不送出 `ValueChanged`），
      合法的文字先被接受；之後原生流程看到的已是合法值，從它加減一步。沒有使用者輸入時 `ValidateEditText` 直接返回（3.29），
      所以一般的連按不受影響。
   3. 決議「回復後仍執行該次加減」（21 / 19），與滾輪、先按 Enter 再按鍵的結果一致；不採用「被拒時不加減」。
   4. 測試（N3 `CheckUpDownButtons`）：起值 20、範圍 0～1000，`2000` / `-1` 之後直接按上 / 下鍵為 21 / 19，`ValueChanged` 各只有一次；
      1.23456 輸入 `2000` 按下鍵為 0.23456；合法的 `30` 按上鍵為 `[30, 31]`。輸入只寫入編輯框，不先驗證，與 12.6 相同。
   5. 反向測試：換回 Round 2 版本的控制項，N3 報 10 筆（`2000` 按上鍵 1000 / `[1000]`、按下鍵 999 / `[1000, 999]`、`-1` 按上鍵 1 / `[0, 1]`、
      按下鍵 0 / `[0]`、1.23456 的案例 999），與 12.4 的表格相同。
2. **R3-02：B3 生效值涵蓋所有型別替換**
   1. 先做實驗：改成「型別不同就比對生效值」後，現存型別之間的替換（`Button` → `ThemedButton`、`Label` → `ThemedLabel`、
      `GroupBox` / `Panel` → `SectionPanel`、`DataGridView` → `ThemedDataGridView`、版面容器 → `Panel` 等）是 0 個差異；
      報出的全部是「無法比對」，來自本議題刪除的 4 種舊型別：`PanelButton`、`GroupPanel`、`MyCAM.Editor.DebouncedNumericUpDown`
      與它在 Designer 中未寫命名空間的寫法 `DebouncedNumericUpDown`。
   2. 修正（`CompareBehavior.ps1`）：
      1. B6 是否允許替換與 B3 是否比對生效值分開判斷：只要 CLR 型別不同就比對生效值。
      2. 已刪除的舊型別以分支起點的原始碼（`git show a4b3826d:`，連同依賴的 `RoundDrawing.cs`）用 runner 同一套 Roslyn `csc` 編譯到暫存資料夾，
         從記憶體載入後立即刪除資料夾；建立實例讀預設值，與現存型別相同。沒有手寫預設值表。
      3. 判斷拆成回傳差異的 `Get-EffectiveDifference` 與報告用的 `Compare-EffectiveValue`。
   3. Fixture（每次執行都跑，結果不符列為問題）：
      1. 預設值相同：`Button` → `ThemedButton` 的 `Enabled`、`Label` → `ThemedLabel` 的 `TabStop`、舊 / 新數值框的 `DecimalPlaces`、`GroupPanel` → `SectionPanel` 的 `Enabled`。
      2. 預設值不同：`NumericUpDown` → `DebouncedNumericUpDown` 的 `DecimalPlaces`。
      3. 只有一邊有寫：`Button` → `ThemedButton` 新寫 `Enabled = false`、`PanelButton` → `ThemedButton` 新寫 `Visible = false`、舊數值框 → 新數值框新寫 `ReadOnly = true`（報出）；
         兩邊都寫 `Enabled = false`（不報）。
   4. 反向測試（比對副本）：把 `NumericUpDown` → `DebouncedNumericUpDown` 加入 B6 允許清單、拿掉 `AxisTransformDlg` 的 `Minimum` 例外，
      新判斷報 3 筆 `Minimum changed (effective): '0' -> '-99999'`，Round 2 的判斷 0 筆；把一個 fixture 的預期反過來，多報 1 個問題。
   5. 保留：容器的 `TabStop` 規則、屬性級的 B3 例外。改後沒有新增例外。
3. **R3-03：debounce 時間門檻**
   1. N2 的連續變化改為間隔 500 ms，兩次變化相隔約 200 ms：斷言最後一次變化後 400 ms 內通知 0 次，之後只通知一次 21。
   2. 拿掉 `OnValueChanged` 的 `m_DebounceTimer.Stop()` 時，計時器以第一次變化為準，約在最後一次變化後 300 ms 通知，N2 報 1 筆。
   3. 沒有加「首次通知距最後一次變化 ≥ 間隔」的下限斷言：量測時間點與計時器啟動點有毫秒級落差，容易不穩定；
      上述 400 ms 的靜默窗口已足以抓到提早通知，且不放寬「不可提早」。
   4. 另外：事件序列不符時，訊息改為寫出實際的事件名稱（原本 `DebouncedValueChanged` 也寫成 `ValueChanged`）。
4. **12.5 文件**：`5_Verification.md` 首段改為「Round 3 修正（3.30）的工作目錄，修正 commit 見本文件」；分支規範檢查的結果寫明日期與版本。
   12.3 量到的 353 是 `b75f03dd` 加上 review 檔的工作目錄，本輪修正的工作目錄為 354，兩者時點不同。

## 13.4 驗證結果

以下在修正版本 `e5c38e8d` 的工作目錄執行（2026-11-22）。

| 驗證 | 結果 |
| --- | --- |
| `Build.bat Debug` | 成功，0 個錯誤 |
| UI 靜態檢查（`UICheck.bat`） | 34 個 Designer 檔，0 個問題 |
| UI 執行時檢查（`UICheck.bat runtime`） | zh-TW、zh-CN 各 42 個畫面狀態，0 個問題 |
| 數值框操作 N1–N4 | 44 個案例，0 個問題；反向測試：Round 2 版本的控制項 10 個問題、拿掉 `Stop()` 1 個問題 |
| 主視窗檢查 | 13 組通過；P6 的 10 個區域相符 |
| 行為比對（`CompareBehavior.ps1`） | 與 `a4b3826d` 比較，0 個未登錄的差異；9 個 fixture 通過 |
| 分支規範檢查（`CheckBranchRules.ps1`） | 354 個檔案，0 個問題 |
| MainLayout 基準比對 | 152 行，0 個差異 |
| 產品輸出殘留 | `MyCAM\bin\Debug` 沒有 `UICheck*`；暫存資料夾沒有 `CompareBehavior_*` |

限制：

1. N1–N4 以子類別走控制項的輸入路徑（編輯框文字、`ValidateEditText`、`UpButton` / `DownButton`、`OnKeyDown`、`OnLostFocus`、`OnMouseWheel`），
   不是實體鍵盤與滑鼠；各畫面的實機操作仍由 5.3 第 3 項確認。
2. 舊型別的預設值來自分支起點原始碼編譯出的實例；只在建構子之後設定的值（例如 `AxisTransformDlg` 的範圍）仍靠程式碼閱讀確認，已寫在例外理由。
3. 人工確認 5.3 第 2～4 項未執行（使用者處理，合併前必做）。

## 13.5 Round 4 請確認的重點

1. 上下鍵先驗證後，連按、按住自動重複（原生 `UpDownBase` 的計時器）與 `ReadOnly` 的互動是否還有遺漏。
2. 編譯分支起點舊型別的作法（來源檔清單、命名空間對應、暫存資料夾清理）是否穩定，fixture 是否涵蓋應有的情況。
3. N2 時間門檻的窗口設定（500 / 200 / 400 ms）是否兼顧穩定與防線。
4. 文件（規範第 5、8 節、議題文件 3.30、4.17、5 章）與實作是否一致。
