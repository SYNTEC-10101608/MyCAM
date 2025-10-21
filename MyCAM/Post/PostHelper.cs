using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class PathEndInfo
	{
		public bool IsExist
		{
			get; set;
		}

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

	internal static class PostHelper
	{
		public static bool SolvePath( PostSolver postSolver, CAMData currentCAMData, PathEndInfo endInfoOfPreviousPath,
			bool isFirstPath, bool isLastPath, EntryAndExitData entryAndExitData,
			out PostData pathG54PostData, out PostData pathMCSPostData, out PathEndInfo currentPathtEndInfo )
		{
			// for simulation
			pathMCSPostData = new PostData();

			// for write NC file
			pathG54PostData = new PostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( postSolver == null || endInfoOfPreviousPath == null || currentCAMData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath.IsExist ? endInfoOfPreviousPath.Master : 0;
			double dLastPointProcess_S = endInfoOfPreviousPath.IsExist ? endInfoOfPreviousPath.Slave : 0;

			// flag for process start point
			bool bStart = false;

			// lead-in
			if( currentCAMData.LeadLineParam.LeadIn.Type != LeadLineType.None && currentCAMData.LeadInCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentCAMData.LeadInCAMPointList,
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
			if( !SolveProcessPath( postSolver, currentCAMData.CAMPointList,
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
			if( currentCAMData.OverCutLength != 0 && currentCAMData.OverCutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentCAMData.OverCutCAMPointList,
					out List<PostPoint> overCutG54, out List<PostPoint> overCutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPointList.AddRange( overCutG54 );
				pathMCSPostData.OverCutPostPointList.AddRange( overCutMCS );
			}

			// lead-out
			if( currentCAMData.LeadLineParam.LeadOut.Type != LeadLineType.None && currentCAMData.LeadOutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentCAMData.LeadOutCAMPointList,
					out List<PostPoint> leadOutG54, out List<PostPoint> leadOutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.LeadOutPostPointList.AddRange( leadOutG54 );
				pathMCSPostData.LeadOutPostPointList.AddRange( leadOutMCS );
			}

			// traverse from previous path to current path
			if( isFirstPath ) {

				// the entry is treat as cut down of the first path
				CalculateEntry( currentCAMData, entryAndExitData, ref pathG54PostData, ref pathMCSPostData );
			}
			else {
				CalculateTraverse( endInfoOfPreviousPath, currentCAMData, ref pathG54PostData, ref pathMCSPostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				IsExist = true,
				EndCAMPoint = currentCAMData.GetProcessEndPoint(),
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};
			return true;
		}

		public static void CalculateExit( PathEndInfo endInfoOfLastPath, EntryAndExitData entryAndExitData,
			out PostPoint G54ExitPoint, out PostPoint MCSExitPoint )
		{
			CAMPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfLastPath.EndCAMPoint, entryAndExitData.ExitDistance );

			// G54
			G54ExitPoint = new PostPoint()
			{
				X = exitPoint.CADPoint.Point.X(),
				Y = exitPoint.CADPoint.Point.Y(),
				Z = exitPoint.CADPoint.Point.Z(),
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

			// solve IK
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			List<bool> sigularTagList = new List<bool>();
			foreach( CAMPoint point in camPointList ) {
				IKSolveResult ikResult = postSolver.SolveIK( point, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
				if( ikResult == IKSolveResult.IvalidInput || ikResult == IKSolveResult.NoSolution ) {
					return false;
				}
				rotateAngleList.Add( new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS ) );
				if( ikResult == IKSolveResult.NoError ) {
					sigularTagList.Add( false );
				}
				else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
					sigularTagList.Add( true );
				}
			}

			// TODO: filter the sigular points
			// solve FK
			for( int i = 0; i < camPointList.Count; i++ ) {
				gp_Pnt pointG54 = camPointList[ i ].CADPoint.Point;
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

		static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, CAMData currentCAMData, ref PostData pathG54PostData, ref PostData pathMCSPostData )
		{
			if( endInfoOfPreviousPath.IsExist == false ) {
				return;
			}

			// p1: end of previous path (not used here)
			// p2: lift up point of previous path
			// p3: frog leap middle point (if frog leap)
			// p4: cut down point of current path
			// p5: start of current path (not used here)
			CAMPoint p1 = endInfoOfPreviousPath.EndCAMPoint;
			CAMPoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfPreviousPath.EndCAMPoint, currentCAMData.TraverseData.LiftUpDistance );
			CAMPoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentCAMData.GetProcessStartPoint(), currentCAMData.TraverseData.CutDownDistance );
			CAMPoint p5 = currentCAMData.GetProcessStartPoint();

			// lift up
			if( currentCAMData.TraverseData.LiftUpDistance > 0 ) {

				// G54
				pathG54PostData.LiftUpPostPoint = new PostPoint()
				{
					X = p2.CADPoint.Point.X(),
					Y = p2.CADPoint.Point.Y(),
					Z = p2.CADPoint.Point.Z(),
					Master = endInfoOfPreviousPath.Master,
					Slave = endInfoOfPreviousPath.Slave
				};

				// MCS
				pathMCSPostData.LiftUpPostPoint = new PostPoint()
				{
					X = p2.CADPoint.Point.X(), // TODO: need to be changed to MCS point
					Y = p2.CADPoint.Point.Y(),
					Z = p2.CADPoint.Point.Z(),
					Master = endInfoOfPreviousPath.Master,
					Slave = endInfoOfPreviousPath.Slave
				};
			}

			// frog leap
			if( currentCAMData.TraverseData.EnableFrogLeap && currentCAMData.TraverseData.FrogLeapDistance > 0 ) {
				CAMPoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, currentCAMData.TraverseData.FrogLeapDistance );

				// G54 middle point
				pathG54PostData.FrogLeapMidPostPoint = new PostPoint()
				{
					X = p3.CADPoint.Point.X(),
					Y = p3.CADPoint.Point.Y(),
					Z = p3.CADPoint.Point.Z(),
					Master = ( endInfoOfPreviousPath.Master + pathG54PostData.ProcessStartPoint.Master ) / 2.0,
					Slave = ( endInfoOfPreviousPath.Slave + pathG54PostData.ProcessStartPoint.Slave ) / 2.0
				};

				// MCS middle point
				pathMCSPostData.FrogLeapMidPostPoint = new PostPoint()
				{
					X = p3.CADPoint.Point.X(), // TODO: need to be changed to MCS point
					Y = p3.CADPoint.Point.Y(),
					Z = p3.CADPoint.Point.Z(),
					Master = ( endInfoOfPreviousPath.Master + pathMCSPostData.ProcessStartPoint.Master ) / 2.0,
					Slave = ( endInfoOfPreviousPath.Slave + pathMCSPostData.ProcessStartPoint.Slave ) / 2.0
				};
			}

			// cut down
			if( currentCAMData.TraverseData.CutDownDistance > 0 ) {

				// G54
				pathG54PostData.CutDownPostPoint = new PostPoint()
				{
					X = p4.CADPoint.Point.X(),
					Y = p4.CADPoint.Point.Y(),
					Z = p4.CADPoint.Point.Z(),
					Master = pathG54PostData.ProcessStartPoint.Master,
					Slave = pathG54PostData.ProcessStartPoint.Slave
				};

				// MCS
				pathMCSPostData.CutDownPostPoint = new PostPoint()
				{
					X = p4.CADPoint.Point.X(), // TODO: need to be changed to MCS point
					Y = p4.CADPoint.Point.Y(),
					Z = p4.CADPoint.Point.Z(),
					Master = pathMCSPostData.ProcessStartPoint.Master,
					Slave = pathMCSPostData.ProcessStartPoint.Slave
				};
			}
			pathG54PostData.FollowSafeDistance = currentCAMData.TraverseData.FollowSafeDistance;
			pathMCSPostData.FollowSafeDistance = currentCAMData.TraverseData.FollowSafeDistance;
		}

		static void CalculateEntry( CAMData currentCAMData, EntryAndExitData entryAndExitData, ref PostData pathG54PostData, ref PostData pathMCSPostData )
		{
			CAMPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( currentCAMData.GetProcessStartPoint(), entryAndExitData.EntryDistance );

			// G54
			pathG54PostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.CADPoint.Point.X(),
				Y = entryPoint.CADPoint.Point.Y(),
				Z = entryPoint.CADPoint.Point.Z(),
				Master = pathG54PostData.ProcessStartPoint.Master,
				Slave = pathG54PostData.ProcessStartPoint.Slave
			};
			pathG54PostData.FollowSafeDistance = currentCAMData.TraverseData.FollowSafeDistance;

			// MCS
			pathMCSPostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.CADPoint.Point.X(), // TODO: need to be changed to MCS point
				Y = entryPoint.CADPoint.Point.Y(),
				Z = entryPoint.CADPoint.Point.Z(),
				Master = pathMCSPostData.ProcessStartPoint.Master,
				Slave = pathMCSPostData.ProcessStartPoint.Slave
			};
			pathMCSPostData.FollowSafeDistance = currentCAMData.TraverseData.FollowSafeDistance;
		}

		public static bool SolvePathSegment( PostSolver postSolver, List<ICAMSegmentElement> pathSegmentList, out PathSegmentPostData pathSegmentPostData, out PathSegmentPostData pathMCSPostData )
		{
			// for simulation
			pathMCSPostData = new PathSegmentPostData();

			// for write NC file
			pathSegmentPostData = new PathSegmentPostData();

			if( postSolver == null || pathSegmentList == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = 0, dLastPointProcess_S = 0;


			for( int i = 0; i < pathSegmentList.Count; i++ ) {
				bool bSegmentTransSuccess = SolveSegmentPart( postSolver, pathSegmentList[ i ], out PostPath segmentPostPath, out PostPath MCSPostPath, ref dLastPointProcess_M, ref dLastPointProcess_S );
				if( bSegmentTransSuccess == false ) {
					return false;
				}
				pathSegmentPostData.MainPathPostPath.Add( segmentPostPath );
				pathMCSPostData.MainPathPostPath.Add( MCSPostPath );
			}
			return true;
		}

		static bool SolveSegmentPart( PostSolver postSolver, ICAMSegmentElement processPathSegment, out PostPath SegmentG54PostPath, out PostPath MCSPostPath, ref double dLastProcessPathM, ref double dLastPointProcess_S )
		{
			SegmentG54PostPath = null;
			MCSPostPath = null;
			CAMPoint camStartPoint = processPathSegment.StartPoint;
			CAMPoint camEndPoint = processPathSegment.EndPoint;
			bool bGetStartPointSuccess = Solve1Point( postSolver, camStartPoint, out PostPoint StartPointG54, out PostPoint StartPointMCS, ref dLastProcessPathM, ref dLastPointProcess_S );
			if( bGetStartPointSuccess == false ) {
				return false;
			}
			bool bGetEndPointSuccess = Solve1Point( postSolver, camEndPoint, out PostPoint EndPointG54, out PostPoint EndPoitMCS, ref dLastProcessPathM, ref dLastPointProcess_S );
			if( bGetEndPointSuccess == false ) {
				return false;
			}

			if( processPathSegment is ArcCAMSegment arcSegment ) {
				CAMPoint cadMidPoint = arcSegment.MidPoint;
				bool bGetMidPointSuccess = Solve1Point( postSolver, cadMidPoint, out PostPoint MidPointG54, out PostPoint MidPointMCS, ref dLastProcessPathM, ref dLastPointProcess_S );
				if( bGetEndPointSuccess == false ) {
					return false;
				}
				SegmentG54PostPath = new ArcPostPath( StartPointG54, MidPointG54, EndPointG54 );
				MCSPostPath = new ArcPostPath( StartPointMCS, MidPointMCS, EndPoitMCS );
			}
			else {
				SegmentG54PostPath = new LinePostPath( StartPointG54, EndPointG54 );
				MCSPostPath = new LinePostPath( StartPointMCS, EndPoitMCS );
			}
			return true;
		}


		static bool Solve1Point( PostSolver postSolver, CAMPoint camPoint, out PostPoint frameDataG54, out PostPoint frameDataMCS, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			frameDataG54 = new PostPoint();
			frameDataMCS = new PostPoint();

			IKSolveResult ikResult = postSolver.SolveIK( camPoint, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
			if( ikResult == IKSolveResult.IvalidInput || ikResult == IKSolveResult.NoSolution ) {
				return false;
			}
			(double, double) rotateAngle = (dLastProcessPathM, dLastProcessPathS);
			gp_Pnt startPointG54 = camPoint.CADPoint.Point;
			gp_Vec statPointTcpOffset = postSolver.SolveFK( rotateAngle.Item1, rotateAngle.Item2, startPointG54 );
			gp_Pnt pointMCS = startPointG54.Translated( statPointTcpOffset );

			// G54 frame data
			frameDataG54 = new PostPoint()
			{
				X = startPointG54.X(),
				Y = startPointG54.Y(),
				Z = startPointG54.Z(),
				Master = rotateAngle.Item1,
				Slave = rotateAngle.Item2
			};

			// MCS frame data
			frameDataMCS = new PostPoint()
			{
				X = pointMCS.X(),
				Y = pointMCS.Y(),
				Z = pointMCS.Z(),
				Master = rotateAngle.Item1,
				Slave = rotateAngle.Item2
			};
			return true;
		}


		#endregion
	}
}
