using System;
using System.Windows.Forms;
using MyCAM.App;
using OCCTool;

namespace MyCAM.Editor.Dialog
{
	public partial class AddPathDlg : Form
	{
		public AddPathDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "AddPathDlg" );
			ApplyResourcesToUI();
			InitializeComboBox();

			// track last-known-good values so an invalid edit can be reverted immediately (on leaving
			// the field) instead of only being caught when the user presses Confirm.
			m_lastValidD1Threshold = AdvancedParams.D1Threshold;
			m_lastValidMaxChordError = AdvancedParams.MaxChordError;
			m_lastValidMaxLineLength = AdvancedParams.MaxLineLength;

			m_txbD1Threshold.TextChanged += m_txbD1Threshold_TextChanged;
			m_txbD1Threshold.Validating += m_txbD1Threshold_Validating;
			m_txbD1Threshold.KeyDown += m_txbAdvanced_KeyDown;
			m_txbMaxChordError.Validating += m_txbMaxChordError_Validating;
			m_txbMaxChordError.KeyDown += m_txbAdvanced_KeyDown;
			m_txbMaxLineLength.Validating += m_txbMaxLineLength_Validating;
			m_txbMaxLineLength.KeyDown += m_txbAdvanced_KeyDown;

			m_lblStepDescription.Visible = false;
			m_lblStepHints.Visible = false;

