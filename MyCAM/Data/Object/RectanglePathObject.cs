using MyCAM.PathCache;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class RectanglePathObject : StdPatternObjectBase
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

		public override PathType PathType => PathType.Rectangle;
	}
}
