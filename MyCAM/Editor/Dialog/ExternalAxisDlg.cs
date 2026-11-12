using MyCAM.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal partial class ExternalAxisDlg : EditDialogBase<ExternalAxisEditData>
	{
		public ExternalAxisDlg( ExternalAxisType type, List<ExternalAxisPos> positions )
		{
			if( type != ExternalAxisType.Single && type != ExternalAxisType.Dual ) {
				throw new ArgumentException( "An enabled external axis configuration is required." );
			}
			m_Type = type;
			InitializeComponent();
			ApplyResourcesToUI();
			m_lblExt2.Visible = m_dbtnExt2.Visible = type == ExternalAxisType.Dual;
			SetPositions( positions );
			m_dbtnExt1.TextChanged += OnAngleTextChanged;
			m_dbtnExt2.TextChanged += OnAngleTextChanged;
			m_dbtnExt1.ValueChanged += OnAngleValueChanged;
			m_dbtnExt2.ValueChanged += OnAngleValueChanged;
			m_dbtnExt1.KeyDown += OnAngleKeyDown;
			m_dbtnExt2.KeyDown += OnAngleKeyDown;
			m_dbtnExt1.ValueRejected += OnValueRejected;
			m_dbtnExt2.ValueRejected += OnValueRejected;
		}

		public void SetPositions( List<ExternalAxisPos> positions )
		{
			positions = positions ?? new List<ExternalAxisPos>();
			m_IsUpdatingUI = true;
			try {
				m_dbtnExt1.Enabled = m_lblExt1.Enabled = positions.Count > 0;
				m_dbtnExt2.Enabled = m_lblExt2.Enabled = positions.Count > 0 && m_Type == ExternalAxisType.Dual;
				bool ext1InRange = SetAngle( m_dbtnExt1, positions.Select( pos => pos.Ext1 ) );
				bool ext2InRange = SetAngle( m_dbtnExt2, positions.Select( pos => pos.Ext2 ) );
				m_lblError.Text = !ext1InRange || ( m_Type == ExternalAxisType.Dual && !ext2InRange )
					? m_LanguageManager.TryGetString( "ExternalAxisExistingAngleOutOfRange", "既有角度超出輸入範圍，顯示為 0；未編輯的軸不會被覆寫。" ) : string.Empty;
				m_Ext1Edited = m_Ext2Edited = false;
			}
			finally {
				m_IsUpdatingUI = false;
			}
		}

		void ApplyResourcesToUI()
		{
			Text = m_LanguageManager.TryGetString( "ExternalAxisDlg", Text );
			m_lblHint.Text = m_LanguageManager.TryGetString( "ExternalAxisHint", m_lblHint.Text );
			m_btnConfirm.Text = m_LanguageManager.TryGetString( "m_btnConfirm", m_btnConfirm.Text );
			m_btnCancel.Text = m_LanguageManager.TryGetString( "m_btnCancel", m_btnCancel.Text );
		}

		static bool SetAngle( DebouncedNumericUpDown control, IEnumerable<double> angles )
		{
			double[] values = angles.Distinct().ToArray();
			bool inRange = values.All( value => value >= (double)control.Minimum && value <= (double)control.Maximum );
			control.SilentSetValue = values.Length == 1 && inRange ? (decimal)values[ 0 ] : 0m;
			return inRange;
		}

		static bool TryParseAngle( string text, out double? angle )
		{
			angle = null;
			if( !decimal.TryParse( text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal value )
				|| value < -99999m || value > 99999m ) {
				return false;
			}
			angle = (double)value;
			return true;
		}

		bool TryGetEditData( out ExternalAxisEditData data )
		{
			data = null;
			double? ext1 = null;
			double? ext2 = null;
			bool valid = !m_Ext1Edited || TryParseAngle( m_dbtnExt1.Text, out ext1 );
			if( m_Type == ExternalAxisType.Dual && m_Ext2Edited ) {
				valid = TryParseAngle( m_dbtnExt2.Text, out ext2 ) && valid;
			}
			if( !valid ) {
				ShowInputError();
				return false;
			}
			m_IsUpdatingUI = true;
			try {
				if( ext1.HasValue ) {
					m_dbtnExt1.SilentSetValue = (decimal)ext1.Value;
				}
				if( ext2.HasValue ) {
					m_dbtnExt2.SilentSetValue = (decimal)ext2.Value;
				}
				m_Ext1Edited = m_Ext2Edited = false;
			}
			finally {
				m_IsUpdatingUI = false;
			}
			m_lblError.Text = string.Empty;
			data = new ExternalAxisEditData( ext1, ext2 );
			return true;
		}

		void OnAngleTextChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingUI ) {
				return;
			}
			if( sender == m_dbtnExt1 ) {
				m_Ext1Edited = true;
			}
			else {
				m_Ext2Edited = true;
			}
		}

		void OnAngleValueChanged( object sender, EventArgs e )
		{
			if( m_IsUpdatingUI ) {
				return;
			}
			m_lblError.Text = string.Empty;
			if( sender == m_dbtnExt1 ) {
				m_Ext1Edited = true;
				RaisePreview( new ExternalAxisEditData( (double)m_dbtnExt1.Value, null ) );
			}
			else if( m_Type == ExternalAxisType.Dual ) {
				m_Ext2Edited = true;
				RaisePreview( new ExternalAxisEditData( null, (double)m_dbtnExt2.Value ) );
			}
		}

		void OnAngleKeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewEnteredValues();
				e.Handled = e.SuppressKeyPress = true;
			}
		}

		protected override bool ProcessDialogKey( Keys keyData )
		{
			if( ( keyData & Keys.KeyCode ) == Keys.Enter ) {
				PreviewEnteredValues();
				return true;
			}
			return base.ProcessDialogKey( keyData );
		}

		void PreviewEnteredValues()
		{
			if( TryGetEditData( out ExternalAxisEditData data ) ) {
				RaisePreview( data );
			}
		}

		void OnValueRejected( object sender, ValueRejectedEventArgs e )
		{
			ShowInputError();
		}

		void ShowInputError()
		{
			m_lblError.Text = m_LanguageManager.TryGetString( "ExternalAxisInputRange", "請輸入 -99999 至 99999 度的角度；不可留白。" );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			if( TryGetEditData( out ExternalAxisEditData data ) ) {
				RaiseConfirm( data );
			}
		}

		void m_btnCancel_Click( object sender, EventArgs e )
		{
			Close();
		}

		readonly ExternalAxisType m_Type;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExternalAxisDlg" );
		bool m_IsUpdatingUI;
		bool m_Ext1Edited;
		bool m_Ext2Edited;
	}
}
