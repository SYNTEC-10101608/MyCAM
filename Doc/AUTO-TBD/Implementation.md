# 實作架構與步驟

每個 STEP 標示對應的 SPEC（見 [Spec.md](Spec.md)）與 TEST（見 [TestPlan.md](TestPlan.md)）。
Commit 訊息結尾標示 `(STEP-n)`，以 `git log --grep "STEP-n"` 查詢對應（SPEC-8.5）。

## 1. 實作架構

| 模組 | 檔案 | SPEC |
| --- | --- | --- |
| 檢查腳本 | `Doc/AUTO-TBD/Check.ps1`、`.editorconfig` | TestPlan 第 1 節（DEC-7.1、DEC-10.3） |
| 流程本文 | `.github/process/issue-workflow.md`、`issue-docs.md`、`review.md`、`git-workflow.md` | SPEC-3～SPEC-9、SPEC-13 |
| 常駐與入口 | `collaboration.instructions.md`、`copilot-instructions.md`、`CLAUDE.md` | SPEC-1、SPEC-12、SPEC-15 |
| 專案層與 Coding Style | `project-overview.instructions.md`、`.github/project/build.md`、`.github/project/claude-worktree.md`、`coding-style-general.instructions.md` | SPEC-10、SPEC-14 |
| 觸發入口 | `.claude/skills/{issue-start,review,release-note}/SKILL.md` | SPEC-2、SPEC-16 |
| 版控設定 | `.gitignore` | SPEC-11 |
| 個人設定（不進版控） | `~/.claude/CLAUDE.md`、`~/.claude/issue-workflow.md`、主 checkout 的 `CLAUDE.local.md` | SPEC-17 |
| 議題文件 | `Doc/AUTO-TBD/*` | SPEC-18、SPEC-19 |

1. 依賴順序：檢查腳本 → 流程本文 → 常駐與入口（引用流程本文的路徑）→ 專案層 → 觸發入口 → 個人設定 → 驗證與收官。
2. 先寫檢查腳本，每完成一個 STEP 就執行，該 STEP 對應的 TEST 須通過；其他 TEST 允許尚未通過。
3. 規範檔的數字條列轉換屬純格式調整，與內容修改分開 commit（SPEC-8.5.5、SPEC-11.2）。

## 2. 實作步驟

### STEP-1 檢查腳本

1. 新增 `Doc/AUTO-TBD/Check.ps1`，實作 TestPlan 中標「自動」的 TEST：
   1. 每條 TEST 一個函式，輸出 `TEST-n PASS / FAIL` 與失敗明細；最後輸出總結，有失敗時 exit code 非 0。
   2. 參數 `-Base`，預設 `24c7276d`，供 TEST-1、TEST-5、TEST-11 比對。
   3. TEST-6 換行以 `git ls-files --eol` 的 index 與 attr 判定，不看工作目錄。
   4. TEST-12 讀取家目錄與主 checkout（`git worktree list` 第一筆）的個人設定。
2. `.editorconfig` 加入 `[*.ps1] charset = utf-8-bom`，腳本以 UTF-8 with BOM 儲存，可直接寫中文特徵字串；與腳本分開 commit。
3. 執行一次確認可運作，預期大部分 TEST 失敗，結果不記錄。
4. SPEC：TestPlan 第 1 節、SPEC-11。TEST：TEST-1～TEST-7、TEST-9、TEST-11、TEST-12（腳本本身）。

### STEP-2 `process/issue-workflow.md`

1. 內容：SPEC-3（階段、分級與確認點、待討論格式、重新分級、回溯）、SPEC-4（各級 DoD）。
2. 開頭一句說明適用時機；文件規範以連結指向 `issue-docs.md`。
3. SPEC：SPEC-3、SPEC-4、SPEC-13.1、SPEC-13.5。TEST：TEST-2、TEST-3、TEST-6、TEST-7、TEST-8。

### STEP-3 `process/issue-docs.md` 與 `.gitignore`

1. `issue-docs.md` 內容：SPEC-5、SPEC-6、各文件內容規範、精簡範本（S 級 `README.md` 完整範本；M、L 骨架）。
2. `.gitignore` 加入 `/SESSION.md`，與 `issue-docs.md` 分開 commit。
3. SPEC：SPEC-5、SPEC-6、SPEC-13.2、SPEC-13.5。TEST：TEST-2、TEST-3、TEST-4.5、TEST-6、TEST-7、TEST-8。

### STEP-4 `process/review.md`

1. 內容：SPEC-7（六段與分工、報告格式、處置規則、回到門檻段）；專案固定檢查項目只寫「見專案指引」。
2. SPEC：SPEC-7、SPEC-13.3、SPEC-13.5。TEST：TEST-2、TEST-3、TEST-6、TEST-7、TEST-8。

### STEP-5 `process/git-workflow.md`

1. 內容：SPEC-8（版本、合併、分支命名、fetch、commit、push、worktree 通用規則）、SPEC-9（release note、merge log）。
2. Worktree 只寫通用規則；專案要複製的檔案寫「見專案指引」。
3. SPEC：SPEC-8、SPEC-9、SPEC-13.4、SPEC-13.5。TEST：TEST-2、TEST-3、TEST-6、TEST-7、TEST-8。

### STEP-6 常駐與入口文件

