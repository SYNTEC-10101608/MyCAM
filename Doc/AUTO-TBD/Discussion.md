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
   2. 團隊通用（`team-collaboration.instructions.md`、`team/`）不含 MyCAM 專屬內容，可直接搬到團隊其他 repo。
   3. MyCAM 專屬內容只寫在 `project-overview` 及其子層，包含專案固定的 review 檢查項目。
3. 考量：人與 AI 是協作關係，應讀同一份文件。**不依讀者拆分，依載入時機拆分**：
   每次互動都適用的規則常駐；流程本文放在 `.github/team/`，由常駐索引指出「時機 → 必讀文件」。
4. 考量：不常駐就可能被遺漏。**對策分三層**：常駐索引寫明時機；真正的門檻放在 `README.md` 的 DoD 勾選清單；之後由自動檢查補上機器層門檻。
5. 考量：skill 不是不常駐的必要條件，價值在於可直接呼叫與自動判斷。
   **只替開議題、review、release note 三個動作建立 skill**，內容只指向 `team/`，不複製本文。
   Visual Studio 統一使用 2026（18 版以上）；是否讀取 `.claude/skills/` 須實測，讀不到時靠常駐索引。
6. `project-overview` 的 Git 流程只在開分支與送 MR 時用到，**移到 `team/git-workflow.md`**，不常駐。
7. 兩個 remote 是常態（公司正式流程走 GitLab，GitHub 方便遠端協作），**不寫進團隊規範**，個人用法寫在 `CLAUDE.local.md`。

## DEC-3 議題分級與文件（2026-11-24）

1. 考量：L 級議題產出大量 commit 與文件是正常的，但小議題需要精簡的最低產出。**分級決定最低必要產出，並各訂 DoD**，S 級約 5 項。
2. **檔名不加數字前綴**：各級檔案組合不同，數字會跳號；順序由 `README.md` 的索引決定。
3. **各級檔案組合**：S 只有 `README.md`；M 為 `README`、`Spec`、`TestPlan`、`Implementation`、`Review`（有決策時加 `Discussion`）；L 再加 `Goal`、`FollowUp`。
   M 也獨立 `Implementation.md`，因為 STEP 與 commit 的對應同樣需要追溯。
4. 「沒有獨立檔」不等於「不用寫」：S 的目標、修正方式、測試、review、後續、決策寫在 `README.md` 各節；M 的盤點、目標、REQ、後續寫在 `README.md`。
5. **後續處理任何級別都可能有**，因為它的定義就是範圍外的項目；S、M 寫在 `README.md`，L 獨立成檔。
6. **`SESSION.md` 保留但只放交接 prompt 與暫時筆記**；狀態與決策寫在提交的文件，不重複（由 DEC-14 移到 `.github/project/claude-code.md`）。
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
2. 考量：交付的只有 Production Code，文件與測試日後可能失去同步。**Production Code 不引用文件、測試或規範條目**；難解釋的特例最多留 `// AUTO-<編號>: <原因>`（前綴由 DEC-16 改為 `<議題編號>`）。

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

1. 考量：worktree 說明混有 MyCAM 專屬內容（DLL 清單、`CopyRef.bat`）。**拆分**：通用規則移到 `git-workflow.md`，專案準備步驟留在專案指引（位置由 DEC-10.1 改為 `project/build.md`，再由 DEC-14 改為 `project/claude-code.md`）。
2. 考量：`.git/info/exclude` 不進版控，其他成員會誤提交。**`SESSION.md` 改由 `.gitignore` 的 `/SESSION.md` 排除**。
3. 考量：commit hash 要 commit 後才知道，記在文件需要事後補，rebase 後也會變。
   **commit 訊息結尾標示 `(STEP-n)` 或 `(REV-n)`**，`Implementation.md` 不記 hash；一個 STEP 可對應多個 commit（標示由 DEC-20 改為建議）。
4. 考量：編號被其他文件與 commit 訊息引用，commit 訊息無法再改。**確認點確認後編號不重排、不重用，刪除即跳號**（有序清單的條列由 DEC-17 改為保留一行）。
5. **文件條列使用數字標號提升為團隊通則**，理由是溝通時可直接指出第幾項；只在新增或修改的檔案套用。
6. 考量：自動載入的內容每次互動都付成本，其他文件的成本來自重複與模糊。
   **所有規範與文件都須精簡，並明訂定義**（SPEC-1 第 6 項）；**自動載入的內容另設硬上限**（由 DEC-11 取消），review 門檻段計算行數，文件段檢查是否符合通則；機器能檢查的規則逐步移入工具。
