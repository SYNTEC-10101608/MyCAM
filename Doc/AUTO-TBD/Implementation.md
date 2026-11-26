# 實作架構與步驟

每個 STEP 標示對應的 SPEC（見 [Spec.md](Spec.md)）與 TEST（見 [TestPlan.md](TestPlan.md)）。
本議題的 commit 標題行結尾標示 `(STEP-n)`。

## 1. 實作架構

依 [copilot-instructions.md](../../.github/copilot-instructions.md) 的檔案分類：

| 分類 | 檔案 | SPEC |
| --- | --- | --- |
| 入口與概觀（常駐） | `copilot-instructions.md`、`CLAUDE.md`、`instructions/team-overview`、`instructions/project-overview` | SPEC-1、SPEC-11 |
| 流程 `process-` | `team/process-{issue,docs,review,git,ai-review}.md`、`project/process-review.md` | SPEC-3～SPEC-9、SPEC-20 |
| 程式規範 `coding-` | `team/coding-{general,csharp,cpp}.md`、`project/coding-{architecture,general}.md` | SPEC-10、SPEC-11 |
| 環境 `env-` | `team/env-ai-tools.md`、`project/env-build.md`、`project/env-worktree.md` | SPEC-11、SPEC-20 |
| 工具 `tool-` | `team/tool-claude-code.md`、`team/tool-copilot-cli.md` | SPEC-11、SPEC-20 |
| 輔助腳本 | `.github/scripts/Invoke-CopilotReview.ps1` | SPEC-20 |
| 觸發入口 | `.claude/skills/{issue-start,review,release-note,ai-tools-review}/SKILL.md` | SPEC-2 |
| 設定 | `.gitignore`、`.editorconfig` | SPEC-11 |
| 檢查腳本 | `Doc/AUTO-TBD/Check.ps1` | TestPlan 第 1 節 |
| 個人設定（不進版控） | `~/.claude/CLAUDE.md`、`~/.claude/issue-workflow.md`、主 checkout 的 `CLAUDE.local.md` | SPEC-17 |
| 議題文件 | `Doc/AUTO-TBD/*` | SPEC-18、SPEC-19 |

1. 依賴順序：檢查腳本 → 流程 → 入口與概觀（引用流程的路徑）→ 專案層 → 觸發入口 → 個人設定 → 驗證與收官。
2. 每完成一個 STEP 就執行 `Check.ps1`，該 STEP 對應的 TEST 須通過。
3. 數字條列轉換屬純格式調整，與內容修改分開 commit。
4. STEP-1～STEP-13 的檔名為施工當時的名稱，DEC-18、DEC-21 改名後的對應見 SPEC-11。

## 2. 實作步驟

### STEP-1 檢查腳本

1. 新增 `Check.ps1`，每條自動 TEST 一個函式，輸出 PASS / FAIL 與明細，有失敗時 exit code 非 0；參數 `-Base`（預設 `24c7276d`）、`-SkipPersonal`。
2. `.editorconfig` 加入 `[*.ps1] charset = utf-8-bom`，與腳本分開 commit。
3. SPEC：SPEC-11。TEST：TEST-1～TEST-7、TEST-9、TEST-11、TEST-12。

### STEP-2 議題處理流程

1. 新增 `team/process-issue.md`：階段、分級與確認點、待討論格式、重新分級、回溯、各級 DoD。
2. SPEC：SPEC-3、SPEC-4。TEST：TEST-2、TEST-3、TEST-6、TEST-7。

### STEP-3 議題文件規範與 `.gitignore`

1. 新增 `team/process-docs.md`：存放、檔案組合、編號與追溯、各文件內容、範本。
2. `.gitignore` 加入 `/SESSION.md`，分開 commit。
3. SPEC：SPEC-5、SPEC-6。TEST：TEST-2、TEST-3、TEST-4、TEST-6、TEST-7。

### STEP-4 Review 規範

1. 新增 `team/process-review.md`：六段與分工、報告格式、處置規則。
2. SPEC：SPEC-7。TEST：TEST-2、TEST-3、TEST-6、TEST-7。

