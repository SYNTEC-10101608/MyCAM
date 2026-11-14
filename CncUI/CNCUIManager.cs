using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CncManager;
using CncManager.CNCCore;
using CncManager.Definition;
using CncUI.Dialog;

namespace CncUI
{
	public sealed class CNCUIManager : IDisposable
	{
		public CNCUIManager( CncClient client, Action<string> saveIP = null,
			Func<string, CancellationToken, Task<CncResult<string>>> prepareProgram = null,
			Action<Form, IWin32Window> showModelessDialog = null )
		{
			m_Client = client;
			m_SaveIP = saveIP;
			m_PrepareProgram = prepareProgram;
			ShowModelessDialog = showModelessDialog;
			Resources = new ResourceMgr( client );
			m_AlarmMonitor = new CNCAlarmMonitor( client );
		}

		public CNCPanel Panel
		{
			get;
			private set;
		}

		public CNCInfoPanel InfoPanel
		{
			get;
			private set;
		}

		public bool IsConnected => !m_Stopped && !m_Disposed && m_Client != null && m_Client.IsConnected;

		public bool HasPanels => Panel != null && InfoPanel != null;
		internal Action<Form, IWin32Window> ShowModelessDialog { get; }
		internal IReadOnlyList<CncAxis> Axes { get; private set; } = Array.AsReadOnly( new CncAxis[ 0 ] );
		internal event Action AxesChanged;
		public CNCAlarmPanel AlarmPanel { get; private set; }
		public bool HasAlarmContent => !m_Stopped && !m_Disposed && m_AlarmMonitor.HasContent;

		public ResourceMgr Resources
		{
			get;
		}

		public ResourcePool<int?> RPool
		{
			get
			{
				return Resources.RPool;
			}
		}

		public event Action<string, bool> Notice;
		public event Action<bool> ConnectionChanged;
		public event Action ControllerRequested;
		public event Action ThreePointCalibrationRequested;
		public event Action AlarmsChanged;

		public bool Init()
		{
			if( m_Disposed || m_Stopped || m_Client == null ) {
				ReportInitError( Language.TryGetString( "Error_Unavailable", UNAVAILABLE_ERROR ) );
				Dispose();
				return false;
			}
			if( m_Initialized ) {
				ReportInitError( Language.TryGetString( "Error_AlreadyInitialized", ALREADY_INITIALIZED ) );
				return false;
			}
			m_Context = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			if( m_Context == null ) {
				ReportInitError( Language.TryGetString( "Error_UIThread", UI_THREAD_ERROR ) );
				Dispose();
				return false;
			}
			try {
				m_UIThreadID = Thread.CurrentThread.ManagedThreadId;
				m_Initialized = true;
				m_Client.ReconnectStatusChanged += OnReconnectStatusChanged;
				m_Client.OperationFailed += OnOperationFailed;
				m_Client.ConnectionStateChanged += OnConnectionStateChanged;
				if( !Resources.Init() ) {
					ReportInitError( Language.TryGetString( "Error_Resources", RESOURCES_ERROR ) );
					Dispose();
					return false;
				}
				AlarmPanel = new CNCAlarmPanel();
				m_AlarmMonitor.Changed += OnAlarmsChanged;
				m_AlarmMonitor.Init();
				OnAlarmsChanged();
				OnConnectionStateChanged( m_Client.ConnectionState );
				return true;
			}
			catch( Exception ex ) {
				ReportInitError( ex.Message );
				Dispose();
				return false;
			}
		}

		public bool CreatePanels()
		{
			if( !m_Initialized || m_Stopped || m_Disposed || Thread.CurrentThread.ManagedThreadId != m_UIThreadID ) {
				return false;
			}
			if( HasPanels ) {
				return true;
			}
			try {
				Panel = new CNCPanel();
				InfoPanel = new CNCInfoPanel();
				m_Controller = new CNCPanelController( Panel, m_Client, this );
				m_InfoController = new CNCInfoPanelController( InfoPanel, Resources, this );
				string error;
				if( !m_Controller.Init( out error ) || !m_InfoController.Init( out error ) ) {
					throw new InvalidOperationException( error );
				}
				Panel.Disposed += OnPanelDisposed;
				InfoPanel.Disposed += OnPanelDisposed;
				RefreshAxes( IsConnected );
				return true;
			}
			catch( Exception ex ) {
				ClosePanels();
				ReportInitError( ex.Message );
				return false;
			}
		}

		public void ShowCalibration()
		{
			if( !m_Initialized || m_Stopped || m_Disposed || !HasPanels || Thread.CurrentThread.ManagedThreadId != m_UIThreadID ) {
				return;
			}
			ThreePointCalibrationRequested?.Invoke();
		}

