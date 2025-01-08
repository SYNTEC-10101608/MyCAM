using DataStructure;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TCollection;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ProcessEdit
{
	public partial class ProcessEditForm : Form
	{
		public Action<List<IProcessData>> EditOK;

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
		}

		public bool Init( ProcessEditModel model )
		{
			if( model == null ) {
				return false;
			}
			m_Model = model;
			ShowPart();

			// create order form
			m_OrderForm.Init( m_Model );
			m_OrderForm.PropertyChanged += OnPropertyChanged;
			m_OrderForm.ItemPick += OnItemPick;
			m_OrderForm.Show();
			return true;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// model
		ProcessEditModel m_Model;

		// view context
		List<AIS_InteractiveObject> m_ProcessList = new List<AIS_InteractiveObject>();
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>();
		List<AIS_Shape> m_TraverseList = new List<AIS_Shape>();

		// order
		ObjectForm m_OrderForm = new ObjectForm();

		void ShowPart()
		{
			// show the part shape
			AIS_Shape partAIS = new AIS_Shape( m_Model.PartShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			partAIS.SetMaterial( aspect );
			partAIS.SetDisplayMode( (int)AISDisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( partAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( partAIS );

			// show contour
			ShowProcess();

			// show index
			ShowIndex();

			// show traverse line
			ShowTraverseLine();

			// arrange the view
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}

		void ShowProcess()
		{
			// clear the previous process
			foreach( AIS_InteractiveObject process in m_ProcessList ) {
				m_OCCViewer.GetAISContext().Remove( process, false );
			}
			m_ProcessList.Clear();

			foreach( IProcessData processData in m_Model.ProcessDataList ) {

				// show path contour if it is cutting process
				if( processData.ProcessType == EProcessType.ProcessType_Cutting ) {
					CuttingProcessData cuttingData = (CuttingProcessData)processData;
					AIS_Shape contourAIS = new AIS_Shape( cuttingData.CAMData.CADData.Contour );
					contourAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
					Prs3d_LineAspect aspect = contourAIS.Attributes().WireAspect();
					aspect.SetWidth( 2 );
					m_ProcessList.Add( contourAIS );
				}

				// show "+" at traverse point if it is traverse process
				else if( processData.ProcessType == EProcessType.ProcessType_Traverse ) {
					TraverseProcessData traverseData = (TraverseProcessData)processData;
					AIS_TextLabel textLabel = new AIS_TextLabel();
					textLabel.SetText( new TCollection_ExtendedString( "+" ) );
					textLabel.SetPosition( traverseData.Point );
					textLabel.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
					textLabel.SetHeight( 20 );
					textLabel.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
					m_ProcessList.Add( textLabel );
				}
			}

			// display process
			foreach( AIS_InteractiveObject process in m_ProcessList ) {
				m_OCCViewer.GetAISContext().Display( process, false );
				m_OCCViewer.GetAISContext().Deactivate( process );
			}

			m_OCCViewer.UpdateView();
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
					nCurrentIndex++;
					szIndex = nCurrentIndex.ToString();
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
				aspect.SetWidth( 1 );
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
					return cuttingData.CAMData.CADPointList[ 0 ].Point;
				}
				else {
					return cuttingData.CAMData.CADPointList[ cuttingData.CAMData.CADPointList.Count - 1 ].Point;
				}
			}
			else if( data.ProcessType == EProcessType.ProcessType_Traverse ) {
				TraverseProcessData traverseData = (TraverseProcessData)data;
				return traverseData.Point;
			}
			return new gp_Pnt();
		}

		// edit ok
		void m_tsmiOK_Click( object sender, EventArgs e )
		{
			EditOK?.Invoke( m_Model.ProcessDataList );
		}

		void OnPropertyChanged()
		{
			ShowProcess();
			ShowIndex();
			ShowTraverseLine();
		}

		void OnItemPick( int nIndex )
		{
			// dehighlight the previous selected
			foreach( AIS_InteractiveObject process in m_ProcessList ) {
				process.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			}
			foreach( AIS_TextLabel textLabel in m_IndexList ) {
				textLabel.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			}

			// highlight the selected
			if( nIndex >= 0 && nIndex < m_ProcessList.Count ) {
				m_ProcessList[ nIndex ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			}
			if( nIndex >= 0 && nIndex < m_IndexList.Count ) {
				m_IndexList[ nIndex ].SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			}
			m_OCCViewer.UpdateView();
		}
	}
}
