# 規格

每條 SPEC 標示對應的 REQ（見 [Goal.md](Goal.md)）與依據的 DEC（見 [Discussion.md](Discussion.md)）。
第 1 節是團隊成員看得到的規則（操作規格），第 2 節說明規則放在哪個檔案、怎麼組織（實作規格），第 3 節是相容性異動。

## 1. 操作規格

### SPEC-1 規範分層、讀取方式與文件通則（REQ-1、REQ-2、REQ-3、REQ-5.5；DEC-2、DEC-9）

1. 規範分四層，各層只寫自己那層的內容：

   | 層 | 檔案 | 內容 | 載入方式 |
   | --- | --- | --- | --- |
   | 入口 | `.github/copilot-instructions.md`、`CLAUDE.md` | 規範如何分類、去哪裡讀 | 工具自動載入 |
   | 團隊通用 | `.github/instructions/collaboration.instructions.md` | 協作原則、文件通則、流程索引 | 常駐（`applyTo: "**"`） |
   | 團隊通用 | `.github/process/*.md` | 流程本文 | 依流程索引的時機讀取 |
   | 團隊通用 | `.github/instructions/coding-style-general.instructions.md` | Coding Style 通用規則 | 常駐（`applyTo: "**"`） |
   | 團隊通用 | `.github/instructions/coding-style-<語言>.instructions.md` | 各語言 Coding Style | 依流程索引讀取，Copilot 另依 `applyTo` |
   | MyCAM 專案 | `.github/instructions/project-overview.instructions.md` | 架構、慣例、專案固定檢查項目 | 常駐 |
   | MyCAM 專案 | `.github/project/*.md` | 建置、執行環境；Claude 與 worktree 工作方式 | 依專案指引的索引讀取 |
   | 個人 | `~/.claude/CLAUDE.md`、`CLAUDE.local.md` 等 | 個人偏好 | 各工具自行載入，不進版控 |

2. 團隊通用層不出現 MyCAM 專屬的檔名、專案名稱、建置指令；需要專案資訊時寫「見專案指引」。
3. 優先順序：MyCAM 專案層可補充或具體化團隊通用層；個人層只放偏好，不改變團隊規範的要求。
4. 人與 AI 讀同一份文件：Copilot 與 Claude Code 都從 `.github/` 讀取，`CLAUDE.md` 只以 `@` 引用，不複製內容。
5. 常駐的流程索引列出「時機 → 必讀文件」，成員或 AI 到了該時機就讀對應的 `process/` 文件：

   | 時機 | 必讀文件 |
   | --- | --- |
   | 開始議題、分級、進入下一階段 | `process/issue-workflow.md` |
   | 撰寫或修改議題文件 | `process/issue-docs.md` |
   | 執行或回應 review | `process/review.md`，加上專案指引的專案固定檢查項目；再依本表加讀本次修改涉及的文件 |
   | 開分支、commit、送 MR、產出 release note、建立 worktree | `process/git-workflow.md` |
   | 新增或修改程式碼 | 對應語言的 `instructions/coding-style-<語言>.instructions.md` |

6. 文件通則，適用所有規範指引與議題文件：
   1. 精簡：只寫讀者做對所需的資訊。不寫也會做對的不寫；同一規則只寫一處，其他地方以連結引用；機器能檢查的交給工具，文件只留一行指引；非直覺的規則附一句理由，容易誤解時才附範例。
   2. 只保留最終結果；正向表述，必要時才以反向表述強調；同類內容整合，不寫流水帳。
   3. 條列使用數字標號（巢狀也編號），表格除外。
   4. 內容使用繁體中文，檔名使用英文。
7. 常駐內容（每次互動都載入：入口文件與 `applyTo: "**"` 的指引）的行數是觀察指標，不設上限，避免為了數字刪減必要內容：
   1. 常駐總行數增加時，review 報告寫明增加量與必須常駐的理由，由人接受。
   2. 只在特定時機需要的內容（依副檔名適用的規範、建置與環境準備）不常駐，依索引讀取。

