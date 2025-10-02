using OCC.AIS;
using OCC.Aspect;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopoDS;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal static class ViewHelper
	{
		public static AIS_Shape CreatePartAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GRAY70 ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( COLOR_FACEBOUNDARY ) );
			aisShape.Attributes().FaceBoundaryAspect().SetWidth( FACE_BOUNDARY_WIDTH );

			// Vertex style
			if( shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				aisShape.Attributes().PointAspect().SetTypeOfMarker( Aspect_TypeOfMarker.Aspect_TOM_BALL );
				aisShape.Attributes().PointAspect().SetColor( new Quantity_Color( COLOR_FEATURE_DEFAULT ) );
				aisShape.Attributes().PointAspect().SetScale( POINT_SIZE );
			}
			return aisShape;
		}

		public static AIS_Shape CreateFeatureAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_FEATURE_DEFAULT ) );
			aisShape.SetWidth( LINE_WIDTH );

			// Vertex style
			if( shape.ShapeType() == TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				aisShape.Attributes().PointAspect().SetTypeOfMarker( Aspect_TypeOfMarker.Aspect_TOM_BALL );
				aisShape.Attributes().PointAspect().SetColor( new Quantity_Color( COLOR_FEATURE_DEFAULT ) );
				aisShape.Attributes().PointAspect().SetScale( POINT_SIZE );
			}
			return aisShape;
		}

		public static AIS_Shape CreatePathAIS( TopoDS_Shape shape, double lineWidth = LINE_WIDTH )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_PATH ) );
			aisShape.SetWidth( lineWidth );
			return aisShape;
		}

		public const Quantity_NameOfColor COLOR_PATH = Quantity_NameOfColor.Quantity_NOC_BLUE;
		public const Quantity_NameOfColor COLOR_FEATURE_DEFAULT = Quantity_NameOfColor.Quantity_NOC_BROWN3;
		public const Quantity_NameOfColor COLOR_FACEBOUNDARY = Quantity_NameOfColor.Quantity_NOC_BLACK;
		public const double FACE_BOUNDARY_WIDTH = 0.5;
		public const int LINE_WIDTH = 2;
		public const int POINT_SIZE = 3;
	}

	internal static class SelectViewHelper
	{
		public static AIS_Shape CreateFaceAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
			aisShape.Attributes().SetFaceBoundaryDraw( true );
			aisShape.Attributes().FaceBoundaryAspect().SetColor( new Quantity_Color( COLOR_FACEBOUNDARY ) );
			aisShape.Attributes().FaceBoundaryAspect().SetWidth( FACE_BOUNDARY_WIDTH );
			return aisShape;
		}

		public static AIS_Shape CreateEdgeAIS( TopoDS_Shape shape )
		{
			AIS_Shape aisShape = new AIS_Shape( shape );
			aisShape.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			aisShape.SetColor( new Quantity_Color( COLOR_DEFAULT ) );
			aisShape.SetWidth( LINE_WIDTH );
			return aisShape;
		}

		public const Quantity_NameOfColor COLOR_DEFAULT = Quantity_NameOfColor.Quantity_NOC_GRAY50;
		public const Quantity_NameOfColor COLOR_FACEBOUNDARY = Quantity_NameOfColor.Quantity_NOC_BLACK;
		public const double FACE_BOUNDARY_WIDTH = 0.5;
		public const int LINE_WIDTH = 2;
	}

	internal class ViewObject
	{
		public ViewObject( AIS_InteractiveObject shape )
		{
			AISHandle = shape;
			Visible = true;
		}

		public bool Visible
		{
			get; set;
		}

		public AIS_InteractiveObject AISHandle
		{
			get; set;
		}
	}

	internal class ViewManager
	{
		public ViewManager()
		{
			ViewObjectMap = new Dictionary<string, ViewObject>();
			TreeNodeMap = new Dictionary<string, TreeNode>();
			PartNode = new TreeNode( "Part" );
			PathNode = new TreeNode( "Path" );
		}

		public Dictionary<string, ViewObject> ViewObjectMap
		{
			get; private set;
		}

		public Dictionary<string, TreeNode> TreeNodeMap
		{
			get; private set;
		}

		public TreeNode PartNode
		{
			get; private set;
		}

		public TreeNode PathNode
		{
			get; private set;
		}
	}
}
