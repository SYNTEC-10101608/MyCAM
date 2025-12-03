using MyCAM.CacheInfo;
using MyCAM.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyCAM.Post
{
	internal class NCWriter
	{
		public NCWriter( DataManager dataManager )
		{
			// fix: check null argument
			if( dataManager == null || dataManager.GetContourCacheInfoList() == null || dataManager.MachineData == null || dataManager.EntryAndExitData == null ) {
				throw new ArgumentNullException( "NCWriter constructor argument is null." );
			}
			m_ProcessCacheInfoList = dataManager.GetContourCacheInfoList();

			foreach( var szID in dataManager.PathIDList ) {
				if( !dataManager.ObjectMap.ContainsKey( szID ) ) {
					throw new ArgumentException( "NCWriter constructor argument contains invalid path ID." );
				}
				m_CraftDataList.Add( ( dataManager.ObjectMap[ szID ] as PathObject ).CraftData );
			}
			m_MachineData = dataManager.MachineData;
			m_PostSolver = new PostSolver( dataManager.MachineData );
			m_MasterAxisName = ConvertRotaryAxisName( m_MachineData.MasterRotaryAxis );
			m_SlaveAxisName = ConvertRotaryAxisName( m_MachineData.SlaveRotaryAxis );
			m_EntryAndExitData = dataManager.EntryAndExitData;
		}

		List<ContourCacheInfo> m_ProcessCacheInfoList;
		List<CraftData> m_CraftDataList = new List<CraftData>();
		StreamWriter m_StreamWriter;
		PostSolver m_PostSolver;
		MachineData m_MachineData;
		string m_MasterAxisName = string.Empty;
		string m_SlaveAxisName = string.Empty;
		EntryAndExitData m_EntryAndExitData;

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

					// to keep last point of previous path
					PathEndInfo endInfoOfPreviousPath = null;
					for( int i = 0; i < m_ProcessCacheInfoList.Count; i++ ) {

						// solve all post data of the path
						if( !PostHelper.SolvePath( m_PostSolver, null,
							endInfoOfPreviousPath, m_EntryAndExitData,
							out PostData postData, out _, out endInfoOfPreviousPath ) ) {
							errorMessage = "後處理運算錯誤，路徑：" + ( i ).ToString();
							return false;
						}
						WriteCutting( postData, i + 1 );
					}

					// write exit
					if( m_ProcessCacheInfoList.Count > 0 ) {

						// calculate exit point
						PostHelper.CalculateExit( endInfoOfPreviousPath, m_EntryAndExitData, out PostPoint exitPoint, out _ );
						m_StreamWriter.WriteLine( "// Exit" );
						WriteOneLinearTraverse( exitPoint );
					}
					m_StreamWriter.WriteLine( "G65 P\"FileEnd\";" );
					m_StreamWriter.WriteLine( "M30;" ); // 程式結束
				}
				return true;
			}
			catch( Exception ex ) {
				errorMessage = ex.Message;
				return false;
			}
		}

		void WriteCutting( PostData currentPathPostData, int N_Index )
		{
			// the N code
			m_StreamWriter.WriteLine( "// Cutting" + N_Index );
			m_StreamWriter.WriteLine( "N" + N_Index );

			// traverse from previous path to current path
			WriteTraverse( currentPathPostData );

			// start cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\" H1;" );

			// write each process path
			WriteOneProcessPath( currentPathPostData.LeadInPostPointList );
			WriteOneProcessPath( currentPathPostData.MainPathPostPointList );
			WriteOneProcessPath( currentPathPostData.OverCutPostPointList );
			WriteOneProcessPath( currentPathPostData.LeadOutPostPointList );

			// end cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_OFF\";" );
			return;
		}

		void WriteOnePoint( PostPoint postPoint )
		{
			if( postPoint == null ) {
				return;
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI );
			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand};" );
		}

		void WriteOneLinearTraverse( PostPoint postPoint, double followSafeDistance = 0 )
		{
			if( postPoint == null ) {
				return;
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI );
			string szFollow = followSafeDistance == 0 ? string.Empty : FOLLOW_SAFE_DISTANCE_COMMAND + followSafeDistance.ToString( "F3" );
			m_StreamWriter.WriteLine( $"G00 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand} {szFollow};" );
		}

		void WriteOneFrogLeap( PostPoint midPoint, PostPoint endPoint, double followSafeDistance = 0 )
		{
			if( midPoint == null || endPoint == null ) {
				return;
			}

			// mid point
			string szX1 = midPoint.X.ToString( "F3" );
			string szY1 = midPoint.Y.ToString( "F3" );
			string szZ1 = midPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand1 = GetRotaryAxisCommand( midPoint.Master * 180 / Math.PI, midPoint.Slave * 180 / Math.PI, "1=" );

			// end point
			string szX2 = endPoint.X.ToString( "F3" );
			string szY2 = endPoint.Y.ToString( "F3" );
			string szZ2 = endPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand2 = GetRotaryAxisCommand( endPoint.Master * 180 / Math.PI, endPoint.Slave * 180 / Math.PI, "2=" );
			string szFollow = followSafeDistance == 0 ? string.Empty : FOLLOW_SAFE_DISTANCE_COMMAND + followSafeDistance.ToString( "F3" );
			m_StreamWriter.WriteLine( $"G65 P\"FROG_LEAP\" X1={szX1} Y1={szY1} Z1={szZ1} {szRotaryAxisCommand1} " +
				$"X2={szX2} Y2={szY2} Z2={szZ2} {szRotaryAxisCommand2} {szFollow};" );
		}

		string GetRotaryAxisCommand( double master_deg, double slave_deg, string szAxisCommandFix = "" )
		{
			string szM = m_MasterAxisName + szAxisCommandFix + master_deg.ToString( "F3" );
			string szS = m_SlaveAxisName + szAxisCommandFix + slave_deg.ToString( "F3" );
			if( m_MachineData.MasterRotaryAxis < m_MachineData.SlaveRotaryAxis ) {
				return szM + " " + szS;
			}
			else {
				return szS + " " + szM;
			}
		}

		void WriteOneProcessPath( List<PostPoint> postPointList )
		{
			if( postPointList == null || postPointList.Count == 0 ) {
				return;
			}
			for( int i = 0; i < postPointList.Count; i++ ) {
				var onePostPoint = postPointList[ i ];
				WriteOnePoint( onePostPoint );
			}
		}

		void WriteTraverse( PostData currentPathPostData )
		{
			// lift up
			if( currentPathPostData.LiftUpPostPoint != null ) {
				WriteOneLinearTraverse( currentPathPostData.LiftUpPostPoint );
			}

			// frog leap with cut down
			if( currentPathPostData.FrogLeapMidPostPoint != null && currentPathPostData.CutDownPostPoint != null ) {
				WriteOneFrogLeap( currentPathPostData.FrogLeapMidPostPoint, currentPathPostData.CutDownPostPoint );

				// cut down
				WriteOneLinearTraverse( currentPathPostData.ProcessStartPoint, currentPathPostData.FollowSafeDistance );
			}

			// form leap without cut down
			else if( currentPathPostData.FrogLeapMidPostPoint != null && currentPathPostData.CutDownPostPoint == null ) {
				WriteOneFrogLeap( currentPathPostData.FrogLeapMidPostPoint, currentPathPostData.ProcessStartPoint, currentPathPostData.FollowSafeDistance );
			}

			// no frog leap
			else if( currentPathPostData.FrogLeapMidPostPoint == null && currentPathPostData.CutDownPostPoint != null ) {
				WriteOneLinearTraverse( currentPathPostData.CutDownPostPoint );

				// cut down
				WriteOneLinearTraverse( currentPathPostData.ProcessStartPoint, currentPathPostData.FollowSafeDistance );
			}

			// no frog leap and no cut down
			else {
				WriteOneLinearTraverse( currentPathPostData.ProcessStartPoint, currentPathPostData.FollowSafeDistance );
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

		const string FOLLOW_SAFE_DISTANCE_COMMAND = "S";
	}
}
