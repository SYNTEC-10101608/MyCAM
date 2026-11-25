# 測試規劃

每條 TEST 標示對應的 SPEC（見 [Spec.md](Spec.md)）。本議題只改文件與設定，沒有建置與 Production Code 的測試；規則內容以指引檔為準，以連結有效（TEST-7）與人工抽查代替逐條比對（DEC-22）。

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
| TEST-1 | 常駐行數 | SPEC-1 | 議題限定 | 靜態 | 自動 |
| TEST-2 | 團隊通用層不含專案內容 | SPEC-1 | 議題限定 | 靜態 | 自動 |
| TEST-3 | 規則只寫一處 | SPEC-1、SPEC-2 | 議題限定 | 靜態 | 自動 |
| TEST-4 | 載入設定、檔案分類與索引涵蓋 | SPEC-1、SPEC-11 | 議題限定 | 靜態 | 自動 |
| TEST-5 | 異動範圍符合清單 | SPEC-11 | 議題限定 | 靜態 | 自動 |
| TEST-6 | 格式與數字條列 | SPEC-1、SPEC-6、SPEC-11 | 議題限定 | 靜態 | 自動 |
| TEST-7 | 連結與引用有效 | SPEC-1～SPEC-10 | 議題限定 | 靜態 | 自動 |
| TEST-9 | 專案指引與 Coding Style 修訂 | SPEC-10、SPEC-11 | 議題限定 | 靜態 | 自動 |
| TEST-10 | 文件精簡審查 | SPEC-1 | 長期維護 | 靜態 | 人工 |
| TEST-11 | 本議題文件與 commit | SPEC-4、SPEC-7、SPEC-8、SPEC-19 | 議題限定 | 靜態 | 自動 |
| TEST-12 | 個人設定收斂 | SPEC-17 | 議題限定 | 靜態 | 自動 |
| TEST-13 | 後續議題完整 | SPEC-18 | 議題限定 | 靜態 | 人工 |
| TEST-14 | Claude Code 載入範圍 | SPEC-1 | 議題限定 | 動態 | 人工 |
| TEST-15 | Claude Code 入口與流程演練 | SPEC-2～SPEC-9 | 議題限定 | 動態 | 人工 |
| TEST-16 | VS 2026 Copilot 讀取與觸發 | SPEC-1、SPEC-2 | 議題限定 | 動態 | 人工 |

現有測試的異動：無（repo 沒有自動測試）。

## 3. 測試項目

### 3.1 靜態檢查

#### TEST-1 常駐行數

1. 方法：計算入口文件與 `.github/instructions/` 中 `applyTo: "**"` 的檔案行數，與比對基準比較。
2. 通過條件：輸出各檔與總行數，以及與基準（`24c7276d`，193 行）的差異；總行數增加時，`Review.md` 寫明增加量與理由。
3. 只驗證本議題的常駐縮減；之後的修改改由 review 文件段檢查載入門檻（TEST-10，DEC-23）。

#### TEST-2 團隊通用層不含專案內容

1. 方法：在 `team-overview.instructions.md` 與 `.github/team/*.md` 搜尋專案專屬字詞（`MyCAM`、`Build.bat`、`FCLTool`、`LanguageManager`、`AUTO-`、`.NET Framework` 等，清單在 `Check.ps1`）。
2. 通過條件：零筆；需要專案資訊處連到 `project/` 或專案概觀的對應檔案（DEC-23）。
3. 列入 `FollowUp.md` 自動化門檻的候選項目（DEC-4.5）。

#### TEST-3 規則只寫一處

1. 方法：以各規則的特徵字串（分級表、嚴重度、分支後綴、release note 前綴、DoD）搜尋 `.github/`、`CLAUDE.md`、`.claude/skills/`。
2. 通過條件：每個特徵字串只出現在一個規範檔；`SKILL.md` 本文只有必讀文件清單，沒有規則內容。

#### TEST-4 載入設定、檔案分類與索引涵蓋

