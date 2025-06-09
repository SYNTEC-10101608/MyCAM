using OCC.AIS;
using OCC.Geom;
using OCC.gp;
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

	public enum EConstraintType
	{
		Axial,
		AxialParallel,
		Plane,
		PlaneParallel,
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
			m_DefaultAction = new DefaultAction( m_Viewer, m_TreeView, m_CADManager );
			m_CurrentAction = m_DefaultAction;
			m_DefaultAction.Start();
		}

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;

		// CAD manager
		CADManager m_CADManager;

		// action
		ICADAction m_DefaultAction;
		ICADAction m_CurrentAction;

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
			throw new NotImplementedException( "Add point action is not implemented yet." );
		}

		public void Create3PCoordSystem()
		{
			throw new NotImplementedException( "Create 3P coordinate system is not implemented yet." );
		}

		public void StartTransform()
		{
			TransformAction action = new TransformAction( m_Viewer, m_TreeView, m_CADManager );
			EditActionStart( action );
		}

		public void ApplyTransform( EConstraintType type, bool bReverse = false )
		{
			if( m_CurrentAction.ActionType != CADActionType.Transform ) {
				return;
			}
			( (TransformAction)m_CurrentAction ).ApplyTransform( type, bReverse );
		}

		public void EndTransform()
		{
			if( m_CurrentAction.ActionType != CADActionType.Transform ) {
				return;
			}
			( (TransformAction)m_CurrentAction ).TransformDone();
		}

		// manager events
		void OnAddCADModelDone( ShapeData model )
		{
			string szUID = model.UID;
			TopoDS_Shape shape = model.Shape;
			if( string.IsNullOrEmpty( szUID ) || shape == null || shape.IsNull() ) {
				return;
			}

			// update the tree view
			TreeNode newNode = new TreeNode( szUID );
			m_CADManager.TreeNodeMap[ szUID ] = newNode;
			m_TreeView.Nodes.Add( newNode );

			// update the viewer
			AIS_Shape aisShape = ViewHelper.CreatePartAIS( shape );
			m_CADManager.ViewObjectMap[ szUID ] = new ViewObject( aisShape );
			m_Viewer.GetAISContext().Display( aisShape, true );
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

		AIS_Trihedron CreateTrihedron( gp_Ax3 ax3, bool global = false )
		{
			AIS_Trihedron trihedron = new AIS_Trihedron( new Geom_Axis2Placement( ax3.Ax2() ) );
			if( global ) {
				trihedron.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
				trihedron.SetAxisColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
				trihedron.SetSize( 100.0 );
			}
			else {
				trihedron.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
				trihedron.SetAxisColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
				trihedron.SetSize( 10.0 );
			}
			trihedron.SetTextColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			return trihedron;
		}

		// edit actions
		void EditActionStart( ICADAction action )
		{
			m_DefaultAction.End();
			m_CurrentAction = action;
			m_CurrentAction.Start();
			m_CurrentAction.EndAction += EditActionEnd;
		}

		void EditActionEnd( ICADAction action )
		{
			if( action == null
				|| action.ActionType == CADActionType.None || action.ActionType == CADActionType.Default ) {
				return;
			}
			m_DefaultAction.Start();
			m_CurrentAction = m_DefaultAction;
		}
	}
}
