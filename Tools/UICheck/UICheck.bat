@echo off
REM UI style checks.
REM   UICheck.bat            static check
REM   UICheck.bat runtime    also opens every dialog / CNC panel in zh-TW and zh-CN, checks the numeric input, and
REM                          the main window landscape / portrait with dialog placement (build Debug first)
REM   UICheck.bat runtime ^<screenshot folder^>   ... and saves screenshots
setlocal
set "HERE=%~dp0"
set "RESULT=0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckStatic.ps1"
if errorlevel 1 set "RESULT=1"

if /i not "%~1"=="runtime" goto :report
REM the runtime checks, one run at a time on this computer (CheckRuntime.ps1); a screenshot folder as a full path,
REM the layout runner works in the build output folder
if "%~2"=="" (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntime.ps1"
) else (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntime.ps1" -ShotDir "%~f2"
)
REM 2: no build or the screen could not be prepared, so the runtime checks did not run
if errorlevel 2 (
	set "RESULT=2"
) else if errorlevel 1 (
	set "RESULT=1"
)

:report
if "%RESULT%"=="0" (
	echo [UICheck] all checks passed.
) else if "%RESULT%"=="2" (
	echo [UICheck] the runtime checks could not run ^(no build or no specified screen^), see above.
) else (
	echo [UICheck] some checks reported problems, see above.
)
exit /b %RESULT%
