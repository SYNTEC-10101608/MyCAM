# 9. Round 1 Review 回覆

## 9.1 回覆資訊與結論

| 項目 | 內容 |
| --- | --- |
| 回覆對象 | [8_ReviewRound1.md](8_ReviewRound1.md)（審查版本 `3d4b5ffd`，快照不修改） |
| 回覆日期 | 2026-11-21 |
| 修正版本 | `36e704dd`（本文件與 `review-reply-done.txt` 在其後的 commit） |
| 決議紀錄 | [4_Discussion.md](4_Discussion.md) 4.15 |
| 實作紀錄 | [3_Implementation.md](3_Implementation.md) 3.28 |

**8 個問題與 R1-C1 查證後全部屬實，全部已處理：7 項照建議修正，R1-06 依使用者決議改為 Danger，R1-C1 正式接受並納入檢查。**
人工驗收（5.3 第 2～4 項）仍未完成，已決議三項都是合併前必做，因此本分支仍不是可合併狀態。

## 9.2 逐項回覆

| ID | 查證 | 處理 | 驗證 |
| --- | --- | --- | --- |
| R1-01 | 屬實；嚴重度修正，見 9.3 第 1 項 | 修正：回復最新合法值；連同 epsilon 一起修 | N1、N2（新增長期檢查），反向測試重現 review 的結果 |
| R1-02 | 屬實；實測量出兩個盲點各藏了什麼，見 9.3 第 2 項 | 修正：B7 例外限縮到單一檔案；型別替換後仍比對 B3 | `RunIssueChecks.bat` 0 個差異；拿掉例外的實測 |
| R1-03 | 屬實 | 修正：三支腳本的清理包住編譯、準備與執行全程 | 失敗路徑實測：修正前留下 `UICheckRunner.exe`，修正後不留 |
| R1-04 | 屬實 | 修正：相容性異動第 3 項 | 文件 |
| R1-05 | 屬實；`TextRole.Title` / `ThemeFonts.Title` 有定義但沒有任何地方使用 | 改規範：刪除 `Title` 列與未使用的 API，不改字型 | 建置、`UICheck.bat` |
| R1-06 | 屬實；目前只有 `DesignSupportBracketDlg` 使用 | 修正：刪除按鈕設 `Danger`，不在規範加例外 | 建置、執行時檢查與截圖 |
| R1-07 | 屬實 | 修正：README 狀態分開寫；合併門檻為 5.3 第 2～4 項全部完成 | 文件 |
| R1-08 | 屬實 | 修正：專案概觀 `CommonUI` 說明；3.16 補 `CommonUI.csproj` | 文件 |
| R1-C1 | 屬實 | 正式接受，具體差異寫入規範與例外理由 | N1、N3 |

## 9.3 各項說明

1. **R1-01：數值框回復舊值**
   1. 原因：`m_LastValue` 同時被當成「上次通知的值」（debounce 判斷用，timer 觸發時才更新）與「最後合法值」（拒絕輸入時回復用）。
      變化不超過 epsilon 時 timer 不啟動，「最後合法值」就落後了。
   2. 嚴重度修正：畫面與模型始終一致（框回到 0，模型也回到 0），觸發條件是「只變動最後一位小數一步，接著打錯字」；
      屬於合法輸入被吃掉，不是資料錯亂。只有訂閱原生 `ValueChanged` 的 `AxisTransformDlg` 看得到反向動作。
      仍視為本議題替換控制項帶進來的差異，合併前修正。
   3. 修正：新增 `m_LastValidValue`，在 `OnValueChanged` 記錄每個被接受的值；拒絕時回復它，且不再設定 `Value`，
      所以回復不送出 `ValueChanged`。回復時不能讀 `Value`：編輯中的文字尚未被接受時，讀 `Value` 會再次驗證並無限遞迴（施工時實際遇到 StackOverflow）。
   4. 延伸問題（review 未列）：epsilon `<= 0.001` 讓 3 位小數的數值框改 0.001 時永遠不送出 `DebouncedValueChanged`，
      10 個對話框、30 個數值框的預覽不更新（確定時重新讀 `Value`，存值正確）。分支起點已有，但本議題把所有數值輸入都換成這個控制項，
      因此一起修：改為 `decimal` 直接比較，刪除 `EPSILON`。
   5. 長期檢查 `Tools/UICheck/CheckControls.ps1` + `ControlRunner.cs`（`UICheck.bat runtime` 執行）：
      1. N1：0.001 之後輸入 `abc`、debounce 中輸入超過上限、通知後輸入低於下限，都回復最新合法值，`ValueChanged` 只有原本的變化。
      2. N2：1.000 → 1.001 送出一次 `DebouncedValueChanged`，值為 1.001。
      3. N3：滾輪一格一個 `Increment`、停在上限（R1-C1）。
   6. 反向測試：換回修正前的控制項建置，N1 報 4 筆、N2 報 2 筆；其中「0.001 之後輸入 `abc`」得到 0.000、`ValueChanged` 為 `[0.001, 0.000]`，與 review 的實驗相同。
