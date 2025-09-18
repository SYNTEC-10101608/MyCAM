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

	internal class CADEditor : EditorBase
	{
		public CADEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PartChanged += OnPartChanged;
			m_DataManager.FeatureAdded += OnFeatureAdded;

			// default action
			m_DefaultAction = new DefaultAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, ESelectObjectType.Part );
		}

		public Action<EActionStatus> AxisTransformActionStausChanged;

		// editor
		public override EEditorType Type
		{
			get
			{
				return EEditorType.CAD;
			}
		}

		public override void EditStart()
		{
			base.EditStart();

			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PartNode );
			m_ViewManager.PartNode.ExpandAll();
		}

		public override void EditEnd()
		{
			// clear tree
			m_TreeView.Nodes.Clear();
			base.EditEnd();
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

		public void ImportProjectFile()
		{
			ReadProjectFileAction action = new ReadProjectFileAction( m_DataManager, m_Viewer, m_ViewManager );
			StartEditAction( action );
		}

		public void SaveProjectFile()
		{
			SaveProjectFileAction action = new SaveProjectFileAction( m_DataManager );
			StartEditAction( action );
		}

		public void AddPoint( AddPointType type )
		{
			AddPointAction action = new AddPointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, type );
			StartEditAction( action );
		}

		public void AddLine( AddLineType type )
		{
			AddLineAction action = new AddLineAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, type );
			StartEditAction( action );
		}

		public void ThreePointTransform()
		{
			ThreePtTransformAction action = new ThreePtTransformAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void StartManaulTransform()
		{
			ManualTransformAction action = new ManualTransformAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void StartAxisTransform()
		{
			// need to use shape data to decide the cneter in the begin, so have to add this preotection
			if( m_DataManager.PartIDList.Count == 0 ) {
				return;
			}
			AxisTransformAction action = new AxisTransformAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void ApplyManualTransform( ETrsfConstraintType type )
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
			foreach( var szNewDataID in m_DataManager.PartIDList ) {
				ShapeData data = m_DataManager.ShapeDataMap[ szNewDataID ];

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
				AIS_Shape aisShape = ViewHelper.CreateFeatureAIS( m_DataManager.ShapeDataMap[ szID ].Shape );
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
			m_DataManager.AddPart( oneShape );
		}

		// edit actions
		protected override void OnEditActionStart( IEditorAction action )
		{
			base.OnEditActionStart( action );
			if( action.ActionType == EditActionType.AxisTransform ) {
				AxisTransformActionStausChanged?.Invoke( EActionStatus.Start );
			}
		}

		protected override void OnEditActionEnd( IEditorAction action )
		{
			if( action.ActionType == EditActionType.AxisTransform ) {
				AxisTransformActionStausChanged?.Invoke( EActionStatus.End );
			}
			base.OnEditActionEnd( action );
		}
	}
}