7. **分級由實作者確認**（使用 AI 時由 AI 建議），合併者在 review 時可要求升級。
8. 考量：級別不變但後段發現前段要改時，沒有規則。**從最早受影響的段回溯，重做該段確認點並往下重跑**；新需求或擴大範圍列後續，不在本 MR 處理。

## DEC-10 實作規劃確認點的決議（2026-11-24）

1. 考量：施工前常駐內容已用滿上限，加上 `collaboration` 後預估超出約 50 行。
   **只在特定時機需要的內容不常駐**：`project-overview` 的建置細節移到 `.github/project/build.md`（worktree 準備由 DEC-14 再移到 `claude-code.md`），常駐處留一行索引；與「專案固定檢查項目」重複的句子刪除。
2. 考量：`team-coding-style-csharp`、`team-coding-style-cpp` 依副檔名適用，Copilot 已依 `applyTo` 按需載入，Claude 卻因 `@` 每次載入。
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
   **一個議題一個 branch，worktree 為選用**，同時處理多個議題時使用；專案的 worktree 準備註明其他工具需手動複製未追蹤檔案（取代 SPEC-8.7 原本的硬性規定；worktree 準備由 DEC-14 移到 `claude-code.md`）。
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
3. **集中到進版控的 `.github/project/claude-code.md`**，只在切換 session、使用 worktree 或 Claude Code 時讀取；由 `project-overview` 的索引導向（常駐 +1 行）。內容包含 `.worktreeinclude` 與需複製的檔案清單、開啟方案後再卸載 `FCLTool`。
4. **從通用流程與建置說明移除**：`issue-workflow.md` 的 session 切換、流程索引的「切換 session」、`issue-docs.md` 的 `SESSION.md`、`git-workflow.md` 的 Claude Code worktree 細節（只留「worktree 為選用，做法見專案指引」）、`build.md` 的 worktree 準備與 worktree 的 `FCLTool` 卸載。
5. **保留**：`.gitignore` 的 `/SESSION.md`、`copilot-instructions.md` 提到 `CLAUDE.md` 的維護規則、`.claude/skills/`（Copilot 也讀得到）、`build.md` 的 `.\Build.bat` 寫法。
6. 回溯修改 REQ-5.1、SPEC-1.1、SPEC-1.5、SPEC-3.8（刪除）、SPEC-5.8（刪除）、SPEC-11、SPEC-13.1、SPEC-13.4、SPEC-14.2、SPEC-14.3，新增 SPEC-14.6；TEST-4、TEST-8、TEST-9 與 `Check.ps1` 同步。

## DEC-15 Review 規範段以流程索引為登記處（2026-11-25）

1. 考量：規範段原本寫「Coding Style、專案慣例」，流程索引的 review 列另寫「加讀 Coding Style」；之後新增 UI 等規範時，兩處都要改，容易漏。
2. **流程索引是規範的唯一登記處**：review 規範段改為「依流程索引列出本次修改涉及的規範，逐一比對」，流程索引的 review 列改為「再依本表加讀本次修改涉及的文件」；新增規範只在索引加一列。
3. 另一做法是每新增規範就同步改 review 的列舉，較明確但兩處維護；使用者同意採第 2 項。
4. 回溯修改 SPEC-1.5、SPEC-7.1。

## DEC-16 議題編號前綴屬於專案指引（2026-11-25）

1. 考量：團隊通用層的註解規則（`// AUTO-<編號>`）、暫代資料夾（`AUTO-TBD`）與分支範例都寫了 `AUTO-` 前綴；使用者確認 `AUTO-` 只有 MyCAM 使用，違反 SPEC-1.2。
2. **團隊通用層只寫 `<議題編號>`、`<前綴>-TBD`，格式與範例寫在 `project-overview` 的「議題編號」**；常駐 +4 行，因為開分支、commit、寫註解時都要用到。
3. TEST-2 的專案字詞加入 `AUTO-`，檢查範圍加入 `team-coding-style-general`。
4. 回溯修改 REQ-1.2、SPEC-5.2、SPEC-8.3.1、SPEC-10.2，新增 SPEC-14.7；TEST-2、TEST-9 與 `Check.ps1` 同步。

## DEC-17 刪除的條列保留編號（2026-11-25）

