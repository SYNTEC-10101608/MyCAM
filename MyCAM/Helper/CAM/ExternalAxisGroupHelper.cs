using MyCAM.Data;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper.CAM
{
	internal static class ExternalAxisGroupHelper
	{
		public static List<List<string>> BuildContiguousGroups( IReadOnlyList<string> pathIDList, double tolerance_deg = DEFAULT_TOLERANCE_DEG )
		{
			List<List<string>> groups = new List<List<string>>();
			if( pathIDList == null || pathIDList.Count == 0 ) {
				return groups;
			}

			List<string> currentGroup = new List<string>();
			double? prevExternalAxis = null;
			foreach( string pathID in pathIDList ) {
				if( string.IsNullOrEmpty( pathID ) || !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					FinalizeGroup( groups, currentGroup );
					prevExternalAxis = null;
					continue;
				}

				double currentExternalAxis = craftData.ExternalAxis_deg;
				if( prevExternalAxis.HasValue && Math.Abs( currentExternalAxis - prevExternalAxis.Value ) > tolerance_deg ) {
					FinalizeGroup( groups, currentGroup );
				}

				currentGroup.Add( pathID );
				prevExternalAxis = currentExternalAxis;
			}

			FinalizeGroup( groups, currentGroup );
			return groups;
		}

		static void FinalizeGroup( List<List<string>> groups, List<string> currentGroup )
		{
			if( currentGroup == null || currentGroup.Count == 0 ) {
				return;
			}
			groups.Add( currentGroup );
			currentGroup.Clear();
		}

		const double DEFAULT_TOLERANCE_DEG = 1e-3;
	}
}
