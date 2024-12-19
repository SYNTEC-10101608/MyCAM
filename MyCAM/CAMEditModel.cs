using OCC.ShapeAnalysis;
using OCC.TopAbs;
using OCC.TopExp;
using OCC.TopoDS;
using OCCTool;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM
{
	public class CADData
	{
		public TopoDS_Wire Contour
		{
			get; set;
		}
	}

	public class CAMEditModel
	{
		public CAMEditModel()
		{
		}

		public bool Init( TopoDS_Shape model, List<TopoDS_Face> extractedFaceList )
		{
			if( model == null || extractedFaceList == null ) {
				return false;
			}
			if( extractedFaceList.Count == 0 ) {
				return false;
			}
			m_ModelShape = model;
			m_EtractedFaceList = extractedFaceList;

			if( !BuildCADData() ) {
				return false;
			}
			return true;
		}

		public TopoDS_Shape ModelShape
		{
			get
			{
				return m_ModelShape;
			}
		}

		public List<CADData> CADDataList
		{
			get
			{
				return m_CADDataList;
			}
		}

		// fields
		TopoDS_Shape m_ModelShape = null;
		List<TopoDS_Face> m_EtractedFaceList = null;
		List<CADData> m_CADDataList = new List<CADData>();

		bool BuildCADData()
		{
			// sew the faces
			TopoDS_Shape sewResult = Sew.SewShape( m_EtractedFaceList.Cast<TopoDS_Shape>().ToList() );

			// get free boundary wires
			List<TopoDS_Wire> boundaryWireList = GetAllFreeBound( sewResult );
			if( boundaryWireList.Count == 0 ) {
				MessageBox.Show( ToString() + "Error: No boundary wire" );
				return false;
			}

			// build CAD data
			m_CADDataList.Clear();
			foreach( TopoDS_Wire wire in boundaryWireList ) {
				CADData cadData = new CADData();
				cadData.Contour = wire;
				m_CADDataList.Add( cadData );
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
