using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.BRepAdaptor;
using OCC.BRepBuilderAPI;
using OCC.Geom;
using OCC.gp;
using OCC.TColgp;
using OCC.TColStd;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

namespace MyCAM.Helper
{
	public static class FlyCutHelper
	{
		internal const double NUMERICAL_TOLERANCE = 1e-6;
		internal const double LENGTH_TOLERANCE_MM = 1e-6;
		internal const double POSITION_TOLERANCE_MM = 1e-3;
		internal const double NUMERICAL_TOLERANCE_SQUARED = NUMERICAL_TOLERANCE * NUMERICAL_TOLERANCE;
		internal const double LENGTH_TOLERANCE_SQUARED = LENGTH_TOLERANCE_MM * LENGTH_TOLERANCE_MM;
		internal const double POSITION_TOLERANCE_SQUARED = POSITION_TOLERANCE_MM * POSITION_TOLERANCE_MM;

		public static bool TryGetClockwise( IReadOnlyList<CAMPoint> points, gp_Pnt origin, gp_Dir axisDir, out bool clockwise )
		{
			clockwise = false;

			// A closed contour needs at least three vertices plus the repeated first point
			if( points == null || points.Count < 4 || origin == null || axisDir == null
				|| points.Any( point => point == null || point.Point == null )
				|| points[ 0 ].Point.SquareDistance( points.Last().Point ) > LENGTH_TOLERANCE_SQUARED ) {
				return false;
			}

			// Unroll the contour into continuous angular and axial coordinates
			gp_Vec axis = new gp_Vec( axisDir );
			gp_Vec previousRadial = null;
			double previousAngle = 0;
			double previousHeight = 0;
			double area = 0;
			foreach( CAMPoint point in points ) {
				gp_Vec offset = new gp_Vec( origin, point.Point );
				gp_Vec radial = offset.Subtracted( axis.Multiplied( offset.Dot( axis ) ) );
				double radius = radial.Magnitude();
				double height = new gp_Vec( points[ 0 ].Point, point.Point ).Dot( axis );
				if( double.IsNaN( radius ) || double.IsInfinity( radius ) || radius <= LENGTH_TOLERANCE_MM
					|| double.IsNaN( height ) || double.IsInfinity( height ) ) {
					return false;
				}
				radial = radial.Multiplied( 1 / radius );
				if( previousRadial != null ) {
					double delta = Math.Atan2(
						previousRadial.Crossed( radial ).Dot( axis ), previousRadial.Dot( radial ) );
					if( Math.Abs( delta ) >= ( Math.PI - NUMERICAL_TOLERANCE ) ) {
						return false;
					}
					double angle = previousAngle + delta;
					area += ( previousAngle * height ) - ( angle * previousHeight );
					previousAngle = angle;
				}
				previousHeight = height;
				previousRadial = radial;
			}

			// The signed double area uses angular and axial units, not mm squared
			if( Math.Abs( previousAngle ) > NUMERICAL_TOLERANCE || double.IsNaN( area )
				|| double.IsInfinity( area ) || Math.Abs( area ) <= ( NUMERICAL_TOLERANCE * LENGTH_TOLERANCE_MM ) ) {
				return false;
			}
			clockwise = area < 0;
			return true;
		}

		public static bool TryGetPlaneClockwise( IReadOnlyList<CAMPoint> points, gp_Ax3 planeCoordinate, out bool clockwise )
		{
			clockwise = false;

			// A closed contour needs at least three vertices plus the repeated first point
			if( points == null || points.Count < 4 || planeCoordinate == null
				|| points.Any( point => point == null || point.Point == null )
				|| points[ 0 ].Point.SquareDistance( points.Last().Point ) > POSITION_TOLERANCE_SQUARED ) {
				return false;
			}

			// Validate coplanarity and accumulate signed double area along the plane normal
			gp_Vec normal = new gp_Vec( planeCoordinate.Direction() );
			gp_Vec previousOffset = new gp_Vec( 0, 0, 0 );
			double area = 0;
			foreach( CAMPoint point in points ) {
				double distance = new gp_Vec( planeCoordinate.Location(), point.Point ).Dot( normal );
				if( double.IsNaN( distance ) || double.IsInfinity( distance ) || Math.Abs( distance ) > POSITION_TOLERANCE_MM ) {
					return false;
				}
				gp_Vec offset = new gp_Vec( points[ 0 ].Point, point.Point );
				area += previousOffset.Crossed( offset ).Dot( normal );
				previousOffset = offset;
			}
			if( double.IsNaN( area ) || double.IsInfinity( area ) || Math.Abs( area ) <= LENGTH_TOLERANCE_SQUARED ) {
				return false;
			}
			clockwise = area < 0;
			return true;
		}

