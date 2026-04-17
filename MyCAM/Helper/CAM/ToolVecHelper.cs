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
		public static void SetToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			Dictionary<int, ToolVecModifyData> toolVecModifyMap, bool isClosed, out List<Tuple<int, int, EToolVecInterpolateType>> interpolateRegionList, bool isPathReverse )
		{
			// arrange the map for closed path
			if( isClosed ) {
				ArrageMapForClosedPath( ref toolVecModifyMap, toolVecPointList );
			}

			// mark the modified point
			for( int i = 0; i < toolVecPointList.Count; i++ ) {
				if( !toolVecModifyMap.ContainsKey( i ) ) {
					continue;
				}
				toolVecPointList[ i ].IsToolVecModPoint = true;
			}
			ModifyToolVec( ref toolVecPointList, toolVecModifyMap, out interpolateRegionList, isPathReverse );
		}

		public static Tuple<double, double> GetMSAngleFromToolVec( gp_Dir toolVec, ISetToolVecPoint toolVecPoint )
		{
			if( toolVec == null || toolVecPoint == null ) {
				return new Tuple<double, double>( 0, 0 );
			}
			double dM_In = toolVecPoint.ModMaster_rad;
			double dS_In = toolVecPoint.ModSlave_rad;

			// Get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return new Tuple<double, double>( 0, 0 );
			}

			// Create PostSolver
			PostSolver postSolver = new PostSolver( machineData );
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

		public static Tuple<double, double> GetABAngleFromToolVec( gp_Dir toolVec, ISetToolVecPoint toolVecPoint )
		{
			if( toolVec == null || toolVecPoint == null ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}
			if( double.IsNaN( toolVec.x ) || double.IsNaN( toolVec.y ) || double.IsNaN( toolVec.z ) ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}
			bool isTooLargeAngle = IsOverCalAngle( toolVec, toolVecPoint.InitToolVec );
			if( isTooLargeAngle ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}
			GetABFromToolVec( toolVec, toolVecPoint, out Tuple<double, double> abAngle_deg );
			return abAngle_deg;
		}

		public static Tuple<double, double> GetMSAngleFromABAngle( double dRA_deg, double dRB_deg, ISetToolVecPoint toolVecPoint )
		{
			if( toolVecPoint == null ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}

			// Convert A/B angles to tool vector
			gp_Dir toolVec = ConvertABAngleToToolVec( toolVecPoint, dRA_deg, dRB_deg );

			if( toolVec == null ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}

			// get M/S angles from tool vector
			return GetMSAngleFromToolVec( toolVec, toolVecPoint );
		}

		public static Tuple<double, double> GetABAngleFromMSAngle( double dMaster_deg, double dSlave_deg, ISetToolVecPoint toolVecPoint )
		{
			if( toolVecPoint == null ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}

			// Use helper to convert MS to ToolVec
			gp_Dir toolVec = ConvertMSAngleToToolVec( dMaster_deg, dSlave_deg );
			Tuple<double, double> abAngle_deg = GetABAngleFromToolVec( toolVec, toolVecPoint );
			return abAngle_deg;
		}

		static void ModifyToolVec( ref List<ISetToolVecPoint> toolVecPointList,
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap,
			out List<Tuple<int, int, EToolVecInterpolateType>> interpolateIntervalList, bool isPathReverse )
		{
			if( toolVecModifyMap.Count == 0 ) {
				interpolateIntervalList = new List<Tuple<int, int, EToolVecInterpolateType>>();
				return;
			}

			// get the interpolate interval list
			interpolateIntervalList = GetInterpolateIntervalList( toolVecModifyMap, isPathReverse );
			bool isWithInterpolate = false;
			if( interpolateIntervalList.Count > 1 ) {
				isWithInterpolate = true;
			}
			SetControlPoint( ref toolVecPointList, interpolateIntervalList, toolVecModifyMap );
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				if( interpolateIntervalList[ i ].Item3 == EToolVecInterpolateType.VectorInterpolation ) {
					ApplyMSAngleInterpolation( ref toolVecPointList, toolVecModifyMap, interpolateIntervalList[ i ].Item1, interpolateIntervalList[ i ].Item2 );
				}
				else if( interpolateIntervalList[ i ].Item3 == EToolVecInterpolateType.Normal ) {
					SolveRegionIK( ref toolVecPointList, toolVecModifyMap, interpolateIntervalList[ i ].Item1, interpolateIntervalList[ i ].Item2, isWithInterpolate );
				}
				else if( interpolateIntervalList[ i ].Item3 == EToolVecInterpolateType.TiltAngleInterpolation ) {
					ApplyTiltAngleInterpolation( ref toolVecPointList, toolVecModifyMap, interpolateIntervalList[ i ].Item1, interpolateIntervalList[ i ].Item2 );
					SolveRegionIK( ref toolVecPointList, toolVecModifyMap, interpolateIntervalList[ i ].Item1, interpolateIntervalList[ i ].Item2, isWithInterpolate );
				}
			}
			SetControlPoint( ref toolVecPointList, interpolateIntervalList, toolVecModifyMap );

		}

		static void SetControlPoint( ref List<ISetToolVecPoint> toolVecPointList, List<Tuple<int, int, EToolVecInterpolateType>> interpolateIntervalList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap )
		{
			if( interpolateIntervalList == null || interpolateIntervalList.Count == 0 ) {
				return;
			}
			for( int i = 0; i < interpolateIntervalList.Count; i++ ) {
				double dStart_Master_deg = toolVecModifyMap[ interpolateIntervalList[ i ].Item1 ].Master_deg;
				double dStart_Slave_deg = toolVecModifyMap[ interpolateIntervalList[ i ].Item1 ].Slave_deg;
				gp_Dir toolVec = ConvertMSAngleToToolVec( dStart_Master_deg, dStart_Slave_deg );
				if( toolVec != null ) {
					toolVecPointList[ interpolateIntervalList[ i ].Item1 ].ToolVec = toolVec;
					toolVecPointList[ interpolateIntervalList[ i ].Item1 ].ModMaster_rad = dStart_Master_deg * Math.PI / 180.0;
					toolVecPointList[ interpolateIntervalList[ i ].Item1 ].ModSlave_rad = dStart_Slave_deg * Math.PI / 180.0;
				}
			}

			// last control point
			int lastIndex = interpolateIntervalList[ interpolateIntervalList.Count - 1 ].Item2;
			double dEnd_Master_deg = toolVecModifyMap[ lastIndex ].Master_deg;
			double dEnd_Slave_deg = toolVecModifyMap[ lastIndex ].Slave_deg;
			gp_Dir lastToolVec = ConvertMSAngleToToolVec( dEnd_Master_deg, dEnd_Slave_deg );
			if( lastToolVec != null ) {
				toolVecPointList[ toolVecPointList.Count - 1 ].ToolVec = lastToolVec;
				toolVecPointList[ toolVecPointList.Count - 1 ].ModMaster_rad = dEnd_Master_deg * Math.PI / 180.0;
				toolVecPointList[ toolVecPointList.Count - 1 ].ModSlave_rad = dEnd_Slave_deg * Math.PI / 180.0;
			}

		}


		public static List<Tuple<int, int, EToolVecInterpolateType>> GetInterpolateIntervalList(
			IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, bool isPathReverse )
		{
			// sort the modify data by index
			List<int> indexInOrder = toolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();
			List<Tuple<int, int, EToolVecInterpolateType>> intervalList = new List<Tuple<int, int, EToolVecInterpolateType>>();
			for( int i = 0; i < indexInOrder.Count - 1; i++ ) {
				if( isPathReverse ) {
					intervalList.Add( new Tuple<int, int, EToolVecInterpolateType>( indexInOrder[ i ], indexInOrder[ i + 1 ], toolVecModifyMap[ indexInOrder[ i ] ].InterpolateType ) );
				}
				else {
					intervalList.Add( new Tuple<int, int, EToolVecInterpolateType>( indexInOrder[ i ], indexInOrder[ i + 1 ], toolVecModifyMap[ indexInOrder[ i + 1 ] ].InterpolateType ) );
				}

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
				totaldistance += toolVecPointList[ i ].Point.Distance( toolVecPointList[ ( i + 1 ) ].Point );
			}

			// interpolate A/B angles and convert to tool vector
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i <= nEndIndex; i++ ) {
				double t = accumulatedDistance / totaldistance;

				// add accumulated distance if not the last point
				if( i < nEndIndex ) {
					accumulatedDistance += toolVecPointList[ i ].Point.Distance( toolVecPointList[ ( i + 1 ) ].Point );
				}

				// Interpolate A/B angles
				double dInterpRA_Deg = dStartRA_Deg + ( dEndRA_Deg - dStartRA_Deg ) * t;
				double dInterpRB_Deg = dStartRB_Deg + ( dEndRB_Deg - dStartRB_Deg ) * t;

				// Convert A/B to ToolVec
				gp_Dir ModifyVec = ConvertABAngleToToolVec( toolVecPointList[ i ],
					dInterpRA_Deg,
					dInterpRB_Deg );

				// set the modified data
				toolVecPointList[ i ].ToolVec = ModifyVec;
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
				totaldistance += toolVecPointList[ i ].Point.Distance( toolVecPointList[ ( i + 1 ) ].Point );
			}

			// interpolate master/slave angles and convert to tool vector
			double accumulatedDistance = 0;
			for( int i = nStartIndex; i <= nEndIndex; i++ ) {
				double t = accumulatedDistance / totaldistance;

				// add accumulated distance if not the last point
				if( i < nEndIndex ) {
					accumulatedDistance += toolVecPointList[ i ].Point.Distance( toolVecPointList[ ( i + 1 ) ].Point );
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
			// rad will be in 0~PI
			double dAngleRad = dirA.Angle( dirB );
			double dSingleDeg = dAngleRad * 180 / Math.PI;
			if( dSingleDeg - MAX_TILTED_ANGLE_DEG > GEOM_TOLERANCE ) {
				return true;
			}
			return false;
		}

		static void ApplyMSAngleInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, int startIndex, int endIndex )
		{
			MSAngle msStartAngleParam = new MSAngle
			{
				dStart_Master_deg = toolVecModifyMap[ startIndex ].Master_deg,
				dStart_Slave_deg = toolVecModifyMap[ startIndex ].Slave_deg,
				dEnd_Master_deg = toolVecModifyMap[ endIndex ].Master_deg,
				dEnd_Slave_deg = toolVecModifyMap[ endIndex ].Slave_deg
			};

			InterpolateToolVecByMS( ref toolVecPointList, startIndex, endIndex,
							 msStartAngleParam.dStart_Master_deg, msStartAngleParam.dStart_Slave_deg,
							 msStartAngleParam.dEnd_Master_deg, msStartAngleParam.dEnd_Slave_deg );
			return;
		}

		static void ApplyTiltAngleInterpolation( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, int startIndex, int endIndex )
		{
			TiltABAngle tiltABAngle = new TiltABAngle
			{
				dStart_RA_deg = toolVecModifyMap[ startIndex ].RA_deg,
				dStart_RB_deg = toolVecModifyMap[ startIndex ].RB_deg,
				dEnd_RA_deg = toolVecModifyMap[ endIndex ].RA_deg,
				dEnd_RB_deg = toolVecModifyMap[ endIndex ].RB_deg
			};
			InterpolateToolVecByTilt( ref toolVecPointList, startIndex, endIndex,
					tiltABAngle.dStart_RA_deg, tiltABAngle.dStart_RB_deg,
					tiltABAngle.dEnd_RA_deg, tiltABAngle.dEnd_RB_deg );
		}

		static void SolveRegionIK( ref List<ISetToolVecPoint> toolVecPointList, IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, int nStartIdx, int nEndIdx, bool isWithInterpolate )
		{
			// Get machine data
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return;
			}

			// Create PostSolver
			PostSolver postSolver = new PostSolver( machineData );
			double dM = toolVecModifyMap[ nStartIdx ].Master_deg * Math.PI / 180.0;
			double dS = toolVecModifyMap[ nStartIdx ].Slave_deg * Math.PI / 180.0;

			// sigularity tag list
			List<bool> singularTagList = new List<bool>();
			nEndIdx = isWithInterpolate ? nEndIdx - 1 : nEndIdx;
			for( int i = nStartIdx; i <= nEndIdx; i++ ) {

				IKSolveResult result = postSolver.SolveIK( toolVecPointList[ i ].ToolVec, dM, dS, out dM, out dS );
				if( result == IKSolveResult.InvalidInput || result == IKSolveResult.NoSolution ) {
					continue;
				}
				else if( result == IKSolveResult.OutOfRange ) {
					// out of range, but still set the angles
				}

				// check singularity
				if( result == IKSolveResult.MasterInfinityOfSolution || result == IKSolveResult.SlaveInfinityOfSolution ) {
					singularTagList.Add( true );
				}
				else {
					singularTagList.Add( false );
				}
				toolVecPointList[ i ].ModMaster_rad = dM;
				toolVecPointList[ i ].ModSlave_rad = dS;
			}

			// filter singular points and apply results directly to toolVecPointList
			bool bFilterMaster = machineData.FiveAxisType == FiveAxisType.Spindle; // which axis is going to filter


			// 我要拿 toolVecPointList[ nStartIdx]~[nEndIdx]的指標
			int range = isWithInterpolate ? nEndIdx - nStartIdx + 2 : nEndIdx - nStartIdx + 1;
			List<ISetToolVecPoint> toolVecPointList2 = toolVecPointList.GetRange( nStartIdx, range );
			if( isWithInterpolate ) {
				singularTagList.Add( false );
			}

			FilterSingularPoints( ref toolVecPointList2, singularTagList, bFilterMaster );
		}

		static void FilterSingularPoints( ref List<ISetToolVecPoint> pointList, List<bool> singularTagList, bool bFilterMaster )
		{
			const int FIRST_INDEX = 0;
			if( pointList == null || singularTagList == null ||
				pointList.Count == 0 || pointList.Count != singularTagList.Count ) {
				return;
			}
			int nPathPntCount = singularTagList.Count;
			int i = 0;

			// find singular regions and interpolate
			while( i < nPathPntCount ) {

				// skip non-singular points or points with IsToolVecModPoint == true
				if( !singularTagList[ i ] || pointList[ i ].IsToolVecModPoint ) {
					i++;
					continue;
				}

				// found a singular point, find the range [regionStart, regionEnd]
				int regionStart = i;
				int regionEnd = i;

				// extend to find the complete singular region (singular and not IsToolVecModPoint)
				// 1. did not exceed the last point
				// 2. is singular point
				// 3. is not IsToolVecModPoint
				while( regionEnd < nPathPntCount && singularTagList[ regionEnd ] && !pointList[ regionEnd ].IsToolVecModPoint ) {
					regionEnd++;
				}
				regionEnd--; // back to last singular point

				// determine start values for interpolation
				double startM, startS;
				if( regionStart == FIRST_INDEX ) {
					// rule 3: if first point is singular, use its value
					startM = pointList[ FIRST_INDEX ].ModMaster_rad;
					startS = pointList[ FIRST_INDEX ].ModSlave_rad;
				}
				else {
					// use n-1 value
					startM = pointList[ regionStart - 1 ].ModMaster_rad;
					startS = pointList[ regionStart - 1 ].ModSlave_rad;
				}

				// determine end values for interpolation
				double endM, endS;
				if( regionEnd == nPathPntCount - 1 ) {
					// rule 4: if last point is singular, use its value
					endM = pointList[ nPathPntCount - 1 ].ModMaster_rad;
					endS = pointList[ nPathPntCount - 1 ].ModSlave_rad;
				}
				else {
					// use m+1 value
					endM = pointList[ regionEnd + 1 ].ModMaster_rad;
					endS = pointList[ regionEnd + 1 ].ModSlave_rad;
				}

				// calculate cumulative path lengths for interpolation
				List<double> cumulativeLength = new List<double>();
				double totalLength = 0.0;
				for( int j = regionStart; j <= regionEnd; j++ ) {

					// first singular pnt is start point && this point is interpolation pnt
					if( j == FIRST_INDEX ) {
						cumulativeLength.Add( 0.0 );
					}
					else {
						gp_Pnt p1 = pointList[ j - 1 ].Point;
						gp_Pnt p2 = pointList[ j ].Point;
						double segmentLength = p1.Distance( p2 );
						totalLength += segmentLength;
						cumulativeLength.Add( totalLength );
					}
				}

				// regionEnd is not the last point of this path
				if( regionEnd != nPathPntCount - 1 ) {
					gp_Pnt p1 = pointList[ regionEnd ].Point;
					gp_Pnt p2 = pointList[ regionEnd + 1 ].Point;
					double segmentLength = p1.Distance( p2 );
					totalLength += segmentLength;
				}

				// linear interpolation along path length and apply directly to pointList
				for( int j = regionStart; j <= regionEnd; j++ ) {
					double t = ( totalLength > GEOM_TOLERANCE ) ? ( cumulativeLength[ j - regionStart ] / totalLength ) : 0.0;
					double interpolatedM = startM + t * ( endM - startM );
					double interpolatedS = startS + t * ( endS - startS );

					// interpolate the axis with infinity solution only
					pointList[ j ].ModMaster_rad = bFilterMaster ? interpolatedM : pointList[ j ].ModMaster_rad;
					pointList[ j ].ModSlave_rad = bFilterMaster ? pointList[ j ].ModSlave_rad : interpolatedS;
				}

				// move to next region
				i = regionEnd + 1;
			}
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

		static gp_Dir ConvertABAngleToToolVec( ISetToolVecPoint toolVecPoint, double dRA_Deg, double dRB_Deg )
		{
			// RA = 0, RB = 0 do not need to calculate
			if( Math.Abs( dRA_Deg ) < GEOM_TOLERANCE && Math.Abs( dRB_Deg ) < GEOM_TOLERANCE ) {
				return new gp_Dir( toolVecPoint.InitToolVec.XYZ() );
			}

			// cal dir at Tool Vec coordinate system based on A/B angles
			gp_Dir localDir = ComputeToolVecDirFromAB( dRA_Deg, dRB_Deg );

			// turn the local dir to world coordinate system
			return ConvertToolVecCoordDirToWorld( toolVecPoint, localDir );
		}

		static void GetABFromToolVec( gp_Dir targetDir, ISetToolVecPoint toolVecPoint, out Tuple<double, double> abAngle_deg )
		{
			abAngle_deg = new Tuple<double, double>( 0, 0 );
			if( toolVecPoint == null || targetDir == null ) {

				// we return a big value for process to identify
				abAngle_deg = new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}

			// turn the target dir (world coord) to ToolVec coordinate system
			gp_Dir toolVecCoordDir = ConvertWorldDirToToolVecCoord( targetDir, toolVecPoint );

			// compute A/B angles based on the toolVecCoordDir
			abAngle_deg = ComputeABFromToolVecCoordDir( toolVecCoordDir );
		}

		// the result dir is on ToolVec local coordinate
		static gp_Dir ComputeToolVecDirFromAB( double dRA_Deg, double dRB_Deg )
		{
			double dRA_rad = dRA_Deg * Math.PI / 180.0;
			double dRB_rad = dRB_Deg * Math.PI / 180.0;

			// RA = 0, RB = 0 
			if( Math.Abs( dRA_rad ) < GEOM_TOLERANCE && Math.Abs( dRB_rad ) < GEOM_TOLERANCE ) {
				return new gp_Dir( 0.0, 0.0, 1.0 );
			}

			// local (X,Y,Z) in gx3
			double X = 0.0;
			double Y = 0.0;
			double Z = 0.0;

			// check is ToolVec result will on XY plane
			bool bRAis90 = Math.Abs( Math.Abs( dRA_rad ) - Math.PI / 2.0 ) < GEOM_TOLERANCE;
			bool bRBis90 = Math.Abs( Math.Abs( dRB_rad ) - Math.PI / 2.0 ) < GEOM_TOLERANCE;

			// on XY plane => result will be like(X,Y,0)
			if( bRAis90 || bRBis90 ) {

				// x-component
				if( bRAis90 ) {
					X = dRA_rad > 0 ? POSITIVE_DIR : NEGATIVE_DIR;
				}
				else {

					// calculate the percentage to 90 degree
					double percentage = Math.Abs( dRA_rad ) < GEOM_TOLERANCE ? 0.0 : dRA_rad / ( Math.PI / 2.0 );
					X = ( dRA_rad > 0 ? POSITIVE_DIR : NEGATIVE_DIR ) * Math.Abs( percentage );
				}

				// y-component
				if( bRBis90 ) {
					Y = dRB_rad > 0 ? NEGATIVE_DIR : POSITIVE_DIR;
				}
				else {
					double percentage = Math.Abs( dRB_rad ) < GEOM_TOLERANCE ? 0.0 : dRB_rad / ( Math.PI / 2.0 );
					Y = ( dRB_rad > 0 ? NEGATIVE_DIR : POSITIVE_DIR ) * Math.Abs( percentage );
				}
				Z = 0.0;
				return new gp_Dir( X, Y, Z );
			}

			// simple case: not on XY plane, calculate the local direction based on the tangent of angles
			// X:Y:Z = tanA:tanB:1
			if( Math.Abs( dRA_rad ) < GEOM_TOLERANCE ) {
				if( Math.Abs( dRB_rad ) > Math.PI / 2.0 ) {
					Z = NEGATIVE_DIR;
				}
				else {
					Z = POSITIVE_DIR;
				}
				X = 0.0;
			}
			else {
				X = dRA_rad < 0 ? NEGATIVE_DIR : POSITIVE_DIR;
				Z = X / Math.Tan( dRA_rad );
			}

			Y = Z * Math.Tan( -dRB_rad );
			return new gp_Dir( X, Y, Z );
		}

		static Tuple<double, double> ComputeABFromToolVecCoordDir( gp_Dir toolVecCoordDir )
		{
			if( toolVecCoordDir == null ) {
				return new Tuple<double, double>( BIG_AB_ANGLE, BIG_AB_ANGLE );
			}
			gp_XYZ v = toolVecCoordDir.XYZ();
			double Vx = v.X();
			double Vy = v.Y();
			double Vz = v.Z();

			// general case: not on XY plane, calculate the angles by atan2
			if( Math.Abs( Vz ) >= GEOM_TOLERANCE ) {
				double dRA_Deg = Math.Atan2( Vx, Vz ) * 180.0 / Math.PI;
				double dRB_Deg = Math.Atan2( -Vy, Vz ) * 180.0 / Math.PI;
				return new Tuple<double, double>( dRA_Deg, dRB_Deg );
			}

			// special case: Vz close to 0 (tool vector lies on XY plane)
			// scale so that max(|Vx|,|Vy|) == 1 to determine the proportion each axis contributes
			gp_XYZ extended = ExtendDirUntilXYComponentIsOne( toolVecCoordDir );
			double scaledVx = extended.X();
			double scaledVy = extended.Y();

			// both X,Y close to 0 means direction is essentially (0,0,1) => RA=RB=0
			if( Math.Abs( scaledVx ) < GEOM_TOLERANCE && Math.Abs( scaledVy ) < GEOM_TOLERANCE ) {
				return new Tuple<double, double>( 0.0, 0.0 );
			}

			// RA: proportional to scaled Vx, capped at ±90°
			double dA_Deg = Math.Sign( scaledVx ) * Math.Min( 1.0, Math.Abs( scaledVx ) ) * 90.0;
			// RB: proportional to scaled Vy (sign inverted), capped at ±90°
			double dB_Deg = -Math.Sign( scaledVy ) * Math.Min( 1.0, Math.Abs( scaledVy ) ) * 90.0;

			// tolerance handling: if the angle is very close to 0 or 90
			if( Math.Abs( dA_Deg ) < GEOM_TOLERANCE ) {
				dA_Deg = 0.0;
			}
			if( 90 - Math.Abs( dA_Deg ) < GEOM_TOLERANCE ) {
				dA_Deg = 90.0;
			}
			if( Math.Abs( dB_Deg ) < GEOM_TOLERANCE ) {
				dB_Deg = 0.0;
			}
			if( 90 - Math.Abs( dB_Deg ) < GEOM_TOLERANCE ) {
				dB_Deg = 90.0;
			}
			return new Tuple<double, double>( dA_Deg, dB_Deg );
		}

		static gp_XYZ ExtendDirUntilXYComponentIsOne( gp_Dir dir )
		{
			gp_XYZ vector = dir.XYZ();
			double x = vector.X();
			double y = vector.Y();

			double finalScale = 1.0;

			// the scale of X and Y
			double scaleX = double.PositiveInfinity;
			double scaleY = double.PositiveInfinity;

			if( Math.Abs( x ) > GEOM_TOLERANCE ) {
				scaleX = 1.0 / Math.Abs( x );
			}
			if( Math.Abs( y ) > GEOM_TOLERANCE ) {
				scaleY = 1.0 / Math.Abs( y );
			}

			// if both x and y are not 0, take the smaller scale to avoid one of them is over 1 after scaling
			finalScale = Math.Min( scaleX, scaleY );
			if( double.IsInfinity( finalScale ) ) {
				// x and y are both close to 0, return the original vector to avoid scaling with infinity
				return vector;
			}

			return new gp_XYZ( vector.X() * finalScale, vector.Y() * finalScale, vector.Z() * finalScale );
		}

		static gp_Dir ConvertToolVecCoordDirToWorld( ISetToolVecPoint toolVecPoint, gp_Dir ToolVecCoordDir )
		{
			gp_Dir worldX = toolVecPoint.TangentVec;
			gp_Dir worldZ = toolVecPoint.InitToolVec;
			gp_Dir worldY = worldZ.Crossed( worldX );

			// localDir = (X, Y, Z) in local frame
			gp_XYZ ToolVecLocalCoord = ToolVecCoordDir.XYZ();
			double X = ToolVecLocalCoord.X();
			double Y = ToolVecLocalCoord.Y();
			double Z = ToolVecLocalCoord.Z();

			gp_XYZ w = worldX.XYZ() * X + worldY.XYZ() * Y + worldZ.XYZ() * Z;
			return new gp_Dir( w );
		}

		static gp_Dir ConvertWorldDirToToolVecCoord( gp_Dir worldDir, ISetToolVecPoint toolVecPoint )
		{
			// create toolvec local coordinate system
			gp_Dir ToolVecX = toolVecPoint.TangentVec;
			gp_Dir ToolVecZ = toolVecPoint.InitToolVec;
			gp_Dir ToolVecY = ToolVecZ.Crossed( ToolVecX );

			// turn the world dir to local coordinate system by projection
			gp_XYZ w = worldDir.XYZ();
			double Vx = w.Dot( ToolVecX.XYZ() );
			double Vy = w.Dot( ToolVecY.XYZ() );
			double Vz = w.Dot( ToolVecZ.XYZ() );

			return new gp_Dir( Vx, Vy, Vz );
		}

		public static void ArrageMapForClosedPath( ref Dictionary<int, ToolVecModifyData> toolVecModifyMap, List<ISetToolVecPoint> toolVecPointList )
		{
			// without control point at the end of closed path
			if( !toolVecModifyMap.ContainsKey( CLOSED_POINT_INDEX ) ) {
				return;
			}

			// reset CLOSED_POINT_INDEX to last index
			ToolVecModifyData closedPointData = toolVecModifyMap[ CLOSED_POINT_INDEX ];
			toolVecModifyMap.Remove( CLOSED_POINT_INDEX );
			toolVecModifyMap[ toolVecPointList.Count - 1 ] = closedPointData;
		}

		public static Tuple<double, double> FlipRotaryAxis( double master_deg, double slave_deg, bool isPositive )
		{
			// Get machine data to determine which axis is rotary (±180)
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return new Tuple<double, double>( master_deg, slave_deg );
			}

			// check if current tool vector is at singular point
			bool isSingular = IsSingular( master_deg, slave_deg );

			// Spindle: master ±180, Table/Mix: slave ±180
			bool isMasterAxis = machineData.FiveAxisType == FiveAxisType.Spindle;

			// Determine if ToolDir is parallel to the rotary axis direction
			gp_Dir rotaryDir = isMasterAxis ? machineData.MasterRotateDir : machineData.SlaveRotateDir;
			bool isToolDirParallelToRotary = machineData.ToolDir.IsParallel( rotaryDir, PARALLEL_TOLERANCE );

			double offset = isPositive ? 180 : -180;
			if( isMasterAxis ) {
				master_deg += offset;

				// at singular point, the non-rotating axis keeps its value
				if( !isSingular ) {
					slave_deg = FlipNonRotatingAxis( slave_deg, isToolDirParallelToRotary );
				}
			}
			else {
				slave_deg += offset;

				// at singular point, the non-rotating axis keeps its value
				if( !isSingular ) {
					master_deg = FlipNonRotatingAxis( master_deg, isToolDirParallelToRotary );
				}
			}
			return new Tuple<double, double>( master_deg, slave_deg );
		}

		static bool IsSingular( double master_deg, double slave_deg )
		{
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				return false;
			}
			PostSolver postSolver = new PostSolver( machineData );
			double dM_rad = master_deg * Math.PI / 180.0;
			double dS_rad = slave_deg * Math.PI / 180.0;
			return postSolver.IsToolVecSingular( dM_rad, dS_rad );
		}

		// flip non-rotating axis based on ToolDir and rotary axis relationship
		// isParallel = true  (X-mirror): v -> -v
		// isParallel = false (Y-mirror): v -> sign(v)*180 - v
		static double FlipNonRotatingAxis( double value_deg, bool isParallel )
		{
			// X-mirror: negate the value
			if( isParallel ) {
				return -value_deg;
			}

			// Y-mirror: supplementary angle
			// handle zero case explicitly to avoid sign ambiguity
			if( Math.Abs( value_deg ) < FLIP_ZERO_TOLERANCE ) {
				return 180.0;
			}
			double sign = value_deg > 0 ? 1.0 : -1.0;
			return sign * 180.0 - value_deg;
		}

		const double MAX_TILTED_ANGLE_DEG = 90.0;
		const double BIG_AB_ANGLE = 999.0;
		const double GEOM_TOLERANCE = 1e-3;
		const double FLIP_ZERO_TOLERANCE = 1e-3;
		const double PARALLEL_TOLERANCE = 1e-3;
		const int POSITIVE_DIR = 1;
		const int NEGATIVE_DIR = -1;

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
