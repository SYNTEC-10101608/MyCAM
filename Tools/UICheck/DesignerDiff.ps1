param(
	[string]$Base = "HEAD",
	[string]$Head = "",
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
)
# Semantic difference of the WinForms designer files (.Designer.cs, .resx) between two versions, for review.
# Visual Studio regenerates the whole InitializeComponent on save (order, indentation, line breaks depend on the
# machine), so the text diff is not read; this lists what the screen is made of instead:
#
#   controls     added / removed / type changed / moved to another parent / child order (z-order) changed
#   properties   added / removed / changed, of the form and of each control
#   events       handlers attached / detached
#   others       remaining statements added / removed (AddRange, local variables ...)
#   resources    .resx data entries added / removed / changed (design-time metadata such as TrayLocation is ignored)
#
# A file whose text changed but whose content did not is reported as "formatting only".
#
#   DesignerDiff.ps1                      working tree against HEAD (before committing)
#   DesignerDiff.ps1 -Base origin/master  working tree against another version
#   DesignerDiff.ps1 -Base A -Head B      two versions (review of a branch: -Base origin/master -Head HEAD)
#
# This is a report, not a check: the exit code is 0 unless git fails.

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DesignerParser.ps1")

# statements that only control layout performance or initialization order
$LAYOUT_CALL_PATTERN = "\.(SuspendLayout|ResumeLayout|PerformLayout|BeginInit|EndInit)\(\w*\)$"

