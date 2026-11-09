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
$flags = [Reflection.BindingFlags]'Static,Public,NonPublic'
$script:passed = 0

function Assert-True($condition, [string]$message) {
	if (-not $condition) { throw $message }
}

function Assert-Close([double]$actual, [double]$expected, [string]$message) {
	Assert-True ([Math]::Abs($actual - $expected) -lt 0.000000001) $message
}

function Get-Enum([string]$type, [string]$value) {
	[Enum]::Parse($script:assembly.GetType('MyCAM.Data.' + $type, $true), $value)
}

function New-Geometry([string]$type, [double]$x, [double]$y, [double]$z) {
	[Activator]::CreateInstance($script:occ.GetType('OCC.gp.' + $type, $true), [object[]]@($x, $y, $z))
}

function Invoke-Static([string]$type, [string]$method, [object[]]$arguments) {
	$methods = @($script:assembly.GetType($type, $true).GetMethods($flags) | Where-Object {
		$_.Name -eq $method -and $_.GetParameters().Length -eq $arguments.Length
	})
	Assert-True ($methods.Count -eq 1) ('Unexpected overloads for ' + $method)
	$result = $methods[0].Invoke($null, $arguments)
	return ,$result
}

function Build-Chain($machine) {
	$null = Invoke-Static 'MyCAM.Data.MachineChainBuilder' 'Build' @($machine)
}

function New-Machine([string]$kind, [string]$externalType = 'None') {
	$machine = [Activator]::CreateInstance($script:assembly.GetType('MyCAM.Data.' + $kind + 'TypeMachineData', $true))
	$machine.ToolDirection = Get-Enum 'ToolDirection' 'Z'
	$machine.MasterRotaryAxis = Get-Enum 'RotaryAxis' 'Z'
	$machine.SlaveRotaryAxis = Get-Enum 'RotaryAxis' 'X'
	$machine.MasterTiltedVec_deg = New-Geometry 'gp_XYZ' 0 0 0
	$machine.SlaveTiltedVec_deg = New-Geometry 'gp_XYZ' 0 0 0
	if ($kind -eq 'Spindle') {
		$machine.ToolToSlaveVec = New-Geometry 'gp_Vec' 0 0 0
		$machine.SlaveToMasterVec = New-Geometry 'gp_Vec' 0 0 0
	} elseif ($kind -eq 'Mix') {
		$machine.ToolToMasterVec = New-Geometry 'gp_Vec' 0 0 0
		$machine.MCSToSlaveVec = New-Geometry 'gp_Vec' 0 0 0
	} else {
		$machine.MasterToSlaveVec = New-Geometry 'gp_Vec' 0 0 0
		$machine.MCSToMasterVec = New-Geometry 'gp_Vec' 0 0 0
	}
	$data = $machine.ExternalAxes
	$data.Type = Get-Enum 'ExternalAxisType' $externalType
	$data.Ext1RotaryAxis = Get-Enum 'RotaryAxis' 'Z'
	$data.Ext2RotaryAxis = Get-Enum 'RotaryAxis' 'X'
	$data.Ext1RotaryDirection = Get-Enum 'RotaryDirection' 'RightHand'
	$data.Ext2RotaryDirection = Get-Enum 'RotaryDirection' 'LeftHand'
	$data.Ext1Start_deg = -180
	$data.Ext1End_deg = 180
	$data.Ext2Start_deg = -90
	$data.Ext2End_deg = 90
	$data.MCSToExt1Vec = New-Geometry 'gp_Vec' 10 20 30
	$data.Ext1ToExt2Vec = New-Geometry 'gp_Vec' 5 7 11
	$data.Ext1TiltedVec_deg = New-Geometry 'gp_XYZ' 1.25 -2.5 3.75
	$data.Ext2TiltedVec_deg = New-Geometry 'gp_XYZ' -4.5 5.25 6.125
	$machine.InitRotateAxes((Get-Enum 'RotaryDirection' 'RightHand'), -180, 180, (Get-Enum 'RotaryDirection' 'LeftHand'), -90, 90)
	Build-Chain $machine
	return $machine
}

function Get-Xml($machine) {
	Invoke-Static 'MyCAM.Helper.MachineDataXMLHelper' 'ConvertMachineDataAndTree2XML' @($machine)
}

