using MyCAM.Data;
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
		IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
		bool isClosed, bool isToolVecReverse, EToolVecInterpolateType interpolateType, gp_Ax1 refCenterDir, out int mod )
	{
		// mark the modified point
		for( int i = 0; i < toolVecPointList.Count; i++ ) {
			if( !toolVecModifyMap.ContainsKey( i ) ) {
				continue;
			}
			toolVecPointList[ i ].IsToolVecModPoint = true;
		}
		ModifyToolVec( ref toolVecPointList, toolVecModifyMap, isClosed, isToolVecReverse, interpolateType, refCenterDir, out mod );
	}

		public static ECalAngleResult GetABAngleToTargetVec( gp_Dir assignDir, ISetToolVecPoint toModifyPnt, bool isToolVecReverse, out Tuple<double, double> param )
		{
			ECalAngleResult CalResult = GetABAngleToTargetVec( toModifyPnt, assignDir, isToolVecReverse, out double dRA_rad, out double dRB_rad );
			param = new Tuple<double, double>( dRA_rad, dRB_rad );
			return CalResult;
		}

		public static gp_Vec GetVecFromABAngle( ISetToolVecPoint toolVecPoint, double dRA_rad, double dRB_rad, bool isToolVecVerse )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				if( isToolVecVerse ) {
					return new gp_Vec( toolVecPoint.ToolVec.Reversed() );
				}
				return new gp_Vec( toolVecPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = toolVecPoint.TangentVec;
			gp_Dir z = isToolVecVerse ? toolVecPoint.InitToolVec.Reversed() : toolVecPoint.InitToolVec;
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

		static void ReverseNorVec( ref List<ISetToolVecPoint> toolVecPointList )
		{
			foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
				toolVecPoint.ToolVec = toolVecPoint.ToolVec.Reversed();
			}
		}

	static void ModifyToolVec( ref List<ISetToolVecPoint> toolVecPointList,
		IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
		bool isClosed, bool isToolVecReverse, EToolVecInterpolateType interpolateType, gp_Ax1 refCenterDir, out int mod )
	{
		mod = 0;
		if( interpolateType == EToolVecInterpolateType.FixedDir ) {
			foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
				if( isToolVecReverse ) {
					toolVecPoint.ToolVec = refCenterDir.Direction().Reversed();
				}
				else {
					toolVecPoint.ToolVec = refCenterDir.Direction();
				}
			}
			return;
		}
		if( toolVecModifyMap.Count == 0 ) {
			if( isToolVecReverse ) {
				ReverseNorVec( ref toolVecPointList );
			}
			return;
		}
		if( interpolateType == EToolVecInterpolateType.VectorInterpolation ) {
			ApplyVectorInterpolation( ref toolVecPointList, toolVecModifyMap, isToolVecReverse, isClosed, out mod );
			return;
		}
		if( interpolateType == EToolVecInterpolateType.TiltAngleInterpolation ) {
			ApplyTiltAngleInterpolation( ref toolVecPointList, toolVecModifyMap, isToolVecReverse, isClosed );
			return;
		}
	}

		static List<Tuple<gp_Vec, gp_Vec>> GetIntervalToolVec( List<Tuple<int, int>> interpolateIntervalList, List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap, bool isToolVecReverse )
		{
			List<Tuple<gp_Vec, gp_Vec>> result = new List<Tuple<gp_Vec, gp_Vec>>();
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				gp_Vec startVec = GetVecFromABAngle( toolVecPointList[ nStartIndex ],
					toolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
					toolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180,
					isToolVecReverse );
				gp_Vec endVec = GetVecFromABAngle( toolVecPointList[ nEndIndex ],
					toolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
					toolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180,
					isToolVecReverse );
				result.Add( new Tuple<gp_Vec, gp_Vec>( startVec, endVec ) );
			}
			return result;
		}

		static List<TiltABAngle> GetABAngleFromInterval( List<Tuple<int, int>> interpolateIntervalList, IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap )
		{
			List<TiltABAngle> result = new List<TiltABAngle>();
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				int nStartIndex = interpolateIntervalList[ i ].Item1;
				int nEndIndex = interpolateIntervalList[ i ].Item2;
				TiltABAngle tiltAngleParam = new TiltABAngle();
				tiltAngleParam.dStart_RA_deg = toolVecModifyMap[ nStartIndex ].Item1;
				tiltAngleParam.dStart_RB_deg = toolVecModifyMap[ nStartIndex ].Item2;
				tiltAngleParam.dEnd_RA_deg = toolVecModifyMap[ nEndIndex ].Item1;
				tiltAngleParam.dEnd_RB_deg = toolVecModifyMap[ nEndIndex ].Item2;
				result.Add( tiltAngleParam );
			}
			return result;
		}


		static List<Tuple<int, int>> GetInterpolateIntervalList(
			IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap,
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

		static void InterpolateToolVecByAxisPosition( ref List<ISetToolVecPoint> toolVecPointList,
			int nStartIndex, int nEndIndex, double dStartM, double dStartS, double dEndM, double dEndS )
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

				// Interpolate axis positions independently
				double dInterpM = dStartM + ( dEndM - dStartM ) * t;
				double dInterpS = dStartS + ( dEndS - dStartS ) * t;

				// Solve forward kinematics for A-C-spindle type
				// Initial vector: [0, 0, 1]
				// First rotation: rotate around Z-axis by C-axis angle (dInterpM)
				// Second rotation: rotate around X-axis by A-axis angle (dInterpS)
				gp_Vec toolVec = SolveACSpindleFK( dInterpM, dInterpS );
				toolVecPointList[ i % toolVecPointList.Count ].ToolVec = new gp_Dir( toolVec );
				toolVecPointList[ i % toolVecPointList.Count ].Master = dInterpM;
				toolVecPointList[ i % toolVecPointList.Count ].Slave = dInterpS;
			}
		}

		static gp_Vec SolveACSpindleFK( double dC_rad, double dA_rad )
		{
			// Initial tool vector pointing in Z direction [0, 0, 1]
			gp_Vec initVec = new gp_Vec( 0, 0, 1 );

			// Second rotation: rotate around X-axis by A-axis angle
			gp_Ax1 xAxis = new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 1, 0, 0 ) );
			gp_Trsf trsfA = new gp_Trsf();
			trsfA.SetRotation( xAxis, dA_rad );
			gp_Vec vecAfterA = initVec.Transformed( trsfA );

			// First rotation: rotate around Z-axis by C-axis angle
			gp_Ax1 zAxis = new gp_Ax1( new gp_Pnt( 0, 0, 0 ), new gp_Dir( 0, 0, 1 ) );
			gp_Trsf trsfC = new gp_Trsf();
			trsfC.SetRotation( zAxis, dC_rad );
			gp_Vec finalVec = vecAfterA.Transformed( trsfC );

			return finalVec;
		}

		static void InterpolateToolVecByTilt( ref List<ISetToolVecPoint> toolVecPointList,
			int nStartIndex, int nEndIndex, double dStartRA_Deg, double dStartRB_Deg, double dEndRA_Deg, double dEndRB_Deg, bool isToolVecReverse )
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
					dInterpRB_rad * Math.PI / 180,
					isToolVecReverse );
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

		static ECalAngleResult GetABAngleToTargetVec( ISetToolVecPoint toolVecPoint, gp_Dir targetDir, bool isToolVecReverse, out double dRA_deg, out double dRB_deg )
		{
			dRA_deg = 0;
			dRB_deg = 0;
			if( toolVecPoint == null || targetDir == null ) {
				return ECalAngleResult.DataError;
			}
			gp_Dir newCoordX = toolVecPoint.TangentVec;
			gp_Dir newCoordZ = isToolVecReverse ? toolVecPoint.InitToolVec.Reversed() : toolVecPoint.InitToolVec;
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

	static void ApplyVectorInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap, bool isToolVecReverse, bool isClosed, out int mod )
	{
		mod = 0;
		// all tool vector are modified to the same value, no need to do interpolation
		if( toolVecModifyMap.Count == 1 ) {
			gp_Vec newVec = SolveACSpindleFK(
				toolVecModifyMap.Values.First().Item1 * Math.PI / 180,
				toolVecModifyMap.Values.First().Item2 * Math.PI / 180 );
			foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
				toolVecPoint.ToolVec = new gp_Dir( newVec.XYZ() );
				toolVecPoint.Master = toolVecModifyMap.Values.First().Item1 * Math.PI / 180;
				toolVecPoint.Slave = toolVecModifyMap.Values.First().Item2 * Math.PI / 180;
			}
			return;
		}

		// get the interpolate interval list
		List<Tuple<int, int>> interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap, isClosed );

		// calculate mod for closed path based on last interval
		if( isClosed && interpolateIntervalList.Count > 0 ) {
			int lastIntervalIndex = interpolateIntervalList.Count - 1;
			int nStartIndex = interpolateIntervalList[ lastIntervalIndex ].Item1;
			int nEndIndex = interpolateIntervalList[ lastIntervalIndex ].Item2;

			// get master angles in degrees
			double dStartM_deg = toolVecModifyMap[ nStartIndex ].Item1;
			double dEndM_deg = toolVecModifyMap[ nEndIndex ].Item1;

			// find mod that minimizes |X1 - Y| where X1 = X + mod * 180
			// try all possible mod values: -2, -1, 0, 1, 2
			int bestMod = 0;
			double minDiff = Math.Abs( dEndM_deg - dStartM_deg );
			
			int[] modCandidates = { -2, -1, 0, 1, 2 };
			foreach( int modCandidate in modCandidates ) {
				double X1 = dEndM_deg + modCandidate * 180;
				double diff = Math.Abs( X1 - dStartM_deg );
				if( diff < minDiff ) {
					minDiff = diff;
					bestMod = modCandidate;
				}
			}
			mod = bestMod;
		}

		// modify the tool vector by interpolating axis positions
		for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
			// get start and end index
			int nStartIndex = interpolateIntervalList[ i ].Item1;
			int nEndIndex = interpolateIntervalList[ i ].Item2;

			// get axis positions in degrees, convert to radians
			double dStartM = toolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180;
			double dStartS = toolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180;
			double dEndM = toolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180;
			double dEndS = toolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180;

			// check if this is the last interval (wraps back to first index)
			bool isLastInterval = isClosed && ( i == interpolateIntervalList.Count - 1 );
			if( isLastInterval ) {
				// adjust end point axis positions for master rotation
				dEndM += mod * Math.PI;
				// if mod is odd, negate slave
				if( mod % 2 != 0 ) {
					dEndS = -dEndS;
				}
			}

			InterpolateToolVecByAxisPosition( ref toolVecPointList, nStartIndex, nEndIndex,
				dStartM, dStartS, dEndM, dEndS );
		}
		return;
	}

	static void ApplyTiltAngleInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap, bool isToolVecReverse, bool isClosed )
	{
		// all tool vector are modified to the same value, no need to do interpolation
		if( toolVecModifyMap.Count == 1 ) {
			double dRA_Deg = toolVecModifyMap.Values.First().Item1;
			double dRB_Deg = toolVecModifyMap.Values.First().Item2;
			foreach( ISetToolVecPoint toolVecPoint in toolVecPointList ) {
				gp_Vec newVec = GetVecFromABAngle( toolVecPoint,
					dRA_Deg * Math.PI / 180,
					dRB_Deg * Math.PI / 180,
					isToolVecReverse );
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
				ctrlPntToolVec[ i ].dEnd_RA_deg, ctrlPntToolVec[ i ].dEnd_RB_deg,
				isToolVecReverse );
		}
	}

		const double TOO_LARGE_ANGLE_DEG = 999;
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
