using CAMEdit;
using DataStructure;
using ExtractPattern;
using ImportModel;
using OCC.TopoDS;
using PartPlacement;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			InitializeComponent();
			IsMdiContainer = true;

			// show the import form in dock fill
			ImportModelForm f = new ImportModelForm();
			f.ImportOK += ImportOK;
			ShowChild( f );
		}

		void ImportOK( TopoDS_Shape modelShape )
		{
			//ExtractPatternForm f = new ExtractPatternForm( modelShape );
			//f.ExtractOK += ExtractOK;
			//ShowChild( f );

			PartPlacementForm f = new PartPlacementForm( modelShape );
			ShowChild( f );
		}

		void ExtractOK( TopoDS_Shape modelShape, List<CADData> cadDataList )
		{
			CAMEditForm f = new CAMEditForm();
			CAMEditModel camEditModel = new CAMEditModel( modelShape, cadDataList );
			f.Init( camEditModel );
			ShowChild( f );
		}

		void ShowChild( Form formToShow )
		{
			foreach( Form f in MdiChildren ) {
				f.Hide();
			}
			formToShow.MdiParent = this;
			formToShow.StartPosition = FormStartPosition.Manual;
			formToShow.Location = new Point( 0, 0 );
			ClientSize = new Size( formToShow.Width + SystemInformation.BorderSize.Width * 2,
			formToShow.Height + SystemInformation.CaptionHeight + SystemInformation.BorderSize.Height * 2 );
			formToShow.Show();
		}
	}
}
