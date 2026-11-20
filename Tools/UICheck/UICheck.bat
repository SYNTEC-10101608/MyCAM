@echo off
REM UI checks of .github/instructions/ui-style.instructions.md, section 8.
REM   UICheck.bat            static check
REM   UICheck.bat runtime    also opens every dialog / CNC panel in zh-TW and zh-CN, and the main window
REM                          landscape / portrait with dialog placement (build Debug first)
REM   UICheck.bat runtime ^<screenshot folder^>   ... and saves screenshots
setlocal
set "HERE=%~dp0"
set "RESULT=0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckStatic.ps1"
if errorlevel 1 set "RESULT=1"

if /i not "%~1"=="runtime" goto :report
if "%~2"=="" (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntimeLayout.ps1"
) else (
	REM full path, the layout runner works in the build output folder
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntimeLayout.ps1" -ShotDir "%~f2"
)
if errorlevel 1 set "RESULT=1"

REM main window screenshots go to the temp folder when no screenshot folder is given
set "SHOTS=%~f2"
if "%~2"=="" set "SHOTS=%TEMP%\UICheck"
REM each language x page (CAD start page, CAM, CNC machining) x landscape / portrait
for %%L in (zh-TW zh-CN) do (
	for %%M in (CAD CAM CNC) do (
		echo ## main window: %%L %%M
		powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CaptureMainWindow.ps1" -Language %%L -Mode %%M -Width 1920 -Height 1040 -OutFile "%SHOTS%\MainWindow_%%L_%%M_Landscape.png"
		if errorlevel 1 set "RESULT=1"
		powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CaptureMainWindow.ps1" -Language %%L -Mode %%M -Width 1080 -Height 1880 -OutFile "%SHOTS%\MainWindow_%%L_%%M_Portrait.png"
		if errorlevel 1 set "RESULT=1"
	)
)

REM P6: the regions at the client size and page of the designer are where the designer shows them
echo ## main window: designer layout
powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CaptureMainWindow.ps1" -Mode CNC -DesignerLayout -OutFile "%SHOTS%\MainWindow_Designer.png"
if errorlevel 1 set "RESULT=1"

:report
if "%RESULT%"=="0" (
	echo [UICheck] all checks passed.
) else (
	echo [UICheck] some checks reported problems, see above.
)
exit /b %RESULT%
