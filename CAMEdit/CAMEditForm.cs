using DataStructure;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CAMEdit
{
	public partial class CAMEditForm : Form
	{
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
			ShowModel();
			return true;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// model
		CAMEditModel m_Model;

		// init
		void ShowModel()
		{
			// get contour list
			List<TopoDS_Wire> contourList = m_Model.CAMDataList.Select( x => x.CADData.Contour ).ToList();
			if( contourList == null || contourList.Count == 0 ) {
				return;
			}

			// get CAM point list
			List<CAMPoint> camPointList = m_Model.CAMDataList.SelectMany( x => x.CAMPointList ).ToList();
			if( camPointList == null || camPointList.Count == 0 ) {
				return;
			}

			// get model shape
			TopoDS_Shape modelShape = m_Model.ModelShape;
			if( modelShape == null ) {
				return;
			}

			// clear the viewer
			m_OCCViewer.GetAISContext().RemoveAll( false );

			// display the model
			AIS_Shape modelAIS = new AIS_Shape( modelShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			modelAIS.SetMaterial( aspect );
			modelAIS.SetDisplayMode( 1 );
			m_OCCViewer.GetAISContext().Display( modelAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( modelAIS );

			// display tool vectors
			foreach( CAMPoint camPoint in camPointList ) {
				gp_Pnt point = camPoint.Point;
				gp_Dir toolVec = camPoint.ToolVec;
				gp_Pnt endPoint = new gp_Pnt( point.XYZ() + toolVec.XYZ() * 0.5 );
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( point, endPoint );
				AIS_Shape lineAIS = new AIS_Shape( edgeMaker.Shape() );
				lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
				lineAIS.SetWidth( 2 );
				m_OCCViewer.GetAISContext().Display( lineAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( lineAIS );
			}

			// display tangent vectors
			foreach( CAMPoint camPoint in camPointList ) {
				gp_Pnt point = camPoint.Point;
				gp_Dir tangentVec = camPoint.TangentVec;
				gp_Pnt endPoint = new gp_Pnt( point.XYZ() + tangentVec.XYZ() * 0.5 );
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( point, endPoint );
				AIS_Shape lineAIS = new AIS_Shape( edgeMaker.Shape() );
				lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
				lineAIS.SetWidth( 2 );
				m_OCCViewer.GetAISContext().Display( lineAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( lineAIS );
			}

			// display normal vectors
			foreach( CAMPoint camPoint in camPointList ) {
				gp_Pnt point = camPoint.Point;
				gp_Dir normalVec = camPoint.NormalVec;
				gp_Pnt endPoint = new gp_Pnt( point.XYZ() + normalVec.XYZ() * 0.5 );
				BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( point, endPoint );
				AIS_Shape lineAIS = new AIS_Shape( edgeMaker.Shape() );
				lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
				lineAIS.SetWidth( 2 );
				m_OCCViewer.GetAISContext().Display( lineAIS, false );
				m_OCCViewer.GetAISContext().Deactivate( lineAIS );
			}

			// display the contours
			foreach( TopoDS_Wire contour in contourList ) {
				AIS_Shape contourAIS = new AIS_Shape( contour );
				contourAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				contourAIS.SetWidth( 1 );
				m_OCCViewer.GetAISContext().Display( contourAIS, false );
			}

			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
			m_OCCViewer.UpdateView();
		}

		// viewer action
		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
		}

		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
		}
	}
}
