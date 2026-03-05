using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class PathCompensateDlg : EditDialogBase<double>
	{
		public PathCompensateDlg()
		{
			InitializeComponent();
			Initialization();
		}

		public Func<double, bool> IsGeomConstraintExceedsLimit;

		void Initialization()
		{
			// initialize numeric up down controls
			m_NumUpDownCompensate.Maximum = decimal.MaxValue;
			m_NumUpDownCompensate.Minimum = decimal.MinValue;
			m_NumUpDownCompensate.DecimalPlaces = 3;
		}

		void m_NumUpDownCompensate_Click( object sender, System.EventArgs e )
		{
			SetCompensateValue();
		}

		void m_NumUpDownCompensate_KeyDown( object sender, System.Windows.Forms.KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Enter ) {
				SetCompensateValue();
			}
		}

		void m_NumUpDownCompensate_Leave( object sender, System.EventArgs e )
		{
			SetCompensateValue();
		}

		void SetCompensateValue()
		{
			bool isExceedsLimit = IsGeomConstraintExceedsLimit?.Invoke( (double)m_NumUpDownCompensate.Value ) ?? false;
			if( isExceedsLimit ) {
				m_NumUpDownCompensate.Value = (decimal)m_CompensateData;
				return;
			}
			m_CompensateData = (double)m_NumUpDownCompensate.Value;
			RaisePreview( m_CompensateData );
		}

		void m_btnReset_Click( object sender, System.EventArgs e )
		{
			m_CompensateData = 0;
			m_NumUpDownCompensate.Value = 0;
			RaiseReset( m_CompensateData );
		}

		void m_btnClear_Click( object sender, System.EventArgs e )
		{
			m_CompensateData = 0.0;
			m_NumUpDownCompensate.Value = 0;
			RaisePreview( m_CompensateData );
		}

		void m_btnConfirm_Click( object sender, System.EventArgs e )
		{
			bool isExceedsLimit = IsGeomConstraintExceedsLimit?.Invoke( (double)m_NumUpDownCompensate.Value ) ?? false;
			if( isExceedsLimit ) {
				m_NumUpDownCompensate.Value = (decimal)m_CompensateData;
				return;
			}
			m_CompensateData = (double)m_NumUpDownCompensate.Value;
			RaiseConfirm( m_CompensateData );
		}

		double m_CompensateData = 0.0;
	}
}
