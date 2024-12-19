using OCC.Bnd;
using OCC.BRepBndLib;
using OCC.BRepProj;
using OCC.gp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ShapeProjectionExtensions
{
	public static void ProjectWireToShape( TopoDS_Wire wire, TopoDS_Shape targetObject, gp_Vec direction, gp_Vec center,
		out List<TopoDS_Wire> frontWires, out List<TopoDS_Wire> backWires )
	{
		if( ( direction == null ) == ( center == null ) )
			throw new ArgumentException( "One of either direction or center must be provided" );

		BRepProj_Projection projectionObject;
		if( direction != null ) {
			projectionObject = new BRepProj_Projection(
				wire,
				targetObject,
				new gp_Dir( direction.X(), direction.Y(), direction.Z() )
			);
		}
		else {
			projectionObject = new BRepProj_Projection(
				wire,
				targetObject,
				new gp_Pnt( center.X(), center.Y(), center.Z() )
			);
		}

		var targetOrientation = wire.Orientation();
		var outputWires = new List<TopoDS_Wire>();

		while( projectionObject.More() ) {
			var projectedWire = projectionObject.Current();
			if( targetOrientation == projectedWire.Orientation() ) {
				outputWires.Add( TopoDS.ToWire( projectedWire ) );
			}
			else {
				outputWires.Add( TopoDS.ToWire( projectedWire.Reversed() ) );
			}
			projectionObject.Next();
		}

		frontWires = new List<TopoDS_Wire>();
		backWires = new List<TopoDS_Wire>();

		if( outputWires.Count > 1 ) {
			var outputWiresCenters = outputWires
	.Select( w =>
	{
		var bbox = new Bnd_Box();
		BRepBndLib.Add( w, ref bbox );
		var min = bbox.CornerMin();
		var max = bbox.CornerMax();
		return new gp_Vec(
			( min.X() + max.X() ) / 2,
			( min.Y() + max.Y() ) / 2,
			( min.Z() + max.Z() ) / 2
		);
	} )
	.ToList();
			var projectionCenter = outputWiresCenters.Aggregate( ( v0, v1 ) => v0 + v1 ) / outputWiresCenters.Count;

			var outputWiresDirections = outputWiresCenters
				.Select( w => ( w - projectionCenter ).Normalized() )
				.ToList();

			var directionNormalized = direction != null
				? direction.Normalized()
				: ( center - projectionCenter ).Normalized();

			for( int i = 0; i < outputWiresDirections.Count; i++ ) {
				if( outputWiresDirections[ i ].Dot( directionNormalized ) > 0 ) {
					frontWires.Add( outputWires[ i ] );
				}
				else {
					backWires.Add( outputWires[ i ] );
				}
			}
		}
		else {
			frontWires = outputWires;
		}
	}
}
