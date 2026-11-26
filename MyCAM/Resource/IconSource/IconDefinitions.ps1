# Toolbar icon definitions, rendered by RenderIcons.ps1.
# Grid 24 x 24. Shape format: "stroke|fill|width|dash|path" (dash: d = dashed, - = solid),
# text: "text|fill|size|x,y|content". Colors are palette keys defined in RenderIcons.ps1:
# k ink, a accent, t accent tint, g guide, r red, y amber, w white, f faint, h / H view highlight.
# Style rules: objects in ink, the action or its result in accent, references dashed in guide color,
# "add" uses an accent badge at the lower right, destructive actions use red.

$Palette[ "H" ] = "#995E9FD3"

# star-like burst used to mark collisions
function Burst( [double]$dX, [double]$dY, [double]$dOuter, [double]$dInner, [int]$nPoint = 8 ) {
	$szPath = ""
	for( $i = 0; $i -lt $nPoint * 2; $i++ ) {
		$dRadius = if( $i % 2 -eq 0 ) { $dOuter } else { $dInner }
		$dAngle = [Math]::PI * $i / $nPoint - [Math]::PI / 2
		$szCommand = if( $i -eq 0 ) { "M" } else { "L" }
		$szPath += ( "{0}{1:0.##},{2:0.##} " -f $szCommand, ( $dX + $dRadius * [Math]::Cos( $dAngle ) ), ( $dY + $dRadius * [Math]::Sin( $dAngle ) ) )
	}
	return $szPath + "Z"
}

$BadgePlus = @(
	"w|a|1.4|-|$(C 18.5 18.5 4.9)",
	"w|-|1.6|-|M18.5 16.2 V20.8 M16.2 18.5 H20.8"
)
$BadgeMinus = @(
	"w|r|1.4|-|$(C 18.5 18.5 4.9)",
	"w|-|1.6|-|M16.2 18.5 H20.8"
)
$CutPath = @(
	"k|-|1.6|-|M3.5 15.5 C 6 7.5, 10 8, 11.5 12 S 16 15.5, 19.5 5.5",
	"k|w|1.4|-|$(C 3.5 15.5 1.8)",
	"k|k|1.4|-|$(C 19.5 5.5 1.8)"
)
$ToolVecSurface = "k|t|1.4|-|M3 15.5 C 7 12, 12 18.5, 21 13 V17 C 12 22.5, 7 16, 3 19.5 Z"

# view cube seen from above, like the previous icon set: hexagon outline, front face lower left, right face
# lower right, top face as a flat rhombus; hidden edges meet at the back bottom corner.
# Corners: T top, UL / UR upper left / right, C center, LL / LR lower left / right, B bottom, H hidden back bottom.
# Zoomed slightly because the viewer bar shows these icons at 20 px.
$CubeZoom = "zoom|1.1"
$CubeHidden = "g|-|1.2|d|M12 13 L3 17.5 M12 13 L21 17.5 M12 13 V2"
$CubeEdges = "k|-|1.4|-|M12 2 L21 6.5 V17.5 L12 22 L3 17.5 V6.5 Z M3 6.5 L12 11 L21 6.5 M12 11 V22"
$CubeFace = @{
	Top = "M12 2 L21 6.5 L12 11 L3 6.5 Z"
	Front = "M3 6.5 L12 11 V22 L3 17.5 Z"
	Right = "M12 11 L21 6.5 V17.5 L12 22 Z"
	Back = "M12 2 L21 6.5 V17.5 L12 13 Z"
	Left = "M3 6.5 L12 2 V13 L3 17.5 Z"
	Bottom = "M3 17.5 L12 13 L21 17.5 L12 22 Z"
}
function ViewCube( [string]$szFace, [string]$szFill ) {
	return @( $CubeZoom, $CubeHidden, "-|$szFill|0|-|$( $CubeFace[ $szFace ] )", $CubeEdges )
}

