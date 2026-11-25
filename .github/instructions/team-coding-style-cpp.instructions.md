---
applyTo: "**/*.{c,cpp,h,hpp}"
---

# Coding Style：C / C++

適用於 C / C++ 原始碼。另須遵守 [通用規則](team-coding-style-general.instructions.md)。

## 格式

- 類別、方法的 `{` 換行；流程控制的 `{` 不換行。
- `else`、`catch` 前換行。
- 空白規則同 C#：`if( a > b ) {`、`Foo( a, b )`、`Foo()`、`list[ 0 ]`、`a * ( b - c )`。
- 型別轉換括號內留空白：`( int )dValue`。
- 指標宣告 `*` 靠變數：`Shape *pShape`。
- `public:` / `private:` 與 `class` 對齊；`case` 與 `switch` 對齊。

## 命名

- 類別成員 `m_` 前綴；指標 `p` 前綴；全域變數 `g_` 前綴（避免使用）。
- 基本型別前綴同 C#：`b` / `is`（bool）、`n`（整數）、`d`（浮點數）、`sz`（字串）。
- 巨集全大寫 + 底線，值以括號包起：`#define MAX_COUNT ( 99 )`。

## 語言用法

- 新程式碼使用標準型別（`int32_t`、`bool`）與 RAII / 智慧指標，不新增手動 `new` / `delete`。
- header 必須能獨立編譯（自行 include 所需的 header，並使用 `#pragma once`）。
