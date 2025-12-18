using MyCAM.App;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class OverCutDlg : EditDialogBase<double>
	{
		public OverCutDlg( double overCutLength )
		{
			InitializeComponent();
			m_OverCutLength = overCutLength;
			m_tbxOverCutLength.Text = m_OverCutLength.ToString();
		}

		public Func<double, bool> CheckValueAccrodGeomRestriction;

		void m_tbxOverCutLength_Leave( object sender, EventArgs e )
		{
			PreviewOverCutResult();
		}

		void m_tbxOverCutLength_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewOverCutResult();
			}
		}

		void m_btnSure_Click( object sender, EventArgs e )
		{
			if( IsValidParam() ) {
				RaiseConfirm( m_OverCutLength );
			}
		}

		void PreviewOverCutResult()
		{
			if( IsValidParam() ) {
				RaisePreview( m_OverCutLength );
			}
		}

		bool IsValidParam()
		{
			if( !double.TryParse( m_tbxOverCutLength.Text, out double dOverCutLength ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return false;
			}
			if( dOverCutLength < 0 ) {
				MyApp.Logger.ShowOnLogPanel( "長度需要大於0", MyApp.NoticeType.Warning );
				return false;
			}
			if( CheckValueAccrodGeomRestriction?.Invoke( dOverCutLength ) == false ) {
				MyApp.Logger.ShowOnLogPanel( "過切長度超出幾何限制", MyApp.NoticeType.Warning );
				return false;
			}
			m_OverCutLength = dOverCutLength;
			return true;
		}

		double m_OverCutLength = 0;
	}
}
