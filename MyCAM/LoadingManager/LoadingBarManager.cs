using MyCAM.Data;
using MyCAM.Editor;
using MyCAM.Helper.LoadingBar;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace MyCAM.LoadingManager
{
	// One instance per operation; create and show it on the UI thread.
	internal sealed class LoadingBarManager : IDisposable, IProgress<LoadingBarInfo>
	{
		readonly Form m_Owner;
		readonly bool m_DisableOwner;
		readonly bool m_PumpEvents;
		readonly int m_nUIThreadID = Thread.CurrentThread.ManagedThreadId;
		readonly Stopwatch m_RefreshWatch = Stopwatch.StartNew();
		LoadingBarDlg m_Dialog;
		LoadingWorkflow m_Workflow;
		WindowsFormsSynchronizationContext m_UIContext;
		string m_szProgressKey;
		string m_szProgressText;
		bool m_IsOwnerEnabled;
		bool m_IsOwnerDisabled;
		bool m_IsReporting;
		int m_nDisposed;

		public LoadingBarManager( Form owner, bool disableOwner = true, bool pumpEvents = true )
		{
			m_Owner = owner;
			m_DisableOwner = disableOwner;
			m_PumpEvents = pumpEvents;
		}

		public void ShowImport( string szFileName, LoadingWorkflow workflow = LoadingWorkflow.RingImport )
		{
			m_Workflow = workflow;
			LoadingStep initialStep = workflow == LoadingWorkflow.DxfImport ? LoadingStep.ReadFile
				: workflow == LoadingWorkflow.ProjectImport ? LoadingStep.ReadProject : LoadingStep.PrepareReader;
			Show( LoadingHelper.Get( workflow == LoadingWorkflow.ProjectImport ? LoadingText.ProjectTitle : LoadingText.ImportTitle ),
				LoadingHelper.Format( LoadingText.FileTopicFormat, szFileName ), initialStep );
		}

		public void ShowPlacement()
		{
			m_Workflow = LoadingWorkflow.RingImport;
			Show( LoadingHelper.Get( LoadingText.ImportTitle ),
				LoadingHelper.Get( LoadingText.PlacementTopic ), LoadingStep.PrepareWorkpiece );
		}

		public void ShowTubeImport()
		{
			m_Workflow = LoadingWorkflow.TubeImport;
			Show( LoadingHelper.Get( LoadingText.ImportTitle ),
				LoadingHelper.Get( LoadingText.TubeTopic ), LoadingStep.SewShape );
		}

		public void ShowSimulation()
		{
			m_Workflow = LoadingWorkflow.Simulation;
			Show( LoadingHelper.Get( LoadingText.SimulationTitle ),
				LoadingHelper.Get( LoadingText.SimulationTopic ), LoadingStep.PrepareSimulation );
		}

		void Show( string szTitle, string szTopic, LoadingStep initialStep )
		{
			if( IsDisposed || m_Dialog != null ) {
				return;
			}
			try {
				if( Thread.CurrentThread.ManagedThreadId != m_nUIThreadID ) {
					throw new InvalidOperationException( UI_THREAD_REQUIRED );
				}
				m_Dialog = new LoadingBarDlg( szTitle, szTopic, LoadingHelper.Get( LoadingText.Preparing ) );
				m_UIContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;

				// Prepare and paint the first frame before calculation starts.
				Report( initialStep );
				Form owner = m_Owner != null && !m_Owner.IsDisposed ? m_Owner : null;
				m_IsOwnerEnabled = owner != null && owner.Enabled;
				m_Dialog.Show( owner );
				if( IsDisposed || m_Dialog == null || m_Dialog.IsDisposed ) {
					return;
				}
				if( m_DisableOwner && owner != null ) {
					m_IsOwnerDisabled = true;
					owner.Enabled = false;
				}
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Show ), ex );
				Dispose();
			}
		}

		public void Report( LoadingStep step )
		{
			Report( new LoadingBarInfo( step ) );
		}

		public void Report( LoadingBarInfo progress )
		{
			if( IsDisposed ) {
				return;
			}
			try {
				LoadingBarDlg dialog = m_Dialog;
				if( dialog == null || dialog.IsDisposed ) {
					return;
				}
				if( Thread.CurrentThread.ManagedThreadId != m_nUIThreadID ) {
					// Recheck disposal in the queued callback, not just before posting.
					if( dialog.IsHandleCreated ) {
						dialog.BeginInvoke( new Action( () => Report( progress ) ) );
					}
					return;
				}
				if( m_IsReporting ) {
					return;
				}
				m_IsReporting = true;
				try {
					LoadingProgressDisplay display = LoadingHelper.Create( progress, m_Workflow );
					bool isStageChanged = m_szProgressKey != display.Key;
					bool isCountComplete = display.IsComplete && m_szProgressText != display.Text;
					if( !isStageChanged && !isCountComplete && m_RefreshWatch.ElapsedMilliseconds < REFRESH_INTERVAL_MS ) {
						return;
					}
					dialog.SetStage( display.Text, display.Percent );
					m_szProgressKey = display.Key;
					m_szProgressText = display.Text;
					m_RefreshWatch.Restart();

					// Only synchronous operations need message pumping; callers retain busy guards.
					if( m_PumpEvents && dialog.Visible ) {
						Application.DoEvents();
					}
				}
				finally {
					m_IsReporting = false;
				}
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Report ), ex );
			}
		}

		public void Dispose()
		{
			// Mark closed before cleanup so pending reports become harmless.
			if( Interlocked.Exchange( ref m_nDisposed, 1 ) != 0 ) {
				return;
			}
			try {
				if( Thread.CurrentThread.ManagedThreadId != m_nUIThreadID ) {
					// The UI context survives a dialog disposed before its manager.
					if( m_UIContext != null ) {
						m_UIContext.Post( state => CloseDialog(), null );
					}
					else {
						Control dispatcher = m_Owner != null && !m_Owner.IsDisposed && m_Owner.IsHandleCreated
							? m_Owner : m_Dialog;
						if( dispatcher != null && !dispatcher.IsDisposed && dispatcher.IsHandleCreated ) {
							dispatcher.BeginInvoke( new Action( CloseDialog ) );
						}
					}
					return;
				}
				CloseDialog();
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( Dispose ), ex );
			}
		}

		bool IsDisposed
		{
			get { return Volatile.Read( ref m_nDisposed ) != 0; }
		}

		void CloseDialog()
		{
			LoadingBarDlg dialog = m_Dialog;
			m_Dialog = null;

			// Enable the owner before closing so Windows can return activation to it.
			RestoreOwner();
			try {
				if( dialog != null && !dialog.IsDisposed ) {
					dialog.CloseForCleanup();
				}
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( CloseDialog ), ex );
			}
			finally {
				try {
					if( dialog != null ) {
						dialog.Dispose();
					}
				}
				catch( Exception ex ) {
					Trace.TraceError( ERROR_FORMAT, nameof( Dispose ), ex );
				}
				finally {
					// Retry restoration if the first attempt failed, even when closing throws.
					RestoreOwner();
				}
			}
		}

		void RestoreOwner()
		{
			if( !m_IsOwnerDisabled ) {
				return;
			}
			try {
				if( m_Owner != null && !m_Owner.IsDisposed ) {
					m_Owner.Enabled = m_IsOwnerEnabled;
				}
				m_IsOwnerDisabled = false;
			}
			catch( Exception ex ) {
				Trace.TraceError( ERROR_FORMAT, nameof( RestoreOwner ), ex );
			}
		}

		const int REFRESH_INTERVAL_MS = 150;
		const string UI_THREAD_REQUIRED = "LoadingBarManager must be shown on its creating UI thread.";
		const string ERROR_FORMAT = "Loading bar {0} failed: {1}";
	}
}
