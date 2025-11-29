using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal class ToolVectorHelper
	{
		public static void CalculateToolVector( ref List<IToolVecCAMPointInfo> camPointInfoList, bool isToolVecReverse, bool isPathReverse, bool isClosed )
		{
			if( camPointInfoList == null || camPointInfoList.Count == 0 ) {
				return;
			}

			// reverse all tool vec if needed
			if( isToolVecReverse ) {
				foreach( var camPntInfo in camPointInfoList ) {
					camPntInfo.ToolVec.Reverse();
				}
			}

			// find all ctrl pnt for interpolation
			List<int> ctrlPntIdx = new List<int>();
			for( int i = 0; i < camPointInfoList.Count; i++ ) {
				if( camPointInfoList[ i ].IsToolVecdPnt ) {
					ctrlPntIdx.Add( i );
				}
			}

			// do not have ctrl pnt
			if( ctrlPntIdx.Count == 0 ) {
				return;
			}

			// only one ctrl pnt, apply to all point
			if( ctrlPntIdx.Count == 1 ) {
				ApplySpecifiedVec( camPointInfoList, ctrlPntIdx[ 0 ], isPathReverse );
				return;
			}
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( ctrlPntIdx, isClosed );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( nStartIndex, nEndIndex, camPointInfoList, isPathReverse );
			}
		}

		static void ApplySpecifiedVec( List<IToolVecCAMPointInfo> pointInfoList, int nSpecifiedIdx, bool isPathReverse )
		{
			// get the specified tool vec
			gp_Vec SpecifiedVec = CalCtrlPntToolVec( pointInfoList[ nSpecifiedIdx ], isPathReverse );
			if( SpecifiedVec == null ) {
				return;
			}

			// apply to all point
			foreach( var pointInfo in pointInfoList ) {
				pointInfo.ToolVec = new gp_Dir( SpecifiedVec );
			}
		}

		static List<Tuple<int, int>> GetInterpolateIntervalList( List<int> ctrlIndex, bool isClosed )
		{

			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if( ctrlIndex.Count < 2 ) {
				return intervalList;
			}
			if( isClosed ) {

				// for closed path, the index is wrapped
				for( int i = 0; i < ctrlIndex.Count; i++ ) {
					int nextIndex = ( i + 1 ) % ctrlIndex.Count;
					intervalList.Add( new Tuple<int, int>( ctrlIndex[ i ], ctrlIndex[ nextIndex ] ) );
				}
			}
			else {
				for( int i = 0; i < ctrlIndex.Count - 1; i++ ) {
					intervalList.Add( new Tuple<int, int>( ctrlIndex[ i ], ctrlIndex[ i + 1 ] ) );
				}
			}
			return intervalList;
		}

		static void InterpolateToolVec( int nStartIndex, int nEndIndex, List<IToolVecCAMPointInfo> pathCAMInfo, bool isPathReverse )
		{
			// consider wrapped
			gp_Vec startVec = CalCtrlPntToolVec( pathCAMInfo[ nStartIndex ], isPathReverse );
			gp_Vec endVec = CalCtrlPntToolVec( pathCAMInfo[ nEndIndex ], isPathReverse );
			if( startVec == null || endVec == null ) {

				// should not run into this
				return;
			}

			// do interpolation
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + pathCAMInfo.Count : nEndIndex;
			QuaternionInterpolate( nStartIndex, nEndIndexModify, startVec, endVec, ref pathCAMInfo );
		}

		static gp_Vec CalCtrlPntToolVec( IToolVecCAMPointInfo controlPoint, bool isPathReverse )
		{
			if( !controlPoint.IsToolVecdPnt || controlPoint.ABValues == null ) {
				return null;
			}
			var abValues = controlPoint.ABValues;

			// use sharing point tangent if it exists and path is reversed
			gp_Dir tangentVec;
			if( isPathReverse && controlPoint.SharingPoint != null ) {
				tangentVec = controlPoint.SharingPoint.TangentVec;
			}
			else {
				tangentVec = controlPoint.Point.TangentVec;
			}
			return GetVecFromAB( controlPoint.ToolVec, tangentVec, abValues.Item1 * Math.PI / 180, abValues.Item2 * Math.PI / 180 );
		}

		static gp_Vec GetVecFromAB( gp_Dir initToolVec, gp_Dir tangentVec, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( initToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = tangentVec;
			gp_Dir z = initToolVec;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {
				X = 0;
				Z = 1;
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
		}

		static void QuaternionInterpolate( int nStartIndex, int nEndIndex, gp_Vec startVec, gp_Vec endVec, ref List<IToolVecCAMPointInfo> pathCAMInfo )
		{
			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndex; i++ ) {
				totaldistance += pathCAMInfo[ i % pathCAMInfo.Count ].DistanceToNext;
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndex; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += pathCAMInfo[ i % pathCAMInfo.Count ].DistanceToNext;
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				pathCAMInfo[ i % pathCAMInfo.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
			}
		}
	}
}
