using OCC.gp;
using System;
using System.Collections.Generic;
using PostToolBridge;

namespace OCCTool
{
	public class PostTool
	{
		public static List<Tuple<double, double>> ConvertIJKToABC( List<gp_Dir> toolVecList )
		{
			List<Tuple<double, double>> result = new List<Tuple<double, double>>();
			List<bool> singularPointList = new List<bool>();
			double dC = 0; // master
			double dA = 0; // slave

			// calculate the A and C angle
			for( int i = 0; i < toolVecList.Count; i++ ) {

				double[] ToolDirection = new double[ 3 ] { toolVecList[ i ].X(), toolVecList[ i ].Y(), toolVecList[ i ].Z() };
				IKSolverWrapper.IJKtoMS( ToolDirection, ToolDirectionAtZero, DirectOfFirstRotAxis, DirectOfSecondRotAxis,
					dC, dA, out double dC1, out double dA1, out double dC2, out double dA2 );


				result.Add( new Tuple<double, double>( dC, dA ) );
			}

			// refine the singular case
			int index = 0;
			while( index < result.Count ) {
				int index0 = index;
				while( index < result.Count - 1 && singularPointList[ index ] ) {
					index++;
				}

				// interpolate C from index0 to index
				double diffC = result[ index ].Item1 - result[ index0 ].Item1;
				for( int i = index0; i < index; i++ ) {
					result[ i ] = new Tuple<double, double>( result[ i ].Item1 + diffC / ( index - index0 ) * ( i - index0 ), 0 );
				}
				index++;
			}
			return result;
		}

		static double[] ToolDirectionAtZero = new double[ 3 ] { 0, 0, 1 };
		static double[] DirectOfFirstRotAxis = new double[ 3 ] { 0, 0, 1 };
		static double[] DirectOfSecondRotAxis = new double[ 3 ] { 1, 0, 0 };
	}
}
