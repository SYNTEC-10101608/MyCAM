using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	public partial class EntryAndExitDlg : EditDialogBase<EntryAndExitData>
	{
		public EntryAndExitDlg( EntryAndExitData data )
		{
			InitializeComponent();
			InitializeControlAndFieldValue( data );
		}

		void InitializeControlAndFieldValue( EntryAndExitData data )
		{
			if( data == null ) {
				m_txbEntryDistance.SilentSetValue = (decimal)m_EntryDistance;
				m_txbExitDistance.SilentSetValue = (decimal)m_ExitDistance;
				return;
			}

			// set control value
			m_txbEntryDistance.SilentSetValue = (decimal)data.EntryDistance;
			m_txbExitDistance.SilentSetValue = (decimal)data.ExitDistance;
			m_txbFollowSafeDistance.SilentSetValue = (decimal)data.FollowSafeDistance;

			// set field value
			m_EntryDistance = data.EntryDistance;
			m_ExitDistance = data.ExitDistance;
			m_FollowSafeDistance = data.FollowSafeDistance;
		}

		void m_txbEntryDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_EntryDistance = (double)m_txbEntryDistance.Value;
			RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
		}

		void m_txbExitDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_ExitDistance = (double)m_txbExitDistance.Value;
			RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
		}

		void m_txbFollowSafeDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FollowSafeDistance = (double)m_txbFollowSafeDistance.Value;
			RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
		}

		double m_EntryDistance = EntryAndExitData.ENTRY_DISTANCE;
		double m_ExitDistance = EntryAndExitData.EXIT_DISTANCE;
		double m_FollowSafeDistance = EntryAndExitData.FOLLOW_SAFE_DISTANCE;
	}
}
