using OCC.AIS;
using OCC.Aspect;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.Geom;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Prs3d;
using OCC.Quantity;
using OCCTool;

namespace MyCAM.Helper
{
	public static class DrawHelper
	{
		public static AIS_Line GetLineAIS( gp_Pnt startPnt, gp_Pnt endPnt, Quantity_NameOfColor color, double width = 2.5, bool isDashLine = false )
		{
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( startPnt ), new Geom_CartesianPoint( endPnt ) );
			lineAIS.SetColor( new Quantity_Color( color ) );
			lineAIS.SetWidth( width );
			if( isDashLine ) {
				Prs3d_LineAspect prs3D_LineAspect = new Prs3d_LineAspect( new Quantity_Color( color ), Aspect_TypeOfLine.Aspect_TOL_DASH, 1 );
				lineAIS.Attributes().SetLineAspect( prs3D_LineAspect );
			}
			return lineAIS;
		}

		public static AIS_Shape GetOrientationAIS( gp_Pnt point, gp_Dir dir, double arrowRadius = 0.5, double arrowHeight = 2.0, Quantity_NameOfColor color = Quantity_NameOfColor.Quantity_NOC_WHITE, AIS_DisplayMode displayMode = AIS_DisplayMode.AIS_WireFrame )
		{
			// draw a cone to indicate the orientation
			gp_Ax2 coneAx2 = new gp_Ax2( point, dir );
			BRepPrimAPI_MakeCone coneMaker = new BRepPrimAPI_MakeCone( coneAx2, arrowRadius, 0, arrowHeight );
			AIS_Shape coneAIS = new AIS_Shape( coneMaker.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			coneAIS.SetMaterial( aspect );
			coneAIS.SetColor( new Quantity_Color( color ) );
			coneAIS.SetDisplayMode( (int)displayMode );
			coneAIS.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			return coneAIS;
		}

		public static AIS_Shape CreatePlaneAIS( double minX, double maxX, double minY, double maxY, double posZ, Quantity_NameOfColor color = Quantity_NameOfColor.Quantity_NOC_ORANGE, AIS_DisplayMode mode = AIS_DisplayMode.AIS_Shaded, double transparency = 0.5 )
		{
			gp_Pnt corner1 = new gp_Pnt( minX, minY, posZ );
			gp_Pnt corner2 = new gp_Pnt( maxX, minY, posZ );
			gp_Pnt corner3 = new gp_Pnt( maxX, maxY, posZ );
			gp_Pnt corner4 = new gp_Pnt( minX, maxY, posZ );

			BRepBuilderAPI_MakeEdge edge1 = new BRepBuilderAPI_MakeEdge( corner1, corner2 );
			BRepBuilderAPI_MakeEdge edge2 = new BRepBuilderAPI_MakeEdge( corner2, corner3 );
			BRepBuilderAPI_MakeEdge edge3 = new BRepBuilderAPI_MakeEdge( corner3, corner4 );
			BRepBuilderAPI_MakeEdge edge4 = new BRepBuilderAPI_MakeEdge( corner4, corner1 );

			BRepBuilderAPI_MakeWire wireBuilder = new BRepBuilderAPI_MakeWire();
			wireBuilder.Add( edge1.Edge() );
			wireBuilder.Add( edge2.Edge() );
			wireBuilder.Add( edge3.Edge() );
			wireBuilder.Add( edge4.Edge() );

			if( !wireBuilder.IsDone() ) {
				return null;
			}

			BRepBuilderAPI_MakeFace faceBuilder = new BRepBuilderAPI_MakeFace( wireBuilder.Wire() );
			if( !faceBuilder.IsDone() ) {
				return null;
			}

			AIS_Shape planeAIS = new AIS_Shape( faceBuilder.Face() );
			planeAIS.SetColor( new Quantity_Color( color ) );
			planeAIS.SetDisplayMode( (int)mode );
			planeAIS.SetTransparency( transparency );
			return planeAIS;
		}
	}
}
