using OCC.gp;
using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM
{
	public class CADData
	{
		public CADData( TopoDS_Wire contour,
			TopTools_IndexedDataMapOfShapeListOfShape shellMap,
			TopTools_IndexedDataMapOfShapeListOfShape solidMap )
		{
			Contour = contour;
			ShellMap = shellMap;
			SolidMap = solidMap;
		}

		public TopoDS_Wire Contour
		{
			get; private set;
		}

		public TopTools_IndexedDataMapOfShapeListOfShape ShellMap
		{
			get; private set;
		}

		public TopTools_IndexedDataMapOfShapeListOfShape SolidMap
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

		public List<CAMPoint> CAMPointList
		{
			get; set;
		}

		public CADData CADData
		{
			get; private set;
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
				const double dSegmentLength = 2;
				SegmentTool.GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), dSegmentLength, true, false, out List<gp_Pnt> pointList );

				// get tool vector for each point
				foreach( gp_Pnt point in pointList ) {
					gp_Dir normal = VectorTool.GetFaceNormalVec( solidFace, point );
					gp_Dir tangent = VectorTool.GetEdgeTangentVec( TopoDS.ToEdge( edge ), point );
					gp_Dir toolVec = VectorTool.CrossProduct( normal, tangent );
					CAMPointList.Add( new CAMPoint( point, toolVec, normal, tangent ) );
				}
			}
		}
	}

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

		public gp_Dir NormalVec
		{
			get; private set;
		}

		public gp_Dir TangentVec
		{
			get; private set;
		}
	}

	public class CAMEditModel
	{
		public CAMEditModel( TopoDS_Shape model, List<TopoDS_Face> extractedFaceList )
		{
			if( model == null || extractedFaceList == null ) {
				throw new System.ArgumentException( ToString() + "Constructior: Null Ref" );
			}
			if( extractedFaceList.Count == 0 ) {
				throw new System.ArgumentException( ToString() + "Constructior: Empty Collection" );
			}
			m_ModelShape = model;
			m_EtractedFaceList = extractedFaceList;
			BuildCADData();
		}

		public TopoDS_Shape ModelShape
		{
			get
			{
				return m_ModelShape;
			}
		}

		public List<CAMData> CAMDataList
		{
			get
			{
				return m_CAMDataList;
			}
		}

		// fields
		TopoDS_Shape m_ModelShape = null;
		List<TopoDS_Face> m_EtractedFaceList = null;
		List<CAMData> m_CAMDataList = new List<CAMData>();

		bool BuildCADData()
		{
			// sew the faces
			TopoDS_Shape sewResult = ShapeTool.SewShape( m_EtractedFaceList.Cast<TopoDS_Shape>().ToList() );

			// get free boundary wires
			List<TopoDS_Wire> boundaryWireList = GetAllFreeBound( sewResult );
			if( boundaryWireList.Count == 0 ) {
				MessageBox.Show( ToString() + "Error: No boundary wire" );
				return false;
			}

			// map the edges to faces
			TopTools_IndexedDataMapOfShapeListOfShape shellMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( sewResult, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref shellMap );
			TopTools_IndexedDataMapOfShapeListOfShape solidMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( m_ModelShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref solidMap );

			// build CAD CAM data
			m_CAMDataList.Clear();
			foreach( TopoDS_Wire wire in boundaryWireList ) {

				// split the map by edges in wire
				TopTools_IndexedDataMapOfShapeListOfShape oneShellMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				TopTools_IndexedDataMapOfShapeListOfShape oneSolidMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				foreach( TopoDS_Shape edge in wire.elementsAsList ) {
					TopTools_ListOfShape shellFaceList = shellMap.FindFromKey( edge );
					TopTools_ListOfShape solidFaceList = solidMap.FindFromKey( edge );
					if( shellFaceList != null && solidFaceList != null ) {
						oneShellMap.Add( edge, shellFaceList );
						oneSolidMap.Add( edge, solidFaceList );
					}
				}

				CADData cadData = new CADData( wire, oneShellMap, oneSolidMap );
				CAMData camData = new CAMData( cadData );
				m_CAMDataList.Add( camData );
			}
			return true;
		}

		// TODO: the grouping method is tricky, need to be improved
		List<TopoDS_Wire> GetAllFreeBound( TopoDS_Shape sewResult )
		{
			List<TopoDS_Shape> faceGroupList = new List<TopoDS_Shape>();

			// single shell or single face
			if( sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_SHELL
				|| sewResult.shapeType == TopAbs_ShapeEnum.TopAbs_FACE ) {
				faceGroupList.Add( sewResult );
			}

			// some shell and free face exist
			else {
				foreach( TopoDS_Shape shape in sewResult.elementsAsList ) {
					faceGroupList.Add( shape );
				}
			}

			// get free boundary wires
			List<TopoDS_Wire> wireList = new List<TopoDS_Wire>();
			foreach( TopoDS_Shape faceGroup in faceGroupList ) {
				ShapeAnalysis_FreeBounds freeBounds = new ShapeAnalysis_FreeBounds( faceGroup );
				TopExp_Explorer wireExp = new TopExp_Explorer( freeBounds.GetClosedWires(), TopAbs_ShapeEnum.TopAbs_WIRE );
				while( wireExp.More() ) {
					wireList.Add( TopoDS.ToWire( wireExp.Current() ) );
					wireExp.Next();
				}
			}
			return wireList;
		}
	}
}
