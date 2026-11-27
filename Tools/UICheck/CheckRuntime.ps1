param(
	[string]$ShotDir = ""
)
# The runtime part of UICheck.bat runtime: dialogs and CNC panels in zh-TW and zh-CN, the numeric input, and the
# main window landscape / portrait with dialog placement (build Debug first).
# The machine must be marked first: MYCAM_UICHECK_UNATTENDED=1 an unattended machine, 0 a work computer
# (.github/project/tool-uicheck.md); it decides where the main window is captured and how a missing specified screen
# is handled.
# Overlapping runs do not disturb each other: each command takes the screen lock in VirtualDisplayRun.cs, and each
# script names its runners after its own process (RunnerFiles.ps1).
# Exit code 1 when a check reports problems, 2 when a check cannot run (machine not marked, no build, or the
# specified screen cannot be prepared): the run stops at the first 2, because no later check can run either.

$ErrorActionPreference = "Stop"

$unattended = [Environment]::GetEnvironmentVariable("MYCAM_UICHECK_UNATTENDED")
if ($unattended -ne "0" -and $unattended -ne "1") {
	Write-Host "[UICheck] the runtime checks did not run: MYCAM_UICHECK_UNATTENDED is not set to 0 or 1 on this machine."
	Write-Host "[UICheck]   1 = unattended machine (nobody uses its screen; the main window is captured on the visible"
	Write-Host "[UICheck]       desktop; with no 1920x1080 100 % screen it needs a resident virtual screen)"
	Write-Host "[UICheck]   0 = work computer (captured on a hidden desktop; with no such screen a Parsec virtual"
	Write-Host "[UICheck]       screen is added during the check)"
	Write-Host "[UICheck] set it once, then open a new terminal: setx MYCAM_UICHECK_UNATTENDED 1   (or 0)"
	Write-Host "[UICheck] details: .github/project/tool-uicheck.md, section 2"
	exit 2
}

# runs one check: 0 passed, 1 problems, 2 the check could not run
function Invoke-Check([string]$Script, [string[]]$Arguments) {
	& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot $Script) @Arguments | Out-Host
	if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 2) {
		return $LASTEXITCODE
	}
	return 1
}

$checkList = New-Object System.Collections.Generic.List[object]
$layoutArguments = @()
if ($ShotDir) {
	$layoutArguments = @( "-ShotDir", $ShotDir )
}
$checkList.Add(@( $null, "CheckRuntimeLayout.ps1", $layoutArguments ))

# behavior of the shared numeric input
$checkList.Add(@( $null, "CheckControls.ps1", @() ))

# main window screenshots go to the temp folder when no screenshot folder is given
$shots = $ShotDir
if (-not $shots) {
	$shots = Join-Path $env:TEMP "UICheck"
}
# each language x page (CAD start page, CAM, CNC machining) x landscape (maximized) / portrait (1080 x 1880 window),
# on the specified screen 1920 x 1080 at 100 % (see VirtualDisplayRun.cs)
foreach ($language in @( "zh-TW", "zh-CN" )) {
	foreach ($mode in @( "CAD", "CAM", "CNC" )) {
		foreach ($screen in @( "Landscape", "Portrait" )) {
			$title = if ($screen -eq "Landscape") { "## main window: $language $mode" } else { $null }
			$outFile = Join-Path $shots "MainWindow_${language}_${mode}_$screen.png"
			$checkList.Add(@( $title, "CaptureMainWindow.ps1",
				@( "-Language", $language, "-Mode", $mode, "-Screen", $screen, "-OutFile", $outFile ) ))
		}
	}
}

# MainDesignerRegion: the regions at the client size and page of the designer are where the designer shows them
$checkList.Add(@( "## main window: designer layout", "CaptureMainWindow.ps1",
	@( "-Mode", "CNC", "-Screen", "Landscape", "-DesignerLayout", "-OutFile",
	(Join-Path $shots "MainWindow_Designer.png") ) ))

$result = 0
foreach ($check in $checkList) {
	if ($check[ 0 ]) {
		Write-Host $check[ 0 ]
	}
	$code = Invoke-Check $check[ 1 ] $check[ 2 ]
	if ($code -eq 2) {
		exit 2
	}
	if ($code -ne 0) {
		$result = 1
	}
}
exit $result
