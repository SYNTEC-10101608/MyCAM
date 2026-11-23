# AUTO-23254 UI 風格盤點、優化、統一

| 項目 | 內容 |
| --- | --- |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 分支起點 | `a4b3826d`（`master`，2026-11-15） |
| 級別 | L |
| 狀態 | 施工、自動檢查、整體審查與人工確認完成；待同步 `master`、推送與發 MR（[5_FollowUp.md](5_FollowUp.md) 5.1） |

## 文件

| 文件 | 內容 |
| --- | --- |
| [1_Goal.md](1_Goal.md) | 議題目標：現況盤點、目標、需求（R）、範圍 |
| [2_Spec.md](2_Spec.md) | 規格提案：操作規格、實作規格（S）、相容性異動 |
| [3_TestPlan.md](3_TestPlan.md) | 測試規劃：依種類與目的分類的測試項目（T）與最終結果 |
| [4_Implementation.md](4_Implementation.md) | 實作架構、實作步驟與異動範圍 |
| [5_FollowUp.md](5_FollowUp.md) | 合併前事項、範圍外的後續議題與議題清單 |
| [6_Discussion.md](6_Discussion.md) | 討論記錄：依主題整理的共識與決策 |

專案指引：`.github/instructions/ui-style.instructions.md`、`.github/instructions/icon-design.instructions.md`。

## 測試程式

| 位置 | 類別 |
| --- | --- |
| `Tools/UICheck/` | 可持續使用：UI 規範檢查，之後改畫面一律要跑 |
| `Doc/AUTO-23254/Tests/` | 本議題限定：與分支起點比對的行為不變、本議題程式碼風格、主視窗區域基準 |
