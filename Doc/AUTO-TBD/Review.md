# Review

依 [process-review.md](../../.github/team/process-review.md) 的報告格式，每回合一節。

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
| REV-1 | 2 | 必修 | `.github/team/git-workflow.md:60` | 「一個議題一個 worktree」為硬性規定，團隊多數成員單線開發，且建立與準備步驟只適用 Claude Code（`.worktreeinclude` 只有 Claude Code 自動複製） | 已修正（回溯至 SPEC-8.7，DEC-12） |
| REV-2 | 2 | 必修 | `.github/team/issue-workflow.md:29` | 開議題流程沒有規定何時開分支；`issue-start` 必讀文件沒有 `git-workflow.md`，AI 要靠流程索引自行補讀 | 已修正（回溯至 SPEC-2、SPEC-3.3，DEC-12） |
| REV-3 | 6 | 建議 | `.github/team/git-workflow.md:14` | 分支命名 `AUTO-<議題編號>_…` 與 `Doc/<議題編號>/`（`AUTO-TBD`）對「議題編號」是否含前綴的定義不一致 | 已修正（回溯至 SPEC-8.3.1：議題編號含前綴） |
| REV-4 | 6 | 建議 | `.github/team/issue-docs.md:65` | S 級 `README.md` 範本缺「現況盤點」與「待討論」，與同檔第 2 節的內容歸屬及確認點文件規則不一致 | 已修正 |
| REV-5 | 6 | 建議 | `.github/team/review.md:23` | S 級報告寫在 `README.md` 的 Review 章節，範本的回合標題同為 `##`，層級衝突 | 已修正 |
| REV-6 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:5` | `param()` 未加 `[CmdletBinding()]`，打錯參數名會被忽略並改用預設比對基準 | 已修正 |
| REV-7 | 6 | 建議 | `Doc/AUTO-TBD/FollowUp.md:60` | TEST-16 確認 VS 2026 Copilot 可讀取 `.claude/skills/`，TODO-6 的前提不成立 | 已修正（刪除 TODO-6；Copilot 未依 `applyTo` 載入 `team-coding-style-csharp` 併入 TODO-5） |
| REV-8 | 1 | 必修 | `Doc/AUTO-TBD/Check.ps1:446` | TEST-7 把 review 報告規定的位置格式 `檔案:行號` 整串當成路徑，報告一定被判定失敗 | 已修正 |

## 第 2 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；獨立 session，未參與規範撰寫 |
| 日期 | 2026-11-24 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL；TEST-11 只缺 `README.md` 的 DoD 勾選與 Release Note 條列，屬收官前的預期狀態。常駐 193 行（入口 48、`collaboration` 34、`project-overview` 79、`team-coding-style-general` 32），基準 193，差異 0，未增加。本議題只改文件，不需建置；專案固定檢查項目 2～4 不適用。
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
| REV-14 | 5 | 建議 | `.github/instructions/team-collaboration.instructions.md:32` | 流程索引的「執行或回應 review」只導向 `review.md` 與專案固定檢查項目；只做 review、不改程式碼時不會觸發「新增或修改程式碼」列，Claude 不會讀 `team-coding-style-<語言>`，第 4 段規範符合缺依據；建議該列加上「review 程式碼時加讀對應語言的 Coding Style」 | 已修正（回溯至 SPEC-1.5） |
| REV-15 | 2 | 提問 | `.github/team/review.md:13` | 第 3 段列「UI 執行緒、角度單位（rad / deg）、座標系」，屬 CAD/CAM 與 WinForms 領域；團隊其他 repo 若不是同領域，違反 SPEC-1.2「團隊通用層不含專案內容」，應移到專案固定檢查項目。團隊其他 repo 是否同屬此領域？ | 已修正（回溯至 SPEC-7.1、SPEC-14.4） |
| REV-16 | 6 | 提問 | `.github/team/issue-workflow.md:25` | S 級判斷依據只有「小 bug」，小型功能或純文字修改（如修正 UI 字串）讀者無法判斷屬 S 或 M；是否放寬為「影響範圍明確的小修改（含小 bug）」？ | 已修正（回溯至 SPEC-3.2） |
| REV-17 | 5 | 建議 | `.github/team/git-workflow.md:26` | Commit 標示只定義施工 `(STEP-n)` 與 review `(REV-n)`；確認點文件（階段 1～5）與回溯修改的 commit 沒有規則，本分支有 8 個未標示的 commit、`bd826055` 自訂 `(DEC-11)`，`Check.ps1` 以「只動議題文件即豁免」默認處理。建議在規範寫明確認點文件不標示（或標示 `DEC-n`），與腳本一致 | 已修正（回溯至 SPEC-8.5.3：確認點文件的 commit 不標示） |
| REV-18 | 2 | 建議 | `Doc/AUTO-TBD/Spec.md:264` | SPEC-12.2.1 協作原則（資深工程師角度、DEC-13 依重要性先問或先做）沒有對應的 REQ，SPEC-12 只標 REQ-3.1、REQ-3.2（載入時機）；DEC-13 回溯時未補需求來源。建議補一條 REQ 或在 REQ-4 下增列子項 | 已修正（回溯至 REQ-11、SPEC-12） |
| REV-19 | 6 | 建議 | `.github/copilot-instructions.md:23` | 指引目錄寫各語言 Coding Style「Copilot 依 `applyTo`」，但 TEST-16 實測 VS 2026 Copilot 未依 `applyTo` 自動載入 `team-coding-style-csharp`；讀者會以為 Copilot 不需依索引讀取。建議改為「Copilot、Claude 皆依流程索引（Copilot 另有 `applyTo`）」，或由 TODO-5 處理 | 已修正 |
| REV-20 | 6 | 建議 | `.github/team/issue-docs.md:29` | 規則寫「確認點文件的最後一節為『待討論』」，但 S 級範本的待討論之後還有 DoD 與 Release Note；M 級 `README.md` 承載確認點 1–2 的內容，骨架卻沒有待討論。建議改為「確認點內容之後列『待討論』」，並在 M、L 骨架補上 | 已修正（待人確認） |
| REV-21 | 6 | 建議 | `Doc/AUTO-TBD/Goal.md:118` | `Goal.md` 是確認點 1、2 的文件，沒有「待討論」一節，不符 `issue-docs.md` 第 2 節第 2 項；Spec、TestPlan、Implementation 都有 | 已修正（待人確認） |
| REV-22 | 6 | 建議 | `.github/project/build.md:50` | 與同檔第 30 行矛盾：第 30 行寫 `CopyRef.bat`「首次建置後執行一次」，第 50 行寫「第一次建置前執行」（腳本會自建目錄，兩者皆可）；同句「以 Visual Studio 開啟前先卸載 `FCLTool`」順序不可行，應為開啟後卸載（第 12 行已寫） | 已修正（待人確認） |
| REV-23 | 6 | 建議 | `.github/team/git-workflow.md:10` | 「Push 與送 MR 由人決定」與常駐的 `collaboration` 協作原則第 2.1 項（合併、push 先由人決定）重複；建議只留本處的具體做法「AI push 前先詢問 remote 與分支」 | 已修正（待人確認） |
| REV-24 | 6 | 建議 | `.github/team/review.md:11` | 門檻段要求計算常駐行數，但常駐的範圍（入口與 `applyTo: "**"`）定義在 `copilot-instructions.md` 維護方式第 3 項，`review.md` 沒有連結，reviewer 無法判斷要算哪些檔案 | 已修正（待人確認） |
| REV-25 | 6 | 建議 | `.claude/skills/issue-start/SKILL.md:6` | 三個 skill 的本文句子與「必讀文件」清單列出相同文件，同一檔內重複；`review` skill 另重述流程索引已有的「加上專案固定檢查項目」。此格式來自 SPEC-16.2，精簡須改規格 | 不修正（本文說明各文件的用途，清單供 AI 依序讀檔；每檔只多兩三行，TEST-16 證明此格式有效） |
| REV-26 | 2 | 建議 | `Doc/AUTO-TBD/TestPlan.md:134` | TEST-13 通過條件寫「含 SPEC-18.2 六項」，實際為五項（第 5 項依 TEST-16 結果結案），執行結果判定通過但條件字面不成立；建議改為「第 5 項依 TEST-16 結果決定」 | 已修正（待人確認） |
| REV-27 | 6 | 建議 | `Doc/AUTO-TBD/README.md:19` | 文件索引沒有 `Review.md`，`Review.md` 已存在；STEP-13 第 2 項列為收官工作，可於收官一併補上 | 已修正 |
| REV-28 | 6 | 建議 | `Doc/AUTO-TBD/TestPlan.md:202` | 執行結果 TEST-11 仍寫「缺 `Review.md`、STEP-12、STEP-13 的 commit」，與目前狀態不符（現僅缺 DoD 與 Release Note）；`753ee71d` 更新了行數但未同步此項 | 已修正 |
| REV-29 | 6 | 建議 | `Doc/AUTO-TBD/Implementation.md:61` | STEP-6 第 1 項「目標 40 行內」是取消行數上限（DEC-11）前的殘留；`Discussion.md:78` DEC-9.1「專案準備步驟留在 `project-overview`」已被 DEC-10.1 取代，未如 DEC-11 標示「取代」 | 已修正（待人確認） |

## 第 3 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；發現來自使用者的人工 review |
| 日期 | 2026-11-25 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL；TEST-11 只缺 DoD 第 9、10 項與待人確認的處置。常駐 194 行（基準 193，差異 +1）。本議題只改文件，不需建置。
2. 常駐增加的理由（REV-30）：指引目錄與 `project-overview` 索引各多一列指向 `claude-code.md`（+2），切換 session、使用 worktree 時才找得到該文件；`CLAUDE.md` 精簡 -2（REV-31）；`project-overview` 索引改為兩項條列 +1。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-30 | 2 | 必修 | `.github/team/issue-workflow.md:45` | Session 交接、`SESSION.md`、Claude Code 的 worktree 位置與分支更名、`.worktreeinclude` 自動複製、`CLAUDE.local.md` 上層載入屬於使用 Claude 或 worktree 時的做法，散在通用流程與 `build.md`；Copilot 不同、成員也未必有此習慣 | 已修正（回溯至 REQ-5.1、SPEC-14，刪除 SPEC-3.8、SPEC-5.8，DEC-14：集中到 `claude-code.md`） |
| REV-31 | 6 | 建議 | `CLAUDE.md:3` | 3 行說明可併為 1 行；`@` 引用 4 行保留 | 已修正（待人確認） |
| REV-32 | 5 | 建議 | `.github/team/review.md:14` | 規範段只列 Coding Style，流程索引的 review 列也另寫 Coding Style；之後新增 UI 等規範時兩處都要改，容易漏 | 已修正（回溯至 SPEC-1.5、SPEC-7.1，DEC-15） |

## 第 4 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；收官前全域自我 review（子 session），範圍為整個分支的最終結果 |
| 日期 | 2026-11-25 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL；TEST-11 只缺 DoD 第 9、10 項與待人決定或待人確認的處置。常駐 202 行（基準 193，差異 +9）：第 3 回合 +1；本回合 `project-overview` 新增「議題編號」+4（REV-43），開分支、commit、寫註解時都要用到前綴格式；`team-collaboration` 回覆人的方式 +4（REV-46），每次回覆都適用；文件通則分段 +4（REV-47），可讀性優先；維護方式 +1（REV-45）；分層說明與指引目錄的「層」欄 +4、流程索引 Claude Code 列 +1、專案固定檢查項目移出常駐 -10（REV-48）。檔案位置以改名後的路徑表示（DEC-18）。本議題只改文件，不需建置。
2. 需求與範圍：REQ-1～REQ-11 皆有 SPEC，SPEC-1～SPEC-19 皆有 TEST 與 STEP；被刪除的 SPEC-3.8、SPEC-5.8 只在討論與 review 記錄中以「刪除」出現。DEC-14、DEC-15 已落實於 `claude-code.md`、`build.md`、`git-workflow.md` 第 5 節、`issue-workflow.md`、`issue-docs.md`、流程索引、指引目錄與 `project-overview` 索引，沒有殘留的 session 或 worktree 說明。
3. REV-43 依使用者回覆改為「必修」並回溯修正。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-33 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:165` | DEC-15 的說明句插在 SPEC-7.1 表格第 4、5 列之間，第 5、6 段兩列脫離表格，無法正確顯示 | 已修正（待人確認） |
| REV-34 | 6 | 建議 | `Doc/AUTO-TBD/TestPlan.md:211` | TEST-8 位置對照與目前檔案不符：`IW:21-34` 應為 `21-35`（待討論三部分在 33～35 行）、`RV:44-49` 應為 `44-50`（第 5 項回到門檻段）、5.3 的 `issue-docs.md:23` 應為 `:22` | 已修正（待人確認） |
| REV-35 | 2 | 建議 | `Doc/AUTO-TBD/Implementation.md:68` | DEC-14 回溯未完整同步 STEP：STEP-7 沒有新增 `claude-code.md` 一項、索引只寫 `build.md`、專案固定檢查項目仍寫「四項」（REV-15 後為五項）；STEP-6.2 指引目錄漏列 `claude-code.md`；STEP-3 對應的 `.gitignore` 檢查應為 TEST-4.6 而非 4.5 | 已修正（待人確認） |
| REV-36 | 6 | 建議 | `.claude/skills/review/SKILL.md:6` | 寫「門檻段與規範段加上專案固定檢查項目」，但專案固定檢查項目也涵蓋功能正確性段（第 5 項），讀者會漏看；改為不指定段，段的對應由 `project-overview` 說明 | 已修正（待人確認） |
| REV-37 | 6 | 建議 | `Doc/AUTO-TBD/Review.md:84` | REV-30 處置寫「回溯至 SPEC-3.8、SPEC-5.8」，兩者已刪除，讀者找不到；改寫為「刪除 SPEC-3.8、SPEC-5.8」 | 已修正（待人確認） |
| REV-38 | 6 | 建議 | `Doc/AUTO-TBD/Discussion.md:85` | DEC-9.6 的「硬上限」已由 DEC-11 取消，DEC-9.6 本身未標示；DEC-9.1 已有同類標示（REV-29） | 已修正（待人確認） |
| REV-39 | 2 | 建議 | `Doc/AUTO-TBD/Spec.md:126` | SPEC-5.4 表格寫「STEP 與 commit 對應」，與 SPEC-8.5.4「`Implementation.md` 不記 commit hash」及 `issue-docs.md:22` 的「STEP」不一致；TEST-8 以措辭差異接受，但規格本身前後矛盾。建議改為「STEP」 | 已修正（使用者同意依建議修正） |
| REV-40 | 2 | 建議 | `Doc/AUTO-TBD/Spec.md:18` | 規格未同步後續 review 的修正：SPEC-1.1 表格仍寫「Copilot 依 `applyTo` 載入；Claude 依流程索引」（REV-19 後指引目錄改為兩者皆依流程索引）；SPEC-8.6 仍寫「Push 與送 MR 由人決定」（REV-23 後 push 的決定權只寫在協作原則） | 已修正（使用者同意依建議修正） |
| REV-41 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:85` | SPEC-6.4「刪除即跳號」與 Markdown 顯示衝突：有序清單只採用第一項的號碼，SPEC-3 的「9.」、SPEC-5 的「9.」在 GitLab / GitHub 顯示為「8.」，引用 SPEC-3.9 的讀者對不上。建議刪除的項目保留一行「8. （刪除，見 DEC-n）」 | 已修正（回溯至 SPEC-6.4，DEC-17；使用者同意依建議修正） |
| REV-42 | 4 | 建議 | `.github/team/review.md:12` | 功能正確性段寫「專案指引的領域檢查項目」，`project-overview` 沒有此名稱，對應的是「專案固定檢查項目」第 5 項；SPEC-7.1 同文。建議改為「專案指引的專案固定檢查項目」 | 已修正（使用者同意依建議修正） |
| REV-43 | 2 | 必修 | `.github/instructions/team-coding-style-general.instructions.md:26` | 團隊通用層含 `AUTO-` 前綴：`team-coding-style-general` 的 `// AUTO-<編號>`、`issue-docs.md:8` 的 `AUTO-TBD`、`git-workflow.md:14` 的範例 `AUTO-23201`。`AUTO-` 是團隊共用的議題系統前綴，還是 MyCAM 專屬？若為專屬，違反 SPEC-1.2，應改為「`<議題編號>`，前綴見專案指引」。使用者回覆：`AUTO-` 只有 MyCAM 使用，改為必修 | 已修正（回溯至 REQ-1.2、SPEC-5.2、SPEC-8.3.1、SPEC-10.2、SPEC-14.7，DEC-16） |
| REV-44 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:649` | TEST-11 只擋空白、「待處置」與「待人確認」，其他文字（如「待人決定」、「待回覆」）都通過，與 `review.md` 第 2 節第 2 項「處置只有三種」不符；改為處置須以已修正、不修正或列後續開頭 | 已修正（待人確認） |
| REV-45 | 6 | 建議 | `.github/copilot-instructions.md:38` | 使用者人工 review：維護方式第 3 項讀起來像「不適合改常駐檔、改了要注意行數」，沒講到重點；重點是常駐耗成本，維護時以通則與精簡為主；且不論常駐與否，修改指引文件都應說明並在 review 檢查 | 已修正（待人確認）：第 3 項改寫，新增第 4 項；`review.md` 文件段加「修改的指引文件是否寫明異動與理由」；SPEC-7.1、SPEC-15.1.4 同步 |
| REV-46 | 6 | 建議 | `~/.claude/CLAUDE.md:4` | 使用者人工 review：個人設定「溝通與文件」第 2～4 項（待決三部分各一兩句、待確認列在回覆、編號附說明）是團隊可共用的 AI 溝通原則 | 已修正（待人確認）：移入 `team-collaboration` 協作原則第 3 項；個人檔留一行指向團隊規範（沒有該規範的 repo 照同樣原則）；SPEC-12.2.1、SPEC-17.2 同步 |
| REV-47 | 6 | 建議 | `.github/instructions/team-collaboration.instructions.md:19` | 使用者人工 review：文件通則「精簡」一句太長 | 已修正（待人確認）：分為四個子項，內容不變 |
| REV-48 | 5 | 建議 | `.github/copilot-instructions.md:5` | 使用者人工 review：指引的四個面向中，通用 / 專案、常駐 / 細節、Claude / 通用在檔案結構上看不出來；`process/` 與 `project/` 命名軸不同、`instructions/` 未分層、Claude 限定內容只有專案層一份且混入通用的 session 交接；專案固定檢查項目只在 review 用到卻常駐 | 已修正（回溯至 SPEC-1.1、SPEC-13.6、SPEC-14、SPEC-15.1.2，DEC-18；使用者同意依建議） |
| REV-49 | 1 | 必修 | `.github/project/claude-code.md:1` | 施工中曾把檔案命名為 `claude.md`，Windows 不分大小寫，Claude Code 立即把它當成 `CLAUDE.md` 自動載入，等於常駐 | 已修正：改名為 `claude-code.md`，維護方式寫明、TEST-4 檢查（DEC-18） |
| REV-50 | 6 | 建議 | `Doc/AUTO-TBD/FollowUp.md:42` | 使用者要求：TODO-4 的建議做法只有一句，接手 UI 分支 rebase 的人不知道衝突點與步驟 | 已修正（待人確認）：新增第 4 節 rebase 指引，依子 session 的唯讀盤點撰寫 |

## 第 5 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess`（HEAD `0788d340`） |
| 比對基準 | `24c7276d`；重點範圍 `857aaab5~1..HEAD`（REV-43 之後） |
| 工具與模型 | Claude Code、Claude Opus 5.5；獨立 review（子 session，未參與撰寫），由主 session 整合 |
| 日期 | 2026-11-25 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL，TEST-11 只缺 DoD 第 9、10 項與待人確認的處置。手算常駐：基準 193、HEAD 202、差異 +9，與第 4 回合的拆解相符；腳本在部分執行環境把基準算成 122（REV-51）。本議題只改文件，不需建置。
2. 需求與範圍：REQ-1～REQ-11 都有 SPEC，SPEC-1～SPEC-19 都有 TEST 與 STEP；SPEC 對照漏了 SPEC-13 的 STEP-7（REV-59）；動態測試結果早於 DEC-18（REV-55）。
3. DEC-18 落地：`.github/`、skills、`CLAUDE.md`、`.editorconfig`、`.worktreeinclude` 沒有殘留舊路徑；舊名只出現在 Discussion、Review、STEP-7.4 與 `Check.ps1` 的改名例外，屬合理；議題文件的簡稱 `collaboration` 未同步（REV-58）。
4. 分層：兩份 `claude-code.md` 分工清楚，指引目錄的層與載入方式和 frontmatter、`@` 引用一致；語言 Coding Style 含專案內容（REV-52），撰寫程式時要用的三條專案規則不再常駐（REV-53）。
5. TestPlan 5.2 的 22 列 `檔案:行號` 逐一對照，全部正確。
6. FollowUp 第 4 節可照做，但漏列改名後失效的連結、引用項次有誤（REV-56、REV-57）。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-51 | 1 | 必修 | `Doc/AUTO-TBD/Check.ps1:165` | `Get-BaseResidentLineCount` 以 `@( git show ... )` 取基準檔內容；主 session 驗證：在 PowerShell 主控台執行時基準為 193，經 Bash 呼叫（輸出被重導）時基準為 122，空行被丟掉。現況行數用 `Get-LineCount`，兩邊口徑不同，門檻報告會誤判常駐增量 | 已修正（使用者同意）：`git show` 對 blob 不理會 `--output`，改以 `Process` 直接讀標準輸出，與 `Get-LineCount` 同一口徑；PowerShell 主控台與經 Bash 呼叫皆為基準 193、差異 +9 |
| REV-52 | 2 | 必修 | `.github/instructions/team-coding-style-cpp.instructions.md:7` | DEC-18 把語言 Coding Style 歸為團隊通用，內容卻有 MyCAM 專屬資訊，違反 SPEC-1.2：cpp 第 7 行「適用於 `PostTool`、`FCLTool`」、csharp 第 139 行 .NET Framework 4.8 / C# 7.3；TEST-2 未掃描這兩份 | 已修正（使用者同意依建議）：cpp 改為「適用於 C / C++ 原始碼」，C# 版本改為「依專案指引的技術棧」；TEST-2 掃描範圍加入兩份並新增 `.NET Framework`；SPEC-11、TODO-5.3 同步 |
| REV-53 | 5 | 建議 | `.github/instructions/project-overview.instructions.md:57` | 四語系 XML 同步、Designer 可開、不改 `reference/` 是寫程式時就要遵守的規則，DEC-18 後只在 review 才讀的 `project/review.md`，AI 新增文字時讀不到 | 已修正（使用者同意依建議）：新增專案程式規範一類 `project-coding-*`，本議題新增 `project-coding-general` 放三項規則，流程索引與專案指引索引導向；功能正確性的領域項目只列名詞、無法照做，刪除；常駐 +3，合計 205（DEC-19） |
| REV-54 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:23` | DEC-18 的說明句插進 SPEC-1.1 表格，「個人」列脫離表格；與 REV-33 同類 | 已修正（待人確認）：說明句移到「個人」列之後 |
| REV-55 | 2 | 必修 | `Doc/AUTO-TBD/TestPlan.md:238` | TEST-14～TEST-16 的結果是 DEC-18 改名與拆檔前跑的，5.3 卻以新檔名改寫，讀起來像已在最終結構驗證；REV-49 證明檔名會影響自動載入，需實測。子 session 重跑時因工作目錄在主 checkout，載入的是 master 的舊指引，TEST-14 無法判定、TEST-15 只完成靜態演練 | 已修正（使用者同意重跑）：TEST-14、TEST-15 在 worktree 重跑（`ff2f9b8f`），TEST-16 由使用者在 VS 2026 重跑（`f0cc1462`），5.3 依實測改寫，全部通過 |
| REV-56 | 6 | 建議 | `Doc/AUTO-TBD/FollowUp.md:78` | 第 4.5.4 項只列 `ui-style` 連到 `coding-style-*`；`icon-design` 的連結與路徑字串、UI 版 `project-overview` 連到 `ui-style`、`Doc/AUTO-23254/` 的檔名也會失效且不提示衝突 | 已修正（待人確認）：改為一條 `git grep` 搜尋並列出已知位置，`project-overview` 的那一行改由專案指引索引指向 |
| REV-57 | 6 | 建議 | `Doc/AUTO-TBD/FollowUp.md:60` | 第 4 節「重跑第 2 項的比對」應為第 1、3 項；衝突預估以 `99dcd2e7` 為對象未寫明；第 6 項補 BOM 不在步驟中 | 已修正（待人確認）：三處照改，步驟加入第 4 步補 BOM |
| REV-58 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:314` | DEC-18 後議題文件仍用舊簡稱 `collaboration`：`Spec.md:314`、`TestPlan.md:238`、第 4 回合門檻說明與 REV-46 | 已修正（待人確認）：改為 `team-collaboration`；第 1、2 回合與 Discussion 的歷史描述保留原名 |
| REV-59 | 2 | 建議 | `Doc/AUTO-TBD/Implementation.md:128` | SPEC 對照的 SPEC-13 漏了 STEP-7；實作架構表漏了 `team/claude-code.md` 與 `project/review.md` | 已修正（待人確認） |
| REV-60 | 1 | 建議 | `.github/team/claude-code.md:7` | 檔尾沒有換行，違反 `.editorconfig` 的 `insert_final_newline`；TEST-6 不檢查檔尾換行 | 已修正（待人確認）：補換行，TEST-6 加入檔尾換行檢查 |
| REV-61 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:267` | SPEC-11.2 寫 `team-coding-style-csharp`、`cpp` 是「未修改的檔案」，表格卻寫改名與更新連結；TODO-5.3 同 | 已修正（待人確認）：兩處改為「只改名與更新連結」 |
| REV-62 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:166` | SPEC-7.1 門檻段寫「動到自動載入的檔案」，`team/review.md` 寫「常駐檔案」；DEC-18.4 寫 -9、第 4 回合寫 -10 | 已修正（待人確認）：SPEC-7.1 改為「常駐檔案」；DEC-18.4 註明淨 -9（移出 -10、索引 +1） |
| REV-63 | 1 | 建議 | `Doc/AUTO-TBD/Check.ps1:755` | TEST-15 演練中發現：TEST-11 只比對 commit 標題行結尾的 `(STEP/REV-n)`，`git-workflow.md:26` 寫「訊息結尾標示」；`2723da5a`、`ff2f9b8f` 把標示放在本文最後一段，被判為未標示。本分支其他 commit 都標在標題行 | 已修正（使用者同意依建議）：早期共識（DEC-9.3）為規範；考量手寫程式的成員不會逐一標示，改為建議、位置為標題行結尾，DoD 改為可追溯，TEST-11 未標示只提示；兩個 commit 不改寫（DEC-20） |

