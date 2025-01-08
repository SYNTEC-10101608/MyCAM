using DataStructure;
using OCC.TopoDS;
using System.Collections.Generic;

namespace ProcessEdit
{
	public class ProcessEditModel
	{
		public ProcessEditModel( TopoDS_Shape partShape, List<IProcessData> processDataList )
		{
			m_PartShape = partShape;
			m_ProcessDataList = processDataList;
		}

		public TopoDS_Shape PartShape
		{
			get
			{
				return m_PartShape;
			}
		}

		public List<IProcessData> ProcessDataList
		{
			get
			{
				return m_ProcessDataList;
			}
		}

		TopoDS_Shape m_PartShape;
		List<IProcessData> m_ProcessDataList;
	}
}
