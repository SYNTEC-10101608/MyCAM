using CncManager.CNCCore;
using System.Threading.Tasks;

namespace CncManager
{
	internal class KVarPool : ResourcePool<int?>
	{
		protected override async Task<int?> ReadOneResourceAsync( int index )
		{
			var result = await CncApi.ReadKVariableAsync( index ).ConfigureAwait( false );
			return result.IsSuccess ? (int?)result.Data : null;
		}
	}
}
