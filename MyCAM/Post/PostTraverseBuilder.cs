using MyCAM.Data;
using MyCAM.Helper;

namespace MyCAM.Post
{
	internal static class PostTraverseBuilder
	{
		public static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, INCPackage currentPathNCPack, ITraversePostData pathG54PostData )
		{
			if( endInfoOfPreviousPath == null || currentPathNCPack == null ) {
				return;
			}

			// p1: end of previous path (not used here)
			// p2: lift up point of previous path
			// p3: frog leap middle point (if frog leap)
			// p4: cut down point of current path
			// p5: start of current path (not used here)
			IProcessPoint p1 = endInfoOfPreviousPath.EndCAMPoint;
			IProcessPoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfPreviousPath.EndCAMPoint, currentPathNCPack.TraverseData.LiftUpDistance );
			IProcessPoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentPathNCPack.ProcessStartPoint, currentPathNCPack.TraverseData.CutDownDistance );
			IProcessPoint p5 = currentPathNCPack.ProcessStartPoint;

			// lift up
			if( currentPathNCPack.TraverseData.LiftUpDistance > 0 && p2 != null ) {

				// G54
				pathG54PostData.LiftUpPostPoint = new PostPoint()
				{
					X = p2.Point.X(),
					Y = p2.Point.Y(),
					Z = p2.Point.Z(),
					Master = endInfoOfPreviousPath.Master,
					Slave = endInfoOfPreviousPath.Slave
				};
			}

			// frog leap
			if( currentPathNCPack.TraverseData.FrogLeapDistance > 0 && p2 != null && p4 != null ) {
				IProcessPoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, currentPathNCPack.TraverseData.FrogLeapDistance );

				if( p3 != null ) {
					// G54 middle point
					pathG54PostData.FrogLeapMidPostPoint = new PostPoint()
					{
						X = p3.Point.X(),
						Y = p3.Point.Y(),
						Z = p3.Point.Z(),
						Master = ( endInfoOfPreviousPath.Master + pathG54PostData.ProcessStartPoint.Master ) / 2.0,
						Slave = ( endInfoOfPreviousPath.Slave + pathG54PostData.ProcessStartPoint.Slave ) / 2.0
					};
				}
			}

			// cut down
			if( currentPathNCPack.TraverseData.CutDownDistance > 0 && p4 != null ) {

				// G54
				pathG54PostData.CutDownPostPoint = new PostPoint()
				{
					X = p4.Point.X(),
					Y = p4.Point.Y(),
					Z = p4.Point.Z(),
					Master = pathG54PostData.ProcessStartPoint.Master,
					Slave = pathG54PostData.ProcessStartPoint.Slave
				};
			}
			pathG54PostData.FollowSafeDistance = currentPathNCPack.TraverseData.FollowSafeDistance;
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
	}
}
