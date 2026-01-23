using MyCAM.Data;
using MyCAM.PathCache;
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
			if( dataManager == null || dataManager.MachineData == null || dataManager.EntryAndExitData == null || dataManager.PathIDList == null ) {
				throw new ArgumentNullException( "NCWriter constructor argument is null." );
			}
			m_DataManager = dataManager;
			m_PathIDList = dataManager.PathIDList;
			m_MachineData = dataManager.MachineData;
			m_PostSolver = new PostSolver( dataManager.MachineData );
			m_MasterAxisName = ConvertRotaryAxisName( m_MachineData.MasterRotaryAxis );
			m_SlaveAxisName = ConvertRotaryAxisName( m_MachineData.SlaveRotaryAxis );
			m_EntryAndExitData = dataManager.EntryAndExitData;
		}

		DataManager m_DataManager;
		IReadOnlyList<string> m_PathIDList;
		StreamWriter m_StreamWriter;
		PostSolver m_PostSolver;
		MachineData m_MachineData;
		string m_MasterAxisName = string.Empty;
		string m_SlaveAxisName = string.Empty;
		EntryAndExitData m_EntryAndExitData;
		const string NC_FILES_RELATIVE_DIR = "C:\\NETWORK\\CNC\\NcFiles";
		const string NC_FILE_DEFAULT_NAME = "MachingNC.nc";

		public StreamWriter Writer => m_StreamWriter;

		public bool ConvertSuccess( out string errorMessage )
		{
			errorMessage = string.Empty;
			string szTargetDir = GetNCFilesDirectory();
			try {
				using( m_StreamWriter = new StreamWriter( Path.Combine( szTargetDir, NC_FILE_DEFAULT_NAME ) ) ) {
					m_StreamWriter.WriteLine( "%@MACRO" );
					m_StreamWriter.WriteLine( "G43.4 P1;" ); // G43.4 新動程
					m_StreamWriter.WriteLine( "G65 P\"FileStart\" X\"Material1\" Y\"1.0\";" ); // 三點校正					

					// to keep last point of previous path
					PathEndInfo endInfoOfPreviousPath = null;
					PathType pathType;
					for( int i = 0; i < m_PathIDList.Count; i++ ) {
						if( !DataGettingHelper.GetPathObject( m_PathIDList[ i ], out PathObject pathObject ) ) {
							continue;
						}
						pathType = pathObject.PathType;
						if( pathType == PathType.Contour ) {

							// solve all post data of the path
							if( !ContourPostHelper.SolvePath( m_PostSolver, BuildPackageByID( m_PathIDList[ i ] ),
								endInfoOfPreviousPath, m_EntryAndExitData,
								out PostData postData, out endInfoOfPreviousPath ) ) {
								errorMessage = "後處理運算錯誤，路徑：" + ( i + 1 ).ToString();
								return false;
							}
							WriteCutting( postData, i + 1 );
						}
						else {
							StdPatternNCPackage package = BuildPackageByID_StandardPattern( m_PathIDList[ i ] );
							if( !StdPatternPostHelper.SolvePath( m_PostSolver, package, endInfoOfPreviousPath, m_EntryAndExitData,
								out StdPatternPostData postData, out endInfoOfPreviousPath ) ) {
								errorMessage = "後處理運算錯誤，路徑：" + ( i + 1 ).ToString();
								return false;
							}
							if( !DataGettingHelper.GetGeomDataByID( m_PathIDList[ i ], out IGeomData geomData ) ) {
								errorMessage = "路徑資訊取得錯誤，路徑：" + ( i + 1 ).ToString();
								return false;
							}
							WriteStandardPatternCutting( pathType, postData, pathObject.CraftData, geomData, i + 1 );
						}
					}

					// write exit
					if( m_PathIDList.Count > 0 ) {

						// calculate exit point
						PostTraverseBuilder.CalculateExit( endInfoOfPreviousPath, m_EntryAndExitData, out PostPoint exitPoint );
						m_StreamWriter.WriteLine( "// Exit" );
						NCWriterHelper.WriteLinearTraverse( m_StreamWriter, exitPoint, 0,
							m_MasterAxisName, m_SlaveAxisName, m_MachineData.MasterRotaryAxis, m_MachineData.SlaveRotaryAxis );
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
			m_StreamWriter.WriteLine( "G65 P\"LoadParameter\" H1;" );

			// traverse from previous path to current path
			NCWriterHelper.WriteTraverse( m_StreamWriter, currentPathPostData,
				m_MasterAxisName, m_SlaveAxisName, m_MachineData.MasterRotaryAxis, m_MachineData.SlaveRotaryAxis );

			// start cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_ON\";" );

			// write each process path
			WriteOneProcessPath( currentPathPostData.LeadInPostPointList );
			WriteOneProcessPath( currentPathPostData.MainPathPostPointList );
			WriteOneProcessPath( currentPathPostData.OverCutPostPointList );
			WriteOneProcessPath( currentPathPostData.LeadOutPostPointList );

			// end cutting
			m_StreamWriter.WriteLine( "G65 P\"LASER_OFF\";" );
			return;
		}

		void WriteStandardPatternCutting( PathType type, StdPatternPostData currentPathPostData, CraftData craftData, IGeomData geomData, int N_Index )
		{
			StdPatternNCWriter.WriteStandardPatternCutting( m_StreamWriter, type, currentPathPostData, craftData, geomData, N_Index,
				m_MasterAxisName, m_SlaveAxisName, m_MachineData.MasterRotaryAxis, m_MachineData.SlaveRotaryAxis );
		}

		void WriteOnePoint( PostPoint postPoint )
		{
			if( postPoint == null ) {
				return;
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = NCWriterHelper.GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI,
				m_MasterAxisName, m_SlaveAxisName, m_MachineData.MasterRotaryAxis, m_MachineData.SlaveRotaryAxis );
			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand};" );
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

		ContourNCPackage BuildPackageByID( string szID )
		{
			if( !PathCacheProvider.TryGetLeadCache( szID, out ILeadCache leadCache )
				|| !PathCacheProvider.TryGetOverCutCache( szID, out IOverCutCache overCutCache )
				|| !PathCacheProvider.TryGetProcessPathStartEndCache( szID, out IProcessPathStartEndCache processPathStartEndCache )
				|| !PathCacheProvider.TryGetTraverseDataCache( szID, out ITraverseDataCache traverseDataCache )
				|| !PathCacheProvider.TryGetMainPathCache( szID, out IMainPathCache mainPathCache ) ) {
				return null;
			}
			return new ContourNCPackage(
				leadCache.LeadData,
				overCutCache.OverCutLength,
				mainPathCache.MainPathPointList,
				leadCache.LeadInCAMPointList,
				leadCache.LeadOutCAMPointList,
				overCutCache.OverCutCAMPointList,
				traverseDataCache.TraverseData,
				traverseDataCache.GetProcessStartPoint(),
				traverseDataCache.GetProcessEndPoint() );
		}

		StdPatternNCPackage BuildPackageByID_StandardPattern( string szID )
		{
			if( !PathCacheProvider.TryGetLeadCache( szID, out ILeadCache leadCache )
				|| !PathCacheProvider.TryGetStdPatternRefPointCache( szID, out IStdPatternRefPointCache refPointCache )
				|| !PathCacheProvider.TryGetMainPathStartPointCache( szID, out IMainPathStartPointCache mainPathStartPointCache )
				|| !PathCacheProvider.TryGetProcessPathStartEndCache( szID, out IProcessPathStartEndCache processPathStartEndCache )
				|| !PathCacheProvider.TryGetTraverseDataCache( szID, out ITraverseDataCache traverseDataCache ) ) {
				return null;
			}
			return new StdPatternNCPackage(
						refPointCache.GetProcessRefPoint(),
						mainPathStartPointCache.GetMainPathStartCAMPoint(),
						traverseDataCache.TraverseData,
						processPathStartEndCache.GetProcessStartPoint(),
						processPathStartEndCache.GetProcessEndPoint(),
						leadCache.LeadData,
						leadCache.LeadInCAMPointList
					);
		}

		string GetNCFilesDirectory()
		{
			string szTargetDir = NC_FILES_RELATIVE_DIR;
			if( Directory.Exists( szTargetDir ) == false ) {
				Directory.CreateDirectory( szTargetDir );
			}
			return szTargetDir;
		}
	}
}
