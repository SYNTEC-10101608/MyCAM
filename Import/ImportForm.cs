using OCC.AIS;
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
			m_panBackGround.SizeChanged += BackGroundSizeChanged;

			// create the viewer
			bool bSucess = m_OCCViewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( ToString() + "Init Error: Init Viewer" );
				return;
			}
			m_panBackGround.Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Right;
			m_OCCViewer.UpdateView();

			m_panViewer.MouseDown += ViewerMoseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// create the tree view
			m_panBackGround.Controls.Add( m_panTreeView );
			m_panTreeView.Dock = DockStyle.Left;
			m_panTreeView.Controls.Add( m_TreeView );
			m_TreeView.Dock = DockStyle.Fill;

			m_TreeView.AfterSelect += TreeViewAfterSelect;
			m_TreeView.KeyDown += TreeViewKeyDown;
		}

		void BackGroundSizeChanged( object sender, EventArgs e )
		{
			m_panViewer.Width = (int)( m_panBackGround.Width * 0.8 );
			m_panTreeView.Width = m_panBackGround.Width - m_panViewer.Width;
			m_OCCViewer.UpdateView();
		}

		// viewer property
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();
		class ViewObject
		{
			public ViewObject( AIS_Shape shape )
			{
				AISHandle = shape;
			}

			public bool Visible { get; set; } = true;

			public AIS_Shape AISHandle { get; private set; } = null;
		}
		Dictionary<string, ViewObject> m_viewObjectMap = new Dictionary<string, ViewObject>();

		// tree view property
		Panel m_panTreeView = new Panel();
		TreeView m_TreeView = new TreeView();

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

		// viewer events
		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.F5 ) {
				m_OCCViewer.AxoView();
				m_OCCViewer.ZoomAllView();
				m_OCCViewer.UpdateView();
			}
		}

		void ViewerMoseDown( object sender, MouseEventArgs e )
		{
			m_panViewer.Focus();
		}

		// tree view events
		void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Delete ) {
				RemoveObject();
			}
			else if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
		}

		void ChangeObjectVisibility()
		{
			// toggle the visibility of the selected object
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return;
			}
			string szUID = m_TreeView.SelectedNode.Text;
			if( !m_viewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_viewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}

			// toggle visibility
			viewObject.Visible = !viewObject.Visible;
			if( viewObject.Visible ) {
				m_OCCViewer.GetAISContext().Display( viewObject.AISHandle, true );
			}
			else {
				m_OCCViewer.GetAISContext().Erase( viewObject.AISHandle, true );
			}
		}

		void RemoveObject()
		{
		}

		void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// synchronize the viewer with the selected node
			if( e.Node == null || string.IsNullOrEmpty( e.Node.Text ) ) {
				return;
			}
			string szUID = e.Node.Text;
			if( !m_viewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_viewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}

			// clear viewer slection
			m_OCCViewer.GetAISContext().ClearSelected( false );

			// select the shape in the viewer
			m_OCCViewer.GetAISContext().SetSelected( viewObject.AISHandle, true );
		}

		// manager events
		void OnAddCADModelDone( string szUID, TopoDS_Shape shape )
		{
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() ) {
				return;
			}

			// update the tree view
			TreeNode newNode = new TreeNode( szUID );
			m_TreeView.Nodes.Add( newNode );
			m_TreeView.SelectedNode = newNode;

			// update the viewer
			AIS_Shape aisShape = new AIS_Shape( shape );
			m_viewObjectMap[ szUID ] = new ViewObject( aisShape );
			m_OCCViewer.GetAISContext().Display( aisShape, true );
			m_OCCViewer.UpdateView();
		}
	}
}
