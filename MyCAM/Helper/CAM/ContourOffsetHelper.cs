using MyCAM.Data;
using OCCTool;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
    public static class ContourOffsetHelper
    {
        /// <summary>
        /// Apply contour offset (inflation/deflation) to a CAD point list.
        /// Positive distance offsets along TangentVec กั NormalVec_1st direction.
        /// Only supports closed contours. Returns null if offset degenerates the entire path.
        /// </summary>
        public static List<CADPoint> ApplyOffset(
            List<CADPoint> cadPointList,
            Dictionary<CADPoint, CADPoint> connectPointMap,
            double offsetDistance,
            bool isClosed,
            out int newPointCount )
        {
            newPointCount = 0;
            if( cadPointList == null || cadPointList.Count == 0 ) {
                return cadPointList;
            }
            if( Math.Abs( offsetDistance ) < GEOM_TOLERANCE ) {
                newPointCount = cadPointList.Count;
                return cadPointList.Select( p => p.Clone() ).ToList();
            }
            if( !isClosed ) {
                newPointCount = cadPointList.Count;
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

            // Step D: resolve corner intersections
            if( !ResolveCornerIntersections( offsetPoints ) ) {
                return null;
            }

            // Step E: collect surviving points
            List<CADPoint> result = new List<CADPoint>();
            for( int i = 0; i < offsetPoints.Count; i++ ) {
                if( !offsetPoints[ i ].IsRemoved ) {
                    result.Add( offsetPoints[ i ].Point );
                }
            }

            if( result.Count < MIN_VALID_POINT_COUNT ) {
                return null;
            }

            // Step F: remove duplicate points (circular)
            result = RemoveDuplicatePoints( result );
            if( result.Count < MIN_VALID_POINT_COUNT ) {
                return null;
            }

            newPointCount = result.Count;
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

            // ConnectPointMap keys are references that exist in cadPointList
            foreach( var kvp in connectPointMap ) {
                int index = -1;
                for( int i = 0; i < cadPointList.Count; i++ ) {
                    if( ReferenceEquals( cadPointList[ i ], kvp.Key ) ) {
                        index = i;
                        break;
                    }
                }
                if( index >= 0 ) {
                    cornerMap[ index ] = kvp.Value;
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
            List<OffsetPoint> result = new List<OffsetPoint>();

            for( int i = 0; i < cadPointList.Count; i++ ) {
                if( cornerConnectMap.ContainsKey( i ) ) {
                    CADPoint incomingPoint = cornerConnectMap[ i ];
                    CADPoint outgoingPoint = cadPointList[ i ];

                    CADPoint offsetIncoming = OffsetSinglePoint( incomingPoint, offsetDistance );
                    result.Add( new OffsetPoint( offsetIncoming, i, true, false ) );

                    CADPoint offsetOutgoing = OffsetSinglePoint( outgoingPoint, offsetDistance );
                    result.Add( new OffsetPoint( offsetOutgoing, i, true, true ) );
                }
                else {
                    CADPoint offsetPoint = OffsetSinglePoint( cadPointList[ i ], offsetDistance );
                    result.Add( new OffsetPoint( offsetPoint, i, false, false ) );
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

        #region Step D: Resolve corner intersections

        static bool ResolveCornerIntersections( List<OffsetPoint> points )
        {
            int maxIterations = points.Count * MAX_ITERATION_FACTOR;
            int iteration = 0;

            while( iteration < maxIterations ) {
                iteration++;
                bool foundUnresolved = false;

                for( int i = 0; i < points.Count; i++ ) {
                    if( points[ i ].IsRemoved || !points[ i ].IsCorner || !points[ i ].IsOutgoing ) {
                        continue;
                    }

                    int incomingIdx = FindPrevAlive( points, i );
                    if( incomingIdx < 0 || !points[ incomingIdx ].IsCorner || points[ incomingIdx ].IsOutgoing ) {
                        continue;
                    }

                    int prevOfIncoming = FindPrevAlive( points, incomingIdx );
                    if( prevOfIncoming < 0 ) {
                        continue;
                    }

                    int nextOfOutgoing = FindNextAlive( points, i );
                    if( nextOfOutgoing < 0 ) {
                        continue;
                    }

                    gp_Pnt p1 = points[ prevOfIncoming ].Point.Point;
                    gp_Pnt p2 = points[ incomingIdx ].Point.Point;
                    gp_Pnt p3 = points[ i ].Point.Point;
                    gp_Pnt p4 = points[ nextOfOutgoing ].Point.Point;

                    gp_Pnt intersection = GeometryTool.FindIntersectPoint(
                        p1, p2, p4, p3,
                        out IntersectType typeL1, out IntersectType typeL2 );

                    if( typeL1 == IntersectType.NoIntersect ) {
                        points[ incomingIdx ].IsCorner = false;
                        points[ i ].IsCorner = false;
                        foundUnresolved = true;
                        continue;
                    }

                    if( typeL1 == IntersectType.Extend && typeL2 == IntersectType.Extend ) {
                        CADPoint interpPoint = InterpolateCADPoint(
                            points[ incomingIdx ].Point, points[ i ].Point, CORNER_INTERPOLATION_PARAM, intersection );
                        OffsetPoint insertedPoint = new OffsetPoint( interpPoint, points[ i ].OriginalIndex, false, false );

                        // mark corner pair as resolved, keep them alive
                        points[ incomingIdx ].IsCorner = false;
                        points[ i ].IsCorner = false;

                        // insert intersection point between incoming and outgoing
                        points.Insert( i, insertedPoint );
                        foundUnresolved = true;
                        break;
                    }
                    else {
                        // L1 self-intersection: remove incoming, promote prevOfIncoming as new corner incoming
                        if( typeL1 == IntersectType.Inbetween || typeL1 == IntersectType.ReverseExtend ) {
                            points[ incomingIdx ].IsRemoved = true;
                            points[ prevOfIncoming ].IsCorner = true;
                            points[ prevOfIncoming ].IsOutgoing = false;
                        }
                        // L2 self-intersection: remove outgoing, promote nextOfOutgoing as new corner outgoing
                        if( typeL2 == IntersectType.Inbetween || typeL2 == IntersectType.ReverseExtend ) {
                            points[ i ].IsRemoved = true;
                            points[ nextOfOutgoing ].IsCorner = true;
                            points[ nextOfOutgoing ].IsOutgoing = true;
                        }

                        foundUnresolved = true;
                        break;
                    }
                }

                if( !foundUnresolved ) {
                    break;
                }

                int aliveCount = points.Count( p => !p.IsRemoved );
                if( aliveCount < MIN_VALID_POINT_COUNT ) {
                    return false;
                }
            }

            return points.Count( p => !p.IsRemoved ) >= MIN_VALID_POINT_COUNT;
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
            return -1;
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
            return -1;
        }

        #endregion

        #region Step F: Remove duplicate points

        static List<CADPoint> RemoveDuplicatePoints( List<CADPoint> points )
        {
            if( points == null || points.Count <= 1 ) {
                return points;
            }

            List<CADPoint> result = new List<CADPoint> { points[ 0 ] };
            for( int i = 1; i < points.Count; i++ ) {
                double dist = points[ i ].Point.Distance( result[ result.Count - 1 ].Point );
                if( dist >= DUPLICATE_POINT_TOLERANCE ) {
                    result.Add( points[ i ] );
                }
            }

            // circular check: compare last point with first point
            if( result.Count > 1 ) {
                double closingDist = result[ result.Count - 1 ].Point.Distance( result[ 0 ].Point );
                if( closingDist < DUPLICATE_POINT_TOLERANCE ) {
                    result.RemoveAt( result.Count - 1 );
                }
            }

            return result;
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
            public OffsetPoint( CADPoint point, int originalIndex, bool isCorner, bool isOutgoing )
            {
                Point = point;
                OriginalIndex = originalIndex;
                IsCorner = isCorner;
                IsOutgoing = isOutgoing;
                IsRemoved = false;
            }

            public CADPoint Point;
            public int OriginalIndex;
            public bool IsCorner;
            public bool IsOutgoing;
            public bool IsRemoved;
        }

        #endregion

        #region Constants

        const double GEOM_TOLERANCE = 1e-6;
        const double DUPLICATE_POINT_TOLERANCE = 1e-3;
        const double CORNER_ANGLE_THRESHOLD_DEG = 5.0;
        const double CORNER_INTERPOLATION_PARAM = 0.5;
        const int MIN_VALID_POINT_COUNT = 3;
        const int MAX_ITERATION_FACTOR = 100;

        #endregion
    }
}
