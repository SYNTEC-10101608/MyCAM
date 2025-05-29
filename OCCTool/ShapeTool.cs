using OCC.BRep;
using OCC.BRepBuilderAPI;
using OCC.TopoDS;
using System.Collections.Generic;

namespace OCCTool
{
	public class ShapeTool
	{
		public static TopoDS_Shape SewShape( List<TopoDS_Shape> shapeList, double dSewingTolerance = 1e-3 )
		{
			BRepBuilderAPI_Sewing sewing = new BRepBuilderAPI_Sewing( dSewingTolerance );
			foreach( TopoDS_Shape shape in shapeList ) {
				sewing.Add( shape );
			}
			sewing.Perform();
			return sewing.SewedShape();
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
