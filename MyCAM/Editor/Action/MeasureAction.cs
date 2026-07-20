using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRep;
using OCC.BRepGProp;
using OCC.gp;
using OCC.GProp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal enum EMeasureType
	{
		None = -1,
		PointCoordinate = 0,
		LineLength = 1,
		CircleArcRadius = 2,
		DistanceBetweenTwoPoints = 3,
		AngleBetweenLines = 4,
	}

	internal class MeasureAction : KeyMouseActionBase
	{
		public MeasureAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.Measure;
			}
		}

		public override void Start()
		{
			base.Start();

			// disable tree view
			m_TreeView.Enabled = false;

			// create and show dialog
			ShowMeasureDialog();
		}

		public override void End()
		{
			// unsubscribe events and close dialog
			if( m_MeasureDlg != null && !m_MeasureDlg.IsDisposed ) {
				m_MeasureDlg.MeasureMethodChanged = null;
				m_MeasureDlg.Cancel = null;
				m_MeasureDlg.Close();
			}
			m_MeasureDlg = null;

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// enable tree view
			m_TreeView.Enabled = true;

			// deactivate
			m_ViewManager.DeactiveAll();
			m_Viewer.UpdateView();
			base.End();
		}

		public void SwitchMeasureMethod( EMeasureType type )
		{
			if( type == m_CurrentMode ) {
				return;
			}
			m_CurrentMode = type;

			// clear the first-click state kept for two-click measures
			m_FirstShape = null;

			// clear selection when mode changes
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.GetAISContext().UpdateCurrentViewer();

			// deactivate all workpiece shapes first
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_ViewManager.DeactivePart( partID );
			}

			// activate workpiece with the corresponding selectable sub-shape mode
			AISActiveMode workpieceMode;
			switch( type ) {
				case EMeasureType.PointCoordinate:
				case EMeasureType.DistanceBetweenTwoPoints:
					workpieceMode = AISActiveMode.Vertex;
					break;
				case EMeasureType.LineLength:
				case EMeasureType.AngleBetweenLines:
				case EMeasureType.CircleArcRadius:
					workpieceMode = AISActiveMode.Edge;
					break;
				default:
					return;
			}

			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)workpieceMode );
			}
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left || !m_Viewer.GetAISContext().HasDetected() ) {
				return;
			}

			TopoDS_Shape clickedShape = GetClickedShape();
			if( clickedShape == null || clickedShape.IsNull() ) {
				return;
			}

			ExecuteMeasureHandle( clickedShape );
		}

		TopoDS_Shape GetClickedShape()
		{
			// for two-step modes, use Add to accumulate selection so both picks stay highlighted; otherwise Replace
			bool isSecondClickOfTwoStep = IsTwoStepMode && m_FirstShape != null;
			AIS_SelectionScheme scheme = isSecondClickOfTwoStep ? AIS_SelectionScheme.AIS_SelectionScheme_Add : AIS_SelectionScheme.AIS_SelectionScheme_Replace;

			m_Viewer.Select( scheme );
			if( m_Viewer.GetAISContext().NbSelected() == 0 ) {
				return null;
			}

			// find the shape the user just clicked. In Add mode there may be multiple selected shapes;
			// pick the one that is not the first (previously stored) shape.
			for( m_Viewer.GetAISContext().InitSelected(); m_Viewer.GetAISContext().MoreSelected(); m_Viewer.GetAISContext().NextSelected() ) {
				TopoDS_Shape SelectedShape = m_Viewer.GetAISContext().SelectedShape();
				if( SelectedShape == null || SelectedShape.IsNull() ) {
					continue;
				}
				if( isSecondClickOfTwoStep && m_FirstShape != null && SelectedShape.IsSame( m_FirstShape ) ) {
					continue;
				}
				return SelectedShape;
			}
			return null;
		}

		void ExecuteMeasureHandle( TopoDS_Shape clickedShape )
		{
			switch( m_CurrentMode ) {
				case EMeasureType.PointCoordinate:
					HandlePointCoordinate( clickedShape );
					break;
				case EMeasureType.LineLength:
					HandleLineLength( clickedShape );
					break;
				case EMeasureType.CircleArcRadius:
					HandleCircleArcRadius( clickedShape );
					break;
				case EMeasureType.DistanceBetweenTwoPoints:
					HandleDistanceBetweenTwoPoints( clickedShape );
					break;
				case EMeasureType.AngleBetweenLines:
					HandleAngleBetweenLines( clickedShape );
					break;
			}
		}

		void HandlePointCoordinate( TopoDS_Shape clickedShape )
		{
			if( clickedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return;
			}
			TopoDS_Vertex vertex = TopoDS.ToVertex( clickedShape );
			gp_Pnt point = BRep_Tool.Pnt( vertex );
			ShowPointCoordinate( point );
		}

		void HandleLineLength( TopoDS_Shape clickedShape )
		{
			if( clickedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				return;
			}
			TopoDS_Edge edge = TopoDS.ToEdge( clickedShape );
			GProp_GProps system = new GProp_GProps();
			BRepGProp.LinearProperties( edge, ref system );
			double dLength = system.Mass();
			ShowLineLength( dLength );
		}

		void HandleCircleArcRadius( TopoDS_Shape clickedShape )
		{
			if( clickedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				return;
			}
			TopoDS_Edge edge = TopoDS.ToEdge( clickedShape );
			bool isValidCircle = GeometryTool.IsCircularArc( edge, out _, out double dRadius, out _ );
			if( !isValidCircle ) {
				ShowResultText( m_LanguageManager.TryGetString( "NotCircularArc", NOT_CIRCULAR_ARC ) );
				return;
			}
			ShowCircleArcRadius( dRadius );
		}

		void HandleDistanceBetweenTwoPoints( TopoDS_Shape clickedShape )
		{
			if( clickedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return;
			}

			// first click: remember the vertex and wait for the second click
			if( m_FirstShape == null ) {
				m_FirstShape = clickedShape;
				ShowResultText( m_LanguageManager.TryGetString( "FirstPointSelected", FIRST_POINT_SELECTED ) );
				return;
			}

			// first shape must be a vertex
			if( m_FirstShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				m_FirstShape = null;
				return;
			}

			// second click: compute the distance between the two selected points
			TopoDS_Vertex vertex1 = TopoDS.ToVertex( m_FirstShape );
			TopoDS_Vertex vertex2 = TopoDS.ToVertex( clickedShape );
			m_FirstShape = null;

			gp_Pnt point1 = BRep_Tool.Pnt( vertex1 );
			gp_Pnt point2 = BRep_Tool.Pnt( vertex2 );
			double dDistance = point1.Distance( point2 );
			ShowDistance( dDistance );
		}

		void HandleAngleBetweenLines( TopoDS_Shape clickedShape )
		{
			if( clickedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				return;
			}

			// first click: remember the edge and wait for the second click
			if( m_FirstShape == null ) {
				m_FirstShape = clickedShape;
				ShowResultText( m_LanguageManager.TryGetString( "FirstLineSelected", FIRST_LINE_SELECTED ) );
				return;
			}

			// first shape must be an edge
			if( m_FirstShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_EDGE ) {
				m_FirstShape = null;
				return;
			}

			TopoDS_Edge edge1 = TopoDS.ToEdge( m_FirstShape );
			TopoDS_Edge edge2 = TopoDS.ToEdge( clickedShape );
			m_FirstShape = null;

			bool isLine1 = GeometryTool.IsLine( edge1, out gp_Pnt p1, out gp_Dir dir1 );
			bool isLine2 = GeometryTool.IsLine( edge2, out gp_Pnt p2, out gp_Dir dir2 );
			if( !isLine1 || !isLine2 ) {
				m_Viewer.GetAISContext().ClearSelected( true );
				ShowResultText( m_LanguageManager.TryGetString( "NotStraightLine", NOT_STRAIGHT_LINE ) );
				return;
			}

			// two parallel lines or two lines that never meet (skew, non-coplanar) cannot form a valid angle
			if( !AreLinesIntersect( p1, dir1, p2, dir2 ) ) {
				m_Viewer.GetAISContext().ClearSelected( true );
				ShowResultText( m_LanguageManager.TryGetString( "LinesNotIntersect", LINES_NOT_INTERSECT ) );
				return;
			}

			gp_Vec vec1 = new gp_Vec( dir1 );
			gp_Vec vec2 = new gp_Vec( dir2 );
			double dAngleRad = vec1.Angle( vec2 );

			// two lines form an angle between 0 and 90 degrees regardless of direction
			if( dAngleRad > System.Math.PI / 2 ) {
				dAngleRad = System.Math.PI - dAngleRad;
			}
			double dAngleDeg = dAngleRad * 180.0 / System.Math.PI;
			ShowAngle( dAngleDeg );
		}

		// two lines intersect (or are the same line) when the shortest distance between them is (nearly) zero
		static bool AreLinesIntersect( gp_Pnt p1, gp_Dir dir1, gp_Pnt p2, gp_Dir dir2 )
		{
			// direction vectors are unit vectors, so |v1 x v2| = sin(angle)
			const double PARALLEL_TOLERANCE = 1e-6;
			// distance tolerance in model units (mm)
			const double DISTANCE_TOLERANCE = 1e-3;

			gp_Vec v1 = new gp_Vec( dir1 );
			gp_Vec v2 = new gp_Vec( dir2 );
			gp_Vec w0 = new gp_Vec( p1, p2 );

			gp_Vec cross = v1.Crossed( v2 );
			double dCrossMag = cross.Magnitude();

			// parallel lines: never intersect unless they are the exact same line, which cannot form an angle either way
			if( dCrossMag < PARALLEL_TOLERANCE ) {
				return false;
			}

			// shortest distance between two skew lines: |w0 . (v1 x v2)| / |v1 x v2|
			double dDistance = System.Math.Abs( w0.Dot( cross ) ) / dCrossMag;
			return dDistance < DISTANCE_TOLERANCE;
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		void ShowMeasureDialog()
		{
			m_MeasureDlg = new MeasureDlg();

			m_MeasureDlg.MeasureMethodChanged += ( type ) =>
			{
				SwitchMeasureMethod( type );
			};

			m_MeasureDlg.Cancel += () =>
			{
				End();
			};

			m_MeasureDlg.Initialize();
			m_MeasureDlg.Show( MyApp.MainForm );
		}

		void ShowPointCoordinate( gp_Pnt point )
		{
			string format = m_LanguageManager.TryGetString( "FormatPointCoordinate", FORMAT_POINT_COORDINATE );
			string result = string.Format( format, point.X(), point.Y(), point.Z() );
			ShowResultText( result );
		}

		void ShowLineLength( double length )
		{
			string format = m_LanguageManager.TryGetString( "FormatLineLength", FORMAT_LINE_LENGTH );
			string result = string.Format( format, length );
			ShowResultText( result );
		}

		void ShowCircleArcRadius( double radius )
		{
			string format = m_LanguageManager.TryGetString( "FormatCircleRadius", FORMAT_CIRCLE_RADIUS );
			string result = string.Format( format, radius );
			ShowResultText( result );
		}

		void ShowDistance( double distance )
		{
			string format = m_LanguageManager.TryGetString( "FormatDistance", FORMAT_DISTANCE );
			string result = string.Format( format, distance );
			ShowResultText( result );
		}

		void ShowAngle( double angleDeg )
		{
			string format = m_LanguageManager.TryGetString( "FormatAngle", FORMAT_ANGLE );
			string result = string.Format( format, angleDeg );
			ShowResultText( result );
		}

		void ShowResultText( string text )
		{
			if( m_MeasureDlg == null || m_MeasureDlg.IsDisposed ) {
				return;
			}
			m_MeasureDlg.ShowResult( text );
		}

		bool IsTwoStepMode
		{
			get
			{
				return m_CurrentMode == EMeasureType.DistanceBetweenTwoPoints ||
					   m_CurrentMode == EMeasureType.AngleBetweenLines;
			}
		}

		EMeasureType m_CurrentMode = EMeasureType.None;
		MeasureDlg m_MeasureDlg;
		TopoDS_Shape m_FirstShape;

		const string FORMAT_POINT_COORDINATE = "X: {0:F3}mm\r\nY: {1:F3}mm\r\nZ: {2:F3}mm";
		const string FORMAT_LINE_LENGTH = "長度: {0:F3}mm";
		const string FORMAT_CIRCLE_RADIUS = "半徑: {0:F3}mm";
		const string FORMAT_DISTANCE = "距離: {0:F3}mm";
		const string FORMAT_ANGLE = "夾角: {0:F3}°";
		const string NOT_CIRCULAR_ARC = "選取的邊不是圓或圓弧";
		const string FIRST_POINT_SELECTED = "已選取第一個點，請選取第二個點";
		const string FIRST_LINE_SELECTED = "已選取第一條線，請選取第二條線";
		const string NOT_STRAIGHT_LINE = "選取的邊不是直線";
		const string LINES_NOT_INTERSECT = "兩條線不相交，無法計算夾角";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "MeasureAction" );
	}
}
