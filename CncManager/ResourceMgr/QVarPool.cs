using System.Threading.Tasks;
using CncManager.CNCCore;
using CncManager.Definition;

namespace CncManager
{
	internal class QVarPool : ResourcePool<int?>
	{
		public QVarPool( int coordID = CncConstants.FIRST_COORD_ID )
		{
			m_CoordID = coordID;
		}

		protected override async Task<int?> ReadOneResourceAsync( int index )
		{
			var result = await CncApi.ReadQVariableAsync( m_CoordID, index ).ConfigureAwait( false );
			return result.IsSuccess ? (int?)result.Data : null;
		}

		readonly int m_CoordID;
	}
}
