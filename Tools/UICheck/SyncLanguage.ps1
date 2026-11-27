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
			# another run in the same build output may copy the same file at the same moment; the copy only has to
			# end up the same as the source
			try {
				Copy-Item $file.FullName $copy -Force
				$nCopied++
			}
			catch [System.IO.IOException] {
				Start-Sleep -Milliseconds 500
				if ((Get-FileHash $copy -Algorithm SHA1).Hash -ne (Get-FileHash $file.FullName -Algorithm SHA1).Hash) {
					throw
				}
			}
		}
	}
	if ($nCopied -gt 0) {
		Write-Host "[UICheck] $nCopied language files were out of date in $target and have been copied"
	}
}
