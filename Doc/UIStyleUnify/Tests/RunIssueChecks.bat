@echo off
REM Issue-only checks of the UI style unification (Doc/UIStyleUnify/5_Verification.md):
REM   behavior comparison with the commit the branch started from, and the coding style of what the issue wrote.
REM The reusable UI checks are Tools\UICheck\UICheck.bat.
setlocal
set "HERE=%~dp0"
set "RESULT=0"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CompareBehavior.ps1"
if errorlevel 1 set "RESULT=1"

powershell -NoProfile -ExecutionPolicy Bypass -File "%HERE%CheckBranchRules.ps1"
if errorlevel 1 set "RESULT=1"

if "%RESULT%"=="0" (
	echo [IssueCheck] all checks passed.
) else (
	echo [IssueCheck] some checks reported problems, see above.
)
exit /b %RESULT%
