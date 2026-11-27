@echo off
REM UI style checks.
REM   UICheck.bat            static check
REM   UICheck.bat runtime    also opens every dialog / CNC panel in zh-TW and zh-CN, checks the numeric input, and
REM                          the main window landscape / portrait with dialog placement (build Debug first)
REM   UICheck.bat runtime ^<screenshot folder^>   ... and saves screenshots
setlocal
set "HERE=%~dp0"
set "RESULT=0"

REM runtime on a machine not marked as unattended (1) or work computer (0): no check runs, CheckRuntime.ps1 explains
if /i not "%~1"=="runtime" goto :static
if "%MYCAM_UICHECK_UNATTENDED%"=="0" goto :static
if "%MYCAM_UICHECK_UNATTENDED%"=="1" goto :static
powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntime.ps1"
set "RESULT=2"
goto :report

:static
powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckStatic.ps1"
if errorlevel 1 set "RESULT=1"

if /i not "%~1"=="runtime" goto :report
REM the runtime checks (CheckRuntime.ps1); a screenshot folder as a full path, the layout runner works in the build
REM output folder
if "%~2"=="" (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntime.ps1"
) else (
	powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntime.ps1" -ShotDir "%~f2"
)
REM 2: a runtime check could not run (machine not marked, no build, no specified screen), the rest did not run
if errorlevel 2 (
	set "RESULT=2"
) else if errorlevel 1 (
	set "RESULT=1"
)

:report
if "%RESULT%"=="0" (
	echo [UICheck] all checks passed.
) else if "%RESULT%"=="2" (
	echo [UICheck] the runtime checks did not complete, see the reason above.
) else (
	echo [UICheck] some checks reported problems, see above.
)
exit /b %RESULT%
