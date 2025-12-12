using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class PolygonPathObject : StdPatternObjectBase
	{
		internal PolygonPathObject( string szUID, TopoDS_Shape shape, PolygonGeomData polygonGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape, polygonGeomData, contourPathObject )
		{
		}

		// read file
		internal PolygonPathObject( string szUID, TopoDS_Shape shape, PolygonGeomData polygonGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape, polygonGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType => GeomData.PathType;
	}
}
