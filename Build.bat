@echo off
setlocal
chcp 65001 > nul

REM 用法：Build.bat [Debug^|Release] [cs^|all]
REM   組態預設 Debug。
REM   cs （預設）：只建置 C# 專案（MyCAM.csproj 及其參考專案），不建置 C++ 的 PostTool / FCLTool。
REM   all        ：建置整個 MyCAM.sln，包含 C++ 專案；FCLTool 編譯非常耗時。

set "CONFIG=%~1"
if "%CONFIG%"=="" set "CONFIG=Debug"
set "SCOPE=%~2"
if "%SCOPE%"=="" set "SCOPE=cs"

if /i "%SCOPE%"=="cs" (
    set "TARGET=%~dp0MyCAM\MyCAM.csproj"
    set "PLATFORM=AnyCPU"
) else if /i "%SCOPE%"=="all" (
    set "TARGET=%~dp0MyCAM.sln"
    set "PLATFORM=Any CPU"
) else (
    echo [Build] 未知的範圍 "%SCOPE%"，請使用 cs 或 all。
    exit /b 1
)

REM 以 vswhere 找出最新版 Visual Studio 的 MSBuild
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo [Build] 找不到 vswhere.exe，請確認已安裝 Visual Studio。
    exit /b 1
)
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    set "MSBUILD=%%i"
    goto :found
)
echo [Build] 找不到 MSBuild.exe。
exit /b 1

:found
echo [Build] %CONFIG% ^| %SCOPE%
"%MSBUILD%" "%TARGET%" /t:Build /p:Configuration=%CONFIG% "/p:Platform=%PLATFORM%" /m /nologo /v:minimal /clp:ErrorsOnly;Summary
exit /b %ERRORLEVEL%
