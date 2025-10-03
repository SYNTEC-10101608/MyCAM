using MyCAM.Data;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyCAM.Post
{
	internal class NCWriter
	{
		public NCWriter( List<CAMData> processDataList, MachineData machineData )
		{
			if( processDataList == null || machineData == null ) {
				return;
			}
			m_ProcessDataList = processDataList;
			m_MachineData = machineData;
			m_PostSolver = new PostSolver( machineData );
			m_MasterAxisName = ConvertRotaryAxisName( m_MachineData.MasterRotaryAxis );
			m_SlaveAxisName = ConvertRotaryAxisName( m_MachineData.SlaveRotaryAxis );
		}

		List<CAMData> m_ProcessDataList;
		StreamWriter m_StreamWriter;
		PostSolver m_PostSolver;
		MachineData m_MachineData;
		string m_MasterAxisName = string.Empty;
		string m_SlaveAxisName = string.Empty;

		public bool ConvertSuccess( out string errorMessage )
		{
			errorMessage = string.Empty;
			try {
				using( m_StreamWriter = new StreamWriter( "0000.nc" ) ) {
					m_StreamWriter.WriteLine( "G65 P\"FileStart\" X\"Material1\" Y\"1.0\" Q1;" ); // 三點校正
					m_StreamWriter.WriteLine( "G90;" ); // NC init
					m_StreamWriter.WriteLine( "G17;" );
					m_StreamWriter.WriteLine( "G53 Z0.;" ); // 機械軸復位
					m_StreamWriter.WriteLine( "G53 " + m_MasterAxisName + "0. " + m_SlaveAxisName + "0." );
					m_StreamWriter.WriteLine( "G43.4 P1;" ); // G43.4 新動程
					for( int i = 0; i < m_ProcessDataList.Count; i++ ) {
						WriteCutting( m_ProcessDataList[ i ], i + 1 );
					}
					m_StreamWriter.WriteLine( "G65 P\"FileEnd\";" );
					m_StreamWriter.WriteLine( "M30;" ); // 程式結束
				}
				return true;
			}
			catch( IOException ioEx ) {
				errorMessage = ioEx.Message;
				return false;
			}
			catch( Exception ex ) {
				errorMessage = ex.Message;
				return false;
			}
		}

		void WriteCutting( CAMData cuttingProcessData, int index )
		{
			// solve master and slave angle
			if( !PostHelper.SolvePath( m_PostSolver, cuttingProcessData, out _, out PostData currentPathPostData ) ) {
				return;
			}

			// tool down
			WriteOnePoint( new gp_Pnt( currentPathPostData.CutDownPostPoint.X, currentPathPostData.CutDownPostPoint.Y, currentPathPostData.CutDownPostPoint.Z ), currentPathPostData.CutDownPostPoint.Master, currentPathPostData.CutDownPostPoint.Slave, true );
			WriteOnePoint( new gp_Pnt( currentPathPostData.FollowSafePostPoint.X, currentPathPostData.FollowSafePostPoint.Y, currentPathPostData.FollowSafePostPoint.Z ), currentPathPostData.FollowSafePostPoint.Master, currentPathPostData.FollowSafePostPoint.Slave, true );

			// start cutting
			m_StreamWriter.WriteLine( "// Cutting" + index );
			m_StreamWriter.WriteLine( "N" + index );
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\" H1;" );

			// write each process path
			WriteOneProcessPath( currentPathPostData.LeadInPostPointList );
			WriteOneProcessPath( currentPathPostData.MainPathPostPointList );
			WriteOneProcessPath( currentPathPostData.OverCutPostPointList );
			WriteOneProcessPath( currentPathPostData.LeadOutPostPointList );

			// end cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_OFF\";" );

			// tool up
			WriteOnePoint( new gp_Pnt( currentPathPostData.LiftUpPostPoint.X, currentPathPostData.LiftUpPostPoint.Y, currentPathPostData.LiftUpPostPoint.Z ), currentPathPostData.LiftUpPostPoint.Master, currentPathPostData.LiftUpPostPoint.Slave, true );
			return;
		}

		void WriteOnePoint( gp_Pnt point, double dM, double dS, bool G00 = false )
		{
			string szX = point.X().ToString( "F3" );
			string szY = point.Y().ToString( "F3" );
			string szZ = point.Z().ToString( "F3" );
			string szM = ( dM * 180 / Math.PI ).ToString( "F3" );
			string szS = ( dS * 180 / Math.PI ).ToString( "F3" );
			string command = G00 ? "G00" : "G01";
			m_StreamWriter.WriteLine( $"{command} X{szX} Y{szY} Z{szZ} {m_MasterAxisName}{szM} {m_SlaveAxisName}{szS};" );
		}

		void WriteOneProcessPath( List<PostPoint> postList, bool G00 = false )
		{
			for( int i = 0; i < postList.Count; i++ ) {
				var pos = postList[ i ];
				WriteOnePoint( new gp_Pnt( pos.X, pos.Y, pos.Z ), pos.Master, pos.Slave, G00 );
			}
		}

		string ConvertRotaryAxisName( RotaryAxis axis )
		{
			switch( axis ) {
				case RotaryAxis.X:
					return "A";
				case RotaryAxis.Y:
					return "B";
				case RotaryAxis.Z:
					return "C";
				default:
					return "";
			}
		}
	}
}