### STEP-5 Git 流程

1. 新增 `team/process-git.md`：版本、分支、commit、release note、merge log、worktree 通用規則。
2. SPEC：SPEC-8、SPEC-9。TEST：TEST-2、TEST-3、TEST-6、TEST-7。

### STEP-6 常駐與入口文件

1. 新增團隊概觀（協作原則、文件通則、流程索引）；修改 `copilot-instructions.md` 與 `CLAUDE.md`。
2. SPEC：SPEC-1。TEST：TEST-1、TEST-3、TEST-4、TEST-6、TEST-7。

### STEP-7 專案指引

1. 由 `project-overview` 搬出建置與執行環境、Git 流程；新增專案固定檢查項目、兩份 Claude Code 做法、專案程式規範；新增「議題編號」。
2. 依 DEC-18 改名，以 `git mv` 保留改名記錄。
3. SPEC：SPEC-1、SPEC-11。TEST：TEST-1、TEST-2、TEST-4、TEST-6、TEST-7、TEST-9。

### STEP-8 Coding Style 修訂

1. 通用 Coding Style 的「修改既有程式碼」與「註解」依 SPEC-10 修訂。
2. SPEC：SPEC-10。TEST：TEST-1、TEST-6、TEST-9。

### STEP-9 Skills

1. 新增三個 `SKILL.md`。
2. SPEC：SPEC-2。TEST：TEST-3、TEST-7、TEST-15。

### STEP-10 `FollowUp.md`

1. 寫入 SPEC-18 各項。
2. SPEC：SPEC-18。TEST：TEST-13。

### STEP-11 個人設定收斂

1. 依 SPEC-17 修改個人設定，在 STEP-12 的動態驗證前執行（DEC-10.4）；完成結果記入 `README.md`。
2. SPEC：SPEC-17。TEST：TEST-12。

### STEP-12 驗證

1. 執行 `Check.ps1` 與人工 TEST；提供 TEST-16 的逐步清單給使用者；結果寫入 `TestPlan.md` 第 5 節。
2. SPEC：SPEC-19。TEST：TEST-1～TEST-16。

### STEP-13 Review 與收官

1. Review 寫入 `Review.md`；`README.md` 的 DoD、文件索引、Release Note。
2. SPEC：SPEC-4、SPEC-7、SPEC-9、SPEC-19。TEST：TEST-10、TEST-11。

### STEP-14 載入門檻與檔案分類（DEC-21、DEC-22）

1. 依 DEC-21 以 `git mv` 改名與搬移指引檔；`project-overview` 拆出 `project/coding-architecture.md`，慣例併入 `project/coding-general.md`；`copilot-instructions.md` 改為分類表並加入 skill 原則；兩份概觀的索引、skills、`CLAUDE.md`、`.editorconfig` 同步。
2. 依 DEC-22 在 `process-docs.md` 加入規範類議題的 Spec 寫法，並以此精簡本議題的 Spec、TestPlan、Implementation。
3. `Check.ps1` 同步：TEST-4 檢查分類開頭與索引涵蓋，TEST-7 不檢查歷史記錄的路徑，TEST-9 檢查拆分結果。
4. 重跑 TEST-14～TEST-16。
5. SPEC：SPEC-1、SPEC-2、SPEC-5、SPEC-7、SPEC-11。TEST：TEST-1～TEST-7、TEST-9、TEST-14～TEST-16。

### STEP-15 規範與建議分開、參照明確（DEC-23）

1. 依 DEC-23 修改 `team/` 的 `process-issue`、`process-docs`、`process-review`、`process-git`、`tool-claude-code`、`coding-csharp`，`project/` 的 `process-review`、`tool-claude-code`（STEP-16 刪除），團隊概觀的協作原則與流程索引，`review` skill 的觸發說明，個人 `~/.claude/CLAUDE.md`。
2. 同步 Goal、Spec、TestPlan、README。
3. SPEC：SPEC-1、SPEC-3、SPEC-4、SPEC-5、SPEC-7、SPEC-8、SPEC-11、SPEC-17。TEST：TEST-1、TEST-2、TEST-7、TEST-9、TEST-10。

