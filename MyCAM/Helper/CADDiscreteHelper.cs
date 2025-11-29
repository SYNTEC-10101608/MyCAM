using MyCAM.Data;
using OCC.BOPTools;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public enum DiscreteCADError
	{
		Done,
		GeomTypeError,
		AdaptorFaild,
		InvalidInputParam,
		InvalidPointCount,
	}

	public struct DiscretizedCADData
	{
		public List<CADPoint> DiscCADPointList;
		public double SegmentLength;
		public double SubSegmentLength;
		public double SubChordLength;
	}


	/// <summary>
	/// The CADDiscreteHelper class discretizes 5D path wire data into CADPoint and CADSegment
	/// </summary>
	public static class CADDiscreteHelper
	{
		// public function area
		public static DiscreteCADError DiscretizeLine( TopoDS_Edge lineEdge, TopoDS_Face shellFace, out DiscretizedCADData cadSegBuildData,
			double dMaxSegmentLength = DISCRETE_MAX_LENGTH )
		{
			cadSegBuildData = new DiscretizedCADData();
			if( lineEdge == null || lineEdge.IsNull() || shellFace == null || shellFace.IsNull() || dMaxSegmentLength < DOUBLE_TOLERANCE ) {
				return DiscreteCADError.InvalidInputParam;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( lineEdge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return DiscreteCADError.AdaptorFaild;
			}

			// curve length from parameter StartU toEndU 
			double dEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );
			List<double> segmentParamList = DiscretizeArcOrLineByLength( dStartU, dEndU, dMaxSegmentLength, dEdgeLength, out double dSubSegLength );
			if( segmentParamList.Count < 2 ) {
				return DiscreteCADError.InvalidPointCount;
			}

			// need to consider orientation
			if( lineEdge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			List<CADPoint> oneSegmentPointList = GetCADPointsFromCurveParams( segmentParamList, lineEdge, shellFace, adaptorCurve );
			if( oneSegmentPointList.Count < 2 ) {
				return DiscreteCADError.InvalidPointCount;
			}
			cadSegBuildData.DiscCADPointList = oneSegmentPointList;
			cadSegBuildData.SegmentLength = dEdgeLength;
			cadSegBuildData.SubSegmentLength = dSubSegLength;

			// discretize line will get the same length of SubSegmentLength and SubChordLength
			cadSegBuildData.SubChordLength = cadSegBuildData.SubSegmentLength;
			return DiscreteCADError.Done;
		}

		public static DiscreteCADError DiscretizeArc( TopoDS_Edge edge, TopoDS_Face shellFace, out List<DiscretizedCADData> cadSegmentBuildData,
			double maxAngleRad = DISCRETE_MAX_ANGLE, double maxDeflection = DISCRETE_MAX_DEFLECTION, double maxSegmentLength = DISCRETE_MAX_LENGTH )
		{
			cadSegmentBuildData = new List<DiscretizedCADData>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() || maxAngleRad < DOUBLE_TOLERANCE ) {
				return DiscreteCADError.InvalidInputParam;
			}
			if( !GeometryTool.IsCircularArc( edge, out _, out double R, out _, out double angle ) ) {
				return DiscreteCADError.GeomTypeError;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return DiscreteCADError.AdaptorFaild;
			}

			// split arc by angle
			List<double> segmentParamList = new List<double>();
			int nSplitCount = (int)Math.Ceiling( angle / maxAngleRad );

			// protection
			if( nSplitCount < 1 ) {
				nSplitCount = 1;
			}
			double dDeltaU = ( dEndU - dStartU ) / nSplitCount;

			// collect parameter lists for each segment of the curve
			for( int i = 0; i < nSplitCount; i++ ) {
				segmentParamList.Add( dStartU + i * dDeltaU );
			}
			segmentParamList.Add( dEndU );
			for( int i = 0; i < segmentParamList.Count - 1; i++ ) {

				// each cad point is ordered by orientation
				DiscreteCADError result = DiscretizeArc( adaptorCurve, edge, shellFace, segmentParamList[ i ], segmentParamList[ i + 1 ], maxDeflection, maxSegmentLength, out DiscretizedCADData cadContstrucElement );
				if( result != DiscreteCADError.Done ) {
					return result;
				}
				cadSegmentBuildData.Add( cadContstrucElement );
			}

			// reverse the outer list if edge is reversed, the inner list has already considered orientation
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				cadSegmentBuildData.Reverse();
			}
			return DiscreteCADError.Done;
		}

		public static DiscreteCADError DiscretizeBspline( TopoDS_Edge edge, TopoDS_Face shellFace, out List<DiscretizedCADData> cadSegmentBuildDataList,
			double maxDeflection = DISCRETE_MAX_DEFLECTION, double maxSegmentLength = DISCRETE_MAX_LENGTH )
		{
			cadSegmentBuildDataList = new List<DiscretizedCADData>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return DiscreteCADError.InvalidInputParam;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return DiscreteCADError.AdaptorFaild;
			}

			// split this bspline by chord error
			List<double> dSegmentParamList = ChordErrorSplit( adaptorCurve, dStartU, dEndU, maxDeflection );
			if( dSegmentParamList == null || dSegmentParamList.Count < 2 ) {
				return DiscreteCADError.InvalidPointCount;
			}

			// need to consider orientation
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				dSegmentParamList.Reverse();
			}
			for( int i = 0; i < dSegmentParamList.Count - 1; i++ ) {

				// each part split by equal length
				double edgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dSegmentParamList[ i ], dSegmentParamList[ i + 1 ] );

				// no need to consider orientation here because the param list has already considered
				List<double> paramList = DiscretizeArcOrLineByLength( dSegmentParamList[ i ], dSegmentParamList[ i + 1 ], maxSegmentLength, edgeLength, out double dSubSegLength );
				if( paramList.Count < 2 ) {
					return DiscreteCADError.InvalidPointCount;
				}
				List<CADPoint> cadPointList = GetCADPointsFromCurveParams( paramList, edge, shellFace, adaptorCurve );
				if( cadPointList.Count < 2 ) {
					return DiscreteCADError.InvalidPointCount;
				}
				DiscretizedCADData cadSegmentBuildData = new DiscretizedCADData();
				cadSegmentBuildData.DiscCADPointList = cadPointList;
				cadSegmentBuildData.SegmentLength = edgeLength;
				cadSegmentBuildData.SubSegmentLength = dSubSegLength;

				// we suppose the chord length is close to the sub segment length here
				cadSegmentBuildData.SubChordLength = cadSegmentBuildData.SubSegmentLength;
				cadSegmentBuildDataList.Add( cadSegmentBuildData );
			}
			return DiscreteCADError.Done;
		}

		// private function area
		static DiscreteCADError DiscretizeArc( BRepAdaptor_Curve adaptorCurve, TopoDS_Edge edge, TopoDS_Face shellFace, double dStartU, double dEndU, double maxDeflection, double maxSegmentLength, out DiscretizedCADData cadBuildData )
		{
			cadBuildData = new DiscretizedCADData();
			if( maxDeflection < DOUBLE_TOLERANCE || maxSegmentLength < DOUBLE_TOLERANCE || edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return DiscreteCADError.InvalidInputParam;
			}
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return DiscreteCADError.AdaptorFaild;
			}
			double segmentLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );

			// choose chord error split or equal length split
			List<double> finalParams = SplitArcByOption( adaptorCurve, dStartU, dEndU, maxDeflection, maxSegmentLength, segmentLength, out double subSegmentLength );
			if( finalParams == null || finalParams.Count < 2 ) {
				return DiscreteCADError.InvalidPointCount;
			}

			// need to consider orientation
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				finalParams.Reverse();
			}

			// create CADPoint list
			List<CADPoint> cadPointList = GetCADPointsFromCurveParams( finalParams, edge, shellFace, adaptorCurve );
			if( cadPointList.Count < 2 ) {
				return DiscreteCADError.InvalidPointCount;
			}

			// set back to construct element
			cadBuildData.DiscCADPointList = cadPointList;
			cadBuildData.SegmentLength = segmentLength;
			cadBuildData.SubSegmentLength = subSegmentLength;
			cadBuildData.SubChordLength = cadPointList.First().Point.Distance( cadPointList[ 1 ].Point );
			return DiscreteCADError.Done;
		}

		static List<double> SplitArcByOption( BRepAdaptor_Curve adaptorCurve, double dStartU, double dEndU, double maxDeflection, double maxSegmentLength, double segmentLength, out double subSegLength )
		{
			subSegLength = 0.0;
			if( maxDeflection < DOUBLE_TOLERANCE || maxSegmentLength < DOUBLE_TOLERANCE || segmentLength < DOUBLE_TOLERANCE || Math.Abs( dStartU - dEndU ) < DOUBLE_TOLERANCE ) {
				return new List<double>();
			}
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return new List<double>();
			}
			List<double> chordErrorParam = ChordErrorSplit( adaptorCurve, dStartU, dEndU, maxDeflection );
			List<double> equlLenghtParam = DiscretizeArcOrLineByLength( dStartU, dEndU, maxSegmentLength, segmentLength, out double lineSubSegLength );

			// use equal length discretize
			if( equlLenghtParam.Count >= chordErrorParam.Count && equlLenghtParam.Count >= 2 ) {
				subSegLength = lineSubSegLength;
				return equlLenghtParam;
			}

			// use chord error discretize
			else if( chordErrorParam.Count >= 2 ) {
				subSegLength = segmentLength / ( chordErrorParam.Count - 1 );
				return chordErrorParam;
			}

			// no valid discretize param
			else {
				return new List<double>();
			}
		}

		public static List<double> DiscretizeArcOrLineByLength( double dStartU, double dEndU, double dMaxSegmentLength, double dEdgeLength, out double dSubSegmentLength )
		{
			if( dMaxSegmentLength < DOUBLE_TOLERANCE || dEdgeLength < DOUBLE_TOLERANCE || Math.Abs( dStartU - dEndU ) < DOUBLE_TOLERANCE ) {
				dSubSegmentLength = 0.0;
				return new List<double>();
			}
			int nSubSegmentCount = (int)Math.Ceiling( dEdgeLength / dMaxSegmentLength );

			// no need to split
			if( nSubSegmentCount <= 1 ) {
				dSubSegmentLength = dEdgeLength;
				return new List<double>() { dStartU, dEndU };
			}

			// make sure to get odd count of points to get middle of edge for arc
			if( nSubSegmentCount % 2 != 0 ) {
				nSubSegmentCount += 1;
			}
			dSubSegmentLength = dEdgeLength / nSubSegmentCount;
			double dDeltaU = ( dEndU - dStartU ) / nSubSegmentCount;
			List<double> segmentParamList = new List<double>();
			for( int i = 0; i < nSubSegmentCount; i++ ) {
				double param = dStartU + i * dDeltaU;
				segmentParamList.Add( param );
			}
			segmentParamList.Add( dEndU );
			return segmentParamList;
		}

		static List<double> ChordErrorSplit( BRepAdaptor_Curve adaptorCurve, double dStartU, double dEndU, double dDeflection )
		{
			if( dDeflection < DOUBLE_TOLERANCE ) {
				return new List<double>();
			}
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
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
			return segmentParamList;
		}

		static List<CADPoint> GetCADPointsFromCurveParams( List<double> segmentParamList, TopoDS_Edge edge, TopoDS_Face shellFace, BRepAdaptor_Curve adC )
		{
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() || adC == null || adC.IsNull()
				|| segmentParamList == null || segmentParamList.Count == 0 ) {
				return new List<CADPoint>();
			}
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

		const double DOUBLE_TOLERANCE = 1e-6;
		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DISCRETE_MAX_LENGTH = 1;
		const double DISCRETE_MAX_ANGLE = Math.PI / 2;
	}
}
