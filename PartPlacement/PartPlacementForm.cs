using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
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
			MakeG54Coord();
			RefreshViewer();
			DisableAllConstraintMenu();

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
		TopoDS_Shape m_G54Shape;
		List<AIS_Shape> m_G54AISList;

		void MakeG54Coord()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			m_G54AISList = new List<AIS_Shape>();

			// XY plane
			const int SIZE = 20;
			gp_Pnt p1 = new gp_Pnt( SIZE / 2, SIZE / 2, 0 );
			gp_Pnt p2 = new gp_Pnt( -SIZE / 2, SIZE / 2, 0 );
			gp_Pnt p3 = new gp_Pnt( -SIZE / 2, -SIZE / 2, 0 );
			gp_Pnt p4 = new gp_Pnt( SIZE / 2, -SIZE / 2, 0 );
			BRepBuilderAPI_MakeEdge edge12 = new BRepBuilderAPI_MakeEdge( p1, p2 );
			BRepBuilderAPI_MakeEdge edge23 = new BRepBuilderAPI_MakeEdge( p2, p3 );
			BRepBuilderAPI_MakeEdge edge34 = new BRepBuilderAPI_MakeEdge( p3, p4 );
			BRepBuilderAPI_MakeEdge edge41 = new BRepBuilderAPI_MakeEdge( p4, p1 );
			BRepBuilderAPI_MakeWire wire = new BRepBuilderAPI_MakeWire( edge12.Edge(), edge23.Edge(), edge34.Edge(), edge41.Edge() );
			BRepBuilderAPI_MakeFace faceXY = new BRepBuilderAPI_MakeFace( wire.Wire() );
			shapeList.Add( faceXY.Face() );

			// YZ plane
			gp_Trsf trsfYZ = new gp_Trsf();
			trsfYZ.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 1, 0 ) ), Math.PI / 2 );
			BRepBuilderAPI_Transform trasformYZ = new BRepBuilderAPI_Transform( faceXY.Face(), trsfYZ );
			shapeList.Add( trasformYZ.Shape() );

			// XZ plane
			gp_Trsf trsfXZ = new gp_Trsf();
			trsfXZ.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( -1, 0, 0 ) ), Math.PI / 2 );
			BRepBuilderAPI_Transform trasformXZ = new BRepBuilderAPI_Transform( faceXY.Face(), trsfXZ );
			shapeList.Add( trasformXZ.Shape() );

			// X axis
			gp_Pnt pX0 = new gp_Pnt( 0, 0, 0 );
			gp_Pnt pX1 = new gp_Pnt( SIZE, 0, 0 );
			BRepBuilderAPI_MakeEdge edgeX = new BRepBuilderAPI_MakeEdge( pX0, pX1 );
			shapeList.Add( edgeX.Edge() );

			// Y axis
			gp_Pnt pY0 = new gp_Pnt( 0, 0, 0 );
			gp_Pnt pY1 = new gp_Pnt( 0, SIZE, 0 );
			BRepBuilderAPI_MakeEdge edgeY = new BRepBuilderAPI_MakeEdge( pY0, pY1 );
			shapeList.Add( edgeY.Edge() );

			// Z axis
			gp_Pnt pZ0 = new gp_Pnt( 0, 0, 0 );
			gp_Pnt pZ1 = new gp_Pnt( 0, 0, SIZE );
			BRepBuilderAPI_MakeEdge edgeZ = new BRepBuilderAPI_MakeEdge( pZ0, pZ1 );
			shapeList.Add( edgeZ.Edge() );

			// make G54 shape
			m_G54Shape = ShapeTool.MakeCompound( shapeList );

			// XY AIS
			AIS_Shape aisXY = new AIS_Shape( faceXY.Face() );
			aisXY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			m_G54AISList.Add( aisXY );

			// YZ AIS
			AIS_Shape aisYZ = new AIS_Shape( trasformYZ.Shape() );
			aisYZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_G54AISList.Add( aisYZ );

			// XZ AIS
			AIS_Shape aisXZ = new AIS_Shape( trasformXZ.Shape() );
			aisXZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_G54AISList.Add( aisXZ );

			// X AIS
			AIS_Shape aisX = new AIS_Shape( edgeX.Edge() );
			aisX.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_G54AISList.Add( aisX );

			// Y AIS
			AIS_Shape aisY = new AIS_Shape( edgeY.Edge() );
			aisY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_G54AISList.Add( aisY );

			// Z AIS
			AIS_Shape aisZ = new AIS_Shape( edgeZ.Edge() );
			aisZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			m_G54AISList.Add( aisZ );

			// set properties
			foreach( AIS_Shape ais in m_G54AISList ) {
				ais.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
				ais.SetTransparency( 0.9f );
			}
		}

		void ShowG54Coord()
		{
			foreach( AIS_Shape ais in m_G54AISList ) {
				m_OCCViewer.GetAISContext().Display( ais, false );
			}
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
				UpdateConstraintMenu();
			}
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
		}

		void UpdateConstraintMenu()
		{
			DisableAllConstraintMenu();
			GetRefAndMoveObject( out TopoDS_Shape refShape, out TopoDS_Shape moveShape );
			if( refShape == null || moveShape == null ) {
				return;
			}
			AxialConstraint ac = new AxialConstraint( refShape, moveShape, false );
			if( ac.IsValid() ) {
				m_tsmiAxial.Enabled = true;
				m_tsmiAxial_R.Enabled = true;
			}
			AxialParallelConstraint apc = new AxialParallelConstraint( refShape, moveShape, false );
			if( apc.IsValid() ) {
				m_tsmiAxialPar.Enabled = true;
				m_tsmiAxialPar_R.Enabled = true;
			}
			PlaneConstraint pc = new PlaneConstraint( refShape, moveShape, false );
			if( pc.IsValid() ) {
				m_tsmiPlane.Enabled = true;
				m_tsmiPlane_R.Enabled = true;
			}
			PlaneParallelConstraint ppc = new PlaneParallelConstraint( refShape, moveShape, false );
			if( ppc.IsValid() ) {
				m_tsmiPlanePar.Enabled = true;
				m_tsmiPlanePar_R.Enabled = true;
			}
		}

		void GetRefAndMoveObject( out TopoDS_Shape refShape, out TopoDS_Shape moveShape )
		{
			refShape = null;
			moveShape = null;
			m_OCCViewer.GetAISContext().InitSelected();
			int test = m_OCCViewer.GetAISContext().NbSelected();
			int nCount = 0;
			List<TopoDS_Shape> selectedShapeList = new List<TopoDS_Shape>();
			while( m_OCCViewer.GetAISContext().MoreSelected() ) {
				nCount++;
				if( nCount > 2 ) {
					return;
				}
				selectedShapeList.Add( m_OCCViewer.GetAISContext().SelectedShape() );
				m_OCCViewer.GetAISContext().NextSelected();
			}
			if( nCount != 2 ) {
				return;
			}
			CheckRefOrMoveObject( selectedShapeList[ 0 ], out bool isRef1, out bool isMove1 );
			CheckRefOrMoveObject( selectedShapeList[ 1 ], out bool isRef2, out bool isMove2 );
			if( isRef1 && isMove2 ) {
				refShape = selectedShapeList[ 0 ];
				moveShape = selectedShapeList[ 1 ];
			}
			else if( isRef2 && isMove1 ) {
				refShape = selectedShapeList[ 1 ];
				moveShape = selectedShapeList[ 0 ];
			}
			else {
				return;
			}
		}

		void CheckRefOrMoveObject( TopoDS_Shape sel, out bool isRef, out bool isMove )
		{
			isRef = false;
			isMove = false;
			if( sel == null ) {
				return;
			}
			if( sel.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				TopExp_Explorer expRef = new TopExp_Explorer( m_G54Shape, TopAbs_ShapeEnum.TopAbs_FACE );
				while( expRef.More() ) {
					TopoDS_Shape face = expRef.Current();
					if( sel.IsEqual( face ) ) {
						isRef = true;
						return;
					}
					expRef.Next();
				}
				TopExp_Explorer expMove = new TopExp_Explorer( m_RawPartShape, TopAbs_ShapeEnum.TopAbs_FACE );
				while( expMove.More() ) {
					TopoDS_Shape face = expMove.Current();
					if( sel.IsEqual( face ) ) {
						isMove = true;
						return;
					}
					expMove.Next();
				}
			}
			else if( sel.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {
				TopExp_Explorer expRef = new TopExp_Explorer( m_G54Shape, TopAbs_ShapeEnum.TopAbs_EDGE );
				while( expRef.More() ) {
					TopoDS_Shape face = expRef.Current();
					if( sel.IsEqual( face ) ) {
						isRef = true;
						return;
					}
					expRef.Next();
				}
				TopExp_Explorer expMove = new TopExp_Explorer( m_RawPartShape, TopAbs_ShapeEnum.TopAbs_EDGE );
				while( expMove.More() ) {
					TopoDS_Shape face = expMove.Current();
					if( sel.IsEqual( face ) ) {
						isMove = true;
						return;
					}
					expMove.Next();
				}
			}
		}

		void DisableAllConstraintMenu()
		{
			m_tsmiAxial.Enabled = false;
			m_tsmiAxial_R.Enabled = false;
			m_tsmiAxialPar.Enabled = false;
			m_tsmiAxialPar_R.Enabled = false;
			m_tsmiPlane.Enabled = false;
			m_tsmiPlane_R.Enabled = false;
			m_tsmiPlanePar.Enabled = false;
			m_tsmiPlanePar_R.Enabled = false;
		}

		void m_tsmiAxial_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.Axial, false );
		}

		void m_tsmiAxial_R_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.Axial, true );
		}

		void m_tsmiAxialPar_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.AxialParallel, false );
		}

		void m_tsmiAxialPar_R_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.AxialParallel, true );
		}

		void m_tsmiPlane_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.Plane, false );
		}

		void m_tsmiPlane_R_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.Plane, true );
		}

		void m_tsmiPlanePar_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.PlaneParallel, false );
		}

		void m_tsmiPlanePar_R_Click( object sender, EventArgs e )
		{
			SetConstraint( EConstraintType.PlaneParallel, true );
		}

		void SetConstraint( EConstraintType type, bool isReverse )
		{
			GetRefAndMoveObject( out TopoDS_Shape refShape, out TopoDS_Shape moveShape );
			if( refShape == null || moveShape == null ) {
				return;
			}

			// create constraint
			IConstraint c = null;
			switch( type ) {
				case EConstraintType.Axial:
					c = new AxialConstraint( refShape, moveShape, isReverse );
					break;
				case EConstraintType.AxialParallel:
					c = new AxialParallelConstraint( refShape, moveShape, isReverse );
					break;
				case EConstraintType.Plane:
					c = new PlaneConstraint( refShape, moveShape, isReverse );
					break;
				case EConstraintType.PlaneParallel:
					c = new PlaneParallelConstraint( refShape, moveShape, isReverse );
					break;
				default:
					return;
			}

			// transform and update
			gp_Trsf trsf = c.SolveConstraint();
			BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( m_RawPartShape, trsf );
			m_RawPartShape = transform.Shape();
			RefreshViewer();
		}
	}
}
