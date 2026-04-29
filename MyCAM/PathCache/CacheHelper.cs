using MyCAM.Data;
using System.Linq;

namespace MyCAM.PathCache
{
	public static class CacheHelper
	{
		public static CAMPoint GetProcessStartPoint( string szPathID )
		{
			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}
			if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
				return null;
			}
			return GetProcessStartPoint( pathCache, craftData );
		}

		public static CAMPoint GetProcessEndPoint( string szPathID )
		{
			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}
			if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
				return null;
			}
			return GetProcessEndPoint( pathCache, craftData );
		}

		public static CAMPoint GetMainPathStartPoint( string szPathID )
		{
			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}
			return GetMainPathStartPoint( pathCache );
		}

		public static CAMPoint GetLeadInStartPoint( string szPathID )
		{
			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}
			if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
				return null;
			}
			return GetLeadInStartPoint( pathCache, craftData );
		}

		public static CAMPoint GetLeadOutEndPoint( string szPathID )
		{
			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}
			if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
				return null;
			}
			return GetLeadOutEndPoint( pathCache, craftData );
		}

		static CAMPoint GetProcessStartPoint( IPathCache pathCache, CraftData craftData )
		{
			CAMPoint camPoint = null;
			if( pathCache.LeadInPointList.Count > 0 && ( craftData.LeadData.LeadIn.StraightLength > 0 || craftData.LeadData.LeadIn.ArcLength > 0 ) ) {
				camPoint = pathCache.LeadInPointList.First().Clone();
			}
			else if( pathCache.MainPathPointList.Count > 0 ) {
				camPoint = pathCache.MainPathPointList.First().Clone();
			}
			return camPoint;
		}

		static CAMPoint GetProcessEndPoint( IPathCache pathCache, CraftData craftData )
		{
			CAMPoint camPoint = null;
			if( pathCache.LeadOutPointList.Count > 0 && ( craftData.LeadData.LeadOut.StraightLength > 0 || craftData.LeadData.LeadOut.ArcLength > 0 ) ) {
				camPoint = pathCache.LeadOutPointList.Last().Clone();
			}
			else if( pathCache.OverCutPointList.Count > 0 && craftData.OverCutLength > 0 ) {
				camPoint = pathCache.OverCutPointList.Last().Clone();
			}
			else if( pathCache.MainPathPointList.Count > 0 ) {
				camPoint = pathCache.MainPathPointList.Last().Clone();
			}
			return camPoint;
		}

		static CAMPoint GetLeadInStartPoint( IPathCache pathCache, CraftData craftData )
		{
			if( pathCache.LeadInPointList.Count > 0 && ( craftData.LeadData.LeadIn.StraightLength > 0 || craftData.LeadData.LeadIn.ArcLength > 0 ) ) {
				return pathCache.LeadInPointList.First().Clone();
			}
			return null;
		}

		static CAMPoint GetLeadOutEndPoint( IPathCache pathCache, CraftData craftData )
		{
			if( pathCache.LeadOutPointList.Count > 0 && ( craftData.LeadData.LeadOut.StraightLength > 0 || craftData.LeadData.LeadOut.ArcLength > 0 ) ) {
				return pathCache.LeadOutPointList.Last().Clone();
			}
			return null;
		}

		static CAMPoint GetMainPathStartPoint( IPathCache pathCache )
		{
			CAMPoint camPoint = null;
			if( pathCache.MainPathPointList.Count > 0 ) {
				camPoint = pathCache.MainPathPointList.First().Clone();
			}
			return camPoint;
		}

		public static bool GetMainPathEndMS( string szPathID, out double master_rad, out double slave_rad )
		{
			master_rad = 0;
			slave_rad = 0;
			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return false;
			}
			if( pathCache.MainPathPointList == null || pathCache.MainPathPointList.Count == 0 ) {
				return false;
			}
			CAMPoint endPoint = pathCache.MainPathPointList.Last();
			master_rad = endPoint.ModMaster_rad;
			slave_rad = endPoint.ModSlave_rad;
			return true;
		}
	}
}
