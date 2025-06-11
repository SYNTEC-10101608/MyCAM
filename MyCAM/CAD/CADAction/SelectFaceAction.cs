using OCC.AIS;
using OCC.Prs3d;
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
		class FaceAISPair
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

		class HLStyleBackup
		{
			public Quantity_Color Color
			{
				get; set;
			}

			public float Transparency
			{
				get; set;
			}

			public int DisplayMode
			{
				get; set;
			}
		}

		public SelectFaceAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
			List<TopoDS_Shape> visibleShapeList = new List<TopoDS_Shape>();
			m_VisibleFaceList = new List<TopoDS_Face>();
			m_VisibleFaceAISPairList = new List<FaceAISPair>();
			foreach( var oneShapeData in m_CADManager.ShapeDataContainer ) {
				if( m_CADManager.ViewObjectMap.ContainsKey( oneShapeData.UID ) && m_CADManager.ViewObjectMap[ oneShapeData.UID ].Visible ) {

					// add shape to make compound
					visibleShapeList.Add( oneShapeData.Shape );

					// collect all faces
					TopExp_Explorer exp = new TopExp_Explorer( oneShapeData.Shape, TopAbs_ShapeEnum.TopAbs_FACE );
					for( ; exp.More(); exp.Next() ) {
						TopoDS_Face face = TopoDS.ToFace( exp.Current() );
						AIS_Shape aisShape = ViewHelper.CreatePartAIS( face );
						m_VisibleFaceList.Add( face );
						m_VisibleFaceAISPairList.Add( new FaceAISPair() { Face = face, AIS = aisShape } );
					}
				}
			}

			// create a compound shape to slect face
			TopoDS_Shape visiblePart = ShapeTool.MakeCompound( visibleShapeList );
			if( visiblePart == null || visiblePart.IsNull() ) {
				throw new System.ArgumentNullException( "SelectFaceAction part shape is null." );
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

			// backup highlight style
			Prs3d_Drawer d1 = m_Viewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Selected );
			m_HLStyleBackup = new HLStyleBackup();
			m_HLStyleBackup.Color = d1.Color();
			m_HLStyleBackup.Transparency = d1.Transparency();
			m_HLStyleBackup.DisplayMode = d1.DisplayMode();

			// set highlight style
			d1.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			d1.SetTransparency( 0.5f );
			d1.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );

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

			// restore highlight style
			Prs3d_Drawer d1 = m_Viewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Selected );
			d1.SetColor( m_HLStyleBackup.Color );
			d1.SetTransparency( m_HLStyleBackup.Transparency );
			d1.SetDisplayMode( m_HLStyleBackup.DisplayMode );

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
				m_Viewer.ShiftSelect();
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
						bool isVisited = false;
						foreach( TopoDS_Face visitedFace in allD1ContFaceList ) {
							if( visitedFace.IsEqual( oneConnectedFace ) ) {
								isVisited = true;
								break;
							}
						}
						if( isVisited ) {
							continue;
						}

						// check D1 continuity
						if( GeometryTool.IsD1Cont( currentFace, oneConnectedFace, oneEdge ) ) {
							allD1ContFaceList.Add( oneConnectedFace );
							faceBFSQueue.Add( oneConnectedFace );
						}
					}
				}
			}

			// select all D1 continuous faces
			m_Viewer.GetAISContext().ClearSelected( false );
			foreach( TopoDS_Face face in allD1ContFaceList ) {

				// find the AIS for the face
				FaceAISPair faceAISPair = m_VisibleFaceAISPairList.Find( pair => pair.Face.IsEqual( face ) );
				if( faceAISPair != null ) {
					m_Viewer.GetAISContext().AddOrRemoveSelected( faceAISPair.AIS, false );
				}
			}
			m_Viewer.GetAISContext().InitSelected();
			int test = 0;
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				test++;
				m_Viewer.GetAISContext().NextSelected();
			}
			//m_Viewer.GetAISContext().HilightSelected( false );
			m_Viewer.UpdateView();
		}

		public void SelectDone()
		{
			m_Viewer.GetAISContext().InitSelected();
			int test = 0;
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				test++;
				m_Viewer.GetAISContext().NextSelected();
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
			List<TopoDS_Face> lstFace = new List<TopoDS_Face>();
			m_Viewer.GetAISContext().InitSelected();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape theShape = m_Viewer.GetAISContext().SelectedShape();
				if( theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
					lstFace.Add( TopoDS.ToFace( theShape ) );
				}
				m_Viewer.GetAISContext().NextSelected();
			}
			return lstFace;
		}

		List<TopoDS_Face> m_VisibleFaceList;
		List<FaceAISPair> m_VisibleFaceAISPairList;
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;

		HLStyleBackup m_HLStyleBackup;
	}
}
