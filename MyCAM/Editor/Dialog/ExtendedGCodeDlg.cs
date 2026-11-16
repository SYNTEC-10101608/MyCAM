using System;
using System.Linq;
using System.Windows.Forms;
using MyCAM.Data;

namespace MyCAM.Editor
{
	internal partial class ExtendedGCodeDlg : EditDialogBase<object>
	{
		public Action AddRequest;
		public Action DeleteRequest;
		public Action ToStartOrEnd;
		public Action<ExtendedGCodeType> PositionChanged;
		public Action<string> ContentChanged;

		public ExtendedGCodeDlg()
		{
			InitializeComponent();

			m_rdoTraverseStart.Tag = ExtendedGCodeType.TraverseHead;
			m_rdoTraverseEnd.Tag = ExtendedGCodeType.TraverseTail;
			m_rdoPathStart.Tag = ExtendedGCodeType.PathHead;
			m_rdoPathEnd.Tag = ExtendedGCodeType.PathTail;
			m_rdoControlPoint.Tag = ExtendedGCodeType.ControlPoint;
		}

		public void ResetParam( ExtendedGCodeParam param )
		{
			m_bSuppressValueChanged = true;
			m_Param = param;

			m_bContentDirty = false;

			// non selected
			if( m_Param == null ) {
				m_pnlRadioButton.Enabled = false;
				foreach( RadioButton rbtn in m_pnlRadioButton.Controls.OfType<RadioButton>() ) {
					rbtn.Checked = false;
				}

				m_lblIsStartOrEnd.Text = string.Empty;
				m_pnlToStartOrEnd.Enabled = false;

				m_lblIsMixData.Text = string.Empty;
				m_txtGCode.Text = string.Empty;
				m_txtGCode.Enabled = false;

				m_btnAdd.Enabled = false;
				m_btnDelete.Enabled = false;

				m_bSuppressValueChanged = false;
				return;
			}

			SetInputEnabled( m_Param.IsModified );

			m_pnlRadioButton.Enabled = true;
			m_rdoControlPoint.Enabled = m_Param.IsSingleSelectMode;
			foreach( RadioButton rbtn in m_pnlRadioButton.Controls.OfType<RadioButton>() ) {
				rbtn.Checked = rbtn.Tag is ExtendedGCodeType gCodeType && gCodeType == m_Param.GCodeType;
			}

			m_pnlToStartOrEnd.Enabled = true;
			m_btnToStartOrEnd.Enabled = ( m_Param.IsPathStartPoint != null );
			m_lblIsStartOrEnd.Text = ( m_Param.GCodeType != ExtendedGCodeType.ControlPoint || m_Param.IsPathStartPoint == null ) ?
				string.Empty
				: ( m_Param.IsPathStartPoint == true ?
				m_LanguageManager.TryGetString( "IsStartPOINT", IS_START_POINT )
				: m_LanguageManager.TryGetString( "IsEndPOINT", IS_END_POINT ) );

			m_lblIsMixData.Text = m_Param.IsMixData ? m_LanguageManager.TryGetString( "IsMixData", IS_MIX_DATA ) : string.Empty;
			m_txtGCode.Text = m_Param.Content;

			m_bSuppressValueChanged = false;
		}

		// must be called before the key changes
		public void FlushContent()
		{
			if( m_bSuppressValueChanged || m_Param == null || !m_bContentDirty ) {
				return;
			}
			m_bContentDirty = false;
			ContentChanged?.Invoke( m_txtGCode.Text );
		}

		#region UI events

		void ExtendedGCodeDlg_Deactivate( object sender, EventArgs e )
		{
			FlushContent();
		}

		void RadioButtonCheckedhanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}

			if( sender is RadioButton rbtn && rbtn.Checked ) {
				FlushContent();
				PositionChanged?.Invoke( (ExtendedGCodeType)rbtn.Tag );
			}
		}

		void m_btnAdd_Click( object sender, EventArgs e )
		{
			AddRequest?.Invoke();
			SetInputEnabled( true );
		}

		void m_btnDelete_Click( object sender, EventArgs e )
		{
			m_bContentDirty = false;
			m_txtGCode.Text = string.Empty;

			DeleteRequest?.Invoke();
			SetInputEnabled( false );
		}

		void m_btnDone_Click( object sender, EventArgs e )
		{
			// trigger FormClosing -> RaiseCancel
			Close();
		}

		void m_txtGCode_TextChanged( object sender, EventArgs e )
		{
			if( m_bSuppressValueChanged || m_Param == null ) {
				return;
			}
			m_bContentDirty = m_txtGCode.Text != m_Param.Content;
		}

		void m_txtGCode_Leave( object sender, EventArgs e )
		{
			FlushContent();
		}

		void m_btnToStartOrEnd_Click( object sender, EventArgs e )
		{
			FlushContent();
			ToStartOrEnd?.Invoke();
		}

		#endregion

		void SetInputEnabled( bool enabled )
		{
			m_txtGCode.Enabled = enabled;

			m_btnAdd.Enabled = !enabled;
			m_btnDelete.Enabled = enabled;
		}

		bool m_bSuppressValueChanged = false;
		bool m_bContentDirty = false;
		ExtendedGCodeParam m_Param = null;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExtendedGCodeDlg" );

		const string IS_MIX_DATA = "所選路徑的G碼不一致";
		const string IS_START_POINT = "目前位置：起點";
		const string IS_END_POINT = "目前位置：終點";
	}

	internal class ExtendedGCodeParam
	{
		public bool IsModified { get; set; }

		public ExtendedGCodeType GCodeType { get; set; }

		public string Content { get; set; }

		// true: path start point, false: path end point, null: cannot toggle
		public bool? IsPathStartPoint { get; set; }

		public bool IsSingleSelectMode { get; set; }

		public bool IsMixData { get; set; }

		public ExtendedGCodeParam( bool isModified, ExtendedGCodeType gCodeType, string content, bool? isPathStartPoint, bool isSingleSelectMode, bool isMixData = false )
		{
			IsModified = isModified;
			GCodeType = gCodeType;
			Content = content;
			IsPathStartPoint = isPathStartPoint;
			IsSingleSelectMode = isSingleSelectMode;
			IsMixData = isMixData;
		}
	}
}