1. 新增 `collaboration.instructions.md`：frontmatter `applyTo: "**"`；協作原則、文件通則、流程索引（完整路徑，含「新增或修改程式碼」一列）。
2. `copilot-instructions.md`：四層分類精簡說明、指引目錄加入 `collaboration`、`process/` 與 `project/build.md`（標示載入方式）、改為「本檔只放目錄與分類說明」、維護方式加入常駐內容的原則；數字條列轉換另一個 commit。
3. `CLAUDE.md`：`@` 引用加入 `collaboration.instructions.md`，移除 `coding-style-csharp`、`coding-style-cpp`。
4. SPEC：SPEC-1、SPEC-12、SPEC-15。TEST：TEST-1、TEST-3、TEST-4、TEST-6、TEST-7。

### STEP-7 專案指引

1. 新增 `.github/project/build.md`：由 `project-overview` 搬入 FCLTool、命令列建置、執行期檔案，新增 worktree 準備；搬移與新增分開 commit，搬移的 commit 只做搬移與條列轉換，方便比對。
2. `project-overview.instructions.md`：
   1. 刪除「Git 流程」章節與建置細節，「建置與執行」只留指向 `project/build.md` 的索引。
   2. 新增「專案固定檢查項目」四項，刪除「慣例」與「技術棧」中重複的三句。
   3. 數字條列轉換另一個 commit。
3. SPEC：SPEC-14、SPEC-1.7。TEST：TEST-1、TEST-2（專案內容不外流到通用層）、TEST-4.2、TEST-6、TEST-7、TEST-9。

### STEP-8 Coding Style 修訂

1. `coding-style-general.instructions.md`：「修改既有程式碼」改為 SPEC-10.1 三項；「註解」加入 SPEC-10.2；數字條列轉換另一個 commit。
2. SPEC：SPEC-10。TEST：TEST-1、TEST-6、TEST-9。

### STEP-9 Skills

1. 新增三個 `SKILL.md`：frontmatter `name`、`description`（寫明觸發情境），本文只有「依 `.github/process/<文件>.md` 執行」與必讀文件清單。
2. SPEC：SPEC-2、SPEC-16。TEST：TEST-3.2、TEST-7、TEST-15。

### STEP-10 `FollowUp.md`

1. 寫入 SPEC-18.2 六項，每項含目標、完成條件、延後原因、建議做法；自動化門檻項目列出 TEST-2 為候選。
2. SPEC：SPEC-18。TEST：TEST-13。

### STEP-11 個人設定收斂

1. 依 SPEC-17 修改 `~/.claude/CLAUDE.md`、刪除 `~/.claude/issue-workflow.md`、修改主 checkout 的 `CLAUDE.local.md`；在 STEP-12 的動態驗證前執行（DEC-10.4）。
2. 個人設定不進版控；完成結果記入 `README.md`，以該 commit 對應本 STEP（SPEC-17.4）。
3. SPEC：SPEC-17。TEST：TEST-12。

### STEP-12 驗證

1. 執行 `Check.ps1`，全部自動 TEST 通過（TEST-11 的 DoD 與 `Review.md` 條件在 STEP-13 後才會通過）。
2. 執行人工 TEST：TEST-8（AI 列位置、人抽查）、TEST-14、TEST-15；提供 TEST-16 的逐步清單給使用者執行。
3. 結果寫入 `TestPlan.md` 第 5 節。
4. SPEC：SPEC-19。TEST：TEST-1～TEST-16。

### STEP-13 Review 與收官

1. 自我 review 與獨立 review，報告依 SPEC-7.3 寫入 `Review.md`；修正的 commit 標示 `(REV-n)`，每輪修正後重跑 `Check.ps1`。
2. `README.md`：L 級 DoD 勾選清單、文件索引（加入 `Implementation`、`Review`、`FollowUp`）、Release Note。
3. 再執行一次 `Check.ps1`，TEST-11 通過。
4. SPEC：SPEC-4、SPEC-7、SPEC-9、SPEC-19。TEST：TEST-10、TEST-11、TEST-15.2.3。

## 3. SPEC 對照

| SPEC | STEP |
| --- | --- |
| SPEC-1 規範分層、讀取方式與文件通則 | STEP-6、STEP-7 |
| SPEC-2 觸發入口 | STEP-9 |
| SPEC-3 議題分級、階段與確認點 | STEP-2 |
| SPEC-4 完成定義 | STEP-2、STEP-13 |
| SPEC-5 議題文件組合與內容歸屬 | STEP-3 |
| SPEC-6 項目編號與追溯 | STEP-3 |
| SPEC-7 Review | STEP-4、STEP-13 |
| SPEC-8 Git 流程 | STEP-5 |
| SPEC-9 Release note 與 merge log | STEP-5、STEP-13 |
| SPEC-10 Coding Style 修訂 | STEP-8 |
| SPEC-11 檔案異動清單 | STEP-1～STEP-13 |
| SPEC-12 `collaboration.instructions.md` | STEP-6 |
| SPEC-13 `process/` 文件 | STEP-2～STEP-5 |
| SPEC-14 `project-overview.instructions.md` | STEP-7 |
| SPEC-15 入口文件 | STEP-6 |
| SPEC-16 Skills | STEP-9 |
| SPEC-17 個人設定收斂 | STEP-11 |
| SPEC-18 後續議題 | STEP-10 |
| SPEC-19 本議題自身的文件 | STEP-12、STEP-13 |

## 4. 待討論

無。確認點 5 的決議與對 SPEC、TEST 的回溯修改見 DEC-10；取消行數上限見 DEC-11；Claude 與 worktree 的做法獨立成檔見 DEC-14。
