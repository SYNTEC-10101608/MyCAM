using MyCAM.Data;
using OCC.BOPTools;
using OCC.BRepAdaptor;
using OCC.GCPnts;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Helper
{
	public static class DisctereContourHelper
	{
		public static bool BuildContourGeomData( List<PathEdge5D> pathEdge5DList, bool isClosed,
			out List<CADPoint> cadPointList, out Dictionary<CADPoint, CADPoint> connectPointMap,
			double maxDeflection = DISCRETE_MAX_DEFLECTION, double maxEdgeLength = DISCRETE_MAX_EDGE_LENGTH )
		{
			cadPointList = new List<CADPoint>();
			connectPointMap = new Dictionary<CADPoint, CADPoint>();
			if( pathEdge5DList == null || pathEdge5DList.Count == 0 ) {
				return false;
			}

			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face
				if( !GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace, i == 0, i == pathEdge5DList.Count - 1, isClosed,
					ref cadPointList, ref connectPointMap,
					maxDeflection, maxEdgeLength ) ) {
					return false;
				}
			}
			return true;
		}

		static bool GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace, bool bFirst, bool bLast, bool isClosed,
			ref List<CADPoint> cadPointList, ref Dictionary<CADPoint, CADPoint> connectPointMap,
			double maxDeflection, double maxEdgeLength )
		{
			List<CADPoint> oneSegmentPointList = new List<CADPoint>();

			// get curve parameters
			BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge, shellFace );
			double dStartU = adC.FirstParameter();
			double dEndU = adC.LastParameter();

			// break the curve into segments with given deflection precision
			GCPnts_QuasiUniformDeflection qUD = new GCPnts_QuasiUniformDeflection( adC, maxDeflection, dStartU, dEndU );

			// break the curve into segments with given max length
			List<double> segmentParamList = new List<double>();
			for( int i = 1; i < qUD.NbPoints(); i++ ) {
				segmentParamList.Add( qUD.Parameter( i ) );
				double dLength = GCPnts_AbscissaPoint.Length( adC, qUD.Parameter( i ), qUD.Parameter( i + 1 ) );

				// add sub-segments if the length is greater than max length
				if( dLength > maxEdgeLength ) {
					int nSubSegmentCount = (int)Math.Ceiling( dLength / maxEdgeLength );
					double dSubSegmentLength = ( qUD.Parameter( i + 1 ) - qUD.Parameter( i ) ) / nSubSegmentCount;

					// using equal parameter to break the segment
					for( int j = 1; j < nSubSegmentCount; j++ ) {
						segmentParamList.Add( qUD.Parameter( i ) + j * dSubSegmentLength );
					}
				}
			}
			segmentParamList.Add( qUD.Parameter( qUD.NbPoints() ) );

			// reverse the segment parameters if the edge is reversed
			if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
				segmentParamList.Reverse();
			}

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

				// build
				CADPoint cadPoint = new CADPoint( point, normalVec_1st, normalVec_2nd, new gp_Dir( tangentVec ) );

				// map the last point of the previous edge to the start point, and use current start point to present
				if( !bFirst && i == 0 && cadPointList.Count > 0 ) {
					CADPoint lastPoint = cadPointList.Last();
					cadPointList.RemoveAt( cadPointList.Count - 1 );
					connectPointMap[ cadPoint ] = lastPoint;
					oneSegmentPointList.Add( cadPoint );
				}
				else if( bLast && i == segmentParamList.Count - 1 && cadPointList.Count > 0 ) {

					// map the last point to the start point
					if( isClosed ) {
						CADPoint firstPoint = cadPointList[ 0 ];
						connectPointMap[ firstPoint ] = cadPoint;
					}

					// add the last point
					else {
						oneSegmentPointList.Add( cadPoint );
					}
				}
				else {
					oneSegmentPointList.Add( cadPoint );
				}
			}
			cadPointList.AddRange( oneSegmentPointList );
			return true;
		}

		public static void GetRefCoordFromContour( List<CADPoint> points, out gp_Ax1 refCoord )
		{
			refCoord = new gp_Ax1();

			if( points == null || points.Count == 0 ) {
				return;
			}

			int nPointNumber = points.Count;
			double xValue = 0.0;
			double yValue = 0.0;
			double zValue = 0.0;
			double xDirValue = 0.0;
			double yDirValue = 0.0;
			double zDirValue = 0.0;

			foreach( var point in points ) {
				xValue += point.Point.X();
				yValue += point.Point.Y();
				zValue += point.Point.Z();
				xDirValue += point.NormalVec_1st.X();
				yDirValue += point.NormalVec_1st.Y();
				zDirValue += point.NormalVec_1st.Z();
			}
			gp_Pnt p = new gp_Pnt( xValue / nPointNumber, yValue / nPointNumber, zValue / nPointNumber );
			gp_Dir d = new gp_Dir( xDirValue / nPointNumber, yDirValue / nPointNumber, zDirValue / nPointNumber );
			refCoord = new gp_Ax1( p, d );
		}

		const double DISCRETE_MAX_DEFLECTION = 0.01;
		const double DISCRETE_MAX_EDGE_LENGTH = 1.0;
	}
}
