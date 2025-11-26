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
	internal static class CADPretreatHelper
	{
		// public function area

		public struct CADSegBuildData
		{
			public List<CADPoint> PointList;
			public double SegmentLength;
			public double SubSegmentLength;
			public double PerChordLength;
		}

		public enum CADError
		{
			Done,
			GeomTypeError,
			AdaptorFaild,
			ParamError,
			PointCountError,
			DiscretizFaild,
			UnknownSegemntType
		}

		// fix: 這個命名建議就叫 DiscretizeArcToCADPoints 我把他的子功能才叫ToBuildData.因為可能很多段
		public static CADError DiscretizeArcToSegmentBuildData( TopoDS_Edge edge, TopoDS_Face shellFace, out List<CADSegBuildData> cadSegmentBuildData, double maxAngleRad = Math.PI / 2 )
		{
			cadSegmentBuildData = new List<CADSegBuildData>();

			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() || maxAngleRad <= 0 ) {
				return CADError.ParamError;
			}
			if( !GeometryTool.IsCircularArc( edge, out _, out double R, out _, out double angle ) ) {
				return CADError.GeomTypeError;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() || dStartU == dEndU ) {
				return CADError.AdaptorFaild;
			}

			// this arc angel is small enough just split by chord error or equal length
			if( angle <= maxAngleRad ) {
				// fix: 這邊正式 code 應該就避免用 tuple 了，可以改用自訂 struct 或 class===>改用struct
				// re: 這邊比我想像中複雜，我覺得 CalculateDeflectionForCircularEdge 可以直接根據 angle/R 算出 segment 數量，當然要直接還傳 u List 也可以 ====>包起來
				CADError result = DiscretizeArcToCADBuildData( adaptorCurve, edge, shellFace, dStartU, dEndU, PRECISION_DEFLECTION, PRECISION_MAX_LENGTH, out CADSegBuildData cadContstrucElement );
				if( result != CADError.Done ) {
					return result;
				}
				cadSegmentBuildData.Add( cadContstrucElement );
				return result;
			}

			// this arc angle is too big need split to small arc first
			List<double> segmentParamList = new List<double>();

			// split big arc to small arc
			int nSplitCount = (int)Math.Ceiling( angle / maxAngleRad );

			// protecton
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
				CADError result = DiscretizeArcToCADBuildData( adaptorCurve, edge, shellFace, segmentParamList[ i ], segmentParamList[ i + 1 ], PRECISION_DEFLECTION, PRECISION_MAX_LENGTH, out CADSegBuildData cadContstrucElement );
				if( result != CADError.Done ) {
					return result;
				}
				cadSegmentBuildData.Add( cadContstrucElement );
			}


			// reverse each segment
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				cadSegmentBuildData.Reverse();
			}
			return CADError.Done;
		}

		// fix: 這個命名建議就叫 DiscretizeBsplineToCADPoints
		public static CADError DiscretizeBsplineToBuildData( TopoDS_Edge edge, TopoDS_Face shellFace, out List<CADSegBuildData> cadSegmentBuildDataList )
		{
			cadSegmentBuildDataList = new List<CADSegBuildData>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return CADError.ParamError;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );

			// fix: adaptorCurve isNull
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return CADError.AdaptorFaild;
			}

			// split this bspline by chord error
			// this param has already considered edge orientation
			List<double> dSegmentParamList = ChordErrorSplit( adaptorCurve, edge, shellFace, dStartU, dEndU );
			if ( dSegmentParamList == null || dSegmentParamList.Count < 2 ) {
				return CADError.ParamError;
			}
			for( int i = 0; i < dSegmentParamList.Count - 1; i++ ) {

				// each part split by equal length
				double dthisEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dSegmentParamList[ i ], dSegmentParamList[ i + 1 ] );
				List<double> thisPartParamList = DiscretizeArcOrLineByLength( dSegmentParamList[ i ], dSegmentParamList[ i + 1 ], PRECISION_MAX_LENGTH, dthisEdgeLength, out double dSubSegLength );
				CADSegBuildData cadSegmentBuildData = new CADSegBuildData();
				List<CADPoint> cadPointList = GetCADPointsFromCurveParams( thisPartParamList, edge, shellFace, adaptorCurve );
				if( cadPointList.Count < 2 ) {
					return CADError.PointCountError;
				}
				cadSegmentBuildData.PointList = cadPointList;
				cadSegmentBuildData.SegmentLength = dthisEdgeLength;
				cadSegmentBuildData.SubSegmentLength = dSubSegLength;
				cadSegmentBuildData.PerChordLength = cadSegmentBuildData.PointList.First().Point.Distance( cadSegmentBuildData.PointList[ 1 ].Point );
				cadSegmentBuildDataList.Add( cadSegmentBuildData );
			}
			return CADError.Done;
		}

		// fix: 這個 API 建議改名叫 DiscretizeArcOrLineByLength
		// fix: 他使用上有限制，應該只適用圓弧或直線
		// fix: dEachArcLength 不只 for arc，不一定要改
		public static List<double> DiscretizeArcOrLineByLength( double dStartU, double dEndU, double dMaxSegmentLength, double dEdgeLength, out double dSubSegmentLength )
		{
			if( dMaxSegmentLength <= 0 || dEdgeLength <= 0 || dStartU == dEndU ) {
				dSubSegmentLength = 0.0;
				return new List<double>();
			}
			int nSubSegmentCount = (int)Math.Ceiling( dEdgeLength / dMaxSegmentLength );
			if( nSubSegmentCount <= 0 ) {
				dSubSegmentLength = 0.0;
				return new List<double>();
			}
			// fix: 如果要分出中點，段落的數量應該是偶數?
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

		// fix: 這個命名建議就叫 DiscretizeLineToCADPoints ===> 更改為大家都out build data
		public static CADError DiscretizeLineToBuildData( TopoDS_Edge lineEdge, TopoDS_Face shellFace, double dMaxSegmentLength, out CADSegBuildData cadSegBuildData )
		{
			cadSegBuildData = new CADSegBuildData();
			// fix: 這裡應該要先做傳入引數的檢查 (其他 function 也檢查一下)
			if( lineEdge == null || lineEdge.IsNull() || shellFace == null || shellFace.IsNull() || dMaxSegmentLength <= 0 ) {
				return CADError.ParamError;
			}

			// get curve parameters
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( lineEdge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return CADError.AdaptorFaild;
			}

			// curve length from parameter StartU toEndU 
			double dEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );
			List<double> segmentParamList = DiscretizeArcOrLineByLength( dStartU, dEndU, dMaxSegmentLength, dEdgeLength, out double dSubSegLength );
			if( segmentParamList.Count < 2 ) {
				return CADError.ParamError;
			}

			// need to consider orientation
			if( lineEdge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			List<CADPoint> oneSegmentPointList = GetCADPointsFromCurveParams( segmentParamList, lineEdge, shellFace, adaptorCurve );
			if( oneSegmentPointList.Count < 2 ) {
				return CADError.PointCountError;
			}
			cadSegBuildData.PointList = oneSegmentPointList;
			cadSegBuildData.SegmentLength = dEdgeLength;
			cadSegBuildData.SubSegmentLength = dSubSegLength;
			cadSegBuildData.PerChordLength = cadSegBuildData.SubSegmentLength;
			return CADError.Done;
		}

		// private function area
		// fix: IsTanVecAdjusted 參數目前沒有使用到，可以考慮移除
		static List<CADPoint> GetCADPointsFromCurveParams( List<double> segmentParamList, TopoDS_Edge edge, TopoDS_Face shellFace, BRepAdaptor_Curve adC )
		{
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() || adC == null || adC.IsNull() || segmentParamList == null || segmentParamList.Count == 0 ) {
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

		static CADError DiscretizeArcToCADBuildData( BRepAdaptor_Curve adaptorCurve, TopoDS_Edge edge, TopoDS_Face shellFace, double dStartU, double dEndU, double maxChordError, double maxSegmentLength, out CADSegBuildData cadBuildData )
		{
			cadBuildData = new CADSegBuildData();
			if( adaptorCurve == null ) {
				return CADError.AdaptorFaild;
			}
			if( maxChordError <= 0.0 || maxSegmentLength <= 0.0 || edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return CADError.ParamError;
			}

			// cal total arc length
			double segmentLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );

			// get chord error split parameters
			List<double> chordErrorParam = ChordErrorSplit( adaptorCurve, edge, shellFace, dStartU, dEndU );
			if( chordErrorParam == null || chordErrorParam.Count < 2 ) {
				return CADError.DiscretizFaild;
			}
			double chordErrorArcLength = GCPnts_AbscissaPoint.Length( adaptorCurve, chordErrorParam[ 0 ], chordErrorParam[ 1 ] );

			// choose chord error split or equal length split
			List<double> finalParams = SelectSplitStrategy( edge, chordErrorArcLength, maxSegmentLength,
				chordErrorParam, dStartU, dEndU, segmentLength, out double subSegmentLength );

			// create CADPoint list
			List<CADPoint> cadPointList = GetCADPointsFromCurveParams( finalParams, edge, shellFace, adaptorCurve );

			if( cadPointList.Count < 2 ) {
				return CADError.PointCountError;
			}

			// set back to construct element
			SetCADConstructElement( ref cadBuildData, cadPointList, segmentLength, subSegmentLength );
			return CADError.Done; ;

		}

		// fix: 這個人應該可以 private
		static List<double> ChordErrorSplit( BRepAdaptor_Curve adaptorCurve, TopoDS_Edge edge, TopoDS_Face shellFace, double dStartU, double dEndU, double dDeflection = PRECISION_DEFLECTION )
		{
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() || adaptorCurve == null || adaptorCurve.IsNull() || dDeflection <= 0 ) {
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

		static List<double> SelectSplitStrategy( TopoDS_Edge edge, double chordErrorArcLength, double maxSegmentLength,
		List<double> chordErrorParam, double dStartU, double dEndU, double totalArcLength, out double subSegLength )
		{
			if( edge == null || edge.IsNull() || chordErrorParam == null || chordErrorParam.Count == 0 ) {
				subSegLength = 0;
				return new List<double>();
			}

			// use chord error discretize
			if( chordErrorArcLength <= maxSegmentLength ) {
				subSegLength = chordErrorArcLength;

				// this param has already considered edge orientation
				return chordErrorParam;
			}
			else {
				List<double> finalParams = DiscretizeArcOrLineByLength( dStartU, dEndU, maxSegmentLength, totalArcLength, out subSegLength );
				if( finalParams == null || finalParams.Count < 2 ) {
					return new List<double>();
				}
				if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					finalParams.Reverse();
				}
				return finalParams;
			}
		}

		static void SetCADConstructElement( ref CADSegBuildData element, List<CADPoint> pointList,
		double totalArcLength, double subSegLength )
		{
			if( pointList == null || pointList.Count < 2 ) {
				element = new CADSegBuildData();
				return;
			}
			element.PointList = pointList;
			element.SegmentLength = totalArcLength;
			element.SubSegmentLength = subSegLength;
			element.PerChordLength = pointList.First().Point.Distance( pointList[ 1 ].Point );
		}

		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
	}
}
