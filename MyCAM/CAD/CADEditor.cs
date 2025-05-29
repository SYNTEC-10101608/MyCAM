using OCC.AIS;
using OCC.Graphic3d;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.Quantity;
using OCC.STEPControl;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	public enum FileFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	internal class CADEditor
	{
		public CADEditor( Viewer viewer, TreeView treeView )
		{
			if( viewer == null || treeView == null ) {
				throw new ArgumentNullException( "Viewer and TreeView cannot be null." );
			}
			m_Viewer = viewer;
			m_TreeView = treeView;

			// viewer events
			viewer.MouseDown += ViewerMouseDown;
			viewer.KeyDown += ViewerKeyDown;

			// tree view events
			treeView.AfterSelect += TreeViewAfterSelect;
			treeView.KeyDown += TreeViewKeyDown;

			// CAD manager
			m_CADManager = new CADManager();
			m_CADManager.AddCADModelDone += OnAddCADModelDone;
		}

		// viewer properties
		Viewer m_Viewer;
		class ViewObject
		{
			public ViewObject( AIS_InteractiveObject shape )
			{
				AISHandle = shape;
			}

			public bool Visible { get; set; } = true;

			public AIS_InteractiveObject AISHandle { get; set; } = null;
		}
		Dictionary<string, ViewObject> m_ViewObjectMap = new Dictionary<string, ViewObject>();

		// tree view properties
		TreeView m_TreeView;
		Dictionary<string, TreeNode> m_TreeNodeMap = new Dictionary<string, TreeNode>();
		bool m_bSuppressTreeViewSync = false;

		// CAD manager
		CADManager m_CADManager;

		// viewer events

		void ViewerMouseDown( MouseEventArgs e )
		{
			// select
			if( e.Button == MouseButtons.Left ) {
				m_Viewer.GetAISContext().SelectDetected();
				m_Viewer.UpdateView();
				SyncSelectionFromViewToTree();
			}
		}

		void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
		}

		// tree view events
		void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Space ) {
				ChangeObjectVisibility();
			}
		}

		void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			foreach( TreeNode node in m_TreeView.Nodes ) {
				node.BackColor = System.Drawing.Color.White;
			}
			if( e.Node == null ) {
				return;
			}
			e.Node.BackColor = System.Drawing.Color.Cyan;
			SyncSelectionFromTreeToView();
		}

		// APIs
		public void ImportFile( FileFormat format )
		{
			OpenFileDialog openDialog = new OpenFileDialog();

			// file dialog filter
			string filter = "";
			switch( format ) {
				case FileFormat.BREP:
					filter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
					break;
				case FileFormat.STEP:
					filter = "STEP Files (*.stp *.step)|*.stp; *.step";
					break;
				case FileFormat.IGES:
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

		public void ChangeObjectVisibility()
		{
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return;
			}
			string szUID = selectedNode.Text;
			ChangeObjectVisibility( szUID );
		}

		public void RemoveObject()
		{
			TreeNode selectedNode = m_TreeView.SelectedNode;
			if( selectedNode == null || string.IsNullOrEmpty( selectedNode.Text ) ) {
				return;
			}
			string szUID = selectedNode.Text;
			RemoveObject( szUID );
		}

		public void ExtractFace()
		{
		}

		// manager events
		void OnAddCADModelDone( CADModel model )
		{
			string szUID = model.UID;
			TopoDS_Shape shape = model.ShapeData;
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() ) {
				return;
			}

			// update the tree view
			TreeNode newNode = new TreeNode( szUID );
			m_TreeNodeMap[ szUID ] = newNode;
			m_TreeView.Nodes.Add( newNode );

			// update the viewer
			AIS_Shape aisShape = CreateAIS( shape );
			m_ViewObjectMap[ szUID ] = new ViewObject( aisShape );
			m_Viewer.GetAISContext().Display( aisShape, true );
			m_Viewer.UpdateView();
		}

		// private methods
		void ReadFileData( FileFormat format, string szFileName )
		{
			// read the file
			XSControl_Reader Reader;
			switch( format ) {
				case FileFormat.BREP:
					Reader = new XSControl_Reader();
					break;
				case FileFormat.STEP:
					Reader = new STEPControl_Reader();
					break;
				case FileFormat.IGES:
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
			oneShape = ShapeTool.SewShape( new List<TopoDS_Shape>() { oneShape }/*, 1e-1*/ );

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

		void ChangeObjectVisibility( string szUID )
		{
			// toggle the visibility of the selected object

			if( !m_ViewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_ViewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}

			// toggle visibility
			viewObject.Visible = !viewObject.Visible;
			if( viewObject.Visible ) {
				m_Viewer.GetAISContext().Display( viewObject.AISHandle, true );
			}
			else {
				m_Viewer.GetAISContext().Erase( viewObject.AISHandle, true );
			}
		}

		void RemoveObject( string szUID )
		{
			throw new NotImplementedException( "RemoveObject method is not implemented yet." );
		}

		AIS_Shape CreateAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			return aisShape;
		}

		void SyncSelectionFromTreeToView()
		{
			if( m_bSuppressTreeViewSync ) {
				return;
			}

			// clear viewer slection
			m_Viewer.GetAISContext().ClearSelected( false );

			// get the selected node
			TreeNode node = m_TreeView.SelectedNode;
			if( node == null || string.IsNullOrEmpty( node.Text ) ) {
				return;
			}
			string szUID = node.Text;

			// find the corresponding view object
			if( !m_ViewObjectMap.ContainsKey( szUID ) ) {
				return;
			}
			ViewObject viewObject = m_ViewObjectMap[ szUID ];
			if( viewObject == null || viewObject.AISHandle == null ) {
				return;
			}

			// select the shape in the viewer
			m_Viewer.GetAISContext().SetSelected( viewObject.AISHandle, true );
		}

		void SyncSelectionFromViewToTree()
		{
			// clear tree view selection
			m_TreeView.SelectedNode = null;
			foreach( TreeNode node in m_TreeView.Nodes ) {
				node.BackColor = System.Drawing.Color.White;
			}

			// get the selected shape
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return;
			}
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape == null || selectedShape.IsNull() ) {
				return;
			}

			// find the corresponding UID
			string szUID = m_CADManager.GetUIDByShape( selectedShape );
			if( string.IsNullOrEmpty( szUID ) ) {
				return;
			}

			// find the node in the tree view
			if( !m_TreeNodeMap.ContainsKey( szUID ) ) {
				return;
			}
			m_bSuppressTreeViewSync = true;
			m_TreeView.SelectedNode = m_TreeNodeMap[ szUID ];
			m_bSuppressTreeViewSync = false;
		}
	}
}
