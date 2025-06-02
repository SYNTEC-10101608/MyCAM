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

	public enum AddPointType
	{
		CircArcCenter = 0,
		EdgeMidPoint = 1,
	}

	internal class ViewObject
	{
		public ViewObject( AIS_InteractiveObject shape )
		{
			AISHandle = shape;
		}

		public bool Visible { get; set; } = true;

		public AIS_InteractiveObject AISHandle { get; set; } = null;
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

			// CAD manager
			m_CADManager = new CADManager();
			m_CADManager.AddCADModelDone += OnAddCADModelDone;

			// default action
			m_DefaultAction = new DefaultAction( m_Viewer, m_TreeView, m_CADManager, m_ViewObjectMap, m_TreeNodeMap );
			m_DefaultAction.Start();
		}

		// viewer properties
		Viewer m_Viewer;
		Dictionary<string, ViewObject> m_ViewObjectMap = new Dictionary<string, ViewObject>();

		// tree view properties
		TreeView m_TreeView;
		Dictionary<string, TreeNode> m_TreeNodeMap = new Dictionary<string, TreeNode>();

		// CAD manager
		CADManager m_CADManager;

		// action
		ICADAction m_DefaultAction;

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

		public void AddPoint( AddPointType type )
		{
			m_DefaultAction.End();
			AddPointAction action = new AddPointAction( m_Viewer, m_TreeView, m_CADManager, m_ViewObjectMap, m_TreeNodeMap, type );
			EditActionStart( action );
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

		void EditActionStart( ICADAction action )
		{
			m_DefaultAction.End();
			action.Start();
			action.EndAction += EditActionEnd;
		}

		void EditActionEnd( ICADAction action )
		{
			if( action == null
				|| action.ActionType == CADActionType.None || action.ActionType == CADActionType.Default ) {
				return;
			}
			m_DefaultAction.Start();
		}
	}
}
