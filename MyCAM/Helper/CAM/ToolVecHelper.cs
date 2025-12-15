using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class ToolVecHelper
	{
		public static void SetToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
			bool isClosed, bool isToolVecReverse )
		{
			// mark the modified point
			for(int i = 0; i < toolVecPointList.Count; i++ ) {
				if( !toolVecModifyMap.ContainsKey( i ) ) {
					continue;
				}
				toolVecPointList[ i ].IsToolVecModPoint = true;
			}

			ModifyToolVec( ref toolVecPointList, toolVecModifyMap, isClosed );
			if( isToolVecReverse ) {
				ReverseToolVec( ref toolVecPointList );
			}
		}

		static void ModifyToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
			bool isClosed )
		{
			if( toolVecModifyMap.Count == 0 ) {
				return;
			}

			// all tool vector are modified to the same value, no need to do interpolation
			if( toolVecModifyMap.Count == 1 ) {
				gp_Vec newVec = GetVecFromAB( toolVecPointList[ toolVecModifyMap.Keys.First() ],
					toolVecModifyMap.Values.First().Item1 * Math.PI / 180,
					toolVecModifyMap.Values.First().Item2 * Math.PI / 180 );
				foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
					toolVecPoint.ToolVec = new gp_Dir( newVec.XYZ() );
				}
			}

			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap, isClosed );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( ref toolVecPointList, toolVecModifyMap, nStartIndex, nEndIndex );
			}
		}

		static List<Tuple<int, int>> GetInterpolateIntervalList(
			IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
			bool isClosed )
		{
			// sort the modify data by index
			List<int> indexInOrder = toolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if( isClosed ) {

				// for closed path, the index is wrapped
				for( int i = 0; i < indexInOrder.Count; i++ ) {
					int nextIndex = ( i + 1 ) % indexInOrder.Count;
					intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ nextIndex ] ) );
				}
			}
			else {
				for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
					intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ i + 1 ] ) );
				}
			}
			return intervalList;
		}

		static void InterpolateToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
			int nStartIndex, int nEndIndex )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + toolVecPointList.Count : nEndIndex;

			// get the start and end tool vector
			gp_Vec startVec = GetVecFromAB( toolVecPointList[ nStartIndex ],
				toolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
				toolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
			gp_Vec endVec = GetVecFromAB( toolVecPointList[ nEndIndex ],
				toolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
				toolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				totaldistance += toolVecPointList[ i % toolVecPointList.Count ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) % toolVecPointList.Count ].Point );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += toolVecPointList[ i % toolVecPointList.Count ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) % toolVecPointList.Count ].Point );
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				toolVecPointList[ i % toolVecPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
			}
		}

		static gp_Vec GetVecFromAB( ISetToolVecPoint tooVecPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( tooVecPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = tooVecPoint.TangentVec;
			gp_Dir z = tooVecPoint.InitToolVec;
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

		static void ReverseToolVec( ref List<ISetToolVecPoint> toolVecPointList )
		{
			foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
				toolVecPoint.ToolVec = toolVecPoint.ToolVec.Reversed();
			}
		}
	}
}
