using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class StdPatternNCPackage : INCPackage
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
			if( currentPathNCPack.LeadInCAMPointList.Count > 0 ) {

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
				PostTraverseBuilder.CalculateEntry( currentPathNCPack, entryAndExitData, pathG54PostData );
			}
			else {

				// traverse from previous path to current path
				PostTraverseBuilder.CalculateTraverse( endInfoOfPreviousPath, currentPathNCPack, pathG54PostData );
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
			CAMPoint camPoint = point as CAMPoint;
			IKSolveResult ikResult = postSolver.SolveIK( camPoint, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
			if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution ) {
				return false;
			}
			else if( ikResult == IKSolveResult.MasterInfinityOfSolution ) {
				dLastProcessPathM = 0;
			}
			else if( ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
				dLastProcessPathS = 0;
			}
			rotateAngleList = new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS );

			// solve FK
			gp_Pnt pointG54 = camPoint.Point;
			PostPoint frameDataG54 = new PostPoint()
			{
				X = pointG54.X(),
				Y = pointG54.Y(),
				Z = pointG54.Z(),
				Master = rotateAngleList.Item1,
				Slave = rotateAngleList.Item2
			};
			resultG54 = frameDataG54;
			return true;
		}
	}
}
