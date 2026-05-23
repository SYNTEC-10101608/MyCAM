namespace MyCAM.Data
{
	internal class RunwayPathObject : StdPatternObjectBase
	{
		internal RunwayPathObject( string szUID, RunwayGeomData runwayGeomData, ContourPathObject contourPathObject )
			: base( szUID, runwayGeomData, contourPathObject )
		{
		}

		// read file
		internal RunwayPathObject( string szUID, RunwayGeomData runwayGeomData, CraftData craftData, ContourPathObject contourPathObject )
			: base( szUID, runwayGeomData, craftData, contourPathObject )
		{
		}

		public override PathType PathType => PathType.Runway;
	}
}
