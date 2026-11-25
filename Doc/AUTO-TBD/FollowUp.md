# 後續處理

本議題（團隊與 AI 協作開發流程規範）範圍外的項目。流程規範本文在 `.github/team/`，常駐規範在 `.github/instructions/team-collaboration.instructions.md`；各項內容自足，接手時不需回溯本議題的討論。

## 1. 文件維護

### TODO-1 使用手冊維護機制

1. 目標：功能修改時，客戶操作手冊（`Documents/`，簡體中文，目前已落後）能同步更新。
2. 完成條件：
   1. 決定手冊的格式、存放位置與負責人。
   2. `team/issue-workflow.md` 或 DoD 加入「手冊是否受影響」的判斷與更新步驟；review 文件段已檢查手冊是否受影響，補上處理方式。
3. 延後原因：手冊的格式與負責人尚未決定，內容更新量大，不屬於流程成文化的範圍。
4. 建議做法：先盤點手冊與現行功能的落差，再決定是否改用可隨程式一起版控的格式（如 Markdown）。

## 2. 自動化

### TODO-2 自動化品質門檻

1. 目標：review 門檻段的機械式檢查由工具執行，文件中的對應規則縮為一行指引。
2. 完成條件，分三階段，各自可獨立完成：
   1. 本機檢查腳本：長期維護的檢查（常駐行數、格式與數字條列、連結有效、團隊通用層不含專案內容）以 PowerShell 腳本放在 repo，成員送 MR 前可執行。
   2. CI：GitLab CI 在 MR 上執行建置與檢查腳本，失敗時不可合併。
   3. 測試專案：建立 C# 測試專案，核心演算法有自動測試。
3. 延後原因：本議題以成文化現有機制為主；議題限定的檢查已在 `Doc/AUTO-TBD/Check.ps1`，長期維護版需要另行設計與團隊同意。
4. 建議做法：
   1. 以 `Doc/AUTO-TBD/Check.ps1` 為起點，抽出長期維護的檢查。
   2. 第一個候選：團隊通用層（`team-collaboration.instructions.md`、`team-coding-style-general.instructions.md`、`.github/team/*.md`）不得含專案專屬字詞（`MyCAM`、`Build.bat`、`CopyRef`、`FCLTool`、`PostTool`、`OCC`、`LanguageManager`、`Designer`、`reference/`、`.worktreeinclude`、`CustPost`、`zh-TW`、`AUTO-`），對應本議題 TEST-2。
   3. 每建立一項自動檢查，就把規範中對應的文字規則縮為一行指引。

### TODO-3 GitLab MR 建立自動化

1. 目標：議題分支 push 時自動建立 MR，標題與描述取自 release note 與 merge log。
2. 完成條件：`team/git-workflow.md` 寫明建立方式，並以一個實際議題驗證。
3. 延後原因：需要團隊決定工具與權限；push 與送 MR 仍由人決定，自動化只省去手動操作。
4. 建議做法：
   1. 不需額外環境：`git push` 的 push options（`-o merge_request.create`、`-o merge_request.target=<版本分支>`、`-o merge_request.title=<標題>`）。
   2. 需要工具與 access token：`glab` CLI 或 GitLab API，可帶入完整描述。

## 3. 規範調整

### TODO-4 既有分支依新規範調整

1. 目標：進行中的分支 `AUTO-23254_f_UIStyleUnify` 依新規範收尾。
2. 完成條件：rebase 到含新規範的版本分支後，議題文件與 commit 標示符合 `.github/team/issue-docs.md` 與 `git-workflow.md`，DoD 全部勾選。
3. 延後原因：該分支在新規範合併前已開工，須等本議題合併後才能 rebase。
4. 建議做法：見第 4 節 rebase 指引。

### TODO-5 Coding Style 與 `.editorconfig` 的重複

