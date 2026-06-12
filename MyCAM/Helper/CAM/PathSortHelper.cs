using MyCAM.Data;
using MyCAM.PathCache;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
    /// <summary>
    /// Pure sorting algorithms for path ordering. No UI dependency.
    /// </summary>
    internal static class PathSortHelper
    {
        // below this radial length the angular coordinate is meaningless
        // keep consistent with CAMEditor original implementation
        const double RADIAL_EPSILON = 1e-6;

        public static List<string> SortByShortestPath( List<string> pathIDList, string startPathID )
        {
            try {
                if( pathIDList == null || pathIDList.Count == 0 || string.IsNullOrEmpty( startPathID ) ) {
                    return new List<string>();
                }

                gp_Pnt currentPoint = CacheHelper.GetProcessStartPoint( startPathID )?.Point;
                if( currentPoint == null ) {
                    return new List<string>();
                }

                List<string> newPathIDList = new List<string> { startPathID };
                bool[] visited = new bool[ pathIDList.Count ];
                int startIdx = pathIDList.IndexOf( startPathID );
                if( startIdx < 0 ) {
                    return new List<string>();
                }
                visited[ startIdx ] = true;
                int visitedCount = 1;

                while( visitedCount < pathIDList.Count ) {
                    double minDistanceSq = double.MaxValue;
                    int nearestIdx = -1;
                    gp_Pnt nearestPoint = null;
                    for( int i = 0; i < pathIDList.Count; i++ ) {
                        if( visited[ i ] ) {
                            continue;
                        }
                        gp_Pnt nextStartPoint = CacheHelper.GetProcessStartPoint( pathIDList[ i ] )?.Point;
                        double distanceSq;
                        if( nextStartPoint == null ) {
                            distanceSq = double.MaxValue;
                        }
                        else {
                            distanceSq = currentPoint.SquareDistance( nextStartPoint );
                        }
                        if( distanceSq < minDistanceSq ) {
                            minDistanceSq = distanceSq;
                            nearestPoint = nextStartPoint;
                            nearestIdx = i;
                        }
                    }
                    if( nearestIdx != -1 ) {
                        currentPoint = nearestPoint;
                        visited[ nearestIdx ] = true;
                        visitedCount++;
                        newPathIDList.Add( pathIDList[ nearestIdx ] );
                    }
                    else {
                        break;
                    }
                }
                return newPathIDList;
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] SortByShortestPath failed: {ex.Message}" );
                return pathIDList != null ? new List<string>( pathIDList ) : new List<string>();
            }
        }

        public static List<string> SortByExtrusionThenRotation( List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation, bool isExtrusionDescending, bool isRotationStartDescending )
        {
            try {
                return SortForCylinder( pathIDList, axisDir, axisLocation, isAxialFirst: true, isPrimaryDescending: isExtrusionDescending, isSecondaryStartDescending: isRotationStartDescending );
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] SortByExtrusionThenRotation failed: {ex.Message}" );
                return pathIDList != null ? new List<string>( pathIDList ) : new List<string>();
            }
        }

        public static List<string> SortByRotationThenExtrusion( List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation, bool isRotationDescending, bool isExtrusionStartDescending )
        {
            try {
                return SortForCylinder( pathIDList, axisDir, axisLocation, isAxialFirst: false, isPrimaryDescending: isRotationDescending, isSecondaryStartDescending: isExtrusionStartDescending );
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] SortByRotationThenExtrusion failed: {ex.Message}" );
                return pathIDList != null ? new List<string>( pathIDList ) : new List<string>();
            }
        }

        static List<string> SortForCylinder( List<string> pathIDList, gp_Dir axisDir, gp_Pnt axisLocation, bool isAxialFirst, bool isPrimaryDescending, bool isSecondaryStartDescending )
        {
            if( pathIDList == null || pathIDList.Count == 0 ) {
                return new List<string>();
            }

            try {
                if( axisDir == null || axisLocation == null ) {
                    System.Diagnostics.Debug.WriteLine( "[PathSortHelper] SortForCylinder: null axis parameters" );
                    return new List<string>( pathIDList );
                }

                // build reference frame perpendicular to axis for angle measurement
                gp_Dir refX = BuildPerpendicularDir( axisDir );
                gp_Dir refY = new gp_Dir( axisDir.Crossed( refX ).XYZ() );

                // convert each path center to unrolled 2D coordinate (axial, angular)
                List<CylinderSortInfo> pathInfoList = new List<CylinderSortInfo>();
                foreach( string pathID in pathIDList ) {
                    try {
                        if( !DataGettingHelper.GetPathCacheByID( pathID, out IPathCache pathCache ) ) {
                            continue;
                        }

                        gp_Pnt center = pathCache.ComputeRefCenterDir?.Location();
                        if( center == null ) {
                            continue;
                        }

                        double vx = center.X() - axisLocation.X();
                        double vy = center.Y() - axisLocation.Y();
                        double vz = center.Z() - axisLocation.Z();

                        // axial coordinate
                        double axial = vx * axisDir.X() + vy * axisDir.Y() + vz * axisDir.Z();

                        // radial vector
                        double radialX = vx - axial * axisDir.X();
                        double radialY = vy - axial * axisDir.Y();
                        double radialZ = vz - axial * axisDir.Z();

                        // angular coordinate
                        double projX = radialX * refX.X() + radialY * refX.Y() + radialZ * refX.Z();
                        double projY = radialX * refY.X() + radialY * refY.Y() + radialZ * refY.Z();
                        double angular = 0;
                        double radialLength = Math.Sqrt( projX * projX + projY * projY );

                        if( radialLength > RADIAL_EPSILON ) {
                            angular = Math.Atan2( projY, projX );
                            if( angular < 0 ) {
                                angular += 2.0 * Math.PI;
                            }
                        }
                        pathInfoList.Add( new CylinderSortInfo( pathID, axial, angular ) );
                    }
                    catch( Exception ex ) {
                        System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] Failed to process path {pathID}: {ex.Message}" );
                        // 繼續處理其他路徑
                    }
                }

                if( pathInfoList.Count == 0 ) {
                    System.Diagnostics.Debug.WriteLine( "[PathSortHelper] SortForCylinder: no valid paths processed" );
                    return new List<string>( pathIDList );
                }

                return SortCylinderPathSnake( pathInfoList, isAxialFirst, isPrimaryDescending, isSecondaryStartDescending );
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] SortForCylinder failed: {ex.Message}" );
                return new List<string>( pathIDList );
            }
        }

        static List<string> SortCylinderPathSnake( List<CylinderSortInfo> pathInfoList, bool isAxialFirst, bool isPrimaryDescending, bool isSecondaryStartDescending )
        {
            try {
                if( pathInfoList == null || pathInfoList.Count == 0 ) {
                    return new List<string>();
                }

                double primaryTolerance = ComputeGroupTolerance( pathInfoList, isAxialFirst );

                List<CylinderSortInfo> sorted = isAxialFirst
                    ? pathInfoList.OrderBy( info => info.Axial ).ToList()
                    : pathInfoList.OrderBy( info => info.Angular ).ToList();

                if( sorted.Count == 0 ) {
                    return new List<string>();
                }

                int groupIndex = 0;
                double prevValue = isAxialFirst ? sorted[ 0 ].Axial : sorted[ 0 ].Angular;
                foreach( CylinderSortInfo info in sorted ) {
                    double currentValue = isAxialFirst ? info.Axial : info.Angular;
                    if( currentValue - prevValue > primaryTolerance ) {
                        groupIndex++;
                    }
                    info.GroupIndex = groupIndex;
                    prevValue = currentValue;
                }

                // angular wraps at 2PI: the zero degree column may split into a head group
                // and a tail group (e.g. 0 deg and 360 deg), merge them back when wrap gap is small
                if( !isAxialFirst && groupIndex > 0 ) {
                    double wrapGap = 2.0 * Math.PI - sorted.Last().Angular + sorted.First().Angular;
                    if( wrapGap <= primaryTolerance ) {
                        int lastGroup = groupIndex;
                        foreach( CylinderSortInfo info in sorted ) {
                            if( info.GroupIndex == lastGroup ) {
                                info.GroupIndex = 0;
                            }
                        }
                        groupIndex--;
                    }
                }
#if DEBUG
                System.Diagnostics.Debug.WriteLine( string.Format( "[CylSort] isAxialFirst={0}, groups={1}, tolerance={2:F4}", isAxialFirst, groupIndex + 1, primaryTolerance ) );
#endif
                int totalGroups = groupIndex + 1;
                List<string> result = new List<string>();
                for( int i = 0; i < totalGroups; i++ ) {
                    int g = isPrimaryDescending ? ( totalGroups - 1 - i ) : i;
                    List<CylinderSortInfo> groupPaths = sorted.Where( info => info.GroupIndex == g ).ToList();

                    if( groupPaths.Count == 0 ) {
                        continue;
                    }

                    bool isDescending = ( i % 2 == 0 ) ? isSecondaryStartDescending : !isSecondaryStartDescending;
                    if( isDescending ) {
                        groupPaths = isAxialFirst
                            ? groupPaths.OrderByDescending( info => info.Angular ).ToList()
                            : groupPaths.OrderByDescending( info => info.Axial ).ToList();
                    }
                    else {
                        groupPaths = isAxialFirst
                            ? groupPaths.OrderBy( info => info.Angular ).ToList()
                            : groupPaths.OrderBy( info => info.Axial ).ToList();
                    }
                    result.AddRange( groupPaths.Select( info => info.PathID ) );
                }
                return result;
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] SortCylinderPathSnake failed: {ex.Message}" );
                return pathInfoList?.Select( info => info.PathID ).ToList() ?? new List<string>();
            }
        }

        static double ComputeGroupTolerance( List<CylinderSortInfo> pathInfoList, bool isAxialFirst )
        {
            try {
                if( pathInfoList == null || pathInfoList.Count == 0 ) {
                    return MIN_GROUP_TOLERANCE;
                }

                // sort primary values internally so this method is independent of input order
                List<double> values = isAxialFirst
                    ? pathInfoList.Select( info => info.Axial ).OrderBy( v => v ).ToList()
                    : pathInfoList.Select( info => info.Angular ).OrderBy( v => v ).ToList();

                if( values.Count == 0 ) {
                    return MIN_GROUP_TOLERANCE;
                }

                if( values.Count == 1 ) {
                    return double.MaxValue;
                }

                double spread = values.Last() - values.First();

                // all paths at the same primary position: one single group
                if( spread < MIN_GROUP_TOLERANCE ) {
                    return double.MaxValue;
                }

                // the largest gap between consecutive values separates rows / columns
                double maxGap = 0;
                for( int i = 1; i < values.Count; i++ ) {
                    double gap = values[ i ] - values[ i - 1 ];
                    if( gap > maxGap ) {
                        maxGap = gap;
                    }
                }

                // half of the largest gap splits in-row noise from row boundaries,
                // spread / divisor keeps the legacy upper bound for dense even spreads
                double tolerance = Math.Min( maxGap * 0.5, spread / GROUP_TOLERANCE_DIVISOR );
                return Math.Max( tolerance, MIN_GROUP_TOLERANCE );
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] ComputeGroupTolerance failed: {ex.Message}" );
                return MIN_GROUP_TOLERANCE;
            }
        }

        const double GROUP_TOLERANCE_DIVISOR = 50.0;
        const double MIN_GROUP_TOLERANCE = 1e-3;

        static gp_Dir BuildPerpendicularDir( gp_Dir axisDir )
        {
            try {
                if( axisDir == null ) {
                    return new gp_Dir( 1, 0, 0 );
                }

                gp_Dir candidate;
                if( Math.Abs( axisDir.X() ) < 0.9 ) {
                    candidate = new gp_Dir( 1, 0, 0 );
                }
                else {
                    candidate = new gp_Dir( 0, 1, 0 );
                }
                gp_XYZ cross = axisDir.XYZ().Crossed( candidate.XYZ() );
                return new gp_Dir( cross );
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( $"[PathSortHelper] BuildPerpendicularDir failed: {ex.Message}" );
                return new gp_Dir( 1, 0, 0 );
            }
        }

        class CylinderSortInfo
        {
            public CylinderSortInfo( string pathID, double axial, double angular )
            {
                PathID = pathID;
                Axial = axial;
                Angular = angular;
            }

            public string PathID
            {
                get;
            }

            public double Axial
            {
                get; set;
            }

            public double Angular
            {
                get; set;
            }

            public int GroupIndex
            {
                get; set;
            }
        }
    }
}
