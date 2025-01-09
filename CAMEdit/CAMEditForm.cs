using DataStructure;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopAbs;
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

			// set AIS selction style
			Prs3d_Drawer d = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected );
			d.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			d.SetTransparency( 0.5f );
			d.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );
			Prs3d_Drawer d1 = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Selected );
			d1.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			d1.SetTransparency( 0.5f );
			d1.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// TODO: panel key down does not work
			PreviewKeyDown += ViewerKeyDown;
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
			editMode = EditMode.None;
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
		enum EditMode
		{
			None,
			StartPoint,
		}
		EditMode m_EditMode = EditMode.None;
		EditMode editMode
		{
			get
			{
				return m_EditMode;
			}
			set
			{
				EndAction();
				m_EditMode = value;
				StartAction();
			}
		}

		// for viewer resource handle
		List<AIS_Shape> m_CADContourAISList = new List<AIS_Shape>(); // for active only, no need refresh
		List<AIS_Shape> m_CAMContourAISList = new List<AIS_Shape>(); // need refresh
		List<AIS_Shape> m_ToolVecAISList = new List<AIS_Shape>(); // need refresh
		List<AIS_Shape> m_OrientationAISList = new List<AIS_Shape>(); // need refresh

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
				contourAIS.SetWidth( 1.5 );
				m_OCCViewer.GetAISContext().Display( contourAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( contourAIS );
				m_CADContourAISList.Add( contourAIS );
			}
		}

		void ShowCAMData()
		{
			ShowCAMContour();
			ShowToolVec();
			ShowOrientation();
			m_OCCViewer.UpdateView();
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
			m_ToolVecAISList.Clear();

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

		void ShowOrientation()
		{
			// clear the previous orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_OCCViewer.GetAISContext().Remove( orientationAIS, false );
			}
			m_OrientationAISList.Clear();

			// build orientation
			foreach( CAMData camData in m_Model.CAMDataList ) {
				int nDataCount = camData.CAMPointList.Count;
				int nShowIndex = 0;
				gp_Pnt showPoint = camData.CAMPointList[ nShowIndex ].Point;

				// the direction of the orientation is vector to the next point
				gp_Dir orientationDir = new gp_Dir( camData.CAMPointList[ nShowIndex + 1 ].Point.XYZ() - showPoint.XYZ() );
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISList.Add( orientationAIS );
			}

			// display the orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_OCCViewer.GetAISContext().Display( orientationAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( orientationAIS );
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

		AIS_Shape GetOrientationAIS( gp_Pnt point, gp_Dir dir )
		{
			// draw a cone to indicate the orientation
			gp_Ax2 coneAx2 = new gp_Ax2( point, dir );
			BRepPrimAPI_MakeCone coneMaker = new BRepPrimAPI_MakeCone( coneAx2, 0.5, 0, 2 );
			AIS_Shape coneAIS = new AIS_Shape( coneMaker.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			coneAIS.SetMaterial( aspect );
			coneAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			coneAIS.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );
			coneAIS.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			return coneAIS;
		}

		void m_tsmiStartPoint_Click( object sender, EventArgs e )
		{
			editMode = EditMode.StartPoint;
		}

		void m_tsmiReverse_Click( object sender, EventArgs e )
		{
			GetSelectedWireInfo( out CAMData camData, out _ );
			if( camData == null ) {
				return;
			}
			camData.IsReverse = !camData.IsReverse;
			ShowCAMData();
		}

		void m_tsmiTV_Default_Click( object sender, EventArgs e )
		{
			OnToolVecTypeChanged( ToolVectorType.Default );
		}

		void m_tsmiTV_Intersecting_Click( object sender, EventArgs e )
		{
			OnToolVecTypeChanged( ToolVectorType.Intersecting );
		}

		void m_tsmiTV_Z_Click( object sender, EventArgs e )
		{
			OnToolVecTypeChanged( ToolVectorType.TowardZ );
		}

		void OnToolVecTypeChanged( ToolVectorType toolVecType )
		{
			GetSelectedWireInfo( out CAMData camData, out _ );
			if( camData == null ) {
				return;
			}
			camData.ToolVectorType = toolVecType;
			ShowCAMData();
		}

		void m_tsmiOffset_Click( object sender, EventArgs e )
		{

		}

		void m_tsmiLead_Click( object sender, EventArgs e )
		{

		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			List<IProcessData> cuttingProcessDataList =
				m_Model.CAMDataList.Select( camData => new CuttingProcessData( camData ) ).Cast<IProcessData>().ToList();
			CADEditOK?.Invoke( m_Model.PartShape, cuttingProcessDataList );
		}

		// viewer action
		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			switch( editMode ) {
				case EditMode.StartPoint:
					if( e.KeyCode == Keys.Escape ) {
						editMode = EditMode.None;
					}
					break;
				case EditMode.None:
				default:
					break;
			}
		}

		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			switch( editMode ) {
				case EditMode.StartPoint:
					if( e.Button != MouseButtons.Left ) {
						return;
					}
					m_OCCViewer.Select();
					GetSelectedVertexInfo( out CAMData camData, out int nIndex );
					if( camData == null || nIndex == -1 ) {
						return;
					}
					camData.StartPoint = nIndex;
					ShowCAMData();
					break;
				case EditMode.None:
				default:
					if( e.Button != MouseButtons.Left ) {
						return;
					}
					m_OCCViewer.Select();
					break;
			}
		}

		void GetSelectedWireInfo( out CAMData camData, out AIS_InteractiveObject selectedAIS )
		{
			camData = null;
			selectedAIS = null;
			m_OCCViewer.GetAISContext().InitSelected();
			if( !m_OCCViewer.GetAISContext().MoreSelected() ) {
				return;
			}

			// get the sleected AIS shape
			selectedAIS = m_OCCViewer.GetAISContext().SelectedInteractive();
			if( selectedAIS == null ) {
				return;
			}

			// get the selected shape
			TopoDS_Shape selectedShape = m_OCCViewer.GetAISContext().SelectedShape();
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_WIRE ) {
				return;
			}

			// TODO: the mapping method is not good, try use OCC map or other method later
			foreach( TopoDS_Wire wire in m_ContourMap.Keys ) {
				if( wire.IsEqual( selectedShape ) ) {
					camData = m_ContourMap[ wire ];
					return;
				}
			}
		}

		void GetSelectedVertexInfo( out CAMData camData, out int nIndex )
		{
			camData = null;
			nIndex = -1;
			m_OCCViewer.GetAISContext().InitSelected();
			if( !m_OCCViewer.GetAISContext().MoreSelected() ) {
				return;
			}

			// get the selected vertex
			TopoDS_Shape selectedShape = m_OCCViewer.GetAISContext().SelectedShape();
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return;
			}

			// TODO: the mapping method is not good, try use OCC map or other method later
			foreach( TopoDS_Vertex vertex in m_VertexMap.Keys ) {

				// using distance threadhold to find the selected vertex
				gp_Pnt p1 = BRep_Tool.Pnt( vertex );
				gp_Pnt p2 = BRep_Tool.Pnt( TopoDS.ToVertex( selectedShape ) );
				double dDistance = p1.Distance( p2 );
				if( dDistance < 1e-6 ) {
					camData = m_VertexMap[ vertex ].Item1;
					nIndex = m_VertexMap[ vertex ].Item2;
					return;
				}
			}
		}

		void StartAction()
		{
			switch( editMode ) {
				case EditMode.StartPoint:
					GetSelectedWireInfo( out CAMData camData, out AIS_InteractiveObject selectedAIS );
					if( camData == null || selectedAIS == null ) {
						editMode = EditMode.None;
						return;
					}

					// activate the selected contour
					m_OCCViewer.GetAISContext().Activate( selectedAIS, (int)AISActiveMode.Vertex );

					// disable all other tsmi
					m_tsmiReverse.Enabled = false;
					m_tsmiToolVec.Enabled = false;
					m_tsmiOffset.Enabled = false;
					m_tsmiLead.Enabled = false;
					m_tsmiOK.Enabled = false;

					// check the start point tsmi
					m_tsmiStartPoint.Checked = true;
					break;
				case EditMode.None:
				default:

					// activate the contour CAD wire selection
					foreach( AIS_Shape oneShape in m_CADContourAISList ) {
						m_OCCViewer.GetAISContext().Activate( oneShape, (int)AISActiveMode.Wire );
					}
					break;
			}
		}

		void EndAction()
		{
			switch( editMode ) {
				case EditMode.StartPoint:

					// clear all selected
					m_OCCViewer.GetAISContext().ClearSelected( true );

					// deactivate the contour CAD wire selection
					foreach( AIS_Shape oneShape in m_CADContourAISList ) {
						m_OCCViewer.GetAISContext().Deactivate( oneShape );
					}

					// enable all other tsmi
					m_tsmiReverse.Enabled = true;
					m_tsmiToolVec.Enabled = true;
					m_tsmiOffset.Enabled = true;
					m_tsmiLead.Enabled = true;
					m_tsmiOK.Enabled = true;

					// uncheck the start point tsmi
					m_tsmiStartPoint.Checked = false;
					break;
				case EditMode.None:
				default:

					// deactivate the contour CAD wire selection
					foreach( AIS_Shape oneShape in m_CADContourAISList ) {
						m_OCCViewer.GetAISContext().Deactivate( oneShape );
					}
					break;
			}
		}
	}
}
