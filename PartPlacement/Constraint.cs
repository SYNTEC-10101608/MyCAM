using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

namespace PartPlacement
{
	enum EConstraintType
	{
		Axial,
		AxialParallel,
		Plane,
		PlaneParallel,
	}

	internal interface IConstraint
	{
		EConstraintType Type
		{
			get;
		}

		bool IsValid();

		gp_Trsf SolveConstraint();
	}

	internal class ConstraintBase : IConstraint
	{
		public ConstraintBase( TopoDS_Shape refShape, TopoDS_Shape moveShape, bool isReverse )
		{
			m_RefShape = refShape;
			m_MoveShape = moveShape;
			m_isReverse = isReverse;
		}

		public virtual EConstraintType Type
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
		public AxialConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape, bool isReverse )
			: base( refShape, moveShape, isReverse ) { }

		public override EConstraintType Type => EConstraintType.Axial;

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
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, EConstraintType.Axial, m_isReverse );
		}
	}

	internal class AxialParallelConstraint : ConstraintBase
	{
		public AxialParallelConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape, bool isReverse )
			: base( refShape, moveShape, isReverse ) { }

		public override EConstraintType Type => EConstraintType.AxialParallel;

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
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, EConstraintType.AxialParallel, m_isReverse );
		}
	}

	// the plane constraint
	internal class PlaneConstraint : ConstraintBase
	{
		public PlaneConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape, bool isReverse )
			: base( refShape, moveShape, isReverse ) { }

		public override EConstraintType Type => EConstraintType.Plane;

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
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, EConstraintType.Plane, m_isReverse );
		}
	}

	// the parallel plane constraint
	internal class PlaneParallelConstraint : ConstraintBase
	{
		public PlaneParallelConstraint( TopoDS_Shape refShape, TopoDS_Shape moveShape, bool isReverse )
			: base( refShape, moveShape, isReverse ) { }

		public override EConstraintType Type => EConstraintType.PlaneParallel;

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
			return ConstraintHelper.SolveConstraint( pR, pM, dR, dM, EConstraintType.PlaneParallel, m_isReverse );
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
				if( GeometryTool.IsAxialSymmetry( TopoDS.ToFace( refShape ), out pR, out dR ) ) {
					isValid1 = true;
				}
				isValid1 = true;
			}
			else if( refShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {
				if( GeometryTool.IsLine( TopoDS.ToEdge( refShape ), out pR, out dR ) ) {
					isValid1 = true;
				}
			}
			bool isValid2 = false;
			if( moveShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_FACE ) {
				if( GeometryTool.IsAxialSymmetry( TopoDS.ToFace( moveShape ), out pM, out dM ) ) {
					isValid2 = true;
				}
			}
			else if( moveShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE ) {
				if( GeometryTool.IsLine( TopoDS.ToEdge( moveShape ), out pM, out dM ) ) {
					isValid2 = true;
				}
			}
			return isValid1 && isValid2;
		}

		public static gp_Trsf SolveConstraint( gp_Pnt pR, gp_Pnt pM, gp_Dir dR, gp_Dir dM, EConstraintType type, bool isReverse )
		{
			if( isReverse ) {
				dR.Reverse();
			}

			// solve rotation
			gp_Vec vecR = new gp_Vec( dR );
			gp_Vec vecM = new gp_Vec( dM );
			gp_Quaternion q = new gp_Quaternion( vecM, vecR );
			gp_Trsf trsfR = new gp_Trsf();
			trsfR.SetRotation( q );

			// return for parallel constraint
			if( type == EConstraintType.AxialParallel || type == EConstraintType.PlaneParallel ) {
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
			if( type == EConstraintType.Axial ) {
				trsfT.SetTranslation( vecPerp );
			}
			else if( type == EConstraintType.Plane ) {
				trsfT.SetTranslation( vecAlong );
			}
			return trsfT.Multiplied( trsfR );
		}
	}
}
