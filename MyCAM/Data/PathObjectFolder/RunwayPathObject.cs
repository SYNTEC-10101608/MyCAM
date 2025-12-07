using MyCAM.CacheInfo;
using MyCAM.Data.GeomDataFolder;
using MyCAM.Editor.Factory;
using OCC.gp;
using OCC.TopoDS;

namespace MyCAM.Data.PathObjectFolder
{
	internal class RunwayPathObject : PathObject
	{
		internal RunwayPathObject( string szUID, TopoDS_Shape shape, RunwayGeomData runwayGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || runwayGeomData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "RunwayPathObject constructing argument null" );
			}

			CraftData craftData = new CraftData( szUID );
			InitializeRunwayPathObject( szUID, runwayGeomData, craftData, contourPathObject );
		}

		// read file
		internal RunwayPathObject( string szUID, TopoDS_Shape shape, RunwayGeomData runwayGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() || runwayGeomData == null || craftData == null || contourPathObject == null ) {
				throw new System.ArgumentNullException( "RunwayPathObject constructing argument null" );
			}

			InitializeRunwayPathObject( szUID, runwayGeomData, craftData, contourPathObject );
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Runway;
			}
		}

		public RunwayGeomData RunwayGeomData
		{
			get
			{
				return m_RunwayGeomData;
			}
		}

		public override CraftData CraftData
		{
			get
			{
				return m_CraftData;
			}
		}

		public RunwayCacheInfo RunwayCacheInfo
		{
			get
			{
				return m_RunwayCacheInfo;
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
			ContourPathObject.DoTransform( transform );

			// Step3: recalculate cache info because CAD points have changed
			m_RunwayCacheInfo.DoTransform( transform );
		}

		void InitializeRunwayPathObject( string szUID, RunwayGeomData runwayGeomData, CraftData craftData, ContourPathObject contourPathObject )
		{
			m_RunwayGeomData = runwayGeomData;
			m_CraftData = craftData;
			m_ContourPathObject = contourPathObject;

			PatternFactory patternFactory = new PatternFactory( contourPathObject.ContourGeomData.CADPointList, runwayGeomData );
			gp_Ax3 coordinateInfo = patternFactory.GetCoordinateInfo();
			m_RunwayCacheInfo = new RunwayCacheInfo( szUID, coordinateInfo, runwayGeomData, m_CraftData );
		}

		CraftData m_CraftData;
		RunwayCacheInfo m_RunwayCacheInfo;
		RunwayGeomData m_RunwayGeomData;
		ContourPathObject m_ContourPathObject;
	}
}
