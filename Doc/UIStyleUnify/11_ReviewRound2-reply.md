# 11. Round 2 Review 回覆

## 11.1 回覆資訊與結論

| 項目 | 內容 |
| --- | --- |
| 回覆對象 | [10_ReviewRound2.md](10_ReviewRound2.md)（審查版本 `1afaf20d`，快照於 `6086614e` 提交，不修改） |
| 回覆日期 | 2026-11-21 |
| 修正版本 | `46c1be30`（本文件在其後的 commit；`review-reply-done.txt` 不提交） |
| 決議紀錄 | [4_Discussion.md](4_Discussion.md) 4.16 |
| 實作紀錄 | [3_Implementation.md](3_Implementation.md) 3.29 |

**3 個問題與 10.5 的測試缺口查證後全部屬實，全部照 review 的建議方向處理。**
人工驗收（5.3 第 2～4 項）仍未完成，三項都是合併前必做，本分支仍不是可合併狀態。

## 11.2 逐項回覆

| ID | 查證 | 處理 | 驗證 |
| --- | --- | --- | --- |
| R2-01 | 屬實；以 10.6 的步驟重現，超出上限的輸入也一樣 | 修正控制項：回復時清除使用者輸入狀態，以完整精度的目前值寫回；刪除 `m_LastValidValue` | N1 新案例；反向測試重現 review 結果 |
| R2-02 | 屬實；例外不分屬性也屬實 | 修正比對：型別替換後比對生效值（反射讀預設值）；B3 例外指定屬性 | `RunIssueChecks.bat` 0 個差異；注入 `ReadOnly = true` 的實測 |
| 10.5 測試缺口 | 屬實，6 類都沒有永續案例 | 全部補上：N1–N3 加案例，新增 N4 | N1–N4 32 個案例 |
| R2-03 | 屬實 | 同步架構摘要、規範第 5 節與 5 章；歷史回覆不改 | 文件 |

## 11.3 各項說明

1. **R2-01：回復保留完整精度**
   1. 原因：Round 1 的回復只把格式化的文字（`1.235`）寫回編輯框。原生 `NumericUpDown` 把任何非程式內部的文字變更視為使用者輸入（`UserEdit = true`），
      下次讀 `Value` 時就解析這段捨入後的文字並改值。0.001 的案例沒事，只因為值本身剛好只有 3 位小數。
   2. 修正（`DebouncedNumericUpDown.cs`）：
      1. 被拒的文字從未進入 `Value`，所以目前的 `Value` 就是最新的合法值；不需要另存一份，刪除 `m_LastValidValue`。
      2. 回復時先 `UserEdit = false`（之後讀 `Value` 不會再驗證，也就沒有 Round 1 遇到的遞迴），再以 `ChangingText = true` 寫回格式化的文字，
         這次寫回不會被當成使用者輸入。
      3. `ValidateEditText` 在 `UserEdit` 為 false 時直接返回：沒有新的輸入時，顯示的是捨入後的文字，再解析就會改值。
         這也擋住「拒絕後又觸發一次驗證」的路徑（施工時的第一版測試就因為這條路徑仍得到 1.235）。
      4. 沒有採用原生的 `UpdateEditText()`：文字為空時它不更新文字，清空後的回復（N4）會失敗。
   3. 沒有用「先把模型值捨入到顯示位數」的方式處理，符合 review 的建議。
2. **R2-02：B3 比對生效值**
   1. 型別替換後的每個 B3 屬性：
      1. 舊型別沒有的屬性：新型別的新設定，不比對。
      2. 舊型別有的屬性：舊值取 Designer 有寫的值，否則取舊型別新實例的預設值；新值同理。數值以去掉尾端 0 的 `decimal`、列舉以名稱比較。
      3. 新型別沒有的屬性：舊控制項改過預設值時才報（例如 TextBox 的 `ReadOnly = true` 改成標籤），只是預設值時不報。
   2. 實測：在 `AutoSortPathDlg.Designer.cs` 暫時加入 `m_numExtrusionBandwidth.ReadOnly = true`，現在報出
      `ReadOnly changed (effective): 'false' -> 'true'`；Round 1 的條件會跳過。
   3. 改後新報出 30 筆，都落在已決議的範圍，登錄為屬性例外：
      1. 唯讀座標改標籤的 `TabStop`（`CalibrationREFDlg` 9 筆、`ThreePtTransformDlg` 9 筆）：舊的唯讀文字框可以 Tab 進入，標籤不行；
         `ThreePtTransformDlg` 的文字框原本就是停用的，本來也進不去。
      2. `AxisTransformDlg` 的 `Minimum` / `Maximum` / `DecimalPlaces`（12 筆）：兩型別的 Designer 預設值不同，但分支起點與現在都在建構子設定範圍與小數位數，執行時的值相同。
   4. B3 例外改為 `B3 <畫面> <控制項> <屬性> <理由>`，既有 3 行改寫成這個格式，新增 5 行；同一控制項的其他屬性仍會比對。
   5. `CompareBehavior.ps1` 需要讀 `CommonUI.dll`，因此要先建置 Debug，缺少時直接報錯結束；已寫入 5.2 與腳本開頭。
