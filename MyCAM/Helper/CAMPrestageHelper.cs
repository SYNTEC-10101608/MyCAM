using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	public interface IToolVecCAMPointInfo
	{
		bool IsToolVecPnt
		{
			get;
		}
		CAMPoint2 Point
		{
			get;
		}
		CAMPoint2 SharingPoint
		{
			get;
		}
		gp_Dir ToolVec
		{
			set;
		}
		Tuple<double, double> ABValues
		{
			get;
		}
		double DistanceToNext
		{
			get;
		}
	}

	public class CAMPointInfo : IToolVecCAMPointInfo
	{
		public CAMPoint2 Point
		{
			get; set;
		}

		// remain null if no sharing point
		public CAMPoint2 SharingPoint
		{
			get; set;
		}

		public gp_Dir ToolVec
		{
			set
			{
				if( value == null ) {
					return;
				}
				if( Point != null ) {
					Point.ToolVec = new gp_Dir( value.XYZ() );
				}
				if( SharingPoint != null ) {
					SharingPoint.ToolVec = new gp_Dir( value.XYZ() );
				}
			}
		}

		public bool IsToolVecPnt
		{
			get; set;
		} = false;

		public bool IsStartPnt
		{
			get; set;
		} = false;

		// if is not control point, ABValues is null
		public Tuple<double, double> ABValues
		{
			get; set;
		}

		public double DistanceToNext
		{
			get; set;
		}

		public CAMPointInfo( CAMPoint2 point )
		{
			Point = point;
			SharingPoint = null;
			IsToolVecPnt = false;
			IsStartPnt = false;
			ABValues = null;
			DistanceToNext = 0;
		}
	}

	public class CAMPrestageHelper
	{
		public static List<CAMPointInfo> FlattenCADSegmentsToCAMPointInfo(
			List<ICADSegment> cadSegmentList, CraftData craftData, bool isClosed )
		{
			List<CAMPointInfo> result = new List<CAMPointInfo>();
			if( cadSegmentList == null || cadSegmentList.Count == 0 ) {
				return result;
			}

			// get key index data
			SegmentPointIndex startPointIndex = craftData.StartPointIndex;
			Dictionary<SegmentPointIndex, Tuple<double, double>> toolVecModifyMap = craftData.ToolVecModifyMap;

			// explore CAD segments and CAD points
			for( int segIdx = 0; segIdx < cadSegmentList.Count; segIdx++ ) {
				ICADSegment cadSegment = cadSegmentList[ segIdx ];
				List<CADPoint> pointList = cadSegment.PointList;
				for( int pntIdx = 0; pntIdx < pointList.Count; pntIdx++ ) {

					// build CAMPoint
					CADPoint cadPoint = pointList[ pntIdx ];
					CAMPoint2 camPoint = new CAMPoint2( cadPoint.Point, cadPoint.NormalVec_1st, cadPoint.NormalVec_2nd, cadPoint.TangentVec, cadPoint.NormalVec_1st );

					// arrange index info
					SegmentPointIndex currentPointIndex = new SegmentPointIndex( segIdx, pntIdx );
					bool isFirstSegFirstPnt = ( pntIdx == 0 && segIdx == 0 );

					// this point is overlapping with last segment's end point
					bool isOverlapPnt = ( pntIdx == 0 && result.Count > 0 );

					// we take the first point of first segment, and treat it later if is closed
					if( isFirstSegFirstPnt ) {
						CAMPointInfo currentInfo = new CAMPointInfo( camPoint );
						currentInfo.SharingPoint = camPoint.Clone();

						// set flags
						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						if( isStartPoint ) {
							currentInfo.IsStartPnt = true;
						}
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isControlPoint ) {
							currentInfo.IsToolVecPnt = true;
							currentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
						}

						// set distance to next
						currentInfo.DistanceToNext = cadSegment.SubChordLength;
						result.Add( currentInfo );
					}

					// overlapping point, for now is first point of non-first segment, we modify the last point info
					else if( isOverlapPnt ) {
						CAMPointInfo lastPointInfo = result[ result.Count - 1 ];
						lastPointInfo.SharingPoint = camPoint;

						// set flags
						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isStartPoint ) {
							lastPointInfo.IsStartPnt = true;
						}
						if( isControlPoint ) {
							lastPointInfo.IsToolVecPnt = true;
							lastPointInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
						}

						// set distance to next
						lastPointInfo.DistanceToNext = cadSegment.SubChordLength;
					}

					// normal point inside a sgment, we add new point info
					else {
						CAMPointInfo currentInfo = new CAMPointInfo( camPoint );

						// set flags
						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						if( isStartPoint ) {
							currentInfo.IsStartPnt = true;
							currentInfo.SharingPoint = camPoint.Clone();
						}
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isControlPoint ) {
							currentInfo.IsToolVecPnt = true;
							currentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
							currentInfo.SharingPoint = camPoint.Clone();
						}

						// wait next segment to set distance to next, or for a non-closed path, just set 0
						if( pntIdx == pointList.Count - 1 ) {
							currentInfo.DistanceToNext = 0;
						}

						// for other points, just set sub chord length
						else {
							currentInfo.DistanceToNext = cadSegment.SubChordLength;
						}
						result.Add( currentInfo );
					}
				}
			}

			// if is closed, we need to merge the last point info to the first point info
			if( isClosed && result.Count > 1 ) {

				// find last point info
				CAMPointInfo lastCAMInfo = result[ result.Count - 1 ];
				CAMPoint2 lastCAMPoint = lastCAMInfo.Point.Clone();

				// remove last point info
				result.RemoveAt( result.Count - 1 );

				// the merged point is point form last point, and the sharing point is from first point
				CAMPointInfo firstPointInfo = result[ 0 ];
				firstPointInfo.Point = lastCAMPoint;

				// merge flags
				if( lastCAMInfo.IsStartPnt ) {
					firstPointInfo.IsStartPnt = true;
				}
				if( lastCAMInfo.IsToolVecPnt ) {
					firstPointInfo.IsToolVecPnt = true;
					firstPointInfo.ABValues = lastCAMInfo.ABValues;
				}
				// the distance to next is seted in the loop already
			}
			return result;
		}
	}
}
