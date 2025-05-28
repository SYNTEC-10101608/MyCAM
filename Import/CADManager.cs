using OCC.TopAbs;
using OCC.TopoDS;
using System;
using System.Collections.Generic;

namespace Import
{
	internal class CADModel
	{
		public CADModel( string szUID, string szName, TopoDS_Shape shapeData )
		{
			UID = szUID;
			Name = szName;
			ShapeData = shapeData;
		}

		public string UID
		{
			get; set;
		}

		public string Name
		{
			get; set;
		}

		public TopoDS_Shape ShapeData
		{
			get; set;
		}
	}

	internal class CADManager
	{
		Action<string, TopoDS_Shape> AddCADModelDone;

		public void AddCADModel( TopoDS_Shape newShape )
		{
			if( newShape == null || newShape.IsNull() ) {
				return;
			}
			int nID = 0;
			string szType = string.Empty;
			switch( newShape.ShapeType() ) {
				case TopAbs_ShapeEnum.TopAbs_SOLID:
					nID = ++m_SolidID;
					szType = "Solid";
					break;
				case TopAbs_ShapeEnum.TopAbs_SHELL:
					nID = ++m_ShellID;
					szType = "Shell";
					break;
				case TopAbs_ShapeEnum.TopAbs_FACE:
					nID = ++m_FaceID;
					szType = "Face";
					break;
				case TopAbs_ShapeEnum.TopAbs_WIRE:
					nID = ++m_WireID;
					szType = "Wire";
					break;
				case TopAbs_ShapeEnum.TopAbs_EDGE:
					nID = ++m_EdgeID;
					szType = "Edge";
					break;
				case TopAbs_ShapeEnum.TopAbs_VERTEX:
					nID = ++m_VertexID;
					szType = "Vertex";
					break;
				default:
					return; // not a valid shape type
			}
			string szUID = szType + nID.ToString();
			string szName = szUID;
			CADModel model = new CADModel( szUID, szName, newShape );
			m_CADModelContainer.Add( model );
			m_CADModelMap.Add( szUID, model );
			AddCADModelDone?.Invoke( szUID, newShape );
		}

		List<CADModel> m_CADModelContainer = new List<CADModel>();
		Dictionary<string, CADModel> m_CADModelMap = new Dictionary<string, CADModel>();

		int m_SolidID = 0;
		int m_ShellID = 0;
		int m_FaceID = 0;
		int m_WireID = 0;
		int m_EdgeID = 0;
		int m_VertexID = 0;
	}
}
