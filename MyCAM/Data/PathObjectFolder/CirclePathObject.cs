using MyCAM.CacheInfo;
using OCC.TopoDS;

namespace MyCAM.Data
{
	internal class CirclePathObject : StandardPatternBasedPathObject
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

		public override PathType PathType
		{
			get
			{
				return PathType.Circle;
			}
		}

		// provide strongly-typed access to GeomData
		public CircleGeomData CircleGeomData
		{
			get
			{
				return (CircleGeomData)GeomData;
			}
		}

		// provide strongly-typed access to CacheInfo
		public CircleCacheInfo CircleCacheInfo
		{
			get
			{
				return (CircleCacheInfo)base.CacheInfo;
			}
		}
	}
}
