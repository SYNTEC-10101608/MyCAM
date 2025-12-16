using MyCAM.Data;
using OCC.gp;
using OCC.TopoDS;
using System;

namespace MyCAM.Helper
{
	public static class StdPatternHelper
	{
		public static gp_Ax3 GetPatternRefCoord( gp_Ax1 refCenterDir, double rotationAngle_deg )
		{
			gp_Pnt centerPoint = refCenterDir.Location();
			gp_Dir normalDir = refCenterDir.Direction();
			gp_Dir xDir;

			// build local coordination system ( reference DOC:https://syntecclub.atlassian.net/wiki/spaces/AUTO/pages/89458700 )
			// the target is the same as TOOL_DIR
			if( normalDir.IsParallel( TOOL_DIR, DIR_TOLERANCE ) && !normalDir.IsOpposite( TOOL_DIR, DIR_TOLERANCE ) ) {
				xDir = new gp_Dir( 1, 0, 0 );
			}

			// the target is opposite to TOOL_DIR
			else if( normalDir.IsParallel( TOOL_DIR, DIR_TOLERANCE ) && normalDir.IsOpposite( TOOL_DIR, DIR_TOLERANCE ) ) {
				xDir = new gp_Dir( -1, 0, 0 );
			}

			// general case
			else {
				xDir = TOOL_DIR.Crossed( normalDir );
			}

			// create coordination system considering rotation
			gp_Ax3 coordSystem = new gp_Ax3( centerPoint, normalDir, xDir );
			if( Math.Abs( rotationAngle_deg ) > ANGLE_TOLERANCE_DEG ) {
				double rotationAngleInRadians = rotationAngle_deg * Math.PI / 180.0;
				gp_Ax1 rotationAxis = new gp_Ax1( centerPoint, normalDir );
				coordSystem.Rotate( rotationAxis, rotationAngleInRadians );
			}
			return coordSystem;
		}

		public static TopoDS_Shape GetPathWire( gp_Ax1 refCenterDir, IStdPatternGeomData StdPatternGeomData )
		{
			// get pattern reference coordination system
			gp_Ax3 patternRefCoord = GetPatternRefCoord( refCenterDir, StdPatternGeomData.RotatedAngle_deg );

			// get path wire
			return GetPathWire( patternRefCoord, StdPatternGeomData );
		}

		public static TopoDS_Shape GetPathWire( gp_Ax3 patternRefCoord, IStdPatternGeomData StdPatternGeomData )
		{
			TopoDS_Wire wire = null;
			StdPatternWireFactory.GetStrategy( StdPatternGeomData.PathType )
				?.CreateWire( patternRefCoord, StdPatternGeomData, out wire );
			return wire;
		}

		const double DIR_TOLERANCE = 0.001;
		const double ANGLE_TOLERANCE_DEG = 0.001;
		static readonly gp_Dir TOOL_DIR = new gp_Dir( 0, 0, 1 );
	}
}
