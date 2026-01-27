using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Helper
{
	public struct TraversePathResult
	{
		public IProcessPoint PreviousPathEnd;
		public IProcessPoint LiftUpPoint;
		public IProcessPoint FrogLeapMiddlePoint;
		public IProcessPoint CutDownPoint;
		public IProcessPoint CurrentPathStart;
		public gp_Pnt SafePlaneLiftUpProjPoint;
		public gp_Pnt SafePlaneCutDownProjPoint;
	}

	public static class TraverseHelper
	{
		public static IProcessPoint GetCutDownOrLiftUpPoint( IProcessPoint pathPoint, double dDistance )
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
			gp_Pnt resultPoint = pathPoint.Point.Translated( scaledToolVec );

			// create tool up/down point info
			return BuildTraversePoint( resultPoint, pathPoint.ToolVec );
		}

		public static IProcessPoint GetFrogLeapMiddlePoint( IProcessPoint frogLeapStartPoint, IProcessPoint frogLeapEndPoint, double frogLeapDistance )
		{
			if( frogLeapStartPoint == null || frogLeapEndPoint == null ) {
				return null;
			}

			// get middle point of traverse
			gp_Pnt moveMidP = new gp_Pnt(
				( frogLeapStartPoint.Point.X() + frogLeapEndPoint.Point.X() ) / 2.0,
				( frogLeapStartPoint.Point.Y() + frogLeapEndPoint.Point.Y() ) / 2.0,
				( frogLeapStartPoint.Point.Z() + frogLeapEndPoint.Point.Z() ) / 2.0
			);

			// if frog leap distance is zero, return middle point between start and end point
			if( frogLeapDistance == 0 ) {
				return BuildTraversePoint( moveMidP, new gp_Dir() ); // tool vector doesn't matter
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
			return BuildTraversePoint( frogLeapMidP, arcMidDir );
		}

		public static gp_Pnt ProjectPointToPlane( gp_Pnt point, gp_Pln plane )
		{
			gp_Pnt planeLocation = plane.Location();
			double distance = planeLocation.Z() - point.Z();
			gp_Pnt projectedPoint = new gp_Pnt( point.X(), point.Y(), point.Z() + distance );
			return projectedPoint;
		}

		public static bool TryCalculateTraversePoints( IProcessPoint previousPathEnd, IProcessPoint currentPathStart, TraverseData traverseData, out TraversePathResult result )
		{
			result = new TraversePathResult();

			if( previousPathEnd == null || currentPathStart == null ) {
				return false;
			}

			// end of previous path
			result.PreviousPathEnd = previousPathEnd;

			// lift up point of previous path
			result.LiftUpPoint = GetCutDownOrLiftUpPoint( previousPathEnd, traverseData.LiftUpDistance );

			// start of current path
			result.CurrentPathStart = currentPathStart;

			// cut down point of current path
			result.CutDownPoint = GetCutDownOrLiftUpPoint( currentPathStart, traverseData.CutDownDistance );

			if( result.LiftUpPoint == null || result.CutDownPoint == null ) {
				return false;
			}

			// frog leap middle point (if frog leap mode)
			if( !traverseData.IsSafePlaneEnable && traverseData.FrogLeapDistance > 0 ) {
				result.FrogLeapMiddlePoint = GetFrogLeapMiddlePoint(
					result.LiftUpPoint,
					result.CutDownPoint,
					traverseData.FrogLeapDistance
				);
			}

			// projected points on safe plane (if safe plane mode)
			if( traverseData.IsSafePlaneEnable ) {
				double safePlaneZ = (double)traverseData.SafePlaneDistance;
				gp_Pln safePlane = CreateSafePlane( safePlaneZ );

				result.SafePlaneLiftUpProjPoint = ProjectPointToPlane( result.LiftUpPoint.Point, safePlane );
				result.SafePlaneCutDownProjPoint = ProjectPointToPlane( result.CutDownPoint.Point, safePlane );
			}

			return true;
		}

		// this method is now for building a CAMPoint as ITraversePoint
		static IProcessPoint BuildTraversePoint( gp_Pnt point, gp_Dir toolVec )
		{
			CADPoint cadPoint = new CADPoint(
				point,
				new gp_Dir(), // doesn't matter
				new gp_Dir(), // doesn't matter
				new gp_Dir() // doesn't matter
			);
			return new CAMPoint( cadPoint, toolVec );
		}

		static gp_Pln CreateSafePlane( double safePlaneZ )
		{
			gp_Pnt safePlaneOrigin = new gp_Pnt( 0, 0, safePlaneZ );
			gp_Dir safePlaneNormal = new gp_Dir( 0, 0, 1 );
			return new gp_Pln( safePlaneOrigin, safePlaneNormal );
		}
	}
}
