using DataStructure;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
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
			m_panViewer.PreviewKeyDown += ViewerKeyDown;
		}

		public bool Init( ProcessEditModel model )
		{
			if( model == null ) {
				return false;
			}
			m_Model = model;
			ShowPart();
			return true;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// model
		ProcessEditModel m_Model;

		// view context
		Dictionary<AIS_InteractiveObject, IProcessData> m_ProcessList = new Dictionary<AIS_InteractiveObject, IProcessData>();
		List<AIS_TextLabel> m_IndexList = new List<AIS_TextLabel>();
		List<AIS_Shape> m_TraverseList = new List<AIS_Shape>();
		IProcessData m_SelectedProcess = null;

		void ShowPart()
		{
			// show the part shape
			AIS_Shape partAIS = new AIS_Shape( m_Model.PartShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			partAIS.SetMaterial( aspect );
			partAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			m_OCCViewer.GetAISContext().Display( partAIS, false );
			m_OCCViewer.GetAISContext().Deactivate( partAIS );

			// show process
			RefreshViewer();

			// arrange the view
			m_OCCViewer.AxoView();
			m_OCCViewer.ZoomAllView();
		}

		void ShowProcess()
		{
			// clear the previous process
			foreach( AIS_InteractiveObject process in m_ProcessList.Keys ) {
				m_OCCViewer.GetAISContext().Remove( process, false );
			}
			m_ProcessList.Clear();

			foreach( IProcessData processData in m_Model.ProcessDataList ) {

				// show path contour if it is cutting process
				if( processData.ProcessType == EProcessType.ProcessType_Cutting ) {
					CuttingProcessData cuttingData = (CuttingProcessData)processData;
					AIS_Shape contourAIS = new AIS_Shape( cuttingData.CAMData.CADData.PathWire );
					contourAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
					contourAIS.SetWidth( 1.5 );
					m_ProcessList.Add( contourAIS, processData );
				}

				// show "+" at traverse point if it is traverse process
				else if( processData.ProcessType == EProcessType.ProcessType_Traverse ) {
					TraverseProcessData traverseData = (TraverseProcessData)processData;
					BRepPrimAPI_MakeSphere makeSphere = new BRepPrimAPI_MakeSphere( traverseData.Point, 0.5 );
					AIS_Shape sphereAIS = new AIS_Shape( makeSphere.Shape() );
					sphereAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
					sphereAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
					m_ProcessList.Add( sphereAIS, processData );
				}
			}

			// display process
			foreach( AIS_InteractiveObject process in m_ProcessList.Keys ) {
				m_OCCViewer.GetAISContext().Display( process, false );
				m_OCCViewer.GetAISContext().Activate( process );
				if( m_SelectedProcess != null && m_SelectedProcess == m_ProcessList[ process ] ) {
					m_OCCViewer.GetAISContext().SetSelected( process, true );
				}
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
				line.SetWidth( 2 );
				Prs3d_LineAspect aspect = line.Attributes().WireAspect();
				aspect.SetTypeOfLine( Aspect_TypeOfLine.Aspect_TOL_DASH );
				m_TraverseList.Add( line );
			}

			// display traverse line
			foreach( AIS_Shape line in m_TraverseList ) {
				m_OCCViewer.GetAISContext().Display( line, false );
				m_OCCViewer.GetAISContext().Deactivate( line );
			}
		}

		gp_Pnt GetKeyPoint( IProcessData data, bool bStart )
		{
			if( data.ProcessType == EProcessType.ProcessType_Cutting ) {
				CuttingProcessData cuttingData = (CuttingProcessData)data;
				if( bStart ) {
					return cuttingData.CAMData.CAMPointList[ 0 ].CADPoint.Point;
				}
				else {
					return cuttingData.CAMData.CAMPointList[ cuttingData.CAMData.CAMPointList.Count - 1 ].CADPoint.Point;
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

		void RefreshViewer()
		{
			ShowProcess();
			ShowIndex();
			ShowTraverseLine();
			m_OCCViewer.UpdateView();
		}

		// viewer action
		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
		}

		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			m_panViewer.Focus();
			if( e.Button == MouseButtons.Left ) {
				m_OCCViewer.Select();
			}
		}

		void GetSelectedProcess()
		{
			m_SelectedProcess = null;
			foreach( AIS_InteractiveObject process in m_ProcessList.Keys ) {
				if( m_OCCViewer.GetAISContext().IsSelected( process ) ) {
					m_SelectedProcess = m_ProcessList[ process ];
					break;
				}
			}
		}

		void m_tsmiMoveUp_Click( object sender, EventArgs e )
		{
			GetSelectedProcess();
			if( m_SelectedProcess == null ) {
				return;
			}
			int nIndex = m_Model.ProcessDataList.IndexOf( m_SelectedProcess );
			if( nIndex == -1 ) {
				return;
			}
			MoveProcess( nIndex, true );
		}

		void m_tsmiMoveDown_Click( object sender, EventArgs e )
		{
			GetSelectedProcess();
			if( m_SelectedProcess == null ) {
				return;
			}
			int nIndex = m_Model.ProcessDataList.IndexOf( m_SelectedProcess );
			if( nIndex == -1 ) {
				return;
			}
			MoveProcess( nIndex, false );
		}

		void MoveProcess( int nIndex, bool bUp )
		{
			if( nIndex < 0 || nIndex > m_Model.ProcessDataList.Count - 1
				|| bUp && nIndex == 0
				|| !bUp && nIndex == m_Model.ProcessDataList.Count - 1 ) {
				return;
			}
			IProcessData data = m_Model.ProcessDataList[ nIndex ];
			m_Model.ProcessDataList.RemoveAt( nIndex );
			if( bUp ) {
				m_Model.ProcessDataList.Insert( nIndex - 1, data );
			}
			else {
				m_Model.ProcessDataList.Insert( nIndex + 1, data );
			}
			RefreshViewer();
		}

		void m_tsmiAddTraverse_Click( object sender, EventArgs e )
		{
			GetSelectedProcess();
			if( m_SelectedProcess == null ) {
				return;
			}
			int nIndex = m_Model.ProcessDataList.IndexOf( m_SelectedProcess );
			if( nIndex == -1 ) {
				return;
			}
			TraverseForm form = new TraverseForm();
			if( form.ShowDialog() != DialogResult.OK ) {
				return;
			}
			TraverseProcessData traverseData = new TraverseProcessData( new gp_Pnt( form.X, form.Y, form.Z ) );
			m_Model.ProcessDataList.Insert( nIndex + 1, traverseData );
			RefreshViewer();
		}

		void m_tsmiEdit_Click( object sender, EventArgs e )
		{
			GetSelectedProcess();
			if( m_SelectedProcess == null ) {
				return;
			}
			if( m_SelectedProcess.ProcessType == EProcessType.ProcessType_Cutting ) {
				return;
			}
			else if( m_SelectedProcess.ProcessType == EProcessType.ProcessType_Traverse ) {
				TraverseProcessData traverseData = (TraverseProcessData)m_SelectedProcess;
				TraverseForm form = new TraverseForm( traverseData );
				if( form.ShowDialog() != DialogResult.OK ) {
					return;
				}
				m_Model.ProcessDataList[ m_Model.ProcessDataList.IndexOf( m_SelectedProcess ) ] = new TraverseProcessData( new gp_Pnt( form.X, form.Y, form.Z ) );
			}
			RefreshViewer();
		}

		void m_tsmiRemove_Click( object sender, EventArgs e )
		{
			GetSelectedProcess();
			if( m_SelectedProcess == null ) {
				return;
			}
			m_Model.ProcessDataList.Remove( m_SelectedProcess );
			RefreshViewer();
		}
	}
}
