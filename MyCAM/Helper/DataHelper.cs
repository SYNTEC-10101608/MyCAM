using MyCAM.App;
using MyCAM.Data;

namespace MyCAM
{
	internal static class DataHelper
	{
		public static bool GetCraftDataByID( DataManager dataManager, string szPathID, out CraftData craftData )
		{
			craftData = null;
			if( string.IsNullOrEmpty( szPathID )
				|| dataManager.ObjectMap.ContainsKey( szPathID ) == false
				|| dataManager.ObjectMap[ szPathID ] == null
				|| !( dataManager.ObjectMap[ szPathID ] is PathObject
				|| ( (PathObject)dataManager.ObjectMap[ szPathID ] ).CraftData == null ) ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]所選路徑資料異常，請重新選擇", MyApp.NoticeType.Hint );
				return false;
			}

			if( ( (PathObject)dataManager.ObjectMap[ szPathID ] ).PathType == PathType.Contour ) {
				craftData = ( (ContourPathObject)dataManager.ObjectMap[ szPathID ] ).CraftData;
			}
			return true;
		}

		public static bool GetCacheInfoByID( DataManager dataManager, string szPathID, out ICacheInfo cacheInfo )
		{
			cacheInfo = null;
			if( string.IsNullOrEmpty( szPathID )
				|| dataManager.ObjectMap.ContainsKey( szPathID ) == false
				|| dataManager.ObjectMap[ szPathID ] == null ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]所選路徑資料異常，請重新選擇", MyApp.NoticeType.Hint );
				return false;
			}
			if( ( (PathObject)dataManager.ObjectMap[ szPathID ] ).PathType == PathType.Contour ) {
				cacheInfo = ( (ContourPathObject)dataManager.ObjectMap[ szPathID ] ).ContourCacheInfo;
			}
			return true;
		}
	}
}
