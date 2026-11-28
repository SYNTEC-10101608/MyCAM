# AUTO-23254 UI 風格盤點、優化、統一

| 項目 | 內容 |
| --- | --- |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 分支起點 | `af5a2e8d`（GitLab `master`，`RLS_0.49.0`）；前次基準 `bd0fa270`，原起點 `a4b3826d` |
| 級別 | L |
| 狀態 | Review 與人工驗收完成；主從軸鎖定視窗的 Designer 重驗通過（TEST-15／TEST-19），待確認 push 與送 MR |

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
| [Appendix-VirtualDisplay.md](Appendix-VirtualDisplay.md) | 附錄：在沒有 Claude Code 的電腦上手動安裝常駐虛擬螢幕並設為同步顯示（TODO-24 的先行策略） |

專案指引：`.github/project/coding-ui-style.md`、`.github/project/coding-icon-design.md`；UI 檢查工具與截圖環境見
`.github/project/tool-uicheck.md`。

## 測試程式

| 位置 | 類別 |
| --- | --- |
| `Tools/UICheck/` | 可持續使用：UI 規範檢查，之後改畫面一律要跑 |
| `Doc/AUTO-23254/Tests/` | 本議題限定：與 rebase 目標比對的行為不變、本議題程式碼風格、主視窗區域基準 |

## 待討論

無。前一輪的待討論已於 2026-11-26 確認（DEC-14）。

## DoD

1. [x] 每個 REQ 有 SPEC、每個 SPEC 有 TEST（`Spec.md` 第 4 節、`TestPlan.md` 第 9 節）。
2. [x] 相容性異動已寫明。
3. [x] `TestPlan.md` 附執行結果；最新 rebase 的 Designer（TEST-15）及輪廓編輯功能回歸（TEST-19）已由使用者確認正常。
4. [x] 每個 STEP 可追溯到 commit：STEP-1～STEP-11 依 `Implementation.md` 人工對照，STEP-12 起以標題標示（DEC-12）。
5. [x] 建置、UI 檢查、行為比對、分支規則與主視窗版面比對均通過（結果見 [TestPlan.md](TestPlan.md#1-總表與最終結果)）。
6. [x] Review 報告寫入 `Review.md`，「必修」全部已修正；第 14 回合依 DEC-21 處置。
7. [x] 範圍外項目寫入 `FollowUp.md`。
8. [x] Release note 寫入本文件。
9. [x] 各確認點分別經人確認：原範圍 2026-11-23；rebase 與截圖環境（REQ-8、SPEC-19、SPEC-20）2026-11-26（DEC-14）。
10. [x] 長期有效的結論已提升到指引規範（`project/coding-ui-style.md`、`project/coding-icon-design.md`、`project/tool-uicheck.md`）；UICheck 的安裝步驟在 `Tools/UICheck/README.md`。

## Release Note

1. 修改 介面外觀與版面：統一主視窗、對話框與 CNC 面板的淺色主題、圖示及版面規則。
2. 修改 輸入與顯示：統一數值欄位的輸入格式與驗證方式，唯讀座標改用文字標籤。
3. 修改 多語系：統一介面文字與語系檔規則；客製語系與操作相容性異動見 [Spec.md](Spec.md#3-相容性異動)。
4. 新增 UI 驗證：提供靜態、執行時版面與截圖檢查工具。
