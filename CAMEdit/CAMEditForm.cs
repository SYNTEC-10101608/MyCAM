using DataStructure;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.Geom;
using OCC.GeomAPI;
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
			d.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			Prs3d_Drawer d1 = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Selected );
			d1.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			d1.SetTransparency( 0.5f );
			d1.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.MouseMove += ViewerMouseMove;
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
			editMode = EditMode.None;
			return true;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// model
		CAMEditModel m_Model;

		// for UI action
		Dictionary<gp_Pnt, Tuple<CAMData, int>> m_VertexMap = new Dictionary<gp_Pnt, Tuple<CAMData, int>>();
		Dictionary<TopoDS_Wire, CAMData> m_ContourMap = new Dictionary<TopoDS_Wire, CAMData>();
		enum EditMode
		{
			None,
			StartPoint,
			TooVec,
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
		gp_Ax2 m_ToolVecAx2;
		CAMData m_SelectedCAMData = null;
		int m_SelectedIndex = -1;

		// for viewer resource handle
		List<AIS_Shape> m_CADContourAISList = new List<AIS_Shape>(); // for active only, no need refresh
		List<AIS_Shape> m_CAMContourAISList = new List<AIS_Shape>(); // need refresh
		List<AIS_Line> m_ToolVecAISList = new List<AIS_Line>(); // need refresh
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
			partAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
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
				BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

				// add points to the polygon
				for( int j = 0; j < camData.CADPointList.Count; j++ ) {
					polygonMaker.Add( camData.CADPointList[ j ].Point );
					m_VertexMap.Add( camData.CADPointList[ j ].Point, new Tuple<CAMData, int>( camData, j ) );
				}
				if( polygonMaker.IsDone() == false ) {
					continue;
				}
				TopoDS_Wire wire = polygonMaker.Wire();
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


			List<TopoDS_Wire> camWireList = new List<TopoDS_Wire>();
			for( int i = 0; i < m_Model.CAMDataList.Count; i++ ) {

				// build wire from cam points
				CAMData camData = m_Model.CAMDataList[ i ];
				BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

				// build edges
				for( int j = 0; j < camData.CAMPointList.Count; j++ ) {
					polygonMaker.Add( camData.CAMPointList[ j ].CADPoint.Point );
				}
				if( polygonMaker.IsDone() == false ) {
					continue;
				}
				TopoDS_Wire wire = polygonMaker.Wire();
				camWireList.Add( wire );
			}

			// display the cam data
			foreach( TopoDS_Wire camWire in camWireList ) {
				AIS_Shape camAIS = new AIS_Shape( camWire );
				camAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_YELLOW ) );
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
			foreach( AIS_Line toolVecAIS in m_ToolVecAISList ) {
				m_OCCViewer.GetAISContext().Remove( toolVecAIS, false );
			}
			m_ToolVecAISList.Clear();

			// build tool vec
			foreach( CAMData camData in m_Model.CAMDataList ) {
				int nIndex = 0;
				foreach( CAMPoint camPoint in camData.CAMPointList ) {
					AIS_Line toolVecAIS = GetVecAIS( camPoint.CADPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
					if( camData.GetToolVecModifyIndex().Contains( nIndex ) ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
					}
					m_ToolVecAISList.Add( toolVecAIS );
					nIndex++;
				}
			}

			// display the tool vec
			foreach( AIS_Line toolVecAIS in m_ToolVecAISList ) {
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
				gp_Pnt showPoint = camData.CAMPointList[ 0 ].CADPoint.Point;
				gp_Pnt endPoint = camData.CAMPointList[ 1 ].CADPoint.Point;

				// the direction of the orientation is vector to the next point
				gp_Dir orientationDir = new gp_Dir( endPoint.XYZ() - showPoint.XYZ() );
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISList.Add( orientationAIS );
			}

			// display the orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_OCCViewer.GetAISContext().Display( orientationAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( orientationAIS );
			}
		}

		AIS_Line GetVecAIS( gp_Pnt point, gp_Dir dir, EvecType vecType )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
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
			coneAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
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
			GetSelectedWireInfo( out CAMData camData, out _ );
			if( camData == null ) {
				return;
			}
			if( camData.Offset == 0 ) {
				camData.Offset = 1;
			}
			else {
				camData.Offset = 0;
			}
			ShowCAMData();
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
				case EditMode.TooVec:
					if( e.KeyCode != Keys.Escape ) {
						return;
					}
					if( m_ToolVecAx2 == null || m_SelectedCAMData == null || m_SelectedIndex == -1 ) {
						return;
					}
					m_SelectedCAMData.SetToolVecModify( m_SelectedIndex, 0, 0 );
					ShowCAMData();
					editMode = EditMode.None;

					break;
				case EditMode.None:
				default:
					break;
			}
		}

		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			m_panViewer.Focus();
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
					//camData.StartPoint = nIndex;
					//ShowCAMData();

					// record point tangent and normal vec ax2
					CADPoint cadPoint = camData.CADPointList[ nIndex ];
					m_ToolVecAx2 = new gp_Ax2( cadPoint.Point, cadPoint.NormalVec.Crossed( cadPoint.TangentVec ), cadPoint.TangentVec );
					m_SelectedCAMData = camData;
					m_SelectedIndex = nIndex;
					editMode = EditMode.TooVec;

					break;
				case EditMode.TooVec:
					if( e.Button != MouseButtons.Left ) {
						return;
					}
					editMode = EditMode.None;

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

		void ViewerMouseMove( object sender, MouseEventArgs e )
		{
			switch( editMode ) {
				case EditMode.TooVec:
					if( m_ToolVecAx2 == null || m_SelectedCAMData == null || m_SelectedIndex == -1 ) {
						return;
					}

					// convert the mouse position to 3D point
					double xp = 0;
					double yp = 0;
					double zp = 0;
					m_OCCViewer.Convert( e.X, e.Y, ref xp, ref yp, ref zp );

					// make a sphere for direction control
					gp_Sphere sphere = new gp_Sphere();
					sphere.SetLocation( m_ToolVecAx2.Location() );
					sphere.SetRadius( 10 );
					Geom_SphericalSurface sphereG = new Geom_SphericalSurface( sphere );

					// make line representing view direction
					gp_Pnt mousePoint = new gp_Pnt( xp, yp, zp );
					gp_Lin viewLine = new gp_Lin( mousePoint, m_OCCViewer.GetViewDir() );
					Geom_Line viewLineG = new Geom_Line( viewLine );

					// get intersection point of the line and the sphere
					GeomAPI_IntCS intCS = new GeomAPI_IntCS( viewLineG, sphereG );
					gp_Pnt ps = new gp_Pnt();
					if( intCS.NbPoints() == 0 ) {

						// get closet point of the line and the sphere center
						GeomAPI_ProjectPointOnCurve projectPoint = new GeomAPI_ProjectPointOnCurve( m_ToolVecAx2.Location(), viewLineG );
						double u = projectPoint.LowerDistanceParameter();
						ps = viewLineG.Value( u );
					}
					else {

						// get the point closet to the mouse point
						double dMin = double.MaxValue;
						for( int i = 1; i <= intCS.NbPoints(); i++ ) {
							gp_Pnt p = intCS.Point( i );
							double d = p.Distance( mousePoint );
							if( d < dMin ) {
								dMin = d;
								ps = p;
							}
						}
					}

					// get direction of the line
					gp_Dir dir = new gp_Dir( ps.XYZ() - m_ToolVecAx2.Location().XYZ() );

					// project the vector to the ax2
					double X = dir.Dot( m_ToolVecAx2.XDirection() );
					double Y = dir.Dot( m_ToolVecAx2.YDirection() );
					double Z = dir.Dot( m_ToolVecAx2.Direction() );

					// get angle A is atan2 Z/X
					double angleA = Math.Atan2( X, Z ) * 180 / Math.PI;

					// get angle B is atan2 Z/Y
					double angleB = Math.Atan2( Y, Z ) * 180 / Math.PI;
					m_SelectedCAMData.SetToolVecModify( m_SelectedIndex, angleA, angleB );
					ShowCAMData();
					break;
				case EditMode.None:
				default:
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
			foreach( gp_Pnt vertex in m_VertexMap.Keys ) {

				// using distance threadhold to find the selected vertex
				gp_Pnt p1 = vertex;
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

					// highlight the start point tsmi
					m_tsmiStartPoint.BackColor = System.Drawing.Color.Yellow;
					break;
				case EditMode.TooVec:
					//m_OCCViewer.SetViewDir( m_ToolVecAx2.YDirection().Reversed() );
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

					// restore the start point tsmi to system control color
					m_tsmiStartPoint.BackColor = System.Drawing.SystemColors.Control;
					break;
				case EditMode.TooVec:
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
