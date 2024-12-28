using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System.Collections.Generic;

namespace DataStructure
{
	public class CAMPoint
	{
		public CAMPoint( gp_Pnt point, gp_Dir toolVec, gp_Dir normalVec, gp_Dir tangentVec )
		{
			Point = point;
			ToolVec = toolVec;
			NormalVec = normalVec;
			TangentVec = tangentVec;
		}

		public gp_Pnt Point
		{
			get; private set;
		}

		public gp_Dir ToolVec
		{
			get; private set;
		}

		// the normal and tangent is temporary reserved for testing
		public gp_Dir NormalVec
		{
			get; private set;
		}

		public gp_Dir TangentVec
		{
			get; private set;
		}
	}

	public class CAMData
	{
		public CAMData( CADData cadData )
		{
			CADData = cadData;
			BuildCAMPointList();
		}

		public CADData CADData
		{
			get; private set;
		}

		public List<CAMPoint> CAMPointList
		{
			get; private set;
		}

		void BuildCAMPointList()
		{
			CAMPointList = new List<CAMPoint>();
			TopExp_Explorer edgeExp = new TopExp_Explorer( CADData.Contour, TopAbs_ShapeEnum.TopAbs_EDGE );

			// TEST: for checking if the point is the same as the last point
			gp_Pnt lastPoint = new gp_Pnt();

			// go through the contour edges
			while( edgeExp.More() ) {
				TopoDS_Shape edge = edgeExp.Current();
				edgeExp.Next();

				// get the solid face which the edge belongs to
				List<TopoDS_Shape> shellFaceList = CADData.ShellMap.FindFromKey( edge ).elementsAsList;
				List<TopoDS_Shape> solidFaceList = CADData.SolidMap.FindFromKey( edge ).elementsAsList;
				if( shellFaceList == null || solidFaceList == null ) {
					continue;
				}
				if( shellFaceList.Count != 1 || solidFaceList.Count != 2 ) {
					throw new System.ArgumentException( ToString() + "BuildToolVecList: Mapping Error" );
				}
				for( int i = 0; i < solidFaceList.Count; i++ ) {
					if( solidFaceList[ i ].IsEqual( shellFaceList[ 0 ] ) ) {
						solidFaceList.RemoveAt( i );
						break;
					}
				}
				TopoDS_Face solidFace = TopoDS.ToFace( solidFaceList[ 0 ] );

				// break the edge into segment points by interval
				const double dSegmentLength = 2.5;
				SegmentTool.GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), dSegmentLength, true, true, out List<gp_Pnt> pointList );

				// TEST: get the first point
				if( CAMPointList.Count == 0 ) {
					lastPoint = pointList[ pointList.Count - 1 ];
				}

				// get tool vector for each point
				foreach( gp_Pnt point in pointList ) {
					gp_Dir normalVec = VectorTool.GetFaceNormalVec( solidFace, point );
					gp_Dir tangentVec = VectorTool.GetEdgeTangentVec( TopoDS.ToEdge( edge ), point );
					gp_Dir toolVec = normalVec.Crossed( tangentVec );

					// check if the point is the same as the last point
					gp_Vec disToLast = new gp_Vec( point.XYZ() - lastPoint.XYZ() );
					if( CAMPointList.Count > 0 && disToLast.Magnitude() < 0.001 ) {

						// using the average vectors
						normalVec = new gp_Dir( ( normalVec.XYZ() + CAMPointList[ CAMPointList.Count - 1 ].NormalVec.XYZ() ) / 2 );
						tangentVec = new gp_Dir( ( tangentVec.XYZ() + CAMPointList[ CAMPointList.Count - 1 ].TangentVec.XYZ() ) / 2 );
						toolVec = new gp_Dir( ( toolVec.XYZ() + CAMPointList[ CAMPointList.Count - 1 ].ToolVec.XYZ() ) / 2 );

						// remove the last point
						CAMPointList.RemoveAt( CAMPointList.Count - 1 );
					}
					CAMPointList.Add( new CAMPoint( point, toolVec, normalVec, tangentVec ) );
					lastPoint = point;
				}
			}

