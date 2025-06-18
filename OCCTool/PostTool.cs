using OCC.gp;
using PostToolBridge;
using System;
using System.Collections.Generic;

namespace OCCTool
{
	public class PostTool
	{
		public static List<Tuple<double, double>> ConvertIJKToABC( List<gp_Dir> toolVecList )
		{
			List<Tuple<double, double>> result = new List<Tuple<double, double>>();
			List<bool> singularPointList = new List<bool>();
			double dM = 0; // master
			double dS = 0; // slave

			// calculate the A and C angle
			for( int i = 0; i < toolVecList.Count; i++ ) {

				double[] ToolDirection = new double[ 3 ] { toolVecList[ i ].X(), toolVecList[ i ].Y(), toolVecList[ i ].Z() };
				int solveResult = IKSolverWrapper.IJKtoMS( ToolDirection, ToolDirectionAtZero, DirectOfFirstRotAxis, DirectOfSecondRotAxis,
					dM, dS, out double dM1, out double dS1, out double dM2, out double dS2 );

				// sigular case, master has infinite solution
				if( solveResult == 2 ) {
					singularPointList.Add( true );
				}
				else {
					singularPointList.Add( false );
				}

				if( Math.Abs( dM1 ) < 1e-3 && Math.Abs( dS1 ) < 1e-3 ) {
					result.Add( new Tuple<double, double>( dM, 0 ) );
					continue;
				}

				if( Math.Abs( dS1 * 180 / Math.PI - ( -0.600 ) ) < 1e-3 || Math.Abs( dS2 * 180 / Math.PI - ( -0.600 ) ) < 1e-3 ) {
				}

				// choose the closest solution
				double dM1N = dM1 - 2 * Math.PI;
				double dM1P = dM1 + 2 * Math.PI;
				double diff1O = Math.Abs( dM - dM1 );
				double diff1N = Math.Abs( dM - dM1N );
				double diff1P = Math.Abs( dM - dM1P );
				if( diff1N < diff1O && diff1N < diff1P ) {
					dM1 = dM1N;
				}
				if( diff1P < diff1O && diff1P < diff1N ) {
					dM1 = dM1P;
				}
				double dM2N = dM2 - 2 * Math.PI;
				double dM2P = dM2 + 2 * Math.PI;
				double diff2O = Math.Abs( dM - dM2 );
				double diff2N = Math.Abs( dM - dM2N );
				double diff2P = Math.Abs( dM - dM2P );
				if( diff2N < diff2O && diff2N < diff2P ) {
					dM2 = dM2N;
				}
				if( diff2P < diff2O && diff2P < diff2N ) {
					dM2 = dM2P;
				}
				double diff1 = Math.Abs( dM - dM1 ) + Math.Abs( dS - dS1 );
				double diff2 = Math.Abs( dM - dM2 ) + Math.Abs( dS - dS2 );
				if( diff1 < diff2 ) {
					dM = dM1;
					dS = dS1;
				}
				else {
					dM = dM2;
					dS = dS2;
				}
				result.Add( new Tuple<double, double>( dM, dS ) );
			}

			// refine the singular case
			//int index = 0;
			//while( index < result.Count ) {
			//	int index0 = index;
			//	while( index < result.Count - 1 && singularPointList[ index ] ) {
			//		index++;
			//	}

			//	// interpolate C from index0 to index
			//	double diffM = result[ index ].Item1 - result[ index0 ].Item1;
			//	for( int i = index0; i < index; i++ ) {
			//		result[ i ] = new Tuple<double, double>( result[ i ].Item1 + diffM / ( index - index0 ) * ( i - index0 ), 0 );
			//	}
			//	index++;
			//}
			return result;
		}

		static double[] ToolDirectionAtZero = new double[ 3 ] { 0, 0, 1 };
		static double[] DirectOfFirstRotAxis = new double[ 3 ] { 0, 0, 1 };
		static double[] DirectOfSecondRotAxis = new double[ 3 ] { 0, 1, 0 };
	}
}
