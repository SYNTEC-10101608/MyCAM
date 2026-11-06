using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using MyCAM.Post;
using OCC.gp;
using OCC.V3d;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class EditableInfo
	{
		public EditableInfo()
		{
			// all set true
			IsStartPointEditable = true;
			IsPathReverseEditable = true;
			IsOverCutEditable = true;
			IsLeadLineEditable = true;
			IsToolVecEditable = true;
			IsToolVecReverseEditable = true;
			IsFixedToolVecEditable = true;
			IsTraverseEditable = true;
			IsAutoOrderEditable = true;
			IsPathCompensateEditable = true;
			IsMicroJointEditable = true;
			IsExtendedGCodeEditable = true;
			IsRCornerEditable = true;
			IsContourEditable = true;
		}

		public bool IsStartPointEditable;
		public bool IsPathReverseEditable;
		public bool IsOverCutEditable;
		public bool IsLeadLineEditable;
		public bool IsToolVecEditable;
		public bool IsToolVecReverseEditable;
		public bool IsFixedToolVecEditable;
		public bool IsTraverseEditable;
		public bool IsAutoOrderEditable;
		public bool IsPathCompensateEditable;
		public bool IsMicroJointEditable;
		public bool IsExtendedGCodeEditable;
		public bool IsRCornerEditable;
		public bool IsContourEditable;
	}

	internal class CAMEditor : EditorBase
	{
		// to notice main form
		public Action<EditableInfo> PathPropertyChanged;
		public Action<EditActionType, EActionStatus> RaiseCAMActionStatusChange;
		public Action<PathType> PathShapeTypeChanged;

		// action with dialog need to disable main form
		// because other action enterance might close this action, but without closing this form
		public Action<EActionStatus> RaiseWithDlgActionStatusChange;

		public CAMEditor( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager )
			: base( dataManager, viewer, treeView, viewManager )
		{
			m_DataManager.PathAdded += OnPathAdded;
			m_DataManager.NeedOptimizedIK += OptimizeIKContinuityData;

			// default action is select object action
			m_DefaultAction = new SelectPathAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			( m_DefaultAction as SelectPathAction ).SelectionChange += OnPathSelectionChange;
			( m_DefaultAction as SelectPathAction ).RemovePath += RemovePath;

			// Initialize renderers
			m_IndexRenderer = new IndexRenderer( m_Viewer, m_DataManager );
			m_OrientationRenderer = new OrientationRenderer( m_Viewer, m_DataManager );
			m_CraftRenderer = new CraftRenderer( m_Viewer, m_DataManager );
			m_ToolVecRenderer = new ToolVecRenderer( m_Viewer, m_DataManager );
			m_TraverseRenderer = new TraverseRenderer( m_Viewer, m_DataManager );
			m_MainPathRenderer = new PathRenderer( m_Viewer, m_DataManager );
			m_MachineRender = MachineRenderFactory.Create( m_Viewer, m_DataManager );
			m_MarkerRenderer = new MarkerRenderer( m_Viewer, m_DataManager );
		}

		public const int PATH_NODE_PREFIX_LENGTH = 5;

		// Renderers
		IndexRenderer m_IndexRenderer;
		OrientationRenderer m_OrientationRenderer;
		CraftRenderer m_CraftRenderer;
		ToolVecRenderer m_ToolVecRenderer;
		TraverseRenderer m_TraverseRenderer;
		PathRenderer m_MainPathRenderer;
		MachineRenderBase m_MachineRender;
		MarkerRenderer m_MarkerRenderer;

		// editor
		public override EEditorType Type
		{
			get
			{
				return EEditorType.CAM;
			}
		}

		public override void EditStart()
		{
			base.EditStart();

			// init tree
			m_TreeView.Nodes.Add( m_ViewManager.PathNode );
			m_ViewManager.PathNode.ExpandAll();
			ShowAllCAMData();
		}

		public override void EditEnd()
		{
			base.EditEnd();

			// clear tree
			m_TreeView.Nodes.Clear();

			RemoveAllCAMData();
		}

		// view API
		public void SetShowToolVec( bool isShowToolVec )
		{
			m_ToolVecRenderer.SetShow( isShowToolVec );
			m_ToolVecRenderer.Show( true );
		}

		public void SetShowOrientation( bool isShowOrientation )
		{
			m_OrientationRenderer.SetShow( isShowOrientation );
			m_OrientationRenderer.Show( true );
		}

		public void SetShowOrder( bool isShowOrder )
		{
			m_IndexRenderer.SetShow( isShowOrder );
			m_IndexRenderer.Show( true );
		}

		public void SetShowTraversePath( bool isShowTraversePath )
		{
			m_TraverseRenderer.SetShow( isShowTraversePath );
			m_TraverseRenderer.Show( true );
		}

		public void SetShowMarker( bool isShowMarker )
		{
			m_MarkerRenderer.SetShow( isShowMarker );
			m_MarkerRenderer.Show( true );
		}

		// add/ remove path API

		public void StartAddPath()
		{
			if( IsSameAction( EditActionType.AddPath ) ) {
				m_CurrentAction?.End();
				return;
			}

			// no workpiece protect
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseAddWorkpiece", PLEASE_ADD_WORKPIECE ), MyApp.NoticeType.Hint );
				return;
			}
			AddPathAction action = new AddPathAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void StartMeasure()
		{
			if( m_CurrentAction.ActionType == EditActionType.Measure ) {
				m_CurrentAction.End();
				return;
			}

			MeasureAction action = new MeasureAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void RemovePath()
		{
			// stop current action
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			int removeCount = szPathIDList.Count;

			foreach( var item in szPathIDList ) {
				string szPathID = item;

				// remove from data manager
				m_DataManager.RemovePath( szPathID );

				// remove from viewer
				m_ViewManager.RemovePath( szPathID );
			}

			// remove path tree nodes from the end
			m_ViewManager.RemovePathNodes( removeCount );

			// clear selection after remove path
			m_DefaultAction.ClearSelection();
			OptimizeIKContinuityData();
			RemovePathCAMData( szPathIDList );
		}

		#region Set CAM

		public void OptimizeIKContinuityData()
		{
			List<string> pathIDList = m_DataManager.PathIDList;
			if( pathIDList.Count == 0 ) {
				return;
			}

			double prevMaster_rad = 0;
			double prevSlave_rad = 0;

			foreach( string szPathID in pathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}
				if( craftData.ProcessPath == false ) {
					craftData.InitMaster_rad = 0;
					craftData.InitSlave_rad = 0;
				}
				else {
					// write previous path end MS as initial value for this path's IK solve
					craftData.InitMaster_rad = prevMaster_rad;
					craftData.InitSlave_rad = prevSlave_rad;

					// trigger cache rebuild
					craftData.CAMFactorChanged?.Invoke();

					// read this path's end MS for next path
					if( CacheHelper.GetMainPathEndMS( szPathID, out double endMaster_rad, out double endSlave_rad ) ) {
						prevMaster_rad = endMaster_rad;
						prevSlave_rad = endSlave_rad;
					}
				}
			}
		}

		public void SetStartPoint()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.StartPoint ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			EndActionIfNotDefault();
			StartPointAction action = new StartPointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList, m_DefaultAction as SelectPathAction );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetReverse()
		{
			// one shot edit, muti edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}

				// toggle reverse state
				craftData.IsPathReverse = !craftData.IsPathReverse;
			}
			ShowCAMData( szPathIDList );
		}

		public void SetOverCut()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.OverCut ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			OverCutAction action = new OverCutAction( m_DataManager, szPathIDList );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetLeadLine()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.SetLead ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			LeadAction action = new LeadAction( m_DataManager, szPathIDList );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetToolVec()
		{
			// action edit, no multi edit supported
			if( IsSameAction( EditActionType.ToolVec ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			EndActionIfNotDefault();
			ToolVectorAction action = new ToolVectorAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList.First(), m_DefaultAction as SelectPathAction );

			// register before init, cause init will change select node to start point, machine need to translate
			action.RaiseTrans += SetTrans;
			action.RaiseActionStart += ToolVecActionStartIO;
			action.NeedOptimizedIK += DoOptimizeIKContinuity;

			// trigger select on star point and translate machine
			StartEditAction( action );
		}

		void ToolVecActionStartIO( bool isStart, bool isUpdatView = true )
		{
			if( isStart ) {
				m_MachineRender.Show();
				m_MainPathRenderer.SetPauseRefreshAndHide( true );
				m_ToolVecRenderer.SetPauseRefreshAndHide( true );
				m_TraverseRenderer.SetPauseRefreshAndHide( true );
			}
			else {
				m_MainPathRenderer.SetPauseRefreshAndHide( false );
				m_ToolVecRenderer.SetPauseRefreshAndHide( false );
				m_TraverseRenderer.SetPauseRefreshAndHide( false );
				m_MachineRender.Remove();
				m_ToolVecRenderer.Reset();
				m_TraverseRenderer.Reset();
				m_OrientationRenderer.Reset();
				m_IndexRenderer.Reset();
				m_MarkerRenderer.Reset();
				m_ViewManager.ShowPathTrsf( new gp_Trsf() );
				m_MainPathRenderer.Reset();
				m_CraftRenderer.Reset();
				SetWorkPieceDisplayTransform( new gp_Trsf() );
				DoOptimizeIKContinuity();
			}
			if( isUpdatView ) {
				m_Viewer.UpdateView();
			}
		}

		void SetWorkPieceDisplayTransform( gp_Trsf trsf = null, bool isUnLockViewerUpdate = true )
		{
			if( trsf == null ) {
				return;
			}

			// backup previous transformation for camera update
			gp_Trsf prevTrsf = null;
			if( m_DataManager.PartIDList.Count > 0 ) {
				prevTrsf = m_ViewManager.ViewObjectMap[ m_DataManager.PartIDList.First() ]?.AISHandle?.LocalTransformation();
			}
			if( prevTrsf == null ) {
				prevTrsf = new gp_Trsf();
			}

			// transform the workpiece display
			m_ViewManager.ShowPartsTrsf( m_DataManager.PartIDList, trsf );

			// apply camera follow
			V3d_View view = m_Viewer.GetView();
			if( view == null ) {
				return;
			}

			// calculate relative transformation (delta)
			// deltaTrsf = newTrsf * oldTrsf^-1
			gp_Trsf inversePrevious = prevTrsf.Inverted();
			gp_Trsf deltaTrsf = trsf.Multiplied( inversePrevious );

			// get current camera parameters
			double eyeX = 0, eyeY = 0, eyeZ = 0;
			double atX = 0, atY = 0, atZ = 0;
			double upX = 0, upY = 0, upZ = 0;

			view.Eye( ref eyeX, ref eyeY, ref eyeZ );
			view.At( ref atX, ref atY, ref atZ );
			view.Up( ref upX, ref upY, ref upZ );

			// apply only the delta transformation to camera
			gp_Pnt oldEye = new gp_Pnt( eyeX, eyeY, eyeZ );
			gp_Pnt oldAt = new gp_Pnt( atX, atY, atZ );
			gp_Dir oldUp = new gp_Dir( upX, upY, upZ );

			gp_Pnt newEye = oldEye.Transformed( deltaTrsf );
			gp_Pnt newAt = oldAt.Transformed( deltaTrsf );
			gp_Dir newUp = oldUp.Transformed( deltaTrsf );

			// disable immediate view update to prevent flickering
			bool previousImmediateUpdate = view.SetImmediateUpdate( false );

			// apply new camera parameters
			view.SetEye( newEye.X(), newEye.Y(), newEye.Z() );
			view.SetAt( newAt.X(), newAt.Y(), newAt.Z() );
			view.SetUp( newUp.X(), newUp.Y(), newUp.Z() );

			if( isUnLockViewerUpdate ) {
				// restore immediate update and update view once
				view.SetImmediateUpdate( previousImmediateUpdate );
			}
		}

		public void SetToolVecReverse()
		{
			// one shot edit, multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}

				// toggle reverse state
				craftData.SetToolVecReverse( !craftData.IsToolVecReverse );
			}
			ShowCAMData( szPathIDList );
		}

		public void SetFixedToolVec2()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.SpecialToolVecEdit ) ) {
				m_CurrentAction.End();
				return;
			}
			EndActionIfNotDefault();

			// an empty selection is allowed, the action asks the user to pick a path by itself
			List<string> szPathIDList = GetSelectedIDList();
			LockABAction action = new LockABAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList, m_DefaultAction as SelectPathAction );
			action.RaiseTrans += SetTrans;
			action.RaiseActionStart += ToolVecActionStartIO;
			action.HideMachine += HideMachine;
			action.ResetPartUI += ResetPartDisplay;
			action.NeedOptimizedIK += DoOptimizeIKContinuity;
			// trigger translate machine
			StartEditAction( action );
		}

		public void SetTraverseData()
		{
			// action edit, multi edit supported
			if( IsSameAction( EditActionType.SetTraverse ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			TraverseAction action = new TraverseAction( m_DataManager, szPathIDList, m_Viewer );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetEntryAndExitParam()
		{
			// action edit, not depends on path
			if( IsSameAction( EditActionType.SetEntryAndExitParam ) ) {
				m_CurrentAction.End();
				return;
			}
			EntryAndExitAction action = new EntryAndExitAction( m_DataManager );

			// false means no need to refresh tree, because set entry dose not need to change tree node color
			action.PropertyChanged += () => ShowAllCAMData( false );
			StartEditAction( action );
		}

		public void SetStandardPattern()
		{
			if( IsSameAction( EditActionType.SetPattern ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			PatternAction action = new PatternAction( m_DataManager, m_Viewer, m_ViewManager, szPathIDList );
			action.PropertyChanged += OnPathShapeTypeChange;
			StartEditAction( action );
		}

		public void SetPathEdit()
		{
			if( IsSameAction( EditActionType.PathEdit ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}

			PathEditAction action = new PathEditAction( m_DataManager, szPathIDList, m_Viewer );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetPathCompensation()
		{
			if( IsSameAction( EditActionType.PathCompensation ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			PathCompensationAction action = new PathCompensationAction( m_DataManager, szPathIDList );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetContourEdit()
		{
			// action edit, no multi edit supported
			if( IsSameAction( EditActionType.ContourEdit ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			string szPathID = szPathIDList[ 0 ];
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType )
				|| pathType != PathType.Contour ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "OnlyContourPathSupported", ONLY_CONTOUR_PATH_SUPPORTED ), MyApp.NoticeType.Hint );
				return;
			}
			EndActionIfNotDefault();
			ContourEditAction action = new ContourEditAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathID, m_DefaultAction as SelectPathAction );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetProcessPath( bool isNeedProcess )
		{
			// one shot edit, multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.ProcessPath = isNeedProcess;
			}

			// will trigger showallcamdata, but tree node color dosen't change
			AutoOptimizeIKContinuity();

			// update tree node color to reflect layer change
			UpdatePathTreeNodeColors( szPathIDList );
		}

		public void SetPathLayer( int nTechLayer )
		{
			// one shot edit, multi edit supported
			if( !ValidateBeforeOneShotEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
					continue;
				}
				if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
					continue;
				}
				craftData.TechLayer = nTechLayer;
			}

			m_ViewManager.UpdatePaths( szPathIDList );
			m_MainPathRenderer.Show( szPathIDList );
		}

		public void SetCalibrationREF()
		{
			if( m_DataManager.PartIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseAddWorkpiece", PLEASE_ADD_WORKPIECE ), MyApp.NoticeType.Hint );
				return;
			}

			if( IsSameAction( EditActionType.CalibrationREF ) ) {
				m_CurrentAction.End();
				return;
			}
			CalibrationREFAction action = new CalibrationREFAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager );
			StartEditAction( action );
		}

		public void SetMicroJoint()
		{
			if( IsSameAction( EditActionType.MicroJoint ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false ) ) {
				return;
			}
			EndActionIfNotDefault();
			MicroJointAction action = new MicroJointAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList.First(), m_DefaultAction as SelectPathAction );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetExtendedGCode()
		{
			if( IsSameAction( EditActionType.ExtendedGCode ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, true ) ) {
				return;
			}
			EndActionIfNotDefault();
			ExtendedGCodeAction action = new ExtendedGCodeAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList, m_DefaultAction as SelectPathAction );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		public void SetRCorner()
		{
			if( IsSameAction( EditActionType.RCorner ) ) {
				m_CurrentAction.End();
				return;
			}
			if( !ValidateBeforeActionEdit( out List<string> szPathIDList, false, true ) ) {
				return;
			}
			EndActionIfNotDefault();
			RCornerAction action = new RCornerAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, szPathIDList.FirstOrDefault(), m_DefaultAction as SelectPathAction );
			action.PropertyChanged += ShowCAMData;
			StartEditAction( action );
		}

		#endregion

		// sort API
		public void SetManualOrder()
		{
			if( IsSameAction( EditActionType.ManualOrder ) ) {
				m_CurrentAction.End();
				return;
			}
			EndActionIfNotDefault();
			ManualOrderAction action = new ManualOrderAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, m_DefaultAction as SelectPathAction );

			// change order will cause tree node color change, so need to refresh tree
			action.ShowOrderData += () => ShowAllCAMData( true );
			action.RaiseActionStart += ManualStartIO;
			action.NeedDoOptimized += DoOptimizeIKContinuity;

			StartEditAction( action );
		}

		public void AutoSortProcess()
		{
			if( IsSameAction( EditActionType.AutoSortPath ) ) {
				m_CurrentAction.End();
				return;
			}

			EndActionIfNotDefault();

			AutoSortPathAction action = new AutoSortPathAction( m_DataManager, m_DefaultAction as SelectPathAction );
			action.SortCompleted += ( newPathIDList ) =>
			{
				// apply new order
				m_DataManager.PathIDList.Clear();
				m_DataManager.PathIDList.AddRange( newPathIDList );

				// optimize IK continuity after sorting
				DoOptimizeIKContinuity();

				// reset select to first path after sort, to avoid confusion, and also trigger select related data refresh
				if( m_DefaultAction is SelectPathAction selectAction ) {
					selectAction.ClearSelection();
					selectAction.SelectPathByID( m_DataManager.PathIDList.First() );
				}
			};

			StartEditAction( action );
		}

		public void AutoOptimizeIKContinuity()
		{
			// one-shot action, no path selection required
			EndActionIfNotDefault();
			DoOptimizeIKContinuity();
		}

		// convert NC
		public void ConverNC()
		{
			// stop current action
			EndActionIfNotDefault();
			const string NC_FILE_DEFAULT_NAME = "FiveAxisProgram.nc";
			using( SaveFileDialog saveFileDialog = new SaveFileDialog() ) {
				saveFileDialog.Filter = "NC Files (*.nc)|*.nc|All Files (*.*)|*.*";
				saveFileDialog.DefaultExt = "nc";
				saveFileDialog.FileName = NC_FILE_DEFAULT_NAME;
				saveFileDialog.Title = m_LanguageManager.TryGetString( "SaveNCFileTitle", SAVE_NC_FILE_TITLE );
				saveFileDialog.RestoreDirectory = true;

				if( saveFileDialog.ShowDialog( MyApp.MainForm ) != DialogResult.OK ) {
					return;
				}

				string szSaveFilePath = saveFileDialog.FileName;
				string szFileName = Path.GetFileName( szSaveFilePath );
				string szTempFilePath = Path.Combine( Path.GetTempPath(), NC_FILE_DEFAULT_NAME );
				NCWriter writer = new NCWriter( m_DataManager );
				bool bNCCreateSuccess = writer.ConvertSuccess( szFileName, szTempFilePath, out string szCreateErrorMessage );

				if( !bNCCreateSuccess ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NCExportFailedAndTransferFailed", NC_EXPORT_FAILED_AND_TRANSFER_FAILED ), szCreateErrorMessage ), MyApp.NoticeType.Error );

					if( File.Exists( szTempFilePath ) ) {
						try {
							File.Delete( szTempFilePath );
						}
						catch {

							// this would not happen often, but still need to handle
							MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "TempFileDeleteFailed", TEMP_FILE_DELETE_FAILED ), szTempFilePath ), MyApp.NoticeType.Warning );
						}
					}
					return;
				}

				try {
					File.Copy( szTempFilePath, szSaveFilePath, true );
					File.Delete( szTempFilePath );
				}
				catch( Exception ex ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NCFileSaveFailed", NC_FILE_SAVE_FAILED ), ex.Message ), MyApp.NoticeType.Error );
					if( File.Exists( szTempFilePath ) ) {
						try {
							File.Delete( szTempFilePath );
						}
						catch {

							// this would not happen often, but still need to handle
							MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "TempFileDeleteFailed", TEMP_FILE_DELETE_FAILED ), szTempFilePath ), MyApp.NoticeType.Warning );
						}
					}
					return;
				}

				// transmit file
				if( string.IsNullOrWhiteSpace( MyApp.CNCIP ) ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NCExportSuccessNoController", NC_EXPORT_SUCCESS_NO_CONTROLLER ), MyApp.NoticeType.Warning, true );
				}
				else {
					bool bTransmitSuccess = FTPTransmission.FileTransmit( szSaveFilePath, MyApp.CNCIP, szFileName, out string szTransmitErrorMessage );

					if( bTransmitSuccess ) {
						MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "NCExportAndTransferSuccess", NC_EXPORT_AND_TRANSFER_SUCCESS ), MyApp.NoticeType.Hint, true );
					}
					else {
						MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NCExportSuccessTransferFailed", NC_EXPORT_SUCCESS_TRANSFER_FAILED ), szTransmitErrorMessage ), MyApp.NoticeType.Warning, true );
					}
				}
			}
		}

		// path added
		void OnPathAdded( List<string> newPathIDs )
		{
			foreach( string szID in newPathIDs ) {
				if( string.IsNullOrEmpty( szID ) ) {
					continue;
				}

				// add a new node to the tree view
				int pathOrderIndex = m_DataManager.PathIDList.IndexOf( szID );
				if( pathOrderIndex == -1 ) {
					continue;
				}
				string nodeID = m_DataManager.GetPathName( pathOrderIndex );
				m_ViewManager.AddPathNode( nodeID, szID );
			}

			// update tree view and viewer
			m_ViewManager.PathNode.ExpandAll();
			ShowAllCAMData();
			m_Viewer.UpdateView();
		}

		// selection changed
		void OnPathSelectionChange()
		{
			if( m_CurrentAction.ActionType != EditActionType.SelectObject ) {
				return;
			}
			List<string> szPathIDList = GetSelectedIDList();
			EditableInfo editableInfo = new EditableInfo();

			// no multi edit: start point, tool vec, sort
			if( szPathIDList.Count > 1 ) {
				editableInfo.IsToolVecEditable = false;
				editableInfo.IsMicroJointEditable = false;
				editableInfo.IsRCornerEditable = false;
				editableInfo.IsContourEditable = false;
				editableInfo.IsAutoOrderEditable = false;
			}

			// closed path editable only: start point, overcut, lead line, change lead dir
			foreach( string szPathID in szPathIDList ) {
				if( !DataGettingHelper.GetGeomDataByID( szPathID, out IGeomData geomData ) ) {
					continue;
				}
				if( !geomData.IsClosed ) {
					editableInfo.IsStartPointEditable = false;
					editableInfo.IsOverCutEditable = false;
					editableInfo.IsLeadLineEditable = false;
					break;
				}

				if( geomData.PathType != PathType.Contour ) {
					editableInfo.IsPathReverseEditable = false;
					editableInfo.IsToolVecEditable = false;
					editableInfo.IsToolVecReverseEditable = false;
					editableInfo.IsFixedToolVecEditable = false;
					editableInfo.IsMicroJointEditable = false;
					editableInfo.IsExtendedGCodeEditable = false;
					editableInfo.IsRCornerEditable = false;
					editableInfo.IsContourEditable = false;
				}
				if( IsSelectSpecialPath( szPathIDList ) ) {
					editableInfo.IsToolVecEditable = false;
				}
			}
			PathPropertyChanged?.Invoke( editableInfo );
		}

		bool IsSelectSpecialPath( List<string> szPathIDList )
		{
			// only valid when the current action is SelectPathAction
			if( !( m_CurrentAction is SelectPathAction ) ) {
				return false;
			}
			return szPathIDList.Any( SelectPathAction.IsSpecialPath );
		}

		void OnPathShapeTypeChange( PathType type, List<string> szPathIDList )
		{
			ShowCAMData( szPathIDList );
			PathShapeTypeChanged?.Invoke( type );
		}

		void DoOptimizeIKContinuity()
		{
			if( m_DataManager.PathIDList.Count == 0 ) {
				return;
			}
			OptimizeIKContinuityData();
			ShowAllCAMData();
			MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CAMEditor_OptimizeIKContinuityCompleted", OPTIMIZE_IK_CONTINUITY_COMPLETED ), MyApp.NoticeType.Hint );
		}

		#region Show CAM

		void ShowAllCAMData( bool needUpdateTreeColor = false )
		{
			// take all path IDs
			List<string> pathIDList = m_DataManager.PathIDList;
			ShowCAMData( pathIDList );
			if( needUpdateTreeColor ) {
				UpdatePathTreeNodeColors( pathIDList );
			}
		}

		void ShowCAMData( List<string> pathIDList )
		{
			m_ViewManager.UpdatePaths( pathIDList );
			m_MainPathRenderer.Show( pathIDList );
			m_MarkerRenderer.Show( pathIDList );
			m_ToolVecRenderer.Show( pathIDList );
			m_OrientationRenderer.Show( pathIDList );
			m_IndexRenderer.Show();
			m_CraftRenderer.Show( pathIDList );
			m_TraverseRenderer.Show( pathIDList );
			m_Viewer.UpdateView();
		}

		void ManualStartIO( bool isStart )
		{
			if( isStart ) {
				m_ToolVecRenderer.SetPauseRefreshAndHide( true );
				m_MainPathRenderer.SetPauseRefresh( true );
				m_TraverseRenderer.SetPauseRefreshAndHide( true );
				m_CraftRenderer.SetPauseRefreshAndHide( true );
				m_OrientationRenderer.SetPauseRefreshAndHide( true );
				m_MarkerRenderer.SetPauseRefreshAndHide( true );
			}
			else {

				// re-display all data
				m_ToolVecRenderer.SetPauseRefreshAndHide( false );
				m_MainPathRenderer.SetPauseRefresh( false );
				m_TraverseRenderer.SetPauseRefreshAndHide( false );
				m_CraftRenderer.SetPauseRefreshAndHide( false );
				m_OrientationRenderer.SetPauseRefreshAndHide( false );
				m_MarkerRenderer.SetPauseRefreshAndHide( false );

				// traverse will change during order change
				m_TraverseRenderer.Show();
			}
			m_Viewer.UpdateView();
		}

		void UpdatePathTreeNodeColors( List<string> pathIDList )
		{
			if( pathIDList == null ) {
				return;
			}
			foreach( string pathID in pathIDList ) {
				int pathOrderIndex = m_DataManager.PathIDList.IndexOf( pathID );
				if( pathOrderIndex == -1 ) {
					continue;
				}
				string nodeID = m_DataManager.GetPathName( pathOrderIndex );
				bool isNoneProcess = DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) &&
					craftData != null && craftData.ProcessPath == false;
				m_ViewManager.UpdatePathNodeForeColor( nodeID, isNoneProcess );
			}
		}

		void ShowTransedCAMData( gp_Trsf trsf )
		{
			// draw with translated location
			m_ViewManager.ShowPathTrsf( trsf );
			m_MainPathRenderer.ShowTrans( trsf );
			m_ToolVecRenderer.ShowTrans( trsf );
			m_IndexRenderer.ShowTrans( trsf );

			// do not need to redraw ,just need trsf
			m_OrientationRenderer.Trans( trsf );
			m_CraftRenderer.Trans( trsf );
			m_TraverseRenderer.Trans( trsf );
			m_MarkerRenderer.Trans( trsf );
		}

		void SetTrans( Dictionary<MachineComponentType, List<gp_Trsf>> transMap, bool isUnLockViewerUpdate = true )
		{
			// update camera FIRST before setting any transformations
			gp_Trsf workPieceTrsf = transMap[ MachineComponentType.WorkPiece ].Last();

			// show
			m_MachineRender.ShowToolVecEditResult( transMap );
			ShowTransedCAMData( workPieceTrsf );
			SetWorkPieceDisplayTransform( workPieceTrsf, isUnLockViewerUpdate );
			m_Viewer.UpdateView();
		}

		void HideMachine()
		{
			m_MachineRender.Remove();
			m_Viewer.UpdateView();
		}

		// reset Part display (path / workpiece) back to un-transformed state without touching machine render
		void ResetPartDisplay()
		{
			gp_Trsf identity = new gp_Trsf();
			ShowTransedCAMData( identity );
			SetWorkPieceDisplayTransform( identity, true );
		}

		void RemoveAllCAMData()
		{
			List<string> pathIDList = m_DataManager.PathIDList;
			RemoveCAMData( pathIDList );
		}

		void RemoveCAMData( List<string> pathIDList )
		{
			m_MainPathRenderer.Remove();
			m_MarkerRenderer.Remove();
			m_ToolVecRenderer.Remove( pathIDList );
			m_CraftRenderer.Remove( pathIDList );
			m_OrientationRenderer.Remove( pathIDList );
			m_IndexRenderer.Remove();
			m_TraverseRenderer.Remove( pathIDList );
			m_Viewer.UpdateView();
		}

		void RemovePathCAMData( List<string> pathIDList )
		{
			m_MainPathRenderer.Remove( pathIDList );
			m_MarkerRenderer.Remove( pathIDList );
			m_ToolVecRenderer.Remove( pathIDList );
			m_CraftRenderer.Remove( pathIDList );
			m_OrientationRenderer.Remove( pathIDList );
			m_TraverseRenderer.Remove( pathIDList );

			// those info are not recorded on each path, need to refresh all not remove
			m_IndexRenderer.Show();
			m_TraverseRenderer.Show();
			m_Viewer.UpdateView();
		}

		#endregion

		// methods
		List<string> GetSelectedIDList()
		{
			return m_DefaultAction.GetSelectedIDs();
		}

		bool IsSameAction( EditActionType newActionType )
		{
			if( m_CurrentAction != null && m_CurrentAction.ActionType == newActionType ) {
				return true;
			}
			return false;
		}

		bool ValidateBeforeActionEdit( out List<string> szIDList, bool isMutiEditable, bool isNoSelectable = false )
		{
			szIDList = GetSelectedIDList();

			// nothing selected
			if( isNoSelectable == false && szIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseSelectPath", PLEASE_SELECT_PATH ), MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return false;
			}

			// multiple selected but not allowed
			else if( isMutiEditable == false && szIDList.Count > 1 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "OnlySinglePathEdit", ONLY_SINGLE_PATH_EDIT ), MyApp.NoticeType.Hint );
				EndActionIfNotDefault();
				return false;
			}
			return true;
		}

		bool ValidateBeforeOneShotEdit( out List<string> szIDList, bool isMutiEditable )
		{
			EndActionIfNotDefault();
			szIDList = GetSelectedIDList();

			// nothing selected
			if( szIDList.Count == 0 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "PleaseSelectPath", PLEASE_SELECT_PATH ), MyApp.NoticeType.Hint );
				return false;
			}

			// multiple selected but not allowed
			else if( isMutiEditable == false && szIDList.Count > 1 ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "OnlySinglePathEdit", ONLY_SINGLE_PATH_EDIT ), MyApp.NoticeType.Hint );
				return false;
			}
			return true;
		}

		// edit actions
		protected override void OnEditActionStart( IEditorAction action )
		{
			base.OnEditActionStart( action );
			if( action.ActionType == EditActionType.OverCut
				|| action.ActionType == EditActionType.SetLead
				|| action.ActionType == EditActionType.SetTraverse
				|| action.ActionType == EditActionType.SetPattern
				|| action.ActionType == EditActionType.PathEdit
				|| action.ActionType == EditActionType.ToolVec
				|| action.ActionType == EditActionType.MicroJoint
				|| action.ActionType == EditActionType.ExtendedGCode
				|| action.ActionType == EditActionType.RCorner
				|| action.ActionType == EditActionType.ContourEdit ) {

				// lock main form
				RaiseWithDlgActionStatusChange?.Invoke( EActionStatus.Start );
			}

			if( action.ActionType == EditActionType.OverCut
				|| action.ActionType == EditActionType.SetLead
				|| action.ActionType == EditActionType.SetTraverse
				|| action.ActionType == EditActionType.SetPattern
				|| action.ActionType == EditActionType.PathEdit ) {

				// for the dlg edit which not support chaging path so far
				m_TreeView.Enabled = false;
			}

			// change display
			RaiseCAMActionStatusChange( action.ActionType, EActionStatus.Start );
		}

		protected override void OnEditActionEnd( IEditorAction action )
		{
			// these action will show dialog, need to lock ui
			if( action.ActionType == EditActionType.OverCut
				|| action.ActionType == EditActionType.SetLead
				|| action.ActionType == EditActionType.SetTraverse
				|| action.ActionType == EditActionType.SetPattern
				|| action.ActionType == EditActionType.PathEdit
				|| action.ActionType == EditActionType.ToolVec
				|| action.ActionType == EditActionType.MicroJoint
				|| action.ActionType == EditActionType.ExtendedGCode
				|| action.ActionType == EditActionType.RCorner
				|| action.ActionType == EditActionType.ContourEdit ) {

				// unlock main form
				RaiseWithDlgActionStatusChange?.Invoke( EActionStatus.End );
			}

			if( action.ActionType == EditActionType.OverCut
				|| action.ActionType == EditActionType.SetLead
				|| action.ActionType == EditActionType.SetTraverse
				|| action.ActionType == EditActionType.SetPattern
				|| action.ActionType == EditActionType.PathEdit ) {

				// for the dlg edit which not support chaging path so far
				m_TreeView.Enabled = true;
			}

			RaiseCAMActionStatusChange?.Invoke( action.ActionType, EActionStatus.End );
			base.OnEditActionEnd( action );
		}

		const string PLEASE_ADD_WORKPIECE = "[操作提醒]請先新增工件";
		const string PLEASE_SELECT_PATH = "[操作提醒]請先選擇路徑";
		const string ONLY_SINGLE_PATH_EDIT = "[操作提醒]只能選擇一個路徑進行編輯";
		const string ONLY_CONTOUR_PATH_SUPPORTED = "[操作提醒]此功能僅支援輪廓路徑";
		const string NC_EXPORT_FAILED_AND_TRANSFER_FAILED = "[NC轉出狀態]轉出NC失敗: {0}\n[檔案傳輸狀態]傳輸NC檔案失敗";
		const string TEMP_FILE_DELETE_FAILED = "[清理臨時檔案] 刪除失敗: {0}";
		const string NC_FILE_SAVE_FAILED = "[NC轉出狀態]儲存檔案失敗: {0}";
		const string NC_EXPORT_SUCCESS_NO_CONTROLLER = "[NC轉出狀態]成功轉出NC\n[檔案傳輸狀態]未與控制器進行連線，檔案無法傳輸";
		const string NC_EXPORT_AND_TRANSFER_SUCCESS = "[NC轉出狀態]成功轉出NC\n[檔案傳輸狀態]傳輸NC檔案成功";
		const string NC_EXPORT_SUCCESS_TRANSFER_FAILED = "[NC轉出狀態]成功轉出NC\n[檔案傳輸狀態]傳輸NC檔案失敗：{0}";
		const string OPTIMIZE_IK_CONTINUITY_COMPLETED = "[操作完成]已完成起點旋轉軸自動優化";
		const string SAVE_NC_FILE_TITLE = "儲存 NC 檔案";
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "CAMEditor" );
	}
}
