# Review

## 第 1～6 回合（VS Copilot，2026-11-21～2026-11-22）

新規範之前的整體審查，編號為 `R1-01` 等（與本議題的 REQ 無關），不轉換為 `REV`。快照在 git 歷史的
`Doc/UIStyleUnify/8_ReviewRound1.md`～`17_ReviewRound6.md`，結論與決議已整理到 [Discussion.md](Discussion.md)
的 DEC-1、DEC-4、DEC-6、DEC-10；人工確認後的增量審查（第 6 回合）確認沒有新缺陷。

## 第 7 回合

併入 `master` 與依新規範調整後的自我 review，重點為衝突的解法、`master` 修正的保留、指引移植與截圖環境（STEP-12、STEP-13）。

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD 為本回合修正後的分支 |
| 工具與模型 | Claude Code |
| 日期 | 2026-11-26 |

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-1 | 1 | 必修 | `MyCAM/MyCAM.csproj` | `master` 的 `ApplicationIcon` 指向不存在的 `MyCAM\AppIcon.ico`（檔案在 `Resource\`），乾淨的 checkout 建置失敗 | 已修正：本分支改為實際位置；`master` 另開 bug（TODO-23、DEC-14） |
| REV-2 | 4 | 必修 | `MyCAM/StartupForm.Designer.cs`、`.resx` | `master` 以 `.resx` 的 `$this.Icon` 指定表單圖示，違反「畫面的 `.resx` 只有設計時資訊」（`ResxResource`） | 已修正：改由 `Properties.Resources.AppIcon` 指定 |
| REV-3 | 3 | 必修 | `CncUI/Panel/CNCInfoPanel.cs` | `master` 隱藏第二旋轉軸時靠自繪跳過隱藏的軸；本議題的軸點是 `ColorSwatch`，直接合併會留下 C 軸色塊 | 已修正：一併隱藏 `m_swtC`，登錄行為比對例外 |
| REV-4 | 3 | 必修 | `MyCAM/Editor/Dialog/LockMSAxisDlg.cs` | 自動合併的 `m_rbtnMasterAuto` 仍是舊名，主從軸條件需取 `master` 的修正 | 已修正：條件取 `master`、名稱取本議題；全 repo 無 `m_rbtn`、`m_dbtn` 殘留 |
| REV-5 | 1 | 必修 | `Tools/UICheck/LayoutRunner.cs` | `master` 把 `SetJogEnabled` 改為兩個參數，`STATE_MAP` 以反射找不到方法，`CNCPanel` 的連線狀態無法開啟 | 已修正：傳入軸數，並新增四軸狀態 |
| REV-6 | 1 | 必修 | `Tools/UICheck` | 執行時檢查在非規格螢幕上被 Form 限制大小而誤報（`DesignSupportBracketDlg` 的 `ControlOutside`），主視窗截圖也不是實際畫面 | 已修正：在規格螢幕上執行（STEP-13） |
| REV-7 | 1 | 提問 | `Tools/UICheck/VirtualDisplayRun.cs` | 虛擬螢幕的建立、100% 縮放與還原尚未在實際驅動上執行過；隱藏桌面上最大化是否取得含工作列的工作區域也待實測 | 結案：在 1920×1080 虛擬主螢幕上 `runtime` 全部通過，最大化的客戶區 1920×1009 與 Designer 相同；加入 Parsec 副螢幕後其工作區域 1920×1032（有工作列），設定失敗時副螢幕被移除且主螢幕不變；直式改以高大於寬的視窗呈現（DEC-13 第 7 項） |
| REV-8 | 5 | 提問 | `CncUI/Dialog/CNCOffsetDialog.cs` | 四軸時隱藏第五列，卡片底部留一列空白，對話框大小不變；`CNCPanel`、`CNCInfoPanel` 隱藏後版面仍整齊 | 不修正：維持現狀，與 `CNCInfoPanel` 隱藏後的呈現一致，對話框大小不隨控制器改變（DEC-14） |
| REV-9 | 6 | 建議 | 本回合的 commit | STEP-12 的前幾個 commit 標題沒有 `(STEP-12)` | 已修正：push 前改寫本機 commit 標題 |
| REV-10 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs` | 第一版把虛擬螢幕設為主螢幕並移動其他螢幕；在闔蓋的筆電上移動面板失敗後，已寫入的配置仍被套用，面板被卸離且無法掛回，遠端畫面全黑 | 已修正：不更動既有螢幕，只加入並設定虛擬副螢幕，失敗只移除它；結束後檢查其他螢幕（DEC-13 第 4 項）。本機以 Parsec 虛擬螢幕恢復畫面 |
| REV-11 | 5 | 建議 | `MyCAM/Layout/MainLayout.cs` | 使用者人工檢視：橫式加工頁的監控卡片上緣比右側「加工控制」卡片低 9 px | 已修正：底部列比例 0.25 → 0.265，Designer 與主視窗區域基準同步更新（DEC-14） |

## 第 8 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD `61fd9c20`（整個分支） |
| 工具與模型 | GitHub Copilot CLI（`Invoke-CopilotReview.ps1`）、gpt-6.2-sol、推理強度 high |
| 日期 | 2026-11-26 |

