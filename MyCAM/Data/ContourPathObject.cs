using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.App;
using MyCAM.CacheInfo;
using MyCAM.Helper;
using OCC.BRep;
using OCC.gp;
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

			// re: 這邊是否可能出現 count 為 0 的情況? 需要保護?
			m_CADSegmentList = BuildCADSegment( pathDataList, isClosed );
			m_CraftData = new CraftData( szUID );
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADSegmentList, m_CraftData, isClosed );

			// re: 這裡應該就讓他是 0,0 也沒關係?
			m_CraftData.StartPointIndex = new SegmentPointIndex( m_CADSegmentList.Count - 1, m_CADSegmentList[ CADSegmentList.Count - 1 ].PointList.Count - 1 );
		}

		// this is for the file read constructor
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<ICADSegmentElement> cadSegmentList, CraftData craftData)
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

		public List<ICADSegmentElement> CADSegmentList
		{
			get
			{
				// re: 我建議這裡就先不要 clone 了，後續再找機會優化
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
			foreach( ICADSegmentElement cadSegment in m_CADSegmentList ) {
				cadSegment.Transform( transform );
			}

			// Step3:recalculate cache info because CAD points have changed
			m_ContourCacheInfo.Transform();
		}

		// re: 這個 is close 引數沒有用到了?
		List<ICADSegmentElement> BuildCADSegment( List<PathEdge5D> pathEdge5DList, bool isClosed )
		{
			List<ICADSegmentElement> cadSegmentList = new List<ICADSegmentElement>();

			if( pathEdge5DList == null ) {
				// re: 應該回傳空 list?
				return null;
			}
			// re: 空行
			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {

					// re: 這個精度的命名需要調整一下
					List<CADPoint> tempCADPointList = PretreatmentHelper.GetSegmentPointsByEqualLength( edge, shellFace, PRECISION_MAX_LENGTH, out double dEdgeLength, out double dPerArcLength, out double dPerChordLength );
					// re: 這邊 tempCADPointList 應該要先檢查
					bool buildSuccess = CADCAMSegmentBuilder.BuildCADSegment( tempCADPointList, EContourType.Line, dEdgeLength, dPerArcLength, dPerChordLength, out ICADSegmentElement cadSegment );
					// re: 這邊 failed 的可能是哪些情況，是否應該直接終止整個流程?
					if( buildSuccess ) {
						cadSegmentList.Add( cadSegment );
					}
				}

				// this curve is arc choose the best option from the two options (chord error vs equal length)
				// re: 這邊 center 跟 arcAngle 沒有用到?
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
						// re: 建議架構統一，ChordLength 在 PretreatmentHelper 算好傳回來
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

		List<ICADSegmentElement> m_CADSegmentList;
		CraftData m_CraftData;
		ContourCacheInfo m_ContourCacheInfo;

		// Discretize parameters
		Dictionary<CADPoint, CADPoint> m_ConnectPointMap = new Dictionary<CADPoint, CADPoint>();

		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
	}
}