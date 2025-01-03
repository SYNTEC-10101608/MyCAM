using DataStructure;
using OCC.Prs3d;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
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

			// set AIS selction style
			Prs3d_Drawer d = m_OCCViewer.GetAISContext().HighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected );
			d.SetColor( new OCC.Quantity.Quantity_Color( OCC.Quantity.Quantity_NameOfColor.Quantity_NOC_RED ) );
			d.SetTransparency( 0.5f );
			d.SetDisplayMode( 1 );
			m_OCCViewer.GetAISContext().SetHighlightStyle( Prs3d_TypeOfHighlight.Prs3d_TypeOfHighlight_LocalSelected, d );

			// set model

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
			return true;
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_OCCViewer = new Viewer();

		// model
		ProcessEditModel m_Model;

		void ShowModel()
		{
		}

		// extract face
		void m_tsmiOK_Click( object sender, EventArgs e )
		{
		}

		// viewer action
		void ViewerMouseDown( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {

				// select the face
				m_OCCViewer.ShiftSelect();
			}
		}

		void ViewerKeyDown( object sender, PreviewKeyDownEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {

				// clear the selected face
				m_OCCViewer.GetAISContext().ClearSelected( true );
			}
		}
	}
}
