# 測試規劃

每條 TEST 標示對應的 SPEC（見 [Spec.md](Spec.md)）。本議題只改文件與設定，沒有建置與 Production Code 的測試。

## 1. 分類

| 欄位 | 值 | 定義 |
| --- | --- | --- |
| 週期 | 議題限定 | 只在本議題驗證，review 每輪修正後重跑 |
| 週期 | 長期維護 | 之後修改規範時仍要執行，屬於 review 對應段的檢查 |
| 時機 | 靜態 | 檢查檔案內容與 git 記錄 |
| 時機 | 動態 | 在 Claude Code 或 VS 2026 Copilot 實際操作 |
| 執行者 | 自動 | 由 `Doc/AUTO-TBD/Check.ps1` 執行並輸出每條 TEST 的結果（DEC-7.1） |
| 執行者 | 人工 | 需要人判斷，或需要 AI 無法啟動的工具環境 |

## 2. 總表

| TEST | 內容 | SPEC | 週期 | 時機 | 執行者 |
| --- | --- | --- | --- | --- | --- |
| TEST-1 | 常駐行數 | SPEC-1.7、SPEC-12.3、相容性 3.2.2 | 長期維護 | 靜態 | 自動 |
| TEST-2 | 團隊通用層不含專案內容 | SPEC-1.2、SPEC-13.3、SPEC-13.4 | 議題限定 | 靜態 | 自動 |
| TEST-3 | 規則只寫一處 | SPEC-1.6.1、SPEC-2.2、SPEC-13.5、SPEC-16.2 | 議題限定 | 靜態 | 自動 |
| TEST-4 | 載入設定與檔案位置 | SPEC-1.1、SPEC-1.4、SPEC-11、SPEC-12.1、SPEC-15 | 議題限定 | 靜態 | 自動 |
| TEST-5 | 異動範圍符合清單 | SPEC-11、相容性 3.2.3 | 議題限定 | 靜態 | 自動 |
| TEST-6 | 格式與數字條列 | SPEC-1.6.3、SPEC-1.6.4、SPEC-6.4、SPEC-11.1、SPEC-11.2 | 議題限定 | 靜態 | 自動 |
| TEST-7 | 連結與引用有效 | SPEC-1.6.1、SPEC-12.2.3、SPEC-13.5、SPEC-16.2 | 議題限定 | 靜態 | 自動 |
| TEST-8 | 規格內容落地 | SPEC-3～SPEC-9、SPEC-13 | 議題限定 | 靜態 | 人工 |
| TEST-9 | 專案指引與 Coding Style 修訂 | SPEC-10、SPEC-14 | 議題限定 | 靜態 | 自動 |
| TEST-10 | 文件精簡審查 | SPEC-1.6.1、SPEC-1.6.2 | 長期維護 | 靜態 | 人工 |
| TEST-11 | 本議題文件與 commit | SPEC-4.4、SPEC-7.3、SPEC-7.4、SPEC-8.5、SPEC-19 | 議題限定 | 靜態 | 自動 |
| TEST-12 | 個人設定收斂 | SPEC-17 | 議題限定 | 靜態 | 自動 |
| TEST-13 | 後續議題完整 | SPEC-18 | 議題限定 | 靜態 | 人工 |
| TEST-14 | Claude Code 載入範圍 | SPEC-1.4、SPEC-12、SPEC-15.2 | 議題限定 | 動態 | 人工 |
| TEST-15 | Claude Code 入口與流程演練 | SPEC-2、SPEC-3、SPEC-4、SPEC-5、SPEC-6、SPEC-9、SPEC-13.2、SPEC-16 | 議題限定 | 動態 | 人工 |
| TEST-16 | VS 2026 Copilot 讀取與觸發 | SPEC-1.4、SPEC-2.3、SPEC-15.1、SPEC-16.4 | 議題限定 | 動態 | 人工 |

現有測試的異動：無（repo 沒有自動測試）。

