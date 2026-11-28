# UICheck

## 從專案根目錄執行

| 指令（PowerShell） | 用途 |
| --- | --- |
| `.\Tools\UICheck\UICheck.bat` | 靜態檢查，只讀原始碼，不需建置。 |
| `.\Tools\UICheck\UICheck.bat runtime` | 先跑靜態，再檢查執行時版面、數值框及主視窗。先備妥 [Debug 建置與執行期依賴](../../.github/project/env-build.md)，並依 [UICheck 指引](../../.github/project/tool-uicheck.md) 設定機台標記與規格螢幕；可在 `runtime` 後指定截圖資料夾。 |
| `.\Tools\UICheck\DesignerDiff.ps1 -Base HEAD` | 列出工作樹相對於 HEAD 的 Designer 語意差異，不改檔；加 `-Head <版本>` 可比對兩個版本。 |
| `.\Tools\UICheck\FixTabOrder.ps1 -Only PatternSettingDlg` | 按版面重排指定畫面的 `TabIndex`，**會寫回 Designer 檔**；省略 `-Only` 會處理所有受支援的畫面。執行後以 `git diff` 檢查異動。 |

## 常駐虛擬螢幕（VirtualDrivers VDD）

只用於沒有規格螢幕的終端機（[UICheck 指引第 2.2 節](../../.github/project/tool-uicheck.md#22-電腦分類)）；需要系統管理員權限，每台安裝一次。

1. 取得：`winget install VirtualDrivers.Virtual-Display-Driver`。只把驅動檔與 `devcon.exe` 解壓到
   `%LOCALAPPDATA%\Microsoft\WinGet\Packages\VirtualDrivers.Virtual-Display-Driver_*`，不會安裝驅動；以下路徑相對於這個資料夾。
2. 設定檔（安裝驅動之前）：把 `SignedDrivers\x86\VDD\vdd_settings.xml` 複製到 `C:\VirtualDisplayDriver\`，改成 1 台螢幕
   （`<monitors><count>1</count></monitors>`）、`<resolutions>` 只留 1920×1080、更新率 60。
3. 安裝驅動：以系統管理員身分在上述資料夾執行下列指令，Windows 詢問是否安裝裝置軟體時選「安裝」。

   ```
   Dependencies\devcon.exe install SignedDrivers\x86\VDD\MttVDD.inf Root\MttVDD
   ```

4. 顯示設定：新螢幕掛上後，在「設定 > 系統 > 顯示器」把它與原本的螢幕（如筆電面板）設為「在這些顯示器上同步顯示」，
   解析度 1920×1080、比例 100%。程式只看到一個 1920×1080 的螢幕，實機與遠端工具看到同一個畫面；實體螢幕解析度較低時畫面會縮小，截圖不受影響。
5. 確認：`Tools\UICheck\UICheck.bat runtime` 開始時印出的 `[display]` 一行為 1920×1080、96 DPI，且有工作列。
6. 不要選「僅在 2 上顯示」：實體螢幕會變黑。已變黑時按 Win+P，以方向鍵「上」移到最上方的「僅電腦螢幕」再按 Enter，畫面恢復後重做第 4 步。
7. 移除：以系統管理員身分在上述資料夾執行 `Dependencies\devcon.exe remove Root\MttVDD`。