### SPEC-2 觸發入口（REQ-3.3、REQ-2.2；DEC-2.5）

1. 提供三個可直接呼叫的入口：

   | 入口 | 動作 | 依據文件 |
   | --- | --- | --- |
   | `/issue-start` | 盤點現況、建議分級、開分支、建立議題資料夾與 `README.md` | `process/issue-workflow.md`、`process/issue-docs.md`、`process/git-workflow.md` |
   | `/review` | 依六段執行 review 並產出報告 | `process/review.md` |
   | `/release-note` | 產出 release note 與 merge log | `process/git-workflow.md` |

2. 入口只負責觸發，規則一律寫在 `process/`；修改流程時只改 `process/`。
3. 沒有入口可用的工具（或成員不想用入口）時，直接說「開議題」、「review」、「產出 release note」，由常駐流程索引導向相同文件，結果相同。

### SPEC-3 議題分級、階段與確認點（REQ-4.1、REQ-4.3、REQ-4.4；DEC-3.1、DEC-8）

1. 處理階段：
   1. 盤點現況。
   2. 明訂需求與目標（REQ）；BUG 明訂問題現象與重現方式。
   3. 明訂規格（SPEC），含相容性異動；BUG 明訂修正後的預期行為。
   4. 明訂測試規劃（TEST），含現有測試的異動；BUG 須含重現測試。
   5. 明訂實作架構與步驟（STEP），含程式、測試、研發文件、專案指引。
   6. 施工：依 STEP 施工、BUG 先完成重現測試、執行測試、自我 review、獨立 review、提交。
   7. Review：與人合作 review、產出 release note 與 merge log、處理同事的 review 意見。
   8. 收官：全域自我 review，長期有效的結論提升到指引規範。
2. 分級與確認點：

   | 級別 | 判斷依據 | 確認點 |
   | --- | --- | --- |
   | S | 影響範圍明確、無規格與相容性異動的小修改（含小 bug） | 1–5 合併為「分析與規劃」、7 |
   | M | 一般功能、跨模組 bug | 1–2、3–4、5、7 |
   | L | 新功能、規範或相容性異動 | 1、2、3、4、5、7 各自確認 |

3. 分級由實作者確認（使用 AI 時由 AI 在盤點後提出建議）；合併者在 review 時認為級別不足，可要求升級並補齊文件。分級確認後依 SPEC-8 開分支，議題文件提交在該分支（DEC-12）。
4. 每個確認點先出草案，最後列「待討論」；人確認沒有待決問題後，才進入下一段。
5. 待討論每題固定三部分，文件與對話中都適用，不可省略為只有選項的摘要：
   1. 問題：背景、為什麼需要決定、要決定什麼。
   2. 建議：做法與理由。
   3. 另一可行選項：做法與取捨。
6. 施工中遇到預料外的狀況，先停下來討論，不自行擴大範圍。
7. 重新分級：發現範圍超出原級別時先停下提出；升級後補齊新級別要求的檔案與 DoD 項目，已完成的內容搬入對應檔案，不重做已確認的確認點。
8. （刪除，見 DEC-14）
9. 回溯：後段（含 review）發現前段內容要修改時：
   1. 原範圍內的修正：修改最早受影響的文件，依級別重做該段確認點，往下更新 TEST、STEP、程式與測試，review 從門檻段重來。
   2. 新需求或擴大範圍：列 `TODO`，以後續 MR 處理；必須在本 MR 處理時，走重新分級。
   3. 觸發回溯的 review 發現，處置寫「已修正（回溯至 SPEC-x）」；需求或決策的修改記入討論記錄。

### SPEC-4 完成定義（DoD）（REQ-4.2；DEC-3.1）

