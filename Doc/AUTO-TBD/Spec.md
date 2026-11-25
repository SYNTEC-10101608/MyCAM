# 規格

每條 SPEC 標示對應的 REQ（見 [Goal.md](Goal.md)）與依據的 DEC（見 [Discussion.md](Discussion.md)）。
本議題的交付物是規範文件，依 [process-docs.md](../../.github/team/process-docs.md) 第 4 節第 4 項：每條 SPEC 只寫決策重點與規則所在位置，規則本文以指引檔為準（DEC-22）。第 1 節為規則，第 2 節為檔案配置，第 3 節為相容性異動；原 SPEC-12～SPEC-16 併入 SPEC-11。

## 1. 規則

### SPEC-1 規範分層、載入與文件通則（REQ-1、REQ-2、REQ-3、REQ-5.5、REQ-11；DEC-2、DEC-9、DEC-11、DEC-13、DEC-18、DEC-21）

位置：[copilot-instructions.md](../../.github/copilot-instructions.md) 的規範分層、檔案分類、維護方式；[team-overview](../../.github/instructions/team-overview.instructions.md) 的文件通則與流程索引；[project-overview](../../.github/instructions/project-overview.instructions.md) 的專案指引索引。

1. 四層：入口、團隊通用、專案、個人；團隊通用層不含專案專屬內容，可直接搬到其他 repo；專案層可補充或具體化團隊通用層，個人層不改變團隊規範的要求。
2. 目錄表示載入方式：`instructions/` 只放每層一份的常駐概觀；`team/`、`project/` 依時機讀取，檔名以分類開頭（`process-`、`coding-`、`env-`、`tool-`）。
3. 載入門檻：AI 無法自行察覺何時該讀的內容才常駐；其他內容由概觀的索引寫明動作名稱或副檔名。Claude Code 只依 `@` 載入、VS 2026 Copilot 未依 `applyTo` 載入，兩者實際都靠索引（TEST-14、TEST-16）。
4. Copilot 與 Claude Code 讀同一份文件；`CLAUDE.md` 只以 `@` 引用常駐檔。
5. 常駐行數是觀察指標，不設上限；增加時 review 報告寫明增加量與理由。
6. 文件通則：精簡（只寫做對所需的資訊、同一規則只寫一處、機器能檢查的交給工具、非直覺的規則附理由）、只保留最終結果、數字條列、內容繁體中文與檔名英文。
7. 協作原則（REQ-11，DEC-13）：AI 以資深工程師角度提出建議；依重要性決定先由人決定或先處理再請人確認；回覆人時待決事項寫問題、建議、另一選項。

### SPEC-2 觸發入口（REQ-3.3、REQ-2.2；DEC-2.5、DEC-21）

位置：`.claude/skills/`；原則在 [copilot-instructions.md](../../.github/copilot-instructions.md) 維護方式第 3 項。

1. 三個 skill：`issue-start`（開議題）、`review`、`release-note`，各自依序列出必讀文件；`review` 讀入 `team/`、`project/` 全部文件。
2. Skill 只是入口，規則寫在指引檔；沒有 skill 時直接說出動作，由流程索引導向相同文件。
3. VS 2026 Copilot 也讀取 `.claude/skills/`（TEST-16）。

### SPEC-3 議題分級、階段與確認點（REQ-4.1、REQ-4.3、REQ-4.4；DEC-3.1、DEC-8、DEC-12）

位置：[process-issue.md](../../.github/team/process-issue.md) 第 1～3 節。

1. 八個處理階段；施工中遇到預料外的狀況先停下討論。
2. S、M、L 三級：S 級把確認點 1～5 合併，L 級每個確認點各自確認；分級由實作者確認，AI 在盤點後提建議；分級確認後開分支。
3. 每個確認點最後列「待討論」，每題寫問題、建議、另一可行選項。
4. 範圍超出級別時重新分級；後段發現前段要修改時，從最早受影響的文件回溯。

### SPEC-4 完成定義（REQ-4.2；DEC-3.1、DEC-20）

位置：[process-issue.md](../../.github/team/process-issue.md) 第 4 節。

1. 議題 `README.md` 放該級別的 DoD 勾選清單，全部勾選才送 MR；S 級精簡為五項。
2. 每個 STEP 可追溯到 commit；commit 標示為建議。

### SPEC-5 議題文件組合與內容歸屬（REQ-5.1、REQ-5.2、REQ-5.4；DEC-3、DEC-16、DEC-22）