## 3. 測試項目

### 3.1 靜態檢查

#### TEST-1 常駐行數

1. 方法：計算入口文件（`.github/copilot-instructions.md`、`CLAUDE.md`）與 `.github/instructions/` 中 `applyTo: "**"` 的檔案行數，與比對基準比較。
2. 通過條件：輸出各檔與總行數，以及與基準 193 行（`24c7276d`：入口 32 行、`project-overview` 130 行、`team-coding-style-general` 31 行）的差異；總行數增加時，`Review.md` 寫明增加量與理由（SPEC-1.7.1）。
3. 長期維護：即 review 門檻段的行數檢查（SPEC-7.1）。

#### TEST-2 團隊通用層不含專案內容

1. 方法：在 `team-collaboration.instructions.md`、`team-coding-style-general`、`team-coding-style-csharp`、`team-coding-style-cpp`（REV-52）與 `.github/team/*.md` 搜尋專案專屬字詞：`MyCAM`、`Build.bat`、`CopyRef`、`FCLTool`、`PostTool`、`OCC`、`LanguageManager`、`Designer`、`reference/`、`.worktreeinclude`、`CustPost`、`zh-TW`、`AUTO-`（DEC-16）、`.NET Framework`（REV-52）。
2. 通過條件：零筆；需要專案資訊處寫「見專案指引」。
3. 本檢查列入 `FollowUp.md` 自動化門檻的候選項目（DEC-4.5）。

#### TEST-3 規則只寫一處

1. 方法：以各規則的特徵字串搜尋 `.github/`、`CLAUDE.md`、`.claude/skills/`，例如分級表（`1–5 合併`）、嚴重度（`必修 / 建議 / 提問`）、分支後綴（`_Alt2`）、release note 前綴（`前綴與內容間空一格`）、DoD 標題。
2. 通過條件：
   1. 每個特徵字串只出現在一個規範檔，其他檔案以連結引用。
   2. `SKILL.md` 本文只有指向 `team/` 的說明與必讀文件清單，沒有規則內容。

#### TEST-4 載入設定與檔案位置

1. 方法：檢查 frontmatter、`@` 引用、`.gitignore` 與 `git check-ignore`。
2. 通過條件：
   1. `team-collaboration.instructions.md` 的 frontmatter 為 `applyTo: "**"`。
   2. `.github/team/` 五份文件與 `.github/project/` 三份文件沒有 `applyTo` frontmatter；`instructions/` 的檔名都有 `team-` 或 `project-` 前綴；`.github/` 下沒有名為 `claude.md` 的檔案（DEC-18）。
   3. `CLAUDE.md` 的 `@` 引用恰為 `copilot-instructions.md`、`team-collaboration`、`project-overview`、`team-coding-style-general` 四份。
   4. `copilot-instructions.md` 的指引目錄列出 `team-collaboration`、`team/` 五份文件、`project/` 三份文件，並標示層與載入方式。
   5. `team-collaboration.instructions.md` 的流程索引有「新增或修改程式碼」一列。
   6. `git check-ignore SESSION.md` 回報由 `.gitignore` 的 `/SESSION.md` 排除（SPEC-11、SPEC-13.6）。

#### TEST-5 異動範圍符合清單

1. 方法：`git diff --name-status 24c7276d HEAD`。
2. 通過條件：異動檔案與 SPEC-11 的清單一致；沒有 Production Code、`reference/`、建置設定的異動。

#### TEST-6 格式與數字條列

1. 方法：檢查本議題新增與修改的規範檔及 `Doc/AUTO-TBD/`。
2. 通過條件：
   1. 編碼與換行符合 `.editorconfig`。
   2. 程式碼區塊以外沒有無序號條列（`- `、`* ` 開頭），有序清單同一層不跳號（SPEC-6.4，DEC-17）。
   3. 檔名為英文，內容為繁體中文。

#### TEST-7 連結與引用有效

