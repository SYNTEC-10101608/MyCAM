using DataStructure;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TCollection;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CAMEdit
{
	public partial class CAMEditForm : Form
	{
		public Action<TopoDS_Shape, List<CuttingProcessData>> CADEditOK;

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

		enum EvecType
		{
			ToolVec,
			TangentVec,
			NormalVec,
		}

		// init
		void ShowModel()
		{
			// get cam data list
			List<CAMData> camDataList = m_Model.CAMDataList;
			if( camDataList == null || camDataList.Count == 0 ) {
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

			// display cam data
			for( int i = 0; i < camDataList.Count; i++ ) {

				// display vectors
				foreach( CAMPoint camPoint in camDataList[ i ].CAMPointList ) {

					// tool vector
					AIS_Shape toolVecAIS = GetVecAIS( camPoint.Point, camPoint.ToolVec, EvecType.ToolVec );
					m_OCCViewer.GetAISContext().Display( toolVecAIS, false );
					m_OCCViewer.GetAISContext().Deactivate( toolVecAIS );

					// tangent vector
					AIS_Shape tangentVecAIS = GetVecAIS( camPoint.Point, camPoint.TangentVec, EvecType.TangentVec );
					m_OCCViewer.GetAISContext().Display( tangentVecAIS, false );
					m_OCCViewer.GetAISContext().Deactivate( tangentVecAIS );

					// normal vector
					AIS_Shape normalVecAIS = GetVecAIS( camPoint.Point, camPoint.NormalVec, EvecType.NormalVec );
					m_OCCViewer.GetAISContext().Display( normalVecAIS, false );
					m_OCCViewer.GetAISContext().Deactivate( normalVecAIS );
				}

				// display index
				gp_Pnt indexPoint = camDataList[ i ].CAMPointList.First().Point;
				AIS_TextLabel indexText = new AIS_TextLabel();
				indexText.SetText( new TCollection_ExtendedString( i.ToString() ) );
				indexText.SetPosition( indexPoint );
				indexText.SetHeight( 20 );
				indexText.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				m_OCCViewer.GetAISContext().Display( indexText, false );
				m_OCCViewer.GetAISContext().Deactivate( indexText );
			}

			// display the contours
			//foreach( TopoDS_Wire contour in contourList ) {
			//	AIS_Shape contourAIS = new AIS_Shape( contour );
			//	contourAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			//	contourAIS.SetWidth( 1 );
			//	m_OCCViewer.GetAISContext().Display( contourAIS, false );
			//}

			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
			m_OCCViewer.UpdateView();
		}

		AIS_Shape GetVecAIS( gp_Pnt point, gp_Dir dir, EvecType vecType )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 0.2 );
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
			List<CuttingProcessData> cuttingProcessDataList =
				m_Model.CAMDataList.Select( camData => new CuttingProcessData( camData ) ).ToList();
			CADEditOK?.Invoke( m_Model.ModelShape, cuttingProcessDataList );
		}
	}
}
