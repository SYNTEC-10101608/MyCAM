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
| TEST-1 | 自動載入行數 | SPEC-1.7、SPEC-12.3、相容性 3.2.2 | 長期維護 | 靜態 | 自動 |
| TEST-2 | 團隊通用層不含專案內容 | SPEC-1.2、SPEC-13.3、SPEC-13.4 | 議題限定 | 靜態 | 自動 |
| TEST-3 | 規則只寫一處 | SPEC-1.6.1、SPEC-2.2、SPEC-13.5、SPEC-16.2 | 議題限定 | 靜態 | 自動 |
| TEST-4 | 載入設定與檔案位置 | SPEC-1.1、SPEC-1.4、SPEC-5.8、SPEC-11.3、SPEC-12.1、SPEC-15 | 議題限定 | 靜態 | 自動 |
| TEST-5 | 異動範圍符合清單 | SPEC-11、相容性 3.2.3 | 議題限定 | 靜態 | 自動 |
| TEST-6 | 格式與數字條列 | SPEC-1.6.3、SPEC-1.6.4、SPEC-11.1、SPEC-11.2 | 議題限定 | 靜態 | 自動 |
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

#### TEST-1 自動載入行數

1. 方法：計算 `.github/copilot-instructions.md`、`CLAUDE.md`、`.github/instructions/*.md` 的行數，與比對基準比較。
2. 通過條件：
   1. 總行數不超過 359 行（基準 `24c7276d`：入口 32 行、`instructions/` 327 行）。
   2. `collaboration.instructions.md` 不超過 60 行。
3. 長期維護：即 review 門檻段的行數檢查（SPEC-7.1）；總行數增加時依 SPEC-1.7.2 寫明理由。

#### TEST-2 團隊通用層不含專案內容

1. 方法：在 `collaboration.instructions.md` 與 `.github/process/*.md` 搜尋專案專屬字詞：`MyCAM`、`Build.bat`、`CopyRef`、`FCLTool`、`PostTool`、`OCC`、`LanguageManager`、`Designer`、`reference/`、`.worktreeinclude`、`CustPost`、`zh-TW`。
2. 通過條件：零筆；需要專案資訊處寫「見專案指引」。
3. 本檢查列入 `FollowUp.md` 自動化門檻的候選項目（DEC-4.5）。

#### TEST-3 規則只寫一處

1. 方法：以各規則的特徵字串搜尋 `.github/`、`CLAUDE.md`、`.claude/skills/`，例如分級表（`1–5 合併`）、嚴重度（`必修 / 建議 / 提問`）、分支後綴（`_Alt2`）、release note 前綴（`前綴與內容間空一格`）、DoD 標題。
2. 通過條件：
   1. 每個特徵字串只出現在一個規範檔，其他檔案以連結引用。
   2. `SKILL.md` 本文只有指向 `process/` 的說明與必讀文件清單，沒有規則內容。

#### TEST-4 載入設定與檔案位置

1. 方法：檢查 frontmatter、`@` 引用、`.gitignore` 與 `git check-ignore`。
2. 通過條件：
   1. `collaboration.instructions.md` 的 frontmatter 為 `applyTo: "**"`。
   2. `.github/process/` 四份文件沒有 `applyTo` frontmatter，且不在 `instructions/` 下。
   3. `CLAUDE.md` 以 `@` 引用 `collaboration.instructions.md`，沒有引用 `process/`。
   4. `copilot-instructions.md` 的指引目錄列出 `collaboration` 與 `process/` 四份文件，並標示「常駐」或「依時機讀取」。
   5. `git check-ignore SESSION.md` 回報由 `.gitignore` 的 `/SESSION.md` 排除。

#### TEST-5 異動範圍符合清單

1. 方法：`git diff --name-status 24c7276d HEAD`。
2. 通過條件：異動檔案與 SPEC-11 的清單一致；沒有 Production Code、`reference/`、建置設定的異動。

#### TEST-6 格式與數字條列

