param(
	[string]$Root = "",
	[switch]$Update
)
# Issue-only check (Doc/UIStyleUnify/5_Verification.md): the main window regions are placed where the baseline
# says. MainWindowRunner prints each host panel ("[region]" lines) for every page and window size below; the result
# must equal MainLayoutBaseline.txt pixel for pixel.
# -Update writes the baseline. It was first written with the TableLayoutPanel layout (the MainLayout rewrite kept every
# region), and rewritten once when the region spacing was unified (5_Verification.md 5.2); only rewrite it for an
# intended layout change.
# Needs the Debug build. The language does not change the regions, so zh-TW is enough.

$ErrorActionPreference = "Stop"
if (-not $Root) {
	$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
}
$capture = Join-Path $Root "Tools\UICheck\CaptureMainWindow.ps1"
$baselinePath = Join-Path $PSScriptRoot "MainLayoutBaseline.txt"
$shotDir = Join-Path $env:TEMP "UICheckMainLayout"

# window sizes: the two of UICheck.bat and a smaller one for each orientation
$sizeList = @( @( 1920, 1040 ), @( 1366, 768 ), @( 1080, 1880 ), @( 900, 1600 ) )
$modeList = @( "CAD", "CAM", "CNC" )

$lineList = New-Object System.Collections.Generic.List[string]
foreach ($size in $sizeList) {
	foreach ($mode in $modeList) {
		$name = "$mode $($size[0])x$($size[1])"
		Write-Host "## $name"
		$outFile = Join-Path $shotDir ("MainLayout_{0}_{1}x{2}.png" -f $mode, $size[0], $size[1])
		$output = & $capture -Root $Root -OutFile $outFile -Width $size[0] -Height $size[1] -Mode $mode `
			-Dialogs @() -ModalDialogs @() 2>&1
		$regionList = @( $output | ForEach-Object { "$_" } | Where-Object { $_ -match "^\[region\] " } )
		if ($regionList.Count -eq 0) {
			Write-Host "[MainLayout] no region printed for $name"
			$output | ForEach-Object { Write-Host "  $_" }
			exit 2
		}
		$lineList.Add("== $name")
		foreach ($region in $regionList) {
			$lineList.Add($region.Substring("[region] ".Length))
		}
	}
}

if ($Update) {
	$utf8Bom = New-Object System.Text.UTF8Encoding($true)
	[System.IO.File]::WriteAllText($baselinePath, (($lineList -join "`r`n") + "`r`n"), $utf8Bom)
	Write-Host "[MainLayout] baseline written: $baselinePath ($($lineList.Count) lines)"
	exit 0
}

$baselineList = @( Get-Content $baselinePath -Encoding UTF8 | Where-Object { $_ -ne "" } )
$problemCount = 0
$count = [Math]::Max($baselineList.Count, $lineList.Count)
$scenario = ""
for ($i = 0; $i -lt $count; $i++) {
	$expected = if ($i -lt $baselineList.Count) { $baselineList[$i] } else { "(none)" }
	$actual = if ($i -lt $lineList.Count) { $lineList[$i] } else { "(none)" }
	if ($expected.StartsWith("== ")) {
		$scenario = $expected
	}
	if ($expected -ne $actual) {
		Write-Host "FAIL  $scenario  expected '$expected', actual '$actual'"
		$problemCount++
	}
}
Write-Host "[MainLayout] $($lineList.Count) lines compared, $problemCount differences"
if ($problemCount -eq 0) {
	exit 0
}
exit 1
