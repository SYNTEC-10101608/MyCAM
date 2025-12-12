using MyCAM.CacheInfo;
using MyCAM.StandardPatternFactory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal abstract class StandardPatternBasedPathObject : PathObject
	{
		protected StandardPatternBasedPathObject( string szUID, TopoDS_Shape shape, IStandardPatternGeomData geomData, ContourPathObject contourPathObject )
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


		protected StandardPatternBasedPathObject( string szUID, TopoDS_Shape shape, IStandardPatternGeomData geomData, CraftData craftData, ContourPathObject contourPathObject )
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

		public IStandardPatternGeomData GeomData
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

		public StandardPatternBasedCacheInfo StandatdPatternCacheInfo
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
			PatternFactory patternFactory = new PatternFactory( m_ContourPathObject.ContourGeomData, m_GeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();

			// factory automatically determines the correct CacheInfo type based on GeomData type
			m_StandatdPatternCacheInfo = (StandardPatternBasedCacheInfo)StandardPatternCacheInfoFactory.CreateCacheInfo( coordinateInfo, m_GeomData, m_CraftData );
		}

		IStandardPatternGeomData m_GeomData;
		StandardPatternBasedCacheInfo m_StandatdPatternCacheInfo;
		ContourPathObject m_ContourPathObject;
	}
}
