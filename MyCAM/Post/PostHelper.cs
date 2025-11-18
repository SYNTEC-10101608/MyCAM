using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.CacheInfo;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;

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

	internal static class PostHelper
	{
		public static bool SolvePath( PostSolver postSolver, ContourCacheInfo currentCAMData, PathNCPackage pathNCPacke, CraftData craftData,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out PathSegmentPostData pathG54PostData, out PathSegmentPostData pathMCSPostData, out PathEndInfo currentPathtEndInfo, bool isNeedDispersion = false )
		{
			// for simulation
			pathMCSPostData = new PathSegmentPostData();

			// for write NC file
			pathG54PostData = new PathSegmentPostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( postSolver == null || pathNCPacke == null || entryAndExitData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath?.Master ?? 0;
			double dLastPointProcess_S = endInfoOfPreviousPath?.Slave ?? 0;

			// flag for process start point
			bool bStart = false;

			// lead-in
			if( pathNCPacke.LeadInSegment.Count > 0 ) {
				if( !SolveProcessPath( postSolver, pathNCPacke.LeadInSegment,
					out List<IPostPath> leadInG54, out List<IPostPath> leadInMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S , isNeedDispersion ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPath.AddRange( leadInG54 );
				pathMCSPostData.LeadInPostPath.AddRange( leadInMCS );

				// set process start point
				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPath.First().StartPoint;
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.LeadInPostPath.First().StartPoint;
				bStart = true;
			}

			// main path
			if( !SolveProcessPath( postSolver, pathNCPacke.MainPathSegment,
				out List<IPostPath> mainG54, out List<IPostPath> mainMCS,
				ref dLastPointProcess_M, ref dLastPointProcess_S , isNeedDispersion ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPath.AddRange( mainG54 );
			pathMCSPostData.MainPathPostPath.AddRange( mainMCS );

			// set process start point
			if( !bStart ) {
				pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPath.First().StartPoint;
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.MainPathPostPath.First().StartPoint;
			}

			// over-cut
			if( pathNCPacke.OverCutSegment.Count > 0 ) {
				if( !SolveProcessPath( postSolver, pathNCPacke.OverCutSegment,
					out List<IPostPath> overCutG54, out List<IPostPath> overCutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S , isNeedDispersion ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPath.AddRange( overCutG54 );
				pathMCSPostData.OverCutPostPath.AddRange( overCutMCS );
			}

			// lead-out
			if( pathNCPacke.LeadOutSegment.Count > 0 ) {
				if( !SolveProcessPath( postSolver, pathNCPacke.LeadOutSegment,
					out List<IPostPath> leadOutG54, out List<IPostPath> leadOutMCS,
					ref dLastPointProcess_M, ref dLastPointProcess_S , isNeedDispersion ) ) {
					return false;
				}
				pathG54PostData.LeadOutPostPath.AddRange( leadOutG54 );
				pathMCSPostData.LeadOutPostPath.AddRange( leadOutMCS );
			}

			// should be the first path
			if( endInfoOfPreviousPath == null ) {

				// the traverse of first path is entry
				CalculateEntry( currentCAMData, entryAndExitData, ref pathG54PostData, ref pathMCSPostData );
			}

			// traverse from previous path to current path
			else {
				CalculateTraverse( endInfoOfPreviousPath, currentCAMData, craftData, ref pathG54PostData, ref pathMCSPostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = currentCAMData.GetProcessEndPoint(),
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
			CAMPoint exitPoint = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfLastPath.EndCAMPoint, entryAndExitData.ExitDistance );
			if( exitPoint == null ) {
				return;
			}

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

		public static List<PostPoint> GetConcatenatedPostList( PathSegmentPostData postData )
		{
			List<PostPoint> result = new List<PostPoint>();
			if( postData == null ) {
				return result;
			}
			if( postData.LeadInPostPath.Count > 0 ) {
				foreach( IPostPath path in postData.LeadInPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			if( postData.MainPathPostPath.Count > 0 ) {
				foreach( IPostPath path in postData.MainPathPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			if( postData.OverCutPostPath.Count > 0 ) {
				foreach( IPostPath path in postData.OverCutPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			if( postData.LeadOutPostPath.Count > 0 ) {
				foreach( IPostPath path in postData.LeadOutPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			return result;
		}

		#region Private methods

		static bool SolveProcessPath( PostSolver postSolver, List<ICAMSegmentElement> pathSegmentList,
			out List<IPostPath> resultG54, out List<IPostPath> resultMCS, ref double dLastProcessPathM, ref double dLastProcessPathS, bool isNeedDispersion = false )
		{
			{
				resultG54 = new List<IPostPath>();
				resultMCS = new List<IPostPath>();
				if( pathSegmentList == null || pathSegmentList.Count == 0 ) {
					return false;
				}

				// solve IK
				if( !SolveIKForPath( postSolver, pathSegmentList, ref dLastProcessPathM, ref dLastProcessPathS,
					out List<Tuple<double, double>> rotateAngleList, out List<bool> singularTagList, isNeedDispersion ) ) {
					return false;
				}

				// TODO: filter the sigular points
				GenerateG54Results( pathSegmentList, rotateAngleList, out resultG54 , isNeedDispersion );
				GenerateMCSResults( postSolver, pathSegmentList, rotateAngleList, out resultMCS , isNeedDispersion );

				return true;
			}
		}

		static bool SolveProcessPath_New( PostSolver postSolver, List<ICAMSegmentElement> SegmentList,
			out List<IPostPath> resultG54, out List<IPostPath> resultMCS, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new List<IPostPath>();
			resultMCS = new List<IPostPath>();
			if( SegmentList == null || SegmentList.Count == 0 ) {
				return false;
			}

			// solve IK
			if( !SolveIKForPath( postSolver, SegmentList, ref dLastProcessPathM, ref dLastProcessPathS,
				out List<Tuple<double, double>> rotateAngleList, out List<bool> singularTagList ) ) {
				return false;
			}

			// TODO: filter the sigular points
			GenerateG54Results( SegmentList, rotateAngleList, out resultG54 );
			GenerateMCSResults( postSolver, SegmentList, rotateAngleList, out resultMCS );

			return true;
		}

		static bool SolveIKForPath( PostSolver postSolver, List<ICAMSegmentElement> SegmentList,
									ref double dLastProcessPathM, ref double dLastProcessPathS,
									out List<Tuple<double, double>> rotateAngleList, out List<bool> singularTagList, bool isNeedDispersion = false )
		{
			rotateAngleList = new List<Tuple<double, double>>();
			singularTagList = new List<bool>();

			foreach( ICAMSegmentElement camSegment in SegmentList ) {
				List<CAMPoint2> pointList = new List<CAMPoint2>();
				if( isNeedDispersion ) {
					pointList = camSegment.CAMPointList;
				}
				else {
					if( camSegment is ArcCAMSegment arcCAMSegment ) {
						pointList.Add( arcCAMSegment.StartPoint );
						pointList.Add( arcCAMSegment.MidPoint );
						pointList.Add( camSegment.EndPoint );
					}
					else {
						pointList.Add( camSegment.StartPoint );
						pointList.Add( camSegment.EndPoint );
					}
				}
				foreach( CAMPoint2 camPoint in pointList ) {
					IKSolveResult ikResult = postSolver.SolveIK( camPoint.ToolVec, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
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
			}
			return true;
		}

		static void GenerateG54Results( List<ICAMSegmentElement> camSegmentList, List<Tuple<double, double>> rotateAngleList, out List<IPostPath> resultG54, bool isNeedDispersion = false )
		{
			resultG54 = new List<IPostPath>();
			int pointIndex = 0;

			foreach( ICAMSegmentElement camSegment in camSegmentList ) {

				// get nc point
				List<CAMPoint2> pointList = GetPointsFromSegment( camSegment, isNeedDispersion );
				List<PostPoint> postPointList = CreateG54PostPoints( pointList, rotateAngleList, ref pointIndex );
				IPostPath g54PostPath = CreatePostPath( camSegment, postPointList, isNeedDispersion );
				if( g54PostPath != null ) {
					resultG54.Add( g54PostPath );
				}
			}
		}

		static List<CAMPoint2> GetPointsFromSegment( ICAMSegmentElement camSegment, bool isNeedDispersion )
		{
			if( isNeedDispersion ) {
				return camSegment.CAMPointList;
			}
			else {
				List<CAMPoint2> pointList = new List<CAMPoint2>();
				if( camSegment is ArcCAMSegment arcCAMSegment ) {
					pointList.Add( arcCAMSegment.StartPoint );
					pointList.Add( arcCAMSegment.MidPoint );
					pointList.Add( camSegment.EndPoint );
				}
				else {
					pointList.Add( camSegment.StartPoint );
					pointList.Add( camSegment.EndPoint );
				}
				return pointList;
			}
		}

		static List<PostPoint> CreateG54PostPoints( List<CAMPoint2> camPoints, List<Tuple<double, double>> rotateAngleList, ref int pointIndex )
		{
			List<PostPoint> postPoints = new List<PostPoint>();

			foreach( CAMPoint2 camPoint in camPoints ) {
				gp_Pnt pointG54 = camPoint.Point;
				PostPoint frameDataG54 = new PostPoint()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ pointIndex ].Item1,
					Slave = rotateAngleList[ pointIndex ].Item2
				};
				postPoints.Add( frameDataG54 );
				pointIndex++;
			}

			return postPoints;
		}

		static IPostPath CreatePostPath( ICAMSegmentElement camSegment, List<PostPoint> postPointList, bool isNeedDispersion )
		{
			if( isNeedDispersion ) {
				if( camSegment.ContourType == EContourType.Arc ) {
					return new DispersionArcPostPath( postPointList );
				}
				else {
					return new DispersionLinePostPath( postPointList );
				}
			}
			else {
				if( camSegment.ContourType == EContourType.Arc && postPointList.Count >= 3 ) {
					return new ArcPostPath( postPointList[ 0 ], postPointList[ 1 ], postPointList[ 2 ] );
				}
				else if( postPointList.Count >= 2 ) {
					return new LinePostPath( postPointList[ 0 ], postPointList[ 1 ] );
				}
				return null;
			}
		}

		static void GenerateMCSResults( PostSolver postSolver, List<ICAMSegmentElement> camSegmentList, List<Tuple<double, double>> rotateAngleList, out List<IPostPath> resultMCS, bool isNeedDispersion = false )
		{
			resultMCS = new List<IPostPath>();
			int pointIndex = 0;

			foreach( ICAMSegmentElement camSegment in camSegmentList ) {
				List<CAMPoint2> pointList = GetPointsFromSegment( camSegment, isNeedDispersion );
				List<PostPoint> postPointList = CreateMCSPostPoints( postSolver, pointList, rotateAngleList, ref pointIndex );
				IPostPath mcsPostPath = CreatePostPath( camSegment, postPointList, isNeedDispersion );
				if( mcsPostPath != null ) {
					resultMCS.Add( mcsPostPath );
				}
			}
		}

		static List<PostPoint> CreateMCSPostPoints( PostSolver postSolver, List<CAMPoint2> camPoints, List<Tuple<double, double>> rotateAngleList, ref int pointIndex )
		{
			List<PostPoint> postPoints = new List<PostPoint>();

			foreach( CAMPoint2 camPoint in camPoints ) {
				gp_Pnt pointG54 = camPoint.Point;
				gp_Vec tcpOffset = postSolver.SolveFK( rotateAngleList[ pointIndex ].Item1, rotateAngleList[ pointIndex ].Item2, pointG54 );
				gp_Pnt pointMCS = pointG54.Translated( tcpOffset );
				PostPoint frameDataMCS = new PostPoint()
				{
					X = pointMCS.X(),
					Y = pointMCS.Y(),
					Z = pointMCS.Z(),
					Master = rotateAngleList[ pointIndex ].Item1,
					Slave = rotateAngleList[ pointIndex ].Item2
				};
				postPoints.Add( frameDataMCS );
				pointIndex++;
			}

			return postPoints;
		}

		static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, ContourCacheInfo currentCAMData, CraftData craftData, ref PathSegmentPostData pathG54PostData, ref PathSegmentPostData pathMCSPostData )
		{
			if( endInfoOfPreviousPath == null || currentCAMData == null ) {
				return;
			}

			// p1: end of previous path (not used here)
			// p2: lift up point of previous path
			// p3: frog leap middle point (if frog leap)
			// p4: cut down point of current path
			// p5: start of current path (not used here)
			CAMPoint p1 = endInfoOfPreviousPath.EndCAMPoint;
			CAMPoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfPreviousPath.EndCAMPoint, craftData.TraverseData.LiftUpDistance );
			CAMPoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( currentCAMData.GetProcessStartPoint(), craftData.TraverseData.CutDownDistance );
			CAMPoint p5 = currentCAMData.GetProcessStartPoint();

			// lift up
			if( craftData.TraverseData.LiftUpDistance > 0 && p2 != null ) {

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
			if( craftData.TraverseData.FrogLeapDistance > 0 && p2 != null && p4 != null ) {
				CAMPoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, craftData.TraverseData.FrogLeapDistance );

				if( p3 != null ) {
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
			}

			// cut down
			if( craftData.TraverseData.CutDownDistance > 0 && p4 != null ) {

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
			pathG54PostData.FollowSafeDistance = craftData.TraverseData.FollowSafeDistance;
			pathMCSPostData.FollowSafeDistance = craftData.TraverseData.FollowSafeDistance;
		}

		static void CalculateEntry( ContourCacheInfo currentCAMData, EntryAndExitData entryAndExitData, ref PathSegmentPostData pathG54PostData, ref PathSegmentPostData pathMCSPostData )
		{
			if( currentCAMData == null || entryAndExitData == null ) {
				return;
			}
			if( entryAndExitData.EntryDistance <= 0 ) {

				// just set follow safe distance
				pathG54PostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
				pathMCSPostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
				return;
			}
			CAMPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( currentCAMData.GetProcessStartPoint(), entryAndExitData.EntryDistance );
			if( entryPoint == null ) {
				return;
			}

			// G54
			pathG54PostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.CADPoint.Point.X(),
				Y = entryPoint.CADPoint.Point.Y(),
				Z = entryPoint.CADPoint.Point.Z(),
				Master = pathG54PostData.ProcessStartPoint.Master,
				Slave = pathG54PostData.ProcessStartPoint.Slave
			};
			pathG54PostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;

			// MCS
			pathMCSPostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.CADPoint.Point.X(), // TODO: need to be changed to MCS point
				Y = entryPoint.CADPoint.Point.Y(),
				Z = entryPoint.CADPoint.Point.Z(),
				Master = pathMCSPostData.ProcessStartPoint.Master,
				Slave = pathMCSPostData.ProcessStartPoint.Slave
			};
			pathMCSPostData.FollowSafeDistance = entryAndExitData.FollowSafeDistance;
		}

		#endregion
	}
}
