# 後續處理

本議題（團隊與 AI 協作開發流程規範）範圍外的項目。流程規範本文在 `.github/process/`，常駐規範在 `.github/instructions/collaboration.instructions.md`；各項內容自足，接手時不需回溯本議題的討論。

## 1. 文件維護

### TODO-1 使用手冊維護機制

1. 目標：功能修改時，客戶操作手冊（`Documents/`，簡體中文，目前已落後）能同步更新。
2. 完成條件：
   1. 決定手冊的格式、存放位置與負責人。
   2. `process/issue-workflow.md` 或 DoD 加入「手冊是否受影響」的判斷與更新步驟；review 文件段已檢查手冊是否受影響，補上處理方式。
3. 延後原因：手冊的格式與負責人尚未決定，內容更新量大，不屬於流程成文化的範圍。
4. 建議做法：先盤點手冊與現行功能的落差，再決定是否改用可隨程式一起版控的格式（如 Markdown）。

## 2. 自動化

### TODO-2 自動化品質門檻

1. 目標：review 門檻段的機械式檢查由工具執行，文件中的對應規則縮為一行指引。
2. 完成條件，分三階段，各自可獨立完成：
   1. 本機檢查腳本：長期維護的檢查（常駐行數、格式與數字條列、連結有效、團隊通用層不含專案內容）以 PowerShell 腳本放在 repo，成員送 MR 前可執行。
   2. CI：GitLab CI 在 MR 上執行建置與檢查腳本，失敗時不可合併。
   3. 測試專案：建立 C# 測試專案，核心演算法有自動測試。
3. 延後原因：本議題以成文化現有機制為主；議題限定的檢查已在 `Doc/AUTO-TBD/Check.ps1`，長期維護版需要另行設計與團隊同意。
4. 建議做法：
   1. 以 `Doc/AUTO-TBD/Check.ps1` 為起點，抽出長期維護的檢查。
   2. 第一個候選：團隊通用層（`collaboration.instructions.md`、`.github/process/*.md`）不得含專案專屬字詞（`MyCAM`、`Build.bat`、`CopyRef`、`FCLTool`、`PostTool`、`OCC`、`LanguageManager`、`Designer`、`reference/`、`.worktreeinclude`、`CustPost`、`zh-TW`），對應本議題 TEST-2。
   3. 每建立一項自動檢查，就把規範中對應的文字規則縮為一行指引。

### TODO-3 GitLab MR 建立自動化

1. 目標：議題分支 push 時自動建立 MR，標題與描述取自 release note 與 merge log。
2. 完成條件：`process/git-workflow.md` 寫明建立方式，並以一個實際議題驗證。
3. 延後原因：需要團隊決定工具與權限；push 與送 MR 仍由人決定，自動化只省去手動操作。
4. 建議做法：
   1. 不需額外環境：`git push` 的 push options（`-o merge_request.create`、`-o merge_request.target=<版本分支>`、`-o merge_request.title=<標題>`）。
   2. 需要工具與 access token：`glab` CLI 或 GitLab API，可帶入完整描述。

## 3. 規範調整

### TODO-4 既有分支依新規範調整

1. 目標：進行中的分支 `AUTO-23254_f_UIStyleUnify` 依新規範收尾。
2. 完成條件：rebase 到含新規範的版本分支後，議題文件與 commit 標示符合 `.github/process/issue-docs.md` 與 `git-workflow.md`，DoD 全部勾選。
3. 延後原因：該分支在新規範合併前已開工，須等本議題合併後才能 rebase。
4. 建議做法：已確認的舊文件不轉換編號（相容性規則：既有議題文件相容），只在新增內容採新規範；補上 `README.md` 的議題資訊表、DoD 與 Release Note。

### TODO-5 Coding Style 與 `.editorconfig` 的重複

1. 目標：格式類規則只寫在 `.editorconfig`，Coding Style 文件只留一行指引；Claude 依副檔名載入各語言 Coding Style。
2. 完成條件：
   1. `coding-style-*.instructions.md` 中與 `.editorconfig` 重複的規則（編碼、換行、縮排、行長、行尾空白）刪除，改為一行「格式依 `.editorconfig`」。
   2. 評估 Claude Code 依檔案路徑自動載入規則的機制，可行時取代流程索引中「新增或修改程式碼」一列的導向；VS 2026 Copilot 實測也未依 `applyTo` 自動載入 `coding-style-csharp`，由 Copilot 依流程索引主動讀取，一併確認。
3. 延後原因：本議題只修訂 `coding-style-general` 的兩項規則；其餘 Coding Style 檔案未修改，依「只改動到的檔案」原則留待後續。
4. 建議做法：先確認 `.editorconfig` 已涵蓋所有被刪的規則，再刪除文件內容。
