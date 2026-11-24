# 討論記錄

依主題整理本議題的重要共識：原始考量、最終決策（粗體）與依據。日期為決議日。

## DEC-1 議題的進行方式（2026-11-24）

1. **本議題自己套用新規範**：文件結構、編號、review 報告都依新規範產出，作為第一個範例並驗證可行性。
2. **以使用者（主管）與 AI 的共識定稿**，不另行徵詢團隊；之後若有不同意見，另開下一階段議題收斂。
3. **規範文件一律使用繁體中文**；檔名與程式內容使用英文。

## DEC-2 規範分層與存放（2026-11-24）

1. 考量：全員都有 Copilot，只有部分成員使用 Claude；流程原本只存在個人設定。
   **團隊規範以 repo 的 `.github/` 為唯一來源**，Claude 以 `CLAUDE.md` 的 `@` 引用同一份。
2. **分為入口、團隊通用、MyCAM 專案、個人四層**：
   1. 入口（`copilot-instructions.md`、`CLAUDE.md`）只說明如何分類與讀取，不定義規範。
   2. 團隊通用（`collaboration.instructions.md`、`process/`）不含 MyCAM 專屬內容，可直接搬到團隊其他 repo。
   3. MyCAM 專屬內容只寫在 `project-overview` 及其子層，包含專案固定的 review 檢查項目。
3. 考量：人與 AI 是協作關係，應讀同一份文件。**不依讀者拆分，依載入時機拆分**：
   每次互動都適用的規則常駐；流程本文放在 `.github/process/`，由常駐索引指出「時機 → 必讀文件」。
4. 考量：不常駐就可能被遺漏。**對策分三層**：常駐索引寫明時機；真正的門檻放在 `README.md` 的 DoD 勾選清單；之後由自動檢查補上機器層門檻。
5. 考量：skill 不是不常駐的必要條件，價值在於可直接呼叫與自動判斷。
   **只替開議題、review、release note 三個動作建立 skill**，內容只指向 `process/`，不複製本文。
   Visual Studio 統一使用 2026（18 版以上）；是否讀取 `.claude/skills/` 須實測，讀不到時靠常駐索引。
6. `project-overview` 的 Git 流程只在開分支與送 MR 時用到，**移到 `process/git-workflow.md`**，不常駐。
7. 兩個 remote 是常態（公司正式流程走 GitLab，GitHub 方便遠端協作），**不寫進團隊規範**，個人用法寫在 `CLAUDE.local.md`。

## DEC-3 議題分級與文件（2026-11-24）

1. 考量：L 級議題產出大量 commit 與文件是正常的，但小議題需要精簡的最低產出。**分級決定最低必要產出，並各訂 DoD**，S 級約 5 項。
2. **檔名不加數字前綴**：各級檔案組合不同，數字會跳號；順序由 `README.md` 的索引決定。
3. **各級檔案組合**：S 只有 `README.md`；M 為 `README`、`Spec`、`TestPlan`、`Implementation`、`Review`（有決策時加 `Discussion`）；L 再加 `Goal`、`FollowUp`。
   M 也獨立 `Implementation.md`，因為 STEP 與 commit 的對應同樣需要追溯。
4. 「沒有獨立檔」不等於「不用寫」：S 的目標、修正方式、測試、review、後續、決策寫在 `README.md` 各節；M 的盤點、目標、REQ、後續寫在 `README.md`。
5. **後續處理任何級別都可能有**，因為它的定義就是範圍外的項目；S、M 寫在 `README.md`，L 獨立成檔。
6. **`SESSION.md` 保留但只放交接 prompt 與暫時筆記**；狀態與決策寫在提交的文件，不重複（由 DEC-14 移到 `.github/project/claude-worktree.md`）。
7. 考量：單一字母編號（`R`）無法分辨是需求還是 review。**改用 `REQ`、`SPEC`、`TEST`、`STEP`、`TODO`、`DEC`、`REV`**，寫法 `REQ-1`。
8. **不納入 GitLab MR 範本**：MR 描述會成為 merge log，勾選清單會混入主線歷史；DoD 留在 `README.md`。