1. 方法：擷取 `.github/`、`CLAUDE.md`、`.claude/skills/`、`Doc/AUTO-TBD/` 中的相對連結、`@` 路徑與反引號內的 `.github/...` 路徑，逐一確認檔案存在。
2. 通過條件：全部存在，大小寫與實際檔名一致。

#### TEST-8 規格內容落地

1. 方法：AI 依 SPEC-13 的對應，逐條列出 SPEC-3～SPEC-9 每個子項目在 `team/` 文件中的位置（`檔案:行號`），人抽查。
2. 通過條件：
   1. 每個子項目都有位置，內容與 SPEC 一致。
   2. `issue-docs.md` 含各文件內容規範與精簡範本（S 級完整範本；M、L 骨架）。
   3. `review.md` 的專案固定檢查項目只寫「見專案指引」。

#### TEST-9 專案指引與 Coding Style 修訂

1. 方法：搜尋 `project-overview.instructions.md`、`.github/project/` 三份文件、`.github/team/claude-code.md` 與 `team-coding-style-general.instructions.md`。
2. 通過條件：
   1. `project-overview` 沒有「Git 流程」與「專案固定檢查項目」章節、沒有建置細節（`FakeDebug`、`vswhere`、`MachineData`）；索引指向 `project-coding-general`、`project/review.md`、`project/build.md`、`project/claude-code.md`；`project/review.md` 有兩項（SPEC-14.4）；「議題編號」含 `AUTO-TBD`（SPEC-14.7）。
   2. `build.md` 有 FCLTool、命令列建置、執行期檔案三節，沒有 worktree 相關內容；`team/claude-code.md` 含 `SESSION.md` 的說明（SPEC-13.6），`project/claude-code.md` 含 `.worktreeinclude` 的說明（SPEC-14.6）。
   3. 四語系同步、Designer 可開、不修改 `reference/` 三項規則不出現在 `project-overview`，且寫在 `project-coding-general`（SPEC-14.5、SPEC-14.8）。
   4. `team-coding-style-general` 不再有「維持該方法內既有的命名方式」；有 SPEC-10.1 三項與 SPEC-10.2 註解規則。

#### TEST-10 文件精簡審查

1. 方法：review 文件段，AI 依 SPEC-1.6 列出重複、讀者無法判斷、可交給工具的內容，人決定處置。
2. 通過條件：發現全部有處置，記入 `Review.md`。
3. 長期維護：即 review 文件段（SPEC-7.1）。

#### TEST-11 本議題文件與 commit

1. 方法：檢查 `Doc/AUTO-TBD/` 檔案組合、`README.md`、`Review.md` 與 `git log 24c7276d..HEAD`。
2. 通過條件：
   1. 有 L 級八份文件；`README.md` 含文件索引、L 級 DoD（全部勾選）、Release Note。
   2. `Review.md` 表頭五欄、發現表格六欄；每項有處置，「必修」全部為「已修正」。
   3. 每個 STEP 至少一個標示 `(STEP-n)` 的 commit；未標示的施工 commit 只提示、不判失敗（DEC-20）；純格式調整獨立 commit。

#### TEST-12 個人設定收斂

1. 方法：檢查使用者家目錄與主 checkout 的個人設定檔。
2. 通過條件：
   1. `~/.claude/issue-workflow.md` 不存在；`~/.claude/CLAUDE.md` 沒有「開發議題處理流程」與對它的 `@` 引用，也沒有「條列使用數字標號」。
   2. `CLAUDE.local.md` 沒有議題資料夾位置與 push 前詢問；有 `origin`、`github` 的 remote 用法。
3. 結果記入 `README.md`（SPEC-17.4）。

#### TEST-13 後續議題完整

1. 方法：人閱讀 `FollowUp.md`。
2. 通過條件：
   1. 含 SPEC-18.2 各項（第 5 項依 TEST-16 結果決定是否需要）；每項有目標、完成條件、延後原因，不需讀本議題其他文件就能開工。
   2. 自動化門檻項目列出 TEST-2 的檢查作為候選。