1. 方法：檢查本議題新增與修改的規範檔及 `Doc/AUTO-TBD/`。
2. 通過條件：
   1. 編碼與換行符合 `.editorconfig`。
   2. 程式碼區塊以外沒有無序號條列（`- `、`* ` 開頭）。
   3. 檔名為英文，內容為繁體中文。

#### TEST-7 連結與引用有效

1. 方法：擷取 `.github/`、`CLAUDE.md`、`.claude/skills/`、`Doc/AUTO-TBD/` 中的相對連結、`@` 路徑與反引號內的 `.github/...` 路徑，逐一確認檔案存在。
2. 通過條件：全部存在，大小寫與實際檔名一致。

#### TEST-8 規格內容落地

1. 方法：AI 依 SPEC-13 的對應，逐條列出 SPEC-3～SPEC-9 每個子項目在 `process/` 文件中的位置（`檔案:行號`），人抽查。
2. 通過條件：
   1. 每個子項目都有位置，內容與 SPEC 一致。
   2. `issue-docs.md` 含各文件內容規範與精簡範本（S 級完整範本；M、L 骨架）。
   3. `review.md` 的專案固定檢查項目只寫「見專案指引」。

#### TEST-9 專案指引與 Coding Style 修訂

1. 方法：搜尋 `project-overview.instructions.md` 與 `coding-style-general.instructions.md`。
2. 通過條件：
   1. `project-overview` 沒有「Git 流程」章節；有「worktree 準備」與「專案固定檢查項目」（四項，SPEC-14.3）。
   2. `coding-style-general` 不再有「維持該方法內既有的命名方式」；有 SPEC-10.1 三項與 SPEC-10.2 註解規則。

#### TEST-10 文件精簡審查

1. 方法：review 文件段，AI 依 SPEC-1.6 列出重複、讀者無法判斷、可交給工具的內容，人決定處置。
2. 通過條件：發現全部有處置，記入 `Review.md`。
3. 長期維護：即 review 文件段（SPEC-7.1）。

#### TEST-11 本議題文件與 commit

1. 方法：檢查 `Doc/AUTO-TBD/` 檔案組合、`README.md`、`Review.md` 與 `git log 24c7276d..HEAD`。
2. 通過條件：
   1. 有 L 級八份文件；`README.md` 含文件索引、L 級 DoD（全部勾選）、Release Note。
   2. `Review.md` 表頭五欄、發現表格六欄；每項有處置，「必修」全部為「已修正」。
   3. 施工 commit 訊息結尾有 `(STEP-n)` 或 `(REV-n)`，每個 STEP 至少一個 commit；純格式調整獨立 commit。

#### TEST-12 個人設定收斂

1. 方法：檢查使用者家目錄與主 checkout 的個人設定檔。
2. 通過條件：
   1. `~/.claude/issue-workflow.md` 不存在；`~/.claude/CLAUDE.md` 沒有「開發議題處理流程」與對它的 `@` 引用，也沒有「條列使用數字標號」。
   2. `CLAUDE.local.md` 沒有議題資料夾位置與 push 前詢問；有 `origin`、`github` 的 remote 用法。
3. 結果記入 `README.md`（SPEC-17.4）。

#### TEST-13 後續議題完整

1. 方法：人閱讀 `FollowUp.md`。
2. 通過條件：
   1. 含 SPEC-18.2 五項；每項有目標、完成條件、延後原因，不需讀本議題其他文件就能開工。
   2. 自動化門檻項目列出 TEST-2 的檢查作為候選。

### 3.2 動態驗證

TEST-14～TEST-16 在 TEST-12 完成後執行，確認只靠 repo 的規範就能完整遵循流程（REQ-2.2）。

#### TEST-14 Claude Code 載入範圍

1. 方法：在 worktree 開新的 Claude Code session，以 `/context` 查看載入的記憶檔。
2. 通過條件：載入 `CLAUDE.md`、`copilot-instructions.md`、`collaboration.instructions.md`；沒有載入 `process/` 文件。

