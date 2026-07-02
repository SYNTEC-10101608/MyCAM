@echo off
setlocal enabledelayedexpansion
:: 設定編碼為 UTF-8，避免路徑有中文時出現亂碼
chcp 65001 > nul

REM 設定來源與目標目錄
set "SRC=%~dp0reference"
set "DBG=%~dp0MyCAM\bin\Debug"
set "REL=%~dp0MyCAM\bin\Release"
set "SRC_LANG=%~dp0Language"
set "DBG_CUST=%DBG%\Custom"
set "REL_CUST=%REL%\Custom"

REM 若 Debug 與 Release 目錄不存在就建立
if not exist "%DBG%" (
    echo creating Debug dir...
    mkdir "%DBG%"
)
if not exist "%REL%" (
    echo creating Release dir...
    mkdir "%REL%"
)

REM 從 reference 複製所有 dll 到 Debug 與 Release
echo Copying reference files...
for /r "%SRC%" %%f in (*.dll *.exe) do (
    copy /y "%%f" "%DBG%\" >nul
    copy /y "%%f" "%REL%\" >nul
)

echo ---------------------------------------------------
echo Copying language files...
echo ---------------------------------------------------

REM 1. 同步 Language 資料夾 (至 Resource\Language)
if exist "%SRC_LANG%" (
    echo 同步 Language 語系檔...
    robocopy "%SRC_LANG%" "%DBG%\Resource\Language" /E /XO /R:1 /W:1 /NDL /NFL > nul
    robocopy "%SRC_LANG%" "%REL%\Resource\Language" /E /XO /R:1 /W:1 /NDL /NFL > nul
) else (
    echo can't find Language files
)

REM 2. 在 Custom 底下生成空的 Language 資料夾
echo Checking and generating an empty Language folder under Custom folder....
if not exist "%DBG_CUST%\Language" mkdir "%DBG_CUST%\Language"
if not exist "%REL_CUST%\Language" mkdir "%REL_CUST%\Language"

echo all done!

REM 顯示 1 秒後自動關閉視窗
timeout /t 1 > nul