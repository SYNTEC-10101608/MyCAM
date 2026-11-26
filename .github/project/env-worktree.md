# 新 worktree 的環境準備

建立或準備本專案的新 worktree 時讀本文件，不限使用的工具。何時使用 worktree 見 [team/process-git.md](../team/process-git.md) 第 5 節。

1. 未追蹤的檔案不會出現在新 worktree，需要由主 checkout 複製：`MyCAM/bin/<組態>/` 下的 `FCLTool.dll`、`PostTool.dll` 與 `Custom/`。這些檔案列在 repo 根目錄的 `.worktreeinclude`，Claude Code 建立 worktree 時會自動複製；以其他工具建立時手動複製。
2. 第一次建置後執行 `CopyRef.bat`（見 [env-build.md](env-build.md) 的執行期檔案）。
3. 新 worktree 沒有 `.vs/`，`FCLTool` 會回到載入狀態；以 Visual Studio 開啟方案後需再卸載一次，或只用 `Build.bat` 建置（原因見 [env-build.md](env-build.md) 的 FCLTool 一節）。
