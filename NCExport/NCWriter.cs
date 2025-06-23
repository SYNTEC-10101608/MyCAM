using DataStructure;
using OCC.gp;
using OCCTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
			m_StreamWriter.WriteLine( "G65 P\"FileStart\" X\"Material1\" Y\"1.0\" Q1;" ); // 三點校正
			m_StreamWriter.WriteLine( "G90;" ); // NC init
			m_StreamWriter.WriteLine( "G17;" );
			m_StreamWriter.WriteLine( "G53 Z0.;" ); // 機械軸復位
			m_StreamWriter.WriteLine( "G53 B0. C0." );
			m_StreamWriter.WriteLine( "G43.4 P1;" ); // G43.4 新動程
			for( int i = 0; i < m_ProcessDataList.Count; i++ ) {
				IProcessData processData = m_ProcessDataList[ i ];
				switch( processData.ProcessType ) {
					case EProcessType.ProcessType_Cutting:
						CuttingProcessData cuttingProcessData = processData as CuttingProcessData;
						WriteCutting( cuttingProcessData, i + 1 );
						break;
					case EProcessType.ProcessType_Traverse:
						TraverseProcessData traverseProcessData = processData as TraverseProcessData;
						WriteTraverse( traverseProcessData );
						break;
					default:
						break;
				}
			}
			m_StreamWriter.WriteLine( "M30;" ); // 程式結束
			m_StreamWriter.Close();
		}

		void WriteCutting( CuttingProcessData cuttingProcessData, int index )
		{
			// get rotary axis
			List<Tuple<double, double>> rotaryAxisPosList = PostTool.ConvertIJKToABC( cuttingProcessData.CAMData.CAMPointList.Select( camPoint => camPoint.ToolVec ).ToList() );

			// compute approach points
			gp_Vec toolVec = new gp_Vec( cuttingProcessData.CAMData.CAMPointList[ 0 ].ToolVec.XYZ() );
			gp_Pnt startPoint = cuttingProcessData.CAMData.CAMPointList[ 0 ].CADPoint.Point;
			gp_Pnt approachPtG00 = startPoint.Translated( toolVec.Scaled( G00_APPROACH_LENGTH ) );
			gp_Pnt approachPtG01 = startPoint.Translated( toolVec.Scaled( G01_APPROACH_LENGTH ) );
			gp_Pnt safePlanePt = new gp_Pnt( approachPtG00.X(), approachPtG00.Y(), SAFE_PLANE_Z );

			// write approach point (下刀)
			WriteOnePoint( safePlanePt, rotaryAxisPosList[ 0 ].Item1, rotaryAxisPosList[ 0 ].Item2, true );
			WriteOnePoint( approachPtG00, rotaryAxisPosList[ 0 ].Item1, rotaryAxisPosList[ 0 ].Item2, true );
			WriteOnePoint( approachPtG01, rotaryAxisPosList[ 0 ].Item1, rotaryAxisPosList[ 0 ].Item2, true );

			// write each cam data point
			m_StreamWriter.WriteLine( "// Cutting" + index.ToString() );
			m_StreamWriter.WriteLine( "N" + index.ToString() ); // N 碼
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\" H1;" ); // 開隨動
			for( int i = 0; i < cuttingProcessData.CAMData.CAMPointList.Count; i++ ) {
				var pos = rotaryAxisPosList[ i ];
				//double dM_MCS_deg = pos.Item1 * 180 / Math.PI;
				//double dS_MCS_deg = pos.Item2 * 180 / Math.PI;
				WriteOnePoint( cuttingProcessData.CAMData.CAMPointList[ i ].CADPoint.Point, pos.Item1, pos.Item2 );
			}
			m_StreamWriter.WriteLine( "G65 P\"LASER_OFF\";" ); // 關隨動

			// write approach point (抬刀，不需要抬到 G01 位置)
			WriteOnePoint( approachPtG00, rotaryAxisPosList.Last().Item1, rotaryAxisPosList.Last().Item2, true );
			WriteOnePoint( safePlanePt, rotaryAxisPosList.Last().Item1, rotaryAxisPosList.Last().Item2, true );
		}

		void WriteOnePoint( gp_Pnt point, double dM, double dS, bool G00 = false )
		{
			string szX = point.X().ToString( "F3" );
			string szY = point.Y().ToString( "F3" );
			string szZ = point.Z().ToString( "F3" );
			string szB = ( dS * 180 / Math.PI ).ToString( "F3" );
			string szC = ( dM * 180 / Math.PI ).ToString( "F3" );
			string command = G00 ? "G00" : "G01";
			m_StreamWriter.WriteLine( $"{command} X{szX} Y{szY} Z{szZ} B{szB} C{szC};" );
		}

		void WriteTraverse( TraverseProcessData traverseProcessData )
		{
			m_StreamWriter.WriteLine( "// Traverse" );
			string szX = traverseProcessData.Point.X().ToString( "F3" );
			string szY = traverseProcessData.Point.Y().ToString( "F3" );
			string szZ = traverseProcessData.Point.Z().ToString( "F3" );
			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} A0 C0" );
		}

		const int SAFE_PLANE_Z = 300;
		const int G00_APPROACH_LENGTH = 40;
		const int G01_APPROACH_LENGTH = 3;
	}
}
