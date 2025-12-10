using MyCAM.CacheInfo;
using MyCAM.StandardPatternFactory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data
{
	/// <summary>
	/// Base class for all standard pattern path objects (Circle, Rectangle, Runway, Polygon)
	/// Provides common functionality and abstracts away repetitive code
	/// </summary>
	internal abstract class StandardPatternBasedPathObject : PathObject
	{
		/// <summary>
		/// Constructor for creating new standard pattern path object
		/// </summary>
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

		/// <summary>
		/// Constructor for reading from file
		/// </summary>
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

		/// <summary>
		/// Gets the geometry data (can be cast to specific type like CircleGeomData)
		/// </summary>
		public IStandardPatternGeomData GeomData
		{
			get
			{
				return m_GeomData;
			}
		}

		/// <summary>
		/// Gets the craft data
		/// </summary>
		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		/// <summary>
		/// Gets the contour path object
		/// </summary>
		public ContourPathObject ContourPathObject
		{
			get
			{
				return m_ContourPathObject;
			}
		}

		/// <summary>
		/// Gets the cache info (can be cast to specific type like CircleCacheInfo)
		/// </summary>
		public IStandardPatternCacheInfo CacheInfo
		{
			get
			{
				return m_CacheInfo;
			}
		}

		/// <summary>
		/// Applies transformation to the path object and its dependencies
		/// </summary>
		public override void DoTransform( gp_Trsf transform )
		{
			// Step1: transform shape first
			base.DoTransform( transform );

			// Step2: then transform CAD points because they depend on shape
			m_ContourPathObject.DoTransform( transform );

			// Step3: recalculate cache info because CAD points have changed
			m_CacheInfo.DoTransform( transform );
		}

		/// <summary>
		/// Initializes the cache info using the factory
		/// </summary>
		void InitializeCacheInfo()
		{
			PatternFactory patternFactory = new PatternFactory( m_ContourPathObject.ContourGeomData, m_GeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();

			// Factory automatically determines the correct CacheInfo type based on GeomData type
			m_CacheInfo = StandardPatternCacheInfoFactory.CreateCacheInfo( coordinateInfo, m_GeomData, m_CraftData );
		}

		IStandardPatternGeomData m_GeomData;
		CraftData m_CraftData;
		IStandardPatternCacheInfo m_CacheInfo;
		ContourPathObject m_ContourPathObject;
	}
}
