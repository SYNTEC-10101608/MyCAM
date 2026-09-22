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

		// rotate 180 degrees around a direction perpendicular to axis, so the shape is flipped along axis
		public static bool FlipShapeAlongAxis( TopoDS_Shape shape, EAlignAxis axis, out TopoDS_Shape flippedShape, out gp_Trsf trsf, gp_Pnt rotationCenter = null )
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

				// rotate 180 degrees around the first radial direction of the given axis
				GetRadialDirs( axis, out gp_Dir radialDir1, out _ );
				gp_Ax1 flipAxis = new gp_Ax1( rotationCenter, radialDir1 );
				trsf.SetRotation( flipAxis, Math.PI );
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

		// move the lowest point along the given axis onto the plane passing through the origin,
		// the position on the other two directions is kept unchanged
		public static bool MoveShapeBottomToAxisZero( TopoDS_Shape shape, EAlignAxis axis, out TopoDS_Shape movedShape, out gp_Trsf trsf )
		{
			movedShape = null;
			trsf = new gp_Trsf();

			try {
				if( shape == null || shape.IsNull() ) {
					return false;
				}

				// step 1: get bounding box
				Bnd_Box bbox = new Bnd_Box();
				BRepBndLib.AddOptimal( shape, ref bbox );

				if( bbox.IsVoid() ) {
					return false;
				}

				double xmin = 0, ymin = 0, zmin = 0, xmax = 0, ymax = 0, zmax = 0;
				bbox.Get( ref xmin, ref ymin, ref zmin, ref xmax, ref ymax, ref zmax );

				// step 2: create translation vector to move the minimum of the given axis to 0
				gp_Vec translation;
				switch( axis ) {
					case EAlignAxis.X:
						translation = new gp_Vec( -xmin, 0, 0 );
						break;
					case EAlignAxis.Y:
						translation = new gp_Vec( 0, -ymin, 0 );
						break;
					default:
						translation = new gp_Vec( 0, 0, -zmin );
						break;
				}

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

		// the two radial directions perpendicular to axis,
		// radialDir1 x radialDir2 = axis direction, so the frame stays right handed
		public static void GetRadialDirs( EAlignAxis axis, out gp_Dir radialDir1, out gp_Dir radialDir2 )
		{
			switch( axis ) {
				case EAlignAxis.X:
					radialDir1 = new gp_Dir( 0, 1, 0 );
					radialDir2 = new gp_Dir( 0, 0, 1 );
					break;
				case EAlignAxis.Y:
					radialDir1 = new gp_Dir( 0, 0, 1 );
					radialDir2 = new gp_Dir( 1, 0, 0 );
					break;
				default:
					radialDir1 = new gp_Dir( 1, 0, 0 );
					radialDir2 = new gp_Dir( 0, 1, 0 );
					break;
			}
		}

		// the direction of the given axis
		public static gp_Dir GetAxisDir( EAlignAxis axis )
		{
			switch( axis ) {
				case EAlignAxis.X:
					return new gp_Dir( 1, 0, 0 );
				case EAlignAxis.Y:
					return new gp_Dir( 0, 1, 0 );
				default:
					return new gp_Dir( 0, 0, 1 );
			}
		}

		// make the OBB edge line completely perpendicular to the axis
		public static bool AlignOBBToRadialAxes( TopoDS_Shape shape, EAlignAxis axis, out TopoDS_Shape alignedShape, out gp_Trsf trsf )
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

				// Step 2: Get OBB principal direction
				gp_XYZ xDirectionXYZ = obb.XDirection();
				gp_Dir xDirection = new gp_Dir( xDirectionXYZ );

				// Step 3: Project the OBB onto the two radial directions
				GetRadialDirs( axis, out gp_Dir radialDir1, out gp_Dir radialDir2 );
				double radialComponent1 = xDirection.Dot( radialDir1 );
				double radialComponent2 = xDirection.Dot( radialDir2 );

				// the projection is too short to give a stable angle, keep the shape as it is
				if( Math.Sqrt( radialComponent1 * radialComponent1 + radialComponent2 * radialComponent2 ) < RADIAL_PROJECTION_Threshold ) {
					alignedShape = shape;
					return true;
				}

				// cal the angle between the projected vector and the first radial direction	
				double angle = Math.Atan2( radialComponent2, radialComponent1 );

				// Step 4: Rotate around the given axis at origin by -angle
				gp_Ax1 axisAtOrigin = new gp_Ax1( new gp_Pnt( 0, 0, 0 ), GetAxisDir( axis ) );

				trsf.SetRotation( axisAtOrigin, -angle );

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

		// minimum radial projection length that gives a stable angle
		const double RADIAL_PROJECTION_Threshold = 1e-3;
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
