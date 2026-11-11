using MyCAM.Data;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class ProcessPathGroupHelper
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
				ProcessPathGroup group = groups.Count == 0 ? null : groups[ groups.Count - 1 ];
				if( group == null
					|| Math.Abs( group.ExternalAxisPos.Ext1 - effective.Ext1 ) > EXTERNAL_AXIS_TOLERANCE_DEG
					|| Math.Abs( group.ExternalAxisPos.Ext2 - effective.Ext2 ) > EXTERNAL_AXIS_TOLERANCE_DEG ) {
					group = new ProcessPathGroup( groups.Count, effective );
					groups.Add( group );
				}
				group.AddPath( pathIDList[ i ], i );
			}
			return groups;
		}

		const double EXTERNAL_AXIS_TOLERANCE_DEG = 1e-3;
	}
}
