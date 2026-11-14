using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CncManager.CNCCore;
using CncManager.Definition;

namespace CncManager
{
	// Shared connection lifecycle. Events may be raised on background threads.
	public class CncClient : IDisposable
	{
		public static CncClient Instance => m_Instance.Value;
		public bool IsConnected
		{
			get
			{
				return ConnectionState == CncConnectionState.Connected && CncApi.IsConnected;
			}
		}

		public CncConnectionState ConnectionState
		{
			get
			{
				lock( m_StateLock ) {
					return m_ConnectionState;
				}
			}
		}

		public string IP
		{
			get
			{
				lock( m_StateLock ) {
					return m_IP;
				}
			}
		}

		public event Action<CncConnectionState> ConnectionStateChanged;
		public event Action<ERecconectStage> ReconnectStatusChanged;
		public event Action<CncStatus> CncStatusChanged;
		public event Action<int, int> RRegisterChanged;
		public event Action<string> ConnectFailed;
		public event Action<string> OperationFailed;
		public event Action ResetHappened;
		public event Action AlarmChanged;
		public event Action ParameterChanged;

		CncClient()
		{
		}

		public bool Connect( string ip, int timeoutMs = CncConstants.DEFAULT_CONNECT_TIMEOUT_MS )
		{
			return ConnectAsync( ip, timeoutMs ).GetAwaiter().GetResult().IsSuccess;
		}

		public async Task<CncResult<bool>> ConnectAsync( string ip,
			int timeoutMs = CncConstants.DEFAULT_CONNECT_TIMEOUT_MS, CancellationToken cancellationToken = default( CancellationToken ) )
		{
			ip = ip?.Trim();
			if( !IPAddress.TryParse( ip, out IPAddress address ) || timeoutMs <= 0 ) {
				return CncResult<bool>.Failure( CncResultStatus.Failed, CncMessages.InvalidConnection );
			}
			if( !await m_Lifecycle.WaitAsync( 0 ).ConfigureAwait( false ) ) {
				return CncResult<bool>.Failure( CncResultStatus.Busy, CncMessages.LifecycleBusy );
			}
			try {
				if( cancellationToken.IsCancellationRequested ) {
					return CncResult<bool>.Failure( CncResultStatus.Cancelled, CncMessages.ConnectionCancelled );
				}
				if( IsConnected && IP == ip ) {
					return CncResult<bool>.Success( true );
				}
				var cleanup = await DisconnectCoreAsync().ConfigureAwait( false );
				if( !cleanup.IsSuccess || CncApi.HasPendingOperation ) {
					return cleanup.IsSuccess ? CncResult<bool>.Failure( CncResultStatus.Busy, CncMessages.PreviousOperationRunning ) : cleanup;
				}
				lock( m_StateLock ) {
					m_ConnectionCancellation = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
				}
				CancellationToken token = m_ConnectionCancellation.Token;
				SetConnectionState( CncConnectionState.Connecting, ip );
				m_CncApi = new CncApi( ip, timeoutMs );
				WireApi( m_CncApi );
				m_Checker = new CncConnectChecker( m_CncApi );
				m_Checker.Reconnecting += OnCheckerReconnecting;
				m_Checker.Reconnected += OnCheckerReconnected;
				m_Checker.ReconnectFailed += OnCheckerReconnectFailed;
				var result = await m_CncApi.ConnectAsync( token ).ConfigureAwait( false );
				if( result.IsSuccess ) {
					result = await m_CncApi.SubscribeAllEventsAsync().ConfigureAwait( false );
				}
				if( token.IsCancellationRequested ) {
					result = CncResult<bool>.Failure( CncResultStatus.Cancelled, CncMessages.ConnectionCancelled );
				}
				if( !result.IsSuccess ) {
					await DisconnectCoreAsync().ConfigureAwait( false );
					SetConnectionState( result.Status == CncResultStatus.Cancelled ? CncConnectionState.Disconnected : CncConnectionState.Error, ip );
					if( result.Status != CncResultStatus.Cancelled ) {
						Raise( ConnectFailed, result.ErrorMessage );
					}
					return result;
				}
				SetConnectionState( CncConnectionState.Connected, ip );
				await RefreshStatusAsync().ConfigureAwait( false );
				if( !token.IsCancellationRequested && IsConnected ) {
					m_Checker.Start();
					return CncResult<bool>.Success( true );
				}
				await DisconnectCoreAsync().ConfigureAwait( false );
				return CncResult<bool>.Failure( CncResultStatus.Cancelled, CncMessages.SessionEnded );
			}
			catch( Exception ex ) {
				await DisconnectCoreAsync().ConfigureAwait( false );
				SetConnectionState( CncConnectionState.Error, ip );
				Raise( ConnectFailed, ex.Message );
				return CncResult<bool>.Failure( CncResultStatus.Failed, ex.Message );
			}
			finally {
				m_Lifecycle.Release();
			}
		}

