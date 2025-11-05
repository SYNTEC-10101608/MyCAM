using System;
using System.Collections.Generic;
using MyCAM.Data;
using OCC.BOPTools;
using OCC.BRep;
using OCC.BRepAdaptor;
using OCC.BRepBuilderAPI;
using OCC.GCPnts;
using OCC.Geom;
using OCC.Geom2d;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

namespace MyCAM.Helper
{
	internal static class Pretreatment
	{
		public static List<TopoDS_Edge> SplitArcEdgeIfTooLarge( TopoDS_Edge edge, TopoDS_Face shellFace, double maxAngleRad = Math.PI / 2 )
		{
			List<TopoDS_Edge> edgeList = new List<TopoDS_Edge>();

			// 取得圓弧的圓心、半徑、方向與弧角
			if( !GeometryTool.IsCircularArc( edge, out gp_Pnt center, out double radius, out gp_Dir dir, out double angle ) )
				return edgeList; // 不是圓弧

			// 若弧角 <= maxAngleRad，直接回傳原 edge
			if( angle <= maxAngleRad ) {
				edgeList.Add( edge );
				return edgeList;
			}

			// 需要拆分
			int nSplit = (int)Math.Ceiling( angle / maxAngleRad );

			// get curve param
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null ) {
				return new List<TopoDS_Edge>();
			}

			double dDeltaU = ( dEndU - dStartU ) / nSplit;

			// collect parameter lists for each segment of the curve
			List<double> segmentParamList = new List<double>();

			for ( int i = 0 ; i <= nSplit; i++ ) {
				segmentParamList.Add( dStartU + i * dDeltaU );
			}

			// reverse the segment parameters if the edge is reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			List<TopoDS_Edge> subEdgeList = SplitEdgeByParamsOnFace( edge, shellFace, segmentParamList );
			return subEdgeList;
		}

		// public function area
		public static List<TopoDS_Edge> GetBsplineToEdgeList( TopoDS_Edge edge, TopoDS_Face shellFace )
		{
			if( edge == null || edge.IsNull() || shellFace == null || shellFace.IsNull() ) {
				return new List<TopoDS_Edge>();
			}

			// get curve param
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( edge, shellFace, out double dStartU, out double dEndU );
			if( adaptorCurve == null ) {
				return new List<TopoDS_Edge>();
			}

			// break the curve into segments with given deflection precision
			GCPnts_QuasiUniformDeflection qUD = new GCPnts_QuasiUniformDeflection( adaptorCurve, PRECISION_DEFLECTION, dStartU, dEndU );

			// collect parameter lists for each segment of the curve
			List<double> segmentParamList = new List<double>();
			for( int i = 1; i <= qUD.NbPoints(); i++ ) {
				segmentParamList.Add( qUD.Parameter( i ) );
			}

			// reverse the segment parameters if the edge is reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}
			List<TopoDS_Edge> subEdgeList = SplitEdgeByParamsOnFace( edge, shellFace, segmentParamList );
			return subEdgeList;
		}

		public static List<LineCADSegment> GetCADSegmentLineFromShortEdge( List<TopoDS_Edge> edgeList, TopoDS_Face shellFace )
		{
			List<LineCADSegment> CADlineSegmentsList = new List<LineCADSegment>();
			for( int i = 0; i < edgeList.Count; i++ ) {

				// edge startU and endU is already setting by orientation
				List<CADPoint> cadPointList = GetSegmentPointsByEqualLength( edgeList[ i ], shellFace, PRECISION_MAX_LENGTH, true, out double dEdgeLength, out double dPointSpace );
				LineCADSegment line = new LineCADSegment( cadPointList, dEdgeLength, dPointSpace );
				CADlineSegmentsList.Add( line );
			}
			return CADlineSegmentsList;
		}