1. 考量：Markdown 的有序清單只採用第一項的號碼，之後依序重排；SPEC-3、SPEC-5 刪除第 8 項後，第 9 項在 GitLab / GitHub 顯示為「8.」，引用 SPEC-3.9 的讀者對不上。
2. **有序清單中刪除的條列保留一行「（刪除，見 DEC-n）」**；以標題呈現的編號（`REQ-n`、`TODO-n` 等）照舊直接移除、跳號。
3. TEST-6 加入有序清單不跳號的檢查。
4. 回溯修改 SPEC-6.4，SPEC-3、SPEC-5 補上第 8 項的刪除標示。

## DEC-18 目錄與檔名依層與載入方式命名（2026-11-25）

1. 考量：使用者人工 review 指出指引可依四個面向區分（個人 / 團隊、通用 / 專案、常駐 / 細節、Claude / 通用），但檔案結構看不出後三者：`process/` 依內容、`project/` 依層命名，語意對不上；`instructions/` 內看不出層；Claude 限定內容只有一份專案層的檔案，其中 session 交接其實是通用做法。
2. **目錄名稱表示層、`instructions/` 表示自動載入**：`process/` 改名為 `team/`；`instructions/` 只放工具自動載入的檔（常駐與依副檔名），檔名加 `team-` 或 `project-` 前綴；各語言 Coding Style 留在 `instructions/`，保留 Copilot 依副檔名載入。指引目錄加「層」欄。
3. **Claude Code 限定做法每層一份 `claude-code.md`**：`team/claude-code.md` 放 session 交接，`project/claude-code.md` 放 worktree 位置與需複製的檔案；Copilot 使用者整份跳過。另一做法是在流程內標註「（Claude 限定）」，檔案少但每個人都要讀到。
4. **專案固定檢查項目移到 `project/review.md`**（其中三項規則由 DEC-19 改放 `project-coding-general`），只在 review 時讀取，常駐淨 -9 行（移出 -10，索引改寫 +1）；`project-overview` 的「建置與執行」改為「專案指引索引」。
5. 檔名不用 `claude.md`：Windows 不分大小寫，Claude Code 會當成 `CLAUDE.md` 自動載入（施工時實際發生），寫在 `copilot-instructions.md` 維護方式並由 TEST-4 檢查。
6. 改名會影響進行中的 `AUTO-23254_f_UIStyleUnify`：rebase 時連到團隊通用檔的連結要更新，指引見 `FollowUp.md` TODO-4。
7. 回溯修改 SPEC-1.1、SPEC-1.5、SPEC-11、SPEC-13.6（新增）、SPEC-14.3、SPEC-14.4、SPEC-14.5、SPEC-14.6、SPEC-15.1.2；TEST-4、TEST-9 與 `Check.ps1` 同步。

## DEC-19 專案程式規範（2026-11-25）

1. 考量：四語系同步、Designer 可開、不修改 `reference/` 是寫程式時就要遵守的規則，DEC-18 後只在 review 才讀的 `project/review.md`，AI 寫程式時讀不到（REV-53）。它們屬於程式規範，但不是 Coding Style；之後 UI 規範等同類規範會陸續增加。
2. **新增「專案程式規範」一類：`instructions/project-coding-<主題>.instructions.md`**，依副檔名 `applyTo`、不常駐；團隊流程索引「新增或修改程式碼」一列加上「專案指引的專案程式規範」，專案概觀的專案指引索引列出各檔。新增同類規範時只在索引加一項，review 規範段依流程索引自動涵蓋。
3. 本議題新增 `project-coding-general`，放上述三項；`project/review.md` 只留建置門檻與「符合專案程式規範」。
4. **刪除功能正確性的領域項目（UI 執行緒、角度單位、座標系）**：只列名詞，寫程式與 review 時都無法照做；日後有具體的陷阱與做法再寫進專案程式規範。
5. UI 分支的 `ui-style`、`icon-design` rebase 時改名為 `project-coding-ui-style`、`project-coding-icon-design`（`FollowUp.md` 第 4 節）。
6. 常駐 +3 行：入口的分層說明與指引目錄各 +1、專案指引索引 +1；常駐合計 205（基準 193）。
7. 回溯修改 SPEC-1.1、SPEC-1.5、SPEC-7.1、SPEC-11、SPEC-14.3、SPEC-14.4、SPEC-14.5，新增 SPEC-14.8；TEST-9 與 `Check.ps1` 同步。

