param(
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path,
	[string]$Base = "a4b3826d",
	[string]$Only = ""
)
# Issue-only check of the UI style unification (Doc/UIStyleUnify/5_Verification.md): behavior comparison against
# the version before the issue. The base is fixed to the commit the branch started from, so the result can be
# reproduced after the merge. Appearance may change; everything below must stay the same unless listed in
# BehaviorExceptions.txt.
#
#   B1  event subscriptions in the designer file
#   B2  radio button groups (which radio buttons are exclusive to each other)
#   B3  non-appearance properties of controls (range, decimals, checked, enabled, visible, read-only, items ...);
#       a control whose type changed is compared by effective values, the defaults read from new instances of both
#       types, so the Debug build (CommonUI.dll) is needed
#   B4  form properties with behavior (AcceptButton, CancelButton, ControlBox, KeyPreview, TopMost)
#   B5  controls used by the code still exist
#   B6  control types: only appearance replacements (Button -> ThemedButton, GroupBox -> SectionPanel ...)
#   B7  Show / ShowDialog call sites in the whole repository
#   B8  event subscriptions and Enabled / Visible / ReadOnly / Checked assignments in the code files changed since
#       the base (screens, main window, CNC panels, helpers)
#
# Controls renamed for the naming rule are listed in RenameMap.txt ("<class> <old name> <new name>"); the base is
# compared after the same renames, so a rename alone is not a difference.
# The tab order is not compared: it follows the layout and is checked by CheckStatic.ps1 (S10).
#
# Exceptions: "<rule> <class> <control> <reason>" in BehaviorExceptions.txt; a B3 exception also names the property
# ("B3 <class> <control> <property> <reason>"). Exit code 1 on a difference.

$ErrorActionPreference = "Stop"
. (Join-Path $Root "Tools\UICheck\DesignerParser.ps1")

$SCREEN_DIRS = @( "MyCAM\Editor\Dialog", "MyCAM\CNC", "CncUI\Panel", "CncUI\Dialog" )
$BEHAVIOR_PROPS = @( "Minimum", "Maximum", "DecimalPlaces", "Increment", "Value", "Checked", "CheckState", "Enabled",
	"Visible", "ReadOnly", "Items", "DropDownStyle", "Multiline", "MaxLength", "TabStop", "Hexadecimal",
	"ThousandsSeparator", "AutoCheck", "DialogResult", "RevertOnOutOfRange", "RestrictInvalidCharacters",
	"DebounceInterval", "AllowUserToAddRows", "AllowUserToDeleteRows", "SelectionMode", "MultiSelect",
	"CheckOnClick", "TickFrequency", "SmallChange", "LargeChange", "ScrollBars", "WordWrap", "AcceptsReturn",
	"UseSystemPasswordChar", "PasswordChar" )
$FORM_BEHAVIOR_PROPS = @( "AcceptButton", "CancelButton", "ControlBox", "KeyPreview", "TopMost" )

# appearance-only replacements; the key is the old type, the value the allowed new types
$TYPE_REPLACEMENTS = @{
	"System.Windows.Forms.Button" = @( "CommonUI.Controls.ThemedButton" )
	"CommonUI.Controls.PanelButton" = @( "CommonUI.Controls.ThemedButton" )
	"System.Windows.Forms.GroupBox" = @( "CommonUI.Controls.SectionPanel", "System.Windows.Forms.Panel" )
	"CommonUI.Controls.GroupPanel" = @( "CommonUI.Controls.SectionPanel" )
	"System.Windows.Forms.Label" = @( "CommonUI.Controls.ThemedLabel" )
	"MyCAM.Editor.DebouncedNumericUpDown" = @( "CommonUI.Controls.DebouncedNumericUpDown" )
	"DebouncedNumericUpDown" = @( "CommonUI.Controls.DebouncedNumericUpDown" )
	"System.Windows.Forms.TableLayoutPanel" = @( "System.Windows.Forms.Panel", "CommonUI.Controls.SectionPanel" )
	"System.Windows.Forms.FlowLayoutPanel" = @( "System.Windows.Forms.Panel", "CommonUI.Controls.SectionPanel" )
	"System.Windows.Forms.Panel" = @( "CommonUI.Controls.SectionPanel" )
	"System.Windows.Forms.DataGridView" = @( "CommonUI.Controls.ThemedDataGridView" )
}

