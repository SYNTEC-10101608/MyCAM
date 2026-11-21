# 8. Round 1 Overall Review

## 8.1 審查資訊與結論

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254：UI 風格統一 |
| 審查日期 | 2026-11-21 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對範圍 | 分支起點 `a4b3826d` → 審查版本 `3d4b5ffd` |
| 範圍 | 議題文件、專案指引、產品實作、行為比對、UI 檢查與產品程式殘留 |
| 性質 | 唯讀審查結果；問題與建議尚待討論，不代表已決議或已修正 |

**主要外觀架構已落實，現有自動檢查全部通過；但仍有操作回歸、驗證防線缺口與文件落差，不宜直接認定「操作不變、整體驗收完成」。**

本輪 review 未修改產品程式、檢查程式或既有決議。以下路徑均相對於 repo 根目錄，行號對應審查版本；後續修改可能使行號移動。

### 與任務目標的對照

| 審查面向 | 結果 |
| --- | --- |
| 實作是否符合規劃 | 外觀／行為分層、共用元件、版面計算與語系整合大致符合；Title 字型與動態刪除按鈕仍有規範落差 |
| 產出是否符合目標 | 自動 UI 與版面檢查通過；但已確認數值操作回歸，人工外觀、數值操作與 CNC 驗收仍未完成 |
| 文件與實作是否對應 | 主要架構與實作紀錄一致；共用文字相容性說明、完成狀態及部分指引／逐檔紀錄需補齊 |
| 是否殘留測試／除錯程式 | 未發現本議題的檢查碼被產品專案引用；本輪結束後無 runner 殘留，但失敗路徑的清理保護不完整 |
| 比對差異是否完整登錄 | 不能回答「全部已列」；外部客製語系遷移說明有缺漏，部分數值操作差異尚未具體確認，且比對例外過寬 |

## 8.2 驗證結果與限制

| 驗證 | 本輪結果 |
| --- | --- |
| `Build.bat Debug` | 成功，3 個警告、0 個錯誤 |
| MyCAM 專案建置 | 成功 |
| UI 靜態檢查 | 34 個 Designer 檔，0 個問題 |
| UI 執行時檢查 | zh-TW、zh-CN 各 42 個畫面狀態，0 個問題 |
| 主視窗檢查 | 12 組語系 × 頁面 × 方向，加 Designer 情境，共 13 組通過；P6 的 10 個區域相符 |
| 行為比對 | 與 `a4b3826d` 比較，0 個未被例外排除的差異；限制見 R1-02 |
| 分支規範檢查 | 347 個新增／修改／改名檔，0 個問題 |
| MainLayout 基準比對 | 152 行，0 個差異 |
| 產品輸出殘留 | Debug、Release 輸出第一層均無 `UICheck*` runner |

`UICheck.bat runtime` 與 `RunIssueChecks.bat` 串行執行，均成功結束，避免兩套檢查同時操作共用 runner 檔案。

本輪產生 97 張截圖，當時保留於 `%TEMP%\MyCAM_UIReview_3d4b5ffd_31908`，不納入版控。未完成人工目視，因此不能以截圖產生成功代替 Designer／外觀驗收。

隱藏桌面的 OpenGL `wglMakeCurrent` 訊息屬既有文件記載的限制，不列為本輪新增問題。實際 CNC 連線、VNC 定位與機台數值／燈號仍需人工確認。

## 8.3 問題總表

| ID | 優先度 | 問題 | 分類 |
| --- | --- | --- | --- |
| R1-01 | 優先 | AxisTransform 的合法 0.001 變化可能被下一次非法輸入撤銷 | 操作回歸 |
| R1-02 | 優先 | 行為比對的 B7 全域例外與 B3 跳過邏輯造成驗證盲點 | 驗證防線 |
| R1-03 | 中 | runner 編譯／準備失敗時可能留下 executable | 清理可靠性 |
| R1-04 | 中 | 共用按鈕客製文字改為全域的遷移說明不完整 | 外部相容性文件 |
| R1-05 | 中 | 主視窗模式頁籤未使用規劃的 Title 字型 | 規範／實作落差 |
| R1-06 | 中 | 動態數值列的刪除按鈕仍為 Normal | 規範／實作落差 |
| R1-07 | 低 | 完成狀態文字與人工驗收的完成定義不一致 | 文件狀態 |
| R1-08 | 低 | 專案概觀與部分逐檔實作紀錄未同步 | 文件收尾 |

