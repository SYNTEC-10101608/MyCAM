using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	public class CAMPointInfo
	{
		public CAMPoint2 Point
		{
			get; set;
		}

		public CAMPoint2 Point2
		{
			get; set;
		}

		public gp_Dir ToolVec
		{
			get
			{
				return m_ToolVec;
			}
			set
			{
				m_ToolVec = value;
				setToolVec( value );
			}

		}

		public bool IsCtrlPnt
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
			IsCtrlPnt = false;
			IsStartPnt = false;
			DistanceToNext = 0;
		}

		void setToolVec( gp_Dir dir )
		{
			if( dir == null ) {
				return;
			}
			if( Point != null ) {
				Point.ToolVec = dir;
			}
			if( Point2 != null ) {
				Point2.ToolVec = dir;
			}
		}
		gp_Dir m_ToolVec;
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

			SegmentPointIndex startPointIndex = craftData.StartPointIndex;
			Dictionary<SegmentPointIndex, Tuple<double, double>> toolVecModifyMap = craftData.ToolVecModifyMap;

			for( int segIdx = 0; segIdx < cadSegmentList.Count; segIdx++ ) {
				ICADSegment cadSegment = cadSegmentList[ segIdx ];
				List<CADPoint> pointList = cadSegment.PointList;
				for( int pntIdx = 0; pntIdx < pointList.Count; pntIdx++ ) {
					CADPoint cadPoint = pointList[ pntIdx ];
					SegmentPointIndex currentPointIndex = new SegmentPointIndex( segIdx, pntIdx );

					CAMPoint2 camPoint = ConvertCADPointToCAMPoint2( cadPoint, craftData.IsToolVecReverse );
					bool isFirstSegFirstPnt = ( pntIdx == 0 && segIdx == 0 );
					bool isOverlapPnt = ( pntIdx == 0 && result.Count > 0 );

					if( isFirstSegFirstPnt ) {
						CAMPointInfo currentInfo = new CAMPointInfo( camPoint );
						currentInfo.Point2 = camPoint.Clone();

						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						if( isStartPoint ) {
							currentInfo.IsStartPnt = true;
						}
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isControlPoint ) {
							currentInfo.IsCtrlPnt = true;
							currentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
						}
						currentInfo.DistanceToNext = cadSegment.SubChordLength;
						result.Add( currentInfo );
					}
					else if( isOverlapPnt ) {
						CAMPointInfo lastPointInfo = result[ result.Count - 1 ];
						lastPointInfo.Point2 = camPoint;

						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isStartPoint ) {
							lastPointInfo.IsStartPnt = true;
						}
						if( isControlPoint ) {
							lastPointInfo.IsCtrlPnt = true;
							lastPointInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
						}
						lastPointInfo.DistanceToNext = cadSegment.SubChordLength;
					}
					else {
						CAMPointInfo currentInfo = new CAMPointInfo( camPoint );

						bool isStartPoint = startPointIndex.Equals( currentPointIndex );
						if( isStartPoint ) {
							currentInfo.IsStartPnt = true;
							currentInfo.Point2 = camPoint.Clone();
						}
						bool isControlPoint = toolVecModifyMap.ContainsKey( currentPointIndex );
						if( isControlPoint ) {
							currentInfo.IsCtrlPnt = true;
							currentInfo.ABValues = toolVecModifyMap[ currentPointIndex ];
							currentInfo.Point2 = camPoint.Clone();
						}

						if( pntIdx == pointList.Count - 1 ) {
							currentInfo.DistanceToNext = 0;
						}
						else {
							currentInfo.DistanceToNext = cadSegment.SubChordLength;
						}
						result.Add( currentInfo );
					}
				}
			}

			if( isClosed && result.Count > 1 ) {
				CAMPointInfo lastCAMInfo = result[ result.Count - 1 ];
				CAMPoint2 lastCAMPoint = lastCAMInfo.Point.Clone();

				result.RemoveAt( result.Count - 1 );

				CAMPointInfo firstPointInfo = result[ 0 ];
				firstPointInfo.Point = lastCAMPoint;

				if( lastCAMInfo.IsStartPnt ) {
					firstPointInfo.IsStartPnt = true;
				}
				if( lastCAMInfo.IsCtrlPnt ) {
					firstPointInfo.IsCtrlPnt = true;
					firstPointInfo.ABValues = lastCAMInfo.ABValues;
				}
			}
			if( craftData.IsReverse ) {
				ReverseCAMInfo( ref result );
			}
			return result;
		}

		private static void ReverseCAMInfo( ref List<CAMPointInfo> camInfoList )
		{
			camInfoList.Reverse();
			foreach( CAMPointInfo camInfo in camInfoList ) {
				if( camInfo.Point2 == null ) {
					continue;
				}
				CAMPoint2 tempPnt = camInfo.Point;
				camInfo.Point = camInfo.Point2;
				camInfo.Point2 = tempPnt;
			}
		}

		private static CAMPoint2 ConvertCADPointToCAMPoint2( CADPoint cadPoint, bool isToolVecReverse )
		{
			gp_Dir initNorVec = isToolVecReverse ? cadPoint.NormalVec_1st.Reversed() : cadPoint.NormalVec_1st;
			return new CAMPoint2(
				cadPoint.Point,
				initNorVec,
				initNorVec,
				cadPoint.TangentVec,
				initNorVec
			);
		}
	}
}
