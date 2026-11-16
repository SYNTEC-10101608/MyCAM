using System;
using System.Windows.Forms;
using CommonUI.Controls;
using CommonUI.Forms;
using MyCAM.App;
using OCCTool;

namespace MyCAM.Editor.Dialog
{
	public partial class AddPathDlg : DialogBase
	{
		public AddPathDlg()
		{
			InitializeComponent();
			m_LanguageManager = new LanguageManager.LanguageManager( "AddPathDlg" );
			InitializeComboBox();

			// track last-known-good values so an invalid edit can be reverted immediately (on leaving
			// the field) instead of only being caught when the user presses Confirm.
			m_lastValidD1Threshold = AdvancedParams.D1Threshold;
			m_lastValidMaxChordError = AdvancedParams.MaxChordError;
			m_lastValidMaxLineLength = AdvancedParams.MaxLineLength;

			m_numD1Threshold.TextChanged += m_numD1Threshold_TextChanged;
			m_numD1Threshold.Validating += m_numD1Threshold_Validating;
			m_numD1Threshold.KeyDown += m_txbAdvanced_KeyDown;
			m_numMaxChordError.Validating += m_numMaxChordError_Validating;
			m_numMaxChordError.KeyDown += m_txbAdvanced_KeyDown;
			m_numMaxLineLength.Validating += m_numMaxLineLength_Validating;
			m_numMaxLineLength.KeyDown += m_txbAdvanced_KeyDown;

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

			m_pnlStep1.Visible = false;
			m_pnlStep3.Visible = false;

			switch( step ) {
				case EDialogStep.ChooseMode:
					m_pnlStep1.Visible = true;
					break;
				case EDialogStep.SelectFace:
					m_hasFaceSelection = false;
					m_lblStepDescription.Text = m_LanguageManager.TryGetString( "Desc_Step2_Title", DESC_STEP2_TITLE );
					m_lblStepHints.Text = m_LanguageManager.TryGetString( "Desc_Step2_Hints", DESC_STEP2_HINTS );
					break;
				case EDialogStep.ChooseWireMode:
					m_pnlStep3.Visible = true;
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
			m_pnlStepHint.Visible = isStepWithLabels;

			m_pnlAdvanced.Visible = m_chkShowAdvanced.Checked;

			// stack the section of the current step, the options and the advanced parameters
			Control stepSection = GetStepSection( isStepWithLabels );
			int nContentBottom = m_chkShowAdvanced.Checked
				? SectionStack.Arrange( stepSection, m_pnlOptions, m_pnlAdvanced )
				: SectionStack.Arrange( stepSection, m_pnlOptions );
			SectionStack.PlaceButtonBar( this, nContentBottom, m_btnNext, m_btnConfirm );

			this.ResumeLayout( true );
		}

		Control GetStepSection( bool isStepWithLabels )
		{
			// face and wire selection steps only show their description
			if( isStepWithLabels ) {
				return m_pnlStepHint;
			}
			return m_currentStep == EDialogStep.ChooseWireMode ? (Control)m_pnlStep3 : m_pnlStep1;
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
			if( sender == m_numD1Threshold ) {
				m_numD1Threshold_Validating( sender, dummy );
			}
			else if( sender == m_numMaxChordError ) {
				m_numMaxChordError_Validating( sender, dummy );
			}
			else if( sender == m_numMaxLineLength ) {
				m_numMaxLineLength_Validating( sender, dummy );
			}
		}

		void m_numD1Threshold_TextChanged( object sender, EventArgs e )
		{
			if( TryGetValidD1ThresholdFromTextBox( out double d1Threshold ) ) {
				D1ThresholdChanged?.Invoke( d1Threshold );
			}
		}

		void m_numD1Threshold_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ValidatePositiveTextBox( m_numD1Threshold, ref m_lastValidD1Threshold, IsD1ThresholdInRange );
		}

		void m_numMaxChordError_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ValidatePositiveTextBox( m_numMaxChordError, ref m_lastValidMaxChordError );
		}

		void m_numMaxLineLength_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		{
			ValidatePositiveTextBox( m_numMaxLineLength, ref m_lastValidMaxLineLength );
		}

		void ValidatePositiveTextBox( Control txb, ref double lastValid, Func<double, bool> rangeCheck = null )
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
			return double.TryParse( m_numD1Threshold.Text, out value ) && IsD1ThresholdInRange( value );
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

		double m_lastValidD1Threshold;
		double m_lastValidMaxChordError;
		double m_lastValidMaxLineLength;

		EDialogStep m_currentStep = EDialogStep.ChooseMode;
		bool m_isConfirmed = false;
		bool m_hasFaceSelection = false;
		bool m_hasWireSelection = false;


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