1. 目標：格式類規則只寫在 `.editorconfig`，Coding Style 文件只留一行指引；Claude 依副檔名載入各語言 Coding Style。
2. 完成條件：
   1. `team-coding-style-*.instructions.md` 中與 `.editorconfig` 重複的規則（編碼、換行、縮排、行長、行尾空白）刪除，改為一行「格式依 `.editorconfig`」。
   2. 評估 Claude Code 依檔案路徑自動載入規則的機制，可行時取代流程索引中「新增或修改程式碼」一列的導向；VS 2026 Copilot 實測也未依 `applyTo` 自動載入 `team-coding-style-csharp`，由 Copilot 依流程索引主動讀取，一併確認。
3. 延後原因：本議題只修訂 `team-coding-style-general` 的兩項規則；其餘 Coding Style 檔案只改名、更新連結與替換專案資訊，依「只改動到的檔案」原則留待後續。
4. 建議做法：先確認 `.editorconfig` 已涵蓋所有被刪的規則，再刪除文件內容。

## 4. `AUTO-23254_f_UIStyleUnify` rebase 指引

TODO-4 的做法。盤點日期 2026-11-25，依本機 ref（當時 `git fetch origin` 失敗），開工時先重新 fetch 並重跑第 1、3 項的盤點與衝突預估。

1. 現況：
   1. 分支只在本機（worktree `.claude/worktrees/ui-style-unify`），未 push；起點 `a4b3826d`，73 個 commit、無 merge commit，對 `origin/master`（`24c7276d`）ahead 73、behind 11。
   2. 修改 356 檔：Production Code 91、`Language/` XML 128、`*.Designer.cs` 36、議題文件 14、指引 5（改 `copilot-instructions.md`、`project-overview`、`CLAUDE.md`，新增 `ui-style.instructions.md`、`icon-design.instructions.md`）、`Tools/UICheck/` 14、圖示與資源 68。
   3. 議題文件在 `Doc/AUTO-23254/`，用舊編號 `R` / `S` / `T` 與數字檔名前綴；沒有 DoD 勾選清單、Release Note、`Review.md`；commit 沒有 `STEP` / `REV` 標示。依新規範屬 L 級。
2. 時機：等本議題以 squash 合併進目標版本分支後，再把 UI 分支 rebase 到該版本分支；先 rebase 到 `AUTO-TBD_f_DevProcess` 會帶入本議題的全部 commit。
3. 預估衝突（以本議題 `99dcd2e7`（DEC-18 之前）為對象逐 commit 模擬，停 6 次；實際 rebase 到版本分支時會不同，以重跑結果為準）：
   1. 指引：`CLAUDE.md`、`copilot-instructions.md`、`project-overview.instructions.md`（`origin/master` 與本議題都改過）。
   2. 程式：`MyCAM/Editor/Dialog/LockMSAxisDlg.cs`、`CNCThreePointDlg.cs`（`origin/master` 的 `AUTO-23220`、`AUTO-23240` 修正）。
4. 程式衝突的解法：
   1. `LockMSAxisDlg.cs`：保留 `master` 的修正邏輯，套 UI 的新控制項名稱；任一邊整份採用都會錯（採 UI 版會讓 `AUTO-23220` 的修正消失）。
   2. 同檔有 git 不報衝突但會編譯失敗的地方：自動併入的 `m_rdoMasterAuto_CheckedChanged` 仍用舊名 `m_rbtnMasterAuto`，要改為 `m_rdoMasterAuto`；rebase 後搜尋舊前綴 `m_rbtn`、`m_dbtn` 確認沒有殘留。
   3. `CNCThreePointDlg.cs`：刪掉建構子中訂閱圖紙欄位 `TextChanged` 的迴圈（`master` 已改由 `SetDrawingPoint` / `SetWorkpiecePoint` 主動觸發），圖紙欄位維持 UI 的寫法。
