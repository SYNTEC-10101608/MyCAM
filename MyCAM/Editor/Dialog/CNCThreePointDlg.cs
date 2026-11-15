using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Forms;
using CommonUI.Theme;

namespace MyCAM.Editor.Dialog
{
	internal enum ThreePointStep
	{
		None,
		Drawing,
		Workpiece,
		Calculation,
	}

	public partial class CNCThreePointDlg : DialogBase
	{
		public CNCThreePointDlg()
		{
			InitializeComponent();
			m_DrawingFields = new Control[ , ] {
				{ m_txtDrawing1X, m_txtDrawing1Y, m_txtDrawing1Z },
				{ m_txtDrawing2X, m_txtDrawing2Y, m_txtDrawing2Z },
				{ m_txtDrawing3X, m_txtDrawing3Y, m_txtDrawing3Z }
			};
			m_WorkpieceFields = new[ , ] {
				{ m_txtWorkpiece1X, m_txtWorkpiece1Y, m_txtWorkpiece1Z },
				{ m_txtWorkpiece2X, m_txtWorkpiece2Y, m_txtWorkpiece2Z },
				{ m_txtWorkpiece3X, m_txtWorkpiece3Y, m_txtWorkpiece3Z }
			};
			ThemedButton[] captureButtons = { m_btnCapture1, m_btnCapture2, m_btnCapture3 };
			ThemedButton[] resetButtons = { m_btnReset1, m_btnReset2, m_btnReset3 };
			for( int point = 0; point < m_WorkpieceFields.GetLength( 0 ); point++ ) {
				int index = point;
				captureButtons[ point ].Click += ( sender, e ) => WorkpiecePointCaptureRequested?.Invoke( index );
				resetButtons[ point ].Click += ( sender, e ) => DrawingPointResetRequested?.Invoke( index );
				for( int axis = 0; axis < m_WorkpieceFields.GetLength( 1 ); axis++ ) {
					int axisIndex = axis;
					WireInput( m_WorkpieceFields[ point, axis ], () => PointInputCommitRequested?.Invoke( index, axisIndex ) );
				}
			}
			foreach( TextBox field in m_WorkpieceFields ) {
				field.TextChanged += OnInputChanged;
			}

			m_txtTolerance.TextChanged += OnInputChanged;
			WireInput( m_txtTolerance, () => ToleranceCommitRequested?.Invoke() );
			m_btnCalculate.Click += ( sender, e ) => CalculateRequested?.Invoke();
			m_btnPickDrawing.Click += ( sender, e ) => DrawingPickRequested?.Invoke();
			m_btnPickCancel.Click += ( sender, e ) => DrawingPickCancelRequested?.Invoke();
			m_chbDrawingOnly.CheckedChanged += ( sender, e ) =>
			{
				if( !m_Loading ) {
					DrawingOnlyChanged?.Invoke();
				}
			};
			ApplyResources();
		}

		internal LanguageManager.LanguageManager Language { get; } = new LanguageManager.LanguageManager( "CNCThreePointDlg" );
		internal event Action<int> WorkpiecePointCaptureRequested;
		internal event Action<int> DrawingPointResetRequested;
		internal event Action CalculateRequested;
		internal event Action DrawingPickRequested;
		internal event Action DrawingPickCancelRequested;
		internal event Action DrawingOnlyChanged;
		internal event Action InputChanged;
		internal event Action<int, int> PointInputCommitRequested;
		internal event Action ToleranceCommitRequested;

		internal string[,] DrawingInput => ReadFields( m_DrawingFields );
		internal string[,] WorkpieceInput => ReadFields( m_WorkpieceFields );
		internal bool DrawingOnlyChecked => m_chbDrawingOnly.Checked;
		internal string ToleranceText { get => m_txtTolerance.Text; set => m_txtTolerance.Text = value; }

