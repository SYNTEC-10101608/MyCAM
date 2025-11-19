using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.BOPTools;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

namespace MyCAM.Helper
{
	internal static class PretreatmentHelper
	{
		// public function area
		public static List<List<CADPoint>> SplitArcEdge( TopoDS_Edge edge, TopoDS_Face shellFace, out List<double> eachSegmentLength, out List<double> dEachArcLength, out List<double> dEachChordLength, double maxAngleRad = Math.PI / 2 )
		{
			eachSegmentLength = new List<double>();
			dEachArcLength = new List<double>();
			dEachChordLength = new List<double>();
			List<List<CADPoint>> arcCADPointList = new List<List<CADPoint>>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return null;
			}
			if( !GeometryTool.IsCircularArc( edge, out _, out double R, out _, out double angle ) ) {
				return null;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() || dStartU == dEndU ) {
				return null;
			}
			List<double> segmentParamList = new List<double>();

			// this arc angel is small enough just split by chord error
			if( angle <= maxAngleRad ) {

				// Tuple<double Deflection, double ChordLength, double ArcLength>
				Tuple<double, double, double> deflctionResult = CalculateDeflectionForCircularEdge( adaptorCurve, dStartU, dEndU, R );
				segmentParamList = ChordErrorSplit( adaptorCurve, edge, shellFace, dStartU, dEndU, deflctionResult.Item1 );

				// out put
				arcCADPointList.Add( GetCADPointsFromCurveParams( segmentParamList, edge, shellFace, adaptorCurve ) );
				dEachChordLength.Add( deflctionResult.Item2 );
				dEachArcLength.Add( deflctionResult.Item3 );
				eachSegmentLength.Add( GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU ) );
				return arcCADPointList;
			}

			// split big arc to small arc
			int nSplitCount = (int)Math.Ceiling( angle / maxAngleRad );
			double dDeltaU = ( dEndU - dStartU ) / nSplitCount;

			// collect parameter lists for each segment of the curve
			for( int i = 0; i < nSplitCount; i++ ) {
				segmentParamList.Add( dStartU + i * dDeltaU );
			}
			segmentParamList.Add( dEndU );

			// Tuple<double Deflection, double ChordLength, double ArcLength>
			Tuple<double, double, double> dDeflction = CalculateDeflectionForCircularEdge( adaptorCurve, segmentParamList.First(), segmentParamList[ 1 ], R );

			for( int i = 0; i < segmentParamList.Count - 1; i++ ) {
				double thisStartU = segmentParamList[ i ];
				double thisEndU = segmentParamList[ i + 1 ];

				// param in this segment will be ordered done , will get ordered points
				List<double> dThisPartParam = ChordErrorSplit( adaptorCurve, edge, shellFace, thisStartU, thisEndU, dDeflction.Item1 );
				eachSegmentLength.Add( GCPnts_AbscissaPoint.Length( adaptorCurve, thisStartU, thisEndU ) );
				dEachArcLength.Add( dDeflction.Item3 );
				dEachChordLength.Add( dDeflction.Item2 );
				arcCADPointList.Add( GetCADPointsFromCurveParams( dThisPartParam, edge, shellFace, adaptorCurve ) );
			}

