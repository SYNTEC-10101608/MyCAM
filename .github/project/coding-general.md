# 專案程式規範：通用

新增或修改程式碼時讀本文件，與團隊的 Coding Style 一起遵守。

## 新增使用者操作

1. 新功能通常是「Action + Dialog」：在 `Editor/Action/CAD` 或 `CAM` 新增 Action 類別，必要時在 `Editor/Dialog` 新增對應的 `*Dlg`。
2. 參考同目錄既有 Action 的寫法（啟動 / 結束流程、選取處理、與 Dialog 的互動），保持一致。

## 多語系

1. 字串檔位於 `Language/<類別名稱>_<語系>.xml`，每個語系一個檔案（目前有 `zh-TW`、`zh-CN`、`en-US`、`ru-RU`）；zh-TW、zh-CN 為主要維護的語系。
2. 程式中取字串時，以自己的類別名稱建立 `new LanguageManager.LanguageManager( "AddPathDlg" )`，再以 `TryGetString( "Id", DEFAULT_TEXT )` 取得，`DEFAULT_TEXT` 為類別內的 `const string`。
3. 新增或修改使用者可見的文字時，`Language/` 中所有語系的 XML 都要同步新增 / 修改。
4. 畫面（對話框、面板、主視窗）的控制項文字與 ID 規則見 [coding-ui-style.md](coding-ui-style.md) 的「多語系」。

## 日誌與訊息

1. 顯示給使用者的訊息使用 `MyApp.Logger.ShowOnLogPanel( szMessage, MyApp.NoticeType.Warning )`（`Hint` / `Warning` / `Error`）。

## 其他規則

1. 修改 `*.Designer.cs` 後，必須仍能在 Visual Studio Designer 中正常開啟；撰寫、修改與 review 的方式見 [coding-ui-style.md](coding-ui-style.md) 的「Designer 檔」。
2. 不修改、不升級 `reference/` 內的檔案，除非議題明確要求。
