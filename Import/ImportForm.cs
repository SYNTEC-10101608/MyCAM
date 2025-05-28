using OCC.IFSelect;
using OCC.IGESControl;
using OCC.STEPControl;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.XSControl;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Import
{
	public enum Format
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	public partial class ImportForm : Form
	{
		public Action<TopoDS_Shape> ImportOK;

		public ImportForm()
		{
			InitializeComponent();
			Controls.Add( m_panBackground );
			m_panBackground.Dock = DockStyle.Fill;
			m_panBackground.SizeChanged += BackGroundSizeChanged;

			// create the viewer
			bool bSucess = m_OCCViewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( ToString() + "Init Error: Init Viewer" );
				return;
			}
			m_panBackground.Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Right;
			m_OCCViewer.UpdateView();

			// create the tree view
			m_panBackground.Controls.Add( m_panTreeView );
			m_panTreeView.Dock = DockStyle.Left;
			m_panTreeView.Controls.Add( m_treeView );
			m_treeView.Dock = DockStyle.Fill;
		}

		void BackGroundSizeChanged( object sender, EventArgs e )
		{
			m_panViewer.Width = (int)( m_panBackground.Width * 0.8 );
			m_panTreeView.Width = m_panBackground.Width - m_panViewer.Width;
			m_OCCViewer.UpdateView();
		}

		// background
		Panel m_panBackground = new Panel();

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// tree view
		Panel m_panTreeView = new Panel();
		TreeView m_treeView = new TreeView();

		// manager
		CADManager m_CADManager = new CADManager();

		// import part
		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			ImportFile( Format.BREP );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			ImportFile( Format.STEP );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			ImportFile( Format.IGES );
		}

		void ImportFile( Format format )
		{
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog filter
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
			ReadFileData( format, szFileName );
		}

		void ReadFileData( Format format, string szFileName )
		{
			// read the file
			XSControl_Reader Reader;
			switch( format ) {
				case Format.BREP:
					Reader = new XSControl_Reader();
					break;
				case Format.STEP:
					Reader = new STEPControl_Reader();
					break;
				case Format.IGES:
					Reader = new IGESControl_Reader();
					break;
				default:
					Reader = new XSControl_Reader();
					break;
			}
			IFSelect_ReturnStatus status = Reader.ReadFile( szFileName );

			// check the status
			if( status != IFSelect_ReturnStatus.IFSelect_RetDone ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			Reader.TransferRoots();

			// prevent from empty shape or null shape
			if( Reader.NbShapes() == 0 ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}
			TopoDS_Shape oneShape = Reader.OneShape();
			if( oneShape == null || oneShape.IsNull() ) {
				MessageBox.Show( ToString() + "Error: Import" );
				return;
			}

			// add the read shape to the manager
			AddToManager( oneShape );
		}

		void AddToManager( TopoDS_Shape oneShape )
		{
			List<TopoDS_Shape> shapeList = ArrangeShapeData( oneShape );
			foreach( TopoDS_Shape shape in shapeList ) {
				if( shape == null || shape.IsNull() ) {
					continue;
				}
				m_CADManager.AddCADModel( shape );
			}
		}

		List<TopoDS_Shape> ArrangeShapeData( TopoDS_Shape oneShape )
		{
			if( oneShape == null || oneShape.IsNull() ) {
				return new List<TopoDS_Shape>();
			}
			if( oneShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_COMPOUND ) {
				return new List<TopoDS_Shape>() { oneShape };
			}
			List<TopoDS_Shape> result = new List<TopoDS_Shape>();
			foreach( TopoDS_Shape subShape in oneShape.elementsAsList ) {
				result.AddRange( ArrangeShapeData( subShape ) );
			}
			return result;
		}
	}
}
