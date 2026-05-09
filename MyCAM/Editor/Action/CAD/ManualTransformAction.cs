using MyCAM.App;
using MyCAM.Data;
using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.SelectMgr;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal enum ETrsfConstraintType
	{
		Point,
		Axial,
		AxialParallel,
		Plane,
		PlaneParallel
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

		public Action<bool> SelectionStatusChanged;
		public Action<int> G54RefIndexChanged;

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( false );

			// disable tree view
			m_TreeView.Enabled = false;

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
			if( e.Button != MouseButtons.Left ) {
				return;
			}

			// if nothing is detected (clicked empty space), keep current selection unchanged
			if( !m_Viewer.GetAISContext().HasDetected() ) {
				return;
			}

			// select with Replace to detect what sub-shape user clicked
			m_Viewer.Select( AIS_SelectionScheme.AIS_SelectionScheme_Replace );

			if( m_Viewer.GetAISContext().NbSelected() == 0 ) {
				return;
			}

			// get both the shape and the entity owner (sub-shape level)
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return;
			}
			TopoDS_Shape clickedShape = m_Viewer.GetAISContext().SelectedShape();
			SelectMgr_EntityOwner clickedOwner = m_Viewer.GetAISContext().SelectedOwner();

			if( clickedShape == null || clickedShape.IsNull() ) {
				return;
			}

			// determine identity: G54 (ref) or workpiece
			CheckRefOrWorkpieceObject( clickedShape, out bool isRef, out bool isWorkpiece );
			if( !isRef && !isWorkpiece ) {
				return;
			}

			if( isRef ) {
				m_RefShape = clickedShape;
				m_RefG54AIS = FindG54AISByShape( clickedShape );

				// notify dialog to sync dropdown
				int dropdownIndex = GetG54DropdownIndex( clickedShape );
				if( dropdownIndex >= 0 ) {
					G54RefIndexChanged?.Invoke( dropdownIndex );
				}
			}
			else {
				// clicking the same workpiece shape deselects it
				if( m_WorkpieceShape != null && clickedShape.IsEqual( m_WorkpieceShape ) ) {
					m_WorkpieceShape = null;
					m_WorkpieceOwner = null;
				}
				else {
					m_WorkpieceShape = clickedShape;
					m_WorkpieceOwner = clickedOwner;
				}
				SelectionStatusChanged?.Invoke( m_WorkpieceShape != null );
			}

			// rebuild visual selection using entity owners for sub-shape precision
			RebuildVisualSelection();
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		public bool ApplyTransform( ETrsfConstraintType type )
		{
			if( m_RefShape == null || m_WorkpieceShape == null ) {
				return false;
			}

			// create constraint
			IConstraint c = null;
			switch( type ) {
				case ETrsfConstraintType.Axial:
					c = new AxialConstraint( m_RefShape, m_WorkpieceShape );
					break;
				case ETrsfConstraintType.AxialParallel:
					c = new AxialParallelConstraint( m_RefShape, m_WorkpieceShape );
					break;
				case ETrsfConstraintType.Plane:
					c = new PlaneConstraint( m_RefShape, m_WorkpieceShape );
					break;
				case ETrsfConstraintType.PlaneParallel:
					c = new PlaneParallelConstraint( m_RefShape, m_WorkpieceShape );
					break;
				case ETrsfConstraintType.Point:
					c = new PointConstraint( m_RefShape, m_WorkpieceShape );
					break;
				default:
					return false;
			}
			if( !c.IsValid() ) {
				MyApp.Logger.ShowOnLogPanel( "對齊無效，請重新選擇工件上適當的元素", MyApp.NoticeType.Warning );
				return false;
			}
			gp_Trsf trsf = c.SolveConstraint();
			ExecuteTransform( trsf );
			m_Viewer.GetAISContext().ClearSelected( true );

			// reset stored selections after applying transform
			m_RefShape = null;
			m_WorkpieceShape = null;
			m_RefG54AIS = null;
			m_WorkpieceOwner = null;
			SelectionStatusChanged?.Invoke( false );
			return true;
		}

		public void SwitchConstrainMethod( ETrsfConstraintType type )
		{
			// store current mode for index mapping
			m_CurrentMode = type;

			// deactivate all workpiece shapes
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Deactivate( m_ViewManager.ViewObjectMap[ partID ].AISHandle );
			}

			// deactivate all G54 shapes first
			foreach( AIS_Shape ais in m_G54AISList ) {
				m_Viewer.GetAISContext().Deactivate( ais );
			}

			// G54AISList index: [0]=XY face, [1]=YZ face, [2]=XZ face, [3]=X axis, [4]=Y axis, [5]=Z axis, [6]=origin
			AISActiveMode workpieceMode;
			switch( type ) {
				case ETrsfConstraintType.Point:
					workpieceMode = AISActiveMode.Vertex;

					// point mode: only origin vertex selectable on G54
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 6 ], (int)AISActiveMode.Vertex );
					break;

				case ETrsfConstraintType.Axial:
				case ETrsfConstraintType.AxialParallel:
					workpieceMode = AISActiveMode.Edge;

					// axis mode: only X/Y/Z axis edges selectable on G54
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 3 ], (int)AISActiveMode.Edge );
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 4 ], (int)AISActiveMode.Edge );
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 5 ], (int)AISActiveMode.Edge );
					break;

				case ETrsfConstraintType.Plane:
				case ETrsfConstraintType.PlaneParallel:
					workpieceMode = AISActiveMode.Face;

					// plane mode: only XY/YZ/XZ planes selectable on G54
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 0 ], (int)AISActiveMode.Face );
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 1 ], (int)AISActiveMode.Face );
					m_Viewer.GetAISContext().Activate( m_G54AISList[ 2 ], (int)AISActiveMode.Face );
					break;

				default:
					return;
			}

			// activate workpiece with the corresponding mode
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				m_Viewer.GetAISContext().Activate( m_ViewManager.ViewObjectMap[ partID ].AISHandle, (int)workpieceMode );
			}

			// clear stored selections and visual selection when mode changes
			m_RefShape = null;
			m_RefG54AIS = null;
			m_WorkpieceShape = null;
			m_WorkpieceOwner = null;
			m_Viewer.GetAISContext().ClearSelected( false );
			m_Viewer.GetAISContext().UpdateCurrentViewer();
			SelectionStatusChanged?.Invoke( false );
		}

		public void SetG54RefFromDropDown( int dropdownIndex )
		{
			// G54AISList: [0]=XY face, [1]=YZ face, [2]=XZ face, [3]=X axis, [4]=Y axis, [5]=Z axis, [6]=origin
			int aisIndex;
			switch( m_CurrentMode ) {
				case ETrsfConstraintType.Point:
					aisIndex = 6;
					break;
				case ETrsfConstraintType.Axial:
				case ETrsfConstraintType.AxialParallel:
					aisIndex = 3 + dropdownIndex;
					break;
				case ETrsfConstraintType.Plane:
				case ETrsfConstraintType.PlaneParallel:
					aisIndex = dropdownIndex;
					break;
				default:
					return;
			}
			if( aisIndex < 0 || aisIndex >= m_G54AISList.Count ) {
				return;
			}

			AIS_Shape targetAIS = m_G54AISList[ aisIndex ];

			// store shape for constraint logic
			m_RefShape = targetAIS.Shape();

			// store AIS for visual highlight (G54 shapes are single-shape AIS, safe to use AIS-level selection)
			m_RefG54AIS = targetAIS;

			RebuildVisualSelection();
			SelectionStatusChanged?.Invoke( m_WorkpieceShape != null );
		}

		int GetG54DropdownIndex( TopoDS_Shape shape )
		{
			switch( m_CurrentMode ) {
				case ETrsfConstraintType.Point:
					return 0;
				case ETrsfConstraintType.Axial:
				case ETrsfConstraintType.AxialParallel:
					for( int i = 0; i < 3; i++ ) {
						if( ContainsSubShape( m_G54AISList[ 3 + i ].Shape(), shape ) ) {
							return i;
						}
					}
					break;
				case ETrsfConstraintType.Plane:
				case ETrsfConstraintType.PlaneParallel:
					for( int i = 0; i < 3; i++ ) {
						if( ContainsSubShape( m_G54AISList[ i ].Shape(), shape ) ) {
							return i;
						}
					}
					break;
			}
			return -1;
		}

		AIS_Shape FindG54AISByShape( TopoDS_Shape shape )
		{
			foreach( AIS_Shape ais in m_G54AISList ) {
				if( ContainsSubShape( ais.Shape(), shape ) ) {
					return ais;
				}
			}
			return null;
		}

		void RebuildVisualSelection()
		{
			m_Viewer.GetAISContext().ClearSelected( false );

			// G54 ref: each G54 AIS is a single shape, AIS-level selection is precise
			if( m_RefG54AIS != null ) {
				m_Viewer.GetAISContext().AddOrRemoveSelected( m_RefG54AIS, false );
			}

			// workpiece move: use EntityOwner
			if( m_WorkpieceOwner != null ) {
				m_Viewer.GetAISContext().AddOrRemoveSelected( m_WorkpieceOwner, false );
			}

			m_Viewer.GetAISContext().UpdateCurrentViewer();
		}

		bool ContainsSubShape( TopoDS_Shape parent, TopoDS_Shape sub )
		{
			TopAbs_ShapeEnum subType = sub.ShapeType();
			TopExp_Explorer exp = new TopExp_Explorer( parent, subType );
			while( exp.More() ) {
				if( exp.Current().IsEqual( sub ) ) {
					return true;
				}
				exp.Next();
			}
			return false;
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
			aisY.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_ORANGE ) );
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

		void CheckRefOrWorkpieceObject( TopoDS_Shape sel, out bool isRef, out bool isWorkpiece )
		{
			isRef = false;
			isWorkpiece = false;
			if( sel == null ) {
				return;
			}

			TopAbs_ShapeEnum shapeType = sel.ShapeType();

			// only handle selectable sub-shape types
			if( shapeType != TopAbs_ShapeEnum.TopAbs_FACE &&
				shapeType != TopAbs_ShapeEnum.TopAbs_EDGE &&
				shapeType != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return;
			}

			// check if it belongs to G54
			TopExp_Explorer expRef = new TopExp_Explorer( m_G54Shape, shapeType );
			while( expRef.More() ) {
				if( sel.IsEqual( expRef.Current() ) ) {
					isRef = true;
					return;
				}
				expRef.Next();
			}

			// check if it belongs to any visible workpiece
			foreach( var partID in m_DataManager.PartIDList ) {
				if( m_ViewManager.ViewObjectMap[ partID ].Visible == false ) {
					continue;
				}
				if( !DataGettingHelper.GetShapeObject( partID, out IShapeObject shapeObj ) ) {
					continue;
				}
				TopExp_Explorer expWorkpiece = new TopExp_Explorer( shapeObj.Shape, shapeType );
				while( expWorkpiece.More() ) {
					if( sel.IsEqual( expWorkpiece.Current() ) ) {
						isWorkpiece = true;
						return;
					}
					expWorkpiece.Next();
				}
			}
		}

		void ExecuteTransform( gp_Trsf trsf )
		{
			TransformHelper transformHelper = new TransformHelper( m_Viewer, m_DataManager, m_ViewManager, trsf );
			transformHelper.TransformData();
		}

		// stored selections: one from G54, one from workpiece
		ETrsfConstraintType m_CurrentMode;
		TopoDS_Shape m_G54Shape;
		List<AIS_Shape> m_G54AISList;

		TopoDS_Shape m_RefShape;
		AIS_Shape m_RefG54AIS;
		TopoDS_Shape m_WorkpieceShape;
		SelectMgr_EntityOwner m_WorkpieceOwner;
	}
}