#### TEST-15 Claude Code 入口與流程演練

1. 方法：
   1. 新 session 中以一個假設的 S 級 BUG 執行 `/issue-start`，產出放在 scratchpad，不提交。
   2. 在本分支執行 `/release-note`；執行 `/review`，確認依六段進行並產出 SPEC-7.3 格式的報告。
   3. 另開新 session，只輸入「開議題」。
2. 通過條件：
   1. 三個入口都出現在 skill 清單，執行後讀取 SPEC-2 表格列出的必讀文件。
   2. `/issue-start` 產出的 `README.md` 符合 S 級範本：議題資訊表、各節、DoD 五項，編號使用 SPEC-6 的前綴；AI 提出分級建議後停下等人確認（SPEC-3.3、SPEC-3.4）。
   3. Release note 每條以 `新增` / `修改` / `刪除` 開頭；merge log 符合 SPEC-9.2 格式，目標版本留空。
   4. 只輸入「開議題」時，經 skill 或流程索引讀取 `issue-workflow.md` 與 `issue-docs.md`。
3. 演練產出不提交，檢查結果摘要寫入執行結果。

#### TEST-16 VS 2026 Copilot 讀取與觸發

1. 執行者：使用者；AI 在施工完成後提供逐步清單並記錄結果。
2. 方法：VS 2026（18 版以上）開啟方案，在 Copilot Chat：
   1. 開啟任一 `.cs` 檔提問，查看回應引用的指引。
   2. 輸入「開議題」，查看是否讀取 `.github/process/issue-workflow.md`。
   3. 輸入 `/issue-start`，確認是否辨識 `.claude/skills/`。
3. 通過條件：
   1. 第 1、2 項：引用 `copilot-instructions.md` 與 `collaboration.instructions.md`，並依索引讀取 `process/`。
   2. 第 3 項只記錄結果，不判定失敗；讀不到時確認 `FollowUp.md` 有 SPEC-18.2.5 項目。

## 4. SPEC 對照

| SPEC | TEST |
| --- | --- |
| SPEC-1 規範分層、讀取方式與文件通則 | TEST-1、TEST-2、TEST-3、TEST-4、TEST-6、TEST-7、TEST-10、TEST-14、TEST-16 |
| SPEC-2 觸發入口 | TEST-3、TEST-15、TEST-16 |
| SPEC-3 議題分級、階段與確認點 | TEST-8、TEST-15 |
| SPEC-4 完成定義 | TEST-8、TEST-11、TEST-15 |
| SPEC-5 議題文件組合與內容歸屬 | TEST-4、TEST-8、TEST-15 |
| SPEC-6 項目編號與追溯 | TEST-8、TEST-15 |
| SPEC-7 Review | TEST-8、TEST-11、TEST-15 |
| SPEC-8 Git 流程 | TEST-8、TEST-11 |
| SPEC-9 Release note 與 merge log | TEST-8、TEST-15 |
| SPEC-10 Coding Style 修訂 | TEST-9 |
| SPEC-11 檔案異動清單 | TEST-4、TEST-5、TEST-6 |
| SPEC-12 `collaboration.instructions.md` | TEST-1、TEST-4、TEST-7、TEST-14 |
| SPEC-13 `process/` 文件 | TEST-2、TEST-3、TEST-7、TEST-8、TEST-15 |
| SPEC-14 `project-overview.instructions.md` | TEST-9 |
| SPEC-15 入口文件 | TEST-4、TEST-14、TEST-16 |
| SPEC-16 Skills | TEST-3、TEST-7、TEST-15、TEST-16 |
| SPEC-17 個人設定收斂 | TEST-12 |
| SPEC-18 後續議題 | TEST-13 |
| SPEC-19 本議題自身的文件 | TEST-11 |

## 5. 執行結果

施工完成後填入。

## 6. 待討論

無。確認點 4 的決議見 DEC-4.5、DEC-7.1。
