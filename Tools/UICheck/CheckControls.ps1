param(
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
	[string]$Configuration = "Debug"
)
# Behavior check of the shared numeric input DebouncedNumericUpDown; the rules are listed in ControlRunner.cs.
# Build first (Build.bat Debug). Exit code 1 when a problem is found.

$ErrorActionPreference = "Stop"
$binDir = Join-Path $Root "MyCAM\bin\$Configuration"
if (-not (Test-Path (Join-Path $binDir "CommonUI.dll"))) {
	Write-Host "[UICheck] CommonUI.dll not found in $binDir, build first."
	exit 2
}

# compile the runner into the output folder, so it loads the CommonUI.dll of the build
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" |
	Select-Object -First 1
$csc = Join-Path (Split-Path $msbuild) "Roslyn\csc.exe"
. (Join-Path $PSScriptRoot "RunnerFiles.ps1")
Remove-StaleRunners $binDir
$runner = Get-RunnerPath $binDir "UICheckControls"
$references = @( "System.dll", "System.Core.dll", "System.Drawing.dll", "System.Windows.Forms.dll",
	(Join-Path $binDir "CommonUI.dll") ) | ForEach-Object { "/r:$_" }
# the runner only lives in the build output while it runs, so it is never shipped with it; the cleanup also covers
# a failed compile
try {
	& $csc /nologo /target:exe /platform:anycpu /out:$runner @references (Join-Path $PSScriptRoot "ControlRunner.cs")
	if ($LASTEXITCODE -ne 0) {
		Write-Host "[UICheck] failed to compile ControlRunner.cs"
		exit 2
	}
	& $runner
	$result = $LASTEXITCODE
}
finally {
	Remove-Item $runner -ErrorAction SilentlyContinue
}
exit $result
