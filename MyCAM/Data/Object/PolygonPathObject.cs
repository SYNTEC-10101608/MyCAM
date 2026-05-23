namespace MyCAM.Data
{
	internal class PolygonPathObject : StdPatternObjectBase
	{
		internal PolygonPathObject( string szUID, PolygonGeomData polygonGeomData, ContourPathObject contourPathObject )
			: base( szUID, polygonGeomData, contourPathObject )
		{
		}

		// read file
		internal PolygonPathObject( string szUID, PolygonGeomData polygonGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, polygonGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType => m_GeomData.PathType;
	}
}
