using OCC.BRep;
using OCC.BRepGProp;
using OCC.Geom;
using OCC.gp;
using OCC.GProp;
using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Collections.Generic;

namespace OCCTool
{
	public class SegmentTool
	{
		public static void GetEdgeSegmentPoints( TopoDS_Edge edge, double dSegmentLength, out List<gp_Pnt> vertexList )
		{
			vertexList = new List<gp_Pnt>();

			// get target edge length
			GProp_GProps system = new GProp_GProps();
			BRepGProp.LinearProperties( edge, ref system );
			double dEdgeLength = system.Mass();

			// get segment count
			int nSegments = (int)Math.Ceiling( dEdgeLength / dSegmentLength );

			// get curve parameters
			double dStartU = 0;
			double dEndU = 0;
			Geom_Curve oneGeomCurve = BRep_Tool.Curve( edge, ref dStartU, ref dEndU );

			// swap when reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				double tempU = dStartU;
				dStartU = dEndU;
				dEndU = tempU;
			}

			// get increment value
			double dIncrement = ( dEndU - dStartU ) / nSegments;

			// get points
			for( int i = 0; i < nSegments; i++ ) {
				double U = dStartU + dIncrement * i;
				vertexList.Add( oneGeomCurve.Value( U ) );
			}
			vertexList.Add( oneGeomCurve.Value( dEndU ) );
		}
	}
}