## 第 6 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；發現來自使用者的人工 review |
| 日期 | 2026-11-25 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL，TEST-11 只缺 DoD 第 9、10 項與待人確認的處置。常駐 112 行（基準 193，差異 -81）：`team/coding-general.md` 移出常駐、`project-overview` 的技術棧、架構與慣例拆到 `project/`、指引目錄改為分類表。本議題只改文件，不需建置。
2. 本回合的位置使用改名後的檔名；第 1～5 回合保留當時的檔名（DEC-21.8）。
3. Spec、TestPlan、Implementation 依 DEC-22 重寫（REV-68）。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-64 | 5 | 建議 | `.github/copilot-instructions.md:14` | 使用者人工 review：`instructions/` 加前綴與放 `team/`、`project/` 的差別不易理解；兩個工具實際都不依副檔名載入，常駐與依時機讀取的門檻沒有定義 | 已修正（回溯至 DEC-21；使用者同意依建議）：訂載入門檻，目錄只表示載入方式，檔名以分類開頭，review 讀入全部規範 |
| REV-65 | 6 | 建議 | `.github/copilot-instructions.md:16` | 指引目錄逐檔列出且常駐，與兩份概觀的索引重複，新增指引時要改兩處 | 已修正（DEC-21.6；使用者同意依建議）：改為分類表，TEST-4 檢查索引涵蓋每份文件 |
| REV-66 | 6 | 建議 | `.claude/skills/review/SKILL.md:6` | 沒有 skill 的撰寫原則；skill 本文的說明句與必讀清單重複（REV-25） | 已修正（DEC-21.7；使用者同意依建議）：原則寫在維護方式第 3 項，skill 只留清單 |
| REV-67 | 6 | 建議 | `.github/team/process-docs.md:44` | 第 3 節第 3 項「刪除的項目直接移除，編號跳號」與「有序清單的條列保留一行」前後矛盾 | 已修正（使用者同意依建議）：改為「刪除的表格列與標題直接移除」 |
| REV-68 | 6 | 建議 | `Doc/AUTO-TBD/Spec.md:6` | 使用者人工 review：Spec 重述指引內容、TEST-8 只為核對重複而存在，分類明確後研發文件可更精簡 | 已修正（回溯至 DEC-22；使用者同意依建議）：`process-docs.md` 加入規範類議題的 Spec 寫法，Spec 重寫為位置與決策重點，刪除 TEST-8，新增 STEP-14 |

