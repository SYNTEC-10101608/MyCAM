using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Post
{
	internal class StdPatternNCPackage : INCPackage
	{
		public StdPatternNCPackage( IProcessPoint refPoint, IProcessPoint startPoint, TraverseData traverseData, IProcessPoint processStartPoint, IProcessPoint processEndPoint, LeadData leadData, IReadOnlyList<IProcessPoint> leadInCAMPointList, IStdPatternGeomData geomData )
		{
			RefPoint = refPoint ?? throw new ArgumentNullException( nameof( refPoint ) );
			StartPoint = startPoint ?? throw new ArgumentNullException( nameof( startPoint ) );
			TraverseData = traverseData ?? throw new ArgumentNullException( nameof( traverseData ) );
			ProcessStartPoint = processStartPoint;
			ProcessEndPoint = processEndPoint;
			LeadData = leadData ?? throw new ArgumentNullException( nameof( leadData ) );
			LeadInCAMPointList = leadInCAMPointList ?? throw new ArgumentNullException( nameof( leadInCAMPointList ) );
			GeomData = geomData ?? throw new ArgumentNullException( nameof( geomData ) );
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

		public IStdPatternGeomData GeomData
		{
			get; private set;
		}
	}

	internal static class StdPatternPostHelper
	{
		public static bool GetPostData( StdPatternNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out StdPatternPostData pathG54PostData, out PathEndInfo currentPathtEndInfo )
		{
			// for write NC file
			pathG54PostData = new StdPatternPostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( currentPathNCPack == null || entryAndExitData == null ) {
				return false;
			}

			if( !ConvertProcessPntToPostPnt( currentPathNCPack.RefPoint, out PostPoint mainG54 ) ) {
				return false;
			}
			pathG54PostData.RefPoint = mainG54;

			if( !ConvertProcessPntToPostPnt( currentPathNCPack.StartPoint, out PostPoint processStartPoint ) ) {
				return false;
			}
			pathG54PostData.ProcessStartPoint = processStartPoint;

			// lead-in
			if( currentPathNCPack.LeadInCAMPointList.Count > 0 ) {
				if( !ConvertProcessPntToPostPnt( currentPathNCPack.LeadInCAMPointList.First(), out processStartPoint ) ) {
					return false;
				}

				// update process start point
				pathG54PostData.ProcessStartPoint = processStartPoint;
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
				Master = currentPathNCPack.ProcessEndPoint.ModMaster_rad,
				Slave = currentPathNCPack.ProcessEndPoint.ModSlave_rad
			};
			return true;
		}

		static bool ConvertProcessPntToPostPnt( IProcessPoint point, out PostPoint resultG54 )
		{
			resultG54 = new PostPoint();
			if( point == null ) {
				return false;
			}
			gp_Pnt pointG54 = ( point as CAMPoint ).Point;
			PostPoint frameDataG54 = new PostPoint()
			{
				X = pointG54.X(),
				Y = pointG54.Y(),
				Z = pointG54.Z(),
				Master = point.ModMaster_rad,
				Slave = point.ModSlave_rad
			};
			resultG54 = frameDataG54;
			return true;
		}
	}
}
