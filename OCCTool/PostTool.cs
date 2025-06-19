using OCC.gp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OCCTool
{
	public static class IKSolverInterop
	{
		[DllImport( "PostTool.dll", CallingConvention = CallingConvention.StdCall )]
		public static extern int IKSolver_IJKtoMS(
			double[] ToolDirection,
			double[] ToolDirectionAtZero,
			double[] DirectOfFirstRotAxis,
			double[] DirectOfSecondRotAxis,
			double LastMasterRotAngle,
			double LastSlaveRotAngle,
			out double MRotAngle1,
			out double SRotAngle1,
			out double MRotAngle2,
			out double SRotAngle2
		);
	}

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
				int solveResult = IKSolverInterop.IKSolver_IJKtoMS( ToolDirection, ToolDirectionAtZero, DirectOfFirstRotAxis, DirectOfSecondRotAxis,
					dM, dS, out double dM1, out double dS1, out double dM2, out double dS2 );

				// sigular case, master has infinite solution
				if( solveResult == 2
					|| Math.Abs( dM1 ) < 1e-3 && Math.Abs( dS1 ) < 1e-3
					|| Math.Abs( dM2 ) < 1e-3 && Math.Abs( dS2 ) < 1e-3 ) {
					singularPointList.Add( true );
					result.Add( new Tuple<double, double>( dM, 0 ) );
					continue;
				}
				singularPointList.Add( false );

				// choose the closest solution
				dM1 = FindClosetCoterminalAngle( dM, dM1 );
				dM2 = FindClosetCoterminalAngle( dM, dM2 );
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
			int index = 0;
			while( index < result.Count ) {
				int index0 = index;
				while( index < result.Count - 1 && singularPointList[ index ] ) {
					index++;
				}

				// interpolate C from index0 to index
				double diffM = result[ index ].Item1 - result[ index0 ].Item1;
				for( int i = index0; i < index; i++ ) {
					result[ i ] = new Tuple<double, double>( result[ i ].Item1 + diffM / ( index - index0 ) * ( i - index0 ), 0 );
				}
				index++;
			}
			return result;
		}

		static double FindClosetCoterminalAngle( double reference, double value )
		{
			double valueN = value - 2 * Math.PI;
			double valueP = value + 2 * Math.PI;

			double diffO = Math.Abs( reference - value );
			double diffN = Math.Abs( reference - valueN );
			double diffP = Math.Abs( reference - valueP );

			if( diffN < diffO && diffN < diffP ) {
				return valueN;
			}
			if( diffP < diffO && diffP < diffN ) {
				return valueP;
			}
			return value;
		}

		static double[] ToolDirectionAtZero = new double[ 3 ] { 0, 0, 1 };
		static double[] DirectOfFirstRotAxis = new double[ 3 ] { 0, 0, 1 };
		static double[] DirectOfSecondRotAxis = new double[ 3 ] { 0, 1, 0 };
	}
}