1. 每個議題的 `README.md` 放該級別的 DoD 勾選清單，全部勾選才送 MR。
2. S 級：
   1. `README.md` 寫明問題現象與重現方式、修正方式、測試結果。
   2. 重現測試修正前失敗、修正後通過；無法自動化時附人工步驟與結果。
   3. 門檻通過（建置與專案固定檢查項目）。
   4. Review 完成，每項發現都有處置。
   5. Release note 寫入 `README.md`。
3. M 級：
   1. 每個 REQ 有 SPEC、每個 SPEC 有 TEST。
   2. 相容性異動已寫明，沒有異動寫「無」。
   3. `TestPlan.md` 附最終執行結果。
   4. 每個 STEP 有對應的 commit（訊息標示 `(STEP-n)`）。
   5. 門檻通過（建置與專案固定檢查項目）。
   6. 獨立 review 報告寫入 `Review.md`，「必修」全部已修正。
   7. 範圍外項目寫入 `README.md` 的後續處理，各附延後原因。
   8. Release note 寫入 `README.md`。
4. L 級：M 級全部項目，再加：
   1. 各確認點分別經人確認。
   2. `FollowUp.md` 每項寫明目標、完成條件與延後原因（取代 M 級第 7 項）。
   3. 長期有效的結論已提升到指引規範。

### SPEC-5 議題文件組合與內容歸屬（REQ-5.1、REQ-5.2、REQ-5.4；DEC-3）

1. 存放於 `Doc/<議題編號>/`；同一議題的後續 MR（`_2nd`、`_3rd`）沿用同一資料夾。
2. 尚未取得議題編號時以 `<前綴>-TBD` 暫代（前綴見專案指引，DEC-16），取得後一併更名資料夾與分支。
3. 檔名不加數字前綴；閱讀順序由 `README.md` 的文件索引決定。
4. 各級檔案組合與內容歸屬（「README」表示寫在 `README.md` 的對應章節）：

   | 內容 | S | M | L |
   | --- | --- | --- | --- |
   | 議題資訊、文件索引、DoD、Release Note | README | README | README |
   | 現況盤點、目標、範圍、REQ（BUG 為現象與重現方式） | README | README | `Goal.md` |
   | SPEC、相容性異動（S 為修正方式與預期行為） | README | `Spec.md` | `Spec.md` |
   | TEST 與執行結果 | README | `TestPlan.md` | `TestPlan.md` |
   | STEP | README | `Implementation.md` | `Implementation.md` |
   | Review 報告 | README | `Review.md` | `Review.md` |
   | 討論記錄（DEC） | README | `Discussion.md`（有決策時） | `Discussion.md` |
   | 後續處理（TODO） | README | README | `FollowUp.md` |

5. `README.md` 開頭為議題資訊表：分支、分支起點（commit 與版本）、級別、狀態。
6. 議題文件遵守 SPEC-1 第 6 項的文件通則。
7. 例外：`Discussion.md` 可增量描述，凸顯最終決策；`Review.md` 依 review 回合分節保留。
8. （刪除，見 DEC-14）
9. 收官時把議題結束後仍長期有效的結論（規則、慣例、架構說明）寫入指引規範，議題文件保留追溯用的記錄。

### SPEC-6 項目編號與追溯（REQ-5.3；DEC-3.7）

1. 前綴：

   | 前綴 | 意義 | 所在文件 |
   | --- | --- | --- |
   | `REQ` | 需求（BUG 為問題現象） | Goal / README |
   | `SPEC` | 規格 | Spec / README |
   | `TEST` | 測試項目 | TestPlan / README |
   | `STEP` | 實作步驟 | Implementation / README |
   | `TODO` | 後續處理 | FollowUp / README |
   | `DEC` | 討論決策 | Discussion / README |
   | `REV` | Review 發現 | Review / README |

