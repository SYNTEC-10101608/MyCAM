using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyCAM.Data;
using OCC.gp;

namespace MyCAM.Post
{
	internal class NCWriter
	{
		public NCWriter( List<CAMData> processDataList, PostSolver postSolver )
		{
			if( processDataList == null || postSolver == null ) {
				return;
			}
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
			// solve master and slave angle
			if( !PostHelper.SolvePath( m_PostSolver, cuttingProcessData, out _, out PostData currentPathPostData ) ) {
				return;
			}
			PostPoint firstPointPostData, lastPointPostData = new PostPoint();

			// get first point on path
			bool bWithLeadIn = cuttingProcessData.LeadInCAMPointList.Count > 0 && cuttingProcessData.LeadLineParam.LeadIn.Type != LeadType.LeadLineType.None;
			CAMPoint startCamPoint = bWithLeadIn ? cuttingProcessData.LeadInCAMPointList[ 0 ] : cuttingProcessData.CAMPointList[ 0 ];
			firstPointPostData = bWithLeadIn ? currentPathPostData.LeadInPostPointList[ 0 ] : currentPathPostData.MainPathPostPointList[ 0 ];

			// approaching
			gp_Vec toolVec = new gp_Vec( startCamPoint.ToolVec.XYZ() );
			gp_Pnt startPoint = startCamPoint.CADPoint.Point;
			gp_Pnt approachPtG00 = startPoint.Translated( toolVec.Scaled( G00_APPROACH_LENGTH ) );
			gp_Pnt approachPtG01 = startPoint.Translated( toolVec.Scaled( G01_APPROACH_LENGTH ) );
			gp_Pnt safePlanePt = new gp_Pnt( approachPtG00.X(), approachPtG00.Y(), SAFE_PLANE_Z );
			WriteOnePoint( safePlanePt, firstPointPostData.Master, firstPointPostData.Slave, true );
			WriteOnePoint( approachPtG00, firstPointPostData.Master, firstPointPostData.Slave, true );
			WriteOnePoint( approachPtG01, firstPointPostData.Master, firstPointPostData.Slave, true );

			// start cutting
			m_StreamWriter.WriteLine( "// Cutting" + index );
			m_StreamWriter.WriteLine( "N" + index );
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\" H1;" );

			// write each process path
			WriteOneProcessPath( cuttingProcessData.LeadInCAMPointList, currentPathPostData.LeadInPostPointList );
			WriteOneProcessPath( cuttingProcessData.CAMPointList, currentPathPostData.MainPathPostPointList );
			WriteOneProcessPath( cuttingProcessData.OverCutCAMPointList, currentPathPostData.OverCutPostPointList );
			WriteOneProcessPath( cuttingProcessData.LeadOutCAMPointList, currentPathPostData.LeadOutPostPointList );

			// end cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_OFF\";" );

			// find last post point for lifting
			// with lead out
			if( cuttingProcessData.LeadLineParam.LeadOut.Type != LeadType.LeadLineType.None && currentPathPostData.LeadOutPostPointList.Count > 0 ) {
				lastPointPostData = currentPathPostData.LeadOutPostPointList.Last();
			}

			// with over cut but no lead out
			else if( cuttingProcessData.OverCutCAMPointList.Count > 0 && currentPathPostData.OverCutPostPointList.Count > 0 ) {
				lastPointPostData = currentPathPostData.OverCutPostPointList.Last();
			}

			// default main path
			else {
				lastPointPostData = currentPathPostData.MainPathPostPointList.Last();
			}
			WriteOnePoint( approachPtG00, lastPointPostData.Master, lastPointPostData.Slave, true );
			WriteOnePoint( safePlanePt, lastPointPostData.Master, lastPointPostData.Slave, true );
			return;
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

		void WriteOneProcessPath( List<CAMPoint> camList, List<PostPoint> postList )
		{
			// use cam point list to check is post point list length valid
			if( camList == null || postList == null || camList.Count == 0 || postList.Count == 0 || camList.Count != postList.Count ) {
				return;
			}
			for( int i = 0; i < camList.Count; i++ ) {
				var pos = postList[ i ];
				WriteOnePoint( new gp_Pnt( pos.X, pos.Y, pos.Z ), pos.Master, pos.Slave );
			}
		}

		const int SAFE_PLANE_Z = 300;
		const int G00_APPROACH_LENGTH = 40;
		const int G01_APPROACH_LENGTH = 3;
	}
}
