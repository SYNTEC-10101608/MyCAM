using OCC.AIS;
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

		public SelectFaceAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
			List<TopoDS_Shape> visibleShapeList = new List<TopoDS_Shape>();
			m_VisibleFaceList = new List<TopoDS_Face>();
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
						m_SelectedFaceList.Add( new FaceAISPair() { Face = face, AIS = aisShape } );
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

			// hide all shape
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// show transform part and G54 coordinate system
			ShowPart();

			// activate face selection
			foreach( var faceAISPair in m_SelectedFaceList ) {
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
			throw new NotImplementedException();
		}

		public void SelectDone()
		{
			throw new NotImplementedException();
		}

		void ShowPart()
		{
			foreach( var faceAISPair in m_SelectedFaceList ) {
				m_Viewer.GetAISContext().Display( faceAISPair.AIS, false );
			}
		}

		void HidePart()
		{
			foreach( var faceAISPair in m_SelectedFaceList ) {
				m_Viewer.GetAISContext().Remove( faceAISPair.AIS, false );
			}
		}

		List<TopoDS_Face> m_VisibleFaceList;
		List<FaceAISPair> m_SelectedFaceList = new List<FaceAISPair>();
		TopTools_IndexedDataMapOfShapeListOfShape m_EdgeFaceMap;
	}
}