| 門檻 | 結果 |
| --- | --- |
| `Build.bat Debug`、`Build.bat Release` | 施工方於受審 HEAD 執行通過，本回合未重跑 |
| `Tools\UICheck\UICheck.bat` | Reviewer 重跑通過：34 個 Designer 檔、0 個問題 |
| `Tools\UICheck\DesignerDiff.ps1 -Base bd0fa270 -Head 61fd9c20` | Reviewer 執行語意比對：39 個檔，38 個有語意差異、1 個僅格式差異 |
| `Tools\UICheck\UICheck.bat runtime` | 施工方於受審 HEAD 使用 1920×1080 虛擬主螢幕執行通過；本回合依請求不執行可能更動顯示設定的指令，未重跑 |
| `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | 施工方於受審 HEAD 執行通過，本回合未重跑 |

靜態核對確認 `master` 的主從軸條件、三點校正的輸入通知、四軸時隱藏第二旋轉軸，以及主視窗與執行檔圖示均有保留；`TEST-19` 的人工操作與 CNC 實機確認仍待執行。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-12 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs:189` | 建立虛擬螢幕後，程式把首個不在初始清單的 GDI 裝置當成這次新增的 Parsec 螢幕，未核對裝置身分。若同時有其他螢幕重新掛載，解析度、位置與縮放可能寫到該螢幕，且清理只會移除本次新增的 Parsec 裝置，違反 DEC-13 不更動既有螢幕的要求。寫入前應確認目標是本次新增的虛擬螢幕，無法唯一確認就中止。 | 已修正：新螢幕須在 Parsec 顯示卡上且唯一，否則不做任何設定（DEC-15） |
| REV-13 | 3 | 必修 | `Tools/UICheck/LayoutRunner.cs:147` | 啟動程式以 `UICHECK_SCREEN` 傳入規格螢幕，但 `LayoutRunner` 沒有讀取，仍依預設位置顯示對話框，面板宿主也固定從 `(0,0)` 開啟。只有副螢幕合規時，視窗仍可能落在不合規的主螢幕，讓尺寸檢查及截圖偏離規格。應依指定螢幕定位，並確認視窗實際所在螢幕，保留 `-CurrentScreen` 的快速檢查模式。 | 已修正：`LayoutRunner` 以指定螢幕的工作區域註冊置中區域，面板承載視窗放到該螢幕（DEC-15） |
| REV-14 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs:447` | `GetOverlappedResultEx` 逾時並不會取消背景 I/O，但 `finally` 隨即關閉事件並釋放 `OVERLAPPED` 和緩衝區。仍在執行的 I/O 可能存取已釋放的記憶體，使檢查程式崩潰並中斷清理。應區分同步完成、立即失敗及待完成；逾時時取消該要求，等到 I/O 確實結束再釋放資源。 | 已修正：區分立即完成、立即失敗與等待中，逾時取消 I/O 並等待結束後才釋放（DEC-15） |
| REV-15 | 3 | 必修 | `CncUI/Panel/CNCInfoPanel.cs:143` | 直式版面依 `Top` 分列，新加入的軸色塊比同軸標籤低 7 px，因此五個色塊各被排成一列。之後 `PlaceAxisMarkers` 把色塊移回標籤旁，卻留下五列空白，撐高座標卡片並擠壓下方卡片。建立列格線時應排除由 `PlaceAxisMarkers` 定位的色塊，並確認四軸及五軸的直式畫面。 | 已修正：分列時排除軸色塊，直式四軸、五軸重新截圖確認（DEC-15） |
| REV-16 | 3 | 建議 | `MyCAM/Resource/IconSource/RenderIcons.ps1:47` | `Head` 用目前文化格式化路徑座標；在 `ru-RU` 實測，小數逗號與座標分隔逗號混用，WPF 無法解析路徑。重新產生圖示可能在部分 PNG 已覆蓋後中止，但不影響本分支已提交的圖示。應使用不隨文化變動的數字格式，並在使用小數逗號的環境驗證。 | 已修正：`Head`、`Burst` 以 InvariantCulture 格式化，ru-RU 下驗證輸出小數點（DEC-15） |

施工方查證上列發現時另外發現（REV-17、REV-18），一併記在本回合：

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-17 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs` | 加入的虛擬副螢幕本來就是 1920×1080，程式仍重設模式與位置，被 Windows 拒絕（`DISP_CHANGE_FAILED`）；「加入虛擬副螢幕」的路徑因此從未成功過，先前的驗證都走主螢幕路徑而沒發現 | 已修正：已是規格模式時不下設定，只在不符時改模式（DEC-15） |
| REV-18 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs` | Windows 依記住的螢幕組合配置，在加入虛擬螢幕時把它設為主螢幕並移動既有螢幕，工具本身沒有下設定也會發生，違反 DEC-13 不更動既有螢幕的前提 | 已修正：加入後既有螢幕的位置或主螢幕有變動就中止並移除虛擬螢幕，Windows 隨即還原（DEC-15） |

## 第 9 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD `419fad57`（僅審 `61fd9c20` → `419fad57` 的差異） |
| 工具與模型 | GitHub Copilot CLI（`Invoke-CopilotReview.ps1`）、gpt-6.2-sol、推理強度 high |
| 日期 | 2026-11-26 |

| 門檻 | 結果 |
| --- | --- |
| `Build.bat Debug` | 施工方於受審 HEAD 執行通過，本回合未重跑 |
| `Build.bat Release` | 上一回合受審 HEAD 通過；本回合受審 HEAD 未提供結果，未重跑 |
| `Tools\UICheck\UICheck.bat` | Reviewer 重跑通過：34 個 Designer 檔、0 個問題；本輪未改 Designer 或 C++ 檔 |
| `Tools\UICheck\UICheck.bat runtime` | 施工方於受審 HEAD 在 1920×1080 主螢幕執行通過，本回合依請求未重跑；此結果不涵蓋新增虛擬副螢幕的路徑 |
| `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | 施工方於受審 HEAD 執行通過，本回合未重跑 |

