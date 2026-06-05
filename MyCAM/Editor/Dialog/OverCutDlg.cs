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
			m_tbxOverCutLength.SilentSetValue = (decimal)m_OverCutLength;
		}

		public Func<double, bool> CheckValueGeomRestriction;

		void m_tbxOverCutLength_DebouncedValueChanged( object sender, EventArgs e )
		{
			PreviewOverCutResult();
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
			double dOverCutLength = (double)m_tbxOverCutLength.Value;
			if( dOverCutLength < 0 ) {
				MyApp.Logger.ShowOnLogPanel( "長度需要大於0", MyApp.NoticeType.Warning );
				return false;
			}
			if( CheckValueGeomRestriction?.Invoke( dOverCutLength ) == false ) {
				MyApp.Logger.ShowOnLogPanel( "過切長度超出幾何限制", MyApp.NoticeType.Warning );
				return false;
			}
			m_OverCutLength = dOverCutLength;
			return true;
		}

		double m_OverCutLength = 0;
	}
}
