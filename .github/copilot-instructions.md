# MyCAM 開發指引

本目錄是團隊的開發規範，同時也是 AI 工具（GitHub Copilot、Claude Code 等）的專案指引；人與 AI 讀同一份文件。

## 規範分層

| 層 | 文件 | 內容 |
| --- | --- | --- |
| 入口 | 本檔、`CLAUDE.md` | 規範如何分類、去哪裡讀 |
| 團隊通用 | `collaboration`、`coding-style-*`、`process/` | 協作原則、文件通則、流程、Coding Style；不含本專案專屬內容 |
| 專案 | `project-overview`、`project/` | 本專案的架構、慣例、專案固定檢查項目、建置與環境；可補充或具體化團隊通用層 |
| 個人 | `~/.claude/CLAUDE.md`、`CLAUDE.local.md` 等（不進版控） | 個人偏好，不改變團隊規範的要求 |

## 指引目錄

「常駐」為每次互動都載入；「依時機讀取」由協作規範的流程索引或專案概觀的索引指出時機。

| 文件 | 內容 | 載入方式 |
| --- | --- | --- |
| [協作規範](instructions/collaboration.instructions.md) | 協作原則、文件通則、流程索引 | 常駐 |
| [專案概觀](instructions/project-overview.instructions.md) | 產品、方案結構、主程式架構、慣例、專案固定檢查項目 | 常駐 |
| [Coding Style：通用規則](instructions/coding-style-general.instructions.md) | 編碼、縮排、檔名、註解、修改既有程式碼的原則 | 常駐 |
| [Coding Style：C#](instructions/coding-style-csharp.instructions.md) | C# 格式、命名、程式結構、語言用法 | `*.cs`：Copilot 依 `applyTo`，Claude 依流程索引 |
| [Coding Style：C / C++](instructions/coding-style-cpp.instructions.md) | C / C++ 格式、命名、語言用法 | `*.c` `*.cpp` `*.h` `*.hpp`：同上 |
| [議題處理流程](process/issue-workflow.md) | 階段、分級與確認點、DoD | 依時機讀取 |
| [議題文件](process/issue-docs.md) | 文件組合、編號與追溯、範本 | 依時機讀取 |
| [Review](process/review.md) | 六段、報告格式、處置規則 | 依時機讀取 |
| [Git 流程](process/git-workflow.md) | 版本、分支、commit、release note、worktree | 依時機讀取 |
| [建置與環境](project/build.md) | FCLTool、命令列建置、執行期檔案、worktree 準備 | 依時機讀取 |

格式類規則同時寫在 repo 根目錄的 `.editorconfig`，衝突時以 `.editorconfig` 為準。

## 維護方式

1. 常駐或依副檔名適用的指引放在 `.github/instructions/`，檔名為 `<主題>.instructions.md`，開頭以 `applyTo` 指定適用檔案；依時機讀取的文件放在 `.github/process/`（團隊通用）或 `.github/project/`（專案），不加 `applyTo`。
2. 新增指引時，同步在上方「指引目錄」表格加一列；常駐的指引另在根目錄 `CLAUDE.md` 加一行 `@` 引用（`@` 會常駐載入，其他文件不加）。
3. 常駐內容（入口與 `applyTo: "**"` 的指引）每次互動都付成本：只在特定時機需要的內容不常駐；常駐總行數增加時，依 review 規範寫明理由。
4. 本檔只放目錄與分類說明，具體規則寫在各自的指引檔。
