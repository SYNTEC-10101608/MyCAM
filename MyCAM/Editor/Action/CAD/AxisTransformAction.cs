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
		XAxis,
		YAxis,
		ZAxis,
	}

	internal class AxisTransformAction : KeyMouseActionBase
	{
		public AxisTransformAction( DataManager cadManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( cadManager, viewer, treeView, viewManager )
		{
			CreateRotationCenter();
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

			// new transform dialog
			AxisTransformDlg axisTransformDlg = new AxisTransformDlg();
			axisTransformDlg.Show( MyApp.MainForm );

			// register event
			axisTransformDlg.Displace += Displacement;
			axisTransformDlg.Reset += Displacement;
			axisTransformDlg.Rotate += Rotatation;
			axisTransformDlg.ResetR += Rotatation;
			axisTransformDlg.FormClose += TransformDone;
		}

		public override void End()
		{
			// enable tree view
			m_TreeView.Enabled = true;
			base.End();
		}

		void Rotatation( decimal value, EAxisTransformDirection axis )
		{
			gp_Pnt position = m_RotationCenter.Location();
			gp_Trsf trsf = new gp_Trsf();
			gp_Dir dir = new gp_Dir();
			switch( axis ) {
				case EAxisTransformDirection.XAxis:
					dir = new gp_Dir( 1, 0, 0 );
					break;
				case EAxisTransformDirection.YAxis:
					dir = new gp_Dir( 0, 1, 0 );
					break;
				case EAxisTransformDirection.ZAxis:
					dir = new gp_Dir( 0, 0, 1 );
					break;
				default:
					dir = new gp_Dir( 1, 0, 0 );
					break;
			}
			gp_Ax1 axl = new gp_Ax1( position, dir );
			trsf.SetRotation( axl, (double)value * Math.PI / 180 );
			PreviewAndAccumulateTransform( trsf );
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
			m_RotationCenter.Translate( vec );
			PreviewAndAccumulateTransform( trsf );
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			// do nothing
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			// do nothing
		}

		protected override void TreeViewAfterSelect( object sender, TreeViewEventArgs e )
		{
			// do nothing
		}

		protected override void TreeViewKeyDown( object sender, KeyEventArgs e )
		{
			// do nothing
		}

		public void TransformDone()
		{
			ResetLocalTransformation();
			ApplyTransform( m_trsf );
			End();
		}

		void ApplyTransform( gp_Trsf trsf )
		{
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_CADManager, m_ViewManager, trsf );
			transformHelper.TransformData();
		}

		void TransfromDisplayedShape( gp_Trsf trsf )
		{
			foreach( string szID in m_CADManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible == false ) {
					continue;
				}
				AIS_Shape oneAIS = AIS_Shape.DownCast( m_ViewManager.ViewObjectMap[ szID ].AISHandle );
				oneAIS.SetLocalTransformation( trsf.Multiplied( oneAIS.LocalTransformation() ) );
			}
		}

		void ResetLocalTransformation()
		{
			foreach( string szID in m_CADManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible == false ) {
					continue;
				}
				AIS_Shape oneAIS = AIS_Shape.DownCast( m_ViewManager.ViewObjectMap[ szID ].AISHandle );
				oneAIS.ResetTransformation();
			}
		}

		void CreateRotationCenter()
		{
			List<TopoDS_Shape> shpaeList = new List<TopoDS_Shape>();
			foreach( string szID in m_CADManager.PartIDList ) {
				ViewObject viewObject = m_ViewManager.ViewObjectMap[ szID ];
				if( viewObject.Visible == false ) {
					continue;
				}
				shpaeList.Add( m_CADManager.ShapeDataMap[ szID ].Shape );
			}
			if( shpaeList == null || shpaeList.Count == 0 ) {
				return;
			}
			TopoDS_Compound compound = new TopoDS_Compound();
			TopoDS_Shape compoundShape = compound;
			BRep_Builder builder = new BRep_Builder();
			builder.MakeCompound( ref compound );
			foreach( TopoDS_Shape shape in shpaeList ) {
				builder.Add( ref compoundShape, shape );
			}

			// Set the manipulator position to the center of the bounding box
			BoundingBox boundingBox = new BoundingBox( compoundShape );
			m_RotationCenter = new gp_Ax2( new gp_Pnt( boundingBox.XCenter, boundingBox.YCenter, boundingBox.ZCenter ), new gp_Dir( 0, 0, 1 ) );
		}

		void PreviewAndAccumulateTransform( gp_Trsf trsf )
		{
			// just for instant preview
			TransfromDisplayedShape( trsf );
			m_Viewer.UpdateView();

			// accumulate transform
			m_trsf.PreMultiply( trsf );
		}

		gp_Ax2 m_RotationCenter = new gp_Ax2( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
		gp_Trsf m_trsf = new gp_Trsf();
	}
}
