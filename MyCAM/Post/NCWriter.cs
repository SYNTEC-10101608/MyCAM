using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
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
			m_TraverseCalculationInfo = TraverseHelper.BuildTraverseCalculationInfo( dataManager );
			m_EntryAndExitData = dataManager.EntryAndExitData;
		}

		DataManager m_DataManager;
		IReadOnlyList<string> m_PathIDList;
		StreamWriter m_StreamWriter;
		AxisConfig m_AxisConfig;
		TraverseCalculationInfo m_TraverseCalculationInfo;
		EntryAndExitData m_EntryAndExitData;

		public bool ConvertSuccess( string fileName, string szTempFilePath, bool drawingOnly, out string errorMessage )
		{
			errorMessage = string.Empty;
			try {
				using( m_StreamWriter = new StreamWriter( szTempFilePath ) ) {
					m_StreamWriter.WriteLine( "%@MACRO" );
					WriteCalibrationSection( drawingOnly );

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
					if( pathObject.CraftData.ProcessPath == false ) {
						continue;
					}
					if( pathType == PathType.Contour ) {
						if( !GetContourPostData( i, ref endInfoOfLastPath, out PostData postData, out errorMessage ) ) {
							return false;
						}
						postDataList.Add( postData );
					}
					else {
						PathEndInfo previousPathEndInfo = endInfoOfLastPath;
						if( !GetStdPostData( i, ref endInfoOfLastPath, out StdPatternPostData stdPostData, out _, out errorMessage ) ) {
							return false;
						}

						if( !GetContourPostData( i, ref previousPathEndInfo, out PostData postData, out errorMessage ) ) {
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

		void WriteCalibrationSection( bool drawingOnly )
		{
			if( !drawingOnly && !m_DataManager.CalibrationData.HasCalculatedSnapshot ) {
				throw new InvalidOperationException( m_LanguageManager.TryGetString( "Error_CalibrationRequired", CALIBRATION_REQUIRED ) );
			}
			if( !NeedToWriteCalibrationPnt( out CalibrationData calibrationData ) ) {
				return;
			}
			m_StreamWriter.WriteLine( FormattableString.Invariant( $"G65 P\"REFPOINT_SET\" X1={calibrationData.Ref_Pnt1.x:F3} Y1={calibrationData.Ref_Pnt1.y:F3} Z1={calibrationData.Ref_Pnt1.z:F3} X2={calibrationData.Ref_Pnt2.x:F3} Y2={calibrationData.Ref_Pnt2.y:F3} Z2={calibrationData.Ref_Pnt2.z:F3} X3={calibrationData.Ref_Pnt3.x:F3} Y3={calibrationData.Ref_Pnt3.y:F3} Z3={calibrationData.Ref_Pnt3.z:F3};" ) );
			if( !drawingOnly && calibrationData.HasCalibrationInputs ) {
				m_StreamWriter.WriteLine( FormattableString.Invariant( $"G65 P\"CALPOINT_SET\" X1={calibrationData.Work_Pnt1.x:F3} Y1={calibrationData.Work_Pnt1.y:F3} Z1={calibrationData.Work_Pnt1.z:F3} X2={calibrationData.Work_Pnt2.x:F3} Y2={calibrationData.Work_Pnt2.y:F3} Z2={calibrationData.Work_Pnt2.z:F3} X3={calibrationData.Work_Pnt3.x:F3} Y3={calibrationData.Work_Pnt3.y:F3} Z3={calibrationData.Work_Pnt3.z:F3} E{calibrationData.Tolerance.Value:F3};" ) );
			}
		}

		void WriteExitSection( PathEndInfo endInfoOfLastPath )
		{
			if( m_PathIDList.Count == 0 ) {
				return;
			}
			PostTraverseBuilder.CalculateExit( endInfoOfLastPath, m_EntryAndExitData, out PostPoint exitPoint );
			m_StreamWriter.WriteLine( "// Exit" );
			NCWriterHelper.WriteLinearTraverse( m_StreamWriter, exitPoint, m_AxisConfig );
		}

		bool ProcessAllPaths( out PathEndInfo endInfoOfLastPath, out string errorMessage )
		{
			errorMessage = string.Empty;
			endInfoOfLastPath = null;
			for( int i = 0; i < m_PathIDList.Count; i++ ) {
				if( !DataGettingHelper.GetPathObject( m_PathIDList[ i ], out PathObject pathObject ) || !DataGettingHelper.GetCraftDataByID( m_PathIDList[ i ], out CraftData craftData ) ) {
					continue;
				}
				PathType pathType = pathObject.PathType;
				if( craftData.ProcessPath == false ) {
					WriteNonProcessingPath( i + 1 );
					continue;
				}
				bool isEntry = endInfoOfLastPath == null;
				if( pathType == PathType.Contour ) {
					if( !GetContourPostData( i, ref endInfoOfLastPath, out PostData postData, out errorMessage ) ) {
						return false;
					}
					WriteCutting( postData, m_PathIDList[ i ], pathObject.CraftData, i + 1, isEntry, craftData.TechLayer );
				}
				else {
					if( !GetStdPostData( i, ref endInfoOfLastPath, out StdPatternPostData postData, out StdPatternNCPackage package, out errorMessage ) ) {
						return false;
					}
					WriteStandardPatternCutting( pathType, postData, pathObject.CraftData, package.GeomData, i + 1, isEntry );
				}
			}
			return true;
		}

		void WriteNonProcessingPath( int pathNumber )
		{
			m_StreamWriter.WriteLine( $"// Cutting{pathNumber}" );
			m_StreamWriter.WriteLine( $"N{pathNumber};" );
			m_StreamWriter.WriteLine( "// None Process Path" );
			m_StreamWriter.WriteLine();
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
				endInfo, m_EntryAndExitData, m_TraverseCalculationInfo, out postData, out endInfo ) ) {
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
			if( !StdPatternPostHelper.GetPostData( package, endInfo, m_EntryAndExitData, m_TraverseCalculationInfo, out postData, out endInfo ) ) {
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
			if( postData.SafeBoundaryPostPointList != null ) {
				postData.SafeBoundaryPostPointList = stdPostData.SafeBoundaryPostPointList;
			}
		}

		#endregion

		#region Write Cutting

		void WriteCutting( PostData currentPathPostData, string szPathID, CraftData craftData, int N_Index, bool isEntry, int nLayIndex = 1 )
		{
			double enterTiltAngle_deg = GetStartOrEndTiltAngle_deg( currentPathPostData, isStartPoint: true );

			// Header
			Dictionary<string, string> varDict = BuildPathDict( N_Index, nLayIndex, enterTiltAngle_deg );
			// traverse header
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.TraverseHeader, varDict );
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.TraverseHead );

			// traverse from previous path to current path
			if( isEntry ) {
				NCWriterHelper.WriteEntry( m_StreamWriter, currentPathPostData, m_AxisConfig );
			}
			else {
				NCWriterHelper.WriteTraverse( m_StreamWriter, currentPathPostData, m_AxisConfig, enterTiltAngle_deg );
			}

			// traverse tail
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.TraverseTail );
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.TraverseTail, varDict );

			// path header			
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.PathHeader, varDict );
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.PathHead );

			// build R corner map for main path
			Dictionary<int, RCornerData> rCornerMap = RCornerWriter.BuildRCornerCAMIndexMap(
				szPathID, craftData, currentPathPostData.MainPathPostPointList.Count );

			// start cutting, write each process path
			WriteOneProcessPath( currentPathPostData.LeadInPostPointList );
			WriteOneProcessPath( currentPathPostData.MainPathPostPointList,
				controlPointGCodeMap: ExtendedGCodeWriter.BuildControlPointGCodeMap( szPathID, craftData, currentPathPostData.MainPathPostPointList.Count ),
				rCornerMap: rCornerMap );
			WriteOneProcessPath( currentPathPostData.OverCutPostPointList );
			WriteOneProcessPath( currentPathPostData.LeadOutPostPointList );

			// path tail
			ExtendedGCodeWriter.WriteExtendedGCode( m_StreamWriter, craftData, ExtendedGCodeType.PathTail );
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.PathTail, varDict );
			return;
		}

		void WriteStandardPatternCutting( PathType type, StdPatternPostData currentPathPostData, CraftData craftData, IStdPatternGeomData geomData, int N_Index, bool isEntry )
		{
			StdPatternNCWriter.WriteStandardPatternCutting( m_StreamWriter, type, currentPathPostData, craftData, geomData, N_Index, m_AxisConfig, isEntry );
		}

		void WriteOnePoint( PostPoint postPoint, double? tiltAngle_deg = null, string szExtendedGCode = null, bool isRCornerStart = false, bool isRCornerEnd = false )
		{
			if( postPoint == null ) {
				return;
			}
			string szX = postPoint.X.ToString( "F3" );
			string szY = postPoint.Y.ToString( "F3" );
			string szZ = postPoint.Z.ToString( "F3" );
			string szRotaryAxisCommand = NCWriterHelper.GetRotaryAxisCommand( postPoint.Master * 180 / Math.PI, postPoint.Slave * 180 / Math.PI, m_AxisConfig );
			//string szTiltAngle = tiltAngle_deg.HasValue ? " S3=" + tiltAngle_deg.Value.ToString( "F3" ) : string.Empty;

			m_StreamWriter.WriteLine( $"G01 X{szX} Y{szY} Z{szZ} {szRotaryAxisCommand};" );

			ExtendedGCodeWriter.WriteContent( m_StreamWriter, szExtendedGCode );

			// R corner end is written after moving to the point
			if( isRCornerEnd ) {
				m_StreamWriter.WriteLine( $"G65 P\"CornerEnd\";" );
			}

			// R corner start is written after moving to the point
			if( isRCornerStart ) {
				m_StreamWriter.WriteLine( $"G65 P\"CornerStart\";" );
			}
			if( postPoint.IsMicroJointEnd ) {
				m_StreamWriter.WriteLine( $"G65 P\"FlyCut\" X{szX} Y{szY} Z{szZ} S1;" );
			}
			if( postPoint.IsMicroJointStart ) {
				m_StreamWriter.WriteLine( $"G65 P\"FlyCut\" X{szX} Y{szY} Z{szZ} S0;" );
			}
		}

		void WriteOneProcessPath( List<PostPoint> postPointList, Dictionary<int, string> controlPointGCodeMap = null, Dictionary<int, RCornerData> rCornerMap = null )
		{
			if( postPointList == null || postPointList.Count == 0 ) {
				return;
			}

			// sort ranges by start index for sequential access
			List<RCornerData> sortedRanges = new List<RCornerData>();
			if( rCornerMap != null ) {
				foreach( RCornerData range in rCornerMap.Values ) {
					sortedRanges.Add( range );
				}
				sortedRanges.Sort( CompareRCornerByStartIndex );
			}
			int rangeIndex = 0;
			RCornerData currentRange = null;

			for( int i = 0; i < postPointList.Count; i++ ) {
				var onePostPoint = postPointList[ i ];

				// move to next range if current point is past current range
				if( currentRange != null && i > currentRange.EndIndex ) {
					currentRange = null;
					rangeIndex++;
				}

				// activate next range if current point reaches its start
				if( currentRange == null && rangeIndex < sortedRanges.Count && i == sortedRanges[ rangeIndex ].StartIndex ) {
					currentRange = sortedRanges[ rangeIndex ];
				}

				bool isRCornerStart = currentRange != null && i == currentRange.StartIndex;
				bool isRCornerEnd = currentRange != null && i == currentRange.EndIndex;

				string szExtendedGCode = null;
				controlPointGCodeMap?.TryGetValue( i, out szExtendedGCode );
				double? tiltAngle_deg = onePostPoint?.TiltAngle_deg;
				WriteOnePoint( onePostPoint, tiltAngle_deg, szExtendedGCode, isRCornerStart, isRCornerEnd );
			}
		}

		double GetStartOrEndTiltAngle_deg( PostData postData, bool isStartPoint )
		{
			// if postData is null, return 0.0 as a fallback
			if( postData == null ) {
				return 0.0;
			}

			// if the main path post point list is null or empty, return 0.0
			if( postData.MainPathPostPointList == null || postData.MainPathPostPointList.Count == 0 ) {
				return 0.0;
			}

			int pointIndex = isStartPoint ? 0 : postData.MainPathPostPointList.Count - 1;
			PostPoint boundaryPoint = postData.MainPathPostPointList[ pointIndex ];
			if( boundaryPoint == null ) {
				return 0.0;
			}
			return boundaryPoint.TiltAngle_deg;
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

		static int CompareRCornerByStartIndex( RCornerData a, RCornerData b )
		{
			return a.StartIndex.CompareTo( b.StartIndex );
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

		Dictionary<string, string> BuildPathDict( int N_Index, int nLayIndex, double enterTiltAngle_deg )
		{
			// Header
			Dictionary<string, string> varDict = new Dictionary<string, string> {
				{ "PathIndex", N_Index.ToString() },
				{ "LayerIndex", nLayIndex.ToString() },
				{ "EnterTiltAngle", enterTiltAngle_deg.ToString( "F3" ) }
			};
			return varDict;
		}

		#endregion

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "NCWriter" );
		const string CALIBRATION_REQUIRED = "目前沒有有效的三點校正計算資料。請先按「計算」取得結果，或明確選擇僅輸出圖紙點。";
		const string ERROR_POST_PROCESSING = "後處理運算錯誤，路徑：{0}";
	}
}
