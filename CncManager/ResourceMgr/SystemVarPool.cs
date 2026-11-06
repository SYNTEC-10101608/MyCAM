using CncManager.CNCCore;
using CncManager.Definition;
using System.Threading.Tasks;

namespace CncManager
{
	internal class SystemVarPool : ResourcePool<double?>
	{
		public SystemVarPool( int coordID = CncConstants.FIRST_COORD_ID )
		{
			m_CoordID = coordID;
		}

		protected override Task<double?> ReadOneResourceAsync( int index )
		{
			return CncApi.ReadSystemVariableAsync( m_CoordID, index );
		}

		readonly int m_CoordID;
	}
}