		static bool TryBuildBSplineCurve( gp_Pnt start, gp_Dir departureTangent, gp_Pnt end, gp_Dir arrivalTangent,
			gp_Vec departureCurvature, gp_Vec arrivalCurvature, out TopoDS_Edge edge )
		{
			const double handleLengthRatio = 0.5;
			const double baseHandleScale = 0.6;

			// A degree-five curve uses the curvature coefficient 5 / 4
			const double curvatureControlScale = 1.25;

			edge = null;
			if( start == null || end == null
				|| departureTangent == null || arrivalTangent == null ) {
				return false;
			}
			double handleLength = start.Distance( end ) * handleLengthRatio;
			if( double.IsNaN( handleLength ) || double.IsInfinity( handleLength ) || handleLength <= LENGTH_TOLERANCE_MM ) {
				return false;
			}
			double distanceSquared = start.SquareDistance( end );
			if( double.IsNaN( distanceSquared ) || double.IsInfinity( distanceSquared )
				|| distanceSquared <= LENGTH_TOLERANCE_SQUARED ) {
				return false;
			}

			// Normalize the endpoint distance and project curvature perpendicular to each tangent
			double distance = Math.Sqrt( distanceSquared );
			gp_Vec chordDirection = new gp_Vec( start, end ).Multiplied( 1 / distance );
			gp_Vec departureVector = new gp_Vec( departureTangent );
			gp_Vec arrivalVector = new gp_Vec( arrivalTangent );
			gp_Vec scaledDepartureCurvature = ( departureCurvature ?? new gp_Vec( 0, 0, 0 ) ).Multiplied( distance );
			gp_Vec scaledArrivalCurvature = ( arrivalCurvature ?? new gp_Vec( 0, 0, 0 ) ).Multiplied( distance );
			scaledDepartureCurvature = scaledDepartureCurvature.Subtracted( departureVector.Multiplied( scaledDepartureCurvature.Dot( departureVector ) ) );
			scaledArrivalCurvature = scaledArrivalCurvature.Subtracted( arrivalVector.Multiplied( scaledArrivalCurvature.Dot( arrivalVector ) ) );
			double departureCurvatureMagnitude = scaledDepartureCurvature.Magnitude();
			double arrivalCurvatureMagnitude = scaledArrivalCurvature.Magnitude();
			if( double.IsNaN( departureCurvatureMagnitude ) || double.IsInfinity( departureCurvatureMagnitude )
				|| double.IsNaN( arrivalCurvatureMagnitude ) || double.IsInfinity( arrivalCurvatureMagnitude ) ) {
				return false;
			}

			// Search the same handle-length pairs in normalized coordinates
			double departureBaseLength = handleLength * baseHandleScale / distance / Math.Sqrt( Math.Max( 1, departureCurvatureMagnitude ) );
			double arrivalBaseLength = handleLength * baseHandleScale / distance / Math.Sqrt( Math.Max( 1, arrivalCurvatureMagnitude ) );
			double[] handleFactors = { 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4 };
			gp_Pnt normalizedStart = new gp_Pnt( 0, 0, 0 );
			gp_Pnt normalizedEnd = new gp_Pnt( chordDirection.XYZ() );
			gp_Pnt[] bestControlPoints = null;
			double bestCurvature = double.PositiveInfinity;
			double bestLength = double.PositiveInfinity;
			foreach( double departureFactor in handleFactors ) {
				foreach( double arrivalFactor in handleFactors ) {
					double departureLength = departureBaseLength * departureFactor;
					double arrivalLength = arrivalBaseLength * arrivalFactor;
					gp_Vec departureHandle = departureVector.Multiplied( departureLength );
					gp_Vec arrivalHandle = arrivalVector.Multiplied( arrivalLength );
					gp_Pnt[] controlPoints = {
						normalizedStart,
						normalizedStart.Translated( departureHandle ),
						normalizedStart.Translated( departureHandle.Multiplied( 2 )
							.Added( scaledDepartureCurvature.Multiplied( curvatureControlScale * departureLength * departureLength ) ) ),
						normalizedEnd.Translated( arrivalHandle.Multiplied( -2 )
							.Added( scaledArrivalCurvature.Multiplied( curvatureControlScale * arrivalLength * arrivalLength ) ) ),
						normalizedEnd.Translated( arrivalHandle.Reversed() ),
						normalizedEnd
					};
					if( !TryScoreFlightCurve( BuildFlightSpline( controlPoints ), out double maximumCurvature, out double curveLength ) ) {
						continue;
					}

					// Prefer lower peak curvature, then shorter length within the score tolerance
					if( maximumCurvature < ( bestCurvature - NUMERICAL_TOLERANCE )
						|| ( Math.Abs( maximumCurvature - bestCurvature ) <= NUMERICAL_TOLERANCE && curveLength < bestLength ) ) {
						bestControlPoints = controlPoints;
						bestCurvature = maximumCurvature;
						bestLength = curveLength;
					}
				}
			}
			if( bestControlPoints == null ) {
				return false;
			}

			// Restore model coordinates and retain the exact supplied endpoints
			gp_Pnt[] worldControlPoints = bestControlPoints
				.Select( point => start.Translated( new gp_Vec( point.X(), point.Y(), point.Z() ).Multiplied( distance ) ) ).ToArray();
			worldControlPoints[ 0 ] = start;
			worldControlPoints[ worldControlPoints.Length - 1 ] = end;
			BRepBuilderAPI_MakeEdge edgeBuilder = new BRepBuilderAPI_MakeEdge( BuildFlightSpline( worldControlPoints ) );
			TopoDS_Edge result = edgeBuilder.Edge();
			if( result == null || result.IsNull() ) {
				return false;
			}
			edge = result;
			return true;
		}

