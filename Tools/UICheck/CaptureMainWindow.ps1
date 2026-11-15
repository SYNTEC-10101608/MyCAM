param(
	[string]$Root = "",
	[string]$Configuration = "Debug",
	[Parameter(Mandatory = $true)][string]$OutFile,
	[int]$Width = 1920,
	[int]$Height = 1040,
	[int]$WaitSeconds = 12
)
# Screenshot of the main window at a given size (e.g. 1920 x 1040 landscape, 1080 x 1880 portrait).
# Starts MyCAM.exe, resizes the main window, renders it with PrintWindow and closes the process.

$ErrorActionPreference = "Stop"
if (-not $Root) {
	$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class MainWindowCapture
{
	[DllImport( "user32.dll" )]
	public static extern bool PrintWindow( IntPtr hWnd, IntPtr hdc, uint nFlags );
	[DllImport( "user32.dll" )]
	public static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags );
	[DllImport( "user32.dll" )]
	public static extern bool ShowWindow( IntPtr hWnd, int nCmdShow );
}
"@
$exe = Join-Path $Root "MyCAM\bin\$Configuration\MyCAM.exe"
$process = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -PassThru
try {
	$deadline = (Get-Date).AddSeconds($WaitSeconds + 30)
	while ($process.MainWindowHandle -eq [IntPtr]::Zero -and (Get-Date) -lt $deadline) {
		Start-Sleep -Milliseconds 500
		$process.Refresh()
	}
	$handle = $process.MainWindowHandle

	# restore from maximized, then size; the layout is recalculated on resize
	[MainWindowCapture]::ShowWindow($handle, 9) | Out-Null
	[MainWindowCapture]::SetWindowPos($handle, [IntPtr]::Zero, 0, 0, $Width, $Height, 0x0014) | Out-Null
	Start-Sleep -Seconds $WaitSeconds
	$bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
	$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
	$hdc = $graphics.GetHdc()

	# PW_RENDERFULLCONTENT also renders the 3D view
	[MainWindowCapture]::PrintWindow($handle, $hdc, 2) | Out-Null
	$graphics.ReleaseHdc($hdc)
	$graphics.Dispose()
	$bitmap.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)
	$bitmap.Dispose()
	Write-Host "[capture] $OutFile"
}
finally {
	if (-not $process.HasExited) {
		$process.Kill()
	}
}
