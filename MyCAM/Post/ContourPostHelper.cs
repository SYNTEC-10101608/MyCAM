using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;

namespace MyCAM.Post
{
	internal class PathEndInfo
	{
		public IProcessPoint EndCAMPoint
		{
			get; set;
		}

		public double Master
		{
			get; set;
		}

		public double Slave
		{
			get; set;
		}
	}

	internal class ContourNCPackage : INCPackage
	{
		public ContourNCPackage( LeadData leadData, double overCutLength,
			IReadOnlyList<IProcessPoint> camPointList,
			IReadOnlyList<IProcessPoint> leadInCAMPointList,
			IReadOnlyList<IProcessPoint> leadOutCAMPointList,
			IReadOnlyList<IProcessPoint> overCutCAMPointList,
			TraverseData traverseData,
			IProcessPoint processStartPoint,
			IProcessPoint processEndPoint
			)
		{
			LeadData = leadData;
			OverCutLength = overCutLength;
			CAMPointList = camPointList;
			LeadInCAMPointList = leadInCAMPointList;
			LeadOutCAMPointList = leadOutCAMPointList;
			OverCutCAMPointList = overCutCAMPointList;
			TraverseData = traverseData;
			ProcessStartPoint = processStartPoint;
			ProcessEndPoint = processEndPoint;
		}

		public LeadData LeadData
		{
			get;
		}

		public double OverCutLength
		{
			get;
		}

		public IReadOnlyList<IProcessPoint> CAMPointList
		{
			get;
		}

		public IReadOnlyList<IProcessPoint> LeadInCAMPointList
		{
			get;
		}

		public IReadOnlyList<IProcessPoint> LeadOutCAMPointList
		{
			get;
		}

		public IReadOnlyList<IProcessPoint> OverCutCAMPointList
		{
			get;
		}

		public TraverseData TraverseData
		{
			get;
		}

		public IProcessPoint ProcessStartPoint
		{
			get; private set;
		}

		public IProcessPoint ProcessEndPoint
		{
			get; private set;
		}
	}

