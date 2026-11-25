# Claude Code 工作方式

以 Claude Code 處理議題、切換 session 時讀本文件；不使用 Claude Code 的成員不需要讀。專案的 worktree 做法見專案指引。

1. 只在確認點切換 session；切換前在工作目錄（主 checkout 或 worktree）根目錄的 `SESSION.md` 更新交接 prompt。
2. `SESSION.md` 以 `.gitignore` 排除，只放交接 prompt 與暫時筆記；狀態與決策寫在提交的議題文件。
3. 主 session 可開子 session 做驗證或獨立 review，結果回到主 session 整合後再提交。