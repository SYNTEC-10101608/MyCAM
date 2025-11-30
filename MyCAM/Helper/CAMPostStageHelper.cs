using MyCAM.Data;
using System.Collections.Generic;

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

				// 往後找到 sharing point 不為 null 的 index j，若到終點還沒找到，表示是最後一個段落，取 j 到最後一個點，然後取 [0]

				// 檢查 i+1 到 j 的 MainPoint 和 MainPointSeg 是否為 null，若是，流程有問題，回傳 false

				// 檢查 i+1 到 j 的 MainPointSeg 是否為同一個，且與 i 的 SharingPointSeg 相同，若不是，流程有問題，回傳 false

				// 建立新的 CAMSegment，起點為 i 的 SharingPoint，終點為 j 的 MainPoint，中間點為 i+1 到 j-1 的 MainPoint

				// 將新的 CAMSegment 加入 PathCAMSegList

				// 若 j 的 IsToolVecPnt 為 true，將此 CAMSegment 的 index 加入 CtrlSegIdx

				// 若 j 的 IsStartPnt 為 true，標記此 CAMSegment 的 index，稍後使用

				// 迴圈從 j 繼續，做一樣的事情，直到處理完整個 pathCAMInfo
			}

			return true;
		}
	}
}
