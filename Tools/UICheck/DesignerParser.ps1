# Reads a WinForms .Designer.cs file into fields, properties and the control tree. Shared by the UI checks.

function Read-DesignerText( [string]$szText, [string]$szCode ) {
	$screen = [pscustomobject]@{
		ClassName = ""
		BaseType = ""
		Fields = @{}
		FormProps = @{}
		Props = @{}
		Events = New-Object System.Collections.Generic.List[string]
		Children = @{}
	}
	if ($szText -match "partial class (\w+)") {
		$screen.ClassName = $Matches[1]
	}
	if ($szCode -match "partial class $($screen.ClassName)\s*:\s*([\w.]+(<[^>]+>)?)") {
		$screen.BaseType = ($Matches[1] -split "\.")[-1]
		if ($Matches[2]) {
			$screen.BaseType = (($Matches[1] -replace "<.*$", "") -split "\.")[-1] + $Matches[2]
		}
	}

	# field declarations at the end of the designer file
	# the access modifier is optional (the default is private)
	foreach ($match in [regex]::Matches($szText, "(?m)^\s*(?:(?:private|internal|public|protected)\s+)?(?:readonly\s+)?((?:\w+\.)+\w+|\w+(?=\s+m_))\s+(\w+)\s*;")) {
		$screen.Fields[$match.Groups[2].Value] = $match.Groups[1].Value
	}

	# statements of InitializeComponent, joined so that multi-line values are read whole
	$body = $szText
	if ($szText -match "(?s)void InitializeComponent\(\)\s*\{(.*)\r?\n\t\t\}") {
		$body = $Matches[1]
	}
	$body = $body -replace "(?m)^\s*//.*$", ""
	$statements = [regex]::Split(($body -replace "\r?\n\s*", " "), ";(?=\s*(this\.|\(\(|$))")
	foreach ($statement in $statements) {
		# hand-edited files put spaces inside parentheses
		$s = $statement.Trim() -replace "\(\s+", "(" -replace "\s+\)", ")"
		if ($s -match "^this\.Controls\.Add\(this\.(\w+)\)") {
			Add-Child $screen "(form)" $Matches[1]
		}
		elseif ($s -match "^this\.(\w+)\.Controls\.Add\(this\.(\w+)(,.*)?\)") {
			Add-Child $screen $Matches[1] $Matches[2]
		}
		elseif ($s -match "^this\.(\w+)\.(\w+)\s*\+=\s*new [\w.]+\(this\.(\w+)\)") {
			$screen.Events.Add("$($Matches[1]).$($Matches[2]) += $($Matches[3])")
		}
		elseif ($s -match "^this\.(\w+)\s*\+=\s*new [\w.]+\(this\.(\w+)\)") {
			$screen.Events.Add("(form).$($Matches[1]) += $($Matches[2])")
		}
		elseif ($s -match "^this\.(\w+)\.([\w.]+)\s*=\s*(.+)$" -and $screen.Fields.ContainsKey($Matches[1])) {
			$name = $Matches[1]
			if (-not $screen.Props.ContainsKey($name)) {
				$screen.Props[$name] = @{}
			}
			$screen.Props[$name][$Matches[2]] = ConvertFrom-DesignerValue $Matches[3]
		}
		elseif ($s -match "^this\.([\w.]+)\s*=\s*(.+)$" -and -not $screen.Fields.ContainsKey($Matches[1])) {
			$screen.FormProps[$Matches[1]] = ConvertFrom-DesignerValue $Matches[2]
		}
	}
	return $screen
}

function Read-DesignerFile( [string]$szPath ) {
	$szText = Get-Content $szPath -Raw -Encoding UTF8
	$codePath = $szPath -replace "\.Designer\.cs$", ".cs"
	$szCode = if (Test-Path $codePath) { Get-Content $codePath -Raw -Encoding UTF8 } else { "" }
	return Read-DesignerText $szText $szCode
}

function Add-Child( $screen, [string]$szParent, [string]$szChild ) {
	if (-not $screen.Children.ContainsKey($szParent)) {
		$screen.Children[$szParent] = New-Object System.Collections.Generic.List[string]
	}
	$screen.Children[$szParent].Add($szChild)
}

function ConvertFrom-DesignerValue( [string]$szValue ) {
	$v = $szValue.Trim()
	if ($v -match '^"(.*)"$') {
		return $Matches[1] -replace '\\"', '"' -replace "\\r\\n", "`n"
	}
	return ($v -replace "\s+", " ")
}

function ConvertTo-Size( [string]$szValue ) {
	if ($szValue -match "\(\s*(-?\d+)\s*,\s*(-?\d+)\s*\)") {
		return [pscustomobject]@{ Width = [int]$Matches[1]; Height = [int]$Matches[2] }
	}
	return $null
}

function Get-Bounds( $screen, [string]$name ) {
	$props = $screen.Props[$name]
	if ($null -eq $props -or -not $props.ContainsKey("Location") -or -not $props.ContainsKey("Size")) {
		return $null
	}
	$location = ConvertTo-Size $props["Location"]
	$size = ConvertTo-Size $props["Size"]
	return [pscustomobject]@{ Name = $name; Left = $location.Width; Top = $location.Height; Width = $size.Width; Height = $size.Height }
}

# Children of a container in tab order: rows from top to bottom, left to right within a row. Controls whose tops are
# at most ROW_TOLERANCE apart share a row (labels sit 4 lower, check boxes 2 lower than the inputs of their row).
function Get-LayoutOrder( $screen, [string]$szContainer ) {
	$ROW_TOLERANCE = 6
	$items = @( $screen.Children[$szContainer] | Where-Object { $_ } | ForEach-Object { Get-Bounds $screen $_ } |
		Where-Object { $_ } | Sort-Object Top, Left )
	$rows = New-Object System.Collections.Generic.List[object]
	foreach ($item in $items) {
		$isSameRow = $rows.Count -gt 0 -and ($item.Top - $rows[$rows.Count - 1][0].Top) -le $ROW_TOLERANCE
		if ($isSameRow) {
			$rows[$rows.Count - 1].Add($item)
		}
		else {
			$row = New-Object System.Collections.Generic.List[object]
			$row.Add($item)
			$rows.Add($row)
		}
	}
	$order = New-Object System.Collections.Generic.List[string]
	foreach ($row in $rows) {
		foreach ($item in ($row | Sort-Object Left)) {
			$order.Add($item.Name)
		}
	}
	return $order
}