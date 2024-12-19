using OCC.BRepBuilderAPI;
using OCC.gp;
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
			BuildToolVecList();
		}

		public List<CAMPoint> CAMPointList
		{
			get; set;
		}

		public CADData CADData
		{
			get; private set;
		}

		void BuildToolVecList()
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
				for( int i = solidFaceList.Count - 1; i > 0; i-- ) {
					if( solidFaceList[ i ].IsEqual( shellFaceList[ 0 ] ) ) {
						solidFaceList.RemoveAt( i );
						break;
					}
				}
				TopoDS_Face solidFace = TopoDS.ToFace( solidFaceList[ 0 ] );

				// break the edge into segment points by 0.1
				SegmentTool.GetEdgeSegmentPoints( TopoDS.ToEdge( edge ), 0.1, out List<gp_Pnt> pointList );

				// get tool vector for each point
				foreach( gp_Pnt point in pointList ) {
					gp_Dir normal = VectorTool.GetFaceNormalVec( solidFace, point );
					gp_Dir tangent = VectorTool.GetEdgeTangentVec( TopoDS.ToEdge( edge ), point );
					gp_Dir toolVec = VectorTool.CrossProduct( normal, tangent );
					CAMPointList.Add( new CAMPoint( point, toolVec ) );
				}
			}
		}
	}

	public class CAMPoint
	{
		public CAMPoint( gp_Pnt point, gp_Dir toolVec )
		{
			Point = point;
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
			List<TopoDS_Wire> boundaryWireList = GetAllPatternContour( out TopTools_IndexedDataMapOfShapeListOfShape faceMap );
			if( boundaryWireList.Count == 0 ) {
				MessageBox.Show( ToString() + "Error: No boundary wire" );
				return false;
			}

			// map the edges to faces
			TopTools_IndexedDataMapOfShapeListOfShape solidMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( m_ModelShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref solidMap );

			// build CAD CAM data
			m_CAMDataList.Clear();
			foreach( TopoDS_Wire wire in boundaryWireList ) {

				// split the map by edges in wire
				TopTools_IndexedDataMapOfShapeListOfShape oneShellMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				TopTools_IndexedDataMapOfShapeListOfShape oneSolidMap = new TopTools_IndexedDataMapOfShapeListOfShape();
				foreach( TopoDS_Shape edge in wire.elementsAsList ) {
					TopTools_ListOfShape shellFaceList = faceMap.FindFromKey( edge );
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

		List<TopoDS_Wire> GetAllPatternContour( out TopTools_IndexedDataMapOfShapeListOfShape edgeFaceMap )
		{
			TopoDS_Compound faceCompound = ShapeTool.MakeCompound( m_EtractedFaceList.Cast<TopoDS_Shape>().ToList() );

			// get the free boundary edges form extracted faces
			edgeFaceMap = new TopTools_IndexedDataMapOfShapeListOfShape();
			TopExp.MapShapesAndAncestors( faceCompound, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, ref edgeFaceMap );
			List<TopoDS_Edge> freeEdgeList = new List<TopoDS_Edge>();
			TopExp_Explorer edgeExp = new TopExp_Explorer( faceCompound, TopAbs_ShapeEnum.TopAbs_EDGE );
			while( edgeExp.More() ) {
				TopoDS_Shape edge = edgeExp.Current();
				edgeExp.Next();

				// if the edge is only shared by one face, it is a free edge
				if( edgeFaceMap.FindFromKey( edge ) != null && edgeFaceMap.FindFromKey( edge ).Count() == 1 ) {
					freeEdgeList.Add( TopoDS.ToEdge( edge ) );
				}
			}

			// group the free edges into wires
			List<TopoDS_Wire> wireList = new List<TopoDS_Wire>();
			List<List<TopoDS_Edge>> sortedEdgeList = ShapeTool.SortEdgeList( freeEdgeList );
			foreach( List<TopoDS_Edge> edgeList in sortedEdgeList ) {
				BRepBuilderAPI_MakeWire wireMaker = new BRepBuilderAPI_MakeWire();
				foreach( TopoDS_Edge edge in edgeList ) {
					wireMaker.Add( edge );
				}
				if( wireMaker.IsDone() == false ) {
					continue;
				}
				wireList.Add( wireMaker.Wire() );
			}
			return wireList;
		}
	}
}
