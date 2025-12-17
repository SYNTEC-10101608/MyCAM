using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Helper
{
	internal static class StdPatternStartPointListFactory
	{
		public static List<CADPoint> GetStartPointList( gp_Ax3 refCoord, IStdPatternGeomData geomData )
		{
			if( geomData == null ) {
				throw new ArgumentNullException( nameof( geomData ) );
			}

			List<CADPoint> startPointList;
			switch( geomData.PathType ) {
				case PathType.Circle:
					startPointList = GetStartPointListTyped<CircleGeomData>( refCoord, geomData, GetCircleStartPointList );
					break;
				case PathType.Rectangle:
					startPointList = GetStartPointListTyped<RectangleGeomData>( refCoord, geomData, GetRectangleStartPointList );
					break;
				case PathType.Runway:
					startPointList = GetStartPointListTyped<RunwayGeomData>( refCoord, geomData, GetRunwayStartPointList );
					break;
				case PathType.Triangle:
				case PathType.Square:
				case PathType.Pentagon:
				case PathType.Hexagon:
					startPointList = GetStartPointListTyped<PolygonGeomData>( refCoord, geomData, GetPolygonStartPointList );
					break;
				default:
					throw new ArgumentException( $"Unsupported geometry type: {geomData.PathType}" );
			}
			return startPointList;
		}

		#region Type-Safe Conversion Helper

		static List<CADPoint> GetStartPointListTyped<TGeomData>( gp_Ax3 refCoord, IStdPatternGeomData geomData, Func<gp_Ax3, TGeomData, List<CADPoint>> generator ) where TGeomData : class, IStdPatternGeomData
		{
			TGeomData typedData = geomData as TGeomData;
			if( typedData == null ) {
				throw new InvalidOperationException(
					$"Geometry data type mismatch: expected {typeof( TGeomData ).Name} for PathType.{geomData.PathType}, but got {geomData.GetType().Name}"
				);
			}
			return generator( refCoord, typedData );
		}

		#endregion

		#region Private Methods for Each Geometry Type

		static List<CADPoint> GetCircleStartPointList( gp_Ax3 refCoord, CircleGeomData circleData )
		{
			double radius = circleData.Diameter / 2.0;

			// calculate intersection point along X-axis direction
			gp_Dir xDirection = refCoord.XDirection();
			gp_Vec radiusVector = new gp_Vec( xDirection.XYZ() * radius );
			gp_Pnt intersectionPoint = refCoord.Location().Translated( radiusVector );

			// define vectors
			gp_Dir normal = refCoord.Direction();
			gp_Dir radialIn = xDirection.Reversed();
			gp_Dir tangent = refCoord.YDirection().Reversed();

			CADPoint cadPoint = new CADPoint( intersectionPoint, normal, radialIn, tangent );

			return new List<CADPoint> { cadPoint };
		}

		static List<CADPoint> GetRectangleStartPointList( gp_Ax3 refCoord, RectangleGeomData rectData )
		{
			double halfLength = rectData.Length / 2.0;
			double halfWidth = rectData.Width / 2.0;

			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			// point on positive X axis (long side)
			gp_Pnt local_Pnt_L_Pos = new gp_Pnt( halfLength, 0, 0 );
			gp_Dir local_N1_L_Pos = local_Z_pos;
			gp_Dir local_N2_L_Pos = local_X_neg;
			gp_Dir local_Tan_L_Pos = local_Y_neg;

			// point on negative Y axis (short side)
			gp_Pnt local_Pnt_S_Neg = new gp_Pnt( 0, -halfWidth, 0 );
			gp_Dir local_N1_S_Neg = local_Z_pos;
			gp_Dir local_N2_S_Neg = local_Y_pos;
			gp_Dir local_Tan_S_Neg = local_X_neg;

			gp_Trsf transformation = CreateCoordTransformation( refCoord );

			CADPoint cad_L_Pos = TransformCADPoint(
				local_Pnt_L_Pos, local_N1_L_Pos, local_N2_L_Pos, local_Tan_L_Pos, transformation
			);

			CADPoint cad_S_Neg = TransformCADPoint(
				local_Pnt_S_Neg, local_N1_S_Neg, local_N2_S_Neg, local_Tan_S_Neg, transformation
			);

			return new List<CADPoint> { cad_L_Pos, cad_S_Neg };
		}

		static List<CADPoint> GetRunwayStartPointList( gp_Ax3 refCoord, RunwayGeomData runwayData )
		{
			double length = runwayData.Length;
			double width = runwayData.Width;
			double radius = width / 2.0;
			double straightLength = length - width;
			double halfStraight = straightLength / 2.0;

			gp_Dir local_X_pos = gp.DX();
			gp_Dir local_X_neg = gp.DX().Reversed();
			gp_Dir local_Y_pos = gp.DY();
			gp_Dir local_Y_neg = gp.DY().Reversed();
			gp_Dir local_Z_pos = gp.DZ();

			// right arc point on positive X-axis
			gp_Pnt local_Pnt_RightArc = new gp_Pnt( halfStraight + radius, 0, 0 );
			gp_Dir local_N1_RightArc = local_Z_pos;
			gp_Dir local_N2_RightArc = local_X_neg;
			gp_Dir local_Tan_RightArc = local_Y_neg;

			// bottom edge midpoint on negative Y-axis
			gp_Pnt local_Pnt_BottomEdge = new gp_Pnt( 0, -radius, 0 );
			gp_Dir local_N1_BottomEdge = local_Z_pos;
			gp_Dir local_N2_BottomEdge = local_Y_pos;
			gp_Dir local_Tan_BottomEdge = local_X_neg;

			gp_Trsf transformation = CreateCoordTransformation( refCoord );

			CADPoint cad_RightArc = TransformCADPoint(
				local_Pnt_RightArc, local_N1_RightArc, local_N2_RightArc, local_Tan_RightArc, transformation
			);

			CADPoint cad_BottomEdge = TransformCADPoint(
				local_Pnt_BottomEdge, local_N1_BottomEdge, local_N2_BottomEdge, local_Tan_BottomEdge, transformation
			);

			return new List<CADPoint> { cad_RightArc, cad_BottomEdge };
		}

		static List<CADPoint> GetPolygonStartPointList( gp_Ax3 refCoord, PolygonGeomData polygonData )
		{
			int sides = polygonData.Sides;
			double sideLength = polygonData.SideLength;

			// calculate circumradius R = sideLength / (2 * sin(£k/n))
			double angleStep = 2.0 * Math.PI / sides;
			double radius = sideLength / ( 2.0 * Math.Sin( Math.PI / sides ) );

			// determine initial angle offset based on number of sides
			double angleOffset = GetPolygonAngleOffset( sides );
			gp_Dir local_Z_pos = gp.DZ();

			// create coordinate system transformation
			gp_Trsf coordTransformation = CreateCoordTransformation( refCoord );

			// find the edge that intersects negative Y-axis
			int startEdgeIndex = FindNegativeYAxisIntersectingEdge( sides, angleOffset, angleStep );

			List<CADPoint> resultList = new List<CADPoint>();

			// generate edge midpoints in clockwise order
			for( int i = 0; i < sides; i++ ) {
				// for clockwise traversal, decrement the edge index
				int currentEdgeIndex = ( startEdgeIndex - i + sides ) % sides;

				// calculate angles for two vertices of current edge
				double vertex1Angle = angleOffset + currentEdgeIndex * angleStep;
				double vertex2Angle = angleOffset + ( ( currentEdgeIndex + 1 ) % sides ) * angleStep;

				// calculate vertex positions on circumcircle
				gp_Pnt vertex1 = new gp_Pnt(
					radius * Math.Cos( vertex1Angle ),
					radius * Math.Sin( vertex1Angle ),
					0
				);

				gp_Pnt vertex2 = new gp_Pnt(
					radius * Math.Cos( vertex2Angle ),
					radius * Math.Sin( vertex2Angle ),
					0
				);

				// calculate edge midpoint
				gp_Pnt edgeMidpoint = new gp_Pnt(
					( vertex1.X() + vertex2.X() ) / 2.0,
					( vertex1.Y() + vertex2.Y() ) / 2.0,
					0
				);

				// calculate edge direction vector for clockwise tangent
				gp_Vec edgeVector = new gp_Vec( vertex2, vertex1 );
				edgeVector.Normalize();
				gp_Dir tangent = new gp_Dir( edgeVector );

				// normal vector pointing toward center
				gp_Vec normalVector = new gp_Vec( edgeMidpoint, new gp_Pnt( 0, 0, 0 ) );
				normalVector.Normalize();
				gp_Dir normal = new gp_Dir( normalVector );

				// transform to final coordinate system
				CADPoint cadPoint = TransformCADPoint(
					edgeMidpoint, local_Z_pos, normal, tangent, coordTransformation
				);

				resultList.Add( cadPoint );
			}

			return resultList;
		}

		#endregion

		#region Common Helper Methods

		static gp_Trsf CreateCoordTransformation( gp_Ax3 refCoord )
		{
			gp_Ax3 targetCoordSystem = new gp_Ax3(
				refCoord.Location(),
				refCoord.Direction(),
				refCoord.XDirection()
			);
			gp_Trsf transformation = new gp_Trsf();
			transformation.SetTransformation( targetCoordSystem, new gp_Ax3() );
			return transformation;
		}

		static CADPoint TransformCADPoint( gp_Pnt localPoint, gp_Dir localNormal1, gp_Dir localNormal2, gp_Dir localTangent, gp_Trsf transformation )
		{
			return new CADPoint(
				localPoint.Transformed( transformation ),
				localNormal1.Transformed( transformation ),
				localNormal2.Transformed( transformation ),
				localTangent.Transformed( transformation )
			);
		}

		#endregion

		#region Polygon Helper Methods

		static int FindNegativeYAxisIntersectingEdge( int sides, double angleOffset, double angleStep )
		{
			for( int i = 0; i < sides; i++ ) {
				double vertex1Angle = angleOffset + i * angleStep;
				double vertex2Angle = angleOffset + ( ( i + 1 ) % sides ) * angleStep;

				// normalize angles to [0, 2£k] range
				vertex1Angle = NormalizeAngle( vertex1Angle );
				vertex2Angle = NormalizeAngle( vertex2Angle );

				// check if this edge crosses 270¢X (3£k/2, which is negative Y-axis)
				double targetAngle = 3.0 * Math.PI / 2.0; // 270¢X

				if( IsAngleBetween( targetAngle, vertex1Angle, vertex2Angle ) ) {
					return i;
				}
			}

			// if not found, return 0 as default
			return 0;
		}

		static double GetPolygonAngleOffset( int sides )
		{
			switch( sides ) {
				case 3:
					return Math.PI / 2.0;
				case 4:
					return Math.PI / 4.0;
				case 5:
					return Math.PI / 2.0;
				case 6:
					return 0.0;
				default:
					return 0.0;
			}
		}

		static double NormalizeAngle( double angle )
		{
			while( angle < 0 )
				angle += 2 * Math.PI;
			while( angle >= 2 * Math.PI )
				angle -= 2 * Math.PI;
			return angle;
		}

		static bool IsAngleBetween( double targetAngle, double startAngle, double endAngle )
		{
			// normalize all angles
			targetAngle = NormalizeAngle( targetAngle );
			startAngle = NormalizeAngle( startAngle );
			endAngle = NormalizeAngle( endAngle );

			if( startAngle <= endAngle ) {
				// no crossing of 0¢X
				return targetAngle >= startAngle && targetAngle <= endAngle;
			}
			else {
				// crossing 0¢X
				return targetAngle >= startAngle || targetAngle <= endAngle;
			}
		}

		#endregion
	}
}
