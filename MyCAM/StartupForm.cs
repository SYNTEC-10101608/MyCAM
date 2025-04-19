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
		}

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

		AppBridge myBridge;

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

		public enum Format
		{
			BREP = 0,
			STEP = 1,
			IGES = 2
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
	}
}
