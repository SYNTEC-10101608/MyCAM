using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAM.Helper
{
	public static class ToolVecHelper
	{
		public static void SetToolVec()
		{
			for( int i = 0; i < m_CADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = m_CADPointList[ i ];
				CAMPoint camPoint;
				if( m_CraftData.IsToolVecReverse ) {
					camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st.Reversed() );
				}
				else {
					camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				}
				m_CAMPointList.Add( camPoint );
			}
			ModifyToolVec();
		}

		static void ModifyToolVec()
		{
			if( m_CraftData.ToolVecModifyMap.Count == 0 ) {
				return;
			}

			// all tool vector are modified to the same value, no need to do interpolation
			if( m_CraftData.ToolVecModifyMap.Count == 1 ) {
				gp_Vec newVec = GetVecFromAB( m_CAMPointList[ m_CraftData.ToolVecModifyMap.Keys.First() ],
					m_CraftData.ToolVecModifyMap.Values.First().Item1 * Math.PI / 180,
					m_CraftData.ToolVecModifyMap.Values.First().Item2 * Math.PI / 180 );
				foreach( CAMPoint camPoint in m_CAMPointList ) {
					camPoint.ToolVec = new gp_Dir( newVec.XYZ() );
				}
			}

			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList();

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( nStartIndex, nEndIndex );
			}
		}

		static gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = camPoint.CADPoint.TangentVec;
			gp_Dir z = camPoint.CADPoint.NormalVec_1st;
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

		static List<Tuple<int, int>> GetInterpolateIntervalList()
		{
			// sort the modify data by index
			List<int> indexInOrder = m_CraftData.ToolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if( IsClosed ) {

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

		static void InterpolateToolVec( int nStartIndex, int nEndIndex )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + m_CAMPointList.Count : nEndIndex;

			// get the start and end tool vector
			gp_Vec startVec = GetVecFromAB( m_CAMPointList[ nStartIndex ],
				m_CraftData.ToolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
				m_CraftData.ToolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
			gp_Vec endVec = GetVecFromAB( m_CAMPointList[ nEndIndex ],
				m_CraftData.ToolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
				m_CraftData.ToolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				totaldistance += m_CAMPointList[ i % m_CAMPointList.Count ].CADPoint.Point.SquareDistance( m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.Point );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += m_CAMPointList[ i % m_CAMPointList.Count ].CADPoint.Point.SquareDistance( m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.Point );
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				m_CAMPointList[ i % m_CAMPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
			}
		}
	}
}