function Read-Xml($document, [string]$expectedStatus = 'ReadSuccess', [string]$expectedParameterId) {
	$arguments = [object[]]@($document, $null, 0, $null)
	$machine = Invoke-Static 'MyCAM.Helper.MachineDataXMLHelper' 'ConvertMachineDataFileToMachineData' $arguments
	Assert-True ($arguments[1].ToString() -eq $expectedStatus) ('Unexpected load status: ' + $arguments[1])
	if ($PSBoundParameters.ContainsKey('expectedParameterId')) {
		Assert-True ($arguments[3] -eq $expectedParameterId) ('Unexpected parameter ID: ' + $arguments[3])
	}
	return $machine
}

function Get-Pr($document, [string]$id) {
	$document.Root.Element('MachineData').Elements('Pr') | Where-Object { $_.Attribute('ID').Value -eq $id }
}

function Test-Case([string]$name, [scriptblock]$test) {
	& $test
	$script:passed++
	Write-Host ('PASS ' + $name)
}

try {
	$env:PATH = $bin + ';' + $env:PATH
	[Environment]::CurrentDirectory = $bin
	Add-Type -AssemblyName System.Xml.Linq
	$script:occ = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'OCCCSharp.dll'))
	$script:assembly = [Reflection.Assembly]::LoadFrom($AssemblyPath)
	Assert-True ($null -ne $script:assembly.GetType('MyCAM.Data.ExternalAxisMachineData')) 'Build the updated solution first.'

	Test-Case 'ExistingComponentIdsAndOrder' {
		Assert-True ([int](Get-Enum 'MachineComponentType' 'WorkPiece') -eq 6) 'WorkPiece id changed.'
		Assert-True ([int](Get-Enum 'MachineComponentType' 'Base') -eq 7) 'Base id changed.'
		Assert-True ([int](Get-Enum 'MachineComponentType' 'T1_ToolFrame') -eq 17) 'Robot component id changed.'
		Assert-True ([int](Get-Enum 'MachineComponentType' 'Ext1') -eq 18) 'Unexpected Ext1 id.'
		Assert-True ([int](Get-Enum 'MachineComponentType' 'Ext2') -eq 19) 'Unexpected Ext2 id.'
		$machine = New-Machine 'Spindle'
		Assert-True (($machine.ComponentTypes -join ',') -eq 'XAxis,YAxis,ZAxis,Master,Slave,Laser,WorkPiece') 'Legacy component order changed.'
		Assert-True (-not $machine.MeshFileNameMap.ContainsKey((Get-Enum 'MachineComponentType' 'Ext1'))) 'Disabled external mesh is included.'
	}

	Test-Case 'LegacyXmlWithoutExternalNodes' {
		foreach ($kind in @('Spindle', 'Table', 'Mix')) {
			$machine = New-Machine $kind
			$document = Get-Xml $machine
			Assert-True ($null -eq (Get-Pr $document 'ExternalAxis_Type')) 'Disabled external nodes were written.'
			$loaded = Read-Xml $document
			Assert-True ($loaded.ExternalAxes.Type.ToString() -eq 'None') 'Old XML enabled external axes.'
			Build-Chain $loaded
			Assert-True (-not $loaded.WorkPieceChain.Contains((Get-Enum 'MachineComponentType' 'Ext1'))) 'Old XML gained an external joint.'
		}
	}

	Test-Case 'SingleSpindleXmlRoundTripAndUnits' {
		$machine = New-Machine 'Spindle' 'Single'
		$document = Get-Xml $machine
		Assert-Close ([double](Get-Pr $document 'Ext1_Offset_X').Attribute('Value').Value) 10000 'Offset must use micrometers.'
		Assert-Close ([double](Get-Pr $document 'Ext1_Tilt_X').Attribute('Value').Value) 1250 'Tilt must use millidegrees.'
		$loaded = Read-Xml $document
		Assert-True ($loaded.ExternalAxes.Type.ToString() -eq 'Single') 'External type was lost.'
		Assert-Close $loaded.ExternalAxes.MCSToExt1Vec.y 20 'Offset was lost.'
		Assert-Close $loaded.ExternalAxes.Ext1TiltedVec_deg.x 1.25 'Tilt was lost.'
		Assert-Close $loaded.ExternalAxes.Ext1Start_deg -180 'Lower limit was lost.'
		Assert-True ($loaded.ExternalAxes.Ext1RotaryDirection.ToString() -eq 'RightHand') 'Direction was lost.'
		Assert-Close $loaded.ExternalAxes.Ext1Axis.RotateDir.x 0 'Tilt unexpectedly changed nominal Z.'
		Assert-Close $loaded.ExternalAxes.Ext1Axis.RotateDir.z 1 'Nominal Z direction changed.'
		Assert-True ($null -eq $loaded.ExternalAxes.Ext2Axis) 'Single configuration created Ext2.'
	}

	Test-Case 'DualSpindleXmlAndZeroCenters' {
		$machine = New-Machine 'Spindle' 'Dual'
		$loaded = Read-Xml (Get-Xml $machine)
		Assert-Close $loaded.ExternalAxes.Ext2Axis.RotateCenter.x 15 'Ext2 center X is wrong.'
		Assert-Close $loaded.ExternalAxes.Ext2Axis.RotateCenter.y 27 'Ext2 center Y is wrong.'
		Assert-Close $loaded.ExternalAxes.Ext2Axis.RotateCenter.z 41 'Ext2 center Z is wrong.'
		Assert-Close $loaded.ExternalAxes.Ext2TiltedVec_deg.z 6.125 'Ext2 tilt was lost.'
		Assert-Close $loaded.ExternalAxes.Ext2Axis.RotateDir.x 1 'Ext2 tilt unexpectedly changed nominal X.'
		Build-Chain $loaded
		$workpiece = Get-Enum 'MachineComponentType' 'WorkPiece'
		Assert-True (($loaded.ChainListMap[$workpiece] -join ',') -eq 'Base,Ext1,Ext2') 'Dual joint order is wrong.'
		Assert-True ($loaded.ComponentTypes.Count -eq 9) 'Dual component count is wrong.'
		Assert-True ($loaded.MeshFileNameMap[(Get-Enum 'MachineComponentType' 'Ext2')] -eq 'Ext2') 'Ext2 mesh mapping is wrong.'
	}

	Test-Case 'SingleMixWorkpieceChain' {
		$loaded = Read-Xml (Get-Xml (New-Machine 'Mix' 'Single'))
		Build-Chain $loaded
		Assert-True (($loaded.ChainListMap[(Get-Enum 'MachineComponentType' 'WorkPiece')] -join ',') -eq 'Base,Slave,Ext1') 'Mix external joint is not after Slave.'
	}

	Test-Case 'DtoCopiesExternalGeometry' {
		$machine = New-Machine 'Spindle' 'Dual'
		$container = Invoke-Static 'MyCAM.FileManager.MachineDataDTOManager' 'ToDTOContainer' @($machine)
		$loaded = Invoke-Static 'MyCAM.FileManager.MachineDataDTOManager' 'ToMachineData' @($container)
		Assert-True (-not [object]::ReferenceEquals($loaded.ExternalAxes.MCSToExt1Vec, $machine.ExternalAxes.MCSToExt1Vec)) 'DTO aliases original geometry.'
		Assert-Close $loaded.ExternalAxes.Ext2TiltedVec_deg.y 5.25 'DTO lost Ext2 tilt.'
	}

	Test-Case 'FullTreePreservesValidExternalChain' {
		$machine = New-Machine 'Spindle' 'Dual'
		$machine.FullMachineSimulation = $true
		$root = $machine.RootNode
		Assert-True (-not $machine.NeedBuildDefaultTree()) 'Valid external tree was rejected.'
		Build-Chain $machine
		Assert-True ([object]::ReferenceEquals($root, $machine.RootNode)) 'Valid tree was replaced.'
	}

	Test-Case 'FullTreeRejectsMissingAndReversedExternalJoints' {
		$machine = New-Machine 'Spindle' 'Dual'
		$machine.FullMachineSimulation = $true
		$ext1 = $machine.RootNode.Children[1]
		$ext1.GetType().GetProperty('Type').SetValue($ext1, (Get-Enum 'MachineComponentType' 'Ext2'), $null)
		$ext2 = $ext1.Children[0]
		$ext2.GetType().GetProperty('Type').SetValue($ext2, (Get-Enum 'MachineComponentType' 'Ext1'), $null)
		Assert-True $machine.NeedBuildDefaultTree() 'Reversed external joints were accepted.'
		Build-Chain $machine
		$machine.RootNode.Children[1] = (New-Machine 'Spindle').RootNode.Children[1]
		Assert-True $machine.NeedBuildDefaultTree() 'Missing external joints were accepted.'
	}

	Test-Case 'XmlRejectsInvalidExternalParameters' {
		$baseline = Get-Xml (New-Machine 'Spindle' 'Single')
		foreach ($pair in @(@('ExternalAxis_Type', '99'), @('Ext1_Axis', '0'), @('Ext1_Axis', '99'), @('Ext1_Direction', '99'), @('Ext1_Offset_X', 'NaN'), @('Ext1_Tilt_Y', 'Infinity'), @('Ext1_Start', '181'))) {
			$document = [System.Xml.Linq.XDocument]::Parse($baseline.ToString())
			(Get-Pr $document $pair[0]).SetAttributeValue('Value', $pair[1])
			$null = Read-Xml $document 'InvalidPrValue' $pair[0]
		}
		$document = [System.Xml.Linq.XDocument]::Parse($baseline.ToString())
		(Get-Pr $document 'Ext1_Offset_Z').Remove()
		$null = Read-Xml $document 'InvalidPrValue' 'Ext1_Offset_Z'
	}

	Test-Case 'XmlRejectsUnsupportedConfigurations' {
		$document = Get-Xml (New-Machine 'Spindle' 'Dual')
		(Get-Pr $document '3001').SetAttributeValue('Value', '3')
		$null = Read-Xml $document 'InvalidPrValue' 'ExternalAxis_Type'
		$document = Get-Xml (New-Machine 'Spindle' 'Single')
		(Get-Pr $document '3001').SetAttributeValue('Value', '2')
		$null = Read-Xml $document 'InvalidPrValue' 'ExternalAxis_Type'
	}

	Test-Case 'ExternalFrameSlotsRemainAtZero' {
		foreach ($configuration in @(@('Spindle', 'None'), @('Spindle', 'Single'), @('Spindle', 'Dual'), @('Mix', 'None'), @('Mix', 'Single'), @('Table', 'None'))) {
			$machine = New-Machine $configuration[0] $configuration[1]
			$expectedCount = if ($configuration[1] -eq 'Dual') { 9 } elseif ($configuration[1] -eq 'Single') { 8 } else { 7 }
			Assert-True ($machine.ComponentTypes.Count -eq $expectedCount) 'Unexpected component count.'
			$builder = [Activator]::CreateInstance($script:assembly.GetType('MyCAM.Helper.FiveAxisPerFrameTransformBuilder'), [object[]]@($machine))
			$point = [Activator]::CreateInstance($script:assembly.GetType('MyCAM.Data.PostPoint'))
			$buffer = [Array]::CreateInstance($script:occ.GetType('OCC.gp.gp_Trsf'), $machine.ComponentTypes.Count)
			$builder.BuildFrameTransforms($point, (New-Geometry 'gp_Vec' 0 0 0), $machine.WorkPieceChain, $buffer)
			foreach ($name in @('Ext1', 'Ext2')) {
				$index = $builder.IndexOf((Get-Enum 'MachineComponentType' $name))
				$enabled = $configuration[1] -eq 'Dual' -or ($configuration[1] -eq 'Single' -and $name -eq 'Ext1')
				if (-not $enabled) {
					Assert-True ($index -eq -1) 'Disabled external frame slot is included.'
					continue
				}
				Assert-True ($index -eq $(if ($name -eq 'Ext1') { 7 } else { 8 })) 'External component index changed.'
				Assert-True ($null -ne $buffer[$index]) 'External frame slot is missing.'
				$translation = $buffer[$index].TranslationPart()
				Assert-Close $translation.x 0 'External zero frame translated.'
				Assert-Close $translation.y 0 'External zero frame translated.'
				Assert-Close $translation.z 0 'External zero frame translated.'
				for ($row = 1; $row -le 3; $row++) {
					for ($column = 1; $column -le 3; $column++) {
						$expected = if ($row -eq $column) { 1 } else { 0 }
						Assert-Close $buffer[$index].Value($row, $column) $expected 'External zero frame rotated.'
					}
				}
			}
		}
	}

	Test-Case 'ExplicitNoneTypeIsCompatible' {
		$document = Get-Xml (New-Machine 'Spindle')
		$node = [System.Xml.Linq.XElement]::new([System.Xml.Linq.XName]::Get('Pr'))
		$node.SetAttributeValue('ID', 'ExternalAxis_Type')
		$node.SetAttributeValue('Value', '0')
		$document.Root.Element('MachineData').Add($node)
		$loaded = Read-Xml $document 'ReadSuccess' ''
		Assert-True ($loaded.ExternalAxes.Type.ToString() -eq 'None' -and $null -eq $loaded.ExternalAxes.Ext1Axis) 'Explicit None enabled external axes.'
	}

	Test-Case 'NoneRotaryDirectionIsAllowed' {
		foreach ($configuration in @(@('Spindle', 'Single'), @('Spindle', 'Dual'), @('Mix', 'Single'))) {
			$machine = New-Machine $configuration[0] $configuration[1]
			$machine.ExternalAxes.Ext1RotaryDirection = Get-Enum 'RotaryDirection' 'None'
			$machine.ExternalAxes.Ext2RotaryDirection = Get-Enum 'RotaryDirection' 'None'
			$loaded = Read-Xml (Get-Xml $machine)
			Assert-True ($loaded.ExternalAxes.Ext1Axis.RotaryDirection.ToString() -eq 'None') 'Ext1 None direction was rejected or changed.'
			if ($configuration[1] -eq 'Dual') {
				Assert-True ($loaded.ExternalAxes.Ext2Axis.RotaryDirection.ToString() -eq 'None') 'Ext2 None direction was rejected or changed.'
			}
		}
	}

	Test-Case 'Ext2ErrorsIdentifyTheirParameter' {
		$baseline = Get-Xml (New-Machine 'Spindle' 'Dual')
		foreach ($pair in @(@('Ext2_Axis', '0'), @('Ext2_Axis', '99'), @('Ext2_Direction', '99'), @('Ext2_Offset_Y', 'NaN'), @('Ext2_Tilt_Z', 'Infinity'), @('Ext2_Start', '91'))) {
			$document = [System.Xml.Linq.XDocument]::Parse($baseline.ToString())
			(Get-Pr $document $pair[0]).SetAttributeValue('Value', $pair[1])
			$null = Read-Xml $document 'InvalidPrValue' $pair[0]
		}
		$document = [System.Xml.Linq.XDocument]::Parse($baseline.ToString())
		(Get-Pr $document 'Ext2_Offset_X').Remove()
		$null = Read-Xml $document 'InvalidPrValue' 'Ext2_Offset_X'
	}

	Test-Case 'ExternalXmlNumbersAreCultureInvariant' {
		$oldCulture = [Threading.Thread]::CurrentThread.CurrentCulture
		try {
			foreach ($culture in @('en-US', 'de-DE', 'fr-FR')) {
				[Threading.Thread]::CurrentThread.CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo($culture)
				$machine = New-Machine 'Spindle' 'Dual'
				$machine.ExternalAxes.MCSToExt1Vec = New-Geometry 'gp_Vec' 0.012345 -0.06789 0.000125
				$machine.ExternalAxes.Ext1ToExt2Vec = New-Geometry 'gp_Vec' 0.009876 0.012345 -0.000625
				$machine.ExternalAxes.Ext1TiltedVec_deg = New-Geometry 'gp_XYZ' 0.123456 -0.654321 0.000125
				$machine.ExternalAxes.Ext2TiltedVec_deg = New-Geometry 'gp_XYZ' -0.987654 0.012345 0.000625
				$machine.ExternalAxes.Ext1Start_deg = -180.5
				$machine.ExternalAxes.Ext1End_deg = 180.75
				$machine.ExternalAxes.Ext2Start_deg = -90.125
				$machine.ExternalAxes.Ext2End_deg = 90.5
				$document = Get-Xml $machine
				$loaded = Read-Xml $document
				Assert-Close $loaded.ExternalAxes.MCSToExt1Vec.x 0.012345 ('Ext1 offset misread under ' + $culture)
				Assert-Close $loaded.ExternalAxes.Ext1ToExt2Vec.x 0.009876 ('Ext2 offset misread under ' + $culture)
				Assert-Close $loaded.ExternalAxes.Ext1TiltedVec_deg.x 0.123456 ('Ext1 tilt misread under ' + $culture)
				Assert-Close $loaded.ExternalAxes.Ext2TiltedVec_deg.x -0.987654 ('Ext2 tilt misread under ' + $culture)
				Assert-Close $loaded.ExternalAxes.Ext1Start_deg -180.5 'Ext1 fractional limit changed.'
				Assert-Close $loaded.ExternalAxes.Ext2End_deg 90.5 'Ext2 fractional limit changed.'
				(Get-Pr $document 'Ext1_Offset_X').SetAttributeValue('Value', '12,345')
				$null = Read-Xml $document 'InvalidPrValue' 'Ext1_Offset_X'
			}
		} finally {
			[Threading.Thread]::CurrentThread.CurrentCulture = $oldCulture
		}
	}

	Test-Case 'LegacyNumericLoadInterfaceStillWorks' {
		$document = Get-Xml (New-Machine 'Spindle')
		(Get-Pr $document '3001').SetAttributeValue('Value', '99')
		$arguments = [object[]]@($document, $null, 0)
		$null = Invoke-Static 'MyCAM.Helper.MachineDataXMLHelper' 'ConvertMachineDataFileToMachineData' $arguments
		Assert-True ($arguments[1].ToString() -eq 'InvalidPrValue' -and $arguments[2] -eq 3001) 'Original numeric PR interface changed.'
		$null = Read-Xml $document 'InvalidPrValue' '3001'
		$arguments = [object[]]@($null, $null, 0)
		$null = Invoke-Static 'MyCAM.Helper.MachineDataXMLHelper' 'ConvertMachineDataFileToMachineData' $arguments
		Assert-True ($arguments[1].ToString() -eq 'NullFile' -and $arguments[2] -eq -1) 'Original null-file behavior changed.'
	}

	Test-Case 'FullTreesForSingleConfigurations' {
		foreach ($kind in @('Spindle', 'Mix')) {
			$machine = New-Machine $kind 'Single'
			$machine.FullMachineSimulation = $true
			$root = $machine.RootNode
			Assert-True (-not $machine.NeedBuildDefaultTree()) 'Valid single-axis tree was rejected.'
			Build-Chain $machine
			Assert-True ([object]::ReferenceEquals($root, $machine.RootNode)) 'Valid single-axis tree was replaced.'
			$workBranch = if ($kind -eq 'Spindle') { 1 } else { 0 }
			$machine.RootNode.Children[$workBranch] = (New-Machine $kind).RootNode.Children[$workBranch]
			Assert-True $machine.NeedBuildDefaultTree() 'Missing single external axis was accepted.'
			Build-Chain $machine
			Assert-True $machine.WorkPieceChain.Contains((Get-Enum 'MachineComponentType' 'Ext1')) 'Fallback tree lost Ext1.'
		}
	}

	Test-Case 'FullTreeRejectsToolSideAndDuplicateExternalAxes' {
		$machine = New-Machine 'Spindle' 'Single'
		$machine.FullMachineSimulation = $true
		$external = $machine.RootNode.Children[1]
		$machine.RootNode.Children[1] = $external.Children[0]
		$toolParent = $machine.RootNode.Children[0]
		while ($toolParent.Children[0].Type.ToString() -ne 'Laser') { $toolParent = $toolParent.Children[0] }
		$external.Children[0] = $toolParent.Children[0]
		$toolParent.Children[0] = $external
		Assert-True $machine.NeedBuildDefaultTree() 'Tool-side external axis was accepted.'
		Build-Chain $machine
		$duplicate = [Activator]::CreateInstance($script:assembly.GetType('MyCAM.Data.MachineTreeNode'), [object[]]@((Get-Enum 'MachineComponentType' 'Ext1')))
		$duplicate.AddChild($machine.RootNode.Children[1])
		$machine.RootNode.Children[1] = $duplicate
		Assert-True $machine.NeedBuildDefaultTree() 'Duplicate external axis was accepted.'
	}

	Write-Host ('All {0} external-axis step1 tests passed.' -f $script:passed)
} finally {
	$env:PATH = $oldPath
	[Environment]::CurrentDirectory = $oldDirectory
}
