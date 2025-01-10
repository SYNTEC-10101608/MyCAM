using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructure
{
	public enum ToolVectorType
	{
		Default,
		Intersecting,
		TowardZ,
	}

	public class CADPoint
	{
		public CADPoint( gp_Pnt point, gp_Dir normalVec, gp_Dir tangentVec )
		{
			Point = point;
			NormalVec = normalVec;
			TangentVec = tangentVec;
		}

		public gp_Pnt Point
		{
			get; private set;
		}

		public gp_Dir NormalVec
		{
			get; private set;
		}

		public gp_Dir TangentVec
		{
			get; private set;
		}
	}

	public class CAMPoint
	{
		public CAMPoint( CADPoint cadPoint, gp_Dir toolVec )
		{
			CADPoint = cadPoint;
			ToolVec = toolVec;
		}

		public CADPoint CADPoint
		{
			get; private set;
		}

		public gp_Dir ToolVec
		{
			get; private set;
		}
	}

	public class CAMData
	{
		// CAD property
		public CAMData( CADData cadData )
		{
			CADData = cadData;

			// build raw data
			BuildCADPointList();
			BuildCAMPointList();
		}

		public CADData CADData
		{
			get; private set;
		}

		public List<CADPoint> CADPointList
		{
			get; private set;
		}

		// CAM property
		public List<CAMPoint> CAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList();
					m_IsDirty = false;
				}
				return m_CAMPointList;
			}
		}

		public List<CAMPoint> OffsetCAMPointList
		{
			get
			{
				if( m_IsDirty ) {
					BuildCAMPointList();
					m_IsDirty = false;
				}
				return m_OffsetCAMPointList;
			}
		}

		public ToolVectorType ToolVectorType
		{
			get
			{
				return m_ToolVectorType;
			}
			set
			{
				if( m_ToolVectorType != value ) {
					m_ToolVectorType = value;
					m_IsDirty = true;
				}
			}
		}

		public bool IsReverse
		{
			get
			{
				return m_IsReverse;
			}
			set
			{
				if( m_IsReverse != value ) {
					m_IsReverse = value;
					m_IsDirty = true;
				}
			}
		}

		public int StartPoint
		{
			get
			{
				return m_StartPoint;
			}
			set
			{
				if( m_StartPoint != value ) {
					m_StartPoint = value;
					m_IsDirty = true;
				}
			}
		}

		public double Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				if( m_Offset != value ) {
					m_Offset = value;
					m_IsDirty = true;
				}
			}
		}

		// backing fields
		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		List<CAMPoint> m_OffsetCAMPointList = new List<CAMPoint>();
		ToolVectorType m_ToolVectorType = ToolVectorType.Default;
		bool m_IsReverse = false;
		int m_StartPoint = 0;
		double m_Offset = 0;

		// dirty flag
		bool m_IsDirty = false;

		void BuildCADPointList()
		{
			CADPointList = new List<CADPoint>();
			if( CADData == null || CADData.Contour == null ) {
				return;
			}
			TopExp_Explorer edgeExp = new TopExp_Explorer( CADData.Contour, TopAbs_ShapeEnum.TopAbs_EDGE );

			// go through the contour edges
			while( edgeExp.More() ) {
				TopoDS_Shape edge = edgeExp.Current();
				edgeExp.Next();

				// get the solid face which the edge belongs to
				if( CADData.ShellMap == null || CADData.SolidMap == null ) {
					continue;
				}
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
				const double dSegmentLength = 0.5;
				SegmentTool.GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), dSegmentLength, false, out List<gp_Pnt> pointList );

				// get tool vector for each point
				foreach( gp_Pnt point in pointList ) {
					gp_Dir normalVec = VectorTool.GetFaceNormalVec( solidFace, point );
					gp_Dir tangentVec = VectorTool.GetEdgeTangentVec( TopoDS.ToEdge( edge ), point );
					CADPointList.Add( new CADPoint( point, normalVec, tangentVec ) );
				}
			}
		}

		void BuildCAMPointList()
		{
			m_CAMPointList = new List<CAMPoint>();

			// build cam points
			for( int i = 0; i < CADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = CADPointList[ i ];
				gp_Dir ToolVec;
				switch( ToolVectorType ) {
					case ToolVectorType.Default:
					default:
						ToolVec = cadPoint.NormalVec.Crossed( cadPoint.TangentVec );
						break;
					case ToolVectorType.Intersecting:

						// get average of cross of neighbor normals
						int lastIndex = i == 0 ? CADPointList.Count - 1 : i - 1;
						int nextIndex = i == CADPointList.Count - 1 ? 0 : i + 1;
						gp_Dir lastNormalVec = CADPointList[ lastIndex ].NormalVec;
						gp_Dir nextNormalVec = CADPointList[ nextIndex ].NormalVec;
						if( CADPointList[ i ].NormalVec.IsParallel( lastNormalVec, 1e-6 ) || CADPointList[ i ].NormalVec.IsParallel( nextNormalVec, 1e-6 ) ) {
							ToolVec = null;
						}
						else {
							gp_Dir ToolVec1 = cadPoint.NormalVec.Crossed( lastNormalVec );
							gp_Dir ToolVec2 = nextNormalVec.Crossed( cadPoint.NormalVec );
							ToolVec = new gp_Dir( ( ToolVec1.XYZ() + ToolVec2.XYZ() ) / 2 );
						}
						break;
					case ToolVectorType.TowardZ:
						ToolVec = new gp_Dir( 0, 0, 1 );
						break;
				}
				CAMPoint camPoint = new CAMPoint( cadPoint, ToolVec );
				m_CAMPointList.Add( camPoint );
			}

			// modify invalid tool vector with previous valid tool vector
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				if( m_CAMPointList[ i ].ToolVec == null ) {
					int stepIndex = 1;
					while( true ) {
						gp_Dir validVec = m_CAMPointList[ ( i - stepIndex + m_CAMPointList.Count ) % m_CAMPointList.Count ].ToolVec;
						if( validVec == null ) {
							stepIndex++;
						}
						else {
							m_CAMPointList[ i ] = new CAMPoint( m_CAMPointList[ i ].CADPoint, validVec );
							break;
						}
					}
				}
			}

			// rearrange cam points to start from the strt index
			if( StartPoint != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + StartPoint ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}

			// reverse the cad points if needed
			if( IsReverse ) {
				m_CAMPointList.Reverse();

				// modify index
				CAMPoint lastPoint = m_CAMPointList.Last();
				m_CAMPointList.Remove( lastPoint );
				m_CAMPointList.Insert( 0, lastPoint );
			}
			BuildOffsetPointList();
		}

		class LineRecord
		{
			public Tuple<CAMPoint, CAMPoint> OriPoint;
			public Tuple<CAMPoint, CAMPoint> OffsetPoint;
		}

		// TODO: code need refine
		void BuildOffsetPointList()
		{
			if( m_CAMPointList.Count == 0 ) {
				m_OffsetCAMPointList = new List<CAMPoint>();
				return;
			}

			// get line by points and offset the line
			List<LineRecord> lineRecordList = new List<LineRecord>();
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				gp_Pnt p1 = m_CAMPointList[ i ].CADPoint.Point;
				gp_Pnt p2 = m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.Point;

				// if p1 is equal to p2, skip
				if( p1.IsEqual( p2, 1e-6 ) ) { // TODO: tolerance
					continue;
				}

				// calculate average normal vector
				gp_Dir p1Normal = m_CAMPointList[ i ].CADPoint.NormalVec;
				gp_Dir p2Normal = m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.NormalVec;

				// offset the point by normal vector
				gp_Pnt p1Offset = new gp_Pnt( p1.XYZ() + p1Normal.XYZ() * Offset );
				gp_Pnt p2Offset = new gp_Pnt( p2.XYZ() + p2Normal.XYZ() * Offset );
				CAMPoint p1CAMOffset = new CAMPoint( new CADPoint( p1Offset, p1Normal, m_CAMPointList[ i ].CADPoint.TangentVec ), m_CAMPointList[ i ].ToolVec );
				CAMPoint p2CAMOffset = new CAMPoint( new CADPoint( p2Offset, p2Normal,
					m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.TangentVec ),
					m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].ToolVec );
				lineRecordList.Add( new LineRecord()
				{
					OriPoint = new Tuple<CAMPoint, CAMPoint>( m_CAMPointList[ i ], m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ] ),
					OffsetPoint = new Tuple<CAMPoint, CAMPoint>( p1CAMOffset, p2CAMOffset )
				} );
			}

			// clip the offset lines and get the intersect points
			m_OffsetCAMPointList = new List<CAMPoint>();
			for( int i = 0; i < lineRecordList.Count + 1; i++ ) {
				bool bClip = true;
				while( bClip ) {
					bClip = false;
					bool wraped = false;
					if( i == lineRecordList.Count - 1 ) {
						wraped = true;
					}
					LineRecord lineRecord1 = lineRecordList[ ( i + lineRecordList.Count ) % lineRecordList.Count ];
					LineRecord lineRecord2 = lineRecordList[ ( i + 1 + lineRecordList.Count ) % lineRecordList.Count ];
					Tuple<CAMPoint, CAMPoint> line1 = lineRecord1.OriPoint;
					Tuple<CAMPoint, CAMPoint> line2 = lineRecord2.OriPoint;
					Tuple<CAMPoint, CAMPoint> offsetLine1 = lineRecord1.OffsetPoint;
					Tuple<CAMPoint, CAMPoint> offsetLine2 = lineRecord2.OffsetPoint;

					// check if the offset vector is intersect
					gp_Pnt p1 = new gp_Pnt( ( line1.Item1.CADPoint.Point.XYZ() + line1.Item2.CADPoint.Point.XYZ() ) / 2 );
					gp_Pnt offsetP1 = new gp_Pnt( ( offsetLine1.Item1.CADPoint.Point.XYZ() + offsetLine1.Item2.CADPoint.Point.XYZ() ) / 2 );
					gp_Pnt p2 = new gp_Pnt( ( line2.Item1.CADPoint.Point.XYZ() + line2.Item2.CADPoint.Point.XYZ() ) / 2 );
					gp_Pnt offsetP2 = new gp_Pnt( ( offsetLine2.Item1.CADPoint.Point.XYZ() + offsetLine2.Item2.CADPoint.Point.XYZ() ) / 2 );

					// find the intersect point of the offset lines
					gp_Pnt intersectPoint = GeometryTool.FindIntersectPoint(
						offsetLine1.Item1.CADPoint.Point,
						offsetLine1.Item2.CADPoint.Point,
						offsetLine2.Item2.CADPoint.Point,
						offsetLine2.Item1.CADPoint.Point,
						out IntersectType typeL1, out IntersectType typeL2 );
					if( typeL1 == IntersectType.ReverseExtend ) {
						bClip = true;
						lineRecordList.Remove( lineRecord1 );

						// if the removed index is wraped, the index should be decreased
						if( wraped ) {
							i--;
						}
					}
					if( typeL2 == IntersectType.ReverseExtend ) {
						bClip = true;
						lineRecordList.Remove( lineRecord2 );
						i--;

						// if the removed index is wraped, the index should be decreased
						if( wraped ) {
							i--;
						}
					}
					if( bClip ) {
						continue;
					}
				}
			}

			// update the CAM point list
			for( int i = 0; i < lineRecordList.Count; i++ ) {
				int indexL1 = i;
				int indexL2 = ( i + 1 ) % lineRecordList.Count;
				gp_Pnt intersectPoint = GeometryTool.FindIntersectPoint(
					lineRecordList[ indexL1 ].OffsetPoint.Item1.CADPoint.Point,
					lineRecordList[ indexL1 ].OffsetPoint.Item2.CADPoint.Point,
					lineRecordList[ indexL2 ].OffsetPoint.Item2.CADPoint.Point,
					lineRecordList[ indexL2 ].OffsetPoint.Item1.CADPoint.Point,
					out _, out _ );

				// get average tagent, normal and tool vector
				gp_Dir tangentVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.CADPoint.TangentVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.CADPoint.TangentVec.XYZ() );
				gp_Dir normalVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.CADPoint.NormalVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.CADPoint.NormalVec.XYZ() );
				gp_Dir toolVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.ToolVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.ToolVec.XYZ() );
				m_OffsetCAMPointList.Add( new CAMPoint( new CADPoint( intersectPoint, normalVec, tangentVec ), toolVec ) );
			}

			// modify index
			CAMPoint lastPoint = m_OffsetCAMPointList.Last();
			m_OffsetCAMPointList.Remove( lastPoint );
			m_OffsetCAMPointList.Insert( 0, lastPoint );

			// add a point to close polygon
			CAMPoint closePoint = new CAMPoint(
				new CADPoint( lastPoint.CADPoint.Point, lastPoint.CADPoint.NormalVec, lastPoint.CADPoint.TangentVec ),
				lastPoint.ToolVec );
			m_OffsetCAMPointList.Add( closePoint );
		}
	}
}