已符合 1920×1080 的虛擬螢幕不會再重設顯示模式。`CNCInfoPanel` 只在直式分列時略過色塊，橫式仍使用原版面快照，四軸仍隱藏 C 軸標籤與色塊。`Head` 與 `Burst` 在 `en-US`、`ru-RU` 產生相同路徑，並確認 WPF 可剖析，未覆寫圖檔。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-19 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs:212` | 輪詢一看到唯一新掛載的 Parsec 螢幕就採用，並未核對它是否由本次 `IOCTL_ADD` 建立。若另一個 Parsec 用戶端的螢幕先掛載、本次新增的螢幕尚未就緒，解析度或縮放就可能寫到另一個螢幕，違反 DEC-13 不對其他螢幕下設定的決策。寫入前應將顯示目標與本次新增請求回傳的索引核對，不能確認就中止。 | 不依建議修正，嚴重度改為建議：Parsec 的索引沒有公開方式對應到 Windows 的顯示裝置；改以全系統的鎖讓同一台電腦一次只跑一個檢查，其他 Parsec 用戶端同時加螢幕記錄為限制（DEC-16） |
| REV-20 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs:244` | 新增的變更檢查只比對 GDI 裝置名稱、位置與主螢幕狀態，未比對既有螢幕的解析度、方向、更新率或縮放。若 Windows 套用記住的配置時只改了顯示模式或縮放，檢查仍會繼續，違反 DEC-15「Windows 改動既有螢幕即中止」。應在新增前記錄既有螢幕的模式與 DPI，並在設定虛擬螢幕前及啟動檢查前確認未變。 | 已修正：既有螢幕另比對解析度、方向、更新率與 DPI，啟動檢查前與結束後都檢查（DEC-16） |
| REV-21 | 3 | 必修 | `Tools/UICheck/LayoutRunner.cs:287` | 明確指定 `UICHECK_SCREEN` 卻找不到該螢幕時，版面工具只記訊息並回到預設位置；顯示視窗後也未驗證實際所在的螢幕。指定螢幕若在啟動過程中消失，對話框和 CNC 面板可能在不符規格的螢幕上檢查並回報通過，違反 SPEC-20 要求在規格螢幕開啟。僅未指定目標時保留 `-CurrentScreen` 的預設行為；有指定時，找不到目標或視窗落在其他螢幕都應判為失敗。 | 已修正：指定的螢幕找不到時 exit 2；對話框、CNC 面板與主視窗開啟後不在該螢幕上判為問題（DEC-16） |
| REV-22 | 3 | 建議 | `Tools/UICheck/VirtualDisplayRun.cs:491` | 等待逾時與取消之間，I/O 仍可能正常完成，但程式忽略最後一次 `GetOverlappedResult` 的結果，一律回傳失敗。若完成的是 `IOCTL_ADD`，程式會丟掉已建立螢幕的索引，跳過明確的移除，只能依賴驅動程式逾時回收。應以最後的完成狀態為準，成功就保留結果並納入清理。 | 已修正：取消後以最後一次 `GetOverlappedResult` 為準，成功就回傳結果，加入的螢幕在結束時移除（DEC-16） |

## 第 10 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD `d7fb29fd`（僅審 `419fad57` → `d7fb29fd` 的差異） |
| 工具與模型 | GitHub Copilot CLI（`Invoke-CopilotReview.ps1`）、gpt-6.2-sol、推理強度 high |
| 日期 | 2026-11-27 |

