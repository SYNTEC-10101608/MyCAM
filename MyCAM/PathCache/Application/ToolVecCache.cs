using MyCAM.Data;
using System;


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

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg, out double master_deg, out double slave_deg )
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_CraftData.ToolVecModifyMap[ index ].RA_deg;
				dRB_deg = m_CraftData.ToolVecModifyMap[ index ].RB_deg;
				master_deg = m_CraftData.ToolVecModifyMap[ index ].Master_deg;
				slave_deg = m_CraftData.ToolVecModifyMap[ index ].Slave_deg;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;

				// get master and slave from InitIKResult and convert rad to deg
				if( index >= 0 && index < m_PathCache.InitIKResult.Count ) {
					master_deg = m_PathCache.InitIKResult[ index ].Item1 * 180.0 / Math.PI;
					slave_deg = m_PathCache.InitIKResult[ index ].Item2 * 180.0 / Math.PI;
				}
				else {
					master_deg = 0;
					slave_deg = 0;
				}
				return false;
			}
		}

		public bool IsToolVecModifyPoint( IProcessPoint point )
		{
			return point.IsToolVecModPoint;
		}

		public bool GetToolVecInterpolateType( out EToolVecInterpolateType interpolateType )
		{
			interpolateType = m_CraftData.InterpolateType;
			return true;
		}
	}
}
