using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.CAD
{
	internal class ManualTransformAction : CADACtionBase
	{
		public ManualTransformAction( Viewer viewer, TreeView treeView, CADManager cadManager )
			: base( viewer, treeView, cadManager )
		{
			// make a coordinate system as reference
			MakeG54Coord();

			// make a compound shape as transform target
			MakePartShape();
		}

		public override CADActionType ActionType
		{
			get
			{
				return CADActionType.ManualTransform;
			}
		}

		public override void Start()
		{
			base.Start();

			// disable tree view
			m_TreeView.Enabled = false;

			// hide all shape
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Remove( viewObject.AISHandle, false );
			}

			// show transform part and G54 coordinate system
			ShowPart();
			ShowG54Coord();

			// activate edge and face selection
			m_Viewer.GetAISContext().Activate( m_PartAIS, (int)AISActiveMode.Edge );
			m_Viewer.GetAISContext().Activate( m_PartAIS, (int)AISActiveMode.Face );
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// enable tree view
			m_TreeView.Enabled = true;

			// hide part and G54 coordinate system
			HidePart();
			HideG54Coord();

			// show all shape
			foreach( ViewObject viewObject in m_CADManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Display( viewObject.AISHandle, false );
				if( !viewObject.Visible ) {
					m_Viewer.GetAISContext().Erase( viewObject.AISHandle, false );
				}
			}
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				m_Viewer.ShiftSelect();
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				m_Viewer.GetAISContext().ClearSelected( true );
			}
		}

		public void ApplyTransform( EConstraintType type, bool bReverse = false )
		{
			SetConstraint( type, bReverse );
		}

		public void TransformDone()
		{
			foreach( var oneData in m_CADManager.ShapeDataMap ) {
				if( m_CADManager.ViewObjectMap.ContainsKey( oneData.Key ) ) {
					AIS_Shape oneAIS = AIS_Shape.DownCast( m_CADManager.ViewObjectMap[ oneData.Key ].AISHandle );
					if( oneAIS == null || oneAIS.IsNull() ) {
						continue;
					}
					oneAIS.SetShape( oneData.Value.Shape );
				}
			}
			End();
		}

		void MakePartShape()
		{
			// TODO: u may not need to make a extra part, use the raw shape directly
			List<TopoDS_Shape> visibleShapeList = new List<TopoDS_Shape>();
			foreach( var oneData in m_CADManager.ShapeDataMap ) {
				if( m_CADManager.ViewObjectMap.ContainsKey( oneData.Key ) && m_CADManager.ViewObjectMap[ oneData.Key ].Visible ) {
					visibleShapeList.Add( oneData.Value.Shape );
				}
			}
			m_PartShape = ShapeTool.MakeCompound( visibleShapeList );
			if( m_PartShape == null || m_PartShape.IsNull() ) {
				throw new ArgumentNullException( "TransformAction part shape is null." );
			}
			m_PartAIS = ViewHelper.CreatePartAIS( m_PartShape );
		}

		void MakeG54Coord()
		{
			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			m_G54AISList = new List<AIS_Shape>();

			// XY plane
			const int SIZE = 20;
			gp_Pnt p1 = new gp_Pnt( SIZE / 2, SIZE / 2, 0 );
			gp_Pnt p2 = new gp_Pnt( -SIZE / 2, SIZE / 2, 0 );
			gp_Pnt p3 = new gp_Pnt( -SIZE / 2, -SIZE / 2, 0 );
			gp_Pnt p4 = new gp_Pnt( SIZE / 2, -SIZE / 2, 0 );
			BRepBuilderAPI_MakeEdge edge12 = new BRepBuilderAPI_MakeEdge( p1, p2 );
			BRepBuilderAPI_MakeEdge edge23 = new BRepBuilderAPI_MakeEdge( p2, p3 );
			BRepBuilderAPI_MakeEdge edge34 = new BRepBuilderAPI_MakeEdge( p3, p4 );
			BRepBuilderAPI_MakeEdge edge41 = new BRepBuilderAPI_MakeEdge( p4, p1 );
			BRepBuilderAPI_MakeWire wire = new BRepBuilderAPI_MakeWire( edge12.Edge(), edge23.Edge(), edge34.Edge(), edge41.Edge() );
			BRepBuilderAPI_MakeFace faceXY = new BRepBuilderAPI_MakeFace( wire.Wire() );
			shapeList.Add( faceXY.Face() );

			// YZ plane
			gp_Trsf trsfYZ = new gp_Trsf();
			trsfYZ.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 1, 0 ) ), Math.PI / 2 );
			BRepBuilderAPI_Transform trasformYZ = new BRepBuilderAPI_Transform( faceXY.Face(), trsfYZ );
			shapeList.Add( trasformYZ.Shape() );

			// XZ plane
			gp_Trsf trsfXZ = new gp_Trsf();
			trsfXZ.SetRotation( new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( -1, 0, 0 ) ), Math.PI / 2 );
			BRepBuilderAPI_Transform trasformXZ = new BRepBuilderAPI_Transform( faceXY.Face(), trsfXZ );
			shapeList.Add( trasformXZ.Shape() );

			// X axis
			gp_Pnt pX0 = new gp_Pnt( 0, 0, 0 );
			gp_Pnt pX1 = new gp_Pnt( SIZE, 0, 0 );
			BRepBuilderAPI_MakeEdge edgeX = new BRepBuilderAPI_MakeEdge( pX0, pX1 );
			shapeList.Add( edgeX.Edge() );

			// Y axis
			gp_Pnt pY0 = new gp_Pnt( 0, 0, 0 );
			gp_Pnt pY1 = new gp_Pnt( 0, SIZE, 0 );
			BRepBuilderAPI_MakeEdge edgeY = new BRepBuilderAPI_MakeEdge( pY0, pY1 );
			shapeList.Add( edgeY.Edge() );

			// Z axis
			gp_Pnt pZ0 = new gp_Pnt( 0, 0, 0 );
			gp_Pnt pZ1 = new gp_Pnt( 0, 0, SIZE );
			BRepBuilderAPI_MakeEdge edgeZ = new BRepBuilderAPI_MakeEdge( pZ0, pZ1 );
			shapeList.Add( edgeZ.Edge() );

			// make G54 shape
			m_G54Shape = ShapeTool.MakeCompound( shapeList );

			// XY AIS
			AIS_Shape aisXY = new AIS_Shape( faceXY.Face() );
			aisXY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			m_G54AISList.Add( aisXY );

			// YZ AIS
			AIS_Shape aisYZ = new AIS_Shape( trasformYZ.Shape() );
			aisYZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_G54AISList.Add( aisYZ );

			// XZ AIS
			AIS_Shape aisXZ = new AIS_Shape( trasformXZ.Shape() );
			aisXZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_G54AISList.Add( aisXZ );

			// X AIS
			AIS_Shape aisX = new AIS_Shape( edgeX.Edge() );
			aisX.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			m_G54AISList.Add( aisX );

			// Y AIS
			AIS_Shape aisY = new AIS_Shape( edgeY.Edge() );
			aisY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			m_G54AISList.Add( aisY );

			// Z AIS
			AIS_Shape aisZ = new AIS_Shape( edgeZ.Edge() );
			aisZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			m_G54AISList.Add( aisZ );

			// set properties
			foreach( AIS_Shape ais in m_G54AISList ) {
				ais.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
				ais.SetTransparency( 0.9f );
			}
		}

		void ShowPart()
		{
			m_Viewer.GetAISContext().Display( m_PartAIS, false );
		}

		void HidePart()
		{
			m_Viewer.GetAISContext().Remove( m_PartAIS, false );
		}

		void ShowG54Coord()
		{
			foreach( AIS_Shape ais in m_G54AISList ) {
				m_Viewer.GetAISContext().Display( ais, false );
			}
		}

		void HideG54Coord()
		{
			foreach( AIS_Shape ais in m_G54AISList ) {
				m_Viewer.GetAISContext().Remove( ais, false );
			}
		}

		void GetRefAndMoveObject( out TopoDS_Shape refShape, out TopoDS_Shape moveShape )
		{
			refShape = null;
			moveShape = null;
			m_Viewer.GetAISContext().InitSelected();
			int test = m_Viewer.GetAISContext().NbSelected();
			int nCount = 0;
			List<TopoDS_Shape> selectedShapeList = new List<TopoDS_Shape>();
			while( m_Viewer.GetAISContext().MoreSelected() ) {
				nCount++;
				if( nCount > 2 ) {
					return;
				}
				selectedShapeList.Add( m_Viewer.GetAISContext().SelectedShape() );
				m_Viewer.GetAISContext().NextSelected();
			}
			if( nCount != 2 ) {
				return;
			}
			CheckRefOrMoveObject( selectedShapeList[ 0 ], out bool isRef1, out bool isMove1 );
			CheckRefOrMoveObject( selectedShapeList[ 1 ], out bool isRef2, out bool isMove2 );
			if( isRef1 && isMove2 ) {
				refShape = selectedShapeList[ 0 ];
				moveShape = selectedShapeList[ 1 ];
			}
			else if( isRef2 && isMove1 ) {
				refShape = selectedShapeList[ 1 ];
				moveShape = selectedShapeList[ 0 ];
			}
			else {
				return;
			}
		}

		void CheckRefOrMoveObject( TopoDS_Shape sel, out bool isRef, out bool isMove )
		{
			isRef = false;
			isMove = false;
			if( sel == null ) {
				return;
			}
			if( sel.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				TopExp_Explorer expRef = new TopExp_Explorer( m_G54Shape, TopAbs_ShapeEnum.TopAbs_FACE );
				while( expRef.More() ) {
					TopoDS_Shape face = expRef.Current();
					if( sel.IsEqual( face ) ) {
						isRef = true;
						return;
					}
					expRef.Next();
				}
				TopExp_Explorer expMove = new TopExp_Explorer( m_PartShape, TopAbs_ShapeEnum.TopAbs_FACE );
				while( expMove.More() ) {
					TopoDS_Shape face = expMove.Current();
					if( sel.IsEqual( face ) ) {
						isMove = true;
						return;
					}
					expMove.Next();
				}
			}
			else if( sel.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {
				TopExp_Explorer expRef = new TopExp_Explorer( m_G54Shape, TopAbs_ShapeEnum.TopAbs_EDGE );
				while( expRef.More() ) {
					TopoDS_Shape face = expRef.Current();
					if( sel.IsEqual( face ) ) {
						isRef = true;
						return;
					}
					expRef.Next();
				}
				TopExp_Explorer expMove = new TopExp_Explorer( m_PartShape, TopAbs_ShapeEnum.TopAbs_EDGE );
				while( expMove.More() ) {
					TopoDS_Shape face = expMove.Current();
					if( sel.IsEqual( face ) ) {
						isMove = true;
						return;
					}
					expMove.Next();
				}
			}
		}

		void SetConstraint( EConstraintType type, bool isReverse )
		{
			GetRefAndMoveObject( out TopoDS_Shape refShape, out TopoDS_Shape moveShape );
			if( refShape == null || moveShape == null ) {
				return;
			}

			// create constraint
			IConstraint c = null;
			switch( type ) {
				case EConstraintType.Axial:
					c = new AxialConstraint( refShape, moveShape, isReverse );
					break;
				case EConstraintType.AxialParallel:
					c = new AxialParallelConstraint( refShape, moveShape, isReverse );
					break;
				case EConstraintType.Plane:
					c = new PlaneConstraint( refShape, moveShape, isReverse );
					break;
				case EConstraintType.PlaneParallel:
					c = new PlaneParallelConstraint( refShape, moveShape, isReverse );
					break;
				default:
					return;
			}
			if( !c.IsValid() ) {
				MessageBox.Show( "Invalid constraint. Please select valid ref and move objects." );
				return;
			}

			// transform preview shape
			gp_Trsf trsf = c.SolveConstraint();
			BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( m_PartShape, trsf );
			m_PartShape = transform.Shape();

			// transform all CAD shapes
			foreach( var oneData in m_CADManager.ShapeDataMap ) {
				BRepBuilderAPI_Transform oneTransform = new BRepBuilderAPI_Transform( oneData.Value.Shape, trsf );
				oneData.Value.Shape = oneTransform.Shape();
			}

			// update view
			m_PartAIS.SetShape( m_PartShape );
			m_Viewer.GetAISContext().Redisplay( m_PartAIS, false );
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.UpdateView();
		}

		TopoDS_Shape m_G54Shape;
		TopoDS_Shape m_PartShape;
		List<AIS_Shape> m_G54AISList;
		AIS_Shape m_PartAIS;
	}
}
