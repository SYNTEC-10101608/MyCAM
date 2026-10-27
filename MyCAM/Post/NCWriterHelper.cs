using MyCAM.Data;
using System;
using System.IO;

namespace MyCAM.Post
{
	internal static class NCWriterHelper
	{
		public static void WriteEntry( StreamWriter writer, ITraversePostData postData, AxisConfig axisConfig )
		{
			if( writer == null || postData == null || axisConfig == null ) {
				throw new ArgumentNullException( "NCWriterHelper.WriteEntry arguments cannot be null" );
			}
			WriteLinearTraverse( writer, postData.CutDownPostPoint, 0, axisConfig );
			WriteLinearTraverse( writer, postData.ProcessStartPoint, postData.FollowSafeDistance, axisConfig );
		}

		public static void WriteTraverse( StreamWriter writer, ITraversePostData postData, AxisConfig axisConfig, double? enterTiltAngle_deg = null )
		{
			if( writer == null || postData == null || axisConfig == null ) {
				throw new ArgumentNullException( "NCWriterHelper.WriteTraverse arguments cannot be null" );
			}
			if( postData.ProcessStartPoint == null ) {
				throw new InvalidOperationException( "Traverse requires a process start point." );
			}
			if( postData.SafeBoundaryPostPointList != null && postData.SafeBoundaryPostPointList.Count < 2 ) {
				throw new InvalidOperationException( "Safe boundary traverse requires at least two post points." );
			}
			double cutDownDistance = 0;
			if( postData.CutDownPostPoint != null ) {
				double dx = postData.ProcessStartPoint.X - postData.CutDownPostPoint.X;
				double dy = postData.ProcessStartPoint.Y - postData.CutDownPostPoint.Y;
				double dz = postData.ProcessStartPoint.Z - postData.CutDownPostPoint.Z;
				cutDownDistance = Math.Sqrt( dx * dx + dy * dy + dz * dz );
			}
			if( double.IsNaN( cutDownDistance ) || double.IsInfinity( cutDownDistance ) ||
				double.IsNaN( postData.FollowSafeDistance ) || double.IsInfinity( postData.FollowSafeDistance ) ||
				postData.FollowSafeDistance < 0 ) {
				throw new InvalidOperationException( "Cut-down and follow safe distances must be finite and non-negative." );
			}
			PostPoint cutDownPoint = postData.CutDownPostPoint ?? postData.ProcessStartPoint;

			// lift up
			if( postData.LiftUpPostPoint != null ) {
				WriteTraverseLinearMove( writer, postData.LiftUpPostPoint, 0, axisConfig );
			}

			if( postData.SafeBoundaryPostPointList != null ) {
				for( int index = 1; index < postData.SafeBoundaryPostPointList.Count; index++ ) {
					WriteTraverseLinearMove( writer, postData.SafeBoundaryPostPointList[ index ], 0, axisConfig );
				}
			}
			// frog leap with cut down
			else if( postData.FrogLeapMidPostPoint != null ) {
				WriteFrogLeap( writer, postData.FrogLeapMidPostPoint, cutDownPoint, 0, axisConfig, enterTiltAngle_deg );
			}
			else {
				if( postData.LiftUpPostSafePlanePoint != null && postData.CutDownPostSafePlanePoint != null ) {
					WriteTraverseLinearMove( writer, postData.LiftUpPostSafePlanePoint, 0, axisConfig );
					WriteTraverseLinearMove( writer, postData.CutDownPostSafePlanePoint, 0, axisConfig );
				}
				WriteTraverseLinearMove( writer, cutDownPoint, 0, axisConfig );
			}

			if( cutDownDistance > 0 ) {
				WriteTraverseLinearMove( writer, postData.ProcessStartPoint,
					Math.Min( postData.FollowSafeDistance, cutDownDistance ), axisConfig );
			}
		}

		static void WriteTraverseLinearMove( StreamWriter writer, PostPoint postPoint, double followSafeDistance, AxisConfig axisConfig )
		{
			if( postPoint == null ) {
				throw new InvalidOperationException( "Traverse move requires a post point." );
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI, axisConfig );
			string szFollow = followSafeDistance == 0 ? string.Empty : " S2=" + followSafeDistance.ToString( "F3" );
			writer.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand}{szFollow} F@10088;" );
		}

		public static void WriteLinearTraverse( StreamWriter writer, PostPoint postPoint, double followSafeDistance, AxisConfig axisConfig, bool isRapid = true )
		{
			if( postPoint == null ) {
				return;
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI, axisConfig );
			string szFollow = followSafeDistance == 0 ? string.Empty : FOLLOW_SAFE_DISTANCE_COMMAND + followSafeDistance.ToString( "F3" );
			string szMotion = isRapid ? "G00" : "G01";
			writer.WriteLine( $"{szMotion} X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand} {szFollow};" );
		}

		public static void WriteFrogLeap( StreamWriter writer, PostPoint midPoint, PostPoint endPoint, double followSafeDistance, AxisConfig axisConfig, double? enterTiltAngle_deg = null )
		{
			if( midPoint == null || endPoint == null ) {
				return;
			}

			// mid point
			string szX1 = midPoint.X.ToString( "F3" );
			string szY1 = midPoint.Y.ToString( "F3" );
			string szZ1 = midPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand1 = GetRotaryAxisCommand( midPoint.Master * 180 / Math.PI, midPoint.Slave * 180 / Math.PI, axisConfig, "1=" );

			// end point
			string szX2 = endPoint.X.ToString( "F3" );
			string szY2 = endPoint.Y.ToString( "F3" );
			string szZ2 = endPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand2 = GetRotaryAxisCommand( endPoint.Master * 180 / Math.PI, endPoint.Slave * 180 / Math.PI, axisConfig, "2=" );
			string szFollow = followSafeDistance == 0 ? string.Empty : " " + FOLLOW_SAFE_DISTANCE_COMMAND + followSafeDistance.ToString( "F3" );
			//string szTiltAngle = enterTiltAngle_deg.HasValue ? " S3=" + enterTiltAngle_deg.Value.ToString( "F3" ) : string.Empty;
			writer.WriteLine( $"G65 P\"FROG_LEAP\" X1={szX1} Y1={szY1} Z1={szZ1} {szRotaryAxisCommand1} " +
				$"X2={szX2} Y2={szY2} Z2={szZ2} {szRotaryAxisCommand2}{szFollow};" );
		}

		public static string GetRotaryAxisCommand( double master_deg, double slave_deg, AxisConfig axisConfig,
			string szAxisCommandFix = "" )
		{
			string szM = axisConfig.MasterAxisName + szAxisCommandFix + master_deg.ToString( "F3" );
			string szS = axisConfig.SlaveAxisName + szAxisCommandFix + slave_deg.ToString( "F3" );

			// single rotary axis machine: skip the locked axis
			if( !axisConfig.WriteMaster ) {
				return szS;
			}
			if( !axisConfig.WriteSlave ) {
				return szM;
			}
			if( axisConfig.MasterRotaryAxis < axisConfig.SlaveRotaryAxis ) {
				return szM + " " + szS;
			}
			else {
				return szS + " " + szM;
			}
		}

		const string FOLLOW_SAFE_DISTANCE_COMMAND = "S";
	}
}
