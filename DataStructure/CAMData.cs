using OCC.gp;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System.Collections.Generic;

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
			Point = cadPoint.Point;
			ToolVec = toolVec;
		}

		public gp_Pnt Point
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

			// rearrange cad points to start from the strt index
			List<CADPoint> rearrangedCADPointList = new List<CADPoint>();
			for( int i = 0; i < CADPointList.Count; i++ ) {
				rearrangedCADPointList.Add( CADPointList[ ( i + StartPoint ) % CADPointList.Count ] );
			}

			// reverse the cad points if needed
			if( IsReverse ) {
				rearrangedCADPointList.Reverse();
			}

			// build cam points
			for( int i = 0; i < rearrangedCADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = rearrangedCADPointList[ i ];
				gp_Dir ToolVec;
				switch( ToolVectorType ) {
					case ToolVectorType.Default:
					default:
						ToolVec = cadPoint.NormalVec.Crossed( cadPoint.TangentVec );
						break;
					case ToolVectorType.Intersecting:

						// TODO: implement this
						ToolVec = new gp_Dir( 0, 0, 1 );
						break;
					case ToolVectorType.TowardZ:
						ToolVec = new gp_Dir( 0, 0, 1 );
						break;
				}
				CAMPoint camPoint = new CAMPoint( cadPoint, ToolVec );
				m_CAMPointList.Add( camPoint );
			}
		}
	}
}
