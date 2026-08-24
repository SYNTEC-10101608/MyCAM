using CncManager.Definition;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CncManager.CNCCore
{
	// Detect if CNC is still alive, and auto-reconnect when connection is lost.
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
			if( m_HeartbeatCancellation != null ) {
				return;
			}

			// PC-alive signal to CNC
			CncApi.WriteRRegister( CncConstants.R_CNC_AND_PC_HANDSHAKE, CncConstants.HANDSHAKE_PC_WRITE_VALUE );

			m_HeartbeatCancellation = new CancellationTokenSource();
			m_HeartbeatFailureCount = 0;
			m_HeartbeatTask = Task.Run( () => HeartbeatLoop( m_HeartbeatCancellation.Token ) );
		}

		public void StopConnectLoop()
		{
			// Cancel any in-progress reconnect first so it does not restart heartbeat.
			if( m_ReconnectCancellation != null ) {
				try {
					m_ReconnectCancellation.Cancel();

					// wait for the background task to exit before releasing its handle
					m_ReconnectTask?.Wait( CncConstants.TASK_SHUTDOWN_WAIT_MS );
				}
				catch( Exception ) {
					// ignore
				}
				m_ReconnectCancellation?.Dispose();
				m_ReconnectCancellation = null;
				m_ReconnectTask = null;
			}

			if( m_HeartbeatCancellation != null ) {
				try {
					m_HeartbeatCancellation.Cancel();

					// wait for the background task to exit before releasing its handle
					m_HeartbeatTask?.Wait( CncConstants.TASK_SHUTDOWN_WAIT_MS );
				}
				catch( Exception ) {
					// ignore
				}
				m_HeartbeatCancellation?.Dispose();
				m_HeartbeatCancellation = null;
				m_HeartbeatTask = null;
			}
		}

		public void Dispose()
		{
			m_CncApi.ConnectionError -= OnCncConnectionError;
			StopConnectLoop();
		}

		async Task HeartbeatLoop( CancellationToken cancellationToken )
		{
			while( !cancellationToken.IsCancellationRequested ) {
				// Wait for the next heartbeat check interval, or exit if cancelled.
				try {
					await Task.Delay( m_CheckConeectInterval, cancellationToken );
				}
				catch( OperationCanceledException ) {
					break;
				}

				if( !CncApi.IsConnected ) {
					return;
				}

				// Check the heartbeat and raise the event. Ignore any exceptions from the event handlers.
				bool isAlive = CheckHeartbeat();
				try {
					HeartbeatChecked?.Invoke( isAlive );
				}
				catch( Exception ) {
					// ignore
				}

				// Reset the failure count if the heartbeat is successful, otherwise increment it.
				if( isAlive ) {
					m_HeartbeatFailureCount = 0;
				}
				else {
					m_HeartbeatFailureCount++;

					// If the heartbeat fails too many times in a row, assume the connection is lost and start the reconnect flow.
					if( m_HeartbeatFailureCount >= m_FailThreshold ) {
						m_HeartbeatFailureCount = 0;

						// Stop the heartbeat loop and hand over to the reconnect flow
						StartReconnect();
						return;
					}
				}
			}
		}

		bool CheckHeartbeat()
		{
			try {
				// Read the handshake register to check if the CNC is alive
				int? value = CncApi.ReadRRegister( CncConstants.R_CNC_AND_PC_HANDSHAKE );
				if( !value.HasValue ) {
					return false;
				}

				// The CNC should echo back the value we wrote (5) as 4. If it is not 4, the CNC is not responding.
				if( value.Value != CncConstants.HANDSHAKE_CNC_ECHO_VALUE ) {
					return false;
				}

				// Reset echo so CNC can flip it back to 4 on next PLC scan
				CncApi.WriteRRegister( CncConstants.R_CNC_AND_PC_HANDSHAKE, CncConstants.HANDSHAKE_PC_WRITE_VALUE );
				return true;
			}
			catch( Exception ) {
				return false;
			}
		}

		// Launch the reconnect flow on its own task so the heartbeat loop can exit cleanly.
		void StartReconnect()
		{
			lock( m_ReconnectLock ) {
				if( m_ReconnectCancellation != null ) {
					return;
				}

				// The heartbeat loop is exiting; cancel and clear its handle so a later Start() works
				if( m_HeartbeatCancellation != null ) {
					try {
						m_HeartbeatCancellation.Cancel();
					}
					catch( Exception ) {
						// ignore
					}
					m_HeartbeatCancellation.Dispose();
					m_HeartbeatCancellation = null;
				}
				m_HeartbeatTask = null;

				m_ReconnectCancellation = new CancellationTokenSource();
				m_ReconnectTask = Task.Run( () => ReconnectLoop( m_ReconnectCancellation.Token ) );
			}
		}

		async Task ReconnectLoop( CancellationToken cancellationToken )
		{
			// only raise Reconnecting once per reconnect session
			if( !m_IsReconnecting ) {
				m_IsReconnecting = true;
				try {
					Reconnecting?.Invoke();
				}
				catch( Exception ) {
				}
			}

			// Ensure the old (dead) connection is cleaned up before retrying.
			try {
				m_CncApi.Disconnect();
			}
			catch( Exception ) {
				// ignore
			}

			for( int attempt = 1; attempt <= CncConstants.RECONNECT_MAX_ATTEMPTS; attempt++ ) {
				if( cancellationToken.IsCancellationRequested ) {
					return;
				}

				try {
					await Task.Delay( CncConstants.RECONNECT_INTERVAL_MS, cancellationToken );
				}
				catch( OperationCanceledException ) {
					return;
				}

				bool connected = false;
				try {
					connected = m_CncApi.Connect( raiseConnectFailed: false );
				}
				catch( Exception ) {
					// ignore
				}

				if( connected ) {
					// Restart heartbeat (Start() writes the PC-alive handshake first).
					m_ReconnectCancellation?.Dispose();
					m_ReconnectCancellation = null;
					m_ReconnectTask = null;
					Start();

					try {
						Reconnected?.Invoke();
					}
					catch( Exception ) {
						// ignore
					}
					m_IsReconnecting = false;
					return;
				}
			}

			m_ReconnectCancellation?.Dispose();
			m_ReconnectCancellation = null;
			m_ReconnectTask = null;

			try {
				ReconnectFailed?.Invoke();
			}
			catch( Exception ) {
				// ignore
			}
			m_IsReconnecting = false;
		}

		// OpenCNC detected connection error, treat as connection lost
		void OnCncConnectionError()
		{
			if( m_IsReconnecting ) {
				return;
			}
			StartReconnect();
		}


		// state to prevent duplicate event
		bool m_IsReconnecting = false;

		// serializes StartReconnect() against concurrent callers
		readonly object m_ReconnectLock = new object();

		// Syntec OpenCNC API wrapper
		readonly CncApi m_CncApi;
		readonly int m_CheckConeectInterval;
		readonly int m_FailThreshold;

		CancellationTokenSource m_HeartbeatCancellation;
		Task m_HeartbeatTask;
		int m_HeartbeatFailureCount;

		CancellationTokenSource m_ReconnectCancellation;
		Task m_ReconnectTask;
	}
}
