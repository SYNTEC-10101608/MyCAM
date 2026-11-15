# MyCAM 開發指引

本目錄是團隊的開發規範，同時也是 AI 工具（GitHub Copilot、Claude Code 等）的專案指引。
產生或修改程式碼時，必須遵守下列文件中的規則。

## 指引目錄

| 文件 | 內容 | 適用檔案 |
| --- | --- | --- |
| [專案概觀](instructions/project-overview.instructions.md) | 產品、方案結構、主程式架構、多語系等慣例、建置與執行、Git 流程 | 全部 |
| [Coding Style：通用規則](instructions/coding-style-general.instructions.md) | 編碼、縮排、檔名、註解、修改既有程式碼的原則 | 全部 |
| [Coding Style：C#](instructions/coding-style-csharp.instructions.md) | C# 格式、命名、程式結構、語言用法 | `*.cs` |
| [Coding Style：C / C++](instructions/coding-style-cpp.instructions.md) | C / C++ 格式、命名、語言用法 | `*.c` `*.cpp` `*.h` `*.hpp` |
| [UI 風格](instructions/ui-style.instructions.md) | 主題 token、控制項白名單、對話框外觀層與版面、多語系自動套用、檢查腳本 | `*.cs`（畫面） |

格式類規則同時寫在 repo 根目錄的 `.editorconfig`，衝突時以 `.editorconfig` 為準。

## 維護方式

- 每份指引放在 `.github/instructions/`，檔名為 `<主題>.instructions.md`，開頭以 `applyTo` 指定適用檔案。
- 新增指引時，同步在上方「指引目錄」表格加一列，並在根目錄 `CLAUDE.md` 加一行 `@` 引用。
- 本檔只放目錄與跨主題的總則，具體規則寫在各自的指引檔。
