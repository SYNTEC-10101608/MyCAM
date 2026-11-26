# Renders the toolbar icon set (IconDefinitions.ps1) to PNG files in MyCAM/Resource.
# Usage: powershell -NoProfile -STA -File RenderIcons.ps1 [-OutDir <dir>] [-Size 40]
# Icons are authored on a 24 x 24 grid with WPF path markup (same syntax as SVG path data).
param(
	[string]$OutDir = (Join-Path $PSScriptRoot ".."),
	[int]$Size = 40,
	[string]$Only = ""
)
Add-Type -AssemblyName PresentationCore, PresentationFramework, WindowsBase

# icon drawing palette, not UI tokens; a and r equal CommonUI.Theme.ThemeColors.ACCENT and ERROR
$Palette = @{
	k = "#334155"	# ink: objects and outlines
	a = "#0067AD"	# accent: the action or the result
	t = "#D3E5F5"	# accent tint: filled areas
	g = "#94A3B8"	# guide: hidden edges, references
	r = "#C42B1C"	# destructive / stop
	y = "#D97706"	# warning: collision
	w = "#FFFFFF"	# white, for marks on badges
	f = "#F1F5F9"	# faint fill
	h = "#5E9FD3"	# highlighted face of view cubes
}

function Get-Brush( [string]$szKey ) {
	if( $szKey -eq "-" -or [string]::IsNullOrEmpty( $szKey ) ) { return $null }
	$color = [System.Windows.Media.ColorConverter]::ConvertFromString( $Palette[ $szKey ] )
	$brush = New-Object System.Windows.Media.SolidColorBrush( $color )
	$brush.Freeze()
	return $brush
}

# circle as path markup
function C( [double]$dX, [double]$dY, [double]$dR ) {
	$dLeft = $dX - $dR
	$dDiameter = 2 * $dR
	return "M$dLeft,$dY a$dR,$dR 0 1 0 $dDiameter,0 a$dR,$dR 0 1 0 -$dDiameter,0 Z"
}

# arrow head pointing from (x1,y1) towards (x2,y2), tip at (x2,y2)
function Head( [double]$dX1, [double]$dY1, [double]$dX2, [double]$dY2, [double]$dLength = 3.2 ) {
	$dAngle = [Math]::Atan2( $dY2 - $dY1, $dX2 - $dX1 )
	$dSpread = 0.55
	$dAx = $dX2 - $dLength * [Math]::Cos( $dAngle - $dSpread )
	$dAy = $dY2 - $dLength * [Math]::Sin( $dAngle - $dSpread )
	$dBx = $dX2 - $dLength * [Math]::Cos( $dAngle + $dSpread )
	$dBy = $dY2 - $dLength * [Math]::Sin( $dAngle + $dSpread )
	return ( "M{0:0.##},{1:0.##} L{2:0.##},{3:0.##} L{4:0.##},{5:0.##}" -f $dAx, $dAy, $dX2, $dY2, $dBx, $dBy )
}

# shape: "stroke|fill|width|dash|path"; text shape: "text|fill|size|x,y|content"
function Draw-Icon( $context, [string[]]$shapeArray ) {
	foreach( $szShape in $shapeArray ) {
		$partArray = $szShape.Split( '|' )
		if( $partArray[ 0 ] -eq "text" ) {
			$brush = Get-Brush $partArray[ 1 ]
			$point = $partArray[ 3 ].Split( ',' )
			$typeface = New-Object System.Windows.Media.Typeface( "Segoe UI Semibold" )
			$text = New-Object System.Windows.Media.FormattedText( $partArray[ 4 ], [Globalization.CultureInfo]::InvariantCulture,
				[System.Windows.FlowDirection]::LeftToRight, $typeface, [double]$partArray[ 2 ], $brush, 1.0 )
			$origin = New-Object System.Windows.Point( ( [double]$point[ 0 ] - $text.Width / 2 ), ( [double]$point[ 1 ] - $text.Height / 2 ) )
			$context.DrawText( $text, $origin )
			continue
		}
		$geometry = [System.Windows.Media.Geometry]::Parse( $partArray[ 4 ] )
		$pen = $null
		$strokeBrush = Get-Brush $partArray[ 0 ]
		if( $strokeBrush -ne $null ) {
			$pen = New-Object System.Windows.Media.Pen( $strokeBrush, [double]$partArray[ 2 ] )
			$pen.StartLineCap = 'Round'
			$pen.EndLineCap = 'Round'
			$pen.LineJoin = 'Round'
			if( $partArray[ 3 ] -eq "d" ) {
				$pen.DashStyle = New-Object System.Windows.Media.DashStyle( [double[]]@( 1.4, 1.8 ), 0 )
				$pen.DashCap = 'Round'
			}
		}
		$context.DrawGeometry( ( Get-Brush $partArray[ 1 ] ), $pen, $geometry )
	}
}

function Save-Icon( [string]$szName, [string[]]$shapeArray ) {
	$visual = New-Object System.Windows.Media.DrawingVisual
	$context = $visual.RenderOpen()
	$dScale = $Size / 24.0
	$context.PushTransform( ( New-Object System.Windows.Media.ScaleTransform( $dScale, $dScale ) ) )

	# optional leading "zoom|factor" enlarges the drawing around the grid center, for icons shown very small
	$nPushCount = 1
	if( $shapeArray.Count -gt 0 -and $shapeArray[ 0 ].StartsWith( "zoom|" ) ) {
		$dZoom = [double]$shapeArray[ 0 ].Split( '|' )[ 1 ]
		$context.PushTransform( ( New-Object System.Windows.Media.ScaleTransform( $dZoom, $dZoom, 12, 12 ) ) )
		$nPushCount++
		$shapeArray = $shapeArray[ 1..( $shapeArray.Count - 1 ) ]
	}
	Draw-Icon $context $shapeArray
	for( $i = 0; $i -lt $nPushCount; $i++ ) {
		$context.Pop()
	}
	$context.Close()
	$bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap( $Size, $Size, 96, 96,
		[System.Windows.Media.PixelFormats]::Pbgra32 )
	$bitmap.Render( $visual )
	$encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
	$encoder.Frames.Add( [System.Windows.Media.Imaging.BitmapFrame]::Create( $bitmap ) )
	$path = Join-Path $OutDir ( $szName + ".png" )
	$stream = [System.IO.File]::Create( $path )
	try {
		$encoder.Save( $stream )
	}
	finally {
		$stream.Dispose()
	}
}

. ( Join-Path $PSScriptRoot "IconDefinitions.ps1" )

$nCount = 0
foreach( $szName in $IconMap.Keys ) {
	if( $Only -ne "" -and ( $Only.Split( ',' ) -notcontains $szName ) ) { continue }
	Save-Icon $szName $IconMap[ $szName ]
	$nCount++
}
"$nCount icons rendered to $OutDir"
