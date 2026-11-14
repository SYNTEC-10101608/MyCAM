using System.Threading.Tasks;
using CncManager.CNCCore;

namespace CncManager
{
	internal class RRegisterPool : ResourcePool<int?>
	{
		protected override async Task<int?> ReadOneResourceAsync( int index )
		{
			var result = await CncApi.ReadRRegisterAsync( index ).ConfigureAwait( false );
			return result.IsSuccess ? (int?)result.Data : null;
		}
	}
}
