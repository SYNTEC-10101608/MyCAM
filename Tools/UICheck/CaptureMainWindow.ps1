param(
	[string]$Root = "",
	[string]$Configuration = "Debug",
	[Parameter(Mandatory = $true)][string]$OutFile,
	[int]$Width = 1920,
	[int]$Height = 1040,
	[string]$Language = "zh-TW",
	[ValidateSet( "CAD", "CAM", "CNC" )][string]$Mode = "CAD",
	[string[]]$Dialogs = @( "AddPathDlg", "OverCutDlg", "StartPointDlg" ),
	[string[]]$ModalDialogs = @( "ConnectDlg", "GlassDXFImportDlg" ),
	[switch]$Visible,
	[switch]$DesignerLayout
)
# Screenshot of the main window at a given size (e.g. 1920 x 1040 landscape, 1080 x 1880 portrait) and check
# that dialogs are centered in the dialog area (see MainWindowRunner.cs). Landscape / portrait only depends on
# the window's aspect ratio, so a portrait window is checked on a landscape screen by sizing it beyond the screen.
# -Mode clicks the CAM or CNC (machining) page before the capture; CAD is the start page.
# -ModalDialogs are opened with ShowDialog, as the program does, and checked the same way.
# -DesignerLayout (P6) sizes the client like StartupForm.Designer.cs and checks that the regions are placed where the
# designer shows them; use it with -Mode CNC, the page the designer shows. It opens no dialog.
# The language is set in memory only; the setting file is not changed.
# The runner works on a hidden desktop (HiddenDesktopRun.cs) so it does not take the screen; -Visible shows it.
# On the hidden desktop the OpenGL 3D view stays empty.

$ErrorActionPreference = "Stop"
if (-not $Root) {
	$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
$binDir = Join-Path $Root "MyCAM\bin\$Configuration"
if (-not (Test-Path (Join-Path $binDir "MyCAM.exe"))) {
	Write-Host "[UICheck] MyCAM.exe not found in $binDir, build first."
	exit 2
}
. (Join-Path $PSScriptRoot "SyncLanguage.ps1")
Sync-LanguageFiles $Root $binDir

# compile the runner into the output folder, so it finds language files and assemblies like MyCAM.exe
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" |
	Select-Object -First 1
$csc = Join-Path (Split-Path $msbuild) "Roslyn\csc.exe"
$runner = Join-Path $binDir "UICheckMainWindow.exe"
$references = @( "System.dll", "System.Core.dll", "System.Drawing.dll", "System.Windows.Forms.dll", "System.Xml.dll",
	(Join-Path $binDir "CommonUI.dll"), (Join-Path $binDir "LanguageManager.dll") ) | ForEach-Object { "/r:$_" }
$sources = @( (Join-Path $PSScriptRoot "MainWindowRunner.cs"), (Join-Path $PSScriptRoot "LayoutRunner.cs") )
& $csc /nologo /target:exe /platform:anycpu /main:UICheck.MainWindowRunner /out:$runner @references @sources
if ($LASTEXITCODE -ne 0) {
	Write-Host "[UICheck] failed to compile MainWindowRunner.cs"
	exit 2
}
$hidden = Join-Path $binDir "UICheckHidden.exe"
& $csc /nologo /target:exe /platform:anycpu /out:$hidden (Join-Path $PSScriptRoot "HiddenDesktopRun.cs")
if ($LASTEXITCODE -ne 0) {
	Write-Host "[UICheck] failed to compile HiddenDesktopRun.cs"
	exit 2
}

$outPath = $OutFile
if (-not [System.IO.Path]::IsPathRooted($outPath)) {
	$outPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $OutFile))
}
New-Item -ItemType Directory -Force (Split-Path $outPath) | Out-Null

# dialogs opened with ShowDialog are checked the same way, marked for the runner
$dialogArguments = @( $Dialogs ) + @( $ModalDialogs | ForEach-Object { "modal:$_" } )
if ($DesignerLayout) {
	$dialogArguments = @( "designer:" + (Join-Path $Root "MyCAM\StartupForm.Designer.cs") )
}
Push-Location $binDir
try {
	if ($Visible) {
		& $runner $Language $Mode $Width $Height $outPath @dialogArguments
	}
	else {
		& $hidden $runner $Language $Mode $Width $Height $outPath @dialogArguments
	}
	$result = $LASTEXITCODE
}
finally {
	Pop-Location

	# the runners only live in the build output while they run, so they are never shipped with it
	Remove-Item $runner, $hidden -ErrorAction SilentlyContinue
}
exit $result
