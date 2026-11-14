using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CncManager;
using CncManager.CNCCore;
using CncManager.Definition;
using CncUI.Dialog;

namespace CncUI
{
	internal sealed class CNCOffsetController : IDisposable
	{
		public CNCOffsetController( CNCOffsetDialog dialog, ResourceMgr resources, CNCUIManager manager )
		{
			m_Dialog = dialog;
			m_Resources = resources;
			m_Manager = manager;
		}

		public bool Init( out string error )
		{
			error = null;
			if( m_Disposed || m_Initialized || m_Dialog.IsDisposed ) {
				error = m_Dialog.Language.TryGetString( "Error_Unavailable", UNAVAILABLE_ERROR );
				return false;
			}
			m_Context = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			if( m_Context == null ) {
				error = m_Dialog.Language.TryGetString( "Error_UIThread", UI_THREAD_ERROR );
				return false;
			}
			try {
				m_Initialized = true;
				m_Dialog.SetZeroClicked += OnSetZeroClicked;
				m_Dialog.ClearClicked += OnClearClicked;
				m_Dialog.Disposed += OnDialogDisposed;
				m_Manager.AxesChanged += OnAxesChanged;
				for( int axisID = 1; axisID <= 5; axisID++ ) {
					IDisposable subscription = m_Resources.SystemVarPool.Subscribe( CncConstants.SYSTEM_VAR_PROGRAM_BASE + axisID, value => PostUpdate() );
					if( subscription == null ) {
						throw new InvalidOperationException( m_Dialog.Language.TryGetString( "Error_Subscription", SUBSCRIPTION_ERROR ) );
					}
					m_Subscriptions.Add( subscription );
				}
				OnAxesChanged();
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
			m_Cancellation?.Cancel();
			m_Manager.AxesChanged -= OnAxesChanged;
			m_Dialog.SetZeroClicked -= OnSetZeroClicked;
			m_Dialog.ClearClicked -= OnClearClicked;
			m_Dialog.Disposed -= OnDialogDisposed;
			foreach( IDisposable subscription in m_Subscriptions ) {
				subscription.Dispose();
			}
			m_Subscriptions.Clear();
		}

		void OnDialogDisposed( object sender, EventArgs e )
		{
			Dispose();
		}

		void OnAxesChanged()
		{
			if( !CanUpdate() ) {
				return;
			}
			m_Cancellation?.Cancel();
			UpdateDisplay();
		}

		void UpdateDisplay()
		{
			if( !CanUpdate() ) {
				return;
			}
			try {
				bool valid = HasAxes;
				for( int slot = 0; slot < 5; slot++ ) {
					ResourceValue<double?> resource;
					double? value = valid && m_Resources.SystemVarPool.TryGetValue( CncConstants.SYSTEM_VAR_PROGRAM_BASE + slot + 1, out resource ) && resource.IsValid
						? resource.Value : null;
					string text = value.HasValue && !double.IsNaN( value.Value ) && !double.IsInfinity( value.Value )
						? value.Value.ToString( "F3", CultureInfo.InvariantCulture ) : "—";
					string name = valid ? m_Manager.Axes[ slot ].Name : slot < 3 ? new[] { "X", "Y", "Z" }[ slot ] : null;
					m_Dialog.SetAxis( slot, name, text + ( slot < 3 ? " mm" : " deg" ) );
				}
				m_Dialog.SetCommandsEnabled( valid && !m_Running );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC offset display update failed: {0}", ex );
				m_Manager.Report( string.Format( m_Dialog.Language.TryGetString( "Error_Display", DISPLAY_ERROR ), ex.Message ), true );
			}
		}

		async void OnSetZeroClicked( int slot )
		{
			if( slot < 0 || slot >= 5 ) {
				return;
			}
			await ExecuteAsync( token => CncApi.SetProgramOffsetAsync( slot + 1, token ), "SetZero", CNCOffsetDialog.SET_ZERO );
		}

		async void OnClearClicked()
		{
			await ExecuteAsync( CncApi.ClearProgramOffsetsAsync, "ClearAll", CNCOffsetDialog.CLEAR_ALL );
		}

		async Task ExecuteAsync( Func<CancellationToken, Task<CncResult<bool>>> operation, string actionKey, string actionDefault )
		{
			if( !CanUpdate() || !HasAxes || m_Running ) {
				return;
			}
			m_Running = true;
			CancellationTokenSource cancellation = new CancellationTokenSource();
			m_Cancellation = cancellation;
			UpdateDisplay();
			try {
				var result = await operation( cancellation.Token );
				if( !CanUpdate() ) {
					return;
				}
				if( cancellation.IsCancellationRequested ) {
					return;
				}
				if( !result.IsSuccess ) {
					string error = string.Format( m_Dialog.Language.TryGetString( "Error_Operation", OPERATION_ERROR ),
						m_Dialog.Language.TryGetString( actionKey, actionDefault ), result.ErrorMessage );
					m_Manager.Report( error, true );
					return;
				}
			}
			catch( Exception ex ) {
				if( CanUpdate() && !cancellation.IsCancellationRequested ) {
					string error = string.Format( m_Dialog.Language.TryGetString( "Error_Operation", OPERATION_ERROR ),
						m_Dialog.Language.TryGetString( actionKey, actionDefault ), ex.Message );
					m_Manager.Report( error, true );
				}
			}
			finally {
				m_Cancellation = null;
				cancellation.Dispose();
				m_Running = false;
				UpdateDisplay();
			}
		}

		void PostUpdate()
		{
			SynchronizationContext context = m_Context;
			if( context == null || !CanUpdate() ) {
				return;
			}
			try {
				context.Post( state => UpdateDisplay(), null );
			}
			catch( InvalidOperationException ) {
				// The UI thread may have already shut down.
			}
		}

		bool CanUpdate() => m_Initialized && !m_Disposed && !m_Dialog.IsDisposed;
		bool HasAxes => m_Manager.IsConnected && m_Manager.Axes.Count == 5;

		readonly CNCOffsetDialog m_Dialog;
		readonly ResourceMgr m_Resources;
		readonly CNCUIManager m_Manager;
		readonly List<IDisposable> m_Subscriptions = new List<IDisposable>();
		SynchronizationContext m_Context;
		CancellationTokenSource m_Cancellation;
		bool m_Initialized;
		bool m_Running;
		volatile bool m_Disposed;

		const string UNAVAILABLE_ERROR = "偏置設定視窗無法使用。";
		const string UI_THREAD_ERROR = "請在 WinForms UI 執行緒初始化偏置設定視窗。";
		const string SUBSCRIPTION_ERROR = "無法初始化偏置座標訂閱。";
		const string OPERATION_ERROR = "{0}失敗，偏置可能尚未完整更新：{1}";
		const string DISPLAY_ERROR = "偏置座標更新失敗：{0}";
	}
}
