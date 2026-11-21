# 10. Round 2 Overall Review

## 10.1 審查資訊與結論

| 項目 | 內容 |
| --- | --- |
| 議題 | AUTO-23254：UI 風格統一 |
| 審查／報告日期 | 2026-11-21 |
| 分支 | `AUTO-23254_f_UIStyleUnify` |
| Round 1 審查版本 | `3d4b5ffd` |
| 實作人修正版本 | `36e704dd` |
| Round 2 審查版本 | `1afaf20d`（含 Round 1 回覆） |
| 回覆依據 | [9_ReviewRound1-reply.md](9_ReviewRound1-reply.md)、討論紀錄 4.15、實作紀錄 3.28 |
| 性質 | 審查快照；問題與建議待討論，不代表已接受或已修正 |

完成通知於 2026-11-21 19:33:48 確認：`review-reply-done.txt` 內容符合約定，且 Round 1 回覆檔已就緒，隨後展開 Round 2。

**原本的 0.001 回歸已修正，多數收尾項目已對齊；但仍有一項合法值精度問題、一項 B3 驗證盲點與文件收尾事項。自動檢查全綠，不代表這些問題已排除，也不代替人工驗收。**

本輪未修改產品程式、檢查程式或既有決議。報告與通知檔在審查完成後產出，不屬於上述審查快照。以下路徑相對於 repo 根目錄，行號對應 `1afaf20d`。

## 10.2 Round 1 回覆的確認結果

| 原 ID | Round 2 確認 |
| --- | --- |
| R1-01 | 原本 0.001 後非法輸入退回 0 的問題已修；最後一位小數的 debounce 通知也已納入 N2。但回復高精度合法值仍有 R2-01 |
| R1-02 | B7 已按檔案限縮，符合決議；B3 已不再整段跳過型別替換，但仍有 R2-02。原始碼比對的證據限制已補入文件 |
| R1-03 | 三支腳本的 `finally` 已涵蓋 runner 編譯、準備與執行。正常執行後無殘留；本輪未重跑實作人的編譯失敗注入實驗 |
| R1-04 | 相容性文件第 3 項已補齊共用文字檔、共用 ID、全畫面一致與不能保留各畫面不同客製文字的影響；26 個 Designer 指派與記載相符 |
| R1-05 | 已依決議移除規範的 Title 與未使用 API，不重報為未實作；架構章摘要仍需同步，見 R2-03 |
| R1-06 | `RemovableValueRow` 已設定 `Kind = ButtonKind.Danger`；動態列的人工外觀確認仍待完成 |
| R1-07 | README 與合併門檻已區分自動檢查／人工驗收，三項人工確認仍是合併前必做 |
| R1-08 | 專案概觀與 `CommonUI.csproj` 逐檔紀錄已補齊；本輪新增／修訂內容尚有 R2-03 的文件落差 |
| R1-C1 | AutoSort 的滾輪與超界回復差異已有正式接受紀錄、規範與例外理由；N1、N3 已涵蓋原回覆列出的主要案例 |

原 R1-01 的嚴重度說明可接受：問題是合法輸入被吃掉，並非畫面與模型永久不一致。本輪不以「資料錯亂」描述 R2-01。

## 10.3 自動驗證結果與限制

| 驗證 | 本輪結果 |
| --- | --- |
| MyCAM 專案建置（Visual Studio） | 成功，涵蓋相關 C# 專案相依 |
| UI 靜態檢查 | 34 個 Designer 檔，0 個問題 |
| UI 執行時檢查 | zh-TW、zh-CN 各 42 個畫面狀態，0 個問題 |
| 共用數值框 N1–N3 | runner 輸出 11 個檢查，0 個問題 |
| 主視窗檢查 | 13 組通過；P6 的 10 個區域相符 |
| 行為比對 | 與 `a4b3826d` 比較，0 個未登錄的差異；限制見 R2-02 |
| 分支規範檢查 | 351 個檔案，0 個問題 |
| MainLayout 基準比對 | 152 行，0 個差異 |
| 產品輸出殘留 | Debug、Release 輸出第一層均無 `UICheck*` runner |

