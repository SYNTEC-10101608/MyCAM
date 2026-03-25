using MyCAM.App;
using MyCAM.Data;
using OCC.gp;
using System;
using System.Windows.Forms;

namespace MyCAM.Editor.Dialog
{
	public partial class CalibrationREFDlg : EditDialogBase<CalibrationData>
	{
		public Action<int> DeletePoint;
		public Action DeleteAllPoints;
		public Action<KeyEventArgs> DeleteKeyDown;

		public CalibrationREFDlg( CalibrationData data )
		{
			InitializeComponent();
			m_CalibrationData = data;
			SetUI();
			Init();
		}

		public void Init()
		{
			// to catch "delete" event to delete point on viewer
			KeyDown += CalibrationREFDlg_KeyDown;

			// avoid focus on text box when dialog show
			// click "delete" on viewer to set point, after point set, dialog will be activated
			// if focus on text box will not trigger delete point (will delete text in this focused textbox)
			Activated += ( s, e ) => { ActiveControl = null; };
		}

		// let user know which point is being edited
		public void ChangeUIState( int currentEditPnt )
		{
			switch( currentEditPnt ) {
				case (int)PointIndex.Pnt1:
					m_gbPnt1.Enabled = true;
					m_gbPnt2.Enabled = false;
					m_gbPnt3.Enabled = false;
					return;
				case (int)PointIndex.Pnt2:
					m_gbPnt1.Enabled = false;
					m_gbPnt2.Enabled = true;
					m_gbPnt3.Enabled = false;
					return;
				case (int)PointIndex.Pnt3:
					m_gbPnt1.Enabled = false;
					m_gbPnt2.Enabled = false;
					m_gbPnt3.Enabled = true;
					return;
				default:
					m_gbPnt1.Enabled = true;
					m_gbPnt2.Enabled = true;
					m_gbPnt3.Enabled = true;
					break;
			}
		}

		// click on viewer to set point, then update point to dialog
		public void SetPnt1( gp_Pnt point )
		{
			m_tbxPnt1X.Text = point.x.ToString( "F3" );
			m_tbxPnt1Y.Text = point.y.ToString( "F3" );
			m_tbxPnt1Z.Text = point.z.ToString( "F3" );
			SetPnt1();
			RaisePreview( m_CalibrationData );
		}

		public void SetPnt2( gp_Pnt point )
		{
			m_tbxPnt2X.Text = point.x.ToString( "F3" );
			m_tbxPnt2Y.Text = point.y.ToString( "F3" );
			m_tbxPnt2Z.Text = point.z.ToString( "F3" );
			SetPnt2();
			RaisePreview( m_CalibrationData );
		}

		public void SetPnt3( gp_Pnt point )
		{
			m_tbxPnt3X.Text = point.x.ToString( "F3" );
			m_tbxPnt3Y.Text = point.y.ToString( "F3" );
			m_tbxPnt3Z.Text = point.z.ToString( "F3" );
			SetPnt3();
			RaisePreview( m_CalibrationData );
		}

		public void DeletPnt1()
		{
			ClearPnt1Tbx();
			m_CalibrationData.Ref_Pnt1 = null;
			DeletePoint?.Invoke( (int)PointIndex.Pnt1 );
			RaisePreview( m_CalibrationData );
		}

		public void DeletPnt2()
		{
			ClearPnt2Tbx();
			m_CalibrationData.Ref_Pnt2 = null;
			DeletePoint?.Invoke( (int)PointIndex.Pnt2 );
			RaisePreview( m_CalibrationData );
		}

		public void DeletPnt3()
		{
			ClearPnt3Tbx();
			m_CalibrationData.Ref_Pnt3 = null;
			DeletePoint?.Invoke( (int)PointIndex.Pnt3 );
			RaisePreview( m_CalibrationData );
		}

		// avoid focus on any text box when dialog activated (reshow)
		protected override void OnActivated( EventArgs e )
		{
			base.OnActivated( e );
			this.ActiveControl = null;
		}
		CalibrationData m_CalibrationData;

		void SetUI()
		{
			if( m_CalibrationData == null || m_CalibrationData.IsBeenSet == false ) {
				m_tbxPnt1X.Text = "";
				m_tbxPnt1Y.Text = "";
				m_tbxPnt1Z.Text = "";
				m_tbxPnt2X.Text = "";
				m_tbxPnt2Y.Text = "";
				m_tbxPnt2Z.Text = "";
				m_tbxPnt3X.Text = "";
				m_tbxPnt3Y.Text = "";
				m_tbxPnt3Z.Text = "";
				return;
			}
			m_tbxPnt1X.Text = m_CalibrationData.Ref_Pnt1.x.ToString( "F3" );
			m_tbxPnt1Y.Text = m_CalibrationData.Ref_Pnt1.y.ToString( "F3" );
			m_tbxPnt1Z.Text = m_CalibrationData.Ref_Pnt1.z.ToString( "F3" );
			m_tbxPnt2X.Text = m_CalibrationData.Ref_Pnt2.x.ToString( "F3" );
			m_tbxPnt2Y.Text = m_CalibrationData.Ref_Pnt2.y.ToString( "F3" );
			m_tbxPnt2Z.Text = m_CalibrationData.Ref_Pnt2.z.ToString( "F3" );
			m_tbxPnt3X.Text = m_CalibrationData.Ref_Pnt3.x.ToString( "F3" );
			m_tbxPnt3Y.Text = m_CalibrationData.Ref_Pnt3.y.ToString( "F3" );
			m_tbxPnt3Z.Text = m_CalibrationData.Ref_Pnt3.z.ToString( "F3" );
		}

		#region UI Event

