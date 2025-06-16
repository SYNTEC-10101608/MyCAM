using CAMEdit;
using DataStructure;
using OCC.AIS;
using OCC.Quantity;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System;
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
			m_VisiblePart = ShapeTool.MakeCompound( visibleShapeList );
			if( m_VisiblePart == null || m_VisiblePart.IsNull() ) {
				throw new ArgumentNullException( "SelectFaceAction part shape is null." );
			}

			// map visible edge and face
			m_EdgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( m_VisiblePart, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref m_EdgeFaceMap );
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
			// TODO: figure how OCCT slect work, dont do shit like this
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
				return;
			}

			// build CAD data
			List<CADData> cadDataList = BuildCADData( extractedFaceList );
			if( cadDataList.Count == 0 ) {
				MessageBox.Show( ToString() + "Error: No Pattern Found" );
				return;
			}

			// show CAMEditForm
			CAMEditForm camEditForm = new CAMEditForm();
			camEditForm.Size = new System.Drawing.Size( 1200, 800 );
			CAMEditModel camEditModel = new CAMEditModel( m_VisiblePart, cadDataList );
			camEditForm.Init( camEditModel );
			camEditForm.ShowDialog();
			if( camEditForm.DialogResult != DialogResult.OK ) {
				End();
				return;
			}
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

		List<CADData> BuildCADData( List<TopoDS_Face> extractedFaceList )
		{
			List<CADData> cadDataList = new List<CADData>();

			// get free boundary wires
			List<TopoDS_Wire> boundaryWireList = GetAllCADContour( extractedFaceList, out TopoDS_Shape sewResult );
			if( boundaryWireList.Count == 0 ) {
				MessageBox.Show( ToString() + "Error: No boundary wire" );
				return cadDataList;
			}

			// map the edges to faces
			TopTools_IndexedDataMapOfShapeListOfShape shellMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( sewResult, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref shellMap );
			TopTools_IndexedDataMapOfShapeListOfShape solidMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( m_VisiblePart, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref solidMap );

			// build CAD data
			foreach( TopoDS_Wire wire in boundaryWireList ) {

				// split the map by edges in wire
				TopTools_IndexedDataMapOfShapeListOfShape oneShellMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				TopTools_IndexedDataMapOfShapeListOfShape oneSolidMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				foreach( TopoDS_Shape edge in wire.elementsAsList ) {
					TopTools_ListOfShape shellFaceList = shellMap.FindFromKey( edge );
					TopTools_ListOfShape solidFaceList = solidMap.FindFromKey( edge );
					if( shellFaceList != null && solidFaceList != null ) {
						oneShellMap.Add( edge, shellFaceList );
						oneSolidMap.Add( edge, solidFaceList );
					}
				}

				// create CAD data
				CADData cadData = new CADData( wire, oneShellMap, oneSolidMap );
				cadDataList.Add( cadData );
			}
			return cadDataList;
		}

		List<TopoDS_Wire> GetAllCADContour( List<TopoDS_Face> extractedFaceList, out TopoDS_Shape sewResult )
		{
			// sew the faces
			sewResult = ShapeTool.SewShape( extractedFaceList.Cast<TopoDS_Shape>().ToList() );
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

			// get free boundary wires
			List<TopoDS_Wire> wireList = new List<TopoDS_Wire>();
			foreach( TopoDS_Shape faceGroup in faceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( faceGroup );
				TopExp_Explorer wireExp = new TopExp_Explorer( freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					wireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}
			return wireList;
		}

		List<FaceHandle> m_VisibleFaceAISPairList;
		TopoDS_Shape m_VisiblePart;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;

		const Quantity_NameOfColor COLOR_SELECTED = Quantity_NameOfColor.Quantity_NOC_RED;
		const Quantity_NameOfColor COLOR_DEFAULT = Quantity_NameOfColor.Quantity_NOC_GRAY50;
	}
}
