using OCC.AIS;
using OCC.Graphic3d;
using OCC.Quantity;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
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
			bool bSucess = m_viewer.InitViewer( m_panViewer );
			if( !bSucess ) {
				MessageBox.Show( "Error: Init Viewer" );
				return;
			}
			Controls.Add( m_panViewer );
			m_panViewer.Dock = DockStyle.Fill;

			// viewer action
			m_panViewer.MouseDown += ViewerMouseDown;
			m_panViewer.PreviewKeyDown += ViewerKeyDown;
		}

		public void Init( TopoDS_Shape model, List<TopoDS_Face> extractedFaceList )
		{
			if( model == null || extractedFaceList == null ) {
				MessageBox.Show( "Error: No model" );
				return;
			}
			if( extractedFaceList.Count == 0 ) {
				MessageBox.Show( "Error: No face" );
				return;
			}
			m_modelShape = model;
			m_extractedFaceList = extractedFaceList;
			ShowExtrctedFace();
		}

		// viewer
		Panel m_panViewer = new Panel();
		Viewer m_viewer = new Viewer();

		// init
		TopoDS_Shape m_modelShape = null;
		List<TopoDS_Face> m_extractedFaceList = null;

		// init
		void ShowExtrctedFace()
		{
			// sew the faces
			TopoDS_Shape sewResult = Sew.SewShape( m_extractedFaceList.Cast<TopoDS_Shape>().ToList() );
			List<TopoDS_Wire> boundaryWireList = GetAllFreeBound( sewResult );

			// clear the viewer
			m_viewer.GetAISContext().RemoveAll( false );

			// display the model
			AIS_Shape modelAIS = new AIS_Shape( m_modelShape );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			modelAIS.SetMaterial( aspect );
			modelAIS.SetDisplayMode( 1 );
			m_viewer.GetAISContext().Display( modelAIS, false );

			// display the faces
			//AIS_Shape faceAIS = new AIS_Shape( sewResult );
			//Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			//faceAIS.SetMaterial( aspect );
			//faceAIS.SetDisplayMode( 1 );
			//m_viewer.GetAISContext().Display( faceAIS, false );

			// display the boundary wires
			foreach( TopoDS_Wire wire in boundaryWireList ) {
				AIS_Shape oneWireAIS = new AIS_Shape( wire );
				oneWireAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
				oneWireAIS.SetWidth( 3 );
				m_viewer.GetAISContext().Display( oneWireAIS, false );
			}
			m_viewer.AxoView();
			m_viewer.ZoomAllView();
			m_viewer.UpdateView();
		}

		// TODO: the grouping method is tricky, need to be improved
		List<TopoDS_Wire> GetAllFreeBound( TopoDS_Shape sewResult )
		{
			List<TopoDS_Shape> faceGroupList = new List<TopoDS_Shape>();

			// single shell or single face
			if( sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_SHELL
				|| sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_FACE ) {
				faceGroupList.Add( sewResult );
			}

			// some shell and free face exist
			else {
				foreach( TopoDS_Shape shape in sewResult.elementsAsList ) {
					faceGroupList.Add( shape );
				}
			}

			// get free boundary wires
			List<TopoDS_Wire> wireList = new List<TopoDS_Wire>();
			foreach( TopoDS_Shape faceGroup in faceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( faceGroup );
				TopExp_Explorer wireExp = new TopExp_Explorer( freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					wireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}
			return wireList;
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
