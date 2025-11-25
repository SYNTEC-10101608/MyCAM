using MyCAM.App;
using MyCAM.CacheInfo;
using MyCAM.Helper;
using OCC.BRep;
using OCC.gp;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static MyCAM.Helper.CADPretreatHelper;

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
			bool isBuildDone =  BuildCADSegment( pathDataList, out List<ICADSegment> cadSegList );
			if ( isBuildDone == false || cadSegList == null || cadSegList.Count == 0 ) {
				throw new Exception( "ContourPathObject CAD segment build failed" );
			}
			m_CADSegmentList = cadSegList;
			m_CraftData = new CraftData( szUID );
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADSegmentList, m_CraftData, isClosed );
			m_CraftData.StartPointIndex = new SegmentPointIndex( m_CADSegmentList.Count - 1, m_CADSegmentList[ CADSegmentList.Count - 1 ].PointList.Count - 1 );
		}

		// this is for the file read constructor
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<ICADSegment> cadSegmentList, CraftData craftData )
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

		public List<ICADSegment> CADSegmentList
		{
			get
			{
				return m_CADSegmentList.Select( segment => segment.Clone() ).ToList();
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
			foreach( ICADSegment cadSegment in m_CADSegmentList ) {
				cadSegment.Transform( transform );
			}

			// Step3:recalculate cache info because CAD points have changed
			m_ContourCacheInfo.Transform();
		}

		bool BuildCADSegment( List<PathEdge5D> pathEdge5DList , out List<ICADSegment> cadSegmentList )
		{
			cadSegmentList = new List<ICADSegment>();
			if( pathEdge5DList == null ) {
				// fix:應該回傳空 list?
				return false;
			}
			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {
					bool isDiscretizeDone = DiscretizeLineToCADPnt( edge, shellFace, PRECISION_MAX_LENGTH, out CADSegBuildData cadSegBuildData );
					if (isDiscretizeDone == false ) {
						return false;
					}
					bool buildDone = CADSegmentBuilder.BuildCADSegment( cadSegBuildData.PointList, ESegmentType.Line, cadSegBuildData.SegmentLength, cadSegBuildData.SubSegmentLength, cadSegBuildData.PerChordLength, out ICADSegment cadSegment );
					if( buildDone == false ) {
						return false;
					}
					cadSegmentList.Add( cadSegment );
				}

				// this curve is arc choose the best option from the two options (chord error vs equal length)
				else if( GeometryTool.IsCircularArc( edge, out _, out _, out gp_Dir centerDir, out double arcAngle ) ) {
					bool isDiscretizeDone = DiscretizeArcToCADSegments( edge, shellFace, out List<CADSegBuildData> cadSegBuildDataList, Math.PI / 2 );
					if( isDiscretizeDone == false || cadSegBuildDataList == null || cadSegBuildDataList.Count == 0 ) {
						return false;
					}

					for( int j = 0; j < cadSegBuildDataList.Count; j++ ) {
						bool buildDone = CADSegmentBuilder.BuildCADSegment( cadSegBuildDataList[ j ].PointList, ESegmentType.Arc, cadSegBuildDataList[ j ].SegmentLength, cadSegBuildDataList[ j ].SubSegmentLength, cadSegBuildDataList[ j ].PerChordLength, out ICADSegment cadSegment );
						if( buildDone == false) {
							return false;
						}
						cadSegmentList.Add( cadSegment );
					}
				}

				// use chord error split
				else {

					// separate this bspline
					bool isDiscretizeDone = CADPretreatHelper.DiscretizeBsplineToCADPnt( edge, shellFace, out List<CADSegBuildData> cadSegmentBuildDataList );
					if( isDiscretizeDone == false || cadSegmentBuildDataList == null || cadSegmentBuildDataList.Count == 0 ) {
						return false;
					}
					for( int j = 0; j < cadSegmentBuildDataList.Count; j++ ) {
						bool buildDone = CADSegmentBuilder.BuildCADSegment( cadSegmentBuildDataList[ j ].PointList, ESegmentType.Line, cadSegmentBuildDataList[ j ].SegmentLength, cadSegmentBuildDataList[ j ].SubSegmentLength, cadSegmentBuildDataList[ j ].PerChordLength, out ICADSegment cadSegment );
						if( buildDone == false ) {
							return false;
						}
						cadSegmentList.Add( cadSegment );
					}
				}
			}
			return true;
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

		List<ICADSegment> m_CADSegmentList;
		CraftData m_CraftData;
		ContourCacheInfo m_ContourCacheInfo;

		// Discretize parameters
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap = new Dictionary<CADPoint, CADPoint>();

		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
	}
}