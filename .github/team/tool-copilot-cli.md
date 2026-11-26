# Copilot CLI

以 GitHub Copilot CLI 當委託 AI Tools Review 的 reviewer 時讀本文件；流程見 [process-ai-review.md](process-ai-review.md)，安裝與登入見 [env-ai-tools.md](env-ai-tools.md)。

1. 以 [Invoke-CopilotReview.ps1](../scripts/Invoke-CopilotReview.ps1) 在工作目錄（主 checkout 或 worktree）執行：
   1. `-ListModels`：列出可用的模型、費用等級與各模型支援的推理強度，供人決定。
   2. `-Model <模型> -Effort <推理強度>`：依 `REVIEW-REQUEST.md` 執行 review，結束後列出工作樹的異動與請求檔狀態。
2. 腳本以全部允許的權限（`--allow-all`）執行，避免中途一再詢問；限制只靠請求檔的 reviewer 規則，所以結束後必須確認異動只有 `Review.md` 與請求檔。
3. VS Copilot 的 Agent 本身是內建的 Copilot CLI，會把 `COPILOT_HOME` 等環境變數傳給子程序，直接執行另裝的 CLI 會找不到登入資訊；腳本會先清除這些變數，所以 VS Copilot 也能當施工方委託。
4. CLI 與 VS Copilot 載入指引的方式不同（CLI 另讀 `CLAUDE.md` 與其 `@` 引用），CLI 的行為不能代替 VS 的驗證。
