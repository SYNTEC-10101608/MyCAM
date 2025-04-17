using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			InitializeComponent();
			myOCCTProxy = new OCCTProxy();
			myOCCTProxy.InitOCCTProxy();
			InitV3D();
		}

		void InitV3D()
		{
			if( !myOCCTProxy.InitViewer( this.Handle ) )
				MessageBox.Show( "Fatal Error during the graphic initialisation", "Error!",
						MessageBoxButtons.OK, MessageBoxIcon.Error );
		}

		void StartupForm_Paint( object sender, PaintEventArgs e )
		{
			myOCCTProxy.RedrawView();
			myOCCTProxy.UpdateView();
		}

		void StartupForm_SizeChanged( object sender, System.EventArgs e )
		{
			myOCCTProxy.UpdateView();
		}

		OCCTProxy myOCCTProxy;
	}
}
