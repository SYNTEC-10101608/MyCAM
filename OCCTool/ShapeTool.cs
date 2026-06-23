using OCC.Bnd;
using OCC.BRep;
using OCC.BRepBndLib;
using OCC.BRepBuilderAPI;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.TopoDS;
using System;
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

		public static TopoDS_Shape SewShape( TopoDS_Shape shape, double dSewingTolerance = 1e-3 )
		{
			BRepBuilderAPI_Sewing sewing = new BRepBuilderAPI_Sewing( dSewingTolerance );
			sewing.Add( shape );
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

		public static bool FlipShapeUpsideDown( TopoDS_Shape shape, out TopoDS_Shape flippedShape, out gp_Trsf trsf, gp_Pnt rotationCenter = null )
		{
			flippedShape = null;
			trsf = new gp_Trsf();

			try {
				if( shape == null || shape.IsNull() ) {
					return false;
				}
				if( rotationCenter == null ) {
					rotationCenter = new gp_Pnt( 0, 0, 0 );
				}

				// rotate 180 degrees around X-axis
				gp_Ax1 xAxis = new gp_Ax1( rotationCenter, new gp_Dir( 1, 0, 0 ) );
				trsf.SetRotation( xAxis, Math.PI );
				BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( shape, trsf, true );
				if( !transform.IsDone() ) {
					return false;
				}
				flippedShape = transform.Shape();
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool MoveShapeBottomToZ0( TopoDS_Shape shape, out TopoDS_Shape movedShape, out gp_Trsf trsf )
		{
			movedShape = null;
			trsf = new gp_Trsf();

			try {
				if( shape == null || shape.IsNull() ) {
					return false;
				}

				// step 1: get bounding box and find minimum Z
				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( shape, ref bbox );

				if( bbox.IsVoid() ) {
					return false;
				}

				double xmin = 0, ymin = 0, zmin = 0, xmax = 0, ymax = 0, zmax = 0;
				bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );

				// step 2: create translation vector to move minimum Z to 0
				gp_Vec translation = new gp_Vec( 0, 0, -zmin );

				// step 3: apply translation
				trsf.SetTranslation( translation );
				BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( shape, trsf, true );
				if( !transform.IsDone() ) {
					return false;
				}
				movedShape = transform.Shape();
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool AlignOBBToXYAxes( TopoDS_Shape shape, out TopoDS_Shape alignedShape )
		{
			return AlignOBBToXYAxes( shape, out alignedShape, out _ );
		}

		public static bool AlignOBBToXYAxes( TopoDS_Shape shape, out TopoDS_Shape alignedShape, out gp_Trsf trsf )
		{
			alignedShape = null;
			trsf = new gp_Trsf();

			try {
				if( shape == null || shape.IsNull() ) {
					return false;
				}

				// Step 1: Calculate OBB
				Bnd_OBB obb = new Bnd_OBB();
				BRepBndLib.AddOBB( shape, ref obb, true, true, true );

				if( obb.IsVoid() ) {
					return false;
				}

				// Step 2: Get OBB X direction (principal axis)
				gp_XYZ xDirectionXYZ = obb.XDirection();
				gp_Dir xDirection = new gp_Dir( xDirectionXYZ );

				// Step 3: Calculate rotation angle to align X direction to coordinate X axis
				// atan2(y, x) returns the angle between the vector and X axis
				double angle = Math.Atan2( xDirection.Y(), xDirection.X() );

				// Step 4: Rotate around Z-axis at origin by -angle to align OBB X direction to coordinate X axis
				gp_Ax1 zAxisAtOrigin = new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );

				trsf.SetRotation( zAxisAtOrigin, -angle );

				// Step 5: Apply transformation
				BRepBuilderAPI_Transform transform = new BRepBuilderAPI_Transform( shape, trsf, true );

				if( !transform.IsDone() ) {
					return false;
				}

				alignedShape = transform.Shape();
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool AlignOBBToXYAxesKeepAxisCentered( TopoDS_Shape shape, out TopoDS_Shape alignedShape )
		{
			alignedShape = null;

			try {
				if( shape == null || shape.IsNull() ) {
					return false;
				}

				// Step 1: Calculate OBB
				Bnd_OBB obb = new Bnd_OBB();
				BRepBndLib.AddOBB( shape, ref obb, true, true, true );

				if( obb.IsVoid() ) {
					return false;
				}

				// Step 2: Get OBB X direction (principal axis)
				gp_XYZ xDirectionXYZ = obb.XDirection();
				gp_Dir xDirection = new gp_Dir( xDirectionXYZ );

				// Step 3: Calculate rotation angle to align X direction to coordinate X axis
				// Project OBB X-direction onto XY plane and calculate angle
				double angle = Math.Atan2( xDirection.Y(), xDirection.X() );

				// Step 4: Rotate around Z-axis at ORIGIN (0,0,0), not OBB center
				// This keeps the revolution axis at (0,0,z)
				gp_Ax1 zAxisAtOrigin = new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );

				gp_Trsf rotationTrsf = new gp_Trsf();
				rotationTrsf.SetRotation( zAxisAtOrigin, -angle );

				// Step 5: Apply rotation
				BRepBuilderAPI_Transform transform1 = new BRepBuilderAPI_Transform( shape, rotationTrsf, true );

				if( !transform1.IsDone() ) {
					return false;
				}

				TopoDS_Shape rotatedShape = transform1.Shape();

				// Step 6: Re-center to (0,0) in case rotation caused slight drift
				// Calculate bounding box to find current center
				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( rotatedShape, ref bbox );

				if( bbox.IsVoid() ) {
					alignedShape = rotatedShape;
					return true;
				}

				double xmin = 0, ymin = 0, zmin = 0, xmax = 0, ymax = 0, zmax = 0;
				bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );

				// Calculate XY center
				double xCenter = ( xmin + xmax ) / 2.0;
				double yCenter = ( ymin + ymax ) / 2.0;

				// Step 7: Translate to center XY at origin (keep Z unchanged)
				gp_Vec recenterVec = new gp_Vec( -xCenter, -yCenter, 0 );
				gp_Trsf recenterTrsf = new gp_Trsf();
				recenterTrsf.SetTranslation( recenterVec );

				BRepBuilderAPI_Transform transform2 = new BRepBuilderAPI_Transform( rotatedShape, recenterTrsf, true );

				if( !transform2.IsDone() ) {
					alignedShape = rotatedShape;
					return true;
				}

				alignedShape = transform2.Shape();
				return true;
			}
			catch {
				return false;
			}
		}

	}



	public class BoundingBox
	{
		public BoundingBox( TopoDS_Shape shape )
		{
			if( shape == null || shape.IsNull() ) {
				throw new ArgumentNullException( "BoundingBox constructing argument null" );
			}
			m_Box = new Bnd_Box();
			BRepBndLib.AddOptimal( shape, ref m_Box );
			m_Box.Get( ref m_Xmin, ref m_Ymin, ref m_Zmin, ref m_Xmax, ref m_Ymax, ref m_Zmax );
		}

		public double Xmin
		{
			get
			{
				return m_Xmin;
			}
		}

		public double Xmax
		{
			get
			{
				return m_Xmax;
			}
		}

		public double Ymin
		{
			get
			{
				return m_Ymin;
			}
		}

		public double Ymax
		{
			get
			{
				return m_Ymax;
			}
		}

		public double Zmin
		{
			get
			{
				return m_Zmin;
			}
		}

		public double Zmax
		{
			get
			{
				return m_Zmax;
			}
		}

		public double XLength
		{
			get
			{
				return m_Xmax - m_Xmin;
			}
		}

		public double YLength
		{
			get
			{
				return m_Ymax - m_Ymin;
			}
		}

		public double ZLength
		{
			get
			{
				return m_Zmax - m_Zmin;
			}
		}

		public double XCenter
		{
			get
			{
				return ( m_Xmax + m_Xmin ) / 2;
			}
		}

		public double YCenter
		{
			get
			{
				return ( m_Ymax + m_Ymin ) / 2;
			}
		}

		public double ZCenter
		{
			get
			{
				return ( m_Zmax + m_Zmin ) / 2;
			}
		}

		public void OffsetBox( double DeltaX, double DeltaY, double DeltaZ )
		{
			m_Xmin += DeltaX;
			m_Xmax += DeltaX;
			m_Ymin += DeltaY;
			m_Ymax += DeltaY;
			m_Zmin += DeltaZ;
			m_Zmax += DeltaZ;
		}

		public TopoDS_Shape GetBoundingBoxShape()
		{
			if( m_Box == null ) {
				return null;
			}
			if( !m_Box.IsVoid() ) {

				gp_Pnt aMinPnt = m_Box.CornerMin();
				gp_Pnt aMaxPnt = m_Box.CornerMax();

				BRepPrimAPI_MakeBox aBoundingBoxMaker = new BRepPrimAPI_MakeBox( aMinPnt, aMaxPnt );
				return aBoundingBoxMaker.Shape();
			}
			return null;
		}

		double m_Xmin, m_Xmax, m_Ymin, m_Ymax, m_Zmin, m_Zmax;
		Bnd_Box m_Box;
	}
}
