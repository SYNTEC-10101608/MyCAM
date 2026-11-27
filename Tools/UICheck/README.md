# UICheck

UI 規範的檢查工具。指令、截圖環境與各類電腦的處理見 `.github/project/tool-uicheck.md`；本檔放安裝與設定的操作步驟。

## 常駐虛擬螢幕（VirtualDrivers VDD）

只用於沒有規格螢幕的終端機（`tool-uicheck.md` 第 2.2 節）；需要系統管理員權限，每台安裝一次。

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