1. 方法：檢查 frontmatter、檔名、`@` 引用、兩份概觀的索引、`.gitignore`。
2. 通過條件：
   1. `instructions/` 只有 `team-overview`、`project-overview`，皆為 `applyTo: "**"`。
   2. `team/`、`project/` 的檔案沒有 `applyTo`，檔名以 `process-`、`coding-`、`env-`、`tool-` 開頭，且都列在該層概觀的索引（各語言 Coding Style 以 `coding-<語言>` 一列涵蓋）；`.github/` 下沒有 `claude.md`。
   3. `CLAUDE.md` 的 `@` 引用恰為 `copilot-instructions.md` 與兩份概觀。
   4. `copilot-instructions.md` 有分類表與 skill 原則。
   5. 流程索引有「新增或修改程式碼」一列；`SESSION.md` 由 `.gitignore` 的 `/SESSION.md` 排除。

#### TEST-5 異動範圍符合清單

1. 方法：`git diff --name-only 24c7276d` 加上未追蹤檔案，與 `Check.ps1` 的清單雙向比對。
2. 通過條件：異動檔案與 SPEC-11 一致；沒有 Production Code、`reference/`、建置設定的異動。

#### TEST-6 格式與數字條列

1. 方法：檢查本議題新增與修改的規範檔及 `Doc/AUTO-TBD/`。
2. 通過條件：編碼、換行、檔尾換行符合 `.editorconfig`；程式碼區塊外沒有無序號條列，有序清單不跳號（DEC-17）；檔名英文、內容繁體中文。

#### TEST-7 連結與引用有效

1. 方法：擷取 `.github/`、`CLAUDE.md`、`.claude/skills/`、`Doc/AUTO-TBD/` 的相對連結、`@` 路徑與反引號內的 `.github/...` 路徑，逐一確認存在；`Review.md`、`Discussion.md` 是歷史記錄，只檢查連結（DEC-21）。
2. 通過條件：全部存在，大小寫一致。Spec 每條 SPEC 的位置連結因此都指向實際的規則檔。

#### TEST-9 專案指引與 Coding Style 修訂

1. 方法：搜尋 `project-overview`、`.github/project/` 與 `team/coding-general.md`。
2. 通過條件：
   1. `project-overview` 沒有 Git 流程、專案固定檢查項目、建置細節、技術棧與架構、慣例；有含 `AUTO-TBD` 的「議題編號」。
   2. `project/process-review.md` 兩項；`env-build.md` 有 FCLTool、命令列建置、執行期檔案三節且沒有 worktree；`env-worktree.md` 有要複製的檔案、`CopyRef.bat`、卸載 `FCLTool` 三項；`process-issue.md` 有 `SESSION.md` 交接，`tool-claude-code.md` 沒有（DEC-24）。
   3. 四語系同步、Designer、`reference/` 三項規則只寫在 `project/coding-general.md`。
   4. `team/coding-general.md` 沒有「維持該方法內既有的命名方式」，有 SPEC-10 的兩項規則。

#### TEST-10 文件精簡審查

1. 方法：review 文件段，AI 依文件通則列出重複、讀者無法判斷、可交給工具的內容，以及不符合載入門檻的新增常駐內容，人決定處置。
2. 通過條件：發現全部有處置，記入 `Review.md`。
3. 長期維護：即 review 文件段。

#### TEST-11 本議題文件與 commit

1. 方法：檢查 `Doc/AUTO-TBD/`、`README.md`、`Review.md` 與 `git log 24c7276d..HEAD`。
2. 通過條件：
   1. 有 L 級八份文件；`README.md` 含文件索引、L 級 DoD（全部勾選）、Release Note。
   2. `Review.md` 表頭五欄、發現表格六欄；處置以已修正、不修正、列後續開頭，沒有待人確認。
   3. 每個 STEP 至少一個標示 `(STEP-n)` 的 commit；未標示的 commit 只提示（DEC-20）。

#### TEST-12 個人設定收斂

1. 方法：檢查家目錄與主 checkout 的個人設定檔；其他成員以 `-SkipPersonal` 略過。
2. 通過條件：符合 SPEC-17。

#### TEST-13 後續議題完整

1. 方法：人閱讀 `FollowUp.md`。
2. 通過條件：含 SPEC-18 各項，每項有目標、完成條件、延後原因，不需讀本議題其他文件就能開工；自動化門檻列出 TEST-2 為候選。

### 3.2 動態驗證

確認只靠 repo 的規範就能完整遵循流程（REQ-2.2）；檔名與載入方式會影響結果（REV-49），結構調整後須重跑。

#### TEST-14 Claude Code 載入範圍

