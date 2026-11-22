# 12. Round 3 Overall Review

## 12.1 審查資訊與結論

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254：UI 風格統一 |
| 審查／報告日期 | 2026-11-21 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| Round 2 審查版本 | `1afaf20d`（報告快照 `6086614e`） |
| 實作人修正版本 | `46c1be30` |
| Round 3 審查版本 | `b75f03dd`（含 Round 2 回覆） |
| 回覆依據 | [11_ReviewRound2-reply.md](11_ReviewRound2-reply.md)、討論紀錄 4.16、實作紀錄 3.29 |
| 性質 | Review 快照；問題與建議待討論，不代表已接受或已修正 |

2026-11-21 21:49:55 收到新 `review-reply-done.txt`，內容為 `review-reply-done`，並確認 Round 2 回覆檔已就緒，隨後展開本輪審查。

**Round 2 的精度重現已修正，N1–N4 與 B3 原案例均通過；但仍有一項未符合規範的原生上下按鈕輸入路徑、B3 生效值比對的範圍缺口，以及 debounce 時間門檻未受測試保護。自動檢查全綠不代表這三項已排除。**

本輪以回覆的四個重點追查：原生輸入流程、B3 生效值／例外、N1–N4 防線、文件對應。未修改產品或檢查程式，未提交；本報告與 README 索引在審查後產出。以下程式路徑相對於 repo 根目錄，行號對應 `b75f03dd`。

## 12.2 Round 2 回覆的確認結果

| 原項目 | Round 3 確認 |
| --- | --- |
| R2-01 | `UserEdit`／`ChangingText` 修正能保留 1.23456；原非法文字、超界與再次驗證案例通過。無新輸入的離焦、Enter、`UpdateEditText` 也保留精度。**但先輸入超界文字再直接按上下按鈕，仍有 R3-01** |
| R2-02 | 原 `NumericUpDown → DebouncedNumericUpDown` 的 `ReadOnly` 盲點已修；B3 例外確實限定屬性；decimal、負數、列舉、布林的代表值正規化正確。**白名單替換仍未走生效值比較，見 R3-02** |
| 10.5 六類測試缺口 | 都有新增案例：精度、邊界、Silent、空白 Enter／離焦、連續變化、下限／唯讀。新增內容有實際防線，不是只增加數量。連續變化的通知值／次數有保護，但尚未保護從最後一次變化起計時，見 R3-03 |
| R2-03 | 架構 TextRole 摘要與 N1–N4 表格已同步；UI 規範已排除 Silent 通知，加入精度、邊界、唯讀與空白輸入。驗證版本的首段敘述仍建議收尾，見 12.5 |

新增五行 B3 例外已查閱：兩個畫面的 `TabStop` 及 `AxisTransformDlg` 的 `Minimum`、`Maximum`、`DecimalPlaces`，對應 4.16 的決議，並非控制項所有屬性的通用豁免。本輪沒有發現它們新增客製檔遷移要求；`7_Compatibility.md` 的三項外部檔案相容性內容未因這次修正改變。R3-01 是待修正的操作差異，不應直接加入相容性例外放行。

## 12.3 本輪驗證結果與限制

| 驗證 | 本輪親自執行的結果 |
| --- | --- |
| MyCAM 專案建置（Visual Studio） | 成功 |
| `CheckControls.ps1` | N1–N4：32 個 runner 案例／檢查，0 個問題 |
| `CompareBehavior.ps1` | 與 `a4b3826d` 比較，0 個差異；證據範圍見 R3-02 |
| `CheckStatic.ps1` | 34 個 Designer 檔，0 個問題 |
| `CheckBranchRules.ps1` | 353 個改動檔案，0 個問題；報告產出前的 `b75f03dd` 工作目錄 |
| `CompareMainLayout.ps1` | 12 個情境、152 行，0 個差異 |
| 額外原生控制項路徑 | 無新輸入／已拒絕後的精度保留通過；未驗證文字直接進入上下按鈕時重現 R3-01 |
| B3 記憶體探針 | 原 ReadOnly 比較、屬性例外與正規化通過；白名單分派重現 R3-02 的範圍缺口 |
| debounce 正向時間探針 | interval 500ms；最後一次變化後約 355ms 通知 0 次；之後通知 1 次、值 21，沒有提早通知 |
| 產品輸出殘留 | Debug、Release 第一層均無 `UICheck*` runner；本輪失敗的臨時探針編譯目錄已自動移除 |

限制：

