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
				m_txbEntryDistance.Text = m_EntryDistance.ToString( "F3" );
				m_txbExitDistance.Text = m_ExitDistance.ToString( "F3" );
				return;
			}

			// set control value
			m_txbEntryDistance.Text = data.EntryDistance.ToString( "F3" );
			m_txbExitDistance.Text = data.ExitDistance.ToString( "F3" );
			m_txbFollowSafeDistance.Text = data.FollowSafeDistance.ToString( "F3" );

			// set field value
			m_EntryDistance = data.EntryDistance;
			m_ExitDistance = data.ExitDistance;
			m_FollowSafeDistance = data.FollowSafeDistance;
		}

		void m_txbEntryDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetEntryDistance();
		}

		void m_txbEntryDistance_Leave( object sender, EventArgs e )
		{
			SetEntryDistance();
		}

		void SetEntryDistance()
		{
			if( double.TryParse( m_txbEntryDistance.Text, out double entryDistance ) && entryDistance >= 0 && entryDistance < double.MaxValue ) {
				m_EntryDistance = entryDistance;
				RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbEntryDistance.Text = m_EntryDistance.ToString( "F3" );
			}
		}

		void m_txbExitDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetExitDistance();
		}

		void m_txbExitDistance_Leave( object sender, EventArgs e )
		{
			SetExitDistance();
		}

		void SetExitDistance()
		{
			if( double.TryParse( m_txbExitDistance.Text, out double exitDistance ) && exitDistance >= 0 && exitDistance < double.MaxValue ) {
				m_ExitDistance = exitDistance;
				RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbExitDistance.Text = m_ExitDistance.ToString( "F3" );
			}
		}

		void m_txbFollowSafeDistance_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode != Keys.Enter ) {
				return;
			}
			SetFollowSafeDistance();
		}

		void m_txbFollowSafeDistance_Leave( object sender, EventArgs e )
		{
			SetFollowSafeDistance();
		}

		void SetFollowSafeDistance()
		{
			if( double.TryParse( m_txbFollowSafeDistance.Text, out double followSafeDistance ) && followSafeDistance >= 0 && followSafeDistance < double.MaxValue ) {
				m_FollowSafeDistance = followSafeDistance;
				RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance, m_FollowSafeDistance ) );
			}
			else {
				m_txbFollowSafeDistance.Text = m_FollowSafeDistance.ToString( "F3" );
			}
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
