using DataStructure;
using OCC.TopoDS;
using System.Collections.Generic;

namespace ProcessEdit
{
	public class ProcessEditModel
	{
		public ProcessEditModel( TopoDS_Shape modelShape, List<IProcessData> processDataList )
		{
			m_ModelShape = modelShape;
			m_ProcessDataList = processDataList;
		}

		public TopoDS_Shape ModelShape
		{
			get
			{
				return m_ModelShape;
			}
		}

		public List<IProcessData> ProcessDataList
		{
			get
			{
				return m_ProcessDataList;
			}
		}

		TopoDS_Shape m_ModelShape;
		List<IProcessData> m_ProcessDataList;
	}
}
