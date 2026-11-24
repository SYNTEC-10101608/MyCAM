# Git 流程

開分支、commit、送 MR、產出 release note、建立 worktree 時讀本文件。

## 1. 版本與合併

1. 版本號 `0.<中>.<小>`；中版號是主要迭代，每版開版本分支 `0.<中>.0`；小版號只用於 hotfix。
2. 版本完成時在版本分支提交 `RLS_0.<中>.<小>`。
3. 議題分支以 MR squash 合併進目標版本分支；合併後保留議題分支，分支內的 commit 供追溯。
4. 送 MR 由人決定；AI push 前先詢問 remote 與分支。

## 2. 分支

1. 命名 `<議題編號>_<類型>_<簡述>`；議題編號含前綴（`AUTO-23201`）；類型 `f` 為功能、`b` 為 bug；簡述為英文 PascalCase（例：`AUTO-23201_b_CannotOutputFrogLeapMacro`）。
2. 後綴：
   1. 同一議題合併後的後續 MR：原名加 `_2nd`、`_3rd`…。
   2. 並存比較的另一種實作：原名加 `_Alt2`、`_Alt3`…。
3. 送 MR 前的自測回合不開新分支，在原分支以 commit 推進。
4. Rebase 前的暫時 squash 分支只留在本機，不 push，用完刪除。
5. 開分支前先 fetch 遠端；fetch 失敗時，新分支會以本機舊的遠端分支為基準，須先確認。

## 3. Commit

1. 每個 STEP 一個 commit；文件、測試、程式盡量分開。
2. 訊息明確描述修改內容，中英文皆可。
3. 訊息結尾標示修改的來由：施工 `(STEP-n)`、review 修正 `(REV-n)`，兩者皆是時並列（`(STEP-3, REV-2)`）；一個 STEP 可對應多個 commit。確認點文件（階段 1～5）的 commit 不標示。
4. 以 `git log --grep "STEP-n"` 查詢 STEP 對應的 commit，議題文件不記 commit hash。
5. 純格式調整（含 Coding Style 的整個方法改寫）獨立 commit。

## 4. Release note 與 merge log

1. Release note：
   1. 讀者為團隊與 QA，直接描述做了什麼處理，不寫模組名稱。
   2. 數字條列，每條以 `新增`、`修改` 或 `刪除` 開頭，前綴與內容間空一格，不加冒號。
   3. 由 AI 產出以統一風格，寫入議題 `README.md` 的 Release Note 章節。
2. Merge log（MR 描述）由 release note 組合而成，不另存於議題文件：

   ```text
   Merge branch '<分支>' into '<目標版本>'

   by <名字>

   1. 新增 ...
   2. 修改 ...
   ```

   1. 條列沿用 release note。
   2. AI 產出時 `<目標版本>` 留空，由合併者在合併時填入。

Release note 範例：

```text
1. 新增 路徑編輯支援批次調整引入長度。
2. 修改 碰撞檢查失敗時的處理，改為顯示警告並繼續輸出 NC 程式。
3. 刪除 路徑設定中已停用的舊版引入方式選項。
```

## 5. Worktree

1. 一個議題一個 branch。Worktree 為選用，同時處理多個議題時使用；單線開發直接在主 checkout 切分支。
2. 未追蹤的檔案不會出現在新 worktree；需要複製的檔案與初次建置步驟見專案指引。
