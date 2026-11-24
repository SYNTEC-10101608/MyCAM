# 議題文件

撰寫或修改議題文件時讀本文件。各階段與各級 DoD 見 [issue-workflow.md](issue-workflow.md)；文件通則見 [collaboration.instructions.md](../instructions/collaboration.instructions.md)。

## 1. 存放

1. 存放於 `Doc/<議題編號>/`；同一議題的後續 MR（`_2nd`、`_3rd`）沿用同一資料夾。
2. 尚未取得議題編號時以 `AUTO-TBD` 暫代，取得後一併更名資料夾與分支。
3. 檔名不加數字前綴；閱讀順序由 `README.md` 的文件索引決定。
4. `SESSION.md` 放在 worktree 根目錄，以 `.gitignore` 排除，只放交接 prompt 與暫時筆記；狀態與決策寫在提交的文件。
5. 收官時把議題結束後仍長期有效的結論（規則、慣例、架構說明）寫入指引規範，議題文件保留追溯用的記錄。

## 2. 各級檔案組合

「README」表示寫在 `README.md` 的對應章節。

| 內容 | S | M | L |
| --- | --- | --- | --- |
| 議題資訊、文件索引、DoD、Release Note | README | README | README |
| 現況盤點、目標、範圍、REQ（BUG 為現象與重現方式） | README | README | `Goal.md` |
| SPEC、相容性異動（S 為修正方式與預期行為） | README | `Spec.md` | `Spec.md` |
| TEST 與執行結果 | README | `TestPlan.md` | `TestPlan.md` |
| STEP | README | `Implementation.md` | `Implementation.md` |
| Review 報告 | README | `Review.md` | `Review.md` |
| 討論記錄（DEC） | README | `Discussion.md`（有決策時） | `Discussion.md` |
| 後續處理（TODO） | README | README | `FollowUp.md` |

1. 文件只保留最終結果；例外是 `Discussion.md` 可增量描述並凸顯最終決策，`Review.md` 依 review 回合分節保留。
2. 確認點文件的最後一節為「待討論」，格式見 [issue-workflow.md](issue-workflow.md) 第 2 節；沒有待決問題時寫「無」。

## 3. 編號與追溯

| 前綴 | 意義 | 所在文件 |
| --- | --- | --- |
| `REQ` | 需求（BUG 為問題現象） | Goal / README |
| `SPEC` | 規格 | Spec / README |
| `TEST` | 測試項目 | TestPlan / README |
| `STEP` | 實作步驟 | Implementation / README |
| `TODO` | 後續處理 | FollowUp / README |
| `DEC` | 討論決策 | Discussion / README |
| `REV` | Review 發現 | Review / README |

1. 寫法為 `前綴-流水號`（`REQ-1`）；引用子項目寫 `REQ-1.2`。
2. 追溯方向：SPEC 註明 REQ；TEST 註明 SPEC（或 REQ）；STEP 註明 SPEC 與 TEST；REV 的處置可指向 TODO。
3. 確認點確認後，編號不重排、不重用；刪除的項目直接移除，編號跳號。

## 4. 各文件內容

| 文件 | 章節與內容 |
| --- | --- |
| `Goal.md` | 現況盤點；目標；範圍（含範圍外）；REQ 清單 |
| `Spec.md` | 操作規格（對外：用途、操作方式、範例、限制）；實作規格（對內：檔案與資料結構、核心 API）；相容性異動；REQ 對照 |
| `TestPlan.md` | 分類（週期：議題限定 / 長期維護；時機：靜態 / 動態；執行者：自動 / 人工）；總表；各 TEST 的方法與通過條件；SPEC 對照；執行結果 |
| `Implementation.md` | 實作架構（模組與檔案）；各 STEP 的內容、SPEC 與 TEST；SPEC 對照 |
| `Review.md` | 依 [review.md](review.md) 的報告格式，每回合一節 |
| `Discussion.md` | 每個 DEC 一節，依主題整理：原始考量、最終決策（粗體）與依據 |
| `FollowUp.md` | 依範疇分節；每個 TODO 寫目標、完成條件、延後原因、建議做法，內容自足 |

1. 相容性異動分輸入（讀取或接收的資料、設定）與輸出（產生或提供的檔案、報表），每項寫異動內容與既有資料如何處理（相容、轉換或不再支援）；沒有異動寫「無」。
2. STEP 包含程式、測試、研發文件與專案指引；`Implementation.md` 不記 commit hash，對應方式見 [git-workflow.md](git-workflow.md)。
3. 測試執行結果寫最終一次的結果。

## 5. `README.md` 範本

S 級完整範本（所有內容都在 `README.md`）：

```markdown
# <議題編號> <標題>

| 項目 | 內容 |
| --- | --- |
| 分支 | `<分支>` |
| 分支起點 | `<commit>`（<版本>） |
| 級別 | S |
| 狀態 | <目前階段> |

## 問題

1. REQ-1 現象：<操作與結果>
2. 重現方式：
   1. <步驟>

## 修正方式

1. SPEC-1 原因：<原因>
2. SPEC-2 預期行為：<修正後的行為>

## 測試

1. TEST-1 重現測試（SPEC-2）：<方法>；結果：修正前失敗、修正後通過。

## 實作

1. STEP-1 <內容>（SPEC-1、TEST-1）

## Review

<依 review.md 的報告格式>

## 討論記錄

無。

## 後續處理

無。

## DoD

1. [ ] <依 issue-workflow.md 第 5 節 S 級逐項列出>

## Release Note

1. 修改 <處理內容>
```

M、L 級骨架（其餘內容放各檔，見第 2 節）：

```markdown
# <議題編號> <標題>

<議題資訊表，同 S 級>

## 文件

| 文件 | 內容 |
| --- | --- |
| <文件連結> | <內容> |

## 現況、目標與需求（M 級；L 級寫在 Goal.md）

## 後續處理（M 級；L 級寫在 FollowUp.md）

## DoD

1. [ ] <依 issue-workflow.md 第 5 節該級別逐項列出>

## Release Note
```
