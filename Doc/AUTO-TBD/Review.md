# Review

依 [review.md](../../.github/process/review.md) 的報告格式，每回合一節。

## 第 1 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；發現來自 STEP-12 動態驗證（TEST-15 演練 session、TEST-16 VS 2026 Copilot） |
| 日期 | 2026-11-24 |

1. 門檻：`Check.ps1` 除 TEST-11（待本 STEP 收官）外通過；常駐 190 行（基準 193，差異 -3），未增加。本議題只改文件，不需建置。
2. 本回合只處理動態驗證中發現的問題；完整六段 review 於第 2 回合進行。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-1 | 2 | 必修 | `.github/process/git-workflow.md:60` | 「一個議題一個 worktree」為硬性規定，團隊多數成員單線開發，且建立與準備步驟只適用 Claude Code（`.worktreeinclude` 只有 Claude Code 自動複製） | 已修正（回溯至 SPEC-8.7，DEC-12） |
| REV-2 | 2 | 必修 | `.github/process/issue-workflow.md:29` | 開議題流程沒有規定何時開分支；`issue-start` 必讀文件沒有 `git-workflow.md`，AI 要靠流程索引自行補讀 | 已修正（回溯至 SPEC-2、SPEC-3.3，DEC-12） |
| REV-3 | 6 | 建議 | `.github/process/git-workflow.md:14` | 分支命名 `AUTO-<議題編號>_…` 與 `Doc/<議題編號>/`（`AUTO-TBD`）對「議題編號」是否含前綴的定義不一致 | 已修正（回溯至 SPEC-8.3.1：議題編號含前綴） |
| REV-4 | 6 | 建議 | `.github/process/issue-docs.md:65` | S 級 `README.md` 範本缺「現況盤點」與「待討論」，與同檔第 2 節的內容歸屬及確認點文件規則不一致 | 已修正 |
| REV-5 | 6 | 建議 | `.github/process/review.md:23` | S 級報告寫在 `README.md` 的 Review 章節，範本的回合標題同為 `##`，層級衝突 | 已修正 |
| REV-6 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:5` | `param()` 未加 `[CmdletBinding()]`，打錯參數名會被忽略並改用預設比對基準 | 已修正 |
| REV-7 | 6 | 建議 | `Doc/AUTO-TBD/FollowUp.md:60` | TEST-16 確認 VS 2026 Copilot 可讀取 `.claude/skills/`，TODO-6 的前提不成立 | 已修正（刪除 TODO-6；Copilot 未依 `applyTo` 載入 `coding-style-csharp` 併入 TODO-5） |
| REV-8 | 1 | 必修 | `Doc/AUTO-TBD/Check.ps1:446` | TEST-7 把 review 報告規定的位置格式 `檔案:行號` 整串當成路徑，報告一定被判定失敗 | 已修正 |

## 第 2 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；獨立 session，未參與規範撰寫 |
| 日期 | 2026-11-24 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL；TEST-11 只缺 `README.md` 的 DoD 勾選與 Release Note 條列，屬收官前的預期狀態。常駐 193 行（入口 48、`collaboration` 34、`project-overview` 79、`coding-style-general` 32），基準 193，差異 0，未增加。本議題只改文件，不需建置；專案固定檢查項目 2～4 不適用。
2. 需求與範圍：REQ-1～REQ-10 皆有 SPEC，SPEC-1～SPEC-19 皆有 TEST 與 STEP；DEC-12 落實於 `git-workflow.md` 第 5 節、`issue-workflow.md` 第 2 節第 2 項、`issue-start` 必讀清單、`build.md` worktree 準備，DEC-13 落實於 `collaboration` 協作原則第 2 項、`review.md` 第 3 節第 1 項；TEST-5 通過，沒有範圍外修改。
3. 第 3 段以 `Check.ps1` 為對象（含以 `-Base deadbeef` 實際執行），第 5 段以 `Check.ps1` 與規範設計為對象；第 6 段即 TEST-10 文件精簡審查。
4. 本回合沒有「必修」。
5. 修正後常駐 194 行，比基準增加 1 行（REV-15）：UI 執行緒、角度單位、座標系是本專案 review 必查的領域項目，隨專案固定檢查項目常駐，review 時才不會漏看。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-9 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:329` | `-Base` 為不存在的 commit 時，`git diff` 失敗只印錯誤、回傳空清單，TEST-5 誤判通過（實測 `-Base deadbeef`：TEST-5 PASS）；應先以 `git rev-parse --verify` 檢查基準，失敗即中止 | 已修正（待人確認） |
| REV-10 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:163` | TEST-1 的基準行數寫死 193，不隨 `-Base` 改變，與 STEP-1 第 1.2 項「`-Base` 供 TEST-1 比對」不符；改以 `git show <Base>:<檔案>` 計算基準，或在參數說明註明 TEST-1 不受 `-Base` 影響 | 已修正（待人確認） |
| REV-11 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:594` | TEST-11 對非必修只檢查處置非空、對必修只檢查開頭為「已修正」，「待處置」與「已修正（待人確認）」都會通過，與 `review.md` 第 3 節第 4 項「送 MR 前不留未處置或待確認」不符 | 已修正（待人確認） |
| REV-12 | 5 | 建議 | `Doc/AUTO-TBD/Check.ps1:640` | TEST-12 讀取執行者本人的 `~/.claude/CLAUDE.md` 與 `CLAUDE.local.md`，合併者或其他成員執行時必定 FAIL 或執行錯誤，門檻結果因人而異；建議偵測到檔案不存在時改為「略過」並輸出說明 | 已修正（待人確認） |
| REV-13 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:331` | TEST-5 只檢查「異動都在清單內」，清單內檔案未異動不會發現；TEST-5 通過條件寫「一致」，建議補反向比對或把通過條件改為「異動不超出清單」 | 已修正（待人確認） |
| REV-14 | 5 | 建議 | `.github/instructions/collaboration.instructions.md:32` | 流程索引的「執行或回應 review」只導向 `review.md` 與專案固定檢查項目；只做 review、不改程式碼時不會觸發「新增或修改程式碼」列，Claude 不會讀 `coding-style-<語言>`，第 4 段規範符合缺依據；建議該列加上「review 程式碼時加讀對應語言的 Coding Style」 | 已修正（回溯至 SPEC-1.5） |
| REV-15 | 2 | 提問 | `.github/process/review.md:13` | 第 3 段列「UI 執行緒、角度單位（rad / deg）、座標系」，屬 CAD/CAM 與 WinForms 領域；團隊其他 repo 若不是同領域，違反 SPEC-1.2「團隊通用層不含專案內容」，應移到專案固定檢查項目。團隊其他 repo 是否同屬此領域？ | 已修正（回溯至 SPEC-7.1、SPEC-14.4） |
| REV-16 | 6 | 提問 | `.github/process/issue-workflow.md:25` | S 級判斷依據只有「小 bug」，小型功能或純文字修改（如修正 UI 字串）讀者無法判斷屬 S 或 M；是否放寬為「影響範圍明確的小修改（含小 bug）」？ | 已修正（回溯至 SPEC-3.2） |
| REV-17 | 5 | 建議 | `.github/process/git-workflow.md:26` | Commit 標示只定義施工 `(STEP-n)` 與 review `(REV-n)`；確認點文件（階段 1～5）與回溯修改的 commit 沒有規則，本分支有 8 個未標示的 commit、`bd826055` 自訂 `(DEC-11)`，`Check.ps1` 以「只動議題文件即豁免」默認處理。建議在規範寫明確認點文件不標示（或標示 `DEC-n`），與腳本一致 | 已修正（回溯至 SPEC-8.5.3：確認點文件的 commit 不標示） |
| REV-18 | 2 | 建議 | `Doc/AUTO-TBD/Spec.md:264` | SPEC-12.2.1 協作原則（資深工程師角度、DEC-13 依重要性先問或先做）沒有對應的 REQ，SPEC-12 只標 REQ-3.1、REQ-3.2（載入時機）；DEC-13 回溯時未補需求來源。建議補一條 REQ 或在 REQ-4 下增列子項 | 已修正（回溯至 REQ-11、SPEC-12） |
| REV-19 | 6 | 建議 | `.github/copilot-instructions.md:23` | 指引目錄寫各語言 Coding Style「Copilot 依 `applyTo`」，但 TEST-16 實測 VS 2026 Copilot 未依 `applyTo` 自動載入 `coding-style-csharp`；讀者會以為 Copilot 不需依索引讀取。建議改為「Copilot、Claude 皆依流程索引（Copilot 另有 `applyTo`）」，或由 TODO-5 處理 | 已修正 |
| REV-20 | 6 | 建議 | `.github/process/issue-docs.md:29` | 規則寫「確認點文件的最後一節為『待討論』」，但 S 級範本的待討論之後還有 DoD 與 Release Note；M 級 `README.md` 承載確認點 1–2 的內容，骨架卻沒有待討論。建議改為「確認點內容之後列『待討論』」，並在 M、L 骨架補上 | 已修正（待人確認） |
| REV-21 | 6 | 建議 | `Doc/AUTO-TBD/Goal.md:118` | `Goal.md` 是確認點 1、2 的文件，沒有「待討論」一節，不符 `issue-docs.md` 第 2 節第 2 項；Spec、TestPlan、Implementation 都有 | 已修正（待人確認） |
| REV-22 | 6 | 建議 | `.github/project/build.md:50` | 與同檔第 30 行矛盾：第 30 行寫 `CopyRef.bat`「首次建置後執行一次」，第 50 行寫「第一次建置前執行」（腳本會自建目錄，兩者皆可）；同句「以 Visual Studio 開啟前先卸載 `FCLTool`」順序不可行，應為開啟後卸載（第 12 行已寫） | 已修正（待人確認） |
| REV-23 | 6 | 建議 | `.github/process/git-workflow.md:10` | 「Push 與送 MR 由人決定」與常駐的 `collaboration` 協作原則第 2.1 項（合併、push 先由人決定）重複；建議只留本處的具體做法「AI push 前先詢問 remote 與分支」 | 已修正（待人確認） |
| REV-24 | 6 | 建議 | `.github/process/review.md:11` | 門檻段要求計算常駐行數，但常駐的範圍（入口與 `applyTo: "**"`）定義在 `copilot-instructions.md` 維護方式第 3 項，`review.md` 沒有連結，reviewer 無法判斷要算哪些檔案 | 已修正（待人確認） |
| REV-25 | 6 | 建議 | `.claude/skills/issue-start/SKILL.md:6` | 三個 skill 的本文句子與「必讀文件」清單列出相同文件，同一檔內重複；`review` skill 另重述流程索引已有的「加上專案固定檢查項目」。此格式來自 SPEC-16.2，精簡須改規格 | 不修正（本文說明各文件的用途，清單供 AI 依序讀檔；每檔只多兩三行，TEST-16 證明此格式有效） |
| REV-26 | 2 | 建議 | `Doc/AUTO-TBD/TestPlan.md:134` | TEST-13 通過條件寫「含 SPEC-18.2 六項」，實際為五項（第 5 項依 TEST-16 結果結案），執行結果判定通過但條件字面不成立；建議改為「第 5 項依 TEST-16 結果決定」 | 已修正（待人確認） |
| REV-27 | 6 | 建議 | `Doc/AUTO-TBD/README.md:19` | 文件索引沒有 `Review.md`，`Review.md` 已存在；STEP-13 第 2 項列為收官工作，可於收官一併補上 | 待處置 |
| REV-28 | 6 | 建議 | `Doc/AUTO-TBD/TestPlan.md:202` | 執行結果 TEST-11 仍寫「缺 `Review.md`、STEP-12、STEP-13 的 commit」，與目前狀態不符（現僅缺 DoD 與 Release Note）；`753ee71d` 更新了行數但未同步此項 | 待處置 |
| REV-29 | 6 | 建議 | `Doc/AUTO-TBD/Implementation.md:61` | STEP-6 第 1 項「目標 40 行內」是取消行數上限（DEC-11）前的殘留；`Discussion.md:78` DEC-9.1「專案準備步驟留在 `project-overview`」已被 DEC-10.1 取代，未如 DEC-11 標示「取代」 | 已修正（待人確認） |