## DEC-20 Commit 標示改為建議（2026-11-25）

1. 考量：`(STEP-n)`、`(REV-n)` 標示是 AI 流程方便追溯的做法，手寫程式的成員通常不會逐一標示；規定為必要會讓不用 AI 的成員難以符合（REV-63）。
2. **標示改為建議，位置為標題行結尾**，`git log --oneline` 即可看到。
3. DoD「每個 STEP 有對應的 commit」改為「每個 STEP 可追溯到 commit」：有標示時以 `git log --grep` 查詢，沒有標示時在 review 需求與範圍段人工對照。
4. 本議題的 `Check.ps1` TEST-11 對標題行未標示的 commit 只提示，每個 STEP 仍須有標示的 commit。
5. 回溯修改 SPEC-4、SPEC-7.3、SPEC-8.5；TEST-11 與 `Check.ps1` 同步。

## DEC-21 載入門檻、檔案分類與 skill 原則（2026-11-25）

1. 考量：使用者人工 review 指出 `instructions/` 加前綴與放 `team/`、`project/` 的差別不易理解。實測 Claude Code 只依 `@` 載入，VS 2026 Copilot 未依 `applyTo` 自動載入（REV-19、TEST-16），兩者實際都靠索引讀取，依副檔名載入只是名義上的機制。
2. **載入門檻**：AI 無法自行察覺何時該讀的內容（協作原則、文件通則、索引）才常駐；能以動作或修改的檔案類型辨識時機的內容放 `team/`、`project/`，索引寫明動作名稱或副檔名。取代 DEC-18 第 2 項「`instructions/` 放常駐與依副檔名」。
3. **review 讀入全部規範**：流程索引的 review 列與 `review` skill 讀 `team/`、`project/` 全部文件，作為寫程式時漏讀的保險；review 規範段不再依流程索引列出涉及的規範，取代 DEC-15。
4. **檔案分類**：目錄表示載入方式（`instructions/` 只放每層一份的 `<層>-overview`）；`team/`、`project/` 的檔名以分類開頭：`process-`（流程）、`coding-`（程式規範）、`env-`（環境）、`tool-`（特定工具）。`team-collaboration` 改名 `team-overview`；`coding-style` 與 `project-coding` 統一為 `coding-`；`issue-` 改為分類後的主題（`process-issue`、`process-docs`）。DEC-19 的 `instructions/project-coding-<主題>` 改為 `project/coding-<主題>.md`。
5. **`project-overview` 拆分**：常駐只留產品簡介、議題編號與專案指引索引；技術棧、方案與主程式結構移到 `project/coding-architecture.md`，慣例併入 `project/coding-general.md`。`team/coding-general.md` 移出常駐。常駐 205 → 112 行。
6. **指引目錄改為分類表**：`copilot-instructions.md` 不再逐檔列出，改為「開頭 → 分類 → 讀取時機」，各檔由兩份概觀的索引指出；`Check.ps1` TEST-4 檢查每份 `team/`、`project/` 文件都列在該層索引。
7. **Skill 原則**寫在 `copilot-instructions.md` 維護方式：skill 只是觸發入口；人主動發起、常用、要讀多份文件的動作才寫成 skill，以動作命名；內容只有 `description` 與依序條列的必讀文件，規則寫在指引檔。三個 skill 刪除與清單重複的說明句（REV-25 改為依此處理）。
8. 議題文件中描述最終狀態的文件（Spec、TestPlan、Implementation、FollowUp、README）路徑改為新檔名；Review 與 Discussion 是歷史記錄，保留當時的檔名，`Check.ps1` TEST-7 不檢查兩者的反引號路徑。
9. 改名再次影響 `AUTO-23254_f_UIStyleUnify`：`FollowUp.md` 第 4 節的對照改為新檔名。

## DEC-22 規範類議題的規格寫法（2026-11-25）

1. 考量：本議題 Spec 的操作規格重述 `team/` 規範的內容，實作規格又逐檔重述各檔內容，同一規則寫兩處，違反文件通則；TEST-8 的「規格 → 檔案:行號」對照只為核對這份重複而存在，每次改名都要重算。
2. **交付物是規範或文件的議題，指引檔本身就是規格**：每條 SPEC 只寫決策重點與規則所在的檔案章節，TEST 以連結有效與抽查代替逐條比對；寫入 `process-docs.md` 第 4 節第 4 項。程式議題的 Spec 仍完整描述行為。
3. 本議題依此重寫 Spec：SPEC-1～SPEC-10 改為「位置 + 決策重點」；SPEC-12～SPEC-16 併入 SPEC-11 的檔案表（標題直接移除，編號跳號）；SPEC 子項目重新整理，DEC-22 之前的 Review、Discussion 引用的子項目編號指當時的版本（見 git 記錄）。
4. 刪除 TEST-8，由 TEST-7 的連結檢查涵蓋；TestPlan、Implementation 的對應改為以 SPEC 為單位，新增 STEP-14。