$exceptions = @{}
$exceptionFile = Join-Path $PSScriptRoot "BehaviorExceptions.txt"
if (Test-Path $exceptionFile) {
	foreach ($line in Get-Content $exceptionFile -Encoding UTF8) {
		# a B3 exception also names the property, so other properties of the same control are still compared
		if ($line -match "^\s*(B3)\s+(\S+)\s+(\S+)\s+(\S+)") {
			$exceptions["$($Matches[1]) $($Matches[2]) $($Matches[3]) $($Matches[4])"] = $true
		}
		elseif ($line -match "^\s*(B\d)\s+(\S+)\s+(\S+)") {
			$exceptions["$($Matches[1]) $($Matches[2]) $($Matches[3])"] = $true
		}
	}
}
# renames per class
$renameMap = @{}
$renameFile = Join-Path $PSScriptRoot "RenameMap.txt"
if (Test-Path $renameFile) {
	foreach ($line in Get-Content $renameFile -Encoding UTF8) {
		if ($line -match "^\s*(\S+)\s+(\w+)\s+(\w+)\s*$") {
			if (-not $renameMap.ContainsKey($Matches[1])) {
				$renameMap[$Matches[1]] = New-Object System.Collections.Generic.List[object]
			}
			$renameMap[$Matches[1]].Add(@( $Matches[2], $Matches[3] ))
		}
	}
}
function Rename-BaseText( [string]$szText, [string]$szClass ) {
	if ($null -eq $szText) {
		return $null
	}
	if (-not $renameMap.ContainsKey($szClass)) {
		return $szText
	}

	# one pass, so a name that is renamed and another renamed to it do not chain; event handlers named
	# <control>_<event> are renamed with their control
	$nameMap = @{}
	foreach ($pair in $renameMap[$szClass]) {
		$nameMap[$pair[0]] = $pair[1]
	}
	$szAlternation = ($nameMap.Keys | Sort-Object Length -Descending | ForEach-Object { [regex]::Escape($_) }) -join "|"
	return [regex]::Replace($szText, "\b($szAlternation)(?=\b|_)", { param( $match ) $nameMap[$match.Groups[1].Value] })
}

$problems = New-Object System.Collections.Generic.List[string]
function Report( [string]$szRule, [string]$szClass, [string]$szControl, [string]$szMessage, [string]$szProperty = "" ) {
	# an exception may use wildcards, e.g. "B6 CalibrationREFDlg m_tbxPnt*"
	$key = "$szRule $szClass $szControl"
	if ($szProperty) {
		$key += " $szProperty"
	}
	if (@($exceptions.Keys | Where-Object { $key -like $_ }).Count -gt 0) {
		return
	}
	$problems.Add("$szRule $szClass.$szControl $szMessage")
}

function Get-BaseText( [string]$szRelative ) {
	$szGitPath = $szRelative -replace "\\", "/"
	$text = & git -C $Root show "$($Base):$szGitPath" 2>$null
	if ($LASTEXITCODE -ne 0) {
		return $null
	}
	return ($text -join "`n")
}

function Get-RadioGroups( $screen ) {
	$groups = @()
	foreach ($parent in $screen.Children.Keys) {
		$radios = @( $screen.Children[$parent] | Where-Object { $screen.Fields[$_] -eq "System.Windows.Forms.RadioButton" } |
			Sort-Object )
		if ($radios.Count -gt 0) {
			$groups += ($radios -join ",")
		}
	}
	return @( $groups | Sort-Object )
}

function Get-ItemsProp( [string]$szText ) {
	$items = @{}
	foreach ($match in [regex]::Matches($szText, "this\.(\w+)\.Items\.AddRange\(new object\[\]\s*\{(?<v>[^}]*)\}\)")) {
		$items[$match.Groups[1].Value] = ($match.Groups["v"].Value -replace "\s+", " ").Trim()
	}
	return $items
}

