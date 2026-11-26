# AI 工具的部署

準備 AI 工具環境、新成員加入時讀本文件。

## 1. 必要

1. Visual Studio 2026 18.5 以上的 GitHub Copilot，Chat 使用 Agent 模式：18.5 以上才會自動找 skill。

## 2. 建議

1. GitHub Copilot CLI：委託 AI Tools Review 時當 reviewer（見 [process-ai-review.md](process-ai-review.md)）。
   1. 從 GitHub `github/copilot-cli` 的 release 下載 `copilot-win32-x64.zip`，核對 release 公布的 SHA256 後解壓；不需要 Node.js。
   2. 解壓到 `%LOCALAPPDATA%\Programs\CopilotCli\`；放在其他位置時，以環境變數 `COPILOT_CLI_PATH` 指向 `copilot.exe`，或加入 `PATH`。
   3. 執行 `copilot login --device-code`，以有 Copilot 授權的帳號登入一次；組織須開啟 Copilot CLI 政策。
   4. 在 repo 根目錄執行 `copilot instruction list`，應列出 `copilot-instructions.md` 與兩份概觀。

## 3. 選用

1. Claude Code：以 Claude 施工的成員使用；團隊流程不依賴它。
