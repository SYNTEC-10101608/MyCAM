using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class PathEndInfo
	{
		public CAMPoint EndCAMPoint
		{
			get; set;
		}

		public double Master
		{
			get; set;
		}

		public double Slave
		{
			get; set;
		}
	}

	internal class ContourNCPackage
	{
		public LeadData LeadLineParam
		{
			get;
		}

		public double OverCutLength
		{
			get;
		}

		public List<CAMPoint> CAMPointList
		{
			get;
		}

		public List<CAMPoint> LeadInCAMPointList
		{
			get;
		}

		public List<CAMPoint> LeadOutCAMPointList
		{
			get;
		}

		public List<CAMPoint> OverCutCAMPointList
		{
			get;
		}

		public TraverseData TraverseData
		{
			get;
		}

		public CAMPoint GetProcessStartPoint()
		{
			return null;
		}

		public CAMPoint GetProcessEndPoint()
		{
			return null;
		}
	}

	internal static class PostHelper
	{
		public static bool SolvePath( PostSolver postSolver, ContourNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out PostData pathG54PostData, out PostData pathMCSPostData, out PathEndInfo currentPathtEndInfo )
		{
			// for simulation
			pathMCSPostData = new PostData();

			// for write NC file
			pathG54PostData = new PostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( postSolver == null || currentPathNCPack == null || entryAndExitData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath?.Master ?? 0;
			double dLastPointProcess_S = endInfoOfPreviousPath?.Slave ?? 0;

			// flag for process start point
			bool bStart = false;

			// lead-in
			if( currentPathNCPack.LeadLineParam.LeadIn.Type != LeadLineType.None && currentPathNCPack.LeadInCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentPathNCPack.LeadInCAMPointList,
					out List<PostPoint> leadInG54, out List<PostPoint> leadInMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPointList.AddRange( leadInG54 );
				pathMCSPostData.LeadInPostPointList.AddRange( leadInMCS );

				// set process start point
				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPointList[ 0 ];
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.LeadInPostPointList[ 0 ];
				bStart = true;
			}

			// main path
			if( !SolveProcessPath( postSolver, currentPathNCPack.CAMPointList,
				out List<PostPoint> mainG54, out List<PostPoint> mainMCS,
				ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPointList.AddRange( mainG54 );
			pathMCSPostData.MainPathPostPointList.AddRange( mainMCS );

			// set process start point
			if( !bStart ) {
				pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPointList[ 0 ];
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.MainPathPostPointList[ 0 ];
			}

			// over-cut
			if( currentPathNCPack.OverCutLength != 0 && currentPathNCPack.OverCutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentPathNCPack.OverCutCAMPointList,
					out List<PostPoint> overCutG54, out List<PostPoint> overCutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPointList.AddRange( overCutG54 );
				pathMCSPostData.OverCutPostPointList.AddRange( overCutMCS );
			}

			// lead-out
			if( currentPathNCPack.LeadLineParam.LeadOut.Type != LeadLineType.None && currentPathNCPack.LeadOutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentPathNCPack.LeadOutCAMPointList,
					out List<PostPoint> leadOutG54, out List<PostPoint> leadOutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.LeadOutPostPointList.AddRange( leadOutG54 );
				pathMCSPostData.LeadOutPostPointList.AddRange( leadOutMCS );
			}

			// should be the first path
			if( endInfoOfPreviousPath == null ) {

				// the traverse of first path is entry
				CalculateEntry( currentPathNCPack, entryAndExitData, ref pathG54PostData, ref pathMCSPostData );
			}

			// traverse from previous path to current path
			else {
				CalculateTraverse( endInfoOfPreviousPath, currentPathNCPack, ref pathG54PostData, ref pathMCSPostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = currentPathNCPack.GetProcessEndPoint(),
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};
			return true;
		}

		public static void CalculateExit( PathEndInfo endInfoOfLastPath, EntryAndExitData entryAndExitData,
			out PostPoint G54ExitPoint, out PostPoint MCSExitPoint )
		{
			G54ExitPoint = null;
			MCSExitPoint = null;
			if( entryAndExitData.ExitDistance <= 0 ) {
				return;
			}
			ITraversePoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfLastPath.EndCAMPoint, entryAndExitData.ExitDistance );
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

			// MCS
			MCSExitPoint = G54ExitPoint.Clone(); // TODO: need to be changed to MCS point
		}

		public static List<PostPoint> GetConcatenatedPostList( PostData postData )
		{
			List<PostPoint> result = new List<PostPoint>();
			if( postData == null ) {
				return result;
			}
			if( postData.LeadInPostPointList.Count > 0 ) {
				result.AddRange( postData.LeadInPostPointList );
			}
			if( postData.MainPathPostPointList.Count > 0 ) {
				result.AddRange( postData.MainPathPostPointList );
			}
			if( postData.OverCutPostPointList.Count > 0 ) {
				result.AddRange( postData.OverCutPostPointList );
			}
			if( postData.LeadOutPostPointList.Count > 0 ) {
				result.AddRange( postData.LeadOutPostPointList );
			}
			return result;
		}

		#region Private methods

		static bool SolveProcessPath( PostSolver postSolver, List<CAMPoint> camPointList,
			out List<PostPoint> resultG54, out List<PostPoint> resultMCS, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new List<PostPoint>();
			resultMCS = new List<PostPoint>();
			if( camPointList == null || camPointList.Count == 0 ) {
				return false;
			}

			// solve IK
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			List<bool> singularTagList = new List<bool>();
			foreach( CAMPoint point in camPointList ) {
				IKSolveResult ikResult = postSolver.SolveIK( point, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
				if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution ) {
					return false;
				}
				rotateAngleList.Add( new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS ) );
				if( ikResult == IKSolveResult.NoError ) {
					singularTagList.Add( false );
				}
				else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					singularTagList.Add( true );
				}
			}

			// TODO: filter the sigular points
			// solve FK
			for( int i = 0; i < camPointList.Count; i++ ) {
				gp_Pnt pointG54 = camPointList[ i ].Point;
				gp_Vec tcpOffset = postSolver.SolveFK( rotateAngleList[ i ].Item1, rotateAngleList[ i ].Item2, pointG54 );
				gp_Pnt pointMCS = pointG54.Translated( tcpOffset );

				// add G54 frame data
				PostPoint frameDataG54 = new PostPoint()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultG54.Add( frameDataG54 );

				// add MCS frame data
				PostPoint frameDataMCS = new PostPoint()
				{
					X = pointMCS.X(),
					Y = pointMCS.Y(),
					Z = pointMCS.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultMCS.Add( frameDataMCS );
			}
			return true;
		}

		static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, ContourNCPackage currentPathNCPack, ref PostData pathG54PostData, ref PostData pathMCSPostData )
		{
			if( endInfoOfPreviousPath == null || currentPathNCPack == null ) {
				return;
			}

			// p1: end of previous path (not used here)
			// p2: lift up point of previous path
			// p3: frog leap middle point (if frog leap)
			// p4: cut down point of current path
			// p5: start of current path (not used here)
			ITraversePoint p1 = endInfoOfPreviousPath.EndCAMPoint;
			ITraversePoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfPreviousPath.EndCAMPoint, currentPathNCPack.TraverseData.LiftUpDistance );
			ITraversePoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentPathNCPack.GetProcessStartPoint(), currentPathNCPack.TraverseData.CutDownDistance );
			ITraversePoint p5 = currentPathNCPack.GetProcessStartPoint();

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

				// MCS
				pathMCSPostData.LiftUpPostPoint = new PostPoint()
				{
					X = p2.Point.X(), // TODO: need to be changed to MCS point
					Y = p2.Point.Y(),
					Z = p2.Point.Z(),
					Master = endInfoOfPreviousPath.Master,
					Slave = endInfoOfPreviousPath.Slave
				};
			}

			// frog leap
			if( currentPathNCPack.TraverseData.FrogLeapDistance > 0 && p2 != null && p4 != null ) {
				ITraversePoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, currentPathNCPack.TraverseData.FrogLeapDistance );

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

					// MCS middle point
					pathMCSPostData.FrogLeapMidPostPoint = new PostPoint()
					{
						X = p3.Point.X(), // TODO: need to be changed to MCS point
						Y = p3.Point.Y(),
						Z = p3.Point.Z(),
						Master = ( endInfoOfPreviousPath.Master + pathMCSPostData.ProcessStartPoint.Master ) / 2.0,
						Slave = ( endInfoOfPreviousPath.Slave + pathMCSPostData.ProcessStartPoint.Slave ) / 2.0
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

				// MCS
				pathMCSPostData.CutDownPostPoint = new PostPoint()
				{
					X = p4.Point.X(), // TODO: need to be changed to MCS point
					Y = p4.Point.Y(),
					Z = p4.Point.Z(),
					Master = pathMCSPostData.ProcessStartPoint.Master,
					Slave = pathMCSPostData.ProcessStartPoint.Slave
				};
			}
			pathG54PostData.FollowSafeDistance = currentPathNCPack.TraverseData.FollowSafeDistance;
			pathMCSPostData.FollowSafeDistance = currentPathNCPack.TraverseData.FollowSafeDistance;
		}

		static void CalculateEntry( ContourNCPackage currentPathNCPack, EntryAndExitData entryAndExitData, ref PostData pathG54PostData, ref PostData pathMCSPostData )
		{
			if( currentPathNCPack == null || entryAndExitData == null ) {
				return;
			}
			if( entryAndExitData.EntryDistance <= 0 ) {

				// just set follow safe distance
				pathG54PostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
				pathMCSPostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
				return;
			}
			ITraversePoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( currentPathNCPack.GetProcessStartPoint(), entryAndExitData.EntryDistance );
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

			// MCS
			pathMCSPostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.Point.X(), // TODO: need to be changed to MCS point
				Y = entryPoint.Point.Y(),
				Z = entryPoint.Point.Z(),
				Master = pathMCSPostData.ProcessStartPoint.Master,
				Slave = pathMCSPostData.ProcessStartPoint.Slave
			};
			pathMCSPostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
		}

		#endregion
	}
}
