using System;
using System.Collections.Generic;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.AIS;
using OCC.gp;
using OCC.Quantity;
using OCC.TCollection;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	internal abstract class Shape3DRendererBase : CAMRendererBase
	{
		protected Shape3DRendererBase( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void SetPauseRefreshAndHide( bool isPause )
		{
			if( m_IsPauseRefreshAndHide == isPause ) {
				return;
			}
			base.SetPauseRefreshAndHide( isPause );
			if( isPause ) {
				foreach( AIS_InteractiveObject ais in GetDisplayObjects() ) {

					m_Viewer.GetAISContext().Erase( ais, false );
				}
			}
			else {
				Show();
			}
		}

		public override void Show( bool bUpdate = false )
		{
			if( m_IsPauseRefresh || m_IsPauseRefreshAndHide ) {
				return;
			}
			Remove();
			if( m_IsShow && m_HasGeometry ) {
				Draw();
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			if( m_ShapeAIS != null ) {
				m_Viewer.GetAISContext().Remove( m_ShapeAIS, false );
				m_ShapeAIS = null;
			}
			if( m_DimensionLine != null ) {
				m_Viewer.GetAISContext().Remove( m_DimensionLine, false );
				m_DimensionLine = null;
			}
			if( m_DimensionArrow != null ) {
				m_Viewer.GetAISContext().Remove( m_DimensionArrow, false );
				m_DimensionArrow = null;
			}
			if( m_DimensionText != null ) {
				m_Viewer.GetAISContext().Remove( m_DimensionText, false );
				m_DimensionText = null;
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		protected abstract void Draw();

		IEnumerable<AIS_InteractiveObject> GetDisplayObjects()
		{
			if( m_ShapeAIS != null ) {
				yield return m_ShapeAIS;
			}
			if( m_DimensionLine != null ) {
				yield return m_DimensionLine;
			}
			if( m_DimensionArrow != null ) {
				yield return m_DimensionArrow;
			}
			if( m_DimensionText != null ) {
				yield return m_DimensionText;
			}
		}

		protected static bool ValidateFinite( double value )
		{
			if( double.IsNaN( value ) || double.IsInfinity( value ) ) {
				return false;
			}
			return true;
		}

		protected static bool ValidateRange( double min, double max )
		{
			if( !ValidateFinite( min ) || !ValidateFinite( max ) ) {
				return false;
			}
			if( min >= max ) {
				return false;
			}
			return true;
		}

		protected void DisplayShape( AIS_Shape shape )
		{
			if( shape == null ) {
				return;
			}
			m_ShapeAIS = shape;
			m_Viewer.GetAISContext().Display( shape, false );
			m_Viewer.GetAISContext().Deactivate( shape );
		}

		protected void DisplaySafeRadialBoundary( AIS_Shape boundary )
		{
			boundary.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_ORANGE ) );
			boundary.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			boundary.SetTransparency( 0.5 );
			boundary.Attributes().SetFaceBoundaryDraw( true );
			boundary.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_ORANGE ) );
			boundary.Attributes().FaceBoundaryAspect().SetWidth( 2.5 );
			DisplayShape( boundary );
		}

		protected void AddSafeRadialDimension( gp_Ax3 axes, double radius, bool isSphere,
			double minU, double maxU, double minV, double maxV )
		{
			double angle = ( minU + maxU ) / 2;
			double positionV = ( minV + maxV ) / 2;
			gp_Dir axis = axes.Direction();
			gp_Dir xDirection = axes.XDirection();
			gp_Dir yDirection = axes.YDirection();
			gp_Pnt center = axes.Location();
			double axialDistance = isSphere ? 0 : positionV;
			gp_Pnt start = new gp_Pnt( center.X() + axialDistance * axis.X(),
				center.Y() + axialDistance * axis.Y(), center.Z() + axialDistance * axis.Z() );
			double radialScale = isSphere ? Math.Cos( positionV ) : 1;
			double axialScale = isSphere ? Math.Sin( positionV ) : 0;
			gp_Dir direction = new gp_Dir(
				radialScale * ( Math.Cos( angle ) * xDirection.X() + Math.Sin( angle ) * yDirection.X() ) + axialScale * axis.X(),
				radialScale * ( Math.Cos( angle ) * xDirection.Y() + Math.Sin( angle ) * yDirection.Y() ) + axialScale * axis.Y(),
				radialScale * ( Math.Cos( angle ) * xDirection.Z() + Math.Sin( angle ) * yDirection.Z() ) + axialScale * axis.Z() );
			gp_Pnt end = start.Translated( new gp_Vec( direction ).Scaled( radius ) );
			double headHeight = Math.Min( ARROW_HEIGHT, radius * 0.2 );
			AddDimension( start, end, end.Translated( new gp_Vec( direction ).Scaled( 5 ) ),
				string.Format( "R={0:F2}", radius ), headHeight, Math.Min( ARROW_RADIUS, headHeight * 0.4 ) );
		}

		protected void AddDimension( gp_Pnt start, gp_Pnt end, gp_Pnt textPosition, string text,
			double headHeight, double headRadius )
		{
			gp_Vec direction = new gp_Vec( start, end );
			if( direction.Magnitude() > 1e-6 ) {
				m_DimensionLine = DrawHelper.GetLineAIS( start, end, Quantity_NameOfColor.Quantity_NOC_ORANGE, 2, false );
				m_Viewer.GetAISContext().Display( m_DimensionLine, false );
				m_Viewer.GetAISContext().Deactivate( m_DimensionLine );
				direction.Normalize();
				gp_Pnt arrowStart = end.Translated( direction.Scaled( -headHeight ) );
				m_DimensionArrow = DrawHelper.GetOrientationAIS( arrowStart, new gp_Dir( direction ),
					headRadius, headHeight, Quantity_NameOfColor.Quantity_NOC_ORANGE, AIS_DisplayMode.AIS_Shaded );
				m_DimensionArrow.SetWidth( 3 );
				m_Viewer.GetAISContext().Display( m_DimensionArrow, false );
				m_Viewer.GetAISContext().Deactivate( m_DimensionArrow );
			}
			m_DimensionText = new AIS_TextLabel();
			m_DimensionText.SetText( new TCollection_ExtendedString( text ) );
			m_DimensionText.SetPosition( textPosition );
			m_DimensionText.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BROWN ) );
			m_DimensionText.SetHeight( 15 );
			m_Viewer.GetAISContext().Display( m_DimensionText, false );
			m_Viewer.GetAISContext().Deactivate( m_DimensionText );
		}

		protected bool m_HasGeometry;
		protected const double ARROW_HEIGHT = 5.0;
		protected const double ARROW_RADIUS = 2.0;
		AIS_Shape m_ShapeAIS;
		AIS_Line m_DimensionLine;
		AIS_Shape m_DimensionArrow;
		AIS_TextLabel m_DimensionText;
	}

	internal class PlaneShape3DRenderer : Shape3DRendererBase
	{
		public PlaneShape3DRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public bool SetGeometry( double z, double minX, double maxX, double minY, double maxY )
		{
			if( !ValidateFinite( z ) || !ValidateRange( minX, maxX ) || !ValidateRange( minY, maxY ) ) {
				return false;
			}

			m_Z = z;
			m_MinX = minX;
			m_MaxX = maxX;
			m_MinY = minY;
			m_MaxY = maxY;
			m_HasGeometry = true;

			return true;
		}

		protected override void Draw()
		{
			DisplayShape( DrawHelper.CreatePlaneAIS( m_MinX, m_MaxX, m_MinY, m_MaxY, m_Z ) );
			AddDimension( new gp_Pnt( 0, 0, 0 ), new gp_Pnt( 0, 0, m_Z ),
				new gp_Pnt( 0, 0, m_Z + 5 ), string.Format( "Z={0:F2}", m_Z ), ARROW_HEIGHT, ARROW_RADIUS );
		}

		double m_Z;
		double m_MinX;
		double m_MaxX;
		double m_MinY;
		double m_MaxY;
	}

	internal class RadialShape3DRenderer : Shape3DRendererBase
	{
		public RadialShape3DRenderer( Viewer viewer, DataManager dataManager, bool isSphere )
			: base( viewer, dataManager )
		{
			m_IsSphere = isSphere;
		}

		public bool SetGeometry( gp_Ax3 axes, double radius, double minU, double maxU, double minV, double maxV )
		{
			if( axes == null ) {
				return false;
			}
			if( !ValidateFinite( radius ) || !ValidateRange( minU, maxU ) || !ValidateRange( minV, maxV ) ) {
				return false;
			}
			if( radius <= 1e-6 || maxU - minU > 2 * Math.PI + 1e-6 || ( m_IsSphere && ( minV < -Math.PI / 2 || maxV > Math.PI / 2 ) ) ) {
				return false;
			}
			m_Axes = new gp_Ax3( axes.Location(), axes.Direction(), axes.XDirection() );
			m_Radius = radius;
			m_MinU = minU;
			m_MaxU = maxU;
			m_MinV = minV;
			m_MaxV = maxV;
			m_HasGeometry = true;

			return true;
		}

		protected override void Draw()
		{
			OCC.BRepBuilderAPI.BRepBuilderAPI_MakeFace faceMaker = m_IsSphere
				? new OCC.BRepBuilderAPI.BRepBuilderAPI_MakeFace( new gp_Sphere( m_Axes, m_Radius ), m_MinU, m_MaxU, m_MinV, m_MaxV )
				: new OCC.BRepBuilderAPI.BRepBuilderAPI_MakeFace( new gp_Cylinder( m_Axes, m_Radius ), m_MinU, m_MaxU, m_MinV, m_MaxV );

			if( !faceMaker.IsDone() ) {
				return;
			}

			DisplaySafeRadialBoundary( new AIS_Shape( faceMaker.Face() ) );
			AddSafeRadialDimension( m_Axes, m_Radius, m_IsSphere, m_MinU, m_MaxU, m_MinV, m_MaxV );
		}

		readonly bool m_IsSphere;
		gp_Ax3 m_Axes;
		double m_Radius;
		double m_MinU;
		double m_MaxU;
		double m_MinV;
		double m_MaxV;
	}
}
