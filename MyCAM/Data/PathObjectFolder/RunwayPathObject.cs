using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class RunwayPathObject : StdPatternObjectBase
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

		public override PathType PathType => PathType.Runway;
	}
}
