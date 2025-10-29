using MyCAM.Data;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
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

			// set field value
			m_EntryDistance = data.EntryDistance;
			m_ExitDistance = data.ExitDistance;
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
				RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance ) );
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
				RaisePreview( new EntryAndExitData( m_EntryDistance, m_ExitDistance ) );
			}
			else {
				m_txbExitDistance.Text = m_ExitDistance.ToString( "F3" );
			}
		}

		void m_btnConfirm_Click( object sender, EventArgs e )
		{
			RaiseConfirm( new EntryAndExitData( m_EntryDistance, m_ExitDistance ) );
		}

		double m_EntryDistance = EntryAndExitData.ENTRY_DISTANCE;
		double m_ExitDistance = EntryAndExitData.EXIT_DISTANCE;
	}
}
