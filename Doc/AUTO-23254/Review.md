# Review

## 第 1～6 回合（VS Copilot，2026-11-21～2026-11-22）

新規範之前的整體審查，編號為 `R1-01` 等（與本議題的 REQ 無關），不轉換為 `REV`。快照在 git 歷史的
`Doc/UIStyleUnify/8_ReviewRound1.md`～`17_ReviewRound6.md`，結論與決議已整理到 [Discussion.md](Discussion.md)
的 DEC-1、DEC-4、DEC-6、DEC-10；人工確認後的增量審查（第 6 回合）確認沒有新缺陷。

## 第 7 回合

併入 `master` 與依新規範調整後的自我 review，重點為衝突的解法、`master` 修正的保留、指引移植與截圖環境（STEP-12、STEP-13）。

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD 為本回合修正後的分支 |
| 工具與模型 | Claude Code |
| 日期 | 2026-11-26 |

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-1 | 1 | 必修 | `MyCAM/MyCAM.csproj` | `master` 的 `ApplicationIcon` 指向不存在的 `MyCAM\AppIcon.ico`（檔案在 `Resource\`），乾淨的 checkout 建置失敗 | 已修正：本分支改為實際位置；`master` 另開 bug（TODO-23、DEC-14） |
| REV-2 | 4 | 必修 | `MyCAM/StartupForm.Designer.cs`、`.resx` | `master` 以 `.resx` 的 `$this.Icon` 指定表單圖示，違反「畫面的 `.resx` 只有設計時資訊」（`ResxResource`） | 已修正：改由 `Properties.Resources.AppIcon` 指定 |
| REV-3 | 3 | 必修 | `CncUI/Panel/CNCInfoPanel.cs` | `master` 隱藏第二旋轉軸時靠自繪跳過隱藏的軸；本議題的軸點是 `ColorSwatch`，直接合併會留下 C 軸色塊 | 已修正：一併隱藏 `m_swtC`，登錄行為比對例外 |
| REV-4 | 3 | 必修 | `MyCAM/Editor/Dialog/LockMSAxisDlg.cs` | 自動合併的 `m_rbtnMasterAuto` 仍是舊名，主從軸條件需取 `master` 的修正 | 已修正：條件取 `master`、名稱取本議題；全 repo 無 `m_rbtn`、`m_dbtn` 殘留 |
| REV-5 | 1 | 必修 | `Tools/UICheck/LayoutRunner.cs` | `master` 把 `SetJogEnabled` 改為兩個參數，`STATE_MAP` 以反射找不到方法，`CNCPanel` 的連線狀態無法開啟 | 已修正：傳入軸數，並新增四軸狀態 |
| REV-6 | 1 | 必修 | `Tools/UICheck` | 執行時檢查在非規格螢幕上被 Form 限制大小而誤報（`DesignSupportBracketDlg` 的 `ControlOutside`），主視窗截圖也不是實際畫面 | 已修正：在規格螢幕上執行（STEP-13） |
| REV-7 | 1 | 提問 | `Tools/UICheck/VirtualDisplayRun.cs` | 虛擬螢幕的建立、100% 縮放與還原尚未在實際驅動上執行過；隱藏桌面上最大化是否取得含工作列的工作區域也待實測 | 結案：在 1920×1080 虛擬主螢幕上 `runtime` 全部通過，最大化的客戶區 1920×1009 與 Designer 相同；加入 Parsec 副螢幕後其工作區域 1920×1032（有工作列），設定失敗時副螢幕被移除且主螢幕不變；直式改以高大於寬的視窗呈現（DEC-13 第 7 項） |
| REV-8 | 5 | 提問 | `CncUI/Dialog/CNCOffsetDialog.cs` | 四軸時隱藏第五列，卡片底部留一列空白，對話框大小不變；`CNCPanel`、`CNCInfoPanel` 隱藏後版面仍整齊 | 不修正：維持現狀，與 `CNCInfoPanel` 隱藏後的呈現一致，對話框大小不隨控制器改變（DEC-14） |
| REV-9 | 6 | 建議 | 本回合的 commit | STEP-12 的前幾個 commit 標題沒有 `(STEP-12)` | 已修正：push 前改寫本機 commit 標題 |
| REV-10 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs` | 第一版把虛擬螢幕設為主螢幕並移動其他螢幕；在闔蓋的筆電上移動面板失敗後，已寫入的配置仍被套用，面板被卸離且無法掛回，遠端畫面全黑 | 已修正：不更動既有螢幕，只加入並設定虛擬副螢幕，失敗只移除它；結束後檢查其他螢幕（DEC-13 第 4 項）。本機以 Parsec 虛擬螢幕恢復畫面 |
| REV-11 | 5 | 建議 | `MyCAM/Layout/MainLayout.cs` | 使用者人工檢視：橫式加工頁的監控卡片上緣比右側「加工控制」卡片低 9 px | 已修正：底部列比例 0.25 → 0.265，Designer 與主視窗區域基準同步更新（DEC-14） |
