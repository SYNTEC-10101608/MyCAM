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

			InitializeCache();
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

			InitializeCache();
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

		public IStdPatternCache StandatdPatternCache
		{
			get
			{
				return m_StandatdPatternCache;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// Step1: transform shape first
			base.DoTransform( transform );

			// Step2: then transform CAD points because they depend on shape
			m_ContourPathObject.DoTransform( transform );

			// Step3: recalculate cache info because CAD points have changed
			m_StandatdPatternCache.DoTransform( transform );
		}

		void InitializeCache()
		{
			gp_Ax3 refCoord = StdPatternHelper.GetPatternRefCoord( m_ContourPathObject.ContourGeomData.RefCenterDir, m_GeomData.RotatedAngle_deg );

			// factory automatically determines the correct Cache type based on GeomData type
			m_StandatdPatternCache = (StdPatternCacheBase)StdPatternCacheFactory.CreatePathCache( refCoord, m_GeomData, m_CraftData );
		}

		IStdPatternGeomData m_GeomData;
		IStdPatternCache m_StandatdPatternCache;
		ContourPathObject m_ContourPathObject;
	}
}
