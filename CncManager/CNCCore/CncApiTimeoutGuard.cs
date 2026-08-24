using System;
using System.Threading.Tasks;
using CncManager.Definition;

namespace CncManager.CNCCore
{
	// Use to against blocking by CNC API forever.
	public class CncApiTimeoutGuard
	{
		public CncApiTimeoutGuard( int defaultTimeoutMs = CncConstants.DEFAULT_API_TIMEOUT_MS )
		{
			m_DefaultTimeoutMs = defaultTimeoutMs;
		}

		// Sync version, blocks calling thread
		public T TryExecute<T>( Func<T> func, int? timeoutMs = null )
		{
			int timeout = timeoutMs ?? m_DefaultTimeoutMs;
			var task = Task.Run( func );

			try {
				bool completed = task.Wait( timeout );
				if( !completed ) {
					return default;
				}
				return task.Result;
			}
			catch( AggregateException ) {
				return default;
			}
			catch( Exception ) {
				return default;
			}
		}

		// Async version, does not block calling thread
		public async Task<T> TryExecuteAsync<T>( Func<T> func, int? timeoutMs = null )
		{
			int timeout = timeoutMs ?? m_DefaultTimeoutMs;

			try {
				Task<T> apiTask = Task.Run( func );
				Task delayTask = Task.Delay( timeout );
				Task completedTask = await Task.WhenAny( apiTask, delayTask );

				if( completedTask == apiTask ) {
					return await apiTask;
				}
				return default;
			}
			catch( Exception ) {
				return default;
			}
		}

		readonly int m_DefaultTimeoutMs;
	}
}
