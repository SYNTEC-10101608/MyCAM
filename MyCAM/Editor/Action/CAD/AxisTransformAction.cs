using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using OCC.AIS;
using OCC.BRep;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal enum EAxisTransformDirection
	{
		XAxis = 0,
		YAxis = 1,
		ZAxis = 2,
	}

	internal class AxisTransformAction : KeyMouseActionBase
	{
		public AxisTransformAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			CreateRotationCenter();
			CreateManipulator();
			m_OneTimeTrsf = new gp_Trsf();
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

			// clear selection to prevent hidden or non-compound objects from being transformed
			m_Viewer.GetAISContext().ClearSelected( false );

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all visible parts
			// hide all paths
			m_ViewManager.EraseAll();

			ShowManipulationShape();
			m_Viewer.UpdateView();

			// new transform dialog
			m_AxisTransformDlg = new AxisTransformDlg();
			m_AxisTransformDlg.Show( MyApp.MainForm );

			// register event
			m_AxisTransformDlg.Displace += Displacement;
			m_AxisTransformDlg.Rotate += Rotatation;
			m_AxisTransformDlg.FormConfirm += OnConfirm;
			m_AxisTransformDlg.FormCancel += OnCancel;
			m_AxisTransformDlg.FormReset += OnReset;
		}

		public override void End()
		{
			// close dialog if still open (suppress event to avoid re-entry)
			if( m_AxisTransformDlg != null ) {
				m_AxisTransformDlg.Displace -= Displacement;
				m_AxisTransformDlg.Rotate -= Rotatation;
				m_AxisTransformDlg.FormConfirm -= OnConfirm;
				m_AxisTransformDlg.FormCancel -= OnCancel;
				m_AxisTransformDlg.FormReset -= OnReset;
				m_AxisTransformDlg.Close();
				m_AxisTransformDlg = null;
			}

			// enable tree view
			m_TreeView.Enabled = true;

			// show all visible parts
			foreach( string szID in m_DataManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible ) {
					m_ViewManager.DisplayPart( szID );
					m_ViewManager.DeactivePart( szID );
				}
			}

			// show all paths (TransformHelper already updated path geometry)
			m_ViewManager.DisplayPaths( m_DataManager.PathIDList );
			m_ViewManager.DeactivePaths( m_DataManager.PathIDList );

			HideManipulationShape();
			m_Viewer.UpdateView();
			base.End();
		}

		void OnConfirm()
		{
			ApplyTransform( m_trsf );
			End();
		}

		void OnCancel()
		{
			End();
		}

		void OnReset()
		{
			// reset reference shape transformation to identity for correct preview
			m_RefAISShape.SetLocalTransformation( new gp_Trsf() );

			// Reset accumulated transform
			m_trsf = new gp_Trsf();

			// Reset manipulator axes to initial state (same as G54)
			m_RotationBasis = new gp_Ax2( m_G54Origin, new gp_Dir( 0, 0, 1 ) );
			ResetManipulatorToG54();

			// Reset dialog values
			m_AxisTransformDlg?.ResetAllValues();

			m_Viewer.UpdateView();
		}

		void Rotatation( decimal value, EAxisTransformDirection axis )
		{
			gp_Trsf trsf = new gp_Trsf();
			gp_Dir dir = new gp_Dir();
			switch( axis ) {
				case EAxisTransformDirection.XAxis:
					dir = m_RotationBasis.XDirection();
					break;
				case EAxisTransformDirection.YAxis:
					dir = m_RotationBasis.YDirection();
					break;
				case EAxisTransformDirection.ZAxis:
					dir = m_RotationBasis.Direction();
					break;
				default:
					dir = m_RotationBasis.XDirection();
					break;
			}
			gp_Ax1 axl = new gp_Ax1( m_G54Origin, dir );
			trsf.SetRotation( axl, (double)value * Math.PI / 180 );
			PreviewAndAccumulateTransform( trsf );
			UpdateManipulatorAxes( trsf );
		}

		void Displacement( decimal value, EAxisTransformDirection axis )
		{
			gp_Vec vec = new gp_Vec( 0, 0, 0 );
			switch( axis ) {
				case EAxisTransformDirection.XAxis:
					vec = new gp_Vec( (double)value, 0, 0 );
					break;
				case EAxisTransformDirection.YAxis:
					vec = new gp_Vec( 0, (double)value, 0 );
					break;
				case EAxisTransformDirection.ZAxis:
					vec = new gp_Vec( 0, 0, (double)value );
					break;
				default:
					vec = new gp_Vec( 1, 0, 0 );
					break;
			}
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetTranslation( vec );
			PreviewAndAccumulateTransform( trsf );
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_Manipulator.StartTransform( e.X, e.Y, m_Viewer.GetView() );
					m_Manipulator.SetModeActivationOnDetection( false );

					// record active mode and axis index for syncing manipulator changes to dialog in real time
					m_ActiveManipulatorMode = m_Manipulator.ActiveMode();
					m_ActiveAxisIndex = m_Manipulator.ActiveAxisIndex();
					m_PrevManipulatorTrsf = new gp_Trsf();
				}
			}
		}

		protected override void ViewerMouseMove( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( m_Manipulator.HasActiveMode() ) {
					m_OneTimeTrsf = m_Manipulator.StepsTransform( e.X, e.Y, m_Viewer.GetView() );

					// force manipulator position to stay at G54 origin while dragging
					if( m_ActiveManipulatorMode == AIS_ManipulatorMode.AIS_MM_Translation ) {
						m_Manipulator.SetPosition( m_RotationBasis );
					}

					m_Viewer.GetView().Redraw();

					// sync manipulator changes to dialog in real time
					SyncManipulatorToDialog( m_OneTimeTrsf );
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

				// add to m_trsf
				m_trsf.PreMultiply( m_OneTimeTrsf );

				// reset manipulator position and axes to match the updated G54 after transformation
				if( m_ActiveManipulatorMode == AIS_ManipulatorMode.AIS_MM_Translation ) {
					ResetManipulatorToG54();
				}
				else if( m_ActiveManipulatorMode == AIS_ManipulatorMode.AIS_MM_Rotation ) {
					UpdateManipulatorAxes( m_OneTimeTrsf );
					ResetManipulatorToG54();
				}

				// reset one time transform beacuse there is a bug in AIS_Manipulator class 
				// DeactivateCurrentMode and HasActiveMode do not work correctly together
				m_OneTimeTrsf = new gp_Trsf();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				OnCancel();
			}
		}

		void UpdateManipulatorAxes( gp_Trsf rotationTrsf )
		{
			// Compute rotation basis after rotation
			// Compute Z and X direction, Y direction can be derived from Z and X direction, so no need to calculate it explicitly
			gp_Dir newMain = m_RotationBasis.Direction().Transformed( rotationTrsf );
			gp_Dir newXDir = m_RotationBasis.XDirection().Transformed( rotationTrsf );

			// Rebuild coordinate
			m_RotationBasis = new gp_Ax2( m_G54Origin, newMain, newXDir );

			// Redisplay manipulator with updated axes
			m_Manipulator.SetPosition( m_RotationBasis );
			m_Viewer.GetAISContext().Redisplay( m_Manipulator, true );
		
		}

		void ResetManipulatorToG54()
		{
			m_Manipulator.SetPosition( m_RotationBasis );
			m_Viewer.GetAISContext().Redisplay( m_Manipulator, true );
		}

		void SyncManipulatorToDialog( gp_Trsf currentTrsf )
		{
			if( m_AxisTransformDlg == null ) {
				return;
			}

			if( m_ActiveManipulatorMode == AIS_ManipulatorMode.AIS_MM_Translation ) {
				gp_XYZ curTranslation = currentTrsf.TranslationPart();
				gp_XYZ prevTranslation = m_PrevManipulatorTrsf.TranslationPart();
				decimal dx = (decimal)( curTranslation.X() - prevTranslation.X() );
				decimal dy = (decimal)( curTranslation.Y() - prevTranslation.Y() );
				decimal dz = (decimal)( curTranslation.Z() - prevTranslation.Z() );

				if( dx != 0 || dy != 0 || dz != 0 ) {
					m_AxisTransformDlg.UpdateDisplacementFromManipulator( dx, dy, dz );
				}
			}
			else if( m_ActiveManipulatorMode == AIS_ManipulatorMode.AIS_MM_Rotation ) {
				double curAngle = ExtractRotationAngleDeg( currentTrsf, m_ActiveAxisIndex );
				double prevAngle = ExtractRotationAngleDeg( m_PrevManipulatorTrsf, m_ActiveAxisIndex );
				decimal deltaAngle = (decimal)( curAngle - prevAngle );

				if( deltaAngle != 0 ) {
					switch( (EAxisTransformDirection) m_ActiveAxisIndex ) {
						case EAxisTransformDirection.XAxis:
							m_AxisTransformDlg.UpdateRotationFromManipulator( deltaAngle, 0, 0 );
							break;
						case EAxisTransformDirection.YAxis:
							m_AxisTransformDlg.UpdateRotationFromManipulator( 0, deltaAngle, 0 );
							break;
						case EAxisTransformDirection.ZAxis:
							m_AxisTransformDlg.UpdateRotationFromManipulator( 0, 0, deltaAngle );
							break;
					}
				}
			}

			m_PrevManipulatorTrsf = currentTrsf;
		}

		double ExtractRotationAngleDeg( gp_Trsf trsf, int axisIndex )
		{
			double angleRad = 0;
			switch( (EAxisTransformDirection)axisIndex ) {
				case EAxisTransformDirection.XAxis: 
					angleRad = Math.Atan2( trsf.Value( 3, 2 ), trsf.Value( 2, 2 ) );
					break;
				case EAxisTransformDirection.YAxis:
					angleRad = Math.Atan2( trsf.Value( 1, 3 ), trsf.Value( 1, 1 ) );
					break;
				case EAxisTransformDirection.ZAxis:
					angleRad = Math.Atan2( trsf.Value( 2, 1 ), trsf.Value( 1, 1 ) );
					break;
			}
			return angleRad * 180.0 / Math.PI;
		}

		void ApplyTransform( gp_Trsf trsf )
		{
			if( trsf == null ) {
				return;
			}
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );
			transformHelper.TransformData();
		}

		void ShowManipulationShape()
		{
			m_Viewer.GetAISContext().Display( m_RefAISShape, false );
			m_Viewer.GetAISContext().Deactivate( m_RefAISShape );
			m_Manipulator.Attach( m_RefAISShape );
			m_Manipulator.SetPosition( m_RotationBasis );
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
				if( !DataGettingHelper.GetShapeObject( szID, out IShapeObject shapeObj ) ) {
					continue;
				}
				shpaeList.Add( shapeObj.Shape );
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

			// display the compound shape as reference
			m_RefAISShape = ViewHelper.CreatePartAIS( compound );
		}
		void PreviewAndAccumulateTransform( gp_Trsf trsf )
		{
			// just for instant preview on the displayed reference shape
			m_RefAISShape.SetLocalTransformation( trsf.Multiplied( m_RefAISShape.LocalTransformation() ) );

			// accumulate transform
			m_trsf.PreMultiply( trsf );

			m_Viewer.UpdateView();
		}

		void CreateManipulator()
		{
			// setting manipulator attributes
			m_Manipulator = new AIS_Manipulator();
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Translation, true );
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Rotation, true );
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_Scaling, false );
			m_Manipulator.SetPart( AIS_ManipulatorMode.AIS_MM_TranslationPlane, false );
			m_Manipulator.SetPosition( m_RotationBasis );
			m_Manipulator.EnableMode( AIS_ManipulatorMode.AIS_MM_Translation );
			m_Manipulator.EnableMode( AIS_ManipulatorMode.AIS_MM_Rotation );
			m_Manipulator.SetRotationSteps( STEP_ROTATION_ANGLE_DEG * Math.PI / 180.0 );
		}

		// G54 origin
		readonly gp_Pnt m_G54Origin = new gp_Pnt( 0, 0, 0 );

		// manipulator rotation basis, initialized to be the same as G54, but will be updated after each rotation to provide better user experience for subsequent rotations
		gp_Ax2 m_RotationBasis = new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );

		gp_Trsf m_trsf = new gp_Trsf();
		gp_Trsf m_OneTimeTrsf;
		gp_Trsf m_PrevManipulatorTrsf = new gp_Trsf();
		AIS_Manipulator m_Manipulator;
		AIS_Shape m_RefAISShape;
		AxisTransformDlg m_AxisTransformDlg;
		AIS_ManipulatorMode m_ActiveManipulatorMode;
		int m_ActiveAxisIndex;
		const double STEP_ROTATION_ANGLE_DEG = 1;
	}
}