			// reverse each segment
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				arcCADPointList.Reverse();
			}
			return arcCADPointList;
		}

		public static List<List<CADPoint>> GetBsplineToEdgeList( TopoDS_Edge edge, TopoDS_Face shellFace, out List<double> eachSegmentLength, out List<double> eachArcLength )
		{
			eachSegmentLength = new List<double>();
			eachArcLength = new List<double>();
			List<List<CADPoint>> CADPointList = new List<List<CADPoint>>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return null;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null ) {
				return null;
			}

			// split this bspline by chord error
			List<double> dSegmentParamList = ChordErrorSplit( adaptorCurve, edge, shellFace, dStartU, dEndU );
			for( int i = 0; i < dSegmentParamList.Count - 1; i++ ) {

				// each part split by equal length
				double dthisEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dSegmentParamList[ i ], dSegmentParamList[ i + 1 ] );
				List<double> thisPartParamList = GetCurveEachSegmentParamByLength( dSegmentParamList[ i ], dSegmentParamList[ i + 1 ], PRECISION_MAX_LENGTH, dthisEdgeLength, out double dEachArcLength );
				CADPointList.Add( GetCADPointsFromCurveParams( thisPartParamList, edge, shellFace, adaptorCurve ) );
				eachSegmentLength.Add( dthisEdgeLength );
				eachArcLength.Add( dEachArcLength );
			}
			return CADPointList;
		}

		public static List<double> ChordErrorSplit( BRepAdaptor_Curve adaptorCurve, TopoDS_Edge edge, TopoDS_Face shellFace, double dStartU, double dEndU, double dDeflection = PRECISION_DEFLECTION )
		{
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return new List<double>();
			}
			// break the curve into segments with given deflection precision
			GCPnts_QuasiUniformDeflection qUD = new GCPnts_QuasiUniformDeflection( adaptorCurve, dDeflection, dStartU, dEndU );

			// collect parameter lists for each segment of the curve
			List<double> segmentParamList = new List<double>();
			for( int i = 1; i < qUD.NbPoints(); i++ ) {
				segmentParamList.Add( qUD.Parameter( i ) );
			}
			segmentParamList.Add( dEndU );

			// reverse the segment parameters if the edge is reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			return segmentParamList;
		}

		public static List<double> GetCurveEachSegmentParamByLength( double dStartU, double dEndU, double dMaxSegmentLength, double dEdgeLength, out double dEachArcLength )
		{
			int nSubSegmentCount = (int)Math.Ceiling( dEdgeLength / dMaxSegmentLength );

			// make sure to get odd count of points to get middle of edge for arc
			if( nSubSegmentCount % 2 == 0 ) {
				nSubSegmentCount += 1;
			}
			dEachArcLength = dEdgeLength / nSubSegmentCount;
			double dDeltaU = ( dEndU - dStartU ) / nSubSegmentCount;
			List<double> segmentParamList = new List<double>();
			for( int i = 0; i < nSubSegmentCount; i++ ) {
				double param = dStartU + i * dDeltaU;
				segmentParamList.Add( param );
			}
			segmentParamList.Add( dEndU );
			return segmentParamList;
		}

		public static List<CADPoint> GetSegmentPointsByEqualLength( TopoDS_Edge lineEdge, TopoDS_Face shellFace, double dMaxSegmentLength, out double dEdgeLength, out double dEachArcLength, out double dEachChordLength )
		{
			dEdgeLength = 0;
			dEachArcLength = 0;
			dEachChordLength = 0;

			// get curve parameters
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( lineEdge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return null;
			}

			// curve length from parameter StartU toEndU 
			dEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );

			List<double> segmentParamList = GetCurveEachSegmentParamByLength( dStartU, dEndU, dMaxSegmentLength, dEdgeLength, out dEachArcLength );
			if( segmentParamList.Count < 2 ) {
				return null;
			}

			// need to consider orientation
			if( lineEdge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			List<CADPoint> oneSegmentPointList = GetCADPointsFromCurveParams( segmentParamList, lineEdge, shellFace, adaptorCurve );

			if( oneSegmentPointList.Count < 2 ) {
				return null;
			}
			dEachChordLength = oneSegmentPointList.First().Point.Distance( oneSegmentPointList[ 1 ].Point );
			return oneSegmentPointList;
		}

		// private function area

		static List<CADPoint> GetCADPointsFromCurveParams( List<double> segmentParamList, TopoDS_Edge edge, TopoDS_Face shellFace, BRepAdaptor_Curve adC, bool IsTanVecAdjusted = false )
		{
			List<CADPoint> oneSegmentPointList = new List<CADPoint>();
			for( int i = 0; i < segmentParamList.Count; i++ ) {
				double U = segmentParamList[ i ];

				// get point
				gp_Pnt point = adC.Value( U );

				// get shell normal (1st)
				gp_Dir normalVec_1st = new gp_Dir();
				BOPTools_AlgoTools3D.GetNormalToFaceOnEdge( edge, shellFace, U, ref normalVec_1st );
				if( shellFace.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					normalVec_1st.Reverse();
				}

				// TODO: get solid normal (2nd)
				gp_Dir normalVec_2nd = new gp_Dir( normalVec_1st.XYZ() );

				// get tangent
				gp_Vec tangentVec = new gp_Vec();
				gp_Pnt _p = new gp_Pnt();
				adC.D1( U, ref _p, ref tangentVec );
				if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					tangentVec.Reverse();
				}
				CADPoint cadPoint = new CADPoint( point, normalVec_1st, normalVec_2nd, new gp_Dir( tangentVec ) );
				oneSegmentPointList.Add( cadPoint );
			}
			return oneSegmentPointList;
		}

		static BRepAdaptor_Curve TryGetAdaptorCurve( TopoDS_Edge edge, TopoDS_Face face, out double dStartU, out double dEndU )
		{
			dStartU = 0.0;
			dEndU = 0.0;

			if( edge == null || face == null || edge.IsNull() || face.IsNull() ) {
				return null;
			}
			try {
				BRepAdaptor_Curve adCurve = new BRepAdaptor_Curve( edge, face );

				// get the start and end point parameters
				dStartU = adCurve.FirstParameter();
				dEndU = adCurve.LastParameter();
				return adCurve;
			}

			// when there is no correspondence between edge and face
			catch( Exception ) {
				return null;
			}

		}

		// Tuple<double Deflection, double ChordLength, double ArcLength>
		static Tuple<double, double, double> CalculateDeflectionForCircularEdge( BRepAdaptor_Curve adaptorCurve, double dStartU, double dEndU, double R, double maxChordError = PRECISION_DEFLECTION, double maxSegmentLength = PRECISION_MAX_LENGTH )
		{

			if( adaptorCurve == null ) {
				throw new ArgumentNullException( nameof( adaptorCurve ) );
			}
			if( maxChordError <= 0.0 || R <= 0 || maxSegmentLength <= 0.0 ) {
				throw new ArgumentException( "param must greater than 0", nameof( maxChordError ) );
			}

			// calculate chord length
			gp_Pnt startPnt = adaptorCurve.Value( dStartU );
			gp_Pnt endPnt = adaptorCurve.Value( dEndU );
			double dChordTotal = startPnt.Distance( endPnt );

			// get total arc
			double ratio = dChordTotal / ( 2.0 * R );
			if( ratio > 1.0 ) {
				ratio = 1.0;
			}
			if( ratio < -1.0 ) {
				ratio = -1.0;
			}

			// angle = 2 * asin( chord / (2R) )
			double totalAngle = 2.0 * Math.Asin( ratio );

			// if arc is small enough, return direct deflection
			if( totalAngle <= 0.0 ) {
				double dDeflection = Math.Min( maxChordError, R * ( 1.0 - Math.Cos( totalAngle / 2.0 ) ) );
				Tuple<double, double, double> DeflectionInfo = new Tuple<double, double, double>( dDeflection, dChordTotal, totalAngle * R );
				return DeflectionInfo;
			}

			// calculation each single segment arc angle
			double dArcCos = 1.0 - maxChordError / R;
			if( dArcCos > 1.0 ) {
				dArcCos = 1.0;
			}
			if( dArcCos < -1.0 ) {
				dArcCos = -1.0;
			}
			double dEachArcAngle = 2.0 * Math.Acos( dArcCos );

			// calcute max segment angle by length
			double dArcOfEachPartMaxLength = maxSegmentLength / R;

			// get min angle to use
			double finalPartArcAngle = Math.Min( dEachArcAngle, dArcOfEachPartMaxLength );

			// protection for too small angle may cut too many segments
			double minAngle = 1e-12;
			if( finalPartArcAngle < minAngle ) {
				finalPartArcAngle = minAngle;
			}

			// back to deflection to use
			double deflectionToUse = R * ( 1.0 - Math.Cos( finalPartArcAngle / 2.0 ) );
			double segmentChordLength = 2.0 * R * Math.Sin( finalPartArcAngle / 2.0 );
			double segmentArcLength = R * finalPartArcAngle;
			Tuple<double, double, double> result = new Tuple<double, double, double>( deflectionToUse, segmentChordLength, segmentArcLength );
			return result;
		}

		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
	}
}
