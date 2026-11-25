# Coding Style：C#

另須遵守 [通用規則](coding-general.md)。

## 格式

- 一個 `.cs` 檔只放一個頂層型別，檔名與型別名稱相同（巢狀型別不限）。
- 型別、方法、屬性、lambda 的 `{` **換行**；`if` / `for` / `while` / `switch` / `try` 等流程控制的 `{` **不換行**。
- `else`、`else if`、`catch`、`finally` 前換行。
- `if` / `for` / `foreach` / `while` 一律加大括號，即使只有一行。
- 一行只寫一個陳述式。
- `switch` 的 `case` 縮排一層。

空白規則：

| 情況 | 寫法 |
| --- | --- |
| 流程控制關鍵字後 | `if( a > b ) {`、`for( int i = 0; i < n; i++ ) {` |
| 方法呼叫 / 宣告有參數 | `Foo( a, b )`、`void Foo( int nCount )` |
| 方法呼叫 / 宣告無參數 | `Foo()` |
| 運算式括號 | `a * ( b - c )` |
| 索引 | `list[ 0 ]`、`string[] args` |
| 型別轉換 | `(int)dValue` |
| 繼承冒號 | `class Foo : IBar` |
| 二元 / 三元運算子 | `a + b`、`a ? b : c` |

範例：

```csharp
public class PathSorter : ISorter
{
	const int MAX_RETRY_COUNT = 3;

	readonly List<CAMData> m_CAMDataList = new List<CAMData>();
	bool m_isSorted;

	public bool IsEmpty => m_CAMDataList.Count == 0;

	public bool Sort( int nStartIndex )
	{
		// index out of range
		if( nStartIndex < 0 || nStartIndex >= m_CAMDataList.Count ) {
			return false;
		}

		for( int i = nStartIndex; i < m_CAMDataList.Count; i++ ) {
			double dLength = m_CAMDataList[ i ].GetLength();
			if( dLength > 0 ) {
				Process( m_CAMDataList[ i ] );
			}
			else {
				Skip( m_CAMDataList[ i ] );
			}
		}
		m_isSorted = true;
		return true;
	}
}
```

## 命名

| 對象 | 規則 | 範例 |
| --- | --- | --- |
| 類別、結構、列舉、委派 | PascalCase | `PathSorter`、`MachineType` |
| 介面 | `I` + PascalCase | `ISorter` |
| 方法、屬性、事件（任何存取層級） | PascalCase | `GetLength()`、`IsEmpty`、`DataChanged` |
| 列舉值 | PascalCase | `MachineType.Spindle` |
| 常數（`const`、`static readonly` 常數值） | 全大寫 + 底線 | `MAX_RETRY_COUNT` |
| 欄位（非 public） | `m_` + 名稱 | `m_Viewer`、`m_nCount` |
| WinForms 控制項欄位 | `m_` + 控制項縮寫 + 名稱 | `m_btnStart`、`m_cmbType`、`m_lblTimeLog`、`m_txtName` |
| 區域變數、參數 | camelCase | `nIndex`、`szFileName`、`shape` |

基本型別的變數（區域變數、參數、欄位）加型別前綴；物件型別不加：

| 型別 | 前綴 | 範例 |
| --- | --- | --- |
| `bool` | `b` 或 `is`（擇一，不可 `bIs`） | `bVisible`、`isChecked` |
| `int`、`long`、`short`、`uint` | `n` | `nCount`、`nSize` |
| `double`、`float` | `d` | `dLength` |
| `string` | `sz` | `szFileName` |
| 其他物件、`enum`、自訂型別 | 無前綴 | `shape`、`machineType`、`m_Viewer` |

其他命名規則：

- 集合名稱加容器後綴：`List<T>` → `xxxList`，`Dictionary<K,V>` → `xxxMap`，陣列 → `xxxArray`。
- 0-based 的整數用 `Index`（`nIndex`、`nPathIndex`）；1-based 用 `ID`（`nAxisID`）。`i`、`j`、`k`、`nCount` 一律 0-based。
- 回傳 `bool` 的方法 / 屬性以 `Is`、`Has`、`Can` 開頭（`IsValid()`）；以 `is` 開頭的變數必須是 `bool`。
- 使用正面語意命名（`bReady`，不用 `bNotReady`）。
- 成員名稱不重複類別名稱（`Storage.Flags`，不用 `Storage.StorageFlags`）。
- 存取成員不加 `this.`（直接寫 `m_btnStart`）。

## 文件註解

`public` 方法 / 屬性使用 XML 文件註解：

```csharp
/// <summary>
/// Sort paths starting from the given index.
/// </summary>
/// <param name="nStartIndex">0-based index of the first path to sort.</param>
/// <returns>False if the index is out of range.</returns>
public bool Sort( int nStartIndex )
```

## 程式結構

- **先做錯誤檢查再做主要工作**：不合法的情況用單一 `if` 提早 `return`，避免巢狀 `if`。
- 判斷條件內不做運算；先算好存入有意義名稱的變數，`if` 內只做簡單比較。

```csharp
// avoid
if( nIndex / 2 + 1 > 0 ) { ... }

// prefer
int nItemCount = nIndex / 2 + 1;
if( nItemCount > 0 ) { ... }
```

- 非絕對互斥的分支，用 `else if` 明確列出每種情況，`else` 只處理「不支援 / 未知」。
- `switch` 必須有 `default`，並明確處理未知值（回傳錯誤、記錄或拋出例外）。
- 方法保持短小，過長時拆成私有子方法。
- 變數宣告在最小作用範圍，於第一次使用時宣告並初始化。
- 布林判斷寫 `if( isOpen )`，不寫 `== true`；否定可寫 `!isOpen` 或 `isOpen == false`。
- 路徑組合使用 `Path.Combine`；路徑字面值使用 `@"C:\Folder"`。

## 語言用法

- 使用 `int`、`string`、`bool` 等關鍵字，不用 `Int32`、`String`。
- `var` 只在右側型別一眼可見時使用（`var list = new List<int>();`），其餘寫明型別。
- 所有成員明確寫出存取修飾詞（`private`、`public` …）。
- 只在初始化時賦值的欄位加 `readonly`。
- Expression-bodied（`=>`）只用於單行屬性 / 存取子 / lambda，方法與建構子使用區塊。
- `using` 放在 `namespace` 外，`System.*` 排最前面。
- 語言版本依 [project/coding-architecture.md](../project/coding-architecture.md) 的技術棧，不使用超出該版本的語法（如 C# 7.3 不使用 C# 8 以後的 nullable reference types、`switch` 運算式、`using` 宣告、範圍運算子 `..`、檔案範圍 namespace 等）。
