# UI 檢查工具（`Tools/UICheck`）

執行 UI 檢查、準備截圖環境時讀本文件。檢查的規則與時機見 [coding-ui-style.md](coding-ui-style.md) 的「檢查」。

## 1. 指令

| 指令 | 內容 |
| --- | --- |
| `Tools\UICheck\UICheck.bat` | 靜態檢查：只讀原始碼，不需建置 |
| `Tools\UICheck\UICheck.bat runtime [截圖資料夾]` | 另以 Debug 建置結果開啟所有畫面，在 zh-TW、zh-CN 下檢查版面、數值框操作與主視窗；需要截圖環境（第 2 節） |
| `Tools\UICheck\DesignerDiff.ps1` | 列出 Designer 檔的語意差異，review 時使用 |

## 2. 截圖環境

UI 的執行時檢查（`Tools\UICheck\UICheck.bat runtime`）要在 1920×1080、縮放 100%、有工作列的螢幕上執行，因為產品依螢幕的工作區域最大化主視窗、置中與限制對話框。直式不需要另外的螢幕：`MainLayout` 只看主視窗的長寬比，檢查在同一個螢幕上把主視窗設成高大於寬。檢查工具不更動既有的螢幕，依序選用：

1. 主螢幕符合時用主螢幕，否則用其他符合的螢幕。
2. 都不符合時（例如 1920×1080 但縮放 125%、筆電 1366×768），在檢查期間加入一個虛擬副螢幕，結束後移除。需要先以系統管理員權限安裝一次虛擬螢幕驅動：`winget install Parsec.ParsecVDD`。

| 情況 | 處理 |
| --- | --- |
| 沒裝驅動、也沒有符合的螢幕 | `runtime` 中止（exit 2）並提示安裝指令 |
| 使用的螢幕沒有工作列 | 中止；開啟「設定 > 個人化 > 工作列 > 工作列行為 > 在所有顯示器上顯示我的工作列」 |
| 加入虛擬螢幕時 Windows 改動了既有螢幕（套用記住的配置，例如把虛擬螢幕設為主螢幕） | 中止並移除虛擬螢幕，Windows 隨即還原；以 Windows 顯示設定把該組合排成虛擬螢幕在右、原主螢幕不變一次即可 |
| 以遠端桌面（RDP）連線執行 | 遠端桌面工作階段看不到虛擬螢幕，改在本機或以 AnyDesk 等操作本機畫面的工具執行 |
| 一般工作電腦 | 主視窗在隱藏桌面上截圖，不佔用螢幕；3D 畫面是空白的，檢查期間右側會短暫多出一個看不見的螢幕 |
| 螢幕無人操作的測試機 | 設定使用者環境變數 `UICHECK_VISIBLE=1`，主視窗改在可見桌面截圖，3D 畫面也會呈現；一般工作電腦不要設定 |
| 闔蓋、只以遠端工具操作的筆電 | 以常駐的虛擬螢幕當主螢幕（見下方步驟），它同時就是規格螢幕；面板被卸離後闔蓋時無法再掛回，不要以顯示設定卸離它 |

常駐虛擬螢幕的安裝（系統管理員權限，一次）：

1. `winget install VirtualDrivers.Virtual-Display-Driver`：只放入驅動檔與 `devcon.exe`（`%LOCALAPPDATA%\Microsoft\WinGet\Packages\VirtualDrivers.Virtual-Display-Driver_*`），不會安裝驅動。
2. 把套件中 `SignedDrivers\x86\VDD\vdd_settings.xml` 複製到 `C:\VirtualDisplayDriver\`，`resolutions` 只留 1920×1080、更新率 60。
3. `Dependencies\devcon.exe install SignedDrivers\x86\VDD\MttVDD.inf Root\MttVDD`，Windows 詢問是否安裝裝置軟體時選安裝。
4. 新螢幕掛上後，確認它是 1920×1080、100%，再移除其他暫時的螢幕，讓它成為主螢幕。