1. 方法：在 worktree 開新的 Claude Code session，讀任何檔案前列出已載入的指引。
2. 通過條件：只載入 `CLAUDE.md`、`copilot-instructions.md` 與兩份概觀（另有個人層與 auto memory）；沒有載入 `team/`、`project/` 的任何檔案。

#### TEST-15 Claude Code 入口與流程演練

1. 方法：新 session 中：
   1. 以假設的 S 級 BUG 執行 `/issue-start`，產出放在 scratchpad，不提交；開分支一步在執行指令前停下，說明分支名稱與基準即可，不實際建立。
   2. 在本分支執行 `/release-note` 與 `/review`。
   3. 另開新 session，只輸入「開議題」；問「切換 session 要怎麼做」。
   4. 要求修改任一 `.cs` 檔的一個方法，修改不保留。
2. 通過條件：
   1. 三個 skill 讀取各自的必讀清單；`/review` 讀入 `team/`、`project/` 全部文件。
   2. `/issue-start` 第一個動作是開分支（DEC-23.5），再盤點並產出符合 S 級範本的 `README.md`，提出分級建議後停下。
   3. Release note 與 merge log 符合格式，目標版本留空。
   4. 「開議題」與「切換 session」經流程索引讀到對應文件；切換 session 讀到 `process-issue.md` 的交接（DEC-24）。
   5. 修改 `.cs` 前經索引讀取 `team/coding-general.md`、`team/coding-csharp.md`、`project/coding-architecture.md`、`project/coding-general.md`。

#### TEST-16 VS 2026 Copilot 讀取與觸發

1. 執行者：使用者；AI 提供逐步清單並記錄結果。
2. 方法：VS 2026（18.5 版以上，記錄實測版本）在 worktree 開啟方案，每項以 Agent 模式開新的 Copilot Chat 對話：
   1. 附加任一 `.cs` 檔，要求修改一個方法（不保留）。
   2. 附加任一 `.cs` 檔，問「寫法有沒有不符規範的地方」。
   3. 輸入「開議題」。
   4. 問「切換 session 要怎麼做」。
   5. 輸入「產出 release note」（DEC-24）。
3. 通過條件：
   1. 第 1 項讀取 `team/coding-csharp.md` 與 `project/` 的 `coding-*`。
   2. 第 2 項觸發 `review` skill，讀入 `team/`、`project/` 的規範。
   3. 第 3 項觸發 `issue-start` skill，先開分支，盤點後在分級確認點停下（DEC-23.5）；第 4 項讀取 `process-issue.md` 的交接。
   4. 第 5 項觸發 `release-note` skill，讀取 `process-git.md`。

## 4. SPEC 對照

| SPEC | TEST |
| --- | --- |
| SPEC-1 規範分層、載入與文件通則 | TEST-1、TEST-2、TEST-3、TEST-4、TEST-6、TEST-7、TEST-10、TEST-14、TEST-16 |
| SPEC-2 觸發入口 | TEST-3、TEST-7、TEST-15、TEST-16 |
| SPEC-3 議題分級、階段與確認點 | TEST-7、TEST-15 |
| SPEC-4 完成定義 | TEST-7、TEST-11、TEST-15 |
| SPEC-5 議題文件組合與內容歸屬 | TEST-7、TEST-15 |
| SPEC-6 項目編號與追溯 | TEST-6、TEST-7、TEST-15 |
| SPEC-7 Review | TEST-7、TEST-11、TEST-15 |
| SPEC-8 Git 流程 | TEST-7、TEST-11 |
| SPEC-9 Release note 與 merge log | TEST-7、TEST-15 |
| SPEC-10 Coding Style 修訂 | TEST-7、TEST-9 |
| SPEC-11 檔案與異動 | TEST-4、TEST-5、TEST-6、TEST-9 |
| SPEC-17 個人設定收斂 | TEST-12 |
| SPEC-18 後續議題 | TEST-13 |
| SPEC-19 本議題自身的文件 | TEST-11 |

## 5. 執行結果

### 5.1 自動（`Check.ps1`，2026-11-25，比對基準 `24c7276d`）

1. TEST-1～TEST-7、TEST-9、TEST-12：通過。
2. TEST-1 常駐 112 行（基準 193，差異 -81）：`copilot-instructions.md` 40、`CLAUDE.md` 7、`team-overview` 43、`project-overview` 22。
3. TEST-11：只剩 DoD 第 9、10 項與 REV 的「待人確認」，待人確認後通過。

