# UI 風格盤點、優化、統一：議題研發報告

| 項目 | 內容 |
| --- | --- |
| 議題編號 | 待補（`TBD`） |
| 分支 | `AUTO-TBD_f_UIStyleUnify`（議題編號決定後改名） |
| 分支起點 | `a4b3826d`（`master`，2026-11-15） |
| 期間 | 2026-11-15 ～ 2026-11-17 |
| 狀態 | 施工與驗收完成，待人工確認與合併（見 [6_FollowUp.md](6_FollowUp.md)） |

## 文件

| 文件 | 內容 | 性質 |
| --- | --- | --- |
| [`.github/instructions/ui-style.instructions.md`](../../.github/instructions/ui-style.instructions.md) | **專案指引**：UI 風格的最終規範，供團隊與 AI 工具遵循 | 最終版，持續維護 |
| [1_Goal.md](1_Goal.md) | 本議題目標、原則、規格限制、範圍 | 最終版 |
| [2_Architecture.md](2_Architecture.md) | 架構規劃：分類、分層、元件、版面、語系、檢查機制的設計與理由 | 最終版 |
| [3_Implementation.md](3_Implementation.md) | 實作紀錄：實際施工內容，與架構規劃逐項對應，並附逐檔異動明細 | 最終版 |
| [4_Discussion.md](4_Discussion.md) | 討論紀錄：所有要求、待決問題、Review 發現與決議（依日期） | 歷史紀錄，只追加 |
| [5_Verification.md](5_Verification.md) | 測試驗證：兩類測試的內容、結果、涵蓋範圍與維護注意事項 | 最終版 |
| [6_FollowUp.md](6_FollowUp.md) | 合併前的收尾事項與範圍外的後續議題 | 隨進度更新 |

## 測試程式

| 位置 | 類別 | 內容 |
| --- | --- | --- |
| `Tools/UICheck/` | 可持續使用 | UI 規範與風格檢查（靜態、執行時、主視窗）。之後改畫面一律要跑，規則隨規範維護 |
| `Doc/UIStyleUnify/Tests/` | 本議題限定 | 與分支起點比對的行為不變檢查、改名對照、本議題寫入的程式碼風格與產品程式殘留檢查 |

兩類的說明、執行方式與維護注意事項見 [5_Verification.md](5_Verification.md)。
