using MyCAM.Data;
using System;
using System.IO;

namespace MyCAM.Post
{
	internal static class NCWriterHelper
	{
		public static void WriteTraverse( StreamWriter writer, ITraversePostData postData,
			string masterAxisName, string slaveAxisName, RotaryAxis masterRotaryAxis, RotaryAxis slaveRotaryAxis )
		{
			if( writer == null || postData == null ) {
				throw new ArgumentNullException( "NCWriterHelper.WriteTraverse arguments cannot be null" );
			}

			// lift up
			if( postData.LiftUpPostPoint != null ) {
				WriteLinearTraverse( writer, postData.LiftUpPostPoint, 0, masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );
			}

			// frog leap with cut down
			if( postData.FrogLeapMidPostPoint != null && postData.CutDownPostPoint != null ) {
				WriteFrogLeap( writer, postData.FrogLeapMidPostPoint, postData.CutDownPostPoint, 0,
					masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );

				// cut down
				WriteLinearTraverse( writer, postData.ProcessStartPoint, postData.FollowSafeDistance,
					masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );
			}

			// frog leap without cut down
			else if( postData.FrogLeapMidPostPoint != null && postData.CutDownPostPoint == null ) {
				WriteFrogLeap( writer, postData.FrogLeapMidPostPoint, postData.ProcessStartPoint, postData.FollowSafeDistance,
					masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );
			}

			// no frog leap
			else if( postData.FrogLeapMidPostPoint == null && postData.CutDownPostPoint != null ) {
				WriteLinearTraverse( writer, postData.CutDownPostPoint, 0,
					masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );

				// cut down
				WriteLinearTraverse( writer, postData.ProcessStartPoint, postData.FollowSafeDistance,
					masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );
			}

			// no frog leap and no cut down
			else {
				WriteLinearTraverse( writer, postData.ProcessStartPoint, postData.FollowSafeDistance,
					masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );
			}
		}

		public static void WriteLinearTraverse( StreamWriter writer, PostPoint postPoint, double followSafeDistance,
			string masterAxisName, string slaveAxisName, RotaryAxis masterRotaryAxis, RotaryAxis slaveRotaryAxis )
		{
			if( postPoint == null ) {
				return;
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI,
				masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis );
			string szFollow = followSafeDistance == 0 ? string.Empty : FOLLOW_SAFE_DISTANCE_COMMAND + followSafeDistance.ToString( "F3" );
			writer.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand};" );
		}

		public static void WriteFrogLeap( StreamWriter writer, PostPoint midPoint, PostPoint endPoint, double followSafeDistance,
			string masterAxisName, string slaveAxisName, RotaryAxis masterRotaryAxis, RotaryAxis slaveRotaryAxis )
		{
			if( midPoint == null || endPoint == null ) {
				return;
			}

			// mid point
			string szX1 = midPoint.X.ToString( "F3" );
			string szY1 = midPoint.Y.ToString( "F3" );
			string szZ1 = midPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand1 = GetRotaryAxisCommand( midPoint.Master * 180 / Math.PI, midPoint.Slave * 180 / Math.PI,
				masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis, "1=" );

			// end point
			string szX2 = endPoint.X.ToString( "F3" );
			string szY2 = endPoint.Y.ToString( "F3" );
			string szZ2 = endPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand2 = GetRotaryAxisCommand( endPoint.Master * 180 / Math.PI, endPoint.Slave * 180 / Math.PI,
				masterAxisName, slaveAxisName, masterRotaryAxis, slaveRotaryAxis, "2=" );
			string szFollow = followSafeDistance == 0 ? string.Empty : FOLLOW_SAFE_DISTANCE_COMMAND + followSafeDistance.ToString( "F3" );
			writer.WriteLine( $"G65 P\"FROG_LEAP\" X1={szX1} Y1={szY1} Z1={szZ1} {szRotaryAxisCommand1} " +
				$"X2={szX2} Y2={szY2} Z2={szZ2} {szRotaryAxisCommand2} {szFollow} W8.0;" );
		}

		public static string GetRotaryAxisCommand( double master_deg, double slave_deg,
			string masterAxisName, string slaveAxisName, RotaryAxis masterRotaryAxis, RotaryAxis slaveRotaryAxis,
			string szAxisCommandFix = "" )
		{
			string szM = masterAxisName + szAxisCommandFix + master_deg.ToString( "F3" );
			string szS = slaveAxisName + szAxisCommandFix + slave_deg.ToString( "F3" );
			if( masterRotaryAxis < slaveRotaryAxis ) {
				return szM + " " + szS;
			}
			else {
				return szS + " " + szM;
			}
		}

		const string FOLLOW_SAFE_DISTANCE_COMMAND = "S";
	}
}
