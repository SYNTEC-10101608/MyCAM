# 建置與執行環境

建置、準備執行環境時讀本文件。以 Visual Studio 開啟 `MyCAM.sln`，組態使用 `Debug | Any CPU` 或 `Release | Any CPU`。

## FCLTool

1. `FCLTool` 編譯非常耗時，且很少修改。
2. 方案的 `Debug` 組態下，`FCLTool` 對應自訂組態 `FakeDebug`：設定與 `Release` 相同（最佳化、`NDEBUG`、非 Debug 執行階段），只是輸出到 `MyCAM/bin/Debug/`。因此 **Debug 建置方案時 `FCLTool` 仍會以 Release 設定完整編譯**，同樣耗時。
3. 建議做法：
   1. 在 Visual Studio 中將 `FCLTool` **卸載（Unload Project）**。卸載狀態只存在個人的 `.vs/`，不影響其他人。
   2. 各開發機保留一份已建置的 `FCLTool.dll` 於 `MyCAM/bin/Debug/` 與 `MyCAM/bin/Release/`；修改 `FCLTool` 時才重新載入並建置。

## 命令列建置

在 repo 根目錄執行 `Build.bat`，腳本以 `vswhere` 自動找出已安裝的 MSBuild：

| 指令 | 內容 |
| --- | --- |
| `Build.bat` | Debug，只建置 C# 專案（預設，不含 `PostTool` / `FCLTool`） |
| `Build.bat Release` | Release，只建置 C# 專案 |
| `Build.bat Debug all` | Debug，建置整個方案（含 C++，`FCLTool` 很耗時） |

從 PowerShell 或 Git Bash 執行時需加上目前目錄：`.\Build.bat`（PowerShell）、`./Build.bat`（Git Bash）。
`cmd /c Build.bat` 在設有 `NoDefaultCurrentDirectoryInExePath` 的環境（如 Claude Code）會找不到檔案，需寫成 `cmd /c .\Build.bat`。
`CopyRef.bat` 同理。

## 執行期檔案

固定不變的依賴由 `CopyRef.bat` 複製到 `MyCAM/bin/Debug` 與 `MyCAM/bin/Release`（首次建置後執行一次，`reference/` 或 `Language/` 有更新時再執行）：

1. `reference/` 下所有 `*.dll`、`*.exe` → `bin/<組態>/`
2. `Language/` → `bin/<組態>/Resource/Language/`
3. 建立空的 `bin/<組態>/Custom/Language/`（客製語系用）

可客製的設定檔不由 `CopyRef.bat` 處理，依當下需求手動放到 `bin/<組態>/Custom/`：

| 檔案 | 範本來源 | 放置位置 |
| --- | --- | --- |
| 機構參數 | `Machine/MachineData_<機型>Type.mac`（`Spindle` / `Table` / `Mix`） | `Custom/MachineData.mac` |
| 機構模型 | `Machine/<機型>/*.stl` | `Custom/<機型>/`（程式依 `MachineData.mac` 的機型讀取對應資料夾） |
| 客製後處理 | `PostConfig/CustPost.cpf` | `Custom/CustPost.cpf` |

## 截圖環境

UI 的執行時檢查（`Tools\UICheck\UICheck.bat runtime`）要在 1920×1080、縮放 100% 的主螢幕上執行，因為產品依螢幕的工作區域最大化主視窗、置中對話框。

1. 主螢幕已符合時直接執行。
2. 不符合時，在該開發機以系統管理員權限安裝一次虛擬螢幕驅動：`winget install Parsec.ParsecVDD`。之後檢查期間會自動建立虛擬螢幕、設為主螢幕與縮放 100%（直式旋轉為 1080×1920），結束後還原並移除，不需要再以系統管理員權限執行。
3. 檢查期間工作列與新開的視窗會移到看不見的虛擬螢幕，結束後回到原螢幕；中途中斷時，驅動約一秒後自動移除虛擬螢幕，Windows 會把原螢幕設回主螢幕。
