using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class PathEndInfo
	{
		public IProcessPoint EndCAMPoint
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

	internal class ContourNCPackage : INCPackage
	{
		public ContourNCPackage( LeadData leadData, double overCutLength,
			IReadOnlyList<IProcessPoint> camPointList,
			IReadOnlyList<IProcessPoint> leadInCAMPointList,
			IReadOnlyList<IProcessPoint> leadOutCAMPointList,
			IReadOnlyList<IProcessPoint> overCutCAMPointList,
			TraverseData traverseData,
			IProcessPoint processStartPoint,
			IProcessPoint processEndPoint
			)
		{
			CAMPointList = camPointList;
			LeadInCAMPointList = leadInCAMPointList;
			LeadOutCAMPointList = leadOutCAMPointList;
			OverCutCAMPointList = overCutCAMPointList;
			TraverseData = traverseData;
			ProcessStartPoint = processStartPoint;
			ProcessEndPoint = processEndPoint;
		}

		public IReadOnlyList<IProcessPoint> CAMPointList
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

		public IReadOnlyList<IProcessPoint> OverCutCAMPointList
		{
			get;
		}

		public TraverseData TraverseData
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

	internal static class ContourPostHelper
	{
		public static bool  GetPostData( ContourNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out PostData pathG54PostData, out PathEndInfo currentPathEndInfo )
		{
			// for write NC file
			pathG54PostData = new PostData();

			// to make solution continuous
			currentPathEndInfo = new PathEndInfo();
			if( currentPathNCPack == null || entryAndExitData == null ) {
				return false;
			}

			// main path
			if( !ConvertCAMPntListToPostPntList( currentPathNCPack.CAMPointList,
				out List<PostPoint> mainG54 ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPointList.AddRange( mainG54 );
			pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPointList[ 0 ];

			// over-cut
			if( currentPathNCPack.OverCutCAMPointList.Count > 0 ) {
				if( !ConvertCAMPntListToPostPntList( currentPathNCPack.OverCutCAMPointList,
					out List<PostPoint> overCutG54 ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPointList.AddRange( overCutG54 );
			}

			// lead-in
			if( currentPathNCPack.LeadInCAMPointList.Count > 0 ) {
				if( !ConvertCAMPntListToPostPntList( currentPathNCPack.LeadInCAMPointList,
					out List<PostPoint> leadInG54 ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPointList.AddRange( leadInG54 );

				// update process start point
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
				PostTraverseBuilder.CalculateTraverse( endInfoOfPreviousPath, currentPathNCPack, pathG54PostData );
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

		#region Private methods

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
					TiltAngle_deg = pointList[ i ].TiltAngle_deg,
					IsMicroJointEnd = pointList[ i ].IsMicroJointEnd,
					IsMicroJointStart = pointList[ i ].IsMicroJointStart
				};
				resultG54.Add( frameDataG54 );
			}
			return true;
		}

		#endregion
	}
}
