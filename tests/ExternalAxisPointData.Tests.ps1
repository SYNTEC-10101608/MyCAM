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
	$references = @($AssemblyPath, (Join-Path $bin 'OCCCSharp.dll'), (Join-Path $bin 'OCCTool.dll'), (Join-Path $bin 'CommonData.dll'), 'System.Core.dll', 'System.Xml.dll')
	$provider = New-Object Microsoft.CSharp.CSharpCodeProvider
	try {
		$parameters = New-Object System.CodeDom.Compiler.CompilerParameters
		$parameters.GenerateInMemory = $true
		$parameters.ReferencedAssemblies.AddRange([string[]]$references)
		$result = $provider.CompileAssemblyFromFile($parameters, [string[]]@(Join-Path $PSScriptRoot 'ExternalAxisPointData.Tests.cs'))
		if ($result.Errors.HasErrors) {
			throw (($result.Errors | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine)
		}
		$testType = $result.CompiledAssembly.GetType('ExternalAxisPointDataTests', $true)
	} finally {
		$provider.Dispose()
	}
	$tests = @(
		'DefaultsAndExactAssignedValues',
		'NotificationAndIndependentPaths',
		'CraftXmlRoundTripAndLegacyDefault',
		'CAMAndInterfaceClones',
		'PostClone',
		'RejectNonFiniteExternalAnglesWithoutMutatingCraft',
		'CraftXmlRejectsNonFiniteExternalAngles',
		'CraftDTORejectsIncompleteExternalAngles',
		'FiniteMultipleTurnsAreNotClamped',
		'ContourCacheRebuild',
		'CircleCacheRebuild',
		'RectangleCacheRebuild',
		'RunwayCacheRebuild',
		'PolygonCacheRebuild',
		'LeadAndOverCutSupplementalPoints',
		'MicroJointSupplementalPoint',
		'TraverseGeometryAndZeroDistanceClones',
		'PostConversionEntryExitAndTraverse',
		'SimulationLinearArcAndCoincidentPoints',
		'FKRetainsExternalAnglesWithoutNewKinematics',
		'SimulationExitRetainsExternalAngles',
		'StandardSimulationReferenceOverride'
	)
	$testType.GetMethod('InitializeCaches').Invoke($null, $null)
	foreach ($test in $tests) {
		try {
			$testType.GetMethod($test).Invoke($null, $null)
		} catch {
			throw ('FAIL ' + $test + ': ' + $_.Exception.ToString())
		}
		$passed++
		Write-Host ('PASS ' + $test)
	}
	Write-Host ('All ' + $passed + ' external-axis point data tests passed.')
} finally {
	$env:PATH = $oldPath
	[Environment]::CurrentDirectory = $oldDirectory
}
