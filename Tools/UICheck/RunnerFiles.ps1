# The runners are compiled into the build output folder, so they load its assemblies and language files. Each run
# names them after its own process ID and removes only its own, so runs that overlap in the same folder (a runtime
# check and a quick check, or a run whose parent was killed) never overwrite or remove each other's runners.
# Dot-sourced by CheckRuntimeLayout.ps1, CaptureMainWindow.ps1 and CheckControls.ps1.

# the runner names of these scripts; only these are ever removed
$script:RUNNER_NAME_LIST = @( "UICheckRunner", "UICheckHidden", "UICheckDisplay", "UICheckMainWindow",
	"UICheckControls" )

function Get-RunnerPath( [string]$szBinDir, [string]$szName ) {
	return Join-Path $szBinDir "$szName.$PID.exe"
}

function Test-FileInUse( [string]$szPath ) {
	try {
		$stream = [System.IO.File]::Open($szPath, "Open", "ReadWrite", "None")
		$stream.Close()
		return $false
	}
	catch {
		return $true
	}
}

# runners left behind by a run that was killed: its script process is gone. A run is removed as a group and only
# when none of its runners is in use: a runner of a killed script can still be running, or waiting for the screen
# lock and about to start the others of its group. Runners of older versions without a process ID are removed when
# not in use.
function Remove-StaleRunners( [string]$szBinDir ) {
	$groupMap = @{}
	foreach ($file in Get-ChildItem $szBinDir -File -Filter "UICheck*.exe") {
		if ($file.Name -notmatch '^(UICheck\w+?)(?:\.(\d+))?\.exe$' -or $RUNNER_NAME_LIST -notcontains $Matches[ 1 ]) {
			continue
		}
		$szId = if ($Matches[ 2 ]) { $Matches[ 2 ] } else { "" }
		if (-not $groupMap.ContainsKey($szId)) {
			$groupMap[ $szId ] = New-Object System.Collections.Generic.List[string]
		}
		$groupMap[ $szId ].Add($file.FullName)
	}
	foreach ($szId in $groupMap.Keys) {
		if ($szId -and $null -ne (Get-Process -Id ([int]$szId) -ErrorAction SilentlyContinue)) {
			continue
		}
		$inUseList = @( $groupMap[ $szId ] | Where-Object { Test-FileInUse $_ } )
		if ($inUseList.Count -gt 0) {
			continue
		}
		$groupMap[ $szId ] | ForEach-Object { Remove-Item $_ -ErrorAction SilentlyContinue }
	}
}
