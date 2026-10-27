using MyCAM.Data;
using System;
using OCC.gp;
using OCC.TopoDS;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	public struct TraversePathResult
	{
		public IProcessPoint PreviousPathEnd;
		public IProcessPoint LiftUpPoint;
		public IProcessPoint FrogLeapMiddlePoint;
		public IProcessPoint CutDownPoint;
		public IProcessPoint CurrentPathStart;
		public gp_Pnt SafePlaneLiftUpProjPoint;
		public gp_Pnt SafePlaneCutDownProjPoint;
		public List<gp_Pnt> SafeBoundaryPointList;
		public int SafeBoundaryEntryIndex;
		public int SafeBoundaryExitIndex;
	}

	public static class TraverseHelper
	{
		public static IProcessPoint GetCutDownOrLiftUpPoint( IProcessPoint pathPoint, double dDistance )
		{
			if( pathPoint == null ) {
				return null;
			}
			if( dDistance == 0 ) {
				return pathPoint.Clone();
			}

			// get cut down point
			gp_Vec toolVec = new gp_Vec( pathPoint.ToolVec );
			toolVec.Normalize();
			gp_Vec scaledToolVec = toolVec.Scaled( dDistance );
			gp_Pnt resultPoint = pathPoint.Point.Translated( scaledToolVec );

			// create tool up/down point info
			return BuildTraversePoint( resultPoint, pathPoint.ToolVec );
		}

		public static IProcessPoint GetFrogLeapMiddlePoint( IProcessPoint frogLeapStartPoint, IProcessPoint frogLeapEndPoint, double frogLeapDistance )
		{
			if( frogLeapStartPoint == null || frogLeapEndPoint == null ) {
				return null;
			}

			// get middle point of traverse
			gp_Pnt moveMidP = new gp_Pnt(
				( frogLeapStartPoint.Point.X() + frogLeapEndPoint.Point.X() ) / 2.0,
				( frogLeapStartPoint.Point.Y() + frogLeapEndPoint.Point.Y() ) / 2.0,
				( frogLeapStartPoint.Point.Z() + frogLeapEndPoint.Point.Z() ) / 2.0
			);

			// if frog leap distance is zero, return middle point between start and end point
			if( frogLeapDistance == 0 ) {
				return BuildTraversePoint( moveMidP, new gp_Dir() ); // tool vector doesn't matter
			}

			// if tool vectors are parallel, use Z axis as moving direction
			gp_Dir arcMidDir;
			if( frogLeapStartPoint.ToolVec.IsOpposite( frogLeapEndPoint.ToolVec, 1e-3 ) ) {
				arcMidDir = new gp_Dir( 0, 0, 1 );
			}

			// get moving direction vector by average of tool vector
			else {
				arcMidDir = new gp_Dir(
				( frogLeapStartPoint.ToolVec.X() + frogLeapEndPoint.ToolVec.X() ) / 2.0,
				( frogLeapStartPoint.ToolVec.Y() + frogLeapEndPoint.ToolVec.Y() ) / 2.0,
				( frogLeapStartPoint.ToolVec.Z() + frogLeapEndPoint.ToolVec.Z() ) / 2.0
			);
			}

			// get frog leap middle point
			gp_Vec moveToArcMidVec = new gp_Vec( arcMidDir );
			moveToArcMidVec.Normalize();
			gp_Vec scaledMoveToArcMidVec = moveToArcMidVec.Scaled( frogLeapDistance );
			gp_Pnt frogLeapMidP = moveMidP.Translated( scaledMoveToArcMidVec );

			// create frog leap middle point info
			return BuildTraversePoint( frogLeapMidP, arcMidDir );
		}

		public static gp_Pnt ProjectPointToPlane( gp_Pnt point, gp_Pln plane )
		{
			gp_Pnt planeLocation = plane.Location();
			double distance = planeLocation.Z() - point.Z();
			gp_Pnt projectedPoint = new gp_Pnt( point.X(), point.Y(), point.Z() + distance );
			return projectedPoint;
		}

		public static bool TryCalculateTraversePoints( IProcessPoint previousPathEnd, IProcessPoint currentPathStart, 
			TraverseData traverseData, out TraversePathResult result )
		{
			gp_Dir boundaryAxis = null;
			if( traverseData != null && ( traverseData.TraverseMode == TraverseData.TraverseType.SafeCylinder ||
				traverseData.TraverseMode == TraverseData.TraverseType.SafeSphere ) ) {
				boundaryAxis = GetSafeBoundaryAxis();
			}
			return TryCalculateTraversePoints( previousPathEnd, currentPathStart, traverseData, boundaryAxis, out result );
		}

		public static bool TryCalculateTraversePoints( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, gp_Dir boundaryAxis, out TraversePathResult result )
		{
			result = new TraversePathResult();

			if( previousPathEnd == null || currentPathStart == null || traverseData == null ) {
				return false;
			}

			// end of previous path
			result.PreviousPathEnd = previousPathEnd;

			// lift up point of previous path
			result.LiftUpPoint = GetCutDownOrLiftUpPoint( previousPathEnd, traverseData.LiftUpDistance );

			// start of current path
			result.CurrentPathStart = currentPathStart;

			// cut down point of current path
			result.CutDownPoint = GetCutDownOrLiftUpPoint( currentPathStart, traverseData.CutDownDistance );

			if( result.LiftUpPoint == null || result.CutDownPoint == null ) {
				return false;
			}

			switch( traverseData.TraverseMode ) {
				// frog leap middle point (if frog leap mode)
				case TraverseData.TraverseType.FrogLeap:
					if( traverseData.FrogLeapDistance > 0 ) {
						result.FrogLeapMiddlePoint = GetFrogLeapMiddlePoint( result.LiftUpPoint, result.CutDownPoint, traverseData.FrogLeapDistance );
					}
					return true;

				case TraverseData.TraverseType.SafePlane:
					gp_Pln safePlane = CreateSafePlane( traverseData.SafePlaneDistance );
					result.SafePlaneLiftUpProjPoint = ProjectPointToPlane( result.LiftUpPoint.Point, safePlane );
					result.SafePlaneCutDownProjPoint = ProjectPointToPlane( result.CutDownPoint.Point, safePlane );
					return true;

				case TraverseData.TraverseType.SafeCylinder:
				case TraverseData.TraverseType.SafeSphere:
					return TryCalculateSafeBoundaryTraverse( previousPathEnd, currentPathStart, traverseData, boundaryAxis,
						out result.SafeBoundaryPointList, out _,
						OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION, OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
						traverseData.TraverseMode,
						out result.SafeBoundaryEntryIndex, out result.SafeBoundaryExitIndex );

				default:
					return false;
			}
		}

		// this method is now for building a CAMPoint as ITraversePoint
		static IProcessPoint BuildTraversePoint( gp_Pnt point, gp_Dir toolVec )
		{
			CADPoint cadPoint = new CADPoint(
				point,
				new gp_Dir(), // doesn't matter
				new gp_Dir(), // doesn't matter
				new gp_Dir() // doesn't matter
			);
			return new CAMPoint( cadPoint, toolVec );
		}

		static gp_Pln CreateSafePlane( double safePlaneZ )
		{
			gp_Pnt safePlaneOrigin = new gp_Pnt( 0, 0, safePlaneZ );
			gp_Dir safePlaneNormal = new gp_Dir( 0, 0, 1 );
			return new gp_Pln( safePlaneOrigin, safePlaneNormal );
		}

		public static gp_Dir GetSafeBoundaryAxis()
		{
			if( DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				if( machineData.TubeDir == RotaryAxis.X ) {
					return new gp_Dir( 1, 0, 0 );
				}
				if( machineData.TubeDir == RotaryAxis.Y ) {
					return new gp_Dir( 0, 1, 0 );
				}
			}
			return new gp_Dir( 0, 0, 1 );
		}

		public static bool TryCalculateSafeCylinderTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, gp_Dir cylinderAxis,
			out System.Collections.Generic.List<gp_Pnt> points, out OCC.TopoDS.TopoDS_Edge surfaceEdge,
			double maxDeflection = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH )
		{
			return TryCalculateSafeBoundaryTraverse( previousPathEnd, currentPathStart, traverseData,
				cylinderAxis, out points, out surfaceEdge, maxDeflection, maxEdgeLength,
				TraverseData.TraverseType.SafeCylinder );
		}

		public static bool TryCalculateSafeSphereTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData,
			out System.Collections.Generic.List<gp_Pnt> points, out OCC.TopoDS.TopoDS_Edge surfaceEdge,
			double maxDeflection = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH )
		{
			return TryCalculateSafeSphereTraverse( previousPathEnd, currentPathStart, traverseData,
				new gp_Dir( 0, 0, 1 ), out points, out surfaceEdge, maxDeflection, maxEdgeLength );
		}

		public static bool TryCalculateSafeSphereTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, gp_Dir latitudeAxis,
			out System.Collections.Generic.List<gp_Pnt> points, out OCC.TopoDS.TopoDS_Edge surfaceEdge,
			double maxDeflection = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH )
		{
			return TryCalculateSafeBoundaryTraverse( previousPathEnd, currentPathStart, traverseData,
				latitudeAxis, out points, out surfaceEdge, maxDeflection, maxEdgeLength,
				TraverseData.TraverseType.SafeSphere );
		}

		public static bool TryCalculateSafeBoundaryTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, gp_Dir cylinderAxis,
			out System.Collections.Generic.List<gp_Pnt> points, out OCC.TopoDS.TopoDS_Edge surfaceEdge,
			double maxDeflection = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION,
			double maxEdgeLength = OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH )
		{
			return TryCalculateSafeBoundaryTraverse( previousPathEnd, currentPathStart, traverseData,
				cylinderAxis, out points, out surfaceEdge, maxDeflection, maxEdgeLength,
				traverseData == null ? TraverseData.TraverseType.FrogLeap : traverseData.TraverseMode );
		}

		static bool TryCalculateSafeBoundaryTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, gp_Dir cylinderAxis,
			out System.Collections.Generic.List<gp_Pnt> points, out OCC.TopoDS.TopoDS_Edge surfaceEdge,
			double maxDeflection, double maxEdgeLength, TraverseData.TraverseType requiredMode )
		{
			return TryCalculateSafeBoundaryTraverse( previousPathEnd, currentPathStart, traverseData, cylinderAxis,
				out points, out surfaceEdge, maxDeflection, maxEdgeLength, requiredMode, out _, out _ );
		}

		static bool TryCalculateSafeBoundaryTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, gp_Dir cylinderAxis,
			out System.Collections.Generic.List<gp_Pnt> points, out OCC.TopoDS.TopoDS_Edge surfaceEdge,
			double maxDeflection, double maxEdgeLength, TraverseData.TraverseType requiredMode,
			out int entryIndex, out int exitIndex )
		{
			points = new System.Collections.Generic.List<gp_Pnt>();
			surfaceEdge = null;
			entryIndex = 0;
			exitIndex = 0;
			if( previousPathEnd == null || currentPathStart == null || traverseData == null ||
				traverseData.TraverseMode != requiredMode ||
				( requiredMode != TraverseData.TraverseType.SafeCylinder && requiredMode != TraverseData.TraverseType.SafeSphere ) ||
				!IsPositiveFinite( maxDeflection ) || !IsPositiveFinite( maxEdgeLength ) ) {
				return false;
			}
			bool isSphere = requiredMode == TraverseData.TraverseType.SafeSphere;
			double radius = isSphere ? traverseData.SafeSphereRadius : traverseData.SafeCylinderRadius;
			if( !IsPositiveFinite( radius ) || radius <= 1e-6 || ( !isSphere && cylinderAxis == null ) ) {
				return false;
			}
			gp_Ax3 axes = new gp_Ax3( new gp_Pnt( 0, 0, 0 ), cylinderAxis ?? new gp_Dir( 0, 0, 1 ) );
			IProcessPoint liftUpPoint = GetCutDownOrLiftUpPoint( previousPathEnd, traverseData.LiftUpDistance );
			IProcessPoint cutDownPoint = GetCutDownOrLiftUpPoint( currentPathStart, traverseData.CutDownDistance );
			if( liftUpPoint == null || cutDownPoint == null ||
				!TryProjectTraversePoint( liftUpPoint, axes, radius, isSphere, out gp_Pnt arcStart ) ||
				!TryProjectTraversePoint( cutDownPoint, axes, radius, isSphere, out gp_Pnt arcEnd ) ) {
				return false;
			}

			if( !TryBuildBoundaryPath( arcStart, arcEnd, axes, radius, isSphere,
				maxDeflection, maxEdgeLength, out points, out surfaceEdge ) ) {
				return false;
			}
			if( liftUpPoint.Point.Distance( points[ 0 ] ) > 0 ) {
				points.Insert( 0, liftUpPoint.Point );
				entryIndex = 1;
			}
			exitIndex = points.Count - 1;
			if( cutDownPoint.Point.Distance( points[ points.Count - 1 ] ) > 0 ) {
				points.Add( cutDownPoint.Point );
			}
			return true;
		}

		public static bool TryProjectToSafeRadialBoundary( gp_Pnt point, gp_Ax3 axes, double radius,
			bool isSphere, out gp_Pnt projectedPoint )
		{
			projectedPoint = null;
			if( point == null || axes == null || !IsPositiveFinite( radius ) || radius <= 1e-6 ||
				double.IsNaN( point.X() ) || double.IsInfinity( point.X() ) ||
				double.IsNaN( point.Y() ) || double.IsInfinity( point.Y() ) ||
				double.IsNaN( point.Z() ) || double.IsInfinity( point.Z() ) ) {
				return false;
			}
			gp_Vec offset = new gp_Vec( axes.Location(), point );
			gp_Vec radial = isSphere ? offset : offset.Subtracted(
				new gp_Vec( axes.Direction() ).Scaled( offset.Dot( new gp_Vec( axes.Direction() ) ) ) );
			if( radial.Magnitude() <= 1e-6 ) {
				return false;
			}
			if( !IsPositiveFinite( radial.Magnitude() ) ) {
				return false;
			}
			gp_Pnt center = isSphere ? axes.Location() : point.Translated( radial.Reversed() );
			projectedPoint = center.Translated( radial.Scaled( radius / radial.Magnitude() ) );
			return IsFinitePoint( projectedPoint );
		}

		static bool TryProjectTraversePoint( IProcessPoint point, gp_Ax3 axes, double radius, bool isSphere,
			out gp_Pnt projectedPoint )
		{
			if( TryProjectToSafeRadialBoundary( point.Point, axes, radius, isSphere, out projectedPoint ) ) {
				return true;
			}
			if( !IsFinitePoint( point.Point ) ) {
				return false;
			}
			gp_Vec offset = new gp_Vec( axes.Location(), point.Point );
			gp_Vec axis = new gp_Vec( axes.Direction() );
			gp_Vec radial = isSphere ? offset : offset.Subtracted( axis.Scaled( offset.Dot( axis ) ) );
			if( radial.Magnitude() > 1e-6 ) {
				return false;
			}
			// On the cylinder axis / sphere center, the nearest point is not unique.
			// Use the outward lift/cut-down tool direction, not the inward cutting motion.
			gp_Vec direction = new gp_Vec( point.ToolVec );
			if( !isSphere ) {
				direction = direction.Subtracted( axis.Scaled( direction.Dot( axis ) ) );
			}
			if( direction.Magnitude() <= 1e-12 ) {
				direction = GetBoundaryReferenceDirection( axis, axes.Direction() );
			}
			direction.Normalize();
			gp_Pnt center = isSphere ? axes.Location() :
				axes.Location().Translated( axis.Scaled( offset.Dot( axis ) ) );
			projectedPoint = center.Translated( direction.Scaled( radius ) );
			return IsFinitePoint( projectedPoint );
		}

		static gp_Vec GetBoundaryReferenceDirection( gp_Vec radial, gp_Dir boundaryAxis )
		{
			var candidates = new List<gp_Dir>() { boundaryAxis };
			if( DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				candidates.Add( machineData.MasterRotateDir );
				candidates.Add( machineData.SlaveRotateDir );
			}
			foreach( gp_Dir candidate in candidates ) {
				gp_Vec reference = new gp_Vec( candidate );
				gp_Vec tangent = reference.Subtracted( radial.Scaled( reference.Dot( radial ) ) );
				if( tangent.Magnitude() > 1e-12 ) {
					tangent.Normalize();
					return tangent;
				}
			}
			gp_Vec fallback = new gp_Vec( 1, 0, 0 );
			if( Math.Abs( radial.Y() ) < Math.Abs( radial.X() ) ) {
				fallback = new gp_Vec( 0, 1, 0 );
			}
			if( Math.Abs( radial.Z() ) < Math.Abs( fallback.Dot( radial ) ) ) {
				fallback = new gp_Vec( 0, 0, 1 );
			}
			gp_Vec result = fallback.Subtracted( radial.Scaled( fallback.Dot( radial ) ) );
			result.Normalize();
			return result;
		}

		static bool TryBuildBoundaryPath( gp_Pnt start, gp_Pnt end, gp_Ax3 axes, double radius, bool isSphere,
			double maxDeflection, double maxEdgeLength, out List<gp_Pnt> points, out TopoDS_Edge surfaceEdge )
		{
			points = new List<gp_Pnt>();
			surfaceEdge = null;
			gp_Vec axis = new gp_Vec( axes.Direction() );
			gp_Vec startOffset = new gp_Vec( axes.Location(), start );
			gp_Vec endOffset = new gp_Vec( axes.Location(), end );
			double startHeight = isSphere ? 0 : startOffset.Dot( axis );
			double heightChange = isSphere ? 0 : endOffset.Dot( axis ) - startHeight;
			gp_Pnt center = axes.Location().Translated( axis.Scaled( startHeight ) );
			gp_Vec entry = new gp_Vec( center, start );
			gp_Vec exit = endOffset.Subtracted( axis.Scaled( isSphere ? 0 : endOffset.Dot( axis ) ) );
			entry.Normalize();
			exit.Normalize();
			gp_Vec normal;
			double sweep;
			if( isSphere ) {
				normal = entry.Crossed( exit );
				double sine = normal.Magnitude();
				double cosine = Math.Max( -1, Math.Min( 1, entry.Dot( exit ) ) );
				sweep = Math.Atan2( sine, cosine );
				if( sine <= 1e-12 ) {
					if( cosine >= 0 ) {
						sweep = 0;
					}
					// Antipodal endpoints admit infinitely many equal-length semicircles.
					// Prefer TubeDir, then the machine axes, then a stable world-axis fallback.
					// TODO: Consider collision constraints when choosing an antipodal plane.
					normal = entry.Crossed( GetBoundaryReferenceDirection( entry, axes.Direction() ) );
				}
				normal.Normalize();
			}
			else {
				double signedAngle = Math.Atan2( axis.Dot( entry.Crossed( exit ) ), entry.Dot( exit ) );
				// Clockwise viewed from the positive boundary axis towards the origin.
				// Equal azimuths move axially without adding a full revolution.
				sweep = Math.Abs( signedAngle ) <= 1e-12 ? 0 :
					( signedAngle > 0 ? 2 * Math.PI - signedAngle : -signedAngle );
				normal = axis.Reversed();
			}
			gp_Vec tangent = normal.Crossed( entry );
			double length = Math.Sqrt( radius * sweep * ( radius * sweep ) + heightChange * heightChange );
			// The axial component is linear; only the circular component contributes chord error.
			double maxAngle = Math.Min( Math.PI, 4 * Math.Asin( Math.Sqrt( Math.Min( 0.5, maxDeflection / ( 2 * radius ) ) ) ) );
			double segmentCount = Math.Max( 1, Math.Max( Math.Ceiling( length / maxEdgeLength ),
				Math.Ceiling( sweep / maxAngle ) ) );
			if( !IsPositiveFinite( segmentCount ) || segmentCount >= int.MaxValue ) {
				return false;
			}
			int count = (int)segmentCount;
			for( int index = 0; index <= count; index++ ) {
				double ratio = (double)index / count;
				double angle = sweep * ratio;
				points.Add( center.Translated( entry.Scaled( radius * Math.Cos( angle ) )
					.Added( tangent.Scaled( radius * Math.Sin( angle ) ) )
					.Added( axis.Scaled( heightChange * ratio ) ) ) );
			}
			// Keep both endpoints even when coincident so stage 3 can rotate in place.
			points[ 0 ] = start;
			points[ count ] = end;
			if( sweep == 0 && heightChange == 0 ) {
				return true;
			}
			OCC.BRepBuilderAPI.BRepBuilderAPI_MakeEdge maker;
			if( sweep == 0 ) {
				maker = new OCC.BRepBuilderAPI.BRepBuilderAPI_MakeEdge( start, end );
			}
			else if( heightChange == 0 ) {
				maker = new OCC.BRepBuilderAPI.BRepBuilderAPI_MakeEdge(
					new gp_Circ( new gp_Ax2( center, new gp_Dir( normal ), new gp_Dir( entry ) ), radius ), 0, sweep );
			}
			else {
				double startAngle = Math.Atan2( entry.Dot( new gp_Vec( axes.YDirection() ) ),
					entry.Dot( new gp_Vec( axes.XDirection() ) ) );
				var line = new OCC.Geom2d.Geom2d_Line( new gp_Pnt2d( startAngle, startHeight ),
					new gp_Dir2d( -sweep, heightChange ) );
				maker = new OCC.BRepBuilderAPI.BRepBuilderAPI_MakeEdge( line,
					new OCC.Geom.Geom_CylindricalSurface( axes, radius ), 0,
					Math.Sqrt( sweep * sweep + heightChange * heightChange ) );
			}
			if( !maker.IsDone() ) {
				return false;
			}
			surfaceEdge = maker.Edge();
			if( heightChange != 0 && sweep != 0 && !OCC.BRepLib.BRepLib.BuildCurve3d( surfaceEdge ) ) {
				return false;
			}
			return true;
		}

		internal static bool TryGetPlanePreviewBounds( IReadOnlyList<TraversePathResult> results,
			out double minX, out double maxX, out double minY, out double maxY )
		{
			minX = maxX = minY = maxY = 0;
			bool hasPoints = false;
			foreach( TraversePathResult result in results ) {
				if( result.SafePlaneLiftUpProjPoint == null || result.SafePlaneCutDownProjPoint == null ) {
					continue;
				}
				foreach( gp_Pnt point in new[] { result.SafePlaneLiftUpProjPoint, result.SafePlaneCutDownProjPoint } ) {
					minX = Math.Min( minX, point.X() );
					maxX = Math.Max( maxX, point.X() );
					minY = Math.Min( minY, point.Y() );
					maxY = Math.Max( maxY, point.Y() );
				}
				hasPoints = true;
			}
			if( !hasPoints ) {
				return false;
			}
			double margin = Math.Max( 10, Math.Max( maxX - minX, maxY - minY ) * 0.2 );
			minX -= margin;
			maxX += margin;
			minY -= margin;
			maxY += margin;
			return true;
		}

		internal static bool TryGetRadialPreviewBounds( IReadOnlyList<TraversePathResult> results, gp_Ax3 axes,
			double radius, bool isSphere, out double minU, out double maxU, out double minV, out double maxV )
		{
			if( axes == null ) {
				throw new ArgumentNullException( nameof( axes ) );
			}
			if( double.IsNaN( radius ) || double.IsInfinity( radius ) || radius <= 1e-6 ) {
				throw new ArgumentOutOfRangeException( nameof( radius ) );
			}
			minU = maxU = 0;
			minV = double.MaxValue;
			maxV = double.MinValue;
			var intervals = new List<(double start, double end)>();
			gp_Vec xDirection = new gp_Vec( axes.XDirection() );
			gp_Vec yDirection = new gp_Vec( axes.YDirection() );
			gp_Vec axis = new gp_Vec( axes.Direction() );
			double maxAngularStep = 0;
			foreach( TraversePathResult result in results ) {
				List<gp_Pnt> points = result.SafeBoundaryPointList;
				if( points == null || points.Count == 0 ) {
					continue;
				}
				double previousAngle = 0;
				double unwrappedAngle = 0;
				double pathMin = double.MaxValue;
				double pathMax = double.MinValue;
				gp_Vec previousOffset = null;
				for( int index = result.SafeBoundaryEntryIndex; index <= result.SafeBoundaryExitIndex; index++ ) {
					gp_Vec offset = new gp_Vec( axes.Location(), points[ index ] );
					double angle = Math.Atan2( offset.Dot( yDirection ), offset.Dot( xDirection ) );
					if( previousOffset == null ) {
						unwrappedAngle = angle;
					}
					else {
						double delta = Math.Atan2( Math.Sin( angle - previousAngle ), Math.Cos( angle - previousAngle ) );
						// Cylinder paths are clockwise, including a possible half-turn segment.
						if( !isSphere && delta > 1e-12 ) {
							delta -= TWO_PI;
						}
						unwrappedAngle += delta;
						if( isSphere ) {
							maxAngularStep = Math.Max( maxAngularStep,
								Math.Atan2( previousOffset.Crossed( offset ).Magnitude(), previousOffset.Dot( offset ) ) );
						}
					}
					pathMin = Math.Min( pathMin, unwrappedAngle );
					pathMax = Math.Max( pathMax, unwrappedAngle );
					double positionV = offset.Dot( axis );
					if( isSphere ) {
						positionV = Math.Asin( Math.Max( -1, Math.Min( 1, positionV / radius ) ) );
					}
					minV = Math.Min( minV, positionV );
					maxV = Math.Max( maxV, positionV );
					previousAngle = angle;
					previousOffset = offset;
				}
				AddCircularInterval( intervals, pathMin, pathMax );
			}
			if( intervals.Count == 0 ) {
				return false;
			}

			GetCircularBounds( intervals, out minU, out maxU );
			bool includesPole = false;
			if( isSphere ) {
				// Latitude changes by at most the central angle of each sampled great-circle segment.
				minV = Math.Max( -Math.PI / 2, minV - maxAngularStep );
				maxV = Math.Min( Math.PI / 2, maxV + maxAngularStep );
				includesPole = minV <= -Math.PI / 2 || maxV >= Math.PI / 2;
				double margin = Math.Max( 10.0 / radius, ( maxV - minV ) * 0.2 );
				minV = Math.Max( -Math.PI / 2, minV - margin );
				maxV = Math.Min( Math.PI / 2, maxV + margin );
			}
			else {
				minV = Math.Min( 0, minV );
				maxV = Math.Max( 0, maxV );
				double margin = Math.Max( 10, ( maxV - minV ) * 0.2 );
				minV -= margin;
				maxV += margin;
			}
			double angularMargin = Math.Max( 10.0 / radius, ( maxU - minU ) * 0.2 );
			if( includesPole || maxU - minU + 2 * angularMargin >= TWO_PI ) {
				minU = 0;
				maxU = TWO_PI;
			}
			else {
				minU -= angularMargin;
				maxU += angularMargin;
			}
			return true;
		}

		static void AddCircularInterval( List<(double start, double end)> intervals, double min, double max )
		{
			double width = Math.Min( TWO_PI, max - min );
			double start = min % TWO_PI;
			if( start < 0 ) {
				start += TWO_PI;
			}
			double end = start + width;
			intervals.Add( (start, Math.Min( TWO_PI, end )) );
			if( end > TWO_PI ) {
				intervals.Add( (0, end - TWO_PI) );
			}
		}

		static void GetCircularBounds( List<(double start, double end)> intervals, out double min, out double max )
		{
			intervals.Sort( ( left, right ) => left.start.CompareTo( right.start ) );
			double end = intervals[ 0 ].end;
			double largestGap = 0;
			min = intervals[ 0 ].start;
			for( int index = 1; index < intervals.Count; index++ ) {
				double gap = intervals[ index ].start - end;
				if( gap > largestGap ) {
					largestGap = gap;
					min = intervals[ index ].start;
				}
				end = Math.Max( end, intervals[ index ].end );
			}
			double seamGap = intervals[ 0 ].start + TWO_PI - end;
			if( seamGap > largestGap ) {
				largestGap = seamGap;
				min = intervals[ 0 ].start;
			}
			max = min + TWO_PI - largestGap;
		}

		const double TWO_PI = 2 * Math.PI;

		static bool IsFinitePoint( gp_Pnt point )
		{
			return point != null &&
				!double.IsNaN( point.X() ) && !double.IsInfinity( point.X() ) &&
				!double.IsNaN( point.Y() ) && !double.IsInfinity( point.Y() ) &&
				!double.IsNaN( point.Z() ) && !double.IsInfinity( point.Z() );
		}

		static bool IsPositiveFinite( double value )
		{
			return !double.IsNaN( value ) && !double.IsInfinity( value ) && value > 0;
		}
	}
}
