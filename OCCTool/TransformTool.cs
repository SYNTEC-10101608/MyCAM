using OCC.gp;

namespace OCCTool
{
	public class TransformTool
	{
		public static gp_Trsf GetCoordTrsf( double dX, double dY, double dZ, double dXR, double dYR, double dZR )
		{
			gp_Trsf trsfT = new gp_Trsf();
			trsfT.SetTranslation( new gp_Vec( dX, dY, dZ ) );
			gp_Trsf trsXR = new gp_Trsf();
			trsXR.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 1, 0, 0 ) ), dXR );
			gp_Trsf trsYR = new gp_Trsf();
			trsYR.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 1, 0 ) ), dYR );
			gp_Trsf trsZR = new gp_Trsf();
			trsZR.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) ), dZR );
			gp_Trsf trsf = trsXR.Multiplied( trsYR.Multiplied( trsZR ) ).Multiplied( trsfT );
			return trsf;
		}
	}
}
