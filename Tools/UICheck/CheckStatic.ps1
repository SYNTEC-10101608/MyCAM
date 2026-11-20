param(
	[string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
	[string]$Only = ""
)
# Static check of the UI rules (ui-style.instructions.md). Reads source files only, no build needed.
#
#   S1  control types: only the white list (section 3); TextBox only for the listed non-numeric fields
#   S2  designer files contain no appearance (font, colors, frame, auto scaling, flat style, dock / anchor)
#   S3  every dialog derives from DialogBase (directly or through FormBase / EditDialogBase), panels from PanelBase
#   S4  screen code contains no Color / SystemColors / new Font (data colors: named static readonly constants)
#   S5  dialog layout (section 4.4): margins, section width / gap, row grid, card height, button bar
#   S6  language: captions written in Chinese have their ID in zh-TW and zh-CN; every language in Language\ has the
#       same IDs; the language file is named after the class
#   S7  language: a control whose name is an ID is not applied by hand (ApplyLanguage applies it)
#   S8  section titles (section 4.5): a card title is SectionPanel.Text; no emphasized label in a card's title bar
#   S9  control field names start with the prefix of their type (section 3), also in the main window
#   S10 tab order follows the layout: rows from top to bottom, left to right (FixTabOrder.ps1 renumbers)
#   S11 dialog code sets no font, color, location or size of its controls (the designer layout is what runs)
#   S12 no DPI scaling (DeviceDpi): the layout is designed for 96 DPI
#   S13 language: an ID written like a control name belongs to a control of the class or is used by its code
#   S14 language: every dialog calls ApplyLanguage() right after InitializeComponent(); every embedded panel calls
#       ApplyLanguage( languageManager )
#   S15 only CommonUI defines control classes (MyCAM and CncUI use the CommonUI controls)
#   S16 button kinds: Start / Hold / Stop only in the CNC panels; at most one Primary in a container
#   S17 button bar order (section 4.4): auxiliary buttons from the left margin, the others from the right margin,
#       8 apart; Primary is the rightmost, Cancel / Close right before it, Reset / Preview on the left
#   S18 alignment in a card: the first input of each row starts at the same x; a unit label is 6 after its input
#   S19 main window designer: only the white list of types (section 3), no appearance
#   S20 data colors: every "static readonly Color" constant is a registered data color (StaticExceptions.txt)
#   S21 every public type of CommonUI is listed in the component catalog (section 9), in the subsection of its folder;
#       a file in CommonUI\<Folder> is in namespace CommonUI.<Folder>
#   S22 title bar of a titled card (section 4.5): button 23 high at y=5, check box at y=5, label 24 high at y=4,
#       the rightmost control ends at the content edge
#   S23 code creates no control outside the white list (section 3)
#   S24 InitializeComponent contains only what the designer can read: no control flow, lambda, interpolated string,
#       call of the screen's own methods, constant of the class, statement without "this."
#
# Exceptions are listed in StaticExceptions.txt: "<rule> <class> <control> <reason>". Exit code 1 on a problem.

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DesignerParser.ps1")

$SCREEN_DIRS = @( "MyCAM\Editor\Dialog", "MyCAM\CNC", "CncUI\Panel", "CncUI\Dialog" )
$CODE_SCOPE = @( "MyCAM\Editor\Dialog", "MyCAM\Editor\Action", "MyCAM\Editor\ViewManager.cs", "MyCAM\LogManager",
	"MyCAM\StartupForm.cs", "MyCAM\Layout", "MyCAM\CNC", "CncUI", "CommonUI\Controls", "CommonUI\Forms",
	"CommonUI\Layout", "CommonUI\Language" )
# every language that has language files (file name "<class>_<language>.xml")
$LANGUAGES = @( Get-ChildItem (Join-Path $Root "Language") -Filter "*.xml" |
	ForEach-Object { [regex]::Match($_.Name, "_([a-z]{2}-[A-Z]{2})\.xml$").Groups[1].Value } |
	Where-Object { $_ } | Sort-Object -Unique )

$ALLOWED_TYPES = @(
	"CommonUI.Controls.SectionPanel", "CommonUI.Controls.ThemedButton", "CommonUI.Controls.ThemedLabel",
	"CommonUI.Controls.DebouncedNumericUpDown", "CommonUI.Controls.MultiSelectTreeView",
	"CommonUI.Controls.LoadingBar", "CommonUI.Controls.RoundedProgressBar", "CommonUI.Controls.ThemedDataGridView",
	"CommonUI.Controls.ColorSwatch",
	"System.Windows.Forms.CheckBox", "System.Windows.Forms.RadioButton",
	"System.Windows.Forms.ComboBox", "System.Windows.Forms.CheckedListBox", "System.Windows.Forms.TrackBar",
	"System.Windows.Forms.PictureBox",
	"System.Windows.Forms.DataGridViewTextBoxColumn", "System.Windows.Forms.Panel",
	"System.Windows.Forms.ToolTip", "System.Windows.Forms.Timer", "System.ComponentModel.IContainer" )

# main window only (section 3)
$MAIN_WINDOW_TYPES = @( "System.Windows.Forms.MenuStrip", "System.Windows.Forms.ToolStrip",
	"System.Windows.Forms.ToolStripButton", "System.Windows.Forms.ToolStripMenuItem",
	"System.Windows.Forms.ToolStripSeparator" )

# non-numeric text fields (section 3)
$ALLOWED_TEXTBOX = @( "ConnectDlg.m_txtIP", "MeasureDlg.m_txtResult", "ExtendedGCodeDlg.m_txtGCode",
	"DesignSupportBracketDlg.m_txtSavePath" )

$FORBIDDEN_FORM_PROPS = @( "AutoScaleMode", "AutoScaleDimensions", "FormBorderStyle", "StartPosition", "MaximizeBox",
	"MinimizeBox", "ShowIcon", "ShowInTaskbar", "Font", "BackColor", "ForeColor" )
$FORBIDDEN_CONTROL_PROPS = @( "Font", "BackColor", "ForeColor", "FlatStyle", "UseVisualStyleBackColor", "Dock",
	"Anchor", "AutoScaleMode", "AutoScaleDimensions", "BorderStyle", "ImageTransparentColor" )

# field name prefix of each control type (section 3)
$PREFIX_MAP = @{
	"SectionPanel" = "m_pnl"; "Panel" = "m_pnl"
	"ThemedButton" = "m_btn"; "ThemedLabel" = "m_lbl"; "DebouncedNumericUpDown" = "m_num"
	"TextBox" = "m_txt"; "CheckBox" = "m_chk"; "RadioButton" = "m_rdo"; "ComboBox" = "m_cmb"
	"CheckedListBox" = "m_clb"; "TrackBar" = "m_trb"; "PictureBox" = "m_pic"; "ThemedDataGridView" = "m_dgv"
	"DataGridViewTextBoxColumn" = "m_col"; "ToolTip" = "m_ttp"; "Timer" = "m_tmr"; "ColorSwatch" = "m_swt"
	"MultiSelectTreeView" = "m_tv"; "TreeView" = "m_tv"; "LoadingBar" = "m_prg"; "RoundedProgressBar" = "m_prg"
	"MenuStrip" = "m_ms"; "ToolStrip" = "m_ts"; "ToolStripButton" = "m_tsb"
	"ToolStripMenuItem" = "m_tsmi"; "ToolStripSeparator" = "m_tss"
}

function Test-FieldNames( $screen ) {
	foreach ($name in $screen.Fields.Keys) {
		$szType = ($screen.Fields[$name] -split "\.")[-1]
		if ($szType -eq "IContainer") {
			continue
		}
		if (-not $PREFIX_MAP.ContainsKey($szType)) {
			Report "S9" $screen.ClassName $name "no prefix defined for $szType"
			continue
		}
		$szPrefix = $PREFIX_MAP[$szType]
		if ($name -cnotmatch "^$($szPrefix)[A-Z0-9]") {
			Report "S9" $screen.ClassName $name "$szType field must be named $szPrefix<Name>"
		}
	}
}

function Test-OrphanIds( $screen, [string]$szCodePath ) {
	$ids = Get-LanguageIds $screen.ClassName "zh-TW"
	if ($null -eq $ids) {
		return
	}
	$code = if (Test-Path $szCodePath) { Get-Content $szCodePath -Raw -Encoding UTF8 } else { "" }
	foreach ($id in $ids.Keys) {
		$isControlName = $id -cmatch "^m_[a-z]+[A-Z0-9]"

		# "<item>_ToolTip" is the tool tip of a menu / tool bar item, applied by name (section 7)
		$szOwner = $id -replace "_ToolTip$", ""
		if ($isControlName -and -not $screen.Fields.ContainsKey($szOwner) -and $code -notmatch "`"$id`"") {
			Report "S13" $screen.ClassName $id "language ID looks like a control name but no control or code uses it"
		}
	}
}

function Test-TabOrder( $screen ) {
	foreach ($szContainer in @( $screen.Children.Keys )) {
		$nPrevious = -1
		foreach ($name in (Get-LayoutOrder $screen $szContainer)) {
			$props = $screen.Props[$name]
			if ($null -eq $props -or -not $props.ContainsKey("TabIndex")) {
				continue
			}
			$nTabIndex = [int]$props["TabIndex"]
			if ($nTabIndex -le $nPrevious) {
				Report "S10" $screen.ClassName $name "TabIndex $nTabIndex is not after the control before it in the layout ($szContainer)"
			}
			$nPrevious = $nTabIndex
		}
	}
}

function Test-ApplyLanguage( $screen, [string]$szCodePath ) {
	$code = if (Test-Path $szCodePath) { Get-Content $szCodePath -Raw -Encoding UTF8 } else { "" }
	if ($code -notmatch "InitializeComponent\(\);\s*\r?\n\s*ApplyLanguage\(\);") {
		Report "S14" $screen.ClassName "*" "ApplyLanguage() must be called right after InitializeComponent()"
	}
}

# CNC machining kinds (section 3) and the single main action of a container
function Test-ButtonKinds( $screen, [bool]$isCncPanel ) {
	$primaryMap = @{}
	foreach ($name in $screen.Props.Keys) {
		$szKind = $screen.Props[$name]["Kind"]
		if ($null -eq $szKind) {
			continue
		}
		$isMachiningKind = $szKind -match "ButtonKind\.(Start|Hold|Stop)$"
		if ($isMachiningKind -and -not $isCncPanel) {
			Report "S16" $screen.ClassName $name "$szKind is only for the CNC panels"
		}
		if ($szKind -match "ButtonKind\.Primary$") {
			$szParent = Get-Parent $screen $name
			if (-not $primaryMap.ContainsKey($szParent)) {
				$primaryMap[$szParent] = New-Object System.Collections.Generic.List[string]
			}
			$primaryMap[$szParent].Add($name)
		}
	}
	foreach ($szParent in $primaryMap.Keys) {
		if ($primaryMap[$szParent].Count -gt 1) {
			Report "S16" $screen.ClassName $szParent "more than one Primary button: $($primaryMap[$szParent] -join ', ')"
		}
	}
}

function Get-Parent( $screen, [string]$name ) {
	foreach ($szContainer in $screen.Children.Keys) {
		if ($screen.Children[$szContainer] -contains $name) {
			return $szContainer
		}
	}
	return "(form)"
}

# the button bar of the form, or of a wizard page (a plain Panel on the form)
function Test-ButtonBarOrder( $screen ) {
	$client = $screen.FormProps["ClientSize"]
	if ($null -eq $client) {
		return
	}
	$clientSize = ConvertTo-Size $client
	$bars = @( [pscustomobject]@{ Container = "(form)"; Left = 12; Right = $clientSize.Width - 12 } )
	foreach ($name in @( $screen.Children["(form)"] | Where-Object { $_ } )) {
		if ($screen.Fields[$name] -eq "System.Windows.Forms.Panel") {
			$bounds = Get-Bounds $screen $name
			$bars += [pscustomobject]@{ Container = $name; Left = 0; Right = $bounds.Width }
		}
	}
	foreach ($bar in $bars) {
		$buttons = @( $screen.Children[$bar.Container] | Where-Object { $_ -and $screen.Fields[$_] -eq "CommonUI.Controls.ThemedButton" } |
			ForEach-Object { Get-Bounds $screen $_ } | Where-Object { $_.Height -eq 28 } | Sort-Object Left )
		if ($buttons.Count -eq 0) {
			continue
		}

		# chains from both margins; every button belongs to one of them
		$leftGroup = New-Object System.Collections.Generic.List[object]
		$nEdge = $bar.Left
		foreach ($button in $buttons) {
			if ($button.Left -ne $nEdge) {
				break
			}
			$leftGroup.Add($button)
			$nEdge = $button.Left + $button.Width + 8
		}
		$rightGroup = New-Object System.Collections.Generic.List[object]
		$nEdge = $bar.Right
		for ($i = $buttons.Count - 1; $i -ge 0; $i--) {
			$button = $buttons[$i]
			if ($button.Left + $button.Width -ne $nEdge -or $leftGroup.Contains($button)) {
				break
			}
			$rightGroup.Insert(0, $button)
			$nEdge = $button.Left - 8
		}
		foreach ($button in $buttons) {
			if (-not $leftGroup.Contains($button) -and -not $rightGroup.Contains($button)) {
				Report "S17" $screen.ClassName $button.Name "not chained 8 apart from the left or right margin of the button bar"
			}
		}

		# the main action is the rightmost, cancel / close right before it, reset / preview on the left
		$nIndex = 0
		foreach ($button in $buttons) {
			$props = $screen.Props[$button.Name]
			$isPrimary = $props["Kind"] -match "ButtonKind\.Primary$"
			$szCommon = $props["CommonText"]
			$isLast = $nIndex -eq $buttons.Count - 1
			if ($isPrimary -and -not $isLast) {
				Report "S17" $screen.ClassName $button.Name "the Primary button must be the rightmost of the button bar"
			}
			if ($szCommon -match "\.(Cancel|Close)$") {
				$next = if ($isLast) { $null } else { $buttons[$nIndex + 1] }
				$isBeforePrimary = $null -ne $next -and $screen.Props[$next.Name]["Kind"] -match "ButtonKind\.Primary$"
				if (-not $rightGroup.Contains($button) -or (-not $isLast -and -not $isBeforePrimary)) {
					Report "S17" $screen.ClassName $button.Name "cancel / close must be right before the Primary button (or the rightmost)"
				}
			}
			if ($szCommon -match "\.(Reset|Preview)$" -and -not $leftGroup.Contains($button)) {
				Report "S17" $screen.ClassName $button.Name "reset / preview are auxiliary actions, placed from the left margin"
			}
			$nIndex++
		}
	}
}

# S18 inside a card or a plain panel: first inputs of the rows aligned, units 6 after their input
function Test-CardAlignment( $screen ) {
	$containers = @( $screen.Fields.Keys | Where-Object { $screen.Fields[$_] -match "(CommonUI.Controls.SectionPanel|System.Windows.Forms.Panel)$" } )
	foreach ($szContainer in $containers) {
		$children = @( $screen.Children[$szContainer] | Where-Object { $_ } | ForEach-Object { Get-Bounds $screen $_ } | Where-Object { $_ } )
		# a multi-line text box is an output area, not an input row
		$inputs = @( $children | Where-Object { $screen.Fields[$_.Name] -match "(DebouncedNumericUpDown|ComboBox|TextBox)$" -and
			$screen.Props[$_.Name]["Multiline"] -ne "true" } )
		$rowLefts = @{}
		foreach ($input in $inputs) {
			$nRowTop = $input.Top
			if (-not $rowLefts.ContainsKey($nRowTop) -or $input.Left -lt $rowLefts[$nRowTop].Left) {
				$rowLefts[$nRowTop] = $input
			}
		}
		$firstInputs = @( $rowLefts.Values )
		$nCommonLeft = ($firstInputs | Group-Object Left | Sort-Object Count -Descending | Select-Object -First 1).Name
		foreach ($input in $firstInputs) {
			if ($null -ne $nCommonLeft -and $input.Left -ne [int]$nCommonLeft) {
				Report "S18" $screen.ClassName $input.Name "first input of the row at x=$($input.Left), the others of $szContainer at x=$nCommonLeft"
			}
		}
		foreach ($unit in @( $children | Where-Object { $_.Name -cmatch "Unit$" -and $screen.Props[$_.Name]["Role"] -match "TextRole\.Secondary$" } )) {
			$input = $inputs | Where-Object { $unit.Top -ge $_.Top -and $unit.Top -lt $_.Top + $_.Height -and $_.Left -lt $unit.Left } |
				Sort-Object { $_.Left + $_.Width } -Descending | Select-Object -First 1
			if ($null -eq $input) {
				continue
			}
			$nExpected = $input.Left + $input.Width + 6
			if ($unit.Left -ne $nExpected) {
				Report "S18" $screen.ClassName $unit.Name "unit label at x=$($unit.Left), expected 6 after $($input.Name) (x=$nExpected)"
			}
		}
	}
}

# button 23 high at y = 5, check box at y = 5, label 24 high at y = 4; the rightmost one ends at the content edge
function Test-TitleBar( $screen ) {
	foreach ($section in @( $screen.Fields.Keys | Where-Object { $screen.Fields[$_] -eq "CommonUI.Controls.SectionPanel" } )) {
		$sectionProps = $screen.Props[$section]
		$isTitled = $sectionProps -and $sectionProps.ContainsKey("Text") -and $sectionProps["Text"] -ne ""
		if (-not $isTitled) {
			continue
		}
		$sectionBounds = Get-Bounds $screen $section
		$items = @( $screen.Children[$section] | Where-Object { $_ } | ForEach-Object { Get-Bounds $screen $_ } |
			Where-Object { $_ -and $_.Top -lt $TITLE_BAR_HEIGHT } )
		foreach ($item in $items) {
			$szType = $screen.Fields[$item.Name]
			$isAutoSize = $screen.Props[$item.Name]["AutoSize"] -eq "true"
			if ($szType -match "ThemedButton$" -and ($item.Top -ne 5 -or $item.Height -ne 23)) {
				Report "S22" $screen.ClassName $item.Name "title bar button must be 23 high at y=5 (is y=$($item.Top), h=$($item.Height))"
			}
			elseif ($szType -match "CheckBox$" -and $item.Top -ne 5) {
				Report "S22" $screen.ClassName $item.Name "title bar check box must be at y=5 (is $($item.Top))"
			}
			elseif ($szType -match "Label$" -and ($item.Top -ne 4 -or (-not $isAutoSize -and $item.Height -ne 24))) {
				Report "S22" $screen.ClassName $item.Name "title bar label must be 24 high at y=4 (is y=$($item.Top), h=$($item.Height))"
			}
		}
		$rightmost = $items | Sort-Object { $_.Left + $_.Width } -Descending | Select-Object -First 1
		$isAutoSize = $rightmost -and $screen.Props[$rightmost.Name]["AutoSize"] -eq "true"
		if ($rightmost -and -not $isAutoSize -and $rightmost.Left + $rightmost.Width -ne $sectionBounds.Width - 10) {
			Report "S22" $screen.ClassName $rightmost.Name "title bar controls end at the content edge x=$($sectionBounds.Width - 10) (is $($rightmost.Left + $rightmost.Width))"
		}
	}
}

# ThemeMetrics.SECTION_TITLE_HEIGHT
$TITLE_BAR_HEIGHT = 32

$exceptions = @{}
$exceptionFile = Join-Path $PSScriptRoot "StaticExceptions.txt"
if (Test-Path $exceptionFile) {
	foreach ($line in Get-Content $exceptionFile -Encoding UTF8) {
		if ($line -match "^\s*(S\d+)\s+(\S+)\s+(\S+)") {
			$exceptions["$($Matches[1]) $($Matches[2]) $($Matches[3])"] = $true
		}
	}
}

$problems = New-Object System.Collections.Generic.List[string]
# S24 InitializeComponent contains only statements in the forms the designer writes, so that the designer can open
# the file: assignments and calls on "this", local variables it declares itself (cell styles, resources), casts for
# BeginInit / EndInit. Control flow, lambdas, interpolated strings, calls of the screen's own methods and constants
# of the class cannot be read by the designer.
function Test-DesignerCode( [string]$szClass, [string]$szText ) {
	$LAYOUT_CALL_LIST = @( "SuspendLayout", "ResumeLayout", "PerformLayout" )
	$statementList = Get-DesignerStatements $szText
	$localList = New-Object System.Collections.Generic.List[string]
	$fieldMap = @{}
	foreach ($match in [regex]::Matches($szText, "(?m)^\s*(?:(?:private|internal|public|protected)\s+)?(?:readonly\s+)?[\w.]+(?:<[^>]+>)?\s+(\w+)\s*;")) {
		$fieldMap[$match.Groups[1].Value] = $true
	}
	foreach ($s in $statementList) {
		# a closing brace left over from a block is reported with the statement that opened it
		$s = $s -replace "^[{}\s]+", ""
		if (-not $s) {
			continue
		}
		# string literals are not code
		$szCode = $s -replace '"(\\.|[^"\\])*"', '""'
		if ($szCode -match "^[\w.]+(<[\w.,\s]+>)? (\w+) = new ") {
			$localList.Add($Matches[2])
			continue
		}
		$isLocalUse = $szCode -match "^(\w+)\." -and $localList -contains $Matches[1]
		$isDesignerForm = $szCode -match "^this\." -or $szCode -match "^\(\(" -or $isLocalUse
		$szProblem = ""
		if (-not $isDesignerForm) {
			$szProblem = "not in a form the designer writes (control flow, a call without this., a field without this.)"
		}
		elseif ($szCode -match "=>|\bdelegate\b|\?\.|\?\?") {
			$szProblem = "lambda, anonymous method or null operator"
		}
		elseif ($s -match '\$"') {
			$szProblem = "interpolated string"
		}
		elseif ($szCode -match "(?<![\w.])[A-Z][A-Z0-9]*_[A-Z0-9_]+\b") {
			$szProblem = "constant of the class (write the value, the designer cannot read class members)"
		}
		else {
			foreach ($call in [regex]::Matches($szCode, "\bthis\.(\w+)\(")) {
				$szMethod = $call.Groups[1].Value
				if ($LAYOUT_CALL_LIST -notcontains $szMethod -and -not $fieldMap.ContainsKey($szMethod)) {
					$szProblem = "call of the screen's own method $szMethod()"
				}
			}
		}
		if ($szProblem) {
			$szShort = if ($s.Length -gt 100) { $s.Substring(0, 100) + " ..." } else { $s }
			Report "S24" $szClass "InitializeComponent" "$szProblem : $szShort"
		}
	}
}

function Report( [string]$szRule, [string]$szClass, [string]$szControl, [string]$szMessage ) {
	# an exception may use wildcards, e.g. "B6 CalibrationREFDlg m_tbxPnt*"
	$key = "$szRule $szClass $szControl"
	if (@($exceptions.Keys | Where-Object { $key -like $_ }).Count -gt 0) {
		return
	}
	$problems.Add("$szRule $szClass.$szControl $szMessage")
}

function Get-LanguageIds( [string]$szName, [string]$szLanguage ) {
	$file = Join-Path $Root "Language\$($szName)_$szLanguage.xml"
	if (-not (Test-Path $file)) {
		return $null
	}
	[xml]$xml = Get-Content $file -Raw -Encoding UTF8
	$ids = @{}
	foreach ($node in $xml.ResMap.Message) {
		$ids[$node.ID] = $node.Content
	}
	return $ids
}

function Test-HasChinese( [string]$szText ) {
	return $szText -match "[一-鿿]"
}

# ---------------------------------------------------------------- language files
function Test-ScreenLanguage( $screen, $file ) {
	$szClass = $screen.ClassName
	$codeFile = $file.FullName -replace "\.Designer\.cs$", ".cs"
	$code = if (Test-Path $codeFile) { Get-Content $codeFile -Raw -Encoding UTF8 } else { "" }

	# language file named after the class
	$fileNames = [regex]::Matches($code, 'new LanguageManager\.LanguageManager\(\s*"(\w+)"\s*\)') |
		ForEach-Object { $_.Groups[1].Value }
	foreach ($szName in $fileNames) {
		if ($szName -ne $szClass) {
			Report "S6" $szClass "*" "language file '$szName' is not named after the class"
		}
	}
	$idsByLanguage = @{}
	foreach ($szLanguage in $LANGUAGES) {
		$idsByLanguage[$szLanguage] = Get-LanguageIds $szClass $szLanguage
	}
	$zhTW = $idsByLanguage["zh-TW"]
	$zhCN = $idsByLanguage["zh-CN"]

	# captions written in Chinese need a translation, or zh-CN shows the zh-TW designer text
	$captions = @()
	$isTitleSetInCode = $code -match "(?m)^\s*(this\.)?Text\s*="
	if (-not $isTitleSetInCode -and (Test-HasChinese $screen.FormProps["Text"])) {
		$captions += [pscustomobject]@{ Name = $szClass; Text = $screen.FormProps["Text"] }
	}
	foreach ($name in $screen.Fields.Keys) {
		$szType = $screen.Fields[$name]
		$isCaption = $szType -match "(Label|CheckBox|RadioButton|ThemedButton|SectionPanel)$"
		if (-not $isCaption -or -not $screen.Props.ContainsKey($name)) {
			continue
		}
		$props = $screen.Props[$name]
		if ($props.ContainsKey("CommonText")) {
			continue
		}
		# a caption whose text is set in code (shared or runtime text) only has a placeholder in the designer:
		# directly, or through an array / a helper parameter that the control is passed in
		$isSetDirectly = $code -match "\b$name\.Text\s*="
		$isPassedOn = $code -match "[\{\(,]\s*$name\s*[,\}\)]" -and $code -match "(?<![\w\.])(?!m_)[a-z]\w*(\[[^\]]*\])?\.Text\s*="
		$isSetInCode = $isSetDirectly -or $isPassedOn
		if (-not $isSetInCode -and $props.ContainsKey("Text") -and (Test-HasChinese $props["Text"])) {
			$captions += [pscustomobject]@{ Name = $name; Text = $props["Text"] }
		}
	}
	foreach ($caption in $captions) {
		foreach ($szLanguage in @( "zh-TW", "zh-CN" )) {
			$ids = $idsByLanguage[$szLanguage]
			if ($null -eq $ids -or -not $ids.ContainsKey($caption.Name)) {
				Report "S6" $szClass $caption.Name "caption '$($caption.Text)' has no ID in $($szClass)_$szLanguage.xml"
			}
		}
	}

	# the four languages have the same IDs
	if ($null -ne $zhTW) {
		foreach ($szLanguage in $LANGUAGES) {
			$ids = $idsByLanguage[$szLanguage]
			if ($null -eq $ids) {
				Report "S6" $szClass "*" "missing $($szClass)_$szLanguage.xml"
				continue
			}
			foreach ($id in $zhTW.Keys) {
				if (-not $ids.ContainsKey($id)) {
					Report "S6" $szClass $id "ID missing in $szLanguage"
				}
			}
			foreach ($id in $ids.Keys) {
				if (-not $zhTW.ContainsKey($id)) {
					Report "S6" $szClass $id "ID only in $szLanguage"
				}
			}
		}
	}

	# S7 a control whose name is an ID gets its text from ApplyLanguage(); code does not apply it again by hand
	if ($null -ne $zhTW) {
		foreach ($name in $screen.Fields.Keys) {
			if (-not $zhTW.ContainsKey($name)) {
				continue
			}
			$pattern = "\b$name\.(Text|HeaderText)\s*=\s*[\w\.]+\.TryGetString\(\s*`"$name`""
			if ($code -match $pattern) {
				Report "S7" $szClass $name "text is applied by hand; ApplyLanguage() applies it by the control name"
			}
		}
	}
}

# ---------------------------------------------------------------- layout
function Test-DialogLayout( $screen ) {
	$szClass = $screen.ClassName
	$client = $screen.FormProps["ClientSize"]
	if ($null -eq $client) {
		Report "S5" $szClass "(form)" "no ClientSize"
		return
	}
	$clientSize = ConvertTo-Size $client
	$topLevel = @( $screen.Children["(form)"] | Where-Object { $_ } )

	# a wizard that shows one page at a time (SectionStack.ShowPage): each page at (12, 12) is laid out like a dialog
	# without margins, and the dialog is fitted to it at runtime
	$isPaged = $script:CurrentCode -match "SectionStack\.ShowPage\("
	if ($isPaged) {
		foreach ($name in $topLevel) {
			$page = Get-Bounds $screen $name
			if ($screen.Fields[$name] -ne "System.Windows.Forms.Panel") {
				Report "S5" $szClass $name "a paged dialog holds only its pages (Panel) on the form"
				continue
			}
			if ($page.Left -ne 12 -or $page.Top -ne 12) {
				Report "S5" $szClass $name "page must be at (12, 12) (is $($page.Left), $($page.Top))"
			}
			Test-CardColumn $screen $name 0 0 $page.Width $page.Height
		}
		return
	}
	Test-CardColumn $screen "(form)" 12 12 ($clientSize.Width - 24) ($clientSize.Height - 12)
	foreach ($name in $topLevel) {
		$szType = $screen.Fields[$name]
		if ($szType -ne "CommonUI.Controls.SectionPanel" -and $szType -ne "CommonUI.Controls.ThemedButton" -and
			$szType -ne "System.Windows.Forms.Panel") {
			Report "S5" $szClass $name "only sections, panels and the button bar are placed on the form"
		}
	}
}

# cards of a container (the form, or a wizard page): one column from the left edge, full width, 8 apart; two cards of
# the same row may stand side by side (equal width, 8 apart); cards at the same location are steps shown one at a
# time and share the height of the tallest; then the button bar 16 below, 28 high, ending at the bottom edge
function Test-CardColumn( $screen, [string]$szContainer, [int]$nLeft, [int]$nTop, [int]$nWidth, [int]$nBottom ) {
	$szClass = $screen.ClassName
	$children = @( $screen.Children[$szContainer] | Where-Object { $_ } )
	$sections = @( $children | Where-Object { $screen.Fields[$_] -match "(CommonUI.Controls.SectionPanel|System.Windows.Forms.Panel)$" } |
		ForEach-Object { Get-Bounds $screen $_ } )
	$buttons = @( $children | Where-Object { $screen.Fields[$_] -eq "CommonUI.Controls.ThemedButton" } |
		ForEach-Object { Get-Bounds $screen $_ } )

	# a dialog that stacks its cards at runtime (SectionStack.Arrange) is checked in each state by the runtime check
	$isStacked = $script:CurrentCode -match "SectionStack\.Arrange\("
	$nExpectedTop = $nTop
	$contentBottom = $nTop
	foreach ($rowGroup in @( $sections | Group-Object Top | Sort-Object { [int]$_.Name } )) {
		$rowCards = @( $rowGroup.Group | Sort-Object Left )
		$slots = @( $rowCards | Group-Object Left | Sort-Object { [int]$_.Name } )

		# horizontal: one full-width slot, or two equal slots side by side
		if ($slots.Count -eq 1) {
			foreach ($card in $rowCards) {
				if ($card.Left -ne $nLeft -or $card.Width -ne $nWidth) {
					Report "S5" $szClass $card.Name "section must be at x=$nLeft with width $nWidth (is x=$($card.Left), w=$($card.Width))"
				}
			}
		}
		else {
			$nHalf = ($nWidth - 8) / 2
			$isPair = $slots.Count -eq 2 -and [int]$slots[0].Name -eq $nLeft -and [int]$slots[1].Name -eq ($nLeft + $nHalf + 8) -and
				@( $rowCards | Where-Object { $_.Width -ne $nHalf } ).Count -eq 0
			if (-not $isPair) {
				Report "S5" $szClass $rowCards[0].Name "side-by-side sections must be two of width $nHalf, 8 apart, filling the width $nWidth"
			}
		}

		# vertical: 8 below the previous row
		if ($rowCards[0].Top -ne $nExpectedTop -and -not $isStacked) {
			Report "S5" $szClass $rowCards[0].Name "section must be at y=$nExpectedTop (is $($rowCards[0].Top))"
		}

		# cards of a slot alternate (one step at a time) and have the height of the tallest content
		$nRowHeight = 0
		foreach ($slot in $slots) {
			$expectedMap = @{}
			foreach ($card in $slot.Group) {
				# a plain Panel (radio group, runtime area) has no card height of its own
				$expectedMap[$card.Name] = if ($screen.Fields[$card.Name] -eq "CommonUI.Controls.SectionPanel") {
					Test-SectionRows $screen $card.Name 36 -NoHeightCheck
				}
				else {
					$card.Height
				}
			}
			$nSlotHeight = ($expectedMap.Values | Measure-Object -Maximum).Maximum
			foreach ($card in $slot.Group) {
				$isCard = $screen.Fields[$card.Name] -eq "CommonUI.Controls.SectionPanel"
				if ($isCard -and $card.Height -ne $nSlotHeight) {
					$szWhy = if ($slot.Group.Count -gt 1) { " (the tallest of the steps at this location)" } else { "" }
					Report "S5" $szClass $card.Name "section height must be $nSlotHeight$szWhy (is $($card.Height))"
				}
			}
			$nRowHeight = [math]::Max($nRowHeight, $nSlotHeight)
		}

		# side-by-side cards share the row height
		if ($slots.Count -gt 1) {
			foreach ($card in $rowCards) {
				if ($card.Height -ne $nRowHeight) {
					Report "S5" $szClass $card.Name "side-by-side sections share the height $nRowHeight (is $($card.Height))"
				}
			}
		}
		$nExpectedTop = $rowCards[0].Top + $nRowHeight + 8
		$contentBottom = [math]::Max($contentBottom, $rowCards[0].Top + $nRowHeight)
	}

	# button bar
	foreach ($button in $buttons) {
		if ($button.Height -ne 28 -or $button.Width -lt 80) {
			Report "S5" $szClass $button.Name "button bar button must be 28 high and at least 80 wide (is $($button.Width)x$($button.Height))"
		}
		if ($button.Top -ne ($contentBottom + 16) -and -not $isStacked) {
			Report "S5" $szClass $button.Name "button bar must be 16 below the content, y=$($contentBottom + 16) (is $($button.Top))"
		}
		if ($button.Top + $button.Height -ne $nBottom) {
			Report "S5" $szClass $button.Name "button bar must end at y=$nBottom (12 above the dialog bottom)"
		}
		if ($button.Left + $button.Width -gt $nLeft + $nWidth -or $button.Left -lt $nLeft) {
			Report "S5" $szClass $button.Name "button outside the margins"
		}
	}
}

function Test-SectionRows( $screen, [string]$szContainer, [int]$nTitledTop, [switch]$NoHeightCheck ) {
	$szClass = $screen.ClassName
	$props = $screen.Props[$szContainer]
	$isTitled = $props -and $props.ContainsKey("Text") -and $props["Text"] -ne ""
	$isSection = $screen.Fields[$szContainer] -eq "CommonUI.Controls.SectionPanel"
	$contentTop = if (-not $isSection) { 0 } elseif ($isTitled) { $nTitledTop } else { 10 }
	$lastRowBottom = 0
	foreach ($name in @( $screen.Children[$szContainer] | Where-Object { $_ } )) {
		$bounds = Get-Bounds $screen $name
		if ($null -eq $bounds) {
			continue
		}
		$szType = $screen.Fields[$name]
		$offset = switch -Regex ($szType) {
			"(DebouncedNumericUpDown|ComboBox|TextBox|ThemedButton|Panel|PictureBox|CheckedListBox|DataGridView|TrackBar)$" { 0 }
			"(Label|ThemedLabel)$" { 4 }
			"(CheckBox|RadioButton)$" { 2 }
			default { $null }
		}
		if ($null -eq $offset) {
			continue
		}
		$isMultiLine = $screen.Props[$name] -and $screen.Props[$name]["Multiline"] -eq "true"
		$isAutoLabel = $offset -eq 4 -and $screen.Props[$name] -and $screen.Props[$name]["AutoSize"] -eq "true"
		if ($offset -eq 4 -and -not $isAutoLabel) {
			# fixed-size labels (coordinate read-outs) sit on the input grid like inputs
			$offset = if (($bounds.Top - $contentTop) % 30 -eq 0) { 0 } else { 4 }
		}
		$row = ($bounds.Top - $offset - $contentTop) / 30
		if ($row -ne [math]::Floor($row) -or $row -lt 0) {
			Report "S5" $szClass $name "y=$($bounds.Top) is off the row grid of $szContainer (rows at $contentTop + 30k, +$offset)"
		}
		if ($szType -match "Panel$" -and $szType -notmatch "SectionPanel") {
			Test-SectionRows $screen $name $nTitledTop
		}
		if (-not $isMultiLine -and $szType -match "(DebouncedNumericUpDown|TextBox)$" -and $bounds.Height -ne 23) {
			Report "S5" $szClass $name "input height must be 23 (is $($bounds.Height))"
		}
		$rowBottom = [math]::Max($bounds.Top + $bounds.Height, $contentTop + [math]::Floor($row) * 30 + 23)
		if ($rowBottom -gt $lastRowBottom) {
			$lastRowBottom = $rowBottom
		}
	}

	# card height: last row + 10 (returned to the caller that compares cards sharing a location)
	$nExpectedHeight = $lastRowBottom + 10
	if ($NoHeightCheck) {
		return $nExpectedHeight
	}
	if ($isSection) {
		$section = Get-Bounds $screen $szContainer
		if ($lastRowBottom -gt 0 -and $section.Height -ne $nExpectedHeight) {
			Report "S5" $szClass $szContainer "section height must be $nExpectedHeight (is $($section.Height))"
		}
	}
}

# ---------------------------------------------------------------- designer files
$designerFiles = foreach ($dir in $SCREEN_DIRS) {
	Get-ChildItem (Join-Path $Root $dir) -Filter "*.Designer.cs" -Recurse
}
foreach ($file in $designerFiles) {
	$screen = Read-DesignerFile $file.FullName
	if ($Only -and $screen.ClassName -notlike $Only) {
		continue
	}
	$szClass = $screen.ClassName
	$isDialog = $screen.BaseType -match "^(DialogBase|FormBase|EditDialogBase<.+>)$"
	$isPanel = $screen.BaseType -match "^(PanelBase|UserControl)$"
	$codePath = $file.FullName -replace "\.Designer\.cs$", ".cs"
	$script:CurrentCode = if (Test-Path $codePath) { Get-Content $codePath -Raw -Encoding UTF8 } else { "" }

	# S3 base class
	if (-not $isDialog -and -not $isPanel) {
		Report "S3" $szClass "*" "derives from $($screen.BaseType), expected DialogBase / FormBase / EditDialogBase"
	}
	if ($screen.BaseType -eq "UserControl") {
		Report "S3" $szClass "*" "derives from UserControl, expected PanelBase"
	}

	# S1 control types
	foreach ($name in $screen.Fields.Keys) {
		$szType = $screen.Fields[$name]
		if ($szType -eq "System.Windows.Forms.TextBox") {
			if ($ALLOWED_TEXTBOX -notcontains "$szClass.$name") {
				Report "S1" $szClass $name "TextBox is only for non-numeric text (use DebouncedNumericUpDown or Label)"
			}
		}
		elseif ($ALLOWED_TYPES -notcontains $szType) {
			Report "S1" $szClass $name "type $szType is not in the white list"
		}
	}

	# S2 appearance in the designer file
	foreach ($prop in $FORBIDDEN_FORM_PROPS) {
		if ($screen.FormProps.ContainsKey($prop)) {
			Report "S2" $szClass "(form)" "sets $prop"
		}
	}
	foreach ($name in $screen.Props.Keys) {
		foreach ($prop in $FORBIDDEN_CONTROL_PROPS) {
			# embedded panels are sized by the main window and may stretch their content
			if ($isPanel -and ($prop -eq "Anchor" -or $prop -eq "Dock")) {
				continue
			}
			if ($screen.Props[$name].ContainsKey($prop)) {
				Report "S2" $szClass $name "sets $prop"
			}
		}
	}

	# S8 an emphasized label at the top of a card stands in for the card title, which is SectionPanel.Text
	# above the content grid: the title bar of a titled card, above the first label row (10 + 4) of an untitled one
	foreach ($section in @( $screen.Fields.Keys | Where-Object { $screen.Fields[$_] -eq "CommonUI.Controls.SectionPanel" } )) {
		$sectionProps = $screen.Props[$section]
		$isTitled = $sectionProps -and $sectionProps.ContainsKey("Text") -and $sectionProps["Text"] -ne ""
		$nTitleLimit = if ($isTitled) { $TITLE_BAR_HEIGHT } else { 14 }
		foreach ($name in @( $screen.Children[$section] | Where-Object { $_ } )) {
			$bounds = Get-Bounds $screen $name
			$props = $screen.Props[$name]
			$isLabel = $screen.Fields[$name] -match "(Label|ThemedLabel)$"
			$isEmphasis = $props -and $props["Role"] -match "TextRole\.(Emphasis|Title)$"
			if ($isLabel -and $isEmphasis -and $bounds -and $bounds.Top -lt $nTitleLimit) {
				Report "S8" $szClass $name "emphasized label above the content of $section; use $section.Text as the title"
			}
		}
	}

	# S5 / S17 layout of dialogs, S14 language call
	if ($isDialog) {
		Test-DialogLayout $screen
		Test-ButtonBarOrder $screen
		Test-ApplyLanguage $screen ($file.FullName -replace "\.Designer\.cs$", ".cs")
	}

	# S14 an embedded panel with a language file applies it through PanelBase.ApplyLanguage
	$hasLanguageFile = Test-Path (Join-Path $Root "Language\$($szClass)_zh-TW.xml")
	if ($isPanel -and $hasLanguageFile -and $script:CurrentCode -notmatch "\bApplyLanguage\(\s*\w+\s*\)") {
		Report "S14" $szClass "*" "an embedded panel applies its language texts with ApplyLanguage( languageManager )"
	}

	# S16 button kinds, S18 alignment in cards (embedded panels are scaled, their grid is checked at runtime)
	Test-ButtonKinds $screen ($file.FullName -match "\\CncUI\\Panel\\")
	if ($isDialog) {
		Test-CardAlignment $screen
	}

	# S22 controls in the title bar of a titled card (section 4.5)
	Test-TitleBar $screen

	# S6 / S7 language
	Test-ScreenLanguage $screen $file

	# S9 names, S10 tab order, S13 language IDs
	Test-FieldNames $screen
	Test-TabOrder $screen
	Test-OrphanIds $screen ($file.FullName -replace "\.Designer\.cs$", ".cs")

	# S24 the designer can read InitializeComponent
	Test-DesignerCode $szClass (Get-Content $file.FullName -Raw -Encoding UTF8)
}

# S9 the main window follows the same naming; its layout is computed by MainLayout, so S10 does not apply
$mainScreen = Read-DesignerFile (Join-Path $Root "MyCAM\StartupForm.Designer.cs")
if (-not $Only -or $mainScreen.ClassName -like $Only) {
	Test-FieldNames $mainScreen
	Test-OrphanIds $mainScreen (Join-Path $Root "MyCAM\StartupForm.cs")
	$szMainText = Get-Content (Join-Path $Root "MyCAM\StartupForm.Designer.cs") -Raw -Encoding UTF8
	Test-DesignerCode $mainScreen.ClassName $szMainText

	# S19 types and appearance of the main window
	foreach ($name in $mainScreen.Fields.Keys) {
		$szType = $mainScreen.Fields[$name]
		if ($ALLOWED_TYPES -notcontains $szType -and $MAIN_WINDOW_TYPES -notcontains $szType) {
			Report "S19" $mainScreen.ClassName $name "type $szType is not in the white list of the main window"
		}
	}
	foreach ($prop in $FORBIDDEN_FORM_PROPS) {
		if ($mainScreen.FormProps.ContainsKey($prop)) {
			Report "S19" $mainScreen.ClassName "(form)" "sets $prop"
		}
	}
	foreach ($name in $mainScreen.Props.Keys) {
		foreach ($prop in @( "Font", "BackColor", "ForeColor", "FlatStyle", "UseVisualStyleBackColor", "ImageTransparentColor" )) {
			if ($mainScreen.Props[$name].ContainsKey($prop)) {
				Report "S19" $mainScreen.ClassName $name "sets $prop"
			}
		}
	}
}

# S21 the component catalog of the guideline lists every public type of CommonUI, in the subsection of its folder;
# the folder is the namespace (CommonUI\<Folder> is CommonUI.<Folder>)
$guideline = Get-Content (Join-Path $Root ".github\instructions\ui-style.instructions.md") -Raw -Encoding UTF8
$catalogIndex = $guideline.IndexOf("## 9. CommonUI")
$catalog = if ($catalogIndex -ge 0) { $guideline.Substring($catalogIndex) } else { "" }

# each subsection names its namespace in the heading: "### 9.n <title>（`CommonUI.<Folder>`）"
$catalogSectionMap = @{}
foreach ($section in [regex]::Split($catalog, "(?m)^(?=### )")) {
	if ($section -match "^### [^\r\n]*``(CommonUI\.\w+)``") {
		$catalogSectionMap[$Matches[1]] = $section
	}
}
$commonUIRoot = Join-Path $Root "CommonUI"
foreach ($file in Get-ChildItem $commonUIRoot -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\(bin|obj|Properties)\\" -and $_.Name -notlike "*.Designer.cs" }) {
	$code = Get-Content $file.FullName -Raw -Encoding UTF8
	$szFolder = $file.DirectoryName.Substring($commonUIRoot.Length).Trim("\")
	$szNamespace = "CommonUI.$szFolder"
	if ($code -notmatch "(?m)^namespace $([regex]::Escape($szNamespace))\s*$") {
		Report "S21" $file.Name "*" "a file in CommonUI\$szFolder must be in namespace $szNamespace"
	}
	if (-not $catalogSectionMap.ContainsKey($szNamespace)) {
		Report "S21" $file.Name "*" "the component catalog (guideline section 9) has no subsection for $szNamespace"
		continue
	}
	foreach ($match in [regex]::Matches($code, "public (static |sealed |partial |abstract )*(class|enum|struct|interface) (\w+)")) {
		$szType = $match.Groups[3].Value
		if ($catalogSectionMap[$szNamespace] -notmatch "``$szType``") {
			Report "S21" $szType "*" "public CommonUI type is not listed in the catalog subsection of $szNamespace (guideline section 9)"
		}
	}
}

# S15 control classes are defined in CommonUI only
$CONTROL_BASES = "Control|UserControl|ScrollableControl|ContainerControl|Panel|Button|ButtonBase|Label|TextBox|TextBoxBase|" +
	"NumericUpDown|UpDownBase|ComboBox|ListBox|CheckedListBox|ListView|TreeView|DataGridView|CheckBox|RadioButton|" +
	"GroupBox|PictureBox|ProgressBar|TrackBar|TabControl|SplitContainer|ToolStrip|MenuStrip|StatusStrip|FlowLayoutPanel|" +
	"TableLayoutPanel|ToolStripItem|ToolStripButton|ToolStripMenuItem|ToolStripControlHost|DataGridViewColumn|DataGridViewCell"
foreach ($dir in @( "MyCAM", "CncUI", "OCCViewer" )) {
	$path = Join-Path $Root $dir
	if (-not (Test-Path $path)) {
		continue
	}
	foreach ($file in Get-ChildItem $path -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" }) {
		$code = Get-Content $file.FullName -Raw -Encoding UTF8
		foreach ($match in [regex]::Matches($code, "class\s+(\w+)\s*:\s*(System\.Windows\.Forms\.)?($CONTROL_BASES)\b")) {
			Report "S15" $match.Groups[1].Value "*" "derives from $($match.Groups[3].Value); controls are defined in CommonUI"
		}
	}
}

# ---------------------------------------------------------------- code
$codeFiles = foreach ($scope in $CODE_SCOPE) {
	$path = Join-Path $Root $scope
	if (Test-Path $path -PathType Leaf) {
		Get-Item $path
	}
	else {
		Get-ChildItem $path -Filter "*.cs" -Recurse
	}
}

# designer files are checked by S2
$codeFiles = $codeFiles | Where-Object { $_.Name -notlike "*.Designer.cs" }
foreach ($file in $codeFiles) {
	$szClass = [IO.Path]::GetFileNameWithoutExtension($file.FullName)
	if ($Only -and $szClass -notlike $Only) {
		continue
	}
	# a dialog's code-behind: its designer file is in a screen folder and it is not an embedded panel
	$designerPath = $file.FullName -replace "\.cs$", ".Designer.cs"
	$isScreenFolder = @( $SCREEN_DIRS | Where-Object { $file.DirectoryName -eq (Join-Path $Root $_) } ).Count -gt 0
	$isDialogCode = $isScreenFolder -and (Test-Path $designerPath) -and ($file.FullName -notmatch "\\CncUI\\Panel\\")
	$nLine = 0
	foreach ($line in Get-Content $file.FullName -Encoding UTF8) {
		$nLine++
		if ($line -match "\bDeviceDpi\b") {
			Report "S12" $szClass "line$nLine" "DPI scaling is not used (96 DPI only): $($line.Trim())"
		}
		if ($line -match "\bnew (System\.Windows\.Forms\.)?(Button|Label|GroupBox|NumericUpDown|FlowLayoutPanel|TableLayoutPanel|TextBox|ListBox|SplitContainer|TabControl|ProgressBar|DataGridView)\s*[\(\{]") {
			Report "S23" $szClass "line$nLine" "control type outside the white list created in code: $($line.Trim())"
		}
		$isAppearanceCode = $line -match "(\bm_\w+\.|\bthis\.|^\s*)(Font|ForeColor|BackColor|Location|Size|ClientSize|Left|Top|Width|Height|Bounds)\s*=[^=]|\bm_\w+\.SetBounds\("
		if ($isDialogCode -and $isAppearanceCode) {
			Report "S11" $szClass "line$nLine" "appearance set in code; lay it out in the designer: $($line.Trim())"
		}
		# Color.Empty (default) and Color.Transparent (no fill) are not colors and are allowed
		if ($line -notmatch "(?<![\w])(Color\.(FromArgb|(?!(Empty|Transparent)\b)[A-Z]\w*)|SystemColors\.\w+|new (System\.Drawing\.)?Font\s*\()") {
			continue
		}

		# data colors are allowed as named constants registered in StaticExceptions.txt (S20)
		if ($line -match "static readonly (System\.Drawing\.)?Color ([A-Z][A-Z0-9_]+) =") {
			Report "S20" $szClass $Matches[2] "data color constant not registered: $($line.Trim())"
			continue
		}
		Report "S4" $szClass "line$nLine" "hard-coded color or font: $($line.Trim())"
	}
}

# ---------------------------------------------------------------- result
foreach ($problem in $problems) {
	Write-Host "FAIL  $problem"
}
Write-Host "[static] $($designerFiles.Count) designer files, $($problems.Count) problems"
if ($problems.Count -gt 0) {
	exit 1
}
exit 0
