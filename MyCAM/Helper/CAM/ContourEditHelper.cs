using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class ContourEditHelper
	{
		// Apply CAD point local displacement to cadPointList.
		// cadPointModifyMap : key = original CAD index, value = displacement data (DX, DY, DZ)
		// isClosed          : whether the contour is a closed path (no closing point appended yet)
		// Returns a new list of CADPoints with displacements applied (deep copy, input is not modified).
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
			Dictionary<int, CADPointModifyData> workMap = cadPointModifyMap
				.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Clone() );

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
				AddBoundaryControlPointsClosed( ref workMap, pointList, sortedKeys );
			}
			else {
				AddBoundaryControlPointsOpen( ref workMap, pointList, sortedKeys );
			}
		}

		static void AddBoundaryControlPointsOpen(
			ref Dictionary<int, CADPointModifyData> workMap,
			List<CADPoint> pointList,
			List<int> sortedKeys )
		{
			int lastIndex = pointList.Count - 1;

			// --- fill index 0 if missing ---
			if( !workMap.ContainsKey( 0 ) ) {
				if( sortedKeys.Count == 1 ) {
					workMap[ 0 ] = workMap[ sortedKeys[ 0 ] ].Clone();
				}
				else {
					int idxA = sortedKeys[ 0 ];
					int idxB = sortedKeys[ 1 ];
					double dist_AB = AccumulatedDistance( pointList, idxA, idxB );
					double dist_0A = AccumulatedDistance( pointList, 0, idxA );
					workMap[ 0 ] = ExtrapolateBackward( workMap[ idxA ], workMap[ idxB ], dist_AB, dist_0A );
				}
				sortedKeys.Insert( 0, 0 );
			}

			// --- fill lastIndex if missing ---
			if( !workMap.ContainsKey( lastIndex ) ) {
				if( sortedKeys.Count == 1 ) {
					workMap[ lastIndex ] = workMap[ sortedKeys[ 0 ] ].Clone();
				}
				else {
					int n = sortedKeys.Count;
					int idxA = sortedKeys[ n - 2 ];
					int idxB = sortedKeys[ n - 1 ];
					double dist_AB = AccumulatedDistance( pointList, idxA, idxB );
					double dist_B_last = AccumulatedDistance( pointList, idxB, lastIndex );
					workMap[ lastIndex ] = ExtrapolateForward( workMap[ idxA ], workMap[ idxB ], dist_AB, dist_B_last );
				}
				sortedKeys.Add( lastIndex );
			}
		}

		static void AddBoundaryControlPointsClosed(
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

			// Treat the path as a ring: the wrap-around interval goes from lastCtrl ¡÷ lastIndex ¡÷ 0 ¡÷ firstCtrl.
			// totalWrapDist = dist(lastCtrl ¡÷ lastIndex) + dist(0 ¡÷ firstCtrl)
			int firstCtrl = sortedKeys[ 0 ];
			int lastCtrl = sortedKeys[ sortedKeys.Count - 1 ];

			CADPointModifyData dataFirst = workMap[ firstCtrl ];
			CADPointModifyData dataLast = workMap[ lastCtrl ];

			double distLastCtrlToEnd = AccumulatedDistance( pointList, lastCtrl, lastIndex );
			double distStartToFirstCtrl = AccumulatedDistance( pointList, 0, firstCtrl );
			double totalWrapDist = distLastCtrlToEnd + distStartToFirstCtrl;

			if( !hasLast ) {
				// lastIndex lies at distLastCtrlToEnd into the wrap-around interval
				double t = ( totalWrapDist > GEOM_TOLERANCE )
					? distLastCtrlToEnd / totalWrapDist
					: 0.0;
				workMap[ lastIndex ] = Lerp( dataLast, dataFirst, t );
				sortedKeys.Add( lastIndex );
			}

			if( !hasFirst ) {
				// index 0 lies at the same seam position as lastIndex on the ring
				double t = ( totalWrapDist > GEOM_TOLERANCE )
					? distLastCtrlToEnd / totalWrapDist
					: 0.0;
				workMap[ 0 ] = Lerp( dataLast, dataFirst, t );
				sortedKeys.Insert( 0, 0 );
			}
		}

		// Interpolate and apply displacements across every consecutive control-point interval.
		static void ApplyDisplacementInterpolation(
			List<CADPoint> pointList,
			IReadOnlyDictionary<int, CADPointModifyData> workMap )
		{
			List<int> sortedKeys = workMap.Keys.ToList();
			sortedKeys.Sort();

			for( int k = 0; k < sortedKeys.Count - 1; k++ ) {
				InterpolateInterval( pointList, workMap, sortedKeys[ k ], sortedKeys[ k + 1 ] );
			}
		}

		// Linearly interpolate displacement for all points in [startIdx, endIdx] by arc length.
		static void InterpolateInterval(
			List<CADPoint> pointList,
			IReadOnlyDictionary<int, CADPointModifyData> workMap,
			int startIdx,
			int endIdx )
		{
			if( endIdx <= startIdx ) {
				return;
			}

			CADPointModifyData startData = workMap[ startIdx ];
			CADPointModifyData endData = workMap[ endIdx ];

			// accumulate total arc length of this interval
			double totalDist = 0.0;
			for( int i = startIdx; i < endIdx; i++ ) {
				totalDist += pointList[ i ].Point.Distance( pointList[ i + 1 ].Point );
			}

			double accumulated = 0.0;
			for( int i = startIdx; i <= endIdx; i++ ) {
				double t = ( totalDist > GEOM_TOLERANCE ) ? accumulated / totalDist : 0.0;
				ApplyDisplacement( pointList[ i ], Lerp( startData, endData, t ) );

				if( i < endIdx ) {
					accumulated += pointList[ i ].Point.Distance( pointList[ i + 1 ].Point );
				}
			}
		}

		// Apply a displacement vector to a CADPoint (position only; direction vectors are unaffected).
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

		// Extrapolate backward from A by dist_0A, using the slope of A¡÷B.
		static CADPointModifyData ExtrapolateBackward(
			CADPointModifyData dataA,
			CADPointModifyData dataB,
			double dist_AB,
			double dist_0A )
		{
			if( dist_AB < GEOM_TOLERANCE ) {
				return dataA.Clone();
			}
			return Lerp( dataA, dataB, -dist_0A / dist_AB );
		}

		// Extrapolate forward past B by dist_B_end, using the slope of A¡÷B.
		static CADPointModifyData ExtrapolateForward(
			CADPointModifyData dataA,
			CADPointModifyData dataB,
			double dist_AB,
			double dist_B_end )
		{
			if( dist_AB < GEOM_TOLERANCE ) {
				return dataB.Clone();
			}
			return Lerp( dataA, dataB, 1.0 + dist_B_end / dist_AB );
		}

		const double GEOM_TOLERANCE = 1e-3;
	}
}