		internal void SetInputs( string[,] drawing, string[,] workpiece, string tolerance )
		{
			m_Loading = true;
			try {
				for( int point = 0; point < m_DrawingFields.GetLength( 0 ); point++ ) {
					for( int axis = 0; axis < m_DrawingFields.GetLength( 1 ); axis++ ) {
						m_DrawingFields[ point, axis ].Text = drawing[ point, axis ];
						m_WorkpieceFields[ point, axis ].Text = workpiece[ point, axis ];
					}
				}
				m_txtTolerance.Text = tolerance;
			}
			finally { m_Loading = false; }
		}

		internal void SetDrawingPoint( int point, string[] coordinates )
		{
			WritePoint( m_DrawingFields, point, coordinates );
			InputChanged?.Invoke();
		}

		internal void SetWorkpiecePoint( int point, string[] coordinates )
		{
			WritePoint( m_WorkpieceFields, point, coordinates );
			InputChanged?.Invoke();
		}

		internal void SetWorkpieceInput( int point, int axis, string value ) => m_WorkpieceFields[ point, axis ].Text = value;

		static string[,] ReadFields( Control[,] fields )
		{
			string[,] values = new string[ fields.GetLength( 0 ), fields.GetLength( 1 ) ];
			for( int point = 0; point < fields.GetLength( 0 ); point++ ) {
				for( int axis = 0; axis < fields.GetLength( 1 ); axis++ ) {
					values[ point, axis ] = fields[ point, axis ].Text;
				}
			}
			return values;
		}

		void WritePoint( Control[,] fields, int point, string[] coordinates )
		{
			m_Loading = true;
			try {
				for( int axis = 0; axis < fields.GetLength( 1 ); axis++ ) {
					fields[ point, axis ].Text = coordinates[ axis ];
				}
			}
			finally {
				m_Loading = false;
			}
		}

		void OnInputChanged( object sender, EventArgs e )
		{
			if( !m_Loading ) {
				InputChanged?.Invoke();
			}
		}

		void WireInput( TextBox field, Action commit )
		{
			field.Leave += ( sender, e ) => commit();
			field.KeyDown += ( sender, e ) =>
			{
				if( e.KeyCode == Keys.Enter ) {
					e.SuppressKeyPress = true;
					e.Handled = true;
					commit();
				}
			};
		}

		internal void DisplayState( ThreePointStep step, bool connected, bool drawingOnly, bool drawingComplete,
			bool workpieceComplete, bool picking, int pickCount, int pickIndex, bool busy, bool capturing,
			double? error, bool successful, bool completed )
		{
			bool calibrationVisible = connected && !drawingOnly;
			bool restoreVisibility = Visible && m_CalibrationVisible != calibrationVisible;
			if( restoreVisibility ) {
				Hide();
			}
			SuspendLayout();
			try {
				UpdateDisplayState( step, connected, drawingOnly, drawingComplete, workpieceComplete,
					picking, pickCount, pickIndex, busy, capturing );
				DisplayCalculationError( error, successful, completed );
				SetCalibrationVisible( calibrationVisible );
			}
			finally {
				try {
					ResumeLayout( false );
					PerformLayout();
				}
				finally {
					if( restoreVisibility && !IsDisposed && !Disposing ) {
						Show();
						Refresh();
					}
				}
			}
		}

