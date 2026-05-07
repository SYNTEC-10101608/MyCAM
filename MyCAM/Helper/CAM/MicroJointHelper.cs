using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper.CAM
{
	public static class MicroJointHelper
	{
		public static void SetMicroJoint( ref List<CAMPoint> camPointList, List<Tuple<int, double>> microJointStartIdxList )
		{
			if( microJointStartIdxList == null || microJointStartIdxList.Count == 0 || camPointList == null || camPointList.Count == 0 ) {
				return;
			}

			foreach( Tuple<int, double> microJointStartIdx in microJointStartIdxList ) {
				if( microJointStartIdx.Item1 < 0 || microJointStartIdx.Item1 >= camPointList.Count - 1 ) {
					continue;
				}
				camPointList[ microJointStartIdx.Item1 ].IsMicroJointStart = true;
			}
			SetMicroJointEnd( ref camPointList, ref microJointStartIdxList );
		}

		const double TOLERANCE = 1e-3;
		const double LENGTH_THRESHOLD = 2.0;

		static void SetMicroJointEnd( ref List<CAMPoint> camPointList, ref List<Tuple<int, double>> microJointStartIdxList )
		{
			if( camPointList == null || camPointList.Count == 0 ) {
				return;
			}
			if( microJointStartIdxList == null || microJointStartIdxList.Count == 0 ) {
				return;
			}
			// order microJointStartIdxList by index to ensure the micro joint end point is calculated in the correct order
			microJointStartIdxList = microJointStartIdxList.OrderBy( x => x.Item1 ).ToList();
			for( int i = 0; i < camPointList.Count; i++ ) {
				if( camPointList[ i ].IsMicroJointStart ) {
					double microJointLength = microJointStartIdxList.FirstOrDefault( x => x.Item1 == i )?.Item2 ?? LENGTH_THRESHOLD;
					CalMicroJointEndPnt( ref camPointList, i, microJointLength, out bool isAddPnt, out int nInsertIdx );
					if( isAddPnt ) {
						UpdateCAMIdxAfterInsertion( ref microJointStartIdxList, nInsertIdx );
					}
				}
			}
		}

		static void UpdateCAMIdxAfterInsertion( ref List<Tuple<int, double>> microJointStartIdxList, int insertPosition )
		{
			if( microJointStartIdxList == null ) {
				return;
			}

			// increment idx for all micro joint start points which after the insertion position
			for( int i = 0; i < microJointStartIdxList.Count; i++ ) {
				if( microJointStartIdxList[ i ].Item1 >= insertPosition ) {
					microJointStartIdxList[ i ] = new Tuple<int, double>(
						microJointStartIdxList[ i ].Item1 + 1,
						microJointStartIdxList[ i ].Item2
					);
				}
			}
		}

		static void CalMicroJointEndPnt( ref List<CAMPoint> camPointList, int nIdx, double microJointLength, out bool isAddPnt, out int insertAtIdx )
		{
			isAddPnt = false;
			insertAtIdx = -1;
			double dLengthTillCurrentIdx = 0;
			double dLengthInclueNextIdx = 0;
			for( int i = nIdx; i < camPointList.Count - 1; i++ ) {
				double dist = camPointList[ i ].Point.Distance( camPointList[ i + 1 ].Point );
				dLengthInclueNextIdx += dist;

				// use this pnt as microjoint end 
				if( Math.Abs( dLengthInclueNextIdx - microJointLength ) <= TOLERANCE ) {
					camPointList[ i + 1 ].IsMicroJointEnd = true;
					return;
				}

				// keep calculating until the distance > LENGTH_THRESHOLD
				if( dLengthInclueNextIdx < microJointLength ) {
					dLengthTillCurrentIdx = dLengthInclueNextIdx;
					continue;
				}

				// add micro joint end pnt at distance of microJointLength from micro joint start point
				double advanceDistance = microJointLength - dLengthTillCurrentIdx;
				bool isCalculated = CalculatePointAtDistance( camPointList[ i ], camPointList[ i + 1 ], advanceDistance, out CAMPoint newCAMPoint );
				if( isCalculated && newCAMPoint != null ) {

					// insert newCAMPoint between camPointList[i-1] and camPointList[i ]
					newCAMPoint.IsMicroJointEnd = true;
					camPointList.Insert( i + 1, newCAMPoint );
					insertAtIdx = i + 1;
					isAddPnt = true;
					return;
				}
			}
			// get the end of the path if the total length is less than 2
			if( dLengthInclueNextIdx < microJointLength ) {
				camPointList.Last().IsMicroJointEnd = true;
			}
		}

		static bool CalculatePointAtDistance( ISetToolVecPoint startPoint, ISetToolVecPoint endPoint, double dLength, out CAMPoint newCAMPoint )
		{
			newCAMPoint = null;
			if( startPoint == null || endPoint == null ) {
				return false;
			}

			if( Math.Abs( dLength ) < TOLERANCE ) {
				return false;
			}
			gp_Pnt startPnt = startPoint.Point;
			gp_Pnt endPnt = endPoint.Point;

			// get direction vector from start to end
			gp_Vec direction = new gp_Vec( startPnt, endPnt );

			// check if zero vector
			if( direction.Magnitude() < TOLERANCE ) {
				newCAMPoint = null;
				return false;
			}
			direction.Normalize();
			direction.Multiply( dLength );

			// get new point
			gp_Pnt newPosition = new gp_Pnt(
				startPnt.X() + direction.X(),
				startPnt.Y() + direction.Y(),
				startPnt.Z() + direction.Z()
			);

			// create new CADPoint with the same normal and tangent vectors as the start point
			gp_Dir averageDir = CalculateVectorAverage( startPoint.InitToolVec, endPoint.InitToolVec );
			gp_Dir averageTangDir = CalculateVectorAverage( startPoint.TangentVec, endPoint.TangentVec );
			CADPoint newCADPoint = new CADPoint(
				newPosition,
				averageDir,
				new gp_Dir( averageDir.XYZ() ),
				averageTangDir
			);
			gp_Dir averageToolVec = CalculateVectorAverage( startPoint.ToolVec, endPoint.ToolVec );

			// create new CAMPoint
			CAMPoint camPoint = new CAMPoint( newCADPoint, averageToolVec );

			// interpolate master/slave angle by distance ratio
			camPoint.ModMaster_rad = startPoint.ModMaster_rad + ( endPoint.ModMaster_rad - startPoint.ModMaster_rad ) * ( dLength / startPnt.Distance( endPnt ) );
			camPoint.ModSlave_rad = startPoint.ModSlave_rad + ( endPoint.ModSlave_rad - startPoint.ModSlave_rad ) * ( dLength / startPnt.Distance( endPnt ) );
			camPoint.IsToolVecModPoint = false;
			camPoint.IsMicroJointEnd = true;
			newCAMPoint = camPoint;
			return true;
		}

		static gp_Dir CalculateVectorAverage( gp_Dir dir1, gp_Dir dir2 )
		{
			if( dir1 == null || dir2 == null ) {
				return new gp_Dir( 0, 0, 1 );
			}

			// convert gp_Dir to gp_Vec for calculation
			gp_Vec vec1 = new gp_Vec( dir1 );
			gp_Vec vec2 = new gp_Vec( dir2 );

			// add the two vectors
			gp_Vec sumVector = vec1.Added( vec2 );

			// get the average by multiplying the sum by 0.5
			sumVector.Multiply( 0.5 );

			// check if the result is opposite directions
			if( sumVector.Magnitude() < 1e-3 ) {
				return new gp_Dir( 0, 0, 1 );
			}

			// convert back to gp_Dir
			return new gp_Dir( sumVector );
		}
	}
}
