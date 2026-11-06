using CncManager.CNCCore;
using System.Threading.Tasks;

namespace CncManager
{
	internal class GlobalVarPool : ResourcePool<double?>
	{
		protected override async Task<double?> ReadOneResourceAsync( int index )
		{
			var result = await CncApi.ReadGlobalVariableAsync( index ).ConfigureAwait( false );
			return result.IsSuccess ? result.Data : null;
		}
	}
}
