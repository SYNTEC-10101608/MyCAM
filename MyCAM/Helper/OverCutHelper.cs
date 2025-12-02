using MyCAM.App;
using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCAM.Helper
{
	public static  class OverCutHelper
	{
		public static void SetOverCut()
		{
			m_OverCutPointList.Clear();
			if( m_CAMPointList.Count == 0 || m_CraftData.OverCutLength == 0 || !IsClosed ) {
				return;
			}
			double dTotalOverCutLength = 0;

			// end point is the start of over cut
			m_OverCutPointList.Add( m_CAMPointList.Last().Clone() );
			for( int i = 0; i < m_CAMPointList.Count - 1; i++ ) {

				// get this edge distance
				double dDistance = m_CAMPointList[ i ].CADPoint.Point.Distance( m_CAMPointList[ i + 1 ].CADPoint.Point );
				if( dTotalOverCutLength + dDistance < m_CraftData.OverCutLength ) {

					// still within overcut length → take next point directly
					m_OverCutPointList.Add( m_CAMPointList[ i + 1 ].Clone() );
					dTotalOverCutLength += dDistance;
				}
				else {

					// need to stop inside this segment
					double dRemain = m_CraftData.OverCutLength - dTotalOverCutLength;
					if( dRemain <= MyApp.PRECISION_MIN_ERROR ) {
						return;
					}

					// compute new point along segment
					gp_Pnt overCutEndPoint = GetExactOverCutEndPoint( m_CAMPointList[ i ].CADPoint.Point, m_CAMPointList[ i + 1 ].CADPoint.Point, dRemain );

					// interpolate tool vector
					InterpolateToolAndTangentVecBetween2CAMPoint( m_CAMPointList[ i ], m_CAMPointList[ i + 1 ], overCutEndPoint, out gp_Dir endPointToolVec, out gp_Dir endPointTangentVec );

					// create new cam poiont
					CADPoint cadPoint = new CADPoint( overCutEndPoint, endPointToolVec, endPointToolVec, endPointTangentVec );
					CAMPoint camPoint = new CAMPoint( cadPoint, endPointToolVec );
					m_OverCutPointList.Add( camPoint );
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

		static gp_Dir InterpolateVecBetween2Vec( gp_Vec currentVec, gp_Vec nextVec, double interpolatePercent )
		{
			// this case is unsolcvable, so just return current vec
			if( currentVec.IsOpposite( nextVec, MyApp.PRECISION_MIN_ERROR ) ) {
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

		static void InterpolateToolAndTangentVecBetween2CAMPoint( CAMPoint currentCAMPoint, CAMPoint nextCAMPoint, gp_Pnt point, out gp_Dir toolDir, out gp_Dir tangentDir )
		{
			toolDir = currentCAMPoint.ToolVec;
			tangentDir = currentCAMPoint.CADPoint.TangentVec;

			// get current and next tool vector
			gp_Vec currentVec = new gp_Vec( currentCAMPoint.ToolVec );
			gp_Vec nextVec = new gp_Vec( nextCAMPoint.ToolVec );

			// get current and next tangent vector
			gp_Vec currentTangentVec = new gp_Vec( currentCAMPoint.CADPoint.TangentVec );
			gp_Vec nextTangentVec = new gp_Vec( nextCAMPoint.CADPoint.TangentVec );

			// calculate new point percentage
			double dDistanceOfCAMPath2Point = currentCAMPoint.CADPoint.Point.Distance( nextCAMPoint.CADPoint.Point );
			double dDistanceBetweenCurrentPoint2NewPoint = currentCAMPoint.CADPoint.Point.Distance( point );

			// two point overlap
			if( dDistanceOfCAMPath2Point <= MyApp.PRECISION_MIN_ERROR ) {
				return;
			}
			double interpolatePercent = dDistanceBetweenCurrentPoint2NewPoint / dDistanceOfCAMPath2Point;

			// get new point dir
			toolDir = InterpolateVecBetween2Vec( currentVec, nextVec, interpolatePercent );
			tangentDir = InterpolateVecBetween2Vec( currentTangentVec, nextTangentVec, interpolatePercent );
		}
	}
}
