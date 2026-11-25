# Claude Code 與 worktree

以 Claude Code 建立 worktree 時讀本文件；不使用 Claude Code 的成員不需要讀。何時使用 worktree 見 [process-git.md](process-git.md) 第 5 節，專案要準備的檔案見 [project/env-worktree.md](../project/env-worktree.md)。

1. Claude Code 建立的 worktree 位於 `.claude/worktrees/<名稱>/`（列入 `.gitignore`），分支名稱為 `worktree-<名稱>`，建立後以 `git branch -m` 改為 Git 流程的分支命名。
2. 未追蹤的檔案不會出現在新 worktree。需要的檔案列在 repo 根目錄的 `.worktreeinclude`，Claude Code 建立 worktree 時自動由主 checkout 複製；`.claude/settings.local.json`（個人的 Claude 設定，只在 worktree 根目錄讀取）也列在其中。
3. `CLAUDE.local.md` 不需複製：worktree 位於 repo 底下，Claude Code 會從上層的主 checkout 載入；因此不要在 repo 的上層目錄放 `CLAUDE.md` / `CLAUDE.local.md` 的備份，否則會重複載入。