### 3.2 動態驗證

TEST-14～TEST-16 在 TEST-12 完成後執行，確認只靠 repo 的規範就能完整遵循流程（REQ-2.2）。

#### TEST-14 Claude Code 載入範圍

1. 方法：在 worktree 開新的 Claude Code session，以 `/context` 查看載入的記憶檔。
2. 通過條件：載入 `CLAUDE.md`、`copilot-instructions.md`、`team-collaboration.instructions.md`、`project-overview`、`team-coding-style-general`；沒有載入 `team/`、`project/` 與 `team-coding-style-csharp`、`team-coding-style-cpp`。

#### TEST-15 Claude Code 入口與流程演練

1. 方法：
   1. 新 session 中以一個假設的 S 級 BUG 執行 `/issue-start`，產出放在 scratchpad，不提交。
   2. 在本分支執行 `/release-note`；執行 `/review`，確認依六段進行並產出 SPEC-7.3 格式的報告。
   3. 另開新 session，只輸入「開議題」。
   4. 另開新 session，要求修改任一 `.cs` 檔的一個方法（修改不保留）。
2. 通過條件：
   1. 三個入口都出現在 skill 清單，執行後讀取 SPEC-2 表格列出的必讀文件。
   2. `/issue-start` 產出的 `README.md` 符合 S 級範本：議題資訊表、各節、DoD 五項，編號使用 SPEC-6 的前綴；AI 提出分級建議後停下等人確認（SPEC-3.3、SPEC-3.4）。
   3. Release note 每條以 `新增` / `修改` / `刪除` 開頭；merge log 符合 SPEC-9.2 格式，目標版本留空。
   4. 只輸入「開議題」時，經 skill 或流程索引讀取 `issue-workflow.md` 與 `issue-docs.md`。
   5. 修改 `.cs` 前，經流程索引讀取 `team-coding-style-csharp.instructions.md`。
3. 演練產出不提交，檢查結果摘要寫入執行結果。

#### TEST-16 VS 2026 Copilot 讀取與觸發

1. 執行者：使用者；AI 在施工完成後提供逐步清單並記錄結果。
2. 方法：VS 2026（18 版以上）開啟方案，在 Copilot Chat：
   1. 開啟任一 `.cs` 檔提問，查看回應引用的指引。
   2. 輸入「開議題」，查看是否讀取 `.github/team/issue-workflow.md`。
   3. 輸入 `/issue-start`，確認是否辨識 `.claude/skills/`。
3. 通過條件：
   1. 第 1、2 項：引用 `copilot-instructions.md` 與 `team-collaboration.instructions.md`，並依索引讀取 `team/`。
   2. 第 3 項只記錄結果，不判定失敗；讀不到時確認 `FollowUp.md` 有 SPEC-18.2.5 項目。

## 4. SPEC 對照

| SPEC | TEST |
| --- | --- |
| SPEC-1 規範分層、讀取方式與文件通則 | TEST-1、TEST-2、TEST-3、TEST-4、TEST-6、TEST-7、TEST-10、TEST-14、TEST-16 |
| SPEC-2 觸發入口 | TEST-3、TEST-15、TEST-16 |
| SPEC-3 議題分級、階段與確認點 | TEST-8、TEST-15 |
| SPEC-4 完成定義 | TEST-8、TEST-11、TEST-15 |
| SPEC-5 議題文件組合與內容歸屬 | TEST-4、TEST-8、TEST-15 |
| SPEC-6 項目編號與追溯 | TEST-6、TEST-8、TEST-15 |
| SPEC-7 Review | TEST-8、TEST-11、TEST-15 |
| SPEC-8 Git 流程 | TEST-8、TEST-11 |
| SPEC-9 Release note 與 merge log | TEST-8、TEST-15 |
| SPEC-10 Coding Style 修訂 | TEST-9 |
| SPEC-11 檔案異動清單 | TEST-4、TEST-5、TEST-6 |
| SPEC-12 `team-collaboration.instructions.md` | TEST-1、TEST-4、TEST-7、TEST-14 |
| SPEC-13 `team/` 文件 | TEST-2、TEST-3、TEST-7、TEST-8、TEST-15 |
| SPEC-14 `project-overview.instructions.md` | TEST-9 |
| SPEC-15 入口文件 | TEST-4、TEST-14、TEST-16 |
| SPEC-16 Skills | TEST-3、TEST-7、TEST-15、TEST-16 |
| SPEC-17 個人設定收斂 | TEST-12 |
| SPEC-18 後續議題 | TEST-13 |
| SPEC-19 本議題自身的文件 | TEST-11 |

