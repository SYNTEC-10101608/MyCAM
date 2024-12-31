using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System;
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

		class LineRecord
		{
			public Tuple<CAMPoint, CAMPoint> OriPoint;
			public Tuple<CAMPoint, CAMPoint> OffsetPoint;
		}

		void BuildCAMPointList()
		{
			CAMPointList = new List<CAMPoint>();
			TopExp_Explorer edgeExp = new TopExp_Explorer( CADData.Contour, TopAbs_ShapeEnum.TopAbs_EDGE );

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
				const double dSegmentLength = 1;
				SegmentTool.GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), dSegmentLength, true, true, out List<gp_Pnt> pointList );

				// get tool vector for each point
				foreach( gp_Pnt point in pointList ) {
					gp_Dir normalVec = VectorTool.GetFaceNormalVec( solidFace, point );
					gp_Dir tangentVec = VectorTool.GetEdgeTangentVec( TopoDS.ToEdge( edge ), point );
					gp_Dir toolVec = normalVec.Crossed( tangentVec );
					CAMPointList.Add( new CAMPoint( point, toolVec, normalVec, tangentVec ) );
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

			const double dOffset = 3;

			// offset each point
			List<LineRecord> lineRecordList = new List<LineRecord>();
			for( int i = 0; i < CAMPointList.Count; i++ ) {
				gp_Pnt p1 = CAMPointList[ i ].Point;
				gp_Pnt p2 = CAMPointList[ ( i + 1 ) % CAMPointList.Count ].Point;

				// if p1 is equal to p2, skip
				if( p1.IsEqual( p2, 1e-6 ) ) { // TODO: tolerance
					continue;
				}

				// calculate average normal vector
				gp_Dir p1Normal = CAMPointList[ i ].NormalVec;
				gp_Dir p2Normal = CAMPointList[ ( i + 1 ) % CAMPointList.Count ].NormalVec;

				// offset the point by normal vector
				gp_Pnt p1Offset = new gp_Pnt( p1.XYZ() + p1Normal.XYZ() * dOffset );
				gp_Pnt p2Offset = new gp_Pnt( p2.XYZ() + p2Normal.XYZ() * dOffset );
				CAMPoint p1CAMOffset = new CAMPoint( p1Offset, CAMPointList[ i ].ToolVec, CAMPointList[ i ].NormalVec, CAMPointList[ i ].TangentVec );
				CAMPoint p2CAMOffset = new CAMPoint( p2Offset,
					CAMPointList[ ( i + 1 ) % CAMPointList.Count ].ToolVec,
					CAMPointList[ ( i + 1 ) % CAMPointList.Count ].NormalVec,
					CAMPointList[ ( i + 1 ) % CAMPointList.Count ].TangentVec );
				lineRecordList.Add( new LineRecord()
				{
					OriPoint = new Tuple<CAMPoint, CAMPoint>( CAMPointList[ i ], CAMPointList[ ( i + 1 ) % CAMPointList.Count ] ),
					OffsetPoint = new Tuple<CAMPoint, CAMPoint>( p1CAMOffset, p2CAMOffset )
				} );
			}

			// clip the offset lines
			bool bNeedClip = true;
			while( bNeedClip ) {
				bNeedClip = false;
				for( int i = 0; i < lineRecordList.Count; i++ ) {
					LineRecord lineRecord1 = lineRecordList[ i ];
					LineRecord lineRecord2 = lineRecordList[ ( i + 1 ) % lineRecordList.Count ];
					Tuple<CAMPoint, CAMPoint> line1 = lineRecord1.OriPoint;
					Tuple<CAMPoint, CAMPoint> line2 = lineRecord2.OriPoint;
					Tuple<CAMPoint, CAMPoint> offsetLine1 = lineRecord1.OffsetPoint;
					Tuple<CAMPoint, CAMPoint> offsetLine2 = lineRecord2.OffsetPoint;

					// check if the offset vector is intersect
					gp_Pnt p1 = new gp_Pnt( ( line1.Item1.Point.XYZ() + line1.Item2.Point.XYZ() ) / 2 );
					gp_Pnt offsetP1 = new gp_Pnt( ( offsetLine1.Item1.Point.XYZ() + offsetLine1.Item2.Point.XYZ() ) / 2 );
					gp_Pnt p2 = new gp_Pnt( ( line2.Item1.Point.XYZ() + line2.Item2.Point.XYZ() ) / 2 );
					gp_Pnt offsetP2 = new gp_Pnt( ( offsetLine2.Item1.Point.XYZ() + offsetLine2.Item2.Point.XYZ() ) / 2 );
					FindIntersectPoint( p1, offsetP1, p2, offsetP2, out IntersectType typeV1, out IntersectType typeV2 );
					if( typeV1 == IntersectType.Inbetween ) {
						lineRecordList.Remove( lineRecord2 );
						bNeedClip = true;
					}
					if( typeV2 == IntersectType.Inbetween ) {
						lineRecordList.Remove( lineRecord1 );
						bNeedClip = true;
					}
					if( bNeedClip ) {
						break;
					}
				}
			}

			// update the CAM point list
			CAMPointList = new List<CAMPoint>();
			for( int i = 0; i < lineRecordList.Count; i++ ) {
				int indexL1 = i;
				int indexL2 = ( i + 1 ) % lineRecordList.Count;
				gp_Pnt intersectPoint = FindIntersectPoint(
					lineRecordList[ indexL1 ].OffsetPoint.Item1.Point,
					lineRecordList[ indexL1 ].OffsetPoint.Item2.Point,
					lineRecordList[ indexL2 ].OffsetPoint.Item2.Point,
					lineRecordList[ indexL2 ].OffsetPoint.Item1.Point,
					out _, out _ );

				// get average tagent, normal and tool vector
				gp_Dir tangentVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.TangentVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.TangentVec.XYZ() );
				gp_Dir normalVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.NormalVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.NormalVec.XYZ() );
				gp_Dir toolVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.ToolVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.ToolVec.XYZ() );
				CAMPointList.Add( new CAMPoint( intersectPoint, toolVec, normalVec, tangentVec ) );
			}
		}

		enum IntersectType
		{
			Extend,
			ReverseExtend,
			Inbetween,
			NoIntersect
		}

		gp_Pnt FindIntersectPoint( gp_Pnt p1, gp_Pnt p2, gp_Pnt p3, gp_Pnt p4,
			out IntersectType TypeL1, out IntersectType TypeL2 )
		{
			gp_Vec d1 = new gp_Vec( p2.XYZ() - p1.XYZ() ); // Direction vector of line 1
			gp_Vec d2 = new gp_Vec( p4.XYZ() - p3.XYZ() ); // Direction vector of line 2
			gp_Vec r = new gp_Vec( p1.XYZ() - p3.XYZ() );  // Vector between the origins of the two lines

			double a = d1.Dot( d1 ); // Dot product of d1 with itself
			double b = d1.Dot( d2 ); // Dot product of d1 with d2
			double c = d2.Dot( d2 ); // Dot product of d2 with itself
			double d = d1.Dot( r );  // Dot product of d1 with r
			double e = d2.Dot( r );  // Dot product of d2 with r

			// Denominator for solving t and s
			double denominator = a * c - b * b;
			if( Math.Abs( denominator ) < 1e-6 ) { //TODO: tolerance
				TypeL1 = IntersectType.NoIntersect;
				TypeL2 = IntersectType.NoIntersect;
				return new gp_Pnt( ( p2.XYZ() + p3.XYZ() ) / 2 );
			}

			double t = ( b * e - c * d ) / denominator; // Parameter t for line 1
			double s = ( a * e - b * d ) / denominator; // Parameter s for line 2

			// Compute the closest points on both lines
			gp_Pnt closestPointOnLine1 = new gp_Pnt( p1.XYZ() + d1.XYZ() * t );
			gp_Pnt closestPointOnLine2 = new gp_Pnt( p3.XYZ() + d2.XYZ() * s );

			// Compute the midpoint of the shortest segment
			gp_Pnt midpoint = new gp_Pnt( ( closestPointOnLine1.XYZ() + closestPointOnLine2.XYZ() ) / 2 );
			if( t <= 0 ) {
				TypeL1 = IntersectType.ReverseExtend;
			}
			else if( t >= 1 ) {
				TypeL1 = IntersectType.Extend;
			}
			else {
				TypeL1 = IntersectType.Inbetween;
			}
			if( s <= 0 ) {
				TypeL2 = IntersectType.ReverseExtend;
			}
			else if( s >= 1 ) {
				TypeL2 = IntersectType.Extend;
			}
			else {
				TypeL2 = IntersectType.Inbetween;
			}

			return midpoint;
		}
	}
}
