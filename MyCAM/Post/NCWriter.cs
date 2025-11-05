using System;
using System.Collections.Generic;
using System.IO;
using MyCAM.Data;
using MyCAM.Helper;
using OCC.gp;

namespace MyCAM.Post
{
	internal class NCWriter
	{
		public NCWriter( List<CAMData> processDataList, MachineData machineData, EntryAndExitData entryAndExitData )
		{
			if( processDataList == null || processDataList.Count == 0 || machineData == null || entryAndExitData == null ) {
				return;
			}
			m_ProcessDataList = processDataList;
			m_MachineData = machineData;
			m_PostSolver = new PostSolver( machineData );
			m_MasterAxisName = ConvertRotaryAxisName( m_MachineData.MasterRotaryAxis );
			m_SlaveAxisName = ConvertRotaryAxisName( m_MachineData.SlaveRotaryAxis );
			m_EntryAndExitData = entryAndExitData;
		}

		List<CAMData> m_ProcessDataList;
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
					PathEndInfo endInfoOfPreviousPath = new PathEndInfo();
					for( int i = 0; i < m_ProcessDataList.Count; i++ ) {

						// solve all post data of the path
						if( !PostHelper.SolvePath( m_PostSolver, m_ProcessDataList[ i ],
							endInfoOfPreviousPath, i == 0, i == m_ProcessDataList.Count - 1, m_EntryAndExitData,
							out PostData postData, out _, out endInfoOfPreviousPath ) ) {
							errorMessage = "後處理運算錯誤，路徑：" + ( i ).ToString();
							return false;
						}
						WriteCutting( postData, i + 1 );
					}

