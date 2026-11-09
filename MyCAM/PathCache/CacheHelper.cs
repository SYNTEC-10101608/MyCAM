using MyCAM.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	public static class CacheHelper
	{
		internal static List<ProcessPathGroup> GetProcessPathGroups( IReadOnlyList<string> pathIDList, FiveAxisMachineData machineData )
		{
			List<ProcessPathGroup> groups = new List<ProcessPathGroup>();
			ExternalAxisType type = machineData?.ExternalAxes.Type ?? ExternalAxisType.None;
			for( int i = 0; i < pathIDList.Count; i++ ) {
				if( !DataGettingHelper.GetCraftDataByID( pathIDList[ i ], out CraftData craftData ) || !craftData.ProcessPath ) {
					continue;
				}
				ExternalAxisPos assigned = craftData.ExternalAxisPos;
				ExternalAxisPos effective = type == ExternalAxisType.None ? default( ExternalAxisPos )
					: new ExternalAxisPos( assigned.Ext1, type == ExternalAxisType.Dual ? assigned.Ext2 : 0 );
				if( groups.Count == 0 || !groups.Last().ExternalAxisPos.Equals( effective ) ) {
					groups.Add( new ProcessPathGroup( groups.Count, effective ) );
				}
				groups.Last().AddPath( pathIDList[ i ], i );
			}
			return groups;
		}

		internal static void OptimizeIKContinuity( IReadOnlyList<string> pathIDList, FiveAxisMachineData machineData )
		{
			foreach( string pathID in pathIDList ) {
				if( DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) && !craftData.ProcessPath ) {
					craftData.InitMaster_rad = 0;
					craftData.InitSlave_rad = 0;
				}
			}
			foreach( ProcessPathGroup group in GetProcessPathGroups( pathIDList, machineData ) ) {
				double master = 0;
				double slave = 0;
				foreach( string pathID in group.PathIDs ) {
					if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
						continue;
					}
					craftData.InitMaster_rad = master;
					craftData.InitSlave_rad = slave;
					craftData.CAMFactorChanged?.Invoke();
					CAMPoint endPoint = GetProcessEndPoint( pathID );
					if( endPoint != null ) {
						master = endPoint.ModMaster_rad;
						slave = endPoint.ModSlave_rad;
					}
				}
			}
		}

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

	internal class ProcessPathGroup
	{
		readonly List<string> m_PathIDs = new List<string>();
		readonly List<int> m_PathIndices = new List<int>();

		public ProcessPathGroup( int index, ExternalAxisPos externalAxisPos )
		{
			Index = index;
			ExternalAxisPos = externalAxisPos;
		}

		public int Index { get; }
		public ExternalAxisPos ExternalAxisPos { get; }
		public IReadOnlyList<string> PathIDs => m_PathIDs.AsReadOnly();
		public IReadOnlyList<int> PathIndices => m_PathIndices.AsReadOnly();

		internal void AddPath( string pathID, int pathIndex )
		{
			m_PathIDs.Add( pathID );
			m_PathIndices.Add( pathIndex );
		}
	}
}
