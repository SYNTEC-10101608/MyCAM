using OCC.AIS;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class SelectPathAction : CADACtionBase
	{
		class EdgeHandle
		{
			public TopoDS_Edge Edge
			{
				get; set;
			}

			public AIS_Shape AIS
			{
				get; set;
			}
		}

		public SelectPathAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
			m_VisibleEdgeAISPairList = new List<EdgeHandle>();
			foreach( var oneShapeData in m_CADManager.ShapeDataContainer ) {
				if( m_CADManager.ViewObjectMap[ oneShapeData.UID ].Visible
					&& m_CADManager.ComponetFaceIDList.Contains( oneShapeData.UID ) ) {

					// collect all edge
					TopExp_Explorer exp = new TopExp_Explorer( oneShapeData.Shape, TopAbs_ShapeEnum.TopAbs_EDGE );
					for( ; exp.More(); exp.Next() ) {
						TopoDS_Edge edge = TopoDS.ToEdge( exp.Current() );
						AIS_Shape aisShape = ViewHelper.CreatePartAIS( edge );
						m_VisibleEdgeAISPairList.Add( new EdgeHandle() { Edge = edge, AIS = aisShape } );
					}
				}
			}
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.SelectPath;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// deactivate all shape and hide non-component-face shape
			foreach( var viewObject in m_CADManager.ViewObjectMap ) {
				m_Viewer.GetAISContext().Deactivate( viewObject.Value.AISHandle );
				if( !m_CADManager.ComponetFaceIDList.Contains( viewObject.Key ) ) {
					m_Viewer.GetAISContext().Erase( viewObject.Value.AISHandle, false );
				}
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

			// hide part for selection
			HidePart();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// TODO: figure how OCCT slect work, dont do shit like this
			if( e.Button == MouseButtons.Left ) {

				// select the edge
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
					ais.SetWidth( 2.0 );
				}
				else {
					ais.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
					ais.SetWidth( 1.0 );
				}
				ais.Attributes().SetFaceBoundaryDraw( true );
				ais.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
				m_Viewer.UpdateView();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
		}

		public void SelectDone()
		{
			List<TopoDS_Edge> extractedFaceList = GetSelectedEdge();
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
			foreach( var faceAISPair in m_VisibleEdgeAISPairList ) {
				m_Viewer.GetAISContext().Display( faceAISPair.AIS, false );
				m_Viewer.GetAISContext().Activate( faceAISPair.AIS, (int)AISActiveMode.Edge );
			}
		}

		void HidePart()
		{
			foreach( var faceAISPair in m_VisibleEdgeAISPairList ) {
				m_Viewer.GetAISContext().Remove( faceAISPair.AIS, false );
			}
		}

		List<TopoDS_Edge> GetSelectedEdge()
		{
			List<TopoDS_Edge> selectedFaceList = new List<TopoDS_Edge>();
			foreach( var faceAISPair in m_VisibleEdgeAISPairList ) {
				Quantity_Color color = new Quantity_Color();
				faceAISPair.AIS.Color( ref color );
				if( color.Name() == COLOR_SELECTED ) {
					selectedFaceList.Add( faceAISPair.Edge );
				}
			}
			return selectedFaceList;
		}

		List<EdgeHandle> m_VisibleEdgeAISPairList;

		const Quantity_NameOfColor COLOR_SELECTED = Quantity_NameOfColor.Quantity_NOC_RED;
		const Quantity_NameOfColor COLOR_DEFAULT = Quantity_NameOfColor.Quantity_NOC_GRAY50;
	}
}
