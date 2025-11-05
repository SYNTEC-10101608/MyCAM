using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;

namespace MyCAM.Editor
{
	internal enum ETrsfConstraintType
	{
		Axial,
		AxialParallel,
		Plane,
		PlaneParallel,
		Point,
	}

	internal class ManualTransformAction : KeyMouseActionBase
	{
		public ManualTransformAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			// make a coordinate system as reference
			MakeG54Coord();
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ManualTransform;
			}
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// disable tree view
			m_TreeView.Enabled = false;

			// activate
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Edge );
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Face );
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)AISActiveMode.Vertex );
			}

			// show transform part and G54 coordinate system
			ShowG54Coord();
			m_Viewer.UpdateView();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// enable tree view
			m_TreeView.Enabled = true;

			// deactivate
			foreach( ViewObject viewObject in m_ViewManager.ViewObjectMap.Values ) {
				m_Viewer.GetAISContext().Deactivate( viewObject.AISHandle );
			}

			// hide part and G54 coordinate system
			HideG54Coord();
			m_Viewer.UpdateView();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) {
				if( ( Control.ModifierKeys & Keys.Control ) == Keys.Control ) {
					m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_XOR );
				}
				else {
					m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_Replace );
				}
			}
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		public void ApplyTransform( ETrsfConstraintType type )
		{
			SetConstraint( type );
		}

		public void TransformDone()
		{
			End();
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


			gp_Pnt oriPnt = new gp_Pnt( 0, 0, 0 );
			BRepBuilderAPI_MakeVertex oriVertex = new BRepBuilderAPI_MakeVertex( oriPnt );
			shapeList.Add( oriVertex.Vertex() );

			// make G54 shape
			m_G54Shape = ShapeTool.MakeCompound( shapeList );

			// XY AIS
			AIS_Shape aisXY = new AIS_Shape( faceXY.Face() );
			aisXY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			aisXY.Attributes().SetFaceBoundaryDraw( true );
			aisXY.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			m_G54AISList.Add( aisXY );

			// YZ AIS
			AIS_Shape aisYZ = new AIS_Shape( trasformYZ.Shape() );
			aisYZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			aisYZ.Attributes().SetFaceBoundaryDraw( true );
			aisYZ.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			m_G54AISList.Add( aisYZ );

			// XZ AIS
			AIS_Shape aisXZ = new AIS_Shape( trasformXZ.Shape() );
			aisXZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			aisXZ.Attributes().SetFaceBoundaryDraw( true );
			aisXZ.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLACK ) );
			m_G54AISList.Add( aisXZ );

			// X AIS
			AIS_Shape aisX = new AIS_Shape( edgeX.Edge() );
			aisX.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
			aisX.SetWidth( 2.0f ); // make X axis thicker
			m_G54AISList.Add( aisX );

			// Y AIS
			AIS_Shape aisY = new AIS_Shape( edgeY.Edge() );
			aisY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ) );
			aisY.SetWidth( 2.0f ); // make Y axis thicker
			m_G54AISList.Add( aisY );

			// Z AIS
			AIS_Shape aisZ = new AIS_Shape( edgeZ.Edge() );
			aisZ.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			aisZ.SetWidth( 2.0f ); // make Z axis thicker
			m_G54AISList.Add( aisZ );

			// original
			AIS_Shape aisOri = new AIS_Shape( oriVertex.Vertex() );
			aisOri.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BROWN3 ) );
			aisOri.Attributes().PointAspect().SetTypeOfMarker( Aspect_TypeOfMarker.Aspect_TOM_BALL );
			aisOri.Attributes().PointAspect().SetScale( 3.0f );
			m_G54AISList.Add( aisOri );

			// set properties
			foreach( AIS_Shape ais in m_G54AISList ) {
				ais.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
				ais.SetTransparency( 0.7f );
			}
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
				foreach( var partID in m_DataManager.PartIDList ) {

					// skip invisible objects
					if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
						continue;
					}
					TopExp_Explorer expMove = new TopExp_Explorer( m_DataManager.ShapeDataMap[ partID ].Shape, TopAbs_ShapeEnum.TopAbs_FACE );
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
				foreach( var partID in m_DataManager.PartIDList ) {

					// skip invisible objects
					if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
						continue;
					}
					TopExp_Explorer expMove = new TopExp_Explorer( m_DataManager.ShapeDataMap[ partID ].Shape, TopAbs_ShapeEnum.TopAbs_EDGE );
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
			else if( sel.ShapeType() == TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				TopExp_Explorer expRef = new TopExp_Explorer( m_G54Shape, TopAbs_ShapeEnum.TopAbs_VERTEX );
				while( expRef.More() ) {
					TopoDS_Shape vertex = expRef.Current();
					if( sel.IsEqual( vertex ) ) {
						isRef = true;
						return;
					}
					expRef.Next();
				}
				foreach( var partID in m_DataManager.PartIDList ) {

					// skip invisible objects
					if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
						continue;
					}
					TopExp_Explorer expMove = new TopExp_Explorer( m_DataManager.ShapeDataMap[ partID ].Shape, TopAbs_ShapeEnum.TopAbs_VERTEX );
					while( expMove.More() ) {
						TopoDS_Shape vertex = expMove.Current();
						if( sel.IsEqual( vertex ) ) {
							isMove = true;
							return;
						}
						expMove.Next();
					}
				}
			}
		}

		void SetConstraint( ETrsfConstraintType type )
		{
			GetRefAndMoveObject( out TopoDS_Shape refShape, out TopoDS_Shape moveShape );
			if( refShape == null || moveShape == null ) {
				return;
			}

			// create constraint
			IConstraint c = null;
			switch( type ) {
				case ETrsfConstraintType.Axial:
					c = new AxialConstraint( refShape, moveShape );
					break;
				case ETrsfConstraintType.AxialParallel:
					c = new AxialParallelConstraint( refShape, moveShape );
					break;
				case ETrsfConstraintType.Plane:
					c = new PlaneConstraint( refShape, moveShape );
					break;
				case ETrsfConstraintType.PlaneParallel:
					c = new PlaneParallelConstraint( refShape, moveShape );
					break;
				case ETrsfConstraintType.Point:
					c = new PointConstraint( refShape, moveShape );
					break;
				default:
					return;
			}
			if( !c.IsValid() ) {
				MyApp.Logger.ShowOnLogPanel( "約束無效。請選擇有效的參考並移動工件", MyApp.NoticeType.Warning );
				return;
			}
			gp_Trsf trsf = c.SolveConstraint();
			ApplyTransform( trsf );
			m_Viewer.GetAISContext().ClearSelected( true );
		}

		void ApplyTransform( gp_Trsf trsf )
		{
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );
			transformHelper.TransformData();
		}

		TopoDS_Shape m_G54Shape;
		List<AIS_Shape> m_G54AISList;
		bool m_IsControlPress = false;
	}
}
