# MyCAM 開發指引

本目錄是團隊的開發規範，同時也是 AI 工具（GitHub Copilot、Claude Code 等）的專案指引；人與 AI 讀同一份文件。

## 規範分層

| 層 | 位置 | 內容 |
| --- | --- | --- |
| 入口 | 本檔、`CLAUDE.md` | 規範如何分類、去哪裡讀 |
| 團隊通用 | `instructions/team-*`、`team/` | 協作原則、文件通則、流程、Coding Style；不含本專案專屬內容 |
| 專案 | `instructions/project-*`、`project/` | 本專案的架構、慣例、程式規範、review 檢查項目、建置與環境；可補充或具體化團隊通用層 |
| 個人 | `~/.claude/CLAUDE.md`、`CLAUDE.local.md` 等（不進版控） | 個人偏好，不改變團隊規範的要求 |

1. `instructions/` 放工具自動載入的指引：`applyTo: "**"` 為常駐，其他依副檔名適用；`team/`、`project/` 依時機讀取，由流程索引或專案概觀的索引指出時機。
2. 本專案寫程式時必須遵守、但不屬於 Coding Style 的規則，放在 `instructions/project-coding-<主題>.instructions.md`，並列入專案概觀的專案指引索引。
3. 只適用 Claude Code 的做法，每層各放一份 `claude-code.md`；不使用 Claude Code 的成員不需要讀。

## 指引目錄

| 文件 | 層 | 內容 | 載入方式 |
| --- | --- | --- | --- |
| [協作規範](instructions/team-collaboration.instructions.md) | 團隊通用 | 協作原則、文件通則、流程索引 | 常駐 |
| [Coding Style：通用規則](instructions/team-coding-style-general.instructions.md) | 團隊通用 | 編碼、縮排、檔名、註解、修改既有程式碼的原則 | 常駐 |
| [Coding Style：C#](instructions/team-coding-style-csharp.instructions.md) | 團隊通用 | C# 格式、命名、程式結構、語言用法 | `*.cs`：依流程索引讀取，Copilot 另依 `applyTo` |
| [Coding Style：C / C++](instructions/team-coding-style-cpp.instructions.md) | 團隊通用 | C / C++ 格式、命名、語言用法 | `*.c` `*.cpp` `*.h` `*.hpp`：同上 |
| [議題處理流程](team/issue-workflow.md) | 團隊通用 | 階段、分級與確認點、DoD | 依時機讀取 |
| [議題文件](team/issue-docs.md) | 團隊通用 | 文件組合、編號與追溯、範本 | 依時機讀取 |
| [Review](team/review.md) | 團隊通用 | 六段、報告格式、處置規則 | 依時機讀取 |
| [Git 流程](team/git-workflow.md) | 團隊通用 | 版本、分支、commit、release note、worktree | 依時機讀取 |
| [Claude Code 工作方式](team/claude-code.md) | 團隊通用 | Session 交接、子 session | 依時機讀取（Claude Code） |
| [專案概觀](instructions/project-overview.instructions.md) | 專案 | 產品、方案結構、主程式架構、慣例、專案指引索引 | 常駐 |
| [專案程式規範：通用](instructions/project-coding-general.instructions.md) | 專案 | 多語系同步、Designer、第三方檔案 | `*.cs` `*.xml`：依專案指引索引讀取，Copilot 另依 `applyTo` |
| [專案固定檢查項目](project/review.md) | 專案 | review 必查的建置門檻與專案程式規範 | 依時機讀取 |
| [建置與環境](project/build.md) | 專案 | FCLTool、命令列建置、執行期檔案 | 依時機讀取 |
| [Claude Code 與 worktree](project/claude-code.md) | 專案 | worktree 位置與分支更名、需複製的檔案 | 依時機讀取（Claude Code） |

格式類規則同時寫在 repo 根目錄的 `.editorconfig`，衝突時以 `.editorconfig` 為準。

## 維護方式

1. 檔案依層與載入方式放置（見「規範分層」）；`instructions/` 的檔名為 `<層>-<主題>.instructions.md`，開頭以 `applyTo` 指定適用檔案；`team/`、`project/` 不加 `applyTo`。檔名不用 `claude.md`：Windows 不分大小寫，Claude Code 會當成 `CLAUDE.md` 自動載入。
2. 新增指引時，同步在上方「指引目錄」表格加一列；常駐的指引另在根目錄 `CLAUDE.md` 加一行 `@` 引用（`@` 會常駐載入，其他文件不加）。
3. 常駐內容（入口與 `applyTo: "**"` 的指引）每次互動都付成本：只放每次都適用的通則並力求精簡，只在特定時機需要的內容改為依時機讀取。
4. 修改任何指引文件（常駐或依時機讀取）時，在議題文件寫明異動與理由，由 review 文件段檢查。
5. 本檔只放目錄與分類說明，具體規則寫在各自的指引檔。
