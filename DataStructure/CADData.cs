using OCC.TopoDS;
using System.Collections.Generic;

namespace DataStructure
{
	public class PathEdge5D
	{
		public TopoDS_Edge PathEdge
		{
			get; set;
		}

		public TopoDS_Face ComponentFace
		{
			get; set;
		}
	}

	public class CADData
	{
		public CADData( TopoDS_Wire pathWire, List<PathEdge5D> pathDataList )
		{
			PathWire = pathWire;
			PathDataList = pathDataList;
		}

		public TopoDS_Wire PathWire
		{
			get; set;
		}

		public List<PathEdge5D> PathDataList
		{
			get; set;
		}
	}
}
