using CncManager.CNCCore;
using CncManager.Definition;
using System;
using System.Threading;

namespace CncManager
{
	// CNC connection client facade for MyCAM.
	public class CncClient : IDisposable
	{
		public static CncClient Instance
		{
			get
			{
				if( m_Instance == null ) {
					m_Instance = new CncClient();
				}
				return m_Instance;
			}
		}

		// state
		public bool IsConnected
		{
			get
			{
				return CNCCore.CncApi.IsConnected;
			}
		}

		// events for MyCAM UI
		public event Action<ERecconectStage> ReconnectStatusChanged;
		public event Action<CncStatus> CncStatusChanged;
		public event Action<int, int> RRegisterChanged;

		// raised when connect fails, carries error message for logging
		public event Action<string> ConnectFailed;

		CncClient()
		{
		}

		// set UI thread context for event callback
		public void SetSynchronizationContext( SynchronizationContext context )
		{
			m_SyncContext = context;
		}

		public bool Connect( string ip, int timeoutMs = CncConstants.DEFAULT_CONNECT_TIMEOUT_MS )
		{
			if( IsConnected ) {
				return true;
			}

			// drop leftovers from a previous failed attempt before creating new instances
			ReleaseConnectionResources();

			// create ConnectChecker and CncApi instances
			m_CncApi = new CncApi( ip, timeoutMs );
			m_Checker = new CncConnectChecker( m_CncApi );

			// wire events
			m_CncApi.OnCncStatusChanged += OnCncStatusChanged;
			m_CncApi.RRegisterChanged += OnRRegisterChanged;
			m_CncApi.ConnectFailed += OnConnectFailed;

			m_Checker.Reconnecting += OnCheckerReconnecting;
			m_Checker.Reconnected += OnCheckerReconnected;
			m_Checker.ReconnectFailed += OnCheckerReconnectFailed;

			// try connect
			bool isSuccess = m_CncApi.Connect();
			if( !isSuccess ) {
				// roll back so the next Connect() starts from a clean state
				ReleaseConnectionResources();
				return false;
			}

			// only after a successful connect: subscribe OpenCNC events and start the heartbeat
			m_CncApi.SubscribeAllEvents();
			m_Checker.Start();
			return true;
		}

		public void Dispose()
		{
			// Disconnect() stops the connection and releases the api/checker instances
			Disconnect();
		}

		public void Disconnect()
		{
			// stop the heartbeat/reconnect loops first
			if( m_Checker != null ) {
				try {
					m_Checker.StopConnectLoop();
				}
				catch( Exception ) {
					// ignore
				}
			}
			if( m_CncApi != null ) {
				try {
					m_CncApi.UnsubscribeAllEvents();
					m_CncApi.Disconnect();
				}
				catch( Exception ) {
					// ignore
				}
			}
			// release instances so the next Connect() creates fresh ones
			ReleaseConnectionResources();
		}

		// Releases the CncApi and CncConnectChecker instances and unwires all events
		void ReleaseConnectionResources()
		{
			if( m_Checker != null ) {
				m_Checker.Reconnecting -= OnCheckerReconnecting;
				m_Checker.Reconnected -= OnCheckerReconnected;
				m_Checker.ReconnectFailed -= OnCheckerReconnectFailed;

				// Dispose() also unsubscribes from CncApi.ConnectionError
				m_Checker.Dispose();
				m_Checker = null;
			}
			if( m_CncApi != null ) {
				m_CncApi.OnCncStatusChanged -= OnCncStatusChanged;
				m_CncApi.RRegisterChanged -= OnRRegisterChanged;
				m_CncApi.ConnectFailed -= OnConnectFailed;
				m_CncApi = null;
			}
		}

		void OnCncStatusChanged( CncStatus status )
		{
			try {
				CncStatusChanged?.Invoke( status );
			}
			catch( Exception ) {
				// ignore
			}
		}

		void OnRRegisterChanged( int index, int value )
		{
			try {
				RRegisterChanged?.Invoke( index, value );
			}
			catch( Exception ) {
				// ignore
			}
		}

		void OnConnectFailed( string errorMessage )
		{
			try {
				ConnectFailed?.Invoke( errorMessage );
			}
			catch( Exception ) {
				// ignore
			}
		}

		// checker -> manager -> UI: forward reconnect events to subscribers
		void OnCheckerReconnecting()
		{
			NotifyUi( ERecconectStage.Reconnecting );
		}

		void OnCheckerReconnected()
		{
			NotifyUi( ERecconectStage.Reconnected );
		}

		void OnCheckerReconnectFailed()
		{
			NotifyUi( ERecconectStage.ReconnectFailed );
		}

		void NotifyUi( ERecconectStage eventType )
		{
			if( ReconnectStatusChanged == null ) {
				return;
			}

			// switch to UI thread if context set
			if( m_SyncContext != null ) {
				m_SyncContext.Post( RaiseReconnectStatusChangedOnUiThread, eventType );
			}
			else {
				try {
					ReconnectStatusChanged.Invoke( eventType );
				}
				catch( Exception ) {
					// ignore
				}
			}
		}

		void RaiseReconnectStatusChangedOnUiThread( object state )
		{
			try {
				ReconnectStatusChanged?.Invoke( (ERecconectStage)state );
			}
			catch( Exception ) {
				// ignore
			}
		}

		static CncClient m_Instance;
		CncApi m_CncApi;
		CncConnectChecker m_Checker;
		SynchronizationContext m_SyncContext;
	}
}
