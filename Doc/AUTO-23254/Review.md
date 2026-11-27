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