3. **10.5 測試缺口**（`ControlRunner.cs`，`UICheck.bat runtime` 執行）：
   1. N1：比顯示位數精確的值（1.23456）遇到無法解析與超出上限的輸入都保留原值、沒有額外的 `ValueChanged`；之後沒有輸入的驗證也不改值；
      上下限 0、1000 可以輸入，−0.001、1000.001 被拒。
   2. N2：間隔內的兩次變化（20、21）只送出一次，值為 21。
   3. N3：向下滾輪停在下限；`ReadOnly` 時滾輪與上下鍵不改值。
   4. N4（新增）：`10 → 20（待通知）→ SilentSetValue 30` 不送出通知，之後改成 30.001 送出一次；清空後離開欄位回復 20 並立即送出待送通知；
      清空後按 Enter 回復，且 `Handled`、`SuppressKeyPress` 都為 true。
   5. 輸入改為寫入內部的編輯框再走 `ValidateEditText`，與 10.6 相同：直接設定控制項的 `Text` 會立刻驗證，不是使用者輸入的路徑。
   6. 反向測試：換回 Round 1 版本的控制項，N1 報 4 筆（含 1.23456 → 1.235、`ValueChanged` 為 `[1.235]`，與 10.6 相同）；
      只拿掉「沒有輸入時不驗證」，N1 報 2 筆。
4. **R2-03：文件**
   1. `2_Architecture.md` 2.3 的 `TextRole` 摘要刪除「標題」，2.12 檢查表加 N1–N4。
   2. UI 規範第 5 節第 3 點改寫：通知寫明 `SilentSetValue` 不送出並取消待送通知（行為不改），另寫明精度、上下限、連續變化、`ReadOnly`、空白輸入。
   3. 檔案數：`5_Verification.md` 記最終一次的結果並註明版本（11.4）。`9_ReviewRound1-reply.md` 的 348 是當時工作目錄（`36e704dd` 加上尚未提交的回覆）的結果，保留不改；
      review 在 `1afaf20d` 量到的 351，多出的是之後提交的回覆與 review 檔。

## 11.4 驗證結果

以下在修正版本的工作目錄執行（2026-11-21）。

| 驗證 | 結果 |
| --- | --- |
| `Build.bat Debug` | 成功，0 個錯誤 |
| UI 靜態檢查（`UICheck.bat`） | 34 個 Designer 檔，0 個問題 |
| UI 執行時檢查（`UICheck.bat runtime`） | zh-TW、zh-CN 各 42 個畫面狀態，0 個問題 |
| 數值框操作 N1–N4 | 32 個案例，0 個問題；反向測試（Round 1 版本的控制項）4 個問題 |
| 主視窗檢查 | 13 組通過；P6 的 10 個區域相符 |
| 行為比對（`CompareBehavior.ps1`） | 與 `a4b3826d` 比較，0 個未登錄的差異（B3 生效值、例外指定屬性）；注入 `ReadOnly = true` 報 1 筆 |
| 分支規範檢查（`CheckBranchRules.ps1`） | 352 個檔案，0 個問題 |
| MainLayout 基準比對 | 152 行，0 個差異 |
| 產品輸出殘留 | 檢查後 `MyCAM\bin\Debug` 沒有 `UICheck*` |

限制：

1. N1–N4 以子類別走控制項的輸入路徑（編輯框文字、`ValidateEditText`、`OnKeyDown`、`OnLostFocus`、`OnMouseWheel`），不是實體鍵盤與滑鼠；各畫面的實機操作仍由 5.3 第 3 項確認。
2. B3 的預設值來自新建的控制項實例；只在建構子之後才設定的值（例如 `AxisTransformDlg` 的範圍）仍要靠程式碼閱讀確認，已在例外理由寫明。
3. 人工確認 5.3 第 2～4 項未執行（使用者處理，合併前必做）。

## 11.5 Round 3 請確認的重點

1. `RevertToLastValue` 與 `ValidateEditText` 的 `UserEdit` / `ChangingText` 處理，是否還有原生流程（例如 `UpdateEditText`、`OnLostFocus`、上下鍵）會讓捨入文字被解析。
2. B3 生效值比對：反射預設值與 Designer 文字的正規化（`decimal`、列舉、布林）是否有漏比或誤報的型別；5 行新例外的範圍是否恰當。
3. N1–N4 是否足以作為 R1-01、R2-01 與 R1-C1 的回歸防線。
4. 文件（規範第 5、8 節、議題文件 2.3、2.12、3.29、4.16、5 章）與實作是否一致。
