using OCC.AIS;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
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
			List<TopoDS_Shape> visibleShapeList = new List<TopoDS_Shape>();
			m_VisibleFaceAISPairList = new List<FaceHandle>();
			foreach( var oneShapeData in m_CADManager.ShapeDataContainer ) {
				if( m_CADManager.ViewObjectMap.ContainsKey( oneShapeData.UID ) && m_CADManager.ViewObjectMap[ oneShapeData.UID ].Visible ) {

					// add shape to make compound
					visibleShapeList.Add( oneShapeData.Shape );

					// collect all faces
					TopExp_Explorer exp = new TopExp_Explorer( oneShapeData.Shape, TopAbs_ShapeEnum.TopAbs_FACE );
					for( ; exp.More(); exp.Next() ) {
						TopoDS_Face face = TopoDS.ToFace( exp.Current() );
						AIS_Shape aisShape = ViewHelper.CreateFaceAIS( face );
						m_VisibleFaceAISPairList.Add( new FaceHandle() { Face = face, AIS = aisShape } );
					}
				}
			}

			// create a compound shape to slect face
			TopoDS_Shape visiblePart = ShapeTool.MakeCompound( visibleShapeList );
			if( visiblePart == null || visiblePart.IsNull() ) {
				throw new ArgumentNullException( "SelectFaceAction part shape is null." );
			}

			// map visible edge and face
			m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( visiblePart, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );
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

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all shape
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// show transform part and G54 coordinate system
			ShowPart();

			// activate face selection
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Activate( faceAISPair.AIS, (int)AISActiveMode.Face );
			}
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// hide part and G54 coordinate system
			HidePart();

			// show all shape
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
				if( !viewObject.Visible ) {
					m_Viewer.GetAISContext().Erase( viewObject.AISHandle, false );
				}
			}
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {

				// select the face
				AIS_InteractiveObject ais = m_Viewer.GetAISContext().DetectedInteractive();
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

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
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
					edgeList.Add( TopoDS.ToEdge( exp.Current() ) );
				}

				// find all D1 continuous faces
				foreach( TopoDS_Edge oneEdge in edgeList ) {
					foreach( TopoDS_Shape _oneConnectedFace in m_EdgeFaceMap.FindFromKey( oneEdge ) ) {
						TopoDS_Face oneConnectedFace = TopoDS.ToFace( _oneConnectedFace );

						// check visited
						if( visitedFaceMap.Contains( oneConnectedFace ) ) {
							continue;
						}

						// check D1 continuity
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
			GetSelectedFace();
			End();
		}

		void ShowPart()
		{
			foreach( var faceAISPair in m_VisibleFaceAISPairList ) {
				m_Viewer.GetAISContext().Display( faceAISPair.AIS, false );
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
