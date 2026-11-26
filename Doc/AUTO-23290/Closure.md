# AUTO-23290 結案報告：團隊與 AI 協作開發流程規範

| 項目 | 內容 |
| --- | --- |
| 級別 | L |
| 分支 | `AUTO-23290_f_DevProcess`（起點 `24c7276d`，RLS_0.48.0） |
| 合併 | 2026-11-26 squash 合併進 `0.49.0`；本報告與文件通則第 5 項隨 2nd MR（`AUTO-23290_f_DevProcess_2nd`）提交 |
| 期間 | 2026-11-23～2026-11-26 |
| 細節 | 本資料夾的研發文件：[README](README.md)、[Goal](Goal.md)、[Spec](Spec.md)、[TestPlan](TestPlan.md)、[Implementation](Implementation.md)、[Review](Review.md)、[FollowUp](FollowUp.md) |

## 1. 為什麼做

1. 開發流程只存在個人設定，團隊成員與他們的 AI 都讀不到。
2. 全部指引常駐，占用 AI 的 context；流程成本不隨議題大小調整。
3. Review 標準、完成定義、分支後綴、文件結構沒有成文，每個人做法不同。

## 2. 做了什麼

1. **規範進版控、人與 AI 共用**：`.github/` 為唯一來源，Copilot 與 Claude Code 讀同一份。
2. **分層與按需載入**：入口、團隊通用、專案、個人四層；常駐只留入口與兩份概觀（193 行降為 115 行），其餘由索引依動作或副檔名讀取。
3. **流程成文**：議題分級（S、M、L）、確認點與 DoD；議題文件組合與編號；六段 review；Git、release note 與 merge log。
4. **協作原則**：小事先做後確認，需求、規格、範圍、合併與 push 先由人決定；待討論寫問題、建議、另一可行選項。
5. **Coding Style 修訂**：新增與修改的程式碼採新規範；Production Code 註解不引用文件。
6. **專案層**：架構、專案慣例、固定檢查項目、建置與 worktree 環境。
7. **四個 skill**：開議題、review、產出 release note、委託 AI Tools Review。
8. **委託 AI Tools Review（建議）**：由施工方的 AI 委託另一個 AI 工具 review，人只在一個對話中決定模型與處置；附 Copilot CLI 輔助腳本。
9. **個人設定收斂**：只留語言、記憶、本機環境與個人習慣。

## 3. 團隊要知道的事

### 3.1 怎麼開始用

1. 直接對 AI 說動作：「開議題」、「review」、「產出 release note」、「委託 AI Tools Review」，AI 會讀對應的規範。
2. 規範入口：[copilot-instructions.md](../../.github/copilot-instructions.md)；流程索引在[團隊概觀](../../.github/instructions/team-overview.instructions.md)。

### 3.2 與以前不同的地方

| 項目 | 現在 |
| --- | --- |
| 開分支 | 開議題時先開，以遠端 `master` 為基準，先 fetch |
| 議題大小 | 由實作者分級；S 級文件全部寫在 `README.md` |
| 送 MR | `README.md` 的 DoD 全部勾選；release note 以「新增 / 修改 / 刪除」開頭 |
| Review | 固定六段與報告格式，每項發現都要有處置 |
| 改指引 | 在議題文件寫明異動與理由 |
| 引用文件 | 快照（結案報告、review 回合）可引用持續維護的文件；持續維護的文件只以追溯編號指向快照 |

### 3.3 環境

1. 必要：VS 2026 18.5 以上，Copilot 使用 Agent 模式。
2. 建議：Copilot CLI（委託 review 用），安裝見 [env-ai-tools.md](../../.github/team/env-ai-tools.md)。
3. 選用：Claude Code。

## 4. 驗證結果

1. 自動：`Check.ps1`（議題限定）10 項全部通過。
2. 動態演練：
   1. Claude Code：只載入入口與概觀；四個 skill 與流程索引都導向正確文件。通過。
   2. VS 2026 Copilot：讀取 `.claude/skills/`、依索引讀規範、觸發 skill。通過；「開議題」後的盤點與分級停點只在 Claude Code 驗證。
   3. 委託 AI Tools Review：Claude Code 與 VS Copilot 當施工方都完成委託；VS 第一次誤觸發 `review` skill 而自己做了 review，新增 `ai-tools-review` skill 後通過。
3. Review：14 回合（Claude Code 自審與獨立 session、人工、VS Copilot、Copilot CLI），共 109 項發現（必修 14、建議 89、提問 6）；已修正 105、不修正 3、列後續 1，其中 20 項回溯修改前段文件。

## 5. 過程中的重要發現

1. VS 2026 Copilot 不依 `applyTo` 自動載入，Claude Code 只依 `@` 載入；兩者實際都靠概觀中的索引找文件，所以索引要寫明動作名稱或副檔名。
2. Windows 不分大小寫，檔名 `claude.md` 會被 Claude Code 當成 `CLAUDE.md` 自動載入。
3. Skill 的觸發說明會互相搶觸發（「委託 review」被 `review` 吃掉），新增 skill 時要在相近的 skill 排除。
4. VS Copilot 會把 `COPILOT_HOME` 等環境變數傳給子程序，直接呼叫另裝的 Copilot CLI 會找不到登入資訊；腳本先清除這些變數。
5. AI reviewer 每回合都會挑上一回合修正的措辭，所以委託 review 規定「增量回合沒有必修就結束」，且與既有決策衝突的只能列「提問」。

## 6. 相容性

1. 既有議題文件、分支名稱、程式碼不轉換，只在修改時套用新規範。
2. 進行中的 `AUTO-23254_f_UIStyleUnify` 改到舊指引檔名，rebase 時會衝突，做法見 [FollowUp.md](FollowUp.md) 第 4 節。
3. Production Code、建置產物、NC 輸出：無影響。

## 7. 後續

| TODO | 內容 |
| --- | --- |
| TODO-1 | 使用手冊（`Documents/`）的維護機制 |
| TODO-2 | 自動化品質門檻：本機檢查腳本 → GitLab CI → C# 測試專案 |
| TODO-3 | GitLab MR 建立自動化 |
| TODO-4 | `AUTO-23254_f_UIStyleUnify` 依新規範 rebase 收尾（本議題已合併，可開工） |
| TODO-5 | Coding Style 與 `.editorconfig` 的重複規則收斂 |

## 8. 移到其他專案或設備

1. 其他專案：
   1. 直接複製入口、團隊概觀、`team/`、`scripts/`、`.claude/skills/`、`CLAUDE.md`。
   2. 重寫專案概觀與 `project/`；`project/` 至少要有 `process-review.md`、`env-worktree.md`、`coding-architecture.md`。
   3. 同步 `.gitignore`、`.editorconfig` 的設定；`Check.ps1` 不搬。
2. 其他設備：
   1. 依 3.3 節安裝工具並登入。
   2. 手動帶過去個人設定：`~/.claude/CLAUDE.md`、`CLAUDE.local.md`、`.claude/settings.local.json`。

細節見 [README.md](README.md) 的議題總結。