## DEC-4 Review（2026-11-24）

1. 現行順序為功能正確、符合規範、測試通過、效率與架構。**最前面加上自動門檻**（建置、腳本、測試），不過門檻就不花人的時間。
2. **六段**：門檻 → 需求與範圍 → 功能正確性 → 規範符合 → 設計品質 → 文件。
   AI 負責可量化的檢查；人負責實機操作、需求是否被誤解、設計取捨，以及決定每項發現的處置。
3. 考量：成員使用 Claude 或 VS Copilot 不同工具。**不限定工具，統一報告格式**，每項發現都要有處置。
4. 合併者為資深工程師或主管，reviewer 不一定是合併者；規範只定 review 內容，不限定由誰執行。
5. 考量：把個別規範（如「團隊通用層不含專案內容」）寫進 review 各段，規範每多一條，review 就跟著膨脹。
   **Review 各段只寫檢查方向，不逐條列規範**；機械式可檢查的規則列入後續的自動化門檻候選。

## DEC-5 Git 流程（2026-11-24）

1. 分支合併後保留在 repo，分支內的 commit 可追溯，**「每個實作步驟一個 commit」維持**。
2. 分支後綴的原意：`_2nd` 為合併後的後續 MR；`V2` 為送 MR 前的自測回合；`_merge`、`_Rebase` 為 rebase 前的暫時 squash。
   **後續 MR 統一 `_2nd`、`_3rd`**；**自測回合不開分支**，以 commit 推進；並存比較的實作用 `_Alt2`；**rebase 暫存分支只在本機，不 push**。
3. **Release note 與 merge log 分開**：release note 只有 `新增` / `修改` / `刪除` 條列，由 AI 產出以統一風格；
   merge log 另含分支、目標版本與實作人，AI 依格式提供，**目標版本留空**，由合併者在合併時決定。

## DEC-6 Coding Style 修訂（2026-11-24）

1. 考量：早期規範不完善，既有程式碼有大量不符合規範之處。**採童子軍法則，以方法為單位**：新增與修改的程式碼採新規範；同一方法新舊混用到難以閱讀時，可將該方法改為新規範，格式調整獨立 commit。
2. 考量：交付的只有 Production Code，文件與測試日後可能失去同步。**Production Code 不引用文件、測試或規範條目**；難解釋的特例最多留 `// AUTO-<編號>: <原因>`。

## DEC-7 範圍外（2026-11-24）

1. **CI、測試專案與長期維護的檢查腳本不在本議題實作**，列後續並寫明目標與完成條件；本議題以成文化現有機制為主。
   議題範疇內驗證需要的腳本屬於本議題：review 每輪修正後都要重跑門檻段，**議題限定的檢查以 PowerShell 腳本 `Doc/AUTO-TBD/Check.ps1` 執行**，隨議題保留；團隊電腦都有 PowerShell，Python 未必在 PATH。
2. **GitLab MR 的建立自動化列後續**：可行方式為 `git push` 的 push options（不需額外環境）或 `glab` CLI / GitLab API（需安裝工具與 access token）。
3. 使用手冊維護、既有分支（`AUTO-23254_f_UIStyleUnify`）依新規範調整，列後續；`ArchInventory-Doc/` 是殘留檔，不處理。

## DEC-8 確認點的待討論格式（2026-11-24）

1. 考量：規格草案的待討論只列了選項，沒有說明問題的背景與要決定什麼，看不出在討論什麼，也無從判斷。
2. **每題固定三部分**：問題（背景、為什麼需要決定、要決定什麼）、建議（做法與理由）、另一可行選項（做法與取捨）。
3. **文件與對話中都適用**；在對話中呈現時不得省略為只有選項的摘要。

## DEC-9 規格確認點的決議（2026-11-24）