| 門檻 | 結果 |
| --- | --- |
| `Build.bat Debug`、`Build.bat Release` | 施工方於受審 HEAD 執行通過，本回合未重跑 |
| `Tools\UICheck\UICheck.bat` | Reviewer 重跑通過：34 個 Designer 檔、0 個問題；本回合未改 Designer 或 C++ 檔 |
| `Tools\UICheck\UICheck.bat runtime` | 施工方於受審 HEAD 在終端機的 1920×1080 主螢幕執行通過，Reviewer 未重跑；受審 HEAD 未實測加入 Parsec 虛擬副螢幕的路徑，現有的 Windows 記憶配置會觸發既有螢幕變更的中止保護（REV-18） |
| `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | 施工方於受審 HEAD 執行通過，本回合未重跑 |

靜態核對確認：終端機旗標會在開啟 Parsec 驅動之前阻止新增虛擬螢幕；取消重疊 I/O 後會採用最後一次完成結果，若新增成功便保留索引供清理。客戶螢幕支援維持列後續，Copilot reviewer 的啟動方式也與 DEC-16 一致。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-23 | 2 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs:46` | 全系統鎖只包單次 `UICheckDisplay.exe` 子命令，兩個 `UICheck.bat runtime` 仍可在語系與截圖案例之間交錯，也會在取得鎖前編譯、釋鎖後刪除同一建置目錄的 runner。第二個檢查可能在等待前就因檔案使用中而失敗，或清掉第一個檢查還要用的 runner。應在整次 `runtime` 的入口取得同一把鎖，涵蓋編譯至清理，並用同一工作目錄同時啟動兩次完整指令驗證。 | 已修正：新增 `CheckRuntime.ps1` 為 `runtime` 入口，整次持有同一把鎖並以 `UICHECK_LOCK_HELD=1` 告知子程序；同一工作目錄同時啟動兩次，後者等待（DEC-17） |
| REV-24 | 2 | 必修 | `Tools/UICheck/UICheck.bat:21` | 終端機沒有規格螢幕時，版面子指令傳回 2，但 `runtime` 入口把它轉成一般失敗 1，並繼續執行數值框檢查及 13 次主視窗截圖嘗試。呼叫端無法用 exit 2 區分環境未就緒，也不符合 DEC-16 的中止行為；應在第一個螢幕準備失敗時停止整次 `runtime` 並保留 exit 2。 | 已修正：`CheckRuntime.ps1` 遇到第一個 exit 2 即停止，`UICheck.bat` 保留 2；以未建置的情境驗證（DEC-17） |
| REV-25 | 3 | 必修 | `Tools/UICheck/VirtualDisplayRun.cs:252` | 新增的 DPI 快照忽略 `GetMonitor` 是否找到該螢幕，底層也未檢查 `GetDpiForMonitor` 的 HRESULT；例如前後都找不到螢幕時，兩筆 DPI 都是 0，仍可被判為縮放未變。應區分有效 DPI 與讀取失敗，無法取得任一既有螢幕的有效基準或核對值時，應指出裝置並中止虛擬螢幕檢查。 | 已修正：DPI 讀取失敗視為找不到螢幕，DPI 為 0 的螢幕一律判為有變動，中止訊息列出裝置（DEC-17） |
| REV-26 | 3 | 必修 | `Tools/UICheck/MainWindowRunner.cs:167` | 主視窗 runner 只驗證主視窗所在螢幕，之後以 `Show` 或 `ShowDialog` 開啟的對話框都未檢查；預期矩形超出工作區時，置中檢查仍可能印出 `OK`。應把指定螢幕傳給這兩條路徑，在對話框可見時核對實際所在螢幕，與是否略過置中比對分開判定。 | 已修正：`Show` 與 `ShowDialog` 開啟的對話框都核對所在螢幕，與置中比對分開判定（DEC-17） |
| REV-27 | 6 | 建議 | `Doc/AUTO-23254/TestPlan.md:42` | TEST-18 仍把沒有規格螢幕一律描述為加入 Parsec 副螢幕，第 8 節也未區分一般電腦與終端機，讀者可能用錯準備方式。應補上終端機在無合規螢幕時不呼叫 Parsec、由 `UICheck.bat runtime` 回傳 2 的驗收案例，並區分受審 HEAD 的結果與前回合的副螢幕驗證。 | 已修正：TEST-18 補終端機中止與 `runtime` 回傳 2 的條件，第 8 節第 5、6 項區分兩類電腦並記錄 Parsec 成功流程未實測（TODO-25）（DEC-17） |
| REV-28 | 6 | 建議 | `.github/project/tool-uicheck.md:26` | DEC-16 已接受鎖只協調 UICheck、無法排除其他 Parsec 用戶端同時加入的限制，但操作者查閱的截圖指引沒有提到。應在 Parsec 使用段落補一句：檢查期間避免讓其他 Parsec 用戶端新增虛擬螢幕，以免工具誤認新增的顯示裝置。 | 已修正：`tool-uicheck.md` 第 2.4 節狀況表補上（DEC-17） |

## 第 11 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD `a608bc9a`（僅審 `d7fb29fd` → `a608bc9a` 的差異，排除僅新增第 10 回合報告的 `b2cf1614`） |
| 工具與模型 | GitHub Copilot CLI（`Invoke-CopilotReview.ps1`）、gpt-6.2-sol、推理強度 high |
| 日期 | 2026-11-27 |

