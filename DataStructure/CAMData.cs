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

		// normal vector on co-face
		public gp_Dir NormalVec
		{
			get; private set;
		}

		// tangent vector on path
		public gp_Dir TangentVec
		{
			get; private set;
		}
	}

	// currently assuming CAM = CAD + ToolVec
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
			GetIntersectingDir();
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
		ToolVectorType m_ToolVectorType = ToolVectorType.Default;
		bool m_IsReverse = false;
		int m_StartPoint = 0;
		double m_Offset = 0;

		// intersecting direction
		gp_Dir m_IntersectingDir = new gp_Dir( 0, 0, 1 );

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
				const double dSegmentLength = 1;
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
			m_IsDirty = false;
			m_CAMPointList = new List<CAMPoint>();
			SetToolVec();
			SetStartPoint();
			SetOrientation();
			DoOffset();
			SetLead();
		}

		void SetToolVec()
		{
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
						ToolVec = m_IntersectingDir;
						break;
					case ToolVectorType.TowardZ:
						ToolVec = new gp_Dir( 0, 0, 1 );
						break;
				}
				CAMPoint camPoint = new CAMPoint( cadPoint, ToolVec );
				m_CAMPointList.Add( camPoint );
			}
		}

		void GetIntersectingDir()
		{
			// get two non-parallel normal vectors if possible
			gp_Dir normalVec1 = CADPointList[ 0 ].NormalVec;
			gp_Dir normalVec2 = null;
			for( int i = 1; i < CADPointList.Count; i++ ) {
				gp_Dir temp = CADPointList[ i ].NormalVec;
				if( !temp.IsParallel( normalVec1, 1e-6 ) ) { // TODO: tolerance
					normalVec2 = temp;
					break;
				}
			}
			if( normalVec2 == null ) {
				return;
			}

			// assume the intersecting direction is the cross product of the two normal vectors
			gp_Dir tempIntersectingDir = normalVec2.Crossed( normalVec1 );

			// check is all normal vectors are perpendicular to the intersecting direction
			for( int i = 0; i < CADPointList.Count; i++ ) {
				if( !CADPointList[ i ].NormalVec.IsNormal( tempIntersectingDir, 1e-6 ) ) { // TODO: tolerance
					return;
				}
			}
			m_IntersectingDir = tempIntersectingDir;
			m_ToolVectorType = ToolVectorType.Intersecting;
		}

		void SetStartPoint()
		{
			// rearrange cam points to start from the strt index
			if( StartPoint != 0 ) {
				List<CAMPoint> newCAMPointList = new List<CAMPoint>();
				for( int i = 0; i < m_CAMPointList.Count; i++ ) {
					newCAMPointList.Add( m_CAMPointList[ ( i + StartPoint ) % m_CAMPointList.Count ] );
				}
				m_CAMPointList = newCAMPointList;
			}
		}

		void SetOrientation()
		{
			// reverse the cad points if is reverse
			if( IsReverse ) {
				m_CAMPointList.Reverse();

				// modify index
				CAMPoint lastPoint = m_CAMPointList.Last();
				m_CAMPointList.Remove( lastPoint );
				m_CAMPointList.Insert( 0, lastPoint );
			}
		}

		class LineRecord
		{
			public Tuple<CAMPoint, CAMPoint> OriPoint;
			public Tuple<CAMPoint, CAMPoint> OffsetPoint;
		}

		void DoOffset()
		{
			if( m_CAMPointList.Count == 0 ) {
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

				// calculate the offset direction by cross of tangent and tool vector
				gp_Dir p1OffsetDir = m_CAMPointList[ i ].CADPoint.TangentVec.Crossed( m_CAMPointList[ i ].ToolVec );
				gp_Dir p2OffsetDir = m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.TangentVec
					.Crossed( m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].ToolVec );

				// offset the point by offset vector
				gp_Pnt p1Offset = new gp_Pnt( p1.XYZ() + p1OffsetDir.XYZ() * Offset );
				gp_Pnt p2Offset = new gp_Pnt( p2.XYZ() + p2OffsetDir.XYZ() * Offset );
				CAMPoint p1CAMOffset = new CAMPoint( new CADPoint( p1Offset, p1OffsetDir, m_CAMPointList[ i ].CADPoint.TangentVec ), m_CAMPointList[ i ].ToolVec );
				CAMPoint p2CAMOffset = new CAMPoint( new CADPoint( p2Offset, p2OffsetDir, m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].CADPoint.TangentVec ),
					m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ].ToolVec );
				lineRecordList.Add( new LineRecord()
				{
					OriPoint = new Tuple<CAMPoint, CAMPoint>( m_CAMPointList[ i ], m_CAMPointList[ ( i + 1 ) % m_CAMPointList.Count ] ),
					OffsetPoint = new Tuple<CAMPoint, CAMPoint>( p1CAMOffset, p2CAMOffset )
				} );
			}

			// clip the offset lines and get the intersect points
			List<CAMPoint> offsetCAMPointList = new List<CAMPoint>();
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
					Tuple<CAMPoint, CAMPoint> offsetLine1 = lineRecord1.OffsetPoint;
					Tuple<CAMPoint, CAMPoint> offsetLine2 = lineRecord2.OffsetPoint;

					// find the intersect point of the offset lines
					GeometryTool.FindIntersectPoint(
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
				offsetCAMPointList.Add( new CAMPoint( new CADPoint( intersectPoint, normalVec, tangentVec ), toolVec ) );
			}

			// modify index
			CAMPoint lastPoint = offsetCAMPointList.Last();
			offsetCAMPointList.Remove( lastPoint );
			offsetCAMPointList.Insert( 0, lastPoint );

			// add a point to close polygon
			CAMPoint closePoint = new CAMPoint(
				new CADPoint( lastPoint.CADPoint.Point, lastPoint.CADPoint.NormalVec, lastPoint.CADPoint.TangentVec ),
				lastPoint.ToolVec );
			offsetCAMPointList.Add( closePoint );
			m_CAMPointList = offsetCAMPointList;
		}

		void SetLead()
		{

		}
	}
}