1. 考量：worktree 說明混有 MyCAM 專屬內容（DLL 清單、`CopyRef.bat`）。**拆分**：通用規則移到 `git-workflow.md`，專案準備步驟留在專案指引（位置由 DEC-10.1 改為 `project/build.md`，再由 DEC-14 改為 `project/claude-worktree.md`）。
2. 考量：`.git/info/exclude` 不進版控，其他成員會誤提交。**`SESSION.md` 改由 `.gitignore` 的 `/SESSION.md` 排除**。
3. 考量：commit hash 要 commit 後才知道，記在文件需要事後補，rebase 後也會變。
   **commit 訊息結尾標示 `(STEP-n)` 或 `(REV-n)`**，`Implementation.md` 不記 hash；一個 STEP 可對應多個 commit。
4. 考量：編號被其他文件與 commit 訊息引用，commit 訊息無法再改。**確認點確認後編號不重排、不重用，刪除即跳號**。
5. **文件條列使用數字標號提升為團隊通則**，理由是溝通時可直接指出第幾項；只在新增或修改的檔案套用。
6. 考量：自動載入的內容每次互動都付成本，其他文件的成本來自重複與模糊。
   **所有規範與文件都須精簡，並明訂定義**（SPEC-1 第 6 項）；**自動載入的內容另設硬上限**（由 DEC-11 取消），review 門檻段計算行數，文件段檢查是否符合通則；機器能檢查的規則逐步移入工具。
7. **分級由實作者確認**（使用 AI 時由 AI 建議），合併者在 review 時可要求升級。
8. 考量：級別不變但後段發現前段要改時，沒有規則。**從最早受影響的段回溯，重做該段確認點並往下重跑**；新需求或擴大範圍列後續，不在本 MR 處理。

## DEC-10 實作規劃確認點的決議（2026-11-24）

1. 考量：施工前常駐內容已用滿上限，加上 `collaboration` 後預估超出約 50 行。
   **只在特定時機需要的內容不常駐**：`project-overview` 的建置細節移到 `.github/project/build.md`（worktree 準備由 DEC-14 再移到 `claude-worktree.md`），常駐處留一行索引；與「專案固定檢查項目」重複的句子刪除。
2. 考量：`coding-style-csharp`、`coding-style-cpp` 依副檔名適用，Copilot 已依 `applyTo` 按需載入，Claude 卻因 `@` 每次載入。
   **`CLAUDE.md` 不再 `@` 引用各語言 Coding Style**，改由流程索引「新增或修改程式碼」導向；行數上限改為只計常駐內容（入口與 `applyTo: "**"`，基準 193 行）。
   與 `.editorconfig` 重複的格式規則列後續。
3. 考量：Windows PowerShell 5.1 把無 BOM 的 UTF-8 腳本當 ANSI 讀取。**`.editorconfig` 加入 `[*.ps1] charset = utf-8-bom`**，檢查腳本直接寫中文特徵字串。
4. 考量：合併前其他議題會讀不到流程。目前只剩 `AUTO-23254_f_UIStyleUnify` 進行中，之後會 rebase 到本議題，**個人設定照 SPEC-17 在動態驗證前直接刪除**。

## DEC-11 取消常駐內容的行數上限（2026-11-24）

1. 考量：施工中常駐總行數超過 193 行，為了壓回上限來回刪改內容。精簡的目的是不講廢話，若上限讓該講的話講不清楚，就本末倒置。
2. **取消行數硬上限（取代 DEC-9.6 的硬上限）**：行數只當觀察指標，review 門檻段照樣計算並與比對基準比較；增加時報告寫明增加量與必須常駐的理由，由人接受。
3. 施工中為壓行數做的兩處精簡保留：`project-overview` 的「WinForms Designer」一節刪除重複句後只剩一句、沒有做法說明；`copilot-instructions` 的一句併入分層表，意思不變。Designer 的規範由 UI 重構議題再調整。

## DEC-12 Worktree 改為選用、開議題時開分支（2026-11-24）

