@echo off
setlocal enabledelayedexpansion

REM 設定來源與目標目錄
set "SRC=%~dp0reference"
set "DBG=%~dp0MyCAM\bin\Debug"
set "REL=%~dp0MyCAM\bin\Release"

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
for /r "%SRC%" %%f in (*.dll) do (
    copy /y "%%f" "%DBG%\" >nul
    copy /y "%%f" "%REL%\" >nul
)

echo all done!