| 門檻 | 結果 |
| --- | --- |
| `Build.bat Debug`、`Build.bat Release` | 施工方於受審 HEAD 執行通過；Reviewer 未重跑，本回合未改 C++ |
| `Tools\UICheck\UICheck.bat` | Reviewer 重跑通過：34 個 Designer 檔、0 個問題 |
| `Tools\UICheck\UICheck.bat runtime` | 施工方於受審 HEAD 在終端機的 1920×1080 主螢幕執行通過；同一工作目錄第二次同時啟動會等待，未建置時回傳 2。Reviewer 依請求未執行；Parsec 成功流程依 DEC-17 不在受審機台驗證，留待 TODO-25 於一般工作電腦實測 |
| `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | 施工方於受審 HEAD 執行通過；Reviewer 未重跑 |

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-29 | 3 | 必修 | `Tools\UICheck\CheckRuntime.ps1:95` | 子程序只憑 `UICHECK_LOCK_HELD=1` 略過取鎖；若只終止持鎖的 PowerShell，Windows 不保證一併終止仍在執行畫面檢查或清理的子程序。第二次 `runtime` 可在前一次子程序仍使用螢幕或共用 runner 時取得鎖並開始，造成畫面互相干擾或檔案衝突。應讓接手鎖前能確定前次子程序已停止，例如讓持鎖程序管理整個子程序樹的生命週期。 | 已修正（改設計）：拿掉整次鎖與 `UICHECK_LOCK_HELD`，用到規格螢幕的子程序一律自己取鎖，外層被終止時鎖仍由執行中的子程序持有（DEC-18） |
| REV-30 | 3 | 必修 | `Tools\UICheck\CheckRuntime.ps1:77` | 整次鎖只在新入口取得，單獨執行文件仍提供的 `CheckRuntimeLayout.ps1 -CurrentScreen` 會直接在同一建置目錄編譯及刪除 `UICheckRunner.exe`、`UICheckHidden.exe` 和 `UICheckDisplay.exe`，連 `-CurrentScreen` 不會編譯的顯示 runner 也會刪除。因此即使 `runtime` 持鎖，快速檢查仍可能覆寫或刪除它下一步需要的 runner。應在可單獨呼叫的入口也保護編譯到清理，或使用各次執行獨立的 runner 檔案。 | 已修正（改設計）：runner 以各次執行的程序編號命名、只清理自己的檔案，開始時清除被中斷留下的檔案；與 `runtime` 同時執行 `-CurrentScreen` 驗證互不影響（DEC-18） |
| REV-31 | 3 | 必修 | `Tools\UICheck\VirtualDisplayRun.cs:157` | 讀不到既有螢幕的 DPI 時，初始快照已標記為 0，卻要等新增 Parsec 螢幕並嘗試設定模式與縮放後才檢查。初始狀態已無法核對時仍會觸發 Windows 套用記住的顯示配置，而且事後也無法確認既有螢幕已恢復。應在呼叫 `IOCTL_ADD` 前列出 DPI 基準無法讀取的裝置並回傳 2；新增後再核對有效的快照。 | 已修正：加入虛擬螢幕前就列出讀不到縮放的既有螢幕並回傳 2，不加入（DEC-18） |
| REV-32 | 3 | 建議 | `Tools\UICheck\MainWindowRunner.cs:345` | `Show` 與 `ShowDialog` 都會在螢幕核對失敗後，只要置中符合或略過比對，就對同一個對話框再印出 `OK`。結束碼雖仍是 1，操作者卻會看到互相矛盾的結果；應只在沒有失敗時印出 `OK`，或把置中結果明確標成另一項檢查。 | 已修正：只在沒有失敗時印出 `OK`（DEC-18） |
| REV-33 | 3 | 建議 | `Tools\UICheck\UICheck.bat:33` | exit 2 的總結把所有中止都寫成「未執行檢查，未建置或沒有規格螢幕」，但已通過前面幾項後仍可能有截圖子指令回傳 2，鎖等待逾時或 runner 編譯失敗也會回傳 2。應保留 exit 2，把總結改為「執行時檢查未完成，原因見上方」，避免把部分完成或其他環境錯誤誤報為未執行。 | 已修正：總結改為「執行時檢查未完成，原因見上方」（DEC-18） |

## 第 12 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD `44d3ceea`（僅審 `a608bc9a` → `44d3ceea` 的 DEC-18 修改，排除僅新增第 11 回合報告的 `c20926e8`） |
| 工具與模型 | GitHub Copilot CLI（`Invoke-CopilotReview.ps1`）、gpt-6.2-sol、推理強度 high |
| 日期 | 2026-11-27 |

| 門檻 | 結果 |
| --- | --- |
| `Build.bat Debug`、`Build.bat Release` | 施工方於受審 HEAD 執行通過；Reviewer 未重跑，本回合未改 C++ |
| `Tools\UICheck\UICheck.bat` | Reviewer 重跑通過：34 個 Designer 檔、0 個問題 |
| `Tools\UICheck\UICheck.bat runtime` | 施工方於受審 HEAD 在終端機的 1920×1080 主螢幕執行通過；同時執行 `-CurrentScreen` 快速檢查與主視窗檢查，三者皆通過且未留下 runner 檔，未標記或未建置時回傳 2。Reviewer 未執行；依 DEC-17 第 3 項，Parsec 成功流程不在受審機台驗證，留待 TODO-25 於一般工作電腦實測 |
| `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | 施工方於受審 HEAD 執行通過；Reviewer 未重跑 |