1. 考量：團隊多數成員單線開發，直接在主 checkout 切分支；「一個議題一個 worktree」寫成硬性規定會卡住不用 worktree 的人，且建立與準備步驟只適用 Claude Code（`.worktreeinclude` 只有 Claude Code 會自動複製）。
   **一個議題一個 branch，worktree 為選用**，同時處理多個議題時使用；專案的 worktree 準備註明其他工具需手動複製未追蹤檔案（取代 SPEC-8.7 原本的硬性規定；worktree 準備由 DEC-14 移到 `claude-worktree.md`）。
2. 考量：TEST-15、TEST-16 演練中，開議題的流程沒有規定何時開分支，AI 要靠流程索引自行補讀 Git 流程文件。
   **分級確認後開分支**，議題文件提交在該分支；`issue-start` 的必讀文件加入 `git-workflow.md`。

## DEC-13 依重要性決定先問或先做（2026-11-24）

1. 考量：協作原則原本寫「review 每項發現的處置一定由人做」，照字面小事也要先停下等人；review 第 1 回合 AI 先修正小問題、再請人一次確認，效率較好。這個做法適用每個環節，不限於 review。
2. **重要事項先由人決定再執行**：需求、規格、範圍、相容性與規範規則的變更，以及分級、確認點定案、合併、push。
3. **小事先處理再請人確認**：措辭、格式、補漏、明顯錯誤；在該階段結束時列出清單，人可逐項否決。無法判斷時視為重要事項。
4. Review 的處置同步調整：「必修」與涉及需求、規格的發現先決定再修；其他可先修正，處置標示「已修正（待人確認）」。

## DEC-14 Claude 與 worktree 的做法獨立成檔（2026-11-25）

1. 考量：session 交接（`SESSION.md`）、Claude Code 建立 worktree 的位置與分支更名、`.worktreeinclude` 自動複製、`CLAUDE.local.md` 由上層載入，都是使用 Claude 或 worktree 時的做法；Copilot 機制不同，團隊成員也未必有這種習慣，混在通用流程與建置說明中不乾淨。
2. 原先決定移到使用者的全域個人設定；但 `build.md` 仍留有 worktree 準備，拆得不完整，且這些做法對其他使用 Claude 的成員也有用。
3. **集中到進版控的 `.github/project/claude-worktree.md`**，只在切換 session、使用 worktree 或 Claude Code 時讀取；由 `project-overview` 的索引導向（常駐 +1 行）。內容包含 `.worktreeinclude` 與需複製的檔案清單、開啟方案後再卸載 `FCLTool`。
4. **從通用流程與建置說明移除**：`issue-workflow.md` 的 session 切換、流程索引的「切換 session」、`issue-docs.md` 的 `SESSION.md`、`git-workflow.md` 的 Claude Code worktree 細節（只留「worktree 為選用，做法見專案指引」）、`build.md` 的 worktree 準備與 worktree 的 `FCLTool` 卸載。
5. **保留**：`.gitignore` 的 `/SESSION.md`、`copilot-instructions.md` 提到 `CLAUDE.md` 的維護規則、`.claude/skills/`（Copilot 也讀得到）、`build.md` 的 `.\Build.bat` 寫法。
6. 回溯修改 REQ-5.1、SPEC-1.1、SPEC-1.5、SPEC-3.8（刪除）、SPEC-5.8（刪除）、SPEC-11、SPEC-13.1、SPEC-13.4、SPEC-14.2、SPEC-14.3，新增 SPEC-14.6；TEST-4、TEST-8、TEST-9 與 `Check.ps1` 同步。

## DEC-15 Review 規範段以流程索引為登記處（2026-11-25）

1. 考量：規範段原本寫「Coding Style、專案慣例」，流程索引的 review 列另寫「加讀 Coding Style」；之後新增 UI 等規範時，兩處都要改，容易漏。
2. **流程索引是規範的唯一登記處**：review 規範段改為「依流程索引列出本次修改涉及的規範，逐一比對」，流程索引的 review 列改為「再依本表加讀本次修改涉及的文件」；新增規範只在索引加一列。
3. 另一做法是每新增規範就同步改 review 的列舉，較明確但兩處維護；使用者同意採第 2 項。
4. 回溯修改 SPEC-1.5、SPEC-7.1。
