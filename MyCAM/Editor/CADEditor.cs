using MyCAM.Data;
using OCC.AIS;
using OCC.IFSelect;
using OCC.IGESControl;
using OCC.STEPControl;
using OCC.TopoDS;
using OCC.XSControl;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal enum FileFormat
	{
		BREP = 0,
		STEP = 1,
		IGES = 2
	}

	internal enum AddPointType
	{
		CircArcCenter = 0,
		EdgeMidPoint = 1,
		TwoVertexMidPoint = 2,
	}

	internal enum AddLineType
	{
		TwoVertexConnectLine = 0,
	}

	internal enum EConstraintType
	{
		Axial,
		AxialParallel,
		Plane,
		PlaneParallel,
		Point,
	}

	internal class CADEditor
	{
		public CADEditor( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
		{
			if( viewer == null || treeView == null || cadManager == null || viewManager == null ) {
				throw new ArgumentNullException( "CADEditor consturcting argument null." );
			}

			// data manager
			m_CADManager = cadManager;
			m_CADManager.PartChanged += OnPartChanged;
			m_CADManager.FeatureAdded += OnFeatureAdded;

			// user interface
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// default action
			m_DefaultAction = new DefaultAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, ESelectObjectType.Part );
		}

		public Action<EActionStatus> AxisTransformActionStausChanged;

		// user interface
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		// data manager
		DataManager m_CADManager;

		// action
		IEditorAction m_DefaultAction;
		IEditorAction m_CurrentAction;

		// editor
		public void EditStart()
		{
			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PartNode );

			// start default action
			m_CurrentAction = m_DefaultAction;
			m_DefaultAction.Start();
		}

		public void EditEnd()
		{
			// clear tree
			m_TreeView.Nodes.Clear();

			// end all action
			if( m_CurrentAction.ActionType == EditActionType.Default ) {
				m_CurrentAction.End();
			}
			else {
				m_CurrentAction.End();
				m_DefaultAction.End();
			}
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

			// read file data and show a progress form
			ReadFileData( format, szFileName );
		}

		public void AddPoint( AddPointType type )
		{
			AddPointAction action = new AddPointAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, type );
			StartEditAction( action );
		}

		public void AddLine( AddLineType type )
		{
			AddLineAction action = new AddLineAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager, type );
			StartEditAction( action );
		}

		public void ThreePointTransform()
		{
			ThreePtTransformAction action = new ThreePtTransformAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			StartEditAction( action );
		}

		public void StartManaulTransform()
		{
			ManualTransformAction action = new ManualTransformAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			StartEditAction( action );
		}

		public void StartAxisTransform()
		{
			// need to use shape data to decide the cneter in the begin, so have to add this preotection
			if( m_CADManager.PartIDList.Count == 0 ) {
				return;
			}
			AxisTransformAction action = new AxisTransformAction( m_Viewer, m_TreeView, m_CADManager, m_ViewManager );
			StartEditAction( action );
		}

		public void ApplyManualTransform( EConstraintType type )
		{
			if( m_CurrentAction.ActionType != EditActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).ApplyTransform( type );
		}

		public void EndManualTransform()
		{
			if( m_CurrentAction.ActionType != EditActionType.ManualTransform ) {
				return;
			}
			( (ManualTransformAction)m_CurrentAction ).TransformDone();
		}

		public void GoToCAM()
		{
			//// build CAD data
			//List<CADData> cadDataList = new List<CADData>();
			//foreach( string szID in m_CADManager.PathIDList ) {
			//	PathData pathData = (PathData)m_CADManager.ShapeDataMap[ szID ];
			//	CADData cadData = new CADData( TopoDS.ToWire( pathData.Shape ), pathData.Edge5DList, pathData.Transform );
			//	cadDataList.Add( cadData );
			//}

			//// show CAMEditForm
			//CAMEditForm camEditForm = new CAMEditForm();
			//camEditForm.Size = new System.Drawing.Size( 1200, 800 );
			//CAMEditModel camEditModel = new CAMEditModel( m_CADManager.PartShape, cadDataList );
			//camEditForm.Init( camEditModel );
			//camEditForm.ShowDialog();
			//if( camEditForm.DialogResult != DialogResult.OK ) {
			//	return;
			//}
		}

		// manager events
		void OnPartChanged()
		{
			// clear the tree view and viewer
			m_ViewManager.PartNode.Nodes.Clear();
			m_ViewManager.PathNode.Nodes.Clear();
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// update view manager data
			m_ViewManager.ViewObjectMap.Clear();
			m_ViewManager.TreeNodeMap.Clear();
			foreach( var szNewDataID in m_CADManager.PartIDList ) {
				ShapeData data = m_CADManager.ShapeDataMap[ szNewDataID ];

				// add node to the tree view
				TreeNode node = new TreeNode( data.UID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( data.UID, node );

				// add shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreatePartAIS( data.Shape );
				m_ViewManager.ViewObjectMap.Add( data.UID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
			m_Viewer.UpdateView();
		}

		void OnFeatureAdded( List<string> newFeatureIDs )
		{
			foreach( string szID in newFeatureIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					return;
				}

				// add a new node to the tree view
				TreeNode node = new TreeNode( szID );
				m_ViewManager.PartNode.Nodes.Add( node );
				m_ViewManager.TreeNodeMap.Add( szID, node );

				// add a new shape to the viewer
				AIS_Shape aisShape = ViewHelper.CreateFeatureAIS( m_CADManager.ShapeDataMap[ szID ].Shape );
				m_ViewManager.ViewObjectMap.Add( szID, new ViewObject( aisShape ) );
				m_Viewer.GetAISContext().Display( aisShape, false ); // this will also activate
			}

			// update tree view and viewer
			m_ViewManager.PartNode.ExpandAll();
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
		void StartEditAction( IEditorAction action )
		{
			// to prevent from non-necessary default action start
			m_IsNextAction = true;

			// end the current action
			m_CurrentAction.End();
			m_IsNextAction = false;

			// start the action
			m_CurrentAction = action;
			m_CurrentAction.StartAction += OnEditActionStart;
			m_CurrentAction.EndAction += OnEditActionEnd;
			m_CurrentAction.Start();
		}

		void OnEditActionEnd( IEditorAction action )
		{
			// start default action if all edit actions are done
			if( !m_IsNextAction ) {
				m_CurrentAction = m_DefaultAction;
				m_CurrentAction.Start();
			}

			if( action.ActionType == EditActionType.AxisTransform ) {
				AxisTransformActionStausChanged?.Invoke( EActionStatus.End );
			}
		}

		void OnEditActionStart( IEditorAction action )
		{
			if( action.ActionType == EditActionType.AxisTransform ) {
				AxisTransformActionStausChanged?.Invoke( EActionStatus.Start );
			}
		}

		bool m_IsNextAction = false;
	}
}
