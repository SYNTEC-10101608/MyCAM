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
		public static bool ImportModel( ModelFormat format, out TopoDS_Shape theShape )
		{
			theShape = null;
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog filter
			string filter = "";
			switch( format ) {
				case ModelFormat.BREP:
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case ModelFormat.STEP:
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case ModelFormat.IGES:
					filter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
					break;
				default:
					break;
			}
			openDialog.Filter = filter + "|All files (*.*)|*.*";

			// show file dialog
			if( openDialog.ShowDialog() != DialogResult.OK ) {
				return false;
			}

			// get the file name
			string szFileName = openDialog.FileName;
			if( string.IsNullOrEmpty( szFileName ) ) {
				return false;
			}

			// read the file
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
					Reader = new XSControl_Reader();
					break;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				return true;
			}
			Reader.TransferRoots();

			// prevent from empty shape
			if( Reader.NbShapes() == 0 ) {
				return true;
			}
			theShape = Reader.OneShape();
			return true;
		}
	}
}
