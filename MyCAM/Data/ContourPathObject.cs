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
using static MyCAM.Helper.CADPretreatHelper;

namespace MyCAM.Data
{
	internal class ContourPathObject : PathObject
	{
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<PathEdge5D> pathDataList )
			: base( szUID, shape )
		{
			if( pathDataList == null || pathDataList.Count == 0 ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			bool isClosed = DetermineIfClosed( shape );
			CADError isBuildDone = BuildCADSegment( pathDataList, out List<ICADSegment> cadSegList );
			if( isBuildDone != CADError.Done || cadSegList == null || cadSegList.Count == 0 ) {
				throw new Exception( "ContourPathObject CAD segment build failed" );
			}
			m_CADSegmentList = cadSegList;
			m_CraftData = new CraftData( szUID );
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADSegmentList, m_CraftData, isClosed );
			m_CraftData.StartPointIndex = new SegmentPointIndex( 0, 0 );
		}

		// this is for the file read constructor
		public ContourPathObject( string szUID, TopoDS_Shape shape, List<ICADSegment> cadSegmentList, CraftData craftData )
			: base( szUID, shape )
		{
			if( cadSegmentList == null || cadSegmentList.Count == 0 || craftData == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			bool isClosed = DetermineIfClosed( shape );
			m_CADSegmentList = cadSegmentList;
			m_CraftData = craftData;
			m_ContourCacheInfo = new ContourCacheInfo( szUID, m_CADSegmentList, m_CraftData, isClosed );
		}

		public IReadOnlyList<ICADSegment> CADSegmentList
		{
			get
			{
				//fix: 我建議這裡就先不要 clone 了，後續再找機會優化
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
			foreach( ICADSegment cadSegment in m_CADSegmentList ) {
				cadSegment.Transform( transform );
			}

			// Step3:recalculate cache info because CAD points have changed
			m_ContourCacheInfo.Transform();
		}

		CADError BuildCADSegment( List<PathEdge5D> pathEdge5DList, out List<ICADSegment> cadSegmentList )
		{
			cadSegmentList = new List<ICADSegment>();
			if( pathEdge5DList == null ) {
				// fix:應該回傳空 list?
				return CADError.ParamError;
			}
			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {

					// fix: 這個精度的命名需要調整一下
					CADError result = DiscretizeLineToBuildData( edge, shellFace, MAX_DISTANCE_BETWEEN_POINTS, out CADSegBuildData cadSegBuildData );
					if( result != CADError.Done ) {
						return result;
					}
					// fix: 這邊 tempCADPointList 應該要先檢查 ===>改為CADSegmentBuilder自己檢查然後吐出結果
					CADError buildResult = CADSegmentBuilder.BuildCADSegment( cadSegBuildData.PointList, ESegmentType.Line, cadSegBuildData.SegmentLength, cadSegBuildData.SubSegmentLength, cadSegBuildData.PerChordLength, out ICADSegment cadSegment );
					// fix: 這邊 failed 的可能是哪些情況，是否應該直接終止整個流程?===>改為直接回傳錯誤碼
					if( buildResult != CADError.Done ) {
						return buildResult;
					}
					cadSegmentList.Add( cadSegment );
				}

				// this curve is arc choose the best option from the two options (chord error vs equal length)
				// fix: 這邊 center 跟 arcAngle 沒有用到?==>done
				else if( GeometryTool.IsCircularArc( edge, out _, out _, out _, out _ ) ) {
					CADError result = DiscretizeArcToSegmentBuildData( edge, shellFace, out List<CADSegBuildData> cadSegBuildDataList, Math.PI / 2 );
					if( result != CADError.Done || cadSegBuildDataList == null || cadSegBuildDataList.Count == 0 ) {
						return result;
					}

					for( int j = 0; j < cadSegBuildDataList.Count; j++ ) {
						CADError buildResult = CADSegmentBuilder.BuildCADSegment( cadSegBuildDataList[ j ].PointList, ESegmentType.Arc, cadSegBuildDataList[ j ].SegmentLength, cadSegBuildDataList[ j ].SubSegmentLength, cadSegBuildDataList[ j ].PerChordLength, out ICADSegment cadSegment );
						if( buildResult != CADError.Done ) {
							return buildResult;
						}
						cadSegmentList.Add( cadSegment );
					}
				}

				// use chord error split
				else {

					// separate this bspline
					CADError result = DiscretizeBsplineToBuildData( edge, shellFace, out List<CADSegBuildData> cadSegmentBuildDataList );
					if( result != CADError.Done ) {
						return result;
					}
					if( cadSegmentBuildDataList == null || cadSegmentBuildDataList.Count == 0 ) {
						return CADError.DiscretizFaild;
					}
					for( int j = 0; j < cadSegmentBuildDataList.Count; j++ ) {
						// fix: 建議架構統一，ChordLength 在 PretreatmentHelper 算好傳回來===>上面DiscretizeBsplineToBuildData已經算回來了
						CADError buildResult = CADSegmentBuilder.BuildCADSegment( cadSegmentBuildDataList[ j ].PointList, ESegmentType.Line, cadSegmentBuildDataList[ j ].SegmentLength, cadSegmentBuildDataList[ j ].SubSegmentLength, cadSegmentBuildDataList[ j ].PerChordLength, out ICADSegment cadSegment );
						if( buildResult != CADError.Done ) {
							return buildResult;
						}
						cadSegmentList.Add( cadSegment );
					}
				}
			}
			return CADError.Done;
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
		const double MAX_DISTANCE_BETWEEN_POINTS = 1;
	}
}