using OCC.AIS;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.Quantity;
using OCC.STEPControl;
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

	internal static class ViewHelper
	{
		public static AIS_Shape CreatePartAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY50 ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			return aisShape;
		}

		public static AIS_Shape CreateReferenceAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_WireFrame );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_YELLOW ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			return aisShape;
		}

		public static AIS_Shape CreateFaceAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY50 ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			return aisShape;
		}
	}

	internal class CADEditor
	{
		public CADEditor( Viewer viewer, TreeView treeView )
		{
			if( viewer == null || treeView == null ) {
				throw new ArgumentNullException( "CADEditor consturcting argument null." );
			}

			// CAD manager
			m_CADManager = new CADManager();
			m_CADManager.PartChanged += OnPartChanged;
			m_CADManager.FeatureAdded += OnFeatureAdded;

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_CADManager.PartNode = new TreeNode( "Part" );
			m_TreeView.Nodes.Add( m_CADManager.PartNode );

			// this is to keep highlighted selected node when tree view looses focus
			m_TreeView.HideSelection = false;

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
		public Action StartProcess;
		public Action EndProcess;

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

			// read file data and show a progress form
			ReadFileData( format, szFileName );
		}

		public void AddPoint( AddPointType type )
		{
			AddPointAction action = new AddPointAction( m_Viewer, m_TreeView, m_CADManager, type );
			StartEditAction( action );
		}

		public void ThreePointTransform()
		{
			ThreePtTransformAction action = new ThreePtTransformAction( m_Viewer, m_TreeView, m_CADManager );
			StartEditAction( action );
		}

		public void StartManaulTransform()
		{
			ManualTransformAction action = new ManualTransformAction( m_Viewer, m_TreeView, m_CADManager );
			StartEditAction( action );
		}

		public void ApplyManualTransform( EConstraintType type, bool bReverse = false )
		{
			if( m_CurrentAction.ActionType != CADActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).ApplyTransform( type, bReverse );
		}

		public void EndManualTransform()
		{
			if( m_CurrentAction.ActionType != CADActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).TransformDone();
		}

		public void StartSelectFace()
		{
			SelectFaceAction action = new SelectFaceAction( m_Viewer, m_TreeView, m_CADManager );
			StartEditAction( action );
		}

		public void SelectD1ContFace()
		{
			if( m_CurrentAction.ActionType != CADActionType.SelectFace ) {
				return;
			}
			( (SelectFaceAction)m_CurrentAction ).SelectD1ContFace();
		}

		public void EndSelectFace()
		{
			if( m_CurrentAction.ActionType != CADActionType.SelectFace ) {
				return;
			}
			( (SelectFaceAction)m_CurrentAction ).SelectDone();
		}

		// manager events
		void OnPartChanged()
		{
			// clear the tree view and viewer
			m_CADManager.PartNode.Nodes.Clear();
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}
			m_CADManager.ViewObjectMap.Clear();

			// clear view manager data
			m_CADManager.ViewObjectMap.Clear();
			m_CADManager.TreeNodeMap.Clear();
			foreach( var data in m_CADManager.ShapeDataContainer ) {

				// add node to the tree view
				TreeNode node = new TreeNode( data.UID );
				m_CADManager.PartNode.Nodes.Add( node );
				m_CADManager.TreeNodeMap.Add( data.UID, node );

				// add shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePartAIS( data.Shape );
				m_CADManager.ViewObjectMap.Add( data.UID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false );
			}

			// update tree view and viewer
			m_CADManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
		}

		void OnFeatureAdded( string szID )
		{
			if( string.IsNullOrEmpty( szID ) ) {
				return;
			}

			// add a new node to the tree view
			TreeNode node = new TreeNode( szID );
			m_CADManager.PartNode.Nodes.Add( node );
			m_CADManager.TreeNodeMap.Add( szID, node );

			// add a new shape to the viewer
			ShapeData shapeData = m_CADManager.ShapeDataMap[ szID ];
			AIS_Shape aisShape = ViewHelper.CreateReferenceAIS( shapeData.Shape );
			m_CADManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
			m_Viewer.GetAISContext().Display( aisShape, false );

			// update tree view and viewer
			m_CADManager.PartNode.ExpandAll();
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
			m_CADManager.AddPart( oneShape );
		}

		// edit actions
		void StartEditAction( ICADAction action )
		{
			// to prevent from non-necessary default action start
			m_IsNextAction = true;

			// end the current action
			m_CurrentAction.End();
			m_IsNextAction = false;

			// start the action
			m_CurrentAction = action;
			m_CurrentAction.Start();
			m_CurrentAction.EndAction += OnEditActionEnd;
		}

		void OnEditActionEnd( ICADAction action )
		{
			// start default action if all edit actions are done
			if( !m_IsNextAction ) {
				m_CurrentAction = m_DefaultAction;
				m_CurrentAction.Start();
			}
		}

		bool m_IsNextAction = false;
	}
}
