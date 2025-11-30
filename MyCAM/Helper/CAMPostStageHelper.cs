using MyCAM.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public interface ICAMSegInfo
	{
		CAMPoint2 MainPoint
		{
			get;
		}
		CAMPoint2 SharingPoint
		{
			get;
		}
		ICADSegment MainPointSeg
		{
			get;
		}
		ICADSegment SharingPointSeg
		{
			get;
		}
		bool IsStartPnt
		{
			get;
		}
		bool IsToolVecPnt
		{
			get;
		}
	}

	public static class CAMPostStageHelper
	{
		public static bool ReBuildCAMSegment( List<ICAMSegInfo> pathCAMInfo, bool isClosed, out List<ICAMSegment> PathCAMSegList, out List<int> CtrlSegIdx )
		{
			PathCAMSegList = new List<ICAMSegment>();
			CtrlSegIdx = new List<int>();

			// validation
			if( pathCAMInfo == null || pathCAMInfo.Count < 2 ) {
				return false;
			}

			// 建構 CAMSegment
			for( int i = 0; i < pathCAMInfo.Count; i++ ) {

				// 檢查 i 的 sharing point 是否為 null，若是，流程有問題，回傳 false
				if( pathCAMInfo[ i ].SharingPoint == null ) {
					return false;
				}
				List<ICAMSegInfo> tempList = new List<ICAMSegInfo>();

				// 往後找到 sharing point 不為 null 的 index j，若到終點還沒找到，表示是最後一個段落，取 j 到最後一個點，然後取 [0]
				int j = i + 1;
				while( j < pathCAMInfo.Count && pathCAMInfo[ j ].SharingPoint == null ) {
					tempList.Add( pathCAMInfo[ j ] );
					j++;
				}
				if( j == pathCAMInfo.Count ) {
					tempList.Add( pathCAMInfo[ 0 ] );
				}
				else {
					tempList.Add( pathCAMInfo[ j ] );
				}

				// 檢查 i+1 到 j 的 MainPoint 和 MainPointSeg 是否為 null，若是，流程有問題，回傳 false
				// 檢查 i+1 到 j 的 MainPointSeg 是否為同一個，且與 i 的 SharingPointSeg 相同，若不是，流程有問題，回傳 false
				foreach( var camSegInfo in tempList ) {
					if( camSegInfo.MainPoint == null || camSegInfo.MainPointSeg == null
						|| camSegInfo.MainPointSeg != pathCAMInfo[ i ].SharingPointSeg ) {
						return false;
					}
				}

				// 建立新的 CAMSegment，起點為 i 的 SharingPoint，後續為 i+1 到 j 的 MainPoint
				List<CAMPoint2> camPointList = new List<CAMPoint2> { pathCAMInfo[ i ].SharingPoint };
				foreach( var camSegInfo in tempList ) {
					camPointList.Add( camSegInfo.MainPoint );
				}
				double segmentLength = pathCAMInfo[ i ].SharingPointSeg.SegmentLength;
				double subSegmentLength = pathCAMInfo[ i ].SharingPointSeg.SubSegmentLength;
				double subChordLength = pathCAMInfo[ i ].SharingPointSeg.SubChordLength;
				if( pathCAMInfo[ i ].SharingPointSeg.SegmentType == ESegmentType.Line ) {
					ICAMSegment newSegment = new LineCAMSegment( camPointList, segmentLength, subSegmentLength, subChordLength );
					PathCAMSegList.Add( newSegment );
				}
				else {
					ICAMSegment newSegment = new ArcCAMSegment( camPointList, segmentLength, subSegmentLength, subChordLength );
					PathCAMSegList.Add( newSegment );
				}

				// 若 j 的 IsToolVecPnt 為 true，將此 CAMSegment 的 index 加入 CtrlSegIdx
				if( tempList.Last().IsToolVecPnt ) {
					CtrlSegIdx.Add( PathCAMSegList.Count - 1 );
				}

				// 若 j 的 IsStartPnt 為 true，標記此 CAMSegment 的 index，稍後使用
				//if( pathCAMInfo[ j ].IsStartPnt ) {
				//	// 標記邏輯可根據需求實現
				//}

				// 迴圈從 j 繼續，做一樣的事情，直到處理完整個 pathCAMInfo
				i = j - 1; // 調整 i 的位置，避免重複處理
			}
			return true;
		}
	}
}
