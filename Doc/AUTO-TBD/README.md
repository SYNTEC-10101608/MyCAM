# AUTO-TBD 團隊與 AI 協作開發流程規範

| 項目 | 內容 |
| --- | --- |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 分支起點 | `24c7276d`（`origin/master`，RLS_0.48.0，2026-11-23） |
| 級別 | L |
| 狀態 | 收官：STEP-1～STEP-17、Review 第 14 回合處置完成，TEST-16、TEST-17 的 VS 案例通過；待人工確認 |

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

## 完成清單（DoD）

1. [x] 每個 REQ 有 SPEC、每個 SPEC 有 TEST（Spec 第 4 節、TestPlan 第 4 節）。
2. [x] 相容性異動已寫明（Spec 第 3 節）。
3. [x] `TestPlan.md` 附最終執行結果（第 5 節）。
4. [x] 每個 STEP 可追溯到 commit（`Check.ps1` TEST-11）。
5. [x] 門檻通過：`Check.ps1` 除 TEST-11 外通過（TEST-11 檢查本清單與待人確認的處置，送 MR 前全部完成才通過）；本議題只改文件，不需建置。
6. [x] Review 報告寫入 `Review.md`，「必修」全部已修正。
7. [x] 範圍外項目寫入 `FollowUp.md`，每項寫明目標、完成條件與延後原因。
8. [x] Release note 寫入 `README.md`。
9. [ ] 各確認點分別經人確認（確認點 1～5 已確認；確認點 7 待人工全域 review）。
10. [ ] 長期有效的結論已提升到指引規範（待人工全域 review 確認）。

## Release Note

1. 新增 團隊與 AI 共用的協作原則、文件通則與流程索引，將個人開發流程移入版控，由 Copilot 與 Claude Code 共用同一份規範。
2. 新增 議題處理流程，定義 S、M、L 分級、階段確認點、完成定義（DoD）、範圍變動與回溯規則。
3. 新增 議題文件規範，統一各級文件組合、項目編號、追溯方式與 README 範本；各級後續處理皆獨立成檔，規範類議題的規格只寫決策與規則位置。
4. 新增 Review 規範，定義六段檢查、人與 AI 的分工、報告格式與發現的處置方式。
5. 新增 Git 流程規範，統一版本與分支命名、合併方式、commit 追溯、release note 與 merge log 格式。
6. 新增 開議題、review、產出 release note、委託 AI Tools Review 四個 skill 入口，提供各動作所需的必讀文件清單。
7. 新增 專案程式規範與 Review 固定檢查項目，涵蓋四語系同步、Designer、第三方檔案與建置門檻。
8. 修改 指引依入口、團隊、專案與個人分層，常駐內容精簡為入口與概觀；程式規範、架構、環境與工具做法依索引按需讀取，Review 時讀入全部規範，並明定指引與 skill 的維護原則。
9. 修改 Coding Style，新增與修改的程式碼採新規範，同一方法新舊混用到難以閱讀時可整體調整，格式調整獨立 commit；Production Code 不引用文件、測試或規範條目。
10. 新增 不限 AI 工具的 Session 交接與新 worktree 環境準備說明，worktree 為選用；Claude Code 專屬機制另列工具指引。
11. 新增 委託 AI Tools Review 的建議流程，以單一請求檔交接，由人決定模型、推理強度與發現的處置；提供 Copilot CLI 輔助腳本與增量 review 的終止條件。
12. 新增 AI 工具部署說明，VS 2026 18.5 以上的 Copilot 為必要，Copilot CLI 為建議，Claude Code 為選用。
