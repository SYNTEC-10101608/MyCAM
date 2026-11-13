using MyCAM.Data;
using MyCAM.PathCache;
using OCC.Geom;
using OCC.gp;
using OCC.ShapeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MyCAM.Helper
{
	// pure sorting algorithms for path ordering, no UI dependency
	internal static class PathSortHelper
	{
		const double GEOMETRY_TOLERANCE = 1e-6;

		public static List<gp_Pnt2d> ConvertCylinderToUV( List<gp_Pnt> points, gp_Ax3 cylinderCoordinate, double radius,
			out double usedSeamAngle_rad, double? seamAngle_rad = null )
		{
			double fullTurn = 2 * Math.PI;
			Func<double, double> normalize = angle => ( angle % fullTurn + fullTurn ) % fullTurn;

			Geom_CylindricalSurface surface = new Geom_CylindricalSurface( cylinderCoordinate, radius );
			ShapeAnalysis_Surface analysis = new ShapeAnalysis_Surface( surface );
			List<gp_Pnt2d> uvPoints = points.Select( point => analysis.ValueOfUV( point, GEOMETRY_TOLERANCE ) ).ToList();

			double seam = normalize( seamAngle_rad ?? 0 );
			if( !seamAngle_rad.HasValue && uvPoints.Count > 0 ) {
				List<double> angles = uvPoints.Select( point => normalize( point.X() ) ).OrderBy( angle => angle ).ToList();
				double largestGap = -1;
				for( int index = 0; index < angles.Count; ++index ) {
					double next = index + 1 < angles.Count
						? angles[ index + 1 ] : angles[ 0 ] + fullTurn;
					double gap = next - angles[ index ];
					if( gap > largestGap ) {
						largestGap = gap;
						seam = normalize( angles[ index ] + gap / 2 );
					}
				}
			}
			usedSeamAngle_rad = seam;
			return uvPoints.Select( point => new gp_Pnt2d( normalize( point.X() - seam ), point.Y() ) ).ToList();
		}


		#region FlyCut

		public static List<string> SortFlyCutOnPlane( List<string> pathIDList, gp_Ax3 planeCoordinate,
			bool leftOrRight, bool topOrBottom, bool verticalOrHorizontal, double bandwidth_mm,
			out Dictionary<string, int> groupIndexByPathID )
		{
			groupIndexByPathID = new Dictionary<string, int>();
			if( pathIDList == null || pathIDList.Count < 2 || planeCoordinate == null
				|| pathIDList.Distinct().Count() != pathIDList.Count ) {
				return new List<string>();
			}

			// Project contour centers onto the plane's horizontal and vertical axes
			gp_Vec planeU = new gp_Vec( planeCoordinate.XDirection() );
			gp_Vec planeV = new gp_Vec( planeCoordinate.YDirection() );
			Dictionary<string, gp_Pnt2d> coordinates = new Dictionary<string, gp_Pnt2d>();
			foreach( string pathID in pathIDList ) {
				if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache cache ) ) {
					return new List<string>();
				}
				gp_Pnt center = cache.ComputeRefCenterDir?.Location();
				if( center == null ) {
					return new List<string>();
				}
				gp_Vec offset = new gp_Vec( planeCoordinate.Location(), center );
				double horizontal = offset.Dot( planeU );
				double vertical = offset.Dot( planeV );
				if( double.IsNaN( horizontal ) || double.IsInfinity( horizontal )
					|| double.IsNaN( vertical ) || double.IsInfinity( vertical ) ) {
					return new List<string>();
				}
				coordinates.Add( pathID, new gp_Pnt2d( horizontal, vertical ) );
			}

			// Choose the grouping axis and the initial snake direction
			Func<string, double> groupKey = pathID => verticalOrHorizontal
				? coordinates[ pathID ].X() : coordinates[ pathID ].Y();
			Func<string, double> snakeKey = pathID => verticalOrHorizontal
				? coordinates[ pathID ].Y() : coordinates[ pathID ].X();
			bool groupDescending = verticalOrHorizontal ? !leftOrRight : topOrBottom;
			bool firstSnakeDescending = verticalOrHorizontal ? topOrBottom : !leftOrRight;

			// Use a fixed group anchor to prevent chained tolerance expansion
			List<string> groupedOrder = pathIDList.OrderBy( groupKey ).ToList();
			List<List<string>> groups = new List<List<string>>();
			double anchor = groupKey( groupedOrder[ 0 ] );
			foreach( string pathID in groupedOrder ) {
				double groupValue = groupKey( pathID );
				if( groups.Count == 0 || ( groupValue - anchor ) > bandwidth_mm ) {
					groups.Add( new List<string>() );
					anchor = groupValue;
				}
				groups.Last().Add( pathID );
			}
			if( groupDescending ) {
				groups.Reverse();
			}

			// Alternate the snake direction and record each path's visit-group index
			List<string> result = new List<string>();
			for( int i = 0; i < groups.Count; ++i ) {
				bool descending = ( i % 2 ) == 0 ? firstSnakeDescending : !firstSnakeDescending;
				IOrderedEnumerable<string> ordered = descending
					? groups[ i ].OrderByDescending( snakeKey ) : groups[ i ].OrderBy( snakeKey );
				ordered = groupDescending ? ordered.ThenByDescending( groupKey ) : ordered.ThenBy( groupKey );
				foreach( string pathID in ordered.ThenBy( pathID => pathID, StringComparer.Ordinal ) ) {
					groupIndexByPathID.Add( pathID, i );
					result.Add( pathID );
				}
			}
			return result;
		}

		// groupWidth input: degrees vertically, mm horizontally.
		public static List<string> SortFlyCut( List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation,
			bool leftOrRight, bool topOrBottom, bool verticalOrHorizontal, double groupWidth,
			out Dictionary<string, int> groupIndexByPathID )
		{
			groupIndexByPathID = new Dictionary<string, int>();
			if( pathIDList == null || pathIDList.Count < 2 || axisDir == null || axisLocation == null
				|| pathIDList.Distinct().Count() != pathIDList.Count ) {
				return new List<string>();
			}

			// Obtain each contour's axial position and circumferential angle
			List<CylinderPathInfo> infos = BuildCylinderPathInfoList( pathIDList, axisDir, axisLocation );
			if( infos.Count != pathIDList.Count ) {
				return new List<string>();
			}

			// Unroll at the largest angular gap so nearby paths do not straddle the seam
			List<CylinderPathInfo> angular = infos.OrderBy( info => info.Rotation ).ToList();

			double fullTurn = 2 * Math.PI;
			double largestGap = -1;
			double seam = 0;

			for( int i = 0; i < angular.Count; ++i ) {
				double next = i + 1 < angular.Count
					? angular[ i + 1 ].Rotation : angular[ 0 ].Rotation + fullTurn;
				double gap = next - angular[ i ].Rotation;
				if( gap > largestGap ) {
					largestGap = gap;
					seam = ( angular[ i ].Rotation + gap / 2 ) % fullTurn;
				}
			}
			infos = infos
				.Select( info => new CylinderPathInfo( info.PathID, info.Extrusion, ( info.Rotation - seam + fullTurn ) % fullTurn ) )
				.ToList();

			// Choose the grouping axis, traversal directions, and matching tolerance units
			bool groupByExtrusion = !verticalOrHorizontal;
			bool groupDescending = groupByExtrusion ? topOrBottom : !leftOrRight;
			bool firstSnakeDescending = groupByExtrusion ? !leftOrRight : topOrBottom;

			double tolerance = verticalOrHorizontal ? ( groupWidth * Math.PI / 180.0 ) : groupWidth;

			// Use a fixed group anchor to prevent chained tolerance expansion
			List<CylinderPathInfo> groupedOrder = infos
				.OrderBy( info => groupByExtrusion ? info.Extrusion : info.Rotation )
				.ThenBy( info => info.Rotation )
				.ToList();
			List<List<CylinderPathInfo>> groups = new List<List<CylinderPathInfo>>();
			double anchor = groupByExtrusion ? groupedOrder[ 0 ].Extrusion : groupedOrder[ 0 ].Rotation;
			foreach( CylinderPathInfo info in groupedOrder ) {
				double groupValue = groupByExtrusion ? info.Extrusion : info.Rotation;
				if( groups.Count == 0 || ( groupValue - anchor ) > tolerance ) {
					groups.Add( new List<CylinderPathInfo>() );
					anchor = groupValue;
				}
				groups.Last().Add( info );
			}
			if( groupDescending ) {
				groups.Reverse();
			}

			// Alternate snake traversal while preserving the within-group floating-point tolerance
			List<string> result = new List<string>();
			for( int i = 0; i < groups.Count; ++i ) {
				bool descending = ( i % 2 ) == 0 ? firstSnakeDescending : !firstSnakeDescending;
				List<CylinderPathInfo> ordered = OrderGroupBySnakeAxis(
					groups[ i ], groupByExtrusion, descending, groupDescending );
				foreach( CylinderPathInfo info in ordered ) {
					groupIndexByPathID.Add( info.PathID, i );
					result.Add( info.PathID );
				}
			}
			return result;
		}

		#endregion

		// greedy nearest neighbor sort
		public static List<string> SortByShortestPath( List<string> pathIDList, string startPathID )
		{
			if( pathIDList == null || pathIDList.Count == 0 || string.IsNullOrEmpty( startPathID ) ) {
				return new List<string>();
			}

			gp_Pnt currentPoint = CacheHelper.GetProcessStartPoint( startPathID ).Point;
			if( currentPoint == null ) {
				return new List<string>();
			}

			List<string> result = new List<string> { startPathID };
			bool[] visited = new bool[ pathIDList.Count ];
			int startIdx = pathIDList.IndexOf( startPathID );
			visited[ startIdx ] = true;
			int visitedCount = 1;

			while( visitedCount < pathIDList.Count ) {
				double minDistanceSq = double.MaxValue;
				int nearestIdx = -1;
				gp_Pnt nearestPoint = null;

				for( int i = 0; i < pathIDList.Count; i++ ) {
					if( visited[ i ] ) {
						continue;
					}
					gp_Pnt candidatePoint = CacheHelper.GetProcessStartPoint( pathIDList[ i ] ).Point;
					double distanceSq = ( candidatePoint == null )
						? double.MaxValue
						: currentPoint.SquareDistance( candidatePoint );

					if( distanceSq < minDistanceSq ) {
						minDistanceSq = distanceSq;
						nearestPoint = candidatePoint;
						nearestIdx = i;
					}
				}

				if( nearestIdx == -1 ) {
					break;
				}

				currentPoint = nearestPoint;
				visited[ nearestIdx ] = true;
				visitedCount++;
				result.Add( pathIDList[ nearestIdx ] );
			}
			return result;
		}

		// cylinder horizontal: group by extrusion rows (mm), snake through rotation within each row
		public static List<string> SortCylinderHorizontal(
			List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation,
			bool isExtrusionDescending, bool isRotationDescending,
			double extrusionBandwidth_mm )
		{
			List<CylinderPathInfo> pathInfoList = BuildCylinderPathInfoList( pathIDList, axisDir, axisLocation );
			if( pathInfoList.Count == 0 ) {
				return new List<string>();
			}

			// horizontal groups by extrusion, so tolerance is in mm
			double groupTolerance = extrusionBandwidth_mm;
			return SnakeSortByGroup( pathInfoList, GroupAxis.Extrusion, isExtrusionDescending, isRotationDescending, groupTolerance );
		}

		// cylinder vertical: group by rotation columns (degree), snake through extrusion within each column
		// starts from the group containing the extreme extrusion path, then traverses circularly
		public static List<string> SortCylinderVertical(
			List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation,
			bool isRotationDescending, bool isExtrusionDescending,
			double rotationBandwidth_deg )
		{
			List<CylinderPathInfo> pathInfoList = BuildCylinderPathInfoList( pathIDList, axisDir, axisLocation );
			if( pathInfoList.Count == 0 ) {
				return new List<string>();
			}

			double groupTolerance = rotationBandwidth_deg * Math.PI / 180.0;

			// sort by rotation to assign group index
			List<CylinderPathInfo> sorted = pathInfoList.OrderBy( p => p.Rotation ).ToList();

			int groupIndex = 0;
			double groupAnchor = sorted[ 0 ].Rotation;
			foreach( CylinderPathInfo info in sorted ) {
				if( info.Rotation - groupAnchor > groupTolerance ) {
					groupIndex++;
					groupAnchor = info.Rotation;
				}
				info.GroupIndex = groupIndex;
			}

			// merge wrap-around groups at 0/2PI boundary
			if( groupIndex > 0 ) {
				double wrapGap = 2.0 * Math.PI - sorted[ sorted.Count - 1 ].Rotation + sorted[ 0 ].Rotation;
				if( wrapGap <= groupTolerance ) {
					int lastGroup = groupIndex;
					foreach( CylinderPathInfo info in sorted ) {
						if( info.GroupIndex == lastGroup ) {
							info.GroupIndex = 0;
						}
					}
					groupIndex--;
				}
			}

			// build group map
			Dictionary<int, List<CylinderPathInfo>> groupMap = new Dictionary<int, List<CylinderPathInfo>>();
			foreach( CylinderPathInfo info in sorted ) {
				if( !groupMap.ContainsKey( info.GroupIndex ) ) {
					groupMap[ info.GroupIndex ] = new List<CylinderPathInfo>();
				}
				groupMap[ info.GroupIndex ].Add( info );
			}

			int totalGroups = groupIndex + 1;

			// find starting group: the group containing the path with extreme extrusion value
			int startGroup = 0;
			if( isExtrusionDescending ) {
				// high to low: start from the group with the highest path
				double maxExtrusion = double.MinValue;
				foreach( CylinderPathInfo info in sorted ) {
					if( info.Extrusion > maxExtrusion ) {
						maxExtrusion = info.Extrusion;
						startGroup = info.GroupIndex;
					}
				}
			}
			else {
				// low to high: start from the group with the lowest path
				double minExtrusion = double.MaxValue;
				foreach( CylinderPathInfo info in sorted ) {
					if( info.Extrusion < minExtrusion ) {
						minExtrusion = info.Extrusion;
						startGroup = info.GroupIndex;
					}
				}
			}

			// snake traverse with circular group order starting from startGroup
			List<string> result = new List<string>();
			for( int i = 0; i < totalGroups; i++ ) {
				// circular traversal following the rotation direction
				int targetGroup;
				if( isRotationDescending ) {
					// clockwise: startGroup, startGroup-1, ..., wrap around
					targetGroup = ( startGroup - i + totalGroups ) % totalGroups;
				}
				else {
					// counter-clockwise: startGroup, startGroup+1, ..., wrap around
					targetGroup = ( startGroup + i ) % totalGroups;
				}

				List<CylinderPathInfo> groupPaths = groupMap[ targetGroup ];

				// alternate extrusion direction each group (snake pattern)
				bool descending;
				if( i % 2 == 0 ) {
					descending = isExtrusionDescending;
				}
				else {
					descending = !isExtrusionDescending;
				}

				// order by extrusion (snake axis), tie-break by rotation (cross axis)
				List<CylinderPathInfo> orderedPaths = OrderGroupBySnakeAxis( groupPaths, false, descending, isRotationDescending );
				foreach( CylinderPathInfo path in orderedPaths ) {
					result.Add( path.PathID );
				}
			}
			return result;
		}

		// convert path list to unrolled cylinder coordinates (extrusion, rotation)
		static List<CylinderPathInfo> BuildCylinderPathInfoList( List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation )
		{
			if( pathIDList == null || pathIDList.Count == 0 ) {
				return new List<CylinderPathInfo>();
			}

			gp_Dir refX = BuildPerpendicularDir( axisDir );
			gp_Dir refY = new gp_Dir( axisDir.Crossed( refX ).XYZ() );

			List<CylinderPathInfo> pathInfoList = new List<CylinderPathInfo>();
			foreach( string pathID in pathIDList ) {
				if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
					continue;
				}

				gp_Pnt center = pathCache.ComputeRefCenterDir.Location();
				double vx = center.X() - axisLocation.X();
				double vy = center.Y() - axisLocation.Y();
				double vz = center.Z() - axisLocation.Z();

				// project onto axis
				double extrusion = vx * axisDir.X() + vy * axisDir.Y() + vz * axisDir.Z();

				// radial vector (remove axial component)
				double radialX = vx - extrusion * axisDir.X();
				double radialY = vy - extrusion * axisDir.Y();
				double radialZ = vz - extrusion * axisDir.Z();

				// angle on perpendicular reference frame, normalized to [0, 2PI)
				double projX = radialX * refX.X() + radialY * refX.Y() + radialZ * refX.Z();
				double projY = radialX * refY.X() + radialY * refY.Y() + radialZ * refY.Z();
				double rotation = 0;
				double radialLength = Math.Sqrt( projX * projX + projY * projY );
				if( radialLength > RADIAL_EPSILON ) {
					rotation = Math.Atan2( projY, projX );
					if( rotation < 0 ) {
						rotation += 2.0 * Math.PI;
					}
				}
				pathInfoList.Add( new CylinderPathInfo( pathID, extrusion, rotation ) );
			}
			return pathInfoList;
		}

		// snake sort: group by groupAxis with given tolerance, then snake traverse the other axis
		static List<string> SnakeSortByGroup(
			List<CylinderPathInfo> pathInfoList,
			GroupAxis groupAxis,
			bool isGroupDescending,
			bool isSnakeStartDescending,
			double groupTolerance )
		{
			bool groupByExtrusion = ( groupAxis == GroupAxis.Extrusion );

			// sort by group axis to assign group index
			List<CylinderPathInfo> sorted = groupByExtrusion
				? pathInfoList.OrderBy( p => p.Extrusion ).ToList()
				: pathInfoList.OrderBy( p => p.Rotation ).ToList();

			int groupIndex = 0;
			double groupAnchor = groupByExtrusion ? sorted[ 0 ].Extrusion : sorted[ 0 ].Rotation;
			foreach( CylinderPathInfo info in sorted ) {
				double currentValue = groupByExtrusion ? info.Extrusion : info.Rotation;
				if( currentValue - groupAnchor > groupTolerance ) {
					groupIndex++;
					groupAnchor = currentValue;
				}
				info.GroupIndex = groupIndex;
			}

			// merge wrap-around groups at 0/2PI boundary
			if( !groupByExtrusion && groupIndex > 0 ) {
				double wrapGap = 2.0 * Math.PI - sorted[ sorted.Count - 1 ].Rotation + sorted[ 0 ].Rotation;
				if( wrapGap <= groupTolerance ) {
					int lastGroup = groupIndex;
					foreach( CylinderPathInfo info in sorted ) {
						if( info.GroupIndex == lastGroup ) {
							info.GroupIndex = 0;
						}
					}
					groupIndex--;
				}
			}

			// pre-group to avoid repeated Where scans
			Dictionary<int, List<CylinderPathInfo>> groupMap = new Dictionary<int, List<CylinderPathInfo>>();
			foreach( CylinderPathInfo info in sorted ) {
				if( !groupMap.ContainsKey( info.GroupIndex ) ) {
					groupMap[ info.GroupIndex ] = new List<CylinderPathInfo>();
				}
				groupMap[ info.GroupIndex ].Add( info );
			}

			// snake traverse each group
			int totalGroups = groupIndex + 1;
			List<string> result = new List<string>();

			for( int i = 0; i < totalGroups; i++ ) {
				int targetGroup = isGroupDescending ? ( totalGroups - 1 - i ) : i;
				List<CylinderPathInfo> groupPaths = groupMap[ targetGroup ];

				// alternate direction each group
				bool descending = ( i % 2 == 0 ) ? isSnakeStartDescending : !isSnakeStartDescending;

				// order by snake axis, breaking ties (same level within tolerance) by cross axis
				List<CylinderPathInfo> orderedPaths = OrderGroupBySnakeAxis( groupPaths, groupByExtrusion, descending, isGroupDescending );
				foreach( CylinderPathInfo path in orderedPaths ) {
					result.Add( path.PathID );
				}
			}
			return result;
		}

		// primary order is the snake axis. if two paths' snake-axis values differ by less than the
		// floating-point tolerance they are treated as the SAME value and ordered by the cross axis
		static List<CylinderPathInfo> OrderGroupBySnakeAxis( List<CylinderPathInfo> groupPaths, bool groupByExtrusion, bool descending, bool crossDescending )
		{
			if( groupPaths == null || groupPaths.Count == 0 ) {
				return new List<CylinderPathInfo>();
			}

			// snake axis = the direction we traverse within this group
			// cross axis = the other axis, used for tie-breaking
			Func<CylinderPathInfo, double> snakeKey;
			Func<CylinderPathInfo, double> crossKey;
			double snakeFloatTolerance;

			if( groupByExtrusion ) {
				// horizontal: groups are rows of extrusion, snake traverses along rotation
				snakeKey = p => p.Rotation;
				crossKey = p => p.Extrusion;
				snakeFloatTolerance = ROTATION_FLOAT_TOLERANCE;
			}
			else {
				// vertical: groups are columns of rotation, snake traverses along extrusion
				snakeKey = p => p.Extrusion;
				crossKey = p => p.Rotation;
				snakeFloatTolerance = EXTRUSION_FLOAT_TOLERANCE;
			}

			// primary sort by snake axis (respect the snake traversal direction)
			List<CylinderPathInfo> snakeSorted;
			if( descending ) {
				snakeSorted = groupPaths.OrderByDescending( snakeKey ).ToList();
			}
			else {
				snakeSorted = groupPaths.OrderBy( snakeKey ).ToList();
			}

			// consecutive paths within the float tolerance of the level anchor are the same level;
			// order them by the cross axis. anchoring to the level's first value (not the previous
			// neighbour) avoids single-linkage drift.
			List<CylinderPathInfo> result = new List<CylinderPathInfo>();
			int idx = 0;
			while( idx < snakeSorted.Count ) {
				double levelAnchor = snakeKey( snakeSorted[ idx ] );
				int end = idx;
				while( end < snakeSorted.Count && Math.Abs( snakeKey( snakeSorted[ end ] ) - levelAnchor ) <= snakeFloatTolerance ) {
					end++;
				}

				// within the same level, tie-break by the cross axis following the group-axis direction
				List<CylinderPathInfo> level = snakeSorted.GetRange( idx, end - idx );
				if( crossDescending ) {
					level = level.OrderByDescending( crossKey ).ToList();
				}
				else {
					level = level.OrderBy( crossKey ).ToList();
				}
				result.AddRange( level );
				idx = end;
			}
			return result;
		}

		static gp_Dir BuildPerpendicularDir( gp_Dir axisDir )
		{
			bool isAlongX = axisDir.IsParallel( new gp_Dir( 1, 0, 0 ), PARALLEL_TOLERANCE );
			bool isAlongY = axisDir.IsParallel( new gp_Dir( 0, 1, 0 ), PARALLEL_TOLERANCE );

			gp_Dir candidate;
			if( isAlongX ) {
				candidate = new gp_Dir( 0, 1, 0 );
			}
			else if( isAlongY ) {
				candidate = new gp_Dir( 0, 0, 1 );
			}
			else {
				// Z axis or any other direction
				candidate = new gp_Dir( 1, 0, 0 );
			}
			gp_XYZ cross = axisDir.XYZ().Crossed( candidate.XYZ() );
			return new gp_Dir( cross );
		}

		const double RADIAL_EPSILON = 1e-6;
		const double PARALLEL_TOLERANCE = 1e-3;

		// paths closer than this on the snake axis are tie-broken by the cross axis
		const double EXTRUSION_FLOAT_TOLERANCE = 1e-3; // mm
		const double ROTATION_FLOAT_TOLERANCE = 1e-3 * Math.PI / 180.0; // 1e-3 deg in rad

		enum GroupAxis
		{
			Extrusion,
			Rotation
		}

		class CylinderPathInfo
		{
			public CylinderPathInfo( string pathID, double extrusion, double rotation )
			{
				PathID = pathID;
				Extrusion = extrusion;
				Rotation = rotation;
			}

			public string PathID { get; }
			public double Extrusion { get; }
			public double Rotation { get; }
			public int GroupIndex { get; set; }
		}
	}
}
