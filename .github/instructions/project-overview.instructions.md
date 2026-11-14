---
applyTo: "**"
---

# 專案概觀

MyCAM 是雷射五軸切割的 CAD/CAM 桌面軟體：匯入 3D 模型、產生與編輯切割路徑、模擬機構運動、輸出 NC 程式，並可連線 CNC 控制器。

## 技術棧

- C# WinForms，.NET Framework 4.8 / 4.8.1，C# 7.3，Windows x64。
- 幾何核心：OpenCASCADE (OCCT) 7.6.0 官方 .NET wrapper（`reference/OCCCSharp.dll`，namespace `OCC.*`）。
- 兩個 C++ 專案（`PostTool`、`FCLTool`）以 P/Invoke 供 C# 呼叫，屬底層、很少修改。
- 第三方二進位放在 `reference/`，以 `HintPath` 參考；不修改、不升級 `reference/` 內的檔案，除非明確要求。

## 方案結構（`MyCAM.sln`）

| 專案 | 職責 |
| --- | --- |
| `MyCAM` | 主程式（WinForms exe），CAD / CAM / 模擬 / 後處理全部邏輯 |
| `OCCTool` | OCCT 幾何工具（`ShapeTool`、`GeometryTool`、`VectorTool`）與 `PostTool.dll` 的 P/Invoke 包裝 |
| `OCCViewer` | OCCT 3D 檢視器（`Viewer`） |
| `CommonData` | 共用資料定義（`CustPost` 客製後處理設定） |
| `FileTranslator` | 檔案格式轉換（`CustPost.cpf` → `CustPost`） |
| `CommonUI` | 共用 WinForms 控制項（`GroupPanel`、`LoadingBar`） |
| `LanguageManager` | 多語系字串管理 |
| `CncManager` / `CncUI` | CNC 控制器連線（Syntec OpenCNC）與其 UI |
| `FALicenseChecker` | 授權驗證 |
| `PostTool`（C++） | 五軸逆解 / 後處理計算（`FiveAxisSolver`） |
| `FCLTool`（C++） | 碰撞檢查（FCL），由 `CollisionSolver` 呼叫 |

`LicenseTool/LicenseTool.sln` 為獨立的授權產生工具，不屬於主方案。

## `MyCAM` 主程式結構

| 目錄 | 內容 |
| --- | --- |
| `MyApp.cs` | 靜態全域服務：主視窗、`AppSetting`、`Logger`、客製後處理設定 |
| `StartupForm.cs` | 主視窗 |
| `Editor/` | `CADEditor`、`CAMEditor`、`SimuEditor`（皆繼承 `EditorBase : IEditor`）、`ViewManager` |
| `Editor/Action/` | 使用者操作：繼承 `EditActionBase` / `KeyMouseActionBase` / `EditCAMActionBase`，依 CAD / CAM 分子目錄 |
| `Editor/Dialog/` | 對話框（`*Dlg`） |
| `Editor/Renderer/` | 3D 顯示（繼承 `CAMRendererBase`） |
| `Data/` | 資料模型（幾何、加工工藝、機構、物件） |
| `Helper/` | 演算法與工具（CAD、CAM、檔案 IO、模擬） |
| `PathCache/` | 路徑計算結果快取 |
| `Post/` | 後處理與 NC 輸出、五軸解算、碰撞檢查 |
| `CNC/` | 與 CNC 的檔案傳輸 |

## 慣例

### 新增使用者操作

- 新功能通常是「Action + Dialog」：在 `Editor/Action/CAD` 或 `CAM` 新增 Action 類別，必要時在 `Editor/Dialog` 新增對應的 `*Dlg`。
- 參考同目錄既有 Action 的寫法（啟動 / 結束流程、選取處理、與 Dialog 的互動），保持一致。

### 多語系

