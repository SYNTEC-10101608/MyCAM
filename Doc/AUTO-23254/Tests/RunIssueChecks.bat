@echo off
REM Issue-only checks of the UI style unification (AUTO-23254):
REM   behavior comparison with the commit the branch started from, the coding style of what the issue wrote, and the
REM   main window regions against the TableLayoutPanel layout (needs the Debug build).
REM The reusable UI checks are Tools\UICheck\UICheck.bat.
setlocal
set "HERE=%~dp0"
set "RESULT=0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CompareBehavior.ps1"
if errorlevel 1 set "RESULT=1"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckBranchRules.ps1"
if errorlevel 1 set "RESULT=1"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CompareMainLayout.ps1"
if errorlevel 1 set "RESULT=1"

if "%RESULT%"=="0" (
	echo [IssueCheck] all checks passed.
) else (
	echo [IssueCheck] some checks reported problems, see above.
)
exit /b %RESULT%