		public static List<CADPoint> GetSegmentPointsByEqualLength( TopoDS_Edge lineEdge, TopoDS_Face shellFace, double dMaxSegmentLength, bool IsTanVecAdjusted, out double dEdgeLength, out double dEachPartLength )
		{
			// get curve parameters
			BRepAdaptor_Curve adaptorCurve = TryGetAdaptorCurve( lineEdge, shellFace, out double dStartU, out double dEndU );

			// curve length from parameter StartU toEndU 
			dEdgeLength = GCPnts_AbscissaPoint.Length( adaptorCurve, dStartU, dEndU );
			dEachPartLength = 0.0;

			if( adaptorCurve == null ) {
				return new List<CADPoint>();
			}
			List<double> segmentParamList = GetCurveEachSegmentParamByLength( adaptorCurve, dStartU, dEndU, dMaxSegmentLength, dEdgeLength, out dEachPartLength );
			if( segmentParamList.Count < 2 ) {
				return new List<CADPoint>();
			}

			// need to consider orientation
			if( IsTanVecAdjusted == false ) {
				if( lineEdge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					segmentParamList.Reverse();
				}
			}
			List<CADPoint> oneSegmentPointList = GetCADPointByEdgeAndEachPointSegmentParam( segmentParamList, lineEdge, shellFace, adaptorCurve, IsTanVecAdjusted );
			return oneSegmentPointList;
		}

		// private function area
		static List<double> GetCurveEachSegmentParamByLength( BRepAdaptor_Curve adaptorCurve, double dStartU, double dEndU, double dMaxSegmentLength, double dEdgeLength, out double dEachPartLength )
		{
			// this edge will be divided into N segments
			int nSubSegmentCount = (int)Math.Ceiling( dEdgeLength / dMaxSegmentLength );
			dEachPartLength = dEdgeLength / nSubSegmentCount;
			double dDeltaU = ( dEndU - dStartU ) / nSubSegmentCount;

			List<double> segmentParamList = new List<double>();
			for( int i = 0; i <= nSubSegmentCount; i++ ) {
				double param = dStartU + i * dDeltaU;
				segmentParamList.Add( param );
			}
			return segmentParamList;
		}

		static List<TopoDS_Edge> SplitEdgeByParamsOnFace( TopoDS_Edge edge, TopoDS_Face edgeOnThisFace, List<double> paramList )
		{
			List<TopoDS_Edge> subEdgeList = new List<TopoDS_Edge>();
			if( paramList == null || paramList.Count < 2 || edge == null || edge.IsNull() || edgeOnThisFace == null || edgeOnThisFace.IsNull() ) {
				return subEdgeList;
			}
			double dStartUParam = 0.0, dEndUParam = 0.0;
			Geom2d_Curve curve2d = BRep_Tool.CurveOnSurface( edge, edgeOnThisFace, ref dStartUParam, ref dEndUParam );
			if( curve2d == null ) {
				return subEdgeList;
			}

			// get the 2D geometric representation of the edge on the face
			Geom_Surface surface = BRep_Tool.Surface( edgeOnThisFace );

			// get each edge by U param
			for( int i = 0; i < paramList.Count - 1; i++ ) {
				double thisCurveStartU = paramList[ i ];
				double thisCurveEndU = paramList[ i + 1 ];

				// trimming 2D curves on a face
				Geom2d_TrimmedCurve trimmed2d = new Geom2d_TrimmedCurve( curve2d, thisCurveStartU, thisCurveEndU );

				// create new edges using 2D curves + faces
				BRepBuilderAPI_MakeEdge maker = new BRepBuilderAPI_MakeEdge( trimmed2d, surface );
				TopoDS_Edge subEdge = maker.Edge();
				Console.WriteLine( "New" + subEdge.Orientation() );
				Console.Write( "Ori" + edge.Orientation() );
				subEdge.Orientation( edge.Orientation() );
				subEdgeList.Add( subEdge );
			}
			return subEdgeList;
		}

		static List<CADPoint> GetCADPointByEdgeAndEachPointSegmentParam( List<double> segmentParamList, TopoDS_Edge edge, TopoDS_Face shellFace, BRepAdaptor_Curve adC, bool IsTanVecAdjusted )
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

				if( IsTanVecAdjusted == false ) {
					if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
						tangentVec.Reverse();
					}
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

		const double PRECISION_DEFLECTION = 0.01;
		const double PRECISION_MAX_LENGTH = 1;
	}
}