## 5. 執行結果

### 5.1 自動（`Check.ps1`，2026-11-25，比對基準 `24c7276d`）

1. TEST-1～TEST-7、TEST-9、TEST-12：通過。
2. TEST-1 常駐 202 行（基準 193，差異 +9，理由見 Review 第 3、4 回合）：入口 51 行、`team-collaboration` 43 行、`project-overview` 76 行、`team-coding-style-general` 32 行。
3. TEST-11：收官後只剩 DoD 第 9、10 項與 REV 的「待人確認」，待人工全域 review 後通過。

### 5.2 TEST-8 規格內容落地

AI 列出位置，待人抽查。`IW` 為 `issue-workflow.md`、`ID` 為 `issue-docs.md`、`RV` 為 `review.md`、`GW` 為 `git-workflow.md`（皆在 `.github/team/`）。

| SPEC | 位置 |
| --- | --- |
| SPEC-3.1 階段（含 3.6 預料外先停） | `IW:5-19` |
| SPEC-3.2～3.5 分級、確認點、待討論格式 | `IW:21-35` |
| SPEC-3.7、3.9 重新分級、回溯 | `IW:37-43` |
| SPEC-4 各級 DoD | `IW:45-67` |
| SPEC-5.1～5.3、5.9 存放 | `ID:5-10` |
| SPEC-5.4、5.7 檔案組合、例外 | `ID:12-28` |
| SPEC-5.5 議題資訊表 | `ID:69-74`（範本） |
| SPEC-5.6 文件通則 | `ID:3`（連結 `team-collaboration`） |
| SPEC-6 編號與追溯 | `ID:30-44` |
| SPEC-13.2.1 各文件內容規範 | `ID:46-60` |
| SPEC-13.2.2 S 級完整範本；M、L 骨架 | `ID:62-145` |
| SPEC-7.1 六段與分工 | `RV:5-19` |
| SPEC-7.2 不限工具與執行者 | `RV:3` |
| SPEC-7.3 報告格式 | `RV:21-42` |
| SPEC-7.4、7.5 處置規則、回到門檻段 | `RV:44-50` |
| SPEC-13.3 專案固定檢查項目只寫「見專案指引」 | `RV:11` |
| SPEC-8.1、8.2、8.6 版本、合併、push | `GW:5-10` |
| SPEC-8.3、8.4 分支命名、後綴、fetch | `GW:12-20` |
| SPEC-8.5 Commit | `GW:22-28` |
| SPEC-9 Release note 與 merge log | `GW:30-56` |
| SPEC-8.7、SPEC-13.4 Worktree 通用規則 | `GW:58-61` |
| SPEC-13.6 Claude Code 的 session 交接 | `.github/team/claude-code.md:5-7` |

### 5.3 人工

