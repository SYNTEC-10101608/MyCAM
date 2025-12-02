using MyCAM.Data;
using OCC.gp;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class OverCutHelper
	{
		public static void SetOverCut( IReadOnlyList<IOverCutPoint> mainPointList, out List<IOverCutPoint> overCutPointList,
			double overCutLength, bool isClosed, double overCutTolerance = OVERCUT_MATH_TOLERANCE )
		{
			overCutPointList = new List<IOverCutPoint>();
			if( mainPointList.Count == 0 || overCutLength == 0 || !isClosed ) {
				return;
			}
			double dTotalOverCutLength = 0;

			// end point is the start of over cut
			overCutPointList.Add( mainPointList.Last().Clone() );
			for( int i = 0; i < mainPointList.Count - 1; i++ ) {

				// get this edge distance
				double dDistance = mainPointList[ i ].Point.Distance( mainPointList[ i + 1 ].Point );
				if( dTotalOverCutLength + dDistance < overCutLength ) {

					// still within overcut length → take next point directly
					overCutPointList.Add( mainPointList[ i + 1 ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = overCutLength - dTotalOverCutLength;
					if( dRemain <= overCutTolerance ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint( mainPointList[ i ].Point, mainPointList[ i + 1 ].Point, dRemain );

					// interpolate tool vector
					InterpolateVecBetween2Point( mainPointList[ i ], mainPointList[ i + 1 ], overCutEndPoint, out gp_Dir endPointToolVec, out gp_Dir endPointTangentVec );

					// create new cam point
					IOverCutPoint camPoint = BuildOverCutPoint( overCutEndPoint, endPointToolVec, endPointTangentVec );
					overCutPointList.Add( camPoint );
					return;
				}
			}
		}

		static gp_Pnt GetExactOverCutEndPoint( gp_Pnt currentPoint, gp_Pnt nextPoint, double dDistanceMoveFromOverPoint )
		{
			// from currentPoint → nextOverLengthPoint
			gp_Vec movingVec = new gp_Vec( currentPoint, nextPoint );

			// normalize to unit vector
			movingVec.Normalize();

			gp_Vec moveVec = movingVec.Multiplied( dDistanceMoveFromOverPoint );

			// shifted along the vector
			return new gp_Pnt( currentPoint.XYZ() + moveVec.XYZ() );
		}

		// get mid point
		static void InterpolateVecBetween2Point( IOverCutPoint currentCAMPoint, IOverCutPoint nextCAMPoint, gp_Pnt point,
			out gp_Dir toolDir, out gp_Dir tangentDir )
		{
			toolDir = currentCAMPoint.ToolVec;
			tangentDir = currentCAMPoint.TangentVec;

			// get current and next tool vector
			gp_Vec currentVec = new gp_Vec( currentCAMPoint.ToolVec );
			gp_Vec nextVec = new gp_Vec( nextCAMPoint.ToolVec );

			// get current and next tangent vector
			gp_Vec currentTangentVec = new gp_Vec( currentCAMPoint.TangentVec );
			gp_Vec nextTangentVec = new gp_Vec( nextCAMPoint.TangentVec );

			// calculate new point percentage
			double dDistanceOfCAMPath2Point = currentCAMPoint.Point.Distance( nextCAMPoint.Point );
			double dDistanceBetweenCurrentPoint2NewPoint = currentCAMPoint.Point.Distance( point );

			// two point overlap
			if( dDistanceOfCAMPath2Point <= OVERCUT_MATH_TOLERANCE ) {
				return;
			}
			double interpolatePercent = dDistanceBetweenCurrentPoint2NewPoint / dDistanceOfCAMPath2Point;

			// get new point dir
			toolDir = InterpolateVecBetween2Vec( currentVec, nextVec, interpolatePercent );
			tangentDir = InterpolateVecBetween2Vec( currentTangentVec, nextTangentVec, interpolatePercent );
		}

		// get vec of mid point
		static gp_Dir InterpolateVecBetween2Vec( gp_Vec currentVec, gp_Vec nextVec, double interpolatePercent )
		{
			// this case is unsolvable, so just return current vec
			if( currentVec.IsOpposite( nextVec, VECTOR_OPPOSITE_TOLERANCE ) ) {
				return new gp_Dir( currentVec.XYZ() );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( currentVec, nextVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );

			// calculate new point attitude
			gp_Quaternion q = new gp_Quaternion();
			slerp.Interpolate( interpolatePercent, ref q );
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetRotation( q );
			gp_Dir resultDir = new gp_Dir( currentVec.Transformed( trsf ) );
			return resultDir;
		}

		// this method is now for building a CAMPoint as OverCutPoint
		static IOverCutPoint BuildOverCutPoint( gp_Pnt overCutEndPoint, gp_Dir endPointToolVec, gp_Dir endPointTangentVec )
		{
			CADPoint cadPoint = new CADPoint( overCutEndPoint, endPointToolVec, endPointToolVec, endPointTangentVec );
			CAMPoint camPoint = new CAMPoint( cadPoint, endPointToolVec );
			return camPoint;
		}

		const double OVERCUT_MATH_TOLERANCE = 0.001;
		const double VECTOR_OPPOSITE_TOLERANCE = 0.001;
	}
}
