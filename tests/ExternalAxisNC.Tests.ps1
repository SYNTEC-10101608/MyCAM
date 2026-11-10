param(
	[string]$AssemblyPath = (Join-Path $PSScriptRoot '..\MyCAM\bin\Release\MyCAM.exe')
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop' -or -not [Environment]::Is64BitProcess) {
	throw 'Run with 64-bit Windows PowerShell (.NET Framework).'
}
$AssemblyPath = (Resolve-Path $AssemblyPath).Path
$bin = Split-Path $AssemblyPath
$oldPath = $env:PATH
$oldDirectory = [Environment]::CurrentDirectory
try {
	$env:PATH = $bin + ';' + $env:PATH
	[Environment]::CurrentDirectory = $bin
	$null = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'OCCCSharp.dll'))
	$null = [Reflection.Assembly]::LoadFrom($AssemblyPath)
	$null = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'CommonData.dll'))
	$null = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'FileReader.dll'))
	$provider = New-Object Microsoft.CSharp.CSharpCodeProvider
	try {
		$parameters = New-Object System.CodeDom.Compiler.CompilerParameters
		$parameters.GenerateInMemory = $true
		$parameters.ReferencedAssemblies.AddRange([string[]]@($AssemblyPath, (Join-Path $bin 'OCCCSharp.dll'), (Join-Path $bin 'OCCTool.dll'), (Join-Path $bin 'CommonData.dll'), (Join-Path $bin 'FileReader.dll'), 'System.dll', 'System.Core.dll'))
		$result = $provider.CompileAssemblyFromFile($parameters, [string[]]@(Join-Path $PSScriptRoot 'ExternalAxisNC.Tests.cs'))
		if ($result.Errors.HasErrors) {
			throw (($result.Errors | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine)
		}
		$testType = $result.CompiledAssembly.GetType('ExternalAxisNCTests', $true)
	} finally {
		$provider.Dispose()
	}
	$tests = $testType.GetMethods([Reflection.BindingFlags]'Public,Static,DeclaredOnly')
	foreach ($test in $tests) {
		try {
			$test.Invoke($null, $null)
		} catch {
			throw ('FAIL ' + $test.Name + ': ' + $_.Exception.ToString())
		}
		Write-Host ('PASS ' + $test.Name)
	}
	Write-Host ('All ' + $tests.Count + ' external-axis NC tests passed.')
} finally {
	$env:PATH = $oldPath
	[Environment]::CurrentDirectory = $oldDirectory
}
