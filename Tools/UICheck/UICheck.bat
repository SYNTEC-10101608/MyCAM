@echo off
REM UI checks of .github/instructions/ui-style.instructions.md, section 8.
REM   UICheck.bat            static check + behavior comparison with master
REM   UICheck.bat runtime    also opens every dialog / CNC panel in zh-TW and zh-CN, and the main window
REM                          landscape / portrait with dialog placement (build Debug first)
REM   UICheck.bat runtime ^<screenshot folder^>   ... and saves screenshots
setlocal
set "HERE=%~dp0"
set "RESULT=0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckStatic.ps1"
if errorlevel 1 set "RESULT=1"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CompareBehavior.ps1"
if errorlevel 1 set "RESULT=1"

if /i not "%~1"=="runtime" goto :report
if "%~2"=="" (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntimeLayout.ps1"
) else (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntimeLayout.ps1" -ShotDir "%~2"
)
if errorlevel 1 set "RESULT=1"

REM main window screenshots go to the temp folder when no screenshot folder is given
set "SHOTS=%~2"
if "%SHOTS%"=="" set "SHOTS=%TEMP%\UICheck"
for %%L in (zh-TW zh-CN) do (
	echo ## main window: %%L
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CaptureMainWindow.ps1" -Language %%L -Width 1920 -Height 1040 -OutFile "%SHOTS%\MainWindow_%%L_Landscape.png"
	if errorlevel 1 set "RESULT=1"
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CaptureMainWindow.ps1" -Language %%L -Width 1080 -Height 1880 -OutFile "%SHOTS%\MainWindow_%%L_Portrait.png"
	if errorlevel 1 set "RESULT=1"
)

:report
if "%RESULT%"=="0" (
	echo [UICheck] all checks passed.
) else (
	echo [UICheck] some checks reported problems, see above.
)
exit /b %RESULT%
