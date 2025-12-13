using MyCAM.PathCache;
using MyCAM.Helper;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal abstract class StdPatternObjectBase : PathObject
	{
		protected StdPatternObjectBase( string szUID, TopoDS_Shape shape, IStdPatternGeomData geomData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || geomData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "StandardPatternBasedPathObject constructing argument null" );
			}

			m_GeomData = geomData;
			m_CraftData = new CraftData();
			m_ContourPathObject = contourPathObject;

			InitializeCacheInfo();
		}


		protected StdPatternObjectBase( string szUID, TopoDS_Shape shape, IStdPatternGeomData geomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || geomData == null || craftData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "StandardPatternBasedPathObject constructing argument null" );
			}

			m_GeomData = geomData;
			m_CraftData = craftData;
			m_ContourPathObject = contourPathObject;

			InitializeCacheInfo();
		}

		public IStdPatternGeomData GeomData
		{
			get
			{
				return m_GeomData;
			}
		}

		public ContourPathObject ContourPathObject
		{
			get
			{
				return m_ContourPathObject;
			}
		}

		public StdPatternCacheBase StandatdPatternCacheInfo
		{
			get
			{
				return m_StandatdPatternCacheInfo;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// Step1: transform shape first
			base.DoTransform( transform );

			// Step2: then transform CAD points because they depend on shape
			m_ContourPathObject.DoTransform( transform );

			// Step3: recalculate cache info because CAD points have changed
			m_StandatdPatternCacheInfo.DoTransform( transform );
		}

		void InitializeCacheInfo()
		{
			gp_Ax3 coordinateInfo = StdPatternHelper.GetPatternRefCoord( m_ContourPathObject.ContourGeomData.RefCenterDir, m_GeomData.RotatedAngle_deg );

			// factory automatically determines the correct CacheInfo type based on GeomData type
			m_StandatdPatternCacheInfo = (StdPatternCacheBase)StdPatternCacheFactory.CreateCacheInfo( coordinateInfo, m_GeomData, m_CraftData );
		}

		IStdPatternGeomData m_GeomData;
		StdPatternCacheBase m_StandatdPatternCacheInfo;
		ContourPathObject m_ContourPathObject;
	}
}