		public void Disconnect()
		{
			DisconnectAsync().GetAwaiter().GetResult();
		}

		public async Task<CncResult<bool>> DisconnectAsync()
		{
			lock( m_StateLock ) {
				m_ConnectionCancellation?.Cancel();
			}
			await m_Lifecycle.WaitAsync().ConfigureAwait( false );
			try {
				return await DisconnectCoreAsync().ConfigureAwait( false );
			}
			finally {
				m_Lifecycle.Release();
			}
		}

		public void Dispose()
		{
			Disconnect();
		}

		async Task<CncResult<bool>> DisconnectCoreAsync()
		{
			m_CncApi?.Invalidate();
			lock( m_StateLock ) {
				m_ConnectionCancellation?.Cancel();
			}
			if( m_Checker != null ) {
				await m_Checker.StopConnectLoopAsync().ConfigureAwait( false );
				m_Checker.Reconnecting -= OnCheckerReconnecting;
				m_Checker.Reconnected -= OnCheckerReconnected;
				m_Checker.ReconnectFailed -= OnCheckerReconnectFailed;
				m_Checker.Dispose();
				m_Checker = null;
			}
			m_UnwireApi?.Invoke();
			m_UnwireApi = null;
			var result = CncResult<bool>.Success( true );
			if( m_CncApi != null ) {
				result = await m_CncApi.DisconnectAsync().ConfigureAwait( false );
				if( result.IsSuccess ) {
					m_CncApi = null;
				}
			}
			lock( m_StateLock ) {
				m_ConnectionCancellation?.Dispose();
				m_ConnectionCancellation = null;
			}
			SetConnectionState( result.IsSuccess ? CncConnectionState.Disconnected : CncConnectionState.Error, IP );
			return result;
		}

		void WireApi( CncApi api )
		{
			Action<CncStatus> status = value => { if( m_CncApi == api ) { OnCncStatusChanged( value ); } };
			Action<int, int> register = ( index, value ) => { if( m_CncApi == api ) { Raise( RRegisterChanged, handler => ( (Action<int, int>)handler )( index, value ) ); } };
			Action<string> error = value => { if( m_CncApi == api ) { Raise( OperationFailed, value ); } };
			Action lost = () => { if( m_CncApi == api ) { SetConnectionState( CncConnectionState.Reconnecting, IP ); } };
			EventHandler reset = ( sender, args ) => Raise( ResetHappened, handler => ( (Action)handler )() );
			EventHandler alarm = ( sender, args ) => Raise( AlarmChanged, handler => ( (Action)handler )() );
			EventHandler param = ( sender, args ) => Raise( ParameterChanged, handler => ( (Action)handler )() );
			api.OnCncStatusChanged += status;
			api.RRegisterChanged += register;
			api.OperationFailed += error;
			api.ConnectionError += lost;
			api.OnCNCResetHappen += reset;
			api.OnCNCAlarmChanged += alarm;
			api.OnParamChanged += param;
			m_UnwireApi = () =>
			{
				api.OnCncStatusChanged -= status;
				api.RRegisterChanged -= register;
				api.OperationFailed -= error;
				api.ConnectionError -= lost;
				api.OnCNCResetHappen -= reset;
				api.OnCNCAlarmChanged -= alarm;
				api.OnParamChanged -= param;
			};
		}

