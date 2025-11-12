using System;
using System.Windows.Forms;
using MyCAM.App;

namespace MyCAM.Editor
{
	public struct ToolVecParam
	{
		public bool IsModified
		{
			get; set;
		}

		public double AngleA_deg
		{
			get; set;
		}

		public double AngleB_deg
		{
			get; set;
		}

		public ToolVecParam( bool isModified, double dAngleA_deg, double dAngelB_deg )
		{
			IsModified = isModified;
			AngleA_deg = dAngleA_deg;
			AngleB_deg = dAngelB_deg;
		}
	}

	public partial class ToolVectorDlg : EditDialogBase<ToolVecParam>
	{
		public ToolVectorDlg( ToolVecParam toolVecParam )
		{
			// struct would not be null
			if( toolVecParam.Equals( default( ToolVecParam ) ) ) {
				toolVecParam = new ToolVecParam( false, 0, 0 );
			}
			InitializeComponent();
			m_ToolVecParam = toolVecParam;

			// initialize textbox
			m_tbxAngleA.Text = m_ToolVecParam.AngleA_deg.ToString();
			m_tbxAngleB.Text = m_ToolVecParam.AngleB_deg.ToString();

			// initialize button
			m_btnRemove.Visible = m_ToolVecParam.IsModified;
		}

		protected override void OnShown( EventArgs e )
		{
			base.OnShown( e );
			SetVertexAsModified();
		}

		ToolVecParam m_ToolVecParam;

		void m_btnOK_Click( object sender, EventArgs e )
		{
			if( CheckParamValid() ) {
				m_ToolVecParam.IsModified = true;
				m_ToolVecParam.AngleA_deg = double.Parse( m_tbxAngleA.Text );
				m_ToolVecParam.AngleB_deg = double.Parse( m_tbxAngleB.Text );
				RaiseConfirm( m_ToolVecParam );
			}
			Close();
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			// set flag to this tool vec is need to erase
			m_ToolVecParam.IsModified = false;
			RaiseConfirm( m_ToolVecParam );
			Close();
		}

		void SetVertexAsModified()
		{
			// this vexter do not modified, set as modified and preview
			if( !m_ToolVecParam.IsModified ) {
				PreviewToolVecResult();
			}
		}

		bool CheckParamValid()
		{
			if( !double.TryParse( m_tbxAngleA.Text, out double angleA_deg )
					|| !double.TryParse( m_tbxAngleB.Text, out double angleB_deg ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return false;
			}
			if( ( ( angleA_deg <= -180 || angleA_deg >= 180 ) && Math.Abs( angleA_deg - 666 ) > 1e-6 ) ||
			   angleB_deg <= -180 || angleB_deg >= 180 ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在 -180~180 範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		void m_tbxAngleA_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewToolVecResult();
			}
		}

		void m_tbxAngleB_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				PreviewToolVecResult();
			}
		}

		void m_tbxAngleA_Leave( object sender, EventArgs e )
		{
			PreviewToolVecResult();
		}

		void m_tbxAngleB_Leave( object sender, EventArgs e )
		{
			PreviewToolVecResult();
		}

		void PreviewToolVecResult()
		{
			if( CheckParamValid() ) {
				m_ToolVecParam.AngleA_deg = double.Parse( m_tbxAngleA.Text );
				m_ToolVecParam.AngleB_deg = double.Parse( m_tbxAngleB.Text );
				m_ToolVecParam.IsModified = true;
				RaisePreview( m_ToolVecParam );
			}
		}
	}
}
