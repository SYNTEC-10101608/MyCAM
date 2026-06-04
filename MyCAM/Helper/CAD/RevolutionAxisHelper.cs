using OCC.Bnd;
using OCC.Bnd;
using OCC.BRep;
using OCC.BRepBndLib;
using OCC.BRepMesh;
using OCC.gp;
using OCC.Poly;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopLoc;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public static gp_Ax1 FindRevolutionAxis( TopoDS_Shape shape, out double axisHalfLength,
            int binCount = 10, double sectionDeflection = 0.1,
            double w1 = 1.0, double w2 = 1.0 )
        {
            axisHalfLength = 0;

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
                axisHalfLength = 0;
                return new gp_Ax1( obbCenter, new gp_Dir( 0, 0, 1 ) );
            }

            // Step 2-4: Get mesh points once, then evaluate each candidate axis
            List<gp_Pnt> allPoints = GetMeshPoints( shape, sectionDeflection );
            double[] scores = new double[3];
            for( int i = 0; i < 3; i++ ) {
                gp_Dir axisDir = new gp_Dir( axes[i] );
                gp_Ax1 candidateAxis = new gp_Ax1( obbCenter, axisDir );
                double candidateHalfLength = halfSizes[i];

                scores[i] = EvaluateAxis( allPoints, candidateAxis, candidateHalfLength, obbDiagonal, binCount, w1, w2 );
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
            gp_Ax1 resultAxis;
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
                    axisHalfLength = halfSizes[2];
                    bestIndex = 2;
                    resultAxis = new gp_Ax1( obbCenter, new gp_Dir( zAxis ) );
                }
                else {
                    axisHalfLength = halfSizes[longestIndex];
                    bestIndex = longestIndex;
                    resultAxis = new gp_Ax1( obbCenter, new gp_Dir( axes[longestIndex] ) );
                }
            }
            else {
                axisHalfLength = halfSizes[bestIndex];
                resultAxis = new gp_Ax1( obbCenter, new gp_Dir( axes[bestIndex] ) );
            }

            // Write diagnostic log
            WriteDiagnosticLog( obbCenter, axes, halfSizes, obbDiagonal, scores, bestIndex, binCount, sectionDeflection, w1, w2, shape );

            return resultAxis;
        }

        static double EvaluateAxis( List<gp_Pnt> allPoints, gp_Ax1 axis, double axisHalfLength,
            double obbDiagonal, int binCount, double w1, double w2 )
        {
            gp_Pnt axisOrigin = axis.Location();
            gp_Vec axisVec = new gp_Vec( axis.Direction() );

            // Bin the points along the axis
            List<List<gp_Pnt>> bins = new List<List<gp_Pnt>>();
            for( int i = 0; i < binCount; i++ ) {
                bins.Add( new List<gp_Pnt>() );
            }

            double fullLength = 2.0 * axisHalfLength;
            foreach( gp_Pnt pt in allPoints ) {
                gp_Vec ptVec = new gp_Vec( axisOrigin, pt );
                double proj = ptVec.Dot( axisVec );
                double normalized = ( proj + axisHalfLength ) / fullLength;
                int binIdx = (int)( normalized * binCount );
                if( binIdx < 0 ) binIdx = 0;
                if( binIdx >= binCount ) binIdx = binCount - 1;
                bins[binIdx].Add( pt );
            }

            // Evaluate each bin
            List<double> centroidDistances = new List<double>();
            List<double> equivalentRadii = new List<double>();

            for( int bin = 0; bin < binCount; bin++ ) {
                if( bins[bin].Count < 2 ) continue;

                List<gp_Pnt> binPoints = bins[bin];
                int n = binPoints.Count;

                double cx = 0, cy = 0, cz = 0;
                foreach( gp_Pnt pt in binPoints ) { cx += pt.X(); cy += pt.Y(); cz += pt.Z(); }
                gp_Pnt centroid = new gp_Pnt( cx / n, cy / n, cz / n );

                gp_Vec c2o = new gp_Vec( axisOrigin, centroid );
                gp_Vec cProj = axisVec.Multiplied( c2o.Dot( axisVec ) );
                double centroidDist = ( c2o - cProj ).Magnitude();
                centroidDistances.Add( centroidDist );

                double sumR = 0;
                foreach( gp_Pnt pt in binPoints ) {
                    gp_Vec p2o = new gp_Vec( axisOrigin, pt );
                    gp_Vec pProj = axisVec.Multiplied( p2o.Dot( axisVec ) );
                    sumR += ( p2o - pProj ).Magnitude();
                }
                equivalentRadii.Add( sumR / n );
            }

            if( centroidDistances.Count == 0 ) return double.MaxValue;

            double sumSqA = centroidDistances.Sum( d => { double norm = d / obbDiagonal; return norm * norm; } );
            double a = Math.Sqrt( sumSqA / centroidDistances.Count );

            double meanR = equivalentRadii.Average();
            double sumSqB = equivalentRadii.Sum( r => ( r - meanR ) * ( r - meanR ) );
            double stdR = Math.Sqrt( sumSqB / equivalentRadii.Count );
            double b = stdR / obbDiagonal;

            return Math.Sqrt( w1 * a * a + w2 * b * b );
        }

        static List<gp_Pnt> GetMeshPoints( TopoDS_Shape shape, double deflection )
        {
            List<gp_Pnt> points = new List<gp_Pnt>();

            BRepMesh_IncrementalMesh mesh = new BRepMesh_IncrementalMesh( shape, deflection );
            mesh.Perform();

            TopExp_Explorer explorer = new TopExp_Explorer( shape, TopAbs_ShapeEnum.TopAbs_FACE );
            while( explorer.More() ) {
                TopoDS_Face face = TopoDS.ToFace( explorer.Current() );
                if( face != null && !face.IsNull() ) {
                    TopLoc_Location loc = new TopLoc_Location();
                    Poly_Triangulation tri = BRep_Tool.Triangulation( face, ref loc );
                    if( tri != null && !tri.IsNull() ) {
                        gp_Trsf trsf = loc.IsIdentity() ? new gp_Trsf() : loc.Transformation();
                        int nbNodes = tri.NbNodes();
                        for( int i = 1; i <= nbNodes; i++ ) {
                            gp_Pnt pt = tri.Node( i );
                            if( !loc.IsIdentity() ) {
                                pt.Transform( trsf );
                            }
                            points.Add( pt );
                        }
                    }
                }
                explorer.Next();
            }

            return points;
        }

        static void WriteDiagnosticLog( gp_Pnt obbCenter, gp_XYZ[] axes, double[] halfSizes, double obbDiagonal,
            double[] scores, int bestIndex, int binCount, double sectionDeflection, double w1, double w2, TopoDS_Shape shape )
        {
            try {
                string[] axisNames = new string[] { "X", "Y", "Z" };
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "=== Revolution Axis Diagnostic Log ===" );
                sb.AppendLine( $"Parameters: binCount={binCount}, meshDeflection={sectionDeflection}, w1={w1}, w2={w2}" );
                sb.AppendLine( $"OBB Center: ({obbCenter.X():F4}, {obbCenter.Y():F4}, {obbCenter.Z():F4})" );
                sb.AppendLine( $"OBB Diagonal: {obbDiagonal:F6}" );

                List<gp_Pnt> allPoints = GetMeshPoints( shape, sectionDeflection );
                sb.AppendLine( $"Total mesh points: {allPoints.Count}" );
                sb.AppendLine();

                for( int i = 0; i < 3; i++ ) {
                    sb.AppendLine( $"--- Candidate Axis {axisNames[i]} ---" );
                    sb.AppendLine( $"  Direction: ({axes[i].X():F6}, {axes[i].Y():F6}, {axes[i].Z():F6})" );
                    sb.AppendLine( $"  HalfSize: {halfSizes[i]:F6}" );
                    sb.AppendLine( $"  Combined Score: {scores[i]:F8}" );

                    gp_Vec axisVec = new gp_Vec( new gp_Dir( axes[i] ) );
                    List<List<gp_Pnt>> bins = new List<List<gp_Pnt>>();
                    for( int b = 0; b < binCount; b++ ) bins.Add( new List<gp_Pnt>() );

                    double fullLength = 2.0 * halfSizes[i];
                    foreach( gp_Pnt pt in allPoints ) {
                        gp_Vec ptVec = new gp_Vec( obbCenter, pt );
                        double proj = ptVec.Dot( axisVec );
                        double norm = ( proj + halfSizes[i] ) / fullLength;
                        int binIdx = (int)( norm * binCount );
                        if( binIdx < 0 ) binIdx = 0;
                        if( binIdx >= binCount ) binIdx = binCount - 1;
                        bins[binIdx].Add( pt );
                    }

                    List<double> centroidDists = new List<double>();
                    List<double> equivRadii = new List<double>();

                    for( int bin = 0; bin < binCount; bin++ ) {
                        if( bins[bin].Count < 2 ) {
                            sb.AppendLine( $"  Bin {bin}: EMPTY (pts={bins[bin].Count})" );
                            continue;
                        }
                        int n = bins[bin].Count;
                        double cx = 0, cy = 0, cz = 0;
                        foreach( gp_Pnt pt in bins[bin] ) { cx += pt.X(); cy += pt.Y(); cz += pt.Z(); }
                        gp_Pnt centroid = new gp_Pnt( cx / n, cy / n, cz / n );

                        gp_Vec c2o = new gp_Vec( obbCenter, centroid );
                        gp_Vec cProj = axisVec.Multiplied( c2o.Dot( axisVec ) );
                        double centroidDist = ( c2o - cProj ).Magnitude();
                        centroidDists.Add( centroidDist );

                        double sumR = 0;
                        foreach( gp_Pnt pt in bins[bin] ) {
                            gp_Vec p2o = new gp_Vec( obbCenter, pt );
                            gp_Vec pProj = axisVec.Multiplied( p2o.Dot( axisVec ) );
                            sumR += ( p2o - pProj ).Magnitude();
                        }
                        double avgR = sumR / n;
                        equivRadii.Add( avgR );

                        sb.AppendLine( $"  Bin {bin}: pts={n} | centroidDist={centroidDist:F6} | avgRadius={avgR:F6}" );
                    }

                    if( centroidDists.Count > 0 ) {
                        double rmsA = Math.Sqrt( centroidDists.Sum( d => ( d / obbDiagonal ) * ( d / obbDiagonal ) ) / centroidDists.Count );
                        double meanR = equivRadii.Average();
                        double stdR = Math.Sqrt( equivRadii.Sum( r => ( r - meanR ) * ( r - meanR ) ) / equivRadii.Count );
                        double normB = stdR / obbDiagonal;
                        sb.AppendLine( $"  >> Indicator a (centroid RMS/diag): {rmsA:F8}" );
                        sb.AppendLine( $"  >> Indicator b (stdR/diag): {normB:F8}" );
                        sb.AppendLine( $"  >> Combined: {Math.Sqrt( w1 * rmsA * rmsA + w2 * normB * normB ):F8}" );
                    }
                    sb.AppendLine();
                }

                sb.AppendLine( $"=== BEST AXIS: {axisNames[bestIndex]} (score={scores[bestIndex]:F8}) ===" );
                string logPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "RevolutionAxis_Diagnostic.txt" );
                File.WriteAllText( logPath, sb.ToString() );
            }
            catch {
                // Logging should not break the algorithm
            }
        }
    }
}