1. 本輪採針對修正面的檢查，沒有重跑完整 `UICheck.bat runtime` 的 T1–T7／13 組 P1–P6，也未人工目視截圖。實作人的全套通過紀錄見 11.4，不能當成本輪親自重跑的結果。
2. 額外輸入實驗使用實際建置 DLL、底層編輯框及公開／受保護方法，不開可見視窗、不連線機台、不寫產品設定；不是實體鍵鼠或完整對話框驗收。
3. 未重跑實作人的控制項反向替換、Designer `ReadOnly = true` 注入；本輪對 B3 使用現行函式與記憶體 fixture，沒有修改產品建構子或 Designer。
4. 沒有執行產品原始碼 mutation；R3-03 的候選變更為 **Candidate survivor (unverified)**，不是已執行的 survivor。
5. `5_Verification.md` 5.3 第 2～4 項仍未完成；最終 Designer、實機數值輸入、CNC 連線三項合併門檻不變。

## 12.4 待討論問題

| ID | 優先度 | 問題 | 分類 |
| --- | --- | --- | --- |
| R3-01 | 優先 | 超界文字尚未驗證時，上下按鈕先走原生夾值流程，沒有回復最新合法值 | 現行操作問題 |
| R3-02 | 中 | 白名單中的實際型別替換仍只比較 Designer 文字，不比較生效預設值 | 驗證防線／文件範圍 |
| R3-03 | 中 | debounce 合併案例只驗證最終值與次數，未驗證從最後一次變化起等待 interval | 測試缺口 |

### R3-01：原生上下按鈕先處理了被拒的文字

**最小輸入**：範圍 0～1000、`Increment = 1`、合法值 20；在底層編輯框寫入超界文字，**不要先呼叫 `ValidateEditText`**，直接呼叫公開的 `UpButton`／`DownButton`。

| 編輯文字 | 動作 | 現行值 | `ValueChanged` 序列 |
| --- | --- | --- | --- |
| `2000` | `UpButton` | **1000** | **[1000]** |
| `2000` | `DownButton` | **999** | **[1000, 999]** |
| `-1` | `UpButton` | **1** | **[0, 1]** |
| `-1` | `DownButton` | **0** | **[0]** |
| `2000`／`-1` | 先驗證，再 `UpButton` | 21 | [21] |
| `2000`／`-1` | 正向滾輪 | 21 | [21] |

**應維持的要求**：被拒的文字不能先把合法值 20 改成上下限，也不應產生由該次夾值造成的 `ValueChanged`。若保留按鈕本身的一步動作，應從回復後的 20 起算，向上 21、向下 19；若選擇拒絕後不執行該次按鈕，需明確決議。兩者都不是目前的 1000／999／1／0。

原因與影響：

- `CommonUI\Controls\DebouncedNumericUpDown.cs:91–107` 只檢查 `ReadOnly`，隨後直接呼叫原生上下按鈕；尚未驗證的編輯文字先被原生流程解析、夾值，再進入改值通知。
- `148–170` 的拒絕流程在這條入口沒有先執行。`181–192` 所依賴的「被拒文字從未進入 Value」不適用於此時序：原生夾值已經改了 Value，再清除 `UserEdit` 也只能保留被改後的值。
- 同一項操作依「先按 Enter 驗證／直接按上下按鈕／使用滾輪」得到不同結果。讀取模型或事件的呼叫方會觀察到改值，不只是顯示文字。
- `ControlRunner.cs` 的 N1 透過 `EnterText` 立即驗證；N3 只從已合法的值開始，沒有交叉測試未驗證文字與按鈕，因此現有 32 個檢查全綠仍可重現。

此路徑本輪首次追查到；不宣稱是 `46c1be30` 才引入。它與數值框統一後的拒絕輸入規範直接相關，且正是 11.5 要確認的原生上下按鈕入口。

**建議**：在原生上下按鈕解析文字之前先走共用的拒絕／驗證流程，再依一致的規則執行該次增減；不以事後再讀 `Value` 修補，也不以提前捨入或相容性例外掩蓋。補上上述四個序列，以及精確值 1.23456 的按鈕交叉案例，確認沒有額外夾值事件，並保留原 N1–N4 通過。

### R3-02：實際型別改變與 B6 是否允許仍共用同一旗標

`Doc\UIStyleUnify\Tests\CompareBehavior.ps1:274–297`：

1. `$isSameKind` 包含「型別完全相同」與「位於 TYPE_REPLACEMENTS 白名單」。
2. `$isTypeChanged = -not $isSameKind`。
3. 只有此旗標為 true 才執行 `Compare-EffectiveValue`；否則比較 Designer 原始文字。

