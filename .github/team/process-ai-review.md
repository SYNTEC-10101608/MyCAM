# 委託 AI Tools Review

要委託另一個 AI 工具或模型 review 時讀本文件。本文件是建議做法，要不要委託由人決定；review 的內容、報告格式與處置規則仍依 [process-review.md](process-review.md)。

## 1. 角色

1. 施工方：施工並與人對話的 AI；準備請求、啟動 reviewer、分析結果，在人決定後填處置、修正與 commit。
2. Reviewer：受委託的 AI 工具；只依請求檔在 `Review.md` 新增一回合，不填處置、不改其他檔案、不 commit。
3. 人：決定是否委託、reviewer 的模型與推理強度、每項發現的處置、是否再開一回合。

## 2. 流程

1. 施工方列出 reviewer 工具可用的模型、費用等級與各模型支援的推理強度，並提出建議：與施工方不同家族、可用的最高等級；由人決定。
2. 施工方提交待審的修改，在工作目錄根目錄寫 `REVIEW-REQUEST.md`（範本見第 4 節，由 `.gitignore` 排除）。請求檔寫明 reviewer 規則，reviewer 不需要讀本文件。
3. 啟動 reviewer：可自動執行的工具由施工方在背景啟動，做法見該工具的 `tool-` 文件（如 [tool-copilot-cli.md](tool-copilot-cli.md)）；其他工具由人在該工具中輸入「依 `REVIEW-REQUEST.md` 做 review」。
4. Reviewer 完成後，施工方先以 `git status` 確認只改了 `Review.md` 與請求檔，再逐項分析：同意、不同意（附理由）、需要人決定；需要人決定的依[團隊概觀](../instructions/team-overview.instructions.md)協作原則第 3 項列出。
5. 人決定後，施工方依 [process-review.md](process-review.md) 第 3 節填處置、修正並 commit。

## 3. 回合與終止

1. 第一回合審整個議題分支；之後只審上一回合受審 HEAD 之後的修改（增量回合）。
2. 增量回合沒有「必修」就結束，要再跑由人決定：reviewer 每回合都會挑上一回合修正的措辭，不設終點就停不下來。
3. 前回合的處置是人決定的結論：與既有 `DEC` 或「不修正」衝突的發現，只能列「提問」並附新理由。

## 4. 請求檔範本

````markdown
狀態：待 review

# Review 請求：第 <n> 回合

你是本議題的 reviewer。請依 `review` skill 的必讀文件執行 review，結果寫入議題的 `Review.md`。

| 項目 | 內容 |
| --- | --- |
| 議題 | <議題編號> |
| 分支 | `<分支>` |
| 比對基準 | `<commit>` |
| 受審 HEAD | `<commit>` |
| 上一回合受審 HEAD | `<commit>`（第一回合寫「無」） |
| 報告檔 | `Doc/<議題編號>/Review.md` |
| 工具與模型 | <工具與版本>、<模型>、推理強度 <等級> |

## 範圍與重點

1. <整個分支，或上一回合受審 HEAD 到受審 HEAD 的修改；本回合的重點>
2. 先讀 `Review.md` 前面的回合與 `Discussion.md`；已處置的發現不重提。
3. 門檻：<要執行的檢查；已知的失敗與原因>

## Reviewer 規則

1. 只能修改兩處：在 `Review.md` 檔尾新增「## 第 <n> 回合」一節，以及把本檔第一行改為 `狀態：完成`（無法完成時改為 `狀態：失敗：<原因>`）。
2. 不修改其他檔案與前面的回合；不 commit、不 push、不切換分支、不 stash。
3. 格式依 `process-review.md` 第 2 節，REV 編號從 `REV-<m>` 接續；「處置」欄一律寫「待處置」。
4. 與 `Discussion.md` 的既有決策或前回合「不修正」的結論衝突時，嚴重度寫「提問」，說明中引用該編號與新理由。
5. 說明欄每項兩三句：問題、影響、建議做法；位置寫 `檔案:行號`，行號以受審 HEAD 為準。
6. 沒有發現也要寫回合資訊與門檻結果，並註明「無新發現」。
````
