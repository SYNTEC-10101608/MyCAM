using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepBuilderAPI;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Data
{
	internal class ShapeData
	{
		public ShapeData( string szUID, TopoDS_Shape shapeData )
		{
			UID = szUID;
			Shape = shapeData;
		}

		public string UID
		{
			get; private set;
		}

		public TopoDS_Shape Shape
		{
			get; private set;
		}

		public virtual void DoTransform( gp_Trsf transform )
		{
			BRepBuilderAPI_Transform shapeTransform = new BRepBuilderAPI_Transform( Shape, transform );
			Shape = shapeTransform.Shape();
		}
	}

	// path data
	internal class PathData : ShapeData
	{
		public PathData( string szUID, TopoDS_Shape shapeData, List<PathEdge5D> pathElementList )
			: base( szUID, shapeData )
		{
			TopoDS_Vertex startVertex = new TopoDS_Vertex();
			TopoDS_Vertex endVertex = new TopoDS_Vertex();
			TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );
			gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
			gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );
			bool isClosed = startPoint.IsEqual( endPoint, 1e-3 );

			m_CAMData = new CAMData( pathElementList, isClosed );
		}

		public CAMData CAMData
		{
			get
			{
				return m_CAMData;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			base.DoTransform( transform );
		}

		CAMData m_CAMData;
	}

	internal class PathEdge5D
	{
		public PathEdge5D( TopoDS_Edge pathEdge, TopoDS_Face componentFace )
		{
			PathEdge = pathEdge;
			ComponentFace = componentFace;
		}

		public TopoDS_Edge PathEdge
		{
			get; private set;
		}

		public TopoDS_Face ComponentFace
		{
			get; private set;
		}
	}

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
			private set
			{
				m_ToolVec = new gp_Dir( value.XYZ() );
			}
		}

		// using backing field to prevent modified outside
		gp_Dir m_ToolVec;
	}

	internal class CAMData
	{
		//CAD property
		public CAMData( List<PathEdge5D> pathDataList, bool isClosed )
		{
			PathDataList = pathDataList;
			IsClosed = isClosed;

			// build raw data
			BuildCADPointList();
			BuildCAMPointList();
		}

		public List<PathEdge5D> PathDataList
		{
			get; private set;
		}

		public List<CADPoint> CADPointList
		{
			get; private set;
		}

		// CAM property
		public List<CAMPoint> CAMPointList
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

		public void GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_ToolVecModifyMap[ index ].Item2;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
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

		// view testing
		public List<int> EdgeStartIndex
		{
			get; private set;
		}

		// backing fields
		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();
		bool m_IsReverse = false;
		int m_StartPoint = 0;
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap = new Dictionary<CADPoint, CADPoint>();

		// dirty flag
		bool m_IsDirty = false;

		// precision
		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;

		void BuildCADPointList()
		{
			CADPointList = new List<CADPoint>();
			EdgeStartIndex = new List<int>();
			if( PathDataList == null ) {
				return;
			}

			// go through the contour edges
			for( int i = 0; i < PathDataList.Count; i++ ) {
				TopoDS_Edge edge = PathDataList[ i ].PathEdge;
				TopoDS_Face shellFace = PathDataList[ i ].ComponentFace;
				TopoDS_Face solidFace = PathDataList[ i ].ComponentFace; // TODO: set solid face

				// break the edge into segment points by interval
				EdgeStartIndex.Add( CADPointList.Count );
				CADPointList.AddRange( GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace, i > 0 ) );
			}
		}

		List<CADPoint> GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace, bool isConnect )
		{
			List<CADPoint> result = new List<CADPoint>();

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
				if( dLength > PRECISION_MAX_LENGTH ) {
					int nSubSegmentCount = (int)Math.Ceiling( dLength / PRECISION_MAX_LENGTH );
					double dSubSegmentLength = ( qUD.Parameter( i + 1 ) - qUD.Parameter( i ) ) / nSubSegmentCount;
					for( int j = 1; j < nSubSegmentCount; j++ ) {
						segmentParamList.Add( qUD.Parameter( i ) + j * dSubSegmentLength );
					}
				}
			}
			segmentParamList.Add( qUD.Parameter( qUD.NbPoints() ) );
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

				// map the start point to the last point of the previous edge
				if( isConnect && i == 0 && result.Count > 0 ) {
					CADPoint lastPoint = result.Last();
					m_ConnectPointMap[ lastPoint ] = cadPoint;
				}

				// add the point to the list
				else {
					result.Add( cadPoint );
				}
			}
			return result;
		}

		void BuildCAMPointList()
		{
			m_IsDirty = false;
			m_CAMPointList = new List<CAMPoint>();
			SetToolVec();
			SetStartPoint();
			SetOrientation();
		}

		void SetToolVec()
		{
			for( int i = 0; i < CADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = CADPointList[ i ];
				CAMPoint camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				m_CAMPointList.Add( camPoint );
			}
			ModifyToolVec();
		}

		void ModifyToolVec()
		{
			if( m_ToolVecModifyMap.Count == 0 ) {
				return;
			}

			// sort the modify data by index
			List<int> indexInOrder = m_ToolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();

			// modify the tool vector
			for( int i = 0; i < indexInOrder.Count; i++ ) {

				// get start and end index
				int nStartIndex = indexInOrder[ i ];
				int nEndIndex = indexInOrder[ ( i + 1 ) % indexInOrder.Count ];
				int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + m_CAMPointList.Count : nEndIndex;

				// do slerp from start to end
				gp_Vec startVec = GetVecFromAB( m_CAMPointList[ nStartIndex ],
					m_ToolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
					m_ToolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
				gp_Vec endVec = GetVecFromAB( m_CAMPointList[ nEndIndex ],
					m_ToolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
					m_ToolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );
				gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
				gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
				m_CAMPointList[ nStartIndex ] = new CAMPoint( m_CAMPointList[ nStartIndex ].CADPoint, new gp_Dir( startVec ) );
				for( int j = nStartIndex + 1; j < nEndIndexModify; j++ ) {
					double t = ( j - nStartIndex ) / (double)( nEndIndexModify - nStartIndex );
					gp_Quaternion q = new gp_Quaternion();
					slerp.Interpolate( t, ref q );
					gp_Trsf trsf = new gp_Trsf();
					trsf.SetRotation( q );
					gp_Dir toolVec = new gp_Dir( startVec.Transformed( trsf ) );
					m_CAMPointList[ j % m_CAMPointList.Count ] = new CAMPoint( m_CAMPointList[ j % m_CAMPointList.Count ].CADPoint, toolVec );
				}
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

		void SetStartPoint()
		{
			// rearrange cam points to start from the strt index
			if( StartPoint != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + StartPoint ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}
		}

		void SetOrientation()
		{
			// reverse the cad points if is reverse
			if( IsReverse ) {
				m_CAMPointList.Reverse();

				// modify start point index for closed path
				if( IsClosed ) {
					CAMPoint lastPoint = m_CAMPointList.Last();
					m_CAMPointList.Remove( lastPoint );
					m_CAMPointList.Insert( 0, lastPoint );
				}
			}
		}
	}
}
