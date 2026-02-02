using MyCAM.Data;
using System;


namespace MyCAM.PathCache
{
	public class ToolVecPackage
	{
		public ToolVecPackage( ContourCache pathCache, CraftData craftData )
		{
			m_PathCache = pathCache;
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

				// get CAM map index
				if( m_PathCache.CADToCAMIndexMap.ContainsKey( index ) ) {
					int camIndex = m_PathCache.CADToCAMIndexMap[ index ];

					// get master and slave from InitIKResult and convert rad to deg
					if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
						master_deg = m_PathCache.MainPathPointList[ camIndex ].InitMaster_rad * 180.0 / Math.PI;
						slave_deg = m_PathCache.MainPathPointList[ camIndex ].InitSlave_rad * 180.0 / Math.PI;
					}
					else {
						master_deg = 0;
						slave_deg = 0;
					}
				}
				else {
					master_deg = 0;
					slave_deg = 0;
				}
				return false;
			}
		}

		public ISetToolVecPoint GetPointByCADIndex( int cadIndex )
		{
			if( m_PathCache.CADToCAMIndexMap.ContainsKey( cadIndex ) ) {
				int camIndex = m_PathCache.CADToCAMIndexMap[ cadIndex ];
				if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
					return m_PathCache.MainPathPointList[ camIndex ];
				}
			}
			return null;
		}

		readonly CraftData m_CraftData;
		readonly ContourCache m_PathCache;
	}
}