## 第 7 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；發現來自使用者的人工 review |
| 日期 | 2026-11-25 |

1. 門檻：`Check.ps1` 9 PASS、1 FAIL，TEST-11 只缺 DoD 第 9、10 項與待人確認的處置。本議題只改文件，不需建置。
2. 本回合處置全部經使用者同意，決策見 DEC-23。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-69 | 6 | 建議 | `.github/team/process-review.md:11` | 門檻段的常駐行數比對在載入門檻訂定後已無必要 | 已修正（回溯至 DEC-23.3；使用者同意依建議）：刪除行數比對，文件段改查新增常駐內容是否符合載入門檻；TEST-1 改為議題限定 |
| REV-70 | 6 | 建議 | `.github/team/process-git.md:15` | 分支後綴、commit 標示是建議；自測回合、rebase 暫存分支、`git log --grep` 是招式，不是規範 | 已修正（DEC-23.2；使用者同意依建議）：後綴與標示改為建議，其餘刪除 |
| REV-71 | 6 | 提問 | `.github/team/process-issue.md:16` | 「獨立 review」沒有定義誰算獨立 | 已修正（DEC-23.4；使用者決定）：取消獨立 review 要求，DoD 與 `review` skill 同步 |
| REV-72 | 2 | 建議 | `.github/team/process-issue.md:30` | 分支應在開始議題時開，而非分級確認後 | 已修正（回溯至 DEC-23.5；使用者同意依建議）：移到第 1 節開頭 |
| REV-73 | 6 | 建議 | `.github/team/process-issue.md:32` | 待討論格式與團隊概觀協作原則第 3 項重複 | 已修正（DEC-23.6；使用者同意依建議）：只定義在協作原則，流程文件改連結 |
| REV-74 | 2 | 建議 | `.github/team/process-docs.md:25` | S、M 級的後續處理也可能很多，不應因範圍小就寫在 README | 已修正（回溯至 DEC-23.7；使用者同意依建議）：各級有項目時都寫 `FollowUp.md`，DoD 同步 |
| REV-75 | 6 | 建議 | `.github/team/tool-claude-code.md:3` | 「見專案指引」未指明檔案，同類共 7 處 | 已修正（DEC-23.8；使用者同意依建議）：改為連到專案概觀或 `project/` 的檔案 |
| REV-76 | 6 | 建議 | `.github/team/tool-claude-code.md:5` | 「只在確認點切換 session」不是規定；「主 session 可開子 session」是招式 | 已修正（DEC-23.2；使用者同意依建議）：改為 AI 提出切換建議，刪除子 session 一項 |
| REV-77 | 5 | 建議 | `.github/project/tool-claude-code.md:5` | Worktree 位置、`.worktreeinclude`、`settings.local.json`、`CLAUDE.local.md` 是 Claude Code 通用行為 | 已修正（DEC-23.9；使用者同意依建議）：移到 `team/tool-claude-code.md` 第 2 節 |
| REV-78 | 1 | 建議 | `.github/project/process-review.md:5` | Review 門檻應建置 Debug 與 Release；C++ 只在修改時建置 | 已修正（DEC-23.10；使用者同意依建議） |
| REV-79 | 6 | 建議 | `~/.claude/CLAUDE.md:4` | 回覆方式已由團隊概觀涵蓋，使用者會帶團隊規範到其他 repo | 已修正（DEC-23.6；使用者同意依建議）：刪除該項 |
| REV-80 | 4 | 建議 | `.github/team/process-issue.md:14` | 議題流程的施工階段沒有提醒讀取程式規範 | 已修正（DEC-23.11；使用者同意依建議）：補「施工前依流程索引讀取程式規範」 |

