using MyCAM.App;
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
			if( dataManager == null || dataManager.EntryAndExitData == null || dataManager.PathIDList == null ) {
				throw new ArgumentNullException( "NCWriter constructor argument is null." );
			}
			m_DataManager = dataManager;
			m_PathIDList = dataManager.PathIDList;
			m_AxisConfig = new AxisConfig( dataManager.MachineData as FiveAxisMachineData );
			m_EntryAndExitData = dataManager.EntryAndExitData;
		}

		DataManager m_DataManager;
		IReadOnlyList<string> m_PathIDList;
		StreamWriter m_StreamWriter;
		AxisConfig m_AxisConfig;
		EntryAndExitData m_EntryAndExitData;

		public bool ConvertSuccess( string fileName, string szTempFilePath, out string errorMessage )
		{
			errorMessage = string.Empty;
			try {
				using( m_StreamWriter = new StreamWriter( szTempFilePath ) ) {
					m_StreamWriter.WriteLine( "%@MACRO" );
					WriteCalibrationSection();

					// file header
					CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.Header,
						BuildFileHeaderDict() );

					// process all paths
					if( !ProcessAllPaths( out PathEndInfo endInfoOfLastPath, out errorMessage ) ) {
						return false;
					}

					WriteExitSection( endInfoOfLastPath );

					// file tail
					CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.Tail, new Dictionary<string, string> { } );
				}
				return true;
			}
			catch( Exception ex ) {
				errorMessage = ex.Message;
				return false;
			}
		}

		public bool ConvertPostDataListSuccess( out string errorMessage, out List<PostData> postDataList )
		{
			errorMessage = string.Empty;
			postDataList = new List<PostData>();
			try {
				PathEndInfo endInfoOfLastPath = null;
				for( int i = 0; i < m_PathIDList.Count; i++ ) {
					if( !DataGettingHelper.GetPathObject( m_PathIDList[ i ], out PathObject pathObject ) ) {
						continue;
					}
					PathType pathType = pathObject.PathType;
					if( pathType == PathType.Contour ) {
						if( !GetContourPostData( i, ref endInfoOfLastPath, out PostData postData, out errorMessage ) ) {
							return false;
						}
						postDataList.Add( postData );
					}
					else {
						if( !GetStdPostData( i, ref endInfoOfLastPath, out StdPatternPostData stdPostData, out _, out errorMessage ) ) {
							return false;
						}

						if( !GetContourPostData( i, ref endInfoOfLastPath, out PostData postData, out errorMessage ) ) {
							return false;
						}

						// TODO：this is temporary solution to make standard pattern post data compatible with std pattern contour post data, which is used for tool path display in process setting form. We will refactor the code structure later to avoid this kind of workaround.
						ApplyStdPatternPostDataToPostData( postData, stdPostData );
						postDataList.Add( postData );
					}
				}
				return true;
			}
			catch( Exception ex ) {
				errorMessage = ex.Message;
				return false;
			}
		}

		void WriteCalibrationSection()
		{
			if( !NeedToWriteCalibrationPnt( out CalibrationData calibrationData ) ) {
				return;
			}
			m_StreamWriter.WriteLine( $"G65 P\"REFPOINT_SET\" X1={calibrationData.Ref_Pnt1.x.ToString( "F3" )} Y1={calibrationData.Ref_Pnt1.y.ToString( "F3" )} Z1={calibrationData.Ref_Pnt1.z.ToString( "F3" )} X2={calibrationData.Ref_Pnt2.x.ToString( "F3" )} Y2={calibrationData.Ref_Pnt2.y.ToString( "F3" )} Z2={calibrationData.Ref_Pnt2.z.ToString( "F3" )} X3={calibrationData.Ref_Pnt3.x.ToString( "F3" )} Y3={calibrationData.Ref_Pnt3.y.ToString( "F3" )} Z3={calibrationData.Ref_Pnt3.z.ToString( "F3" )};" );
			m_StreamWriter.WriteLine( $"G65 P\"CALPOINT_SET\" X1=0.000 Y1=0.000 Z1=0.000 X2=0.000 Y2=0.000 Z2=0.000 X3=0.000 Y3=0.000 Z3=0.000;" );
		}

		void WriteExitSection( PathEndInfo endInfoOfLastPath )
		{
			if( m_PathIDList.Count == 0 ) {
				return;
			}
			PostTraverseBuilder.CalculateExit( endInfoOfLastPath, m_EntryAndExitData, out PostPoint exitPoint );
			m_StreamWriter.WriteLine( "// Exit" );
			NCWriterHelper.WriteLinearTraverse( m_StreamWriter, exitPoint, 0,
				m_AxisConfig.MasterAxisName, m_AxisConfig.SlaveAxisName, m_AxisConfig.MasterRotaryAxis, m_AxisConfig.SlaveRotaryAxis );
		}

		bool ProcessAllPaths( out PathEndInfo endInfoOfLastPath, out string errorMessage )
		{
			errorMessage = string.Empty;
			endInfoOfLastPath = null;
			for( int i = 0; i < m_PathIDList.Count; i++ ) {
				if( !DataGettingHelper.GetPathObject( m_PathIDList[ i ], out PathObject pathObject ) ) {
					continue;
				}
				PathType pathType = pathObject.PathType;
				int nPathLayer = pathObject.CraftData.TechLayer;
				if( pathType == PathType.Contour ) {
					if( !GetContourPostData( i, ref endInfoOfLastPath, out PostData postData, out errorMessage ) ) {
						return false;
					}
					WriteCutting( postData, m_PathIDList[ i ], pathObject.CraftData, i + 1, nPathLayer );
				}
				else {
					if( !GetStdPostData( i, ref endInfoOfLastPath, out StdPatternPostData postData, out StdPatternNCPackage package, out errorMessage ) ) {
						return false;
					}
					WriteStandardPatternCutting( pathType, postData, pathObject.CraftData, package.GeomData, i + 1 );
				}
			}
			return true;
		}

		#region NC Package Builder

		ContourNCPackage BuildPackageByID( string szID )
		{
			if( !DataGettingHelper.GetCraftDataByID( szID, out CraftData craftData ) ) {
				return null;
			}
			if( !DataGettingHelper.GetPathCacheByID( szID, out IPathCache pathCache ) ) {
				return null;
			}
			return new ContourNCPackage(
				craftData.LeadData,
				craftData.OverCutLength,
				pathCache.MainPathPointList,
				pathCache.LeadInPointList,
				pathCache.LeadOutPointList,
				pathCache.OverCutPointList,
				craftData.TraverseData,
				CacheHelper.GetProcessStartPoint( szID ),
				CacheHelper.GetProcessEndPoint( szID )
			);
		}

		StdPatternNCPackage BuildPackageByID_StandardPattern( string szID )
		{
			if( !DataGettingHelper.GetStdPatternCacheByID( szID, out IStdPatternCache stdPatternCache ) ) {
				return null;
			}
			if( !DataGettingHelper.GetCraftDataByID( szID, out CraftData craftData ) ) {
				return null;
			}
			return new StdPatternNCPackage(
				stdPatternCache.RefPoint,
				CacheHelper.GetMainPathStartPoint( szID ),
				craftData.TraverseData,
				CacheHelper.GetProcessStartPoint( szID ),
				CacheHelper.GetProcessEndPoint( szID ),
				craftData.LeadData,
				stdPatternCache.LeadInPointList,
				stdPatternCache.ComputeGeomData
			);
		}

		#endregion

		#region Build Post Data

		bool GetContourPostData( int pathIndex, ref PathEndInfo endInfo, out PostData postData, out string errorMessage )
		{
			errorMessage = string.Empty;
			postData = null;
			if( !ContourPostHelper.GetPostData( BuildPackageByID( m_PathIDList[ pathIndex ] ),
				endInfo, m_EntryAndExitData, out postData, out endInfo ) ) {
				errorMessage = string.Format( m_LanguageManager.TryGetString( "Error_PostProcessing", ERROR_POST_PROCESSING ), pathIndex );
				return false;
			}
			return true;
		}

		bool GetStdPostData( int pathIndex, ref PathEndInfo endInfo, out StdPatternPostData postData, out StdPatternNCPackage package, out string errorMessage )
		{
			errorMessage = string.Empty;
			postData = null;
			package = BuildPackageByID_StandardPattern( m_PathIDList[ pathIndex ] );
			if( !StdPatternPostHelper.GetPostData( package, endInfo, m_EntryAndExitData,
				out postData, out endInfo ) ) {
				errorMessage = string.Format( m_LanguageManager.TryGetString( "Error_PostProcessing", ERROR_POST_PROCESSING ), pathIndex );
				return false;
			}
			return true;
		}

		void ApplyStdPatternPostDataToPostData( PostData postData, StdPatternPostData stdPostData )
		{
			// Update main path points
			for( int j = 0; j < postData.MainPathPostPointList.Count; j++ ) {
				postData.MainPathPostPointList[ j ].Master = stdPostData.RefPoint.Master;
				postData.MainPathPostPointList[ j ].Slave = stdPostData.RefPoint.Slave;
			}

			// Update lead-in points
			if( postData.LeadInPostPointList.Count != 0 ) {
				foreach( var postPoint in postData.LeadInPostPointList ) {
					postPoint.Master = stdPostData.RefPoint.Master;
					postPoint.Slave = stdPostData.RefPoint.Slave;
				}
			}

			// Update over-cut points
			if( postData.OverCutPostPointList.Count != 0 ) {
				foreach( var postPoint in postData.OverCutPostPointList ) {
					postPoint.Master = stdPostData.RefPoint.Master;
					postPoint.Slave = stdPostData.RefPoint.Slave;
				}
			}

			// Update traverse points
			if( postData.LiftUpPostPoint != null ) {
				postData.LiftUpPostPoint = stdPostData.LiftUpPostPoint;
			}
			if( postData.CutDownPostPoint != null ) {
				postData.CutDownPostPoint = stdPostData.CutDownPostPoint;
			}
			if( postData.LiftUpPostSafePlanePoint != null ) {
				postData.LiftUpPostSafePlanePoint = stdPostData.LiftUpPostSafePlanePoint;
			}
			if( postData.CutDownPostSafePlanePoint != null ) {
				postData.CutDownPostSafePlanePoint = stdPostData.CutDownPostSafePlanePoint;
			}
			if( postData.FrogLeapMidPostPoint != null ) {
				postData.FrogLeapMidPostPoint = stdPostData.FrogLeapMidPostPoint;
			}
			if( postData.ProcessStartPoint != null ) {
				postData.ProcessStartPoint = stdPostData.ProcessStartPoint;
			}
		}

		#endregion

		#region Write Cutting

		void WriteCutting( PostData currentPathPostData, string szPathID, CraftData craftData, int N_Index, int nLayIndex = 1 )
		{
			// Header
			Dictionary<string, string> varDict = BuildPathDict( N_Index, nLayIndex );
			// traverse header
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.TraverseHeader, varDict );
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.TraverseHead );

			// traverse from previous path to current path
			NCWriterHelper.WriteTraverse( m_StreamWriter, currentPathPostData,
				m_AxisConfig.MasterAxisName, m_AxisConfig.SlaveAxisName, m_AxisConfig.MasterRotaryAxis, m_AxisConfig.SlaveRotaryAxis );

			// traverse tail
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.TraverseTail );
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.TraverseTail, varDict );

			// path header			
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.PathHeader, varDict );
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.PathHead );

			// start cutting, write each process path
			WriteOneProcessPath( currentPathPostData.LeadInPostPointList );
			WriteOneProcessPath( currentPathPostData.MainPathPostPointList,
				ExtendedGCodeWriter.BuildControlPointGCodeMap( szPathID, craftData, currentPathPostData.MainPathPostPointList.Count ) );
			WriteOneProcessPath( currentPathPostData.OverCutPostPointList );
			WriteOneProcessPath( currentPathPostData.LeadOutPostPointList );

			// path tail
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.PathTail );
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.PathTail, varDict );
			return;
		}

		void WriteStandardPatternCutting( PathType type, StdPatternPostData currentPathPostData, CraftData craftData, IStdPatternGeomData geomData, int N_Index )
		{
			StdPatternNCWriter.WriteStandardPatternCutting( m_StreamWriter, type, currentPathPostData, craftData, geomData, N_Index,
				m_AxisConfig.MasterAxisName, m_AxisConfig.SlaveAxisName, m_AxisConfig.MasterRotaryAxis, m_AxisConfig.SlaveRotaryAxis );
		}

		void WriteOnePoint( PostPoint postPoint, string szExtendedGCode = null )
		{
			if( postPoint == null ) {
				return;
			}
			ExtendedGCodeWriter.WriteContent( m_StreamWriter, szExtendedGCode );
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = NCWriterHelper.GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI,
				m_AxisConfig.MasterAxisName, m_AxisConfig.SlaveAxisName, m_AxisConfig.MasterRotaryAxis, m_AxisConfig.SlaveRotaryAxis );
			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand};" );

			if( postPoint.IsMicroJointEnd ) {
				m_StreamWriter.WriteLine( $"G65 P\"FlyCut\" X{szX} Y{szY} Z{szZ} S1;" );
			}
			if( postPoint.IsMicroJointStart ) {
				m_StreamWriter.WriteLine( $"G65 P\"FlyCut\" X{szX} Y{szY} Z{szZ} S0;" );
			}
		}

		void WriteOneProcessPath( List<PostPoint> postPointList, Dictionary<int, string> controlPointGCodeMap = null )
		{
			if( postPointList == null || postPointList.Count == 0 ) {
				return;
			}
			for( int i = 0; i < postPointList.Count; i++ ) {
				var onePostPoint = postPointList[ i ];
				string szExtendedGCode = null;
				controlPointGCodeMap?.TryGetValue( i, out szExtendedGCode );
				WriteOnePoint( onePostPoint, szExtendedGCode );
			}
		}

		bool NeedToWriteCalibrationPnt( out CalibrationData calibrationData )
		{
			// get calibration data
			bool isGetREFPointSuccess = DataGettingHelper.GetREFPnt( out calibrationData );
			if( isGetREFPointSuccess && calibrationData != null && calibrationData.IsBeenSet ) {
				return true;
			}
			return false;
		}

		#endregion

		#region Customized Variable Dic Maker

		Dictionary<string, string> BuildFileHeaderDict()
		{
			if( m_PathIDList.Count == 0 ) {
				return new Dictionary<string, string>();
			}
			IProcessPoint startPoint = CacheHelper.GetProcessStartPoint( m_PathIDList[ 0 ] );
			if( startPoint == null ) {
				return new Dictionary<string, string>();
			}
			return new Dictionary<string, string> {
				{ "EntryMaster", ( startPoint.ModMaster_rad * 180 / Math.PI ).ToString( "F3" ) },
				{ "EntrySlave", ( startPoint.ModSlave_rad * 180 / Math.PI ).ToString( "F3" ) }
			};
		}

		Dictionary<string, string> BuildPathDict( int N_Index, int nLayIndex )
		{
			// Header
			Dictionary<string, string> varDict = new Dictionary<string, string> {
				{ "PathIndex", N_Index.ToString() },
				{ "LayerIndex", nLayIndex.ToString() }
			};
			return varDict;
		}

		#endregion

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "NCWriter" );
		const string ERROR_POST_PROCESSING = "後處理運算錯誤，路徑：{0}";
	}
}
