using MyCAM.CacheInfo;
using MyCAM.Data.GeomDataFolder;
using MyCAM.StandardPatternFactory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data.PathObjectFolder
{
	internal class PolygonPathObject : PathObject
	{
		internal PolygonPathObject( string szUID, TopoDS_Shape shape, PolygonGeomData polygonGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || polygonGeomData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "PolygonPathObject constructing argument null" );
			}

			CraftData craftData = new CraftData();
			InitializePolygonPathObject( szUID, polygonGeomData, craftData, contourPathObject );
		}

		// read file
		internal PolygonPathObject( string szUID, TopoDS_Shape shape, PolygonGeomData polygonGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || polygonGeomData == null || craftData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "PolygonPathObject constructing argument null" );
			}

			InitializePolygonPathObject( szUID, polygonGeomData, craftData, contourPathObject );
		}

		public override PathType PathType
		{
			get
			{
				return m_PolygonGeomData.PathType;
			}
		}

		public PolygonGeomData PolygonGeomData
		{
			get
			{
				return m_PolygonGeomData;
			}
		}

		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public PolygonCacheInfo PolygonCacheInfo
		{
			get
			{
				return m_PolygonCacheInfo;
			}
		}

		public ContourPathObject ContourPathObject
		{
			get
			{
				return m_ContourPathObject;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// Step1: transform shape first
			base.DoTransform( transform );

			// Step2: then transform CAD points because they depend on shape
			m_ContourPathObject.DoTransform( transform );

			// Step3: recalculate cache info because CAD points have changed
			m_PolygonCacheInfo.DoTransform( transform );
		}

		void InitializePolygonPathObject( string szUID, PolygonGeomData polygonGeomData, CraftData craftData, ContourPathObject contourPathObject )
		{
			m_PolygonGeomData = polygonGeomData;
			m_CraftData = craftData;
			m_ContourPathObject = contourPathObject;

			PatternFactory patternFactory = new PatternFactory( contourPathObject.ContourGeomData, polygonGeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();
			m_PolygonCacheInfo = new PolygonCacheInfo( coordinateInfo, polygonGeomData, m_CraftData );
		}

		CraftData m_CraftData;
		PolygonCacheInfo m_PolygonCacheInfo;
		PolygonGeomData m_PolygonGeomData;
		ContourPathObject m_ContourPathObject;
	}
}
