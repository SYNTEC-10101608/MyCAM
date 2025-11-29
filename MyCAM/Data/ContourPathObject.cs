using MyCAM.App;
using MyCAM.CacheInfo;
using MyCAM.Helper;
using OCC.BRep;
using OCC.gp;
using OCC.TopExp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;

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
			BuildCADError isBuildDone = CADPretreatHelper.BuildCADSegment( pathDataList, out List<ICADSegment> cadSegList );
			if( isBuildDone != BuildCADError.Done || cadSegList == null || cadSegList.Count == 0 ) {
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
	}
}