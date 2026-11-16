# Copies the language files the build output is missing or has out of date (the language part of CopyRef.bat).
# Dot-sourced by CheckRuntimeLayout.ps1 and CaptureMainWindow.ps1: a stale copy keeps old IDs, the texts are then not
# found and the designer texts stay, which no layout check can tell.

function Sync-LanguageFiles( [string]$szRoot, [string]$szBinDir ) {
	$source = Join-Path $szRoot "Language"
	$target = Join-Path $szBinDir "Resource\Language"
	if (-not (Test-Path $target)) {
		New-Item -ItemType Directory -Path $target | Out-Null
	}
	$nCopied = 0
	foreach ($file in Get-ChildItem $source -Filter "*.xml") {
		$copy = Join-Path $target $file.Name
		$isSame = (Test-Path $copy) -and
			((Get-FileHash $copy -Algorithm SHA1).Hash -eq (Get-FileHash $file.FullName -Algorithm SHA1).Hash)
		if (-not $isSame) {
			Copy-Item $file.FullName $copy -Force
			$nCopied++
		}
	}
	if ($nCopied -gt 0) {
		Write-Host "[UICheck] $nCopied language files were out of date in $target and have been copied"
	}
}
