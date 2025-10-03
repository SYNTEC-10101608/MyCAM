using System;
using System.Windows.Forms;
using MyCAM.App;

namespace MyCAM.Editor
{
	public partial class ToolVectorDlg : Form
	{
		public ToolVectorDlg( bool bModified, double angleA_deg, double angleB_deg )
		{
			InitializeComponent();

			// initialize textbox
			m_tbxAngleA.Text = angleA_deg.ToString();
			m_tbxAngleB.Text = angleB_deg.ToString();

			// initialize button
			m_btnRemove.Visible = bModified;
		}

		public Action RemoveEditData;

		public void GetAngleValue( out double angleA_deg, out double angleB_deg )
		{
			angleA_deg = m_AngleA_deg;
			angleB_deg = m_AngleB_deg;
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			// get value from textbox
			if( !double.TryParse( m_tbxAngleA.Text, out m_AngleA_deg )
					|| !double.TryParse( m_tbxAngleB.Text, out m_AngleB_deg ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return;
			}
			if( m_AngleA_deg <= -180 || m_AngleA_deg >= 180 ||
			   m_AngleB_deg <= -180 || m_AngleB_deg >= 180 ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在 -180~180 範圍內", MyApp.NoticeType.Warning );
				return;
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			RemoveEditData?.Invoke();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		double m_AngleA_deg;
		double m_AngleB_deg;
	}
}