		public void ShowOffset( IWin32Window owner )
		{
			if( !m_Initialized || m_Stopped || m_Disposed || Thread.CurrentThread.ManagedThreadId != m_UIThreadID ) {
				return;
			}
			if( m_OffsetDialog != null && !m_OffsetDialog.IsDisposed ) {
				m_OffsetDialog.Activate();
				m_OffsetDialog.BringToFront();
				return;
			}
			try {
				m_OffsetDialog = new CNCOffsetDialog();
				m_OffsetController = new CNCOffsetController( m_OffsetDialog, Resources, this );
				string error;
				if( !m_OffsetController.Init( out error ) ) {
					throw new InvalidOperationException( error );
				}
				m_OffsetDialog.FormClosed += OnOffsetDialogClosed;
				m_OffsetDialog.Disposed += OnOffsetDialogClosed;
				if( ShowModelessDialog != null ) {
					ShowModelessDialog( m_OffsetDialog, owner );
				}
				else if( owner != null ) {
					m_OffsetDialog.Show( owner );
				}
				else {
					m_OffsetDialog.Show();
				}
			}
			catch( Exception ex ) {
				CloseOffsetDialog();
				Report( string.Format( Language.TryGetString( "Error_Offset", OFFSET_ERROR ), ex.Message ), true );
			}
		}

		public void ShowProcessSettings()
		{
			if( !m_Initialized || m_Stopped || m_Disposed || !HasPanels || Thread.CurrentThread.ManagedThreadId != m_UIThreadID ) {
				return;
			}
			m_Controller?.ShowProcessSettings();
		}

		public void ApplyResources()
		{
			if( m_Stopped || m_Disposed ) {
				return;
			}
			Panel?.ApplyResources();
			InfoPanel?.ApplyResources();
			AlarmPanel?.ApplyResources();
			if( m_OffsetDialog != null && !m_OffsetDialog.IsDisposed ) {
				m_OffsetDialog.ApplyResources();
			}
		}

		public void ClosePanels()
		{
			CloseOffsetDialog();
			CNCPanel panel = Panel;
			CNCInfoPanel infoPanel = InfoPanel;
			Panel = null;
			InfoPanel = null;
			if( panel != null ) {
				panel.Disposed -= OnPanelDisposed;
			}
			if( infoPanel != null ) {
				infoPanel.Disposed -= OnPanelDisposed;
			}
			m_Controller?.Dispose();
			m_Controller = null;
			m_InfoController?.Dispose();
			m_InfoController = null;
			if( panel != null && !panel.IsDisposed && !panel.Disposing ) {
				panel.Dispose();
			}
			if( infoPanel != null && !infoPanel.IsDisposed && !infoPanel.Disposing ) {
				infoPanel.Dispose();
			}
		}

		internal async Task<CncResult<string>> PrepareProgramAsync( string ip, CancellationToken cancellationToken )
		{
			if( m_PreparationRunning ) {
				return CncResult<string>.Failure( CncResultStatus.Busy, Language.TryGetString( "Error_PreparationBusy", PREPARATION_BUSY ) );
			}
			if( m_PrepareProgram == null ) {
				return CncResult<string>.Failure( CncResultStatus.Failed, Language.TryGetString( "Error_PreparationUnavailable", PREPARATION_UNAVAILABLE ) );
			}
			m_PreparationRunning = true;
			try {
				using( CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, m_Shutdown.Token ) ) {
					linked.Token.ThrowIfCancellationRequested();
					return await m_PrepareProgram( ip, linked.Token );
				}
			}
			finally {
				m_PreparationRunning = false;
			}
		}

		public async Task<CncResult<bool>> ConnectAsync( string ip )
		{
			ip = ip?.Trim();
			if( !m_Initialized || m_Stopped || m_Disposed ) {
				return CncResult<bool>.Failure( CncResultStatus.Cancelled, Language.TryGetString( "Error_NotInitialized", NOT_INITIALIZED ) );
			}
			if( !IPAddress.TryParse( ip, out IPAddress address ) ) {
				return CncResult<bool>.Failure( CncResultStatus.Failed, Language.TryGetString( "Error_InvalidIP", INVALID_IP ) );
			}
			if( Interlocked.CompareExchange( ref m_ConnectionOperationRunning, 1, 0 ) != 0 ) {
				return CncResult<bool>.Failure( CncResultStatus.Busy, Language.TryGetString( "Error_ConnectionBusy", CONNECTION_BUSY ) );
			}
			try {
				m_SaveIP?.Invoke( ip );
				var result = await m_Client.ConnectAsync( ip, cancellationToken: m_Shutdown.Token ).ConfigureAwait( false );
				Report( result.IsSuccess ? Language.TryGetString( "Notice_Connected", CONNECTED )
					: string.Format( Language.TryGetString( "Error_Connect", CONNECT_ERROR ), result.ErrorMessage ), !result.IsSuccess );
				return result;
			}
			catch( Exception ex ) {
				Report( string.Format( Language.TryGetString( "Error_Connect", CONNECT_ERROR ), ex.Message ), true );
				return CncResult<bool>.Failure( CncResultStatus.Failed, ex.Message );
			}
			finally {
				Interlocked.Exchange( ref m_ConnectionOperationRunning, 0 );
			}
		}

