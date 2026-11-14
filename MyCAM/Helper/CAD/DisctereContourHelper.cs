using System;
using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.BRepAdaptor;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCTool;

namespace MyCAM.Helper
{
	public static class DisctereContourHelper
	{
		public static bool BuildContourGeomData( TopoDS_Edge edge, bool isClosed,
			CAMPoint start, CAMPoint end,
			out List<CADPoint> cadPointList, out Dictionary<CADPoint, CADPoint> connectPointMap,
			double maxDeflection = GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION, double maxEdgeLength = GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
			EDiscretizeMethod discretizeMethod = GeometryTool.DEFAULT_DISCRETIZE_METHOD )
		{
			cadPointList = new List<CADPoint>();
			connectPointMap = new Dictionary<CADPoint, CADPoint>();

			// go through the contour edges
			if( !GetEdgeSegmentPoints( edge, isClosed,
				start, end,
				ref cadPointList, ref connectPointMap,
				maxDeflection, maxEdgeLength, discretizeMethod ) ) {
				return false;
			}

			return true;
		}

		static bool GetEdgeSegmentPoints(
			TopoDS_Edge edge, bool isClosed,
			CAMPoint start, CAMPoint end,
			ref List<CADPoint> cadPointList, ref Dictionary<CADPoint, CADPoint> connectPointMap,
			double maxDeflection, double maxEdgeLength, EDiscretizeMethod discretizeMethod )
		{
			// get curve parameters
			BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge );

			// discretization logic (deflection + max length + orientation)
			List<double> segmentParamList = GeometryTool.DiscretizeEdgeParameters( adC, edge, maxDeflection, maxEdgeLength, discretizeMethod );
			double length = segmentParamList.Last() - segmentParamList.First();

			for( int i = 0; i < segmentParamList.Count; i++ ) {
				double U = segmentParamList[ i ];
				double ratio = ( U - segmentParamList.First() ) / length;

				gp_Dir normalVec_1st = new gp_Dir( new gp_Vec( start.NormalVec_1st ).Multiplied( 1 - ratio )
					.Added( new gp_Vec( end.NormalVec_1st ).Multiplied( ratio ) ).XYZ() );

				gp_Dir normalVec_2nd = new gp_Dir( new gp_Vec( start.NormalVec_2nd ).Multiplied( 1 - ratio )
					.Added( new gp_Vec( end.NormalVec_2nd ).Multiplied( ratio ) ).XYZ() );

				// get point
				gp_Pnt point = adC.Value( U );

				// get tangent
				gp_Vec tangentVec = new gp_Vec();
				gp_Pnt _p = new gp_Pnt();
				adC.D1( U, ref _p, ref tangentVec );
				if( edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED ) {
					tangentVec.Reverse();
				}

				// build
				CADPoint cadPoint = new CADPoint( point, normalVec_1st, normalVec_2nd, new gp_Dir( tangentVec ) );


				// map the last point to the start point
				if( i == segmentParamList.Count - 1 && isClosed ) {
					CADPoint firstPoint = cadPointList[ 0 ];
					connectPointMap[ firstPoint ] = cadPoint;
				}
				else {
					cadPointList.Add( cadPoint );
				}
			}

			return true;
		}


		public static bool BuildContourGeomData( List<PathEdge5D> pathEdge5DList, bool isClosed,
			out List<CADPoint> cadPointList, out Dictionary<CADPoint, CADPoint> connectPointMap,
			double maxDeflection = GeometryTool.DEFAULT_DISCRETE_MAX_DEFLECTION, double maxEdgeLength = GeometryTool.DEFAULT_DISCRETE_MAX_LENGTH,
			EDiscretizeMethod discretizeMethod = GeometryTool.DEFAULT_DISCRETIZE_METHOD,
			Action<LoadingBarInfo> reportProgress = null )
		{
			cadPointList = new List<CADPoint>();
			connectPointMap = new Dictionary<CADPoint, CADPoint>();
			if( pathEdge5DList == null || pathEdge5DList.Count == 0 ) {
				return false;
			}

			// go through the contour edges
			for( int i = 0; i < pathEdge5DList.Count; i++ ) {
				// Report progress to the loading bar.
				reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.DiscretizeCurves, i, pathEdge5DList.Count ) );
				TopoDS_Edge edge = pathEdge5DList[ i ].PathEdge;
				TopoDS_Face shellFace = pathEdge5DList[ i ].ComponentFace;
				TopoDS_Face solidFace = pathEdge5DList[ i ].ComponentFace; // TODO: set solid face
				if( !GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), shellFace, solidFace, i == 0, i == pathEdge5DList.Count - 1, isClosed,
					ref cadPointList, ref connectPointMap,
					maxDeflection, maxEdgeLength, discretizeMethod, reportProgress ) ) {
					return false;
				}
			}

			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.DiscretizeCurves, pathEdge5DList.Count, pathEdge5DList.Count ) );
			return true;
		}

		static bool GetEdgeSegmentPoints( TopoDS_Edge edge, TopoDS_Face shellFace, TopoDS_Face solidFace, bool bFirst, bool bLast, bool isClosed,
			ref List<CADPoint> cadPointList, ref Dictionary<CADPoint, CADPoint> connectPointMap,
			double maxDeflection, double maxEdgeLength, EDiscretizeMethod discretizeMethod,
			Action<LoadingBarInfo> reportProgress )
		{
			List<CADPoint> oneSegmentPointList = new List<CADPoint>();

			// get curve parameters
			BRepAdaptor_Curve adC = new BRepAdaptor_Curve( edge, shellFace );

			// discretization logic (deflection + max length + orientation)
			List<double> segmentParamList = GeometryTool.DiscretizeEdgeParameters( adC, edge, maxDeflection, maxEdgeLength, discretizeMethod );

			// Report progress to the loading bar.
			reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CalculatePathPoints, 0, segmentParamList.Count ) );

			for( int i = 0; i < segmentParamList.Count; i++ ) {
				double U = segmentParamList[ i ];

				// get point
				gp_Pnt point = adC.Value( U );

				// get shell normal (1st)
				gp_Dir normalVec_1st = GetSurfaceNormal( edge, shellFace, U );

				// no valid tool direction from this point, give z-direction as default normal
				if( normalVec_1st == null ) {
					normalVec_1st = new gp_Dir( 0, 0, 1 );
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
				else if( bLast && i == segmentParamList.Count - 1 && ( cadPointList.Count > 0 || oneSegmentPointList.Count > 0 ) ) {

					// map the last point to the start point
					if( isClosed ) {
						CADPoint firstPoint = cadPointList.Count > 0 ? cadPointList[ 0 ] : oneSegmentPointList[ 0 ];
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
				if( ( i + 1 ) % 32 == 0 || i == segmentParamList.Count - 1 ) {
					// Report progress to the loading bar.
					reportProgress?.Invoke( new LoadingBarInfo( LoadingStep.CalculatePathPoints, i + 1, segmentParamList.Count ) );
				}
			}
			cadPointList.AddRange( oneSegmentPointList );
			return true;
		}

		public static void GetRefCenterDirFromContour( List<CADPoint> points, out gp_Ax1 refCoord )
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

		static gp_Dir GetSurfaceNormal( TopoDS_Edge edge, TopoDS_Face face, double param )
		{
			return GeometryTool.GetSurfaceNormal( edge, face, param );
		}

	}
}
