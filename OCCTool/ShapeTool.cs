using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.BRepGProp;
using OCC.gp;
using OCC.GProp;
using OCC.TopoDS;
using System.Collections.Generic;

namespace OCCTool
{
	public class ShapeTool
	{
		public static TopoDS_Shape SewShape( List<TopoDS_Shape> shapeList )
		{
			// split the faces into shells
			double dSewingTolerance = 0.001;
			BRepBuilderAPI_Sewing sewing = new BRepBuilderAPI_Sewing( dSewingTolerance );
			foreach( TopoDS_Shape shape in shapeList ) {
				sewing.Add( shape );
			}
			sewing.Perform();
			return sewing.SewedShape();
		}

		public static gp_Pnt GetFaceCenter( TopoDS_Face face )
		{
			GProp_GProps props = new GProp_GProps();
			BRepGProp.SurfaceProperties( face, ref props );
			gp_Pnt center = props.CentreOfMass();
			return center;
		}

		public static TopoDS_Shape MakeCompound( List<TopoDS_Shape> shapeList )
		{
			TopoDS_Compound compound = new TopoDS_Compound();
			TopoDS_Shape compoundShape = compound;
			BRep_Builder builder = new BRep_Builder();
			builder.MakeCompound( ref compound );
			foreach( TopoDS_Shape shape in shapeList ) {
				builder.Add( ref compoundShape, shape );
			}
			return compound;
		}
	}
}