$IconMap = [ordered]@{

	#region File

	Import3DFile = @(
		"k|t|1.4|-|M10 2.5 L16.5 6 L10 9.5 L3.5 6 Z",
		"k|f|1.4|-|M3.5 6 L10 9.5 V17 L3.5 13.5 Z",
		"k|w|1.4|-|M16.5 6 V13.5 L10 17 V9.5 Z"
	) + $BadgePlus

	Revolution = @(
		"g|-|1.2|d|M10 1.5 V18.8",
		"g|-|1.2|d|M10 3.8 H6.4 V7.4 L4.6 9.2 V15.9 H10",
		"a|t|1.6|-|M10 3.8 H13.6 V7.4 L15.4 9.2 V15.9 H10 Z",
		"k|-|1.4|-|M4.6 17.5 A 5.9 1.7 0 0 0 15.4 17.5",
		"k|-|1.4|-|$(Head 6.6 18.8 4.6 17.5 2.4)"
	) + $BadgePlus

	AddTube = @(
		"k|f|1.4|-|M4.5 6 V16.5 A6 2.4 0 0 0 13 18.7 M16.5 6 V12.5",
		"k|t|1.4|-|M4.5 6 a6,2.4 0 1 0 12,0 a6,2.4 0 1 0 -12,0 Z",
		"k|w|1.2|-|M8 6 a2.5,1 0 1 0 5,0 a2.5,1 0 1 0 -5,0 Z"
	) + $BadgePlus

	ReadProjectFile = @(
		"k|t|1.4|-|M3 9 V18.5 a1.5,1.5 0 0 0 1.5,1.5 H19.5 a1.5,1.5 0 0 0 1.5,-1.5 V11.5 a1.5,1.5 0 0 0 -1.5,-1.5 H11.5 L9.5 7.5 H4.5 a1.5,1.5 0 0 0 -1.5,1.5 Z",
		"a|-|1.8|-|M15 2.5 V14",
		"a|-|1.8|-|$(Head 15 10 15 14.2)"
	)

	SaveProjectFile = @(
		"k|f|1.4|-|M5 3.5 H16 L20.5 8 V19 a1.5,1.5 0 0 1 -1.5,1.5 H5 a1.5,1.5 0 0 1 -1.5,-1.5 V5 a1.5,1.5 0 0 1 1.5,-1.5 Z",
		"k|t|1.4|-|M7.5 3.5 V8.5 H15 V3.5",
		"a|w|1.4|-|M6.5 20.5 V14.5 a1,1 0 0 1 1,-1 H16.5 a1,1 0 0 1 1,1 V20.5",
		"a|-|1.4|-|M9.5 17 H14.5"
	)

	#endregion

	#region CAD

	SewPart = @(
		"k|t|1.4|-|M3 5.5 H10.5 V18.5 H3 Z",
		"k|t|1.4|-|M13.5 5.5 H21 V18.5 H13.5 Z",
		"a|-|1.8|-|M8.5 8.5 H15.5 M8.5 12 H15.5 M8.5 15.5 H15.5"
	)

	SupportBracket = @(
		"a|t|1.4|-|M2.9 8.4 C 5.6 2.5, 14.6 2.5, 17.3 8.4 H15.5 C 13.3 4.6, 7 4.6, 4.7 8.4 Z",
		"k|-|1.6|-|M5.2 17.4 V8.8 M15.1 17.4 V8.8 M2 17.4 H18.2",
		"k|-|1.6|-|M3.8 8.8 H6.5 M13.7 8.8 H16.4",
		"g|-|1.2|-|M5.2 13.3 H15.1"
	) + $BadgePlus

	BoundingBoxCenter = @(
		"g|-|1.2|d|M5 5 H19 V19 H5 Z",
		"k|-|1.6|-|M5 9 V5 H9 M15 5 H19 V9 M19 15 V19 H15 M9 19 H5 V15",
		"a|-|1.6|-|M12 8.3 V15.7 M8.3 12 H15.7",
		"a|a|1|-|$(C 12 12 1.5)"
	)

	EdgeCenter = @(
		"k|-|1.6|-|M5 18.5 L19 5.5",
		"k|w|1.4|-|$(C 5 18.5 1.8)",
		"k|w|1.4|-|$(C 19 5.5 1.8)",
		"w|a|1.2|-|$(C 12 12 2.6)"
	)

	PointCenter = @(
		"g|-|1.2|d|M5 18.5 L19 5.5",
		"k|k|1.2|-|$(C 5 18.5 1.8)",
		"k|k|1.2|-|$(C 19 5.5 1.8)",
		"a|w|1.6|-|$(C 12 12 2.3)",
		"a|-|1.6|-|M12 7.3 V9.4 M12 14.6 V16.7 M7.3 12 H9.4 M14.6 12 H16.7"
	)

	AcrCenter = @(
		"k|-|1.6|-|M12 4 A8 8 0 1 1 4 12",
		"k|w|1.4|-|$(C 12 4 1.6)",
		"k|w|1.4|-|$(C 4 12 1.6)",
		"g|-|1.2|d|M12 12 L17.6 6.4",
		"a|-|1.6|-|M12 9.2 V14.8 M9.2 12 H14.8"
	)

	TwoVertexConnect = @(
		"a|-|2|-|M5.5 18.5 L18.5 5.5",
		"k|w|1.4|-|$(C 5.5 18.5 2.2)",
		"k|w|1.4|-|$(C 18.5 5.5 2.2)"
	)

	GenerateVertex = @(
		"k|f|1.6|-|M6 7 C 9 3, 15 4, 17 7 C 20 11, 20 16, 16 18.5 C 12 21, 6 19.5, 5 15 C 4 11.5, 4.5 9, 6 7 Z",
		"w|a|1.2|-|$(C 6 7 2)",
		"w|a|1.2|-|$(C 17 7 2)",
		"w|a|1.2|-|$(C 16 18.5 2)",
		"w|a|1.2|-|$(C 5 15 2)"
	)

	AxisTrans = @(
		"k|-|1.6|-|M7 17 H19 M7 17 V5 M7 17 L3.5 20.5",
		"k|-|1.6|-|$(Head 15 17 19.5 17 2.8)",
		"k|-|1.6|-|$(Head 7 9 7 4.5 2.8)",
		"a|-|1.6|-|M12 7.5 A4.3 4.3 0 1 1 15.3 14.3",
		"a|-|1.6|-|$(Head 17.6 13 15.2 14.4 2.8)"
	)

	ManualTransform = @(
		"k|-|1.6|-|M4 3.5 V20 H20.5",
		"g|-|1.2|d|M11.5 4 H19 V11.5 H11.5 Z",
		"k|t|1.4|-|M6 11 H13 V18 H6 Z",
		"a|-|1.6|-|M15 8 L10.8 12.6",
		"a|-|1.6|-|$(Head 13 10 10.6 12.8 2.8)"
	)

	ThreePntTrans = @(
		"g|t|1.2|d|M5.5 18 L18.5 18 L9 5.5 Z",
		"w|a|1.2|-|$(C 5.5 18 2.2)",
		"w|a|1.2|-|$(C 18.5 18 2.2)",
		"w|a|1.2|-|$(C 9 5.5 2.2)"
	)

	Measure = @(
		"k|-|1.6|-|M4 6.5 V17.5 M20 6.5 V17.5",
		"a|-|1.6|-|M5 12 H19",
		"a|-|1.6|-|$(Head 8 12 4.9 12 2.8)",
		"a|-|1.6|-|$(Head 16 12 19.1 12 2.8)",
		"g|-|1.2|-|M8 16.5 V18 M12 16 V18 M16 16.5 V18"
	)

	#endregion

	#region CAM

	AddPath = $CutPath + $BadgePlus

	DeletePath = $CutPath + $BadgeMinus

	AutoOrder = @(
		"k|-|1.8|-|M10.5 6 H20 M10.5 12 H20 M10.5 18 H17",
		"a|a|0.8|-|M5 2 L5.7 3.8 L7.5 4.5 L5.7 5.2 L5 7 L4.3 5.2 L2.5 4.5 L4.3 3.8 Z",
		"a|-|1.6|-|M5 9.5 V19.5",
		"a|-|1.6|-|$(Head 5 15 5 19.7 2.8)"
	)

	ManualOrder = @(
		"k|-|1.8|-|M10.5 6 H20 M10.5 18 H20",
		"a|-|1.8|-|M10.5 12 H20",
		"a|-|1.6|-|M5 5 V19",
		"a|-|1.6|-|$(Head 5 9 5 4.6 2.8)",
		"a|-|1.6|-|$(Head 5 15 5 19.4 2.8)"
	)

	StartPointSetting = @(
		"k|f|1.6|-|$(C 12 12.5 7.5)",
		"k|-|1.6|-|$(Head 19.3 10 19.5 12.8 2.8)",
		"w|a|1.4|-|$(C 12 5 2.6)"
	)

	Reverse = @(
		"k|-|1.6|-|M4 8.5 H18.5",
		"k|-|1.6|-|$(Head 15 8.5 19 8.5 3)",
		"a|-|1.6|-|M20 15.5 H5.5",
		"a|-|1.6|-|$(Head 9 15.5 5 15.5 3)"
	)

	EntryAndExit = @(
		"k|-|1.6|-|M3 18.5 H21",
		"a|-|1.6|-|M5 4.5 C 5 11, 6.5 15.5, 9.5 18",
		"a|-|1.6|-|$(Head 7.3 16 9.6 18.1 2.8)",
		"a|-|1.6|-|M14.5 18 C 17.5 15.5, 19 11, 19 4.5",
		"a|-|1.6|-|$(Head 19 8 19 4.3 2.8)"
	)

	TraverseParam = @(
		"a|-|1.6|d|M5 16.5 C 5 5, 19 5, 19 15.5",
		"a|-|1.6|-|$(Head 19 12.5 19 16.3 2.8)",
		"k|k|1.2|-|$(C 5 18.5 1.8)",
		"k|w|1.4|-|$(C 19 18.5 1.8)"
	)

	FlyCut = @(
		"k|f|1.4|-|$(C 6 12 3)",
		"k|f|1.4|-|$(C 12 12 3)",
		"k|f|1.4|-|$(C 18 12 3)",
		"a|-|1.8|-|M2.5 12 H21",
		"a|-|1.8|-|$(Head 18 12 21.5 12 2.8)"
	)

	Lead = @(
		"k|-|1.6|-|M3 18 H21",
		"g|-|1.2|d|M14 18 C 18 18, 20 13, 20 7",
		"a|-|1.6|-|M5 5.5 C 5 13, 8 18, 11.8 18",
		"a|-|1.6|-|$(Head 9 17.7 12 18 2.8)",
		"w|a|1.2|-|$(C 13 18 2)"
	)

	Overcut = @(
		"k|f|1.6|-|$(C 11 12.5 7)",
		"a|-|2.4|-|M11 19.5 A7 7 0 0 0 17.6 14.9",
		"a|-|1.8|-|$(Head 17.8 17.4 17.9 14 3)",
		"w|k|1.2|-|$(C 11 19.5 2)"
	)

	MicroJoint = @(
		"k|f|1.6|-|M18.79 13.69 A7 7 0 0 1 5.21 13.69 M5.21 10.31 A7 7 0 0 1 18.79 10.31",
		"a|-|2.4|-|M16.8 12 H21 M3 12 H7.2"
	)

	RCorner = @(
		"g|-|1.2|d|M5 13 V5 H13",
		"a|-|1.8|-|M5 20 V13 A8 8 0 0 1 13 5 H20",
		"g|-|1.2|d|M13 13 L7.4 7.4",
		"k|k|1|-|$(C 13 13 1.2)"
	)

	ExtendedGCode = @(
		"k|f|1.4|-|M5 3 H12.5 L16.5 7 V16 M16.5 7 H12.5 V3 M5 3 a1.5,1.5 0 0 0 -1.5,1.5 V19 a1.5,1.5 0 0 0 1.5,1.5 H12.5",
		"text|a|9|10,12.5|G"
	) + $BadgePlus

	ToolVec = @(
		$ToolVecSurface,
		"a|-|1.8|-|M11.5 17 L15 5.5",
		"a|-|1.8|-|$(Head 14 8.8 15.1 5.2 3)",
		"w|a|1.2|-|$(C 11.5 17 1.8)"
	)

	FixedToolVec = @(
		"k|t|1.4|-|M5 17.5 a7,2.6 0 1 0 14,0 a7,2.6 0 1 0 -14,0 Z",
		"a|-|1.8|-|M12 17.5 V4.5",
		"a|-|1.8|-|$(Head 12 8 12 4.2 3)",
		"w|a|1.2|-|$(C 12 17.5 1.8)"
	)

	ToolVecReverse = @(
		$ToolVecSurface,
		"g|-|1.2|d|M8.5 16.5 L10.5 8",
		"g|-|1.2|-|$(Head 9.8 11 10.6 7.6 2.6)",
		"a|-|1.8|-|M17 4 L14.6 14.2",
		"a|-|1.8|-|$(Head 15.6 11 14.5 14.6 3)"
	)

	OptimizeIK = @(
		"k|-|1.6|-|M4.5 10 A8 8 0 0 1 14.5 4.4",
		"k|-|1.6|-|$(Head 11.4 3.9 14.7 4.5 2.8)",
		"k|-|1.6|-|M19.5 14 A8 8 0 0 1 9.5 19.6",
		"k|-|1.6|-|$(Head 12.6 20.1 9.3 19.5 2.8)",
		"a|a|0.8|-|M12 7 C12.4 10.6 13.4 11.6 17 12 C13.4 12.4 12.4 13.4 12 17 C11.6 13.4 10.6 12.4 7 12 C10.6 11.6 11.6 10.6 12 7 Z"
	)

	StandardPattern = @(
		"k|t|1.4|-|$(C 7.5 7.5 3.8)",
		"k|-|1.4|-|M16.5 3.5 L20.8 11 H12.2 Z",
		"k|-|1.4|-|M3.8 13.2 H11.2 V20.6 H3.8 Z",
		"a|t|1.4|-|M16.5 13 L20 15 V19 L16.5 21 L13 19 V15 Z"
	)

	PathEdit = @(
		"g|-|1.2|d|M3.5 10 H12.5 V19 H3.5 Z",
		"k|t|1.4|-|M11.5 4.5 H20.5 V13.5 H11.5 Z",
		"a|-|1.8|-|M6 16.5 L13.6 9.6",
		"a|-|1.8|-|$(Head 11.2 10.4 14 9.3 3)"
	)

	PathCompensate = @(
		"k|f|1.6|-|$(C 12 12 8.5)",
		"g|-|1.2|d|$(C 12 12 4)",
		"a|-|1.6|-|M14.9 9.1 L17.6 6.4 M9.1 14.9 L6.4 17.6",
		"a|-|1.6|-|$(Head 15.5 6.8 17.8 6.2 2.6)",
		"a|-|1.6|-|$(Head 8.5 17.2 6.2 17.8 2.6)"
	)

	ContourEdit = @(
		"g|-|1.2|-|M4 18 L5.5 11 M20 6 L14 6.5",
		"k|-|1.6|-|M4 18 C 5.5 11, 14 6.5, 20 6",
		"a|w|1.4|-|M2.5 16.5 h3 v3 h-3 Z",
		"a|w|1.4|-|M18.5 4.5 h3 v3 h-3 Z",
		"a|a|1|-|$(C 5.5 11 1.3)",
		"a|a|1|-|$(C 14 6.5 1.3)"
	)

	Calibration = @(
		"k|t|1.4|-|M3 17 L9 7 H21 L15 17 Z",
		"w|a|1.2|-|$(C 7.5 14.5 2)",
		"w|a|1.2|-|$(C 11.5 9.5 2)",
		"w|a|1.2|-|$(C 16.5 13.5 2)",
		"k|-|1.4|-|M7.5 18.5 V21.5 M16.5 18.5 V21.5"
	)

	NCFile = @(
		"k|f|1.4|-|M5.5 3 H13 L17.5 7.5 V13 M17.5 7.5 H13 V3 M5.5 3 a1.5,1.5 0 0 0 -1.5,1.5 V19.5 a1.5,1.5 0 0 0 1.5,1.5 H13",
		"text|a|6.6|10.3,13.5|NC",
		"w|a|1.4|-|$(C 18.5 18.5 4.9)",
		"w|-|1.6|-|M16.6 20.4 L20.3 16.7 M17.3 16.6 H20.4 V19.7"
	)

	SetOffset = @(
		"k|-|1.6|-|M4 4.5 V20 H19.5",
		"g|-|1.2|d|M4 20 L13.6 10.4",
		"a|-|1.6|-|$(C 16 8 3.4)",
		"a|-|1.6|-|M16 2.8 V5 M16 11 V13.2 M10.8 8 H13 M19 8 H21.2",
		"a|a|1|-|$(C 16 8 1.1)"
	)

	ProcessSetting = @(
		"k|-|1.6|-|M4 7 H20 M4 12 H20 M4 17 H20",
		"a|w|1.6|-|$(C 9 7 2.2)",
		"a|w|1.6|-|$(C 15.5 12 2.2)",
		"a|w|1.6|-|$(C 7.5 17 2.2)"
	)

	# layer bar legends, shown at 20 x 20: machined as an accent check, not machined as an ink cross
	MachinePath = @(
		"a|-|2.6|-|M5 12.5 L10 17.5 L19 7"
	)

	NoMachinePath = @(
		"k|-|2.4|-|M6.5 6.5 L17.5 17.5 M17.5 6.5 L6.5 17.5"
	)

	#endregion

	#region Simulation

	Play = @( "a|a|1.6|-|M8 5.5 L18.5 12 L8 18.5 Z" )

	Pause = @( "k|k|1.2|-|M7 5.5 H10.5 V18.5 H7 Z M13.5 5.5 H17 V18.5 H13.5 Z" )

	Stop = @( "r|r|1.6|-|M6.5 6.5 H17.5 V17.5 H6.5 Z" )

	PreviousPath = @(
		"k|-|1.8|-|M6.5 6 V18",
		"k|t|1.4|-|M18 6 L9.5 12 L18 18 Z"
	)

	NextPath = @(
		"k|-|1.8|-|M17.5 6 V18",
		"k|t|1.4|-|M6 6 L14.5 12 L6 18 Z"
	)

	PreviousCol = @(
		"k|-|1.8|-|M5 9.5 V20",
		"k|t|1.4|-|M15.5 9.5 L7.5 14.75 L15.5 20 Z",
		"y|y|0.8|-|$(Burst 18.5 5.5 4.2 1.9)"
	)

	NextCol = @(
		"k|t|1.4|-|M4.5 9.5 L12.5 14.75 L4.5 20 Z",
		"k|-|1.8|-|M15 9.5 V20",
		"y|y|0.8|-|$(Burst 18.8 5.5 4.2 1.9)"
	)

	PrePoint = @(
		"g|-|1.2|d|M3 12 H21",
		"a|-|1.8|-|M13 12 H5",
		"a|-|1.8|-|$(Head 8.5 12 4.5 12 3)",
		"k|k|1|-|$(C 16 12 2.4)"
	)

	NextPoint = @(
		"g|-|1.2|d|M3 12 H21",
		"a|-|1.8|-|M11 12 H19",
		"a|-|1.8|-|$(Head 15.5 12 19.5 12 3)",
		"k|k|1|-|$(C 8 12 2.4)"
	)

	SpeedUp = @(
		"k|-|1.6|-|M4.48 17.24 A8 8 0 1 1 19.52 17.24",
		"g|-|1.2|-|M12 6.5 V8 M6.3 9.2 L7.3 10.2 M17.7 9.2 L16.7 10.2",
		"a|-|1.8|-|M12 14.5 L16.6 9.4",
		"a|a|1|-|$(C 12 14.5 1.7)"
	)

	SlowDown = @(
		"k|-|1.6|-|M4.48 17.24 A8 8 0 1 1 19.52 17.24",
		"g|-|1.2|-|M12 6.5 V8 M6.3 9.2 L7.3 10.2 M17.7 9.2 L16.7 10.2",
		"a|-|1.8|-|M12 14.5 L7.4 9.4",
		"a|a|1|-|$(C 12 14.5 1.7)"
	)

	Check = @(
		"k|f|1.6|-|$(C 10.5 10.5 6.2)",
		"k|-|2.4|-|M15.2 15.2 L20 20",
		"y|y|0.8|-|$(Burst 10.5 10.5 3.6 1.6)"
	)

	#endregion

	#region View cube

	Axo = @(
		$CubeZoom,
		$CubeHidden,
		"-|t|0|-|$( $CubeFace[ 'Top' ] )",
		"-|f|0|-|$( $CubeFace[ 'Front' ] )",
		$CubeEdges
	)
	MainView = ViewCube "Front" "h"
	"Right View" = ViewCube "Right" "h"
	TopView = ViewCube "Top" "h"
	BackView = ViewCube "Back" "H"
	LeftView = ViewCube "Left" "H"
	BottomView = ViewCube "Bottom" "H"

	#endregion
}
