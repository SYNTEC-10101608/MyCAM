using MyCAM.PathCache;
using OCC.gp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;

namespace MyCAM.Data
{
	public class ContourPathObject : PathObject
	{
		// overload that takes pre-computed isClosed flag, for callers that build path from edge list directly
		public ContourPathObject( string szUID, List<PathEdge5D> pathDataList, bool isClosed,
			double maxDeflection = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
			OCCTool.EDiscretizeMethod discretizeMethod = OCCTool.GeometryTool.DEFAULT_DISCRETIZE_METHOD,
			Action<LoadingBarInfo> reportProgress = null )
			: base( szUID )
		{
			if( string.IsNullOrEmpty( szUID ) || pathDataList == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			if( pathDataList.Count == 0 ) {
				throw new ArgumentException( "ContourPathObject constructing argument empty pathDataList" );
			}

			m_ContourGeomData = new ContourGeomData( pathDataList, isClosed, maxDeflection, maxEdgeLength, discretizeMethod, reportProgress );
			m_CraftData = new CraftData();
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.BuildPathCache ) );
			m_ContourCache = new ContourCache( m_ContourGeomData, m_CraftData );
		}

		// this is for the file read constructor
		public ContourPathObject( string szUID, TopoDS_Shape shape, ContourGeomData geomData, CraftData craftData )
			: base( szUID )
		{
			if( string.IsNullOrEmpty( szUID ) || geomData == null || craftData == null ) {
				throw new ArgumentNullException( "ContourPathObject constructing argument null" );
			}
			m_ContourGeomData = geomData;
			m_CraftData = craftData;
			m_ContourCache = new ContourCache( m_ContourGeomData, m_CraftData );
			if( !m_ContourCache.ValidateRCornerData() ) {
				m_CraftData.ClearRCorner();
			}
			BuildUnClosePathStartEndPntParam();
		}

		public ContourGeomData GeomData
		{
			get
			{
				return m_ContourGeomData;
			}
		}

		public ContourCache ContourCache
		{
			get
			{
				return m_ContourCache;
			}
		}

		internal void SetCraftData( CraftData craftData )
		{
			ContourCache cache = new ContourCache( m_ContourGeomData, craftData );
			m_CraftData = craftData;
			m_ContourCache = cache;
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
			// Step1:transform the geom data
			m_ContourGeomData.DoTransform( transform );

			// Step2:recalculate cache, currently dont really need gp_trsf
			m_ContourCache.DoTransform( transform );
		}

		// old file may have no start end point param for unclosed path
		void BuildUnClosePathStartEndPntParam()
		{
			if( m_ContourGeomData.IsClosed ) {
				return;
			}
			int lastIndex = m_ContourCache.MainPathPointList.Count - 1;

			// cad end point is in the map
			m_CraftData.ToolVecModifyMap.TryGetValue( lastIndex, out ToolVecModifyData toolVecModifyData );
			if( toolVecModifyData == null ) {
				return;
			}
			ToolVecModifyData modifyData = toolVecModifyData.Clone();
			if( m_CraftData.IsPathReverse ) {
				m_CraftData.StartPntToolVecData.StartPnt = modifyData;
				return;
			}
			m_CraftData.StartPntToolVecData.EndPnt = modifyData;
		}

		ContourGeomData m_ContourGeomData;
		ContourCache m_ContourCache;

		const string DETERMINE_CLOSED_PATH_ERROR = "判斷路徑是否封閉時發生錯誤: {0}";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ContourPathObject" );
	}
}
