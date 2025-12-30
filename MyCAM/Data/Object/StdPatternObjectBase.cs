using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal abstract class StdPatternObjectBase : PathObject
	{
		// TODO: this is not using so far
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

		public IStdPatternCache StdPatternCache
		{
			get
			{
				return m_StdPatternCache;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// Step1: transform shape first
			base.DoTransform( transform );

			// Step2: then transform geom data, currently do nothing since transform is not depends on geom data
			m_GeomData.DoTransform( transform );

			// Step3: recalculate cache
			m_StdPatternCache.DoTransform( transform );

			// Step4: transform the underlying contour path object
			m_ContourPathObject.DoTransform( transform );
		}

		void InitializeCache()
		{
			gp_Ax3 refCoord = StdPatternHelper.GetPatternRefCoord( m_ContourPathObject.GeomData.RefCenterDir, m_GeomData.IsCoordinateReversed, m_GeomData.RotatedAngle_deg );

			// factory automatically determines the correct Cache type based on GeomData type
			m_StdPatternCache = (StdPatternCacheBase)PathCacheFactory.CreateStdPatternCache( refCoord, m_GeomData, m_CraftData );
		}

		protected IStdPatternGeomData m_GeomData;
		protected IStdPatternCache m_StdPatternCache;
		protected ContourPathObject m_ContourPathObject;
	}
}