		static Geom_BSplineCurve BuildFlightSpline( gp_Pnt[] controlPoints )
		{
			int degree = controlPoints.Length - 1;
			TColgp_Array1OfPnt poles = new TColgp_Array1OfPnt( 1, controlPoints.Length );
			TColStd_Array1OfReal weights = new TColStd_Array1OfReal( 1, controlPoints.Length );

			// Clamped knots and unit weights represent the control polygon as one Bezier span
			for( int i = 1; i <= controlPoints.Length; ++i ) {
				poles.SetValue( i, controlPoints[ i - 1 ] );
				weights.SetValue( i, 1.0 );
			}
			TColStd_Array1OfReal knots = new TColStd_Array1OfReal( 1, 2 );
			knots.SetValue( 1, 0.0 );
			knots.SetValue( 2, 1.0 );
			TColStd_Array1OfInteger multiplicities = new TColStd_Array1OfInteger( 1, 2 );
			multiplicities.SetValue( 1, degree + 1 );
			multiplicities.SetValue( 2, degree + 1 );
			return new Geom_BSplineCurve( poles, weights, knots, multiplicities, degree, false );
		}

		static bool TryScoreFlightCurve( Geom_BSplineCurve curve, out double maximumCurvature, out double curveLength )
		{
			const int sampleCount = 64;
			const double maxNormalizedLength = 4;

			maximumCurvature = 0;
			curveLength = 0;
			gp_Pnt previousPoint = null;
			gp_Vec previousDerivative = null;

			// Sample normalized geometry, rejecting degenerate or sharply reversing derivatives
			for( int i = 0; i <= sampleCount; ++i ) {
				gp_Pnt point = new gp_Pnt();
				gp_Vec firstDerivative = new gp_Vec();
				gp_Vec secondDerivative = new gp_Vec();
				curve.D2( (double)i / sampleCount, ref point, ref firstDerivative, ref secondDerivative );
				double speedSquared = firstDerivative.SquareMagnitude();
				if( double.IsNaN( speedSquared ) || double.IsInfinity( speedSquared ) || speedSquared <= NUMERICAL_TOLERANCE_SQUARED
					|| ( previousDerivative != null && previousDerivative.Dot( firstDerivative ) <= 0 ) ) {
					return false;
				}
				double curvature = firstDerivative.Crossed( secondDerivative ).Magnitude() / ( speedSquared * Math.Sqrt( speedSquared ) );
				if( double.IsNaN( curvature ) || double.IsInfinity( curvature ) ) {
					return false;
				}
				maximumCurvature = Math.Max( maximumCurvature, curvature );
				if( previousPoint != null ) {
					curveLength += previousPoint.Distance( point );
				}

				// This length limit is relative to the endpoint distance, not measured in mm
				if( double.IsNaN( curveLength ) || double.IsInfinity( curveLength ) || curveLength > maxNormalizedLength ) {
					return false;
				}
				previousPoint = point;
				previousDerivative = firstDerivative;
			}
			return true;
		}