# ---------------------------------------------------------------- default values
# A designer file only writes the properties that differ from the default, so a control whose type changed is
# compared by effective values: the written value, or the default of its type read from a new instance.
Add-Type -AssemblyName System.Windows.Forms
$commonUIPath = Join-Path $Root "MyCAM\bin\Debug\CommonUI.dll"
if (-not (Test-Path $commonUIPath)) {
	Write-Host "[CompareBehavior] $commonUIPath not found, build first (Build.bat Debug)."
	exit 1
}
[void][Reflection.Assembly]::LoadFrom($commonUIPath)
$defaultInstances = @{}

# canonical text of a designer value or a property value: decimals without trailing zeros, enums by name
function ConvertTo-Canonical( $value ) {
	if ($value -is [bool]) {
		return $value.ToString().ToLowerInvariant()
	}
	if ($value -is [decimal]) {
		return $value.ToString("G29", [Globalization.CultureInfo]::InvariantCulture)
	}
	if ($value -is [enum]) {
		return $value.ToString()
	}
	$szValue = "$value"
	if ($szValue -match "^new decimal\(new int\[\] \{\s*(-?\d+), (-?\d+), (-?\d+), (-?\d+)\s*\}\)$") {
		$bits = [int[]]@( [int]$Matches[1], [int]$Matches[2], [int]$Matches[3], [int]$Matches[4] )
		return ConvertTo-Canonical (New-Object decimal (, $bits))
	}
	if ($szValue -match "^[A-Za-z_][\w\.]*\.(\w+)$") {
		return $Matches[1]
	}
	return $szValue
}

# $null when the type cannot be created; otherwise Has tells whether the type has the property
function Get-DefaultValue( [string]$szType, [string]$szProperty ) {
	if (-not $defaultInstances.ContainsKey($szType)) {
		$type = $szType -as [type]
		$defaultInstances[$szType] = if ($null -eq $type) { $null } else { New-Object $type }
	}
	$instance = $defaultInstances[$szType]
	if ($null -eq $instance) {
		return $null
	}
	$info = $instance.GetType().GetProperty($szProperty, [Reflection.BindingFlags]"Public, Instance")
	if ($null -eq $info) {
		return [pscustomobject]@{ Has = $false; Value = "(none)" }
	}
	if ($szProperty -eq "Items") {
		return [pscustomobject]@{ Has = $true; Value = "" }
	}
	return [pscustomobject]@{ Has = $true; Value = (ConvertTo-Canonical $info.GetValue($instance, $null)) }
}

# B3 of a control whose type changed: a property only the new type has is a new setting and not compared;
# a property the old type has must keep its effective value, whether the old designer wrote it or not
function Compare-EffectiveValue( [string]$szClass, [string]$name, [string]$prop, [string]$oldType, [string]$newType,
	[string]$szOldValue, [string]$szNewValue ) {
	$oldDefault = Get-DefaultValue $oldType $prop
	$newDefault = Get-DefaultValue $newType $prop
	if ($null -eq $oldDefault -or $null -eq $newDefault) {
		Report "B3" $szClass $name "$prop cannot be compared: type $oldType or $newType cannot be created" $prop
		return
	}
	if (-not $oldDefault.Has) {
		return
	}
	$szOld = if ($szOldValue -ne "") { ConvertTo-Canonical $szOldValue } else { $oldDefault.Value }

	# a feature the new type does not have is only lost when the old control changed it from the default
	if (-not $newDefault.Has -and $szOld -eq $oldDefault.Value) {
		return
	}
	$szNew = if (-not $newDefault.Has) { "(none)" } elseif ($szNewValue -ne "") { ConvertTo-Canonical $szNewValue } else { $newDefault.Value }
	if ($szOld -ne $szNew) {
		Report "B3" $szClass $name "$prop changed (effective): '$szOld' -> '$szNew'" $prop
	}
}

