namespace MyCAM.Data
{
	internal class CirclePathObject : StdPatternObjectBase
	{
		internal CirclePathObject( string szUID, CircleGeomData circleGeomData, ContourPathObject contourPathObject )
			: base( szUID, circleGeomData, contourPathObject )
		{
		}

		// read file
		internal CirclePathObject( string szUID, CircleGeomData circleGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, circleGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType => PathType.Circle;
	}
}
