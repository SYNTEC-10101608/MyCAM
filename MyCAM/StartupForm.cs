using System;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			InitializeComponent();
			m_Bridge = new AppBridge();
			InitV3D();

			m_panView.MouseWheel += OnMouseWheel;
			m_panView.MouseDown += OnMouseDown;
			m_panView.MouseMove += OnMouseMove;
			m_panView.PreviewKeyDown += OnKeyDown;
		}

		// viewer
		void InitV3D()
		{
			if( !m_Bridge.InitViewer( m_panView.Handle ) )
				MessageBox.Show( "Fatal Error during the graphic initialisation", "Error!",
						MessageBoxButtons.OK, MessageBoxIcon.Error );
		}

		void m_panView_Paint( object sender, PaintEventArgs e )
		{
			m_Bridge.RedrawView();
			m_Bridge.UpdateView();
		}

		void m_panView_SizeChanged( object sender, System.EventArgs e )
		{
			m_Bridge.UpdateView();
		}

		void OnMouseMove( object sender, MouseEventArgs e )
		{
			m_Bridge.MouseMove( (int)e.Button, e.X, e.Y );
		}

		void OnMouseDown( object sender, MouseEventArgs e )
		{
			m_panView.Focus();
			m_Bridge.MouseDown( (int)e.Button, e.X, e.Y );
		}

		void OnKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			m_Bridge.KeyDown( (int)e.KeyCode );
		}

		void OnMouseWheel( object sender, MouseEventArgs e )
		{
			m_Bridge.MouseWheel( e.Delta, e.X, e.Y );
		}

		// import
		public enum Format
		{
			BREP = 0,
			STEP = 1,
			IGES = 2
		}

		void m_tsmiImportSTEP_Click( object sender, System.EventArgs e )
		{
			DoImport( Format.STEP );
		}

		void m_tsmiImportIGES_Click( object sender, System.EventArgs e )
		{
			DoImport( Format.IGES );
		}

		void m_tsmiImportBRep_Click( object sender, System.EventArgs e )
		{
			DoImport( Format.BREP );
		}

		void DoImport( Format format )
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			string filter = "";
			switch( format ) {
				case Format.BREP:
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case Format.STEP:
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case Format.IGES:
					filter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
					break;
			}
			openDialog.Filter = filter + "|All files (*.*)|*.*";
			if( openDialog.ShowDialog() != DialogResult.OK ) {
				return;
			}
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return;
			}

			// call the bridge  
			bool bImportSucess = m_Bridge.ImportFile( szFileName, (int)format );
			if( !bImportSucess ) {
				MessageBox.Show( "Error: Import" );
				return;
			}
		}

		// fields
		AppBridge m_Bridge;

		// view action
		int m_nXMousePosition;
		int m_nYMousePosition;
		const double ZOOM_Ratio = 0.0002;
	}
}
