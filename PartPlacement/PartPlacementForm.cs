using OCC.AIS;
using OCC.BRepAdaptor;
using OCC.BRepBuilderAPI;
using OCC.GeomAbs;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Windows.Forms;

namespace PartPlacement
{
	public partial class PartPlacementForm : Form
	{
		public Action<gp_Trsf> PlaceOK;

		public PartPlacementForm( TopoDS_Shape partShape )
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

			// show part
			m_RawPartShape = partShape;
			if( m_RawPartShape == null ) {
				return;
			}
			RefreshViewer();

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.MouseUp += ViewerMouseUp;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;
			m_panViewer.MouseMove += ViewerMouseMove;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// raw part
		TopoDS_Shape m_RawPartShape;

		// G54
		TopoDS_Face m_G54PlaneXY;
		TopoDS_Face m_G54PlaneYZ;
		TopoDS_Face m_G54PlaneXZ;

		void ShowG54Coord()
		{
			// make 3 plane representing XY YZ XZ
			const int SIZE = 10;
			gp_Pnt p1 = new gp_Pnt( SIZE, SIZE, 0 );
			gp_Pnt p2 = new gp_Pnt( -SIZE, SIZE, 0 );
			gp_Pnt p3 = new gp_Pnt( -SIZE, -SIZE, 0 );
			gp_Pnt p4 = new gp_Pnt( SIZE, -SIZE, 0 );
			BRepBuilderAPI_MakeEdge edge12 = new BRepBuilderAPI_MakeEdge( p1, p2 );
			BRepBuilderAPI_MakeEdge edge23 = new BRepBuilderAPI_MakeEdge( p2, p3 );
			BRepBuilderAPI_MakeEdge edge34 = new BRepBuilderAPI_MakeEdge( p3, p4 );
			BRepBuilderAPI_MakeEdge edge41 = new BRepBuilderAPI_MakeEdge( p4, p1 );
			BRepBuilderAPI_MakeWire wire = new BRepBuilderAPI_MakeWire( edge12.Edge(), edge23.Edge(), edge34.Edge(), edge41.Edge() );
			BRepBuilderAPI_MakeFace faceXY = new BRepBuilderAPI_MakeFace( wire.Wire() );

			// XY plane
			m_G54PlaneXY = faceXY.Face();

			// YZ plane
			gp_Trsf trsfYZ = new gp_Trsf();
			trsfYZ.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 1, 0 ) ), Math.PI / 2 );
			BRepBuilderAPI_Transform trasformYZ = new BRepBuilderAPI_Transform( faceXY.Face(), trsfYZ );
			m_G54PlaneYZ = TopoDS.ToFace( trasformYZ.Shape() );

			// XZ plane
			gp_Trsf trsfXZ = new gp_Trsf();
			trsfXZ.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 1, 0, 0 ) ), Math.PI / 2 );
			BRepBuilderAPI_Transform trasformXZ = new BRepBuilderAPI_Transform( faceXY.Face(), trsfXZ );
			m_G54PlaneXZ = TopoDS.ToFace( trasformXZ.Shape() );

			// display XY
			AIS_Shape aisXY = new AIS_Shape( m_G54PlaneXY );
			aisXY.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisXY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			aisXY.SetTransparency( 0.9f );
			m_OCCViewer.GetAISContext().Display( aisXY, false );

			// display YZ
			AIS_Shape aisYZ = new AIS_Shape( m_G54PlaneYZ );
			aisYZ.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisYZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			aisYZ.SetTransparency( 0.9f );
			m_OCCViewer.GetAISContext().Display( aisYZ, false );

			// display XZ
			AIS_Shape aisXZ = new AIS_Shape( m_G54PlaneXZ );
			aisXZ.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisXZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			aisXZ.SetTransparency( 0.9f );
			m_OCCViewer.GetAISContext().Display( aisXZ, false );
		}

		void ShowPart()
		{
			// create part AIS_Shape
			AIS_Shape partAIS = new AIS_Shape( m_RawPartShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			partAIS.SetMaterial( aspect );
			partAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );

			// display the shape
			m_OCCViewer.GetAISContext().Display( partAIS, false );
			m_OCCViewer.GetAISContext().Activate( partAIS, (int)AISActiveMode.Face );
			m_OCCViewer.GetAISContext().Activate( partAIS, (int)AISActiveMode.Edge );
		}

		void RefreshViewer()
		{
			m_OCCViewer.GetAISContext().RemoveAll( false );
			ShowPart();
			ShowG54Coord();
			m_OCCViewer.UpdateView();
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}

		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			PlaceOK?.Invoke( new gp_Trsf() );
		}

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			m_panViewer.Focus();
			if( e.Button == MouseButtons.Left ) {
				m_OCCViewer.ShiftSelect();
			}
		}

		bool IsObjectPlane( TopoDS_Shape shape, out gp_Pnt p, out gp_Dir d )
		{
			p = new gp_Pnt();
			d = new gp_Dir();
			TopExp_Explorer explorer = new TopExp_Explorer( m_RawPartShape, TopAbs_ShapeEnum.TopAbs_FACE );
			while( explorer.More() ) {
				if( shape.IsEqual( explorer.Current() ) ) {
					BRepAdaptor_Surface surface = new BRepAdaptor_Surface( TopoDS.ToFace( shape ) );
					if( surface.GetSurfaceType() == GeomAbs_SurfaceType.GeomAbs_Plane ) {
						p = surface.Plane().Location();
						d = surface.Plane().Axis().Direction();
						return true;
					}
				}
				explorer.Next();
			}
			return false;
		}

		bool IsReferencePlane( TopoDS_Shape shape, out gp_Pnt p, out gp_Dir d )
		{
			p = new gp_Pnt();
			d = new gp_Dir();
			if( shape.IsEqual( m_G54PlaneXY ) ) {
				p = new gp_Pnt( 0, 0, 0 );
				d = new gp_Dir( 0, 0, 1 );
				return true;
			}
			else if( shape.IsEqual( m_G54PlaneYZ ) ) {
				p = new gp_Pnt( 0, 0, 0 );
				d = new gp_Dir( 1, 0, 0 );
				return true;
			}
			else if( shape.IsEqual( m_G54PlaneXZ ) ) {
				p = new gp_Pnt( 0, 0, 0 );
				d = new gp_Dir( 0, 1, 0 );
				return true;
			}
			return false;
		}

		bool IsValidSelection( out gp_Pnt pO, out gp_Dir dO, out gp_Pnt pR, out gp_Dir dR )
		{
			pO = new gp_Pnt();
			dO = new gp_Dir();
			pR = new gp_Pnt();
			dR = new gp_Dir();
			m_OCCViewer.GetAISContext().InitSelected();
			TopoDS_Shape[] shapes = new TopoDS_Shape[ 2 ];
			int count = 0;
			while( m_OCCViewer.GetAISContext().MoreSelected() ) {
				if( count >= 2 ) {
					return false;
				}
				shapes[ count ] = m_OCCViewer.GetAISContext().SelectedShape();
				count++;
				m_OCCViewer.GetAISContext().NextSelected();
			}
			if( count != 2 ) {
				return false;
			}
			if( IsObjectPlane( shapes[ 0 ], out pO, out dO ) && IsReferencePlane( shapes[ 1 ], out pR, out dR )
				|| IsObjectPlane( shapes[ 1 ], out pO, out dO ) && IsReferencePlane( shapes[ 0 ], out pR, out dR ) ) {
				return true;
			}
			return false;
		}

		void ViewerMouseUp( object sender, MouseEventArgs e )
		{
		}

		void ViewerMouseMove( object sender, MouseEventArgs e )
		{
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				m_OCCViewer.GetAISContext().ClearSelected( true );
			}
			else if( e.KeyCode == Keys.Enter || e.KeyCode == Keys.R ) {
				if( !IsValidSelection( out gp_Pnt pO, out gp_Dir dO, out gp_Pnt pR, out gp_Dir dR ) ) {
					return;
				}
				if( e.KeyCode == Keys.R ) {
					dR.Reverse();
				}

				// calculate the rotation to make the object plane parallel with the reference plane
				gp_Trsf trsfR = new gp_Trsf();
				gp_Vec vO = new gp_Vec( dO );
				gp_Vec vR = new gp_Vec( dR );
				gp_Quaternion q = new gp_Quaternion( vO, vR );
				trsfR.SetRotation( q );

				// calculate the translation to make the object plane coincident with the reference plane
				gp_Trsf trsfT = new gp_Trsf();
				gp_Pnt p1 = pO.Transformed( trsfR );
				gp_Vec dP = new gp_Vec( p1, pR );
				gp_Vec move = new gp_Vec( dR );
				move.Scale( dP.Dot( move ) );
				trsfT.SetTranslation( move );
				gp_Trsf T = trsfT.Multiplied( trsfR );
				BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( m_RawPartShape, T );
				m_RawPartShape = transform.Shape();
				m_OCCViewer.GetAISContext().ClearSelected( false );
				RefreshViewer();
			}
		}
	}
}
