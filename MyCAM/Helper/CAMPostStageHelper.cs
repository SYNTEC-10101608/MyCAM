using MyCAM.CacheInfo; // Import ContourCacheInfo to access its properties
using MyCAM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAM.Helper
{
	public static class CAMPostStageHelper
	{
		public static bool ReBuildCAMSegment( List<CAMPointInfo> pathCAMInfo, IReadOnlyList<ICADSegment> cadSegmentList, bool isClosed, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx )
		{
			CtrlSegIdx = new List<int>();
			int currentSegmentIdx = 0;
			PathCAMSegList = new List<ICAMSegment>();
			bool isBuildDone = ReBuildCAMSegmentFromStartPnt( pathCAMInfo, cadSegmentList, isClosed, ref CtrlSegIdx, ref currentSegmentIdx, out List<ICAMSegment> camSegmentList );
			if( isBuildDone && camSegmentList != null ) {
				PathCAMSegList.AddRange( camSegmentList );
			}
			else {
				return false;
			}
			bool isBuildPreDone = ReBuildCAMSegBeforStartPnt( pathCAMInfo, cadSegmentList, isClosed, ref CtrlSegIdx, ref currentSegmentIdx, out List<ICAMSegment> preCamSegmentList );
			if( isBuildPreDone && preCamSegmentList != null ) {
				PathCAMSegList.AddRange( preCamSegmentList );
			}
			else {
				return false;
			}
			return true;
		}

		static bool ReBuildCAMSegBeforStartPnt( List<CAMPointInfo> camPntList, IReadOnlyList<ICADSegment> cadSegmentList, bool isClosed, ref List<int> CtrlSegIdx, ref int currentSegmentIdx, out List<ICAMSegment> camSegmentList )
		{
			camSegmentList = new List<ICAMSegment>();
			if( camPntList == null || camPntList.Count == 0 ) {
				return false;
			}
			int nStartPntIndx = 0;
			for( int i = 0; i < camPntList.Count; i++ ) {
				if( camPntList[ i ].IsStartPnt ) {
					nStartPntIndx = i;
					break;
				}
			}

			// start point is at [0][0], no point before it
			if( nStartPntIndx == 0 ) {
				return true;
			}
			List<CAMPoint2> currentSegmentPoints = new List<CAMPoint2>();
			for( int i = 0; i <= nStartPntIndx; i++ ) {
				CAMPointInfo currentPointInfo = camPntList[ i ];
				currentSegmentPoints.Add( currentPointInfo.Point );

				// check need to build CAMSegment
				bool isSplitPoint = currentPointInfo.SharingPoint != null && i != 0;
				bool reachStartPoint = i == nStartPntIndx;

				if( isSplitPoint || reachStartPoint ) {
					if( currentSegmentPoints.Count >= 2 ) {

						// check last pnt is at with segment
						bool isGetDone = GetSegmentType( i, cadSegmentList, out ESegmentType segmentType );
						if( isGetDone == false ) {
							return false;
						}
						ICAMSegment camSegment = BuildCAMSegmentFromCAMPointInfo( currentSegmentPoints, segmentType );
						if( camSegment == null ) {
							return false;
						}
						// for record on CtrlSegIdx
						currentSegmentIdx++;
						if( camPntList[ i ].IsToolVecPnt ) {
							CtrlSegIdx.Add( currentSegmentIdx );
						}
						camSegmentList.Add( camSegment );
					}

					// is new segment start
					if( !reachStartPoint ) {
						currentSegmentPoints = new List<CAMPoint2>();
						currentSegmentPoints.Add( currentPointInfo.SharingPoint );
					}
				}
			}
			return true;
		}

		static bool ReBuildCAMSegmentFromStartPnt( List<CAMPointInfo> camPntList, IReadOnlyList<ICADSegment> cadSegmentList, bool isClosed, ref List<int> CtrlSegIdx, ref int currentSegmentIdx, out List<ICAMSegment> camSegmentList )
		{
			camSegmentList = new List<ICAMSegment>();
			if( camPntList == null || camPntList.Count == 0 ) {
				return false;
			}

			// find start point at with index
			int nStartPntIndx = 0;
			for( int i = 0; i < camPntList.Count; i++ ) {
				if( camPntList[ i ].IsStartPnt ) {
					nStartPntIndx = i;
					break;
				}
			}
			List<CAMPoint2> currentSegmentPoints = new List<CAMPoint2>();

			for( int i = nStartPntIndx; i < camPntList.Count; i++ ) {
				CAMPointInfo currentPointInfo = camPntList[ i ];

				// start point is special case
				if( i == nStartPntIndx ) {

					// any segment to build start with point2
					if( currentPointInfo.SharingPoint != null ) {
						currentSegmentPoints.Add( currentPointInfo.SharingPoint );
					}

					// start point caminfo need have two pnt
					else {
						return false;
					}
				}
				else {
					currentSegmentPoints.Add( currentPointInfo.Point );
				}

				// check special case (break pnt)
				bool isSplitPoint = currentPointInfo.SharingPoint != null && i != nStartPntIndx;
				bool isLastPoint = i == camPntList.Count - 1;
				if( isSplitPoint || isLastPoint ) {

					// it is close path last pnt
					if( isLastPoint && isClosed ) {

						// add point back : real end pnt is first segment index[0]
						currentSegmentPoints.Add( camPntList[ 0 ].Point );
					}

					// build cam segment
					if( currentSegmentPoints.Count >= 2 ) {
						// check last pnt is at with segment
						bool isGetDone = GetSegmentType( i, cadSegmentList, out ESegmentType segmentType );
						if( isGetDone == false ) {
							return false;
						}
						ICAMSegment camSegment = BuildCAMSegmentFromCAMPointInfo( currentSegmentPoints, segmentType );
						if( camSegment == null ) {
							return false;
						}

						// for CtrlSegIdx to record
						currentSegmentIdx = camSegmentList.Count;

						// segment end not with isCtrlPnt means it is normal overlap
						if( camPntList[ i ].IsToolVecPnt ) {
							CtrlSegIdx.Add( currentSegmentIdx );
						}
						camSegmentList.Add( camSegment );
					}

					// is not last pnt, prepare new pnt for next segment (every segment start with point2)
					if( !isLastPoint ) {

						// reset
						currentSegmentPoints = new List<CAMPoint2>();

						// next seg start at point2
						currentSegmentPoints.Add( currentPointInfo.SharingPoint );
					}
				}
			}
			return true;
		}

		static ICAMSegment BuildCAMSegmentFromCAMPointInfo( List<CAMPoint2> camPointList, ESegmentType segmentType )
		{
			if( camPointList == null || camPointList.Count < 2 ) {
				return null;
			}
			double dChordLength = camPointList[ 0 ].Point.Distance( camPointList[ 1 ].Point );
			double dEdgeLength = dChordLength * ( camPointList.Count - 1 );
			ICAMSegment camSegment = null;
			if( segmentType == ESegmentType.Line ) {

				camSegment = new LineCAMSegment( camPointList, dEdgeLength, dChordLength, dChordLength );
			}
			else if( segmentType == ESegmentType.Arc ) {
				if( camPointList.Count < 3 ) {
					camSegment = new LineCAMSegment( camPointList, dEdgeLength, dChordLength, dChordLength );
				}
				else {
					camSegment = new ArcCAMSegment( camPointList, dEdgeLength, dChordLength, dChordLength );
				}
			}
			return camSegment;
		}

		static bool GetSegmentType( int nPntIndex, IReadOnlyList<ICADSegment> cadsegment, out ESegmentType segmentType )
		{
			int nPntSum = 0;
			segmentType = ESegmentType.Line;
			if( cadsegment == null || cadsegment.Count == 0 ) {
				return false;
			}

			for( int i = 0; i < cadsegment.Count; i++ ) {

				// removew over pnt
				if( i != 0 ) {
					nPntSum -= 1;
				}

				// cal current seg range
				int segmentStartIndex = nPntSum;
				int segmentEndIndex = nPntSum + cadsegment[ i ].PointList.Count - 1;

				// check nPntIndex is in this range
				if( nPntIndex >= segmentStartIndex && nPntIndex <= segmentEndIndex ) {
					segmentType = cadsegment[ i ].SegmentType;
					return true;
				}
				nPntSum += cadsegment[ i ].PointList.Count;
			}
			return false;
		}
	}
}
