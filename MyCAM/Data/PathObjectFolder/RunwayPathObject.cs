using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class RunwayPathObject : StandardPatternBasedPathObject
	{
		internal RunwayPathObject( string szUID, TopoDS_Shape shape, RunwayGeomData runwayGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape, runwayGeomData, contourPathObject )
		{
		}

		// read file
		internal RunwayPathObject( string szUID, TopoDS_Shape shape, RunwayGeomData runwayGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape, runwayGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Runway;
			}
		}

		// Provide strongly-typed access to GeomData
		public RunwayGeomData RunwayGeomData
		{
			get
			{
				return (RunwayGeomData)GeomData;
			}
		}

		// Provide strongly-typed access to CacheInfo
		public RunwayCacheInfo RunwayCacheInfo
		{
			get
			{
				return (RunwayCacheInfo)base.CacheInfo;
			}
		}
	}
}