靜態核對確認：加入 Parsec 螢幕前會拒絕無法讀取的 DPI 基準（REV-31）；非模態與模態對話框核對失敗後不再印出 `OK`（REV-32）；exit 2 的總結指出執行時檢查未完成（REV-33）。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-34 | 2 | 建議 | `Tools\UICheck\UICheck.bat:11` | 未標記或標記無效時，批次檔先執行靜態檢查，才由 `CheckRuntime.ps1` 印出提示並回傳 2，與 DEC-18 第 3 項及指引的「不執行任何檢查」不一致。建議只對 `runtime` 入口在靜態檢查前驗證標記，保留不帶參數的靜態檢查行為。 | 已修正：`runtime` 時先檢查標記，未設定就不執行靜態檢查，直接說明並回傳 2（DEC-19） |
| REV-35 | 3 | 必修 | `Tools\UICheck\SyncLanguage.ps1:14` | 同一建置目錄的兩次檢查都判定語系檔過期時，後一次可在前一次 runner 讀取 XML 時直接覆寫；同時建目錄或讀取目的檔雜湊的失敗也不在重試範圍內。除了讓正常的並行檢查中止，若共用語系檔讀取失敗回退成預設文字，也可能漏報文字裁切。應只對同一建置目錄的語系同步加短鎖，在鎖內建立目錄及重新核對內容，再以完整的暫存檔原子替換，避免讀到寫入中的內容。 | 已修正：語系同步依建置目錄加鎖，鎖內建立目錄與重新比對，以暫存檔整檔替換、使用中時重試（DEC-19） |
| REV-36 | 3 | 建議 | `Tools\UICheck\RunnerFiles.ps1:16` | 清理只查檔名中的 PowerShell PID；若該程序在規格螢幕子程序等鎖時被終止，下一次檢查會把同一組尚未啟動的 hidden／layout runner 當成殘留。仍在執行的 display runner 本身雖然無法被刪除，取得鎖後卻可能因要啟動的檔案已刪除而失敗。建議把同組仍在執行的子程序納入存活判定，等整組無程序使用再清理。 | 已修正：同一組 runner 只要有檔案使用中就整組保留，以執行中的 runner 驗證（DEC-19） |
| REV-37 | 3 | 建議 | `Tools\UICheck\RunnerFiles.ps1:13` | 清理把所有不符 PID 格式的 `UICheck*.exe` 都視為舊 runner，未驗證是否為工具曾產生的檔名，也未限制只列檔案。建置目錄內同前綴的其他診斷程式可能被無聲刪除；建議只列出檔案，且刪除範圍限於三支腳本使用的 runner 名稱及其舊版檔名。 | 已修正：只列檔案，只刪工具的五種 runner 名稱與其舊版檔名（DEC-19） |
| REV-38 | 3 | 建議 | `Tools\UICheck\CheckRuntime.ps1:50` | 同時執行的 `runtime` 預設把主視窗截圖寫到同一個 `%TEMP%\UICheck`，且檔名只區分案例。規格螢幕鎖只防止同時畫面操作，不能防止逐項覆寫；即使兩次檢查都通過，不同 worktree 的截圖仍可能混在同一份結果裡。建議預設使用每次執行獨立的截圖目錄並印出其位置；並行時若明確指定目錄，也應使用不同路徑。 | 已修正：未指定截圖資料夾時使用各次執行的資料夾並印出位置；指定同一資料夾時仍會覆寫，寫入 `tool-uicheck.md`（DEC-19） |
| REV-39 | 4 | 建議 | `Tools\UICheck\RunnerFiles.ps1:1` | 新增的 PowerShell 腳本缺少 `.editorconfig` 規定的 UTF-8 BOM；目前只有 ASCII，不影響執行，但日後加入非 ASCII 文字時，Windows PowerShell 5.1 可能以 ANSI 解讀。建議補上 BOM，與呼叫它的三支腳本一致。 | 已修正：`CheckRuntime.ps1`、`RunnerFiles.ps1` 補 BOM（DEC-19） |

## 第 13 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 比對基準 | `bd0fa270`；受審 HEAD `1181e066`（整個議題分支） |
| 工具與模型 | GitHub Copilot CLI（`Invoke-CopilotReview.ps1`）、gpt-6.2-sol、推理強度 high |
| 日期 | 2026-11-27 |

| 門檻 | 結果 |
| --- | --- |
| `Build.bat Debug` | 施工方於受審 HEAD 執行通過（0 個錯誤）；Reviewer 未重跑 |
| `Build.bat Release` | 施工方於受審 HEAD 執行通過（0 個錯誤，請求檔漏寫）；Reviewer 未重跑。本分支未改 C++ 專案 |
| `Doc\AUTO-23254\Tests\RunIssueChecks.bat` | 施工方於受審 HEAD 執行通過；Reviewer 未重跑 |
| `Tools\UICheck\UICheck.bat` | Reviewer 重跑通過：34 個 Designer 檔、0 個問題 |
| `Tools\UICheck\UICheck.bat runtime` | Reviewer 依請求未執行；施工方在 DEC-19 記錄終端機流程通過。依 DEC-17 第 3 項，Parsec 成功流程留待 TODO-25 於一般工作電腦驗證 |
| `Tools\UICheck\DesignerDiff.ps1 -Base bd0fa270 -Head 1181e066` | Reviewer 執行語意比對：39 個檔，38 個有語意差異、1 個僅格式差異 |