# ---------------------------------------------------------------- designer files
$designerFiles = @( foreach ($dir in $SCREEN_DIRS) {
	Get-ChildItem (Join-Path $Root $dir) -Filter "*.Designer.cs" -Recurse
} ) + @( Get-Item (Join-Path $Root "MyCAM\StartupForm.Designer.cs") )
foreach ($file in $designerFiles) {
	$szRelative = $file.FullName.Substring($Root.Length).TrimStart("\")
	$szFileClass = $file.Name -replace "\.Designer\.cs$", ""
	$baseText = Rename-BaseText (Get-BaseText $szRelative) $szFileClass
	$codeRelative = $szRelative -replace "\.Designer\.cs$", ".cs"
	$baseCode = Rename-BaseText (Get-BaseText $codeRelative) $szFileClass
	if ($null -eq $baseText) {
		continue
	}
	$old = Read-DesignerText $baseText $baseCode
	$new = Read-DesignerFile $file.FullName
	$szClass = $new.ClassName
	if ($Only -and $szClass -notlike $Only) {
		continue
	}

	# B1 events
	foreach ($event in $old.Events) {
		if (-not $new.Events.Contains($event)) {
			Report "B1" $szClass ($event -split "\.")[0] "event removed: $event"
		}
	}
	foreach ($event in $new.Events) {
		if (-not $old.Events.Contains($event)) {
			Report "B1" $szClass ($event -split "\.")[0] "event added: $event"
		}
	}

	# B2 radio groups
	$oldGroups = Get-RadioGroups $old
	$newGroups = Get-RadioGroups $new
	if (($oldGroups -join " | ") -ne ($newGroups -join " | ")) {
		Report "B2" $szClass "*" "radio groups changed: [$($oldGroups -join ' | ')] -> [$($newGroups -join ' | ')]"
	}

	# B3 / B6 controls
	$oldItems = Get-ItemsProp $baseText
	$newItems = Get-ItemsProp (Get-Content $file.FullName -Raw -Encoding UTF8)
	foreach ($name in $old.Fields.Keys) {
		$oldType = $old.Fields[$name]
		if (-not $new.Fields.ContainsKey($name)) {
			$isLayoutOnly = $oldType -match "(TableLayoutPanel|FlowLayoutPanel|GroupBox|GroupPanel|\.Panel)$"
			if (-not $isLayoutOnly) {
				Report "B6" $szClass $name "control removed ($oldType)"
			}
			continue
		}
		$newType = $new.Fields[$name]
		$isSameKind = $oldType -eq $newType -or ($TYPE_REPLACEMENTS.ContainsKey($oldType) -and $TYPE_REPLACEMENTS[$oldType] -contains $newType)
		# a decided type change is still compared: the properties the base set must stay, the ones only the new
		# type has are new settings and not compared
		$isTypeChanged = -not $isSameKind
		if ($isTypeChanged) {
			Report "B6" $szClass $name "type changed: $oldType -> $newType"
		}
		$oldProps = if ($old.Props.ContainsKey($name)) { $old.Props[$name] } else { @{} }
		$newProps = if ($new.Props.ContainsKey($name)) { $new.Props[$name] } else { @{} }
		$isContainer = $oldType -match "(GroupBox|GroupPanel|\.Panel|TableLayoutPanel|FlowLayoutPanel)$"
		foreach ($prop in $BEHAVIOR_PROPS) {
			# containers never take the focus; GroupBox only wrote its default TabStop = false
			if ($isContainer -and $prop -eq "TabStop") {
				continue
			}
			$oldValue = if ($prop -eq "Items") { $oldItems[$name] } else { $oldProps[$prop] }
			$newValue = if ($prop -eq "Items") { $newItems[$name] } else { $newProps[$prop] }
			if ($isTypeChanged) {
				Compare-EffectiveValue $szClass $name $prop $oldType $newType "$oldValue" "$newValue"
				continue
			}
			if ("$oldValue" -ne "$newValue") {
				Report "B3" $szClass $name "$prop changed: '$oldValue' -> '$newValue'" $prop
			}
		}
	}

	# B4 form properties
	foreach ($prop in $FORM_BEHAVIOR_PROPS) {
		if ("$($old.FormProps[$prop])" -ne "$($new.FormProps[$prop])") {
			Report "B4" $szClass "(form)" "$prop changed: '$($old.FormProps[$prop])' -> '$($new.FormProps[$prop])'"
		}
	}

	# B5 controls used by the code
	$codePath = $file.FullName -replace "\.Designer\.cs$", ".cs"
	if (Test-Path $codePath) {
		$code = Get-Content $codePath -Raw -Encoding UTF8
		foreach ($name in $old.Fields.Keys) {
			if ($new.Fields.ContainsKey($name) -or $code -notmatch "\b$name\b") {
				continue
			}
			if ($code -notmatch "\b\w+\s+$name\s*[;=]") {
				Report "B5" $szClass $name "used by the code but no longer exists"
			}
		}
	}
}

# ---------------------------------------------------------------- B7 Show / ShowDialog
function Get-ShowCalls( [string[]]$lines ) {
	$calls = @{}
	foreach ($line in $lines) {
		if ($line -match "^([^:]+):(.*)$") {
			$key = "$($Matches[1]): $($Matches[2].Trim())"
			$calls[$key] = 1 + [int]$calls[$key]
		}
	}
	return $calls
}
# the file is the "control" of a B7 exception, so an exception covers the call sites of one file only
function Get-CallFile( [string]$szKey ) {
	return ($szKey -split ": ", 2)[ 0 ]
}
$pattern = "\.(Show|ShowDialog)\s*\("
$oldCalls = Get-ShowCalls (& git -C $Root grep -I -E -e $pattern $Base -- "*.cs" | ForEach-Object { $_ -replace "^$($Base):", "" })
$newCalls = Get-ShowCalls (& git -C $Root grep -I -E --untracked -e $pattern -- "*.cs" | Where-Object { $_ -notmatch "^Tools/" })
foreach ($key in $oldCalls.Keys) {
	if ($newCalls[$key] -ne $oldCalls[$key]) {
		Report "B7" "repository" (Get-CallFile $key) "call site changed or removed: $key"
	}
}
foreach ($key in $newCalls.Keys) {
	if (-not $oldCalls.ContainsKey($key)) {
		Report "B7" "repository" (Get-CallFile $key) "call site added: $key"
	}
}

# ---------------------------------------------------------------- B8 behavior statements in the code
# one statement per line is the coding style, so a line is a statement; whitespace is normalized
function Get-BehaviorStatements( [string]$szText ) {
	$statements = @{}
	if ($null -eq $szText) {
		return $statements
	}
	foreach ($line in ($szText -split "`n")) {
		$szLine = ($line.Trim() -replace "\s+", " ")
		# an event is a PascalCase member and its handler a method name, a delegate or a lambda;
		# "y += row.Height + 2" and "nTop += nHeight" are arithmetic
		$isEvent = $szLine -cmatch "^([\w\[\]\(\)]+\.)*[A-Z]\w*\s(\+|-)= (new [\w\.]+\(|\(.*\) =>|\w+ =>|delegate|[\w\.]+;$)"
		$isState = $szLine -match "\.(Enabled|Visible|ReadOnly|Checked) = "
		if ($isEvent -or $isState) {
			$statements[$szLine] = 1 + [int]$statements[$szLine]
		}
	}
	return $statements
}
$changedFiles = & git -C $Root diff --name-only --diff-filter=M $Base -- "*.cs" |
	Where-Object { $_ -notmatch "\.Designer\.cs$" -and $_ -notmatch "^(Tools|CommonUI)/" }
foreach ($szGitPath in $changedFiles) {
	$szClass = [IO.Path]::GetFileNameWithoutExtension($szGitPath)
	if ($Only -and $szClass -notlike $Only) {
		continue
	}
	$oldStatements = Get-BehaviorStatements (Rename-BaseText (Get-BaseText $szGitPath) $szClass)
	$newStatements = Get-BehaviorStatements (Get-Content (Join-Path $Root $szGitPath) -Raw -Encoding UTF8)
	foreach ($szLine in $oldStatements.Keys) {
		if ($newStatements[$szLine] -ne $oldStatements[$szLine]) {
			$szTarget = ($szLine -split "[ \.]")[0]
			Report "B8" $szClass $szTarget "statement removed or count changed: $szLine"
		}
	}
	foreach ($szLine in $newStatements.Keys) {
		if (-not $oldStatements.ContainsKey($szLine)) {
			$szTarget = ($szLine -split "[ \.]")[0]
			Report "B8" $szClass $szTarget "statement added: $szLine"
		}
	}
}

foreach ($problem in $problems) {
	Write-Host "FAIL  $problem"
}
Write-Host "[behavior] compared with $Base, $($problems.Count) differences"
if ($problems.Count -gt 0) {
	exit 1
}
exit 0
