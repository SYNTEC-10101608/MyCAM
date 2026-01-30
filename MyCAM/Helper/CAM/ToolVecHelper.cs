using MyCAM.Data;
using MyCAM.Post;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class ToolVecHelper
	{
		public enum ECalAngleResult
		{
			Done,
			TooLargeAngle,
			DataError
		}

		public static void SetToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, bool isClosed, EToolVecInterpolateType interpolateType )
		{
			// mark the modified point
			for( int i = 0; i < toolVecPointList.Count; i++ ) {
				if( !toolVecModifyMap.ContainsKey( i ) ) {
					continue;
				}
				toolVecPointList[ i ].IsToolVecModPoint = true;
			}
			ModifyToolVec( ref toolVecPointList, toolVecModifyMap, isClosed, interpolateType );
		}

		public static ECalAngleResult GetABAngleToTargetVec( gp_Dir assignDir, ISetToolVecPoint toModifyPnt, out Tuple<double, double> abAngle_deg )
		{
			ECalAngleResult CalResult = GetABAngleToTargetVec( toModifyPnt, assignDir, out double dRA_rad, out double dRB_rad );
			abAngle_deg = new Tuple<double, double>( dRA_rad, dRB_rad );
			return CalResult;
		}

		public static gp_Vec GetVecFromABAngle( ISetToolVecPoint toolVecPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( toolVecPoint.InitToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = toolVecPoint.TangentVec;
			gp_Dir z = toolVecPoint.InitToolVec;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {

				// when |RB| > 90°, Z will be negative
				if( Math.Abs( dRB_rad ) > Math.PI / 2 ) {
					Z = -1;
				}
				else {
					Z = 1;
				}
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( -dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
		}

		public static Tuple<double, double> GetMSAngleFromABAngle( double dRA_deg, double dRB_deg, ISetToolVecPoint toolVecPoint )
		{
			// Get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// Create PostSolver
			PostSolver postSolver = new PostSolver( machineData );

			// Convert A/B angles to tool vector
			gp_Vec toolVec = GetVecFromABAngle( toolVecPoint, dRA_deg * Math.PI / 180.0, dRB_deg * Math.PI / 180.0 );
			CAMPoint p = ( toolVecPoint as CAMPoint ).Clone();
			p.ToolVec = new gp_Dir( toolVec.XYZ() );

			// Use SolveIK to get master/slave angles (initial values = 0, 0)
			IKSolveResult result = postSolver.SolveIK( p, 0, 0, out double dMaster_rad, out double dSlave_rad );

			if( result == IKSolveResult.InvalidInput || result == IKSolveResult.NoSolution || result == IKSolveResult.OutOfRange ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// Convert radians to degrees
			double dMaster_deg = dMaster_rad * 180.0 / Math.PI;
			double dSlave_deg = dSlave_rad * 180.0 / Math.PI;

			return new Tuple<double, double>( dMaster_deg, dSlave_deg );
		}

		public static Tuple<double, double> GetABAngleFromMSAngle( double dMaster_deg, double dSlave_deg, ISetToolVecPoint toolVecPoint )
		{
			// Get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// Create PostSolver
			PostSolver postSolver = new PostSolver( machineData );

			// Convert degrees to radians for FK solver
			double dMaster_rad = dMaster_deg * Math.PI / 180.0;
			double dSlave_rad = dSlave_deg * Math.PI / 180.0;

			// Use PostSolver API to get the tool vector from master/slave angles
			gp_Dir toolVec = postSolver.SolveToolVec( dMaster_rad, dSlave_rad );

			// Use GetABAngleToTargetVec to convert tool vector to A/B angles
			ECalAngleResult result = GetABAngleToTargetVec( toolVecPoint, toolVec, out double dRA_deg, out double dRB_deg );

			if( result != ECalAngleResult.Done ) {
				return new Tuple<double, double>( 0, 0 );
			}
			return new Tuple<double, double>( dRA_deg, dRB_deg );
		}

		static void ModifyToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap,
			bool isClosed, EToolVecInterpolateType interpolateType )
		{
			if( toolVecModifyMap.Count == 0 ) {
				return;
			}
			if( interpolateType == EToolVecInterpolateType.VectorInterpolation ) {
				ApplyVectorInterpolation( ref toolVecPointList, toolVecModifyMap, isClosed );
				return;
			}
			if( interpolateType == EToolVecInterpolateType.TiltAngleInterpolation ) {
				ApplyTiltAngleInterpolation( ref toolVecPointList, toolVecModifyMap, isClosed );
				return;
			}
		}

		static List<Tuple<gp_Vec, gp_Vec>> GetIntervalToolVec( List<Tuple<int, int>> interpolateIntervalList, List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			List<Tuple<gp_Vec, gp_Vec>> result = new List<Tuple<gp_Vec, gp_Vec>>();
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				gp_Vec startVec = GetVecFromABAngle( toolVecPointList[ nStartIndex ],
					toolVecModifyMap[ nStartIndex ].RA_deg * Math.PI / 180,
					toolVecModifyMap[ nStartIndex ].RB_deg * Math.PI / 180 );
				gp_Vec endVec = GetVecFromABAngle( toolVecPointList[ nEndIndex ],
					toolVecModifyMap[ nEndIndex ].RA_deg * Math.PI / 180,
					toolVecModifyMap[ nEndIndex ].RB_deg * Math.PI / 180 );
				result.Add( new Tuple<gp_Vec, gp_Vec>( startVec, endVec ) );
			}
			return result;
		}

		static List<TiltABAngle> GetABAngleFromInterval( List<Tuple<int, int>> interpolateIntervalList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			List<TiltABAngle> result = new List<TiltABAngle>();
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				TiltABAngle tiltAngleParam = new TiltABAngle();
				tiltAngleParam.dStart_RA_deg = toolVecModifyMap[ nStartIndex ].RA_deg;
				tiltAngleParam.dStart_RB_deg = toolVecModifyMap[ nStartIndex ].RB_deg;
				tiltAngleParam.dEnd_RA_deg = toolVecModifyMap[ nEndIndex ].RA_deg;
				tiltAngleParam.dEnd_RB_deg = toolVecModifyMap[ nEndIndex ].RB_deg;
				result.Add( tiltAngleParam );
			}
			return result;
		}


		static List<Tuple<int, int>> GetInterpolateIntervalList(
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap,
			bool isClosed )
		{
			// sort the modify data by index
			List<int> indexInOrder = toolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			if( isClosed ) {

				// for closed path, the index is wrapped
				for( int i = 0; i < indexInOrder.Count; i++ ) {
					int nextIndex = ( i + 1 ) % indexInOrder.Count;
					intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ nextIndex ] ) );
				}
			}
			else {
				for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
					intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ i + 1 ] ) );
				}
			}
			return intervalList;
		}

		static void InterpolateToolVecByTilt( ref List<ISetToolVecPoint> toolVecPointList,
			int nStartIndex, int nEndIndex, double dStartRA_Deg, double dStartRB_Deg, double dEndRA_Deg, double dEndRB_Deg )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + toolVecPointList.Count : nEndIndex;

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				totaldistance += toolVecPointList[ i % toolVecPointList.Count ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) % toolVecPointList.Count ].Point );
			}
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += toolVecPointList[ i % toolVecPointList.Count ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) % toolVecPointList.Count ].Point );
				double dInterpRA_rad = dStartRA_Deg + ( dEndRA_Deg - dStartRA_Deg ) * t;
				double dInterpRB_rad = dStartRB_Deg + ( dEndRB_Deg - dStartRB_Deg ) * t;
				gp_Vec ModifyVec = GetVecFromABAngle( toolVecPointList[ i % toolVecPointList.Count ],
					dInterpRA_rad * Math.PI / 180,
					dInterpRB_rad * Math.PI / 180 );
				toolVecPointList[ i % toolVecPointList.Count ].ToolVec = new gp_Dir( ModifyVec );
			}
		}

		static void InterpolateToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			int nStartIndex, int nEndIndex, gp_Vec startVec, gp_Vec endVec )
		{
			// consider wrapped
			int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + toolVecPointList.Count : nEndIndex;

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				totaldistance += toolVecPointList[ i % toolVecPointList.Count ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) % toolVecPointList.Count ].Point );
			}

			// get the quaternion for interpolation
			gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
			gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i < nEndIndexModify; i++ ) {
				double t = accumulatedDistance / totaldistance;
				accumulatedDistance += toolVecPointList[ i % toolVecPointList.Count ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) % toolVecPointList.Count ].Point );
				gp_Quaternion q = new gp_Quaternion();
				slerp.Interpolate( t, ref q );
				gp_Trsf trsf = new gp_Trsf();
				trsf.SetRotation( q );
				toolVecPointList[ i % toolVecPointList.Count ].ToolVec = new gp_Dir( startVec.Transformed( trsf ) );
			}
		}

		static ECalAngleResult GetABAngleToTargetVec( ISetToolVecPoint toolVecPoint, gp_Dir targetDir, out double dRA_deg, out double dRB_deg )
		{
			dRA_deg = 0;
			dRB_deg = 0;
			if( toolVecPoint == null || targetDir == null ) {
				return ECalAngleResult.DataError;
			}
			gp_Dir newCoordX = toolVecPoint.TangentVec;
			gp_Dir newCoordZ = toolVecPoint.InitToolVec;
			gp_Dir newCoordY = newCoordZ.Crossed( newCoordX );

			// check angle limit ( difference is too great )
			bool isTooLargeAngle = IsOverCalAngle( targetDir, newCoordZ );
			if( isTooLargeAngle ) {
				return ECalAngleResult.TooLargeAngle;
			}

			// same direction
			if( targetDir.IsEqual( newCoordZ, RADIUS_TOLERANCE ) ) {
				dRA_deg = 0;
				dRB_deg = 0;
				return ECalAngleResult.Done;
			}
			double dProjectX = targetDir.XYZ().Dot( newCoordX.XYZ() );
			double dProjectY = targetDir.XYZ().Dot( newCoordY.XYZ() );
			double dProjectZ = targetDir.XYZ().Dot( newCoordZ.XYZ() );

			// get dRA_rad
			double dRA_rad;

			// x ≈ 0 , do not with ra_rad
			if( Math.Abs( dProjectX ) < PROJECT_TOLERANCE ) {
				dRA_rad = 0;
			}

			// normal case
			else {
				dRA_rad = Math.Atan2( dProjectX, dProjectZ );
			}

			// get dRB_rad
			double dRB_rad;

			// if y and z ≈ 0, rb = 0 => means target vector is on the x axis
			if( Math.Abs( dProjectY ) < PROJECT_TOLERANCE && Math.Abs( dProjectZ ) < PROJECT_TOLERANCE ) {
				dRB_rad = 0;
			}

			// means project is on xz plane, Y = 0 => RB = 0
			else if( Math.Abs( dProjectY ) < PROJECT_TOLERANCE ) {
				dRB_rad = 0;
			}

			// Y is not 0 but Z ≈ 0, means project is on xy plane, RB ≈ ±90°
			else if( Math.Abs( dProjectZ ) < PROJECT_TOLERANCE ) {
				dRB_rad = ( dProjectY > 0 ) ? -Math.PI / 2 : Math.PI / 2;
			}
			else {
				// Y = Z * Tan(-RB)
				dRB_rad = Math.Atan2( -dProjectY, dProjectZ );
			}

			// rad -> deg
			dRA_deg = dRA_rad * 180 / Math.PI;
			dRB_deg = dRB_rad * 180 / Math.PI;
			return ECalAngleResult.Done;
		}

		static bool IsOverCalAngle( gp_Dir dirA, gp_Dir dirB )
		{
			double dAngleRad = dirA.Angle( dirB );
			double dSingleDeg = dAngleRad * 180 / Math.PI;
			if( dSingleDeg > TOO_LARGE_ANGLE_DEG ) {
				return true;
			}
			return false;
		}

		static void ApplyVectorInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, bool isClosed )
		{
			// all tool vector are modified to the same value, no need to do interpolation
			if( toolVecModifyMap.Count == 1 ) {
				gp_Vec newVec = GetVecFromABAngle( toolVecPointList[ toolVecModifyMap.Keys.First() ],
					toolVecModifyMap.Values.First().RA_deg * Math.PI / 180,
					toolVecModifyMap.Values.First().RB_deg * Math.PI / 180 );
				foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
					toolVecPoint.ToolVec = new gp_Dir( newVec.XYZ() );
				}
				return;
			}

			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap, isClosed );

			// get the control point tool vector for each interval
			List<Tuple<gp_Vec, gp_Vec>> ctrlPntToolVec = GetIntervalToolVec( interpolateIntervalList, toolVecPointList, toolVecModifyMap );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVec( ref toolVecPointList, nStartIndex, nEndIndex, ctrlPntToolVec[ i ].Item1, ctrlPntToolVec[ i ].Item2 );
			}
			return;
		}

		static void ApplyTiltAngleInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, bool isClosed )
		{
			// all tool vector are modified to the same value, no need to do interpolation
			if( toolVecModifyMap.Count == 1 ) {
				double dRA_Deg = toolVecModifyMap.Values.First().RA_deg;
				double dRB_Deg = toolVecModifyMap.Values.First().RB_deg;
				foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
					gp_Vec newVec = GetVecFromABAngle( toolVecPoint,
						dRA_Deg * Math.PI / 180,
						dRB_Deg * Math.PI / 180 );
					toolVecPoint.ToolVec = new gp_Dir( newVec.XYZ() );
				}
				return;
			}

			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap, isClosed );

			// get the control point tool vector for each interval
			List<TiltABAngle> ctrlPntToolVec = GetABAngleFromInterval( interpolateIntervalList, toolVecModifyMap );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVecByTilt( ref toolVecPointList, nStartIndex, nEndIndex,
					ctrlPntToolVec[ i ].dStart_RA_deg, ctrlPntToolVec[ i ].dStart_RB_deg,
					ctrlPntToolVec[ i ].dEnd_RA_deg, ctrlPntToolVec[ i ].dEnd_RB_deg );
			}
		}

		const double TOO_LARGE_ANGLE_DEG = 60.0;
		const double PROJECT_TOLERANCE = 1e-3;
		const double RADIUS_TOLERANCE = 1e-3;

		// information for tilt angle interpolation
		struct TiltABAngle
		{
			public double dStart_RA_deg;
			public double dStart_RB_deg;
			public double dEnd_RA_deg;
			public double dEnd_RB_deg;
		}
	}
}
