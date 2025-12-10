using MyCAM.CacheInfo;
using MyCAM.StandardPatternFactory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class CirclePathObject : PathObject
	{
		internal CirclePathObject( string szUID, TopoDS_Shape shape, CircleGeomData circleGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || circleGeomData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "CirclePathObject constructing argument null" );
			}

			CraftData craftData = new CraftData();
			InitializeCirclePathObject( szUID, circleGeomData, craftData, contourPathObject );
		}

		// read file
		internal CirclePathObject( string szUID, TopoDS_Shape shape, CircleGeomData circleGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || circleGeomData == null || craftData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "CirclePathObject constructing argument null" );
			}

			InitializeCirclePathObject( szUID, circleGeomData, craftData, contourPathObject );
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Circle;
			}
		}

		public CircleGeomData CircleGeomData
		{
			get
			{
				return m_CircleGeomData;
			}
		}

		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public ContourPathObject ContourPathObject
		{
			get
			{
				return m_ContourPathObject;
			}
		}

		public CircleCacheInfo CircleCacheInfo
		{
			get
			{
				return m_CircleCacheInfo;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// Step1:tranform shape first
			base.DoTransform( transform );

			// Step2:then transform CAD points because they depend on shape
			m_ContourPathObject.DoTransform( transform );

			// Step3:recalculate cache info because CAD points have changed
			m_CircleCacheInfo.DoTransform( transform );
		}

		void InitializeCirclePathObject( string szUID, CircleGeomData circleGeomData, CraftData craftData, ContourPathObject contourPathObject )
		{
			m_CircleGeomData = circleGeomData;
			m_CraftData = craftData;
			m_ContourPathObject = contourPathObject;

			PatternFactory patternFactory = new PatternFactory( contourPathObject.ContourGeomData, circleGeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();
			m_CircleCacheInfo = (CircleCacheInfo)StandardPatternCacheInfoFactory.CreateCacheInfo( coordinateInfo, circleGeomData, craftData );
		}

		CircleGeomData m_CircleGeomData;
		CraftData m_CraftData;
		CircleCacheInfo m_CircleCacheInfo;
		ContourPathObject m_ContourPathObject;
	}
}
