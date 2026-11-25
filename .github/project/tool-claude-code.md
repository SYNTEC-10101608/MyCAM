# Claude Code 與 worktree（專案）

以 Claude Code 建立或使用本專案的 worktree 時讀本文件；Session 交接等通用做法見 [team/tool-claude-code.md](../team/tool-claude-code.md)，何時使用 worktree 見 [process-git.md](../team/process-git.md) 第 5 節。

1. Claude Code 建立的 worktree 位於 `.claude/worktrees/<名稱>/`（已列入 `.gitignore`），分支名稱為 `worktree-<名稱>`，建立後以 `git branch -m` 改為 Git 流程的分支命名。
2. 未追蹤的檔案不會出現在新 worktree。需要的檔案列在 repo 根目錄的 `.worktreeinclude`，Claude Code 建立 worktree 時自動由主 checkout 複製；以其他工具建立時手動複製：
   1. `MyCAM/bin/<組態>/` 下的 `FCLTool.dll`、`PostTool.dll` 與 `Custom/`。
   2. `.claude/settings.local.json`（個人的 Claude 設定，只在 worktree 根目錄讀取）。
3. `CLAUDE.local.md` 不需複製：worktree 位於 repo 底下，Claude Code 會從上層的主 checkout 載入；因此不要在 repo 的上層目錄放 `CLAUDE.md` / `CLAUDE.local.md` 的備份，否則會重複載入。
4. 第一次建置後執行 `CopyRef.bat`（見 [env-build.md](env-build.md) 的執行期檔案）。
5. 新 worktree 沒有 `.vs/`，`FCLTool` 會回到載入狀態；以 Visual Studio 開啟方案後需再卸載一次，或只用 `Build.bat` 建置（原因見 [env-build.md](env-build.md) 的 FCLTool 一節）。
