using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class CirclePathObject : StdPatternObjectBase
	{
		internal CirclePathObject( string szUID, TopoDS_Shape shape, CircleGeomData circleGeomData, ContourPathObject contourPathObject )
			: base( szUID, shape, circleGeomData, contourPathObject )
		{
		}

		// read file
		internal CirclePathObject( string szUID, TopoDS_Shape shape, CircleGeomData circleGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, shape, circleGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType => PathType.Circle;
	}
}
