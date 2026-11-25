# 專案程式規範：通用

新增或修改程式碼時讀本文件，與團隊的 Coding Style 一起遵守。

## 新增使用者操作

1. 新功能通常是「Action + Dialog」：在 `Editor/Action/CAD` 或 `CAM` 新增 Action 類別，必要時在 `Editor/Dialog` 新增對應的 `*Dlg`。
2. 參考同目錄既有 Action 的寫法（啟動 / 結束流程、選取處理、與 Dialog 的互動），保持一致。

## 多語系

1. 每個 Form / Dialog / Action 以自己的類別名稱建立 `new LanguageManager.LanguageManager( "AddPathDlg" )`。
2. 字串檔位於 `Language/<類別名稱>_<語系>.xml`，支援 `zh-TW`、`zh-CN`、`en-US`、`ru-RU` 四種語系。
3. 控制項文字以控制項欄位名稱為 `ID`（`<Message ID="m_rdoManual" Content="..." />`）。
4. 程式中取字串使用 `m_LanguageManager.TryGetString( "Id", DEFAULT_TEXT )`，`DEFAULT_TEXT` 為類別內的 `const string`。
5. 新增或修改使用者可見的文字時，四個語系的 XML 都要同步新增 / 修改。

## 日誌與訊息

1. 顯示給使用者的訊息使用 `MyApp.Logger.ShowOnLogPanel( szMessage, MyApp.NoticeType.Warning )`（`Hint` / `Warning` / `Error`）。

## 其他規則

1. 修改 `*.Designer.cs` 後，必須仍能在 Visual Studio Designer 中正常開啟。
2. 不修改、不升級 `reference/` 內的檔案，除非議題明確要求。
