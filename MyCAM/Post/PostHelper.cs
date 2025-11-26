using System;
using System.Collections.Generic;
using System.Linq;
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
		public static bool SolvePath( PostSolver postSolver, PathNCPackage pathNCPacke,
	PathEndInfo endInfoOfPreviousPath,
	// re: isNeedDispersion 沒用到了，可以移除
	// re: 如同討論，useACSolution 不做引數傳遞，現階段固定為 true
	out PathPostData pathG54PostData, out PathPostData pathMCSPostData, out PathEndInfo currentPathtEndInfo, bool isNeedDispersion = true, bool useACSolution = true )
		{
			// for simulation
			pathMCSPostData = new PathPostData();

			// for write NC file
			pathG54PostData = new PathPostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( postSolver == null || pathNCPacke == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath?.Master ?? 0;
			double dLastPointProcess_S = endInfoOfPreviousPath?.Slave ?? 0;

			if( useACSolution ) {
				return SolvePathWithACCase( postSolver, pathNCPacke, endInfoOfPreviousPath,
					out pathG54PostData, out pathMCSPostData, out currentPathtEndInfo,
					ref dLastPointProcess_M, ref dLastPointProcess_S );
			}
			else {
				return SolvePathNormalCase( postSolver, pathNCPacke, endInfoOfPreviousPath,
					out pathG54PostData, out pathMCSPostData, out currentPathtEndInfo,
					ref dLastPointProcess_M, ref dLastPointProcess_S );
			}
		}

		static bool SolvePathWithACCase( PostSolver postSolver, PathNCPackage pathNCPacke,
			PathEndInfo endInfoOfPreviousPath,
			out PathPostData pathG54PostData, out PathPostData pathMCSPostData, out PathEndInfo currentPathtEndInfo,
			ref double dLastPointProcess_M, ref double dLastPointProcess_S )
		{
			pathG54PostData = new PathPostData();
			pathMCSPostData = new PathPostData();
			currentPathtEndInfo = new PathEndInfo();
			bool bStart = false;

			// Step 1 : solve main path
			if( !SolvePath_ACSolution( postSolver, pathNCPacke.MainPathSegment,
				ref dLastPointProcess_M, ref dLastPointProcess_S,
				out List<ISegmentPostData> mainG54 ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPath.AddRange( mainG54 );
			pathMCSPostData.MainPathPostPath.AddRange( mainG54 );

			// lead-in with fixed rotation based on main path start
			if( pathNCPacke.LeadInSegment.Count > 0 ) {
				List<ISegmentPostData> leadInPostData = BuildPostSegmentsWithFixedRotation(
					pathNCPacke.LeadInSegment,
					mainG54.First().StartPoint.Master,
					mainG54.First().StartPoint.Slave );
				pathG54PostData.LeadInPostPath.AddRange( leadInPostData );
				pathMCSPostData.LeadInPostPath.AddRange( leadInPostData );

				// re: 如果 leadin 是後算，那應該就不需要 bStart，有進這裡再更新
				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPath.First().StartPoint;
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.LeadInPostPath.First().StartPoint;
				bStart = true;
			}

			// lead-out with fixed rotation based on main path end
			if( pathNCPacke.LeadOutSegment.Count > 0 ) {
				List<ISegmentPostData> leadOutPostData = BuildPostSegmentsWithFixedRotation(
					pathNCPacke.LeadOutSegment,
					mainG54.Last().EndPoint.Master,
					mainG54.Last().EndPoint.Slave );
				pathG54PostData.LeadOutPostPath.AddRange( leadOutPostData );
				pathMCSPostData.LeadOutPostPath.AddRange( leadOutPostData );
			}

			// set process start point if no lead-in
			if( !bStart ) {
				pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPath.First().StartPoint;
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.MainPathPostPath.First().StartPoint;
			}

			// calculate entry/traverse
			if( endInfoOfPreviousPath == null ) {
				CalculateEntry( pathNCPacke.ProcessStartPnt, pathNCPacke.EntryDistance, pathNCPacke.Entry_FollowSafeDistance, ref pathG54PostData, ref pathMCSPostData );
			}
			else {
				CalculateTraverse( endInfoOfPreviousPath, pathNCPacke.ProcessStartPnt, pathNCPacke.LiftUpDistance, pathNCPacke.FrogLeapDistance, pathNCPacke.CutDownDistance, pathNCPacke.FollowSafeDistance, ref pathG54PostData, ref pathMCSPostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = pathNCPacke.ProcessEndPnt,
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};
			return true;
		}

		static bool SolvePathNormalCase( PostSolver postSolver, PathNCPackage pathNCPack,
			PathEndInfo endInfoOfPreviousPath, out PathPostData pathG54PostData,
			out PathPostData pathMCSPostData, out PathEndInfo currentPathtEndInfo,
			ref double dLastPointProcess_M, ref double dLastPointProcess_S )
		{
			pathG54PostData = new PathPostData();
			pathMCSPostData = new PathPostData();
			currentPathtEndInfo = new PathEndInfo();
			bool bStart = false;

			// lead-in
			if( pathNCPack.LeadInSegment.Count > 0 ) {
				if( !SolveProcessPathIK( postSolver, pathNCPack.LeadInSegment,
					ref dLastPointProcess_M, ref dLastPointProcess_S,
					out List<ISegmentPostData> leadInG54 ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPath.AddRange( leadInG54 );
				pathMCSPostData.LeadInPostPath.AddRange( leadInG54 );

				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPath.First().StartPoint;
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.LeadInPostPath.First().StartPoint;
				bStart = true;
			}

			// main path
			if( !SolveProcessPathIK( postSolver, pathNCPack.MainPathSegment,
				ref dLastPointProcess_M, ref dLastPointProcess_S,
				out List<ISegmentPostData> mainG54 ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPath.AddRange( mainG54 );
			pathMCSPostData.MainPathPostPath.AddRange( mainG54 );

			if( !bStart ) {
				pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPath.First().StartPoint;
				pathMCSPostData.ProcessStartPoint = pathMCSPostData.MainPathPostPath.First().StartPoint;
			}

			// over-cut
			if( pathNCPack.OverCutSegment.Count > 0 ) {
				if( !SolveProcessPathIK( postSolver, pathNCPack.OverCutSegment,
					ref dLastPointProcess_M, ref dLastPointProcess_S,
					out List<ISegmentPostData> overCutG54 ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPath.AddRange( overCutG54 );
				pathMCSPostData.OverCutPostPath.AddRange( overCutG54 );
			}

			// lead-out
			if( pathNCPack.LeadOutSegment.Count > 0 ) {
				if( !SolveProcessPathIK( postSolver, pathNCPack.LeadOutSegment,
					ref dLastPointProcess_M, ref dLastPointProcess_S,
					out List<ISegmentPostData> leadOutG54 ) ) {
					return false;
				}
				pathG54PostData.LeadOutPostPath.AddRange( leadOutG54 );
				pathMCSPostData.LeadOutPostPath.AddRange( leadOutG54 );
			}

			// calculate entry/traverse
			if( endInfoOfPreviousPath == null ) {
				CalculateEntry( pathNCPack.ProcessStartPnt, pathNCPack.EntryDistance, pathNCPack.Entry_FollowSafeDistance, ref pathG54PostData, ref pathMCSPostData );
			}
			else {
				CalculateTraverse( endInfoOfPreviousPath, pathNCPack.ProcessStartPnt, pathNCPack.LiftUpDistance, pathNCPack.FrogLeapDistance, pathNCPack.CutDownDistance, pathNCPack.FollowSafeDistance, ref pathG54PostData, ref pathMCSPostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = pathNCPack.ProcessEndPnt,
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

		public static List<PostPoint> GetConcatenatedPostList( PathPostData postData )
		{
			List<PostPoint> result = new List<PostPoint>();
			if( postData == null ) {
				return result;
			}
			if( postData.LeadInPostPath.Count > 0 ) {
				foreach( ISegmentPostData path in postData.LeadInPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			if( postData.MainPathPostPath.Count > 0 ) {
				foreach( ISegmentPostData path in postData.MainPathPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			if( postData.OverCutPostPath.Count > 0 ) {
				foreach( ISegmentPostData path in postData.OverCutPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			if( postData.LeadOutPostPath.Count > 0 ) {
				foreach( ISegmentPostData path in postData.LeadOutPostPath ) {
					result.AddRange( path.GetPostPointList() );
				}
			}
			return result;
		}

		static List<ISegmentPostData> BuildPostSegmentsWithFixedRotation( List<ICAMSegmentElement> camSegmentList, double dMaster, double dSlave )
		{
			List<ISegmentPostData> segmentPostData = new List<ISegmentPostData>();
			foreach( ICAMSegmentElement camSegment in camSegmentList ) {
				List<CAMPoint2> pointList = camSegment.CAMPointList;
				List<PostPoint> postPointList = new List<PostPoint>();
				foreach( CAMPoint2 camPoint in pointList ) {
					PostPoint postList = new PostPoint()
					{
						X = camPoint.Point.X(),
						Y = camPoint.Point.Y(),
						Z = camPoint.Point.Z(),
						Master = dMaster,
						Slave = dSlave,
						IsInSigularRabge = false
					};
					postPointList.Add( postList );
				}
				ISegmentPostData segment = CreateSegmentPostData( camSegment, postPointList );
				segmentPostData.Add( segment );
			}
			return segmentPostData;
		}

		// re: 這個命名需要雕琢一下，他不僅建構了 PostPoint，還做了 IK 求解
		static List<PostPoint> CreateSegmentPostPoint( PostSolver postSolver, List<CAMPoint2> pointList,
										   ref double dLastProcessPathM, ref double dLastProcessPathS, bool isModify )
		{
			List<PostPoint> postPointList = new List<PostPoint>();
			for( int i = 0; i < pointList.Count; i++ ) {
				CAMPoint2 camPoint = pointList[ i ];
				IKSolveResult ikResult = postSolver.SolveIK( camPoint.ToolVec, dLastProcessPathM, dLastProcessPathS,
															out dLastProcessPathM, out dLastProcessPathS );

				if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution ) {
					return null;
				}
				bool isRealSingularPnt = IsRealSingular( ikResult, isModify, i == pointList.Count - 1 );
				PostPoint postPoint = new PostPoint()
				{
					X = camPoint.Point.X(),
					Y = camPoint.Point.Y(),
					Z = camPoint.Point.Z(),
					Master = dLastProcessPathM,
					Slave = dLastProcessPathS,
					IsInSigularRabge = isRealSingularPnt
				};
				postPointList.Add( postPoint );
			}
			return postPointList;
		}

		static ISegmentPostData CreateSegmentPostData( ICAMSegmentElement camSegment, List<PostPoint> postPointList )
		{
			bool isModified = camSegment.IsModify;

			if( camSegment is ArcCAMSegment ) {
				if( postPointList.Count < 3 ) {
					return null;
				}
				return new ArcPost( postPointList, isModified );
			}
			else if( camSegment is LineCAMSegment ) {
				if( postPointList.Count < 2 ) {
					return null;
				}
				return new LinePost( postPointList, isModified );
			}
			return null;
		}

		static bool IsRealSingular( IKSolveResult ikResult, bool isModify, bool isLastPntOfSegment )
		{
			// re: 這邊可能要註解一下，不然很難理解
			bool isSingular = ( ikResult == IKSolveResult.MasterInfinityOfSolution ||
								  ikResult == IKSolveResult.SlaveInfinityOfSolution );
			if( isSingular ) {
				if( isModify && isLastPntOfSegment ) {
					return false;
				}
				return true;
			}
			return false;
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

		static void CalculateTraverse( PathEndInfo endInfoOfPreviousPath, CAMPoint ProcessStartPoint, double dLiftUpDistance, double dFrogLeapDistance, double CutDownDistance, double FollowSafeDistance, ref PathPostData pathG54PostData, ref PathPostData pathMCSPostData )
		{
			if( ProcessStartPoint == null || endInfoOfPreviousPath == null ) {
				return;
			}

			// p1: end of previous path (not used here)
			// p2: lift up point of previous path
			// p3: frog leap middle point (if frog leap)
			// p4: cut down point of current path
			// p5: start of current path (not used here)
			CAMPoint p1 = endInfoOfPreviousPath.EndCAMPoint;
			CAMPoint p2 = TraverseHelper.GetCutDownOrLiftUpPoint( endInfoOfPreviousPath.EndCAMPoint, dLiftUpDistance );
			CAMPoint p4 = TraverseHelper.GetCutDownOrLiftUpPoint( ProcessStartPoint, CutDownDistance );
			CAMPoint p5 = ProcessStartPoint;

			// lift up
			if( dLiftUpDistance > 0 && p2 != null ) {

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
			if( dFrogLeapDistance > 0 && p2 != null && p4 != null ) {
				CAMPoint p3 = TraverseHelper.GetFrogLeapMiddlePoint( p2, p4, dFrogLeapDistance );

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
			if( CutDownDistance > 0 && p4 != null ) {

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
			pathG54PostData.FollowSafeDistance = FollowSafeDistance;
			pathMCSPostData.FollowSafeDistance = FollowSafeDistance;
		}

		static void CalculateEntry( CAMPoint entryCAM, double dEntryDistance, double dFollowSafeDistance, ref PathPostData pathG54PostData, ref PathPostData pathMCSPostData )
		{
			if( entryCAM == null ) {
				return;
			}
			if( dEntryDistance <= 0 ) {

				// just set follow safe distance
				pathG54PostData.FollowSafeDistance = dFollowSafeDistance;
				pathMCSPostData.FollowSafeDistance = dFollowSafeDistance;
				return;
			}
			CAMPoint entryPoint = TraverseHelper.GetCutDownOrLiftUpPoint( entryCAM, dEntryDistance );
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
			pathG54PostData.FollowSafeDistance = dFollowSafeDistance;

			// MCS
			pathMCSPostData.CutDownPostPoint = new PostPoint()
			{
				X = entryPoint.CADPoint.Point.X(), // TODO: need to be changed to MCS point
				Y = entryPoint.CADPoint.Point.Y(),
				Z = entryPoint.CADPoint.Point.Z(),
				Master = pathMCSPostData.ProcessStartPoint.Master,
				Slave = pathMCSPostData.ProcessStartPoint.Slave
			};
			pathMCSPostData.FollowSafeDistance = dFollowSafeDistance;
		}

		#region AC Solution

		static bool SolvePath_ACSolution( PostSolver postSolver, List<ICAMSegmentElement> segmentList,
							ref double dLastProcessPathM, ref double dLastProcessPathS, out List<ISegmentPostData> segmentPostData )
		{
			bool isSolveDone = SolveProcessPathIK( postSolver, segmentList,
							ref dLastProcessPathM, ref dLastProcessPathS,
							out segmentPostData );
			if( isSolveDone == false ) {
				return false;
			}
			bool isAdjustDone = AdjustSingular( ref segmentPostData );
			if( isAdjustDone == false ) {
				return false;
			}
			return true;
		}

		static bool SolveProcessPathIK( PostSolver postSolver, List<ICAMSegmentElement> segmentList,
							ref double dLastProcessPathM, ref double dLastProcessPathS,
							out List<ISegmentPostData> segmentPostData )
		{
			segmentPostData = new List<ISegmentPostData>();
			if( segmentList == null || segmentList.Count == 0 ) {
				return false;
			}
			foreach( ICAMSegmentElement camSegment in segmentList ) {
				List<CAMPoint2> pointList = camSegment.CAMPointList;

				// create post points form each cam point
				List<PostPoint> postPointList = CreateSegmentPostPoint( postSolver, pointList, ref dLastProcessPathM, ref dLastProcessPathS, camSegment.IsModify );

				// re: 這邊應該也走 return bool，out 計算結果，比較一致
				if( postPointList == null ) {
					return false;
				}
				ISegmentPostData segment = CreateSegmentPostData( camSegment, postPointList );

				// re: 這邊應該也走 return bool，out 計算結果，比較一致；然後這邊如果 null 應該要直接 return false?
				if( segment != null ) {
					segmentPostData.Add( segment );
				}
			}
			return true;
		}

		// re: 這個運算是否有考慮全都在奇異點的情境?
		static bool AdjustSingular( ref List<ISegmentPostData> segmentPostList )
		{
			if( segmentPostList == null || segmentPostList.Count == 0 ) {
				return false;
			}
			((List<PostPoint> allPoints, List<List<(int segmentIndex, int pointIndex)>> pointToSegmentMapping) globalPointInfo, bool isClosedPath) = BuildPointMap( segmentPostList );
			if( globalPointInfo.allPoints.Count == 0 ) {
				return false;
			}
			List<(int regionStart, int regionEnd, bool isWrappedArea)> singularRegions = FindSingularRegions( globalPointInfo.allPoints, isClosedPath );

			// AC interpolation for each singular region
			foreach( var region in singularRegions ) {
				InterpolateSingularRegion( globalPointInfo, region, isClosedPath );
			}

			// synch back to original segments
			SynBackToOriginalSegments( ref segmentPostList, globalPointInfo );
			return true;
		}

		static ((List<PostPoint> allPoints, List<List<(int segmentIndex, int pointIndex)>> pointToSegmentMapping), bool isClosedPath) BuildPointMap( List<ISegmentPostData> segmentPostList )
		{
			List<PostPoint> allPoints = new List<PostPoint>();
			List<List<(int segmentIndex, int pointIndex)>> pointToSegmentMapping = new List<List<(int segmentIndex, int pointIndex)>>();
			bool isClosedPath = IsClosedPathPreCheck( segmentPostList );

			for( int segIdx = 0; segIdx < segmentPostList.Count; segIdx++ ) {
				ISegmentPostData segment = segmentPostList[ segIdx ];
				List<PostPoint> postPointList = segment.GetPostPointList();

				for( int ptIdx = 0; ptIdx < postPointList.Count; ptIdx++ ) {
					PostPoint currentPoint = postPointList[ ptIdx ];
					bool isRepeatPnt = false;
					int nRepeatIdx = -1;

					// beside segment
					if( segIdx > 0 && ptIdx == 0 ) {
						isRepeatPnt = true;
						nRepeatIdx = allPoints.Count - 1;
					}

					// close path first segment first point
					else if( isClosedPath && segIdx == 0 && ptIdx == 0 && allPoints.Count > 0 ) {
						isRepeatPnt = true;
						nRepeatIdx = allPoints.Count - 1;
					}

					if( isRepeatPnt ) {
						// dulicate point: update mapping
						pointToSegmentMapping[ nRepeatIdx ].Add( (segIdx, ptIdx) );
					}
					else {
						// not duplicate point: add new point and mapping
						allPoints.Add( currentPoint.Clone() );
						var newMapping = new List<(int segmentIndex, int pointIndex)> { (segIdx, ptIdx) };
						pointToSegmentMapping.Add( newMapping );
					}
				}
			}

			return ((allPoints, pointToSegmentMapping), isClosedPath);
		}

		static bool IsClosedPathPreCheck( List<ISegmentPostData> segmentPostList )
		{
			if( segmentPostList == null || segmentPostList.Count == 0 ) {
				return false;
			}

			var firstSegment = segmentPostList[ 0 ].GetPostPointList();
			var lastSegment = segmentPostList[ segmentPostList.Count - 1 ].GetPostPointList();

			if( firstSegment.Count == 0 || lastSegment.Count == 0 ) {
				return false;
			}

			var firstPoint = firstSegment[ 0 ];
			var lastPoint = lastSegment[ lastSegment.Count - 1 ];

			double distance = Math.Sqrt(
				Math.Pow( lastPoint.X - firstPoint.X, 2 ) +
				Math.Pow( lastPoint.Y - firstPoint.Y, 2 ) +
				Math.Pow( lastPoint.Z - firstPoint.Z, 2 )
			);

			return distance < 1e-6; // to do
		}

		static List<(int start, int end, bool isWrappedArea)> FindSingularRegions( List<PostPoint> allPoints, bool isClosedPath )
		{
			var regions = new List<(int start, int end, bool isWrapped)>();
			int start = -1;

			// find all singular regions
			for( int i = 0; i < allPoints.Count; i++ ) {
				bool isSingular = allPoints[ i ].IsInSigularRabge;

				// this singular area start
				if( isSingular && start == -1 ) {
					start = i;
				}

				// this singular area end
				else if( !isSingular && start != -1 ) {
					regions.Add( (start, i - 1, false) );
					start = -1;
				}
			}

			// path end is singular
			if( start != -1 ) {
				regions.Add( (start, allPoints.Count - 1, false) );
			}

			// close path special case: check if first and last regions can be merged
			if( isClosedPath && regions.Count >= 2 ) {
				var firstRegion = regions[ 0 ];
				var lastRegion = regions[ regions.Count - 1 ];

				// first region starts at index 0 and last region ends at last index
				if( firstRegion.start == 0 && lastRegion.end == allPoints.Count - 1 ) {
					regions.RemoveAt( regions.Count - 1 );
					regions.RemoveAt( 0 );

					// readd the merged region
					regions.Insert( 0, (lastRegion.start, firstRegion.end, true) );
				}
			}
			return regions;
		}

		static void InterpolateSingularRegion(
			(List<PostPoint> allPoints, List<List<(int segmentIndex, int pointIndex)>> pointToSegmentMapping) globalPointInfo,
			(int start, int end, bool isWrapped) region,
			bool isClosedPath )
		{
			List<PostPoint> allPoints = globalPointInfo.allPoints;
			int singularStart = region.start;
			int singularEnd = region.end;
			bool isWrappedRegion = region.isWrapped;

			// get entry and exit angles
			(double entryMaster, double entrySlave) = GetEntryAngle( allPoints, singularStart, singularEnd, isWrappedRegion, isClosedPath );
			(double exitMaster, double exitSlave) = GetExitAngle( allPoints, singularStart, singularEnd, isWrappedRegion, isClosedPath );

			if( isWrappedRegion ) {
				// handle wrapped singular region
				InterpolateWrappedSingularRegion( allPoints, singularStart, singularEnd, entryMaster, entrySlave, exitMaster, exitSlave );
			}
			else {
				InterpolateNormalSingularRegion( allPoints, singularStart, singularEnd, entryMaster, entrySlave, exitMaster, exitSlave );
			}
		}

		static void SynBackToOriginalSegments( ref List<ISegmentPostData> segmentPostList,
			(List<PostPoint> allPoints, List<List<(int segmentIndex, int pointIndex)>> pointToSegmentMapping) globalPointInfo )
		{
			List<PostPoint> allPoints = globalPointInfo.allPoints;
			List<List<(int segmentIndex, int pointIndex)>> pointToSegmentMapping = globalPointInfo.pointToSegmentMapping;

			for( int globalIdx = 0; globalIdx < allPoints.Count; globalIdx++ ) {
				PostPoint interpolatedPoint = allPoints[ globalIdx ];
				var mappingList = pointToSegmentMapping[ globalIdx ];

				// find original points and update
				foreach( var (segmentIndex, pointIndex) in mappingList ) {
					ISegmentPostData segment = segmentPostList[ segmentIndex ];
					List<PostPoint> originalPointList = segment.GetPostPointList();

					// change ori interpolated 
					originalPointList[ pointIndex ].Master = interpolatedPoint.Master;
					originalPointList[ pointIndex ].Slave = interpolatedPoint.Slave;
				}
			}
		}

		static void InterpolateNormalSingularRegion( List<PostPoint> allPoints, int singularStart, int singularEnd,
			double entryMaster, double entrySlave, double exitMaster, double exitSlave )
		{
			var distances = CalSingularDistances( allPoints, singularStart, singularEnd, out double dTotalDistance );

			// use ac interpolate
			for( int j = singularStart; j <= singularEnd; j++ ) {
				double t = dTotalDistance > 0 ? distances[ j - singularStart ] / dTotalDistance : 0;

				double interpolatedMaster = entryMaster + ( exitMaster - entryMaster ) * t;
				double interpolatedSlave = entrySlave + ( exitSlave - entrySlave ) * t;

				// assign new value
				allPoints[ j ].Master = interpolatedMaster;
				allPoints[ j ].Slave = interpolatedSlave;
			}
		}

		static void InterpolateWrappedSingularRegion( List<PostPoint> allPoints, int singularStart, int singularEnd,
	double entryMaster, double entrySlave, double exitMaster, double exitSlave )
		{
			List<PostPoint> wrappedPoints = new List<PostPoint>();
			List<int> wrappedIndices = new List<int>();
			for( int i = singularStart; i < allPoints.Count; i++ ) {
				wrappedPoints.Add( allPoints[ i ] );
				wrappedIndices.Add( i );
			}
			for( int i = 0; i <= singularEnd; i++ ) {
				wrappedPoints.Add( allPoints[ i ] );
				wrappedIndices.Add( i );
			}
			List<double> distanceList = CalSingularDistancesAtWrappedArea( allPoints, wrappedPoints, singularStart, singularEnd, out double dTotalDistance );

			// AC interpolate
			for( int j = 0; j < wrappedPoints.Count; j++ ) {
				double t = dTotalDistance > 0 ? distanceList[ j ] / dTotalDistance : 0;

				double interpolatedMaster = entryMaster + ( exitMaster - entryMaster ) * t;
				double interpolatedSlave = entrySlave + ( exitSlave - entrySlave ) * t;

				// asssign new value
				int originalIndex = wrappedIndices[ j ];
				allPoints[ originalIndex ].Master = interpolatedMaster;
				allPoints[ originalIndex ].Slave = interpolatedSlave;
			}
		}

		static (double master, double slave) GetEntryAngle( List<PostPoint> allPoints, int singularStart, int singularEnd, bool isWrapped, bool isClosedPath )
		{
			if( isWrapped ) {
				// 跨起點區域：入口角度是 singularStart 的前一個點
				if( singularStart > 0 ) {
					var entryPoint = allPoints[ singularStart - 1 ];
					return (entryPoint.Master, entryPoint.Slave);
				}
				else {
					// 如果 singularStart 是 0，在封閉路徑中前一個點是最後一個點
					if( isClosedPath && allPoints.Count > 1 ) {
						var entryPoint = allPoints[ allPoints.Count - 1 ];
						return (entryPoint.Master, entryPoint.Slave);
					}
				}
			}
			else {
				// 普通區域：正常處理
				if( singularStart > 0 ) {
					var entryPoint = allPoints[ singularStart - 1 ];
					return (entryPoint.Master, entryPoint.Slave);
				}
			}

			// 預設值
			return (0.0, 0.0);
		}

		static (double master, double slave) GetExitAngle( List<PostPoint> allPoints, int singularStart, int singularEnd, bool isWrapped, bool isClosedPath )
		{
			if( isWrapped ) {
				// 跨起點區域：出口角度是 singularEnd 的下一個點
				if( singularEnd < allPoints.Count - 1 ) {
					var exitPoint = allPoints[ singularEnd + 1 ];
					return (exitPoint.Master, exitPoint.Slave);
				}
				else {
					// 如果 singularEnd 是最後一個點，在封閉路徑中下一個點是第一個點
					if( isClosedPath && allPoints.Count > 1 ) {
						var exitPoint = allPoints[ 0 ];
						return (exitPoint.Master, exitPoint.Slave);
					}
				}
			}
			else {
				// 普通區域：正常處理
				if( singularEnd < allPoints.Count - 1 ) {
					var exitPoint = allPoints[ singularEnd + 1 ];
					return (exitPoint.Master, exitPoint.Slave);
				}
			}

			// 預設值
			return (0.0, 0.0);
		}

		static List<double> CalSingularDistances( List<PostPoint> allPoints, int singularStart, int singularEnd, out double dTotalDistance )
		{
			List<double> distances = new List<double>();
			dTotalDistance = 0.0;

			// first segment distance: from entry point to first singular point
			if( singularStart > 0 ) {
				PostPoint entryPoint = allPoints[ singularStart - 1 ];
				PostPoint firstSingularPoint = allPoints[ singularStart ];
				double entryDistance = Math.Sqrt(
					Math.Pow( firstSingularPoint.X - entryPoint.X, 2 ) +
					Math.Pow( firstSingularPoint.Y - entryPoint.Y, 2 ) +
					Math.Pow( firstSingularPoint.Z - entryPoint.Z, 2 )
				);
				dTotalDistance += entryDistance;
			}
			distances.Add( dTotalDistance );
			for( int j = singularStart; j < singularEnd; j++ ) {
				PostPoint p1 = allPoints[ j ];
				PostPoint p2 = allPoints[ j + 1 ];

				double dist = Math.Sqrt(
					Math.Pow( p2.X - p1.X, 2 ) +
					Math.Pow( p2.Y - p1.Y, 2 ) +
					Math.Pow( p2.Z - p1.Z, 2 )
				);
				dTotalDistance += dist;
				distances.Add( dTotalDistance );
			}

			// last segment distance: from last singular point to exit point
			if( singularEnd < allPoints.Count - 1 ) {
				PostPoint lastSingularPoint = allPoints[ singularEnd ];
				PostPoint exitPoint = allPoints[ singularEnd + 1 ];
				double exitDistance = Math.Sqrt(
					Math.Pow( exitPoint.X - lastSingularPoint.X, 2 ) +
					Math.Pow( exitPoint.Y - lastSingularPoint.Y, 2 ) +
					Math.Pow( exitPoint.Z - lastSingularPoint.Z, 2 )
				);
				dTotalDistance += exitDistance;
			}

			return distances;
		}

		static List<double> CalSingularDistancesAtWrappedArea( List<PostPoint> allPoints, List<PostPoint> wrappedPoints, int singularStart, int singularEnd, out double totalDistance )
		{
			List<double> distances = new List<double>();
			totalDistance = 0.0;

			// first distance: from entry point to first singular point
			if( singularStart > 0 ) {
				PostPoint entryPoint = allPoints[ singularStart - 1 ];
				PostPoint firstSingularPoint = wrappedPoints[ 0 ];
				double entryDistance = Math.Sqrt(
					Math.Pow( firstSingularPoint.X - entryPoint.X, 2 ) +
					Math.Pow( firstSingularPoint.Y - entryPoint.Y, 2 ) +
					Math.Pow( firstSingularPoint.Z - entryPoint.Z, 2 )
				);
				totalDistance += entryDistance;
			}
			distances.Add( totalDistance );

			// cal point in singular area each distance
			for( int j = 0; j < wrappedPoints.Count - 1; j++ ) {
				PostPoint p1 = wrappedPoints[ j ];
				PostPoint p2 = wrappedPoints[ j + 1 ];

				double dist = Math.Sqrt(
					Math.Pow( p2.X - p1.X, 2 ) +
					Math.Pow( p2.Y - p1.Y, 2 ) +
					Math.Pow( p2.Z - p1.Z, 2 )
				);

				totalDistance += dist;
				distances.Add( totalDistance );
			}

			// last distance: from last singular point to exit point
			if( singularEnd < allPoints.Count - 1 ) {
				PostPoint lastSingularPoint = wrappedPoints[ wrappedPoints.Count - 1 ];
				PostPoint exitPoint = allPoints[ singularEnd + 1 ];
				double exitDistance = Math.Sqrt(
					Math.Pow( exitPoint.X - lastSingularPoint.X, 2 ) +
					Math.Pow( exitPoint.Y - lastSingularPoint.Y, 2 ) +
					Math.Pow( exitPoint.Z - lastSingularPoint.Z, 2 )
				);
				totalDistance += exitDistance;
			}

			return distances;
		}

		#endregion
	}
}
