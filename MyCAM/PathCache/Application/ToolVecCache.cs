using MyCAM.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.PathCache
{
	internal class ToolVecCache : MainPathPointCache, IToolVecCache
	{
		private readonly CraftData m_CraftData;

		public ToolVecCache( IPathCache pathCache, CraftData craftData )
			: base( pathCache )
		{
			m_CraftData = craftData;
		}

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_CraftData.ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_CraftData.ToolVecModifyMap[ index ].Item2;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
				return false;
			}
		}

		public bool IsToolVecModifyPoint( IProcessPoint point )
		{
			return ( point as CAMPoint ).IsToolVecModPoint;
		}
	}
}
