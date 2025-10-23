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
		public static bool SolvePath( PostSolver postSolver, PathEndInfo endInfoOfPreviousPath, CAMData currentCAMData,
			 out PostData pathMCSPostData, out PostData pathG54PostData, out PathEndInfo currenPathtEndInfo )
		{
			// for simulation
			pathMCSPostData = new PostData();

			// for write NC file
			pathG54PostData = new PostData();

			// to make solution continuous
			currenPathtEndInfo = new PathEndInfo();
			if( postSolver == null || endInfoOfPreviousPath == null || currentCAMData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath.IsExist ? endInfoOfPreviousPath.Master : 0;
			double dLastPointProcess_S = endInfoOfPreviousPath.IsExist ? endInfoOfPreviousPath.Slave : 0;

			// lead-in
			if( currentCAMData.LeadLineParam.LeadIn.Type != LeadLineType.None && currentCAMData.LeadInCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentCAMData.LeadInCAMPointList,
					out List<PostPoint> leadInPost, out List<PostPoint> leadInMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathMCSPostData.LeadInPostPointList.AddRange( leadInMCS );
				pathG54PostData.LeadInPostPointList.AddRange( leadInPost );
			}

			// main path
			if( !SolveProcessPath( postSolver, currentCAMData.CAMPointList,
				out List<PostPoint> mainPost, out List<PostPoint> mainMCS,
				ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
				return false;
			}
			pathMCSPostData.MainPathPostPointList.AddRange( mainMCS );
			pathG54PostData.MainPathPostPointList.AddRange( mainPost );

			// over-cut
			if( currentCAMData.OverCutLength != 0 && currentCAMData.OverCutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentCAMData.OverCutCAMPointList,
					out List<PostPoint> overCutPost, out List<PostPoint> overCutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathMCSPostData.OverCutPostPointList.AddRange( overCutMCS );
				pathG54PostData.OverCutPostPointList.AddRange( overCutPost );
			}

			// lead-out
			if( currentCAMData.LeadLineParam.LeadOut.Type != LeadLineType.None && currentCAMData.LeadOutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentCAMData.LeadOutCAMPointList,
					out List<PostPoint> leadOutPost, out List<PostPoint> leadOutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathMCSPostData.LeadOutPostPointList.AddRange( leadOutMCS );
				pathG54PostData.LeadOutPostPointList.AddRange( leadOutPost );
			}

			// traverse from previous path to current path
			CalculateTraverse( endInfoOfPreviousPath, currentCAMData, dLastPointProcess_M, dLastPointProcess_S,
				ref pathMCSPostData, ref pathG54PostData );

			// end info of current path
			currenPathtEndInfo = new PathEndInfo()
			{
				IsExist = true,
				EndCAMPoint = currentCAMData.GetProcessEndPoint(),
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};
			return true;
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


		#region Private methods

		static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, CAMData currentCAMData, double dEndPointProcess_M, double dEndPointProcess_S,
			ref PostData pathMCSPostData, ref PostData pathG54PostData )
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
			CAMPoint p2 = TraverseHelper.GetLiftUpPoint( endInfoOfPreviousPath.EndCAMPoint, currentCAMData.TraverseData.LiftUpDistance );
			CAMPoint p4 = TraverseHelper.GetCutDownPoint( currentCAMData.GetProcessStartPoint(), currentCAMData.TraverseData.CutDownDistance );
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
					Master = ( endInfoOfPreviousPath.Master + dEndPointProcess_M ) / 2.0,
					Slave = ( endInfoOfPreviousPath.Slave + dEndPointProcess_S ) / 2.0
				};

				// MCS middle point
				pathMCSPostData.FrogLeapMidPostPoint = new PostPoint()
				{
					X = p3.CADPoint.Point.X(), // TODO: need to be changed to MCS point
					Y = p3.CADPoint.Point.Y(),
					Z = p3.CADPoint.Point.Z(),
					Master = ( endInfoOfPreviousPath.Master + dEndPointProcess_M ) / 2.0,
					Slave = ( endInfoOfPreviousPath.Slave + dEndPointProcess_S ) / 2.0
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
					Master = dEndPointProcess_M,
					Slave = dEndPointProcess_S
				};

				// MCS
				pathMCSPostData.CutDownPostPoint = new PostPoint()
				{
					X = p4.CADPoint.Point.X(), // TODO: need to be changed to MCS point
					Y = p4.CADPoint.Point.Y(),
					Z = p4.CADPoint.Point.Z(),
					Master = dEndPointProcess_M,
					Slave = dEndPointProcess_S
				};
			}
			pathG54PostData.FollowSafeDistance = currentCAMData.TraverseData.FollowSafeDistance;
			pathMCSPostData.FollowSafeDistance = currentCAMData.TraverseData.FollowSafeDistance;
		}

		#endregion
	}
}
