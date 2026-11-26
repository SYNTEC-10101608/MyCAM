# AUTO-23254 UI 風格盤點、優化、統一

| 項目 | 內容 |
| --- | --- |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 分支起點 | `bd0fa270`（`master`，2026-11-26，rebase 後）；原起點 `a4b3826d`（2026-11-15），rebase 前的版本在本機分支 `backup/AUTO-23254_before_rebase` |
| 級別 | L |
| 狀態 | 已併入 `master` 並依新規範調整（STEP-12）、新增規格螢幕的截圖環境（STEP-13），`runtime` 已在規格螢幕上全部通過；待人工確認 `master` 修正（TEST-19）、待討論定案，再推送與發 MR |

## 文件

| 文件 | 內容 |
| --- | --- |
| [Goal.md](Goal.md) | 現況盤點、目標、範圍、需求（REQ） |
| [Spec.md](Spec.md) | 操作規格、實作規格、相容性異動（SPEC） |
| [TestPlan.md](TestPlan.md) | 測試規劃與執行結果（TEST） |
| [Implementation.md](Implementation.md) | 實作架構、步驟（STEP）與異動範圍 |
| [Review.md](Review.md) | Review 報告（REV） |
| [Discussion.md](Discussion.md) | 討論記錄：依主題整理的共識與決策（DEC） |
| [FollowUp.md](FollowUp.md) | 範圍外的後續處理（TODO）與議題清單 |

專案指引：`.github/project/coding-ui-style.md`、`.github/project/coding-icon-design.md`；截圖環境見
`.github/project/env-build.md`。

## 測試程式

| 位置 | 類別 |
| --- | --- |
| `Tools/UICheck/` | 可持續使用：UI 規範檢查，之後改畫面一律要跑 |
| `Doc/AUTO-23254/Tests/` | 本議題限定：與 rebase 目標比對的行為不變、本議題程式碼風格、主視窗區域基準 |

## 待討論

1. DEC-12 依 rebase 指引的建議做法先執行的項目（直接 rebase、指引與議題文件的轉換方式、舊 commit 不補 `STEP`、檢查基準改為 `bd0fa270`、`UICheck` 加入專案門檻）：
   1. 問題：這些是 `Doc/AUTO-23290/FollowUp.md` 第 4 節第 8 項列為「開工時先由人決定」的事項，已先依其建議執行。
   2. 建議：維持，理由見 DEC-12。
   3. 另一可行選項：逐項否決後調整；若要改為 squash 需重做，`backup/AUTO-23254_before_rebase` 保留原狀。
2. 四軸時 `CNCOffsetDialog` 卡片底部的空白列（REV-8）：
   1. 問題：`master`（AUTO-23275）四軸時隱藏第五列，對話框大小不變，卡片底部留一列空白。
   2. 建議：維持現狀，與 `CNCInfoPanel` 隱藏 C 軸後的呈現一致，對話框大小不隨控制器改變。
   3. 另一可行選項：以 `SectionStack` 依軸數縮短卡片並上移按鈕列，畫面較緊湊，但對話框大小會隨控制器改變。
3. `master` 的 `ApplicationIcon` 路徑錯誤（REV-1）：
   1. 問題：`master` 在乾淨的 checkout 無法建置；本分支已修正，`master` 本身仍是錯的。
   2. 建議：另開 bug 先在 `master` 修正，不必等本議題合併。
   3. 另一可行選項：只隨本議題合併修正。
4. 測試機的常駐顯示器與 `UICHECK_VISIBLE`：已決議（DEC-13 第 8 項）。

## DoD

1. [x] 每個 REQ 有 SPEC、每個 SPEC 有 TEST（`Spec.md` 第 4 節、`TestPlan.md` 第 9 節）。
2. [x] 相容性異動已寫明。
3. [ ] `TestPlan.md` 附最終執行結果：TEST-19 待人工確認，其餘已附。
4. [x] 每個 STEP 可追溯到 commit：STEP-1～STEP-11 依 `Implementation.md` 人工對照，STEP-12 之後以標題標示（DEC-12）。
5. [x] 門檻通過：`Build.bat Debug`、`Build.bat Release`、`UICheck.bat runtime`（含靜態檢查）、`RunIssueChecks.bat`。
6. [ ] Review 報告寫入 `Review.md`，「必修」全部已修正：REV-1 待人確認，REV-8 待回覆。
7. [x] 範圍外項目寫入 `FollowUp.md`。
8. [x] Release note 寫入本文件。
9. [ ] 各確認點分別經人確認：原範圍已於 2026-11-23 確認；rebase 與截圖環境（REQ-8、SPEC-19、SPEC-20）待確認。
10. [x] 長期有效的結論已提升到指引規範（`project/coding-ui-style.md`、`project/coding-icon-design.md`、`project/env-build.md`）。

## Release Note

1. 修改 所有對話框、CNC 面板與主視窗改為統一的淺色主題（色票、字型、圓角按鈕、卡片與標題列），字型依介面語系。
2. 修改 對話框統一為工具視窗，不可改大小、不出現在工作列；開啟時置中，橫式在主視窗中央、直式在上半部 CAM 區域。
3. 修改 對話框的區塊改為卡片排列，按鈕列主要動作在最右、取消在其左；確定鍵文字統一為「確定」。
4. 修改 數值欄位統一為同一種數值框，可用上下鍵與滾輪調整，非法或超出範圍的輸入回復為最近的合法值。
5. 修改 只顯示的座標改為文字標籤，不能再選取複製。
6. 修改 工具列圖示統一為透明背景，新增類的圖示右下角加「+」。
7. 修改 主視窗各區域與 CNC 卡片的間距統一；橫式加工頁的警報移到日誌右側，監控卡片與 3D 畫面對齊。
8. 修改 CNC 加工的開始、暫停、停止按鈕改為淡綠、淡橘、淡紅。
9. 修改 語系 ID 改為控制項名稱，客製語系檔中的舊 ID 不再生效；共用按鈕文字改由共用語系檔提供。
10. 修改 語系檔 `LockABDlg`、`CNCThreePointDialog` 改名為 `LockMSAxisDlg`、`CNCThreePointDlg`。
11. 修改 主視窗圖示的設定方式，並修正找不到應用程式圖示而無法建置的問題。
12. 新增 UI 規範的自動檢查工具，可在 1920×1080 的虛擬螢幕上截圖檢查。
