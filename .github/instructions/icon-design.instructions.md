---
applyTo: "MyCAM/Resource/IconSource/**"
---

# 圖示設計

適用於主視窗選單與工具列的圖示（`MyCAM/Resource/IconSource`）。圖示在畫面上的尺寸與使用方式見
[UI 風格](ui-style.instructions.md) 第 6 節。本規範由議題 AUTO-23254（UI 風格統一）制定。

## 1. 產生方式

1. 圖示一律由腳本產生，不手繪、不另找圖檔：
   1. `IconDefinitions.ps1`：圖示的向量定義（`$IconMap`，依族群以 `#region` 分區）。新增或修改圖示時在這裡擴充。
   2. `RenderIcons.ps1 [-Only <名稱>]`：產生 40×40 透明背景 PNG 到 `MyCAM/Resource/`。
2. 新的 PNG 加入 `MyCAM/Properties/Resources.resx`，在 Designer 以 `Image` 指定。
3. 產生後在 40×40 與 20×20（選單、圖層列、視角列由 `ImageScalingSize` 縮小）下目視確認。

## 2. 格線與線條

1. 在 24×24 的格線上繪製，主體留約 2 的邊距；圖形為 SVG path 語法。
2. 線條圓頭、圓角（腳本固定）。
3. 線寬：

   | 對象 | 線寬 |
   | --- | --- |
   | 物件輪廓 | 1.4（細部 1.2） |
   | 動作或結果 | 1.6～1.8 |
   | 參考線、隱藏邊 | 1.2，虛線 |

4. 筆畫要在 20×20 下仍可辨識；只在小尺寸顯示、圖形偏小的圖示，以開頭的 `zoom|<倍率>` 放大。

## 3. 色彩

色票是 `RenderIcons.ps1` 的 `$Palette`，屬於圖示的繪圖色，不是 UI token。圖示只使用下列代號：

| 代號 | 用途 |
| --- | --- |
| `k` | 物件與輪廓 |
| `a` | 動作或結果（等於 `ThemeColors.ACCENT`） |
| `g` | 參考線、隱藏邊 |
| `r` | 刪除、停止（等於 `ThemeColors.ERROR`） |
| `y` | 碰撞等警示 |
| `t` / `f` / `w` | 填色：結果或重點面 / 暗面 / 亮面；`w` 也用於徽章上的記號 |
| `h` / `H` | 視角立方體的高亮面 |

1. 修改 `ACCENT`、`ERROR` 時，一併修改 `a`、`r` 並重新產生圖示。
2. 立體物件以等角視角繪製，以 `t` / `f` / `w` 區分三個面。

## 4. 共用元素

1. 新增類的動作在右下角加 `$BadgePlus`，移除類加 `$BadgeMinus`。
2. 路徑使用 `$CutPath`，碰撞使用 `Burst`。
3. 同一族群（`#region`）的圖示沿用相同的基本形，只以 `a` 色的部分表示各自的動作。
4. 文字只在沒有更好的圖形時使用，且只用英數字（例：`G`、`NC`），字型由腳本固定。

## 5. 以 AI 產生圖示

提示中指明本規範、要放的族群、功能說明與參考的既有圖示，例：

```text
依 .github/instructions/icon-design.instructions.md，在 IconDefinitions.ps1 的 <族群> 區新增 <名稱>，
表示「<功能說明>」，參考同族群的 <相似圖示>。以 RenderIcons.ps1 -Only <名稱> 產生，
並提供 40×40 與 20×20 的預覽。
```
