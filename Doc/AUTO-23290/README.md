# AUTO-23290 團隊與 AI 協作開發流程規範

| 項目 | 內容 |
| --- | --- |
| 分支 | `AUTO-23290_f_DevProcess` |
| 分支起點 | `24c7276d`（`origin/master`，RLS_0.48.0，2026-11-23） |
| 級別 | L |
| 狀態 | 完成：STEP-1～STEP-17、Review 第 14 回合處置完成，人工全域 review 通過；待送 MR |

## 文件

| 文件 | 內容 |
| --- | --- |
| [Goal.md](Goal.md) | 現況盤點、目標、範圍、需求（REQ） |
| [Spec.md](Spec.md) | 規則與位置、檔案配置、相容性異動（SPEC） |
| [TestPlan.md](TestPlan.md) | 測試規劃與執行結果（TEST） |
| [Implementation.md](Implementation.md) | 實作架構與步驟（STEP） |
| [Review.md](Review.md) | Review 報告（REV） |
| [Discussion.md](Discussion.md) | 討論記錄：依主題整理的共識與決策（DEC） |
| [FollowUp.md](FollowUp.md) | 後續處理（TODO） |

## 個人設定收斂

個人設定不進版控，依 SPEC-17 於 2026-11-24 完成，TEST-12 通過：

1. 刪除 `~/.claude/issue-workflow.md`；`~/.claude/CLAUDE.md` 移除「開發議題處理流程」章節、數字條列規則與回覆方式（2026-11-25，DEC-23）。
2. 主 checkout 的 `CLAUDE.local.md` 刪除議題資料夾位置與 push 章節，新增 `origin`、`github` 的 remote 用法；開分支前先詢問以哪個 remote 的 `master` 為基準（REV-91）。

## 議題總結

規則本文以指引檔為準，本節只整理重點與位置。

### 1. 處理的事項

1. 開發流程原本只在個人設定（`~/.claude/issue-workflow.md`），現在移入 repo 的 `.github/`，Copilot 與 Claude Code 讀同一份（REQ-1、REQ-2）。
2. 規範分為入口、團隊通用、專案、個人四層，依載入時機分目錄：常駐只留入口與兩份概觀（193 行降為 114 行），其餘由索引依動作或副檔名讀取（REQ-3）。
3. 流程成文：議題分級、確認點、DoD、範圍變動與回溯（`process-issue.md`）；議題文件的組合與編號（`process-docs.md`）；六段 review 與報告格式（`process-review.md`）；版本、分支、commit、release note、merge log（`process-git.md`）。
4. 協作原則：AI 以資深工程師的角度提出建議；小事先做後確認，重要事項先由人決定；待討論一律寫問題、建議、另一可行選項（REQ-11）。
5. Coding Style 修訂：新增與修改的程式碼採新規範；Production Code 不引用文件（REQ-8）。
6. 專案層拆分：架構、專案慣例（加入四語系同步、Designer、`reference/`）、建置環境、新 worktree 準備、專案固定檢查項目。
7. 委託 AI Tools Review（建議做法）與 AI 工具部署說明，附 Copilot CLI 輔助腳本（REQ-12）。
8. 四個 skill：開議題、review、產出 release note、委託 AI Tools Review。
9. 個人設定收斂到只剩個人偏好（REQ-9）。
10. 範圍外項目列為 TODO-1～TODO-5，並寫好 `AUTO-23254_f_UIStyleUnify` 的 rebase 指引（REQ-10）。
11. 驗證：`Check.ps1` 自動檢查、TEST-12～TEST-17 人工與動態演練（Claude Code、VS 2026 Copilot、Copilot CLI）；Review 共 14 回合，REV-1～REV-109。

### 2. 團隊要知道的事

#### 2.1 規範

| 時機 | 要求 | 位置 |
| --- | --- | --- |
| 開議題 | 先開分支，以遠端 `master` 為基準，開分支前先 fetch；命名 `<議題編號>_<類型>_<簡述>` | `team/process-git.md` 第 2 節 |
| 分級 | S、M、L 由實作者確認；各級的確認點與 DoD 不同 | `team/process-issue.md` 第 2、4 節 |
| 議題文件 | 放 `Doc/<議題編號>/`，檔案組合依級別；編號用 `REQ`、`SPEC`、`TEST`、`STEP`、`TODO`、`DEC`、`REV` | `team/process-docs.md` |
| Commit | 每個 STEP 一個 commit；格式調整獨立 commit；建議在標題行結尾標 `(STEP-n)` / `(REV-n)` | `team/process-git.md` 第 3 節 |
| Review | 依序六段，報告格式固定，處置只有已修正、不修正、列後續；送 MR 前不留待確認 | `team/process-review.md` |
| 專案檢查 | Debug 與 Release 建置；四語系同步；Designer 仍可開啟；不修改 `reference/` | `project/process-review.md`、`project/coding-general.md` |
| 送 MR | README 的 DoD 全部勾選；release note 以 `新增` / `修改` / `刪除` 開頭，由 AI 產出；merge log 的目標版本留空，由合併者填入 | `team/process-issue.md` 第 4 節、`team/process-git.md` 第 4 節 |
| 寫程式 | 新增與修改的程式碼採新規範；Production Code 的註解不引用文件與條目 | `team/coding-general.md` |
| 改指引 | 在議題文件寫明異動與理由；新增常駐內容須符合載入門檻 | `copilot-instructions.md` 的維護方式 |

