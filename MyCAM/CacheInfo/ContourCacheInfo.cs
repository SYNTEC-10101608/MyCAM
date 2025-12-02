using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.CacheInfo
{
	public class ContourCacheInfo : ICacheInfo
	{
		public ContourCacheInfo( string szID, ContourGeomData geomData, CraftData craftData, bool isClose )
		{
			if( string.IsNullOrEmpty( szID ) || geomData == null || craftData == null ) {
				throw new ArgumentNullException( "ContourCacheInfo constructing argument null" );
			}
			if( geomData.CADPointList.Count == 0 ) {
				throw new ArgumentException( "ContourCacheInfo constructing argument empty cadPointList" );
			}
			UID = szID;
			m_CADPointList = geomData.CADPointList;
			m_CraftData = craftData;
			IsClosed = isClose;
			m_CraftData.ParameterChanged += SetCraftDataDirty;
			BuildCAMPointList();
		}

		public string UID
		{
			get; private set;
		}

		public PathType PathType
		{
			get
			{
				return PathType.Contour;
			}
		}

		#region computation result

		internal List<CAMPoint> CAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_CAMPointList;
			}
		}

		internal List<CAMPoint> LeadInCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadInCAMPointList;
			}
		}

		internal List<CAMPoint> LeadOutCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_LeadOutCAMPointList;
			}
		}

		internal List<CAMPoint> OverCutCAMPointList
		{
			get
			{
				if( m_IsCraftDataDirty ) {
					BuildCAMPointList();
				}
				return m_OverCutPointList;
			}
		}

		public CAMPoint GetProcessStartPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			CAMPoint camPoint = null;
			if( m_LeadInCAMPointList.Count > 0 && m_CraftData.LeadLineParam.LeadIn.Length > 0 ) {
				camPoint = m_LeadInCAMPointList.First().Clone();
			}
			else if( m_CAMPointList.Count > 0 ) {
				camPoint = m_CAMPointList.First().Clone();
			}
			return camPoint;
		}

		public CAMPoint GetProcessEndPoint()
		{
			if( m_IsCraftDataDirty ) {
				BuildCAMPointList();
			}
			CAMPoint camPoint = null;
			if( m_LeadOutCAMPointList.Count > 0 && m_CraftData.LeadLineParam.LeadOut.Length > 0 ) {
				camPoint = m_LeadOutCAMPointList.Last().Clone();
			}
			else if( m_OverCutPointList.Count > 0 && m_CraftData.OverCutLength > 0 ) {
				camPoint = m_OverCutPointList.Last().Clone();
			}
			else if( m_CAMPointList.Count > 0 ) {
				camPoint = m_CAMPointList.Last().Clone();
			}
			return camPoint;
		}
		#endregion

		#region API
		// when the shape has tranform, need to call this to update the cache info
		public void DoTransform()
		{
			BuildCAMPointList();
		}
		#endregion

		#region craft data
		public int GetPathStartPointIndex()
		{
			return m_CraftData.StartPointIndex;
		}

		public bool GetPathIsReverse()
		{
			return m_CraftData.IsReverse;
		}

		public LeadData GetPathLeadData()
		{
			return m_CraftData.LeadLineParam;
		}

		public double GetPathOverCutLength()
		{
			return m_CraftData.OverCutLength;
		}

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_CraftData.ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_CraftData.ToolVecModifyMap[ index ].Item2;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
				return false;
			}
		}

		public HashSet<int> GetToolVecModifyIndex()
		{
			HashSet<int> result = new HashSet<int>();
			foreach( int nIndex in m_CraftData.ToolVecModifyMap.Keys ) {
				result.Add( nIndex );
			}
			return result;
		}
		#endregion

		public bool IsClosed
		{
			get; private set;
		}

		void BuildCAMPointList()
		{
			m_IsCraftDataDirty = false;
			m_CAMPointList = new List<CAMPoint>();
			foreach( CADPoint cadPoint in m_CADPointList ) {
				CAMPoint camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				m_CAMPointList.Add( camPoint );
			}
			List<IToolVecPoint> toolVecPointList = m_CADPointList.Cast<IToolVecPoint>().ToList();
			ToolVecHelper.SetToolVec( ref toolVecPointList, m_CraftData.ToolVecModifyMap, IsClosed, m_CraftData.IsReverse );
			SetStartPoint();
			SetOrientation();

			// close the loop if is closed
			if( IsClosed && m_CAMPointList.Count > 0 ) {
				m_CAMPointList.Add( m_CAMPointList[ 0 ].Clone() );
			}

			// all CAM point are settled down, start set lead / overcut
			SetOverCut();
			SetLeadIn();
			SetLeadout();
		}

		void SetStartPoint()
		{
			// rearrange cam points to start from the strt index
			if( m_CraftData.StartPointIndex != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + m_CraftData.StartPointIndex ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}
		}

		void SetOrientation()
		{
			// reverse the cad points if is reverse
			if( m_CraftData.IsReverse ) {
				m_CAMPointList.Reverse();

				// modify start point index for closed path
				if( IsClosed ) {
					CAMPoint lastPoint = m_CAMPointList.Last();
					m_CAMPointList.Remove( lastPoint );
					m_CAMPointList.Insert( 0, lastPoint );
				}
			}
		}

		void SetCraftDataDirty()
		{
			if( !m_IsCraftDataDirty ) {
				m_IsCraftDataDirty = true;
			}
		}

		#region Over cut

		void SetOverCut()
		{
			m_OverCutPointList.Clear();
			if( m_CAMPointList.Count == 0 || m_CraftData.OverCutLength == 0 || !IsClosed ) {
				return;
			}
			double dTotalOverCutLength = 0;

			// end point is the start of over cut
			m_OverCutPointList.Add( m_CAMPointList.Last().Clone() );
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {

				// get this edge distance
				double dDistance = m_CAMPointList[ i ].CADPoint.Point.Distance( m_CAMPointList[ i + 1 ].CADPoint.Point );
				if( dTotalOverCutLength + dDistance < m_CraftData.OverCutLength ) {

					// still within overcut length → take next point directly
					m_OverCutPointList.Add( m_CAMPointList[ i + 1 ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = m_CraftData.OverCutLength - dTotalOverCutLength;
					if( dRemain <= MyApp.PRECISION_MIN_ERROR ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint( m_CAMPointList[ i ].CADPoint.Point, m_CAMPointList[ i + 1 ].CADPoint.Point, dRemain );

					// interpolate tool vector
					InterpolateToolAndTangentVecBetween2CAMPoint( m_CAMPointList[ i ], m_CAMPointList[ i + 1 ], overCutEndPoint, out gp_Dir endPointToolVec, out gp_Dir endPointTangentVec );

					// create new cam poiont
					CADPoint cadPoint = new CADPoint( overCutEndPoint, endPointToolVec, endPointToolVec, endPointTangentVec );
					CAMPoint camPoint = new CAMPoint( cadPoint, endPointToolVec );
					m_OverCutPointList.Add( camPoint );
					return;
				}
			}
		}

		gp_Pnt GetExactOverCutEndPoint( gp_Pnt currentPoint, gp_Pnt nextPoint, double dDistanceMoveFromOverPoint )
		{
			// from currentPoint → nextOverLengthPoint
			gp_Vec movingVec = new gp_Vec( currentPoint, nextPoint );

			// normalize to unit vector
			movingVec.Normalize();

			gp_Vec moveVec = movingVec.Multiplied( dDistanceMoveFromOverPoint );

			// shifted along the vector
			return new gp_Pnt( currentPoint.XYZ() + moveVec.XYZ() );
		}

		gp_Dir InterpolateVecBetween2Vec( gp_Vec currentVec, gp_Vec nextVec, double interpolatePercent )
		{
			// this case is unsolcvable, so just return current vec
			if( currentVec.IsOpposite( nextVec, MyApp.PRECISION_MIN_ERROR ) ) {
				return new gp_Dir( currentVec.XYZ() );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( currentVec, nextVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			// calculate new point attitude
			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( interpolatePercent, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir resultDir = new gp_Dir( currentVec.Transformed( trsf ) );
			return resultDir;
		}

		void InterpolateToolAndTangentVecBetween2CAMPoint( CAMPoint currentCAMPoint, CAMPoint nextCAMPoint, gp_Pnt point, out gp_Dir toolDir, out gp_Dir tangentDir )
		{
			toolDir = currentCAMPoint.ToolVec;
			tangentDir = currentCAMPoint.CADPoint.TangentVec;

			// get current and next tool vector
			gp_Vec currentVec = new gp_Vec( currentCAMPoint.ToolVec );
			gp_Vec nextVec = new gp_Vec( nextCAMPoint.ToolVec );

			// get current and next tangent vector
			gp_Vec currentTangentVec = new gp_Vec( currentCAMPoint.CADPoint.TangentVec );
			gp_Vec nextTangentVec = new gp_Vec( nextCAMPoint.CADPoint.TangentVec );

			// calculate new point percentage
			double dDistanceOfCAMPath2Point = currentCAMPoint.CADPoint.Point.Distance( nextCAMPoint.CADPoint.Point );
			double dDistanceBetweenCurrentPoint2NewPoint = currentCAMPoint.CADPoint.Point.Distance( point );

			// two point overlap
			if( dDistanceOfCAMPath2Point <= MyApp.PRECISION_MIN_ERROR ) {
				return;
			}
			double interpolatePercent = dDistanceBetweenCurrentPoint2NewPoint / dDistanceOfCAMPath2Point;

			// get new point dir
			toolDir = InterpolateVecBetween2Vec( currentVec, nextVec, interpolatePercent );
			tangentDir = InterpolateVecBetween2Vec( currentTangentVec, nextTangentVec, interpolatePercent );
		}

		#endregion

		#region Lead function

		void SetLeadIn()
		{
			m_LeadInCAMPointList.Clear();
			if( m_CAMPointList.Count == 0 ) {
				return;
			}
			switch( m_CraftData.LeadLineParam.LeadIn.Type ) {
				case LeadLineType.Line:
					m_LeadInCAMPointList = LeadHelper.BuildStraightLeadLine( m_CAMPointList.First(), true, m_CraftData.LeadLineParam.LeadIn.Length, m_CraftData.LeadLineParam.LeadIn.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse );
					break;
				case LeadLineType.Arc:
					m_LeadInCAMPointList = LeadHelper.BuildArcLeadLine( m_CAMPointList.First(), true, m_CraftData.LeadLineParam.LeadIn.Length, m_CraftData.LeadLineParam.LeadIn.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse, MyApp.DISCRETE_MAX_DEFLECTION, MyApp.DISCRETE_MAX_EDGE_LENGTH );
					break;
				default:
					break;
			}
		}

		void SetLeadout()
		{
			m_LeadOutCAMPointList.Clear();
			if( m_CAMPointList.Count == 0 ) {
				return;
			}

			// with over cut means lead out first point is over cut last point
			CAMPoint leadOutStartPoint;
			if( m_CraftData.OverCutLength > 0 && m_OverCutPointList.Count > 0 ) {
				leadOutStartPoint = m_OverCutPointList.Last();
			}
			else {
				leadOutStartPoint = m_CAMPointList.Last();
			}
			switch( m_CraftData.LeadLineParam.LeadOut.Type ) {
				case LeadLineType.Line:
					m_LeadOutCAMPointList = LeadHelper.BuildStraightLeadLine( leadOutStartPoint, false, m_CraftData.LeadLineParam.LeadOut.Length, m_CraftData.LeadLineParam.LeadOut.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse );
					break;
				case LeadLineType.Arc:
					m_LeadOutCAMPointList = LeadHelper.BuildArcLeadLine( leadOutStartPoint, false, m_CraftData.LeadLineParam.LeadOut.Length, m_CraftData.LeadLineParam.LeadOut.Angle, m_CraftData.LeadLineParam.IsChangeLeadDirection, m_CraftData.IsReverse, MyApp.DISCRETE_MAX_DEFLECTION, MyApp.DISCRETE_MAX_EDGE_LENGTH );
					break;
				default:
					break;
			}
		}

		#endregion

		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();

		// they are sibling pointer, and change the declare order
		CraftData m_CraftData;
		List<CADPoint> m_CADPointList = new List<CADPoint>();

		// flag to indicate craft data changed
		bool m_IsCraftDataDirty = false;
	}
}