	internal static class ContourPostHelper
	{
		public static bool SolvePath( PostSolver postSolver, ContourNCPackage currentPathNCPack,
			PathEndInfo endInfoOfPreviousPath, EntryAndExitData entryAndExitData,
			out PostData pathG54PostData, out PathEndInfo currentPathtEndInfo )
		{
			// for write NC file
			pathG54PostData = new PostData();

			// to make solution continuous
			currentPathtEndInfo = new PathEndInfo();
			if( postSolver == null || currentPathNCPack == null || entryAndExitData == null ) {
				return false;
			}

			// to ensure joint space continuity of process path
			double dLastPointProcess_M = endInfoOfPreviousPath?.Master ?? 0;
			double dLastPointProcess_S = endInfoOfPreviousPath?.Slave ?? 0;

			// main path
			if( !SolveProcessPath( postSolver, currentPathNCPack.CAMPointList,
				out List<PostPoint> mainG54,
				ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
				return false;
			}
			pathG54PostData.MainPathPostPointList.AddRange( mainG54 );
			pathG54PostData.ProcessStartPoint = pathG54PostData.MainPathPostPointList[ 0 ];

			// over-cut
			if( currentPathNCPack.OverCutLength != 0 && currentPathNCPack.OverCutCAMPointList.Count > 0 ) {
				if( !SolveProcessPath( postSolver, currentPathNCPack.OverCutCAMPointList,
					out List<PostPoint> overCutG54,
					ref dLastPointProcess_M, ref dLastPointProcess_S ) ) {
					return false;
				}
				pathG54PostData.OverCutPostPointList.AddRange( overCutG54 );
			}

			// lead-in
			if( currentPathNCPack.LeadInCAMPointList.Count > 0 ) {
				if( pathG54PostData.MainPathPostPointList.Count == 0 ) {
					return false;
				}
				double startM = pathG54PostData.MainPathPostPointList[ 0 ].Master;
				double startS = pathG54PostData.MainPathPostPointList[ 0 ].Slave;
				if( !BuildProcessPath( currentPathNCPack.LeadInCAMPointList, startM, startS,
					out List<PostPoint> leadInG54 ) ) {
					return false;
				}
				pathG54PostData.LeadInPostPointList.AddRange( leadInG54 );

				// update process start point
				pathG54PostData.ProcessStartPoint = pathG54PostData.LeadInPostPointList[ 0 ];
			}

			// lead-out
			if( currentPathNCPack.LeadOutCAMPointList.Count > 0 ) {
				if( !BuildProcessPath( currentPathNCPack.LeadOutCAMPointList, dLastPointProcess_M, dLastPointProcess_S,
					out List<PostPoint> leadOutG54 ) ) {
					return false;
				}
				pathG54PostData.LeadOutPostPointList.AddRange( leadOutG54 );
			}

			// should be the first path
			if( endInfoOfPreviousPath == null ) {

				// the traverse of first path is entry
				PostTraverseBuilder.CalculateEntry( currentPathNCPack, entryAndExitData, pathG54PostData );
			}
			else {

				// traverse from previous path to current path
				PostTraverseBuilder.CalculateTraverse( endInfoOfPreviousPath, currentPathNCPack, pathG54PostData );
			}

			// end info of current path
			currentPathtEndInfo = new PathEndInfo()
			{
				EndCAMPoint = currentPathNCPack.ProcessEndPoint,
				Master = dLastPointProcess_M,
				Slave = dLastPointProcess_S
			};
			return true;
		}

		public static List<PostPoint> GetConcatenatedPostList( PostData postData )
		{
			List<PostPoint> result = new List<PostPoint>();
			if( postData == null ) {
				return result;
			}
			if( postData.LeadInPostPointList.Count > 0 ) {
				result.AddRange( postData.LeadInPostPointList );
			}
			if( postData.MainPathPostPointList.Count > 0 ) {
				result.AddRange( postData.MainPathPostPointList );
			}
			if( postData.OverCutPostPointList.Count > 0 ) {
				result.AddRange( postData.OverCutPostPointList );
			}
			if( postData.LeadOutPostPointList.Count > 0 ) {
				result.AddRange( postData.LeadOutPostPointList );
			}
			return result;
		}

		/// <summary>
		/// Filter path by removing points that are insignificant to both geometry and orientation.
		/// </summary>
		/// <param name="path">Input path points</param>
		/// <param name="epsilon">Tolerance for geometry simplification</param>
		/// <param name="orientationError">Tolerance for orientation simplification</param>
		/// <returns>Filtered path with insignificant points removed</returns>
		public static List<PostPoint> FilterPath( List<PostPoint> path, double epsilon = 0.001, double orientationError = 0.001 )
		{
			if( path == null || path.Count < 3 ) {
				return path;
			}

			int len = path.Count;
			double epsSqr = epsilon * epsilon;

			// Step 1: Mark points insignificant to geometry (based on X, Y, Z)
			bool[] geometryInsignificant = MarkGeometryInsignificantPoints( path, epsSqr );

			// Step 2: Mark points insignificant to orientation (based on Master, Slave)
			bool[] orientationInsignificant = MarkOrientationInsignificantPoints( path, orientationError );

			// Step 3: Build filtered result - remove points insignificant to both geometry and orientation
			List<PostPoint> filteredPath = new List<PostPoint>();
			for( int i = 0; i < len; i++ ) {
				// Keep point if it's significant to either geometry or orientation
				if( !geometryInsignificant[ i ] || !orientationInsignificant[ i ] ) {
					filteredPath.Add( path[ i ].Clone() );
				}
			}

			return filteredPath;
		}

		#region Private methods

		/// <summary>
		/// Mark points that are insignificant to the path geometry (X, Y, Z).
		/// Based on SimplifyPath algorithm logic.
		/// </summary>
		static bool[] MarkGeometryInsignificantPoints( List<PostPoint> path, double epsSqr )
		{
			int len = path.Count;
			int high = len - 1;
			bool[] flags = new bool[ len ];
			double[] dsq = new double[ len ];

			// Calculate initial perpendicular distances
			// First and last points are always kept
			dsq[ 0 ] = double.MaxValue;
			dsq[ high ] = double.MaxValue;

			for( int i = 1; i < high; ++i ) {
				dsq[ i ] = PerpendicDistFromLineSqrd( path[ i ], path[ i - 1 ], path[ i + 1 ] );
			}

			// Iteratively mark insignificant points
			int curr = 0;
			for(; ; ) {
				// Skip points with distance > epsSqr
				if( dsq[ curr ] > epsSqr ) {
					int start = curr;
					do {
						curr = GetNext( curr, high, ref flags );
					} while( curr != start && dsq[ curr ] > epsSqr );
					if( curr == start )
						break;
				}

				int prev = GetPrior( curr, high, ref flags );
				int next = GetNext( curr, high, ref flags );
				if( next == prev )
					break;

				int prior2;
				if( dsq[ next ] < dsq[ curr ] ) {
					prior2 = prev;
					prev = curr;
					curr = next;
					next = GetNext( next, high, ref flags );
				}
				else {
					prior2 = GetPrior( prev, high, ref flags );
				}

				// Mark current point as insignificant
				flags[ curr ] = true;
				curr = next;
				next = GetNext( next, high, ref flags );

				// Update distances for affected neighbors
				if( curr != high && curr != 0 ) {
					dsq[ curr ] = PerpendicDistFromLineSqrd( path[ curr ], path[ prev ], path[ next ] );
				}
				if( prev != 0 && prev != high ) {
					dsq[ prev ] = PerpendicDistFromLineSqrd( path[ prev ], path[ prior2 ], path[ curr ] );
				}
			}

			return flags;
		}

		/// <summary>
		/// Calculate squared perpendicular distance from a point to a line defined by two other points.
		/// </summary>
		static double PerpendicDistFromLineSqrd( PostPoint pt, PostPoint line1, PostPoint line2 )
		{
			double a = pt.X - line1.X;
			double b = pt.Y - line1.Y;
			double c = pt.Z - line1.Z;
			double d = line2.X - line1.X;
			double e = line2.Y - line1.Y;
			double f = line2.Z - line1.Z;

			double denomSqr = d * d + e * e + f * f;
			if( denomSqr < GEOM_TOLERANCE * GEOM_TOLERANCE ) {
				return 0;
			}

			// Cross product for 3D distance
			double cross_x = b * f - c * e;
			double cross_y = c * d - a * f;
			double cross_z = a * e - b * d;

			return ( cross_x * cross_x + cross_y * cross_y + cross_z * cross_z ) / denomSqr;
		}

		/// <summary>
		/// Get next non-flagged index, wrapping around if necessary.
		/// </summary>
		static int GetNext( int current, int high, ref bool[] flags )
		{
			++current;
			while( current <= high && flags[ current ] )
				++current;
			if( current <= high )
				return current;
			current = 0;
			while( flags[ current ] )
				++current;
			return current;
		}

		/// <summary>
		/// Get prior non-flagged index, wrapping around if necessary.
		/// </summary>
		static int GetPrior( int current, int high, ref bool[] flags )
		{
			if( current == 0 )
				current = high;
			else
				--current;
			while( current > 0 && flags[ current ] )
				--current;
			if( !flags[ current ] )
				return current;
			current = high;
			while( flags[ current ] )
				--current;
			return current;
		}

		/// <summary>
		/// Mark points that are insignificant to the path orientation (Master, Slave).
		/// Uses iterative marking similar to geometry simplification.
		/// </summary>
		static bool[] MarkOrientationInsignificantPoints( List<PostPoint> path, double orientationError )
		{
			int len = path.Count;
			int high = len - 1;
			bool[] flags = new bool[ len ];
			double orientationErrorSqr = orientationError * orientationError;

			// Arrays to store master and slave dsq values
			double[] masterDsq = new double[ len ];
			double[] slaveDsq = new double[ len ];

			// Calculate initial orientation deviations
			// First and last points are always kept
			masterDsq[ 0 ] = double.MaxValue;
			masterDsq[ high ] = double.MaxValue;
			slaveDsq[ 0 ] = double.MaxValue;
			slaveDsq[ high ] = double.MaxValue;

			for( int i = 1; i < high; ++i ) {
				CalculateOrientationDsq( path[ i - 1 ], path[ i ], path[ i + 1 ], out masterDsq[ i ], out slaveDsq[ i ] );
			}

			// Iteratively mark insignificant points
			int curr = 0;
			for(; ; ) {
				// Skip points where either master or slave has significant deviation
				if( masterDsq[ curr ] > orientationErrorSqr || slaveDsq[ curr ] > orientationErrorSqr ) {
					int start = curr;
					do {
						curr = GetNext( curr, high, ref flags );
					} while( curr != start && ( masterDsq[ curr ] > orientationErrorSqr || slaveDsq[ curr ] > orientationErrorSqr ) );
					if( curr == start )
						break;
				}

				int prev = GetPrior( curr, high, ref flags );
				int next = GetNext( curr, high, ref flags );
				if( next == prev )
					break;

				int prior2;
				// Choose point with smaller combined deviation to remove first
				double currCombinedDsq = masterDsq[ curr ] + slaveDsq[ curr ];
				double nextCombinedDsq = masterDsq[ next ] + slaveDsq[ next ];

				if( nextCombinedDsq < currCombinedDsq ) {
					prior2 = prev;
					prev = curr;
					curr = next;
					next = GetNext( next, high, ref flags );
				}
				else {
					prior2 = GetPrior( prev, high, ref flags );
				}

				// Mark current point as insignificant
				flags[ curr ] = true;
				curr = next;
				next = GetNext( next, high, ref flags );

				// Update deviations for affected neighbors
				if( curr != high && curr != 0 ) {
					CalculateOrientationDsq( path[ prev ], path[ curr ], path[ next ], out masterDsq[ curr ], out slaveDsq[ curr ] );
				}
				if( prev != 0 && prev != high ) {
					CalculateOrientationDsq( path[ prior2 ], path[ prev ], path[ curr ], out masterDsq[ prev ], out slaveDsq[ prev ] );
				}
			}

			return flags;
		}

		/// <summary>
		/// Calculate orientation deviation squared (dsq) for both Master and Slave axes.
		/// Formula: predictVal = angleL + Dis(pM-pL)/Dis(pR-pL) * (angleR - angleL)
		///          dsq = (predictVal - angleM)^2
		/// </summary>
		/// <param name="pL">Left point</param>
		/// <param name="pM">Middle point to check</param>
		/// <param name="pR">Right point</param>
		/// <param name="masterDsq">Output: squared deviation for Master axis</param>
		/// <param name="slaveDsq">Output: squared deviation for Slave axis</param>
		static void CalculateOrientationDsq( PostPoint pL, PostPoint pM, PostPoint pR, out double masterDsq, out double slaveDsq )
		{
			// Calculate 3D distances in Cartesian space (once for both axes)
			double distML = Math.Sqrt(
				( pM.X - pL.X ) * ( pM.X - pL.X ) +
				( pM.Y - pL.Y ) * ( pM.Y - pL.Y ) +
				( pM.Z - pL.Z ) * ( pM.Z - pL.Z )
			);

			double distRL = Math.Sqrt(
				( pR.X - pL.X ) * ( pR.X - pL.X ) +
				( pR.Y - pL.Y ) * ( pR.Y - pL.Y ) +
				( pR.Z - pL.Z ) * ( pR.Z - pL.Z )
			);

			// Avoid division by zero
			if( distRL < GEOM_TOLERANCE ) {
				masterDsq = 0.0;
				slaveDsq = 0.0;
				return;
			}

			// Calculate interpolation parameter (same for both axes)
			double t = distML / distRL;

			// Calculate Master axis dsq
			double predictMaster = pL.Master + t * ( pR.Master - pL.Master );
			double deviationMaster = predictMaster - pM.Master;
			masterDsq = deviationMaster * deviationMaster;

			// Calculate Slave axis dsq
			double predictSlave = pL.Slave + t * ( pR.Slave - pL.Slave );
			double deviationSlave = predictSlave - pM.Slave;
			slaveDsq = deviationSlave * deviationSlave;
		}

		static bool SolveProcessPath( PostSolver postSolver, IReadOnlyList<IProcessPoint> pointList,
			out List<PostPoint> resultG54, ref double dLastProcessPathM, ref double dLastProcessPathS )
		{
			resultG54 = new List<PostPoint>();
			if( pointList == null || pointList.Count == 0 ) {
				return false;
			}

			bool isSolved = true;
			foreach( IProcessPoint point in pointList ) {
				if( point.Master == CAMPoint.MAGIC_MS_INIT_VAL || point.Slave == CAMPoint.MAGIC_MS_INIT_VAL ) {
					isSolved = false;
					break;
				}
			}

			// solve IK
			List<Tuple<double, double>> rotateAngleList = new List<Tuple<double, double>>();
			List<bool> singularTagList = new List<bool>();
			if( isSolved ) {
				// use existing solution
				foreach( IProcessPoint point in pointList ) {
					rotateAngleList.Add( new Tuple<double, double>( point.Master, point.Slave ) );
					singularTagList.Add( false );
				}
				dLastProcessPathM = rotateAngleList[ rotateAngleList.Count - 1 ].Item1;
				dLastProcessPathS = rotateAngleList[ rotateAngleList.Count - 1 ].Item2;
			}
			else {
				foreach( IProcessPoint point in pointList ) {
					IKSolveResult ikResult = postSolver.SolveIK( point, dLastProcessPathM, dLastProcessPathS, out dLastProcessPathM, out dLastProcessPathS );
					if( ikResult == IKSolveResult.InvalidInput || ikResult == IKSolveResult.NoSolution || ikResult == IKSolveResult.OutOfRange ) {
						return false;
					}
					rotateAngleList.Add( new Tuple<double, double>( dLastProcessPathM, dLastProcessPathS ) );
					if( ikResult == IKSolveResult.NoError ) {
						singularTagList.Add( false );
					}
					else if( ikResult == IKSolveResult.MasterInfinityOfSolution || ikResult == IKSolveResult.SlaveInfinityOfSolution ) {
						singularTagList.Add( true );
					}
				}

				// filter the singular points
				FilterSingularPoints( pointList, rotateAngleList, singularTagList );
			}

			// build post data
			for( int i = 0; i < pointList.Count; i++ ) {
				gp_Pnt pointG54 = pointList[ i ].Point;

				// add G54 frame data
				PostPoint frameDataG54 = new PostPoint()
				{
					X = pointG54.X(),
					Y = pointG54.Y(),
					Z = pointG54.Z(),
					Master = rotateAngleList[ i ].Item1,
					Slave = rotateAngleList[ i ].Item2
				};
				resultG54.Add( frameDataG54 );
			}

			resultG54 = FilterPath( resultG54, 0.01, 0.1 );
			return true;
		}

		static void FilterSingularPoints( IReadOnlyList<IProcessPoint> pointList,
			List<Tuple<double, double>> rotateAngleList, List<bool> singularTagList )
		{
			if( pointList == null || rotateAngleList == null || singularTagList == null ||
				pointList.Count == 0 || pointList.Count != rotateAngleList.Count || pointList.Count != singularTagList.Count ) {
				return;
			}

			int n = singularTagList.Count;
			int i = 0;

			// find singular regions and interpolate
			while( i < n ) {

				// skip non-singular points or points with IsToolVecModPoint == true
				if( !singularTagList[ i ] || pointList[ i ].IsToolVecModPoint ) {
					i++;
					continue;
				}

				// found a singular point, find the range [regionStart, regionEnd]
				int regionStart = i;
				int regionEnd = i;

				// extend to find the complete singular region (singular and not IsToolVecModPoint)
				while( regionEnd < n && singularTagList[ regionEnd ] && !pointList[ regionEnd ].IsToolVecModPoint ) {
					regionEnd++;
				}
				regionEnd--; // back to last singular point

				// determine start values for interpolation
				double startM, startS;
				if( regionStart == 0 ) {
					// rule 3: if first point is singular, use its value
					startM = rotateAngleList[ 0 ].Item1;
					startS = rotateAngleList[ 0 ].Item2;
				}
				else {
					// use n-1 value
					startM = rotateAngleList[ regionStart - 1 ].Item1;
					startS = rotateAngleList[ regionStart - 1 ].Item2;
				}

				// determine end values for interpolation
				double endM, endS;
				if( regionEnd == n - 1 ) {
					// rule 4: if last point is singular, use its value
					endM = rotateAngleList[ n - 1 ].Item1;
					endS = rotateAngleList[ n - 1 ].Item2;
				}
				else {
					// use m+1 value
					endM = rotateAngleList[ regionEnd + 1 ].Item1;
					endS = rotateAngleList[ regionEnd + 1 ].Item2;
				}

				// calculate cumulative path lengths for interpolation
				List<double> cumulativeLength = new List<double>();
				cumulativeLength.Add( 0.0 );
				double totalLength = 0.0;

				for( int j = regionStart; j <= regionEnd; j++ ) {
					if( j > regionStart ) {
						gp_Pnt p1 = pointList[ j - 1 ].Point;
						gp_Pnt p2 = pointList[ j ].Point;
						double segmentLength = p1.Distance( p2 );
						totalLength += segmentLength;
					}
					cumulativeLength.Add( totalLength );
				}

				// linear interpolation along path length
				for( int j = regionStart; j <= regionEnd; j++ ) {
					double t = ( totalLength > GEOM_TOLERANCE ) ? ( cumulativeLength[ j - regionStart ] / totalLength ) : 0.0;
					double interpolatedM = startM + t * ( endM - startM );
					double interpolatedS = startS + t * ( endS - startS );
					rotateAngleList[ j ] = new Tuple<double, double>( interpolatedM, interpolatedS );
				}

				// move to next region
				i = regionEnd + 1;
			}
		}

		static bool BuildProcessPath( IReadOnlyList<IProcessPoint> camPointList, double dM, double dS, out List<PostPoint> resultG54 )
		{
			resultG54 = new List<PostPoint>();
			if( camPointList == null || camPointList.Count == 0 ) {
				return false;
			}

			// build G54 points
			foreach( CAMPoint point in camPointList ) {
				PostPoint g54Point = new PostPoint()
				{
					X = point.Point.X(),
					Y = point.Point.Y(),
					Z = point.Point.Z(),
					Master = dM,
					Slave = dS
				};
				resultG54.Add( g54Point );
			}
			return true;
		}
		#endregion

		const double GEOM_TOLERANCE = 0.001;
	}
}
