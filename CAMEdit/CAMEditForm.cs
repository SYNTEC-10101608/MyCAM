using DataStructure;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CAMEdit
{
	public partial class CAMEditForm : Form
	{
		public Action<TopoDS_Shape, List<IProcessData>> CADEditOK;

		public CAMEditForm()
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

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;
		}

		public bool Init( CAMEditModel model )
		{
			if( model == null ) {
				return false;
			}
			m_Model = model;
			ShowPart();
			ShowCADContour();
			ShowCAMData();
			return true;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// model
		CAMEditModel m_Model;

		// for UI action
		Dictionary<TopoDS_Vertex, Tuple<CAMData, int>> m_VertexMap = new Dictionary<TopoDS_Vertex, Tuple<CAMData, int>>();
		Dictionary<TopoDS_Wire, CAMData> m_ContourMap = new Dictionary<TopoDS_Wire, CAMData>();

		// for viewer resource handle
		List<AIS_Shape> m_CAMContourAISList = new List<AIS_Shape>();
		List<AIS_Shape> m_ToolVecAISList = new List<AIS_Shape>();

		enum EvecType
		{
			ToolVec,
			TangentVec,
			NormalVec,
		}

		void ShowPart()
		{
			// get part shape
			TopoDS_Shape partShape = m_Model.PartShape;
			if( partShape == null ) {
				return;
			}

			// display the part
			AIS_Shape partAIS = new AIS_Shape( partShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			partAIS.SetMaterial( aspect );
			partAIS.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( partAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( partAIS );

			// update the viewer
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
			m_OCCViewer.UpdateView();
		}

		void ShowCADContour()
		{
			for( int i = 0; i < m_Model.CAMDataList.Count; i++ ) {

				// build wire from cad points
				CAMData camData = m_Model.CAMDataList[ i ];
				TopoDS_Vertex lastVertex = null;
				List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();

				// add vertex to map and build edges
				for( int j = 0; j < camData.CADPointList.Count; j++ ) {
					BRepBuilderAPI_MakeVertex vertexMaker = new BRepBuilderAPI_MakeVertex( camData.CADPointList[ j ].Point );
					TopoDS_Vertex vertex = vertexMaker.Vertex();

					// first vertex
					if( j == 0 || lastVertex == null ) {
						m_VertexMap.Add( vertex, new Tuple<CAMData, int>( camData, j ) );
						lastVertex = vertex;
						continue;
					}

					// check distance of two points
					double dDistance = camData.CADPointList[ j - 1 ].Point.Distance( camData.CADPointList[ j ].Point );
					if( dDistance < 1e-6 ) {
						continue;
					}
					BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( lastVertex, vertex );
					edgeList.Add( edgeMaker.Edge() );
					m_VertexMap.Add( vertex, new Tuple<CAMData, int>( camData, j ) );
					lastVertex = vertex;
				}

				// build wire
				BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
				foreach( TopoDS_Edge edge in edgeList ) {
					wireMaker.Add( edge );
				}
				TopoDS_Wire wire = wireMaker.Wire();
				m_ContourMap.Add( wire, camData );
			}

			// display the contour
			foreach( TopoDS_Wire contour in m_ContourMap.Keys ) {
				AIS_Shape contourAIS = new AIS_Shape( contour );
				contourAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
				contourAIS.SetWidth( 1 );
				m_OCCViewer.GetAISContext().Display( contourAIS, false );
				m_OCCViewer.GetAISContext().Activate( contourAIS, (int)AISActiveMode.Vertex );
			}
		}

		void ShowCAMData()
		{
			ShowCAMContour();
			ShowToolVec();
		}

		void ShowCAMContour()
		{
			// clear the previous cam data
			foreach( AIS_Shape camAIS in m_CAMContourAISList ) {
				m_OCCViewer.GetAISContext().Remove( camAIS, false );
			}
			m_CAMContourAISList.Clear();

			// build cam data
			List<TopoDS_Wire> camWireList = new List<TopoDS_Wire>();
			for( int i = 0; i < m_Model.CAMDataList.Count; i++ ) {

				// build wire from cam points
				CAMData camData = m_Model.CAMDataList[ i ];
				TopoDS_Vertex lastVertex = null;
				List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();

				// build edges
				for( int j = 0; j < camData.CADPointList.Count; j++ ) {
					BRepBuilderAPI_MakeVertex vertexMaker = new BRepBuilderAPI_MakeVertex( camData.CADPointList[ j ].Point );
					TopoDS_Vertex vertex = vertexMaker.Vertex();

					// first vertex
					if( j == 0 || lastVertex == null ) {
						lastVertex = vertex;
						continue;
					}

					// check distance of two points
					double dDistance = camData.CADPointList[ j - 1 ].Point.Distance( camData.CADPointList[ j ].Point );
					if( dDistance < 1e-6 ) {
						continue;
					}
					BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( lastVertex, vertex );
					edgeList.Add( edgeMaker.Edge() );
					lastVertex = vertex;
				}

				// build wire
				BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
				foreach( TopoDS_Edge edge in edgeList ) {
					wireMaker.Add( edge );
				}
				TopoDS_Wire wire = wireMaker.Wire();
				camWireList.Add( wire );
			}

			// display the cam data
			foreach( TopoDS_Wire camWire in camWireList ) {
				AIS_Shape camAIS = new AIS_Shape( camWire );
				camAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				camAIS.SetWidth( 1 );
				Prs3d_LineAspect aspect = camAIS.Attributes().WireAspect();
				aspect.SetTypeOfLine( Aspect_TypeOfLine.Aspect_TOL_DASH );
				m_OCCViewer.GetAISContext().Display( camAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( camAIS );
				m_CAMContourAISList.Add( camAIS );
			}
		}

		void ShowToolVec()
		{
			// clear the previous tool vec
			foreach( AIS_Shape toolVecAIS in m_ToolVecAISList ) {
				m_OCCViewer.GetAISContext().Remove( toolVecAIS, false );
			}

			// build tool vec
			foreach( CAMData camData in m_Model.CAMDataList ) {
				foreach( CAMPoint camPoint in camData.CAMPointList ) {
					AIS_Shape toolVecAIS = GetVecAIS( camPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
					m_ToolVecAISList.Add( toolVecAIS );
				}
			}

			// display the tool vec
			foreach( AIS_Shape toolVecAIS in m_ToolVecAISList ) {
				m_OCCViewer.GetAISContext().Display( toolVecAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( toolVecAIS );
			}
		}

		AIS_Shape GetVecAIS( gp_Pnt point, gp_Dir dir, EvecType vecType )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 1 );
			BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( point, endPoint );
			AIS_Shape lineAIS = new AIS_Shape( edgeMaker.Shape() );
			switch( vecType ) {
				case EvecType.ToolVec:
					lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
					break;
				case EvecType.TangentVec:
					lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
					break;
				case EvecType.NormalVec:
					lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
					break;
			}
			lineAIS.SetWidth( 2 );
			return lineAIS;
		}

		// viewer action
		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
		}

		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
		}

		void m_btnOK_Click( object sender, EventArgs e )
		{
			List<IProcessData> cuttingProcessDataList =
				m_Model.CAMDataList.Select( camData => new CuttingProcessData( camData ) ).Cast<IProcessData>().ToList();
			CADEditOK?.Invoke( m_Model.PartShape, cuttingProcessDataList );
		}
	}
}