		void UpdateDisplayState( ThreePointStep step, bool connected, bool drawingOnly, bool drawingComplete,
			bool workpieceComplete, bool picking, int pickCount, int pickIndex, bool busy, bool capturing )
		{
			m_Loading = true;
			try { m_chbDrawingOnly.Checked = drawingOnly; }
			finally { m_Loading = false; }
			m_chbDrawingOnly.Text = connected ? Language.TryGetString( "DrawingOnly", DRAWING_ONLY )
				: Language.TryGetString( "DrawingOnly_Offline", DRAWING_ONLY_OFFLINE );
			bool enabled = !busy && !picking;
			m_pnlDrawing.Enabled = !busy;
			foreach( Control field in m_DrawingFields ) {
				field.Enabled = enabled;
			}

			m_btnPickDrawing.Enabled = enabled && !capturing;
			m_btnReset1.Enabled = m_btnReset2.Enabled = m_btnReset3.Enabled = enabled && !capturing;
			m_btnPickCancel.Visible = picking;
			m_btnPickCancel.Enabled = picking;
			m_chbDrawingOnly.Enabled = enabled && !capturing && connected;
			bool calibrationEnabled = connected && !drawingOnly;
			m_pnlWorkpiece.Enabled = calibrationEnabled && enabled && drawingComplete && !capturing;
			m_txtTolerance.Enabled = calibrationEnabled && enabled;
			m_btnCalculate.Enabled = calibrationEnabled && enabled && drawingComplete && !capturing;
			SetStepAppearance( m_pnlDrawing, Language.TryGetString( "Step_Drawing", DRAWING_STEP ), ThreePointStep.Drawing, step );
			SetStepAppearance( m_pnlWorkpiece, Language.TryGetString( "Step_Workpiece", WORKPIECE_STEP ), ThreePointStep.Workpiece, step );
			SetStepAppearance( m_secCalculation, Language.TryGetString( "Step_Calculation", CALCULATION_STEP ), ThreePointStep.Calculation, step );
			m_lblDrawingDescription.Text = picking
				? pickIndex >= 0 ? string.Format( Language.TryGetString( "Drawing_Repicking", DRAWING_REPICKING ), pickIndex + 1 )
				: string.Format( Language.TryGetString( "Drawing_Picking", DRAWING_PICKING ), pickCount + 1, pickCount )
				: drawingComplete ? Language.TryGetString( "Drawing_Complete", DRAWING_COMPLETE ) : Language.TryGetString( "Drawing_Initial", DRAWING_INITIAL );
			m_lblDrawingDescription.Role = step == ThreePointStep.Drawing ? TextRole.Body : TextRole.Secondary;
			m_lblWorkpieceDescription.Text = step == ThreePointStep.Drawing ? Language.TryGetString( "Workpiece_Waiting", WORKPIECE_WAITING )
				: workpieceComplete ? Language.TryGetString( "Workpiece_Complete", WORKPIECE_COMPLETE ) : Language.TryGetString( "Workpiece_Initial", WORKPIECE_INITIAL );
			m_lblWorkpieceDescription.Role = step == ThreePointStep.Workpiece ? TextRole.Body : TextRole.Secondary;
		}

		void SetCalibrationVisible( bool visible )
		{
			if( m_CalibrationVisible == visible ) {
				return;
			}

			m_CalibrationVisible = visible;
			m_pnlWorkpiece.Visible = m_secCalculation.Visible = visible;
			m_lblTolerance.Visible = m_txtTolerance.Visible = m_lblToleranceUnit.Visible = visible;
			m_lblError.Visible = m_lblErrorValue.Visible = m_btnCalculate.Visible = visible;

			// the dialog ends below the last shown section
			int nContentBottom = visible
				? SectionStack.Arrange( m_pnlDrawing, m_secOption, m_pnlWorkpiece, m_secCalculation )
				: SectionStack.Arrange( m_pnlDrawing, m_secOption );
			SectionStack.PlaceButtonBar( this, nContentBottom );
		}

		void DisplayCalculationError( double? error, bool successful, bool completed )
		{
			m_lblErrorValue.Text = error.HasValue ? error.Value.ToString( NUMBER_FORMAT, CultureInfo.InvariantCulture ) + " mm"
				: completed ? Language.TryGetString( "Error_Unavailable", ERROR_UNAVAILABLE ) : "— mm";
			m_lblErrorValue.Role = !error.HasValue ? TextRole.Secondary : successful ? TextRole.Success : TextRole.Error;
		}