			// TEST: offset
			Offset();
		}

		void Offset()
		{
			if( CAMPointList.Count == 0 ) {
				return;
			}

			const double dOffset = 5;
			List<CAMPoint> offsetPointList = new List<CAMPoint>();
			for( int i = 0; i < CAMPointList.Count; i++ ) {
				gp_Pnt p1 = CAMPointList[ i ].Point;
				gp_Pnt p2 = CAMPointList[ ( i + 1 ) % CAMPointList.Count ].Point;
				gp_Pnt p3 = CAMPointList[ ( i + 2 ) % CAMPointList.Count ].Point;
				gp_Pnt p12_1 = new gp_Pnt( p1.XYZ() + CAMPointList[ i ].NormalVec.XYZ() * dOffset );
				gp_Pnt p12_2 = new gp_Pnt( p2.XYZ() + CAMPointList[ i ].NormalVec.XYZ() * dOffset );
				gp_Pnt p23_1 = new gp_Pnt( p2.XYZ() + CAMPointList[ ( i + 1 ) % CAMPointList.Count ].NormalVec.XYZ() * dOffset );
				gp_Pnt p23_2 = new gp_Pnt( p3.XYZ() + CAMPointList[ ( i + 1 ) % CAMPointList.Count ].NormalVec.XYZ() * dOffset );
				gp_Pnt pIntersect = FindIntersectPoint( p12_1, p12_2, p23_1, p23_2 );
				offsetPointList.Add( new CAMPoint( pIntersect,
					CAMPointList[ ( i + 1 ) % CAMPointList.Count ].ToolVec,
					CAMPointList[ ( i + 1 ) % CAMPointList.Count ].NormalVec,
					CAMPointList[ ( i + 1 ) % CAMPointList.Count ].TangentVec ) );
			}
			CAMPointList = offsetPointList;
		}

		gp_Pnt FindIntersectPoint( gp_Pnt p12_1, gp_Pnt p12_2, gp_Pnt p23_1, gp_Pnt p23_2 )
		{
			// Direction of line <p1, p2>
			gp_Vec d1 = new gp_Vec( p12_2.XYZ() - p12_1.XYZ() );

			// Direction of line <p3, p4>
			gp_Vec d2 = new gp_Vec( p23_2.XYZ() - p23_1.XYZ() );

			// Check if lines are parallel
			gp_Vec cross = d1.Crossed( d2 );
			double crossMagnitude = cross.Magnitude();
			if( crossMagnitude < 0.001 ) {

				// Use the midpoint of p2 and p3
				return new gp_Pnt( ( p12_2.XYZ() + p23_1.XYZ() ) / 2 );
			}

			// Check if lines intersect
			gp_Vec r = new gp_Vec( p12_1.XYZ() - p23_1.XYZ() );
			double t = ( r.Dot( d2.Crossed( cross ) ) ) / crossMagnitude / crossMagnitude;
			double s = ( r.Dot( d1.Crossed( cross ) ) ) / crossMagnitude / crossMagnitude;

			gp_Pnt closestOnL1 = new gp_Pnt( p12_1.XYZ() + d1.XYZ() * t );
			gp_Pnt closestOnL2 = new gp_Pnt( p23_1.XYZ() + d2.XYZ() * s );
			gp_Vec v = new gp_Vec( closestOnL1.XYZ() - closestOnL2.XYZ() );

			// Lines intersect, Use the intersection point
			if( v.Magnitude() < 0.001 ) {
				return closestOnL1;
			}

			// Lines are skew
			return new gp_Pnt( ( closestOnL1.XYZ() + closestOnL2.XYZ() ) / 2 );
		}
	}
}
