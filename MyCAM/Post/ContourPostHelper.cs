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
			LeadData = leadData;
			OverCutLength = overCutLength;
			CAMPointList = camPointList;
			LeadInCAMPointList = leadInCAMPointList;
			LeadOutCAMPointList = leadOutCAMPointList;
			OverCutCAMPointList = overCutCAMPointList;
			TraverseData = traverseData;
			ProcessStartPoint = processStartPoint;
			ProcessEndPoint = processEndPoint;
		}

		public LeadData LeadData
		{
			get;
		}

		public double OverCutLength
		{
			get;
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
		public static bool SolvePath( PostSolver postSolver, ContourNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out PostData pathG54PostData, out PathEndInfo currentPathtEndInfo )
		{
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

			// main path
			if( !SolveProcessPath( postSolver, currentPathNCPack.CAMPointList,
				out List<PostPoint> mainG54,
				ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPointList.AddRange( mainG54 );
			pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPointList[ 0 ];

			// over-cut
			if( currentPathNCPack.OverCutLength != 0 && currentPathNCPack.OverCutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentPathNCPack.OverCutCAMPointList,
					out List<PostPoint> overCutG54,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPointList.AddRange( overCutG54 );
			}

			// lead-in
			if( currentPathNCPack.LeadInCAMPointList.Count > 0 ) {
				if( pathG54PostData.MainPathPostPointList.Count == 0 ) {
					return false;
				}
				double startM = pathG54PostData.MainPathPostPointList[ 0 ].Master;
				double startS = pathG54PostData.MainPathPostPointList[ 0 ].Slave;
				if( !BuildProcessPath( currentPathNCPack.LeadInCAMPointList, startM, startS,
					out List<PostPoint> leadInG54 ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPointList.AddRange( leadInG54 );

				// update process start point
				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPointList[ 0 ];
			}

			// lead-out
			if( currentPathNCPack.LeadOutCAMPointList.Count > 0 ) {
				if( !BuildProcessPath( currentPathNCPack.LeadOutCAMPointList, dLastPointProcess_M, dLastPointProcess_S,
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
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = currentPathNCPack.ProcessEndPoint,
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

		#region Private methods

		static bool SolveProcessPath( PostSolver postSolver, IReadOnlyList<IProcessPoint> pointList,
			out List<PostPoint> resultG54, ref double dLastProcessPathM, ref double dLastProcessPathS )
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
			dLastProcessPathM = rotateAngleList[ rotateAngleList.Count - 1 ].Item1;
			dLastProcessPathS = rotateAngleList[ rotateAngleList.Count - 1 ].Item2;

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
					Slave = rotateAngleList[ i ].Item2
				};
				resultG54.Add( frameDataG54 );
			}
			return true;
		}

		static bool BuildProcessPath( IReadOnlyList<IProcessPoint> camPointList, double dM, double dS, out List<PostPoint> resultG54 )
		{
			resultG54 = new List<PostPoint>();
			if( camPointList == null || camPointList.Count == 0 ) {
				return false;
			}

			// build G54 points
			foreach( CAMPoint point in camPointList ) {
				PostPoint g54Point = new PostPoint()
				{
					X = point.Point.X(),
					Y = point.Point.Y(),
					Z = point.Point.Z(),
					Master = dM,
					Slave = dS
				};
				resultG54.Add( g54Point );
			}
			return true;
		}
		#endregion

		const double GEOM_TOLERANCE = 0.001;
	}
}