					// write exit
					if( m_ProcessDataList.Count > 0 ) {

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

		public bool ConvertSuccess_New( out string errorMessage )
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
					PathEndInfo endInfoOfPreviousPath = new PathEndInfo();

					// 這條路
					for( int i = 0; i < m_ProcessDataList.Count; i++ ) {

						// 這條路的各段,現在只有Main
						// solve master and slave angle
						bool buildSuccess = BuildCAMSegmentHelper.BuildCAMSegment_New( m_ProcessDataList[ i ], out PathCAMSegmentPage camSegmentPage );
						if( buildSuccess == false ) {
							return false;
						}

						// solve all post data of the path
						if( !PostHelper.SolvePath_New_New( m_PostSolver, m_ProcessDataList[ i ], camSegmentPage,
							endInfoOfPreviousPath, i == 0, i == m_ProcessDataList.Count - 1, m_EntryAndExitData,
							out PathSegmentPostData pathSegmentPostData, out _, out endInfoOfPreviousPath ) ) {
							errorMessage = "後處理運算錯誤，路徑：" + ( i ).ToString();
							return false;
						}
						WriteCutting_New( pathSegmentPostData, i + 1 );
					}

					// write exit
					if( m_ProcessDataList.Count > 0 ) {

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

		void WriteCutting_New( PathSegmentPostData currentPathPostData, int N_Index )
		{
			// the N code
			m_StreamWriter.WriteLine( "// Cutting" + N_Index );
			m_StreamWriter.WriteLine( "N" + N_Index );

			// traverse from previous path to current path
			WriteTraverse_New( currentPathPostData );

			// start cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\" H1;" );

			// write each process path
			WriteOneProcessPath_New( currentPathPostData.MainPathPostPath );

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

		void WriteOneSegmentPath( PostPath segmentPostPath )
		{
			if( segmentPostPath is ArcPostPath arcPostPath ) {
				WriteG02Path( arcPostPath );
				return;
			}
			if( segmentPostPath is LinePostPath linePostPath ) {
				WriteG01Path( linePostPath );
			}
		}

		void WriteG01Path( LinePostPath linePostPath )
		{
			string szX = linePostPath.EndPoint.X.ToString( "F3" );
			string szY = linePostPath.EndPoint.Y.ToString( "F3" );
			string szZ = linePostPath.EndPoint.Z.ToString( "F3" );
			string szM = ( linePostPath.EndPoint.Master * 180 / Math.PI ).ToString( "F3" );
			string szS = ( linePostPath.EndPoint.Slave * 180 / Math.PI ).ToString( "F3" );
			string command = "G01";
			m_StreamWriter.WriteLine( $"{command} X{szX} Y{szY} Z{szZ} {m_MasterAxisName}{szM} {m_SlaveAxisName}{szS};" );
		}

		void WriteG02Path( ArcPostPath arcPostPath )
		{
			string szMidPointX = arcPostPath.MidPoint.X.ToString( "F3" );
			string szMidPointY = arcPostPath.MidPoint.Y.ToString( "F3" );
			string szMidPointZ = arcPostPath.MidPoint.Z.ToString( "F3" );
			string szMidPointM = ( arcPostPath.MidPoint.Master * 180 / Math.PI ).ToString( "F3" );
			string szMidPointS = ( arcPostPath.MidPoint.Slave * 180 / Math.PI ).ToString( "F3" );

			// end point data
			string szEndPointX = arcPostPath.EndPoint.X.ToString( "F3" );
			string szEndPointY = arcPostPath.EndPoint.Y.ToString( "F3" );
			string szEndPointZ = arcPostPath.EndPoint.Z.ToString( "F3" );
			string szEndPointM = ( arcPostPath.EndPoint.Master * 180 / Math.PI ).ToString( "F3" );
			string szEndPointS = ( arcPostPath.EndPoint.Slave * 180 / Math.PI ).ToString( "F3" );
			m_StreamWriter.WriteLine( $"G65 P\"TPCI\" X1={szMidPointX} Y1={szMidPointY} Z1={szMidPointZ} {m_MasterAxisName}1={szMidPointM} {m_SlaveAxisName}1={szMidPointS} X2={szEndPointX} Y2={szEndPointY} Z2={szEndPointZ} {m_MasterAxisName}2={szEndPointM} {m_SlaveAxisName}2={szEndPointS};" );
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

		void WriteTraverse_New( PathSegmentPostData currentPathPostData )
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

		void WriteOneProcessPath_New( List<PostPath> processPathSegmentPostPath )
		{
			PostPoint lastPostPoint = null;
			for( int i = 0; i < processPathSegmentPostPath.Count; i++ ) {

				// 第一條路要從安全距離下降到起點
				if( i == 0 ) {
					WriteOnePoint( new gp_Pnt( processPathSegmentPostPath[ i ].StartPoint.X, processPathSegmentPostPath[ i ].StartPoint.Y, processPathSegmentPostPath[ i ].StartPoint.Z ), processPathSegmentPostPath[ i ].StartPoint.Master, processPathSegmentPostPath[ i ].StartPoint.Slave );
					lastPostPoint = processPathSegmentPostPath[ i ].StartPoint;
				}
				if( IsSamePostPoint( processPathSegmentPostPath[ i ].StartPoint, lastPostPoint ) == false ) {
					return;
				}
				WriteOneSegmentPath( processPathSegmentPostPath[ i ] );
				lastPostPoint = processPathSegmentPostPath[ i ].EndPoint;
			}
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


		bool IsSamePostPoint( PostPoint postPoint1, PostPoint postPoint2 )
		{
			if( postPoint1 == null || postPoint2 == null ) {
				return false;
			}
			if( Math.Abs( postPoint1.X - postPoint2.X ) > 0.01 ) {
				Console.WriteLine( $"P1.X = {postPoint1.X}" );
				Console.WriteLine( $"P2.X = {postPoint2.X}" );
				return false;
			}
			if( Math.Abs( postPoint1.Y - postPoint2.Y ) > 0.01 ) {
				Console.WriteLine( $"P1.Y = {postPoint1.Y}" );
				Console.WriteLine( $"P2.Y = {postPoint2.Y}" );
				return false;
			}
			if( Math.Abs( postPoint1.Z - postPoint2.Z ) > 0.01 ) {
				Console.WriteLine( $"P1.Z = {postPoint1.Z}" );
				Console.WriteLine( $"P2.Z = {postPoint2.Z}" );
				return false;
			}
			if( Math.Abs( postPoint1.Master - postPoint2.Master ) > 0.01 ) {
				Console.WriteLine( $"P1.M = {postPoint1.Master}" );
				Console.WriteLine( $"P2.M = {postPoint2.Master}" );
				return false;
			}
			if( Math.Abs( postPoint1.Slave - postPoint2.Slave ) > 0.01 ) {
				Console.WriteLine( $"P1.S = {postPoint1.Slave}" );
				Console.WriteLine( $"P2.S = {postPoint2.Slave}" );
				return false;
			}
			return true;
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