### R1-01：AxisTransform 的 0.001 操作回歸

**現象與影響**

`AxisTransformDlg` 原本使用原生 `NumericUpDown`，現在改為 `DebouncedNumericUpDown`。新控制項對恰好 0.001 的變化不啟動 debounce timer，最後有效值仍為初始的 0。下一次非法輸入會回復該舊值，再次發出 `ValueChanged`。

Axis 的 handler 依新舊值計算差量，因此事件 `[0.001, 0.000]` 會對應 `+0.001、−0.001` 的回呼，撤銷前一次合法變化。等待 debounce 也不能解決這個邊界，因為 timer 未啟動。

**證據**

- 替換控制項：`MyCAM\Editor\Dialog\AxisTransformDlg.Designer.cs:34–45`。
- 訂閱 `ValueChanged`：`MyCAM\Editor\Dialog\AxisTransformDlg.cs:98–103`。
- 計算差量、更新前值並送出 callback：同檔 `175–179`。
- 非法輸入回復最後有效值：`CommonUI\Controls\DebouncedNumericUpDown.cs:177–186`。
- `<= EPSILON` 提早返回、不啟動 timer：同檔 `198–204`；`EPSILON = 1e-3` 位於 `295`。
- 正常更新最後有效值需 timer tick：同檔 `279–283`。

本輪以現行 DLL 與原生控制項做無視窗實驗：設定 `Value = 0.001`、`Text = "abc"`，再讀取 `Value` 觸發驗證。現行結果為 `0.000`，事件 `[0.001, 0.000]`；原生保留 `0.001`。兩者均未建立視窗 handle。

這確認的是控制項驗證路徑；Axis 的反向回呼影響由產品程式追蹤確認，未宣稱已做實機操作驗證。epsilon 雖原已存在，但 Axis 原本不使用此控制項，屬本議題替換引入的回歸。

**建議**：合併前修正，將合法值追蹤與 debounce 通知門檻分開處理。回歸驗證應確認非法輸入後仍保留 0.001，事件僅有原合法變化，不送出反向 callback。未找到接受此回歸的決議。

### R1-02：行為比對防線過寬

**B7 全域例外**

`Doc\UIStyleUnify\Tests\BehaviorExceptions.txt:65` 的 `B7 repository *` 理由是精靈換頁移入 `SectionStack.ShowPage`，但實際匹配並未限定該搬移：

- `CompareBehavior.ps1:58–59` 只以前三欄建立例外 key，理由不參與篩選。
- 同檔 `244、249` 對所有被掃到的 `Show／ShowDialog` 差異都使用 `B7 repository *`。
- 同檔 `97–101` 以 wildcard 匹配後直接返回，不加入問題清單。

因此 B7 的全部差異都會被排除，不只是已決議的換頁搬移。這是驗證 guard 缺陷，不代表本輪另外找到實際的 modal 開啟回歸。

**B3 跨型別屬性比對被跳過**

`CompareBehavior.ps1:185–188` 在控制項型別替換時報 B6 後直接 `continue`。即使 B6 已被接受，後續 B3 的數值範圍、精度等適用屬性也不再比較。

此外 B8 排除 `CommonUI`（同檔 `272–273`），且檢查主要是原始碼屬性、事件與狀態賦值，不是操作結果測試。不能把「0 differences」解讀成所有操作完全等價。

**建議**：例外限縮至可識別的已接受呼叫點；型別替換後仍比較適用的行為屬性，並補 R1-01 的具體操作回歸驗證。保留現有比對的價值，但明確記載其證據範圍。