		internal static gp_Vec EstimateEndpointCurvature( IReadOnlyList<CAMPoint> points, bool isEnd, gp_Dir tangent )
		{
			const double tangentRadiusToleranceRatio = 0.1;

			// Preserve zero curvature as the fallback when the estimate is unavailable or unreliable
			gp_Vec zeroCurvature = new gp_Vec( 0, 0, 0 );

			// Estimate curvature from the endpoint and two distinct inward samples
			if( points == null || points.Count < 3 || tangent == null ) {
				return zeroCurvature;
			}
			int endpointIndex = isEnd ? points.Count - 1 : 0;
			gp_Pnt endpoint = points[ endpointIndex ]?.Point;
			if( endpoint == null ) {
				return zeroCurvature;
			}

			// Find two distinct samples inward from the endpoint
			gp_Vec firstOffset = null;
			gp_Vec secondOffset = null;
			int step = isEnd ? -1 : 1;
			for( int i = endpointIndex + step; i >= 0 && i < points.Count; i += step ) {
				gp_Pnt point = points[ i ]?.Point;
				if( point == null ) {
					return zeroCurvature;
				}
				gp_Vec offset = new gp_Vec( endpoint, point );
				double offsetSquared = offset.SquareMagnitude();
				if( double.IsNaN( offsetSquared ) || double.IsInfinity( offsetSquared ) ) {
					return zeroCurvature;
				}
				if( offsetSquared <= LENGTH_TOLERANCE_SQUARED ) {
					continue;
				}
				if( firstOffset == null ) {
					firstOffset = offset;
				}
				else if( offset.Subtracted( firstOffset ).SquareMagnitude() > LENGTH_TOLERANCE_SQUARED ) {
					secondOffset = offset;
					break;
				}
			}
			if( firstOffset == null || secondOffset == null ) {
				return zeroCurvature;
			}

			// Fit the three-point circle and reject nearly collinear samples
			gp_Vec normal = firstOffset.Crossed( secondOffset );
			double normalSquared = normal.SquareMagnitude();
			if( normalSquared <= ( NUMERICAL_TOLERANCE_SQUARED * firstOffset.SquareMagnitude() * secondOffset.SquareMagnitude() ) ) {
				return zeroCurvature;
			}
			gp_Vec centerOffset = secondOffset.Crossed( normal ).Multiplied( firstOffset.SquareMagnitude() )
				.Added( normal.Crossed( firstOffset ).Multiplied( secondOffset.SquareMagnitude() ) ).Multiplied( 0.5 / normalSquared );

			// Accept only a radius nearly perpendicular to the supplied tangent
			double radiusSquared = centerOffset.SquareMagnitude();
			gp_Vec tangentVector = new gp_Vec( tangent );
			double tangentComponent = centerOffset.Dot( tangentVector );
			if( double.IsNaN( radiusSquared ) || double.IsInfinity( radiusSquared ) || radiusSquared <= LENGTH_TOLERANCE_SQUARED
				|| Math.Abs( tangentComponent ) > ( tangentRadiusToleranceRatio * Math.Sqrt( radiusSquared ) ) ) {
				return zeroCurvature;
			}
			return centerOffset.Subtracted( tangentVector.Multiplied( tangentComponent ) ).Multiplied( 1 / radiusSquared );
		}

		public static bool TryCalculateFlightCAM( CAMPoint start, CAMPoint end, gp_Dir departureTangent, gp_Dir arrivalTangent, out TopoDS_Edge edge, out List<CAMPoint> points )
		{
			return TryCalculateFlightCAM( start, end, departureTangent, arrivalTangent, null, null, out edge, out points );
		}

		public static bool TryCalculateFlightCAM( CAMPoint start, CAMPoint end, gp_Dir departureTangent, gp_Dir arrivalTangent,
			gp_Vec departureCurvature, gp_Vec arrivalCurvature, out TopoDS_Edge edge, out List<CAMPoint> points )
		{
			edge = null;
			points = null;
			if( !TryBuildFlightCurve( start, end, departureTangent, arrivalTangent, departureCurvature, arrivalCurvature, out TopoDS_Edge flightEdge ) ) {
				return false;
			}

			if( !TryDiscretizeFlight( flightEdge, start, end, out List<CAMPoint> flightPoints ) ) {
				return false;
			}

			// Publish both outputs only after curve construction and discretization succeed
			edge = flightEdge;
			points = flightPoints;
			return true;
		}