1. TEST-8：AI 逐條比對，位置如 5.2，內容一致（SPEC-5.4 的「STEP 與 commit 對應」措辭已由 REV-39 對齊）。使用者接受 AI 對照結果，免抽查。通過。
2. TEST-13：`FollowUp.md` 含 SPEC-18.2 第 1～4、6 項（TODO-1～TODO-5）；第 5 項以 TEST-16 讀不到 skill 為條件，實測讀得到，TODO-6 結案刪除（REV-7）。每項有目標、完成條件、延後原因、建議做法；TODO-2 列出 TEST-2 為候選。通過。
3. TEST-10：於 Review 第 2 回合文件段執行（獨立 session），發現列為 REV-20～REV-29，全部有處置。通過。
4. TEST-14：DEC-18 改名後於 worktree（HEAD `ff2f9b8f`）開新 session 重跑，讀任何檔案前依系統提示列出已載入的檔案，共 8 份：`CLAUDE.md`、`copilot-instructions.md`、`team-collaboration`、`project-overview`、`team-coding-style-general`、個人層 `~/.claude/CLAUDE.md` 與主 checkout 的 `CLAUDE.local.md`、auto memory 索引（1 項）。沒有載入 `team-coding-style-csharp`、`team-coding-style-cpp`、`team/`、`project/`、任何 `claude-code.md`。通過。
5. TEST-15：與 TEST-14 同一 session 演練（HEAD `ff2f9b8f`），產出放在 scratchpad，未提交，`.cs` 修改已還原。
   1. 三個 skill 都在清單中，必讀文件與 SPEC-2 一致。通過。
   2. `/issue-start`（假設 S 級 BUG）：依序讀 `team/issue-workflow.md`、`issue-docs.md`、`git-workflow.md`；草稿有 S 級範本各節與 DoD 五項，編號使用 REQ、SPEC、TEST、STEP；盤點後在「待討論」提出分級建議並停下，未開分支。通過。
   3. `/review`：讀 `team/review.md`，並經常駐的專案指引索引讀 `project/review.md`；依六段進行，表頭五欄、發現表六欄。通過。
   4. `/release-note`：每條以 `新增` 或 `修改` 開頭，merge log 為 `Merge branch 'AUTO-TBD_f_DevProcess' into ''` 加 `by <名字>`。通過。
   5. 只輸入「開議題」、問「切換 session 要怎麼做」：常駐流程索引分別導向 `team/issue-workflow.md`、`team/claude-code.md`，後者可答出確認點切換與 `SESSION.md` 交接。通過；限制：在同一 session 內判斷，未另開乾淨的 session（改名前曾以新 session 實測「開議題」通過）。
   6. 要求修改 `.cs` 方法：動手前經流程索引讀取 `team-coding-style-csharp.instructions.md`（不常駐），修改符合 C# 規範；修改已還原。通過。
6. TEST-16：使用者在 VS 2026 Copilot Chat（GPT-6.1）於 worktree 方案執行，每項開新對話；以下為 DEC-18 改名前的結果，待使用者在最終結構重跑後更新。
   1. 一般提問：附加 `.cs` 檔提問，回應引用 `team-coding-style-general`、使用 `project-overview` 內容，未讀檔；另以未附檔的提問「哪些決策一定要由人做」，未讀檔即依 `team-collaboration.instructions.md` 答出五項。`team-coding-style-csharp` 未依 `applyTo` 自動載入，由 Copilot 主動讀取。通過。
   2. 輸入「開議題」：自動觸發 `issue-start` skill，讀取 `issue-workflow.md` 與 `issue-docs.md` 後停下詢問議題內容與編號。通過。
   3. 輸入 `/issue-start`：辨識為 skill 並讀取相同兩份文件。VS 2026 Copilot 可讀取 `.claude/skills/`。
7. 演練中發現的規範問題於 STEP-13 review 處理：議題編號是否含 `AUTO-` 前綴的寫法不一致、S 級範本缺「現況盤點」與「待討論」、`review.md` 報告標題層級放入 S 級 `README.md` 時衝突、`issue-start` 必讀清單未含 `git-workflow.md`、`Check.ps1` 不檢查未知參數。

## 6. 待討論

無。確認點 4 的決議見 DEC-4.5、DEC-7.1；確認點 5 回溯修改見 DEC-10；TEST-1 取消上限見 DEC-11。
