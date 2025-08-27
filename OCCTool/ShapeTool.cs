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
