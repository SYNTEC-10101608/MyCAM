using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CncManager;
using CncManager.CNCCore;
using CncManager.Definition;

namespace CncUI
{
	internal enum CNCAlarmStatus
	{
		None,
		Updating,
		Disconnected,
		ReadFailed,
	}

	internal sealed class CNCAlarmMonitor : IDisposable
	{
		public CNCAlarmMonitor( CncClient client )
		{
			m_Client = client;
		}

		public IReadOnlyList<CncAlarm> Alarms { get; private set; } = Array.AsReadOnly( new CncAlarm[ 0 ] );
		public CNCAlarmStatus Status { get; private set; }
		public string Error { get; private set; }
		public bool IsCurrentData { get; private set; }
		public int? Count => IsCurrentData ? (int?)Alarms.Count : null;
		public bool HasContent => Alarms.Count > 0 || Status != CNCAlarmStatus.None;

		public event Action Changed;

		public void Init()
		{
			if( m_Initialized || m_Disposed || m_Client == null ) {
				throw new InvalidOperationException( CNCUIManager.Language.TryGetString( "Error_AlarmUnavailable", ALARM_UNAVAILABLE ) );
			}
			m_Context = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			if( m_Context == null ) {
				throw new InvalidOperationException( CNCUIManager.Language.TryGetString( "Error_AlarmUIThread", ALARM_UI_THREAD ) );
			}
			m_Initialized = true;
			m_Client.AlarmChanged += OnAlarmChanged;
			m_Client.ConnectionStateChanged += OnConnectionStateChanged;
			SetConnected( m_Client.IsConnected );
		}

		public void Dispose()
		{
			if( m_Disposed ) {
				return;
			}
			m_Disposed = true;
			if( m_Client != null ) {
				m_Client.AlarmChanged -= OnAlarmChanged;
				m_Client.ConnectionStateChanged -= OnConnectionStateChanged;
			}
			m_Context = null;
			m_Session = null;
			m_Pending = false;
			m_Connected = false;
			Alarms = Array.AsReadOnly( new CncAlarm[ 0 ] );
			Status = CNCAlarmStatus.None;
			Error = null;
			IsCurrentData = false;
		}

		void OnAlarmChanged()
		{
			Post( RequestRead );
		}

		void OnConnectionStateChanged( CncConnectionState state )
		{
			bool connected = state == CncConnectionState.Connected && m_Client.IsConnected;
			Post( () => SetConnected( connected ) );
		}

		void SetConnected( bool connected )
		{
			if( m_Connected == connected ) {
				return;
			}
			m_Connected = connected;
			m_Session = connected ? new object() : null;
			m_Pending = false;
			if( connected && m_IP != m_Client.IP ) {
				Alarms = Array.AsReadOnly( new CncAlarm[ 0 ] );
				m_IP = m_Client.IP;
			}
			IsCurrentData = false;
			bool needsUpdate = Alarms.Count > 0 || m_Reading || Status != CNCAlarmStatus.None;
			Status = needsUpdate ? ( connected ? CNCAlarmStatus.Updating : CNCAlarmStatus.Disconnected ) : CNCAlarmStatus.None;
			Error = null;
			NotifyChanged();
			if( connected ) {
				RequestRead();
			}
		}

		async void RequestRead()
		{
			if( m_Disposed || !m_Connected || !m_Client.IsConnected ) {
				return;
			}
			m_Pending = true;
			if( m_Reading ) {
				return;
			}
			m_Reading = true;
			object session = m_Session;
			try {
				while( IsCurrent( session ) && m_Pending ) {
					m_Pending = false;
					CncResult<CncAlarm[]> result;
					do {
						result = await CncApi.ReadPendingAlarmsAsync();
						if( result.Status == CncResultStatus.Busy && IsCurrent( session ) ) {
							await Task.Delay( CncConstants.PLC_SCANNING_TIME_MS );
						}
					} while( result.Status == CncResultStatus.Busy && IsCurrent( session ) );
					if( !IsCurrent( session ) ) {
						return;
					}
					if( result.IsSuccess && result.Data != null ) {
						ApplySnapshot( result.Data );
					}
					else {
						ReadFailed( result.ErrorMessage );
					}
				}
			}
			catch( Exception ex ) {
				if( IsCurrent( session ) ) {
					ReadFailed( ex.Message );
				}
			}
			finally {
				m_Reading = false;
				if( !m_Disposed && m_Pending && m_Connected && m_Client.IsConnected ) {
					RequestRead();
				}
			}
		}

		bool IsCurrent( object session )
		{
			return !m_Disposed && m_Connected && m_Client.IsConnected && m_Session == session;
		}

		void ApplySnapshot( CncAlarm[] alarms )
		{
			Dictionary<string, CncAlarm> current = new Dictionary<string, CncAlarm>();
			foreach( CncAlarm alarm in alarms ) {
				current[ alarm.Key ] = alarm;
			}
			Alarms = new List<CncAlarm>( current.Values ).AsReadOnly();
			IsCurrentData = true;
			Status = CNCAlarmStatus.None;
			Error = null;
			NotifyChanged();
		}

		void ReadFailed( string error )
		{
			IsCurrentData = false;
			Status = CNCAlarmStatus.ReadFailed;
			Error = error;
			NotifyChanged();
		}

		void NotifyChanged()
		{
			try {
				Changed?.Invoke();
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC alarm notification failed: {0}", ex );
			}
		}

		void Post( Action action )
		{
			SynchronizationContext context = m_Context;
			if( context == null || m_Disposed ) {
				return;
			}
			try {
				context.Post( state =>
				{
					try {
						if( !m_Disposed ) {
							action();
						}
					}
					catch( Exception ex ) {
						System.Diagnostics.Trace.TraceError( "CNC alarm update failed: {0}", ex );
					}
				}, null );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC alarm update scheduling failed: {0}", ex );
			}
		}

		readonly CncClient m_Client;
		SynchronizationContext m_Context;
		string m_IP;
		object m_Session;
		bool m_Initialized;
		volatile bool m_Disposed;
		bool m_Connected;
		bool m_Reading;
		bool m_Pending;

		const string ALARM_UNAVAILABLE = "CNC 警報監控無法使用。";
		const string ALARM_UI_THREAD = "請在 WinForms UI 執行緒初始化 CNC 警報監控。";
	}
}