function Invoke-Git( [string[]]$argArray ) {
	$psi = New-Object System.Diagnostics.ProcessStartInfo
	$psi.FileName = "git"
	$psi.Arguments = (@( "-C", "`"$Root`"" ) + $argArray) -join " "
	$psi.UseShellExecute = $false
	$psi.RedirectStandardOutput = $true
	$psi.RedirectStandardError = $true
	$psi.StandardOutputEncoding = [Text.Encoding]::UTF8
	$process = [Diagnostics.Process]::Start($psi)
	$szOutput = $process.StandardOutput.ReadToEnd()
	$process.StandardError.ReadToEnd() | Out-Null
	$process.WaitForExit()
	return [pscustomobject]@{ ExitCode = $process.ExitCode; Output = $szOutput }
}

# text of a file in a version, the working tree when the version is empty; $null when the file does not exist
function Get-VersionText( [string]$szVersion, [string]$szPath ) {
	if (-not $szVersion) {
		$fullPath = Join-Path $Root $szPath
		if (-not (Test-Path $fullPath)) {
			return $null
		}
		return [IO.File]::ReadAllText($fullPath, [Text.Encoding]::UTF8)
	}
	$result = Invoke-Git @( "show", "`"$($szVersion):$szPath`"" )
	if ($result.ExitCode -ne 0) {
		return $null
	}
	return $result.Output
}

function Get-ChangedFiles {
	$range = if ($Head) { @( $Base, $Head ) } else { @( $Base ) }
	$result = Invoke-Git (@( "diff", "--name-only" ) + $range + @( "--", "*.Designer.cs", "*.resx" ))
	if ($result.ExitCode -ne 0) {
		throw "git diff failed: $Base $Head"
	}
	$fileList = @( $result.Output -split "\r?\n" | Where-Object { $_ } )
	# new files that are not added yet belong to the working tree
	if (-not $Head) {
		$untracked = Invoke-Git @( "ls-files", "--others", "--exclude-standard", "--", "*.Designer.cs", "*.resx" )
		$fileList += @( $untracked.Output -split "\r?\n" | Where-Object { $_ } )
	}
	return @( $fileList | Sort-Object -Unique )
}

function Read-Screen( [string]$szText ) {
	if ($null -eq $szText) {
		return $null
	}
	$screen = Read-DesignerText $szText ""

	# children in order, also of inner panels such as "m_container.ContentPanel"
	$screen.Children = @{}
	$parentMap = @{}
	$otherList = New-Object System.Collections.Generic.List[string]
	foreach ($s in (Get-DesignerStatements $szText)) {
		if ($s -match "^this\.(?:([\w.]+)\.)?Controls\.Add\(this\.(\w+)(,.*)?\)$") {
			$szParent = if ($Matches[1]) { $Matches[1] } else { "(form)" }
			Add-Child $screen $szParent $Matches[2]
			$parentMap[$Matches[2]] = $szParent
			continue
		}

		# statements not read into fields, properties, events and children
		$isKnown = $s -match "^this\.(\w+\.)?\w+ \+= new [\w.]+\(this\.\w+\)$" -or
			$s -match "^this\.[\w.]+ = " -or
			$s -match $LAYOUT_CALL_PATTERN
		if (-not $isKnown) {
			$otherList.Add($s)
		}
	}
	return [pscustomobject]@{ Screen = $screen; ParentMap = $parentMap; Others = $otherList }
}

# Name is written by the designer for every control; a hand-written file may leave it out (same value)
function Get-Props( $screen, [string]$name ) {
	$props = @{}
	if ($screen.Props.ContainsKey($name)) {
		$props = $screen.Props[$name].Clone()
	}
	if (-not $props.ContainsKey("Name")) {
		$props["Name"] = $name
	}
	return $props
}

# value without the namespaces of the common types, for reading
function Format-Value( [string]$szValue ) {
	return $szValue -replace "\bSystem\.(Drawing|Windows\.Forms)\.", ""
}

function Compare-PropMap( [string]$szOwner, $oldProps, $newProps, $changeList ) {
	foreach ($prop in @( $oldProps.Keys + $newProps.Keys | Sort-Object -Unique )) {
		$isOld = $oldProps.ContainsKey($prop)
		$isNew = $newProps.ContainsKey($prop)
		$szOld = Format-Value $oldProps[$prop]
		$szNew = Format-Value $newProps[$prop]
		if ($isOld -and -not $isNew) {
			$changeList.Add("  - $szOwner.$prop = $szOld")
		}
		elseif ($isNew -and -not $isOld) {
			$changeList.Add("  + $szOwner.$prop = $szNew")
		}
		elseif ($oldProps[$prop] -cne $newProps[$prop]) {
			$changeList.Add("  ~ $szOwner.$prop : $szOld -> $szNew")
		}
	}
}

function Compare-List( [string]$szLabel, $oldList, $newList, $changeList ) {
	$oldCountMap = @{}
	foreach ($item in $oldList) {
		$oldCountMap[$item] = 1 + [int]$oldCountMap[$item]
	}
	$newCountMap = @{}
	foreach ($item in $newList) {
		$newCountMap[$item] = 1 + [int]$newCountMap[$item]
	}
	foreach ($item in @( $oldCountMap.Keys + $newCountMap.Keys | Sort-Object -Unique )) {
		$nDelta = [int]$newCountMap[$item] - [int]$oldCountMap[$item]
		for ($i = 0; $i -lt [math]::Abs($nDelta); $i++) {
			$szSign = if ($nDelta -gt 0) { "+" } else { "-" }
			$changeList.Add("  $szSign $szLabel $(Format-Value $item)")
		}
	}
}

function Compare-Designer( $old, $new, $changeList ) {
	$oldScreen = $old.Screen
	$newScreen = $new.Screen

	# controls: type and parent
	foreach ($name in @( $oldScreen.Fields.Keys + $newScreen.Fields.Keys | Sort-Object -Unique )) {
		$isOld = $oldScreen.Fields.ContainsKey($name)
		$isNew = $newScreen.Fields.ContainsKey($name)
		if ($isOld -and -not $isNew) {
			$changeList.Add("  - control $name ($(Format-Value $oldScreen.Fields[$name]))")
			continue
		}
		if ($isNew -and -not $isOld) {
			$changeList.Add("  + control $name ($(Format-Value $newScreen.Fields[$name])) in $($new.ParentMap[$name])")
			continue
		}
		if ($oldScreen.Fields[$name] -ne $newScreen.Fields[$name]) {
			$szOldType = Format-Value $oldScreen.Fields[$name]
			$szNewType = Format-Value $newScreen.Fields[$name]
			$changeList.Add("  ~ control $name type : $szOldType -> $szNewType")
		}
		if ($old.ParentMap[$name] -ne $new.ParentMap[$name]) {
			$changeList.Add("  ~ control $name parent : $($old.ParentMap[$name]) -> $($new.ParentMap[$name])")
		}
	}

	# child order decides which control is drawn on top when they overlap
	foreach ($parent in @( $oldScreen.Children.Keys | Where-Object { $newScreen.Children.ContainsKey($_) } )) {
		$oldOrder = @( $oldScreen.Children[$parent] | Where-Object { $newScreen.Children[$parent] -contains $_ } )
		$newOrder = @( $newScreen.Children[$parent] | Where-Object { $oldScreen.Children[$parent] -contains $_ } )
		if (($oldOrder -join ",") -ne ($newOrder -join ",")) {
			$changeList.Add("  ~ children of $parent : order (z-order) changed")
		}
	}

	# properties of the form and of each control
	Compare-PropMap "(form)" $oldScreen.FormProps $newScreen.FormProps $changeList
	foreach ($name in @( $oldScreen.Fields.Keys + $newScreen.Fields.Keys | Sort-Object -Unique )) {
		$oldProps = if ($oldScreen.Fields.ContainsKey($name)) { Get-Props $oldScreen $name } else { @{} }
		$newProps = if ($newScreen.Fields.ContainsKey($name)) { Get-Props $newScreen $name } else { @{} }
		Compare-PropMap $name $oldProps $newProps $changeList
	}

	Compare-List "event" $oldScreen.Events $newScreen.Events $changeList
	Compare-List "statement" $old.Others $new.Others $changeList
}

# data entries of a .resx (name -> type and value); metadata is design-time only and not compiled
function Read-ResxData( [string]$szText ) {
	$dataMap = @{}
	if ($null -eq $szText) {
		return $dataMap
	}
	$xml = New-Object System.Xml.XmlDocument
	$xml.LoadXml($szText.TrimStart([char]0xFEFF))
	foreach ($node in $xml.SelectNodes("/root/data")) {
		$valueNode = $node.SelectSingleNode("value")
		$szValue = if ($valueNode) { ($valueNode.InnerText -replace "\s+", "") } else { "" }
		$szType = "$($node.GetAttribute("type"))|$($node.GetAttribute("mimetype"))"
		$dataMap[$node.GetAttribute("name")] = "$szType|$szValue"
	}
	return $dataMap
}

function Compare-Resx( $oldMap, $newMap, $changeList ) {
	foreach ($name in @( $oldMap.Keys + $newMap.Keys | Sort-Object -Unique )) {
		if (-not $newMap.ContainsKey($name)) {
			$changeList.Add("  - resource $name")
		}
		elseif (-not $oldMap.ContainsKey($name)) {
			$changeList.Add("  + resource $name")
		}
		elseif ($oldMap[$name] -cne $newMap[$name]) {
			$changeList.Add("  ~ resource $name changed")
		}
	}
}

# ---------------------------------------------------------------- compare
$szHeadName = if ($Head) { $Head } else { "working tree" }
Write-Host "[designer diff] $Base -> $szHeadName"
$nChangedCount = 0
$nFormattingCount = 0
$fileList = Get-ChangedFiles
foreach ($szPath in $fileList) {
	$szOldText = Get-VersionText $Base $szPath
	$szNewText = Get-VersionText $Head $szPath
	$changeList = New-Object System.Collections.Generic.List[string]
	if ($null -eq $szOldText -and $null -eq $szNewText) {
		continue
	}
	elseif ($null -eq $szOldText) {
		$changeList.Add("  + new file")
	}
	elseif ($null -eq $szNewText) {
		$changeList.Add("  - file removed")
	}
	elseif ($szPath -like "*.resx") {
		Compare-Resx (Read-ResxData $szOldText) (Read-ResxData $szNewText) $changeList
	}
	else {
		Compare-Designer (Read-Screen $szOldText) (Read-Screen $szNewText) $changeList
	}

	if ($changeList.Count -eq 0) {
		$nFormattingCount++
		Write-Host "$szPath : formatting only"
		continue
	}
	$nChangedCount++
	Write-Host $szPath
	foreach ($change in $changeList) {
		Write-Host $change
	}
}
Write-Host "[designer diff] $($fileList.Count) files, $nChangedCount with changes, $nFormattingCount formatting only"
exit 0