		public void ApplyResources()
		{
			Text = Language.TryGetString( "Title", TITLE );
			m_btnPickDrawing.Text = Language.TryGetString( "PickDrawing", PICK_DRAWING );
			m_btnReset1.Text = m_btnReset2.Text = m_btnReset3.Text = Language.TryGetString( "Reset", RESET );
			m_btnPickCancel.Text = Language.TryGetString( "Cancel", CANCEL );
			Label[] drawingLabels = { m_lblDrawing1, m_lblDrawing2, m_lblDrawing3 };
			Label[] workpieceLabels = { m_lblWorkpiece1, m_lblWorkpiece2, m_lblWorkpiece3 };
			for( int point = 0; point < drawingLabels.Length; point++ ) {
				drawingLabels[ point ].Text = workpieceLabels[ point ].Text = string.Format( Language.TryGetString( "Point", POINT ), point + 1 );
			}
			m_btnCapture1.Text = m_btnCapture2.Text = m_btnCapture3.Text = Language.TryGetString( "Capture", CAPTURE );
			m_lblTolerance.Text = Language.TryGetString( "Tolerance", TOLERANCE );
			m_lblError.Text = Language.TryGetString( "Error", ERROR );
			m_btnCalculate.Text = Language.TryGetString( "Calculate", CALCULATE );
		}

		void SetStepAppearance( SectionPanel section, string title, ThreePointStep step, ThreePointStep current )
		{
			bool active = step == current;
			string status = active ? Language.TryGetString( "Step_Current", STEP_CURRENT )
				: current == ThreePointStep.None || step < current ? Language.TryGetString( "Step_Complete", STEP_COMPLETE )
				: step == ThreePointStep.Workpiece ? Language.TryGetString( "Step_WaitDrawing", STEP_WAIT_DRAWING ) : Language.TryGetString( "Step_WaitBoth", STEP_WAIT_BOTH );
			section.Text = string.Format( Language.TryGetString( "Step_Format", STEP_FORMAT ), title, status );
			section.Highlighted = active;
		}

		readonly Control[,] m_DrawingFields;
		readonly TextBox[,] m_WorkpieceFields;
		bool m_Loading;
		bool m_CalibrationVisible = true;

		const string NUMBER_FORMAT = "F3";
		internal const string TITLE = "三點校正";
		const string PICK_DRAWING = "重選圖紙點";
		const string RESET = "重設";
		const string CANCEL = "取消";
		const string POINT = "點 {0}";
		const string CAPTURE = "帶入";
		const string TOLERANCE = "誤差容忍";
		const string ERROR = "誤差";
		const string CALCULATE = "計算";
		const string DRAWING_ONLY = "僅輸出圖紙點";
		const string DRAWING_ONLY_OFFLINE = "僅輸出圖紙點（離線）";
		const string ERROR_UNAVAILABLE = "未取得";
		const string DRAWING_STEP = "步驟一：設定圖紙點";
		const string WORKPIECE_STEP = "步驟二：設定工作點";
		const string CALCULATION_STEP = "步驟三：設定誤差與計算";
		const string DRAWING_REPICKING = "請到主畫面畫布重新點選圖紙點 {0}。";
		const string DRAWING_PICKING = "請到主畫面畫布點選第 {0} 個圖紙點（已選 {1}／3 點）。";
		const string DRAWING_COMPLETE = "圖紙點已完成；需要重新選取時，請按「重選圖紙點」。";
		const string DRAWING_INITIAL = "請在主畫面的畫布上依序選取三個圖紙點。";
		const string WORKPIECE_WAITING = "請先在主畫面畫布選滿三個圖紙點。";
		const string WORKPIECE_COMPLETE = "工作點已完整；可設定誤差容忍並按「計算」。";
		const string WORKPIECE_INITIAL = "請依序移動至三個工作點，按「帶入」或輸入座標。";
		const string STEP_CURRENT = "目前步驟";
		const string STEP_COMPLETE = "已完成";
		const string STEP_WAIT_DRAWING = "等待步驟一完成";
		const string STEP_WAIT_BOTH = "等待前兩步完成";
		const string STEP_FORMAT = "{0}（{1}）";
	}
}
