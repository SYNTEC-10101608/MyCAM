param(
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
	[string]$Only = ""
)
# Renumbers TabIndex in the designer files so the tab order follows the layout (ui-style.instructions.md, S10 of
# CheckStatic.ps1): in each container, rows from top to bottom and left to right within a row, numbered from 0.
# Only the TabIndex lines change; open the dialog in the Visual Studio designer afterwards if in doubt.

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DesignerParser.ps1")

$SCREEN_DIRS = @( "MyCAM\Editor\Dialog", "MyCAM\CNC", "CncUI\Panel", "CncUI\Dialog" )
$designerFiles = foreach ($dir in $SCREEN_DIRS) {
	Get-ChildItem (Join-Path $Root $dir) -Filter "*.Designer.cs" -Recurse
}
$nFileCount = 0
foreach ($file in $designerFiles) {
	$screen = Read-DesignerFile $file.FullName
	if ($Only -and $screen.ClassName -notlike $Only) {
		continue
	}

	# new index of every control that has one
	$newIndexMap = @{}
	foreach ($szContainer in @( $screen.Children.Keys )) {
		$nIndex = 0
		foreach ($name in (Get-LayoutOrder $screen $szContainer)) {
			$props = $screen.Props[$name]
			if ($null -eq $props -or -not $props.ContainsKey("TabIndex")) {
				continue
			}
			$newIndexMap[$name] = $nIndex
			$nIndex++
		}
	}

	$szText = [IO.File]::ReadAllText($file.FullName)
	$szNewText = [regex]::Replace($szText, "(this\.(\w+)\.TabIndex\s*=\s*)(\d+)(\s*;)", {
		param( $match )
		$name = $match.Groups[2].Value
		if (-not $newIndexMap.ContainsKey($name)) {
			return $match.Value
		}
		return $match.Groups[1].Value + $newIndexMap[$name] + $match.Groups[4].Value
	})
	if ($szNewText -ne $szText) {
		[IO.File]::WriteAllText($file.FullName, $szNewText, (New-Object System.Text.UTF8Encoding($true)))
		Write-Host "renumbered $($screen.ClassName)"
		$nFileCount++
	}
}
Write-Host "[tab order] $nFileCount files renumbered"
