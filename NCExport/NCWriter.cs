using DataStructure;
using OCCTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NCExport
{
	public class NCWriter
	{
		public NCWriter( List<IProcessData> processDataList )
		{
			m_ProcessDataList = processDataList;
		}

		List<IProcessData> m_ProcessDataList;
		StreamWriter m_StreamWriter;

		public void Convert()
		{
			m_StreamWriter = new StreamWriter( "0000.nc" );
			m_StreamWriter.WriteLine( "%@MACRO" );
			m_StreamWriter.WriteLine( "G43.4;" );
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
			m_StreamWriter.WriteLine( "M30;" );
			m_StreamWriter.Close();
		}

		void WriteCutting( CuttingProcessData cuttingProcessData )
		{
			List<Tuple<double, double>> rotaryAxisPosList = PostTool.ConvertIJKToABC( cuttingProcessData.CAMData.CAMPointList.Select( camPoint => camPoint.ToolVec ).ToList() );

			// write each cam data point
			m_StreamWriter.WriteLine( "// Cutting" );
			int i = 0;
			foreach( var pos in rotaryAxisPosList ) {
				double dM_MCS_deg = pos.Item1 * 180 / Math.PI;
				double dS_MCS_deg = pos.Item2 * 180 / Math.PI;
				string szX = cuttingProcessData.CAMData.CAMPointList[ i ].CADPoint.Point.X().ToString( "F3" );
				string szY = cuttingProcessData.CAMData.CAMPointList[ i ].CADPoint.Point.Y().ToString( "F3" );
				string szZ = cuttingProcessData.CAMData.CAMPointList[ i ].CADPoint.Point.Z().ToString( "F3" );
				string szB = dS_MCS_deg.ToString( "F3" );
				string szC = dM_MCS_deg.ToString( "F3" );
				m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} B{szB} C{szC};" );
				i++;
			}
		}

		void WriteTraverse( TraverseProcessData traverseProcessData )
		{
			m_StreamWriter.WriteLine( "// Traverse" );
			string szX = traverseProcessData.Point.X().ToString( "F3" );
			string szY = traverseProcessData.Point.Y().ToString( "F3" );
			string szZ = traverseProcessData.Point.Z().ToString( "F3" );
			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} A0 C0" );
		}
	}
}
