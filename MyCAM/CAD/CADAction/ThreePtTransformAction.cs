using OCC.AIS;
using OCC.BRep;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class ThreePtTransformAction : CADACtionBase
	{
		public ThreePtTransformAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
			m_ActionStage = EActionStage.P1;

			// create labels
			m_LabelO = new AIS_TextLabel();
			m_LabelX.SetText( new TCollection_ExtendedString( "X" ) );
			m_LabelX.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_LabelX.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			m_LabelO = new AIS_TextLabel();
			m_LabelO.SetText( new TCollection_ExtendedString( "O" ) );
			m_LabelO.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_LabelO.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.Create3PCS;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// disable tree view
			m_TreeView.Enabled = false;

			// activate vertex selection mode
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Activate( viewObject.AISHandle, (int)AISActiveMode.Vertex );
			}
			m_ActionStage = EActionStage.P1;
		}

		public override void End()
		{
			// clear label
			m_Viewer.GetAISContext().Remove( m_LabelO, false );
			m_Viewer.GetAISContext().Remove( m_LabelX, false );

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();

			// enable tree view
			m_TreeView.Enabled = true;

			// deactivate
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate();
			}
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				m_Viewer.GetAISContext().SelectDetected();
				bool bSelected = GetSelectPoint( out gp_Pnt slectedPoint );
				if( !bSelected ) {
					return;
				}
				if( m_ActionStage == EActionStage.P1 ) {
					m_1stPoint = slectedPoint;
					m_LabelO.SetPosition( m_1stPoint );
					m_Viewer.GetAISContext().Display( m_LabelO, true );
					m_ActionStage = EActionStage.P2;
				}
				else if( m_ActionStage == EActionStage.P2 ) {
					if( slectedPoint.IsEqual( m_1stPoint, 1e-6 ) ) {
						MessageBox.Show( "point coincident 2nd" );
						return;
					}
					m_2ndPoint = slectedPoint;
					m_LabelX.SetPosition( m_2ndPoint );
					m_Viewer.GetAISContext().Display( m_LabelX, true );
					m_ActionStage = EActionStage.P3;
				}
				else if( m_ActionStage == EActionStage.P3 ) {
					if( slectedPoint.IsEqual( m_1stPoint, 1e-6 ) || slectedPoint.IsEqual( m_2ndPoint, 1e-6 ) ) {
						MessageBox.Show( "point coincident 3rd" );
						return;
					}
					m_3rdPoint = slectedPoint;
					bool isValid = CreateCoordSystem();
					if( !isValid ) {
						MessageBox.Show( "3 points are colinear" );
						return;
					}
					ShowCoordSystem();
					End();
				}
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		bool GetSelectPoint( out gp_Pnt point )
		{
			point = new gp_Pnt();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return false;
			}
			TopoDS_Shape selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape == null || selectedShape.IsNull() ) {
				return false;
			}
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return false;
			}
			TopoDS_Vertex vertex = TopoDS.ToVertex( selectedShape );
			point = BRep_Tool.Pnt( vertex );
			return true;
		}

		bool CreateCoordSystem()
		{
			gp_Vec v12 = new gp_Vec( m_1stPoint, m_2ndPoint );
			gp_Vec v13 = new gp_Vec( m_1stPoint, m_3rdPoint );
			gp_Vec vZ = v12.Crossed( v13 );
			if( vZ.Magnitude() < 1e-6 ) {
				return false; // points are collinear
			}
			m_3PCoordSys = new gp_Ax3( m_1stPoint, new gp_Dir( vZ ), new gp_Dir( v12 ) );
			return true;
		}

		void ShowCoordSystem()
		{
			// create AIS Trihedron
			AIS_Trihedron trihedron = new AIS_Trihedron( new Geom_Axis2Placement( m_3PCoordSys.Ax2() ) );
			trihedron.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			trihedron.SetAxisColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY ) );
			trihedron.SetSize( 10.0 );
			trihedron.SetTextColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );

			// display the trihedron
			m_Viewer.GetAISContext().Display( trihedron, true );
			m_Viewer.GetAISContext().Deactivate( trihedron );
		}

		enum EActionStage
		{
			P1 = 0,
			P2 = 1,
			P3 = 2,
		}
		EActionStage m_ActionStage;
		gp_Pnt m_1stPoint;
		gp_Pnt m_2ndPoint;
		gp_Pnt m_3rdPoint;
		gp_Ax3 m_3PCoordSys;
		AIS_TextLabel m_LabelO;
		AIS_TextLabel m_LabelX;
	}
}