## 第 8 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d`；受審 HEAD `5e05f844` |
| 工具與模型 | Visual Studio 2026 GitHub Copilot（本機 Visual Studio 18.12；模型未記錄） |
| 日期 | 2026-11-26 |

1. 門檻：Debug 與 Release 建置成功，各 6 個警告、0 個錯誤；未修改 C++，不需單獨建置。分支的 `git diff --check` 通過。
2. `Check.ps1 -Base 24c7276d -SkipPersonal`：7 PASS、3 FAIL。TEST-4、TEST-5 因工作樹有未追蹤的 `.github/instructions/fromCLAUDE.instructions.md`；它不在受審 commit，未變動該檔。TEST-11 有兩項 DoD 未勾選，並有前回合處置仍待人確認。TEST-12 依參數略過實際檢查。
3. 本次 VS Copilot session 可載入兩份概觀及 `review` skill，並依路徑讀取規範；沒有重跑最舊支援版本、無 skill 的路徑或 VS 的 release-note 入口。
4. 處置（2026-11-26）：全部經使用者同意，決策見 DEC-24；另依使用者意見把 session 交接改為通用規則（DEC-24.4）。未追蹤的 `fromCLAUDE.instructions.md` 是個人設定，已移到 `%USERPROFILE%\copilot-instructions.md`。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-81 | 2 | 必修 | `Doc/AUTO-TBD/TestPlan.md:140` | REQ-2 涵蓋 VS 2026 18 版以上，但 [Microsoft 的 Agent Skills 文件](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-agent-skills?view=visualstudio) 將 Visual Studio 的 skill 探索門檻列為 18.5。TEST-16 沒記實測的次版本，只驗有 skill 的情境，四個案例也沒有 `release-note`。這不代表 18.0–18.4 讀不到 Markdown，而是未驗證這些版本不靠 skill 仍能讀到規範。建議在 Copilot 入口直接連到兩份概觀、說明顯式開檔的替代路徑，並按最低支援次版本驗證無 skill 的讀取及三項動作；若要把 18.5 設為最低版本，須先由人確認 REQ-2 的範圍。 | 已修正（回溯至 DEC-24.2；使用者決定）：REQ-2 最低版本改為 VS 2026 18.5；TEST-16 記錄實測版本、以 Agent 模式執行並加測 release-note，待使用者重跑 |
| REV-82 | 2 | 必修 | `Doc/AUTO-TBD/TestPlan.md:187` | DEC-23.5 和現行 `process-issue.md` 第 1 節要求開始時先開分支，但 TEST-15 將提出 S 級建議後停下、未開分支的舊演練列為通過；TEST-16 的「開議題」也只記觸發和讀檔。現有結果無法證明兩個入口都遵守新順序。應以現行 HEAD 在一次性情境重驗：先開分支，再盤點並於分級確認點停下。實測時可用已有正式編號的情境。 | 已修正（使用者同意依建議）：TEST-15、TEST-16 的「開議題」通過條件改為先開分支、盤點後在分級確認點停下，待重跑 |
| REV-83 | 4 | 必修 | `.github/project/tool-claude-code.md:3` | 文件將讀取條件限定為 Claude Code，卻包含其他工具建立 MyCAM worktree 時也要做的檔案準備。專案概觀只對 Claude 使用者導向此檔，通用 Git 工作流又沒給其他工具獨立的準備路徑，可能漏掉 `FCLTool.dll`、`PostTool.dll` 或 `Custom/`。將專案準備放入 `project/env-build.md` 的 Worktree 章節，由專案索引對所有工具導向；Claude 文件只說明自動複製方式並單向引用。 | 已修正（回溯至 DEC-24.3；使用者同意依建議）：新增 `project/env-worktree.md`（不限工具），刪除 `project/tool-claude-code.md`，專案索引同步 |
| REV-84 | 6 | 建議 | `.github/team/process-git.md:53` | 團隊 Git 規範導向 Claude 操作文件，Claude 文件又要求回讀 Git §5；團隊與專案的 `tool-claude-code.md` 亦互相指定必讀。`process-issue.md` 和 `process-review.md` 也互指對方作為施工與回溯入口。區分「切換到下一個流程」與「解釋本條規則所需的前置閱讀」；索引負責導覽，專案或工具補充只單向指向團隊規則，不讓通用文件反過來依賴工具。C++ 的「同 C#」也應改指共同權威來源，而非重列規則。 | 已修正（DEC-24.5；使用者同意依建議）：`process-git.md` 改指 `env-worktree.md`，工具文件只單向引用通用流程；`process-issue` 與 `process-review` 的互相引用是流程接續，不修正；C++ 的「同 C#」列後續（TODO-5） |
| REV-85 | 6 | 建議 | `.github/team/coding-general.md:3` | 編碼、換行、tab 等已由 `.editorconfig` 定義，入口和 Coding Style 仍重述；例：通用文件將 120 字元寫成所有原始碼與設定檔的限制，但 `.editorconfig` 只對 C#、C/C++ 設這個值。C# 文件另列「方法保持短小」「變數在最小作用範圍」等一般建議，沒有團隊特有的判定條件。保留工具驗證不到的團隊慣例，格式只連到 `.editorconfig`；若沿用 TODO-5，明確列為尚未符合本次精簡與單一來源的項目。 | 列後續（TODO-5；使用者同意依建議）：TODO-5 本為 Coding Style 與 `.editorconfig` 的重複，完成條件加入 120 字元適用範圍與 C# 一般性建議 |
| REV-86 | 6 | 建議 | `.github/team/process-issue.md:51` | FollowUp 的必填欄位在 DoD 與 `process-docs.md` §4 各寫一次，後者另外要求「建議做法」與「內容自足」。S 級文件範本又寫「所有內容都在 README」，與 §2 的 FollowUp 例外不符。DoD 只檢查是否符合 `process-docs.md` §2、§4，範本明列 FollowUp 例外，讓寫法只維護一處。 | 已修正（使用者同意依建議）：DoD 改為依 `process-docs.md` 第 4 節寫入，S 級範本註明 `FollowUp.md` 例外 |
| REV-87 | 6 | 建議 | `.claude/skills/issue-start/SKILL.md:3` | TEST-15 重跑發現：description 依序寫「盤點現況、建議分級、開分支」，與 DEC-23.5 先開分支不一致，AI 可能以為分支在分級後才開 | 已修正（待人確認）：順序改為「開分支、盤點現況、建議分級」 |
| REV-88 | 2 | 提問 | `.github/team/process-git.md:12` | TEST-15 重跑發現：第 2 節沒有寫新議題分支以哪個分支為基準（目標版本分支或 `master`）；本機沒有目標版本分支時 AI 只能推測 | 已修正（DEC-24.6；使用者決定）：補「新議題分支以遠端的 `master` 為基準」，與現行做法一致 |