位置：[process-docs.md](../../.github/team/process-docs.md) 第 1、2、4、5 節。

1. 存放於 `Doc/<議題編號>/`，後續 MR 沿用；尚未取得編號時以 `<前綴>-TBD` 暫代，前綴見專案指引。
2. S 級全部寫在 `README.md`；M 級拆出 Spec、TestPlan、Implementation、Review；L 級再拆出 Goal、Discussion、FollowUp。
3. 只保留最終結果，Discussion 與 Review 例外；收官時長期有效的結論提升到指引規範。
4. 規範類議題的 Spec 不重述指引內容。

### SPEC-6 項目編號與追溯（REQ-5.3；DEC-3.7、DEC-17）

位置：[process-docs.md](../../.github/team/process-docs.md) 第 3 節。

1. 前綴 `REQ`、`SPEC`、`TEST`、`STEP`、`TODO`、`DEC`、`REV`，寫法 `前綴-流水號`。
2. 確認後編號不重排、不重用；刪除的表格列與標題直接移除，有序清單的條列保留一行「（刪除，見 DEC-n）」。

### SPEC-7 Review（REQ-6；DEC-4、DEC-13、DEC-21）

位置：[team/process-review.md](../../.github/team/process-review.md)、[project/process-review.md](../../.github/project/process-review.md)。

1. 六段依序：門檻、需求與範圍、功能正確性、規範符合、設計品質、文件；每段寫明 AI 與人的分工。
2. Review 讀入 `team/`、`project/` 全部規範，作為寫程式時漏讀的保險。
3. 報告格式固定；處置只有已修正、不修正、列後續；非必修可先修正，寫「已修正（待人確認）」，送 MR 前不留待確認。
4. 專案固定檢查項目：建置門檻與專案程式規範。

### SPEC-8 Git 流程（REQ-7.1～REQ-7.3；DEC-5、DEC-12、DEC-20）

位置：[process-git.md](../../.github/team/process-git.md) 第 1～3、5 節。

1. 成文化現行版本號、版本分支、squash 合併與保留分支。
2. 分支 `<議題編號>_<類型>_<簡述>`，後綴 `_2nd`、`_Alt2`；開分支前先 fetch。
3. 每個 STEP 一個 commit；標題行結尾建議標示 `(STEP-n)` / `(REV-n)`。
4. Worktree 為選用；送 MR 由人決定，push 前先詢問 remote 與分支。

### SPEC-9 Release note 與 merge log（REQ-7.4；DEC-5.3）

位置：[process-git.md](../../.github/team/process-git.md) 第 4 節。

1. Release note 數字條列，以 `新增` / `修改` / `刪除` 開頭，由 AI 產出寫入 `README.md`。
2. Merge log 由 release note 組成，目標版本留空由合併者填入。

### SPEC-10 Coding Style 修訂（REQ-8；DEC-6、DEC-16）

位置：[team/coding-general.md](../../.github/team/coding-general.md) 的「註解」第 7 項與「修改既有程式碼」。

1. 新增與修改的程式碼採新規範；同一方法混用到難讀時可整個方法改寫，格式調整獨立 commit；不修改沒動到的方法。
2. Production Code 不引用文件、測試或規範條目，特例最多留 `// <議題編號>: <原因>`。

## 2. 檔案配置

### SPEC-11 檔案與異動（REQ-1、REQ-2、REQ-3；DEC-14、DEC-18、DEC-19、DEC-21）

| 檔案 | 分類與載入 | 異動 |
| --- | --- | --- |
| `.github/copilot-instructions.md`、`CLAUDE.md` | 入口，常駐 | 修改 |
| `.github/instructions/team-overview.instructions.md` | 團隊概觀，常駐 | 新增 |
| `.github/instructions/project-overview.instructions.md` | 專案概觀，常駐 | 修改：只留產品簡介、議題編號、專案指引索引 |
| `.github/team/process-issue.md`、`process-docs.md`、`process-review.md`、`process-git.md` | 團隊流程 | 新增 |
| `.github/team/coding-general.md` | 團隊程式規範 | 改名（原 `instructions/coding-style-general`）、修改、移出常駐 |
| `.github/team/coding-csharp.md`、`coding-cpp.md` | 團隊程式規範 | 改名、更新連結、專案資訊改為依專案指引（REV-52） |
| `.github/team/tool-claude-code.md` | 團隊工具做法 | 新增 |
| `.github/project/coding-architecture.md` | 專案程式規範 | 新增：由 `project-overview` 搬入技術棧、方案與主程式結構 |
| `.github/project/coding-general.md` | 專案程式規範 | 新增：由 `project-overview` 搬入慣例，加上四語系同步、Designer、`reference/` |
| `.github/project/process-review.md` | 專案流程 | 新增：專案固定檢查項目 |
| `.github/project/env-build.md` | 專案環境 | 新增：由 `project-overview` 搬入建置與執行環境 |
| `.github/project/tool-claude-code.md` | 專案工具做法 | 新增：worktree 做法 |
| `.claude/skills/{issue-start,review,release-note}/SKILL.md` | 觸發入口 | 新增 |
| `.gitignore` | 版控設定 | 加入 `/SESSION.md` |
| `.editorconfig` | 格式設定 | 加入 `[*.ps1] charset = utf-8-bom`，更新註解路徑 |
| `Doc/AUTO-TBD/*` | 議題文件 | 新增 |

