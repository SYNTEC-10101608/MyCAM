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
	[ValidateSet( "", "Landscape", "Portrait" )][string]$Screen = "",
	[switch]$Visible,
	[switch]$DesignerLayout
)
# Screenshot of the main window at a given size (e.g. 1920 x 1040 landscape, 1080 x 1880 portrait) and check
# that dialogs are centered in the dialog area (see MainWindowRunner.cs). Landscape / portrait only depends on
# the window's aspect ratio, so a portrait window is checked on a landscape screen by sizing it beyond the screen.
# -Mode clicks the CAM or CNC (machining) page before the capture; CAD is the start page.
# -ModalDialogs are opened with ShowDialog, as the program does, and checked the same way.
# -DesignerLayout (MainDesignerRegion) sizes the client like StartupForm.Designer.cs and checks that the regions are placed where the
# designer shows them; use it with -Mode CNC, the page the designer shows. It opens no dialog.
# The language is set in memory only; the setting file is not changed.
# The runner works on a hidden desktop (HiddenDesktopRun.cs) so it does not take the screen; -Visible shows it.
# On the hidden desktop the OpenGL 3D view stays empty. The environment variable MYCAM_UICHECK_UNATTENDED=1 marks a
# machine whose screen nobody uses and does the same as -Visible, so UICheck.bat captures the 3D view too (0 marks a
# work computer, see .github/project/tool-uicheck.md).
# -Screen runs on the specified screen of the product, 1920 x 1080 at 100 % scaling (VirtualDisplayRun.cs: an attached
# screen that is one, or else a virtual screen added for the run), and -Width / -Height are ignored: Landscape keeps
# the main window maximized there as the program shows it; Portrait sizes it 1080 x 1880 on that screen, because
# MainLayout only looks at the aspect ratio of the window. Exit code 2 when the screen cannot be prepared.

$ErrorActionPreference = "Stop"
if ($env:MYCAM_UICHECK_UNATTENDED -eq "1") {
	$Visible = $true
}
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
. (Join-Path $PSScriptRoot "RunnerFiles.ps1")
Remove-StaleRunners $binDir
$runner = Get-RunnerPath $binDir "UICheckMainWindow"
$references = @( "System.dll", "System.Core.dll", "System.Drawing.dll", "System.Windows.Forms.dll", "System.Xml.dll",
	(Join-Path $binDir "CommonUI.dll"), (Join-Path $binDir "LanguageManager.dll") ) | ForEach-Object { "/r:$_" }
$sources = @( (Join-Path $PSScriptRoot "MainWindowRunner.cs"), (Join-Path $PSScriptRoot "LayoutRunner.cs") )
$hidden = Get-RunnerPath $binDir "UICheckHidden"
$display = Get-RunnerPath $binDir "UICheckDisplay"
# the runners only live in the build output while they run, so they are never shipped with it; the cleanup also
# covers a failed compile or preparation
try {
	& $csc /nologo /target:exe /platform:anycpu /main:UICheck.MainWindowRunner /out:$runner @references @sources
	if ($LASTEXITCODE -ne 0) {
		Write-Host "[UICheck] failed to compile MainWindowRunner.cs"
		exit 2
	}
	& $csc /nologo /target:exe /platform:anycpu /out:$hidden (Join-Path $PSScriptRoot "HiddenDesktopRun.cs")
	if ($LASTEXITCODE -ne 0) {
		Write-Host "[UICheck] failed to compile HiddenDesktopRun.cs"
		exit 2
	}
	if ($Screen) {
		& $csc /nologo /target:exe /platform:anycpu /out:$display (Join-Path $PSScriptRoot "VirtualDisplayRun.cs")
		if ($LASTEXITCODE -ne 0) {
			Write-Host "[UICheck] failed to compile VirtualDisplayRun.cs"
			exit 2
		}
		# the runner keeps the window maximized when it gets no size
		$Width = 0
		$Height = 0
		if ($Screen -eq "Portrait") {
			$Width = 1080
			$Height = 1880
		}
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
		$command = @( $runner, $Language, $Mode, $Width, $Height, $outPath ) + $dialogArguments
		if (-not $Visible) {
			$command = @( $hidden ) + $command
		}
		if ($Screen) {
			$command = @( $display ) + $command
		}
		$arguments = @( $command | Select-Object -Skip 1 )
		& $command[ 0 ] @arguments
		$result = $LASTEXITCODE
	}
	finally {
		Pop-Location
	}
}
finally {
	Remove-Item $runner, $hidden, $display -ErrorAction SilentlyContinue
}
exit $result
