using OCC.AIS;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class SelectFaceAction : CADACtionBase
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

		public SelectFaceAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
			m_VisibleFaceAISPairList = new List<FaceHandle>();
			m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			foreach( var oneShapeData in m_CADManager.ShapeDataContainer ) {
				if( m_CADManager.ViewObjectMap[ oneShapeData.UID ].Visible ) {

					// collect all faces
					TopExp_Explorer exp = new TopExp_Explorer( oneShapeData.Shape, TopAbs_ShapeEnum.TopAbs_FACE );
					for( ; exp.More(); exp.Next() ) {
						TopoDS_Face face = TopoDS.ToFace( exp.Current() );
						AIS_Shape aisShape = ViewHelper.CreatePartAIS( face );
						m_VisibleFaceAISPairList.Add( new FaceHandle() { Face = face, AIS = aisShape } );
					}

					// add into map
					TopExp.MapShapesAndAncestors( oneShapeData.Shape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );
				}
			}
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.SelectFace;
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
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
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
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				if( viewObject.Visible ) {
					m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
				}
			}

			// hide part for selction
			HidePart();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// TODO: figure how OCCT slect work, dont do shit like this
			if( e.Button == MouseButtons.Left ) {

				// select the face
				AIS_InteractiveObject ais = null;

				// the program will crash if nothing detected
				try {
					ais = m_Viewer.GetAISContext().DetectedInteractive();
				}
				catch {
					return;
				}
				if( ais == null || ais.IsNull() ) {
					return;
				}

				// arrange the colors
				Quantity_Color color = new Quantity_Color();
				ais.Color( ref color );

				// toggle color
				if( color.Name() == COLOR_DEFAULT ) {
					ais.SetColor( new Quantity_Color( COLOR_SELECTED ) );
				}
				else {
					ais.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
				}
				ais.Attributes().SetFaceBoundaryDraw( true );
				ais.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
				m_Viewer.UpdateView();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
		}

		public void SelectD1ContFace()
		{
			List<TopoDS_Face> faceBFSQueue = GetSelectedFace();
			if( faceBFSQueue.Count == 0 ) {
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

			// select all D1 continuous faces
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				bool isD1Cont = false;
				foreach( TopoDS_Face oneFace in allD1ContFaceList ) {
					if( faceAISPair.Face.IsEqual( oneFace ) ) {
						isD1Cont = true;
						break;
					}
				}
				if( isD1Cont ) {
					faceAISPair.AIS.SetColor( new Quantity_Color( COLOR_SELECTED ) );
				}
				else {
					faceAISPair.AIS.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
				}
				faceAISPair.AIS.Attributes().SetFaceBoundaryDraw( true );
				faceAISPair.AIS.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			}
			m_Viewer.UpdateView();
		}

		public void SelectDone()
		{
			List<TopoDS_Face> extractedFaceList = GetSelectedFace();
			if( extractedFaceList.Count == 0 ) {
				End();
				return;
			}

			// sew the faces
			TopoDS_Shape sewResult = ShapeTool.SewShape( extractedFaceList.Cast<TopoDS_Shape>().ToList() );
			List<TopoDS_Shape> faceGroupList = new List<TopoDS_Shape>();

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

			// update datas
			m_CADManager.AddComponentFaceFeature( faceGroupList );
			End();
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
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				Quantity_Color color = new Quantity_Color();
				faceAISPair.AIS.Color( ref color );
				if( color.Name() == COLOR_SELECTED ) {
					selectedFaceList.Add( faceAISPair.Face );
				}
			}
			return selectedFaceList;
		}

		List<FaceHandle> m_VisibleFaceAISPairList;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;

		const Quantity_NameOfColor COLOR_SELECTED = Quantity_NameOfColor.Quantity_NOC_RED;
		const Quantity_NameOfColor COLOR_DEFAULT = Quantity_NameOfColor.Quantity_NOC_GRAY50;
	}
}