`.github\scripts\`、`.claude\skills\` 與團隊的 `process-docs.md`、`process-issue.md`、`process-review.md` 在本分支未變動；專案層新增的 `coding-*`、`tool-*` 檔案分類及載入時機符合入口索引。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-40 | 2 | 建議 | `.github\team\process-ai-review.md:15` | DEC-14 第 8 項已同意釐清工具權限與人員決策的界線，DEC-16 第 8 項也記錄了統一 Copilot 啟動方式的理由，但 `Goal.md` 第 3 節只列專案指引與依既有流程調整分支，未納入修改全團隊流程。兩項內容都可能長期有效，但會隨 UI MR 一起改變其他議題的作法，建議另案交付；若確認留在本 MR，請將團隊規範補充明列在範圍及 STEP，Copilot 指令細節由 `tool-copilot-cli.md` 單一維護。 | 已修正：留在本 MR，`Goal.md` 範圍與 `Implementation.md` 異動範圍寫明兩項補充與理由（DEC-20） |
| REV-41 | 2 | 建議 | `MyCAM\Editor\Dialog\PatternSettingDlg.Designer.cs:168` | DEC-6 同意統一數值框，但圓直徑欄位未覆寫共用 `Maximum = 99999`，而基準版只要求可解析且大於零，原本可輸入的 `100000` 現在會在驗證時被拒。`BehaviorExceptions.txt` 只說採預設範圍，SPEC-5 與相容性說明未列這個新上限；建議確認這是否為可接受的操作差異，同步規格與邊界測試，否則保留原欄位的可接受範圍。 | 已修正：接受 ±99999 上限，寫入 SPEC-5 相容性說明、TODO-20 與 `BehaviorExceptions.txt`（DEC-20） |
| REV-42 | 4 | 建議 | `.github\project\coding-general.md:12` | 通用規範仍要求每個 Form／Dialog 自建 `LanguageManager`，但只使用固定文字的 `SewPartDlg` 直接呼叫基底 `ApplyLanguage()`，不必自建；控制項 ID 與所有語系同步的規則也與 `coding-ui-style.md` 第 7 節重複。建議通用層保留檔案結構與所有語系同步的原則，畫面套用時機、ID 與何時需要自行取字串交由 UI 規範說明並互連。 | 已修正：`coding-general.md` 只留語系檔、取字串與同步規則，畫面規則連到 `coding-ui-style.md`；後者的同步規則改為連結（DEC-20） |
| REV-43 | 4 | 建議 | `.github\project\tool-uicheck.md:10` | `runtime` 的入口表只寫使用 Debug 建置結果，沒有連到 `env-build.md` 的執行期依賴；預設 `Build.bat Debug` 只建 C#，語系自動同步也不會準備原生 DLL。建議在入口表補一個連結與「先備妥 Debug 建置及執行期依賴」的說明，讓乾淨工作目錄的使用者能按既有流程準備，不重抄部署命令。 | 已修正：`runtime` 一列連到 `env-build.md` 的建置與執行期依賴（DEC-20） |
| REV-44 | 6 | 建議 | `.github\project\tool-uicheck.md:60` | DEC-16 第 3 項與 DEC-17 第 4 項把常駐 VDD 的使用情境留在指引，但新增附錄後，套件路徑、`devcon`、XML 及同步顯示步驟已有兩份，且指引把目前套件的內部目錄寫成長期做法。建議指引保留適用機台、前置安全條件與顯示驗收條件，安裝入口以供應商持續維護的文件為準；特定套件版號下的命令、XML 範例與還原紀錄只留在附錄。 | 已修正：安裝步驟移到持續維護的 `Tools/UICheck/README.md`，指引只留通則與連結；附錄保留客戶電腦的完整操作記錄並註明以 README 為準（DEC-20） |
| REV-45 | 6 | 建議 | `.github\project\coding-ui-style.md:112` | 特例表把 `ComboBox` 將改 `RadioButton`、三點校正待可空數值框完成後移除例外，寫成長期指引的一部分，但實際施工條件已在 TODO-7、TODO-6。建議指引只保留目前允許的型別、適用範圍與理由；未來的替換計畫留在 `FollowUp.md`，避免讀者把尚未規劃的遷移當成現在的規則。 | 已修正：特例表只留目前的處理與理由，後續以 TODO-6、TODO-7 指向（DEC-20） |
| REV-46 | 6 | 建議 | `Doc\AUTO-23254\TestPlan.md:42` | TEST-18 把所有規格螢幕的橫式最大化客戶區都定為 Designer 的 1920×1009，但工具只要求螢幕有工作列並記錄實際客戶區，副螢幕工作區與終端機可不同（REV-7）。建議 TEST-18 驗收選中的螢幕、實際工作區及最大化行為；固定 1920×1009 的版面比對留給 TEST-10，避免合規副螢幕仍無法按文件驗收。 | 已修正：TEST-18 改為最大化並列出實際客戶區，固定尺寸比對屬 TEST-10（DEC-20） |
| REV-47 | 6 | 建議 | `Doc\AUTO-23254\FollowUp.md:111` | TODO-25 要求在「未設終端機標記」的電腦上實測，但未設定 `MYCAM_UICHECK_UNATTENDED` 會依 DEC-18 在 Parsec 啟動前以 exit 2 結束。請把完成條件明寫成 `MYCAM_UICHECK_UNATTENDED=0`、已安裝 Parsec VDD 且沒有規格螢幕，並保留執行後確認既有螢幕不變、虛擬副螢幕已移除的條件。 | 已修正：TODO-25 完成條件改為已安裝 Parsec VDD、`MYCAM_UICHECK_UNATTENDED=0`（DEC-20） |
| REV-48 | 6 | 建議 | `Doc\AUTO-23254\TestPlan.md:8` | 「最終結果」仍把 2026-11-26 的 Debug 寫成最後一次執行，但後面混列了 DEC-18 後的結果，且未納入 DEC-19 的重跑；README 已勾選的 Release 也無法由這份總表核對到版本。建議依門檻記錄最終一次的受測版本、日期、結果與環境，區分本版與舊版證據，沒有本版結果的項目明寫未驗證。 | 已修正：總表改記最終受測版本、日期與各門檻結果（DEC-20） |
| REV-49 | 6 | 建議 | `Doc\AUTO-23254\README.md:8` | 狀態欄仍寫待第 9 回合，但同檔 DoD 已記到第 12 回合，交接時無法從入口判斷目前階段。建議入口只保留當前狀態與真正待辦，review 進度連到 `Review.md`，DoD 則在本回合發現處置後再確認。 | 已修正：狀態只留目前階段，review 進度連到本檔；DoD 第 6 項補第 13 回合（DEC-20） |
| REV-50 | 6 | 建議 | `Doc\AUTO-23254\Implementation.md:19` | 多語系步驟記了 `CommonUI` 的自動套用，未記錄 `LanguageManager\LanguageManager.cs:233` 新增的 `HasString` 公開查詢。這是自動套用能辨識本語系未定義的可選 ID、保留既有文字且不刷缺 ID 日誌的必要改動；建議在 SPEC-14 或其 STEP 補上跨專案相依與缺 ID 的預期行為。 | 已修正：SPEC-14 與 `Implementation.md` 補 `HasString` 與缺 ID 的行為（DEC-20） |

## 第 14 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify_rebased` |
| 比對基準 | `bd0fa270`；受審 HEAD `734c6b05` |
| 工具與模型 | Visual Studio GitHub Copilot |
| 日期 | 2026-11-27 |

