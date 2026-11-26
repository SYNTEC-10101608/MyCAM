# MyCAM 開發指引

本目錄是團隊的開發規範，同時也是 AI 工具（GitHub Copilot、Claude Code 等）的專案指引；人與 AI 讀同一份文件。

## 規範分層

| 層 | 位置 | 內容 |
| --- | --- | --- |
| 入口 | 本檔、`CLAUDE.md` | 規範如何分類、去哪裡讀 |
| 團隊通用 | `instructions/team-overview.instructions.md`、`team/` | 協作原則、文件通則、流程、Coding Style；不含本專案專屬內容 |
| 專案 | `instructions/project-overview.instructions.md`、`project/` | 本專案的架構、慣例、程式規範、review 檢查項目、建置與環境；可補充或具體化團隊通用層 |
| 個人 | `~/.claude/CLAUDE.md`、`CLAUDE.local.md` 等（不進版控） | 個人偏好，不改變團隊規範的要求 |

## 檔案分類

1. 目錄表示載入方式：`instructions/` 只放常駐的概觀，每層一份；`team/`、`project/` 依時機讀取，時機由各層概觀的索引指出；`scripts/` 放指引引用的輔助腳本。
2. 檔名開頭表示分類：

   | 開頭 | 分類 | 讀取時機 |
   | --- | --- | --- |
   | `<層>-overview` | 概觀：協作原則、文件通則、專案簡介、索引 | 常駐 |
   | `process-` | 流程 | 開議題、撰寫議題文件、review、Git 操作 |
   | `coding-` | 程式規範：團隊層為 Coding Style，專案層為架構、慣例與專案規則 | 新增或修改程式碼 |
   | `env-` | 環境 | 建置、準備執行環境 |
   | `tool-` | 特定工具的做法 | 使用該工具時；不使用的成員不需要讀 |

3. `team/` 與 `project/` 的同名檔（如 `process-review.md`）是同一主題的團隊規範與專案補充，一起讀。

格式類規則同時寫在 repo 根目錄的 `.editorconfig`，衝突時以 `.editorconfig` 為準。

## 維護方式

1. 常駐或依時機讀取：AI 無法自行察覺何時該讀的內容（協作原則、文件通則、索引）才常駐；能以動作或修改的檔案類型辨識時機的內容放 `team/`、`project/`，並在索引寫明動作名稱或副檔名。常駐每次互動都付成本，力求精簡。
2. 新增指引時依分類命名，並在該層概觀的索引加一項；常駐檔另在根目錄 `CLAUDE.md` 加一行 `@` 引用（`@` 會常駐載入，其他文件不加）。`team/`、`project/` 不加 `applyTo`。檔名不用 `claude.md`：Windows 不分大小寫，Claude Code 會當成 `CLAUDE.md` 自動載入。
3. Skill（`.claude/skills/`，Copilot 也會讀取）只是觸發入口：
   1. 人主動發起、常用、且要讀多份文件或有讀取順序的動作，才寫成 skill；以動作命名。
   2. 內容只有 `description`（觸發的說法與時機）與依序條列的必讀文件；規則寫在指引檔，不寫在 skill，直接說出動作時才能經索引得到相同結果。
   3. 指引檔改名或新增時，同步 skill 內的路徑。
4. 修改任何指引文件時，在議題文件寫明異動與理由，由 review 文件段檢查。
5. 本檔只放分類與維護方式，具體規則寫在各自的指引檔。