#### 2.2 執行上的差異

| 項目 | 之前 | 之後 |
| --- | --- | --- |
| 流程來源 | 個人設定，只有個人的 AI 讀得到 | repo 的 `.github/`，全員與 AI 共用 |
| AI 的載入 | 指引全部常駐 | 常駐概觀，其餘由索引依時機讀取 |
| 觸發 | 自行描述 | 說出動作（「開議題」、「review」、「產出 release note」、「委託 AI Tools Review」），由 skill 或流程索引導向必讀文件 |
| 小議題 | 與大議題同一套 | S 級合併確認點，文件全部寫在 `README.md` |
| Review | 標準未成文 | 六段，分工寫明 AI 與人各做什麼；可另委託其他 AI 工具（建議） |
| 分支後綴 | `_2nd`、`(2nd)`、`_V2` 等並存 | 建議用 `_2nd`、`_Alt2` |
| Worktree 與交接 | Claude Code 專用的個人做法 | Worktree 為選用，準備步驟不限工具；換 session 時以 `SESSION.md` 交接 |
| 與 AI 的分工 | 未定義 | 小事先做後列清單，需求、規格、範圍、合併與 push 先由人決定 |

#### 2.3 環境部署

1. 必要：Visual Studio 2026 18.5 以上，Copilot Chat 使用 Agent 模式（`team/env-ai-tools.md` 第 1 節）。
2. 建議：Copilot CLI，委託 review 時當 reviewer；安裝、核對 SHA256、登入與驗證見 `team/env-ai-tools.md` 第 2 節。
3. 選用：Claude Code，團隊流程不依賴它。
4. 專案建置與新 worktree 的準備：卸載 `FCLTool`、首次建置後執行 `CopyRef.bat`、複製 `FCLTool.dll`、`PostTool.dll` 與 `Custom/`（`project/env-build.md`、`project/env-worktree.md`）。

### 3. 規範文件架構

#### 3.1 進版控（團隊）

```text
CLAUDE.md                         入口（Claude Code），以 @ 引用下列三份常駐檔
.editorconfig                     格式規則；與指引衝突時以此為準
.gitignore                        排除 /SESSION.md、/REVIEW-REQUEST.md、CLAUDE.local.md、.claude/settings.local.json、.claude/worktrees/
.worktreeinclude                  Claude Code 建立 worktree 時要複製的未追蹤檔案
.github/
  copilot-instructions.md         入口（常駐）：規範分層、檔案分類、維護方式
  instructions/
    team-overview.instructions.md      團隊概觀（常駐）：協作原則、文件通則、流程索引
    project-overview.instructions.md   專案概觀（常駐）：產品簡介、議題編號、專案指引索引
  team/                           團隊通用，依流程索引讀取；不含專案內容
    process-issue.md              階段、分級與確認點、範圍變動、DoD、交接
    process-docs.md               議題文件的存放、組合、編號、範本
    process-review.md             六段、報告格式、處置規則
    process-ai-review.md          委託 AI Tools Review（建議）
    process-git.md                版本、分支、commit、release note、merge log、worktree
    coding-general.md             Coding Style 通用規則
    coding-csharp.md、coding-cpp.md   各語言的 Coding Style
    env-ai-tools.md               AI 工具部署
    tool-claude-code.md           Claude Code 的 worktree 機制
    tool-copilot-cli.md           Copilot CLI 當 reviewer
  project/                        MyCAM 專屬，依專案指引索引讀取
    coding-architecture.md        技術棧、方案與主程式結構
    coding-general.md             專案慣例：Action + Dialog、多語系、日誌、Designer、reference/
    process-review.md             專案固定檢查項目
    env-build.md                  建置與執行環境
    env-worktree.md               新 worktree 的環境準備
  scripts/
    Invoke-CopilotReview.ps1      列出模型、以 Copilot CLI 執行委託 review
.claude/skills/                   觸發入口，只列必讀文件（VS Copilot 也讀）
  issue-start/、review/、release-note/、ai-tools-review/
Doc/<議題編號>/                   議題文件
```

1. Copilot 讀 `copilot-instructions.md` 與 `instructions/`；Claude Code 讀 `CLAUDE.md` 的 `@` 引用。兩者實際都靠索引讀取 `team/`、`project/`（TEST-14、TEST-16）。
2. `team/` 只單向引用專案層的固定檔名：專案概觀、`project/process-review.md`、`project/env-worktree.md`、`project/coding-architecture.md`。

#### 3.2 不進版控（個人與暫存）

