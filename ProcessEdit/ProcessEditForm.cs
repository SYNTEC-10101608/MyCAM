using DataStructure;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TCollection;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProcessEdit
{
	public partial class ProcessEditForm : Form
	{
		public Action<TopoDS_Shape, List<CADData>> ExtractOK;

		public ProcessEditForm()
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

			// menu strip
			m_msOrder.Visible = false;

			// set AIS selction style
			Prs3d_Drawer d = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_Selected );
			d.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			d.SetDisplayMode( (int)AISDisplayMode.AIS_WireFrame );

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// TODO: panel key down does not work
			PreviewKeyDown += ViewerKeyDown;
		}

		public bool Init( ProcessEditModel model )
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
		ProcessEditModel m_Model;

		// view context
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>();
		List<AIS_Shape> m_TraverseList = new List<AIS_Shape>();
		List<AIS_Shape> m_ContourList = new List<AIS_Shape>();

		// order map
		Dictionary<TopoDS_Wire, int> m_OrderMap = new Dictionary<TopoDS_Wire, int>();

		// viewer action
		enum ActionMode
		{
			None,
			Order,
		}
		ActionMode m_ActionMode = ActionMode.None;
		ActionMode actionMode
		{
			get
			{
				return m_ActionMode;
			}
			set
			{
				OnEndAction();
				m_ActionMode = value;
				OnStartAction();
			}
		}

		void ShowModel()
		{
			// show the model shape
			AIS_Shape modelAIS = new AIS_Shape( m_Model.ModelShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			modelAIS.SetMaterial( aspect );
			modelAIS.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( modelAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( modelAIS );

			// show contour
			ShowContour();

			// show index
			ShowIndex();

			// show traverse line
			ShowTraverseLine();

			// arrange the view
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}

		void ShowContour()
		{
			// create contour
			List<TopoDS_Wire> contourList = m_Model.ProcessDataList.
				Where( p => p.ProcessType == EProcessType.ProcessType_Cutting ).
				Select( x => ( (CuttingProcessData)x ).CAMData.CADData.Contour ).ToList();
			for( int i = 0; i < contourList.Count; i++ ) {
				TopoDS_Wire contour = contourList[ i ];
				AIS_Shape contourAIS = new AIS_Shape( contour );
				contourAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
				contourAIS.SetWidth( 2 );
				m_ContourList.Add( contourAIS );
				m_OrderMap.Add( contour, i );
			}

			// display contour
			foreach( AIS_Shape contour in m_ContourList ) {
				m_OCCViewer.GetAISContext().Display( contour, false );
				m_OCCViewer.GetAISContext().Deactivate( contour );
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
			foreach( IProcessData data in m_Model.ProcessDataList ) {
				gp_Pnt location = GetKeyPoint( data, true );
				string szIndex = string.Empty;
				if( data.ProcessType == EProcessType.ProcessType_Cutting ) {
					szIndex = nCurrentIndex.ToString();
					nCurrentIndex++;
				}
				else if( data.ProcessType == EProcessType.ProcessType_Traverse ) {
					szIndex = nCurrentIndex.ToString() + ".5";
				}

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
			m_OCCViewer.UpdateView();
		}

		void ShowTraverseLine()
		{
			// clear the previous traverse line
			foreach( AIS_Shape line in m_TraverseList ) {
				m_OCCViewer.GetAISContext().Remove( line, false );
			}
			m_TraverseList.Clear();

			// create traverse line
			for( int i = 0; i < m_Model.ProcessDataList.Count - 1; i++ ) {
				gp_Pnt start = GetKeyPoint( m_Model.ProcessDataList[ i ], false );
				gp_Pnt end = GetKeyPoint( m_Model.ProcessDataList[ i + 1 ], true );
				BRepBuilderAPI_MakeEdge makeEdge = new BRepBuilderAPI_MakeEdge( start, end );
				AIS_Shape line = new AIS_Shape( makeEdge.Edge() );
				line.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
				Prs3d_LineAspect aspect = line.Attributes().WireAspect();
				aspect.SetWidth( 2 );
				aspect.SetTypeOfLine( Aspect_TypeOfLine.Aspect_TOL_DASH );
				aspect.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
				m_TraverseList.Add( line );
			}

			// display traverse line
			foreach( AIS_Shape line in m_TraverseList ) {
				m_OCCViewer.GetAISContext().Display( line, false );
				m_OCCViewer.GetAISContext().Deactivate( line );
			}
			m_OCCViewer.UpdateView();
		}

		gp_Pnt GetKeyPoint( IProcessData data, bool bStart )
		{
			if( data.ProcessType == EProcessType.ProcessType_Cutting ) {

				// TODO: using start point API later
				CuttingProcessData cuttingData = (CuttingProcessData)data;
				if( bStart ) {
					return cuttingData.CAMData.CAMPointList[ 0 ].Point;
				}
				else {
					return cuttingData.CAMData.CAMPointList[ cuttingData.CAMData.CAMPointList.Count - 1 ].Point;
				}
			}
			else if( data.ProcessType == EProcessType.ProcessType_Traverse ) {
				TraverseProcessData traverseData = (TraverseProcessData)data;
				return traverseData.Point_MCS;
			}
			return new gp_Pnt();
		}

		// edit ok
		void m_tsmiOK_Click( object sender, EventArgs e )
		{
		}

		void m_tsmiOrder_Click( object sender, EventArgs e )
		{
			actionMode = ActionMode.Order;
		}

		void m_tsmiTraverse_Click( object sender, EventArgs e )
		{

		}

		void m_tsmiSiftUp_Click( object sender, EventArgs e )
		{
			// get selected contour
			m_OCCViewer.GetAISContext().InitSelected();
		}

		void m_tsmiSiftDown_Click( object sender, EventArgs e )
		{

		}

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			switch( m_ActionMode ) {
				case ActionMode.Order:
					if( e.Button == MouseButtons.Left ) {

						// select the contour
						m_OCCViewer.Select();
					}
					break;
				default:
					break;
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			switch( m_ActionMode ) {
				case ActionMode.Order:
					if( e.KeyCode == Keys.Escape ) {

						// exit the order mode
						actionMode = ActionMode.None;
					}
					break;
				default:
					break;
			}
		}

		void OnEndAction()
		{
			switch( m_ActionMode ) {
				case ActionMode.Order:
					m_msOrder.Visible = false;
					menuStrip1.Visible = true;

					// deactive contour selection
					foreach( AIS_Shape contour in m_ContourList ) {
						m_OCCViewer.GetAISContext().Deactivate( contour );
					}

					// clear all selection
					m_OCCViewer.GetAISContext().ClearSelected( true );
					break;
				default:
					break;
			}
		}

		void OnStartAction()
		{
			switch( m_ActionMode ) {
				case ActionMode.Order:
					m_msOrder.Visible = true;
					menuStrip1.Visible = false;

					// active contour selection
					foreach( AIS_Shape contour in m_ContourList ) {
						m_OCCViewer.GetAISContext().Activate( contour, (int)AISActiveMode.Wire );
					}
					break;
				default:
					break;
			}
		}
	}
}
