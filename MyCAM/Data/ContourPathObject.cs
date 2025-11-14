using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.App;
using MyCAM.CacheInfo;
using MyCAM.Helper;
using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;

namespace MyCAM.Data
{
	internal class ContourPathObject : PathObject
	{
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<PathEdge5D> pathDataList )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || pathDataList == null || pathDataList.Count == 0 ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}

			bool isClosed = DetermineIfClosed( shape );
			m_CADPointList = BuildCADPointList( pathDataList, isClosed );
			m_CADSegmentList = BuildCADSegment( pathDataList, isClosed );
			m_CraftData = new CraftData( szUID );
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADPointList, m_CraftData, isClosed );
		}

		// this is for the file read constructor
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<ICADSegmentElement> cadSegmentList, CraftData craftData )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || cadSegmentList == null || cadSegmentList.Count == 0 || craftData == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			bool isClosed = DetermineIfClosed( shape );
			m_CADSegmentList = cadSegmentList;
			m_CraftData = craftData;
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADSegmentList, m_CraftData, isClosed );
		}

		public List<CADPoint> CADPointList
		{
			get
			{
				return m_CADPointList;
			}
		}

		public List<ICADSegmentElement> CADSegmentList
		{
			get
			{
				return m_CADSegmentList;
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
			// fix:
			// Step1:tranform shape first
			base.DoTransform( transform );

			// Step2:then transform CAD points because they depend on shape
			foreach( CADPoint cadPoint in m_CADPointList ) {
				cadPoint.Transform( transform );
			}

			// Step3:recalculate cache info because CAD points have changed
			m_ContourCacheInfo.Transform();
		}

		List<CADPoint> BuildCADPointList( List<PathEdge5D> pathEdge5DList, bool isClosed )
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
					i == 0, i == pathEdge5DList.Count - 1, isClosed ) );
			}
			return CADPointList;
		}

		List<ICADSegmentElement> BuildCADSegment( List<PathEdge5D> pathEdge5DList, bool isClosed )
		{
			List<ICADSegmentElement> cadSegmentList = new List<ICADSegmentElement>();

			if( pathEdge5DList == null ) {
				return null;
			}
			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {
					List<CADPoint> tempCADPointList = PretreatmentHelper.GetSegmentPointsByEqualLength( edge, shellFace, PRECISION_MAX_LENGTH, out double dEdgeLength, out double dPerArcLength, out double dPerChordLength );
					bool buildSuccess = CADCAMSegmentBuilder.BuildCADSegment( tempCADPointList, EContourType.Line, dEdgeLength, dPerArcLength, dPerChordLength, out ICADSegmentElement cadSegment );
					if( buildSuccess ) {
						cadSegmentList.Add( cadSegment );
					}
				}

				// this curve is arc choose the best option from the two options (chord error vs equal length)
				else if( GeometryTool.IsCircularArc( edge, out _, out _, out gp_Dir centerDir, out double arcAngle ) ) {
					List<List<CADPoint>> cadPointList = PretreatmentHelper.SplitArcEdge( edge, shellFace, out List<double> eachSegmentLength, out List<double> dEachArcLength, out List<double> dEachChordLength );
					if( cadPointList == null || cadPointList.Count == 0 ) {
						continue;
					}
					for( int j = 0; j < cadPointList.Count; j++ ) {
						bool buildSuccess = CADCAMSegmentBuilder.BuildCADSegment( cadPointList[ j ], EContourType.Arc, eachSegmentLength[ j ], dEachArcLength[ j ], dEachChordLength[ j ], out ICADSegmentElement cadSegment );
						if( buildSuccess ) {
							cadSegmentList.Add( cadSegment );
						}
					}
				}

				// use chord error split
				else {

					// separate this bspline
					List<List<CADPoint>> cadPointList = PretreatmentHelper.GetBsplineToEdgeList( edge, shellFace, out List<double> eachSegmentLength, out List<double> eachArcLength );
					if( cadPointList == null || cadPointList.Count == 0 ) {
						continue;
					}
					for( int j = 0; j < cadPointList.Count; j++ ) {

						// calculate chord length
						double dChordLength = cadPointList[ j ].First().Point.Distance( cadPointList[ j ][ 1 ].Point );
						bool buildSuccess = CADCAMSegmentBuilder.BuildCADSegment( cadPointList[ j ], EContourType.Line, eachSegmentLength[ j ], eachArcLength[ j ], dChordLength, out ICADSegmentElement cadSegment );
						if( buildSuccess ) {
							cadSegmentList.Add( cadSegment );
						}
					}
				}
			}
			return cadSegmentList;
		}



		List<CADPoint> GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace, bool bFirst, bool bLast, bool isClosed )
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
					if( isClosed ) {
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

		bool DetermineIfClosed( TopoDS_Shape shapeData )
		{
			if( shapeData == null || shapeData.IsNull() )
				return false;

			try {
				TopoDS_Vertex startVertex = new TopoDS_Vertex();
				TopoDS_Vertex endVertex = new TopoDS_Vertex();
				TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );

				gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
				gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );

				return startPoint.IsEqual( endPoint, 1e-3 );
			}
			catch( Exception ex ) {
				MyApp.Logger.ShowOnLogPanel( $"Error occurred while determining a closed path.: {ex.Message}", MyApp.NoticeType.Warning );
				return false;
			}
		}

		List<CADPoint> m_CADPointList;
		List<ICADSegmentElement> m_CADSegmentList;
		CraftData m_CraftData;
		ContourCacheInfo m_ContourCacheInfo;

		// Discretize parameters
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap = new Dictionary<CADPoint, CADPoint>();

		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
	}
}