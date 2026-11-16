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
#   S6  language: captions written in Chinese have their ID in zh-TW and zh-CN; the four languages have the same
#       IDs; the language file is named after the class
#   S7  language: a control whose name is an ID is not given another text in code (auto-applied texts would win)
#   S8  section titles (section 4.5): a card title is SectionPanel.Text; no emphasized label in a card's title bar
#
# Exceptions are listed in StaticExceptions.txt: "<rule> <class> <control> <reason>". Exit code 1 on a problem.

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DesignerParser.ps1")

$SCREEN_DIRS = @( "MyCAM\Editor\Dialog", "MyCAM\CNC", "CncUI\Panel", "CncUI\Dialog" )
$CODE_SCOPE = @( "MyCAM\Editor\Dialog", "MyCAM\Editor\Action", "MyCAM\Editor\ViewManager.cs", "MyCAM\LogManager",
	"MyCAM\StartupForm.cs", "MyCAM\Layout", "MyCAM\CNC", "CncUI", "CommonUI\Controls", "CommonUI\Forms" )
$LANGUAGES = @( "zh-TW", "zh-CN", "en-US", "ru-RU" )

$ALLOWED_TYPES = @(
	"CommonUI.Controls.SectionPanel", "CommonUI.Controls.ThemedButton", "CommonUI.Controls.ThemedLabel",
	"CommonUI.Controls.DebouncedNumericUpDown", "CommonUI.Controls.MultiSelectTreeView",
	"CommonUI.Controls.LoadingBar", "CommonUI.Controls.RoundedProgressBar", "CommonUI.Controls.ThemedDataGridView",
	"System.Windows.Forms.Label", "System.Windows.Forms.CheckBox", "System.Windows.Forms.RadioButton",
	"System.Windows.Forms.ComboBox", "System.Windows.Forms.CheckedListBox", "System.Windows.Forms.TrackBar",
	"System.Windows.Forms.PictureBox", "System.Windows.Forms.DataGridView",
	"System.Windows.Forms.DataGridViewTextBoxColumn", "System.Windows.Forms.Panel",
	"System.Windows.Forms.ToolTip", "System.Windows.Forms.Timer", "System.ComponentModel.IContainer" )

# non-numeric text fields (section 3)
$ALLOWED_TEXTBOX = @( "ConnectDlg.m_txbIP", "MeasureDlg.m_txtResult", "ExtendedGCodeDlg.m_txtGCode",
	"DesignSupportBracketDlg.m_txbSavePath", "GlassDXFImportDlg.m_txbFilePath" )

$FORBIDDEN_FORM_PROPS = @( "AutoScaleMode", "AutoScaleDimensions", "FormBorderStyle", "StartPosition", "MaximizeBox",
	"MinimizeBox", "ShowIcon", "ShowInTaskbar", "Font", "BackColor", "ForeColor" )
$FORBIDDEN_CONTROL_PROPS = @( "Font", "BackColor", "ForeColor", "FlatStyle", "UseVisualStyleBackColor", "Dock",
	"Anchor", "AutoScaleMode", "AutoScaleDimensions", "BorderStyle", "ImageTransparentColor" )

# ThemeMetrics.SECTION_TITLE_HEIGHT
$TITLE_BAR_HEIGHT = 32

