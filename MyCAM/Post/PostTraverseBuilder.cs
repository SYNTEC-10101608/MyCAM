using MyCAM.Data;
using MyCAM.Helper;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal static class PostTraverseBuilder
	{
		public static bool CalculateTraverse( PathEndInfo endInfoOfPreviousPath, INCPackage currentPathNCPack, ITraversePostData pathG54PostData )
		{
			if( endInfoOfPreviousPath == null || currentPathNCPack == null ||
				pathG54PostData == null || pathG54PostData.ProcessStartPoint == null ) {
				return false;
			}

			IProcessPoint previousEndPoint = endInfoOfPreviousPath.EndCAMPoint;
			IProcessPoint currentStartPoint = currentPathNCPack.ProcessStartPoint;

			if( !TraverseHelper.TryCalculateTraversePoints( previousEndPoint, currentStartPoint, currentPathNCPack.TraverseData, out TraversePathResult result ) ) {
				return false;
			}

			if( currentPathNCPack.TraverseData.TraverseMode == TraverseData.TraverseType.SafeCylinder ||
				currentPathNCPack.TraverseData.TraverseMode == TraverseData.TraverseType.SafeSphere ) {
				var boundaryPoints = result.SafeBoundaryPointList;
				int entryIndex = result.SafeBoundaryEntryIndex;
				int exitIndex = result.SafeBoundaryExitIndex;
				if( boundaryPoints == null || entryIndex < 0 || exitIndex <= entryIndex || exitIndex >= boundaryPoints.Count ) {
					return false;
				}
				double[] cumulativeLengths = new double[ boundaryPoints.Count ];
				for( int index = entryIndex + 1; index <= exitIndex; index++ ) {
					cumulativeLengths[ index ] = cumulativeLengths[ index - 1 ] +
						boundaryPoints[ index - 1 ].Distance( boundaryPoints[ index ] );
				}
				double totalLength = cumulativeLengths[ exitIndex ];
				if( double.IsNaN( totalLength ) || double.IsInfinity( totalLength ) ) {
					return false;
				}
				var boundaryPostPoints = new List<PostPoint>( boundaryPoints.Count );
				for( int index = 0; index < boundaryPoints.Count; index++ ) {
					// Hold the old/new angles on the approach/departure; rotate only on the surface.
					// Coincident surface endpoints still carry distinct angles for rotation in place.
					double ratio = index <= entryIndex ? 0 : index >= exitIndex ? 1 :
						( totalLength > 0 ? cumulativeLengths[ index ] / totalLength :
							(double)( index - entryIndex ) / ( exitIndex - entryIndex ) );
					boundaryPostPoints.Add( new PostPoint()
					{
						X = boundaryPoints[ index ].X(),
						Y = boundaryPoints[ index ].Y(),
						Z = boundaryPoints[ index ].Z(),
						Master = endInfoOfPreviousPath.Master + ratio *
							( pathG54PostData.ProcessStartPoint.Master - endInfoOfPreviousPath.Master ),
						Slave = endInfoOfPreviousPath.Slave + ratio *
							( pathG54PostData.ProcessStartPoint.Slave - endInfoOfPreviousPath.Slave )
					} );
				}
				pathG54PostData.SafeBoundaryPostPointList = boundaryPostPoints;
			}

			// lift up
			if( currentPathNCPack.TraverseData.LiftUpDistance > 0 && result.LiftUpPoint != null ) {
				pathG54PostData.LiftUpPostPoint = new PostPoint()
				{
					X = result.LiftUpPoint.Point.X(),
					Y = result.LiftUpPoint.Point.Y(),
					Z = result.LiftUpPoint.Point.Z(),
					Master = endInfoOfPreviousPath.Master,
					Slave = endInfoOfPreviousPath.Slave
				};
			}

			// frog leap middle point
			if( result.FrogLeapMiddlePoint != null ) {
				pathG54PostData.FrogLeapMidPostPoint = new PostPoint()
				{
					X = result.FrogLeapMiddlePoint.Point.X(),
					Y = result.FrogLeapMiddlePoint.Point.Y(),
					Z = result.FrogLeapMiddlePoint.Point.Z(),
					Master = ( endInfoOfPreviousPath.Master + pathG54PostData.ProcessStartPoint.Master ) / 2.0,
					Slave = ( endInfoOfPreviousPath.Slave + pathG54PostData.ProcessStartPoint.Slave ) / 2.0
				};
			}

			// safe plane projections
			if( result.SafePlaneLiftUpProjPoint != null ) {
				pathG54PostData.LiftUpPostSafePlanePoint = new PostPoint()
				{
					X = result.SafePlaneLiftUpProjPoint.X(),
					Y = result.SafePlaneLiftUpProjPoint.Y(),
					Z = result.SafePlaneLiftUpProjPoint.Z(),
					Master = endInfoOfPreviousPath.Master,
					Slave = endInfoOfPreviousPath.Slave
				};
			}
			if( result.SafePlaneCutDownProjPoint != null ) {
				pathG54PostData.CutDownPostSafePlanePoint = new PostPoint()
				{
					X = result.SafePlaneCutDownProjPoint.X(),
					Y = result.SafePlaneCutDownProjPoint.Y(),
					Z = result.SafePlaneCutDownProjPoint.Z(),
					Master = pathG54PostData.ProcessStartPoint.Master,
					Slave = pathG54PostData.ProcessStartPoint.Slave
				};
			}

			// cut down
			if( currentPathNCPack.TraverseData.CutDownDistance > 0 && result.CutDownPoint != null ) {
				pathG54PostData.CutDownPostPoint = new PostPoint()
				{
					X = result.CutDownPoint.Point.X(),
					Y = result.CutDownPoint.Point.Y(),
					Z = result.CutDownPoint.Point.Z(),
					Master = pathG54PostData.ProcessStartPoint.Master,
					Slave = pathG54PostData.ProcessStartPoint.Slave
				};
			}
			pathG54PostData.FollowSafeDistance = currentPathNCPack.TraverseData.FollowSafeDistance;
			return true;
		}

		public static void CalculateEntry( INCPackage currentPathNCPack, EntryAndExitData entryAndExitData, ITraversePostData pathG54PostData )
		{
			if( currentPathNCPack == null || entryAndExitData == null ) {
				return;
			}
			if( entryAndExitData.EntryDistance <= 0 ) {

				// just set follow safe distance
				pathG54PostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
				return;
			}
			IProcessPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( currentPathNCPack.ProcessStartPoint, entryAndExitData.EntryDistance );
			if( entryPoint == null ) {
				return;
			}

			// G54
			pathG54PostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.Point.X(),
				Y = entryPoint.Point.Y(),
				Z = entryPoint.Point.Z(),
				Master = pathG54PostData.ProcessStartPoint.Master,
				Slave = pathG54PostData.ProcessStartPoint.Slave
			};
			pathG54PostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
		}

		public static void CalculateExit( PathEndInfo endInfoOfLastPath, EntryAndExitData entryAndExitData, out PostPoint G54ExitPoint )
		{
			G54ExitPoint = null;
			if( entryAndExitData.ExitDistance <= 0 ) {
				return;
			}
			IProcessPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfLastPath.EndCAMPoint, entryAndExitData.ExitDistance );
			if( exitPoint == null ) {
				return;
			}

			// G54
			G54ExitPoint = new PostPoint()
			{
				X = exitPoint.Point.X(),
				Y = exitPoint.Point.Y(),
				Z = exitPoint.Point.Z(),
				Master = endInfoOfLastPath.Master,
				Slave = endInfoOfLastPath.Slave
			};
		}
	}
}
