using MyCAM.App;
using System;
using System.Windows.Forms;

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
		public Action RaiseKeep;
		public Action RaiseZDir;

		public ToolVectorDlg( ToolVecParam toolVecParam, bool isReverse )
		{
			// struct would not be null
			if( toolVecParam.Equals( default( ToolVecParam ) ) ) {
				toolVecParam = new ToolVecParam( false, 0, 0 );
			}
			InitializeComponent();
			m_ToolVecParam = toolVecParam;
			m_IsPathRevese = isReverse;

			// initialize textbox
			m_tbxAngleA.Text = m_IsPathRevese ? ( -m_ToolVecParam.AngleA_deg ).ToString() : m_ToolVecParam.AngleA_deg.ToString();
			m_tbxAngleB.Text = m_IsPathRevese ? ( -m_ToolVecParam.AngleB_deg ).ToString() : m_ToolVecParam.AngleB_deg.ToString();

			// initialize button
			m_btnRemove.Visible = m_ToolVecParam.IsModified;
		}

		public void SetParamBack( Tuple<double, double> ToolVecParam )
		{
			if( ToolVecParam.Item1 < MIN_Angle || ToolVecParam.Item1 > MAX_Angle ||
				ToolVecParam.Item2 < MIN_Angle || ToolVecParam.Item2 > MAX_Angle ) {
				return;
			}
			m_tbxAngleA.Text = m_IsPathRevese ? ( -ToolVecParam.Item1 ).ToString( "F3" ) : ToolVecParam.Item1.ToString( "F3" );
			m_tbxAngleB.Text = m_IsPathRevese ? ( -ToolVecParam.Item2 ).ToString( "F3" ) : ToolVecParam.Item2.ToString( "F3" );
			PreviewToolVecResult();
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
				m_ToolVecParam.AngleA_deg = m_IsPathRevese ? -double.Parse( m_tbxAngleA.Text ) : double.Parse( m_tbxAngleA.Text );
				m_ToolVecParam.AngleB_deg = m_IsPathRevese ? -double.Parse( m_tbxAngleB.Text ) : double.Parse( m_tbxAngleB.Text );
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
			if( angleA_deg < MIN_Angle || angleA_deg > MAX_Angle ||
			   angleB_deg < MIN_Angle || angleB_deg > MAX_Angle ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在 -60~+60 範圍內", MyApp.NoticeType.Warning );
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
				m_ToolVecParam.AngleA_deg = m_IsPathRevese ? -double.Parse( m_tbxAngleA.Text ) : double.Parse( m_tbxAngleA.Text );
				m_ToolVecParam.AngleB_deg = m_IsPathRevese ? -double.Parse( m_tbxAngleB.Text ) : double.Parse( m_tbxAngleB.Text );
				m_ToolVecParam.IsModified = true;
				RaisePreview( m_ToolVecParam );
			}
		}

		void m_btnKeep_Click( object sender, EventArgs e )
		{
			RaiseKeep();
		}

		void m_btnZDir_Click( object sender, EventArgs e )
		{
			RaiseZDir();
		}

		bool m_IsPathRevese = false;
		const double MAX_Angle = 60.0;
		const double MIN_Angle = -60.0;
	}
}
