using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRep;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class AxisTransformAction : KeyMouseActionBase
	{
		public AxisTransformAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			CreateRotationCenter();
			CreateManipulator();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.AxisTransform;
			}
		}

		public override void Start()
		{
			base.Start();

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all visible parts
			foreach( string szID in m_DataManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Erase( viewObject.AISHandle, false );
			}
			ShowManipulationShape();
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// enable tree view
			m_TreeView.Enabled = true;

			// show all visible parts
			foreach( string szID in m_DataManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible ) {
					m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
					m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
				}
			}
			HideManipulationShape();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_Manipulator.StartTransform( e.X, e.Y, m_Viewer.GetView() );
					m_Manipulator.SetModeActivationOnDetection( false );
				}
			}
		}

		protected override void ViewerMouseMove( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_OneTimeTrsf = m_Manipulator.StepsTransform( e.X, e.Y, m_Viewer.GetView() );
					m_Viewer.GetView().Redraw();
				}
			}
		}

		protected override void ViewerMouseUp( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_Manipulator.StopTransform( true );
					m_Manipulator.SetModeActivationOnDetection( true );
					m_Manipulator.DeactivateCurrentMode();
				}
				ApplyTransform( m_OneTimeTrsf );

				// reset one time transform beacuse there is a bug in AIS_Manipulator class 
				// DeactivateCurrentMode and HasActiveMode do not work correctly together
				m_OneTimeTrsf = new gp_Trsf();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		void ApplyTransform( gp_Trsf trsf )
		{
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );
			transformHelper.TransformData();
		}

		void ShowManipulationShape()
		{
			m_Viewer.GetAISContext().Display( m_RefAISShape, false );
			m_Viewer.GetAISContext().Deactivate( m_RefAISShape );
			m_Manipulator.Attach( m_RefAISShape );
			m_Manipulator.SetModeActivationOnDetection( true );
		}

		void HideManipulationShape()
		{
			m_Manipulator.Detach();
			m_Viewer.GetAISContext().Remove( m_RefAISShape, false );
		}

		void CreateRotationCenter()
		{
			List<TopoDS_Shape> shpaeList = new List<TopoDS_Shape>();
			foreach( string szID in m_DataManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible == false ) {
					continue;
				}
				shpaeList.Add( m_DataManager.ShapeDataMap[ szID ].Shape );
			}
			if( shpaeList == null || shpaeList.Count == 0 ) {
				return;
			}

			// create a compound shape for all visible shapes
			TopoDS_Compound compound = new TopoDS_Compound();
			TopoDS_Shape compoundShape = compound;
			BRep_Builder builder = new BRep_Builder();
			builder.MakeCompound( ref compound );
			foreach( TopoDS_Shape shape in shpaeList ) {
				builder.Add( ref compoundShape, shape );
			}

			// set the manipulator position to the center of the bounding box
			BoundingBox boundingBox = new BoundingBox( compoundShape );
			m_RotationCenter = new gp_Ax2( new gp_Pnt( boundingBox.XCenter, boundingBox.YCenter, boundingBox.ZCenter ), new gp_Dir( 0, 0, 1 ) );

			// display the compound shape as reference
			m_RefAISShape = ViewHelper.CreatePartAIS( compound );
		}

		void CreateManipulator()
		{
			// setting manipulator attributes
			m_Manipulator = new AIS_Manipulator();
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Translation, true );
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Rotation, true );
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Scaling, false );
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_TranslationPlane, false );
			m_Manipulator.SetPosition( m_RotationCenter );
			m_Manipulator.EnableMode( AIS_ManipulatorMode.AIS_MM_Translation );
			m_Manipulator.EnableMode( AIS_ManipulatorMode.AIS_MM_Rotation );
			m_Manipulator.SetRotationSteps( STEP_ROTATION_ANGLE_DEG * Math.PI / 180.0 );
		}

		gp_Ax2 m_RotationCenter;
		gp_Trsf m_OneTimeTrsf;
		AIS_Manipulator m_Manipulator;
		AIS_Shape m_RefAISShape;
		const double STEP_ROTATION_ANGLE_DEG = 15;
	}
}
