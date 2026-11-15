@echo off
REM UI checks of .github/instructions/ui-style.instructions.md, section 8.
REM   UICheck.bat            static check + behavior comparison with master
REM   UICheck.bat runtime    also opens every dialog / CNC panel in zh-TW and zh-CN (build Debug first)
REM   UICheck.bat runtime ^<screenshot folder^>   ... and saves screenshots
setlocal
set "HERE=%~dp0"
set "RESULT=0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckStatic.ps1"
if errorlevel 1 set "RESULT=1"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CompareBehavior.ps1"
if errorlevel 1 set "RESULT=1"

if /i "%~1"=="runtime" (
	if "%~2"=="" (
		powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntimeLayout.ps1"
	) else (
		powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckRuntimeLayout.ps1" -ShotDir "%~2"
	)
	if errorlevel 1 set "RESULT=1"
)

if "%RESULT%"=="0" (
	echo [UICheck] all checks passed.
) else (
	echo [UICheck] some checks reported problems, see above.
)
exit /b %RESULT%
