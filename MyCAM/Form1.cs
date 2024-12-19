using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class Form1 : Form
	{
		public Form1()
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

			// make a square
			gp_Pnt p1 = new gp_Pnt( 10, 10, 0 );
			gp_Pnt p2 = new gp_Pnt( -10, 10, 0 );
			gp_Pnt p3 = new gp_Pnt( -10, -10, 0 );
			gp_Pnt p4 = new gp_Pnt( 10, -10, 0 );
			BRepBuilderAPI_MakeEdge edge12 = new BRepBuilderAPI_MakeEdge( p1, p2 );
			BRepBuilderAPI_MakeEdge edge23 = new BRepBuilderAPI_MakeEdge( p2, p3 );
			BRepBuilderAPI_MakeEdge edge34 = new BRepBuilderAPI_MakeEdge( p3, p4 );
			BRepBuilderAPI_MakeEdge edge41 = new BRepBuilderAPI_MakeEdge( p4, p1 );
			BRepBuilderAPI_MakeWire wire = new BRepBuilderAPI_MakeWire();
			wire.Add( edge12.Edge() );
			wire.Add( edge23.Edge() );
			wire.Add( edge34.Edge() );
			wire.Add( edge41.Edge() );
			BRepBuilderAPI_MakeFace face = new BRepBuilderAPI_MakeFace( wire.Wire() );

			// make a box
			gp_Vec vec = new gp_Vec( 0, 0, 10 );
			BRepPrimAPI_MakePrism prism = new BRepPrimAPI_MakePrism( face.Face(), vec );

			// make a line
			gp_Pnt lp1 = new gp_Pnt( 0, 5, 20 );
			gp_Pnt lp2 = new gp_Pnt( 0, 15, 20 );
			BRepBuilderAPI_MakeEdge line = new BRepBuilderAPI_MakeEdge( lp1, lp2 );

			// dispaly the box
			AIS_Shape boxAIS = new AIS_Shape( prism.Shape() );
			m_OCCViewer.GetAISContext().Display( boxAIS, true );

			// display the line
			AIS_Shape lineAIS = new AIS_Shape( line.Edge() );
			m_OCCViewer.GetAISContext().Display( lineAIS, true );

			ShapeProjectionExtensions.ProjectWireToShape( wire.Wire(), prism.Shape(), new gp_Vec( 0, 0, 1 ), null, out List<TopoDS_Wire> frontWires, out List<TopoDS_Wire> backWires );

			// display the projected wire
			foreach( TopoDS_Wire w in frontWires ) {
				AIS_Shape wireAIS = new AIS_Shape( w );
				wireAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
				m_OCCViewer.GetAISContext().Display( wireAIS, true );
			}
			//foreach( TopoDS_Wire w in backWires ) {
			//	AIS_Shape wireAIS = new AIS_Shape( w );
			//	wireAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			//	m_OCCViewer.GetAISContext().Display( wireAIS, true );
			//}
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();
	}
}
