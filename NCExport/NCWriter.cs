using DataStructure;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.IO;

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
			m_StreamWriter.WriteLine( "G43.4" );
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
			m_StreamWriter.WriteLine( "M30" );
			m_StreamWriter.Close();
		}

		void WriteCutting( CuttingProcessData cuttingProcessData )
		{
			// write each cam data point
			m_StreamWriter.WriteLine( "// Cutting" );
			foreach( CAMPoint camPoint in cuttingProcessData.CAMData.CAMPointList ) {
				ConvertIJKToABC( camPoint.ToolVec, out double dA_MCS, out double dC_MCS );
				string szX = camPoint.CADPoint.Point.X().ToString( "F3" );
				string szY = camPoint.CADPoint.Point.Y().ToString( "F3" );
				string szZ = camPoint.CADPoint.Point.Z().ToString( "F3" );
				string szA = dA_MCS.ToString( "F3" );
				string szC = dC_MCS.ToString( "F3" );

				// for G43.5 test
				//string szI = camPoint.ToolVec.X().ToString( "F3" );
				//string szJ = camPoint.ToolVec.Y().ToString( "F3" );
				//string szK = camPoint.ToolVec.Z().ToString( "F3" );
				//m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} I{szI} J{szJ} K{szK}" );
				m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} A{szA} C{szC}" );
			}
		}

		void ConvertIJKToABC( gp_Dir ToolVec_G54, out double dA_MCS_deg, out double dC_MCS_deg )
		{
			// calculate the A and C angle
			if( Math.Abs( ToolVec_G54.X() ) < 1e-6 && Math.Abs( ToolVec_G54.Y() ) < 1e-6 ) {
				dC_MCS_deg = 0;
			}
			else {
				dC_MCS_deg = ( ( Math.Atan2( ToolVec_G54.X(), -ToolVec_G54.Y() ) * 180 / Math.PI ) + 360 ) % 360;
			}
			dA_MCS_deg = Math.Acos( ToolVec_G54.Z() ) * 180 / Math.PI;
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
