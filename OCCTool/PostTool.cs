using OCC.gp;
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
			double dC = 0;

			// calculate the A and C angle
			for( int i = 0; i < toolVecList.Count; i++ ) {
				bool bCPlus = true;
				if( Math.Abs( toolVecList[ i ].X() ) < 1e-6 && Math.Abs( toolVecList[ i ].Y() ) < 1e-6 ) {

					// the singular case
					singularPointList.Add( true );
				}
				else {
					//double dC1 = ( ( Math.Atan2( toolVecList[ i ].X(), -toolVecList[ i ].Y() ) ) + 2 * Math.PI ) % ( 2 * Math.PI );
					//double dC2 = ( ( Math.Atan2( -toolVecList[ i ].X(), toolVecList[ i ].Y() ) ) + 2 * Math.PI ) % ( 2 * Math.PI );
					double dC1 = ( ( Math.Atan2( toolVecList[ i ].X(), -toolVecList[ i ].Y() ) ) );
					double dC2 = ( ( Math.Atan2( -toolVecList[ i ].X(), toolVecList[ i ].Y() ) ) );
					if( Math.Abs( dC1 - dC ) <= Math.Abs( dC2 - dC ) ) {
						dC = dC1;
						bCPlus = true;
					}
					else {
						dC = dC2;
						bCPlus = false;
					}
					singularPointList.Add( false );
				}
				double dA = Math.Acos( toolVecList[ i ].Z() );
				if( !bCPlus ) {
					dA *= -1;
				}
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
	}
}
