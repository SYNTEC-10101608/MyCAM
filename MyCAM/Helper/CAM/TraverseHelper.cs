using LanguageManager;
using MyCAM.App;
using MyCAM.Data;
using OCC.gp;
using OCC.TopoDS;
using OCCTool;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	public struct TraverseCalculationInfo
	{
		public gp_Ax3 SafeBoundaryAxes;

		public bool? RotaryAxisIsMaster;
	}

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
	}

	public static class TraverseHelper
	{
		const double TWO_PI = 2 * Math.PI;
		const double PREVIEW_MARGIN = 10;
		const double PREVIEW_MARGIN_RATIO = 0.2;

		internal static TraverseCalculationInfo BuildTraverseCalculationInfo( DataManager dataManager )
		{
			TraverseCalculationInfo result = new TraverseCalculationInfo();

			if( dataManager == null ) {
				return result;
			}
			if( !( dataManager.MachineData is FiveAxisMachineData machineData ) ) {
				return result;
			}

			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( string partID in dataManager.PartIDList ) {
				if( !dataManager.ObjectMap.TryGetValue( partID, out IObject obj ) ||
					!( obj is PartObject partObject ) ||
					partObject.Role != PartRole.Workpiece ) {
					continue;
				}
				if( partObject.Shape == null || partObject.Shape.IsNull() ) {
					continue;
				}
				shapeList.Add( partObject.Shape );
			}
			if( shapeList.Count == 0 ) {
				return result;
			}

			TopoDS_Shape workpieceCompound = ShapeTool.MakeCompound( shapeList );
			if( workpieceCompound == null || workpieceCompound.IsNull() ) {
				return result;
			}

			BoundingBox boundingBox = new BoundingBox( workpieceCompound );
			gp_Pnt bottomCenter = ( machineData.TubeDir == RotaryAxis.X )
				? new gp_Pnt( boundingBox.Xmin, boundingBox.YCenter, boundingBox.ZCenter )
				: ( machineData.TubeDir == RotaryAxis.Y )
					? new gp_Pnt( boundingBox.XCenter, boundingBox.Ymin, boundingBox.ZCenter )
					: new gp_Pnt( boundingBox.XCenter, boundingBox.YCenter, boundingBox.Zmin );

			result.SafeBoundaryAxes = new gp_Ax3( bottomCenter, RotateAxisDef.RotaryAxisToDir( machineData.TubeDir ) );

			result.RotaryAxisIsMaster = ( machineData.RotaryAxisConfig.RotaryAxisType == machineData.MasterRotaryAxis )
				? true
				: ( machineData.RotaryAxisConfig.RotaryAxisType == machineData.SlaveRotaryAxis )
					? false
					: (bool?)null;

			return result;
		}

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

		static bool GetFrogLeapMiddlePoint( IProcessPoint frogLeapStartPoint, IProcessPoint frogLeapEndPoint, double frogLeapDistance, out IProcessPoint middlePoint )
		{
			middlePoint = null;

			if( frogLeapStartPoint == null || frogLeapEndPoint == null ) {
				return false;
			}

			// get middle point of traverse
			gp_Pnt moveMidP = new gp_Pnt(
				( frogLeapStartPoint.Point.X() + frogLeapEndPoint.Point.X() ) / 2.0,
				( frogLeapStartPoint.Point.Y() + frogLeapEndPoint.Point.Y() ) / 2.0,
				( frogLeapStartPoint.Point.Z() + frogLeapEndPoint.Point.Z() ) / 2.0
			);

			// if frog leap distance is zero, return middle point between start and end point
			if( frogLeapDistance == 0 ) {
				middlePoint = BuildTraversePoint( moveMidP, new gp_Dir() ); // tool vector doesn't matter
				return true;
			}

			// if tool vectors are parallel, use Z axis as moving direction
			gp_Dir arcMidDir;
			if( frogLeapStartPoint.ToolVec.IsOpposite( frogLeapEndPoint.ToolVec, 1e-3 ) ) {
				arcMidDir = new gp_Dir( 0, 0, 1 );

				// use X when Z would be parallel to either tool vector.
				if( frogLeapStartPoint.ToolVec.IsEqual( arcMidDir, 1e-3 ) || frogLeapEndPoint.ToolVec.IsEqual( arcMidDir, 1e-3 ) ) {
					arcMidDir = new gp_Dir( 1, 0, 0 );
				}
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
			middlePoint = BuildTraversePoint( frogLeapMidP, arcMidDir );
			return true;
		}

		static gp_Pnt ProjectPointToPlane( gp_Pnt point, gp_Pln plane )
		{
			gp_Pnt planeLocation = plane.Location();
			double distance = planeLocation.Z() - point.Z();
			gp_Pnt projectedPoint = new gp_Pnt( point.X(), point.Y(), point.Z() + distance );
			return projectedPoint;
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

		public static bool TryCalculateTraversePoints( IProcessPoint previousPathEnd, IProcessPoint currentPathStart,
			TraverseData traverseData, TraverseCalculationInfo calculationData, out TraversePathResult result )
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
				// middle point for frog leap
				case TraverseData.TraverseType.FrogLeap:
					if( traverseData.FrogLeapDistance > 0 ) {
						if( !GetFrogLeapMiddlePoint( result.LiftUpPoint, result.CutDownPoint, traverseData.FrogLeapDistance, out result.FrogLeapMiddlePoint ) ) {
							result.FrogLeapMiddlePoint = null;
						}
					}
					return true;

				// project points for safe plane
				case TraverseData.TraverseType.SafePlane:
					gp_Pln safePlane = CreateSafePlane( traverseData.SafePlaneDistance );
					result.SafePlaneLiftUpProjPoint = ProjectPointToPlane( result.LiftUpPoint.Point, safePlane );
					result.SafePlaneCutDownProjPoint = ProjectPointToPlane( result.CutDownPoint.Point, safePlane );
					return true;

				// radial traverse for safe cylinder and sphere
				case TraverseData.TraverseType.SafeCylinder:
				case TraverseData.TraverseType.SafeSphere:
					if( !TryCalculateRadialTraverse( previousPathEnd, currentPathStart, result.LiftUpPoint, result.CutDownPoint,
						traverseData, calculationData, out result.SafeBoundaryPointList ) ) {
						result.SafeBoundaryPointList = null;
					}
					return true;

				default:
					return false;
			}
		}

		#region Safe cylinder and sphere

		public static double GetBoundingMaxValue( List<IShapeObject> shapeObjects, gp_Ax3 axes, bool isSphere )
		{
			if( shapeObjects == null || axes == null ) {
				return 0;
			}

			List<TopoDS_Shape> shapeList = new List<TopoDS_Shape>();
			foreach( IShapeObject shapeObject in shapeObjects ) {
				if( shapeObject?.Shape == null || shapeObject.Shape.IsNull() ) {
					continue;
				}
				shapeList.Add( shapeObject.Shape );
			}
			if( shapeList.Count == 0 ) {
				return 0;
			}

			TopoDS_Shape compound = ShapeTool.MakeCompound( shapeList );
			BoundingBox bbox = new BoundingBox( compound );

			gp_Pnt center = axes.Location();
			gp_Vec axis = new gp_Vec( axes.Direction() );
			double maxDistance = 0;
			double[] xValues = { bbox.Xmin, bbox.Xmax };
			double[] yValues = { bbox.Ymin, bbox.Ymax };
			double[] zValues = { bbox.Zmin, bbox.Zmax };

			foreach( double x in xValues ) {
				foreach( double y in yValues ) {
					foreach( double z in zValues ) {
						gp_Pnt corner = new gp_Pnt( x, y, z );
						double distance;
						if( isSphere ) {
							distance = corner.Distance( center );
						}
						else {
							gp_Vec offset = new gp_Vec( center, corner );
							distance = offset.Subtracted( axis.Scaled( offset.Dot( axis ) ) ).Magnitude();
						}
						maxDistance = Math.Max( maxDistance, distance );
					}
				}
			}

			return Math.Ceiling( maxDistance );
		}

		static bool TryCalculateRadialTraverse( IProcessPoint previousPathEnd, IProcessPoint currentPathStart, IProcessPoint liftUpPoint, IProcessPoint cutDownPoint,
			TraverseData traverseData, TraverseCalculationInfo calculationData, out List<gp_Pnt> points )
		{
			points = new List<gp_Pnt>();

			if( previousPathEnd == null || currentPathStart == null || traverseData == null || calculationData.SafeBoundaryAxes == null ) {
				return false;
			}

			bool isSphere = traverseData.TraverseMode == TraverseData.TraverseType.SafeSphere;
			double radius = isSphere ? traverseData.SafeSphereRadius : traverseData.SafeCylinderRadius;

			// chack radius valid 
			if( !IsPositiveFinite( radius ) || radius <= 1e-6 ) {
				return false;
			}

			gp_Ax3 axes = calculationData.SafeBoundaryAxes;

			// project the lift-up and cut-down points to get the boundary path endpoints
			if( !TryProjectToRadialPlane( liftUpPoint, axes, radius, isSphere, out gp_Pnt arcStart ) ||
				!TryProjectToRadialPlane( cutDownPoint, axes, radius, isSphere, out gp_Pnt arcEnd ) ) {
				return false;
			}

			double previousLongitudeAngle = 0;
			double currentLongitudeAngle = 0;
			if( calculationData.RotaryAxisIsMaster == true ) {
				previousLongitudeAngle = previousPathEnd.ModMaster_rad;
				currentLongitudeAngle = currentPathStart.ModMaster_rad;
			}
			else if( calculationData.RotaryAxisIsMaster == false ) {
				previousLongitudeAngle = previousPathEnd.ModSlave_rad;
				currentLongitudeAngle = currentPathStart.ModSlave_rad;
			}
			else {
				// machine doesnt have rotaryAxis, always use clockwise direction
			}

			// use angles to determine the rotate direction
			bool clockwise = previousLongitudeAngle >= currentLongitudeAngle;

			// Generate the sampled points and matching geometry edge for the boundary path
			if( !TryBuildBoundaryPath( arcStart, arcEnd, axes, radius, isSphere,
				OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION, OCCTool.GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
				clockwise, out points ) ) {
				return false;
			}

			return true;
		}

		static bool TryProjectToRadialPlane( IProcessPoint point, gp_Ax3 axes, double radius, bool isSphere, out gp_Pnt projectedPoint )
		{
			projectedPoint = null;

			gp_Vec axis = new gp_Vec( axes.Direction() );

			gp_Vec radial = new gp_Vec( axes.Location(), point.Point );

			// subtract axial vector if cylinder
			if( !isSphere ) {
				radial = radial.Subtracted( axis.Scaled( radial.Dot( axis ) ) );
			}

			double radialLength = radial.Magnitude();

			if( !IsPositiveFinite( radialLength ) ) {
				return false;
			}

			// cannot project, the point is on the axis or at the sphere center
			if( radialLength <= 1e-6 ) {

				//for spherer,  use Z axis as moving direction
				if( isSphere ) {
					radial = new gp_Vec( 0, 0, 1 );
				}

				//for cylinder, try X Y Z axis as moving direction
				else {
					gp_Vec[] directions = {
						new gp_Vec( 1, 0, 0 ),
						new gp_Vec( 0, 1, 0 ),
						new gp_Vec( 0, 0, 1 )
					};

					foreach( gp_Vec direction in directions ) {
						gp_Vec candidate = direction.Subtracted( axis.Scaled( direction.Dot( axis ) ) );
						if( candidate.Magnitude() > 1e-6 ) {
							radial = candidate;
							break;
						}
					}
				}
				radialLength = radial.Magnitude();
			}

			gp_Pnt center = isSphere ? axes.Location() : point.Point.Translated( radial.Reversed() );
			projectedPoint = center.Translated( radial.Scaled( radius / radialLength ) );
			return IsFinitePoint( projectedPoint );
		}

		static bool TryBuildBoundaryPath( gp_Pnt start, gp_Pnt end, gp_Ax3 axes, double radius, bool isSphere,
			double maxDeflection, double maxEdgeLength, bool isClockWise, out List<gp_Pnt> points )
		{
			points = new List<gp_Pnt>();
			if( start == null || end == null || axes == null ||
				!IsFinitePoint( start ) || !IsFinitePoint( end ) ||
				!IsPositiveFinite( radius ) || radius <= 1e-6 ||
				!IsPositiveFinite( maxDeflection ) || !IsPositiveFinite( maxEdgeLength ) ) {
				return false;
			}

			gp_Vec axis = new gp_Vec( axes.Direction() );
			gp_Vec xDirection = new gp_Vec( axes.XDirection() );
			gp_Vec yDirection = new gp_Vec( axes.YDirection() );
			gp_Vec startOffset = new gp_Vec( axes.Location(), start );
			gp_Vec endOffset = new gp_Vec( axes.Location(), end );

			// U is longitude; V is axial height for a cylinder or latitude for a sphere
			double startLongitude = Math.Atan2( startOffset.Dot( yDirection ), startOffset.Dot( xDirection ) );
			double endLongitude = Math.Atan2( endOffset.Dot( yDirection ), endOffset.Dot( xDirection ) );
			double longitudeChange = GetBoundaryAngleChange( startLongitude, endLongitude, isClockWise );
			double startPosition = startOffset.Dot( axis );
			double endPosition = endOffset.Dot( axis );

			if( isSphere ) {
				// Clamp the normalized height to avoid asin domain errors from rounding
				startPosition = Math.Asin( Math.Max( -1, Math.Min( 1, startPosition / radius ) ) );
				endPosition = Math.Asin( Math.Max( -1, Math.Min( 1, endPosition / radius ) ) );
			}

			double positionChange = endPosition - startPosition;

			// Preserve the existing in-place path behavior without constructing a zero-direction 2D line
			if( longitudeChange == 0 && positionChange == 0 ) {
				points.Add( start );
				points.Add( end );
				return true;
			}

			OCC.Geom.Geom_Surface surface;
			if( isSphere ) {
				surface = new OCC.Geom.Geom_SphericalSurface( axes, radius );
			}
			else {
				surface = new OCC.Geom.Geom_CylindricalSurface( axes, radius );
			}

			// The 2D line uses a unit direction, so its parameter range must equal the UV distance
			double lastParameter = Math.Sqrt( longitudeChange * longitudeChange + positionChange * positionChange );
			if( !IsPositiveFinite( lastParameter ) ) {
				return false;
			}

			// Linear UV interpolation preserves the existing path, not necessarily a spherical geodesic
			OCC.Geom2d.Geom2d_Line parameterCurve = new OCC.Geom2d.Geom2d_Line(
				new gp_Pnt2d( startLongitude, startPosition ),
				new gp_Dir2d( longitudeChange, positionChange ) );

			// Map the bounded UV line onto the surface to obtain the boundary edge
			var maker = new OCC.BRepBuilderAPI.BRepBuilderAPI_MakeEdge( parameterCurve, surface, 0, lastParameter );
			if( !maker.IsDone() ) {
				return false;
			}

			// Reuse the shared discretization policy to evaluate points on the surface curve
			TopoDS_Edge edge = maker.Edge();
			var curveAdaptor = new OCC.BRepAdaptor.BRepAdaptor_Curve( edge );
			List<double> parameters = GeometryTool.DiscretizeEdgeParameters( curveAdaptor, edge, maxDeflection, maxEdgeLength );

			if( parameters == null || parameters.Count < 2 ) {
				return false;
			}

			List<gp_Pnt> sampledPoints = new List<gp_Pnt>( parameters.Count );
			foreach( double parameter in parameters ) {
				if( double.IsNaN( parameter ) || double.IsInfinity( parameter ) ) {
					return false;
				}

				gp_Pnt point = curveAdaptor.Value( parameter );
				if( !IsFinitePoint( point ) ) {
					return false;
				}
				sampledPoints.Add( point );
			}

			// Align the sampled order with the requested start and end before restoring exact endpoints
			int lastIndex = sampledPoints.Count - 1;
			double forwardDistance = sampledPoints[ 0 ].Distance( start ) + sampledPoints[ lastIndex ].Distance( end );
			double reverseDistance = sampledPoints[ 0 ].Distance( end ) + sampledPoints[ lastIndex ].Distance( start );
			if( reverseDistance < forwardDistance ) {
				sampledPoints.Reverse();
			}

			// Preserve the exact projected endpoints
			sampledPoints[ 0 ] = start;
			sampledPoints[ sampledPoints.Count - 1 ] = end;
			points = sampledPoints;
			return true;
		}

		// calculate traverse angle by rotate direction
		static double GetBoundaryAngleChange( double startAngle, double endAngle, bool isClockWise )
		{
			double signedAngle = Math.Atan2( Math.Sin( endAngle - startAngle ), Math.Cos( endAngle - startAngle ) );
			if( Math.Abs( signedAngle ) < 1e-6 ) {
				return 0;
			}

			if( isClockWise == false ) {
				return signedAngle > 0 ? signedAngle : signedAngle + TWO_PI;
			}
			return signedAngle < 0 ? signedAngle : signedAngle - TWO_PI;
		}

		#endregion

		#region Preview Bounds

		public static bool TryGetPlanePreviewBounds( IReadOnlyList<TraversePathResult> results,
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

			// Add padding around the XY bounds so the preview does not sit against the edge
			double margin = Math.Max( PREVIEW_MARGIN, Math.Max( maxX - minX, maxY - minY ) * PREVIEW_MARGIN_RATIO );
			minX -= margin;
			maxX += margin;
			minY -= margin;
			maxY += margin;
			return true;
		}

		public static bool TryGetRadialPreviewBounds( IReadOnlyList<TraversePathResult> results, gp_Ax3 axes,
			double radius, bool isSphere, out double minU, out double maxU, out double minV, out double maxV )
		{
			minU = 0;
			maxU = 0;
			minV = double.MaxValue;
			maxV = double.MinValue;

			if( results == null || axes == null ) {
				return false;
			}
			if( double.IsNaN( radius ) || double.IsInfinity( radius ) || radius <= 1e-6 ) {
				return false;
			}

			var intervals = new List<(double start, double end)>();
			gp_Vec xDirection = new gp_Vec( axes.XDirection() );
			gp_Vec yDirection = new gp_Vec( axes.YDirection() );
			gp_Vec axis = new gp_Vec( axes.Direction() );
			double maxAngularStep = 0;

			// Process each safe-boundary path and inspect only its boundary segment
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

				// Unwrap angles along the path so crossing 0 and 2π does not split the bounds
				for( int i = 0; i < points.Count; i++ ) {
					gp_Vec offset = new gp_Vec( axes.Location(), points[ i ] );
					double angle = Math.Atan2( offset.Dot( yDirection ), offset.Dot( xDirection ) );
					if( previousOffset == null ) {
						unwrappedAngle = angle;
					}
					else {
						double delta = Math.Atan2( Math.Sin( angle - previousAngle ), Math.Cos( angle - previousAngle ) );

						// Unwrap cylinder paths clockwise, including segments that span exactly half a turn
						if( !isSphere && delta > 1e-6 ) {
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

			// Cannot calculate preview bounds when there are no valid boundary points
			if( intervals.Count == 0 ) {
				return false;
			}

			// Combine the angular intervals and find the smallest range that covers the paths
			GetCircularBounds( intervals, out minU, out maxU );
			bool includesPole = false;

			// Use latitude for the sphere's V bounds and account for angles between sampled points
			if( isSphere ) {
				minV = Math.Max( -Math.PI / 2, minV - maxAngularStep );
				maxV = Math.Min( Math.PI / 2, maxV + maxAngularStep );
				includesPole = minV <= -Math.PI / 2 || maxV >= Math.PI / 2;
				double margin = Math.Max( PREVIEW_MARGIN / radius, ( maxV - minV ) * PREVIEW_MARGIN_RATIO );
				minV = Math.Max( -Math.PI / 2, minV - margin );
				maxV = Math.Min( Math.PI / 2, maxV + margin );
			}

			// Use axial position for the cylinder's V bounds and add preview padding
			else {
				minV = Math.Min( 0, minV );
				maxV = Math.Max( 0, maxV );
				double margin = Math.Max( PREVIEW_MARGIN, ( maxV - minV ) * PREVIEW_MARGIN_RATIO );
				minV -= margin;
				maxV += margin;
			}
			double angularMargin = Math.Max( PREVIEW_MARGIN / radius, ( maxU - minU ) * PREVIEW_MARGIN_RATIO );

			// Show the full angular range when the path reaches a pole or the padded range covers a full turn
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

		// Normalize an angular range to 0 through 2π and split it if it crosses the seam
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

		// Find the largest gap on the circle so the remaining range is the smallest one covering all intervals
		static void GetCircularBounds( List<(double start, double end)> intervals, out double min, out double max )
		{
			intervals.Sort( ( left, right ) => left.start.CompareTo( right.start ) );
			double end = intervals[ 0 ].end;
			double largestGap = 0;
			min = intervals[ 0 ].start;

			// Compare gaps between adjacent intervals to find the largest one to skip
			for( int index = 1; index < intervals.Count; index++ ) {
				double gap = intervals[ index ].start - end;
				if( gap > largestGap ) {
					largestGap = gap;
					min = intervals[ index ].start;
				}
				end = Math.Max( end, intervals[ index ].end );
			}

			// The seam between the last and first intervals may also be the largest gap
			double seamGap = intervals[ 0 ].start + TWO_PI - end;
			if( seamGap > largestGap ) {
				largestGap = seamGap;
				min = intervals[ 0 ].start;
			}

			// Start after the largest gap and cover all remaining angular intervals
			max = min + TWO_PI - largestGap;
		}

		#endregion

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
