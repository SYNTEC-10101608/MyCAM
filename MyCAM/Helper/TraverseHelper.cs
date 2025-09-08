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

		public static CAMPoint GetFollowSafePoint( CAMPoint processPathStartPoint, double followSafeDistance )
		{
			if( processPathStartPoint == null ) {
				return null;
			}
			if( followSafeDistance == 0 ) {
				return processPathStartPoint;
			}
			CAMPoint startCAMPoint = processPathStartPoint.Clone();
			gp_Vec normalizedToolVec = new gp_Vec( startCAMPoint.ToolVec );
			normalizedToolVec.Normalize();
			gp_Vec scaledToolVec = normalizedToolVec.Scaled( followSafeDistance );
			gp_Pnt safePnt = startCAMPoint.CADPoint.Point.Translated( scaledToolVec );

			// create safe point info
			CADPoint safeCAMPoint = new CADPoint(
				safePnt,
				startCAMPoint.CADPoint.TangentVec,
				startCAMPoint.CADPoint.NormalVec_2nd,
				startCAMPoint.ToolVec
			);
			CAMPoint followSafeCAMPoint = new CAMPoint( safeCAMPoint, startCAMPoint.ToolVec );
			return followSafeCAMPoint;
		}
	}
}
