using CncManager;
using CncManager.Definition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace CncUI
{
	internal sealed class CNCInfoPanelController : IDisposable
	{
		public CNCInfoPanelController( CNCInfoPanel panel, ResourceMgr resources, CNCUIManager manager )
		{
			m_Panel = panel;
			m_Resources = resources;
			m_Manager = manager;
		}

		public bool Init( out string error )
		{
			error = null;
			if( m_Disposed || m_Panel == null || m_Panel.IsDisposed || m_Resources == null || m_Manager == null ) {
				error = CNCUIManager.Language.TryGetString( "Error_MonitorUnavailable", MONITOR_UNAVAILABLE );
				Dispose();
				return false;
			}
			if( m_Initialized ) {
				error = CNCUIManager.Language.TryGetString( "Error_MonitorInitialized", MONITOR_INITIALIZED );
				return false;
			}
			m_Context = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			if( m_Context == null ) {
				error = CNCUIManager.Language.TryGetString( "Error_MonitorUIThread", MONITOR_UI_THREAD );
				Dispose();
				return false;
			}
			try {
				m_Initialized = true;
				m_Panel.Disposed += OnPanelDisposed;
				m_Panel.CoordinateModeClicked += OnCoordinateModeClicked;
				m_Manager.AxesChanged += OnAxesChanged;
				m_Panel.SetCoordinateMode( m_MachineCoordinates );
				AddSubscription( m_Resources.KVarPool.Subscribe( CncConstants.K_CNC_STATUS, value => PostToUI( UpdateMachiningStatus ) ) );
				UpdateMachiningStatus();
				AddSubscription( m_Resources.MainProgramPool.Subscribe( CncConstants.FIRST_COORD_ID, value => PostToUI( UpdateMainProgram ) ) );
				UpdateMainProgram();
				for( int id = 1; id <= m_CoordinateFields.Length; id++ ) {
					AddSubscription( m_Resources.SystemVarPool.Subscribe( CncConstants.SYSTEM_VAR_PROGRAM_BASE + id, OnCoordinateChanged ) );
					AddSubscription( m_Resources.QVarPool.Subscribe( CncConstants.Q_VAR_MACHINE_BASE + id, value => PostToUI( UpdateCoordinates ) ) );
				}
				OnAxesChanged();
				SubscribeR( CncConstants.R_CAPACITANCE, CNCInfoField.Capacitance, 1m, "F0" );
				SubscribeR( CncConstants.R_HEIGHT_AXIS_POSITION, CNCInfoField.HeightAxisPosition );
				SubscribeR( CncConstants.R_FOLLOW_HEIGHT, CNCInfoField.FollowHeight );
				SubscribeR( CncConstants.R_ACTUAL_HEIGHT, CNCInfoField.ActualHeight );
				SubscribeR( CncConstants.R_FEED_SPEED, CNCInfoField.FeedSpeed );
				SubscribeR( CncConstants.R_FOCUS_POSITION, CNCInfoField.FocusPosition );
				SubscribeGlobal( CncConstants.GLOBAL_VAR_POWER, CNCInfoField.Power );
				SubscribeGlobal( CncConstants.GLOBAL_VAR_FREQUENCY, CNCInfoField.Frequency );
				SubscribeGlobal( CncConstants.GLOBAL_VAR_DUTY_CYCLE, CNCInfoField.DutyCycle );
				AddSubscription( m_Resources.GlobalVarPool.Subscribe( CncConstants.GLOBAL_VAR_MACHINING_HOURS, OnTimeChanged ) );
				AddSubscription( m_Resources.GlobalVarPool.Subscribe( CncConstants.GLOBAL_VAR_MACHINING_MINUTES, OnTimeChanged ) );
				AddSubscription( m_Resources.GlobalVarPool.Subscribe( CncConstants.GLOBAL_VAR_MACHINING_SECONDS, OnTimeChanged ) );
				UpdateTime();
				return true;
			}
			catch( Exception ex ) {
				error = ex.Message;
				Dispose();
				return false;
			}
		}

		public void Dispose()
		{
			if( m_Disposed ) {
				return;
			}
			m_Disposed = true;
			m_Context = null;
			if( m_Manager != null ) {
				m_Manager.AxesChanged -= OnAxesChanged;
			}
			foreach( IDisposable subscription in m_Subscriptions ) {
				subscription.Dispose();
			}
			m_Subscriptions.Clear();
			if( m_Panel != null ) {
				m_Panel.Disposed -= OnPanelDisposed;
				m_Panel.CoordinateModeClicked -= OnCoordinateModeClicked;
			}
		}

		void OnPanelDisposed( object sender, EventArgs e )
		{
			Dispose();
		}

		void AddSubscription( IDisposable subscription )
		{
			if( subscription == null ) {
				throw new InvalidOperationException( CNCUIManager.Language.TryGetString( "Error_MonitorSubscription", MONITOR_SUBSCRIPTION ) );
			}
			m_Subscriptions.Add( subscription );
		}

		void UpdateMachiningStatus()
		{
			ResourceValue<int?> value;
			CncStatus? status = m_Resources.KVarPool.TryGetValue( CncConstants.K_CNC_STATUS, out value ) && value.IsValid && value.Value.HasValue
				? (CncStatus?)value.Value.Value : null;
			m_Panel.SetMachiningStatus( status );
		}

		void UpdateMainProgram()
		{
			ResourceValue<string> value;
			string programName = m_Resources.MainProgramPool.TryGetValue( CncConstants.FIRST_COORD_ID, out value ) && value.IsValid
				? value.Value : null;
			m_Panel.SetMainProgram( programName );
		}

		void OnCoordinateModeClicked()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_MachineCoordinates = !m_MachineCoordinates;
			m_Panel.SetCoordinateMode( m_MachineCoordinates );
			UpdateCoordinates();
		}

		void OnAxesChanged()
		{
			if( !CanUpdate() ) {
				return;
			}
			bool valid = m_Manager.Axes.Count == m_CoordinateFields.Length;
			m_Panel.SetRotaryAxisNames( valid ? m_Manager.Axes[ 3 ].Name : null, valid ? m_Manager.Axes[ 4 ].Name : null );
			m_Panel.SetRotaryAxisRoles( valid ? m_Manager.Axes[ 3 ].Role : null, valid ? m_Manager.Axes[ 4 ].Role : null );
			UpdateCoordinates();
		}

		void OnCoordinateChanged( ResourceValue<double?> value )
		{
			PostToUI( UpdateCoordinates );
		}

		void UpdateCoordinates()
		{
			for( int i = 0; i < m_CoordinateFields.Length; i++ ) {
				double? value = null;
				if( m_Manager.IsConnected && m_Manager.Axes.Count == m_CoordinateFields.Length ) {
					int axisID = m_Manager.Axes[ i ].ID;
					value = m_MachineCoordinates ? GetMachineCoordinate( axisID )
						: GetValue( m_Resources.SystemVarPool, CncConstants.SYSTEM_VAR_PROGRAM_BASE + axisID );
				}
				m_Panel.SetValue( m_CoordinateFields[ i ], FormatCoordinate( value ) );
			}
		}

		double? GetMachineCoordinate( int axisID )
		{
			ResourceValue<int?> value;
			return m_Resources.QVarPool.TryGetValue( CncConstants.Q_VAR_MACHINE_BASE + axisID, out value ) && value.IsValid && value.Value.HasValue
				? value.Value.Value / MACHINE_COORDINATE_DIVISOR : (double?)null;
		}

		void SubscribeR( int index, CNCInfoField field, decimal divisor = 1000m, string format = "F3" )
		{
			Action update = () =>
			{
				ResourceValue<int?> value;
				int? number = m_Resources.RPool.TryGetValue( index, out value ) && value.IsValid ? value.Value : null;
				m_Panel.SetValue( field, number.HasValue ? ( number.Value / divisor ).ToString( format, CultureInfo.InvariantCulture ) : "—" );
			};
			AddSubscription( m_Resources.RPool.Subscribe( index, value => PostToUI( update ) ) );
			update();
		}

		void SubscribeGlobal( int index, CNCInfoField field )
		{
			Action update = () =>
			{
				double? value = GetValue( m_Resources.GlobalVarPool, index );
				m_Panel.SetValue( field, value.HasValue ? value.Value.ToString( CultureInfo.InvariantCulture ) : "—" );
			};
			AddSubscription( m_Resources.GlobalVarPool.Subscribe( index, value => PostToUI( update ) ) );
			update();
		}

		static double? GetValue( ResourcePool<double?> pool, int index )
		{
			ResourceValue<double?> value;
			if( pool.TryGetValue( index, out value ) && value.IsValid && value.Value.HasValue &&
				!double.IsNaN( value.Value.Value ) && !double.IsInfinity( value.Value.Value ) ) {
				return value.Value;
			}
			return null;
		}

		static string FormatCoordinate( double? value )
		{
			return value.HasValue ? value.Value.ToString( "F3", CultureInfo.InvariantCulture ) : "—";
		}

		void OnTimeChanged( ResourceValue<double?> value )
		{
			PostToUI( UpdateTime );
		}

		void UpdateTime()
		{
			double? hours = GetValue( m_Resources.GlobalVarPool, CncConstants.GLOBAL_VAR_MACHINING_HOURS );
			double? minutes = GetValue( m_Resources.GlobalVarPool, CncConstants.GLOBAL_VAR_MACHINING_MINUTES );
			double? seconds = GetValue( m_Resources.GlobalVarPool, CncConstants.GLOBAL_VAR_MACHINING_SECONDS );
			string text = hours.HasValue && minutes.HasValue && seconds.HasValue
				? string.Format( CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}",
					Math.Truncate( hours.Value ), Math.Truncate( minutes.Value ), Math.Truncate( seconds.Value ) )
				: "—";
			m_Panel.SetValue( CNCInfoField.MachiningTime, text );
		}

		void PostToUI( Action update )
		{
			SynchronizationContext context = m_Context;
			if( context == null || !CanUpdate() ) {
				return;
			}
			try {
				context.Post( state =>
				{
					if( !CanUpdate() ) {
						return;
					}
					try {
						update();
					}
					catch( Exception ex ) {
						System.Diagnostics.Trace.TraceError( "CNC monitor update failed: {0}", ex );
					}
				}, null );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC monitor update scheduling failed: {0}", ex );
			}
		}

		bool CanUpdate()
		{
			return m_Initialized && !m_Disposed && !m_Panel.IsDisposed;
		}

		readonly CNCInfoPanel m_Panel;
		readonly ResourceMgr m_Resources;
		readonly CNCUIManager m_Manager;
		readonly CNCInfoField[] m_CoordinateFields = { CNCInfoField.X, CNCInfoField.Y, CNCInfoField.Z, CNCInfoField.B, CNCInfoField.C };
		readonly List<IDisposable> m_Subscriptions = new List<IDisposable>();
		SynchronizationContext m_Context;
		bool m_MachineCoordinates;
		volatile bool m_Initialized;
		volatile bool m_Disposed;

		const double MACHINE_COORDINATE_DIVISOR = 1000d;
		const string MONITOR_UNAVAILABLE = "CNC 監控面板無法使用。";
		const string MONITOR_INITIALIZED = "CNC 監控面板已初始化。";
		const string MONITOR_UI_THREAD = "請在 WinForms UI 執行緒初始化 CNC 監控面板。";
		const string MONITOR_SUBSCRIPTION = "無法初始化 CNC 監控面板資源訂閱。";
	}
}
