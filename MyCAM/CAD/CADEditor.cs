using CAMEdit;
using DataStructure;
using OCC.AIS;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.Quantity;
using OCC.ShapeAnalysis;
using OCC.STEPControl;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
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

	public enum EFeatureType
	{
		Reference = 0,
		ComponentFace = 1,
		Path = 2,
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

		public static AIS_Shape CreateFeatureAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_YELLOW ) );
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
			m_TreeView.Nodes.Add( m_CADManager.PartNode );
			m_TreeView.Nodes.Add( m_CADManager.ComponentFaceNode );
			m_TreeView.Nodes.Add( m_CADManager.PathNode );

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

		public void SelectPath_FreeBound()
		{
			// get selected face group from select face action
			if( m_CurrentAction.ActionType != CADActionType.SelectFace ) {
				return;
			}
			List<TopoDS_Shape> selectedFaceGroupList = ( (SelectFaceAction)m_CurrentAction ).GetResult();

			// stay in select face action if no face is selected
			if( selectedFaceGroupList.Count == 0 ) {
				return;
			}
			m_CurrentAction.End();

			// get path from free boundaries
			List<TopoDS_Wire> pathWireList = new List<TopoDS_Wire>();
			TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			foreach( TopoDS_Shape oneFace in selectedFaceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( oneFace );

				// add to map
				TopExp.MapShapesAndAncestors( oneFace, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref edgeFaceMap );

				// get all closed wires
				TopExp_Explorer wireExp = new TopExp_Explorer( freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					pathWireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}
			m_CADManager.AddPath( pathWireList, edgeFaceMap );
		}

		public void StartSelectPath_Manual()
		{
			// get selected face group from select face action
			if( m_CurrentAction.ActionType != CADActionType.SelectFace ) {
				return;
			}
			List<TopoDS_Shape> selectedFaceGroupList = ( (SelectFaceAction)m_CurrentAction ).GetResult();

			// stay in select face action if no face is selected
			if( selectedFaceGroupList.Count == 0 ) {
				return;
			}
			SelectPathAction action = new SelectPathAction( m_Viewer, m_TreeView, m_CADManager, selectedFaceGroupList );
			StartEditAction( action );
		}

		public void EndSelectPath_Manual()
		{
			if( m_CurrentAction.ActionType != CADActionType.SelectPath ) {
				return;
			}
			( (SelectPathAction)m_CurrentAction ).SelectDone();
		}

		public void GoToCAM()
		{
			// build CAD data
			List<CADData> cadDataList = new List<CADData>();
			foreach( string szID in m_CADManager.PathIDList ) {
				PathData pathData = (PathData)m_CADManager.ShapeDataMap[ szID ];
				CADData cadData = new CADData( TopoDS.ToWire( pathData.Shape ), pathData.Edge5DList, pathData.Transform );
				cadDataList.Add( cadData );
			}

			// show CAMEditForm
			CAMEditForm camEditForm = new CAMEditForm();
			camEditForm.Size = new System.Drawing.Size( 1200, 800 );
			CAMEditModel camEditModel = new CAMEditModel( m_CADManager.PartShape, cadDataList );
			camEditForm.Init( camEditModel );
			camEditForm.ShowDialog();
			if( camEditForm.DialogResult != DialogResult.OK ) {
				return;
			}
		}

		// manager events
		void OnPartChanged()
		{
			// clear the tree view and viewer
			m_CADManager.PartNode.Nodes.Clear();
			m_CADManager.ComponentFaceNode.Nodes.Clear();
			m_CADManager.PathNode.Nodes.Clear();
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

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

		void OnFeatureAdded( List<string> newFeatureIDs, EFeatureType type )
		{
			foreach( string szID in newFeatureIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}

				// add a new node to the tree view
				TreeNode node = new TreeNode( szID );
				if( type == EFeatureType.Reference ) {
					m_CADManager.PartNode.Nodes.Add( node );
				}
				else if( type == EFeatureType.ComponentFace ) {
					m_CADManager.ComponentFaceNode.Nodes.Add( node );
				}
				else if( type == EFeatureType.Path ) {
					m_CADManager.PathNode.Nodes.Add( node );
				}
				else {
					continue; // unknown type
				}
				m_CADManager.TreeNodeMap.Add( szID, node );

				// add a new shape to the viewer
				ShapeData shapeData = m_CADManager.ShapeDataMap[ szID ];
				AIS_Shape aisShape = ViewHelper.CreateFeatureAIS( shapeData.Shape );
				if( type == EFeatureType.Path ) {
					aisShape.SetWidth( 2.0 ); // TEST
				}
				m_CADManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false );
			}

			// update tree view and viewer
			m_CADManager.PartNode.ExpandAll();
			m_CADManager.ComponentFaceNode.ExpandAll();
			m_CADManager.PathNode.ExpandAll();
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
