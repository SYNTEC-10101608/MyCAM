using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			InitializeComponent();
			myBridge = new AppBridge();
			InitV3D();

			MouseWheel += OnMouseWheel;
			MouseDown += OnMouseDown;
			MouseMove += OnMouseMove;
		}

		// viewer
		void InitV3D()
		{
			if( !myBridge.InitViewer( this.Handle ) )
				MessageBox.Show( "Fatal Error during the graphic initialisation", "Error!",
						MessageBoxButtons.OK, MessageBoxIcon.Error );
		}

		void StartupForm_Paint( object sender, PaintEventArgs e )
		{
			myBridge.RedrawView();
			myBridge.UpdateView();
		}

		void StartupForm_SizeChanged( object sender, System.EventArgs e )
		{
			myBridge.UpdateView();
		}

		void OnMouseMove( object sender, MouseEventArgs e )
		{
			myBridge.MoveTo( e.X, e.Y );
			switch( e.Button ) {

				// translate the viewer
				case MouseButtons.Middle:
					myBridge.Pan( e.X - m_nXMousePosition, m_nYMousePosition - e.Y );
					m_nXMousePosition = e.X;
					m_nYMousePosition = e.Y;
					break;

				// rotate the viewer
				case MouseButtons.Right:
					myBridge.Rotation( e.X, e.Y );
					break;
				default:
					break;
			}
		}

		void OnMouseDown( object sender, MouseEventArgs e )
		{
			switch( e.Button ) {

				// press down middle button, then start translate the viewer
				case MouseButtons.Middle:
					m_nXMousePosition = e.X;
					m_nYMousePosition = e.Y;
					break;

				// press down right button, then start rotatae the viewer
				case MouseButtons.Right:
					myBridge.StartRotation( e.X, e.Y );
					break;
				default:
					break;
			}
		}

		void OnMouseWheel( object sender, MouseEventArgs e )
		{
			// zoom viewer at start point
			myBridge.StartZoomAtPoint( e.X, e.Y );

			int nEndX = (int)( e.X + e.X * e.Delta * ZOOM_Ratio );
			int nEndY = (int)( e.Y + e.Y * e.Delta * ZOOM_Ratio );

			// zoom viewer with mouse wheel delta and scaling ratio
			myBridge.ZoomAtPoint( e.X, e.Y, nEndX, nEndY );
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
			bool bImportSucess = myBridge.ImportFile( szFileName, (int)format );
			if( !bImportSucess ) {
				MessageBox.Show( "Error: Import" );
				return;
			}
		}

		// fields
		AppBridge myBridge;

		// view action
		int m_nXMousePosition;
		int m_nYMousePosition;
		const double ZOOM_Ratio = 0.0002;
	}
}
