using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class RectanglePathObject : StandardPatternBasedPathObject
	{
		internal RectanglePathObject( string szUID, TopoDS_Shape shape, RectangleGeomData rectangleGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape, rectangleGeomData, contourPathObject )
		{
		}

		// read file
		internal RectanglePathObject( string szUID, TopoDS_Shape shape, RectangleGeomData rectangleGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape, rectangleGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType
		{
			get
			{
				return PathType.Rectangle;
			}
		}

		// Provide strongly-typed access to GeomData
		public RectangleGeomData RectangleGeomData
		{
			get
			{
				return (RectangleGeomData)GeomData;
			}
		}

		// Provide strongly-typed access to CacheInfo
		public RectangleCacheInfo RectangleCacheInfo
		{
			get
			{
				return (RectangleCacheInfo)base.CacheInfo;
			}
		}
	}
}
