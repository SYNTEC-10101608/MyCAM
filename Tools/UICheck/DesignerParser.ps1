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
	foreach ($match in [regex]::Matches($szText, "(?m)^\s*(?:private|internal|public|protected)\s+(?:readonly\s+)?([\w.]+)\s+(\w+)\s*;")) {
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
		$s = $statement.Trim()
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
