using System.Threading.Tasks;
using CncManager.CNCCore;

namespace CncManager
{
	internal class MainProgramPool : ResourcePool<string>
	{
		protected override async Task<string> ReadOneResourceAsync( int index )
		{
			var result = await CncApi.ReadMainProgramAsync( index ).ConfigureAwait( false );
			return result.IsSuccess ? result.Data : null;
		}
	}
}
