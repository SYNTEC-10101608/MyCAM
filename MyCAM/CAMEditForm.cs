using OCC.AIS;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM
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
			// get boundary wires
			List<TopoDS_Wire> boundaryWireList = m_Model.CAMDataList.Select( x => x.CADData.Contour ).ToList();
			if( boundaryWireList == null || boundaryWireList.Count == 0 ) {
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

			// display the boundary wires
			foreach( TopoDS_Wire wire in boundaryWireList ) {
				AIS_Shape oneWireAIS = new AIS_Shape( wire );
				oneWireAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
				oneWireAIS.SetWidth( 3 );
				m_OCCViewer.GetAISContext().Display( oneWireAIS, false );
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
