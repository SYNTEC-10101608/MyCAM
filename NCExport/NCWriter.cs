using DataStructure;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCExport
{
	public class NCWriter
	{
		public NCWriter( List<IProcessData> processDataList )
		{
			m_ProcessDataList = processDataList;
			Convert();
		}

		List<IProcessData> m_ProcessDataList;
		StreamWriter m_StreamWriter;

		void Convert()
		{
			m_StreamWriter = new StreamWriter( "TEST000.nc" );
			foreach( IProcessData processData in m_ProcessDataList ) {
				switch( processData.ProcessType ) {
					case EProcessType.ProcessType_Cutting:
						CuttingProcessData cuttingProcessData = processData as CuttingProcessData;
						WriteCutting( cuttingProcessData );
						break;
					case EProcessType.ProcessType_Traverse:
						TraverseProcessData traverseProcessData = processData as TraverseProcessData;
						WriteTraverse( traverseProcessData );
						break;
					default:
						break;
				}
			}
			m_StreamWriter.Close();
		}

		void WriteCutting( CuttingProcessData cuttingProcessData )
		{
			// write each cam data point
			foreach( CAMPoint camPoint in cuttingProcessData.CAMData.CAMPointList ) {
				m_StreamWriter.WriteLine( "G01 X{0} Y{1} Z{2}", camPoint.Point.X(), camPoint.Point.Y(), camPoint.Point.Z() );
			}
		}

		void ConvertIJKToABC( gp_Dir vec, out double dA, out double dB, out double dC )
		{
			dA = 0;
			dB = 0;
			dC = 0;
		}

		void WriteTraverse( TraverseProcessData traverseProcessData )
		{
		}
	}
}
