using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class StdPatternNCPackage
	{
		public StdPatternNCPackage( IProcessPoint refPoint, IProcessPoint startPoint, TraverseData traverseData, IProcessPoint processStartPoint,
			IProcessPoint processEndPoint, LeadData leadData, IReadOnlyList<IProcessPoint> leadInCAMPointList )
		{
			RefPoint = refPoint ?? throw new ArgumentNullException( nameof( refPoint ) );
			StartPoint = startPoint ?? throw new ArgumentNullException( nameof( startPoint ) );
			TraverseData = traverseData ?? throw new ArgumentNullException( nameof( traverseData ) );
			ProcessStartPoint = processStartPoint;
			ProcessEndPoint = processEndPoint;
			LeadData = leadData ?? throw new ArgumentNullException( nameof( leadData ) );
			LeadInCAMPointList = leadInCAMPointList ?? throw new ArgumentNullException( nameof( leadInCAMPointList ) );
		}

		public IProcessPoint RefPoint
		{
			get; private set;
		}

		public IProcessPoint StartPoint
		{
			get; private set;
		}

		public TraverseData TraverseData
		{
			get; private set;
		}

		public IProcessPoint ProcessStartPoint
		{
			get; private set;
		}

		public IProcessPoint ProcessEndPoint
		{
			get; private set;
		}

		public LeadData LeadData
		{
			get; private set;
		}

		public IReadOnlyList<IProcessPoint> LeadInCAMPointList
		{
			get; private set;
		}
	}

	internal static class StdPatternPostHelper
	{
		public static bool SolvePath( PostSolver postSolver, StdPatternNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out StdPatternPostData pathG54PostData, out PathEndInfo currentPathtEndInfo )
		{
			// for write NC file
			pathG54PostData = new StdPatternPostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( postSolver == null || currentPathNCPack == null || entryAndExitData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath?.Master ?? 0;
			double dLastPointProcess_S = endInfoOfPreviousPath?.Slave ?? 0;

			// main path
			if( !SolveProcessPath( postSolver, currentPathNCPack.RefPoint,
				out PostPoint mainG54,
				ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
				return false;
			}
			pathG54PostData.RefPoint = mainG54;
			pathG54PostData.ProcessStartPoint = new PostPoint()
			{
				X = currentPathNCPack.StartPoint.Point.X(),
				Y = currentPathNCPack.StartPoint.Point.Y(),
				Z = currentPathNCPack.StartPoint.Point.Z(),
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};

			// lead-in
			if( LeadHelper.HasLeadIn( currentPathNCPack.LeadData ) && currentPathNCPack.LeadInCAMPointList.Count > 0 ) {

				// update process start point
				pathG54PostData.ProcessStartPoint = new PostPoint()
				{
					X = currentPathNCPack.LeadInCAMPointList[ 0 ].Point.X(),
					Y = currentPathNCPack.LeadInCAMPointList[ 0 ].Point.Y(),
					Z = currentPathNCPack.LeadInCAMPointList[ 0 ].Point.Z(),
					Master = dLastPointProcess_M,
					Slave = dLastPointProcess_S
				};
			}

			if( endInfoOfPreviousPath == null ) {

				// the traverse of first path is entry
				CalculateEntry( currentPathNCPack, entryAndExitData, ref pathG54PostData );
			}

			// traverse from previous path to current path
			else {
				CalculateTraverse( endInfoOfPreviousPath, currentPathNCPack, ref pathG54PostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = currentPathNCPack.ProcessEndPoint,
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};
			return true;
		}

		static bool SolveProcessPath( PostSolver postSolver, IProcessPoint point,
			out PostPoint resultG54, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new PostPoint();
			if( point == null ) {
				return false;
			}

			// solve IK
			Tuple<double, double> rotateAngleList = new Tuple<double, double>( 0, 0 );
			bool singularTagList = false;
			CAMPoint camPoint = point as CAMPoint;
			IKSolveResult ikResult = postSolver.SolveIK( camPoint, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
			if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution ) {
				return false;
			}
			rotateAngleList = new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS );
			if( ikResult == IKSolveResult.NoError ) {
				singularTagList = false;
			}
			else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
				singularTagList = true;
			}

			double masterAngle = rotateAngleList.Item1;
			double slaveAngle = rotateAngleList.Item2;

			// in singularity, set angles to zero
			if( singularTagList ) {
				slaveAngle = 0;
				masterAngle = 0;
			}

			// solve FK
			gp_Pnt pointG54 = camPoint.Point;
			PostPoint frameDataG54 = new PostPoint()
			{
				X = pointG54.X(),
				Y = pointG54.Y(),
				Z = pointG54.Z(),
				Master = masterAngle,
				Slave = slaveAngle
			};
			resultG54 = frameDataG54;
			return true;
		}

		static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, StdPatternNCPackage currentPathNCPack, ref StdPatternPostData pathG54PostData )
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

		static void CalculateEntry( StdPatternNCPackage currentPathNCPack, EntryAndExitData entryAndExitData, ref StdPatternPostData pathG54PostData )
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
