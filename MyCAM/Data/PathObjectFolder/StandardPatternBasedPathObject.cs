using MyCAM.CacheInfo;
using MyCAM.StandardPatternFactory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data.PathObjectFolder
{
	internal abstract class StandardPatternBasedPathObject : PathObject
	{
		public StandardPatternBasedPathObject( string szUID, TopoDS_Shape shape, IStandardPatternGeomData geomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			m_GeomData = geomData;
			m_CraftData = craftData;
			m_ContourPathObject = contourPathObject;

			InitializePathObject( szUID, geomData, craftData, contourPathObject );
		}

		public IGeomData GeomData
		{
			get
			{
				return m_GeomData;
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

		public IStandardPatternCacheInfo CacheInfo
		{
			get
			{
				return m_CacheInfo;
			}
		}

		public override void DoTransform( gp_Trsf transform )
		{
			// Step1:tranform shape first
			base.DoTransform( transform );

			// Step2:then transform CAD points because they depend on shape
			m_ContourPathObject.DoTransform( transform );

			// Step3:recalculate cache info because CAD points have changed
			m_CacheInfo.DoTransform( transform );
		}

		void InitializePathObject( string szUID, IStandardPatternGeomData standardPatternGeomData, CraftData craftData, ContourPathObject contourPathObject )
		{

			PatternFactory patternFactory = new PatternFactory( contourPathObject.ContourGeomData, standardPatternGeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();

			// Factory returns IStandardPatternCacheInfo interface
			m_CacheInfo = StandardPatternCacheInfoFactory.CreateCacheInfo( coordinateInfo, standardPatternGeomData, craftData );
		}

		IGeomData m_GeomData;
		CraftData m_CraftData;
		IStandardPatternCacheInfo m_CacheInfo;
		ContourPathObject m_ContourPathObject;
	}
}
