using ImportExport;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM
{
	public partial class StartupForm : Form
	{
		public StartupForm()
		{
			InitializeComponent();

			// create the viewer
			bool bSucess = m_viewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( "Failed to initialize the viewer" );
				return;
			}
			Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Fill;

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;

			// init menu
			m_tsmiExtractFace.Enabled = false;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_viewer = new Viewer();

		// action
		enum EActionMode
		{
			None = 0,
			ExtractFace = 1
		}
		EActionMode m_eActionMode = EActionMode.None;
		EActionMode ActionMode
		{
			get
			{
				return m_eActionMode;
			}
			set
			{
				OnBeforeActionModeChanged();
				m_eActionMode = value;
				OnAfterActionModeChanged();
			}
		}

		// import model
		void m_tsmiImportBRep_Click( object sender, EventArgs e )
		{
			ImportModel( ModelFormat.BREP );
		}

		void m_tsmiImportStep_Click( object sender, EventArgs e )
		{
			ImportModel( ModelFormat.STEP );
		}

		void m_tsmiImportIges_Click( object sender, EventArgs e )
		{
			ImportModel( ModelFormat.IGES );
		}

		void ImportModel( ModelFormat format )
		{
			ImportHandler.ImportModel( format, out TopoDS_Shape theShape );
			ShowModel( theShape );
			m_tsmiExtractFace.Enabled = true;
		}

		void ShowModel( TopoDS_Shape theShape )
		{
			if( theShape == null ) {
				MessageBox.Show( "Failed to import the model" );
				return;
			}

			// create AIS_Shape
			AIS_Shape aisShape = new AIS_Shape( theShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			aisShape.SetMaterial( aspect );
			aisShape.SetDisplayMode( 1 );

			// display the shape
			m_viewer.GetAISContext().RemoveAll( false );
			m_viewer.GetAISContext().Display( aisShape, true );
			m_viewer.AxoView();
			m_viewer.ZoomAllView();
		}


		// extract face
		void m_tsmiExtractFace_Click( object sender, EventArgs e )
		{
			ActionMode = EActionMode.ExtractFace;
		}

		List<TopoDS_Face> GetSelectedFace()
		{
			List<TopoDS_Face> lstFace = new List<TopoDS_Face>();
			m_viewer.GetAISContext().InitSelected();
			while( m_viewer.GetAISContext().MoreSelected() ) {
				TopoDS_Shape theShape = m_viewer.GetAISContext().SelectedShape();
				if( theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
					lstFace.Add( TopoDS.ToFace( theShape ) );
				}
				m_viewer.GetAISContext().NextSelected();
			}
			return lstFace;
		}

		void ShowExtrctedFace()
		{
			// get selected face
			List<TopoDS_Face> faceList = GetSelectedFace();
			if( faceList.Count == 0 ) {
				MessageBox.Show( "No face is selected" );
				return;
			}

			// split the faces into shells
			double SewTolerance = 0.001;
			BRepBuilderAPI_Sewing sewer = new BRepBuilderAPI_Sewing( SewTolerance );
			foreach( TopoDS_Face face in faceList ) {
				sewer.Add( face );
			}
			sewer.Perform();
			TopoDS_Shape theShape = sewer.SewedShape();
			List<TopoDS_Shape> shellList = new List<TopoDS_Shape>();
			if( theShape.shapeType == TopAbs_ShapeEnum.TopAbs_SHELL ) {

				// all selected faces are in the same shell
				shellList.Add( TopoDS.ToShell( theShape ) );
			}
			else {
				TopExp_Explorer shellExp = new TopExp_Explorer( theShape, TopAbs_ShapeEnum.TopAbs_SHELL );
				while( shellExp.More() ) {
					shellList.Add( TopoDS.ToShell( shellExp.Current() ) );
					shellExp.Next();
				}

				// get faces not belong to any shell
				List<TopoDS_Shape> freeFaceList = new List<TopoDS_Shape>();
				TopExp_Explorer faceExp = new TopExp_Explorer( theShape, TopAbs_ShapeEnum.TopAbs_FACE );
				while( faceExp.More() ) {
					TopoDS_Face face = TopoDS.ToFace( faceExp.Current() );
					if( !shellList.Exists( shell => shell.elementsAsList.Contains( face ) ) ) {
						freeFaceList.Add( face );
					}
					faceExp.Next();
				}
				shellList.AddRange( freeFaceList );
			}

			// get all bondary wire of the extracted faces
			List<TopoDS_Wire> wireList = new List<TopoDS_Wire>();
			foreach( TopoDS_Shape shell in shellList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( shell );
				TopExp_Explorer wireExp = new TopExp_Explorer( freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					wireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}

			m_viewer.GetAISContext().RemoveAll( false );
			foreach( TopoDS_Face face in faceList ) {
				AIS_Shape aisShape = new AIS_Shape( face );
				Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
				aisShape.SetMaterial( aspect );
				aisShape.SetDisplayMode( 1 );
				m_viewer.GetAISContext().Display( aisShape, false );
			}
			foreach( TopoDS_Wire wire in wireList ) {
				AIS_Shape aisShape = new AIS_Shape( wire );
				Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
				aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
				aisShape.SetWidth( 3 );
				m_viewer.GetAISContext().Display( aisShape, false );
			}
			m_viewer.AxoView();
			m_viewer.ZoomAllView();
			m_viewer.UpdateView();
		}

		// action
		void OnBeforeActionModeChanged() // end current action
		{
			switch( m_eActionMode ) {
				case EActionMode.ExtractFace:

					// deactive face selection mode
					m_viewer.GetAISContext().ClearSelected( true );
					m_viewer.GetAISContext().Deactivate();
					m_tsmiExtractFace.BackColor = System.Drawing.SystemColors.Control;
					m_tsmiImportFile.Enabled = true;
					break;
				default:
					break;
			}
		}

		void OnAfterActionModeChanged() // start new action
		{
			switch( m_eActionMode ) {
				case EActionMode.ExtractFace:

					// active face selection mode
					m_viewer.GetAISContext().Activate( 4 );
					m_panViewer.Focus();
					m_tsmiExtractFace.BackColor = System.Drawing.Color.Yellow;
					m_tsmiImportFile.Enabled = false;
					break;
				default:
					break;
			}
		}

		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			switch( m_eActionMode ) {
				case EActionMode.ExtractFace:
					if( e.Button == MouseButtons.Left ) {

						// select the face
						m_viewer.ShiftSelect();
					}
					break;
				default:
					break;
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			switch( m_eActionMode ) {
				case EActionMode.ExtractFace:
					if( e.KeyCode == Keys.Escape ) {
						ActionMode = EActionMode.None;
					}
					else if( e.KeyCode == Keys.Enter ) {

						// extract the face
						ShowExtrctedFace();
						ActionMode = EActionMode.None;
					}
					break;
				default:
					break;
			}
		}
	}
}
