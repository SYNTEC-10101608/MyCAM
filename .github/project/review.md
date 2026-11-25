# 專案固定檢查項目

執行或回應 review 時，與 [team/review.md](../team/review.md) 一起讀；以下項目在 review 中必查。

1. 門檻：`Build.bat Debug` 建置成功。
2. 規範：使用者可見文字的四語系 XML 同步新增 / 修改（語系見專案概觀的「多語系」）。
3. 規範：修改過的 `*.Designer.cs` 仍可在 Designer 中開啟。
4. 規範：未修改 `reference/` 內的檔案，除非議題明確要求。
5. 功能正確性：UI 執行緒、角度單位（rad / deg）、座標系。
