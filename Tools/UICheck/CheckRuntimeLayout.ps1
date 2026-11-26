param(
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
	[string]$Configuration = "Debug",
	[string[]]$Languages = @( "zh-TW", "zh-CN" ),
	[string]$ShotDir = "",
	[string]$Only = "",
	[switch]$CurrentScreen,
	[switch]$Visible
)
# Runtime layout check. Opens every dialog and CNC panel of the built MyCAM in each language and reports the rules
# listed in LayoutRunner.cs.
# Build first (Build.bat Debug). Exit code 1 when a problem is found.
# The runner works on a hidden desktop (HiddenDesktopRun.cs) so it does not take the screen; -Visible shows it.
# It runs on the specified screen of the product (VirtualDisplayRun.cs, see CaptureMainWindow.ps1), because a form is
# clamped to the screen it opens on; -CurrentScreen runs on the current screen instead.

$ErrorActionPreference = "Stop"
$binDir = Join-Path $Root "MyCAM\bin\$Configuration"
if (-not (Test-Path (Join-Path $binDir "MyCAM.exe"))) {
	Write-Host "[UICheck] MyCAM.exe not found in $binDir, build first."
	exit 2
}
. (Join-Path $PSScriptRoot "SyncLanguage.ps1")
Sync-LanguageFiles $Root $binDir

# compile the runner into the output folder, so it finds language files and assemblies like MyCAM.exe
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" |
	Select-Object -First 1
$csc = Join-Path (Split-Path $msbuild) "Roslyn\csc.exe"
$runner = Join-Path $binDir "UICheckRunner.exe"
$references = @( "System.dll", "System.Core.dll", "System.Drawing.dll", "System.Windows.Forms.dll", "System.Xml.dll",
	(Join-Path $binDir "CommonUI.dll"), (Join-Path $binDir "LanguageManager.dll") ) | ForEach-Object { "/r:$_" }
$hidden = Join-Path $binDir "UICheckHidden.exe"
$display = Join-Path $binDir "UICheckDisplay.exe"
# the runners only live in the build output while they run, so they are never shipped with it; the cleanup also
# covers a failed compile or preparation
try {
	& $csc /nologo /target:exe /platform:anycpu /out:$runner @references (Join-Path $PSScriptRoot "LayoutRunner.cs")
	if ($LASTEXITCODE -ne 0) {
		Write-Host "[UICheck] failed to compile LayoutRunner.cs"
		exit 2
	}
	& $csc /nologo /target:exe /platform:anycpu /out:$hidden (Join-Path $PSScriptRoot "HiddenDesktopRun.cs")
	if ($LASTEXITCODE -ne 0) {
		Write-Host "[UICheck] failed to compile HiddenDesktopRun.cs"
		exit 2
	}
	if (-not $CurrentScreen) {
		& $csc /nologo /target:exe /platform:anycpu /out:$display (Join-Path $PSScriptRoot "VirtualDisplayRun.cs")
		if ($LASTEXITCODE -ne 0) {
			Write-Host "[UICheck] failed to compile VirtualDisplayRun.cs"
			exit 2
		}
	}

	# the runner works in the output folder, so a relative screenshot folder is resolved here first
	if (-not $ShotDir) {
		$ShotDir = "-"
	}
	elseif (-not [System.IO.Path]::IsPathRooted($ShotDir)) {
		$ShotDir = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ShotDir))
	}
	$result = 0
	foreach ($szLanguage in $Languages) {
		Write-Host "## runtime layout: $szLanguage"
		$arguments = @( $szLanguage, $ShotDir )
		if ($Only) {
			$arguments += $Only
		}
		Push-Location $binDir
		try {
			$command = @( $runner ) + $arguments
			if (-not $Visible) {
				$command = @( $hidden ) + $command
			}
			if (-not $CurrentScreen) {
				$command = @( $display ) + $command
			}
			$commandArguments = @( $command | Select-Object -Skip 1 )
			& $command[ 0 ] @commandArguments
			# the screen could not be prepared: no check ran
			if ($LASTEXITCODE -eq 2) {
				exit 2
			}
			if ($LASTEXITCODE -ne 0) {
				$result = 1
			}
		}
		finally {
			Pop-Location
		}
	}
}
finally {
	Remove-Item $runner, $hidden, $display -ErrorAction SilentlyContinue
}
exit $result
