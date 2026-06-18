using MyCAM.Data;
using MyCAM.PathCache;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	// pure sorting algorithms for path ordering, no UI dependency
	internal static class PathSortHelper
	{
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