2. 寫法為 `前綴-流水號`（`REQ-1`）；引用子項目寫 `REQ-1.2`。
3. 追溯方向：SPEC 註明 REQ；TEST 註明 SPEC（或 REQ）；STEP 註明 SPEC 與 TEST；REV 的處置可指向 TODO。
4. 確認點確認後，編號不重排、不重用；刪除的項目直接移除，編號跳號；有序清單的條列保留一行「（刪除，見 DEC-n）」，因為 Markdown 會依序重排清單號碼（DEC-17）。

### SPEC-7 Review（REQ-6；DEC-4）

1. 六段依序進行，前一段的「必修」未解決前，不進入耗費人力的後段：

   | 段 | 內容 | AI | 人 |
   | --- | --- | --- | --- |
   | 1 門檻 | 建置、檢查腳本、測試、專案固定檢查項目；動到自動載入的檔案時計算行數並與比對基準比較（SPEC-1 第 7 項） | 執行並回報結果 | 提供 AI 無法執行的環境 |
   | 2 需求與範圍 | 每個 REQ / SPEC 都有實作與測試，沒有範圍外的修改 | 追溯比對 | 判斷需求是否被誤解 |
   | 3 功能正確性 | 邊界、錯誤處理、空值、資源釋放、執行緒；專案指引的專案固定檢查項目 | 靜態分析 | 實機操作驗證 |
   | 4 規範符合 | 依流程索引列出本次修改涉及的規範（如修改 C# 檔對應 C# Coding Style），逐一比對；專案慣例 | 檢查 | 確認處置 |
   | 5 設計品質 | 架構、重複、效率、可維護性 | 提出 | 取捨 |
   | 6 文件 | 議題文件與 DoD、指引規範、使用手冊是否受影響；是否符合文件通則（重複、無法判斷、可交給工具的規則） | 檢查 | 確認 |

   流程索引是規範的唯一登記處：新增規範只在索引加一列，review 自動涵蓋，不需修改本表（DEC-15）。

2. 規範只定 review 的內容與報告格式，不限定工具，也不限定由誰執行；每項發現的處置由人決定或確認。
3. 報告格式：
   1. 表頭：議題、分支、比對基準 commit、工具與模型、日期。
   2. 發現表格欄位：`REV-n`、段（1～6）、嚴重度（必修 / 建議 / 提問）、位置（`檔案:行號`）、說明、處置。
   3. 處置只有三種：已修正、不修正（附理由）、列後續（`TODO-n`）；修正的 commit 訊息標示 `(REV-n)`。
4. 處置規則：
   1. 「必修」與涉及需求、規格的發現，先由人決定處置再修；其他發現可先修正，處置寫「已修正（待人確認）」，人確認後移除標示（DEC-13）。
   2. 「必修」只能是「已修正」；若人判定不需修正，先調整嚴重度並寫明理由。
   3. 「提問」須有回覆，回覆後依結論改為其他嚴重度或結案。
   4. 送 MR 前，報告中不留未處置或待確認的發現。
5. 修正後回到門檻段重新確認，再進入下一回合；需要修改需求或規格時依 SPEC-3 第 9 項回溯。

### SPEC-8 Git 流程（REQ-7.1～REQ-7.3；DEC-5）

1. 版本：
   1. 版本號 `0.<中>.<小>`；中版號是主要迭代，每版開版本分支 `0.<中>.0`；小版號只用於 hotfix。
   2. 版本完成時在版本分支提交 `RLS_0.<中>.<小>`。
2. 合併：議題分支以 MR squash 合併進目標版本分支；合併後保留議題分支，分支內的 commit 供追溯。
3. 分支命名：
   1. `<議題編號>_<類型>_<簡述>`；議題編號含前綴，格式與範例見專案指引（DEC-16）；類型 `f` 為功能、`b` 為 bug；簡述為英文 PascalCase。
   2. 同一議題合併後的後續 MR：原名加 `_2nd`、`_3rd`…。
   3. 並存比較的另一種實作：原名加 `_Alt2`、`_Alt3`…。
   4. 送 MR 前的自測回合不開新分支，在原分支以 commit 推進。
   5. Rebase 前的暫時 squash 分支只留在本機，不 push，用完刪除。
