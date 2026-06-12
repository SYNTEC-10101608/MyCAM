using MyCAM.Data;
using OCC.gp;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class ContourOffsetHelper
	{
		public static List<CADPoint> ApplyOffset(
			List<CADPoint> cadPointList,
			Dictionary<CADPoint, CADPoint> connectPointMap,
			double offsetDistance,
			bool isClosed,
			out List<int> originalIndexMap )
		{
			originalIndexMap = null;

			// invalid input
			if( cadPointList == null || cadPointList.Count == 0 ) {
				return cadPointList;
			}

			// no offset
			if( Math.Abs( offsetDistance ) < GEOM_TOLERANCE ) {
				originalIndexMap = Enumerable.Range( 0, cadPointList.Count ).ToList();
				return cadPointList.Select( p => p.Clone() ).ToList();
			}

			// not support non-closed path
			if( !isClosed ) {
				originalIndexMap = Enumerable.Range( 0, cadPointList.Count ).ToList();
				return cadPointList.Select( p => p.Clone() ).ToList();
			}

			// Step A: find corner indices from ConnectPointMap
			Dictionary<int, CADPoint> cornerConnectMap = BuildCornerIndexMap( cadPointList, connectPointMap );

			// Step B: 5-degree check to filter out false corners
			FilterFalseCorners( cadPointList, cornerConnectMap );

			// Step C: offset all points, expanding corners into 2 points
			List<OffsetPoint> offsetPoints = BuildOffsetPointList( cadPointList, cornerConnectMap, offsetDistance );
			if( offsetPoints.Count < MIN_VALID_POINT_COUNT ) {
				return null;
			}

			// Step D: detect collapsed arc regions and mark as corner pairs
			if( !DetectCollapsedArcs( ref offsetPoints ) ) {
				return null;
			}

			// Step E: resolve corner intersections
			if( !ResolveCornerIntersections( ref offsetPoints ) ) {
				return null;
			}

			// Step F: collect surviving points and their original index mapping
			List<CADPoint> result = new List<CADPoint>();
			List<int> indexMap = new List<int>();
			for( int i = 0; i < offsetPoints.Count; i++ ) {
				if( !offsetPoints[ i ].IsRemoved ) {
					result.Add( offsetPoints[ i ].Point );
					indexMap.Add( offsetPoints[ i ].OriginalIndex );
				}
			}

			// result points count validation
			if( result.Count < MIN_VALID_POINT_COUNT ) {
				return null;
			}

			// Step G: remove duplicate points (circular), keeping index map in sync
			RemoveDuplicatePoints( ref result, ref indexMap );

			// result points count validation
			if( result.Count < MIN_VALID_POINT_COUNT ) {
				return null;
			}
			originalIndexMap = indexMap;
			return result;
		}

		#region Step A: Build corner index map

		static Dictionary<int, CADPoint> BuildCornerIndexMap(
			List<CADPoint> cadPointList,
			Dictionary<CADPoint, CADPoint> connectPointMap )
		{
			Dictionary<int, CADPoint> cornerMap = new Dictionary<int, CADPoint>();
			if( connectPointMap == null || connectPointMap.Count == 0 ) {
				return cornerMap;
			}

			for( int i = 0; i < cadPointList.Count; i++ ) {
				if( connectPointMap.ContainsKey( cadPointList[ i ] ) ) {
					cornerMap[ i ] = connectPointMap[ cadPointList[ i ] ];
				}
			}
			return cornerMap;
		}

		#endregion

		#region Step B: Filter false corners (tangent change < threshold)

		static void FilterFalseCorners(
			List<CADPoint> cadPointList,
			Dictionary<int, CADPoint> cornerConnectMap )
		{
			List<int> falseCorners = new List<int>();
			foreach( var kvp in cornerConnectMap ) {
				int cornerIndex = kvp.Key;
				CADPoint incomingPoint = kvp.Value;
				CADPoint outgoingPoint = cadPointList[ cornerIndex ];

				double angleDeg = GetAngleBetweenDirs( incomingPoint.TangentVec, outgoingPoint.TangentVec );
				if( angleDeg < CORNER_ANGLE_THRESHOLD_DEG ) {
					falseCorners.Add( cornerIndex );
				}
			}
			foreach( int idx in falseCorners ) {
				cornerConnectMap.Remove( idx );
			}
		}

		#endregion

		#region Step C: Build offset point list

		static List<OffsetPoint> BuildOffsetPointList(
			List<CADPoint> cadPointList,
			Dictionary<int, CADPoint> cornerConnectMap,
			double offsetDistance )
		{
			// to record all data needed for offset process
			List<OffsetPoint> result = new List<OffsetPoint>();

			// offset each point
			for( int i = 0; i < cadPointList.Count; i++ ) {

				// the point is a corner
				if( cornerConnectMap.ContainsKey( i ) ) {

					// by cureent design, the souce point of corner is always the outgoing point, and the incoming point is from ConnectPointMap
					CADPoint incomingPoint = cornerConnectMap[ i ];
					CADPoint outgoingPoint = cadPointList[ i ];

					// add offset incoming point first (corner, incoming)
					CADPoint offsetIncoming = OffsetSinglePoint( incomingPoint, offsetDistance );
					result.Add( new OffsetPoint( offsetIncoming, OFFSET_GENERATED_INDEX, true, false, i ) );

					// add offset outgoing point second (corner, outgoing)
					CADPoint offsetOutgoing = OffsetSinglePoint( outgoingPoint, offsetDistance );
					result.Add( new OffsetPoint( offsetOutgoing, OFFSET_GENERATED_INDEX, true, true, i ) );
				}

				// the point is not a corner, just offset normally
				else {
					CADPoint offsetPoint = OffsetSinglePoint( cadPointList[ i ], offsetDistance );
					result.Add( new OffsetPoint( offsetPoint, i, false, false, INVALID_CORNER_INDEX ) );
				}
			}
			return result;
		}

		static CADPoint OffsetSinglePoint( CADPoint point, double distance )
		{
			gp_Vec tangent = new gp_Vec( point.TangentVec );
			gp_Vec normal = new gp_Vec( point.NormalVec_1st );
			gp_Vec offsetDir = tangent.Crossed( normal );

			double mag = offsetDir.Magnitude();
			if( mag < GEOM_TOLERANCE ) {
				return point.Clone();
			}
			offsetDir.Normalize();

			gp_Vec displacement = offsetDir * distance;
			CADPoint result = point.Clone();
			result.Translate( displacement );
			return result;
		}

		#endregion

		#region Step D: Detect collapsed arc regions

		static bool DetectCollapsedArcs( ref List<OffsetPoint> points )
		{
			int count = points.Count;
			if( count < MIN_VALID_POINT_COUNT ) {
				return false;
			}

			// compute "leaving direction flipped" flag for each alive point
			bool[] isFlipped = new bool[ count ];
			for( int i = 0; i < count; i++ ) {

				// should not happened in current design, just a safety check
				if( points[ i ].IsRemoved ) {
					isFlipped[ i ] = false;
					continue;
				}
				int nextIdx = FindNextAlive( points, i );
				if( nextIdx < 0 ) {
					isFlipped[ i ] = false;
					continue;
				}
				gp_Vec actualDir = new gp_Vec( points[ i ].Point.Point, points[ nextIdx ].Point.Point );
				if( actualDir.Magnitude() < GEOM_TOLERANCE ) {
					isFlipped[ i ] = false;
					continue;
				}
				gp_Vec storedTangent = new gp_Vec( points[ i ].Point.TangentVec );
				double dot = actualDir.Dot( storedTangent );
				isFlipped[ i ] = dot < 0;
			}

			// find collapsed regions based on flipped flags (Pin is the first flipped point, Pout is the first non-flipped point)
			List<Tuple<int, int>> collapsedRegions = FindCollapsedRegions( points, isFlipped );

			// process each collapsed region
			foreach( var region in collapsedRegions ) {
				int pinIdx = region.Item1;
				int poutIdx = region.Item2;
				MarkCollapsedRegion( points, pinIdx, poutIdx );
			}

			return points.Count( p => !p.IsRemoved ) > MIN_VALID_POINT_COUNT;
		}

		static List<Tuple<int, int>> FindCollapsedRegions( List<OffsetPoint> points, bool[] isFlipped )
		{
			// tuple(in, out)
			List<Tuple<int, int>> regions = new List<Tuple<int, int>>();
			int count = points.Count;

			// find a starting point that is alive and not flipped (to avoid starting inside a collapsed region)
			int startScan = -1;
			for( int i = 0; i < count; i++ ) {

				// in current design, there should be no removed point at this stage, just a safety check
				if( !points[ i ].IsRemoved && !isFlipped[ i ] ) {
					startScan = i;
					break;
				}
			}
			if( startScan < 0 ) {

				// all points flipped: entire path collapsed
				return regions;
			}

			// scan circularly from startScan
			const int NORMAL_STATE_INDEX = -1;
			int pinIdx = NORMAL_STATE_INDEX;
			int scanned = 0;
			int current = startScan;

			while( scanned < count ) {

				// in current design, there should be no removed point at this stage, just a safety check
				if( points[ current ].IsRemoved ) {
					current = ( current + 1 ) % count;
					scanned++;
					continue;
				}

				if( pinIdx == NORMAL_STATE_INDEX ) {

					// NORMAL state: looking for collapse start
					if( isFlipped[ current ] ) {

						// current is Pin (its leaving edge is flipped)
						pinIdx = current;
					}
				}
				else {

					// COLLAPSED state: looking for collapse end
					if( !isFlipped[ current ] ) {

						// current point is Pout (it has flipped arrival but valid leaving)
						regions.Add( new Tuple<int, int>( pinIdx, current ) );
						pinIdx = NORMAL_STATE_INDEX;
					}
				}

				current = ( current + 1 ) % count;
				scanned++;
			}

			// if we ended in collapsed state, the region wraps around
			if( pinIdx != NORMAL_STATE_INDEX ) {

				// pout is the first non-flipped alive point we started scanning from
				regions.Add( new Tuple<int, int>( pinIdx, startScan ) );
			}

			return regions;
		}

		static void MarkCollapsedRegion( List<OffsetPoint> points, int pinIdx, int poutIdx )
		{
			// determine the InheritedCornerIndex from the outgoing point (consistent with Step C convention)
			int cornerIdx = points[ poutIdx ].OriginalIndex;

			// should not happened in current design, just a safety check
			if( cornerIdx == OFFSET_GENERATED_INDEX ) {
				cornerIdx = points[ poutIdx ].InheritedCornerIndex;
			}

			// mark Pin as corner incoming
			points[ pinIdx ].IsCorner = true;
			points[ pinIdx ].IsOutgoing = false;
			points[ pinIdx ].InheritedCornerIndex = cornerIdx;

			// mark Pout as corner outgoing
			points[ poutIdx ].IsCorner = true;
			points[ poutIdx ].IsOutgoing = true;
			points[ poutIdx ].InheritedCornerIndex = cornerIdx;

			// remove all points between Pin and Pout (exclusive, circular)
			int current = FindNextAlive( points, pinIdx );
			while( current != IVALID_SEARCH_INDEX && current != poutIdx ) {
				points[ current ].IsRemoved = true;
				current = FindNextAlive( points, current );
				if( current == pinIdx ) {
					break;
				}
			}
		}

		#endregion

		#region Step E: Resolve corner intersections

		static bool ResolveCornerIntersections( ref List<OffsetPoint> points )
		{
			int maxIterations = points.Count * MAX_ITERATION_FACTOR;
			int iteration = 0;

			while( iteration < maxIterations ) {
				iteration++;

				// to check if we need a next iteration
				bool foundUnresolved = false;

				// solve each corner pair
				for( int i = 0; i < points.Count; i++ ) {

					// find the out going index
					if( points[ i ].IsRemoved || !points[ i ].IsCorner || !points[ i ].IsOutgoing ) {
						continue;
					}

					// find the corresponding incoming index
					int incomingIdx = FindPrevAlive( points, i );
					if( incomingIdx < 0 || !points[ incomingIdx ].IsCorner || points[ incomingIdx ].IsOutgoing ) {
						continue;
					}

					// find the corresponding previous of oincoming
					int prevOfIncoming = FindPrevAlive( points, incomingIdx );
					if( prevOfIncoming < 0 ) {
						continue;
					}

					// find the corresponding next of outgoing
					int nextOfOutgoing = FindNextAlive( points, i );
					if( nextOfOutgoing < 0 ) {
						continue;
					}

					// i is the out going index
					gp_Pnt p1 = points[ prevOfIncoming ].Point.Point;
					gp_Pnt p2 = points[ incomingIdx ].Point.Point;
					gp_Pnt p3 = points[ i ].Point.Point;
					gp_Pnt p4 = points[ nextOfOutgoing ].Point.Point;

					// for the definition of API¡Awe need to put line2 as P4-P3
					gp_Pnt intersection = GeometryTool.FindIntersectPoint(
						p1, p2, p4, p3,
						out IntersectType typeL1, out IntersectType typeL2 );

					// basicly should not happened, just a safety check
					if( typeL1 == IntersectType.NoIntersect ) {
						points[ incomingIdx ].IsCorner = false;
						points[ i ].IsCorner = false;
						foundUnresolved = true;
						continue;
					}


					// the convex corner
					if( typeL1 == IntersectType.Extend && typeL2 == IntersectType.Extend ) {
						CADPoint interpPoint = InterpolateCADPoint(
							points[ incomingIdx ].Point, points[ i ].Point, CORNER_INTERPOLATION_PARAM, intersection );
						int inheritedIdx = points[ i ].InheritedCornerIndex;
						OffsetPoint insertedPoint = new OffsetPoint( interpPoint, inheritedIdx, false, false, INVALID_CORNER_INDEX );

						// mark corner pair as resolved, keep them alive
						points[ incomingIdx ].IsCorner = false;
						points[ i ].IsCorner = false;

						// insert intersection point between incoming and outgoing
						points.Insert( i, insertedPoint );
						foundUnresolved = true;
						break;
					}

					// the concave corner
					else {

						// L1 self-intersection: remove incoming, promote prevOfIncoming as new corner incoming
						if( typeL1 == IntersectType.Inbetween || typeL1 == IntersectType.ReverseExtend ) {
							points[ incomingIdx ].IsRemoved = true;
							points[ prevOfIncoming ].IsCorner = true;
							points[ prevOfIncoming ].IsOutgoing = false;
							points[ prevOfIncoming ].InheritedCornerIndex = points[ incomingIdx ].InheritedCornerIndex;
						}

						// L2 self-intersection: remove outgoing, promote nextOfOutgoing as new corner outgoing
						if( typeL2 == IntersectType.Inbetween || typeL2 == IntersectType.ReverseExtend ) {
							points[ i ].IsRemoved = true;
							points[ nextOfOutgoing ].IsCorner = true;
							points[ nextOfOutgoing ].IsOutgoing = true;
							points[ nextOfOutgoing ].InheritedCornerIndex = points[ i ].InheritedCornerIndex;
						}

						foundUnresolved = true;
						break;
					}
				}

				if( !foundUnresolved ) {
					break;
				}

				if( points.Count( p => !p.IsRemoved ) < MIN_VALID_POINT_COUNT ) {
					return false;
				}
			}

			return points.Count( p => !p.IsRemoved ) > MIN_VALID_POINT_COUNT;
		}

		static int FindPrevAlive( List<OffsetPoint> points, int currentIdx )
		{
			int count = points.Count;
			for( int step = 1; step < count; step++ ) {
				int idx = ( currentIdx - step + count ) % count;
				if( !points[ idx ].IsRemoved ) {
					return idx;
				}
			}
			return IVALID_SEARCH_INDEX;
		}

		static int FindNextAlive( List<OffsetPoint> points, int currentIdx )
		{
			int count = points.Count;
			for( int step = 1; step < count; step++ ) {
				int idx = ( currentIdx + step ) % count;
				if( !points[ idx ].IsRemoved ) {
					return idx;
				}
			}
			return IVALID_SEARCH_INDEX;
		}

		#endregion

		#region Step G: Remove duplicate points

		static void RemoveDuplicatePoints( ref List<CADPoint> points, ref List<int> indexMap )
		{
			if( points == null || points.Count <= 1 ) {
				return;
			}

			List<CADPoint> resultPoints = new List<CADPoint> { points[ 0 ] };
			List<int> resultMap = new List<int> { indexMap[ 0 ] };
			for( int i = 1; i < points.Count; i++ ) {
				double dist = points[ i ].Point.Distance( resultPoints[ resultPoints.Count - 1 ].Point );
				if( dist >= DUPLICATE_POINT_TOLERANCE ) {
					resultPoints.Add( points[ i ] );
					resultMap.Add( indexMap[ i ] );
				}
			}

			// circular check: compare last point with first point
			if( resultPoints.Count > 1 ) {
				double closingDist = resultPoints[ resultPoints.Count - 1 ].Point.Distance( resultPoints[ 0 ].Point );
				if( closingDist < DUPLICATE_POINT_TOLERANCE ) {
					resultPoints.RemoveAt( resultPoints.Count - 1 );
					resultMap.RemoveAt( resultMap.Count - 1 );
				}
			}

			points = resultPoints;
			indexMap = resultMap;
		}

		#endregion

		#region Utilities

		static CADPoint InterpolateCADPoint( CADPoint a, CADPoint b, double t, gp_Pnt overridePosition )
		{
			gp_Dir normal1st = LerpDir( a.NormalVec_1st, b.NormalVec_1st, t );
			gp_Dir normal2nd = LerpDir( a.NormalVec_2nd, b.NormalVec_2nd, t );
			gp_Dir tangent = LerpDir( a.TangentVec, b.TangentVec, t );

			return new CADPoint( overridePosition, normal1st, normal2nd, tangent );
		}

		static gp_Dir LerpDir( gp_Dir a, gp_Dir b, double t )
		{
			double x = a.X() * ( 1.0 - t ) + b.X() * t;
			double y = a.Y() * ( 1.0 - t ) + b.Y() * t;
			double z = a.Z() * ( 1.0 - t ) + b.Z() * t;
			double mag = Math.Sqrt( x * x + y * y + z * z );
			if( mag < GEOM_TOLERANCE ) {
				return new gp_Dir( a.XYZ() );
			}
			return new gp_Dir( x / mag, y / mag, z / mag );
		}

		static double GetAngleBetweenDirs( gp_Dir a, gp_Dir b )
		{
			double dot = a.X() * b.X() + a.Y() * b.Y() + a.Z() * b.Z();
			dot = Math.Max( -1.0, Math.Min( 1.0, dot ) );
			return Math.Acos( dot ) * 180.0 / Math.PI;
		}

		#endregion

		#region Internal types

		class OffsetPoint
		{
			public OffsetPoint( CADPoint point, int originalIndex, bool isCorner, bool isOutgoing, int inheritedCornerIndex )
			{
				Point = point;
				OriginalIndex = originalIndex;
				IsCorner = isCorner;
				IsOutgoing = isOutgoing;
				IsRemoved = false;
				InheritedCornerIndex = inheritedCornerIndex;
			}

			public CADPoint Point;
			public int OriginalIndex;
			public int InheritedCornerIndex;
			public bool IsCorner;
			public bool IsOutgoing;
			public bool IsRemoved;
		}

		#endregion

		#region Constants

		public const int OFFSET_GENERATED_INDEX = -2;
		const int INVALID_CORNER_INDEX = -3;
		const int IVALID_SEARCH_INDEX = -1;
		const double GEOM_TOLERANCE = 1e-3;
		const double DUPLICATE_POINT_TOLERANCE = 1e-3;
		const double CORNER_ANGLE_THRESHOLD_DEG = 5.0;
		const double CORNER_INTERPOLATION_PARAM = 0.5;
		const int MIN_VALID_POINT_COUNT = 3;
		const int MAX_ITERATION_FACTOR = 100;

		#endregion
	}
}