		void m_btnDelPnt1_Click( object sender, System.EventArgs e )
		{
			DeletPnt1();
		}

		void m_btnDelPnt2_Click( object sender, EventArgs e )
		{
			DeletPnt2();
		}

		void m_btnDelPnt3_Click( object sender, EventArgs e )
		{
			DeletPnt3();
		}

		void m_btnDeletAll_Click( object sender, EventArgs e )
		{
			ClearPnt1Tbx();
			ClearPnt2Tbx();
			ClearPnt3Tbx();
			SetPnt1();
			SetPnt2();
			SetPnt3();
			DeleteAllPoints?.Invoke();
		}

		#endregion

		enum PointIndex
		{
			Pnt1 = 1,
			Pnt2 = 2,
			Pnt3 = 3
		}

		void ClearPnt1Tbx()
		{
			m_tbxPnt1X.Text = "";
			m_tbxPnt1Y.Text = "";
			m_tbxPnt1Z.Text = "";
			m_CalibrationData.Ref_Pnt1 = null;
		}

		void ClearPnt2Tbx()
		{
			m_tbxPnt2X.Text = "";
			m_tbxPnt2Y.Text = "";
			m_tbxPnt2Z.Text = "";
			m_CalibrationData.Ref_Pnt2 = null;
		}

		void ClearPnt3Tbx()
		{
			m_tbxPnt3X.Text = "";
			m_tbxPnt3Y.Text = "";
			m_tbxPnt3Z.Text = "";
			m_CalibrationData.Ref_Pnt3 = null;
		}

		void SetPnt1()
		{
			if( string.IsNullOrEmpty( m_tbxPnt1X.Text ) || string.IsNullOrEmpty( m_tbxPnt1Y.Text ) || string.IsNullOrEmpty( m_tbxPnt1Z.Text ) ) {
				m_CalibrationData.Ref_Pnt1 = null;
				return;
			}
			m_CalibrationData.Ref_Pnt1 = new gp_Pnt()
			{
				x = double.TryParse( m_tbxPnt1X.Text, out double xValue ) ? xValue : 0,
				y = double.TryParse( m_tbxPnt1Y.Text, out double yValue ) ? yValue : 0,
				z = double.TryParse( m_tbxPnt1Z.Text, out double zValue ) ? zValue : 0,
			};
		}

		void SetPnt2()
		{
			if( string.IsNullOrEmpty( m_tbxPnt2X.Text ) || string.IsNullOrEmpty( m_tbxPnt2Y.Text ) || string.IsNullOrEmpty( m_tbxPnt2Z.Text ) ) {
				m_CalibrationData.Ref_Pnt2 = null;
				return;
			}
			m_CalibrationData.Ref_Pnt2 = new gp_Pnt()
			{
				x = double.Parse( m_tbxPnt2X.Text ),
				y = double.Parse( m_tbxPnt2Y.Text ),
				z = double.Parse( m_tbxPnt2Z.Text )
			};
		}

		void SetPnt3()
		{
			if( string.IsNullOrEmpty( m_tbxPnt3X.Text ) || string.IsNullOrEmpty( m_tbxPnt3Y.Text ) || string.IsNullOrEmpty( m_tbxPnt3Z.Text ) ) {
				m_CalibrationData.Ref_Pnt3 = null;
				return;
			}
			m_CalibrationData.Ref_Pnt3 = new gp_Pnt()
			{
				x = double.Parse( m_tbxPnt3X.Text ),
				y = double.Parse( m_tbxPnt3Y.Text ),
				z = double.Parse( m_tbxPnt3Z.Text )
			};
		}

		void CalibrationREFDlg_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				this.Close();
				return;
			}

			if( e.KeyCode == Keys.Delete ) {

				// is editing tbx
				bool textBoxFocused = false;
				foreach( Control c in this.Controls ) {
					if( c is TextBox && c.Focused ) {
						textBoxFocused = true;
						break;
					}
				}
				if( !textBoxFocused ) {
					DeleteKeyDown?.Invoke( e );
					e.Handled = true;
				}
			}
		}

		void m_btnSure_Click( object sender, EventArgs e )
		{
			// do not want to have REF point
			if( m_CalibrationData.IsBeenSet == false && m_CalibrationData.Ref_Pnt1 == null && m_CalibrationData.Ref_Pnt2 == null && m_CalibrationData.Ref_Pnt3 == null ) {
				RaiseConfirm( m_CalibrationData );
				return;
			}

			// some point be set but not all
			if( m_CalibrationData.IsBeenSet == false ) {
				MyApp.Logger.ShowOnLogPanel( $"需要三個參考點", MyApp.NoticeType.Warning, true );
				return;
			}
			if( m_CalibrationData.Ref_Pnt1.IsEqual( m_CalibrationData.Ref_Pnt2, 1e-6 )
				|| m_CalibrationData.Ref_Pnt1.IsEqual( m_CalibrationData.Ref_Pnt3, 1e-6 ) ) {
				MyApp.Logger.ShowOnLogPanel( "參考點重覆", MyApp.NoticeType.Warning, true );
				return;
			}
			gp_Vec v12 = new gp_Vec( m_CalibrationData.Ref_Pnt1, m_CalibrationData.Ref_Pnt2 );
			gp_Vec v13 = new gp_Vec( m_CalibrationData.Ref_Pnt1, m_CalibrationData.Ref_Pnt3 );
			if( v12.IsParallel( v13, 1e-3 ) ) {
				MyApp.Logger.ShowOnLogPanel( "3點共線", MyApp.NoticeType.Warning, true );
				return;
			}
			RaiseConfirm( m_CalibrationData );
		}
	}
}