4. 開分支前先 fetch 遠端；fetch 失敗時以本機舊的遠端分支為基準有風險，須先確認。
5. Commit：
   1. 每個 STEP 一個 commit；文件、測試、程式盡量分開。
   2. 訊息明確描述修改內容，中英文皆可。
   3. 訊息結尾標示修改的來由：施工 `(STEP-n)`、review 修正 `(REV-n)`，兩者皆是時並列（`(STEP-3, REV-2)`）；一個 STEP 可對應多個 commit。確認點文件（階段 1～5）的 commit 不標示。
   4. `Implementation.md` 不記 commit hash，以 `git log --grep "STEP-n"` 查詢對應。
   5. 純格式調整（含 SPEC-10 的方法改寫）獨立 commit。
6. 送 MR 由人決定；AI push 前先詢問 remote 與分支（push 由人決定寫在協作原則，SPEC-12.2.1）。
7. 一個議題一個 branch。Worktree 為選用，同時處理多個議題時使用；未追蹤檔案不會出現在新 worktree，需要的檔案依專案指引複製（DEC-12）。

### SPEC-9 Release note 與 merge log（REQ-7.4；DEC-5.3）

1. Release note：
   1. 讀者為團隊與 QA，直接描述做了什麼處理，不寫模組名稱。
   2. 數字條列，每條以 `新增`、`修改` 或 `刪除` 開頭，前綴與內容間空一格，不加冒號。
   3. 由 AI 產出以統一風格，寫入議題 `README.md` 的 Release Note 章節。
2. Merge log（MR 描述）：

   ```text
   Merge branch '<分支>' into '<目標版本>'

   by <名字>

   1. 新增 ...
   2. 修改 ...
   ```

   1. 條列沿用 release note。
   2. AI 產出時 `<目標版本>` 留空，由合併者在合併時填入。
   3. Merge log 由 release note 組合而成，不另存於議題文件。

### SPEC-10 Coding Style 修訂（REQ-8；DEC-6）

1. 修改既有程式碼（取代現行「維持該方法內既有的命名方式」）：
   1. 新增與修改的程式碼採新規範。
   2. 同一方法新舊混用到難以閱讀時，可將整個方法改為新規範；格式調整獨立 commit。
   3. 不修改本次沒有動到的方法。
2. 註解新增：Production Code 不引用文件、測試或規範條目（如 `REQ-1`、`TEST-3`、文件路徑）；難以從程式碼解釋的特例，最多留 `// <議題編號>: <原因>`（DEC-16）。

## 2. 實作規格

### SPEC-11 檔案異動清單（REQ-1、REQ-2、REQ-3）

| 檔案 | 異動 | 依據 |
| --- | --- | --- |
| `.github/instructions/collaboration.instructions.md` | 新增 | SPEC-12 |
| `.github/process/issue-workflow.md` | 新增 | SPEC-13 |
| `.github/process/issue-docs.md` | 新增 | SPEC-13 |
| `.github/process/review.md` | 新增 | SPEC-13 |
| `.github/process/git-workflow.md` | 新增 | SPEC-13 |
| `.github/instructions/project-overview.instructions.md` | 修改 | SPEC-14 |
| `.github/project/build.md` | 新增 | SPEC-14 |
| `.github/project/claude-worktree.md` | 新增 | SPEC-14.6 |
| `.github/instructions/coding-style-general.instructions.md` | 修改 | SPEC-10 |
| `.github/copilot-instructions.md` | 修改 | SPEC-15 |
| `CLAUDE.md` | 修改 | SPEC-15 |
| `.gitignore` | 修改：加入 `/SESSION.md`，防止個人交接檔誤提交 | DEC-9.2、DEC-14 |
| `.editorconfig` | 修改：加入 `[*.ps1] charset = utf-8-bom` | DEC-10.3 |
| `.claude/skills/issue-start/SKILL.md` | 新增 | SPEC-16 |
| `.claude/skills/review/SKILL.md` | 新增 | SPEC-16 |
| `.claude/skills/release-note/SKILL.md` | 新增 | SPEC-16 |
| `Doc/AUTO-TBD/*` | 新增 / 修改 | SPEC-19 |
| 個人設定（不進版控） | 修改 / 刪除 | SPEC-17 |