## 第 9 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d`；受審 HEAD `89f5d06f` |
| 工具與模型 | GitHub Copilot CLI 1.0.116、gpt-6.2-sol |
| 日期 | 2026-11-26 |

1. 門檻：新增報告前執行 `Check.ps1 -Base 24c7276d -SkipPersonal`，結果為 8 PASS、2 FAIL（結束碼 2）。TEST-5 唯一的失敗原因是根目錄未追蹤的交接檔 `REVIEW-REQUEST.md`，不在受審 commit；TEST-11 因 DoD 第 9、10 項未勾選及前回合處置待人確認而未通過。TEST-12 列為 PASS，但依參數略過實際檢查。受審分支的 `git diff --check` 通過；本議題只改文件與設定，未建置。
2. 本回合重點為 `5e05f844..89f5d06f`。已確認 VS 18.5 下限、Agent 模式和 release-note 案例已寫入，TEST-16 仍待使用者重跑；不限工具的 worktree 環境準備與 session 交接、編碼規範後續項目、FollowUp 例外及開議題 skill 的順序已同步。新分支基準已寫為遠端 `master`，但尚未定義如何在多個遠端之間選擇。
3. 本回合由 Claude Code 在背景以 Copilot CLI 非互動執行，交接檔為根目錄的 `REVIEW-REQUEST.md`；處置（2026-11-26）由 Claude Code 依使用者決定填寫。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-89 | 2 | 建議 | `Doc/AUTO-TBD/TestPlan.md:130` | REV-82 修訂後的 TEST-15 方法明令不建立分支，通過條件卻要求先開分支再盤點；重跑僅遇到 fetch 逾時，仍把未開分支後繼續盤點判為通過。應在隔離的測試環境以現行規範驗證實際基準及開分支順序，並將 fetch 失敗的情境獨立記錄。 | 已修正（使用者同意依建議）：方法第 1 項的「說明名稱與基準後停下、不實際建立」即通過條件的「第一個動作是開分支」，兩者不矛盾，嚴重度由「必修」改為「建議」；通過條件補寫這句以免誤讀 |
| REV-90 | 2 | 建議 | `Doc/AUTO-TBD/TestPlan.md:188` | TEST-15 未依方法另開 session，且自然語句「開議題」只核對 description、「切換 session」只核對索引，不能證明在未預讀規範時實際觸發或讀取。應用全新 session 實測這兩種入口，或把現有結果標為靜態抽查。 | 已修正（使用者同意依建議）：第 3 項以全新的無頭 session 重跑並通過，結果記入 TestPlan 5.2；第 4 項（修改 `.cs`）沒有要求另開 session，不重跑 |
| REV-91 | 2 | 提問 | `.github/team/process-git.md:16` | DEC-24.6 已定案使用遠端 `master`，但本機的 `origin/master`（`24c7276d`）和 `github/master`（`7829c338`）不同，照規則仍可能從不同提交起枝。考量 DEC-2.7 不把雙遠端配置寫入團隊規範，是否在團隊流程只指定以專案確認的來源遠端為準，並以本議題使用的 `origin/master` 作為 MyCAM 的選擇範例？ | 不修正（使用者決定）：以哪個 remote 為基準屬於個人的 remote 配置（DEC-2.7），團隊規範不變；`CLAUDE.local.md` 改為開分支前先詢問以哪個 remote 的 `master` 為基準 |
| REV-92 | 6 | 建議 | `Doc/AUTO-TBD/README.md:35` | 狀態列寫 TEST-15、TEST-16 均待重跑，TestPlan 卻已記錄 TEST-15 重跑、僅 TEST-16 尚未重跑；DoD 又把最終結果與門檻都勾成完成。應依實際證據更新狀態列及 DoD，直到最終測試和門檻通過才勾選。 | 已修正（使用者同意依建議）：位置實為 `README.md:8`；狀態列更新，DoD 第 3 項在 TEST-16 重跑前取消勾選 |
| REV-93 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:670` | TEST-9 的通過條件要求 `env-worktree.md` 說明卸載 `FCLTool`，腳本只檢查 `FCLTool.dll`、`CopyRef.bat` 與 `.worktreeinclude`，刪掉卸載步驟仍會通過。應檢查該步驟存在，或在 TestPlan 將它列為人工抽查。 | 已修正（使用者同意依建議）：TEST-9 檢查清單加入「卸載」 |

## 第 10 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d`；受審 HEAD `04b8aaf8` |
| 工具與模型 | GitHub Copilot CLI 1.0.116、gpt-6.2-sol |
| 日期 | 2026-11-26 |

