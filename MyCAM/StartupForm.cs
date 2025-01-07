using CAMEdit;
using DataStructure;
using ExtractPattern;
using Import;
using NCExport;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopoDS;
using PartPlacement;
using ProcessEdit;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		gp_Trsf m_PartTrsf = new gp_Trsf();

		public StartupForm()
		{
			InitializeComponent();
			IsMdiContainer = true;

			// show the import form in dock fill
			ImportForm f = new ImportForm();
			f.ImportOK += ImportOK;
			ShowChild( f );
		}

		void ImportOK( TopoDS_Shape modelShape )
		{
			PartPlacementForm f = new PartPlacementForm( modelShape );
			f.PlaceOK += ( partTrsf ) =>
			{
				m_PartTrsf = partTrsf;
				PlaceOK( modelShape );
			};
			ShowChild( f );

		}

		void PlaceOK( TopoDS_Shape modelShape )
		{
			// transform the model
			gp_Quaternion q = m_PartTrsf.GetRotation();
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( modelShape, trsf, true );
			TopoDS_Shape transformedShape = transform.Shape();

			ExtractPatternForm f = new ExtractPatternForm( transformedShape );
			f.ExtractOK += ExtractOK;
			ShowChild( f );
		}

		void ExtractOK( TopoDS_Shape modelShape, List<CADData> cadDataList )
		{
			CAMEditForm f = new CAMEditForm();
			CAMEditModel camEditModel = new CAMEditModel( modelShape, cadDataList );
			f.CADEditOK += CAMEditOK;
			f.Init( camEditModel );
			ShowChild( f );
		}

		void CAMEditOK( TopoDS_Shape modelShape, List<IProcessData> processDataList )
		{
			ProcessEditForm f = new ProcessEditForm();
			ProcessEditModel processEditModel = new ProcessEditModel( modelShape, processDataList );
			f.EditOK += ProcessEditOK;
			f.Init( processEditModel );
			ShowChild( f );
		}

		void ProcessEditOK( List<IProcessData> list )
		{
			NCWriter w = new NCWriter( list, m_PartTrsf );
			w.Convert();
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
