using System;
using System.Threading;
using System.Threading.Tasks;
using CncManager.Definition;

namespace CncManager.CNCCore
{
	// Use to against blocking by CNC API forever.
	public class CncApiTimeoutGuard
	{
		public CncApiTimeoutGuard( int defaultTimeoutMs = CncConstants.DEFAULT_API_TIMEOUT_MS )
		{
			if( defaultTimeoutMs <= 0 ) {
				throw new ArgumentOutOfRangeException( nameof( defaultTimeoutMs ) );
			}
			m_DefaultTimeoutMs = defaultTimeoutMs;
		}

		public bool HasPendingOperation
		{
			get
			{
				lock( m_Lock ) {
					return m_Pending != null && !m_Pending.IsCompleted;
				}
			}
		}

		public event Action<string> LateOperationFailed;

		public CncResult<T> Execute<T>( Func<T> func )
		{
			if( func == null ) {
				throw new ArgumentNullException( nameof( func ) );
			}
			var completion = new TaskCompletionSource<bool>( TaskCreationOptions.RunContinuationsAsynchronously );
			lock( m_Lock ) {
				if( m_Pending != null && !m_Pending.IsCompleted ) {
					return CncResult<T>.Failure( CncResultStatus.Busy, CncMessages.OperationBusy );
				}
				m_Pending = completion.Task;
			}
			try {
				return CncResult<T>.Success( func() );
			}
			catch( Exception ex ) {
				return CncResult<T>.Failure( CncResultStatus.Failed, ex.Message );
			}
			finally {
				completion.SetResult( true );
			}
		}

		public async Task<CncResult<T>> ExecuteAsync<T>( Func<T> func, int? timeoutMs = null,
			CancellationToken cancellationToken = default( CancellationToken ), Action<T> abandoned = null )
		{
			if( func == null ) {
				throw new ArgumentNullException( nameof( func ) );
			}
			int timeout = timeoutMs ?? m_DefaultTimeoutMs;
			if( timeout <= 0 ) {
				throw new ArgumentOutOfRangeException( nameof( timeoutMs ) );
			}
			if( cancellationToken.IsCancellationRequested ) {
				return CncResult<T>.Failure( CncResultStatus.Cancelled, CncMessages.OperationCancelled );
			}

			var completion = new TaskCompletionSource<CncResult<T>>( TaskCreationOptions.RunContinuationsAsynchronously );
			bool isAbandoned = false;
			Task operation;
			lock( m_Lock ) {
				if( m_Pending != null && !m_Pending.IsCompleted ) {
					return CncResult<T>.Failure( CncResultStatus.Busy, CncMessages.OperationBusy );
				}
				operation = m_Pending = Task.Run( () =>
				{
					T value = default( T );
					CncResult<T> result;
					try {
						value = func();
						result = CncResult<T>.Success( value );
					}
					catch( Exception ex ) {
						result = CncResult<T>.Failure( CncResultStatus.Failed, ex.Message );
					}
					bool cleanup;
					lock( m_Lock ) {
						cleanup = isAbandoned;
						if( !cleanup ) {
							completion.TrySetResult( result );
						}
					}
					if( cleanup ) {
						try {
							if( result.IsSuccess ) {
								abandoned?.Invoke( value );
							}
							else {
								LateOperationFailed?.Invoke( result.ErrorMessage );
							}
						}
						catch( Exception ex ) {
							System.Diagnostics.Trace.TraceError( "Late CNC cleanup failed: {0}", ex );
						}
					}
				} );
			}

			using( var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) ) {
				Task delay = Task.Delay( timeout, delayCancellation.Token );
				await Task.WhenAny( completion.Task, delay ).ConfigureAwait( false );
				lock( m_Lock ) {
					if( !completion.Task.IsCompleted ) {
						isAbandoned = true;
						return CncResult<T>.Failure(
							cancellationToken.IsCancellationRequested ? CncResultStatus.Cancelled : CncResultStatus.TimedOut,
							CncMessages.WaitStopped );
					}
				}
				delayCancellation.Cancel();
				await operation.ConfigureAwait( false );
				return await completion.Task.ConfigureAwait( false );
			}
		}

		public async Task<bool> WaitForIdleAsync( int timeoutMs )
		{
			Task pending;
			lock( m_Lock ) {
				pending = m_Pending;
			}
			if( pending == null || pending.IsCompleted ) {
				return true;
			}
			await Task.WhenAny( pending, Task.Delay( timeoutMs ) ).ConfigureAwait( false );
			return pending.IsCompleted;
		}

		readonly int m_DefaultTimeoutMs;
		readonly object m_Lock = new object();
		Task m_Pending;
	}
}
