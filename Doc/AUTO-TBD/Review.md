# Review

依 [review.md](../../.github/process/review.md) 的報告格式，每回合一節。

## 第 1 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-TBD |
| 分支 | `AUTO-TBD_f_DevProcess` |
| 比對基準 | `24c7276d` |
| 工具與模型 | Claude Code、Claude Opus 5.5；發現來自 STEP-12 動態驗證（TEST-15 演練 session、TEST-16 VS 2026 Copilot） |
| 日期 | 2026-11-24 |

1. 門檻：`Check.ps1` 除 TEST-11（待本 STEP 收官）外通過；常駐 190 行（基準 193，差異 -3），未增加。本議題只改文件，不需建置。
2. 本回合只處理動態驗證中發現的問題；完整六段 review 於第 2 回合進行。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-1 | 2 | 必修 | `.github/process/git-workflow.md:60` | 「一個議題一個 worktree」為硬性規定，團隊多數成員單線開發，且建立與準備步驟只適用 Claude Code（`.worktreeinclude` 只有 Claude Code 自動複製） | 已修正（回溯至 SPEC-8.7，DEC-12） |
| REV-2 | 2 | 必修 | `.github/process/issue-workflow.md:29` | 開議題流程沒有規定何時開分支；`issue-start` 必讀文件沒有 `git-workflow.md`，AI 要靠流程索引自行補讀 | 已修正（回溯至 SPEC-2、SPEC-3.3，DEC-12） |
| REV-3 | 6 | 建議 | `.github/process/git-workflow.md:14` | 分支命名 `AUTO-<議題編號>_…` 與 `Doc/<議題編號>/`（`AUTO-TBD`）對「議題編號」是否含前綴的定義不一致 | 已修正（回溯至 SPEC-8.3.1：議題編號含前綴） |
| REV-4 | 6 | 建議 | `.github/process/issue-docs.md:65` | S 級 `README.md` 範本缺「現況盤點」與「待討論」，與同檔第 2 節的內容歸屬及確認點文件規則不一致 | 已修正 |
| REV-5 | 6 | 建議 | `.github/process/review.md:23` | S 級報告寫在 `README.md` 的 Review 章節，範本的回合標題同為 `##`，層級衝突 | 已修正 |
| REV-6 | 3 | 建議 | `Doc/AUTO-TBD/Check.ps1:5` | `param()` 未加 `[CmdletBinding()]`，打錯參數名會被忽略並改用預設比對基準 | 已修正 |
| REV-7 | 6 | 建議 | `Doc/AUTO-TBD/FollowUp.md:60` | TEST-16 確認 VS 2026 Copilot 可讀取 `.claude/skills/`，TODO-6 的前提不成立 | 已修正（刪除 TODO-6；Copilot 未依 `applyTo` 載入 `coding-style-csharp` 併入 TODO-5） |
| REV-8 | 1 | 必修 | `Doc/AUTO-TBD/Check.ps1:446` | TEST-7 把 review 報告規定的位置格式 `檔案:行號` 整串當成路徑，報告一定被判定失敗 | 已修正 |
