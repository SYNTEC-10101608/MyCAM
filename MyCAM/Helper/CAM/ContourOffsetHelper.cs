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
                // offset only supports closed contours for now
                newPointCount = cadPointList.Count;
                return cadPointList.Select( p => p.Clone() ).ToList();
            }

            // Step A: find corner indices from ConnectPointMap
            Dictionary<int, CADPoint> cornerConnectMap = BuildCornerIndexMap( cadPointList, connectPointMap );

            // Step B: 5-degree check to filter out false corners
            FilterFalseCorners( cadPointList, cornerConnectMap );

            // Step C: offset all points, expanding corners into 2 points
            List<OffsetPoint> offsetPoints = BuildOffsetPointList( cadPointList, cornerConnectMap, offsetDistance );
            if( offsetPoints.Count < 3 ) {
                return null;
            }

            // Step D: resolve corner intersections
            if( !ResolveCornerIntersections( offsetPoints ) ) {
                return null; // entire path degenerated
            }

            // Step E: collect surviving points
            List<CADPoint> result = new List<CADPoint>();
            for( int i = 0; i < offsetPoints.Count; i++ ) {
                if( !offsetPoints[ i ].IsRemoved ) {
                    result.Add( offsetPoints[ i ].Point );
                }
            }

            if( result.Count < 3 ) {
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

        #region Step B: Filter false corners (tangent change < 5 degrees)

        static void FilterFalseCorners(
            List<CADPoint> cadPointList,
            Dictionary<int, CADPoint> cornerConnectMap )
        {
            List<int> falseCorners = new List<int>();
            foreach( var kvp in cornerConnectMap ) {
                int cornerIndex = kvp.Key;
                CADPoint incomingPoint = kvp.Value;   // previous edge end (incoming tangent)
                CADPoint outgoingPoint = cadPointList[ cornerIndex ]; // current edge start (outgoing tangent)

                // compare tangent directions
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
                    // Corner point: produce 2 offset points (incoming, then outgoing)
                    CADPoint incomingPoint = cornerConnectMap[ i ];
                    CADPoint outgoingPoint = cadPointList[ i ];

                    // incoming offset (from value point - previous edge's end)
                    CADPoint offsetIncoming = OffsetSinglePoint( incomingPoint, offsetDistance );
                    result.Add( new OffsetPoint( offsetIncoming, i, true, false ) );

                    // outgoing offset (from key point - current edge's start)
                    CADPoint offsetOutgoing = OffsetSinglePoint( outgoingPoint, offsetDistance );
                    result.Add( new OffsetPoint( offsetOutgoing, i, true, true ) );
                }
                else {
                    // Normal point: single offset
                    CADPoint offsetPoint = OffsetSinglePoint( cadPointList[ i ], offsetDistance );
                    result.Add( new OffsetPoint( offsetPoint, i, false, false ) );
                }
            }
            return result;
        }

        static CADPoint OffsetSinglePoint( CADPoint point, double distance )
        {
            // offset direction = TangentVec กั NormalVec_1st
            gp_Vec tangent = new gp_Vec( point.TangentVec );
            gp_Vec normal = new gp_Vec( point.NormalVec_1st );
            gp_Vec offsetDir = tangent.Crossed( normal );

            double mag = offsetDir.Magnitude();
            if( mag < GEOM_TOLERANCE ) {
                // fallback: can't compute offset direction, just clone
                return point.Clone();
            }
            offsetDir.Normalize();

            // translate point
            gp_Vec displacement = offsetDir * distance;
            CADPoint result = point.Clone();
            result.Translate( displacement );
            return result;
        }

        #endregion

        #region Step D: Resolve corner intersections

        static bool ResolveCornerIntersections( List<OffsetPoint> points )
        {
            // Find all corner pairs (incoming, outgoing) and resolve them
            int maxIterations = points.Count * points.Count; // safety limit
            int iteration = 0;

            while( iteration < maxIterations ) {
                iteration++;
                bool foundUnresolved = false;

                for( int i = 0; i < points.Count; i++ ) {
                    if( points[ i ].IsRemoved || !points[ i ].IsCorner || !points[ i ].IsOutgoing ) {
                        continue;
                    }

                    // find the paired incoming point (should be immediately before)
                    int incomingIdx = FindPrevAlive( points, i );
                    if( incomingIdx < 0 || !points[ incomingIdx ].IsCorner || points[ incomingIdx ].IsOutgoing ) {
                        continue;
                    }

                    // L1: from prev of incoming ก๗ incoming
                    int prevOfIncoming = FindPrevAlive( points, incomingIdx );
                    if( prevOfIncoming < 0 ) {
                        continue;
                    }

                    // L2: from outgoing ก๗ next of outgoing
                    int nextOfOutgoing = FindNextAlive( points, i );
                    if( nextOfOutgoing < 0 ) {
                        continue;
                    }

                    // compute intersection
                    gp_Pnt p1 = points[ prevOfIncoming ].Point.Point;
                    gp_Pnt p2 = points[ incomingIdx ].Point.Point;
                    gp_Pnt p3 = points[ i ].Point.Point;
                    gp_Pnt p4 = points[ nextOfOutgoing ].Point.Point;

                    gp_Pnt intersection = GeometryTool.FindIntersectPoint(
                        p1, p2, p4, p3,
                        out IntersectType typeL1, out IntersectType typeL2 );

                    if( typeL1 == IntersectType.NoIntersect ) {
                        // parallel lines: just remove the corner pair
                        points[ incomingIdx ].IsRemoved = true;
                        points[ i ].IsRemoved = true;
                        foundUnresolved = true;
                        continue;
                    }

                    if( typeL1 == IntersectType.Extend && typeL2 == IntersectType.Extend ) {
                        // Both extend: insert intersection point between incoming and outgoing
                        CADPoint interpPoint = InterpolateCADPoint(
                            points[ incomingIdx ].Point, points[ i ].Point, 0.5, intersection );
                        OffsetPoint insertedPoint = new OffsetPoint( interpPoint, points[ i ].OriginalIndex, false, false );

                        // Replace the corner pair with the intersection point
                        points[ incomingIdx ].IsRemoved = true;
                        points[ i ].IsRemoved = true;

                        // insert after the outgoing position
                        points.Insert( i + 1, insertedPoint );
                        foundUnresolved = true;
                        break; // restart scan since indices shifted
                    }
                    else {
                        // Inbetween or ReverseExtend on at least one side: degenerate
                        // Remove the offending endpoint and retry
                        if( typeL1 == IntersectType.Inbetween || typeL1 == IntersectType.ReverseExtend ) {
                            // L1 has self-intersection issue: remove prev of incoming, demote incoming
                            points[ prevOfIncoming ].IsRemoved = true;

                            // the incoming point now becomes the new L1 endpoint
                            // update its position to be the same (it's still a corner incoming)
                            // but L1 is now (prev-prev, incoming)
                        }
                        if( typeL2 == IntersectType.Inbetween || typeL2 == IntersectType.ReverseExtend ) {
                            // L2 has self-intersection issue: remove next of outgoing, demote outgoing
                            points[ nextOfOutgoing ].IsRemoved = true;
                        }

                        foundUnresolved = true;
                        break; // restart scan
                    }
                }

                if( !foundUnresolved ) {
                    break; // all corners resolved
                }

                // check if we have enough points left
                int aliveCount = points.Count( p => !p.IsRemoved );
                if( aliveCount < 3 ) {
                    return false;
                }
            }

            return points.Count( p => !p.IsRemoved ) >= 3;
        }

        /// <summary>
        /// Find previous alive point index (circular).
        /// </summary>
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

        /// <summary>
        /// Find next alive point index (circular).
        /// </summary>
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

        #region Utilities

        static CADPoint InterpolateCADPoint( CADPoint a, CADPoint b, double t, gp_Pnt overridePosition )
        {
            // Lerp vectors
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
            public bool IsOutgoing; // true = outgoing side of corner, false = incoming side
            public bool IsRemoved;
        }

        #endregion

        const double GEOM_TOLERANCE = 1e-6;
        const double CORNER_ANGLE_THRESHOLD_DEG = 5.0;
    }
}