### R1-03：runner 失敗路徑清理不完整

- `Tools\UICheck\CheckRuntimeLayout.ps1:31–40、51、75–77`。
- `Tools\UICheck\CaptureMainWindow.ps1:46–55、62、69–83`。

兩支腳本的 runner 編譯與部分準備工作位於清理 `try/finally` 之前。第一個 executable 已產生後，第二次編譯失敗直接退出；截圖目錄建立失敗也可能在清理保護之外。

因此存在留下 runner 於產品 bin 的失敗路徑，與文件「執行結束即刪除、不隨產品部署」的要求不一致。**本輪正常執行後實際沒有殘留**，不要把條件式缺陷描述為目前已有殘留。

**建議**：清理保護涵蓋產生、準備與執行全流程；驗證正常及編譯／準備失敗路徑均不留下已產生的 runner。

### R1-04：共用按鈕客製文字遷移說明不足

`Doc\UIStyleUnify\7_Compatibility.md:10` 目前只要求依新舊 ID 改寫客製檔。但 `CommonUI\Language\LanguageApplier.cs:80–84` 對設定 `CommonText` 的按鈕優先使用共用文字，不讀取畫面自己的按鈕 ID。

共用文字來源見 `CommonUI\Language\CommonTexts.cs:18、46、54–56`。例如原本各畫面的 `m_btnOK` 或 `m_btnSure`，不能只在該畫面客製檔改成 `m_btnConfirm` 就保留效果。

**建議補充**：

- 客製文字須遷移至 `Custom\Language\CommonUI_<語系>.xml` 的 `Confirm`、`Cancel` 等共用 ID。
- 同一共用 ID 會影響所有使用它的按鈕；原本不同畫面的不同客製確定文字，現在無法各自保留。
- 部署時告知此影響，不只提供控制項改名對照。

共用文字設計已接受；本項要求補齊遷移文件，不是要求恢復畫面各自的文字設計。

### R1-05：模式頁籤的 Title 字型未落實

`.github\instructions\ui-style.instructions.md:66` 定義模式頁籤使用 Title 10.5pt；`CommonUI\Forms\MainWindowBase.cs:60–66` 卻對所有 ToolStrip／MenuStrip 套用 Body 9pt。產品沒有使用 `TextRole.Title` 或 `ThemeFonts.Title`，本輪也確認實際模式選單為 9pt。

**建議**：決議補實作或修改最終規範。若改字型，需一併確認工具列高度、Designer 區域與 layout 基準，不宜只改字型而忽略版面影響。

### R1-06：動態刪除按鈕未使用 Danger

`CommonUI\Controls\RemovableValueRow.cs:71–77` 建立刪除按鈕時未設定 `Kind`，本輪確認實際為 Normal。`.github\instructions\ui-style.instructions.md:129` 要求刪除／移除已存在資料使用 Danger，未找到接受例外。

**建議**：在共用元件修正，讓所有動態數值列一致。此項是外觀規範落差，不是操作回歸。

### R1-07：完成狀態與完成定義不一致

`Doc\UIStyleUnify\README.md:9` 寫「施工與驗收完成，待人工確認」，但 `1_Goal.md:41–46` 的完成定義包含人工確認；`5_Verification.md:131–133` 的最新 Designer、數值操作與 CNC 連線仍待確認。

**建議**：狀態區分「施工／自動驗證完成」與「人工驗收未完成」，並明確決定合併門檻。歷史上較早的 Designer 確認不等於已確認後續改動的最終版本。

### R1-08：文件收尾未同步

- `.github\instructions\project-overview.instructions.md:25` 仍列已刪除的 `GroupPanel`，應更新為現行 `SectionPanel` 等 API。
- `Doc\UIStyleUnify\3_Implementation.md:211–223` 的逐檔清單漏列 `CommonUI\CommonUI.csproj`；其編譯項目與 LanguageManager 相依異動未完整列入追蹤。