1. 檔案格式依 `.editorconfig`；本議題新增與修改的規範檔使用數字條列，`team/coding-csharp.md`、`coding-cpp.md` 的條列留待 TODO-5。
2. `team/`、`project/` 不加 `applyTo`；檔名不用 `claude.md`。

### SPEC-17 個人設定收斂（REQ-9；DEC-2.7）

1. 刪除 `~/.claude/issue-workflow.md`；`~/.claude/CLAUDE.md` 移除「開發議題處理流程」章節與 `@` 引用，已成為團隊通則的數字條列與回覆方式也刪除，保留溝通語言、記憶規則、本機環境。
2. 主 checkout 的 `CLAUDE.local.md` 刪除議題資料夾位置與 push 前詢問；新增 `origin`（公司 GitLab，正式流程、MR）與 `github`（遠端協作）的用法。
3. 不進版控，完成結果記入 `README.md`。

### SPEC-18 後續議題（REQ-10；DEC-7）

1. `FollowUp.md` 每項含目標、完成條件、延後原因、建議做法，內容自足。
2. 至少包含：使用手冊維護機制、自動化品質門檻（本機檢查 → CI → 測試專案）、GitLab MR 建立自動化、既有分支 `AUTO-23254_f_UIStyleUnify` 依新規範調整、Coding Style 與 `.editorconfig` 的重複。

### SPEC-19 本議題自身的文件（REQ-5；DEC-1.1）

1. 依 L 級產出八份文件；`README.md` 含 L 級 DoD、文件索引與 Release Note；`Review.md` 為第一份依新格式的報告。

## 3. 相容性異動

### 3.1 輸入

1. 既有議題文件（舊結構與 `R` / `S` / `T` 編號）：相容，不轉換；後續 MR 沿用舊資料夾時，新增內容採新規範。
2. 既有分支名稱（`(2nd)`、`_V2`、`_merge` 等）：相容，不更名。
3. 既有程式碼的 Coding Style：相容，只在修改時依 SPEC-10 處理。
4. 個人設定：只影響使用者本人。
5. 進行中的分支改到舊指引檔名時，rebase 會衝突或連結失效；`AUTO-23254_f_UIStyleUnify` 的對照見 `FollowUp.md` 第 4 節。

### 3.2 輸出

1. Merge log：格式與現行一致，差別是 AI 產出時目標版本留空。
2. 常駐內容：只剩入口與兩份概觀，常駐總行數比施工前少。
3. Production Code、建置產物、NC 輸出：無。

## 4. 需求對照

| REQ | SPEC |
| --- | --- |
| REQ-1 規範分層 | SPEC-1、SPEC-11 |
| REQ-2 工具共用 | SPEC-1、SPEC-2、SPEC-11 |
| REQ-3 載入時機 | SPEC-1、SPEC-2、SPEC-11 |
| REQ-4 議題分級與完成定義 | SPEC-3、SPEC-4、SPEC-7 |
| REQ-5 議題文件 | SPEC-1、SPEC-5、SPEC-6、SPEC-19 |
| REQ-6 Review | SPEC-7 |
| REQ-7 Git 流程 | SPEC-8、SPEC-9 |
| REQ-8 Coding Style 修訂 | SPEC-10 |
| REQ-9 個人設定收斂 | SPEC-17 |
| REQ-10 後續議題 | SPEC-18 |
| REQ-11 協作原則 | SPEC-1 |

## 5. 待討論

無。各確認點與施工中的決策見 DEC-9～DEC-22。
