using MyCAM.App;
using MyCAM.Data;
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

		public EToolVecInterpolateType InterpolateType
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

		public double Master_deg
		{
			get; set;
		}

		public double Slave_deg
		{
			get; set;
		}

		public ToolVecParam( bool isModified = false, double dAngleA_deg = 0, double dAngelB_deg = 0,
			double dMaster_deg = 0, double dSlave_deg = 0,
			EToolVecInterpolateType interpolateType = EToolVecInterpolateType.Normal )
		{
			IsModified = isModified;
			AngleA_deg = dAngleA_deg;
			AngleB_deg = dAngelB_deg;
			Master_deg = dMaster_deg;
			Slave_deg = dSlave_deg;
			InterpolateType = interpolateType;
		}

		public const double MAX_Angle = 60.0;
		public const double MIN_Angle = -60.0;
	}

	public partial class ToolVectorDlg : EditDialogBase<ToolVecParam>
	{
		public Action RaiseKeep;
		public Action RaiseZDir;
		public Func<double, double, Tuple<double, double>> RaiseCalculateMSAngleFromABAngle;
		public Func<double, double, Tuple<double, double>> RaiseCalculateABAngleFromMSAngle;

		public ToolVectorDlg( ToolVecParam toolVecParam, bool isPathReverse )
		{
			// struct would not be null
			InitializeComponent();
			m_ToolVecParam = toolVecParam;
			m_IsPathRevese = isPathReverse;

			// initialize textbox
			m_tbxAngleA.Text = m_IsPathRevese ? ( -m_ToolVecParam.AngleA_deg ).ToString( "F3" ) : m_ToolVecParam.AngleA_deg.ToString( "F3" );
			m_tbxAngleB.Text = m_IsPathRevese ? ( -m_ToolVecParam.AngleB_deg ).ToString( "F3" ) : m_ToolVecParam.AngleB_deg.ToString( "F3" );
			m_tbxMaster.Text = m_ToolVecParam.Master_deg.ToString( "F3" );
			m_tbxSlave.Text = m_ToolVecParam.Slave_deg.ToString( "F3" );

			// initialize button
			m_btnRemove.Visible = m_ToolVecParam.IsModified;

			// initialize modify type
			switch( toolVecParam.InterpolateType ) {
				case EToolVecInterpolateType.VectorInterpolation:
					m_rbtVecSpace.Checked = true;
					break;
				case EToolVecInterpolateType.TiltAngleInterpolation:
					m_rbtTiltAngleSpace.Checked = true;
					break;
				default:
					m_rbtInit.Checked = true;
					break;
			}
		}

		public void SetAngleFromTargetVec( Tuple<double, double> abAngles_deg, Tuple<double, double> msAngles_deg )
		{
			SetABAngleBack( abAngles_deg );
			SetMSAngleBack( msAngles_deg );
			if( !CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 ) ) {
				return;
			}
			PreviewToolVecResult();
		}

		protected override void OnShown( EventArgs e )
		{
			base.OnShown( e );
			PreviewToolVecResult();
		}

		ToolVecParam m_ToolVecParam;

		void PreviewToolVecResult()
		{
			if( !SaveToolVecParam() ) {
				return;
			}
			SaveInterpolateType();
			RaisePreview( m_ToolVecParam );
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			if( !SaveToolVecParam() ) {
				return;
			}
			SaveInterpolateType();
			RaiseConfirm( m_ToolVecParam );
			Close();
		}

		void SaveInterpolateType()
		{
			if( m_rbtTiltAngleSpace.Checked ) {
				m_ToolVecParam.InterpolateType = EToolVecInterpolateType.TiltAngleInterpolation;
			}
			else if( m_rbtVecSpace.Checked ) {
				m_ToolVecParam.InterpolateType = EToolVecInterpolateType.VectorInterpolation;
			}
			else {
				m_ToolVecParam.InterpolateType = EToolVecInterpolateType.Normal;
			}
		}

		bool SaveToolVecParam()
		{
			m_ToolVecParam.IsModified = true;
			if( !GetABAngleFromDialog( out double angleA_deg, out double angleB_deg ) ) {
				return false;
			}
			if( !CheckABAngleRange( angleA_deg, angleB_deg ) ) {
				return false;
			}
			if( !GetMSAngleFromDialog( out double master_deg, out double slave_deg ) ) {
				return false;
			}
			m_ToolVecParam.AngleA_deg = angleA_deg;
			m_ToolVecParam.AngleB_deg = angleB_deg;
			m_ToolVecParam.Master_deg = master_deg;
			m_ToolVecParam.Slave_deg = slave_deg;
			return true;
		}

		void m_btnRemove_Click( object sender, EventArgs e )
		{
			// set flag to this tool vec is need to erase
			m_ToolVecParam.IsModified = false;
			RaiseConfirm( m_ToolVecParam );
			Close();
		}

		void m_tbxAngleA_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleABAngleChanged();
			}
		}

		void m_tbxAngleB_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleABAngleChanged();
			}
		}

		void m_tbxMaster_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleMSAngleChanged();
			}
		}

		void m_tbxSlave_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				HandleMSAngleChanged();
			}
		}

		void m_tbxAngleA_Leave( object sender, EventArgs e )
		{
			HandleABAngleChanged();
		}

		void m_tbxAngleB_Leave( object sender, EventArgs e )
		{
			HandleABAngleChanged();
		}

		void m_tbxMaster_Leave( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void m_tbxSlave_Leave( object sender, EventArgs e )
		{
			HandleMSAngleChanged();
		}

		void m_btnKeep_Click( object sender, EventArgs e )
		{
			RaiseKeep();
		}

		void m_btnZDir_Click( object sender, EventArgs e )
		{
			RaiseZDir();
		}

		void m_rbtVecSpaceCase_CheckedChanged( object sender, EventArgs e )
		{
			PreviewToolVecResult();
		}

		void m_rbtTiltAngleCase_CheckedChanged( object sender, EventArgs e )
		{
			PreviewToolVecResult();
		}

		bool CheckABAngleRange( double angleA_deg, double angleB_deg )
		{
			if( angleA_deg < ToolVecParam.MIN_Angle || angleA_deg > ToolVecParam.MAX_Angle ||
				angleB_deg < ToolVecParam.MIN_Angle || angleB_deg > ToolVecParam.MAX_Angle ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在 -60~+60 範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		void HandleABAngleChanged()
		{
			// Get AB angles from dialog
			if( !GetABAngleFromDialog( out double angleA_deg, out double angleB_deg ) ) {
				return;
			}

			// Check if AB angles are in valid range
			if( !CheckABAngleRange( angleA_deg, angleB_deg ) ) {
				return;
			}

			// Calculate MS angles from AB angles
			if( RaiseCalculateMSAngleFromABAngle != null ) {
				Tuple<double, double> msAngles_deg = RaiseCalculateMSAngleFromABAngle( angleA_deg, angleB_deg );
				SetMSAngleBack( msAngles_deg );
			}

			// Preview the result
			PreviewToolVecResult();
		}

		void HandleMSAngleChanged()
		{
			// Get MS angles from dialog
			if( !GetMSAngleFromDialog( out double master_deg, out double slave_deg ) ) {
				return;
			}

			// Calculate AB angles from MS angles
			if( RaiseCalculateABAngleFromMSAngle != null ) {
				Tuple<double, double> abAngles_deg = RaiseCalculateABAngleFromMSAngle( master_deg, slave_deg );

				// Fill back AB angles to textboxes
				SetABAngleBack( abAngles_deg );

				// Check if calculated AB angles are in valid range
				if( !CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 ) ) {
					return;
				}
			}

			// Preview the result
			PreviewToolVecResult();
		}

		bool GetABAngleFromDialog( out double angleA_deg, out double angleB_deg )
		{
			// Parse AB angles from textboxes
			angleA_deg = 0;
			angleB_deg = 0;
			if( !double.TryParse( m_tbxAngleA.Text, out angleA_deg ) ||
				!double.TryParse( m_tbxAngleB.Text, out angleB_deg ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return false;
			}

			// Convert to actual angles (considering path reverse)
			angleA_deg = m_IsPathRevese ? -angleA_deg : angleA_deg;
			angleB_deg = m_IsPathRevese ? -angleB_deg : angleB_deg;
			return true;
		}

		bool GetMSAngleFromDialog( out double master_deg, out double slave_deg )
		{
			// Parse MS angles from textboxes
			master_deg = 0;
			slave_deg = 0;
			if( !double.TryParse( m_tbxMaster.Text, out master_deg ) ||
				!double.TryParse( m_tbxSlave.Text, out slave_deg ) ) {
				MyApp.Logger.ShowOnLogPanel( "無效字串", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		// trigger by event loop
		void SetMSAngleBack( Tuple<double, double> MSAngleParam_deg )
		{
			m_tbxMaster.Text = MSAngleParam_deg.Item1.ToString( "F3" );
			m_tbxSlave.Text = MSAngleParam_deg.Item2.ToString( "F3" );
		}

		void SetABAngleBack( Tuple<double, double> ABAngleParam_deg )
		{
			m_tbxAngleA.Text = m_IsPathRevese ? ( -ABAngleParam_deg.Item1 ).ToString( "F3" ) : ABAngleParam_deg.Item1.ToString( "F3" );
			m_tbxAngleB.Text = m_IsPathRevese ? ( -ABAngleParam_deg.Item2 ).ToString( "F3" ) : ABAngleParam_deg.Item2.ToString( "F3" );
		}

		bool m_IsPathRevese = false;
	}
}
