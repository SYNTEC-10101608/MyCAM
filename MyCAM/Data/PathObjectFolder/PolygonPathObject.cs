using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class PolygonPathObject : StandardPatternBasedPathObject
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

		public override PathType PathType
		{
			get
			{
				// Polygon uses PathType from GeomData (Triangle, Square, Pentagon, Hexagon)
				return GeomData.PathType;
			}
		}

		// Provide strongly-typed access to GeomData
		public PolygonGeomData PolygonGeomData
		{
			get
			{
				return (PolygonGeomData)GeomData;
			}
		}

		// Provide strongly-typed access to CacheInfo
		public PolygonCacheInfo PolygonCacheInfo
		{
			get
			{
				return (PolygonCacheInfo)base.CacheInfo;
			}
		}
	}
}
