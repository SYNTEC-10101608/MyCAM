using OCC.Bnd;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepAlgoAPI;
using OCC.BRepBndLib;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
    public static class RevolutionAxisHelper
    {
        /// <summary>
        /// 找出 shape 的最佳迴轉軸候選
        /// </summary>
        /// <param name="shape">輸入 3D shape</param>
        /// <param name="binCount">沿軸切片數量，預設 10</param>
        /// <param name="sectionDeflection">截面離散精度(弦高偏差)，預設 0.1</param>
        /// <param name="w1">指標a(重心偏移)權重</param>
        /// <param name="w2">指標b(尺寸變異)權重</param>
        /// <returns>最佳迴轉軸</returns>
        public static gp_Ax1 FindRevolutionAxis( TopoDS_Shape shape,
            int binCount = 10, double sectionDeflection = 0.1,
            double w1 = 1.0, double w2 = 1.0 )
        {
            // Step 1: Compute OBB and extract 3 candidate axes
            Bnd_OBB obb = new Bnd_OBB();
            BRepBndLib.AddOBB( shape, ref obb, true, true, false );

            gp_Pnt obbCenter = new gp_Pnt( obb.Center() );
            gp_XYZ xAxis = obb.XDirection();
            gp_XYZ yAxis = obb.YDirection();
            gp_XYZ zAxis = obb.ZDirection();
            double[] halfSizes = new double[] { obb.XHSize(), obb.YHSize(), obb.ZHSize() };
            gp_XYZ[] axes = new gp_XYZ[] { xAxis, yAxis, zAxis };

            // OBB diagonal length for normalization
            double obbDiagonal = 2.0 * Math.Sqrt(
                halfSizes[0] * halfSizes[0] +
                halfSizes[1] * halfSizes[1] +
                halfSizes[2] * halfSizes[2] );

            if( obbDiagonal < 1e-12 ) {
                return new gp_Ax1( obbCenter, new gp_Dir( 0, 0, 1 ) );
            }

            // Step 2-4: Evaluate each candidate axis
            double[] scores = new double[3];
            for( int i = 0; i < 3; i++ ) {
                gp_Dir axisDir = new gp_Dir( axes[i] );
                gp_Ax1 candidateAxis = new gp_Ax1( obbCenter, axisDir );
                double axisHalfLength = halfSizes[i];

                scores[i] = EvaluateAxis( shape, candidateAxis, axisHalfLength, obbDiagonal, binCount, sectionDeflection, w1, w2 );
            }

            // Step 5-6: Select best axis with degeneracy handling
            int bestIndex = 0;
            double bestScore = scores[0];
            for( int i = 1; i < 3; i++ ) {
                if( scores[i] < bestScore ) {
                    bestScore = scores[i];
                    bestIndex = i;
                }
            }

            // Degeneracy: all scores within 1e-6
            double maxScore = scores.Max();
            double minScore = scores.Min();
            if( maxScore - minScore < 1e-6 ) {
                // Pick longest axis
                int longestIndex = 0;
                double longestHalf = halfSizes[0];
                for( int i = 1; i < 3; i++ ) {
                    if( halfSizes[i] > longestHalf ) {
                        longestHalf = halfSizes[i];
                        longestIndex = i;
                    }
                }
                // If axis lengths also within 1e-6, return Z axis
                double maxHalf = halfSizes.Max();
                double minHalf = halfSizes.Min();
                if( maxHalf - minHalf < 1e-6 ) {
                    return new gp_Ax1( obbCenter, new gp_Dir( zAxis ) );
                }
                return new gp_Ax1( obbCenter, new gp_Dir( axes[longestIndex] ) );
            }

            return new gp_Ax1( obbCenter, new gp_Dir( axes[bestIndex] ) );
        }

        static double EvaluateAxis( TopoDS_Shape shape, gp_Ax1 axis, double axisHalfLength,
            double obbDiagonal, int binCount, double sectionDeflection, double w1, double w2 )
        {
            gp_Pnt axisOrigin = axis.Location();
            gp_Dir axisDir = axis.Direction();
            gp_Vec axisVec = new gp_Vec( axisDir );

            // Section planes are placed at bin centers within the OBB extent
            // Range: [-axisHalfLength, +axisHalfLength], bin centers at offset from center
            List<double> centroidDistances = new List<double>();
            List<double> equivalentRadii = new List<double>();

            for( int bin = 0; bin < binCount; bin++ ) {
                // Bin center position: from 0.5/binCount to (binCount-0.5)/binCount mapped to [-halfLen, +halfLen]
                double t = ( bin + 0.5 ) / binCount; // [0.5/n, (n-0.5)/n]
                double offset = -axisHalfLength + 2.0 * axisHalfLength * t;

                // Create section plane at this offset along axis
                gp_Pnt planeCenter = new gp_Pnt(
                    axisOrigin.X() + axisVec.X() * offset,
                    axisOrigin.Y() + axisVec.Y() * offset,
                    axisOrigin.Z() + axisVec.Z() * offset );
                gp_Pln sectionPlane = new gp_Pln( planeCenter, axisDir );

                // Get section points
                List<gp_Pnt> sectionPoints = GetSectionPoints( shape, sectionPlane, sectionDeflection );
                if( sectionPoints == null || sectionPoints.Count < 2 ) {
                    continue; // Skip empty bins
                }

                // Compute centroid of section points
                double cx = 0, cy = 0, cz = 0;
                foreach( gp_Pnt pt in sectionPoints ) {
                    cx += pt.X();
                    cy += pt.Y();
                    cz += pt.Z();
                }
                int n = sectionPoints.Count;
                gp_Pnt centroid = new gp_Pnt( cx / n, cy / n, cz / n );

                // Distance from centroid to axis line
                gp_Vec centroidToOrigin = new gp_Vec( axisOrigin, centroid );
                gp_Vec projection = axisVec.Multiplied( centroidToOrigin.Dot( axisVec ) );
                gp_Vec perpendicular = centroidToOrigin - projection;
                double centroidDist = perpendicular.Magnitude();
                centroidDistances.Add( centroidDist );

                // Equivalent radius: average distance of section points to axis line
                double sumR = 0;
                foreach( gp_Pnt pt in sectionPoints ) {
                    gp_Vec ptToOrigin = new gp_Vec( axisOrigin, pt );
                    gp_Vec ptProj = axisVec.Multiplied( ptToOrigin.Dot( axisVec ) );
                    gp_Vec ptPerp = ptToOrigin - ptProj;
                    sumR += ptPerp.Magnitude();
                }
                double avgR = sumR / n;
                equivalentRadii.Add( avgR );
            }

            if( centroidDistances.Count == 0 ) {
                return double.MaxValue;
            }

            // Indicator a: RMS of centroid distances / OBB diagonal
            double sumSqA = 0;
            foreach( double d in centroidDistances ) {
                double normalized = d / obbDiagonal;
                sumSqA += normalized * normalized;
            }
            double a = Math.Sqrt( sumSqA / centroidDistances.Count );

            // Indicator b: standard deviation of equivalent radii / OBB diagonal
            double meanR = equivalentRadii.Average();
            double sumSqB = 0;
            foreach( double r in equivalentRadii ) {
                double diff = r - meanR;
                sumSqB += diff * diff;
            }
            double stdR = Math.Sqrt( sumSqB / equivalentRadii.Count );
            double b = stdR / obbDiagonal;

            // Combined score
            return Math.Sqrt( w1 * a * a + w2 * b * b );
        }

        static List<gp_Pnt> GetSectionPoints( TopoDS_Shape shape, gp_Pln plane, double deflection )
        {
            List<gp_Pnt> points = new List<gp_Pnt>();

            try {
                BRepAlgoAPI_Section section = new BRepAlgoAPI_Section( shape, plane, false );
                section.ComputePCurveOn1( false );
                section.Approximation( false );
                section.Build();
                if( !section.IsDone() ) {
                    return points;
                }

                TopoDS_Shape sectionShape = section.Shape();
                if( sectionShape == null || sectionShape.IsNull() ) {
                    return points;
                }

                // Iterate edges and discretize
                TopExp_Explorer explorer = new TopExp_Explorer( sectionShape, TopAbs_ShapeEnum.TopAbs_EDGE );
                while( explorer.More() ) {
                    TopoDS_Edge edge = TopoDS.ToEdge( explorer.Current() );
                    if( edge != null && !edge.IsNull() ) {
                        BRepAdaptor_Curve curve = new BRepAdaptor_Curve( edge );
                        double first = curve.FirstParameter();
                        double last = curve.LastParameter();

                        GCPnts_QuasiUniformDeflection discretizer =
                            new GCPnts_QuasiUniformDeflection( curve, deflection, first, last );

                        if( discretizer.IsDone() ) {
                            for( int i = 1; i <= discretizer.NbPoints(); i++ ) {
                                points.Add( discretizer.Value( i ) );
                            }
                        }
                    }
                    explorer.Next();
                }
            }
            catch {
                // Section may fail for degenerate cases
            }

            return points;
        }
    }
}
