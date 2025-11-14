using System;
using OCC.BRep;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

namespace MyCAM.Editor
{
	internal interface IConstraint
	{
		ETrsfConstraintType Type
		{
			get;
		}

		bool IsValid();

		gp_Trsf SolveConstraint();
	}

	internal class ConstraintBase : IConstraint
	{
		public ConstraintBase( TopoDS_Shape refShape, TopoDS_Shape moveShape )
		{
			m_RefShape = refShape;
			m_MoveShape = moveShape;
		}

		public virtual ETrsfConstraintType Type
		{
			get;
		}

		public virtual bool IsValid()
		{
			return false;
		}

		public virtual gp_Trsf SolveConstraint()
		{
			return new gp_Trsf();
		}

		protected TopoDS_Shape m_RefShape;
		protected TopoDS_Shape m_MoveShape;
		protected bool m_isReverse;
	}

	internal class AxialConstraint : ConstraintBase
	{
		public AxialConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape )
			: base( refShape, moveShape ) { }

		public override ETrsfConstraintType Type => ETrsfConstraintType.Axial;

		public override bool IsValid()
		{
			return ConstraintHelper.IsAxisAxisValid( m_RefShape, m_MoveShape, out _, out _, out _, out _ );
		}

		public override gp_Trsf SolveConstraint()
		{
			if( !ConstraintHelper.IsAxisAxisValid( m_RefShape, m_MoveShape,
				out gp_Pnt pR, out gp_Dir dR, out gp_Pnt pM, out gp_Dir dM ) ) {
				return new gp_Trsf();
			}
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, ETrsfConstraintType.Axial );
		}
	}

	internal class AxialParallelConstraint : ConstraintBase
	{
		public AxialParallelConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape )
			: base( refShape, moveShape ) { }

		public override ETrsfConstraintType Type => ETrsfConstraintType.AxialParallel;

		public override bool IsValid()
		{
			return ConstraintHelper.IsAxisAxisValid( m_RefShape, m_MoveShape, out _, out _, out _, out _ );
		}

		public override gp_Trsf SolveConstraint()
		{
			if( !ConstraintHelper.IsAxisAxisValid( m_RefShape, m_MoveShape,
				out gp_Pnt pR, out gp_Dir dR, out gp_Pnt pM, out gp_Dir dM ) ) {
				return new gp_Trsf();
			}
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, ETrsfConstraintType.AxialParallel );
		}
	}

	// the plane constraint
	internal class PlaneConstraint : ConstraintBase
	{
		public PlaneConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape )
			: base( refShape, moveShape ) { }

		public override ETrsfConstraintType Type => ETrsfConstraintType.Plane;

		public override bool IsValid()
		{
			return ConstraintHelper.IsPlanePlaneValid( m_RefShape, m_MoveShape, out _, out _, out _, out _ );
		}

		public override gp_Trsf SolveConstraint()
		{
			if( !ConstraintHelper.IsPlanePlaneValid( m_RefShape, m_MoveShape,
				out gp_Pnt pR, out gp_Dir dR, out gp_Pnt pM, out gp_Dir dM ) ) {
				return new gp_Trsf();
			}
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, ETrsfConstraintType.Plane );
		}
	}

	// the parallel plane constraint
	internal class PlaneParallelConstraint : ConstraintBase
	{
		public PlaneParallelConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape )
			: base( refShape, moveShape ) { }

		public override ETrsfConstraintType Type => ETrsfConstraintType.PlaneParallel;

		public override bool IsValid()
		{
			return ConstraintHelper.IsPlanePlaneValid( m_RefShape, m_MoveShape, out _, out _, out _, out _ );
		}

		public override gp_Trsf SolveConstraint()
		{
			if( !ConstraintHelper.IsPlanePlaneValid( m_RefShape, m_MoveShape,
				out gp_Pnt pR, out gp_Dir dR, out gp_Pnt pM, out gp_Dir dM ) ) {
				return new gp_Trsf();
			}
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, ETrsfConstraintType.PlaneParallel );
		}
	}


	internal class PointConstraint : ConstraintBase
	{
		public PointConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape )
			: base( refShape, moveShape ) { }

		public override ETrsfConstraintType Type => ETrsfConstraintType.Point;

		public override bool IsValid()
		{
			return ConstraintHelper.IsPointValid( m_RefShape, m_MoveShape, out _, out _ );
		}

		public override gp_Trsf SolveConstraint()
		{
			if( !ConstraintHelper.IsPointValid( m_RefShape, m_MoveShape, out gp_Pnt pR, out gp_Pnt pM ) ) {
				return new gp_Trsf();
			}
			return ConstraintHelper.SolveConstraint( pR, pM );
		}
	}

	internal static class ConstraintHelper
	{
		public static bool IsPlanePlaneValid( TopoDS_Shape refShape, TopoDS_Shape moveShape,
			out gp_Pnt pR, out gp_Dir dR, out gp_Pnt pM, out gp_Dir dM )
		{
			pR = new gp_Pnt();
			dR = new gp_Dir();
			pM = new gp_Pnt();
			dM = new gp_Dir();
			if( refShape == null || moveShape == null ) {
				return false;
			}
			bool isValid1 = false;
			if( refShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				if( GeometryTool.IsPlane( TopoDS.ToFace( refShape ), out pR, out dR ) ) {
					isValid1 = true;
				}
			}
			bool isValid2 = false;
			if( moveShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				if( GeometryTool.IsPlane( TopoDS.ToFace( moveShape ), out pM, out dM ) ) {
					isValid2 = true;
				}
			}
			return isValid1 && isValid2;
		}

		public static bool IsAxisAxisValid( TopoDS_Shape refShape, TopoDS_Shape moveShape,
			out gp_Pnt pR, out gp_Dir dR, out gp_Pnt pM, out gp_Dir dM )
		{
			pR = new gp_Pnt();
			dR = new gp_Dir();
			pM = new gp_Pnt();
			dM = new gp_Dir();
			if( refShape == null || moveShape == null ) {
				return false;
			}
			bool isValid1 = false;
			if( refShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				if( GeometryTool.IsAxialSymmetrySurface( TopoDS.ToFace( refShape ), out pR, out dR ) ) {
					isValid1 = true;
				}
			}
			else if( refShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {

				// check is line?
				isValid1 = GeometryTool.IsLine( TopoDS.ToEdge( refShape ), out pR, out dR );

				// check is circular arc?
				if( !isValid1 ) {
					isValid1 = GeometryTool.IsCircularArc( TopoDS.ToEdge( refShape ), out pR, out _, out dR, out _ );
				}
			}
			bool isValid2 = false;
			if( moveShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				if( GeometryTool.IsAxialSymmetrySurface( TopoDS.ToFace( moveShape ), out pM, out dM ) ) {
					isValid2 = true;
				}
			}
			else if( moveShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {

				// check is line?
				isValid2 = GeometryTool.IsLine( TopoDS.ToEdge( moveShape ), out pM, out dM );

				// check is circular arc?
				if( !isValid2 ) {
					isValid2 = GeometryTool.IsCircularArc( TopoDS.ToEdge( moveShape ), out pM, out _, out dM, out _ );
				}
			}
			return isValid1 && isValid2;
		}

		public static bool IsPointValid( TopoDS_Shape refShape, TopoDS_Shape moveShape,
			out gp_Pnt pR, out gp_Pnt pM )
		{
			pR = new gp_Pnt();
			pM = new gp_Pnt();
			if( refShape == null || moveShape == null ) {
				return false;
			}

			if( refShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX
				|| moveShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				return false;
			}
			pR = BRep_Tool.Pnt( TopoDS.ToVertex( refShape ) );
			pM = BRep_Tool.Pnt( TopoDS.ToVertex( moveShape ) );
			return true;
		}

		public static gp_Trsf SolveConstraint( gp_Pnt pR, gp_Pnt pM, gp_Dir dR, gp_Dir dM, ETrsfConstraintType type )
		{
			gp_Vec vecR = new gp_Vec( dR );
			gp_Vec vecM = new gp_Vec( dM );
			double angle = vecR.Angle( vecM );

			if( angle > Math.PI / 2 || angle < -( Math.PI / 2 ) ) {
				vecM.Reverse();
			}

			// solve rotation
			gp_Quaternion q = new gp_Quaternion( vecM, vecR );
			gp_Trsf trsfR = new gp_Trsf();
			trsfR.SetRotation( q );

			// return for parallel constraint
			if( type == ETrsfConstraintType.AxialParallel || type == ETrsfConstraintType.PlaneParallel ) {
				return trsfR;
			}

			// solve translation
			gp_Pnt pM1 = pM.Transformed( trsfR );
			gp_Vec vec = new gp_Vec( pM1, pR );

			// get projection vec along dR
			double dot = vec.Dot( new gp_Vec( dR ) );
			gp_Vec vecAlong = new gp_Vec( dR );
			vecAlong.Multiply( dot );

			// get projection vec perpendicular to dR
			gp_Vec vecPerp = vec - vecAlong;

			// solve translation by case
			gp_Trsf trsfT = new gp_Trsf();
			if( type == ETrsfConstraintType.Axial ) {
				trsfT.SetTranslation( vecPerp );
			}
			else if( type == ETrsfConstraintType.Plane ) {
				trsfT.SetTranslation( vecAlong );
			}
			return trsfT.Multiplied( trsfR );
		}

		public static gp_Trsf SolveConstraint( gp_Pnt pR, gp_Pnt pM )
		{
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetTranslation( new gp_Vec( pM, pR ) );
			return trsf;
		}
	}
}
