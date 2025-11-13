using MyCAM.Helper;
using OCC.BOPTools;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	public class CADPoint
	{
		public CADPoint( gp_Pnt point, gp_Dir normalVec_1st, gp_Dir normalVec_2nd, gp_Dir tangentVec )
		{
			m_Point = new gp_Pnt( point.X(), point.Y(), point.Z() );
			m_NormalVec_1st = new gp_Dir( normalVec_1st.XYZ() );
			m_NormalVec_2nd = new gp_Dir( normalVec_2nd.XYZ() );
			m_TangentVec = new gp_Dir( tangentVec.XYZ() );
		}

		public gp_Pnt Point
		{
			get
			{
				return new gp_Pnt( m_Point.X(), m_Point.Y(), m_Point.Z() );
			}
		}

		public gp_Dir NormalVec_1st
		{
			get
			{
				return new gp_Dir( m_NormalVec_1st.XYZ() );
			}
		}

		// normal vector on co-face
		public gp_Dir NormalVec_2nd
		{
			get
			{
				return new gp_Dir( m_NormalVec_2nd.XYZ() );
			}
		}

		// tangent vector on path
		public gp_Dir TangentVec
		{
			get
			{
				return new gp_Dir( m_TangentVec.XYZ() );
			}
		}

		public void Transform( gp_Trsf transform )
		{
			m_Point.Transform( transform );
			m_NormalVec_1st.Transform( transform );
			m_NormalVec_2nd.Transform( transform );
			m_TangentVec.Transform( transform );
		}

		public CADPoint Clone()
		{
			return new CADPoint( Point, NormalVec_1st, NormalVec_2nd, TangentVec );
		}

		// using backing fields to prevent modified outside
		gp_Pnt m_Point;
		gp_Dir m_NormalVec_1st;
		gp_Dir m_NormalVec_2nd;
		gp_Dir m_TangentVec;
	}

	// currently assuming CAM = CAD + ToolVec
	public class CAMPoint
	{
		public CAMPoint( CADPoint cadPoint, gp_Dir toolVec )
		{
			CADPoint = cadPoint;
			m_ToolVec = new gp_Dir( toolVec.XYZ() );
		}

		public CADPoint CADPoint
		{
			get; private set;
		}

		public gp_Dir ToolVec
		{
			get
			{
				return new gp_Dir( m_ToolVec.XYZ() );
			}
			set
			{
				m_ToolVec = new gp_Dir( value.XYZ() );
			}
		}

		public CAMPoint Clone()
		{
			return new CAMPoint( CADPoint.Clone(), ToolVec );
		}

		// using backing field to prevent modified outside
		gp_Dir m_ToolVec;
	}

	internal class CAMData
	{
		//CAD property
		public CAMData( List<PathEdge5D> pathDataList, bool isClosed )
		{
			m_PathEdge5DList = pathDataList;
			IsClosed = isClosed;

			// build raw data
			BuildCADPointList();
			BuildCAMPointList();
		}

		// for construct path by reading file
		public CAMData( List<CADPoint> cadPointList, bool isClosed, int nStartPoint, LeadData leadData, bool isReverse, bool isToolVecReverse, double dOverCutLength, Dictionary<int, Tuple<double, double>> ToolVecModifyMap, TraverseData traverseData )
		{
			CADPointList = cadPointList;
			IsClosed = isClosed;
			m_StartPoint = nStartPoint;
			m_LeadParam = leadData.Clone();
			m_TraverseData = traverseData.Clone();
			m_IsReverse = isReverse;
			m_IsToolVecReverse = isToolVecReverse;
			m_OverCutLength = dOverCutLength;
			m_ToolVecModifyMap = ToolVecModifyMap;
			BuildCAMPointList();
		}

		internal List<CADPoint> CADPointList
		{
			get; private set;
		}

		// CAM property

		internal List<CAMPoint> CAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList();
					m_IsDirty = false;
				}
				return m_CAMPointList;
			}
		}

		internal List<CAMPoint> LeadInCAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList();
					m_IsDirty = false;
				}
				return m_LeadInCAMPointList;
			}
		}

		internal List<CAMPoint> LeadOutCAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList();
					m_IsDirty = false;
				}
				return m_LeadOutCAMPointList;
			}
		}

		internal List<CAMPoint> OverCutCAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList();
					m_IsDirty = false;
				}
				return m_OverCutPointList;
			}
		}

		public bool IsReverse
		{
			get
			{
				return m_IsReverse;
			}
			set
			{
				if( m_IsReverse != value ) {
					m_IsReverse = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsToolVecReverse
		{
			get
			{
				return m_IsToolVecReverse;
			}
			set
			{
				if( m_IsToolVecReverse != value ) {
					m_IsToolVecReverse = value;
					m_IsDirty = true;
				}
			}
		}

		public int StartPoint
		{
			get
			{
				return m_StartPoint;
			}
			set
			{
				if( m_StartPoint != value ) {
					m_StartPoint = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsClosed
		{
			get; private set;
		}

		public LeadData LeadLineParam
		{
			get
			{
				// to prevent null value
				if( m_LeadParam == null ) {
					m_LeadParam = new LeadData();
				}
				return m_LeadParam;
			}
			set
			{
				// to prevent null value
				if( value == null ) {
					value = new LeadData();
				}
				if( m_LeadParam != value ) {
					m_LeadParam = value;
					m_IsDirty = true;
				}
			}
		}

		public TraverseData TraverseData
		{
			get
			{
				// to prevent null value
				if( m_TraverseData == null ) {
					m_TraverseData = new TraverseData();
				}
				return m_TraverseData;
			}
			set
			{
				// to prevent null value
				if( value == null ) {
					value = new TraverseData();
				}
				if( m_TraverseData != value ) {
					m_TraverseData = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsHasLead
		{
			get
			{
				if( m_LeadParam == null ) {
					return false;
				}
				return ( m_LeadParam.LeadIn.Type != LeadLineType.None ) || ( m_LeadParam.LeadOut.Type != LeadLineType.None );
			}
		}

		public double OverCutLength
		{
			get
			{
				return m_OverCutLength;
			}
			set
			{
				if( m_OverCutLength != value ) {
					m_OverCutLength = value;
					m_IsDirty = true;
				}
			}
		}

		public Dictionary<int, Tuple<double, double>> ToolVecModifyMap
		{
			get
			{
				return new Dictionary<int, Tuple<double, double>>( m_ToolVecModifyMap );
			}
		}

		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap[ index ] = new Tuple<double, double>( dRA_deg, dRB_deg );
			}
			else {
				m_ToolVecModifyMap.Add( index, new Tuple<double, double>( dRA_deg, dRB_deg ) );
			}
			m_IsDirty = true;
		}

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_ToolVecModifyMap[ index ].Item2;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
				return false;
			}
		}

		public void RemoveToolVecModify( int index )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap.Remove( index );
				m_IsDirty = true;
			}
		}

		public HashSet<int> GetToolVecModifyIndex()
		{
			HashSet<int> result = new HashSet<int>();
			foreach( int nIndex in m_ToolVecModifyMap.Keys ) {
				result.Add( nIndex );
			}
			return result;
		}

		public void Transform( gp_Trsf transform )
		{
			// transform CAD points
			foreach( CADPoint cadPoint in CADPointList ) {
				cadPoint.Transform( transform );
			}
			m_IsDirty = true;
		}

		public CAMPoint GetProcessStartPoint()
		{
			if( m_IsDirty ) {
				BuildCAMPointList();
				m_IsDirty = false;
			}
			CAMPoint camPoint = null;
			if( m_LeadInCAMPointList.Count > 0 && m_LeadParam.LeadIn.Length > 0 ) {
				camPoint = m_LeadInCAMPointList.First().Clone();
			}
			else if( m_CAMPointList.Count > 0 ) {
				camPoint = m_CAMPointList.First().Clone();
			}
			return camPoint;
		}

		public CAMPoint GetProcessEndPoint()
		{
			if( m_IsDirty ) {
				BuildCAMPointList();
				m_IsDirty = false;
			}
			CAMPoint camPoint = null;
			if( m_LeadOutCAMPointList.Count > 0 && m_LeadParam.LeadOut.Length > 0 ) {
				camPoint = m_LeadOutCAMPointList.Last().Clone();
			}
			else if( m_OverCutPointList.Count > 0 && m_OverCutLength > 0 ) {
				camPoint = m_OverCutPointList.Last().Clone();
			}
			else if( m_CAMPointList.Count > 0 ) {
				camPoint = m_CAMPointList.Last().Clone();
			}
			return camPoint;
		}

		// backing fields
		List<PathEdge5D> m_PathEdge5DList;
		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();
		bool m_IsReverse = false;
		bool m_IsToolVecReverse = false;
		int m_StartPoint = 0;
		double m_OverCutLength = 0;

		// lead param
		LeadData m_LeadParam = new LeadData();
		TraverseData m_TraverseData = new TraverseData();

		// dirty flag
		bool m_IsDirty = false;

		// Discretize parameters
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap = new Dictionary<CADPoint, CADPoint>();
		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
		const double PRECISION_MIN_ERROR = 0.001;

		void BuildCADPointList()
		{
			CADPointList = new List<CADPoint>();
			if( m_PathEdge5DList == null ) {
				return;
			}

			// go through the contour edges
			for( int i = 0; i < m_PathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = m_PathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = m_PathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = m_PathEdge5DList[ i ].ComponentFace; // TODO: set solid face
				CADPointList.AddRange( GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace,
					i == 0, i == m_PathEdge5DList.Count - 1 ) );
			}
		}

		/// <summary>
		/// each edge start point is map to the last point of the previous edge
		/// for a open-loop, the last point of the last edge (end point) is added to the list
		/// for a closed-loop, the last point of the last edge (end point) is mapped to the first point of the first edge (start point)
		/// </summary>
		List<CADPoint> GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace,
					bool bFirst, bool bLast )
		{
			List<CADPoint> oneSegmentPointList = new List<CADPoint>();

			// get curve parameters
			BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge, shellFace );
			double dStartU = adC.FirstParameter();
			double dEndU = adC.LastParameter();

			// break the curve into segments with given deflection precision
			GCPnts_QuasiUniformDeflection qUD = new GCPnts_QuasiUniformDeflection( adC, PRECISION_DEFLECTION, dStartU, dEndU );

			// break the curve into segments with given max length
			List<double> segmentParamList = new List<double>();
			for( int i = 1; i < qUD.NbPoints(); i++ ) {
				segmentParamList.Add( qUD.Parameter( i ) );
				double dLength = GCPnts_AbscissaPoint.Length( adC, qUD.Parameter( i ), qUD.Parameter( i + 1 ) );

				// add sub-segments if the length is greater than max length
				if( dLength > PRECISION_MAX_LENGTH ) {
					int nSubSegmentCount = (int)Math.Ceiling( dLength / PRECISION_MAX_LENGTH );
					double dSubSegmentLength = ( qUD.Parameter( i + 1 ) - qUD.Parameter( i ) ) / nSubSegmentCount;

					// using equal parameter to break the segment
					for( int j = 1; j < nSubSegmentCount; j++ ) {
						segmentParamList.Add( qUD.Parameter( i ) + j * dSubSegmentLength );
					}
				}
			}
			segmentParamList.Add( qUD.Parameter( qUD.NbPoints() ) );

			// reverse the segment parameters if the edge is reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}

			for( int i = 0; i < segmentParamList.Count; i++ ) {
				double U = segmentParamList[ i ];

				// get point
				gp_Pnt point = adC.Value( U );

				// get shell normal (1st)
				gp_Dir normalVec_1st = new gp_Dir();
				BOPTools_AlgoTools3D.GetNormalToFaceOnEdge( edge, shellFace, U, ref normalVec_1st );
				if( shellFace.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					normalVec_1st.Reverse();
				}

				// TODO: get solid normal (2nd)
				gp_Dir normalVec_2nd = new gp_Dir( normalVec_1st.XYZ() );

				// get tangent
				gp_Vec tangentVec = new gp_Vec();
				gp_Pnt _p = new gp_Pnt();
				adC.D1( U, ref _p, ref tangentVec );
				if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					tangentVec.Reverse();
				}

				// build
				CADPoint cadPoint = new CADPoint( point, normalVec_1st, normalVec_2nd, new gp_Dir( tangentVec ) );

				// map the last point of the previous edge to the start point, and use current start point to present
				if( !bFirst && i == 0 && CADPointList.Count > 0 ) {
					CADPoint lastPoint = CADPointList.Last();
					CADPointList.RemoveAt( CADPointList.Count - 1 );
					m_ConnectPointMap[ cadPoint ] = lastPoint;
					oneSegmentPointList.Add( cadPoint );
				}
				else if( bLast && i == segmentParamList.Count - 1 && CADPointList.Count > 0 ) {

					// map the last point to the start point
					if( IsClosed ) {
						CADPoint firstPoint = CADPointList[ 0 ];
						m_ConnectPointMap[ firstPoint ] = cadPoint;
					}

					// add the last point
					else {
						oneSegmentPointList.Add( cadPoint );
					}
				}
				else {
					oneSegmentPointList.Add( cadPoint );
				}
			}
			return oneSegmentPointList;
		}

		void BuildCAMPointList()
		{
			m_IsDirty = false;
			m_CAMPointList = new List<CAMPoint>();
			SetToolVec();
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

		void SetToolVec()
		{
			for( int i = 0; i < CADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = CADPointList[ i ];
				CAMPoint camPoint;
				if( m_IsToolVecReverse ) {
					camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st.Reversed() );
				}
				else {
					camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				}
				m_CAMPointList.Add( camPoint );
			}
			ModifyToolVec();
		}

		void ModifyToolVec()
		{
			if( m_ToolVecModifyMap.Count == 0 ) {
				return;
			}

			// all tool vector are modified to the same value, no need to do interpolation
			if( m_ToolVecModifyMap.Count == 1 ) {
				gp_Vec newVec = GetVecFromAB( m_CAMPointList[ m_ToolVecModifyMap.Keys.First() ],
					m_ToolVecModifyMap.Values.First().Item1 * Math.PI / 180,
					m_ToolVecModifyMap.Values.First().Item2 * Math.PI / 180 );
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

		gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			if( Math.Abs( dRA_rad * 180 / Math.PI - 666 ) < 1e-6 ) {
				return new gp_Vec( 0, 0, 1 );
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

		List<Tuple<int, int>> GetInterpolateIntervalList()
		{
			// sort the modify data by index
			List<int> indexInOrder = m_ToolVecModifyMap.Keys.ToList();
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

		void InterpolateToolVec( int nStartIndex, int nEndIndex )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + m_CAMPointList.Count : nEndIndex;

			// get the start and end tool vector
			gp_Vec startVec = GetVecFromAB( m_CAMPointList[ nStartIndex ],
				m_ToolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
				m_ToolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
			gp_Vec endVec = GetVecFromAB( m_CAMPointList[ nEndIndex ],
				m_ToolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
				m_ToolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );

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
				//m_CAMPointList[ i % m_CAMPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
				if( i == nStartIndex ) {
					m_CAMPointList[ i % m_CAMPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
				}
				else {
					m_CAMPointList[ i % m_CAMPointList.Count ].ToolVec = new gp_Dir( 0, 0, 1 );
				}
			}
		}

		void SetStartPoint()
		{
			// rearrange cam points to start from the strt index
			if( m_StartPoint != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + m_StartPoint ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}
		}

		void SetOrientation()
		{
			// reverse the cad points if is reverse
			if( m_IsReverse ) {
				m_CAMPointList.Reverse();

				// modify start point index for closed path
				if( IsClosed ) {
					CAMPoint lastPoint = m_CAMPointList.Last();
					m_CAMPointList.Remove( lastPoint );
					m_CAMPointList.Insert( 0, lastPoint );
				}
			}
		}

		#region Over cut

		void SetOverCut()
		{
			m_OverCutPointList.Clear();
			if( m_CAMPointList.Count == 0 || m_OverCutLength == 0 || !IsClosed ) {
				return;
			}
			double dTotalOverCutLength = 0;

			// end point is the start of over cut
			m_OverCutPointList.Add( m_CAMPointList.Last().Clone() );
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {

				// get this edge distance
				double dDistance = m_CAMPointList[ i ].CADPoint.Point.Distance( m_CAMPointList[ i + 1 ].CADPoint.Point );
				if( dTotalOverCutLength + dDistance < m_OverCutLength ) {

					// still within overcut length → take next point directly
					m_OverCutPointList.Add( m_CAMPointList[ i + 1 ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = m_OverCutLength - dTotalOverCutLength;
					if( dRemain <= PRECISION_MIN_ERROR ) {
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
			if( currentVec.IsOpposite( nextVec, PRECISION_MIN_ERROR ) ) {
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
			if( dDistanceOfCAMPath2Point <= PRECISION_MIN_ERROR ) {
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
			switch( m_LeadParam.LeadIn.Type ) {
				case LeadLineType.Line:
					m_LeadInCAMPointList = LeadHelper.BuildStraightLeadLine( m_CAMPointList.First(), true, m_LeadParam.LeadIn.Length, m_LeadParam.LeadIn.Angle, m_LeadParam.IsChangeLeadDirection, m_IsReverse );
					break;
				case LeadLineType.Arc:
					m_LeadInCAMPointList = LeadHelper.BuildArcLeadLine( m_CAMPointList.First(), true, m_LeadParam.LeadIn.Length, m_LeadParam.LeadIn.Angle, m_LeadParam.IsChangeLeadDirection, m_IsReverse, PRECISION_DEFLECTION, PRECISION_MAX_LENGTH );
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
			if( m_OverCutLength > 0 && m_OverCutPointList.Count > 0 ) {
				leadOutStartPoint = m_OverCutPointList.Last();
			}
			else {
				leadOutStartPoint = m_CAMPointList.Last();
			}
			switch( m_LeadParam.LeadOut.Type ) {
				case LeadLineType.Line:
					m_LeadOutCAMPointList = LeadHelper.BuildStraightLeadLine( leadOutStartPoint, false, m_LeadParam.LeadOut.Length, m_LeadParam.LeadOut.Angle, m_LeadParam.IsChangeLeadDirection, m_IsReverse );
					break;
				case LeadLineType.Arc:
					m_LeadOutCAMPointList = LeadHelper.BuildArcLeadLine( leadOutStartPoint, false, m_LeadParam.LeadOut.Length, m_LeadParam.LeadOut.Angle, m_LeadParam.IsChangeLeadDirection, m_IsReverse, PRECISION_DEFLECTION, PRECISION_MAX_LENGTH );
					break;
				default:
					break;
			}
		}

		#endregion
	}
}