2. **R1-02：行為比對防線**
   1. 實測 B7：拿掉 `B7 repository *` 重跑，只多出 1 筆 `CommonUI/Layout/SectionStack.cs: form.Show();`，正是已決議的精靈換頁搬移，沒有其他 modal 開啟差異。
      修正後 B7 以呼叫點所在檔案為「控制項」欄，例外改為 `B7 repository CommonUI/Layout/SectionStack.cs`，其他檔案的差異都會報出。
   2. 實測 B3：拿掉型別替換後的 `continue` 重跑，多出 79 筆，逐筆看過都落在已決議的大類：
      1. TextBox 改數值框：多出 `DecimalPlaces`、`Minimum`、`Maximum`、`Value` 等新型別的設定（`ThreePtTransformDlg` 的 `Value = 1` 對應舊的 `Text = "1"`）。
      2. TextBox 改標籤：少了 `ReadOnly`、`TabStop`、`Enabled`。
      3. `AutoSortPathDlg`（原生 → Debounced）沒有屬性差異。
   3. 修正後型別替換仍比對 B3，但只比對分支起點有設定的屬性（必須保留或登錄），新型別才有的屬性是新設定、不比對。
      剩下 36 筆都是唯讀座標改標籤，依 2026-11-15「只顯示座標的欄位」逐畫面登錄 3 行 B3 例外。
   4. 證據範圍寫入 [5_Verification.md](5_Verification.md) 5.2：本比對讀原始碼宣告，「0 個差異」不代表操作完全等價；
      共用控制項的操作由 N1–N3 檢查，各畫面的實機操作由 5.3 第 3 項確認；B8 不比對 `CommonUI` 的理由也寫明。
3. **R1-03：runner 清理**：`CheckRuntimeLayout.ps1`、`CaptureMainWindow.ps1` 與新的 `CheckControls.ps1`，
   runner 路徑先決定，刪除寫在包住編譯、準備（截圖資料夾等）與執行全程的 `finally`。
   在腳本副本中讓第二次編譯（`HiddenDesktopRun.cs`）失敗：修正前留下 `UICheckRunner.exe`，修正後三支都不留。
4. **R1-04**：[7_Compatibility.md](7_Compatibility.md) 新增第 3 項：26 個設定 `CommonText` 的按鈕只讀 `CommonUI_<語系>.xml` 的共用 ID；
   客製文字要改寫到 `Custom/Language/CommonUI_<語系>.xml`，同一 ID 全畫面一致，原本依畫面不同的客製文字無法保留，部署時告知。
5. **R1-05**：不補實作，從規範 2.2 刪除 `Title`，並刪除未使用的 `TextRole.Title`、`ThemeFonts.Title`。理由：改成 10.5pt 要重驗選單高度、
   MainLayout 基準（152 行）與 Designer，收尾階段風險偏高；需要視覺層級時另開議題。Designer 以名稱序列化 `Role`，刪除列舉值不影響既有 Designer 檔。
