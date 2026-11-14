using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CncManager;
using CncManager.CNCCore;
using CncManager.Definition;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Editor.Renderer;
using MyCAM.Helper.FormShow;
using OCC.BRep;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal sealed class CNCThreePointAction : EditActionBase, IDisposable
	{
		public CNCThreePointAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager,
			CncClient client, Form owner, params Control[] editingControls ) : base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;
			m_Client = client;
			m_Owner = owner;
			m_EditingControls = editingControls;
		}

		public override EditActionType ActionType => EditActionType.ThreePointCalibration;

		public override void Start()
		{
			if( m_Started || m_Finished ) {
				return;
			}

			m_Started = true;
			base.Start();
			try {
				m_Context = SynchronizationContext.Current;
				m_Original = m_DataManager.CalibrationData;
				m_OriginalSnapshot = m_Original.Clone();
				m_Accepted = m_Original.Clone();
				m_Dialog = new CNCThreePointDlg();
				m_Dialog.DrawingPickRequested += OnDrawingPickRequested;
				m_Dialog.DrawingPointResetRequested += BeginPicking;
				m_Dialog.DrawingPickCancelRequested += CancelPicking;
				m_Dialog.DrawingOnlyChanged += OnDrawingOnlyChanged;
				m_Dialog.WorkpiecePointCaptureRequested += CapturePoint;
				m_Dialog.CalculateRequested += Calculate;
				m_Dialog.InputChanged += OnInputChanged;
				m_Dialog.PointInputCommitRequested += CommitPointInput;
				m_Dialog.ToleranceCommitRequested += CommitTolerance;
				m_Dialog.FormClosing += OnFormClosing;
				m_Dialog.FormClosed += OnFormClosed;
				LoadDraft( m_Accepted );
				m_Baseline = GetInputSnapshot();
				m_Client.ConnectionStateChanged += OnConnectionChanged;
				DialogShowHelper.ShowCentered( m_Dialog, m_Owner );
				if( !IsDrawingComplete ) {
					BeginPicking( ALL_POINTS );
				}
			}
			catch {
				End();
				throw;
			}
		}

		public void Activate()
		{
			m_Dialog?.Activate();
			m_Dialog?.BringToFront();
		}

		bool EffectiveDrawingOnly => !m_Client.IsConnected || m_DrawingOnly;
		bool IsDrawingComplete => ArePointsComplete( m_Dialog.DrawingInput );
		bool CanOperate => m_Dialog != null && !m_Closing && m_Client.IsConnected && !EffectiveDrawingOnly;

		void LoadDraft( CalibrationData data )
		{
			m_DrawingOnly = data.DrawingOnly;
			m_Dialog.SetInputs( PointInputs( data.Ref_Pnt1, data.Ref_Pnt2, data.Ref_Pnt3 ),
				PointInputs( data.Work_Pnt1, data.Work_Pnt2, data.Work_Pnt3 ), FormatNumber( data.Tolerance ?? DEFAULT_TOLERANCE ) );
			m_CalibrationError = data.CalibrationError;
			m_CalculationCompleted = data.CalculationCompleted;
			m_CalculationSucceeded = data.CalculationSucceeded;
			RefreshDisplay();
		}

		void RefreshDisplay()
		{
			if( m_Dialog == null || m_Dialog.IsDisposed ) {
				return;
			}

			bool drawingComplete = IsDrawingComplete;
			bool workpieceComplete = ArePointsComplete( m_Dialog.WorkpieceInput );
			ThreePointStep step = m_Picking || !drawingComplete ? ThreePointStep.Drawing : EffectiveDrawingOnly ? ThreePointStep.None
				: !workpieceComplete ? ThreePointStep.Workpiece : m_CalculationCompleted ? ThreePointStep.None : ThreePointStep.Calculation;
			bool successful = m_CalculationCompleted && m_CalculationSucceeded != false && m_CalibrationError.HasValue &&
				TryReadNumber( m_Dialog.ToleranceText, out double tolerance ) && m_CalibrationError.Value <= tolerance;
			m_Dialog.DisplayState( step, m_Client.IsConnected, EffectiveDrawingOnly, drawingComplete, workpieceComplete,
				m_Picking, m_PickCount, m_PickIndex, m_Calculating, m_Capturing,
				m_CalibrationError, successful, m_CalculationCompleted );
		}

		void OnInputChanged()
		{
			m_CalibrationError = null;
			m_CalculationCompleted = false;
			m_CalculationSucceeded = null;
			RefreshDisplay();
		}

		void CommitPointInput( int point, int axis )
		{
			if( m_Calculating || m_Capturing || m_Picking || EffectiveDrawingOnly || !IsDrawingComplete ) {
				return;
			}

			string text = m_Dialog.WorkpieceInput[ point, axis ];
			if( string.IsNullOrWhiteSpace( text ) ) {
				return;
			}

			if( !TryReadNumber( text, out double value ) ) {
				Report( Text( "Validation_Input", INVALID_INPUT ) );
				return;
			}
			m_Dialog.SetWorkpieceInput( point, axis, FormatNumber( value ) );
		}

		void CommitTolerance()
		{
			if( m_Calculating || m_Picking || EffectiveDrawingOnly || string.IsNullOrWhiteSpace( m_Dialog.ToleranceText ) ) {
				return;
			}

			if( !TryGetTolerance( out double tolerance ) ) {
				Report( Text( "Validation_Input", INVALID_INPUT ) );
				return;
			}
			m_Dialog.ToleranceText = FormatNumber( tolerance );
		}

		bool TryGetTolerance( out double value )
		{
			if( !TryReadNumber( m_Dialog.ToleranceText, out value ) || value < 0 ) {
				return false;
			}

			value = CalibrationData.Round( value );
			return true;
		}

		string[] GetInputSnapshot()
		{
			string[,] drawing = m_Dialog.DrawingInput;
			string[,] workpiece = m_Dialog.WorkpieceInput;
			string[] values = drawing.Cast<string>().Concat( workpiece.Cast<string>() ).Concat( new[] { m_Dialog.ToleranceText } ).ToArray();
			for( int index = 0; index < values.Length; index++ ) {
				values[ index ] = TryReadNumber( values[ index ], out double value )
					? CalibrationData.Round( value ).ToString( "R", CultureInfo.InvariantCulture ) : values[ index ].Trim();
			}
			return values;
		}

		bool TryGetDraft( out CalibrationData data, out string error, bool drawingOnly = false )
		{
			data = null;
			if( !TryReadPoints( m_Dialog.DrawingInput, Text( "DrawingPoints", DRAWING_POINTS ), out gp_Pnt[] drawing, out error ) ) {
				return false;
			}

			if( !IsDrawingComplete || m_Picking ) {
				error = Text( "Validation_DrawingIncomplete", DRAWING_INCOMPLETE );
				return false;
			}
			bool hasTolerance = TryGetTolerance( out double tolerance );
			gp_Pnt[] workpiece;
			if( m_DrawingOnly || drawingOnly ) {
				string[,] fields = m_Dialog.WorkpieceInput;
				gp_Pnt[] retained = { m_Accepted.Work_Pnt1, m_Accepted.Work_Pnt2, m_Accepted.Work_Pnt3 };
				workpiece = new gp_Pnt[ POINT_COUNT ];
				for( int point = 0; point < POINT_COUNT; point++ ) {
					workpiece[ point ] = TryReadNumber( fields[ point, X_AXIS ], out double x ) && TryReadNumber( fields[ point, Y_AXIS ], out double y ) &&
						TryReadNumber( fields[ point, Z_AXIS ], out double z ) ? CreatePoint( x, y, z ) : retained[ point ];
				}
				if( !hasTolerance && m_Accepted.Tolerance.HasValue ) {
					tolerance = m_Accepted.Tolerance.Value;
					hasTolerance = true;
				}
			}
			else if( !TryReadPoints( m_Dialog.WorkpieceInput, Text( "WorkpiecePoints", WORKPIECE_POINTS ), out workpiece, out error ) || !ArePointsComplete( m_Dialog.WorkpieceInput ) ) {
				error = error ?? Text( "Validation_WorkpieceIncomplete", WORKPIECE_INCOMPLETE );
				return false;
			}
			if( !m_DrawingOnly && !drawingOnly && !hasTolerance ) {
				error = Text( "Validation_Tolerance", INVALID_TOLERANCE );
				return false;
			}
			data = new CalibrationData( drawing[ 0 ], drawing[ 1 ], drawing[ 2 ] ) { DrawingOnly = m_DrawingOnly };
			data.SetWorkpieceData( workpiece[ 0 ], workpiece[ 1 ], workpiece[ 2 ], hasTolerance ? (double?)tolerance : null );
			data.SetCalibrationError( m_CalibrationError, m_CalculationCompleted, m_CalculationSucceeded );
			if( m_Accepted.HasTransmitted ) {
				data.MarkTransmitted();
			}

			return true;
		}

		bool TryReadPoints( string[,] fields, string name, out gp_Pnt[] points, out string error )
		{
			points = new gp_Pnt[ fields.GetLength( 0 ) ];
			error = null;
			for( int point = 0; point < points.Length; point++ ) {
				if( string.IsNullOrWhiteSpace( fields[ point, X_AXIS ] ) && string.IsNullOrWhiteSpace( fields[ point, Y_AXIS ] ) && string.IsNullOrWhiteSpace( fields[ point, Z_AXIS ] ) ) {
					continue;
				}

				if( !TryReadNumber( fields[ point, X_AXIS ], out double x ) || !TryReadNumber( fields[ point, Y_AXIS ], out double y ) || !TryReadNumber( fields[ point, Z_AXIS ], out double z ) ) {
					error = string.Format( Text( "Validation_Point", INVALID_POINT ), name, point + 1 );
					return false;
				}
				points[ point ] = CreatePoint( x, y, z );
			}
			return true;
		}

		static bool ArePointsComplete( string[,] fields ) => fields.Cast<string>().All( text => TryReadNumber( text, out double value ) );

		static bool TryReadNumber( string text, out double value )
		{
			return ( double.TryParse( text, NumberStyles.Float, CultureInfo.InvariantCulture, out value ) ||
				double.TryParse( text, NumberStyles.Float, CultureInfo.CurrentCulture, out value ) ) && !double.IsNaN( value ) && !double.IsInfinity( value );
		}

		static gp_Pnt CreatePoint( double x, double y, double z ) => new gp_Pnt( CalibrationData.Round( x ), CalibrationData.Round( y ), CalibrationData.Round( z ) );
		static string FormatNumber( double value ) => CalibrationData.Round( value ).ToString( "F3", CultureInfo.InvariantCulture );
		static string[] PointText( gp_Pnt point ) => point == null ? new string[ AXIS_COUNT ] : new[] { FormatNumber( point.x ), FormatNumber( point.y ), FormatNumber( point.z ) };

		static string[,] PointInputs( params gp_Pnt[] points )
		{
			string[,] values = new string[ points.Length, AXIS_COUNT ];
			for( int point = 0; point < points.Length; point++ ) {
				string[] text = PointText( points[ point ] );
				for( int axis = 0; axis < AXIS_COUNT; axis++ ) {
					values[ point, axis ] = text[ axis ];
				}
			}
			return values;
		}

		void OnFormClosing( object sender, FormClosingEventArgs e )
		{
			if( e.Cancel || m_Closing ) {
				return;
			}

			m_PendingSave = null;
			if( e.CloseReason != CloseReason.UserClosing ) {
				return;
			}

			if( m_ClosePrompt ) { e.Cancel = true; return; }
			bool valid = TryGetDraft( out CalibrationData draft, out string message );
			bool sameProject = ReferenceEquals( m_Original, m_DataManager.CalibrationData ) && m_Original.HasSameInputs( m_OriginalSnapshot ) && m_Original.DrawingOnly == m_OriginalSnapshot.DrawingOnly;
			CalibrationData saved = null;
			bool discardUncalculated = false;
			if( !sameProject ) {
				message = Text( "Error_ProjectChanged", PROJECT_CHANGED );
			}
			else if( m_DrawingOnly && valid ) {
				saved = draft;
			}
			else if( !m_DrawingOnly && m_Accepted.HasCalculatedSnapshot && valid && draft.HasSameInputs( m_Accepted ) && !m_Picking && !m_Calculating ) {
				saved = m_Accepted.Clone();
				saved.DrawingOnly = false;
			}
			else if( !m_DrawingOnly ) {
				bool changed = !GetInputSnapshot().SequenceEqual( m_Baseline );
				if( changed || m_Calculating || m_Picking || !m_Accepted.HasCalculatedSnapshot ) {
					message = Text( "Confirm_DiscardUncalculated", DISCARD_UNCALCULATED );
					discardUncalculated = true;
				}
				else {
					saved = m_Accepted.Clone();
					saved.DrawingOnly = false;
				}
			}
			if( saved == null ) {
				m_ClosePrompt = true;
				DialogResult answer;
				try {
					string prompt = discardUncalculated ? message : message + "\n" + Text( "Confirm_DiscardAndClose", DISCARD_AND_CLOSE );
					answer = MessageBox.Show( m_Dialog, prompt, Text( "Title", CNCThreePointDlg.TITLE ), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 );
				}
				finally { m_ClosePrompt = false; }
				if( answer != DialogResult.Yes ) { e.Cancel = true; return; }
				if( sameProject && !m_DrawingOnly ) {
					saved = m_Accepted.Clone();
					saved.DrawingOnly = false;
				}
			}
			m_PendingSave = saved;
		}

		void OnDrawingOnlyChanged()
		{
			bool previous = m_DrawingOnly;
			m_DrawingOnly = m_Dialog.DrawingOnlyChecked;
			if( previous && !m_DrawingOnly ) {
				bool valid = TryGetDraft( out CalibrationData drawing, out string error, true );
				if( !valid || !CalibrationData.SamePoint( drawing.Ref_Pnt1, m_Accepted.Ref_Pnt1 ) || !CalibrationData.SamePoint( drawing.Ref_Pnt2, m_Accepted.Ref_Pnt2 ) || !CalibrationData.SamePoint( drawing.Ref_Pnt3, m_Accepted.Ref_Pnt3 ) ) {
					if( valid ) {
						m_Accepted.Ref_Pnt1 = drawing.Ref_Pnt1;
						m_Accepted.Ref_Pnt2 = drawing.Ref_Pnt2;
						m_Accepted.Ref_Pnt3 = drawing.Ref_Pnt3;
					}
					m_Accepted.SetCalibrationError( null, false );
				}
			}
			RefreshDisplay();
		}

		void OnDrawingPickRequested() => BeginPicking( ALL_POINTS );

		void BeginPicking( int point )
		{
			if( m_Dialog == null || m_Closing || m_Calculating || m_Capturing || m_Picking ) {
				return;
			}

			if( point < ALL_POINTS || point >= POINT_COUNT ) {
				throw new ArgumentOutOfRangeException( nameof( point ) );
			}

			if( point < 0 ) {
				for( int index = 0; index < POINT_COUNT; index++ ) {
					m_Dialog.SetDrawingPoint( index, PointText( null ) );
				}
			}
			else {
				m_Dialog.SetDrawingPoint( point, PointText( null ) );
			}

			m_Picking = true;
			m_PickCount = 0;
			m_PickIndex = point;
			RefreshDisplay();
			try {
				if( m_DataManager.PartIDList.Count == 0 ) {
					throw new InvalidOperationException( Text( "Error_DrawingUnavailable", DRAWING_UNAVAILABLE ) );
				}

				m_EditingStates = m_EditingControls.Select( control => control.Enabled ).ToArray();
				foreach( Control control in m_EditingControls ) {
					control.Enabled = false;
				}

				m_TreeEnabled = m_TreeView.Enabled;
				m_PickingStarted = true;
				m_TreeView.Enabled = false;
				m_PickDraft = new CalibrationData();
				m_PointRenderer = new REFPointRenderer( m_Viewer, m_DataManager );
				m_Viewer.MouseClick += OnDrawingMouseClick;
				m_Viewer.KeyDown += OnDrawingKeyDown;
				m_Viewer.GetAISContext().ClearSelected( false );
				SetVertexSelection( true );
				m_Viewer.UpdateView();
			}
			catch( Exception ex ) {
				EndPicking();
				Report( ex.Message );
			}
		}

		void CancelPicking() => EndPicking();

		void EndPicking()
		{
			if( !m_Picking ) {
				return;
			}

			m_Picking = false;
			try {
				if( m_PickingStarted ) {
					m_Viewer.GetAISContext().ClearSelected( false );
					SetVertexSelection( false );
					m_PointRenderer?.Remove();
					m_Viewer.UpdateView();
				}
			}
			finally {
				if( m_PickingStarted ) {
					m_Viewer.MouseClick -= OnDrawingMouseClick;
					m_Viewer.KeyDown -= OnDrawingKeyDown;
					m_TreeView.Enabled = m_TreeEnabled;
					m_PickingStarted = false;
				}
				m_PointRenderer = null;
				m_PickDraft = null;
				if( m_EditingStates != null ) {
					for( int index = 0; index < m_EditingControls.Length; index++ ) {
						if( !m_EditingControls[ index ].IsDisposed ) {
							m_EditingControls[ index ].Enabled = m_EditingStates[ index ];
						}
					}
					m_EditingStates = null;
				}
				RefreshDisplay();
			}
		}

		void OnDrawingMouseClick( MouseEventArgs e )
		{
			if( !m_Picking || m_Closing || e.Button != MouseButtons.Left ) {
				return;
			}

			m_Viewer.Select();
			var context = m_Viewer.GetAISContext();
			context.InitSelected();
			if( !context.MoreSelected() ) {
				return;
			}

			TopoDS_Shape shape = context.SelectedShape();
			if( shape == null || shape.IsNull() || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return;
			}

			AcceptDrawingPoint( BRep_Tool.Pnt( TopoDS.ToVertex( shape ) ) );
		}

		void AcceptDrawingPoint( gp_Pnt point )
		{
			if( !m_Picking || m_Closing || point == null ) {
				return;
			}

			int index = m_PickIndex == ALL_POINTS ? m_PickCount : m_PickIndex;
			if( index == 0 ) {
				m_PickDraft.Ref_Pnt1 = point;
			}
			else if( index == 1 ) {
				m_PickDraft.Ref_Pnt2 = point;
			}
			else {
				m_PickDraft.Ref_Pnt3 = point;
			}

			m_PointRenderer?.Show( m_PickDraft, true );
			m_Dialog.SetDrawingPoint( index, PointText( point ) );
			m_PickCount++;
			if( m_PickCount == ( m_PickIndex == ALL_POINTS ? POINT_COUNT : 1 ) ) {
				EndPicking();
			}
			else {
				RefreshDisplay();
			}
		}

		void OnDrawingKeyDown( KeyEventArgs e )
		{
			if( m_Picking && e.KeyCode == Keys.Escape ) {
				CancelPicking();
			}
		}

		void SetVertexSelection( bool active )
		{
			foreach( string id in m_DataManager.PartIDList ) {
				if( !m_ViewManager.ViewObjectMap.ContainsKey( id ) || !m_ViewManager.ViewObjectMap[ id ].Visible ) {
					continue;
				}

				if( active ) {
					m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ id ].AISHandle, (int)AISActiveMode.Vertex );
				}
				else {
					m_Viewer.GetAISContext().Deactivate( m_ViewManager.ViewObjectMap[ id ].AISHandle, (int)AISActiveMode.Vertex );
				}
			}
		}

		async void CapturePoint( int point )
		{
			try {
				await CapturePointAsync( point );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC calibration point capture failed: {0}", ex );
				Report( ex.Message );
			}
		}

		async Task CapturePointAsync( int point )
		{
			if( !CanOperate || m_Calculating || m_Capturing || m_Picking || point < 0 || point >= POINT_COUNT ) {
				return;
			}

			CNCThreePointDlg dialog = m_Dialog;
			CancellationTokenSource operation = new CancellationTokenSource();
			m_Operation = operation;
			m_Capturing = true;
			try {
				RefreshDisplay();
				var result = await CncApi.ReadCalibrationPositionAsync( operation.Token );
				if( !IsCurrent( dialog, operation ) ) {
					return;
				}

				if( result.IsSuccess ) {
					dialog.SetWorkpiecePoint( point, PointText( new gp_Pnt( result.Data[ 0 ], result.Data[ 1 ], result.Data[ 2 ] ) ) );
				}
				else {
					Report( result.ErrorMessage );
				}
			}
			finally {
				if( ReferenceEquals( m_Dialog, dialog ) ) {
					m_Capturing = false;
				}

				if( ReferenceEquals( m_Operation, operation ) ) {
					m_Operation = null;
				}

				operation.Dispose();
				if( ReferenceEquals( m_Dialog, dialog ) ) {
					RefreshDisplay();
				}
			}
		}

		async void Calculate()
		{
			try {
				await CalculateAsync();
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC calibration calculation failed: {0}", ex );
				Report( ex.Message );
			}
		}

		async Task CalculateAsync()
		{
			if( !CanOperate || m_Calculating || m_Capturing || m_Picking ) {
				return;
			}

			if( !TryGetDraft( out CalibrationData draft, out string error ) ) { Report( error ); return; }
			CNCThreePointDlg dialog = m_Dialog;
			CalibrationData project = m_Original;
			CancellationTokenSource operation = new CancellationTokenSource();
			m_Operation = operation;
			m_Calculating = true;
			try {
				OnInputChanged();
				var result = await CncApi.CalculateCalibrationAsync( Flatten( draft.Ref_Pnt1, draft.Ref_Pnt2, draft.Ref_Pnt3 ),
					Flatten( draft.Work_Pnt1, draft.Work_Pnt2, draft.Work_Pnt3 ), draft.Tolerance.Value, operation.Token, project.MarkTransmitted );
				while( m_ClosePrompt && !operation.IsCancellationRequested ) {
					await Task.Delay( 50 );
				}

				if( !IsCurrent( dialog, operation ) || !ReferenceEquals( m_DataManager.CalibrationData, project ) ) {
					return;
				}

				if( !result.IsSuccess ) { Report( result.ErrorMessage ); return; }
				draft.MarkTransmitted();
				draft.SetCalibrationError( result.Data.Deviation, true, result.Data.Succeeded );
				m_Accepted = draft.Clone();
				m_Baseline = GetInputSnapshot();
				m_CalibrationError = draft.CalibrationError;
				m_CalculationCompleted = draft.CalculationCompleted;
				m_CalculationSucceeded = draft.CalculationSucceeded;
				RefreshDisplay();
				if( !draft.IsCalculationSuccessful ) {
					Report( Text( "Error_Calculation", CALCULATION_ERROR ) );
				}
			}
			finally {
				if( ReferenceEquals( m_Dialog, dialog ) ) {
					m_Calculating = false;
				}

				if( ReferenceEquals( m_Operation, operation ) ) {
					m_Operation = null;
				}

				operation.Dispose();
				if( ReferenceEquals( m_Dialog, dialog ) ) {
					RefreshDisplay();
				}
			}
		}

		static double[] Flatten( gp_Pnt first, gp_Pnt second, gp_Pnt third ) => new[] { first.x, first.y, first.z, second.x, second.y, second.z, third.x, third.y, third.z };

		void OnConnectionChanged( CncConnectionState state )
		{
			CNCThreePointDlg dialog = m_Dialog;
			try {
				m_Context?.Post( ignored =>
				{
					try {
						if( dialog == null || !ReferenceEquals( m_Dialog, dialog ) || m_Closing ) {
							return;
						}

						m_Operation?.Cancel();
						CancelPicking();
						RefreshDisplay();
					}
					catch( Exception ex ) {
						System.Diagnostics.Trace.TraceError( "CNC calibration connection update failed: {0}", ex );
						Report( ex.Message );
					}
				}, null );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC calibration update scheduling failed: {0}", ex );
			}
		}

		bool IsCurrent( CNCThreePointDlg dialog, CancellationTokenSource operation ) => ReferenceEquals( m_Dialog, dialog ) && !m_Closing && !operation.IsCancellationRequested && m_Client.IsConnected;
		string Text( string key, string fallback ) => m_Dialog.Language.TryGetString( key, fallback );

		void Report( string message )
		{
			if( m_Dialog == null || m_Closing ) {
				return;
			}

			try {
				MyApp.Logger?.ShowOnLogPanel( message, MyApp.NoticeType.Warning );
				MessageBox.Show( m_Dialog, message, Text( "Title", CNCThreePointDlg.TITLE ), MessageBoxButtons.OK, MessageBoxIcon.Warning );
			}
			catch( Exception ex ) {
				System.Diagnostics.Trace.TraceError( "CNC calibration error notification failed ({0}): {1}", message, ex );
			}
		}

		void OnFormClosed( object sender, FormClosedEventArgs e )
		{
			if( e.CloseReason == CloseReason.UserClosing && m_PendingSave != null ) {
				if( m_Original.HasTransmitted ) {
					m_PendingSave.MarkTransmitted();
				}

				m_Original.CopyFrom( m_PendingSave );
			}
			End();
		}

		public override void End()
		{
			if( m_Finished ) {
				return;
			}

			m_Finished = true;
			m_Closing = true;
			m_PendingSave = null;
			m_Operation?.Cancel();
			CancelPicking();
			m_Client.ConnectionStateChanged -= OnConnectionChanged;
			CNCThreePointDlg dialog = m_Dialog;
			m_Dialog = null;
			if( dialog != null ) {
				dialog.FormClosing -= OnFormClosing;
				dialog.FormClosed -= OnFormClosed;
				dialog.Dispose();
			}
			if( m_Started ) {
				base.End();
			}
		}

		public void Dispose() => End();

		readonly CncClient m_Client;
		readonly Viewer m_Viewer;
		readonly TreeView m_TreeView;
		readonly ViewManager m_ViewManager;
		readonly Form m_Owner;
		readonly Control[] m_EditingControls;
		CNCThreePointDlg m_Dialog;
		CalibrationData m_Original;
		CalibrationData m_OriginalSnapshot;
		CalibrationData m_Accepted;
		CalibrationData m_PendingSave;
		CalibrationData m_PickDraft;
		string[] m_Baseline;
		bool[] m_EditingStates;
		REFPointRenderer m_PointRenderer;
		SynchronizationContext m_Context;
		CancellationTokenSource m_Operation;
		double? m_CalibrationError;
		bool? m_CalculationSucceeded;
		bool m_CalculationCompleted;
		bool m_DrawingOnly;
		bool m_Picking;
		bool m_PickingStarted;
		bool m_TreeEnabled;
		bool m_Capturing;
		bool m_Calculating;
		bool m_Closing;
		bool m_ClosePrompt;
		bool m_Started;
		bool m_Finished;
		int m_PickCount;
		int m_PickIndex = ALL_POINTS;
		const int POINT_COUNT = 3;
		const int AXIS_COUNT = 3;
		const int X_AXIS = 0;
		const int Y_AXIS = 1;
		const int Z_AXIS = 2;
		const int ALL_POINTS = -1;
		const double DEFAULT_TOLERANCE = 1d;
		const string DRAWING_POINTS = "圖紙點";
		const string WORKPIECE_POINTS = "工作點";
		const string DRAWING_INCOMPLETE = "請完整設定三個圖紙點。";
		const string WORKPIECE_INCOMPLETE = "請完整設定三個工作點。";
		const string INVALID_TOLERANCE = "誤差容忍必須是大於或等於 0 的有效數值。";
		const string INVALID_INPUT = "請輸入有效數值；誤差容忍不可小於 0。";
		const string INVALID_POINT = "{0} {1} 的 X、Y、Z 必須完整且為有效數值。";
		const string PROJECT_CHANGED = "專案或圖紙校正資料已改變，本次修改將不儲存。";
		const string DISCARD_UNCALCULATED = "目前資料尚未取得計算結果，或修改後尚未重新計算；關閉將還原上次計算資料。";
		const string DISCARD_AND_CLOSE = "是否放棄本次未計算的修改並關閉？";
		const string DRAWING_UNAVAILABLE = "請先加入工件，並完成目前的圖紙選點。";
		const string CALCULATION_ERROR = "校正計算失敗、誤差超過容忍，或未取得有效誤差；已保留本次計算資料。";
	}
}