		static bool TryDiscretizeFlight( TopoDS_Edge edge, CAMPoint start, CAMPoint end, out List<CAMPoint> points )
		{
			points = null;
			if( edge == null || edge.IsNull() ) {
				return false;
			}
			if( start?.Point == null || end?.Point == null ) {
				return false;
			}

			double startMaster_rad = start.ModMaster_rad;
			double startSlave_rad = start.ModSlave_rad;
			double endMaster_rad = end.ModMaster_rad;
			double endSlave_rad = end.ModSlave_rad;

			if( new[] { startMaster_rad, startSlave_rad, endMaster_rad, endSlave_rad }
				.Any( angle => double.IsNaN( angle ) || double.IsInfinity( angle ) ) ) {
				return false;
			}

			// Discretize with the existing geometric tolerances and verify endpoint correspondence
			BRepAdaptor_Curve curve = new BRepAdaptor_Curve( edge );
			List<double> parameters = GeometryTool.DiscretizeEdgeParameters( curve, edge,
				GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION, GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
				GeometryTool.DEFAULT_DISCRETIZE_METHOD );

			// At least the start and end parameters are needed to form a flight segment
			if( parameters == null || parameters.Count < 2 ) {
				return false;
			}
			List<gp_Pnt> flightPoints = parameters.Select( parameter => curve.Value( parameter ) ).ToList();

			if( flightPoints[ 0 ].SquareDistance( start.Point ) > POSITION_TOLERANCE_SQUARED
				|| flightPoints.Last().SquareDistance( end.Point ) > POSITION_TOLERANCE_SQUARED ) {
				return false;
			}

			// Use accumulated polyline length, rather than curve parameter, for pose interpolation
			double[] accumulatedLengths = new double[ flightPoints.Count ];
			for( int i = 1; i < flightPoints.Count; ++i ) {
				double segmentLength = flightPoints[ i - 1 ].Distance( flightPoints[ i ] );
				if( double.IsNaN( segmentLength ) || double.IsInfinity( segmentLength )
					|| segmentLength <= LENGTH_TOLERANCE_MM ) {
					return false;
				}
				accumulatedLengths[ i ] = accumulatedLengths[ i - 1 ] + segmentLength;
			}
			double totalLength = accumulatedLengths[ flightPoints.Count - 1 ];
			if( double.IsNaN( totalLength ) || double.IsInfinity( totalLength )
				|| totalLength <= LENGTH_TOLERANCE_MM ) {
				return false;
			}

			// Interpolate rotary-axis angles, has not calculate toolvec
			List<CAMPoint> result = new List<CAMPoint>();
			for( int i = 0; i < flightPoints.Count; ++i ) {
				double lengthRatio = accumulatedLengths[ i ] / totalLength;
				double master_rad = startMaster_rad + ( lengthRatio * ( endMaster_rad - startMaster_rad ) );
				double slave_rad = startSlave_rad + ( lengthRatio * ( endSlave_rad - startSlave_rad ) );

				gp_Dir toolVec = new gp_Dir();
				gp_Pnt point = new gp_Pnt();
				gp_Vec tangent = new gp_Vec();
				curve.D1( parameters[ i ], ref point, ref tangent );
				double tangentMagnitudeSquared = tangent.SquareMagnitude();
				if( double.IsNaN( tangentMagnitudeSquared ) || double.IsInfinity( tangentMagnitudeSquared ) || tangentMagnitudeSquared <= LENGTH_TOLERANCE_SQUARED ) {
					return false;
				}
				if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					tangent.Reverse();
				}
				CADPoint cadPoint = new CADPoint( flightPoints[ i ], toolVec, toolVec, new gp_Dir( tangent ) );
				result.Add( new CAMPoint( cadPoint, toolVec )
				{
					InitPathIndex = i,
					InitMaster_rad = master_rad,
					InitSlave_rad = slave_rad,
					ModMaster_rad = master_rad,
					ModSlave_rad = slave_rad
				} );
			}

			// Preserve endpoint data and laser-off/on markers across the flight
			result[ 0 ] = start.Clone();
			result[ result.Count - 1 ] = end.Clone();
			result[ 0 ].IsMicroJointStart = true;
			result[ 0 ].IsMicroJointEnd = false;
			result[ result.Count - 1 ].IsMicroJointStart = false;
			result[ result.Count - 1 ].IsMicroJointEnd = true;
			points = result;
			return true;
		}

		static bool TryBuildFlightCurve( CAMPoint start, CAMPoint end, gp_Dir departureTangent, gp_Dir arrivalTangent,
			gp_Vec departureCurvature, gp_Vec arrivalCurvature, out TopoDS_Edge edge )
		{
			edge = null;
			if( start?.Point == null || end?.Point == null ) {
				return false;
			}

			return TryBuildBSplineCurve( start.Point, departureTangent, end.Point, arrivalTangent,
				departureCurvature, arrivalCurvature, out edge );
		}
	}
}
