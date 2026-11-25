# Claude Code 與 worktree（專案）

以 Claude Code 建立或使用本專案的 worktree 時讀本文件；通用做法見 [team/tool-claude-code.md](../team/tool-claude-code.md)。

1. `.worktreeinclude` 列出的專案檔案：`MyCAM/bin/<組態>/` 下的 `FCLTool.dll`、`PostTool.dll` 與 `Custom/`；以其他工具建立 worktree 時手動複製。
2. 第一次建置後執行 `CopyRef.bat`（見 [env-build.md](env-build.md) 的執行期檔案）。
3. 新 worktree 沒有 `.vs/`，`FCLTool` 會回到載入狀態；以 Visual Studio 開啟方案後需再卸載一次，或只用 `Build.bat` 建置（原因見 [env-build.md](env-build.md) 的 FCLTool 一節）。
