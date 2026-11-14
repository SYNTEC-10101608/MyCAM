using System;
using System.Threading;
using System.Threading.Tasks;
using CncManager.Definition;

namespace CncManager.CNCCore
{
	// One background loop owns heartbeat and reconnect for this connection session.
	public class CncConnectChecker : IDisposable
	{
		// Raised after every check (true = alive)
		public event Action<bool> HeartbeatChecked;

		// Raised when auto-reconnect starts
		public event Action Reconnecting;

		// Raised when auto-reconnect succeeds
		public event Action Reconnected;

		// Raised when auto-reconnect gives up after max attempts
		public event Action ReconnectFailed;

		public CncConnectChecker(
			CncApi cnc,
			int CheckConeectInterval = CncConstants.CHECKER_INTERVAL_MS,
			int failThreshold = CncConstants.CHECKER_FAIL_THRESHOLD )
		{
			m_CncApi = cnc;
			m_CheckConeectInterval = CheckConeectInterval;
			m_FailThreshold = failThreshold;
			m_CncApi.ConnectionError += OnCncConnectionError;
		}

		public void Start()
		{
			lock( m_Lock ) {
				if( m_Disposed || m_Task != null ) {
					return;
				}
				m_Cancellation = new CancellationTokenSource();
				CancellationToken token = m_Cancellation.Token;
				m_Task = Task.Run( () => RunAsync( token ) );
			}
		}

		public void StopConnectLoop()
		{
			StopConnectLoopAsync().GetAwaiter().GetResult();
		}

		public async Task StopConnectLoopAsync()
		{
			Task task;
			CancellationTokenSource cancellation;
			lock( m_Lock ) {
				task = m_Task;
				cancellation = m_Cancellation;
				cancellation?.Cancel();
			}
			if( task == null ) {
				return;
			}
			try {
				await task.ConfigureAwait( false );
			}
			catch( OperationCanceledException ) {
			}
			finally {
				lock( m_Lock ) {
					if( m_Task == task ) {
						m_Task = null;
						m_Cancellation = null;
						cancellation.Dispose();
					}
				}
			}
		}

		public void Dispose()
		{
			lock( m_Lock ) {
				m_Disposed = true;
			}
			m_CncApi.ConnectionError -= OnCncConnectionError;
			StopConnectLoop();
		}

		async Task RunAsync( CancellationToken cancellationToken )
		{
			int failures = 0;
			bool initialize = true;
			try {
				while( !cancellationToken.IsCancellationRequested ) {
					if( m_ConnectionLost || !CncApi.IsConnected ) {
						if( !await ReconnectAsync( cancellationToken ).ConfigureAwait( false ) ) {
							return;
						}
						failures = 0;
						initialize = true;
					}
					var result = await CncApi.CheckHeartbeatAsync( initialize ).ConfigureAwait( false );
					cancellationToken.ThrowIfCancellationRequested();
					if( result.Status != CncResultStatus.Busy ) {
						bool alive = result.IsSuccess && result.Data;
						Notify( () => HeartbeatChecked?.Invoke( alive ) );
						if( alive ) {
							initialize = false;
							failures = 0;
						}
						else if( ++failures >= m_FailThreshold ) {
							m_ConnectionLost = true;
						}
					}
					await Task.Delay( m_CheckConeectInterval, cancellationToken ).ConfigureAwait( false );
				}
			}
			catch( OperationCanceledException ) {
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC connection check failed: {0}", ex );
				if( !cancellationToken.IsCancellationRequested ) {
					Notify( () => ReconnectFailed?.Invoke() );
				}
			}
		}

		async Task<bool> ReconnectAsync( CancellationToken cancellationToken )
		{
			cancellationToken.ThrowIfCancellationRequested();
			Notify( () => Reconnecting?.Invoke() );
			var disconnected = await m_CncApi.DisconnectAsync().ConfigureAwait( false );
			cancellationToken.ThrowIfCancellationRequested();
			if( !disconnected.IsSuccess ) {
				Notify( () => ReconnectFailed?.Invoke() );
				return false;
			}
			for( int attempt = 1; attempt <= CncConstants.RECONNECT_MAX_ATTEMPTS; attempt++ ) {
				await Task.Delay( CncConstants.RECONNECT_INTERVAL_MS, cancellationToken ).ConfigureAwait( false );
				var connected = await m_CncApi.ConnectAsync( cancellationToken ).ConfigureAwait( false );
				cancellationToken.ThrowIfCancellationRequested();
				if( connected.IsSuccess ) {
					var subscribed = await m_CncApi.SubscribeAllEventsAsync().ConfigureAwait( false );
					cancellationToken.ThrowIfCancellationRequested();
					if( subscribed.IsSuccess ) {
						m_ConnectionLost = false;
						Notify( () => Reconnected?.Invoke() );
						return true;
					}
					await m_CncApi.DisconnectAsync().ConfigureAwait( false );
				}
				if( connected.Status == CncResultStatus.TimedOut || CncApi.HasPendingOperation ) {
					break;
				}
			}
			cancellationToken.ThrowIfCancellationRequested();
			Notify( () => ReconnectFailed?.Invoke() );
			return false;
		}

		// OpenCNC detected connection error, treat as connection lost
		void OnCncConnectionError()
		{
			m_ConnectionLost = true;
		}


		static void Notify( Action action )
		{
			try {
				action();
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC check subscriber failed: {0}", ex );
			}
		}

		readonly object m_Lock = new object();
		bool m_Disposed;
		volatile bool m_ConnectionLost;

		// Syntec OpenCNC API wrapper
		readonly CncApi m_CncApi;
		readonly int m_CheckConeectInterval;
		readonly int m_FailThreshold;

		CancellationTokenSource m_Cancellation;
		Task m_Task;
	}
}
