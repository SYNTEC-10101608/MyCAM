using System;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.GC;
using OCC.Geom;
using OCC.Quantity;
using OCC.TopoDS;

namespace MyCAM.Helper
{
	internal static class TopoBuilder
	{
		public static TopoDS_Edge ConvertSegmentToTopo( ICAMSegmentElement camSegment )
		{
			TopoDS_Edge edge;
			if( camSegment == null ) {
				return null;
			}
			if( camSegment.ContourType == EContourType.Line ) {
				BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( camSegment.StartPoint.Point, camSegment.EndPoint.Point );
				edge = edgeBuilder.Edge();
				return edge;
			}
			if( camSegment.ContourType == EContourType.Arc ) {
				// build arc from 3 points
				if( camSegment.CAMPointList.Count >= 3 ) {
					ArcCAMSegment arcCAMSegment = camSegment as ArcCAMSegment;
					GC_MakeArcOfCircle arcBuilder = new GC_MakeArcOfCircle( arcCAMSegment.StartPoint.Point, arcCAMSegment.MidPoint.Point, arcCAMSegment.EndPoint.Point );

					if( !arcBuilder.IsDone() ) {
						throw new Exception( "無法由三點建立圓弧，請確認三點不共線。" );
					}
					Geom_TrimmedCurve arcCurve = arcBuilder.Value();
					BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( arcCurve );
					edge = edgeBuilder.Edge();
					return edge;
				}
				else {
					// not enough points, build line instead
					BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( camSegment.StartPoint.Point, camSegment.EndPoint.Point );
					edge = edgeBuilder.Edge();
					return edge;
				}
			}
			return null;
		}

		public static AIS_Shape CurveToAIS( Geom_Curve curve, Quantity_NameOfColor color, double lineWidth = 1, double dTransparancy = 1, bool isDashLine = false )
		{
			if( curve == null ) {
				return null;
			}
			BRepBuilderAPI_MakeEdge edgeMaker = new BRepBuilderAPI_MakeEdge( curve );
			TopoDS_Edge edge = edgeMaker.Edge();
			AIS_Shape aisCurve = new AIS_Shape( edge );
			aisCurve.SetColor( new Quantity_Color( color ) );
			aisCurve.SetWidth( lineWidth );
			return aisCurve;
		}
	}
}