1. 門檻：新增本回合前執行 `powershell -ExecutionPolicy Bypass -File Doc/AUTO-TBD/Check.ps1 -Base 24c7276d -SkipPersonal`，8 PASS、2 FAIL（結束碼 2）。TEST-5 僅因未追蹤的交接檔 `REVIEW-REQUEST.md` 失敗；TEST-11 因 DoD 第 3、9、10 項未勾選及既有 REV 處置待人確認而失敗。TEST-9 通過；TEST-12 列 PASS，但依參數略過。增量 `git diff --check` 無異常；依本次範圍未建置。
2. 僅審查 `89f5d06f..04b8aaf8` 及其直接影響。REV-89 已補上不實際建分支的條件；REV-90 已補記兩個全新 session 的重跑結果；REV-91 依 DEC-2.7 維持不修正團隊規範，未檢查個人設定；REV-92 已更新重跑狀態並取消勾選 DoD 第 3 項；REV-93 已加入「卸載」檢查。仍有下列不一致。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-94 | 2 | 建議 | `Doc/AUTO-TBD/TestPlan.md:135` | REV-89 的處置保留不實際建立分支的演練方式，但修訂後同一句同時要求「停下」與「再盤點」，無法判斷未獲確認就繼續是否算通過。可否在不改變 DEC-23.5 開分支順序的前提下，明訂在分支確認點停下，以及用何種模擬確認繼續驗證盤點及分級？ | 已修正（使用者同意依建議）：嚴重度由「提問」改為「建議」；通過條件改為說明分支名稱與基準、不實際建立，演練中視同人已確認，接著盤點與分級 |
| REV-95 | 6 | 建議 | `Doc/AUTO-TBD/TestPlan.md:188` | REV-90 的第 3 項已用兩個全新 session 重跑，但結果總述仍稱第 3、4 項在同一 session 執行、以索引對應為依據，容易被誤讀為目前的通過證據。建議明示第 3 項以最新實測為準，並分開標示沿用的第 4 項結果。 | 已修正（使用者同意依建議）：總述註明方法第 3 項以全新 session 的重跑結果為準 |
| REV-96 | 6 | 建議 | `Doc/AUTO-TBD/README.md:35` | REV-92 取消勾選最終測試結果後，門檻項仍勾選並宣稱 `Check.ps1` 通過，但本次 TEST-11 仍失敗。建議門檻通過前保持未勾選，並區分已通過的檢查與仍待補齊的條件。 | 已修正（使用者同意依建議）：未照建議取消勾選，因為 TEST-11 會檢查本項是否勾選，兩者會互相卡住；改為寫明門檻是 TEST-11 以外全部通過 |
| REV-97 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:670` | REV-93 新增的「卸載」可偵測整項刪除，但改成只說「卸載狀態不會保留」、不指示卸載 `FCLTool` 時，現有關鍵字仍全數命中。建議檢查卸載動作與 `FCLTool` 的關聯，或把該操作要求列為人工抽查。 | 不修正（使用者同意依建議）：`Check.ps1` 的關鍵字只用來防止整段被刪除，語意是否正確由 review 文件段負責；加嚴關鍵字只會讓措辭修改誤判失敗 |
