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
$passed = 0
try {
	$env:PATH = $bin + ';' + $env:PATH
	[Environment]::CurrentDirectory = $bin
	$null = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'OCCCSharp.dll'))
	$null = [Reflection.Assembly]::LoadFrom($AssemblyPath)
	$provider = New-Object Microsoft.CSharp.CSharpCodeProvider
	try {
		$parameters = New-Object System.CodeDom.Compiler.CompilerParameters
		$parameters.GenerateInMemory = $true
		$parameters.ReferencedAssemblies.AddRange([string[]]@($AssemblyPath, (Join-Path $bin 'OCCCSharp.dll'), (Join-Path $bin 'OCCTool.dll'), (Join-Path $bin 'CommonData.dll'), 'System.Core.dll'))
		$result = $provider.CompileAssemblyFromFile($parameters, [string[]]@(Join-Path $PSScriptRoot 'ExternalAxisKinematics.Tests.cs'))
		if ($result.Errors.HasErrors) {
			throw (($result.Errors | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine)
		}
		$testType = $result.CompiledAssembly.GetType('ExternalAxisKinematicsTests', $true)
	} finally {
		$provider.Dispose()
	}
	$tests = @(
		'NoExternalAxisAndZeroRegression',
		'LeftHandConventionAndDirectionMetadata',
		'OffsetAndBiasedPivot',
		'DualParentOrder',
		'PositionDirectionAndVectorInverse',
		'TiltRetainedButNotApplied',
		'IKAndToolVectorFrames',
		'RTCPDoesNotIncludeExternalDisplacement',
		'FKIntermediatePointAppliesPositioningOnce',
		'CacheUsesAssignedAnglesBeforeIK',
		'LocalABAndAssignedLockedAngles',
		'FlipAndSingularityStayInFiveAxisFrame',
		'MachiningDirectionPhaseRemainsBeforeExternalPositioning',
		'SafePlaneRemainsInPrePositioningWorkpieceCoordinates'
	)
	foreach ($test in $tests) {
		try {
			$testType.GetMethod($test).Invoke($null, $null)
		} catch {
			throw ('FAIL ' + $test + ': ' + $_.Exception.ToString())
		}
		$passed++
		Write-Host ('PASS ' + $test)
	}
	Write-Host ('All ' + $passed + ' external-axis kinematics tests passed.')
} finally {
	$env:PATH = $oldPath
	[Environment]::CurrentDirectory = $oldDirectory
}