### 5.2 人工

1. TEST-13：`FollowUp.md` 含 SPEC-18 各項（TODO-1～TODO-5），每項有目標、完成條件、延後原因、建議做法；TODO-2 列出 TEST-2 為候選。通過。
2. TEST-10：於 Review 第 2 回合文件段執行（獨立 session），發現列為 REV-20～REV-29，全部有處置。通過。
3. TEST-15 已依 DEC-23.5、DEC-24 的新條件重跑；TEST-16 待使用者重跑（REV-81、REV-82），以下 TEST-16 為前一次結果。TEST-14～TEST-16：DEC-21 調整結構後重跑（2026-11-25，`8b513b37`）；前一次（DEC-19 之後）三項皆通過，並確認 VS 2026 Copilot 可讀取 `.claude/skills/`。
   1. TEST-14：在 worktree 開新 session，載入 worktree 的 `CLAUDE.md`、`copilot-instructions.md`、`team-overview`、`project-overview`，以及個人層 `~/.claude/CLAUDE.md`、主 checkout 的 `CLAUDE.local.md` 與 auto memory 的 `MEMORY.md`；沒有載入 `team/`、`project/` 的檔案，主 checkout 的 `CLAUDE.md` 及其 `@` 引用（master 的 `coding-style-*.instructions.md`）也沒有載入。通過。
   2. TEST-15：DEC-24 後在 `b558e814` 重跑（2026-11-26，Claude Code、Opus 5.5，worktree 子 session）；第 3、4 項在同一 session 內執行，判斷依據為索引的對應關係。開始與結束時 `git status` 皆乾淨。
      1. `/issue-start`：讀 `process-issue`、`process-docs`、`process-git`；第一個動作是開分支，fetch 逾時時依 `process-git.md` 第 2 節第 3 項停下確認，說明分支名稱與基準後未實際建立；之後讀 `coding-architecture`、`project/coding-general` 盤點，在 scratchpad 產出符合 S 級範本的 `README.md`，提出分級建議後停下。通過。
      2. `/release-note`：讀 `process-git` 第 4 節；13 條 release note 符合前綴與格式，merge log 由 release note 組合、目標版本留空。通過。
      3. `/review`：讀兩份 `process-review`，再讀 `team/` 8 份、`project/` 5 份；門檻段執行 `Check.ps1`，依六段進行（未寫入 `Review.md`）。通過。
      4. 「開議題」符合 `issue-start` 的 description；「切換 session 或對話」經流程索引指向 `process-issue.md` 第 5 節的交接。通過。
      5. 修改 `AddPathDlg.SetFaceSelected` 前依兩份索引讀 `team/coding-general.md`、`team/coding-csharp.md`、`project/coding-architecture.md`、`project/coding-general.md`，修改已還原。通過。
      6. 發現：`issue-start` 的 description 把開分支寫在分級之後（已修正，REV-87）；新分支的基準沒有寫明（REV-88）。
   3. TEST-16：使用者在 VS 2026 Copilot Chat 於 worktree 方案執行，每項開新對話；結果依使用者提供的畫面記錄。
      1. 附加 `VectorTool.cs` 要求為 `GetAngle_deg` 加 `gp_Vec` 多載：觸發 `issue-start`，讀 `process-issue`、`process-docs`、`process-git`、`team/coding-general.md`、`coding-csharp.md`、`coding-architecture.md`、`project/coding-general.md`、`.editorconfig`；提出 S 級建議後停下等人確認。通過。
      2. 附加 `VectorTool.cs` 問「寫法是否有不符合規範的地方」：觸發 `review`，讀兩份 `process-review` 後搜尋 `*.md`，讀入 `team/`、`project/` 全部 13 份與 `.editorconfig`，並讀 `Build.bat` 執行建置門檻。通過。
      3. 輸入「開議題」：觸發 `issue-start`，讀 `process-issue`、`process-docs`、`process-git`。通過。
      4. 問「切換 session 要怎麼做」：讀兩份 `tool-claude-code.md`。通過。

## 6. 待討論

無。確認點 4 的決議見 DEC-4.5、DEC-7.1；TEST-8 由 DEC-22 刪除。
