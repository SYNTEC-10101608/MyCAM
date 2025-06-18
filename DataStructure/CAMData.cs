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
		public CADPoint( gp_Pnt point, gp_Dir normalVec_1st, gp_Dir normalVec_2nd, gp_Dir tangentVec )
		{
			Point = point;
			NormalVec_1st = normalVec_1st;
			NormalVec_2nd = normalVec_2nd;
			TangentVec = tangentVec;
		}

		public gp_Pnt Point
		{
			get; private set;
		}

		public gp_Dir NormalVec_1st
		{
			get; private set;
		}

		// normal vector on co-face
		public gp_Dir NormalVec_2nd
		{
			get; private set;
		}

		// tangent vector on path
		public gp_Dir TangentVec
		{
			get; private set;
		}

		public CADPoint Clone()
		{
			return new CADPoint( Point, NormalVec_1st, NormalVec_2nd, TangentVec );
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

		public CAMPoint Clone()
		{
			return new CAMPoint( CADPoint.Clone(), new gp_Dir( ToolVec.XYZ() ) );
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
			//GetIntersectingDir();
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

		public void SetToolVecModify( int index, double dRA_deg, double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				m_ToolVecModifyMap[ index ] = new Tuple<double, double>( dRA_deg, dRB_deg );
			}
			else {
				m_ToolVecModifyMap.Add( index, new Tuple<double, double>( dRA_deg, dRB_deg ) );
			}
			m_IsDirty = true;
		}

		public void GetToolVecModify( int index, out double dRA_deg, out double dRB_deg )
		{
			if( m_ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_ToolVecModifyMap[ index ].Item1;
				dRB_deg = m_ToolVecModifyMap[ index ].Item2;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;
			}
		}

		public HashSet<int> GetToolVecModifyIndex()
		{
			HashSet<int> result = new HashSet<int>();
			foreach( int nIndex in m_ToolVecModifyMap.Keys ) {
				result.Add( nIndex );
			}
			return result;
		}

		// view testing
		public List<int> EdgeStartIndex
		{
			get; private set;
		}

		// backing fields
		List<CAMPoint> m_CAMPointList = new List<CAMPoint>();
		Dictionary<int, Tuple<double, double>> m_ToolVecModifyMap = new Dictionary<int, Tuple<double, double>>();
		bool m_IsReverse = false;
		int m_StartPoint = 0;
		double m_Offset = 0;

		// CADCAM index map
		Dictionary<int, int> m_IndexMap = new Dictionary<int, int>();

		// intersecting direction
		gp_Dir m_IntersectingDir = new gp_Dir( 0, 0, 1 );

		// dirty flag
		bool m_IsDirty = false;

		void BuildCADPointList()
		{
			CADPointList = new List<CADPoint>();
			EdgeStartIndex = new List<int>();
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
				TopoDS_Face shellDace = TopoDS.ToFace( shellFaceList[ 0 ] );
				TopoDS_Face solidFace = TopoDS.ToFace( solidFaceList[ 0 ] );

				// break the edge into segment points by interval
				const double dSegmentLength = 0.01;
				SegmentTool.GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), dSegmentLength, false, out List<gp_Pnt> pointList );

				// get tool vector for each point
				for( int i = 0; i < pointList.Count; i++ ) {
					gp_Pnt point = pointList[ i ];
					gp_Dir normalVec_1st = VectorTool.GetFaceNormalVec( shellDace, point );
					gp_Dir normalVec_2nd = VectorTool.GetFaceNormalVec( solidFace, point );
					gp_Dir tangentVec = VectorTool.GetEdgeTangentVec( TopoDS.ToEdge( edge ), point );
					CADPointList.Add( new CADPoint( point, normalVec_1st, normalVec_2nd, tangentVec ) );
					if( i == 0 ) {
						EdgeStartIndex.Add( CADPointList.Count - 1 );
					}
				}
			}

			// filter the coincident points
			for( int i = 0; i < CADPointList.Count - 1; i++ ) {
				if( CADPointList[ i ].Point.IsEqual( CADPointList[ i + 1 ].Point, 1e-3 ) ) {
					CADPointList.RemoveAt( i );
					i--;
				}
			}
			CADPointList.RemoveAt( CADPointList.Count - 1 );
		}

		void BuildCAMPointList()
		{
			m_IsDirty = false;
			m_CAMPointList = new List<CAMPoint>();
			SetToolVec();
			SetStartPoint();
			SetOrientation();
			DoOffset();
			//SetLead();
		}

		void SetToolVec()
		{
			for( int i = 0; i < CADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = CADPointList[ i ];
				//gp_Dir toolVec = cadPoint.NormalVec_2nd.Crossed( cadPoint.TangentVec );
				CAMPoint camPoint = new CAMPoint( cadPoint, cadPoint.NormalVec_1st );
				m_CAMPointList.Add( camPoint );
			}
			ModifyToolVec();
		}

		void ModifyToolVec()
		{
			if( m_ToolVecModifyMap.Count == 0 ) {
				return;
			}

			// sort the modify data by index
			List<int> indexInOrder = m_ToolVecModifyMap.Keys.ToList();
			indexInOrder.Sort();

			// modify the tool vector
			for( int i = 0; i < indexInOrder.Count; i++ ) {

				// get start and end index
				int nStartIndex = indexInOrder[ i ];
				int nEndIndex = indexInOrder[ ( i + 1 ) % indexInOrder.Count ];
				int nEndIndexModify = nEndIndex <= nStartIndex ? nEndIndex + m_CAMPointList.Count : nEndIndex;

				// do slerp from start to end
				gp_Vec startVec = GetVecFromAB( m_CAMPointList[ nStartIndex ],
					m_ToolVecModifyMap[ nStartIndex ].Item1 * Math.PI / 180,
					m_ToolVecModifyMap[ nStartIndex ].Item2 * Math.PI / 180 );
				gp_Vec endVec = GetVecFromAB( m_CAMPointList[ nEndIndex ],
					m_ToolVecModifyMap[ nEndIndex ].Item1 * Math.PI / 180,
					m_ToolVecModifyMap[ nEndIndex ].Item2 * Math.PI / 180 );
				gp_Quaternion q12 = new gp_Quaternion( startVec, endVec );
				gp_QuaternionSLerp slerp = new gp_QuaternionSLerp( new gp_Quaternion(), q12 );
				m_CAMPointList[ nStartIndex ] = new CAMPoint( m_CAMPointList[ nStartIndex ].CADPoint, new gp_Dir( startVec ) );
				for( int j = nStartIndex + 1; j < nEndIndexModify; j++ ) {
					double t = ( j - nStartIndex ) / (double)( nEndIndexModify - nStartIndex );
					gp_Quaternion q = new gp_Quaternion();
					slerp.Interpolate( t, ref q );
					gp_Trsf trsf = new gp_Trsf();
					trsf.SetRotation( q );
					gp_Dir toolVec = new gp_Dir( startVec.Transformed( trsf ) );
					m_CAMPointList[ j % m_CAMPointList.Count ] = new CAMPoint( m_CAMPointList[ j % m_CAMPointList.Count ].CADPoint, toolVec );
				}
			}
		}

		gp_Quaternion GetQuaternionFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Quaternion();
			}

			// get the quaternion
			gp_Vec v0 = new gp_Vec( camPoint.ToolVec );
			gp_Vec v1 = GetVecFromAB( camPoint, dRA_rad, dRB_rad );
			return new gp_Quaternion( v0, v1 );
		}

		gp_Vec GetVecFromAB( CAMPoint camPoint, double dRA_rad, double dRB_rad )
		{
			// TDOD: RA == 0 || RB == 0
			if( dRA_rad == 0 && dRB_rad == 0 ) {
				return new gp_Vec( camPoint.ToolVec );
			}

			// get the x, y, z direction
			gp_Dir x = camPoint.CADPoint.TangentVec;
			gp_Dir z = camPoint.CADPoint.NormalVec_1st;
			gp_Dir y = z.Crossed( x );

			// X:Y:Z = tanA:tanB:1
			double X = 0;
			double Y = 0;
			double Z = 0;
			if( dRA_rad == 0 ) {
				X = 0;
				Z = 1;
			}
			else {
				X = dRA_rad < 0 ? -1 : 1;
				Z = X / Math.Tan( dRA_rad );
			}
			Y = Z * Math.Tan( dRB_rad );
			gp_Dir dir1 = new gp_Dir( x.XYZ() * X + y.XYZ() * Y + z.XYZ() * Z );
			return new gp_Vec( dir1.XYZ() );
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
			if( m_CAMPointList.Count == 0 || m_Offset == 0 ) {
				return;
			}

			// get line by points and offset the line
			List<LineRecord> lineRecordList = new List<LineRecord>();
			for( int i = 0; i < m_CAMPointList.Count; i++ ) {
				int i1 = i;
				int i2 = ( i + 1 ) % m_CAMPointList.Count;
				gp_Pnt p1 = m_CAMPointList[ i ].CADPoint.Point;
				gp_Pnt p2 = m_CAMPointList[ i2 ].CADPoint.Point;

				// if p1 is equal to p2, skip, this will result a "milter"
				if( p1.IsEqual( p2, 1e-6 ) ) {
					continue;
				}

				// calculate the offset direction by cross of tangent and tool vector
				gp_Dir p1OffsetDir = m_CAMPointList[ i ].CADPoint.TangentVec.Crossed( m_CAMPointList[ i ].ToolVec );
				gp_Dir p2OffsetDir = m_CAMPointList[ i2 ].CADPoint.TangentVec.Crossed( m_CAMPointList[ i2 ].ToolVec );

				// offset the point by offset vector
				gp_Pnt p1Offset = new gp_Pnt( p1.XYZ() + p1OffsetDir.XYZ() * Offset );
				gp_Pnt p2Offset = new gp_Pnt( p2.XYZ() + p2OffsetDir.XYZ() * Offset );
				if( p1Offset.IsEqual( p2Offset, 1e-6 ) ) {
					continue;
				}
				CAMPoint p1CAMOffset = new CAMPoint( OffsetCADPoint( m_CAMPointList[ i ].CADPoint, p1Offset ), m_CAMPointList[ i ].ToolVec );
				CAMPoint p2CAMOffset = new CAMPoint( OffsetCADPoint( m_CAMPointList[ i2 ].CADPoint, p2Offset ), m_CAMPointList[ i2 ].ToolVec );
				lineRecordList.Add( new LineRecord()
				{
					OriPoint = new Tuple<CAMPoint, CAMPoint>( m_CAMPointList[ i ], m_CAMPointList[ i2 ] ),
					OffsetPoint = new Tuple<CAMPoint, CAMPoint>( p1CAMOffset, p2CAMOffset )
				} );
			}
			if( lineRecordList.Count == 0 ) {
				return;
			}

			// clip the self-intersecting lines
			List<CAMPoint> offsetCAMPointList = new List<CAMPoint>();
			for( int i = 0; i < lineRecordList.Count + 1; i++ ) {
				bool bClip = true;
				while( bClip ) {
					if( lineRecordList.Count == 0 ) {
						return;
					}
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
					else {
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
					else {
					}
				}
			}

			// cant construct a loop
			if( lineRecordList.Count < 3 ) {
				return;
			}

			// serialize the offset lines
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
				gp_Dir normalVec_1st = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.CADPoint.NormalVec_1st.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.CADPoint.NormalVec_1st.XYZ() );
				gp_Dir normalVec_2nd = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.CADPoint.NormalVec_2nd.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.CADPoint.NormalVec_2nd.XYZ() );
				gp_Dir toolVec = new gp_Dir( lineRecordList[ indexL1 ].OriPoint.Item2.ToolVec.XYZ() + lineRecordList[ indexL2 ].OriPoint.Item1.ToolVec.XYZ() );
				offsetCAMPointList.Add( new CAMPoint( new CADPoint( intersectPoint, normalVec_1st, normalVec_2nd, tangentVec ), toolVec ) );
			}

			// modify index
			CAMPoint lastPoint = offsetCAMPointList.Last();
			offsetCAMPointList.Remove( lastPoint );
			offsetCAMPointList.Insert( 0, lastPoint );

			// add a point to close polygon
			CAMPoint closePoint = new CAMPoint(
				new CADPoint( lastPoint.CADPoint.Point,
				lastPoint.CADPoint.NormalVec_1st,
				lastPoint.CADPoint.NormalVec_2nd,
				lastPoint.CADPoint.TangentVec ), lastPoint.ToolVec );
			offsetCAMPointList.Add( closePoint );
			m_CAMPointList = offsetCAMPointList;
		}

		CADPoint OffsetCADPoint( CADPoint originalPoint, gp_Pnt newLocation )
		{
			return new CADPoint( newLocation,
				originalPoint.NormalVec_1st,
				originalPoint.NormalVec_2nd,
				originalPoint.TangentVec );
		}

		void SetLead()
		{

		}
	}
}