6. **R1-06**：`RemovableValueRow` 的刪除按鈕設 `Kind = ButtonKind.Danger`，所有動態數值列一致。
   曾考慮在規範加「移除尚未確定的輸入列用 Normal」的例外，使用者決議不加：例外不值得，畫面偏吵可以接受。
7. **R1-07**：README 狀態改為「施工與自動檢查完成；人工驗收未完成，三項都是合併前必做」；5.3 與 6.1 寫明合併門檻。
   5.3 第 3 項（數值框實機操作）在本次修正之後執行。
8. **R1-08**：專案概觀的 `CommonUI` 改為現行內容；[3_Implementation.md](3_Implementation.md) 3.16 補 `CommonUI.csproj`（五類資料夾的編譯項目、`LanguageManager` 專案參考）。
9. **R1-C1**：正式接受兩項差異：滾輪一格一個 `Increment`（原生為系統捲動行數 × `Increment`），超出範圍回復最新合法值（原生夾到上下限）。
   這是全 App 數值框一致的行為，寫入 UI 規範第 5 節第 3 點與 `BehaviorExceptions.txt` 的 `AutoSortPathDlg` 理由，由 N1、N3 檢查。
   注意：R1-01 修正後，「回復」的對象是最新合法值；review 表中「起值 10、輸入 2000 → 回復 10」的結果不變（10 就是最新合法值）。
10. **8.4 其餘項目**：同意 review 的分類，維持既有決議與後續處理，不重列。

## 9.4 驗證結果

| 驗證 | 結果 |
| --- | --- |
| `Build.bat Debug` | 成功，0 個錯誤 |
| UI 靜態檢查（`UICheck.bat`） | 34 個 Designer 檔，0 個問題 |
| UI 執行時檢查（`UICheck.bat runtime UICheckScreens`） | zh-TW、zh-CN 各 42 個畫面狀態，0 個問題 |
| 數值框操作 N1–N3（新增） | 11 個案例，0 個問題；反向測試（修正前的控制項）6 個問題 |
| 主視窗檢查 | 12 組語系 × 頁面 × 方向加 Designer 情境，共 13 組通過；P6 的 10 個區域相符 |
| 行為比對（`CompareBehavior.ps1`） | 與 `a4b3826d` 比較，0 個未登錄的差異（B7 例外限縮、B3 型別替換後仍比對） |
| 分支規範檢查（`CheckBranchRules.ps1`） | 348 個檔案，0 個問題 |
| MainLayout 基準比對 | 152 行，0 個差異 |
| 產品輸出殘留 | 檢查後 `MyCAM\bin\Debug` 沒有 `UICheck*`；失敗路徑實測也沒有 |

限制：

1. `RemovableValueRow` 的列在執行時才新增，執行時截圖不含這些列；`Danger` 的外觀在人工確認（5.3 第 2、3 項）時一併目視。
2. N1–N3 以子類別直接呼叫控制項的輸入路徑（`ValidateEditText`、`OnMouseWheel`），不是真的鍵盤與滑鼠事件；各畫面的實機操作仍由 5.3 第 3 項確認。
3. 人工確認 5.3 第 2～4 項未執行（使用者處理，合併前必做）。

## 9.5 Round 2 請確認的重點

1. `DebouncedNumericUpDown` 的修正（3.28 第 1 點）：回復、通知與 `SilentSetValue`、`OnLostFocus` / `Enter` 空白輸入路徑的互動是否還有遺漏。
2. N1–N3 是否足以作為 R1-01 與 R1-C1 的回歸防線；是否有應補的案例。
3. B3 限縮規則（只比對分支起點有設定的屬性）是否會漏掉應該報出的差異，以及 3 行 B3 例外的範圍是否恰當。
4. 文件（規範第 5、8 節、議題文件 3.28、4.15、5 章、7 章）與實作是否一致。