		void OnCncStatusChanged( CncStatus status )
		{
			lock( m_StateLock ) {
				if( !CncApi.IsConnected || m_ConnectionCancellation == null || m_ConnectionCancellation.IsCancellationRequested ||
					( m_ConnectionState != CncConnectionState.Connected && m_ConnectionState != CncConnectionState.Connecting ) ) {
					return;
				}
			}
			Raise( CncStatusChanged, status );
		}

		public async Task<CncResult<CncStatus>> RefreshStatusAsync()
		{
			CncApi api = m_CncApi;
			CncResult<CncStatus> result;
			do {
				result = await CncApi.GetCncStatusAsync().ConfigureAwait( false );
				if( result.Status == CncResultStatus.Busy ) {
					await Task.Delay( CncConstants.PLC_SCANNING_TIME_MS ).ConfigureAwait( false );
				}
			} while( m_CncApi == api && IsConnected && result.Status == CncResultStatus.Busy );
			if( api != null && m_CncApi == api && IsConnected && result.IsSuccess ) {
				OnCncStatusChanged( result.Data );
			}
			return result;
		}

		void SetConnectionState( CncConnectionState state, string ip )
		{
			bool changed;
			lock( m_StateLock ) {
				changed = m_ConnectionState != state;
				m_ConnectionState = state;
				m_IP = ip;
			}
			if( changed ) {
				Raise( ConnectionStateChanged, state );
			}
		}

		// checker -> manager -> UI: forward reconnect events to subscribers
		void OnCheckerReconnecting()
		{
			if( IsStopping ) {
				return;
			}
			SetConnectionState( CncConnectionState.Reconnecting, IP );
			Raise( ReconnectStatusChanged, ERecconectStage.Reconnecting );
		}

		async void OnCheckerReconnected()
		{
			try {
				if( IsStopping || !CncApi.IsConnected ) {
					return;
				}
				SetConnectionState( CncConnectionState.Connected, IP );
				Raise( ReconnectStatusChanged, ERecconectStage.Reconnected );
				await RefreshStatusAsync().ConfigureAwait( false );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC reconnect status refresh failed: {0}", ex );
				if( !IsStopping ) {
					Raise( OperationFailed, ex.Message );
				}
			}
		}

		void OnCheckerReconnectFailed()
		{
			if( IsStopping ) {
				return;
			}
			SetConnectionState( CncConnectionState.Error, IP );
			Raise( ReconnectStatusChanged, ERecconectStage.ReconnectFailed );
		}

		static void Raise<T>( Action<T> handlers, T value )
		{
			Raise( handlers, handler => ( (Action<T>)handler )( value ) );
		}

		static void Raise( Delegate handlers, Action<Delegate> invoke )
		{
			if( handlers == null ) {
				return;
			}
			foreach( Delegate handler in handlers.GetInvocationList() ) {
				try {
					invoke( handler );
				}
				catch( Exception ex ) {
					System.Diagnostics.Trace.TraceError( "CNC subscriber failed: {0}", ex );
				}
			}
		}

		static readonly Lazy<CncClient> m_Instance = new Lazy<CncClient>( () => new CncClient() );
		bool IsStopping
		{
			get
			{
				lock( m_StateLock ) {
					return m_ConnectionCancellation == null || m_ConnectionCancellation.IsCancellationRequested;
				}
			}
		}

		readonly SemaphoreSlim m_Lifecycle = new SemaphoreSlim( 1, 1 );
		readonly object m_StateLock = new object();
		CncConnectionState m_ConnectionState = CncConnectionState.Disconnected;
		string m_IP;
		CancellationTokenSource m_ConnectionCancellation;
		CncApi m_CncApi;
		CncConnectChecker m_Checker;
		Action m_UnwireApi;
	}
}
