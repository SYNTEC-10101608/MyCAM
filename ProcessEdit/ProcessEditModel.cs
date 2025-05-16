using DataStructure;
using OCC.gp;
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
			//SortProcess();
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

		public void SortProcess()
		{
			List<IProcessData> sortResult = new List<IProcessData>();

			// the initial point is assume to be <0,0,0>
			gp_Pnt startPoint = new gp_Pnt( 0, 0, 0 );

			// sort the process using shortest distance algorithm greedily
			while( m_ProcessDataList.Count > 0 ) {
				double minDistance = double.MaxValue;
				int minIndex = -1;
				for( int i = 0; i < m_ProcessDataList.Count; i++ ) {
					if( m_ProcessDataList[ i ].ProcessType != EProcessType.ProcessType_Cutting ) {
						continue;
					}
					CuttingProcessData processData = (CuttingProcessData)m_ProcessDataList[ i ];
					double distance = startPoint.Distance( processData.CAMData.CAMPointList[ 0 ].CADPoint.Point );
					if( distance < minDistance ) {
						minDistance = distance;
						minIndex = i;
					}
				}
				if( minIndex >= 0 ) {
					// update the start point
					CuttingProcessData processData = (CuttingProcessData)m_ProcessDataList[ minIndex ];
					startPoint = processData.CAMData.CAMPointList[ 0 ].CADPoint.Point;
					sortResult.Add( processData );
					m_ProcessDataList.RemoveAt( minIndex );
				}
			}
			m_ProcessDataList = sortResult;
		}

		TopoDS_Shape m_PartShape;
		List<IProcessData> m_ProcessDataList;
	}
}
