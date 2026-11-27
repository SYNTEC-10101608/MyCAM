# The runners are compiled into the build output folder, so they load its assemblies and language files. Each run
# names them after its own process ID and removes only its own, so runs that overlap in the same folder (a runtime
# check and a quick check, or a run whose parent was killed) never overwrite or remove each other's runners.
# Dot-sourced by CheckRuntimeLayout.ps1, CaptureMainWindow.ps1 and CheckControls.ps1.

function Get-RunnerPath( [string]$szBinDir, [string]$szName ) {
	return Join-Path $szBinDir "$szName.$PID.exe"
}

# runners left behind by a run that was killed: its process is gone; a runner still running cannot be removed and
# stays until a later run
function Remove-StaleRunners( [string]$szBinDir ) {
	foreach ($file in Get-ChildItem $szBinDir -Filter "UICheck*.exe") {
		$isStale = $true
		if ($file.Name -match '^UICheck\w+\.(\d+)\.exe$') {
			$isStale = $null -eq (Get-Process -Id ([int]$Matches[ 1 ]) -ErrorAction SilentlyContinue)
		}
		if ($isStale) {
			Remove-Item $file.FullName -ErrorAction SilentlyContinue
		}
	}
}
