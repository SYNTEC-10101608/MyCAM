param(
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
	[string]$Base = "master",
	[string]$Only = ""
)
# Behavior comparison against the version before the UI unification (ui-style.instructions.md, section 8).
# Appearance may change; everything below must stay the same unless listed in BehaviorExceptions.txt.
#
#   B1  event subscriptions in the designer file
#   B2  radio button groups (which radio buttons are exclusive to each other)
#   B3  non-appearance properties of controls (range, decimals, checked, enabled, visible, read-only, items ...)
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
# Exceptions: "<rule> <class> <control> <reason>" in BehaviorExceptions.txt. Exit code 1 on a difference.

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DesignerParser.ps1")

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
		if ($line -match "^\s*(B\d)\s+(\S+)\s+(\S+)") {
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
function Report( [string]$szRule, [string]$szClass, [string]$szControl, [string]$szMessage ) {
	# an exception may use wildcards, e.g. "B6 CalibrationREFDlg m_tbxPnt*"
	$key = "$szRule $szClass $szControl"
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
		if (-not $isSameKind) {
			Report "B6" $szClass $name "type changed: $oldType -> $newType"
			continue
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
			if ("$oldValue" -ne "$newValue") {
				Report "B3" $szClass $name "$prop changed: '$oldValue' -> '$newValue'"
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
$pattern = "\.(Show|ShowDialog)\s*\("
$oldCalls = Get-ShowCalls (& git -C $Root grep -I -E -e $pattern $Base -- "*.cs" | ForEach-Object { $_ -replace "^$($Base):", "" })
$newCalls = Get-ShowCalls (& git -C $Root grep -I -E --untracked -e $pattern -- "*.cs" | Where-Object { $_ -notmatch "^Tools/" })
foreach ($key in $oldCalls.Keys) {
	if ($newCalls[$key] -ne $oldCalls[$key]) {
		Report "B7" "repository" "*" "call site changed or removed: $key"
	}
}
foreach ($key in $newCalls.Keys) {
	if (-not $oldCalls.ContainsKey($key)) {
		Report "B7" "repository" "*" "call site added: $key"
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
