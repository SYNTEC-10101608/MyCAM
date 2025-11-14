using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class SelectFaceAction : KeyMouseActionBase
	{
		class FaceHandle
		{
			public TopoDS_Face Face
			{
				get; set;
			}

			public AIS_Shape AIS
			{
				get; set;
			}
		}

		public SelectFaceAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_VisibleFaceAISPairList = new List<FaceHandle>();
			m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				TopoDS_Shape oneshape = m_DataManager.ObjectMap[ partID ].Shape;

				// collect all faces
				TopExp_Explorer exp = new TopExp_Explorer( oneshape, TopAbs_ShapeEnum.TopAbs_FACE );
				for( ; exp.More(); exp.Next() ) {
					TopoDS_Face face = TopoDS.ToFace( exp.Current() );
					AIS_Shape aisShape = SelectViewHelper.CreateFaceAIS( face );
					m_VisibleFaceAISPairList.Add( new FaceHandle() { Face = face, AIS = aisShape } );
				}

				// add into map
				TopExp.MapShapesAndAncestors( oneshape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.SelectFace;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all shape
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Erase( viewObject.AISHandle, false );
			}

			// show part for selction
			ShowPart();
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// show all shape
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				if( viewObject.Visible ) {
					m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
					m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
				}
			}

			// hide part for selction
			HidePart();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Viewer.GetAISContext().DetectedOwner().IsNull()
					|| m_Viewer.GetAISContext().DetectedOwner().HasSelectable() == false ) {
					return;
				}
				AIS_InteractiveObject detectedObject = m_Viewer.GetAISContext().DetectedInteractive();
				if( ( Control.ModifierKeys & Keys.Control ) != Keys.Control ) {
					m_Viewer.GetAISContext().ClearSelected( false );
				}
				m_Viewer.GetAISContext().AddOrRemoveSelected( detectedObject, false );
				m_Viewer.UpdateView();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		public void SelectD1ContFace()
		{
			List<TopoDS_Face> faceBFSQueue = GetSelectedFace();
			if( faceBFSQueue.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( "[操作提醒]請先選擇面", MyApp.NoticeType.Hint );
				return;
			}

			// BFS all D1 continuous faces
			List<TopoDS_Face> allD1ContFaceList = new List<TopoDS_Face>( faceBFSQueue );
			TopTools_MapOfShape visitedFaceMap = new TopTools_MapOfShape();
			TopTools_MapOfShape visitedEdgeMap = new TopTools_MapOfShape();
			foreach( TopoDS_Face oneFace in faceBFSQueue ) {
				visitedFaceMap.Add( oneFace );
			}
			while( faceBFSQueue.Count > 0 ) {

				// get the first face in the queue
				TopoDS_Face currentFace = faceBFSQueue[ 0 ];
				faceBFSQueue.RemoveAt( 0 );

				// get all edges of the current face
				List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();
				TopExp_Explorer exp = new TopExp_Explorer( currentFace, TopAbs_ShapeEnum.TopAbs_EDGE );
				for( ; exp.More(); exp.Next() ) {
					if( visitedEdgeMap.Contains( exp.Current() ) ) {
						continue;
					}
					edgeList.Add( TopoDS.ToEdge( exp.Current() ) );
					visitedEdgeMap.Add( exp.Current() );
				}

				// find all D1 continuous faces
				foreach( TopoDS_Edge oneEdge in edgeList ) {
					foreach( TopoDS_Shape _oneConnectedFace in m_EdgeFaceMap.FindFromKey( oneEdge ) ) {

						// check visited
						if( visitedFaceMap.Contains( _oneConnectedFace ) ) {
							continue;
						}

						// check D1 continuity
						TopoDS_Face oneConnectedFace = TopoDS.ToFace( _oneConnectedFace );
						if( GeometryTool.IsD1Cont( currentFace, oneConnectedFace, oneEdge ) ) {
							visitedFaceMap.Add( oneConnectedFace );
							allD1ContFaceList.Add( oneConnectedFace );
							faceBFSQueue.Add( oneConnectedFace );
						}
					}
				}
			}
			m_Viewer.GetAISContext().ClearSelected( false );

			// select all D1 continuous faces
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				foreach( TopoDS_Face oneFace in allD1ContFaceList ) {
					if( faceAISPair.Face.IsEqual( oneFace ) ) {
						m_Viewer.GetAISContext().AddOrRemoveSelected( faceAISPair.AIS, false );
					}
				}
			}
			m_Viewer.UpdateView();
		}

		public List<TopoDS_Shape> GetResult()
		{
			List<TopoDS_Face> extractedFaceList = GetSelectedFace();
			List<TopoDS_Shape> faceGroupList = new List<TopoDS_Shape>();
			if( extractedFaceList.Count == 0 ) {
				return faceGroupList;
			}

			// sew the faces
			TopoDS_Shape sewResult = ShapeTool.SewShape( extractedFaceList.Cast<TopoDS_Shape>().ToList() );

			// single shell or single face
			if( sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_SHELL
				|| sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_FACE ) {
				faceGroupList.Add( sewResult );
			}

			// some shell and free face exist
			else {
				foreach( TopoDS_Shape shape in sewResult.elementsAsList ) {
					faceGroupList.Add( shape );
				}
			}
			return faceGroupList;
		}

		void ShowPart()
		{
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Display( faceAISPair.AIS, false );
				m_Viewer.GetAISContext().Activate( faceAISPair.AIS, (int)AISActiveMode.Face );
			}
		}

		void HidePart()
		{
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Remove( faceAISPair.AIS, false );
			}
		}

		List<TopoDS_Face> GetSelectedFace()
		{
			List<TopoDS_Face> selectedFaceList = new List<TopoDS_Face>();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape shape = m_Viewer.GetAISContext().SelectedShape();
				if( shape == null || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_FACE ) {
					continue;
				}
				selectedFaceList.Add( TopoDS.ToFace( shape ) );
				m_Viewer.GetAISContext().NextSelected();
			}
			return selectedFaceList;
		}

		List<FaceHandle> m_VisibleFaceAISPairList;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;
	}
}
