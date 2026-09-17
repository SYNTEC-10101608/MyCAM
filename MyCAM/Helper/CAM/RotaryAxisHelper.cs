using MyCAM.Data;
using MyCAM.Editor;
using OCC.gp;
using System;

namespace MyCAM.Helper
{
	public static class RotaryAxisHelper
	{
		public static RotaryAxisConfig CreateRotaryAxisConfig( FiveAxisMachineData machineData )
		{
			RotaryAxisConfig config = new RotaryAxisConfig();
			config.MasterName = ConvertRotaryAxisName( machineData.MasterRotaryAxis );
			config.SlaveName = ConvertRotaryAxisName( machineData.SlaveRotaryAxis );
			config.RotaryAxisType = GetRotaryAxis( machineData );
			config.RotaryAxisName = ConvertRotaryAxisName( config.RotaryAxisType );
			return config;
		}

		// the axis that can be flipped by +-180, table and mix type are driven by the slave one
		public static RotaryAxis GetRotaryAxis( FiveAxisMachineData machineData )
		{
			if( machineData == null ) {
				return RotaryAxis.Z;
			}
			return ( machineData.FiveAxisType == FiveAxisType.Table || machineData.FiveAxisType == FiveAxisType.Mix )
				? machineData.SlaveRotaryAxis
				: machineData.MasterRotaryAxis;
		}

		public static string ConvertRotaryAxisName( RotaryAxis axis )
		{
			switch( axis ) {
				case RotaryAxis.X:
					return "A";
				case RotaryAxis.Y:
					return "B";
				case RotaryAxis.Z:
					return "C";
				default:
					return "";
			}
		}

		// the linear direction name the rotary axis points to, used by UI text
		public static string ConvertRotaryAxisDirName( RotaryAxis axis )
		{
			switch( axis ) {
				case RotaryAxis.X:
					return "X";
				case RotaryAxis.Y:
					return "Y";
				case RotaryAxis.Z:
					return "Z";
				default:
					return "";
			}
		}

		public static gp_Dir GetRotaryAxisDir( RotaryAxis axis )
		{
			return RotateAxisDef.RotaryAxisToDir( axis );
		}

		// the rotary axis that the ZDir type actually faces, falls back to Z when machine data is absent
		public static gp_Dir GetRotaryAxisDir()
		{
			if( !DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) ) {
				return new gp_Dir( 0, 0, 1 );
			}
			return RotateAxisDef.RotaryAxisToDir( GetRotaryAxis( machineData ) );
		}

		// the phase angle of a point around the rotary axis, measured with the left hand rule as the machine does
		public static double GetPhaseAngle_deg( gp_Pnt contourCenter, gp_Pnt rotateCenter, RotaryAxis axis )
		{
			if( contourCenter == null ) {
				return 0;
			}

			// cal orientation angle of the contour center in the g54 and center is rotate center
			double dX = contourCenter.X() - ( rotateCenter != null ? rotateCenter.X() : 0 );
			double dY = contourCenter.Y() - ( rotateCenter != null ? rotateCenter.Y() : 0 );
			double dZ = contourCenter.Z() - ( rotateCenter != null ? rotateCenter.Z() : 0 );

			double dFirst;
			double dSecond;
			switch( axis ) {
				case RotaryAxis.X:
					dFirst = dY;
					dSecond = dZ;
					break;
				case RotaryAxis.Y:
					dFirst = dZ;
					dSecond = dX;
					break;
				case RotaryAxis.Z:
				default:
					dFirst = dX;
					dSecond = dY;
					break;
			}

			// the point is right on rotate center
			if( Math.Abs( dFirst ) < PHASE_TOLERANCE && Math.Abs( dSecond ) < PHASE_TOLERANCE ) {
				return 0;
			}

			// take negative sign means this axis of rotation follows the left hand rule
			return -Math.Atan2( dSecond, dFirst ) * 180.0 / Math.PI;
		}

		// pick the angle equivalent to target_deg ( modulo 360 ) that is closest to reference_deg
		public static double GetNearestEquivalentAngle_deg( double target_deg, double reference_deg )
		{
			double dDiff = target_deg - reference_deg;

			// round to the nearest turn, ties go to the positive side
			double dTurn = Math.Ceiling( dDiff / 360.0 - 0.5 );
			return target_deg - dTurn * 360.0;
		}

		// the world phase of the assigned machining direction, measured the same way GetPhaseAngle_deg does
		public static double GetMachiningDirPhase_deg( EMachiningDir dir )
		{
			switch( dir ) {
				case EMachiningDir.CrossAxisPositive:
					return -90;
				case EMachiningDir.RefAxisNegative:
					return 180;
				case EMachiningDir.CrossAxisNegative:
					return 90;
				case EMachiningDir.RefAxisPositive:
				default:
					return 0;
			}
		}

		// the display name of the machining direction, the ref and cross axis depend on the rotary axis
		public static string ConvertMachiningDirName( EMachiningDir dir, RotaryAxis axis )
		{
			string szRefAxis;
			string szCrossAxis;
			switch( axis ) {
				case RotaryAxis.X:
					szRefAxis = "Y";
					szCrossAxis = "Z";
					break;
				case RotaryAxis.Y:
					szRefAxis = "Z";
					szCrossAxis = "X";
					break;
				case RotaryAxis.Z:
				default:
					szRefAxis = "X";
					szCrossAxis = "Y";
					break;
			}

			switch( dir ) {
				case EMachiningDir.RefAxisPositive:
					return szRefAxis + " +";
				case EMachiningDir.CrossAxisPositive:
					return szCrossAxis + " +";
				case EMachiningDir.RefAxisNegative:
					return szRefAxis + " -";
				case EMachiningDir.CrossAxisNegative:
					return szCrossAxis + " -";
				default:
					return "";
			}
		}

		const double PHASE_TOLERANCE = 1e-3;
	}
}
