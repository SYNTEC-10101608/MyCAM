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

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_CraftData.ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_CraftData.ToolVecModifyMap[ index ].Item2;
				return true;
			}
			else {
				dRA_deg = m_PathCache.InitIKResult[ index ].Item1 * 180 / Math.PI;
				dRB_deg = m_PathCache.InitIKResult[ index ].Item2 * 180 / Math.PI;
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
