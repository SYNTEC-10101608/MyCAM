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

建置、準備執行環境或 worktree 時，讀 [`.github/project/build.md`](../project/build.md)。

## Git 流程

- 分支命名：`AUTO-<議題編號>_<類型>_<簡述>`，類型 `f` 為功能、`b` 為 bug（例：`AUTO-23201_b_CannotOutputFrogLeapMacro`）。
- Commit 訊息需明確描述修改內容，中英文皆可。

### 多分支並行（git worktree）

- 以 Claude Code 建立的 worktree 位於 `.claude/worktrees/<名稱>/`（已列入 `.gitignore`）。
- Claude Code 自動建立的分支名稱為 `worktree-<名稱>`，建立後以 `git branch -m` 改為上述命名規則。
- 建立前先 `git fetch origin`；fetch 失敗（如未連上公司網路）時，新分支會以本機舊的 `origin/<預設分支>` 為基準。
- 未追蹤的檔案不會出現在新 worktree，需要的檔案列在 repo 根目錄的 `.worktreeinclude`，建立時由主 checkout 複製：
  `.claude/settings.local.json`、`FCLTool.dll`、`PostTool.dll`、`bin/<組態>/Custom/`。
- `CLAUDE.local.md` 不需複製：worktree 位於 repo 底下，Claude Code 會從上層的主 checkout 載入。
  也因此不要在 repo 的上層目錄放 `CLAUDE.md` / `CLAUDE.local.md` 的備份，否則會被重複載入。
- 新 worktree 第一次建置前執行 `CopyRef.bat`。
