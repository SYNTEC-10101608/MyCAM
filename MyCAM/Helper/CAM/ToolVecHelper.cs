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
			Dictionary<int, ToolVecModifyData> toolVecModifyMap, bool isClosed, EToolVecInterpolateType interpolateType )
		{
			// arrange the map for closed path
			if( isClosed ) {
				ArrageMapForClosedPath( ref toolVecModifyMap, toolVecPointList );
			}
			else {
				ArrangeMapForOpenPath( ref toolVecModifyMap, toolVecPointList );
			}

			// mark the modified point
			for( int i = 0; i < toolVecPointList.Count; i++ ) {
				if( !toolVecModifyMap.ContainsKey( i ) ) {
					continue;
				}
				toolVecPointList[ i ].IsToolVecModPoint = true;
			}
			ModifyToolVec( ref toolVecPointList, toolVecModifyMap, interpolateType );
		}

		public static ECalAngleResult GetABAngleFromToolVec( gp_Dir targetDir, ISetToolVecPoint toolVecPoint, out Tuple<double, double> abAngle_deg )
		{
			abAngle_deg = new Tuple<double, double>( 0, 0 );
			if( toolVecPoint == null || targetDir == null ) {
				return ECalAngleResult.DataError;
			}
			gp_Dir newCoordX = toolVecPoint.TangentVec;
			gp_Dir newCoordZ = toolVecPoint.InitToolVec;
			gp_Dir newCoordY = newCoordZ.Crossed( newCoordX );

			// check angle limit ( difference is too great )
			bool isTooLargeAngle = IsOverCalAngle( targetDir, newCoordZ );
			if( isTooLargeAngle ) {

				// we return a big value for process to identify
				abAngle_deg = new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
				return ECalAngleResult.TooLargeAngle;
			}

			// same direction
			if( targetDir.IsEqual( newCoordZ, RADIUS_TOLERANCE ) ) {
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
			abAngle_deg = new Tuple<double, double>( dRA_rad * 180.0 / Math.PI, dRB_rad * 180.0 / Math.PI );
			return ECalAngleResult.Done;
		}

		public static Tuple<double, double> GetMSAngleFromABAngle( double dRA_deg, double dRB_deg, ISetToolVecPoint toolVecPoint )
		{
			// Convert A/B angles to tool vector
			gp_Dir toolVec = ConvertABAngleToToolVec( toolVecPoint, dRA_deg * Math.PI / 180.0, dRB_deg * Math.PI / 180.0 );
			if( toolVec == null ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// get M/S angles from tool vector
			return GetMSAngleFromToolVec( toolVec, toolVecPoint );
		}

		public static Tuple<double, double> GetABAngleFromMSAngle( double dMaster_deg, double dSlave_deg, ISetToolVecPoint toolVecPoint )
		{
			// Use helper to convert MS to ToolVec
			gp_Dir toolVec = ConvertMSAngleToToolVec( dMaster_deg, dSlave_deg );
			if( toolVec == null ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// get A/B angles from tool vector
			ECalAngleResult result = GetABAngleFromToolVec( toolVec, toolVecPoint, out Tuple<double, double> abAngle_deg );
			return abAngle_deg;
		}

		static void ModifyToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap,
			EToolVecInterpolateType interpolateType )
		{
			if( toolVecModifyMap.Count == 0 ) {
				return;
			}
			if( interpolateType == EToolVecInterpolateType.VectorInterpolation ) {
				ApplyMSAngleInterpolation( ref toolVecPointList, toolVecModifyMap );
				return;
			}
			if( interpolateType == EToolVecInterpolateType.TiltAngleInterpolation ) {
				ApplyTiltAngleInterpolation( ref toolVecPointList, toolVecModifyMap );
				return;
			}
		}

		static List<MSAngle> GetIntervalMSAngles( List<Tuple<int, int>> interpolateIntervalList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			List<MSAngle> result = new List<MSAngle>();
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				MSAngle msAngleParam = new MSAngle
				{
					dStart_Master_deg = toolVecModifyMap[ nStartIndex ].Master_deg,
					dStart_Slave_deg = toolVecModifyMap[ nStartIndex ].Slave_deg,
					dEnd_Master_deg = toolVecModifyMap[ nEndIndex ].Master_deg,
					dEnd_Slave_deg = toolVecModifyMap[ nEndIndex ].Slave_deg
				};
				result.Add( msAngleParam );
			}
			return result;
		}

		static List<TiltABAngle> GetIntervalABAngles( List<Tuple<int, int>> interpolateIntervalList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
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
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			// sort the modify data by index
			List<int> indexInOrder = toolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			List<Tuple<int, int>> intervalList = new List<Tuple<int, int>>();
			for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
				intervalList.Add( new Tuple<int, int>( indexInOrder[ i ], indexInOrder[ i + 1 ] ) );
			}
			return intervalList;
		}

		static void InterpolateToolVecByTilt( ref List<ISetToolVecPoint> toolVecPointList,
			int nStartIndex, int nEndIndex, double dStartRA_Deg, double dStartRB_Deg, double dEndRA_Deg, double dEndRB_Deg )
		{
			if( nEndIndex <= nStartIndex ) {
				return;
			}

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndex; i++ ) {
				if( i >= toolVecPointList.Count - 1 ) {
					break;
				}
				totaldistance += toolVecPointList[ i ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) ].Point );
			}

			// interpolate A/B angles and convert to tool vector
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i <= nEndIndex; i++ ) {
				double t = accumulatedDistance / totaldistance;

				// add accumulated distance if not the last point
				if( i < nEndIndex ) {
					accumulatedDistance += toolVecPointList[ i ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) ].Point );
				}

				// Interpolate A/B angles
				double dInterpRA_rad = dStartRA_Deg + ( dEndRA_Deg - dStartRA_Deg ) * t;
				double dInterpRB_rad = dStartRB_Deg + ( dEndRB_Deg - dStartRB_Deg ) * t;

				// Convert A/B to ToolVec
				gp_Dir ModifyVec = ConvertABAngleToToolVec( toolVecPointList[ i ],
					dInterpRA_rad * Math.PI / 180,
					dInterpRB_rad * Math.PI / 180 );

				// get MS angles from tool vector
				Tuple<double, double> msAngle_deg = GetMSAngleFromToolVec( ModifyVec, toolVecPointList[ i ] );

				// set the modified data
				toolVecPointList[ i ].ToolVec = ModifyVec;
				toolVecPointList[ i ].ModMaster_rad = msAngle_deg.Item1 * Math.PI / 180.0;
				toolVecPointList[ i ].ModSlave_rad = msAngle_deg.Item2 * Math.PI / 180.0;
			}
		}

		static void InterpolateToolVecByMS( ref List<ISetToolVecPoint> toolVecPointList,
				int nStartIndex, int nEndIndex, double dStartMaster_deg, double dStartSlave_deg, double dEndMaster_deg, double dEndSlave_deg )
		{
			if( nEndIndex <= nStartIndex ) {
				return;
			}

			// get the total distance for interpolation parameter
			double totaldistance = 0;
			for( int i = nStartIndex; i < nEndIndex; i++ ) {
				if( i >= toolVecPointList.Count - 1 ) {
					break;
				}
				totaldistance += toolVecPointList[ i ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) ].Point );
			}

			// interpolate master/slave angles and convert to tool vector
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i <= nEndIndex; i++ ) {
				double t = accumulatedDistance / totaldistance;

				// add accumulated distance if not the last point
				if( i < nEndIndex ) {
					accumulatedDistance += toolVecPointList[ i ].Point.SquareDistance( toolVecPointList[ ( i + 1 ) ].Point );
				}

				// Interpolate M/S angles
				double dInterpMaster_deg = dStartMaster_deg + ( dEndMaster_deg - dStartMaster_deg ) * t;
				double dInterpSlave_deg = dStartSlave_deg + ( dEndSlave_deg - dStartSlave_deg ) * t;

				// Convert M/S to ToolVec
				gp_Dir toolVec = ConvertMSAngleToToolVec( dInterpMaster_deg, dInterpSlave_deg );
				if( toolVec != null ) {
					toolVecPointList[ i ].ToolVec = toolVec;
					toolVecPointList[ i ].ModMaster_rad = dInterpMaster_deg * Math.PI / 180.0;
					toolVecPointList[ i ].ModSlave_rad = dInterpSlave_deg * Math.PI / 180.0;
				}
			}
		}

		static bool IsOverCalAngle( gp_Dir dirA, gp_Dir dirB )
		{
			double dAngleRad = dirA.Angle( dirB );
			double dSingleDeg = dAngleRad * 180 / Math.PI;
			if( dSingleDeg > MAX_TILTED_ANGLE_DEG ) {
				return true;
			}
			return false;
		}

		static void ApplyMSAngleInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap );

			// get the control point master/slave angles for each interval
			List<MSAngle> ctrlPntMSAngles = GetIntervalMSAngles( interpolateIntervalList, toolVecModifyMap );

			// modify the tool vector
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {

				// get start and end index
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				InterpolateToolVecByMS( ref toolVecPointList, nStartIndex, nEndIndex,
					ctrlPntMSAngles[ i ].dStart_Master_deg, ctrlPntMSAngles[ i ].dStart_Slave_deg,
					ctrlPntMSAngles[ i ].dEnd_Master_deg, ctrlPntMSAngles[ i ].dEnd_Slave_deg );
			}
			return;
		}

		static void ApplyTiltAngleInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			// get the interpolate interval list
			List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap );

			// get the control point tool vector for each interval
			List<TiltABAngle> ctrlPntToolVec = GetIntervalABAngles( interpolateIntervalList, toolVecModifyMap );

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

		static Tuple<double, double> GetMSAngleFromToolVec( gp_Dir toolVec, ISetToolVecPoint toolVecPoint )
		{
			// Get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// Create PostSolver
			PostSolver postSolver = new PostSolver( machineData );
			double dM_In = toolVecPoint.InitMaster_rad;
			double dS_In = toolVecPoint.InitSlave_rad;
			IKSolveResult result = postSolver.SolveIK( toolVec, dM_In, dS_In, out double dMaster_rad, out double dSlave_rad );

			if( result == IKSolveResult.InvalidInput || result == IKSolveResult.NoSolution ) {
				return new Tuple<double, double>( 0, 0 );
			}
			else if( result == IKSolveResult.OutOfRange ) {
				// out of range, but still return the angles
			}

			// Convert radians to degrees
			double dMaster_deg = dMaster_rad * 180.0 / Math.PI;
			double dSlave_deg = dSlave_rad * 180.0 / Math.PI;

			return new Tuple<double, double>( dMaster_deg, dSlave_deg );
		}

		static gp_Dir ConvertMSAngleToToolVec( double dMaster_deg, double dSlave_deg )
		{
			// Get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return null;
			}

			// Create PostSolver
			PostSolver postSolver = new PostSolver( machineData );

			// Convert degrees to radians for FK solver
			double dMaster_rad = dMaster_deg * Math.PI / 180.0;
			double dSlave_rad = dSlave_deg * Math.PI / 180.0;

			// Use PostSolver API to get the tool vector from master/slave angles
			return postSolver.SolveToolVec( dMaster_rad, dSlave_rad );
		}

		static gp_Dir ConvertABAngleToToolVec( ISetToolVecPoint toolVecPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Dir( toolVecPoint.InitToolVec.XYZ() );
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
			return new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
		}

		static void ArrageMapForClosedPath( ref Dictionary<int, ToolVecModifyData> toolVecModifyMap, List<ISetToolVecPoint> toolVecPointList )
		{
			// when we dont have both 0 and CLOSED_POINT_INDEX, we add them in
			if( !toolVecModifyMap.ContainsKey( 0 ) && !toolVecModifyMap.ContainsKey( CLOSED_POINT_INDEX ) ) {
				toolVecModifyMap[ 0 ] = new ToolVecModifyData()
				{
					RA_deg = 0,
					RB_deg = 0,
					Master_deg = toolVecPointList[ 0 ].InitMaster_rad * 180.0 / Math.PI,
					Slave_deg = toolVecPointList[ 0 ].InitSlave_rad * 180.0 / Math.PI
				};
				toolVecModifyMap[ CLOSED_POINT_INDEX ] = new ToolVecModifyData()
				{
					RA_deg = 0,
					RB_deg = 0,
					Master_deg = toolVecPointList[ toolVecPointList.Count - 1 ].InitMaster_rad * 180.0 / Math.PI,
					Slave_deg = toolVecPointList[ toolVecPointList.Count - 1 ].InitSlave_rad * 180.0 / Math.PI
				};
			}

			// when we have only CLOSED_POINT_INDEX, we copy it to index 0
			else if( toolVecModifyMap.ContainsKey( CLOSED_POINT_INDEX ) && !toolVecModifyMap.ContainsKey( 0 ) ) {
				toolVecModifyMap[ 0 ] = toolVecModifyMap[ CLOSED_POINT_INDEX ].Clone();
			}

			// when we have only 0, we copy it to index CLOSED_POINT_INDEX
			else if( !toolVecModifyMap.ContainsKey( CLOSED_POINT_INDEX ) && toolVecModifyMap.ContainsKey( 0 ) ) {
				toolVecModifyMap[ CLOSED_POINT_INDEX ] = toolVecModifyMap[ 0 ].Clone();
			}

			// both 0 and CLOSED_POINT_INDEX exist
			else {
				// do nothing
			}

			// reset CLOSED_POINT_INDEX
			ToolVecModifyData closedPointData = toolVecModifyMap[ CLOSED_POINT_INDEX ];
			toolVecModifyMap.Remove( CLOSED_POINT_INDEX );
			toolVecModifyMap[ toolVecPointList.Count - 1 ] = closedPointData;
		}

		static void ArrangeMapForOpenPath( ref Dictionary<int, ToolVecModifyData> toolVecModifyMap, List<ISetToolVecPoint> toolVecPointList )
		{
			// add index 0 if not exist
			if( !toolVecModifyMap.ContainsKey( 0 ) ) {
				toolVecModifyMap[ 0 ] = new ToolVecModifyData()
				{
					RA_deg = 0,
					RB_deg = 0,
					Master_deg = toolVecPointList[ 0 ].InitMaster_rad * 180.0 / Math.PI,
					Slave_deg = toolVecPointList[ 0 ].InitSlave_rad * 180.0 / Math.PI
				};
			}

			// add index last if not exist
			int lastIndex = toolVecPointList.Count - 1;
			if( !toolVecModifyMap.ContainsKey( lastIndex ) ) {
				toolVecModifyMap[ lastIndex ] = new ToolVecModifyData()
				{
					RA_deg = 0,
					RB_deg = 0,
					Master_deg = toolVecPointList[ lastIndex ].InitMaster_rad * 180.0 / Math.PI,
					Slave_deg = toolVecPointList[ lastIndex ].InitSlave_rad * 180.0 / Math.PI
				};
			}
		}

		const double MAX_TILTED_ANGLE_DEG = 60.0;
		const double BIG_AB_ANGLE = 999.0;
		const double PROJECT_TOLERANCE = 1e-3;
		const double RADIUS_TOLERANCE = 1e-3;

		const int CLOSED_POINT_INDEX = -1;

		// information for tilt angle interpolation
		struct TiltABAngle
		{
			public double dStart_RA_deg;
			public double dStart_RB_deg;
			public double dEnd_RA_deg;
			public double dEnd_RB_deg;
		}

		// information for master/slave interpolation
		struct MSAngle
		{
			public double dStart_Master_deg;
			public double dStart_Slave_deg;
			public double dEnd_Master_deg;
			public double dEnd_Slave_deg;
		}
	}
}