### STEP-16 VS 支援版本、worktree 準備與交接不限工具（DEC-24）

1. 新增 `project/env-worktree.md`，刪除 `project/tool-claude-code.md`；session 交接移到 `process-issue.md` 第 5 節，`team/tool-claude-code.md` 只留 Claude Code 的 worktree 機制；兩份概觀的索引、`process-git.md` 第 5 節同步。
2. DoD 的 FollowUp 項目改為依 `process-docs.md` 第 4 節，S 級範本註明 FollowUp 例外；TODO-5 加入 REV-84、REV-85 指出的項目。
3. Goal（REQ-2 最低版本 18.5）、Spec、TestPlan（TEST-9、TEST-15、TEST-16 條件）與 `Check.ps1` 同步；重跑 TEST-15、TEST-16。
4. SPEC：SPEC-2、SPEC-3、SPEC-4、SPEC-8、SPEC-11、SPEC-18。TEST：TEST-4、TEST-7、TEST-9、TEST-15、TEST-16。

### STEP-17 委託 AI Tools Review（DEC-25）

1. 新增 `team/process-ai-review.md`、`team/tool-copilot-cli.md`、`team/env-ai-tools.md` 與 `.github/scripts/Invoke-CopilotReview.ps1`；`process-review.md` 報告格式加註受審 HEAD 與推理強度；團隊概觀的流程索引加兩列；`copilot-instructions.md` 的檔案分類加入 `scripts/`；`.gitignore` 加入 `/REVIEW-REQUEST.md`。
2. Goal（REQ-12）、Spec、TestPlan（TEST-17）與 `Check.ps1`（TEST-4、TEST-5、TEST-9）同步。
3. 以腳本跑一回合委託 review 驗收；VS 端由使用者執行 TEST-17 第 1.2 項。
4. VS 案例第一次未通過後，新增 `ai-tools-review` skill，`review` skill 的觸發說明排除委託（DEC-25.12）。
5. SPEC：SPEC-1、SPEC-2、SPEC-7、SPEC-11、SPEC-20。TEST：TEST-4、TEST-5、TEST-7、TEST-9、TEST-17。

## 3. SPEC 對照

| SPEC | STEP |
| --- | --- |
| SPEC-1 規範分層、載入與文件通則 | STEP-6、STEP-7、STEP-14、STEP-15、STEP-17 |
| SPEC-2 觸發入口 | STEP-9、STEP-14、STEP-16、STEP-17 |
| SPEC-3 議題分級、階段與確認點 | STEP-2、STEP-15、STEP-16 |
| SPEC-4 完成定義 | STEP-2、STEP-13、STEP-15、STEP-16 |
| SPEC-5 議題文件組合與內容歸屬 | STEP-3、STEP-14、STEP-15 |
| SPEC-6 項目編號與追溯 | STEP-3 |
| SPEC-7 Review | STEP-4、STEP-13、STEP-14、STEP-15、STEP-17 |
| SPEC-8 Git 流程 | STEP-5、STEP-15、STEP-16 |
| SPEC-9 Release note 與 merge log | STEP-5、STEP-13 |
| SPEC-10 Coding Style 修訂 | STEP-8 |
| SPEC-11 檔案與異動 | STEP-1、STEP-3、STEP-7、STEP-14、STEP-15、STEP-16、STEP-17 |
| SPEC-17 個人設定收斂 | STEP-11、STEP-15 |
| SPEC-18 後續議題 | STEP-10、STEP-16 |
| SPEC-19 本議題自身的文件 | STEP-12、STEP-13 |
| SPEC-20 委託 AI Tools Review 與 AI 工具部署 | STEP-17 |

## 4. 待討論

無。確認點 5 的決議見 DEC-10；施工中的結構調整見 DEC-14、DEC-18、DEC-21～DEC-25。