整體 review：逐層核對實作、測試、規格、目標及後續處理，不重提已決議的取捨。`Tools/UICheck` 的正式入口不呼叫議題專用測試；固定基準、改名對照、行為例外與版面快照留在 `Doc/AUTO-23254/Tests`。議題測試共用長期工具的解析能力是單向依賴，不影響長期入口獨立執行。

| 門檻 | 本回合結果 |
| --- | --- |
| `Build.bat Debug` / `Build.bat Release` | 都通過，分別 3、6 個警告，皆 0 個錯誤；本分支未改 C++ 專案 |
| `UICheck.bat` / `UICheck.bat runtime` | 都通過：34 個 Designer 檔、zh-TW 與 zh-CN 各 45 個畫面狀態、46 個數值框案例、12 組主視窗截圖及 10 個 Designer 區域，皆 0 個問題。使用一般工作電腦既有的 1920×1080、96 DPI 主螢幕；此結果不涵蓋新增 Parsec 副螢幕的路徑 |
| `RunIssueChecks.bat` | 通過：行為比對 0 個差異，分支規則 358 個檔案、0 個問題，版面基準 12 情境／152 行、0 個差異 |
| `DesignerDiff.ps1 -Base bd0fa270 -Head HEAD` | 39 個檔案，38 個有語意差異、1 個僅格式差異 |

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-51 | 2 | 必修 | `Doc\AUTO-23254\Goal.md:63` | REQ-4 的一律延期與既有已核准例外矛盾。 | 已修正（回溯至 REQ-4；DEC-21） |
| REV-52 | 2 | 必修 | `Doc\AUTO-23254\Implementation.md:45` | 漏記刀向圖例與 3D 路徑共用色表、OCCT → sRGB 轉換及動態日誌標籤的 `TextRole`。 | 已修正（STEP-3、STEP-8 補實作與測試邊界） |
| REV-53 | 2 | 必修 | `Doc\AUTO-23254\Spec.md:58` | `1e2` 可被畫面舊解析器接受並更新預覽，卻被共用數值框拒絕。 | 已修正（回溯至 SPEC-5；DEC-21 採一般十進位，拒絕早於預覽與提交） |
| REV-54 | 6 | 建議 | `Doc\AUTO-23254\Implementation.md:74` | `SPEC → STEP` 與 STEP 欄不一致，SPEC-19 的規範檢查與合入行為未區分。 | 已修正（雙向對照與驗收邊界） |
| REV-55 | 6 | 建議 | `Doc\AUTO-23254\TestPlan.md:53` | TEST-11 漏記 9 組 fixture，總表混寫未標記及未建置。 | 已修正（補自測通過條件及兩種中止情境） |
| REV-56 | 6 | 建議 | `Tools\UICheck\README.md:3` | README 缺工具入口與 `FixTabOrder` 寫入警告。 | 已修正（基本指令、前置條件、指引連結與寫入範例） |
| REV-57 | 2 | 建議 | `Doc\AUTO-23254\FollowUp.md:36` | 切換語系後既有 CNC 面板的字族限制未寫入規格。 | 已修正（回溯至 SPEC-1；即時刷新依 DEC-21 保留 TODO-10） |

TODO-25 對應 SPEC-20 的 Parsec 成功路徑，技術上是本議題的驗證，不是新需求；DEC-17 第 3 項已同意在其他電腦補驗且不擋 push，本回合保留該決議，不把它記成已通過。

處置後驗收（2026-11-28）見 [TestPlan.md](TestPlan.md#1-總表與最終結果)。新增的共用測試只驗 `DebouncedNumericUpDown`，未引用議題測試；TEST-16 實機回歸通過。TEST-19 原僅 `LockMSAxisDlg` 的 Designer 無法載入泛型基底；STEP-15 保留既有編輯行為，改用非泛型的直接基底，使用者重驗確認可正常開啟，TEST-15／TEST-19 通過。

## 第 15 回合

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify_rebased` |
| 比對基準 | rebase 前 `a0a1f8e5`、GitLab `master` `af5a2e8d`；受審 HEAD `5c12b4c1` 加 STEP-17 工作樹 |
| 工具與模型 | GitHub Copilot |
| 日期 | 2026-11-28 |

逐檔比對後，產品語意差異皆屬上游輪廓編輯與版本更新、提示及語系清理、或已確認的對話框繼承統一。舊中介基底在目前工作樹無引用。建置、完整 UI 檢查、行為與主視窗版面比對通過；Designer 與功能由使用者確認正常。

| REV | 段 | 嚴重度 | 位置 | 說明 | 處置 |
| --- | --- | --- | --- | --- | --- |
| REV-58 | 1 | 建議 | `CommonUI/CommonUI.csproj` | 本機未提交的縮排與檔尾格式異動不屬於 rebase；分支規則只在此檔報 `FinalNewline`。 | 已修正：排除本機純格式異動，未納入提交；分支規則重跑通過。 |