**建議**：更新持續使用的專案指引與逐檔紀錄，不擴張為無關文件重整。

## 8.4 相容性差異的分類與待決事項

### R1-C1：AutoSort 的具體操作差異尚未確認

`4_Discussion.md:53` 明確接受所有數值輸入改用 `DebouncedNumericUpDown`，包括原生 `NumericUpDown`。但 `Tests\BehaviorExceptions.txt:5–6` 對 AutoSort 只記載替換後範圍與小數位數相同，未具體記載下列差異：

| 條件 | baseline 原生控制項 | 現行控制項 |
| --- | --- | --- |
| 起值 10、Increment 0.5、滾輪 Delta 120、系統 ScrollLines 3 | 11.5 | 10.5 |
| 起值／最後有效值 10、Maximum 1000、輸入 2000 | 限制為 1000 | 回復 10 |

本輪以無視窗控制項實驗確認差異。替換與設定見 `MyCAM\Editor\Dialog\AutoSortPathDlg.Designer.cs:50、53、199–207`；現行滾輪與超界處理見 `DebouncedNumericUpDown.cs:250–275、157–163、177–186`。值會成為實際排序參數（`AutoSortPathDlg.cs:217–218`）。

**分類**：控制項替換大類已接受，但這兩項操作結果尚未具體確認；不直接將其認定為與 R1-01 同等的未接受回歸。

**待決**：保留原生操作結果，或正式接受新結果並記錄具體影響與驗證依據。

### 相容性文件的涵蓋範圍

`7_Compatibility.md` 定位為既有資料、設定、客製檔案與外部介面的部署影響，不是所有操作差異的清單。建議維持此定位：

- 外部客製語系影響補入相容性文件，尤其 R1-04。
- 操作差異在討論／驗證／例外紀錄中具體追蹤，能對應接受決議；不能只由型別替換的大類例外推論全部接受。
- R1-01 應作為回歸修正，不以增加寬鬆例外消除檢查報告。

### 已有接受或後續決議，不重列為新問題

- CNC 切語系後既有面板本文仍用舊字族，卡片標題改新字族：`4_Discussion.md:373` 已決議列後續，`6_FollowUp.md:50` 已追蹤。
- `DialogShowHelper.SetFormConfig` 保留重複的 ShowInTaskbar／MaximizeBox／MinimizeBox 設定：已決議為保持行為而保留。
- 三點校正部分 TextBox 的空白代表未輸入，nullable numeric 已列後續。
- 唯讀座標改標籤後不能選取複製，以及 TextBox 改數值框的大類操作變化，已有接受／後續紀錄；仍須區分未具體確認的個別結果。
- CNCDlg／VNC 為單一遠端內容承載特例，需實際連線人工確認。

## 8.5 產品程式殘留的審查結論

未發現產品專案引用 `Doc`／`Tools` 的檢查碼或 runner；審查中的可疑新增成員均找到產品用途，`SetCommandsEnabled` 並非只供檢查使用的 seam。

本輪正常執行後 Debug／Release 輸出第一層均無 runner。I9 主要檢查新增行、名稱與輸出第一層，不能單獨當作「任何形式的檢查碼都不存在」的證明；本結論結合了產品引用與程式閱讀。失敗路徑仍有 R1-03 的清理問題。

## 8.6 建議討論順序

1. **R1-01、R1-02**：確認合併前修正操作回歸與驗證防線，避免以全綠結果掩蓋未驗證行為。
2. **R1-C1**：決定 AutoSort 的滾輪及超界操作保持原樣，或正式接受新結果。
3. **R1-04、R1-03**：補齊部署相容性說明及 runner 失敗清理。
4. **R1-05、R1-06**：對齊外觀規範與共用元件。
5. **R1-07、R1-08**：整理完成狀態與文件收尾，確認人工驗收與合併門檻。

本報告是 Round 1 的問題快照；後續接受、修正或列後續的結果，應在討論紀錄中明確對應上述 ID，再據以進行下一輪驗證。
