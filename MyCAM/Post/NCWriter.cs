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

		public StreamWriter Writer => m_StreamWriter;

		public bool ConvertSuccess( string fileName, string szTempFilePath, out string errorMessage )
		{
			errorMessage = string.Empty;
			try {
				using( m_StreamWriter = new StreamWriter( szTempFilePath ) ) {
					m_StreamWriter.WriteLine( "%@MACRO" );

					// check if need to write calibration point
					bool isNeed = NeedToWriteCalibrationPnt( out CalibrationData calibrationData );
					if( isNeed ) {
						m_StreamWriter.WriteLine( $"G65 P\"REFPOINT_SET\" X1={calibrationData.Ref_Pnt1.x.ToString( "F3" )} Y1={calibrationData.Ref_Pnt1.y.ToString( "F3" )} Z1={calibrationData.Ref_Pnt1.z.ToString( "F3" )} X2={calibrationData.Ref_Pnt2.x.ToString( "F3" )} Y2={calibrationData.Ref_Pnt2.y.ToString( "F3" )} Z2={calibrationData.Ref_Pnt2.z.ToString( "F3" )} X3={calibrationData.Ref_Pnt3.x.ToString( "F3" )} Y3={calibrationData.Ref_Pnt3.y.ToString( "F3" )} Z3={calibrationData.Ref_Pnt3.z.ToString( "F3" )};" );
					}

					// file header
					CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.Header,
						new Dictionary<string, string> { } );

					// to keep last point of previous path
					PathEndInfo endInfoOfPreviousPath = null;
					PathType pathType;
					for( int i = 0; i < m_PathIDList.Count; i++ ) {
						if( !DataGettingHelper.GetPathObject( m_PathIDList[ i ], out PathObject pathObject ) ) {
							continue;
						}
						pathType = pathObject.PathType;
						int nPathLayer = pathObject.CraftData.TechLayer;
						if( pathType == PathType.Contour ) {

							// solve all post data of the path
							if( !ContourPostHelper.SolvePath( m_PostSolver, BuildPackageByID( m_PathIDList[ i ] ),
								endInfoOfPreviousPath, m_EntryAndExitData,
								out PostData postData, out endInfoOfPreviousPath ) ) {
								errorMessage = "後處理運算錯誤，路徑：" + ( i + 1 ).ToString();
								return false;
							}
							WriteCutting( postData, i + 1, nPathLayer );
						}
						else {
							StdPatternNCPackage package = BuildPackageByID_StandardPattern( m_PathIDList[ i ] );
							if( !StdPatternPostHelper.SolvePath( m_PostSolver, package, endInfoOfPreviousPath, m_EntryAndExitData,
								out StdPatternPostData postData, out endInfoOfPreviousPath ) ) {
								errorMessage = "後處理運算錯誤，路徑：" + ( i + 1 ).ToString();
								return false;
							}
							WriteStandardPatternCutting( pathType, postData, pathObject.CraftData, package.GeomData, i + 1 );
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
						postDataList.Add( postData );
					}
					else {
						StdPatternNCPackage package = BuildPackageByID_StandardPattern( m_PathIDList[ i ] );
						if( !StdPatternPostHelper.SolvePath( m_PostSolver, package, endInfoOfPreviousPath, m_EntryAndExitData,
							out StdPatternPostData stdPostData, out endInfoOfPreviousPath ) ) {
							errorMessage = "後處理運算錯誤，路徑：" + ( i + 1 ).ToString();
							return false;
						}

						if( !ContourPostHelper.SolvePath( m_PostSolver, BuildPackageByID( m_PathIDList[ i ] ),
							endInfoOfPreviousPath, m_EntryAndExitData,
							out PostData postData, out _ ) ) {

							errorMessage = "後處理運算錯誤，路徑：" + ( i + 1 ).ToString();
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

		void WriteCutting( PostData currentPathPostData, int N_Index, int nLayIndex = 1 )
		{
			// Header
			Dictionary<string, string> varDict = new Dictionary<string, string> {
				{ "PathIndex", N_Index.ToString() },
				{ "LayerIndex", nLayIndex.ToString() }
			};

			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.PathHeader, varDict );


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

			// path tail
			CustPostWriter.WriteCustomizedSection( m_StreamWriter, MyApp.CustomizedPostInfo?.PathTail, varDict );
			return;
		}

		void WriteStandardPatternCutting( PathType type, StdPatternPostData currentPathPostData, CraftData craftData, IStdPatternGeomData geomData, int N_Index )
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

		bool NeedToWriteCalibrationPnt( out CalibrationData calibrationData )
		{
			// get calibration data
			bool isGetREFPointSuccess = DataGettingHelper.GetREFPnt( out calibrationData );
			if( isGetREFPointSuccess && calibrationData != null && calibrationData.IsBeenSet ) {
				return true;
			}
			return false;
		}
	}
}
