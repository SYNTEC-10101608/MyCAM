# 專案固定檢查項目

執行或回應 review 時，與 [team/process-review.md](../team/process-review.md) 一起讀；以下項目在 review 中必查。

1. 門檻：
   1. `Build.bat Debug` 與 `Build.bat Release` 建置成功。
   2. 修改到 C++ 專案時，另外單獨建置該專案的 Debug 與 Release（`/p:Platform=x64`）。
   3. 修改到畫面時，`Tools\UICheck\UICheck.bat` 全部通過；新增畫面或改動版面時另跑 `runtime`（見 [tool-uicheck.md](tool-uicheck.md)）。
2. 規範：修改的檔案符合 `project/` 下的程式規範（`coding-*`）。