			GoToStep( EDialogStep.ChooseMode );
		}

		// Callbacks wired up by AddPathAction.
		internal Action<EAddPathMode> AutoFindPathConfirm;
		internal Action Cancel;
		internal Action StartSelectFace;
		internal Action FreezeSelectFace;
		internal Func<string> AutoFromFaceConfirm;
		internal Func<string> StartWireSelect;
		internal Func<string> WireConfirm;
		internal Action CancelSubAction;
		internal Action<double> D1ThresholdChanged;

		public AddPathAdvancedParams AdvancedParams { get; private set; } = new AddPathAdvancedParams();

		public double CurrentD1ThresholdDegrees => m_lastValidD1Threshold;

		// Called by AddPathAction when face selection changes.
		public void SetFaceSelected( bool hasSelection )
		{
			if( m_currentStep != EDialogStep.SelectFace ) {
				return;
			}
			m_hasFaceSelection = hasSelection;
			UpdateButtons();
		}

		// Called by AddPathAction when wire selection changes.
		public void SetWireSelected( bool hasSelection )
		{
			if( m_currentStep != EDialogStep.SelectWire ) {
				return;
			}
			m_hasWireSelection = hasSelection;
			UpdateButtons();
		}

		void GoToStep( EDialogStep step )
		{
			m_currentStep = step;
			ClearError();

			m_grpStep1.Visible = false;
			m_grpStep3.Visible = false;

			switch( step ) {
				case EDialogStep.ChooseMode:
					m_grpStep1.Visible = true;
					break;
				case EDialogStep.SelectFace:
					m_hasFaceSelection = false;
					m_lblStepDescription.Text = m_LanguageManager.TryGetString( "Desc_Step2_Title", DESC_STEP2_TITLE );
					m_lblStepHints.Text = m_LanguageManager.TryGetString( "Desc_Step2_Hints", DESC_STEP2_HINTS );
					break;
				case EDialogStep.ChooseWireMode:
					m_grpStep3.Visible = true;
					m_rdoAutoFromFace.Checked = true;
					break;
				case EDialogStep.SelectWire:
					m_hasWireSelection = false;
					m_lblStepDescription.Text = m_LanguageManager.TryGetString( "Desc_Step4_Title", DESC_STEP4_TITLE );
					m_lblStepHints.Text = m_LanguageManager.TryGetString( "Desc_Step4_Hints", DESC_STEP4_HINTS );
					break;
				default:
					break;
			}

			UpdateButtons();
			UpdateLayout();
		}

		void UpdateButtons()
		{
			switch( m_currentStep ) {
				case EDialogStep.ChooseMode:
					m_btnConfirm.Enabled = !m_rdoManual.Checked;
					m_btnNext.Enabled = m_rdoManual.Checked;
					m_btnNext.Visible = true;
					m_btnConfirm.Visible = true;
					break;
				case EDialogStep.SelectFace:
					m_btnConfirm.Visible = false;
					m_btnNext.Enabled = m_hasFaceSelection;
					m_btnNext.Visible = true;
					break;
				case EDialogStep.ChooseWireMode:
					m_btnConfirm.Enabled = m_rdoAutoFromFace.Checked;
					m_btnNext.Enabled = m_rdoManualWire.Checked;
					m_btnNext.Visible = true;
					m_btnConfirm.Visible = true;
					break;
				case EDialogStep.SelectWire:
					m_btnConfirm.Enabled = m_hasWireSelection;
					m_btnNext.Visible = false;
					m_btnConfirm.Visible = true;
					break;
				default:
					break;
			}
		}

		void UpdateLayout()
		{
			this.SuspendLayout();

			bool isStepWithLabels = ( m_currentStep == EDialogStep.SelectFace || m_currentStep == EDialogStep.SelectWire );
			m_lblStepDescription.Visible = isStepWithLabels;
			m_lblStepHints.Visible = isStepWithLabels;

			m_grpAdvanced.Visible = m_chkShowAdvanced.Checked;
			if( m_chkShowAdvanced.Checked ) {
				m_grpAdvanced.Location = new System.Drawing.Point( m_grpAdvanced.Left, m_chkShowAdvanced.Bottom + ADVANCED_GROUP_GAP );
			}

			int clientHeight = m_chkShowAdvanced.Checked
				? m_grpAdvanced.Bottom + EDGE_MARGIN
				: m_chkShowAdvanced.Bottom + EDGE_MARGIN;
			this.ClientSize = new System.Drawing.Size( this.ClientSize.Width, clientHeight );

			this.ResumeLayout( true );
		}

		void OnStep1RadioChanged( object sender, EventArgs e )
		{
			UpdateButtons();
		}

		void OnStep3RadioChanged( object sender, EventArgs e )
		{
			UpdateButtons();
		}

		void m_chkShowAdvanced_CheckedChanged( object sender, EventArgs e )
		{
			UpdateLayout();
		}

		void m_btnNext_Click( object sender, EventArgs e )
		{
			ClearError();
			if( m_chkShowAdvanced.Checked ) {
				ReadAdvancedParams();
			}

			switch( m_currentStep ) {
				case EDialogStep.ChooseMode:
					GoToStep( EDialogStep.SelectFace );
					StartSelectFace?.Invoke();
					break;
				case EDialogStep.SelectFace:
					FreezeSelectFace?.Invoke();
					GoToStep( EDialogStep.ChooseWireMode );
					break;
				case EDialogStep.ChooseWireMode:
					string err = StartWireSelect?.Invoke();
					if( !string.IsNullOrEmpty( err ) ) {
						ShowError( err );
						return;
					}
					GoToStep( EDialogStep.SelectWire );
					break;
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			ClearError();
			if( m_chkShowAdvanced.Checked ) {
				ReadAdvancedParams();
			}

			switch( m_currentStep ) {
				case EDialogStep.ChooseMode: {
						EAddPathMode autoMode = m_rdoAutoHat.Checked ? EAddPathMode.Hat : EAddPathMode.Tube;
						AutoFindPathConfirm?.Invoke( autoMode );
						m_isConfirmed = true;
						Close();
						break;
					}
				case EDialogStep.ChooseWireMode: {
						string err = AutoFromFaceConfirm?.Invoke();
						if( !string.IsNullOrEmpty( err ) ) {
							ShowError( err );
							return;
						}
						m_isConfirmed = true;
						Close();
						break;
					}
				case EDialogStep.SelectWire: {
						string err = WireConfirm?.Invoke();
						if( !string.IsNullOrEmpty( err ) ) {
							ShowError( err );
							return;
						}
						m_isConfirmed = true;
						Close();
						break;
					}
			}
		}

		void AddPathDlg_FormClosing( object sender, FormClosingEventArgs e )
		{
			if( !m_isConfirmed ) {
				Cancel?.Invoke();
			}
		}

		void m_txbAdvanced_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			e.SuppressKeyPress = true;
			var dummy = new System.ComponentModel.CancelEventArgs();
			if( sender == m_txbD1Threshold ) {
				m_txbD1Threshold_Validating( sender, dummy );
			}
			else if( sender == m_txbMaxChordError ) {
				m_txbMaxChordError_Validating( sender, dummy );
			}
			else if( sender == m_txbMaxLineLength ) {
				m_txbMaxLineLength_Validating( sender, dummy );
			}
		}

		void m_txbD1Threshold_TextChanged( object sender, EventArgs e )
		{
			if( TryGetValidD1ThresholdFromTextBox( out double d1Threshold ) ) {
				D1ThresholdChanged?.Invoke( d1Threshold );
			}
		}

		void m_txbD1Threshold_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ValidatePositiveTextBox( m_txbD1Threshold, ref m_lastValidD1Threshold, IsD1ThresholdInRange );
		}

		void m_txbMaxChordError_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ValidatePositiveTextBox( m_txbMaxChordError, ref m_lastValidMaxChordError );
		}

		void m_txbMaxLineLength_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ValidatePositiveTextBox( m_txbMaxLineLength, ref m_lastValidMaxLineLength );
		}

		void ValidatePositiveTextBox( TextBox txb, ref double lastValid, Func<double, bool> rangeCheck = null )
		{
			if( double.TryParse( txb.Text, out double value ) && value > 0
				&& ( rangeCheck == null || rangeCheck( value ) ) ) {
				lastValid = value;
				ClearError();
				return;
			}
			txb.Text = lastValid.ToString();
		}

		static bool IsD1ThresholdInRange( double value )
		{
			return value >= D1_THRESHOLD_MIN && value <= D1_THRESHOLD_MAX;
		}

		bool TryGetValidD1ThresholdFromTextBox( out double value )
		{
			return double.TryParse( m_txbD1Threshold.Text, out value ) && IsD1ThresholdInRange( value );
		}

		void ReadAdvancedParams()
		{
			AdvancedParams.D1Threshold = m_lastValidD1Threshold;
			AdvancedParams.DiscreteMethod = m_cmbDiscreteMode.SelectedIndex == DISCRETE_MODE_QUASI_INDEX
				? EDiscretizeMethod.QuasiUniformDeflection : EDiscretizeMethod.UniformDeflection;
			AdvancedParams.MaxChordError = m_lastValidMaxChordError;
			AdvancedParams.MaxLineLength = m_lastValidMaxLineLength;
		}

		void ShowError( string message )
		{
			m_lblError.Text = message;
			m_lblError.Visible = true;
			MyApp.Logger.ShowOnLogPanel( message, MyApp.NoticeType.Hint );
		}

		void ClearError()
		{
			if( !m_lblError.Visible ) {
				return;
			}
			m_lblError.Text = string.Empty;
			m_lblError.Visible = false;
		}

		void InitializeComboBox()
		{
			m_cmbDiscreteMode.Items.Clear();
			m_cmbDiscreteMode.Items.Add( m_LanguageManager.TryGetString( "DiscreteMode_QuasiUniformDeflection", DISCRETE_MODE_QUASI ) );
			m_cmbDiscreteMode.Items.Add( m_LanguageManager.TryGetString( "DiscreteMode_UniformDeflection", DISCRETE_MODE_CHORD ) );
			m_cmbDiscreteMode.SelectedIndex = 0;
		}

		void ApplyResourcesToUI()
		{
			this.Text = m_LanguageManager.TryGetString( "AddPathDlg", this.Text );
			m_grpStep1.Text = m_LanguageManager.TryGetString( "m_grpStep1", m_grpStep1.Text );
			m_rdoAutoHat.Text = m_LanguageManager.TryGetString( "m_rdoAutoHat", m_rdoAutoHat.Text );
			m_rdoAutoTube.Text = m_LanguageManager.TryGetString( "m_rdoAutoTube", m_rdoAutoTube.Text );
			m_rdoManual.Text = m_LanguageManager.TryGetString( "m_rdoManual", m_rdoManual.Text );
			m_grpStep3.Text = m_LanguageManager.TryGetString( "m_grpStep3", m_grpStep3.Text );
			m_rdoAutoFromFace.Text = m_LanguageManager.TryGetString( "m_rdoAutoFromFace", m_rdoAutoFromFace.Text );
			m_rdoManualWire.Text = m_LanguageManager.TryGetString( "m_rdoManualWire", m_rdoManualWire.Text );
			m_chkShowAdvanced.Text = m_LanguageManager.TryGetString( "m_chkShowAdvanced", m_chkShowAdvanced.Text );
			m_grpAdvanced.Text = m_LanguageManager.TryGetString( "m_grpAdvanced", m_grpAdvanced.Text );
			m_lblD1Threshold.Text = m_LanguageManager.TryGetString( "m_lblD1Threshold", m_lblD1Threshold.Text );
			m_lblDiscreteMode.Text = m_LanguageManager.TryGetString( "m_lblDiscreteMode", m_lblDiscreteMode.Text );
			m_lblMaxChordError.Text = m_LanguageManager.TryGetString( "m_lblMaxChordError", m_lblMaxChordError.Text );
			m_lblMaxLineLength.Text = m_LanguageManager.TryGetString( "m_lblMaxLineLength", m_lblMaxLineLength.Text );
			m_lblD1Unit.Text = m_LanguageManager.TryGetString( "m_lblD1Unit", m_lblD1Unit.Text );
			m_lblChordUnit.Text = m_LanguageManager.TryGetString( "m_lblChordUnit", m_lblChordUnit.Text );
			m_lblLineUnit.Text = m_LanguageManager.TryGetString( "m_lblLineUnit", m_lblLineUnit.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnOK", m_btnConfirm.Text );
			m_btnNext.Text = m_LanguageManager.TryGetString( "m_btnNext", m_btnNext.Text );
		}

		double m_lastValidD1Threshold;
		double m_lastValidMaxChordError;
		double m_lastValidMaxLineLength;

		EDialogStep m_currentStep = EDialogStep.ChooseMode;
		bool m_isConfirmed = false;
		bool m_hasFaceSelection = false;
		bool m_hasWireSelection = false;

		const int EDGE_MARGIN = 15;
		const int ADVANCED_GROUP_GAP = 5;

		// ComboBox index for QuasiUniformDeflection, must match the order items are added in InitializeComboBox
		const int DISCRETE_MODE_QUASI_INDEX = 0;

		const double D1_THRESHOLD_MIN = 0.001;
		const double D1_THRESHOLD_MAX = 90.0;

		const string DESC_STEP2_TITLE = "選擇欲生成路徑的面";
		const string DESC_STEP2_HINTS = "滑鼠雙擊：連續面\r\nCtrl：多選";
		const string DESC_STEP4_TITLE = "選擇欲生成的路徑";
		const string DESC_STEP4_HINTS = "滑鼠雙擊：連續路徑\r\nCtrl：多選";

		const string DISCRETE_MODE_QUASI = "普通離散";
		const string DISCRETE_MODE_CHORD = "精確離散";

		readonly LanguageManager.LanguageManager m_LanguageManager;
	}

	public enum EDialogStep
	{
		ChooseMode,
		SelectFace,
		ChooseWireMode,
		SelectWire,
	}

	public enum EAddPathMode
	{
		Hat,
		Tube,
		ManualSelectFace,
		ManualSelectWire
	}

	public class AddPathAdvancedParams
	{
		public double D1Threshold { get; set; } = 5.0;
		public EDiscretizeMethod DiscreteMethod { get; set; } = EDiscretizeMethod.QuasiUniformDeflection;
		public double MaxChordError { get; set; } = 0.01;
		public double MaxLineLength { get; set; } = 1.0;
	}
}
