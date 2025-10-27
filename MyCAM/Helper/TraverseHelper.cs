using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	internal static class TraverseHelper
	{
		public static CAMPoint GetCutDownPoint( CAMPoint processPathStartPoint, double cutDownDistance )
		{
			if( processPathStartPoint == null ) {
				return null;
			}
			if( cutDownDistance == 0 ) {
				return processPathStartPoint;
			}

			// get cut down point
			CAMPoint startPoint = processPathStartPoint.Clone();
			gp_Vec toolVec = new gp_Vec( startPoint.ToolVec );
			toolVec.Normalize();
			gp_Vec scaledToolVec = toolVec.Scaled( cutDownDistance );
			gp_Pnt toolDownPnt = startPoint.CADPoint.Point.Translated( scaledToolVec );

			// create tool up/down point info
			CADPoint cutDownCADPoint = new CADPoint(
				toolDownPnt,
				startPoint.CADPoint.TangentVec,
				startPoint.CADPoint.NormalVec_2nd,
				startPoint.ToolVec
			);
			CAMPoint cutDownCAMPoint = new CAMPoint( cutDownCADPoint, startPoint.ToolVec );
			return cutDownCAMPoint;
		}

		public static CAMPoint GetLiftUpPoint( CAMPoint processPathEndPoint, double liftUpDistance )
		{
			if( processPathEndPoint == null ) {
				return null;
			}
			if( liftUpDistance == 0 ) {
				return processPathEndPoint;
			}
			CAMPoint startPoint = processPathEndPoint.Clone();
			gp_Vec toolVec = new gp_Vec( startPoint.ToolVec );
			toolVec.Normalize();
			gp_Vec scaledToolVec = toolVec.Scaled( liftUpDistance );
			gp_Pnt liftUpPnt = startPoint.CADPoint.Point.Translated( scaledToolVec );

			// create tool up/down point info
			CADPoint liftUpCADPoint = new CADPoint(
				liftUpPnt,
				startPoint.CADPoint.TangentVec,
				startPoint.CADPoint.NormalVec_2nd,
				startPoint.ToolVec
			);
			CAMPoint liftUpCAMPoint = new CAMPoint( liftUpCADPoint, startPoint.ToolVec );
			return liftUpCAMPoint;
		}

		public static CAMPoint GetFrogLeapMiddlePoint( CAMPoint frogLeapStartPoint, CAMPoint frogLeapEndPoint, double frogLeapDistance )
		{
			// get middle point of frog leap
			gp_Pnt moveMidP = new gp_Pnt(
				( frogLeapStartPoint.CADPoint.Point.X() + frogLeapEndPoint.CADPoint.Point.X() ) / 2.0,
				( frogLeapStartPoint.CADPoint.Point.Y() + frogLeapEndPoint.CADPoint.Point.Y() ) / 2.0,
				( frogLeapStartPoint.CADPoint.Point.Z() + frogLeapEndPoint.CADPoint.Point.Z() ) / 2.0
			);

			// get moving direction vector by average of tool vector
			gp_Dir arcMidDir = new gp_Dir(
				( frogLeapStartPoint.ToolVec.X() + frogLeapEndPoint.ToolVec.X() ) / 2.0,
				( frogLeapStartPoint.ToolVec.Y() + frogLeapEndPoint.ToolVec.Y() ) / 2.0,
				( frogLeapStartPoint.ToolVec.Z() + frogLeapEndPoint.ToolVec.Z() ) / 2.0
			);

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
