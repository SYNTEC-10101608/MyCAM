using DataStructure;
using OCC.AIS;
using OCC.Graphic3d;
using OCC.Prs3d;
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

namespace ProcessEdit
{
	public partial class ProcessEditForm : Form
	{
		public Action<TopoDS_Shape, List<CADData>> ExtractOK;

		public ProcessEditForm( TopoDS_Shape modelShape )
		{
			InitializeComponent();

			// create the viewer
			bool bSucess = m_OCCViewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( ToString() + "Init Error: Init Viewer" );
				return;
			}
			Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Fill;
			m_OCCViewer.UpdateView();

			// set AIS selction style
			Prs3d_Drawer d = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected );
			d.SetColor( new OCC.Quantity.Quantity_Color( OCC.Quantity.Quantity_NameOfColor.Quantity_NOC_RED ) );
			d.SetTransparency( 0.5f );
			d.SetDisplayMode( 1 );
			m_OCCViewer.GetAISContext().SetHighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected, d );

			// set model
			if( modelShape == null ) {
				return;
			}
			m_ModelShape = modelShape;
			ShowModel();

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// TODO: panel key down does not work
			PreviewKeyDown += ViewerKeyDown;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// import model
		TopoDS_Shape m_ModelShape = null;

		void ShowModel()
		{
			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( m_ModelShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( 1 );

			// display the shape
			m_OCCViewer.GetAISContext().RemoveAll( false );
			m_OCCViewer.GetAISContext().Display( aisShape, true );
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();

			// start face selection mode
			m_OCCViewer.GetAISContext().Deactivate();
			m_OCCViewer.GetAISContext().Activate( 4 /*face mode*/ );
		}

		// extract face
		void m_tsmiOK_Click( object sender, EventArgs e )
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
			ExtractOK?.Invoke( m_ModelShape, cadDataList );
		}

		List<TopoDS_Face> GetSelectedFace()
		{
			List<TopoDS_Face> lstFace = new List<TopoDS_Face>();
			m_OCCViewer.GetAISContext().InitSelected();
			while( m_OCCViewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape theShape = m_OCCViewer.GetAISContext().SelectedShape();
				if( theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
					lstFace.Add( TopoDS.ToFace( theShape ) );
				}
				m_OCCViewer.GetAISContext().NextSelected();
			}
			return lstFace;
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
			TopExp.MapShapesAndAncestors( m_ModelShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref solidMap );

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

		// TODO: the grouping method is tricky, need to be improved
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

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {

				// select the face
				m_OCCViewer.ShiftSelect();
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {

				// clear the selected face
				m_OCCViewer.GetAISContext().ClearSelected( true );
			}
		}
	}
}
