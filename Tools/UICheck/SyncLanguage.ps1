# Copies the language files the build output is missing or has out of date (the language part of CopyRef.bat).
# Dot-sourced by CheckRuntimeLayout.ps1 and CaptureMainWindow.ps1: a stale copy keeps old IDs, the texts are then not
# found and the designer texts stay, which no layout check can tell.
# Runs that overlap in the same build output take a lock for that folder, compare again inside it, and replace a file
# as a whole from a temporary copy, so a runner never reads a half written file.

function Sync-LanguageFiles( [string]$szRoot, [string]$szBinDir ) {
	$source = Join-Path $szRoot "Language"
	$target = Join-Path $szBinDir "Resource\Language"
	$sha1 = [System.Security.Cryptography.SHA1]::Create()
	$pathBytes = [System.Text.Encoding]::UTF8.GetBytes(([System.IO.Path]::GetFullPath($szBinDir)).ToLowerInvariant())
	$szFolderId = [System.BitConverter]::ToString($sha1.ComputeHash($pathBytes)).Replace("-", "")
	$szLockName = "Global\MyCAM_UICheck_Language_$szFolderId"
	$syncLock = New-Object System.Threading.Mutex($false, $szLockName)
	try {
		try {
			[void]$syncLock.WaitOne()
		}
		# a run that was killed leaves the lock abandoned; it is taken over
		catch [System.Threading.AbandonedMutexException] {
		}
		try {
			if (-not (Test-Path $target)) {
				New-Item -ItemType Directory -Path $target | Out-Null
			}
			$nCopied = 0
			foreach ($file in Get-ChildItem $source -Filter "*.xml") {
				$copy = Join-Path $target $file.Name
				$isSame = (Test-Path $copy) -and
					((Get-FileHash $copy -Algorithm SHA1).Hash -eq (Get-FileHash $file.FullName -Algorithm SHA1).Hash)
				if (-not $isSame) {
					Copy-LanguageFile $file.FullName $copy
					$nCopied++
				}
			}
		}
		finally {
			$syncLock.ReleaseMutex()
		}
	}
	finally {
		$syncLock.Dispose()
	}
	if ($nCopied -gt 0) {
		Write-Host "[UICheck] $nCopied language files were out of date in $target and have been copied"
	}
}

# a runner of another run may have the file open for a moment, so the replacement is tried a few times
function Copy-LanguageFile( [string]$szSource, [string]$szCopy ) {
	$szTemp = "$szCopy.$PID.tmp"
	Copy-Item $szSource $szTemp -Force
	try {
		for ($i = 1; ; $i++) {
			try {
				Move-Item $szTemp $szCopy -Force
				return
			}
			catch {
				if ($i -ge 10) {
					throw
				}
				Start-Sleep -Milliseconds 200
			}
		}
	}
	finally {
		Remove-Item $szTemp -ErrorAction SilentlyContinue
	}
}
