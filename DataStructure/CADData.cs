using OCC.TopoDS;
using System;
using System.Collections.Generic;

namespace DataStructure
{
	public class CADData
	{
		public CADData( TopoDS_Wire pathWire, List<Tuple<TopoDS_Edge, TopoDS_Face>> pathDataList )
		{
			PathWire = pathWire;
			PathDataList = pathDataList;
		}

		public TopoDS_Wire PathWire
		{
			get; set;
		}

		public List<Tuple<TopoDS_Edge, TopoDS_Face>> PathDataList
		{
			get; set;
		}
	}
}