## DEC-23 規範與建議分開、參照明確（2026-11-25）

1. 考量：使用者人工 review 第 7 回合指出，部分條目是建議或操作招式卻寫成規範，部分內容重複或參照對象不明，部分通用做法放在專案層。
2. **只把要求寫成規範**：分支後綴、commit 標示改寫為建議；刪除自測回合不開分支、rebase 暫存分支、`git log --grep`、子 session、只在確認點切換 session（改為 AI 提出切換建議）。
3. **常駐行數不再逐回合比對**（取代 DEC-11 的 review 門檻段計算）：review 文件段改為檢查新增的常駐內容是否符合載入門檻；TEST-1 改為議題限定。
4. **取消「獨立 review」要求**：沒有定義誰算獨立，各級只要求 review 報告。
5. **開始議題時開分支**（取代 DEC-12.2 的「分級確認後」）：分支名稱與分級無關，盤點文件也需要提交處。
6. **待討論格式只定義在團隊概觀協作原則第 3 項**，`process-issue.md`、`process-docs.md` 連到該處；個人 `~/.claude/CLAUDE.md` 的回覆方式一併刪除。
7. **各級的後續處理都寫 `FollowUp.md`**（有項目時）：範圍外項目的多寡與議題大小無關。
8. **參照寫明檔案**：團隊層原本寫「見專案指引」的 7 處改為連到專案概觀或 `project/` 的固定檔名，這是分層結構的一部分，搬到其他 repo 仍成立。
9. **Claude Code 通用的 worktree 做法移到 `team/tool-claude-code.md`**；專案層只留要複製的專案檔、`CopyRef.bat`、卸載 `FCLTool`。
10. **專案 review 門檻建置 Debug 與 Release**，改到 C++ 時另建該專案；平時只建 Debug 是個人習慣，留在 `CLAUDE.local.md`。
11. 施工階段補一句「施工前依流程索引讀取程式規範」，補上議題流程中讀取程式規範的提醒；不新增寫程式的 skill。

## DEC-24 VS 支援版本、worktree 準備與交接不限工具（2026-11-26）

1. 考量：Review 第 8 回合（VS 2026 Copilot）指出 skill 的最低支援版本是 VS 2026 18.5，TEST-16 沒有記錄版本，也沒有測 release-note；新 worktree 的檔案準備只寫在 Claude 專用的文件；使用者另指出 session 交接也不限 Claude Code。
2. **VS 2026 最低支援 18.5**（修改 REQ-2）：團隊統一使用 VS 2026，更新到 18.5 的成本低；不在入口另加連結，也不驗 18.0～18.4 無 skill 的路徑。TEST-16 記錄實測版本、在 Agent 模式執行，並加測 release-note。
3. **新 worktree 的環境準備獨立成 `project/env-worktree.md`，不限工具**（取代 DEC-14.3 後半）：要複製的檔案、`CopyRef.bat`、卸載 `FCLTool` 是任何工具建立 worktree 都要做的；建置文件只講編譯，不放 worktree（DEC-14.4 維持）。刪除 `project/tool-claude-code.md`，專案索引改為「建立或準備新 worktree（不限工具）」。
4. **Session 交接改為通用規則**（取代 DEC-14.1 把交接歸為 Claude 專用的前提）：寫入 `process-issue.md` 第 5 節，流程索引「切換 session 或對話」指向該檔；`team/tool-claude-code.md` 只留 Claude Code 的 worktree 機制。
5. **工具文件只單向引用通用流程**：`process-git.md` 第 5 節改指 `project/env-worktree.md`，不再導向 Claude 文件；`process-issue.md` 與 `process-review.md` 的互相引用是流程接續，保留。
6. **新議題分支以遠端 `master` 為基準**（REV-88，使用者決定）：現行做法不會先切目標版本分支，一律由 `master` 開分支；寫入 `process-git.md` 第 2 節。