1. 所有規範文件遵守 SPEC-1 第 6 項；檔案格式依 `.editorconfig`。
2. 本議題新增與修改的規範檔改為數字條列；未修改的檔案（如 `coding-style-csharp`、`coding-style-cpp`）不動，日後修改時再改。
3. `process/`、`project/` 不放 `applyTo` frontmatter，也不放在 `instructions/` 下，避免被工具自動常駐。

### SPEC-12 `collaboration.instructions.md`（REQ-3.1、REQ-3.2、REQ-11；DEC-2.3、DEC-2.4、DEC-13）

1. Frontmatter `applyTo: "**"`。
2. 章節：
   1. 協作原則：AI 以資深工程師角度提出想法與建議，有疑慮直接指出並附替代方案；依重要性決定先問或先做（DEC-13）：需求、規格、範圍、相容性與規範規則的變更，以及分級、確認點定案、合併、push，先由人決定再執行；措辭、格式、補漏、明顯錯誤可先處理，在該階段結束時列清單請人確認；無法判斷時先由人決定。
   2. 文件通則：SPEC-1 第 6 項。
   3. 流程索引：SPEC-1 第 5 項的「時機 → 必讀文件」表格，路徑寫完整（`.github/process/...`）。
3. 只放每次互動都適用的規則；行數依 SPEC-1 第 7 項檢查。

### SPEC-13 `process/` 文件（REQ-4～REQ-7）

1. `issue-workflow.md`：SPEC-3（分級、階段、確認點、重新分級、回溯）與 SPEC-4（各級 DoD）。
2. `issue-docs.md`：SPEC-5、SPEC-6，另含：
   1. 各文件的內容規範（Goal、Spec、TestPlan、Implementation、Review、Discussion、FollowUp）。
   2. 精簡範本：S 級 `README.md` 完整範本；M、L 的 `README.md` 骨架與各檔章節骨架。
3. `review.md`：SPEC-7；專案固定檢查項目只寫「見專案指引」，不列內容。
4. `git-workflow.md`：SPEC-8、SPEC-9；worktree 只寫通用規則（SPEC-8 第 7 項），建立與準備 worktree 的做法寫「見專案指引」（SPEC-14.6，DEC-14）。
5. 各文件開頭一句說明適用時機，內容不重複；需要其他文件的規則時以連結引用。

### SPEC-14 `project-overview.instructions.md`（REQ-1.2；DEC-2.6）

1. 刪除「Git 流程」章節：分支命名、commit 規則、Claude Code worktree 的通用說明移到 `process/git-workflow.md`。
2. 「建置與執行」的內容（FCLTool、命令列建置、執行期檔案）移到 `.github/project/build.md`；與 worktree 相關的內容不放在此檔（SPEC-14.6）。
3. `project-overview` 的「建置與執行」只留索引：建置、執行環境時讀 `.github/project/build.md`；切換 session、使用 worktree 或 Claude Code 時讀 `.github/project/claude-worktree.md`。
4. 新增「專案固定檢查項目」章節，作為 review 門檻段、功能正確性段與規範段的專案項目：
   1. `Build.bat Debug` 建置成功。
   2. 使用者可見文字的四語系 XML（`zh-TW`、`zh-CN`、`en-US`、`ru-RU`）同步。
   3. 修改過的 `*.Designer.cs` 仍可在 Designer 開啟。
   4. 未修改 `reference/` 內的檔案（除非議題明確要求）。
   5. 功能正確性的領域項目：UI 執行緒、角度單位（rad / deg）、座標系（由團隊通用的 review 移入）。
