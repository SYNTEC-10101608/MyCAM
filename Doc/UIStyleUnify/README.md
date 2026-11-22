# UI 風格盤點、優化、統一：議題研發報告

| 項目 | 內容 |
| --- | --- |
| 議題編號 | AUTO-23254 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| 分支起點 | `a4b3826d`（`master`，2026-11-15） |
| 期間 | 2026-11-15 ～ 2026-11-21 |
| 狀態 | 施工與自動檢查完成（含 Round 1～3 review 的修正）；人工驗收（5.3 第 2～4 項）未完成，三項都是合併前必做（見 [6_FollowUp.md](6_FollowUp.md)） |

## 文件

| 文件 | 內容 | 性質 |
| --- | --- | --- |
| [`.github/instructions/ui-style.instructions.md`](../../.github/instructions/ui-style.instructions.md) | **專案指引**：UI 風格的最終規範，供團隊與 AI 工具遵循 | 最終版，持續維護 |
| [1_Goal.md](1_Goal.md) | 本議題目標、原則、規格限制、範圍 | 最終版 |
| [2_Architecture.md](2_Architecture.md) | 架構規劃：分類、分層、元件、版面、語系、檢查機制的設計與理由 | 最終版 |
| [3_Implementation.md](3_Implementation.md) | 實作紀錄：實際施工內容，與架構規劃逐項對應，並附逐檔異動明細 | 最終版 |
| [4_Discussion.md](4_Discussion.md) | 討論紀錄：所有要求、待決問題、Review 發現與決議（依日期） | 歷史紀錄，只追加 |
| [5_Verification.md](5_Verification.md) | 測試驗證：兩類測試的內容、結果、涵蓋範圍與維護注意事項 | 最終版 |
| [6_FollowUp.md](6_FollowUp.md) | 合併前的收尾、範圍外的後續議題與議題清單 | 隨進度更新 |
| [7_Compatibility.md](7_Compatibility.md) | 相容性異動：合併後對客製語系檔等既有檔案的影響與處理方式，合併時寫入 Merge Request 說明 | 最終版 |
| [8_ReviewRound1.md](8_ReviewRound1.md) | Round 1 整體審查（VS Copilot）：問題、證據、驗證限制與待討論事項 | Review 快照，不修改 |
| [9_ReviewRound1-reply.md](9_ReviewRound1-reply.md) | Round 1 review 的回覆：逐項決議、修正內容與驗證結果，供 Round 2 review | 回覆，Round 2 時對照 |
| [10_ReviewRound2.md](10_ReviewRound2.md) | Round 2 審查：回覆確認、精度回復問題、比對防線、測試缺口與驗證結果 | Review 快照，不修改 |
| [11_ReviewRound2-reply.md](11_ReviewRound2-reply.md) | Round 2 review 的回覆：逐項決議、修正內容與驗證結果，供 Round 3 review | 回覆，Round 3 時對照 |
| [12_ReviewRound3.md](12_ReviewRound3.md) | Round 3 審查：原生上下按鈕、B3 生效值範圍、debounce 時間防線與回覆確認 | Review 快照，不修改 |
| [13_ReviewRound3-reply.md](13_ReviewRound3-reply.md) | Round 3 review 的回覆：逐項決議、修正內容與驗證結果，供 Round 4 review | 回覆，Round 4 時對照 |
| [14_ReviewRound4.md](14_ReviewRound4.md) | Round 4 審查：原生重複操作、B3 舊型別與清理、N2 時間判定及回覆確認 | Review 快照，待討論 |

## 測試程式

| 位置 | 類別 | 內容 |
| --- | --- | --- |
| `Tools/UICheck/` | 可持續使用 | UI 規範與風格檢查（靜態、執行時、主視窗）。之後改畫面一律要跑，規則隨規範維護 |
| `Doc/UIStyleUnify/Tests/` | 本議題限定 | 與分支起點比對的行為不變檢查、改名對照、本議題寫入的程式碼風格與產品程式殘留檢查 |

兩類的說明、執行方式與維護注意事項見 [5_Verification.md](5_Verification.md)。
