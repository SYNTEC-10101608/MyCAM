using DataStructure;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.Geom;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TCollection;
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
			ToolVecSelect,
			TooVecEdit,
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
		gp_Ax2 m_SelectedToolVecAx2;
		CAMData m_SelectedCAMData = null;
		int m_SelectedIndex = -1;
		int m_nGap = 100;

		// for viewer resource handle
		AIS_Shape m_PartAIS = null; // for part shape
		List<AIS_Shape> m_CADContourAISList = new List<AIS_Shape>(); // no need refresh, need activate
		List<AIS_Line> m_ToolVecAISList = new List<AIS_Line>(); // need refresh, need activate
		List<AIS_Shape> m_OrientationAISList = new List<AIS_Shape>(); // need refresh, no need activate
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>(); // need refresh, no need activate

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
			m_PartAIS = new AIS_Shape( partShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			m_PartAIS.SetMaterial( aspect );
			m_PartAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( m_PartAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( m_PartAIS );

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

					// only add key index to the polygon to save the performance
					if( camData.EdgeStartIndex.Contains( j ) || j % m_nGap == 0 ) {
						polygonMaker.Add( camData.CADPointList[ j ].Point );
						m_VertexMap.Add( camData.CADPointList[ j ].Point, new Tuple<CAMData, int>( camData, j ) );
					}
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
			ShowToolVec();
			ShowOrientation();
			ShowIndex();
			m_OCCViewer.UpdateView();
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
				List<CAMPoint> filteredPath = camData.CAMPointList;
				for( int i = 0; i < filteredPath.Count; i++ ) {
					if( IsKeyToolVecIndex( i, camData, out bool bHL ) ) {
						CAMPoint camPoint = filteredPath[ i ];
						AIS_Line toolVecAIS = GetVecAIS( camPoint.CADPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
						if( bHL ) {
							toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
							toolVecAIS.SetWidth( 4 );
						}
						m_ToolVecAISList.Add( toolVecAIS );
					}
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
				gp_Dir orientationDir = new gp_Dir( camData.CAMPointList[ 0 ].CADPoint.TangentVec.XYZ() );
				if( camData.IsReverse ) {
					orientationDir.Reverse();
				}
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISList.Add( orientationAIS );
			}

			// display the orientation
			foreach( AIS_Shape orientationAIS in m_OrientationAISList ) {
				m_OCCViewer.GetAISContext().Display( orientationAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( orientationAIS );
			}
		}

		void ShowIndex()
		{
			// clear the previous text label
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_OCCViewer.GetAISContext().Remove( textLabel, false );
			}
			m_IndexList.Clear();

			// create text label
			int nCurrentIndex = 0;
			foreach( CAMData camData in m_Model.CAMDataList ) {
				gp_Pnt location = camData.CAMPointList[ 0 ].CADPoint.Point;
				string szIndex = nCurrentIndex++.ToString();

				// create text label ais
				AIS_TextLabel textLabel = new AIS_TextLabel();
				textLabel.SetText( new TCollection_ExtendedString( szIndex ) );
				textLabel.SetPosition( location );
				textLabel.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				textLabel.SetHeight( 20 );
				textLabel.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
				m_IndexList.Add( textLabel );
			}

			// display text label
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				m_OCCViewer.GetAISContext().Display( textLabel, false );
				m_OCCViewer.GetAISContext().Deactivate( textLabel );
			}
		}

		bool IsKeyToolVecIndex( int index, CAMData camData, out bool isToolVecMod )
		{
			// map CAD and CAM point index
			int modifiedIndex = camData.IsReverse
				? ( camData.CAMPointList.Count - 1 - index + camData.StartPoint ) % camData.CAMPointList.Count
				: ( index + camData.StartPoint ) % camData.CAMPointList.Count;

			// need highlight if the index is modified index
			isToolVecMod = camData.GetToolVecModifyIndex().Contains( modifiedIndex );

			// need to show if the index is key index
			bool isEdgeStart = camData.EdgeStartIndex.Contains( modifiedIndex );
			return index % m_nGap == 0 || isEdgeStart || isToolVecMod;
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
			lineAIS.SetWidth( 1 );
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

		// CAM edit action
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

		void m_tsmiToolVec_Click( object sender, EventArgs e )
		{
			editMode = EditMode.ToolVecSelect;
		}

		void m_tsmiLead_Click( object sender, EventArgs e )
		{

		}

		void m_tsmiMoveUp_Click( object sender, EventArgs e )
		{
			GetSelectedWireInfo( out CAMData camData, out AIS_InteractiveObject selectedAIS );
			if( camData == null ) {
				return;
			}
			int nIndex = m_Model.CAMDataList.IndexOf( camData );
			if( nIndex == -1 ) {
				return;
			}
			MoveProcess( nIndex, true );
		}

		void m_tsmiMoveDown_Click( object sender, EventArgs e )
		{
			GetSelectedWireInfo( out CAMData camData, out AIS_InteractiveObject selectedAIS );
			if( camData == null ) {
				return;
			}
			int nIndex = m_Model.CAMDataList.IndexOf( camData );
			if( nIndex == -1 ) {
				return;
			}
			MoveProcess( nIndex, false );
		}

		void m_tsmiSort_Click( object sender, EventArgs e )
		{
			SortProcess();
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			//// path filtering
			//foreach( CAMData camData in m_Model.CAMDataList ) {
			//	List<CAMPoint> filteredPath = PathFiltering( camData.CAMPointList );
			//	camData.CAMPointList.Clear();
			//	camData.CAMPointList.AddRange( filteredPath );

			//	// add the start point to close the loop
			//	camData.CAMPointList.Add( camData.CAMPointList[ 0 ].Clone() );
			//}
			//List<IProcessData> cuttingProcessDataList =
			//	m_Model.CAMDataList.Select( camData => new CuttingProcessData( camData ) ).Cast<IProcessData>().ToList();
			//NCWriter w = new NCWriter( cuttingProcessDataList );
			//w.Convert();
		}

		// viewer action
		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			switch( editMode ) {
				case EditMode.ToolVecSelect:
				case EditMode.StartPoint:
					if( e.KeyCode == Keys.Escape ) {
						editMode = EditMode.None;
					}
					break;
				//case EditMode.TooVecEdit:
				//	if( e.KeyCode != Keys.Escape ) {
				//		return;
				//	}
				//	if( m_SelectedToolVecAx2 == null || m_SelectedCAMData == null || m_SelectedIndex == -1 ) {
				//		return;
				//	}
				//	m_SelectedCAMData.SetToolVecModify( m_SelectedIndex, 0, 0 );
				//	ShowCAMData();
				//	editMode = EditMode.None;

				//	break;
				case EditMode.None:
				default:
					break;
			}
			if( e.KeyCode == Keys.F5 ) {
				m_OCCViewer.AxoView();
				m_OCCViewer.ZoomAllView();
				m_OCCViewer.UpdateView();
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
					GetSelectedVertexInfo( out CAMData camData_StartPoint, out int nIndex_StartPoint );
					if( camData_StartPoint == null || nIndex_StartPoint == -1 ) {
						return;
					}
					camData_StartPoint.StartPoint = nIndex_StartPoint;
					ShowCAMData();

					break;
				case EditMode.ToolVecSelect:
					if( e.Button != MouseButtons.Left ) {
						return;
					}
					m_OCCViewer.Select();
					GetSelectedVertexInfo( out CAMData camData_ToolVec, out int nIndex_ToolVec );
					if( camData_ToolVec == null || nIndex_ToolVec == -1 ) {
						return;
					}

					// show dialog to ask for angle
					camData_ToolVec.GetToolVecModify( nIndex_ToolVec, out double angleA_deg, out double angleB_deg );
					ToolVecForm form = new ToolVecForm( angleA_deg, angleB_deg );
					DialogResult result = form.ShowDialog();
					if( result != DialogResult.OK ) {
						return;
					}
					form.GetAngleValue( out double newAngleA_deg, out double newAnfleB_deg );
					camData_ToolVec.SetToolVecModify( nIndex_ToolVec, newAngleA_deg, newAnfleB_deg );
					ShowCAMData();
					break;
				//case EditMode.TooVecEdit:
				//	if( e.Button != MouseButtons.Left ) {
				//		return;
				//	}
				//	editMode = EditMode.None;

				//	break;
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
				case EditMode.TooVecEdit:
					//	if( m_SelectedToolVecAx2 == null || m_SelectedCAMData == null || m_SelectedIndex == -1 ) {
					//		return;
					//	}

					//	// convert the mouse position to 3D point
					//	double xp = 0;
					//	double yp = 0;
					//	double zp = 0;
					//	m_OCCViewer.Convert( e.X, e.Y, ref xp, ref yp, ref zp );

					//	// make a sphere for direction control
					//	gp_Sphere sphere = new gp_Sphere();
					//	sphere.SetLocation( m_SelectedToolVecAx2.Location() );
					//	sphere.SetRadius( 10 );
					//	Geom_SphericalSurface sphereG = new Geom_SphericalSurface( sphere );

					//	// make line representing view direction
					//	gp_Pnt mousePoint = new gp_Pnt( xp, yp, zp );
					//	gp_Lin viewLine = new gp_Lin( mousePoint, m_OCCViewer.GetViewDir() );
					//	Geom_Line viewLineG = new Geom_Line( viewLine );

					//	// get intersection point of the line and the sphere
					//	GeomAPI_IntCS intCS = new GeomAPI_IntCS( viewLineG, sphereG );
					//	gp_Pnt ps = new gp_Pnt();
					//	if( intCS.NbPoints() == 0 ) {

					//		// get closet point of the line and the sphere center
					//		GeomAPI_ProjectPointOnCurve projectPoint = new GeomAPI_ProjectPointOnCurve( m_SelectedToolVecAx2.Location(), viewLineG );
					//		double u = projectPoint.LowerDistanceParameter();
					//		ps = viewLineG.Value( u );
					//	}
					//	else {

					//		// get the point closet to the mouse point
					//		double dMin = double.MaxValue;
					//		for( int i = 1; i <= intCS.NbPoints(); i++ ) {
					//			gp_Pnt p = intCS.Point( i );
					//			double d = p.Distance( mousePoint );
					//			if( d < dMin ) {
					//				dMin = d;
					//				ps = p;
					//			}
					//		}
					//	}

					//	// get direction of the line
					//	gp_Dir dir = new gp_Dir( ps.XYZ() - m_SelectedToolVecAx2.Location().XYZ() );

					//	// project the vector to the ax2
					//	double X = dir.Dot( m_SelectedToolVecAx2.XDirection() );
					//	double Y = dir.Dot( m_SelectedToolVecAx2.YDirection() );
					//	double Z = dir.Dot( m_SelectedToolVecAx2.Direction() );

					//	// get angle A is atan2 Z/X
					//	double angleA = Math.Atan2( X, Z ) * 180 / Math.PI;

					//	// get angle B is atan2 Z/Y
					//	double angleB = Math.Atan2( Y, Z ) * 180 / Math.PI;
					//	m_SelectedCAMData.SetToolVecModify( m_SelectedIndex, angleA, angleB );
					//	ShowCAMData();
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
			GetSelectedWireInfo( out CAMData camData, out AIS_InteractiveObject selectedAIS );
			switch( editMode ) {
				case EditMode.StartPoint:
					if( camData == null || selectedAIS == null ) {
						editMode = EditMode.None;
						return;
					}

					// activate the selected contour
					m_OCCViewer.GetAISContext().Activate( selectedAIS, (int)AISActiveMode.Vertex );
					DisableAllToolStripMenuItems();
					m_tsmiStartPoint.BackColor = System.Drawing.Color.Yellow;
					break;
				case EditMode.ToolVecSelect:
					if( camData == null || selectedAIS == null ) {
						editMode = EditMode.None;
						return;
					}

					// activate the selected contour
					m_OCCViewer.GetAISContext().Activate( selectedAIS, (int)AISActiveMode.Vertex );
					DisableAllToolStripMenuItems();
					m_tsmiToolVec.BackColor = System.Drawing.Color.Yellow;
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
					EnableAllToolStripMenuItems();
					m_tsmiStartPoint.BackColor = System.Drawing.SystemColors.Control;
					break;
				case EditMode.ToolVecSelect:

					// clear all selected
					m_OCCViewer.GetAISContext().ClearSelected( true );

					// deactivate the contour CAD wire selection
					foreach( AIS_Shape oneShape in m_CADContourAISList ) {
						m_OCCViewer.GetAISContext().Deactivate( oneShape );
					}
					EnableAllToolStripMenuItems();
					m_tsmiToolVec.BackColor = System.Drawing.SystemColors.Control;
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

		void EnableAllToolStripMenuItems()
		{
			m_tsmiStartPoint.Enabled = true;
			m_tsmiReverse.Enabled = true;
			m_tsmiToolVec.Enabled = true;
			m_tsmiLead.Enabled = true;
			m_tsmiMoveUp.Enabled = true;
			m_tsmiMoveDown.Enabled = true;
			m_tsmiOK.Enabled = true;
		}

		void DisableAllToolStripMenuItems()
		{
			m_tsmiStartPoint.Enabled = false;
			m_tsmiReverse.Enabled = false;
			m_tsmiToolVec.Enabled = false;
			m_tsmiLead.Enabled = false;
			m_tsmiMoveUp.Enabled = false;
			m_tsmiMoveDown.Enabled = false;
			m_tsmiOK.Enabled = false;
		}

		// path filtering
		List<CAMPoint> PathFiltering( List<CAMPoint> camPointList )
		{
			List<CAMPoint> filteredList = new List<CAMPoint>();

			// filtering by location
			List<gp_Pnt> path = camPointList.Select( camPoint => camPoint.CADPoint.Point ).ToList();
			bool[] flagsL = SimplifyPathByLocation( path, 0.01 );

			// filtering by orientation
			List<gp_Dir> orientation = camPointList.Select( camPoint => camPoint.ToolVec ).ToList();
			bool[] flagsO = SimplifyPathByOrientation( orientation, 1 );

			// combine the two filtering results
			for( int i = 0; i < path.Count; i++ ) {
				if( !flagsL[ i ] || !flagsO[ i ] ) {
					filteredList.Add( camPointList[ i ] );
				}
			}
			return filteredList;
		}

		public static bool[] SimplifyPathByLocation( List<gp_Pnt> path, double epsilon, bool isClosedPath = true )
		{
			int len = path.Count, high = len - 1;
			double epsSqr = epsilon * epsilon;

			// Not enough points to simplify
			if( len < 4 ) {
				return new bool[ len ];
			}

			bool[] flags = new bool[ len ];
			double[] dsq = new double[ len ];
			int curr = 0;

			// Compute initial distances
			if( isClosedPath ) {
				dsq[ 0 ] = PerpendicDistFromLineSqrd( path[ 0 ], path[ high ], path[ 1 ] );
				dsq[ high ] = PerpendicDistFromLineSqrd( path[ high ], path[ 0 ], path[ high - 1 ] );
			}
			else {
				dsq[ 0 ] = double.MaxValue;
				dsq[ high ] = double.MaxValue;
			}

			for( int i = 1; i < high; ++i ) {
				dsq[ i ] = PerpendicDistFromLineSqrd( path[ i ], path[ i - 1 ], path[ i + 1 ] );
			}

			// Main simplification loop
			while( true ) {
				if( dsq[ curr ] > epsSqr ) {
					int start = curr;
					do {
						curr = GetNext( curr, high, ref flags );
					} while( curr != start && dsq[ curr ] > epsSqr );
					if( curr == start ) {
						break;
					}
				}

				int prev = GetPrior( curr, high, ref flags );
				int next = GetNext( curr, high, ref flags );
				if( next == prev ) {
					break;
				}

				int prior2;
				if( dsq[ next ] < dsq[ curr ] ) {
					prior2 = prev;
					prev = curr;
					curr = next;
					next = GetNext( next, high, ref flags );
				}
				else {
					prior2 = GetPrior( prev, high, ref flags );
				}

				flags[ curr ] = true;
				curr = next;
				next = GetNext( next, high, ref flags );

				if( isClosedPath || ( ( curr != high ) && ( curr != 0 ) ) ) {
					dsq[ curr ] = PerpendicDistFromLineSqrd( path[ curr ], path[ prev ], path[ next ] );
				}
				if( isClosedPath || ( ( prev != 0 ) && ( prev != high ) ) ) {
					dsq[ prev ] = PerpendicDistFromLineSqrd( path[ prev ], path[ prior2 ], path[ curr ] );
				}
			}
			return flags;
		}

		public static bool[] SimplifyPathByOrientation( List<gp_Dir> toolpath, double angleEpsilon, bool isClosedPath = true )
		{
			int len = toolpath.Count, high = len - 1;
			double epsSqr = angleEpsilon * angleEpsilon;

			// Not enough orientations to simplify
			if( len < 4 ) {
				return new bool[ len ];
			}

			bool[] flags = new bool[ len ];
			double[] dsq = new double[ len ];
			int curr = 0;

			// Compute initial angular deviations
			if( isClosedPath ) {
				dsq[ 0 ] = AngularDeviationSqrd( toolpath[ 0 ], toolpath[ high ], toolpath[ 1 ] );
				dsq[ high ] = AngularDeviationSqrd( toolpath[ high ], toolpath[ 0 ], toolpath[ high - 1 ] );
			}
			else {
				dsq[ 0 ] = double.MaxValue;
				dsq[ high ] = double.MaxValue;
			}

			for( int i = 1; i < high; ++i ) {
				dsq[ i ] = AngularDeviationSqrd( toolpath[ i ], toolpath[ i - 1 ], toolpath[ i + 1 ] );
			}

			// Main simplification loop
			while( true ) {
				if( dsq[ curr ] > epsSqr ) {
					int start = curr;
					do {
						curr = GetNext( curr, high, ref flags );
					} while( curr != start && dsq[ curr ] > epsSqr );
					if( curr == start ) {
						break;
					}
				}

				int prev = GetPrior( curr, high, ref flags );
				int next = GetNext( curr, high, ref flags );
				if( next == prev ) {
					break;
				}

				int prior2;
				if( dsq[ next ] < dsq[ curr ] ) {
					prior2 = prev;
					prev = curr;
					curr = next;
					next = GetNext( next, high, ref flags );
				}
				else {
					prior2 = GetPrior( prev, high, ref flags );
				}

				flags[ curr ] = true;
				curr = next;
				next = GetNext( next, high, ref flags );

				if( isClosedPath || ( ( curr != high ) && ( curr != 0 ) ) ) {
					dsq[ curr ] = AngularDeviationSqrd( toolpath[ curr ], toolpath[ prev ], toolpath[ next ] );
				}
				if( isClosedPath || ( ( prev != 0 ) && ( prev != high ) ) ) {
					dsq[ prev ] = AngularDeviationSqrd( toolpath[ prev ], toolpath[ prior2 ], toolpath[ curr ] );
				}
			}
			return flags;
		}

		private static double AngularDeviationSqrd( gp_Dir v, gp_Dir v1, gp_Dir v2 )
		{
			if( v.IsParallel( v1, 1e-6 ) && v1.IsParallel( v2, 1e-6 ) && v2.IsParallel( v, 1e-6 ) ) {
				return 0;
			}
			else {
				return double.MaxValue;
			}
		}

		private static double PerpendicDistFromLineSqrd( gp_Pnt pt, gp_Pnt line1, gp_Pnt line2 )
		{
			double ax = pt.X() - line1.X();
			double ay = pt.Y() - line1.Y();
			double az = pt.Z() - line1.Z();

			double bx = line2.X() - line1.X();
			double by = line2.Y() - line1.Y();
			double bz = line2.Z() - line1.Z();

			// Cross product of (P - A) and (B - A)
			double crossX = ay * bz - az * by;
			double crossY = az * bx - ax * bz;
			double crossZ = ax * by - ay * bx;

			// Squared magnitude of cross product
			double crossMagSq = crossX * crossX + crossY * crossY + crossZ * crossZ;

			// Squared magnitude of (B - A)
			double lineMagSq = bx * bx + by * by + bz * bz;
			if( lineMagSq == 0 ) {
				return 0; // Avoid division by zero (degenerate case)
			}
			return crossMagSq / lineMagSq;
		}

		private static int GetNext( int current, int high, ref bool[] flags )
		{
			++current;
			while( current <= high && flags[ current ] ) {
				++current;
			}
			if( current <= high ) {
				return current;
			}
			current = 0;
			while( flags[ current ] ) {
				++current;
			}
			return current;
		}

		private static int GetPrior( int current, int high, ref bool[] flags )
		{
			if( current == 0 ) {
				current = high;
			}
			else {
				--current;
			}
			while( current > 0 && flags[ current ] ) {
				--current;
			}
			if( !flags[ current ] ) {
				return current;
			}
			current = high;
			while( flags[ current ] ) {
				--current;
			}
			return current;
		}

		// sorting
		void MoveProcess( int nIndex, bool bUp )
		{
			if( nIndex < 0 || nIndex > m_Model.CAMDataList.Count - 1
				|| bUp && nIndex == 0
				|| !bUp && nIndex == m_Model.CAMDataList.Count - 1 ) {
				return;
			}
			CAMData data = m_Model.CAMDataList[ nIndex ];
			m_Model.CAMDataList.RemoveAt( nIndex );
			if( bUp ) {
				m_Model.CAMDataList.Insert( nIndex - 1, data );
			}
			else {
				m_Model.CAMDataList.Insert( nIndex + 1, data );
			}
			ShowCAMData();
		}

		void SortProcess()
		{
			List<CAMData> sortResult = new List<CAMData>();

			// the initial point is assume to be <0,0,0>
			gp_Pnt startPoint = new gp_Pnt( 0, 0, 0 );

			// sort the process using shortest distance algorithm greedily
			while( m_Model.CAMDataList.Count > 0 ) {
				double minDistance = double.MaxValue;
				int minIndex = -1;
				for( int i = 0; i < m_Model.CAMDataList.Count; i++ ) {
					CAMData oneCAMData = m_Model.CAMDataList[ i ];
					double distance = startPoint.Distance( oneCAMData.CAMPointList[ 0 ].CADPoint.Point );
					if( distance < minDistance ) {
						minDistance = distance;
						minIndex = i;
					}
				}
				if( minIndex >= 0 ) {

					// update the start point
					CAMData shortestDisCAMData = m_Model.CAMDataList[ minIndex ];
					startPoint = shortestDisCAMData.CAMPointList[ 0 ].CADPoint.Point;
					sortResult.Add( shortestDisCAMData );
					m_Model.CAMDataList.RemoveAt( minIndex );
				}
			}
			m_Model.CAMDataList.Clear();
			m_Model.CAMDataList.AddRange( sortResult );
			ShowCAMData();
		}
	}
}
