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
		NormalXTangent,
		CrossOfNormals,
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
		public CAMData( CADData cadData )
		{
			CADData = cadData;

			// initialize fields
			CADPointList = new List<CADPoint>();
			CAMPointList = new List<CAMPoint>();
			ToolVectorType = ToolVectorType.NormalXTangent;
			IsReverse = false;
			StartPoint = 0;
			Offset = 0;

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

		public List<CAMPoint> CAMPointList
		{
			get; private set;
		}

		public ToolVectorType ToolVectorType
		{
			get; set;
		}

		public bool IsReverse
		{
			get; set;
		}

		public int StartPoint
		{
			get; set;
		}

		public double Offset
		{
			get; set;
		}

		void BuildCADPointList()
		{
			CADPointList = new List<CADPoint>();
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
			CAMPointList = new List<CAMPoint>();
			for( int i = 0; i < CADPointList.Count; i++ ) {

				// calculate tool vector
				CADPoint cadPoint = CADPointList[ i ];
				gp_Dir ToolVec;
				switch( ToolVectorType ) {
					case ToolVectorType.NormalXTangent:
					default:
						ToolVec = cadPoint.NormalVec.Crossed( cadPoint.TangentVec );
						break;
					case ToolVectorType.CrossOfNormals:

						// get average of cross of neighbor normals
						int lastIndex = i == 0 ? CADPointList.Count - 1 : i - 1;
						int nextIndex = i == CADPointList.Count - 1 ? 0 : i + 1;
						gp_Dir lastNormalVec = CADPointList[ lastIndex ].NormalVec;
						gp_Dir nextNormalVec = CADPointList[ nextIndex ].NormalVec;
						gp_Dir TooV1 = lastNormalVec.Crossed( cadPoint.NormalVec );
						gp_Dir TooV2 = cadPoint.NormalVec.Crossed( nextNormalVec );
						ToolVec = new gp_Dir( ( TooV1.X() + TooV2.X() ) / 2, ( TooV1.Y() + TooV2.Y() ) / 2, ( TooV1.Z() + TooV2.Z() ) / 2 );
						break;
					case ToolVectorType.TowardZ:
						ToolVec = new gp_Dir( 0, 0, 1 );
						break;
				}

				CAMPoint camPoint = new CAMPoint( cadPoint, new gp_Dir() );
				CAMPointList.Add( camPoint );
			}
		}
	}
}