		public Task<CncResult<bool>> StopAsync()
		{
			if( m_StopTask != null ) {
				return m_StopTask;
			}
			m_Stopped = true;
			m_StopTask = StopCoreAsync();
			return m_StopTask;
		}

		async Task<CncResult<bool>> StopCoreAsync()
		{
			try {
				m_AxesCancellation?.Cancel();
				m_Context = null;
				if( m_Client != null ) {
					m_Client.ReconnectStatusChanged -= OnReconnectStatusChanged;
					m_Client.OperationFailed -= OnOperationFailed;
					m_Client.ConnectionStateChanged -= OnConnectionStateChanged;
				}
				m_AlarmMonitor.Changed -= OnAlarmsChanged;
				m_AlarmMonitor.Dispose();
				AlarmPanel?.Dispose();
				AlarmPanel = null;
				ClosePanels();
				Resources.Dispose();
				m_Shutdown.Cancel();
				if( m_Client == null ) {
					return CncResult<bool>.Failure( CncResultStatus.Failed, Language.TryGetString( "Error_ClientUnavailable", CLIENT_UNAVAILABLE ) );
				}
				return await m_Client.DisconnectAsync().ConfigureAwait( false );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC module shutdown failed: {0}", ex );
				return CncResult<bool>.Failure( CncResultStatus.Failed, ex.Message );
			}
		}

		public void Dispose()
		{
			if( m_Disposed ) {
				return;
			}
			m_Disposed = true;
			var result = StopAsync().GetAwaiter().GetResult();
			if( !result.IsSuccess ) {
				System.Diagnostics.Trace.TraceError( "CNC module shutdown not confirmed: {0}", result.ErrorMessage );
			}
			m_Shutdown.Dispose();
		}

		void OnPanelDisposed( object sender, EventArgs e )
		{
			ClosePanels();
		}

		void OnOffsetDialogClosed( object sender, EventArgs e )
		{
			if( sender == m_OffsetDialog ) {
				CloseOffsetDialog();
			}
		}

		public void CloseOffsetDialog()
		{
			CNCOffsetDialog dialog = m_OffsetDialog;
			CNCOffsetController controller = m_OffsetController;
			m_OffsetDialog = null;
			m_OffsetController = null;
			if( dialog != null ) {
				dialog.FormClosed -= OnOffsetDialogClosed;
				dialog.Disposed -= OnOffsetDialogClosed;
			}
			controller?.Dispose();
			if( dialog != null && !dialog.IsDisposed && !dialog.Disposing ) {
				dialog.Dispose();
			}
		}

		void OnConnectionStateChanged( CncConnectionState state )
		{
			Post( () =>
			{
				RefreshAxes( state == CncConnectionState.Connected && IsConnected );
				ConnectionChanged?.Invoke( IsConnected );
			} );
		}

		async void RefreshAxes( bool connected )
		{
			CancellationTokenSource cancellation = null;
			try {
				m_AxesCancellation?.Cancel();
				m_AxesCancellation = null;
				Axes = Array.AsReadOnly( new CncAxis[ 0 ] );
				AlarmPanel?.SetAxes( Axes );
				AxesChanged?.Invoke();
				if( !connected ) {
					return;
				}
				cancellation = new CancellationTokenSource();
				m_AxesCancellation = cancellation;
				string ip = m_Client.IP;
				var result = await CncApi.ReadAxesAsync();
				if( cancellation.IsCancellationRequested || !IsConnected || m_Client.IP != ip ) {
					return;
				}
				if( !result.IsSuccess ) {
					Report( string.Format( Language.TryGetString( "Error_ReadAxes", READ_AXES_ERROR ), result.ErrorMessage ), true );
					return;
				}
				Axes = Array.AsReadOnly( result.Data );
				AlarmPanel?.SetAxes( Axes );
				AxesChanged?.Invoke();
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC axes refresh failed: {0}", ex );
				if( cancellation?.IsCancellationRequested != true && !m_Stopped && !m_Disposed ) {
					Report( string.Format( Language.TryGetString( "Error_ReadAxes", READ_AXES_ERROR ), ex.Message ), true );
				}
			}
			finally {
				if( m_AxesCancellation == cancellation ) {
					m_AxesCancellation = null;
				}
				cancellation?.Dispose();
			}
		}