5. 指引衝突的解法：以本議題的版本為底，移植 UI 的新增內容。
   1. `ui-style`、`icon-design` 屬專案程式規範、依副檔名或路徑適用，依新命名改為 `project-coding-ui-style.instructions.md`、`project-coding-icon-design.instructions.md`（DEC-19）；不加 `CLAUDE.md` 的 `@` 引用（只有 `applyTo: "**"` 的指引常駐）。
   2. `copilot-instructions.md`：指引目錄加兩列，填「層」與「載入方式」。
   3. `project-overview`：`CommonUI` 的方案結構採 UI 版；「多語系」與 `project-coding-general` 第 1 項的語系措辭合併；「專案指引索引」第 1 項加入 `project-coding-ui-style`、`project-coding-icon-design`。
   4. 改名會讓連結失效，但新增檔與議題文件不會提示衝突：以 `git grep -nE "coding-style-|ui-style\.instructions|icon-design\.instructions"` 搜尋，結果全部改為新檔名（`team-coding-style-*`、`project-coding-ui-style`、`project-coding-icon-design`）。盤點時已知：`ui-style` 連到 `coding-style-*` 與 `icon-design`、`icon-design` 內的路徑字串、`Doc/AUTO-23254/` 的 `README.md`、`1_Goal.md`、`2_Spec.md`；UI 版 `project-overview` 連到 `ui-style` 的那一行位於本議題已刪除的段落，改由第 3 項的專案指引索引指向。
   5. 依 `copilot-instructions.md` 維護方式第 4 項，在議題文件寫明指引的異動與理由。
6. 格式：`MyCAM/Resource/IconSource/` 的兩個 `.ps1` 沒有 BOM，依 `.editorconfig` 補上，獨立 commit。
7. 步驟：
   1. 連上公司網路 `git fetch origin`；失敗就停下確認，不用舊的遠端 ref。
   2. 在 UI worktree 確認工作目錄乾淨，建立只留本機的備份分支，保住舊 hash（議題文件記錄的人工測試版本 `c4e6dd64` 等）。
   3. `git rebase <目標版本分支>`，依第 4、5 項解衝突。
   4. 依第 6 項補 `.ps1` 的 BOM，獨立 commit。
   5. 執行 `CopyRef.bat`（`Language/` 有更新）、`Build.bat Debug`、`Tools\UICheck\UICheck.bat runtime`、議題的 `Doc\AUTO-23254\Tests\RunIssueChecks.bat`。
   6. 人工確認鎖軸與三點校正兩個對話框（含 CNC 連線），確認 `master` 的兩個修正仍有效。
   7. 補議題文件：`README.md` 的分支起點、狀態、L 級 DoD、Release Note；`TestPlan` 的結果；後續處理中「master 只前進 2 個 commit」的過時描述。
   8. 依新格式做一回合獨立 review，寫入 `Review.md`，處理完「必修」。
   9. 由人決定 remote 與分支後 push、送 MR。
8. 開工時先由人決定（建議在前）：
   1. Rebase 方式：直接 rebase 保留 73 個 commit；另一選項是本機暫時 squash 後再 rebase，只解一次衝突。
   2. 既有 commit 沒有 `STEP` 標示：不改舊訊息，`README.md` 註明依相容性規則不適用，rebase 後的新 commit 標 `(STEP-n)` / `(REV-n)`。
   3. 舊文件中新增項目的編號：用新前綴接續（`TEST-18`、`STEP-12`），符合「新增內容採新規範」。
   4. 既有後續處理缺完成條件：視為既有內容不轉換，只有新增項用 `TODO` 完整格式。
   5. 獨立 review：先前 6 回合 VS Copilot review 的結論已在討論記錄；rebase 後再做一回合，重點為衝突解法、`master` 併入與指引移植。
   6. 議題檢查腳本的固定基準 `a4b3826d`：改為 rebase 目標 commit，文件註明原基準與備份分支；沿用舊基準會把 `master` 與本議題的變更算成差異。
   7. `UICheck.bat` 是否加入 `project/review.md` 的專案固定檢查項目：建議加入門檻；`project-coding-general` 的語系措辭改為「`Language/` 中所有語系」。
   8. 目標版本：本議題與 UI 議題進同一個下一版，本議題先合。
   9. 人工確認範圍：自動檢查全跑，人工只重做衝突涉及的兩個對話框與其 Designer。
