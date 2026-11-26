# AUTO-TBD 團隊與 AI 協作開發流程規範

| 項目 | 內容 |
| --- | --- |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 分支起點 | `24c7276d`（`origin/master`，RLS_0.48.0，2026-11-23） |
| 級別 | L |
| 狀態 | 收官：STEP-1～STEP-16、Review 第 9 回合處置完成；TEST-16 待使用者重跑，待人工確認 |

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
3. [ ] `TestPlan.md` 附最終執行結果（第 5 節；TEST-16 待使用者重跑）。
4. [x] 每個 STEP 可追溯到 commit（`Check.ps1` TEST-11）。
5. [x] 門檻通過：`Check.ps1` 通過；本議題只改文件，不需建置。
6. [x] Review 報告寫入 `Review.md`，「必修」全部已修正。
7. [x] 範圍外項目寫入 `FollowUp.md`，每項寫明目標、完成條件與延後原因。
8. [x] Release note 寫入 `README.md`。
9. [ ] 各確認點分別經人確認（確認點 1～5 已確認；確認點 7 待人工全域 review）。
10. [ ] 長期有效的結論已提升到指引規範（待人工全域 review 確認）。

## Release Note

1. 新增 團隊協作規範：協作原則、文件通則與流程索引，Copilot 與 Claude 共用同一份並常駐讀取。
2. 新增 議題處理流程：S、M、L 分級，各階段確認點與完成定義（DoD）。
3. 新增 議題文件規範：各級文件組合（後續處理各級都獨立成檔）、項目編號與追溯方式、README 範本。
4. 新增 Review 規範：六段檢查、報告格式與處置規則。
5. 新增 Git 流程規範：版本與分支命名、commit 標示、release note 與 merge log 格式，worktree 改為選用。
6. 新增 開議題、review、產出 release note 三個 AI 入口指令，Claude Code 與 VS 2026（18.5 以上）Copilot 皆可使用。
7. 新增 專案程式規範與專案固定檢查項目，專案架構、慣例與多語系同步、Designer、第三方檔案的規則在寫程式時讀取，review 另查建置門檻。
8. 修改 專案概觀只留產品簡介、議題編號格式與專案指引索引，架構、慣例與建置說明改為需要時才讀取。
9. 修改 Coding Style 通用規則，修改既有程式碼時以方法為單位套用新規範，程式碼不引用文件編號。
10. 修改 Coding Style 不再常駐載入，改為修改程式碼時依流程索引讀取；review 時讀入全部規範。
11. 新增 Session 交接與新 worktree 環境準備的說明，不限 AI 工具；Claude Code 的 worktree 機制另寫一份，只在使用時讀取。
12. 修改 指引檔案依層與分類命名（流程、程式規範、環境、工具），常駐內容只留協作通則與索引，約為原本的六成。
13. 新增 指引維護原則：常駐與依時機讀取的判斷門檻、skill 的撰寫原則；規範類議題的規格只寫決策與位置。
