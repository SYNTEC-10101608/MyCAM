using DataStructure;
using OCC.TopoDS;
using System.Collections.Generic;

namespace ProcessEdit
{
	public class ProcessEditModel
	{
		public ProcessEditModel( TopoDS_Shape modelShape, List<CuttingProcessData> processDataList )
		{
			m_ModelShape = modelShape;
			m_ProcessDataList = processDataList;
		}

		TopoDS_Shape m_ModelShape;
		List<CuttingProcessData> m_ProcessDataList;
	}
}
