using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyCAM.Post
{
	internal class NCWriter
	{
		public NCWriter( List<CAMData> processDataList, PostSolver postSolver )
		{
			m_ProcessDataList = processDataList;
			m_PostSolver = postSolver;
		}

		List<CAMData> m_ProcessDataList;
		StreamWriter m_StreamWriter;
		PostSolver m_PostSolver;

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
				WriteCutting( m_ProcessDataList[ i ], i + 1 );
			}
			m_StreamWriter.WriteLine( "M30;" ); // 程式結束
			m_StreamWriter.Close();
		}

		void WriteCutting( CAMData cuttingProcessData, int index )
		{
			// get rotary axis
			PostHelper.SolvePath( m_PostSolver, cuttingProcessData, out List<PostData> postDataList, out _ );

			// compute approach points
			gp_Vec toolVec = new gp_Vec( cuttingProcessData.CAMPointList[ 0 ].ToolVec.XYZ() );
			gp_Pnt startPoint = cuttingProcessData.CAMPointList[ 0 ].CADPoint.Point;
			gp_Pnt approachPtG00 = startPoint.Translated( toolVec.Scaled( G00_APPROACH_LENGTH ) );
			gp_Pnt approachPtG01 = startPoint.Translated( toolVec.Scaled( G01_APPROACH_LENGTH ) );
			gp_Pnt safePlanePt = new gp_Pnt( approachPtG00.X(), approachPtG00.Y(), SAFE_PLANE_Z );

			// write approach point (下刀)
			WriteOnePoint( safePlanePt, postDataList[ 0 ].Master, postDataList[ 0 ].Slave, true );
			WriteOnePoint( approachPtG00, postDataList[ 0 ].Master, postDataList[ 0 ].Slave, true );
			WriteOnePoint( approachPtG01, postDataList[ 0 ].Master, postDataList[ 0 ].Slave, true );

			// write each cam data point
			m_StreamWriter.WriteLine( "// Cutting" + index.ToString() );
			m_StreamWriter.WriteLine( "N" + index.ToString() ); // N 碼
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\" H1;" ); // 開隨動
			for( int i = 0; i < cuttingProcessData.CAMPointList.Count; i++ ) {
				var pos = postDataList[ i ];
				WriteOnePoint( cuttingProcessData.CAMPointList[ i ].CADPoint.Point, pos.Master, pos.Slave );
			}
			m_StreamWriter.WriteLine( "G65 P\"LASER_OFF\";" ); // 關隨動

			// write approach point (抬刀，不需要抬到 G01 位置)
			WriteOnePoint( approachPtG00, postDataList.Last().Master, postDataList.Last().Slave, true );
			WriteOnePoint( safePlanePt, postDataList.Last().Master, postDataList.Last().Slave, true );
		}

		void WriteOnePoint( gp_Pnt point, double dM, double dS, bool G00 = false )
		{
			string szX = point.X().ToString( "F3" );
			string szY = point.Y().ToString( "F3" );
			string szZ = point.Z().ToString( "F3" );
			string szB = ( dM * 180 / Math.PI ).ToString( "F3" );
			string szC = ( dS * 180 / Math.PI ).ToString( "F3" );
			string command = G00 ? "G00" : "G01";
			m_StreamWriter.WriteLine( $"{command} X{szX} Y{szY} Z{szZ} B{szB} C{szC};" );
		}

		const int SAFE_PLANE_Z = 300;
		const int G00_APPROACH_LENGTH = 40;
		const int G01_APPROACH_LENGTH = 3;
	}
}