$exceptions = @{}
$exceptionFile = Join-Path $PSScriptRoot "StaticExceptions.txt"
if (Test-Path $exceptionFile) {
	foreach ($line in Get-Content $exceptionFile -Encoding UTF8) {
		if ($line -match "^\s*(S\d)\s+(\S+)\s+(\S+)") {
			$exceptions["$($Matches[1]) $($Matches[2]) $($Matches[3])"] = $true
		}
	}
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
	if (Test-HasChinese $screen.FormProps["Text"]) {
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
		if ($props.ContainsKey("Text") -and (Test-HasChinese $props["Text"])) {
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

	# S7 a control whose name is an ID gets its text automatically; code must not set another one
	if ($null -ne $zhTW) {
		foreach ($name in $screen.Fields.Keys) {
			if (-not $zhTW.ContainsKey($name)) {
				continue
			}
			$pattern = "\b$name\.Text\s*=\s*(?!m_LanguageManager\.TryGetString\(\s*`"$name`")"
			if ($code -match $pattern) {
				Report "S7" $szClass $name "text is set in code although '$name' is an auto-applied ID"
			}
			$pattern = "\b$name\.Text\s*=\s*m_LanguageManager\.TryGetString\(\s*`"$name`""
			if ($code -match $pattern) {
				Report "S7" $szClass $name "text is applied by hand; DialogBase applies it automatically"
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
	$sections = @( $topLevel | Where-Object { $screen.Fields[$_] -match "(CommonUI.Controls.SectionPanel|System.Windows.Forms.Panel)$" } |
		ForEach-Object { Get-Bounds $screen $_ } | Sort-Object Top )
	$buttons = @( $topLevel | Where-Object { $screen.Fields[$_] -eq "CommonUI.Controls.ThemedButton" } |
		ForEach-Object { Get-Bounds $screen $_ } )

	# sections: one column from the margin, full width, 8 apart
	$previous = $null
	foreach ($section in $sections) {
		if ($section.Left -ne 12 -or $section.Width -ne ($clientSize.Width - 24)) {
			Report "S5" $szClass $section.Name "section must be at x=12 with width $($clientSize.Width - 24) (is x=$($section.Left), w=$($section.Width))"
		}
		$expectedTop = if ($null -eq $previous) { 12 } else { $previous.Top + $previous.Height + 8 }
		if ($section.Top -ne $expectedTop -and -not (Test-RuntimeStacked $screen $section.Name)) {
			Report "S5" $szClass $section.Name "section must be at y=$expectedTop (is $($section.Top))"
		}
		if ($screen.Fields[$section.Name] -eq "CommonUI.Controls.SectionPanel") {
			Test-SectionRows $screen $section.Name 36
		}
		$previous = $section
	}

	# button bar
	$contentBottom = if ($sections.Count -gt 0) { ($sections | ForEach-Object { $_.Top + $_.Height } | Measure-Object -Maximum).Maximum } else { 12 }
	foreach ($button in $buttons) {
		if ($button.Height -ne 28 -or $button.Width -lt 80) {
			Report "S5" $szClass $button.Name "button bar button must be 28 high and at least 80 wide (is $($button.Width)x$($button.Height))"
		}
		if ($button.Top -ne ($contentBottom + 16) -and -not (Test-RuntimeStacked $screen $button.Name)) {
			Report "S5" $szClass $button.Name "button bar must be 16 below the content, y=$($contentBottom + 16) (is $($button.Top))"
		}
		if ($button.Top + $button.Height + 12 -ne $clientSize.Height) {
			Report "S5" $szClass $button.Name "button bar must be 12 above the bottom (client height $($clientSize.Height))"
		}
		if ($button.Left + $button.Width -gt $clientSize.Width - 12 -or $button.Left -lt 12) {
			Report "S5" $szClass $button.Name "button outside the margins"
		}
	}
	foreach ($name in $topLevel) {
		$szType = $screen.Fields[$name]
		if ($szType -ne "CommonUI.Controls.SectionPanel" -and $szType -ne "CommonUI.Controls.ThemedButton" -and
			$szType -ne "System.Windows.Forms.Panel") {
			Report "S5" $szClass $name "only sections, panels and the button bar are placed on the form"
		}
	}
}

# sections placed by SectionStack at runtime are exempt from the designer position
function Test-RuntimeStacked( $screen, [string]$name ) {
	return $exceptions.ContainsKey("S5 $($screen.ClassName) $name")
}

function Test-SectionRows( $screen, [string]$szContainer, [int]$nTitledTop ) {
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

	# card height: last row + 10
	if ($isSection) {
		$section = Get-Bounds $screen $szContainer
		if ($lastRowBottom -gt 0 -and $section.Height -ne ($lastRowBottom + 10)) {
			Report "S5" $szClass $szContainer "section height must be $($lastRowBottom + 10) (is $($section.Height))"
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

	# S5 layout of dialogs
	if ($isDialog) {
		Test-DialogLayout $screen
	}

	# S6 / S7 language
	Test-ScreenLanguage $screen $file
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
	$nLine = 0
	foreach ($line in Get-Content $file.FullName -Encoding UTF8) {
		$nLine++
		# Color.Empty (default) and Color.Transparent (no fill) are not colors and are allowed
		if ($line -notmatch "(?<![\w])(Color\.(FromArgb|(?!(Empty|Transparent)\b)[A-Z]\w*)|SystemColors\.\w+|new (System\.Drawing\.)?Font\s*\()") {
			continue
		}

		# data colors are allowed as named constants
		if ($line -match "static readonly (System\.Drawing\.)?Color [A-Z][A-Z0-9_]+ =") {
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
