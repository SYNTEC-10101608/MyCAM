using OCC.IFSelect;
using OCC.IGESControl;
using OCC.STEPControl;
using OCC.TopoDS;
using OCC.XSControl;
using System.Windows.Forms;

namespace ImportExport
{
	public enum ModelFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	public class ImportHandler
	{
		public static void ImportModel( ModelFormat format, out TopoDS_Shape theShape )
		{
			theShape = null;

			//int theformat = 10;
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog settings
			//string DataDir = Environment.GetEnvironmentVariable( "CSF_OCCTDataPath" );
			string filter = "";
			switch( format ) {
				case ModelFormat.BREP:
					//openDialog.InitialDirectory = ( DataDir + "\\occ" );
					//theformat = 0;
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case ModelFormat.STEP:
					//openDialog.InitialDirectory = ( DataDir + "\\step" );
					//theformat = 1;
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case ModelFormat.IGES:
					//openDialog.InitialDirectory = ( DataDir + "\\iges" );
					//theformat = 2;
					filter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
					break;
				default:
					break;
			}
			openDialog.Filter = filter + "|All files (*.*)|*.*";

			// show file dialog
			if( openDialog.ShowDialog() != DialogResult.OK ) {
				return;
			}

			// get the file name
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return;
			}

			// transfer model
			XSControl_Reader Reader;
			switch( format ) {
				case ModelFormat.BREP:
					Reader = new XSControl_Reader();
					break;
				case ModelFormat.STEP:
					Reader = new STEPControl_Reader();
					break;
				case ModelFormat.IGES:
					Reader = new IGESControl_Reader();
					break;
				default:
					return;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape
			if( Reader.NbShapes() == 0 ) {
				return;
			}
			theShape = Reader.OneShape();
		}
	}
}