5. 第 4 項的規則只寫在「專案固定檢查項目」：「慣例」與「技術棧」中重複的句子（四語系同步、Designer 可開、不修改 `reference/`）刪除，保留做法說明。
6. 新增 `.github/project/claude-worktree.md`，集中使用 Claude 或 worktree 時才需要的做法，不使用的成員不需讀（DEC-14）：
   1. Session 交接：只在確認點切換 session，切換前更新工作目錄根目錄的 `SESSION.md`（`.gitignore` 排除，只放交接 prompt 與暫時筆記）；主 session 可開子 session 做驗證或獨立 review 再整合。
   2. Worktree：Claude Code 的 worktree 位置與分支更名；`.worktreeinclude` 列出需複製的未追蹤檔案（`FCLTool.dll`、`PostTool.dll`、`Custom/`、`.claude/settings.local.json`），Claude Code 自動複製、其他工具手動複製；`CLAUDE.local.md` 由上層載入；第一次建置後執行 `CopyRef.bat`；開啟方案後再卸載 `FCLTool`。
7. 「慣例」新增「議題編號」：格式 `AUTO-<數字>`、未取得時以 `AUTO-TBD` 暫代、分支範例；團隊通用層只寫 `<議題編號>` 並指向專案指引（DEC-16）。

### SPEC-15 入口文件（REQ-1.3、REQ-2.1；DEC-2.1、DEC-2.2）

1. `copilot-instructions.md`：
   1. 說明四層分類與載入方式（SPEC-1 第 1 項的精簡版）。
   2. 指引目錄加入 `collaboration.instructions.md`、`process/` 四份文件、`project/build.md` 與 `project/claude-worktree.md`，每份標示「常駐」或「依時機讀取」（各語言 Coding Style 標示依流程索引讀取，Copilot 另依 `applyTo`）。
   3. 刪除「本檔只放目錄與跨主題的總則」中的「總則」說法，改為「本檔只放目錄與分類說明」。
   4. 「維護方式」加入常駐內容的原則（SPEC-1 第 7 項）。
2. `CLAUDE.md`：`@` 只引用常駐的檔案（`copilot-instructions.md`、`collaboration`、`project-overview`、`coding-style-general`）；`coding-style-csharp`、`coding-style-cpp`、`process/`、`project/` 不以 `@` 引用（`@` 會常駐載入），由流程索引導向。

### SPEC-16 Skills（REQ-3.3；DEC-2.5）

1. 位置 `.claude/skills/<名稱>/SKILL.md`，名稱為 `issue-start`、`review`、`release-note`。
2. 內容只有 frontmatter（`name`、`description`）與一段本文：「依 `.github/process/<文件>.md` 執行」，並列出該動作的必讀文件（SPEC-2 表格）。
3. `description` 寫明觸發情境，讓工具能自動判斷時機。
4. VS 2026 的 Copilot 能否讀取 `.claude/skills/` 由測試確認；讀不到時以 SPEC-2 第 3 項的常駐索引作為替代。

### SPEC-17 個人設定收斂（REQ-9；DEC-2.7）

1. 刪除 `~/.claude/issue-workflow.md`；`~/.claude/CLAUDE.md` 移除「開發議題處理流程」章節與 `@` 引用。
2. `~/.claude/CLAUDE.md` 保留個人偏好：溝通語言、記憶規則、本機環境；「文件條列使用數字標號」已成為團隊通則，刪除。
3. 主 checkout 的 `CLAUDE.local.md`：
   1. 刪除已成為團隊規範的項目（議題資料夾位置、push 前詢問）。
   2. 保留個人偏好（議題編號格式提問、commit 訊息使用繁體中文、建置習慣、FCLTool 與 worktree 提醒）。
   3. 新增 remote 用法：`origin` 為公司 GitLab（正式流程、MR）；`github` 用於遠端協作，push 前仍先詢問。
4. 這些檔案不進版控，施工後在議題文件記錄已完成。