完整 `UICheck.bat runtime` 與 `RunIssueChecks.bat` 串行重跑，結果為 `UI_RESULT=0`、`ISSUE_RESULT=0`，避免共用 runner 衝突。

本輪產生 97 張截圖，當時保留於 `%TEMP%\MyCAM_UIReview_Round2_20261121_194139`，不納入版控。未完成人工目視；隱藏桌面的 OpenGL `wglMakeCurrent` 訊息是已知限制，不列為本輪新增問題。

限制與證據邊界：

- 本輪實際成功的是 Visual Studio 的 MyCAM 專案建置，不將未成功的命令列 `Build.bat` 呼叫記為通過。
- N1–N3 與額外實驗使用控制項／輸入驗證路徑，不是機台、實體鍵鼠或完整畫面的實機驗收。
- 實作人記錄的舊版反向測試與編譯失敗清理實驗未在本輪重跑；不與本輪親自執行的結果混用。
- 沒有修改產品原始碼來執行 mutation，因此下列測試缺口不標為已驗證的 mutation survivor。
- `5_Verification.md` 5.3 第 2～4 項（最終 Designer、數值框實機操作、CNC 連線）仍未完成，合併門檻不變。

## 10.4 待討論問題

| ID | 優先度 | 問題 | 分類 |
| --- | --- | --- | --- |
| R2-01 | 優先 | 非法輸入仍可能捨入最新合法值並送出 `ValueChanged` | 合法值回復／精度 |
| R2-02 | 中 | B3 忽略 baseline 未明寫的共用屬性新增覆寫 | 驗證防線 |
| R2-03 | 低 | 架構摘要、Silent 通知例外與驗證版本統計未完整對齊 | 文件收尾 |

### R2-01：回復文字的格式化可能改變合法值

**輸入與觀察**

1. 建立數值框，`DecimalPlaces = 3`。
2. 設定 `SilentSetValue = 1.23456`：控制項接受此範圍內的 decimal；顯示位數不等於底層值已被捨入。
3. 在底層編輯框輸入 `abc`，走 `ValidateEditText` 路徑，再讀取 `Value`。

| 對照 | 值 | 拒絕輸入後的 `ValueChanged` |
| --- | --- | --- |
| 預期 | 1.23456 | 無 |
| 現行控制項 | **1.235** | **[1.235]** |
| 原生控制項對照 | 1.23456 | — |

此結果已用現行建置 DLL、底層編輯框與重複讀值確認，不只是改變顯示文字。原生控制項對照保留 1.23456；沒有宣稱重新編譯並執行了 `3d4b5ffd` 的舊控制項。

**原因與產品影響**

- `CommonUI\Controls\DebouncedNumericUpDown.cs:185–189` 以 `F + DecimalPlaces` 格式化 `m_LastValidValue`，只回寫文字。
- 格式化後的文字是 1.235；在原生編輯／驗證流程中，它可能再被接受為數值，導致合法值與事件改變。
- 規範 `.github\instructions\ui-style.instructions.md:287` 要求回復最新合法值，且回復本身不送出 `ValueChanged`，此案例不符合。
- `MyCAM\Editor\Dialog\LeadDlg.cs:19–26` 從既有 double 參數以 `SilentSetValue` 載入，`104–109` 在預覽／確定時讀回數值。此類呼叫並未先強制捨入為顯示位數，因此問題可能影響讀回與儲存的參數，而非只有外觀。

**建議**：保留完整合法 decimal，並正確同步原生編輯狀態，不只回寫格式化文字；仍須避免在拒絕輸入的遞迴驗證路徑直接讀取 `Value`。不要用提前捨入模型值來掩蓋問題。先補能重現此案例的回歸檢查，再修正控制項。

### R2-02：未明寫的共用屬性仍被跳過

`Doc\UIStyleUnify\Tests\CompareBehavior.ps1:202–206` 以「型別改變，而且 baseline 屬性沒有文字值」直接跳過比較。這不區分：

1. 真正只有新型別才有的屬性。
2. 兩型別都有、baseline 使用預設值的屬性。

具體例子：

