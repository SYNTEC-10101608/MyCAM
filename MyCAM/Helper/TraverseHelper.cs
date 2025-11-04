using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class TraverseHelper
	{
		public static CAMPoint GetCutDownOrLiftUpPoint( CAMPoint pathPoint, double dDistance )
		{
			if( pathPoint == null ) {
				return null;
			}
			if( dDistance == 0 ) {
				return pathPoint.Clone();
			}

			// get cut down point
			gp_Vec toolVec = new gp_Vec( pathPoint.ToolVec );
			toolVec.Normalize();
			gp_Vec scaledToolVec = toolVec.Scaled( dDistance );
			gp_Pnt resultPoint = pathPoint.CADPoint.Point.Translated( scaledToolVec );

			// create tool up/down point info
			CADPoint resultCAMPoint = new CADPoint(
				resultPoint,
				pathPoint.CADPoint.NormalVec_1st,
				pathPoint.CADPoint.NormalVec_2nd,
				pathPoint.CADPoint.TangentVec
			);
			return new CAMPoint( resultCAMPoint, pathPoint.ToolVec );
		}

		public static CAMPoint GetFrogLeapMiddlePoint( CAMPoint frogLeapStartPoint, CAMPoint frogLeapEndPoint, double frogLeapDistance )
		{
			// get middle point of frog leap
			gp_Pnt moveMidP = new gp_Pnt(
				( frogLeapStartPoint.CADPoint.Point.X() + frogLeapEndPoint.CADPoint.Point.X() ) / 2.0,
				( frogLeapStartPoint.CADPoint.Point.Y() + frogLeapEndPoint.CADPoint.Point.Y() ) / 2.0,
				( frogLeapStartPoint.CADPoint.Point.Z() + frogLeapEndPoint.CADPoint.Point.Z() ) / 2.0
			);
			gp_Dir arcMidDir;

			// if tool vectors are parallel, use Z axis as moving direction
			if( frogLeapStartPoint.ToolVec.IsOpposite( frogLeapEndPoint.ToolVec, 1e-3 ) ) {
				arcMidDir = new gp_Dir( 0, 0, 1 );
			}

			// get moving direction vector by average of tool vector
			else {
				arcMidDir = new gp_Dir(
				( frogLeapStartPoint.ToolVec.X() + frogLeapEndPoint.ToolVec.X() ) / 2.0,
				( frogLeapStartPoint.ToolVec.Y() + frogLeapEndPoint.ToolVec.Y() ) / 2.0,
				( frogLeapStartPoint.ToolVec.Z() + frogLeapEndPoint.ToolVec.Z() ) / 2.0
			);
			}

			// get frog leap middle point
			gp_Vec moveToArcMidVec = new gp_Vec( arcMidDir );
			moveToArcMidVec.Normalize();
			gp_Vec scaledMoveToArcMidVec = moveToArcMidVec.Scaled( frogLeapDistance );
			gp_Pnt frogLeapMidP = moveMidP.Translated( scaledMoveToArcMidVec );

			// create frog leap middle point info
			CAMPoint frogLeapMiddlePoint = new CAMPoint(
				new CADPoint(
					frogLeapMidP,
					new gp_Dir(), // doesn't matter
					new gp_Dir(), // doesn't matter
					new gp_Dir() // doesn't matter
				),
				arcMidDir
			);
			return frogLeapMiddlePoint;
		}
	}
}
