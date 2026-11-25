---
applyTo: "**/*.{cs,xml}"
---

# 專案程式規範：通用

新增或修改程式碼時讀本文件，與團隊的 Coding Style 一起遵守。本專案寫程式時必須遵守、但不屬於 Coding Style 的規則，放在 `project-coding-<主題>.instructions.md`。

1. 新增或修改使用者可見的文字時，四個語系的 XML 都要同步新增 / 修改（語系與檔案位置見專案概觀的「多語系」）。
2. 修改 `*.Designer.cs` 後，必須仍能在 Visual Studio Designer 中正常開啟。
3. 不修改、不升級 `reference/` 內的檔案，除非議題明確要求。