- AutoSort baseline 的原生 `NumericUpDown` 沒有明寫 `ReadOnly`，實際預設是 false。
- 新型別也具有此屬性，預設同為 false。
- 若在新 Designer 新增 `ReadOnly = true`，使用者的上下鍵／滾輪操作會被禁止，卻仍符合現行 skip 條件。

本輪讀取 baseline 宣告、確認兩型別的預設值，並執行現行 B3 條件原樣判斷，結果為 skip。**沒有修改 Designer 後重跑全套，也沒有發現目前已有畫面真的被改成唯讀；這是驗證盲點，不是另一個已確認的現行操作回歸。**

**建議**：共用行為屬性應納入有依據的預設值對照，或以等效的基準驗證補足；只有新型別獨有的設定才排除。若仍選擇只比較顯式宣告，文件必須明確寫出此限制，不能等同於「只忽略新型別才有的屬性」。

**三行 B3 例外的範圍**

`BehaviorExceptions.txt:30–32` 已依三個畫面與相應座標標籤登錄，理由能解釋實作人記錄的 ReadOnly、TabStop、Enabled 替換差異。本輪不把它們另列為已存在的操作回歸。

但 `CompareBehavior.ps1:95–101` 的例外 key 未包含屬性，理由也不參與匹配。若擴充預設值比對，需一併將例外限定到已接受的屬性與差異，避免把同一控制項的其他狀態變化一併放行。

B7 按檔案限縮已符合 Round 1 決議，不重報為全域豁免。

### R2-03：文件的最終狀態仍有漏同步

1. `Doc\UIStyleUnify\2_Architecture.md:55` 的 TextRole 摘要仍列「標題」，但 Title 已依決議刪除；`154` 的長期檢查表尚未列 N1–N3。
2. `.github\instructions\ui-style.instructions.md:288` 寫「任何值的變化」都送出 `DebouncedValueChanged`，需明確排除 `SilentSetValue` 的通知抑制與待送通知取消；此 API 的現行行為是有意設計，不建議為配合文句而改行為。
3. `5_Verification.md:102` 記 347 個檔案，`9_ReviewRound1-reply.md:86` 記 348，本輪 `1afaf20d` 重跑為 351。應記錄各次驗證對應的版本，或更新持續維護的結果，避免把不同時點的統計當成同一最終快照。

**建議**：同步最終規範與摘要；歷史回覆保留當時結果，但標明驗證版本。不要為統一數字而修改 Round 1 審查快照。

## 10.5 N1–N3 回歸防線評估

**Mixed：原問題已有有效防線，但重要互動尚未受保護。**

N1 有具體值與事件序列斷言，N2 釘住最後一位小數的通知次數／值，N3 釘住單格滾輪與上限夾值；這些檢查有實際價值。不能以 11 個檢查通過推論所有回復與通知路徑均已驗證。

以下為對現有 `Tools\UICheck\ControlRunner.cs:31–89` 的來源／斷言對照。**No coverage 指現有永續 N1–N3 沒有驗證該行為分區，不是本輪額外實驗也沒有涵蓋，更不是已執行的 mutation survivor。**

| 風險 | 未受保護的行為 | 可能漏掉的變更 | 結果／證據 | 最小補測案例 |
| --- | --- | --- | --- | --- |
| 優先 | 回復比顯示位數更精確的合法值 | 回復時捨入或改值 | No coverage；R2-01 已實測失敗 | 3 位顯示、合法值 1.23456，非法輸入後仍為 1.23456，無額外 `ValueChanged` |
| 中 | Silent 取消待送通知並保留後續正常通知能力 | 少了 timer 取消或誤抑制後續變化 | No coverage；額外正向實驗通過 | `10 → 20（待通知）→ Silent 30`，通知 0 次；再改 30.001，通知 1 次且值正確 |
| 中 | 空白 Enter／離焦的回復與通知互動 | Enter 沒被處理／抑制，或離焦沒有完成待送通知 | No coverage；額外編輯框實驗的值、通知與 Enter flags 通過 | 最新值 20、編輯框清空；保留 20，無額外改值；確認離焦通知及 Enter 的 Handled／SuppressKeyPress |
| 中 | 文字輸入的上下限與相鄰值 | 邊界誤拒或近界超範圍值被接受／夾值 | No coverage；目前只有較遠的超界案例與滾輪上限 | 初值 20、範圍 0～1000：接受 0、1000；拒絕 −0.001、1000.001 並保留 20 |
| 中 | 連續變動的 debounce 合併 | 沒有重新計時，導致提前或重複通知 | No coverage；N2 只改一次值 | 小於 interval 的間隔連續改為 20、21，最後只通知一次且值為 21 |
| 低 | 滾輪下限與 ReadOnly 阻擋操作 | 缺下限夾值或唯讀保護 | No coverage；額外下限／唯讀正向實驗通過 | 下限 0、值 0.2、Increment 0.5、向下滾輪後為 0；唯讀時滾輪與上下鍵不改值 |

