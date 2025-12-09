using MyCAM.CacheInfo;
using MyCAM.Data.GeomDataFolder;
using MyCAM.Editor.Factory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class RectanglePathObject : PathObject
	{
		internal RectanglePathObject( string szUID, TopoDS_Shape shape, RectangleGeomData rectangleGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || rectangleGeomData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "RectanglePathObject constructing argument null" );
			}

			CraftData craftData = new CraftData( szUID );
			InitializeRectanglePathObject( szUID, rectangleGeomData, craftData, contourPathObject );
		}

		// read file
		public RectanglePathObject( string szUID, TopoDS_Shape shape, RectangleGeomData rectangleGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || rectangleGeomData == null || craftData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "RectanglePathObject constructing argument null" );
			}

			InitializeRectanglePathObject( szUID, rectangleGeomData, craftData, contourPathObject );
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Rectangle;
			}
		}

		public RectangleGeomData RectangleGeomData
		{
			get
			{
				return m_RectangleGeomData;
			}
		}

		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public RectangleCacheInfo RectangleCacheInfo
		{
			get
			{
				return m_RectangleCacheInfo;
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
			// Step1:tranform shape first
			base.DoTransform( transform );

			// Step2:then transform CAD points because they depend on shape
			ContourPathObject.DoTransform( transform );

			// Step3:recalculate cache info because CAD points have changed
			m_RectangleCacheInfo.Transform( transform );
		}

		void InitializeRectanglePathObject( string szUID, RectangleGeomData rectangleGeomData, CraftData craftData, ContourPathObject contourPathObject )
		{
			m_RectangleGeomData = rectangleGeomData;
			m_CraftData = craftData;
			m_ContourPathObject = contourPathObject;

			PatternFactory patternFactory = new PatternFactory( contourPathObject.ContourGeomData, rectangleGeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();
			m_RectangleCacheInfo = new RectangleCacheInfo( szUID, coordinateInfo, rectangleGeomData, m_CraftData );
		}

		CraftData m_CraftData;
		RectangleCacheInfo m_RectangleCacheInfo;
		RectangleGeomData m_RectangleGeomData;
		ContourPathObject m_ContourPathObject;
	}
}
