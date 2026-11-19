# 6. 重要的後續處理事項

## 6.1 合併前的收尾

| # | 事項 | 說明 |
| --- | --- | --- |
| 1 | 議題編號 | 決定後把分支 `AUTO-TBD_f_UIStyleUnify` 以 `git branch -m` 改名，並更新 [README.md](README.md) 的議題編號 |
| 2 | 人工確認 | [5_Verification.md](5_Verification.md) 5.3 的第 2～4 項（Designer、實機數值框、CNC 連線） |
| 3 | 同步 `master` | `master` 已在分支起點之後前進兩個 commit（`8db7be81`、`7829c338`，只改開發環境檔案，與本議題沒有交集）。合併前 merge / rebase，重新建置，跑 `Tools\UICheck\UICheck.bat runtime` 與 `Doc\UIStyleUnify\Tests\RunIssueChecks.bat` |
| 4 | 推送 | 尚未推送；推送前確認 remote（`origin` GitLab / `github` 個人）與分支 |
| 5 | 語系檔部署 | 語系 ID 改名、`LockABDlg` / `CNCThreePointDialog` 語系檔改名；部署或更新建置輸出時重跑 `CopyRef.bat`，並移除建置輸出中舊檔名的語系檔。客製語系檔（`Custom/Language`）的舊 ID 會失效（已決議接受） |

## 6.2 範圍外的後續議題

本議題只統一外觀；以下屬於行為或範圍外，另開議題處理。

| 日期 | 項目 | 說明 |
| --- | --- | --- |
| 2026-11-15 | 編輯工具對話框補「取消」按鈕 | 部分對話框只能按 X 取消。有些 Action 可能是刻意不讓使用者按 X，補上會影響操作規格，需逐一確認各 Action 後處理 |
| 2026-11-15 | 對話框行為模式統一 | 三種行為基底（`EditDialogBase<T>`、`FormBase`、直接繼承 `DialogBase`）的事件機制（預覽 / 確定 / 取消 / 重設）不一致。**之後做行為統一時，必須沿用本議題建立的外觀層（`DialogBase` 與 `CommonUI` 元件），不可重新引入各自的外觀設定** |
| 2026-11-15 | `Show` / `ShowDialog` 統一 | 多數以 `Show( owner )` 非強制回應開啟，`GlassDXFImportDlg`、`ConnectDlg` 以 `ShowDialog`；開啟入口也不一致（`FormBase.Show`、`DialogShowHelper.ShowCentered`、直接 `Form.Show`）。改變強制回應會影響操作 |
| 2026-11-15 | 橫式的警報 / 日誌面板 | 橫式時兩者在訊息區上下分割（60% / 40%），空間都很小。希望改成以按鈕切換、同時只顯示一個並佔滿訊息區。屬於版面行為 |
| 2026-11-15 | 主視窗行為重構 | `StartupForm.cs` 約 2000 行，混雜編輯器切換、工具列顯示 / 隱藏、CNC 面板管理。拆分屬於行為重構 |
| 2026-11-16 | 支援未輸入的數值框 | `CNCThreePointDlg` 工件點 9 欄與誤差容忍欄以空白代表「未輸入」且有意義；工件點可「擷取」也必須可手動輸入。做法：`CommonUI.Controls` 新增可為空的數值框（繼承 `NumericUpDown`，覆寫 `ValidateEditText` / `UpdateEditText`，空白時保留空白、以 `decimal?` 回傳 null），外觀與其他數值框一致；另訂空白時上下鍵 / 滾輪的起始值，並把以空白字串判斷「點是否完整」的邏輯改為判斷 null。會改變輸入行為（擋非數字、小數位數固定），需連 CNC 實測。完成後移除 `StaticExceptions.txt` 的兩行例外，並在規範第 9 節登錄 |
| 2026-11-16 | `CalibrationREFDlg` KeyDown 重複訂閱 | Designer 與 `Init()` 都訂閱 `CalibrationREFDlg_KeyDown`，按 Delete 觸發兩次 `DeleteKeyDown`。既有行為 |
| 2026-11-16 | 數值框取代 `TextBox` 的細節差異 | 可用上下鍵 / 滾輪；小數位數固定（`AddPathDlg` 弦高誤差 4 位，其餘 3 位）；`ThreePtTransformDlg` 機械座標不會再空白，「座標無效」提示只在非數字輸入時出現。若現場有問題再調整（`Doc/UIStyleUnify/Tests/BehaviorExceptions.txt`） |
| 2026-11-16 | 唯讀座標無法複製 | 三個對話框的唯讀座標改為 `Label` 後不能選取複製。若現場有需求，再評估提供複製按鈕 |
| 2026-11-16 | 切換語系後的字族 | 主視窗切換語系後，已建立的對話框與 CNC 面板中以文字角色設定的字型（粗體、數值字型）仍是舊語系字族，重新開啟畫面即正確 |
| 2026-11-16 | 檢查涵蓋範圍的缺口 | 1. 行為比對 B8 只比對事件訂閱與 `Enabled` / `Visible` / `ReadOnly` / `Checked` 賦值行，不涵蓋其他邏輯；刪除的容器原本的 `Enabled` / `Visible` 由誰承接需人工判斷。2. 嵌入式面板的卡片座標規則沒有靜態檢查（由 T1–T4、P4 在實際大小下檢查）。3. 執行時才填入的文字（約 40 個控制項）在 zh-CN 下是否截斷，需要假資料或連線。4. CNC 面板的連線中狀態只以設計尺寸檢查文字寬度，未實際連線截圖 |
| 2026-11-16 | 直式主視窗 CAM 工具列溢出 | 既有問題，與本議題無關 |
| 2026-11-17 | 既有程式碼的通用規範 | 本議題只修正自己寫入的部分；既有程式碼仍有大量超過 120 字元的行、一檔多個頂層型別（例如對話框檔內的列舉）、語系 XML 沒有 BOM 或檔尾換行。若要全面整理，另開議題，並可參考 `Doc/UIStyleUnify/Tests/CheckBranchRules.ps1` 做成可持續使用的檢查 |
