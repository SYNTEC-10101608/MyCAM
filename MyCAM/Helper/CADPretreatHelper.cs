using MyCAM.Data;
using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public enum BuildCADError
	{
		Done,
		GeomTypeError,
		AdaptorFaild,
		InvalidInputParam,
		InvalidPointCount,
		DiscretizFaild,
		UnknownSegemntType
	}

	public struct DiscretizedCADData
	{
		public List<CADPoint> DiscCADPointList;
		public double SegmentLength;
		public double SubSegmentLength;
		public double SubChordLength;
	}

	public static class CADPretreatHelper
	{
		// public function area
		public static BuildCADError BuildCADSegment( List<PathEdge5D> pathEdge5DList, out List<ICADSegment> cadSegmentList )
		{
			cadSegmentList = new List<ICADSegment>();
			if( pathEdge5DList == null || pathEdge5DList.Count == 0 ) {
				return BuildCADError.InvalidInputParam;
			}

			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face

				// this curve is line use equal length split
				if( GeometryTool.IsLine( edge, out _, out _ ) ) {
					BuildCADError result = DiscretizeLine( edge, shellFace, DISCRETE_MAX_LENGTH, out DiscretizedCADData cadSegBuildData );
					if( result != BuildCADError.Done ) {
						return result;
					}
					BuildCADError buildResult = CADSegmentBuilder.BuildCADSegment( cadSegBuildData, ESegmentType.Line, out ICADSegment cadSegment );
					if( buildResult != BuildCADError.Done ) {
						return buildResult;
					}
					cadSegmentList.Add( cadSegment );
				}

				// this curve is arc choose the best option from the two options (chord error vs equal length)
				// fix: 這邊 center 跟 arcAngle 沒有用到?==>done
				else if( GeometryTool.IsCircularArc( edge, out _, out _, out _, out _ ) ) {
					BuildCADError result = DiscretizeArc( edge, shellFace, out List<DiscretizedCADData> cadSegBuildDataList, Math.PI / 2 );
					if( result != BuildCADError.Done || cadSegBuildDataList == null || cadSegBuildDataList.Count == 0 ) {
						return result;
					}

					for( int j = 0; j < cadSegBuildDataList.Count; j++ ) {
						BuildCADError buildResult = CADSegmentBuilder.BuildCADSegment( cadSegBuildDataList[ j ], ESegmentType.Arc, out ICADSegment cadSegment );
						if( buildResult != BuildCADError.Done ) {
							return buildResult;
						}
						cadSegmentList.Add( cadSegment );
					}
				}

				// use chord error split
				else {

					// separate this bspline
					BuildCADError result = DiscretizeBspline( edge, shellFace, out List<DiscretizedCADData> cadSegmentBuildDataList );
					if( result != BuildCADError.Done ) {
						return result;
					}
					if( cadSegmentBuildDataList == null || cadSegmentBuildDataList.Count == 0 ) {
						return BuildCADError.DiscretizFaild;
					}
					for( int j = 0; j < cadSegmentBuildDataList.Count; j++ ) {
						// fix: 建議架構統一，ChordLength 在 PretreatmentHelper 算好傳回來===>上面DiscretizeBsplineToBuildData已經算回來了
						BuildCADError buildResult = CADSegmentBuilder.BuildCADSegment( cadSegmentBuildDataList[ j ], ESegmentType.Line, out ICADSegment cadSegment );
						if( buildResult != BuildCADError.Done ) {
							return buildResult;
						}
						cadSegmentList.Add( cadSegment );
					}
				}
			}
			return BuildCADError.Done;
		}

		public static BuildCADError DiscretizeLine( TopoDS_Edge lineEdge, TopoDS_Face shellFace, double dMaxSegmentLength, out DiscretizedCADData cadSegBuildData )
		{
			cadSegBuildData = new DiscretizedCADData();
			if( lineEdge == null || lineEdge.IsNull() || shellFace == null || shellFace.IsNull() || dMaxSegmentLength < DOUBLE_TOLERANCE ) {
				return BuildCADError.InvalidInputParam;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( lineEdge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return BuildCADError.AdaptorFaild;
			}

			// curve length from parameter StartU toEndU 
			double dEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );
			List<double> segmentParamList = DiscretizeArcOrLineByLength( dStartU, dEndU, dMaxSegmentLength, dEdgeLength, out double dSubSegLength );
			if( segmentParamList.Count < 2 ) {
				return BuildCADError.InvalidPointCount;
			}

			// need to consider orientation
			if( lineEdge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			List<CADPoint> oneSegmentPointList = GetCADPointsFromCurveParams( segmentParamList, lineEdge, shellFace, adaptorCurve );
			if( oneSegmentPointList.Count < 2 ) {
				return BuildCADError.InvalidPointCount;
			}
			cadSegBuildData.DiscCADPointList = oneSegmentPointList;
			cadSegBuildData.SegmentLength = dEdgeLength;
			cadSegBuildData.SubSegmentLength = dSubSegLength;

			// discretize line will get the same length of SubSegmentLength and PerChordLength
			cadSegBuildData.SubChordLength = cadSegBuildData.SubSegmentLength;
			return BuildCADError.Done;
		}

		public static BuildCADError DiscretizeArc( TopoDS_Edge edge, TopoDS_Face shellFace, out List<DiscretizedCADData> cadSegmentBuildData, double maxAngleRad = Math.PI / 2 )
		{
			cadSegmentBuildData = new List<DiscretizedCADData>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() || maxAngleRad < DOUBLE_TOLERANCE ) {
				return BuildCADError.InvalidInputParam;
			}
			if( !GeometryTool.IsCircularArc( edge, out _, out double R, out _, out double angle ) ) {
				return BuildCADError.GeomTypeError;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return BuildCADError.AdaptorFaild;
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
				BuildCADError result = DiscretizeArc( adaptorCurve, edge, shellFace, segmentParamList[ i ], segmentParamList[ i + 1 ], DISCRETE_MAX_DEFLECTION, DISCRETE_MAX_LENGTH, out DiscretizedCADData cadContstrucElement );
				if( result != BuildCADError.Done ) {
					return result;
				}
				cadSegmentBuildData.Add( cadContstrucElement );
			}

			// reverse the outer list if edge is reversed, the inner list has already considered orientation
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				cadSegmentBuildData.Reverse();
			}
			return BuildCADError.Done;
		}

		public static BuildCADError DiscretizeBspline( TopoDS_Edge edge, TopoDS_Face shellFace, out List<DiscretizedCADData> cadSegmentBuildDataList )
		{
			cadSegmentBuildDataList = new List<DiscretizedCADData>();
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return BuildCADError.InvalidInputParam;
			}
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return BuildCADError.AdaptorFaild;
			}

			// split this bspline by chord error
			List<double> dSegmentParamList = ChordErrorSplit( adaptorCurve, dStartU, dEndU );
			if( dSegmentParamList == null || dSegmentParamList.Count < 2 ) {
				return BuildCADError.InvalidInputParam;
			}
			for( int i = 0; i < dSegmentParamList.Count - 1; i++ ) {

				// each part split by equal length
				double dthisEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dSegmentParamList[ i ], dSegmentParamList[ i + 1 ] );
				List<double> thisPartParamList = DiscretizeArcOrLineByLength( dSegmentParamList[ i ], dSegmentParamList[ i + 1 ], DISCRETE_MAX_LENGTH, dthisEdgeLength, out double dSubSegLength );
				DiscretizedCADData cadSegmentBuildData = new DiscretizedCADData();
				List<CADPoint> cadPointList = GetCADPointsFromCurveParams( thisPartParamList, edge, shellFace, adaptorCurve );
				if( cadPointList.Count < 2 ) {
					return BuildCADError.InvalidPointCount;
				}
				cadSegmentBuildData.DiscCADPointList = cadPointList;
				cadSegmentBuildData.SegmentLength = dthisEdgeLength;
				cadSegmentBuildData.SubSegmentLength = dSubSegLength;
				cadSegmentBuildData.SubChordLength = cadSegmentBuildData.DiscCADPointList.First().Point.Distance( cadSegmentBuildData.DiscCADPointList[ 1 ].Point );
				cadSegmentBuildDataList.Add( cadSegmentBuildData );
			}
			return BuildCADError.Done;
		}

		// TODO: 不想讓他 public
		public static List<double> DiscretizeArcOrLineByLength( double dStartU, double dEndU, double dMaxSegmentLength, double dEdgeLength, out double dSubSegmentLength )
		{
			if( dMaxSegmentLength < DOUBLE_TOLERANCE || dEdgeLength < DOUBLE_TOLERANCE || Math.Abs( dStartU - dEndU ) < DOUBLE_TOLERANCE ) {
				dSubSegmentLength = 0.0;
				return new List<double>();
			}
			int nSubSegmentCount = (int)Math.Ceiling( dEdgeLength / dMaxSegmentLength );
			if( nSubSegmentCount <= 0 ) {
				dSubSegmentLength = 0.0;
				return new List<double>();
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

		public static List<double> ChordErrorSplit( BRepAdaptor_Curve adaptorCurve, double dStartU, double dEndU, double dDeflection = DISCRETE_MAX_DEFLECTION )
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

		static BuildCADError DiscretizeArc( BRepAdaptor_Curve adaptorCurve, TopoDS_Edge edge, TopoDS_Face shellFace, double dStartU, double dEndU, double maxChordError, double maxSegmentLength, out DiscretizedCADData cadBuildData )
		{
			cadBuildData = new DiscretizedCADData();
			if( maxChordError < DOUBLE_TOLERANCE || maxSegmentLength < DOUBLE_TOLERANCE || edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return BuildCADError.InvalidInputParam;
			}
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return BuildCADError.AdaptorFaild;
			}
			double segmentLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );

			// choose chord error split or equal length split
			List<double> finalParams = SplitArcByOption( adaptorCurve, dStartU, dEndU, maxChordError, maxSegmentLength, segmentLength, out double subSegmentLength );
			if( finalParams == null || finalParams.Count < 2 ) {
				return BuildCADError.InvalidPointCount;
			}

			// need to consider orientation
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				finalParams.Reverse();
			}

			// create CADPoint list
			List<CADPoint> cadPointList = GetCADPointsFromCurveParams( finalParams, edge, shellFace, adaptorCurve );
			if( cadPointList.Count < 2 ) {
				return BuildCADError.InvalidPointCount;
			}

			// set back to construct element
			cadBuildData.DiscCADPointList = cadPointList;
			cadBuildData.SegmentLength = segmentLength;
			cadBuildData.SubSegmentLength = subSegmentLength;
			cadBuildData.SubChordLength = cadPointList.First().Point.Distance( cadPointList[ 1 ].Point );
			return BuildCADError.Done;
		}

		static List<double> SplitArcByOption( BRepAdaptor_Curve adaptorCurve, double dStartU, double dEndU, double maxChordError, double maxSegmentLength, double segmentLength, out double subSegLength )
		{
			subSegLength = 0.0;
			if( maxChordError < DOUBLE_TOLERANCE || maxSegmentLength < DOUBLE_TOLERANCE || segmentLength < DOUBLE_TOLERANCE || Math.Abs( dStartU - dEndU ) < DOUBLE_TOLERANCE ) {
				return new List<double>();
			}
			if( adaptorCurve == null || adaptorCurve.IsNull() ) {
				return new List<double>();
			}
			List<double> chordErrorParam = ChordErrorSplit( adaptorCurve, dStartU, dEndU );
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

		public static bool DetermineIfClosed( TopoDS_Shape shapeData )
		{
			if( shapeData == null || shapeData.IsNull() ) {
				return false;
			}
			try {
				TopoDS_Vertex startVertex = new TopoDS_Vertex();
				TopoDS_Vertex endVertex = new TopoDS_Vertex();
				TopExp.Vertices( TopoDS.ToWire( shapeData ), ref startVertex, ref endVertex );
				gp_Pnt startPoint = BRep_Tool.Pnt( TopoDS.ToVertex( startVertex ) );
				gp_Pnt endPoint = BRep_Tool.Pnt( TopoDS.ToVertex( endVertex ) );
				return startPoint.IsEqual( endPoint, GEOM_TOLERANCE );
			}
			catch {
				return false;
			}
		}

		const double DOUBLE_TOLERANCE = 1e-6;
		const double GEOM_TOLERANCE = 1e-3;
		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DISCRETE_MAX_LENGTH = 1;
	}
}
