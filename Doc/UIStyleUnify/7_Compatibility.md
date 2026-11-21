# 7. 相容性異動

合併後對既有資料、設定、客製檔案或外部介面的影響與處理方式。以下項目都已決議接受，不需要修改程式；
合併時寫入 Merge Request 說明，部署到有客製檔案的現場時告知。

## 7.1 異動項目

| # | 異動 | 影響 | 處理方式 |
| --- | --- | --- | --- |
| 1 | 語系 ID 改名與刪除 | 語系 ID 改為控制項名稱（4.3「語系 ID」），控制項欄位依型別前綴改名（4.3「控制項欄位縮寫」），並刪除無用 ID：以 zh-TW 計，30 個語系檔共 173 個舊 ID 不再使用。客製語系檔（`Custom/Language`）以 ID 逐筆覆蓋標準語系，舊 ID 不再對應任何控制項，該處的客製文字失效，畫面改顯示標準文字；不會出錯 | 已決議接受（2026-11-15「客製語系檔相容性」）。部署到有客製語系檔的現場時，依 `git diff a4b3826d <合併 commit> -- Language/` 的舊 ID 與新 ID 對照改寫客製檔 |
| 2 | 語系檔改名 | `LockABDlg_<語系>.xml` → `LockMSAxisDlg_<語系>.xml`、`CNCThreePointDialog_<語系>.xml` → `CNCThreePointDlg_<語系>.xml`（檔名與類別名一致）。客製語系檔若沿用舊檔名，整份不會被讀取 | 同上；客製檔改為新檔名 |

## 7.2 不需特別處理的部分

部署的固定流程（clean 後重跑 `CopyRef.bat`）已涵蓋標準語系檔的更新：

1. `CopyRef.bat` 以 `robocopy /E /XO` 複製 `Language/`，改過的語系檔一定會複製過去。
2. 改名前的舊檔（`LockABDlg_*`、`CNCThreePointDialog_*`）不會被刪除：`CopyRef.bat` 不刪檔，MSBuild 的 clean 也只刪除建置產生的檔案。
   程式依類別名稱讀取語系檔，不會讀到這些舊檔，留著無害；要清乾淨時手動刪除 `bin/<組態>/Resource/Language` 再重跑 `CopyRef.bat`。
