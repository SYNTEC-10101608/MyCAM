using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class FlyCutNCPackage : INCPackage
	{
		public FlyCutNCPackage(
			IReadOnlyList<FlyCutGroupPathCache.IFlyCutStepCache> flyCutSteps,
			IReadOnlyList<IProcessPoint> leadInCAMPointList,
			IReadOnlyList<IProcessPoint> leadOutCAMPointList,
			TraverseData traverseData,
			IProcessPoint processStartPoint,
			IProcessPoint processEndPoint
			)
		{
			FlyCutSteps = flyCutSteps;
			LeadInCAMPointList = leadInCAMPointList;
			LeadOutCAMPointList = leadOutCAMPointList;
			TraverseData = traverseData;
			ProcessStartPoint = processStartPoint;
			ProcessEndPoint = processEndPoint;
		}

		public IReadOnlyList<FlyCutGroupPathCache.IFlyCutStepCache> FlyCutSteps
		{
			get;
		}

		public TraverseData TraverseData
		{
			get;
		}

		public IReadOnlyList<IProcessPoint> LeadInCAMPointList
		{
			get;
		}

		public IReadOnlyList<IProcessPoint> LeadOutCAMPointList
		{
			get;
		}

		public IProcessPoint ProcessStartPoint
		{
			get; private set;
		}

		public IProcessPoint ProcessEndPoint
		{
			get; private set;
		}
	}

	internal static class FlyCutPostHelper
	{
		public static bool GetPostData( FlyCutNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData, TraverseCalculationInfo traverseCalculationInfo,
			out FlyCutPostData pathG54PostData, out PathEndInfo currentPathEndInfo )
		{
			// for write NC file
			pathG54PostData = new FlyCutPostData();

			// to make solution continuous
			currentPathEndInfo = new PathEndInfo();
			if( currentPathNCPack == null || entryAndExitData == null ) {
				return false;
			}

			// flycut step list should not be less than 3
			// first cut step exclude the last point
			IReadOnlyList<IProcessPoint> firstPointList = currentPathNCPack.FlyCutSteps[ 0 ].CAMPointList
				.GetRange( 0, currentPathNCPack.FlyCutSteps[ 0 ].CAMPointList.Count - 1 );
			if( !ConvertCAMPntListToPostPntList( firstPointList, out List<PostPoint> firstMainG54 ) ) {
				return false;
			}
			pathG54PostData.StepPostPointList.Add( firstMainG54 );

			// middle cut steps exclude the first and last point
			for( int i = 1; i < currentPathNCPack.FlyCutSteps.Count - 1; ++i ) {
				IReadOnlyList<IProcessPoint> pointList = ( i % 2 == 0 )
					? currentPathNCPack.FlyCutSteps[ i ].CAMPointList.GetRange( 1, currentPathNCPack.FlyCutSteps[ i ].CAMPointList.Count - 2 )
					: currentPathNCPack.FlyCutSteps[ i ].CAMPointList;

				if( !ConvertCAMPntListToPostPntList( pointList, out List<PostPoint> mainG54 ) ) {
					return false;
				}
				pathG54PostData.StepPostPointList.Add( mainG54 );
			}

			// last cut step exclude the first point
			FlyCutGroupPathCache.IFlyCutStepCache lastCutStep = currentPathNCPack.FlyCutSteps[ currentPathNCPack.FlyCutSteps.Count - 1 ];
			IReadOnlyList<IProcessPoint> lastPointList = lastCutStep.CAMPointList
				.GetRange( 1, lastCutStep.CAMPointList.Count - 1 );
			if( !ConvertCAMPntListToPostPntList( lastPointList, out List<PostPoint> LastMainG54 ) ) {
				return false;
			}
			pathG54PostData.StepPostPointList.Add( LastMainG54 );


			pathG54PostData.ProcessStartPoint = pathG54PostData.StepPostPointList[ 0 ][ 0 ];

			// over-cut
			//if( currentPathNCPack.OverCutCAMPointList.Count > 0 ) {
			//	if( !ConvertCAMPntListToPostPntList( currentPathNCPack.OverCutCAMPointList,
			//		out List<PostPoint> overCutG54 ) ) {
			//		return false;
			//	}
			//	pathG54PostData.OverCutPostPointList.AddRange( overCutG54 );
			//}

			// lead-in
			if( currentPathNCPack.LeadInCAMPointList.Count > 0 ) {
				if( !ConvertCAMPntListToPostPntList( currentPathNCPack.LeadInCAMPointList,
					out List<PostPoint> leadInG54 ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPointList.AddRange( leadInG54 );
				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPointList[ 0 ];
			}

			// lead-out
			if( currentPathNCPack.LeadOutCAMPointList.Count > 0 ) {
				if( !ConvertCAMPntListToPostPntList( currentPathNCPack.LeadOutCAMPointList,
					out List<PostPoint> leadOutG54 ) ) {
					return false;
				}
				pathG54PostData.LeadOutPostPointList.AddRange( leadOutG54 );
			}

			// should be the first path
			if( endInfoOfPreviousPath == null ) {

				// the traverse of first path is entry
				PostTraverseBuilder.CalculateEntry( currentPathNCPack, entryAndExitData, pathG54PostData );
			}
			else {

				// traverse from previous path to current path
				PostTraverseBuilder.CalculateTraverse( endInfoOfPreviousPath, currentPathNCPack, pathG54PostData, traverseCalculationInfo );
			}

			// end info of current path
			currentPathEndInfo = new PathEndInfo()
			{
				EndCAMPoint = currentPathNCPack.ProcessEndPoint,
				Master = currentPathNCPack.ProcessEndPoint.ModMaster_rad,
				Slave = currentPathNCPack.ProcessEndPoint.ModSlave_rad,
			};
			return true;
		}

		static bool ConvertCAMPntListToPostPntList( IReadOnlyList<IProcessPoint> pointList,
			out List<PostPoint> resultG54 )
		{
			resultG54 = new List<PostPoint>();
			if( pointList == null || pointList.Count == 0 ) {
				return false;
			}

			// get rotate angles from point
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			foreach( IProcessPoint point in pointList ) {
				rotateAngleList.Add( new Tuple<double, double>( point.ModMaster_rad, point.ModSlave_rad ) );
			}

			// build post data
			for( int i = 0; i < pointList.Count; i++ ) {
				gp_Pnt pointG54 = pointList[ i ].Point;

				// add G54 frame data
				PostPoint frameDataG54 = new PostPoint()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2,
					IsMicroJointEnd = pointList[ i ].IsMicroJointEnd,
					IsMicroJointStart = pointList[ i ].IsMicroJointStart
				};
				resultG54.Add( frameDataG54 );
			}
			return true;
		}
	}
}
