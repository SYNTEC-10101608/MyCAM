using OCC.gp;
using OCC.TopoDS;
using System.Collections.Generic;

namespace DataStructure
{
	public class PathEdge5D
	{
		public PathEdge5D( TopoDS_Edge pathEdge, TopoDS_Face componentFace )
		{
			PathEdge = pathEdge;
			ComponentFace = componentFace;
		}

		public TopoDS_Edge PathEdge
		{
			get; private set;
		}

		public TopoDS_Face ComponentFace
		{
			get; private set;
		}
	}

	public class CADData
	{
		public CADData( TopoDS_Wire pathWire, List<PathEdge5D> pathDataList, gp_Trsf innerTrsf )
		{
			PathWire = pathWire;
			PathDataList = pathDataList;
			InnerTrsf = innerTrsf;
		}

		public TopoDS_Wire PathWire
		{
			get; private set;
		}

		public List<PathEdge5D> PathDataList
		{
			get; private set;
		}

		public gp_Trsf InnerTrsf
		{
			get; private set;
		}
	}
}
