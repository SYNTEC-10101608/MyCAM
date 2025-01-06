using DataStructure;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.IO;

namespace NCExport
{
	public class NCWriter
	{
		public NCWriter( List<IProcessData> processDataList, gp_Trsf partTrsf )
		{
			m_ProcessDataList = processDataList;
			m_PartTrsf = partTrsf;
		}

		List<IProcessData> m_ProcessDataList;
		gp_Trsf m_PartTrsf;
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
				string szX = camPoint.Point.X().ToString( "F3" );
				string szY = camPoint.Point.Y().ToString( "F3" );
				string szZ = camPoint.Point.Z().ToString( "F3" );
				string szA = dA_MCS.ToString( "F3" );
				string szC = dC_MCS.ToString( "F3" );
				m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} A{szA} C{szC}" );
			}
		}

		// TODO: currently for spindle-spindle C-A Type only
		void ConvertIJKToABC( gp_Dir ToolVec_G54, out double dA_MCS_deg, out double dC_MCS_deg )
		{
			// rotate the tool vector to the part coordinate system
			gp_Dir ToolVec_MCS = ToolVec_G54.Transformed( m_PartTrsf );

			// calculate the A and C angle
			dA_MCS_deg = Math.Atan2( ToolVec_MCS.Y(), ToolVec_MCS.X() ) * 180 / Math.PI;
			dC_MCS_deg = Math.Asin( ToolVec_MCS.Z() ) * 180 / Math.PI;
		}

		void WriteTraverse( TraverseProcessData traverseProcessData )
		{
			m_StreamWriter.WriteLine( "// Traverse" );
			string szX = traverseProcessData.Point_MCS.X().ToString( "F3" );
			string szY = traverseProcessData.Point_MCS.Y().ToString( "F3" );
			string szZ = traverseProcessData.Point_MCS.Z().ToString( "F3" );

			// TODO: add tool vector
			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} A0 C0" );
		}
	}
}