| 檔案 | 範圍 | 內容 |
| --- | --- | --- |
| `~/.claude/CLAUDE.md` | 個人，所有專案 | 溝通語言、記憶規則、本機環境 |
| 主 checkout 的 `CLAUDE.local.md` | 個人，本專案 | 議題編號、commit 語言、建置習慣、分支與 worktree、remote 用法 |
| `.claude/settings.local.json` | 個人，本專案 | Claude Code 的權限設定 |
| `~/.claude/projects/<專案路徑>/memory/` | 個人，依專案路徑存放 | auto memory；本專案只剩 `act-first-on-minor-items`，合併後刪除 |
| 工作目錄根目錄的 `SESSION.md` | 暫存 | Session 交接 prompt |
| 工作目錄根目錄的 `REVIEW-REQUEST.md` | 暫存 | 委託 review 的請求檔 |

### 4. 移到其他專案或設備

#### 4.1 其他專案

1. 直接複製：`.github/copilot-instructions.md`、`instructions/team-overview.instructions.md`、`team/`、`scripts/`、`.claude/skills/`、`CLAUDE.md`。
2. 依新專案修改：
   1. `copilot-instructions.md` 的標題（目前為「MyCAM 開發指引」）。
   2. `team/coding-*.md` 只留該專案用到的語言，並同步團隊概觀流程索引的程式碼列。
3. 依新專案重寫：
   1. `project-overview.instructions.md`：產品簡介、議題編號前綴與分支範例、專案指引索引。
   2. `project/` 至少要有團隊層引用的三份：`process-review.md`（固定檢查項目）、`env-worktree.md`、`coding-architecture.md`；專案慣例寫在 `coding-general.md`；其他依需要新增，並在專案概觀的索引加列。
4. 設定檔：`.gitignore` 加入第 3.1 節列出的排除項；`.editorconfig` 加入 `[*.ps1] charset = utf-8-bom`；有成員用 Claude Code 時，依專案列出 `.worktreeinclude`。
5. 不要搬：`Doc/AUTO-23290/Check.ps1` 與本議題文件；Check.ps1 只檢查本議題，通用檢查另見 TODO-2。
6. 驗證：
   1. 在 `team/`、`.claude/skills/` 搜尋舊專案名稱與議題前綴（如 `MyCAM`、`AUTO-`），應該沒有結果。
   2. VS Copilot 說「開議題」時，應讀到 `issue-start` 的必讀清單。
   3. 執行 `copilot instruction list`，應列出入口與兩份概觀。

#### 4.2 其他設備

1. 依第 2.3 節安裝 VS 2026 與 Copilot CLI。Copilot CLI 不在預設路徑時，設定 `COPILOT_CLI_PATH`，並以 `copilot login --device-code` 登入。使用 Claude Code 的成員另外安裝並登入。
2. 個人設定要手動帶過去：
   1. `~/.claude/CLAUDE.md`：「本機環境」一節依新設備改寫。
   2. 主 checkout 的 `CLAUDE.local.md` 與 `.claude/settings.local.json`。
3. Auto memory 依專案路徑存放，換設備或換路徑時不會跟著過去；本專案只剩一條，合併後刪除，不需要搬。
4. 專案環境依 `project/env-build.md` 準備：卸載 `FCLTool`、複製原生 DLL、執行 `CopyRef.bat`、放 `Custom/`。

## 完成清單（DoD）

1. [x] 每個 REQ 有 SPEC、每個 SPEC 有 TEST（Spec 第 4 節、TestPlan 第 4 節）。
2. [x] 相容性異動已寫明（Spec 第 3 節）。
3. [x] `TestPlan.md` 附最終執行結果（第 5 節）。
4. [x] 每個 STEP 可追溯到 commit（`Check.ps1` TEST-11）。
5. [x] 門檻通過：`Check.ps1` 除 TEST-11 外通過（TEST-11 檢查本清單與待人確認的處置，送 MR 前全部完成才通過）；本議題只改文件，不需建置。
6. [x] Review 報告寫入 `Review.md`，「必修」全部已修正。
7. [x] 範圍外項目寫入 `FollowUp.md`，每項寫明目標、完成條件與延後原因。
8. [x] Release note 寫入 `README.md`。
9. [x] 各確認點分別經人確認（確認點 7 的人工全域 review 於 2026-11-26 完成，45 項待確認 REV 一併確認）。
10. [x] 長期有效的結論已提升到指引規範（見「議題總結」第 2 節）。

## Release Note

1. 新增 團隊與 AI 共用的開發規範，Copilot 與 Claude Code 讀同一份，依團隊、專案、個人分層並依需要載入。
2. 新增 議題處理流程，涵蓋分級、確認點、完成定義、議題文件、review 與 Git 規範。
3. 新增 專案程式規範、review 固定檢查項目，以及建置與 worktree 的環境說明。
4. 新增 開議題、review、產出 release note、委託 AI Tools Review 的 skill 入口。
5. 新增 委託其他 AI 工具 review 的建議流程與 AI 工具部署說明。
6. 修改 Coding Style，新增與修改的程式碼採新規範，程式註解不引用文件。
