using MyCAM.Data;
using OCC.AIS;
using OCC.BRep;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class ThreePtTransformAction : CADACtionBase
	{
		public ThreePtTransformAction( Viewer viewer, TreeView treeView, DataManager cadManager, ViewManager viewManager )
			: base( viewer, treeView, cadManager, viewManager )
		{
			m_ActionStage = EActionStage.P1;

			// create labels
			m_LabelX = new AIS_TextLabel();
			m_LabelX.SetText( new TCollection_ExtendedString( "X" ) );
			m_LabelX.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_LabelX.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			m_LabelO = new AIS_TextLabel();
			m_LabelO.SetText( new TCollection_ExtendedString( "O" ) );
			m_LabelO.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_LabelO.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ThreePtTransform;
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
			foreach( var partID in m_CADManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Vertex );
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
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
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

					// check point coincidence
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

					// check point coincidence
					if( slectedPoint.IsEqual( m_1stPoint, 1e-6 ) || slectedPoint.IsEqual( m_2ndPoint, 1e-6 ) ) {
						MessageBox.Show( "point coincident 3rd" );
						return;
					}
					m_3rdPoint = slectedPoint;

					// check point colinearity
					gp_Vec v12 = new gp_Vec( m_1stPoint, m_2ndPoint );
					gp_Vec v13 = new gp_Vec( m_1stPoint, m_3rdPoint );
					if( v12.IsParallel( v13, 1e-3 ) ) {
						MessageBox.Show( "3 points are colinear" );
						return;
					}

					// create 3-point coordinate system
					VectorTool.Create3PCoordSystem( m_1stPoint, m_2ndPoint, m_3rdPoint, out m_3PCoordSys );
					if( !Show3PtTransformDialog() ) {
						End();
						return;
					}

					// final transformation
					TransformHelper transformHelper = new TransformHelper( m_Viewer, m_CADManager, m_ViewManager, m_3PTransform );
					transformHelper.TransformData();
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

		bool Show3PtTransformDialog()
		{
			// Create and show the 3-point transform dialog
			ThreePtTransformDlg dialog = new ThreePtTransformDlg( m_1stPoint, m_2ndPoint, m_3rdPoint );
			dialog.ShowDialog();
			if( dialog.DialogResult != DialogResult.OK ) {
				return false;
			}

			// get point from dialog
			bool isValid = dialog.GetMachinePoint( out gp_Pnt machineP1, out gp_Pnt machineP2, out gp_Pnt machineP3 );
			if( !isValid ) {
				MessageBox.Show( "Invalid machine points.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}

			// create 3-point coordinate system
			if( !VectorTool.Create3PCoordSystem( machineP1, machineP2, machineP3, out gp_Ax3 machineCoordSys ) ) {
				MessageBox.Show( "Failed to create 3-point coordinate system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return false;
			}

			// create transformation
			m_3PTransform = new gp_Trsf();
			m_3PTransform.SetDisplacement( m_3PCoordSys, machineCoordSys );
			return true;
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
		gp_Trsf m_3PTransform;
		AIS_TextLabel m_LabelO;
		AIS_TextLabel m_LabelX;
	}
}
