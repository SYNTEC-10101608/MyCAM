using OCC.TopoDS;
using OCC.TopTools;

namespace DataStructure
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
}