例如 `System.Windows.Forms.Button → CommonUI.Controls.ThemedButton`：實際型別不同，但旗標為 false；`Label → ThemedLabel`、`DataGridView → ThemedDataGridView` 等也一樣。因此 5.2、3.29 與回覆中「型別替換後比對生效值」仍不涵蓋所有實際替換。

**記憶體探針**：載入現行函式／白名單，以新按鈕實例設定 `Enabled = false` 作為模擬預設值的 fixture，舊／新 Designer 都省略 Enabled：

| 相同 fixture 的比較方式 | 結果 |
| --- | --- |
| 現行白名單分派：原始文字 `''` 對 `''` | 0 個差異 |
| 直接呼叫現行 `Compare-EffectiveValue` | `Enabled changed (effective): 'true' -> 'false'` |

這驗證的是分派防線，不是產品建構子 mutation：**沒有改 ThemedButton 原始碼，沒有在 Designer 注入，也沒有發現目前的按鈕預設為停用。** 原 ReadOnly 案例已修，因原生數值框轉換不是這份白名單項目；不能把該單一成功案例當成其他替換都有生效值保護。

**建議**：分開「實際 CLR 型別是否改變」與「B6 是否允許該替換」的判斷。共用行為屬性的生效值比對不應因外觀替換已獲允許而繞過。舊自訂型別已不存在於現行組件的情況，需以有依據的舊型別預設值／對照處理，不可直接假設或默默跳過；保留容器 TabStop 等已決議規則及屬性級例外。補白名單替換的預設值相同／不同兩類 fixture，並同步比對範圍文件。

正規化與例外的正向確認：1.23456 的 decimal bits、負 decimal、`DialogResult.OK`、false 均正確；只豁免 ReadOnly 時，Enabled 的 false 覆寫仍報出。本輪未發現這些代表型別的錯誤，問題在分派範圍，不在這幾個正規化案例。

### R3-03：連續變化未釘住「最後一次變化後」的時間門檻

`Tools\UICheck\ControlRunner.cs:115–126` 設 interval 300ms，改 20、等約 50ms、改 21，等待任一通知出現，再確認最終序列是 [21]。這保護合併的次數與值，**沒有檢查第二次改值後、interval 尚未屆滿時不能通知**。

具體候選變更：移除 `DebouncedNumericUpDown.cs:209` 的 `m_DebounceTimer.Stop()`。當 timer 已啟動，再呼叫 Start 不會重新起算；在正常排程下仍只通知一次 21，但以第一次變化的 timer 為準，早於第二次變化後的完整 interval。現有最終序列斷言沒有觀察這個差異。

| 公開序列 | 應保護的結果 | 現有斷言 | 分類 |
| --- | --- | --- | --- |
| interval 500ms，改 20，約 200ms 後改 21 | 最後一次變化後約 350ms 應仍為 0 次；至少等待最後一次變化後 500ms，才可通知一次 21 | 只驗證最後一次值與次數，不驗證早期無通知 | **Candidate survivor (unverified)**；時間分區未覆蓋 |

本輪沒有移除 Stop 或執行 mutant。未修改控制項的正向探針在最後一次變化後約 355ms 通知 0 次，之後通知一次 21；只證明現行正向序列正確，不能代替測試防線。

**建議**：保留最終 [21] 的斷言，另記錄每次變化與通知的單調時間，驗證首個通知不早於最後一次變化後的 interval；在早期窗口明確斷言 0 次。使用足夠寬的間隔與容許延遲，避免把 OS 排程較晚通知當成失敗；但不得放寬「不可提早」的門檻。

## 12.5 防線評估與文件收尾

**Mixed：主要舊問題已有有效防線，尚有原生按鈕入口與時間分區未受保護。**

| 行為 | 現有防線／本輪觀察 |
| --- | --- |
| 0.001、被拒後保留尚未通知值／完整精度 | N1 有值與事件序列斷言；本輪通過 |
| 正好上下限接受、相鄰超界拒絕 | N1 有邊界與事件序列斷言；本輪通過 |
| 最後一位小數通知、連續變化只通知一次最後值 | N2 已保護；從最後一次變化起等待完整 interval 尚未保護（R3-03） |
| 滾輪上下限、唯讀操作 | N3 已保護；未驗證文字與非唯讀上下按鈕的交叉入口為 **No coverage**（R3-01 已實測） |
| Silent 取消 pending、後續恢復通知、空白 Enter／離焦 | N4 有新增斷言；本輪通過 |