		void ReportInitError( string error )
		{
			System.Diagnostics.Trace.TraceError( "CNC UI initialization failed: {0}", error );
			try {
				Notice?.Invoke( string.Format( Language.TryGetString( "Error_Initialization", INITIALIZATION_ERROR ), error ), true );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC initialization notification failed: {0}", ex );
			}
		}

		void OnAlarmsChanged()
		{
			if( AlarmPanel != null && !AlarmPanel.IsDisposed ) {
				AlarmPanel.SetSnapshot( m_AlarmMonitor.Alarms, m_AlarmMonitor.IsCurrentData, m_AlarmMonitor.Status, m_AlarmMonitor.Error );
			}
			AlarmsChanged?.Invoke();
		}

		void OnOperationFailed( string error )
		{
			Report( string.Format( Language.TryGetString( "Error_Operation", OPERATION_ERROR ), error ), true );
		}

		void OnReconnectStatusChanged( ERecconectStage stage )
		{
			switch( stage ) {
				case ERecconectStage.Reconnecting:
					Report( Language.TryGetString( "Notice_Reconnecting", RECONNECTING ), false );
					break;
				case ERecconectStage.Reconnected:
					Report( Language.TryGetString( "Notice_Reconnected", RECONNECTED ), false );
					break;
				case ERecconectStage.ReconnectFailed:
					Report( Language.TryGetString( "Error_Reconnect", RECONNECT_ERROR ), true );
					break;
			}
		}

		internal void Report( string message, bool isError )
		{
			Post( () => Notice?.Invoke( message, isError ) );
		}

		internal void RequestController()
		{
			if( m_Initialized && IsConnected && Thread.CurrentThread.ManagedThreadId == m_UIThreadID ) {
				ControllerRequested?.Invoke();
			}
		}

		void Post( Action action )
		{
			SynchronizationContext context = m_Context;
			if( context == null || !m_Initialized || m_Stopped || m_Disposed ) {
				return;
			}
			try {
				context.Post( state =>
				{
					if( m_Stopped || m_Disposed ) {
						return;
					}
					try {
						action();
					}
					catch( Exception ex ) {
						System.Diagnostics.Trace.TraceError( "CNC UI notification failed: {0}", ex );
					}
				}, null );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC UI notification scheduling failed: {0}", ex );
			}
		}

		internal static LanguageManager.LanguageManager Language { get; } = new LanguageManager.LanguageManager( "CNCUIManager" );

		const string UNAVAILABLE_ERROR = "CNC UI 模組無法使用。";
		const string ALREADY_INITIALIZED = "CNC UI 模組已初始化。";
		const string UI_THREAD_ERROR = "請在宿主載入後，於 WinForms UI 執行緒初始化 CNC UI 模組。";
		const string RESOURCES_ERROR = "無法初始化 CNC 資源。";
		const string PREPARATION_BUSY = "NC 加工準備仍在執行。";
		const string PREPARATION_UNAVAILABLE = "尚未設定 NC 加工準備流程。";
		const string NOT_INITIALIZED = "CNC UI 模組尚未初始化。";
		const string INVALID_IP = "請輸入有效的 CNC IP 位址。";
		const string CONNECTION_BUSY = "CNC 連線流程正在執行。";
		const string CONNECTED = "CNC 連線成功。";
		const string CONNECT_ERROR = "CNC 連線未完成：{0}";
		const string CLIENT_UNAVAILABLE = "CNC 用戶端無法使用。";
		const string READ_AXES_ERROR = "CNC 軸設定讀取失敗：{0}";
		const string INITIALIZATION_ERROR = "CNC 初始化失敗：{0}";
		const string OPERATION_ERROR = "CNC 操作失敗：{0}";
		const string OFFSET_ERROR = "無法開啟程式座標偏置：{0}";
		const string RECONNECTING = "CNC 重新連線中…";
		const string RECONNECTED = "CNC 重新連線成功。";
		const string RECONNECT_ERROR = "CNC 重新連線失敗，請檢查網路或 CNC 狀態。";

		readonly CncClient m_Client;
		readonly CNCAlarmMonitor m_AlarmMonitor;
		readonly Func<string, CancellationToken, Task<CncResult<string>>> m_PrepareProgram;
		CNCPanelController m_Controller;
		CNCInfoPanelController m_InfoController;
		CNCOffsetDialog m_OffsetDialog;
		CNCOffsetController m_OffsetController;
		readonly Action<string> m_SaveIP;
		readonly CancellationTokenSource m_Shutdown = new CancellationTokenSource();
		SynchronizationContext m_Context;
		Task<CncResult<bool>> m_StopTask;
		CancellationTokenSource m_AxesCancellation;
		volatile bool m_Initialized;
		volatile bool m_Stopped;
		volatile bool m_Disposed;
		int m_ConnectionOperationRunning;
		int m_UIThreadID;
		bool m_PreparationRunning;
	}
}
