using MyCAM.App;
using MyCAM.CacheInfo;
using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepAdaptor;
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
	internal class ContourPathObject : PathObject
	{
		public ContourPathObject( string szUID, TopoDS_Shape shapeData, List<PathEdge5D> pathDataList )
			: base( szUID, shapeData )
		{
			TopoDS_Vertex startVertex = new TopoDS_Vertex();
			TopoDS_Vertex endVertex = new TopoDS_Vertex();
			TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );
			gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
			gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );
			bool isClosed = startPoint.IsEqual( endPoint, 1e-3 );

			m_CraftData = new CraftData( szUID, isClosed );
			m_CADPointList = BuildCADPointList( pathDataList );
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADPointList, m_CraftData );
		}

		public ContourPathObject( string szUID, TopoDS_Shape shapeData, List<CADPoint> cadPointList, CraftData craftData )
			: base( szUID, shapeData )
		{
			if( shapeData == null || cadPointList == null || craftData == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			m_CADPointList = cadPointList;
			m_CraftData = craftData;
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADPointList, m_CraftData );
		}

		public List<CADPoint> CADPointList
		{
			get
			{
				return m_CADPointList;
			}
		}

		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public ContourCacheInfo ContourCacheInfo
		{
			get
			{
				return m_ContourCacheInfo;
			}
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Contour;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			base.DoTransform( transform );
			foreach( CADPoint cadPoint in CADPointList ) {
				cadPoint.Transform( transform );
			}
		}

		List<CADPoint> BuildCADPointList( List<PathEdge5D> pathEdge5DList )
		{
			m_CADPointList = new List<CADPoint>();
			if( pathEdge5DList == null ) {
				return new List<CADPoint>();
			}

			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face
				CADPointList.AddRange( GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace,
					i == 0, i == pathEdge5DList.Count - 1 ) );
			}
			return CADPointList;
		}

		List<CADPoint> GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace, bool bFirst, bool bLast )
		{
			List<CADPoint> oneSegmentPointList = new List<CADPoint>();

			// get curve parameters
			BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge, shellFace );
			double dStartU = adC.FirstParameter();
			double dEndU = adC.LastParameter();

			// break the curve into segments with given deflection precision
			GCPnts_QuasiUniformDeflection qUD = new GCPnts_QuasiUniformDeflection( adC, MyApp.PRECISION_DEFLECTION, dStartU, dEndU );

			// break the curve into segments with given max length
			List<double> segmentParamList = new List<double>();
			for( int i = 1; i < qUD.NbPoints(); i++ ) {
				segmentParamList.Add( qUD.Parameter( i ) );
				double dLength = GCPnts_AbscissaPoint.Length( adC, qUD.Parameter( i ), qUD.Parameter( i + 1 ) );

				// add sub-segments if the length is greater than max length
				if( dLength > MyApp.PRECISION_MAX_LENGTH ) {
					int nSubSegmentCount = (int)Math.Ceiling( dLength / MyApp.PRECISION_MAX_LENGTH );
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
				if( !bFirst && i == 0 && m_CADPointList.Count > 0 ) {
					CADPoint lastPoint = m_CADPointList.Last();
					m_CADPointList.RemoveAt( m_CADPointList.Count - 1 );
					m_ConnectPointMap[ cadPoint ] = lastPoint;
					oneSegmentPointList.Add( cadPoint );
				}
				else if( bLast && i == segmentParamList.Count - 1 && m_CADPointList.Count > 0 ) {

					// map the last point to the start point
					if( m_CraftData.IsClosed ) {
						CADPoint firstPoint = m_CADPointList[ 0 ];
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


		List<CADPoint> m_CADPointList;
		CraftData m_CraftData;
		ContourCacheInfo m_ContourCacheInfo;

		// Discretize parameters
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap = new Dictionary<CADPoint, CADPoint>();
	}
}