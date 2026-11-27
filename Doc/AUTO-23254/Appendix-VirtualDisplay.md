# 附錄：手動安裝常駐虛擬螢幕（同步顯示）

在客戶電腦等沒有 Claude Code 的電腦上，以人工重現「實體螢幕不是 1920×1080 時，以常駐虛擬螢幕提供 1920×1080 桌面」的做法；用途見 [FollowUp.md](FollowUp.md) 的 TODO-24。內容是 2026-11-27 開發用終端機的實際配置與操作記錄，不是長期規範；UICheck 終端機的規範見 `.github/project/tool-uicheck.md` 第 2 節。

## 1. 參考配置（開發用終端機）

| 項目 | 內容 |
| --- | --- |
| 來源 | 開源專案 Virtual Display Driver（https://github.com/VirtualDrivers/Virtual-Display-Driver，MIT 授權） |
| 取得 | `winget install VirtualDrivers.Virtual-Display-Driver`（套件 25.7.26）：只把 zip 解壓到 `%LOCALAPPDATA%\Microsoft\WinGet\Packages\VirtualDrivers.Virtual-Display-Driver_*`，不安裝驅動 |
| 驅動 | 11.30.4.434（2024/12/24），提供者 MikeTheTech；裝置管理員「顯示卡」下為 `Virtual Display Driver`，「監視器」下為 `Generic Monitor (VDD by MTT)` |
| 設定檔 | `C:\VirtualDisplayDriver\vdd_settings.xml`：1 台螢幕，解析度只有 1920×1080、60 Hz |
| 顯示設定 | 筆電面板與虛擬螢幕「同步顯示」，1920×1080、縮放 100%；面板原生解析度較低，顯示縮小後的畫面 |

## 2. 安裝與設定

前置：系統管理員權限；x64 的 Windows 10 / 11；人在實機旁；先截圖記下「設定 > 系統 > 顯示器」的現況，供還原對照。

1. 取得檔案：
   1. 有 winget：執行上表的 `winget install`，到 Packages 資料夾。
   2. 沒有 winget：從 GitHub 專案的 Releases 下載對應版本的 zip 解壓（winget 裝的就是這份 zip）。
   3. 解壓後應有 `Dependencies\devcon.exe` 與 `SignedDrivers\x86\VDD\MttVDD.inf`；`x86` 資料夾就是一般 64 位元電腦用的，`ARM64` 給 ARM 電腦。
2. 準備設定檔（在安裝驅動之前）：
   1. 建立 `C:\VirtualDisplayDriver\`，把 `SignedDrivers\x86\VDD\vdd_settings.xml` 複製進去。
   2. 以記事本編輯，下列三段改成如下，其餘不動；`<resolutions>` 原本列了很多解析度，刪到只剩這一筆：

      ```xml
      <monitors>
        <count>1</count>
      </monitors>
      <global>
        <g_refresh_rate>60</g_refresh_rate>
      </global>
      <resolutions>
        <resolution>
          <width>1920</width>
          <height>1080</height>
          <refresh_rate>60</refresh_rate>
        </resolution>
      </resolutions>
      ```

3. 安裝驅動：以系統管理員身分開命令提示字元，切到解壓資料夾，執行下列指令；Windows 詢問「是否要安裝此裝置軟體」時選「安裝」。完成後在裝置管理員確認第 1 節的兩個裝置。

   ```
   Dependencies\devcon.exe install SignedDrivers\x86\VDD\MttVDD.inf Root\MttVDD
   ```

4. 顯示設定（設定 > 系統 > 顯示器）：
   1. 螢幕圖示出現 1（實體螢幕）與 2（虛擬螢幕）。
   2. 圖示下方的下拉選單選「在這些顯示器上同步顯示」；不要選「僅在 2 上顯示」（實體螢幕會變黑，見第 3 節）。
   3. 「顯示器解析度」設為 1920 × 1080，「比例」設為 100%。
   4. 確認：圖示顯示「1|2」；實體螢幕顯示縮小後的 1920×1080 畫面（字較小、略糊）；有工作列。
5. 驗證（TODO-24 的驗證項目）：MyCAM 最大化後版面正常；觸控螢幕點擊位置一致；3D 畫面操作順暢。

## 3. 還原與異常處理

1. 移除：以系統管理員身分在解壓資料夾執行 `Dependencies\devcon.exe remove Root\MttVDD`，或在裝置管理員對 `Virtual Display Driver` 選「解除安裝裝置」並勾選刪除驅動程式；Windows 隨即回到只有實體螢幕，再對照前置記下的設定。
2. 實體螢幕變黑（多半是選成「僅在 2 上顯示」）：Win+P 的選單出現在看不見的虛擬螢幕上，只能盲操作：按 Win+P，以方向鍵「上」移到最上方的「僅電腦螢幕」，按 Enter；畫面恢復後重做第 2 節第 4 步。
3. 未驗證：開發用終端機在同步顯示時，Windows 直接提供 1920×1080；若下拉選單沒有 1920×1080，可能要在顯示卡控制台（Intel、NVIDIA 等）調整縮放或解析度設定。