### SPEC-18 後續議題（REQ-10；DEC-7）

1. `FollowUp.md` 每項含：目標、完成條件、延後原因、建議做法（若有）；內容自足，不需回溯本議題討論。
2. 至少包含：
   1. 使用手冊維護機制。
   2. 自動化品質門檻，分三階段：本機檢查腳本 → CI → 測試專案；每建立一項自動檢查，就把對應的文字規則縮為一行指引。
   3. GitLab MR 建立自動化（push options，或 `glab` CLI / GitLab API）。
   4. 既有分支 `AUTO-23254_f_UIStyleUnify` 依新規範調整。
   5. VS 2026 Copilot 讀取 skill 的結果若為讀不到，評估改用 Copilot 的入口機制。
   6. Coding Style 與 `.editorconfig` 重複的格式規則縮減為一行指引，並評估 Claude 依副檔名自動載入各語言 Coding Style 的機制。

### SPEC-19 本議題自身的文件（REQ-5；DEC-1.1）

1. 本議題依新規範的 L 級產出：`README`、`Goal`、`Spec`、`TestPlan`、`Implementation`、`Review`、`Discussion`、`FollowUp`。
2. `README.md` 補上 L 級 DoD、文件索引與 Release Note。
3. `Review.md` 依 SPEC-7 格式產出，作為第一份範例。

## 3. 相容性異動

### 3.1 輸入

1. 既有議題文件（`Doc/<議題編號>/`，舊結構與 `R` / `S` / `T` 編號）：相容，不轉換；後續 MR 沿用舊資料夾時，新增內容採新規範。
2. 既有分支名稱（`(2nd)`、`_V2`、`_merge` 等）：相容，不更名；新分支依 SPEC-8。
3. 既有程式碼的 Coding Style：相容；只在修改時依 SPEC-10 處理。
4. 個人設定：只影響使用者本人；其他成員原本沒有流程設定，不需轉換。

### 3.2 輸出

1. Merge log：格式與現行一致，差別是 AI 產出時目標版本留空，由合併者填入。
2. 常駐內容：新增 `collaboration.instructions.md`，`project-overview` 移出 Git 流程與建置細節，Claude 不再常駐各語言 Coding Style；常駐總行數與施工前（193 行）的差異依 SPEC-1 第 7 項在 review 說明。
3. Production Code、建置產物、NC 輸出：無。

## 4. 需求對照

| REQ | SPEC |
| --- | --- |
| REQ-1 規範分層 | SPEC-1、SPEC-11、SPEC-14、SPEC-15 |
| REQ-2 工具共用 | SPEC-1、SPEC-2、SPEC-15、SPEC-16 |
| REQ-3 載入時機 | SPEC-1、SPEC-2、SPEC-12、SPEC-16 |
| REQ-4 議題分級與完成定義 | SPEC-3、SPEC-4、SPEC-7、SPEC-13 |
| REQ-5 議題文件 | SPEC-1、SPEC-5、SPEC-6、SPEC-8、SPEC-13、SPEC-19 |
| REQ-6 Review | SPEC-7、SPEC-13、SPEC-14 |
| REQ-7 Git 流程 | SPEC-8、SPEC-9、SPEC-13 |
| REQ-8 Coding Style 修訂 | SPEC-10 |
| REQ-9 個人設定收斂 | SPEC-17 |
| REQ-10 後續議題 | SPEC-18 |
| REQ-11 協作原則 | SPEC-12 |

## 5. 待討論

無。確認點 3 的決議見 DEC-9；確認點 5 回溯修改見 DEC-10；施工中取消行數上限見 DEC-11；worktree 改為選用、開議題時開分支見 DEC-12；先做後確認見 DEC-13；Claude 與 worktree 的做法獨立成檔見 DEC-14；review 規範段以流程索引為登記處見 DEC-15；議題編號前綴移到專案指引見 DEC-16；刪除的條列保留編號見 DEC-17。
