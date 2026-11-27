param(
	[string]$ShotDir = ""
)
# The runtime part of UICheck.bat runtime: dialogs and CNC panels in zh-TW and zh-CN, the numeric input, and the
# main window landscape / portrait with dialog placement (build Debug first).
# One runtime at a time on this computer: the lock VirtualDisplayRun.cs takes for each command is held here for the
# whole run, so two runs do not interleave on the screen or compile and remove the runners of the same build output
# under each other; the commands started here are told with UICHECK_LOCK_HELD=1.
# Exit code 1 when a check reports problems, 2 when a check cannot run (no build, or the specified screen cannot be
# prepared): the run stops at the first 2, because no later check can run either.

$ErrorActionPreference = "Stop"
$lockName = "Global\MyCAM_UICheck_Screen"
$lockTimeoutMs = 30 * 60 * 1000

# runs one check: 0 passed, 1 problems, 2 the screen could not be prepared
function Invoke-Check([string]$Script, [string[]]$Arguments) {
	& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot $Script) @Arguments | Out-Host
	if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 2) {
		return $LASTEXITCODE
	}
	return 1
}

# every check in order; stops at the first 2
function Invoke-AllChecks {
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
	# each language x page (CAD start page, CAM, CNC machining) x landscape (maximized) / portrait (1080 x 1880
	# window), on the specified screen 1920 x 1080 at 100 % (see VirtualDisplayRun.cs)
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
			return 2
		}
		if ($code -ne 0) {
			$result = 1
		}
	}
	return $result
}

$runLock = New-Object System.Threading.Mutex($false, $lockName)
$isLocked = $false
try {
	$isLocked = $runLock.WaitOne(0)
	if (-not $isLocked) {
		Write-Host "[UICheck] waiting for another UICheck run on this computer to end"
		$isLocked = $runLock.WaitOne($lockTimeoutMs)
	}
}
# a run that was killed leaves the lock abandoned; it is taken over
catch [System.Threading.AbandonedMutexException] {
	$isLocked = $true
}
if (-not $isLocked) {
	Write-Host "[UICheck] another UICheck run did not end within $($lockTimeoutMs / 60000) minutes"
	$runLock.Dispose()
	exit 2
}
try {
	$env:UICHECK_LOCK_HELD = "1"
	$exitCode = Invoke-AllChecks
}
finally {
	$runLock.ReleaseMutex()
	$runLock.Dispose()
}
exit $exitCode
