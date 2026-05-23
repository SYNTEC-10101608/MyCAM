namespace MyCAM.Data
{
	internal class RectanglePathObject : StdPatternObjectBase
	{
		internal RectanglePathObject( string szUID, RectangleGeomData rectangleGeomData, ContourPathObject contourPathObject )
			: base( szUID, rectangleGeomData, contourPathObject )
		{
		}

		// read file
		internal RectanglePathObject( string szUID, RectangleGeomData rectangleGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, rectangleGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType => PathType.Rectangle;
	}
}
