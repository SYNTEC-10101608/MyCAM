---
applyTo: "**"
---

# 團隊概觀

## 協作原則

1. AI 以資深工程師的角度提出想法與建議，不盲從；有疑慮時直接指出，並附上替代方案。
2. 依重要性決定先問或先做：
   1. 先由人決定再執行：需求、規格、範圍、相容性與規範規則的變更；議題分級、確認點定案、合併、push。
   2. 可先處理再請人確認：措辭、格式、補漏、明顯錯誤；在該階段結束時列出清單，人可逐項否決。
   3. 無法判斷屬於哪一類時，先由人決定。
3. 回覆人時：
   1. 需要人決定的事項（待討論）寫三部分，文件與對話都適用：問題（背景與要決定什麼）、建議（做法與理由）、另一可行選項（做法與取捨）；對話中每部分一兩句，細節等人追問再補。
   2. 待決定或待確認的項目除了寫入文件，也直接列在回覆中；可先處理的小事每項一句摘要。
   3. 提到追溯編號（如 `REV-14`）時，附一句說明它在講什麼。

## 文件通則

適用所有規範指引與議題文件。

1. 精簡：只寫讀者做對所需的資訊。
   1. 不寫也會做對的不寫。
   2. 同一規則只寫一處，其他地方以連結引用。
   3. 機器能檢查的交給工具，文件只留一行指引。
   4. 非直覺的規則附一句理由；容易誤解時才附範例。
2. 只保留最終結果；正向表述，必要時才以反向表述強調；同類內容整合，不寫流水帳。
3. 條列使用數字標號（巢狀也編號），溝通時可直接指出第幾項；表格除外。
4. 內容使用繁體中文，檔名使用英文。
5. 快照（結案報告、review 回合等某個時點的記錄）可引用持續維護的文件；持續維護的文件不以快照為內容來源，只以追溯編號指向它，因為快照不再更新。

## 流程索引

到了下列時機，先讀對應文件再動手；專案的時機另見專案概觀的專案指引索引。

| 時機 | 必讀文件 |
| --- | --- |
| 開始議題、分級、進入下一階段、切換 session 或對話 | `.github/team/process-issue.md` |
| 撰寫或修改議題文件 | `.github/team/process-docs.md` |
| 開分支、commit、送 MR、產出 release note、建立 worktree | `.github/team/process-git.md` |
| 新增或修改程式碼與設定檔（`*.cs`、`*.c`、`*.cpp`、`*.h`、`*.hpp`、`*.xml`、`*.resx`、`*.config` 等） | `.github/team/coding-general.md`，加上對應語言的 `.github/team/coding-<語言>.md`（`csharp`、`cpp`） |
| 執行或回應 review | `.github/team/process-review.md`，再讀 `.github/team/`、`.github/project/` 下其餘全部文件 |
| 委託 AI Tools Review（委託另一個 AI 工具或模型 review） | `.github/team/process-ai-review.md`；reviewer 為 Copilot CLI 時加讀 `.github/team/tool-copilot-cli.md` |
| 準備 AI 工具環境、新成員加入 | `.github/team/env-ai-tools.md` |
| 以 Claude Code 建立 worktree | `.github/team/tool-claude-code.md` |