本評估聚焦回覆要求的回復、通知、Silent、空白焦點／Enter 與 N1–N3，不擴張為全工作區測試品質審計。未修改產品或既有檢查來執行 mutation。

## 10.6 R2-01 的最小重現

在新的 Windows PowerShell 工作階段執行，使用審查版本的 Debug 建置 DLL。此實驗不開視窗、不連線機台、不寫產品設定，結束時釋放控制項。

```powershell
$root = 'D:\Decrypt\MyCAM\MyCAM\.claude\worktrees\ui-style-unify'
$dll = Join-Path $root 'MyCAM\bin\Debug\CommonUI.dll'
Add-Type -AssemblyName System.Windows.Forms
[void][Reflection.Assembly]::LoadFrom($dll)
$code = @'
using System;
using System.Windows.Forms;
using CommonUI.Controls;

/// <summary>Exposes the editor validation path without showing a window.</summary>
public class Round2PrecisionProbe : DebouncedNumericUpDown
{
	/// <summary>Write into the native editor and validate the input.</summary>
	public void EnterText( string szText )
	{
		foreach( Control child in Controls ) {
			TextBox editor = child as TextBox;
			if( editor == null ) {
				continue;
			}
			editor.Text = szText;
			ValidateEditText();
			return;
		}
		throw new InvalidOperationException( "No numeric editor found." );
	}
}
'@
Add-Type -TypeDefinition $code -ReferencedAssemblies @(
	'System.dll', 'System.Core.dll', 'System.Drawing.dll',
	'System.Windows.Forms.dll', $dll
)
$numeric = New-Object Round2PrecisionProbe
try {
	$numeric.DecimalPlaces = 3
	$numeric.SilentSetValue = [decimal]1.23456
	$valueList = New-Object 'System.Collections.Generic.List[decimal]'
	$numeric.add_ValueChanged({ $valueList.Add($numeric.Value) })
	$numeric.EnterText('abc')
	$first = $numeric.Value
	$second = $numeric.Value
	Write-Output "Value reads: $first, $second"
	Write-Output ("ValueChanged: [" + ($valueList -join ',') + "]")
}
finally {
	$numeric.Dispose()
}
```

審查版本結果為兩次讀值皆 1.235，事件 `[1.235]`。應保留 1.23456 且沒有拒絕輸入造成的額外改值事件。此重現使用底層編輯框與受保護的驗證路徑，不宣稱代替實體鍵鼠或整個對話框的人工驗收。

## 10.7 建議處理順序與交付訊號

1. **R2-01**：先補精度回復案例，再修正控制項；同時保持原 R1-01 與 N2 通知案例通過。
2. **R2-02**：決定如何補共用屬性的預設值比對，並相應限縮 B3 例外；若保留現行範圍，須明確記錄驗證限制。
3. **測試缺口**：優先補 Silent 與空白 Enter／離焦互動，再補範圍邊界、連續 debounce、下限／唯讀案例。
4. **R2-03**：同步文件摘要、通知例外與驗證版本，不修改歷史審查快照。
5. **人工驗收**：最終修正後執行 `5_Verification.md` 5.3 第 2～4 項；不能因自動檢查全綠而省略。

本報告檢查完成後，以同目錄 `review-log-round2.txt` 作為交付訊號，內容為 `review-log-round2`。該訊號只表示 Round 2 報告就緒，不代表產品問題已修正或可合併。