不把缺少特定函式的專用測試一律視為缺口：R3-01 有不同公開結果，R3-03 有不同公開通知時序；沒有為重複覆蓋或私有實作細節要求更多案例。

文件收尾建議（非另一項已確認的操作回歸）：

- `5_Verification.md` 首段仍引用 Round 1 修正 3.28；結果表寫 Round 2 修正版本，但沒有具體 hash。建議首段直接標明 `46c1be30` 的驗證工作目錄，後續實際修正後再更新最終驗證版本，避免「最後一次執行」與階段摘要混淆。
- 本輪 `b75f03dd` 量到 353；11.4 在修正工作目錄記 352。兩者是不同時點，不能只因數字不同就判定歷史紀錄有錯；本輪數字保留於本報告，不改既有回覆或 Review 快照。
- 完成 R3-01／R3-02 後同步 3.29、驗證內容及比對範圍；不能仍以「所有原生輸入都保留完整精度／所有替換都比較生效值」概括目前已確認的子集。

## 12.6 R3-01 最小重現

在新的 Windows PowerShell 工作階段，以審查版本的 Debug DLL 執行。寫入的是底層編輯框，未在按鈕前預先驗證；控制項於每個案例結束時 Dispose。

```powershell
$root = 'D:\Decrypt\MyCAM\MyCAM\.claude\worktrees\ui-style-unify'
$dll = Join-Path $root 'MyCAM\bin\Debug\CommonUI.dll'
Add-Type -AssemblyName System.Windows.Forms
[void][Reflection.Assembly]::LoadFrom($dll)
$code = @'
using System;
using System.Windows.Forms;
using CommonUI.Controls;

/// <summary>Exposes typing separately from validation.</summary>
public sealed class Round3ArrowReportProbe : DebouncedNumericUpDown
{
	/// <summary>Write into the editor without validating.</summary>
	public void TypeText( string szText )
	{
		foreach( Control child in Controls ) {
			TextBox editor = child as TextBox;
			if( editor != null ) {
				editor.Text = szText;
				return;
			}
		}
		throw new InvalidOperationException( "No numeric editor found." );
	}

	/// <summary>Validate the pending edit.</summary>
	public void ValidateNow()
	{
		ValidateEditText();
	}
}
'@
Add-Type -TypeDefinition $code -ReferencedAssemblies @(
	'System.dll', 'System.Core.dll', 'System.Drawing.dll',
	'System.Windows.Forms.dll', $dll
)
foreach ($action in @('up', 'down', 'validate-up')) {
	$numeric = New-Object Round3ArrowReportProbe
	try {
		$numeric.Minimum = 0
		$numeric.Maximum = 1000
		$numeric.SilentSetValue = 20
		$valueList = New-Object 'System.Collections.Generic.List[decimal]'
		$numeric.add_ValueChanged({ $valueList.Add($numeric.Value) })
		$numeric.TypeText('2000')
		switch ($action) {
			'up' { $numeric.UpButton() }
			'down' { $numeric.DownButton() }
			'validate-up' { $numeric.ValidateNow(); $numeric.UpButton() }
			default { throw 'Unknown action.' }
		}
		Write-Output ("$action => " + $numeric.Value + '; events=[' + ($valueList -join ',') + ']')
	}
	finally {
		$numeric.Dispose()
	}
}
```

審查版本輸出：`up => 1000; events=[1000]`、`down => 999; events=[1000,999]`、`validate-up => 21; events=[21]`。這是控制項公開按鈕入口的重現，不宣稱實體鍵鼠或機台驗收已完成。

## 12.7 建議處理順序與交付訊號

1. **R3-01**：先補未驗證超界文字與上下按鈕的交叉案例，再修正解析入口；保留精度、範圍、事件與 Silent 防線，不以夾值取代已決議的回復行為。
2. **R3-02**：分開型別實際改變與允許替換的判斷，補白名單 fixture，正確處理舊自訂型別預設值，保留屬性級例外。
3. **R3-03**：新增早期無通知與最短時間門檻斷言，保留最終值／次數；如做反向測試，在測試程式／臨時副本進行並還原，不殘留產品 mutation。
4. **文件／驗收**：更新最終驗證版本與範圍；最新修正後仍須完成 5.3 第 2～4 項，不能用自動檢查全綠取代。

報告核對完成後，以同目錄 `review-log-round3.txt` 作為交付訊號，內容為 `review-log-round3`。訊號只表示報告就緒，不代表問題已修正或可以合併。
