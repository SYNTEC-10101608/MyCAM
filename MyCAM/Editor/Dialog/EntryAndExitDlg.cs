using System;
using MyCAM.Data;

namespace MyCAM.Editor
{
	public partial class EntryAndExitDlg : EditDialogBase<EntryAndExitData>
	{
		public EntryAndExitDlg( EntryAndExitData data )
		{
			InitializeComponent();
			ApplyLanguage();
			InitializeControlAndFieldValue( data );
		}

		void InitializeControlAndFieldValue( EntryAndExitData data )
		{
			if( data == null ) {
				m_numEntryDistance.SilentSetValue = (decimal)m_EntryDistance;
				m_numExitDistance.SilentSetValue = (decimal)m_ExitDistance;
				return;
			}

			// set control value
			m_numEntryDistance.SilentSetValue = (decimal)data.EntryDistance;
			m_numExitDistance.SilentSetValue = (decimal)data.ExitDistance;
			m_numFollowSafeDistance.SilentSetValue = (decimal)data.FollowSafeDistance;

			// set field value
			m_EntryDistance = data.EntryDistance;
			m_ExitDistance = data.ExitDistance;
			m_FollowSafeDistance = data.FollowSafeDistance;
		}

		void m_numEntryDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_EntryDistance = (double)m_numEntryDistance.Value;
			RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
		}

		void m_numExitDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_ExitDistance = (double)m_numExitDistance.Value;
			RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
		}

		void m_numFollowSafeDistance_DebouncedValueChanged( object sender, EventArgs e )
		{
			m_FollowSafeDistance = (double)m_numFollowSafeDistance.Value;
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
