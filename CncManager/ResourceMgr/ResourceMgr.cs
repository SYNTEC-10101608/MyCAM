using System;
using System.Diagnostics;
using System.Threading;
using CncManager.Definition;

namespace CncManager
{
	public sealed class ResourceMgr : IDisposable
	{
		public ResourceMgr( CncClient client, int coordID = CncConstants.FIRST_COORD_ID )
		{
			m_Client = client;
			RPool = new RRegisterPool();
			SystemVarPool = new SystemVarPool( coordID );
			QVarPool = new QVarPool( coordID );
			GlobalVarPool = new GlobalVarPool();
			KVarPool = new KVarPool();
			MainProgramPool = new MainProgramPool();
			m_Timer = new Timer( OnPolling, null, Timeout.Infinite, Timeout.Infinite );
		}

		public ResourcePool<int?> RPool
		{
			get;
		}

		public ResourcePool<double?> SystemVarPool
		{
			get;
		}

		public ResourcePool<int?> QVarPool
		{
			get;
		}

		public ResourcePool<double?> GlobalVarPool
		{
			get;
		}

		public ResourcePool<int?> KVarPool
		{
			get;
		}

		public ResourcePool<string> MainProgramPool
		{
			get;
		}

		public bool Init()
		{
			try {
				lock( m_Lock ) {
					if( m_Disposed || m_Initialized ) {
						return false;
					}
					if( m_Client == null ) {
						throw new InvalidOperationException( "CNC client is not available." );
					}
					m_Initialized = true;
					m_Client.ConnectionStateChanged += OnConnectionStateChanged;
					m_Timer.Change( CncConstants.RESOURCE_POLL_INTERVAL_MS, CncConstants.RESOURCE_POLL_INTERVAL_MS );
				}
				OnConnectionStateChanged( m_Client.ConnectionState );
				return true;
			}
			catch( Exception ex ) {
				Trace.TraceError( "CNC resource initialization failed: {0}", ex );
				Dispose();
				return false;
			}
		}

		void SetPoolsConnected( bool connected, int version )
		{
			RPool.SetConnected( connected, version );
			SystemVarPool.SetConnected( connected, version );
			QVarPool.SetConnected( connected, version );
			GlobalVarPool.SetConnected( connected, version );
			KVarPool.SetConnected( connected, version );
			MainProgramPool.SetConnected( connected, version );
		}

		void OnConnectionStateChanged( CncConnectionState state )
		{
			bool connected;
			int version;
			lock( m_Lock ) {
				if( !m_Initialized || m_Disposed ) {
					return;
				}
				connected = state == CncConnectionState.Connected && m_Client.IsConnected;
				if( m_Connected == connected ) {
					return;
				}
				m_Connected = connected;
				version = ++m_ConnectionVersion;
			}
			SetPoolsConnected( connected, version );
		}

		async void OnPolling( object state )
		{
			lock( m_Lock ) {
				if( !m_Initialized || !m_Connected || m_Disposed || m_Polling ) {
					return;
				}
				m_Polling = true;
			}
			try {
				await RPool.PollingAsync().ConfigureAwait( false );
				await SystemVarPool.PollingAsync().ConfigureAwait( false );
				await QVarPool.PollingAsync().ConfigureAwait( false );
				await GlobalVarPool.PollingAsync().ConfigureAwait( false );
				await KVarPool.PollingAsync().ConfigureAwait( false );
				await MainProgramPool.PollingAsync().ConfigureAwait( false );
			}
			catch( Exception ex ) {
				Trace.TraceError( "CNC resource update failed: {0}", ex );
			}
			finally {
				lock( m_Lock ) {
					m_Polling = false;
				}
			}
		}

		public void Dispose()
		{
			int version;
			lock( m_Lock ) {
				if( m_Disposed ) {
					return;
				}
				m_Disposed = true;
				m_Connected = false;
				version = ++m_ConnectionVersion;
				m_Timer.Dispose();
				if( m_Client != null ) {
					m_Client.ConnectionStateChanged -= OnConnectionStateChanged;
				}
			}
			SetPoolsConnected( false, version );
			RPool.Dispose();
			SystemVarPool.Dispose();
			QVarPool.Dispose();
			GlobalVarPool.Dispose();
			KVarPool.Dispose();
			MainProgramPool.Dispose();
		}

		readonly CncClient m_Client;
		readonly object m_Lock = new object();
		readonly Timer m_Timer;
		bool m_Initialized;
		bool m_Connected;
		bool m_Polling;
		bool m_Disposed;
		int m_ConnectionVersion;
	}
}