- 每個 Form / Dialog / Action 以自己的類別名稱建立 `new LanguageManager.LanguageManager( "AddPathDlg" )`。
- 字串檔位於 `Language/<類別名稱>_<語系>.xml`，支援 `zh-TW`、`zh-CN`、`en-US`、`ru-RU` 四種語系。
- 控制項文字以控制項欄位名稱為 `ID`（`<Message ID="m_rdoManual" Content="..." />`）。
- 程式中取字串使用 `m_LanguageManager.TryGetString( "Id", DEFAULT_TEXT )`，`DEFAULT_TEXT` 為類別內的 `const string`。
- **新增或修改任何使用者可見的文字時，四個語系的 XML 都要同步新增 / 修改。**

### 日誌與訊息

- 顯示給使用者的訊息使用 `MyApp.Logger.ShowOnLogPanel( szMessage, MyApp.NoticeType.Warning )`（`Hint` / `Warning` / `Error`）。

### WinForms Designer

- `*.Designer.cs` 由 Visual Studio Designer 產生；修改後必須仍能在 Designer 中正常開啟。

## 建置與執行

- 以 Visual Studio 開啟 `MyCAM.sln`，組態使用 `Debug | Any CPU` 或 `Release | Any CPU`。

### FCLTool

- `FCLTool` 編譯非常耗時，且很少修改。
- 方案的 `Debug` 組態下，`FCLTool` 對應自訂組態 `FakeDebug`：設定與 `Release` 相同（最佳化、`NDEBUG`、非 Debug 執行階段），只是輸出到 `MyCAM/bin/Debug/`。因此 **Debug 建置方案時 `FCLTool` 仍會以 Release 設定完整編譯**，同樣耗時。
- 建議做法：
  - 在 Visual Studio 中將 `FCLTool` **卸載（Unload Project）**。卸載狀態只存在個人的 `.vs/`，不影響其他人。
  - 各開發機保留一份已建置的 `FCLTool.dll` 於 `MyCAM/bin/Debug/` 與 `MyCAM/bin/Release/`；修改 `FCLTool` 時才重新載入並建置。

### 命令列建置

在 repo 根目錄執行 `Build.bat`，腳本以 `vswhere` 自動找出已安裝的 MSBuild：

| 指令 | 內容 |
| --- | --- |
| `Build.bat` | Debug，只建置 C# 專案（預設，不含 `PostTool` / `FCLTool`） |
| `Build.bat Release` | Release，只建置 C# 專案 |
| `Build.bat Debug all` | Debug，建置整個方案（含 C++，`FCLTool` 很耗時） |

### 執行期檔案

固定不變的依賴由 `CopyRef.bat` 複製到 `MyCAM/bin/Debug` 與 `MyCAM/bin/Release`（首次建置後執行一次，`reference/` 或 `Language/` 有更新時再執行）：

- `reference/` 下所有 `*.dll`、`*.exe` → `bin/<組態>/`
- `Language/` → `bin/<組態>/Resource/Language/`
- 建立空的 `bin/<組態>/Custom/Language/`（客製語系用）

可客製的設定檔不由 `CopyRef.bat` 處理，依當下需求手動放到 `bin/<組態>/Custom/`：

| 檔案 | 範本來源 | 放置位置 |
| --- | --- | --- |
| 機構參數 | `Machine/MachineData_<機型>Type.mac`（`Spindle` / `Table` / `Mix`） | `Custom/MachineData.mac` |
| 機構模型 | `Machine/<機型>/*.stl` | `Custom/<機型>/`（程式依 `MachineData.mac` 的機型讀取對應資料夾） |
| 客製後處理 | `PostConfig/CustPost.cpf` | `Custom/CustPost.cpf` |

## Git 流程

- 分支命名：`AUTO-<議題編號>_<類型>_<簡述>`，類型 `f` 為功能、`b` 為 bug（例：`AUTO-23201_b_CannotOutputFrogLeapMacro`）。
- Commit 訊息需明確描述修改內容，中英文皆可。
