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
			if( frogLeapStartPoint == null || frogLeapEndPoint == null ) {
				return null;
			}

			// get middle point of traverse
			gp_Pnt moveMidP = new gp_Pnt(
				( frogLeapStartPoint.CADPoint.Point.X() + frogLeapEndPoint.CADPoint.Point.X() ) / 2.0,
				( frogLeapStartPoint.CADPoint.Point.Y() + frogLeapEndPoint.CADPoint.Point.Y() ) / 2.0,
				( frogLeapStartPoint.CADPoint.Point.Z() + frogLeapEndPoint.CADPoint.Point.Z() ) / 2.0
			);

			// if frog leap distance is zero, return middle point between start and end point
			if( frogLeapDistance == 0 ) {
				CADPoint centerCAMPoint = new CADPoint(
					moveMidP,
					new gp_Dir(), // doesn't matter
					new gp_Dir(), // doesn't matter
					new gp_Dir() // doesn't matter
				);
				return new CAMPoint( centerCAMPoint, new gp_Dir() ); // tool vector doesn't matter
			}

			// if tool vectors are parallel, use Z axis as moving direction
			gp_Dir arcMidDir;
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
