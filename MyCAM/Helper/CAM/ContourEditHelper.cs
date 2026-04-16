using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class ContourEditHelper
	{
		public static List<CADPoint> ApplyCADPointModify(
			List<CADPoint> cadPointList,
			IReadOnlyDictionary<int, CADPointModifyData> cadPointModifyMap,
			bool isClosed )
		{
			if( cadPointList == null || cadPointList.Count == 0 ) {
				return cadPointList?.Select( p => p.Clone() ).ToList() ?? new List<CADPoint>();
			}

			// deep copy so we never mutate the original list
			List<CADPoint> result = cadPointList.Select( p => p.Clone() ).ToList();

			// if no control points, nothing to do
			if( cadPointModifyMap == null || cadPointModifyMap.Count == 0 ) {
				return result;
			}

			// build a working copy of the map so we can add boundary control points
			Dictionary<int, CADPointModifyData> workMap =
				cadPointModifyMap.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Clone() );
			AddBoundaryControlPoints( ref workMap, result, isClosed );
			ApplyDisplacementInterpolation( result, workMap );
			return result;
		}

		// Ensure index 0 and index (last) exist in the map.
		static void AddBoundaryControlPoints(
			ref Dictionary<int, CADPointModifyData> workMap,
			List<CADPoint> pointList,
			bool isClosed )
		{
			List<int> sortedKeys = workMap.Keys.ToList();
			sortedKeys.Sort();
			bool hasFirst = workMap.ContainsKey( 0 );
			bool hasLast = workMap.ContainsKey( pointList.Count - 1 );
			if( hasFirst && hasLast ) {
				return;
			}
			if( isClosed ) {
				AddBoundaryControlPoints_Closed( ref workMap, pointList, sortedKeys );
			}
			else {
				AddBoundaryControlPoints_Open( ref workMap, pointList, sortedKeys );
			}
		}

		static void AddBoundaryControlPoints_Open(
			ref Dictionary<int, CADPointModifyData> workMap,
			List<CADPoint> pointList,
			List<int> sortedKeys )
		{
			int lastIndex = pointList.Count - 1;

			// --- fill index 0 if missing ---
			if( !workMap.ContainsKey( 0 ) ) {

				// Clamp to first control point; no extrapolation.
				workMap[ 0 ] = workMap[ sortedKeys[ 0 ] ].Clone();
			}

			// --- fill lastIndex if missing ---
			if( !workMap.ContainsKey( lastIndex ) ) {

				// Clamp to last control point; no extrapolation.
				workMap[ lastIndex ] = workMap[ sortedKeys[ sortedKeys.Count - 1 ] ].Clone();
			}
		}

		static void AddBoundaryControlPoints_Closed(
			ref Dictionary<int, CADPointModifyData> workMap,
			List<CADPoint> pointList,
			List<int> sortedKeys )
		{
			int lastIndex = pointList.Count - 1;
			bool hasFirst = workMap.ContainsKey( 0 );
			bool hasLast = workMap.ContainsKey( lastIndex );
			if( hasFirst && hasLast ) {
				return;
			}
			int firstCtrl = sortedKeys[ 0 ];
			int lastCtrl = sortedKeys[ sortedKeys.Count - 1 ];

			// Single control point: clamp both boundary points to that value.
			if( sortedKeys.Count == 1 ) {
				if( !hasLast ) {
					workMap[ lastIndex ] = workMap[ firstCtrl ].Clone();
				}
				if( !hasFirst ) {
					workMap[ 0 ] = workMap[ lastCtrl ].Clone();
				}
				return;
			}

			// Treat the path as a ring: the wrap-around interval goes from
			CADPointModifyData dataFirst = workMap[ firstCtrl ];
			CADPointModifyData dataLast = workMap[ lastCtrl ];
			double distLastCtrlToEnd = AccumulatedDistance( pointList, lastCtrl, lastIndex );
			double distStartToFirstCtrl = AccumulatedDistance( pointList, 0, firstCtrl );

			// gapDist accounts for the closing step between lastIndex and index 0.
			double gapDist = pointList[ lastIndex ].Point.Distance( pointList[ 0 ].Point );

			// lastCtrl → lastIndex → [gap] → 0 → firstCtrl.
			double totalWrapDist = distLastCtrlToEnd + gapDist + distStartToFirstCtrl;

			if( !hasLast ) {
				// lastIndex is distLastCtrlToEnd into the wrap-around interval.
				double t = ( totalWrapDist > GEOM_TOLERANCE )
					? distLastCtrlToEnd / totalWrapDist
					: 0.0;
				workMap[ lastIndex ] = Lerp( dataLast, dataFirst, t );
			}

			if( !hasFirst ) {
				// index 0 is distLastCtrlToEnd + gapDist into the wrap-around interval.
				double t = ( totalWrapDist > GEOM_TOLERANCE )
					? ( distLastCtrlToEnd + gapDist ) / totalWrapDist
					: 0.0;
				workMap[ 0 ] = Lerp( dataLast, dataFirst, t );
			}
		}

		static void ApplyDisplacementInterpolation(
			List<CADPoint> pointList,
			IReadOnlyDictionary<int, CADPointModifyData> workMap )
		{
			List<int> sortedKeys = workMap.Keys.ToList();
			sortedKeys.Sort();

			// Interpolate interior points of every interval first (control points are not touched here).
			for( int k = 0; k < sortedKeys.Count - 1; k++ ) {
				InterpolateInterval( pointList, workMap, sortedKeys[ k ], sortedKeys[ k + 1 ] );
			}

			// Apply the recorded displacement to every control point after all interpolation is done.
			foreach( KeyValuePair<int, CADPointModifyData> entry in workMap ) {
				ApplyDisplacement( pointList[ entry.Key ], entry.Value );
			}
		}

		// Interpolate "in" the interval, the control points are not modified here
		static void InterpolateInterval(
			List<CADPoint> pointList,
			IReadOnlyDictionary<int, CADPointModifyData> workMap,
			int startIdx,
			int endIdx )
		{
			// No interior points when the interval spans one step or less.
			if( endIdx - startIdx < 2 ) {
				return;
			}
			CADPointModifyData startData = workMap[ startIdx ];
			CADPointModifyData endData = workMap[ endIdx ];

			// Step 1: accumulate total arc length using original (not yet translated) positions.
			double totalDist = AccumulatedDistance( pointList, startIdx, endIdx );

			// Step 2: pre-compute t for every interior index using original positions.
			int interiorCount = endIdx - startIdx - 1;
			double[] tValues = new double[ interiorCount ];
			double accumulated = 0.0;
			for( int i = startIdx; i < endIdx - 1; i++ ) {
				accumulated += pointList[ i ].Point.Distance( pointList[ i + 1 ].Point );
				tValues[ i - startIdx ] = ( totalDist > GEOM_TOLERANCE ) ? accumulated / totalDist : 0.0;
			}

			// Step 3: apply interpolated displacement to interior points only.
			for( int i = startIdx + 1; i < endIdx; i++ ) {
				double t = tValues[ i - startIdx - 1 ];
				ApplyDisplacement( pointList[ i ], Lerp( startData, endData, t ) );
			}
		}

		// Apply the displacement to the given point.
		static void ApplyDisplacement( CADPoint cadPoint, CADPointModifyData disp )
		{
			if( Math.Abs( disp.DX ) < GEOM_TOLERANCE &&
				Math.Abs( disp.DY ) < GEOM_TOLERANCE &&
				Math.Abs( disp.DZ ) < GEOM_TOLERANCE ) {
				return;
			}
			cadPoint.Translate( new gp_Vec( disp.DX, disp.DY, disp.DZ ) );
		}

		// Accumulated arc-length distance from index a to index b (a <= b).
		static double AccumulatedDistance( List<CADPoint> pointList, int a, int b )
		{
			double dist = 0.0;
			for( int i = a; i < b; i++ ) {
				dist += pointList[ i ].Point.Distance( pointList[ i + 1 ].Point );
			}
			return dist;
		}

		// Linear interpolation between two CADPointModifyData at parameter t.
		static CADPointModifyData Lerp( CADPointModifyData a, CADPointModifyData b, double t )
		{
			return new CADPointModifyData(
				a.DX + ( b.DX - a.DX ) * t,
				a.DY + ( b.DY - a.DY ) * t,
				a.DZ + ( b.DZ - a.DZ ) * t );
		}

		const double GEOM_TOLERANCE = 1e-3;
	}
}
