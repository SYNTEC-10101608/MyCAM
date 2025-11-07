using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Helper;
using OCC.BOPTools;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

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
			BuildCADSegment();
			BuildCAMPointList_New();
			BuildPathCAMSegment();
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
			BuildCAMPointList_New();
			BuildPathCAMSegment();
		}

		internal List<CADPoint> CADPointList
		{
			get; private set;
		}

		internal List<ICADSegmentElement> CADSegmentList
		{
			get
			{
				return m_CADSegmentList.Select( segment => segment.Clone() ).ToList();
			}
		}

		void AddSegment( ICADSegmentElement segment )
		{
			if( segment != null ) {
				m_CADSegmentList.Add( segment );
			}
		}

		void AddSegment( IEnumerable<ICADSegmentElement> segmentList )
		{
			if( segmentList != null ) {
				m_CADSegmentList.AddRange( segmentList );
			}
		}

		internal List<ICAMSegmentElement> BreakedCAMSegmentList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList_New();
					BuildPathCAMSegment();
					m_IsDirty = false;
				}
				return m_BrakedCAMSegment;
			}
			set
			{
				if( value != null ) {
					m_BrakedCAMSegment = value;
				}
			}
		}

		internal List<int> ControlBarIndexList
		{
			get
			{
				return m_ControlBarIndex;
			}
			set
			{
				if( value != null ) {
					m_ControlBarIndex = value;
				}
			}
		}

		// CAM property

		internal List<CAMPoint> CAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList_New();
					BuildPathCAMSegment();
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
					BuildCAMPointList_New();
					BuildPathCAMSegment();
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
					BuildCAMPointList_New();
					BuildPathCAMSegment();
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
					BuildCAMPointList_New();
					BuildPathCAMSegment();
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

		public (int, int) NewStartPoint
		{
			get
			{
				return m_NewStartPoint;
			}
			set
			{
				if( m_NewStartPoint != value ) {
					m_NewStartPoint = value;
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

		public Dictionary<(int, int), Tuple<double, double>> ToolVecModifyMap_New
		{
			get
			{
				return new Dictionary<(int, int), Tuple<double, double>>( m_ToolVecModifyMap_New );
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

		public void SetToolVecModify_New( (int, int) index, double dRA_deg, double dRB_deg )
		{
			if( m_ToolVecModifyMap_New.ContainsKey( index ) ) {
				m_ToolVecModifyMap_New[ index ] = new Tuple<double, double>( dRA_deg, dRB_deg );
			}
			else {
				m_ToolVecModifyMap_New.Add( index, new Tuple<double, double>( dRA_deg, dRB_deg ) );
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

		public bool GetToolVecModify_New( (int, int) index, out double dRA_deg, out double dRB_deg )
		{
			if( m_ToolVecModifyMap_New.ContainsKey( index ) ) {
				dRA_deg = m_ToolVecModifyMap_New[ index ].Item1;
				dRB_deg = m_ToolVecModifyMap_New[ index ].Item2;
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

		public void RemoveToolVecModify_New( (int, int) index )
		{
			if( m_ToolVecModifyMap_New.ContainsKey( index ) ) {
				m_ToolVecModifyMap_New.Remove( index );
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

			foreach( ICADSegmentElement cADSegmentElement in m_CADSegmentList ) {
				foreach( CADPoint cadPoint in cADSegmentElement.PointList ) {
					cadPoint.Transform( transform );
				}
			}
			m_IsDirty = true;
		}

		public CAMPoint GetProcessStartPoint()
		{
			if( m_IsDirty ) {
				BuildCAMPointList_New();
				BuildPathCAMSegment();
			}
			CAMPoint camPoint = null;

			if( m_LeadParam.LeadIn.Type != LeadLineType.None ) {
				List<ICAMSegmentElement> leadCAMSegment = BuildCAMSegmentHelper.BuildLeadCAMSegment( this, true );
				camPoint = leadCAMSegment.FirstOrDefault()?.StartPoint;
				return camPoint;
			}
			camPoint = BreakedCAMSegmentList[ 0 ].StartPoint;
			return camPoint;
		}

		public CAMPoint GetProcessEndPoint()
		{
			if( m_IsDirty ) {
				BuildCAMPointList_New();
				BuildPathCAMSegment();
				m_IsDirty = false;
			}

			CAMPoint camPoint = null;
			if( m_LeadParam.LeadOut.Type != LeadLineType.None ) {
				List<ICAMSegmentElement> leadCAMSegment = BuildCAMSegmentHelper.BuildLeadCAMSegment( this, false );
				camPoint = leadCAMSegment.LastOrDefault()?.EndPoint;
				return camPoint;
			}

			camPoint = BreakedCAMSegmentList[ 0 ].StartPoint;
			return camPoint;
		}

		// backing fields
		List<PathEdge5D> m_PathEdge5DList;
		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadInCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_LeadOutCAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OverCutPointList = new List<CAMPoint>();
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();

		// tunple <segment, index> , tuple<RA, RB>
		Dictionary<(int, int), Tuple<double, double>> m_ToolVecModifyMap_New = new Dictionary<(int, int), Tuple<double, double>>();
		List<ICAMSegmentElement> m_BrakedCAMSegment = new List<ICAMSegmentElement>();
		List<ICADSegmentElement> m_CADSegmentList = new List<ICADSegmentElement>();
		List<int> m_ControlBarIndex = new List<int>();
		bool m_IsReverse = false;
		bool m_IsToolVecReverse = false;
		int m_StartPoint = 0;
		int m_StartPointIndex = 0;
		(int, int) m_NewStartPoint = (0, 0);
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

		void BuildCADSegment()
		{
			CADPointList = new List<CADPoint>();
			List<CADPoint> tempCADPointList = new List<CADPoint>();
			if( m_PathEdge5DList == null ) {
				return;
			}

			// go through the contour edges
			for( int i = 0; i < m_PathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = m_PathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = m_PathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = m_PathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {
					tempCADPointList = Pretreatment.GetSegmentPointsByEqualLength( edge, shellFace, PRECISION_MAX_LENGTH, false, out double dEdgeLength, out double dPointSpace );
					LineCADSegment lineSegment = new LineCADSegment( tempCADPointList, dEdgeLength, dPointSpace );
					AddSegment( lineSegment );
				}

				// this curve is arc use equal length split
				else if( GeometryTool.IsCircularArc( edge, out gp_Pnt circleCenter, out _, out gp_Dir centerDir, out double arcAngle ) ) {

					// separate arc into angle - pi/2
					List<TopoDS_Edge> arcEdgeList = new List<TopoDS_Edge>();
					if( arcAngle > Math.PI / 2 ) {
						arcEdgeList = Pretreatment.SplitArcEdgeIfTooLarge( edge, shellFace );
					}
					else {
						arcEdgeList.Add( edge );
					}
					for( int j = 0; j < arcEdgeList.Count; j++ ) {
						tempCADPointList = Pretreatment.GetSegmentPointsByEqualLength( arcEdgeList[ j ], shellFace, PRECISION_MAX_LENGTH, false, out double dEdgeLength, out double dPointSpace );
						ArcCADSegment arcSegment = new ArcCADSegment( tempCADPointList, dEdgeLength, dPointSpace );
						AddSegment( arcSegment );
					}
				}

				// use chord error split
				else {
					// separate this bspline to several edge by chord error
					List<TopoDS_Edge> segmentEdgeList = Pretreatment.GetBsplineToEdgeList( edge, shellFace );

					// each edge use equal length split
					AddSegment( Pretreatment.GetCADSegmentLineFromShortEdge( segmentEdgeList, shellFace ) );
				}
				CADPointList.AddRange( GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace, i == 0, i == m_PathEdge5DList.Count - 1 ) );
			}
			m_NewStartPoint = (CADSegmentList.Count - 1, CADSegmentList[ CADSegmentList.Count - 1 ].PointList.Count - 1);
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

			//這條wire中的這段edge的分割點
			segmentParamList.Add( qUD.Parameter( qUD.NbPoints() ) );

			// reverse the segment parameters if the edge is reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}

			// 要在這條wire中的這段edge開始分割點位
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

				// 分割出一個點
				// build
				CADPoint cadPoint = new CADPoint( point, normalVec_1st, normalVec_2nd, new gp_Dir( tangentVec ) );

				// map the last point of the previous edge to the start point, and use current start point to present
				// 這不是這條wire中第一個edge,但是是這段edge的第一個點
				if( !bFirst && i == 0 && CADPointList.Count > 0 ) {

					// 找到上一段edge的最後一個點
					CADPoint lastPoint = CADPointList.Last();

					// 刪除上段EDGE的最後一個點,用這段EDGE的第一個點
					CADPointList.RemoveAt( CADPointList.Count - 1 );

					// 記住這個重複點的另一個分身(上一段的最後一個點)
					m_ConnectPointMap[ cadPoint ] = lastPoint;
					oneSegmentPointList.Add( cadPoint );
				}

				// 這是這條Wire的最後一個Edge,而且是這個Edge的最後一個點
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

		void BuildCAMPointList_New()
		{
			m_IsDirty = false;
			m_CAMPointList = new List<CAMPoint>();
			SetToolVec();
			SetStartPoint();

			// close the loop if is closed
			if( IsClosed && m_CAMPointList.Count > 0 ) {
				m_CAMPointList.Add( m_CAMPointList[ 0 ].Clone() );
			}

			// all CAM point are settled down, start set lead / overcut
			SetOverCut();
			SetLeadIn();
			SetLeadout();
		}

		#region Build CAM Segment

		void BuildPathCAMSegment()
		{
			List<ICADSegmentElement> reorderedSegment = BreakAndReorderByStartPoint( this );
			List<(int, int)> ModifyMapList = ModidyInexMap( this, reorderedSegment.Count, out Dictionary<(int, int), (int, int)> ControlBarMap );
			List<ICADSegmentElement> breakedCADSegment = BreakByToolVecBar( reorderedSegment, ModifyMapList, ControlBarMap, out Dictionary<int, (int, int)> ControlBarMapedAsIndex );
			List<ICAMSegmentElement> camSegmentList = BuildCAMSegment( this, breakedCADSegment, ControlBarMapedAsIndex );
			List<int> controlBarIndex = ControlBarMapedAsIndex.Keys.ToList();
			this.BreakedCAMSegmentList = camSegmentList;
			this.ControlBarIndexList = controlBarIndex;
		}

		List<ICAMSegmentElement> BuildCAMSegment( CAMData camData, List<ICADSegmentElement> breakedCADSegment, Dictionary<int, (int, int)> ControlBarMapedAsIndex )
		{
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();

			if( ControlBarMapedAsIndex.Count == 0 ) {
				for( int i = 0; i < breakedCADSegment.Count; i++ ) {
					if( breakedCADSegment[ i ].ContourType == EContourType.Line ) {
						LineCAMSegment lineCAMSegment = BuildCAMSegmentHelper.BuildCAMLineSegment( breakedCADSegment[ i ].PointList, camData.IsToolVecReverse );
						camSegmentList.Add( lineCAMSegment );
					}
					else {
						ArcCAMSegment arcCAMSegment = BuildCAMSegmentHelper.BuildCAMArcSegment( breakedCADSegment[ i ].PointList, camData.IsToolVecReverse );
						camSegmentList.Add( arcCAMSegment );
					}
				}
				return camSegmentList;
			}
			camSegmentList = BuildCAMSegmentWithSeveralToolBar( camData, breakedCADSegment, ControlBarMapedAsIndex );
			return camSegmentList;
		}

		List<ICAMSegmentElement> BuildCAMSegmentWithSeveralToolBar( CAMData camData, List<ICADSegmentElement> breakedCADSegment, Dictionary<int, (int, int)> ControlBarMapedAsIndex )
		{
			if( ControlBarMapedAsIndex.Count == 0 || breakedCADSegment.Count == 0 ) {
				return new List<ICAMSegmentElement>();
			}
			List<ICAMSegmentElement> camSegmentList = new List<ICAMSegmentElement>();

			// the segment with control bar
			List<int> barIndexList = ControlBarMapedAsIndex.Keys.ToList();
			barIndexList.Sort();

			for( int i = 0; i < breakedCADSegment.Count; i++ ) {
				List<int> barIndexRange = FindBarIndexRange( barIndexList, i );
				int startBarIndex = barIndexRange[ 0 ];
				int endBarIndex = barIndexRange[ 1 ];

				gp_Vec startToolVec = GetToolVecByBreakedSegmenIndex( camData, breakedCADSegment, startBarIndex, ControlBarMapedAsIndex );
				gp_Vec endToolVec = GetToolVecByBreakedSegmenIndex( camData, breakedCADSegment, endBarIndex, ControlBarMapedAsIndex );

				if( startToolVec == null || endToolVec == null ) {
					continue;
				}

				// calculate total length from start bar to end bar
				double dTotalLength = SumSegmentLength( breakedCADSegment, startBarIndex, endBarIndex );
				double dLengthFromStartBar = SumSegmentLength( breakedCADSegment, startBarIndex, i );
				gp_Dir camSegmentStartToolVec = GetInterpolateToolVecByLength( startToolVec, endToolVec, dLengthFromStartBar - breakedCADSegment[ i ].TotalLength, dTotalLength );
				gp_Dir camSegmentEndToolVec = GetInterpolateToolVecByLength( startToolVec, endToolVec, dLengthFromStartBar, dTotalLength );
				CAMPoint startCAMPoint = new CAMPoint( breakedCADSegment[ i ].StartPoint, camSegmentStartToolVec );
				CAMPoint endCAMPoint = new CAMPoint( breakedCADSegment[ i ].EndPoint, camSegmentEndToolVec );

				if( breakedCADSegment[ i ].ContourType == EContourType.Line ) {
					LineCAMSegment lineCAMSegment = new LineCAMSegment( startCAMPoint, endCAMPoint, false );
					camSegmentList.Add( lineCAMSegment );
				}
				else {
					gp_Dir midToolVec = GeometryTool.GetDirAverage( camSegmentStartToolVec, camSegmentEndToolVec );
					CAMPoint midCAMPoint;
					if( breakedCADSegment[ i ].PointList.Count < 3 ) {
						CADPoint midCADPoint = GetMidCAMPointForShortSegment( breakedCADSegment[ i ].StartPoint, breakedCADSegment[ i ].EndPoint );
						midCAMPoint = new CAMPoint( midCADPoint, midToolVec );
					}
					else {
						midCAMPoint = new CAMPoint( breakedCADSegment[ i ].PointList[ breakedCADSegment[ i ].PointList.Count / 2 ], midToolVec );
					}
					ArcCAMSegment arcCAMSegment = new ArcCAMSegment( startCAMPoint, endCAMPoint, midCAMPoint, false );
					camSegmentList.Add( arcCAMSegment );
				}
			}
			return camSegmentList;
		}

		CADPoint GetMidCAMPointForShortSegment( CADPoint startCADPoint, CADPoint endCAMPoint )
		{
			gp_Pnt midPoint = GeometryTool.FindMidPoint( startCADPoint.Point, endCAMPoint.Point );
			gp_Dir normalVec = GeometryTool.GetDirAverage( startCADPoint.NormalVec_1st, endCAMPoint.NormalVec_1st );
			gp_Dir tanVec = GeometryTool.GetDirAverage( startCADPoint.TangentVec, endCAMPoint.TangentVec );
			CADPoint midCADPoint = new CADPoint( midPoint, normalVec, normalVec, tanVec );
			return midCADPoint;
		}

		double SumSegmentLength( List<ICADSegmentElement> segmentList, int startIndex, int endIndex )
		{
			int nSegmentCount = segmentList.Count;
			if( segmentList == null || segmentList.Count == 0 ) {
				return 0.0;
			}
			if( startIndex < 0 || startIndex >= nSegmentCount || endIndex < 0 || endIndex >= nSegmentCount ) {
				return 0.0;
			}

			double dLength = 0.0;
			int nCurrent = ( startIndex + 1 ) % nSegmentCount;
			while( true ) {
				dLength += segmentList[ nCurrent ].TotalLength;
				if( nCurrent == endIndex ) {
					break;
				}
				nCurrent = ( nCurrent + 1 ) % nSegmentCount;
			}
			return dLength;
		}

		gp_Dir GetInterpolateToolVecByLength( gp_Vec startToolVec, gp_Vec endToolVec, double dDeltaLength, double dTotalLength )
		{
			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startToolVec, endToolVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( dDeltaLength / dTotalLength, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir toolVecDir = new gp_Dir( startToolVec.Transformed( trsf ) );
			return toolVecDir;
		}

		gp_Vec GetToolVecByBreakedSegmenIndex( CAMData camData, List<ICADSegmentElement> breakedCADSegment, int targetIndex, Dictionary<int, (int, int)> ControlBarMapedAsIndex )
		{
			// real bar index in original CAD segment
			if( ControlBarMapedAsIndex.TryGetValue( targetIndex, out (int, int) oriSegmentIndex ) ) {

				// get AB value
				if( camData.ToolVecModifyMap_New.TryGetValue( oriSegmentIndex, out Tuple<double, double> AB_Value ) ) {
					CADPoint cadPoint = breakedCADSegment[ targetIndex ].PointList.Last();
					CAMPoint camPoint = BuildCAMSegmentHelper.GetCAMPoint( cadPoint, camData.IsToolVecReverse );
					gp_Vec ToolVec = GetVecFromAB( camPoint, AB_Value.Item1 * Math.PI / 180, AB_Value.Item2 * Math.PI / 180 );
					return ToolVec;
				}
			}
			return null;
		}

		List<int> FindBarIndexRange( List<int> barIndex, int targetIndex )
		{
			if( barIndex == null || barIndex.Count == 0 ) {
				return new List<int>();
			}
			barIndex.Sort();

			// find first index which >= targetIndex
			int nextBarPos = barIndex.FindIndex( x => x >= targetIndex );

			// no found means all bar is smaller than target index
			if( nextBarPos == -1 ) {
				return new List<int> { barIndex.Last(), barIndex.First() };
			}

			// find the largest value less than taget index
			int prevBarPos = ( nextBarPos - 1 + barIndex.Count ) % barIndex.Count;
			return new List<int> { barIndex[ prevBarPos ], barIndex[ nextBarPos ] };
		}

		List<(int, int)> ModidyInexMap( CAMData camData, int SegmentCount, out Dictionary<(int, int), (int, int)> ControlBarMap )
		{
			ControlBarMap = new Dictionary<(int, int), (int, int)>();
			List<(int, int)> modifyMap = camData.ToolVecModifyMap_New.Keys.ToList();
			if( modifyMap.Count == 0 ) {
				return modifyMap;
			}

			(int segment, int pointIndex) startPoint = camData.NewStartPoint;
			for( int i = 0; i < modifyMap.Count; i++ ) {

				// start point is at the end of segment, that segment no change
				if( camData.CADSegmentList.Count == SegmentCount ) {
					(int, int) backup = modifyMap[ i ];
					int newSegmentIndex = ( modifyMap[ i ].Item1 - startPoint.segment - 1 + camData.CADSegmentList.Count ) % camData.CADSegmentList.Count;
					modifyMap[ i ] = (newSegmentIndex, modifyMap[ i ].Item2);
					ControlBarMap.Add( modifyMap[ i ], backup );
				}

				// start point is at the middle of segment, that segment be breaked into two segments
				else {

					// not at start segment, need to modify segment index
					if( modifyMap[ i ].Item1 != startPoint.segment ) {
						(int, int) backup = modifyMap[ i ];
						int newSegmentIndex = ( ( modifyMap[ i ].Item1 - startPoint.segment + camData.CADSegmentList.Count ) % camData.CADSegmentList.Count );
						modifyMap[ i ] = (newSegmentIndex, modifyMap[ i ].Item2);
						ControlBarMap.Add( modifyMap[ i ], backup );
					}

					// is at start segment
					else {
						if( modifyMap[ i ].Item2 > startPoint.pointIndex ) {
							(int, int) backup = modifyMap[ i ];

							// in the first part
							modifyMap[ i ] = (0, modifyMap[ i ].Item2 - startPoint.pointIndex);
							ControlBarMap.Add( modifyMap[ i ], backup );
						}
						else {

							// in the last 
							(int, int) backup = modifyMap[ i ];
							int newSegmentIndex = SegmentCount - 1;
							modifyMap[ i ] = (newSegmentIndex, modifyMap[ i ].Item2);
							ControlBarMap.Add( modifyMap[ i ], backup );
						}
					}
				}
			}
			return modifyMap;
		}

		List<ICADSegmentElement> BreakAndReorderByStartPoint( CAMData camData )
		{
			(int segment, int pointIndex) startPoint = camData.NewStartPoint;
			List<ICADSegmentElement> reorderedCADSegmentList = new List<ICADSegmentElement>();

			// reorder segment list
			List<ICADSegmentElement> cadSegmentList = camData.CADSegmentList;
			for( int i = 0; i < cadSegmentList.Count; i++ ) {
				int index = ( startPoint.segment + i ) % cadSegmentList.Count;
				reorderedCADSegmentList.Add( cadSegmentList[ index ] );
			}

			// no need to break segment
			if( startPoint.pointIndex == camData.CADSegmentList[ startPoint.segment ].PointList.Count - 1 ) {
				ICADSegmentElement realLastSegment = reorderedCADSegmentList.First();
				reorderedCADSegmentList.RemoveAt( 0 );
				reorderedCADSegmentList.Add( realLastSegment );
				return reorderedCADSegmentList;
			}

			bool isSuccess = SeparateCADSegmentAtTargetIndex( camData.CADSegmentList[ startPoint.segment ], startPoint.pointIndex, out List<ICADSegmentElement> breakedCADSegmentList );

			if( isSuccess ) {

				// this segment need to break
				reorderedCADSegmentList.RemoveAt( 0 );

				// insert breaked segment ( [0] is segment after start point, [1] is segment before start point)
				reorderedCADSegmentList.Insert( 0, breakedCADSegmentList.First() );
				reorderedCADSegmentList.Add( breakedCADSegmentList.Last() );
				return reorderedCADSegmentList;
			}
			return reorderedCADSegmentList;
		}

		List<ICADSegmentElement> BreakByToolVecBar( List<ICADSegmentElement> orderedCADSegmentList, List<(int, int)> modifyBar, Dictionary<(int, int), (int, int)> ControlBarMap, out Dictionary<int, (int, int)> ControlBarMapedAsIndex )
		{
			ControlBarMapedAsIndex = new Dictionary<int, (int, int)>();
			List<ICADSegmentElement> breakedCADSegmentList = new List<ICADSegmentElement>();
			if( modifyBar.Count == 0 ) {
				return orderedCADSegmentList;
			}

			modifyBar.Sort();
			for( int segmentIndex = 0; segmentIndex < orderedCADSegmentList.Count; segmentIndex++ ) {
				List<int> breakPointIndex = new List<int>();
				for( int j = 0; j < modifyBar.Count; j++ ) {

					// last point no need to break
					if( modifyBar[ j ].Item1 == segmentIndex ) {
						breakPointIndex.Add( modifyBar[ j ].Item2 );
					}
				}
				// no need to break
				if( breakPointIndex.Count == 0 ) {
					breakedCADSegmentList.Add( orderedCADSegmentList[ segmentIndex ] );
					continue;
				}

				List<List<CADPoint>> splitedCADPointList = SplitCADPointList( orderedCADSegmentList[ segmentIndex ].PointList, breakPointIndex, out bool isLastSegmentModify );
				for( int k = 0; k < splitedCADPointList.Count; k++ ) {
					ICADSegmentElement newCADSegment = CreatCADSegmentByCADPoint(
						splitedCADPointList[ k ],
						orderedCADSegmentList[ segmentIndex ].ContourType,
						orderedCADSegmentList[ segmentIndex ].PointSpace );
					if( newCADSegment != null ) {
						breakedCADSegmentList.Add( newCADSegment );

						// 紀錄controlbart Index
						if( k != splitedCADPointList.Count - 1 ) {
							(int, int) oriSegmentIndex = ControlBarMap[ (segmentIndex, breakPointIndex[ k ]) ];
							ControlBarMapedAsIndex[ breakedCADSegmentList.Count - 1 ] = oriSegmentIndex;
						}
						else {
							if( isLastSegmentModify ) {
								(int, int) oriSegmentIndex = ControlBarMap[ (segmentIndex, breakPointIndex[ k ]) ];
								ControlBarMapedAsIndex[ breakedCADSegmentList.Count - 1 ] = oriSegmentIndex;
							}
						}
					}
				}
			}
			return breakedCADSegmentList;
		}

		ICADSegmentElement CreatCADSegmentByCADPoint( List<CADPoint> cadPointList, EContourType contourType, double pointSpace )
		{
			if( contourType == EContourType.Line ) {
				return new LineCADSegment( cadPointList, pointSpace * ( cadPointList.Count - 1 ), pointSpace );
			}
			if( contourType == EContourType.Arc ) {
				return new ArcCADSegment( cadPointList, pointSpace * ( cadPointList.Count - 1 ), pointSpace );
			}
			return null;
		}

		public List<List<CADPoint>> SplitCADPointList( List<CADPoint> segmentCADPointList, List<int> separateLocation, out bool isLastSegmentModify )
		{
			List<List<CADPoint>> resultCADPointList = new List<List<CADPoint>>();
			isLastSegmentModify = true;
			if( segmentCADPointList == null || segmentCADPointList.Count == 0 ) {
				return resultCADPointList;
			}
			separateLocation = separateLocation.OrderBy( index => index ).ToList();
			int nStartIndex = 0;
			foreach( int nIndex in separateLocation ) {

				// avoid out of range
				if( nIndex > segmentCADPointList.Count - 1 ) {
					break;
				}
				resultCADPointList.Add( segmentCADPointList.GetRange( nStartIndex, nIndex - nStartIndex + 1 ) );
				nStartIndex = nIndex;
			}

			// last part
			if( nStartIndex < segmentCADPointList.Count - 1 ) {
				resultCADPointList.Add( segmentCADPointList.GetRange( nStartIndex, segmentCADPointList.Count - nStartIndex ) );
				isLastSegmentModify = false;
			}
			return resultCADPointList;
		}

		bool SeparateCADSegmentAtTargetIndex( ICADSegmentElement segmentElement, int targetIndex, out List<ICADSegmentElement> breakedCADSegmentList )
		{
			breakedCADSegmentList = new List<ICADSegmentElement>();
			if( segmentElement == null || targetIndex == 0 || targetIndex == segmentElement.PointList.Count - 1 ) {
				return false;
			}
			List<int> separateLocation = new List<int> { targetIndex };
			List<List<CADPoint>> splitedCADPointList = SplitCADPointList( segmentElement.PointList, separateLocation, out _ );

			List<CADPoint> pointListAfterTargetIndex = splitedCADPointList.Last();
			List<CADPoint> pointListBeforeTargetIndex = splitedCADPointList.First();
			if( segmentElement is LineCADSegment ) {
				LineCADSegment lineSegmentAfterTargetIndex = new LineCADSegment( pointListAfterTargetIndex, segmentElement.PointSpace * ( pointListAfterTargetIndex.Count - 1 ), segmentElement.PointSpace );
				LineCADSegment lineSegmentBeforeTargetIndex = new LineCADSegment( pointListBeforeTargetIndex, segmentElement.PointSpace * ( pointListBeforeTargetIndex.Count - 1 ), segmentElement.PointSpace );
				breakedCADSegmentList.Add( lineSegmentAfterTargetIndex );
				breakedCADSegmentList.Add( lineSegmentBeforeTargetIndex );
				return true;
			}
			if( segmentElement is ArcCADSegment ) {
				ArcCADSegment arcSegmentAfterTargetIndex = new ArcCADSegment( pointListAfterTargetIndex, segmentElement.PointSpace * ( pointListAfterTargetIndex.Count - 1 ), segmentElement.PointSpace );
				ArcCADSegment arcSegmentBeforTargetIndex = new ArcCADSegment( pointListBeforeTargetIndex, segmentElement.PointSpace * ( pointListBeforeTargetIndex.Count - 1 ), segmentElement.PointSpace );
				breakedCADSegmentList.Add( arcSegmentAfterTargetIndex );
				breakedCADSegmentList.Add( arcSegmentBeforTargetIndex );
				return true;
			}
			return false;
		}

		#endregion

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
				m_CAMPointList[ i % m_CAMPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
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