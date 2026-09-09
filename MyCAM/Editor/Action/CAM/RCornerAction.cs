using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.Aspect;
using OCC.Prs3d;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class RCornerAction : EditActionBase
	{
		public RCornerAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_CurrentPathID = pathID;
			m_PathIndexAction = pathIndexAction;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.RCorner;
			}
		}

		public Action<List<string>> PropertyChanged;

		public override void Start()
		{
			base.Start();

			// setup initial path, may be empty; user can select a path after entering
			bool hasInitialPath = !string.IsNullOrEmpty( m_CurrentPathID );
			if( hasInitialPath && !TryChangePath( m_CurrentPathID ) ) {
				End();
				return;
			}

			// create dialog
			m_Dlg = new RCornerDlg();
			m_Dlg.Cancel += End;
			m_Dlg.DeleteRequest += OnDeleteRequest;
			m_Dlg.SwitchStartOrEndRequest += OnSwitchStartOrEndRequest;
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;
			m_PathIndexAction.EnterSingleSelectMode();

			// disable delete-key path removal while RCorner editing is active
			m_OriginalRemovePathHandler = m_PathIndexAction.RemovePath;
			m_PathIndexAction.RemovePath = null;

			if( hasInitialPath ) {
				// activate point selection for initial path
				ActivatePointSelection();

				// create and show the R corner segment renderer
				m_RCornerRenderer = new RCornerRenderer( m_Viewer, m_DataManager );
				m_RCornerRenderer.Show( new List<string>() { m_CurrentPathID }, true );
			}

			UpdateDialogState();
		}

		public override void End()
		{
			// restore delete-key path removal handler
			m_PathIndexAction.RemovePath = m_OriginalRemovePathHandler;

			// remove the R corner segment renderer
			if( m_RCornerRenderer != null ) {
				m_RCornerRenderer.Remove( true );
				m_RCornerRenderer = null;
			}

			m_PathIndexAction.ExitSingleSelectMode();
			DeactivatePointSelection( m_CurrentPathID );

			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			// close dialog
			if( m_Dlg != null && !m_Dlg.IsDisposed ) {
				m_Dlg.Cancel -= End;
				m_Dlg.DeleteRequest -= OnDeleteRequest;
				m_Dlg.SwitchStartOrEndRequest -= OnSwitchStartOrEndRequest;
				m_Dlg.Close();
				m_Dlg = null;
			}

			base.End();
		}

		#region Path Selection

		string m_CurrentPathID = null;
		SelectPathAction m_PathIndexAction = null;
		Action m_OriginalRemovePathHandler = null;

		const string NOT_CONTOUR_PATH = "路徑 {0} 不是輪廓路徑，無法編輯";
		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";

		void OnPathIndexChanged()
		{
			List<string> selectedIDs = m_PathIndexAction.GetSelectedIDs();
			if( selectedIDs.Count != 1 ) {
				return;
			}
			string newPathID = selectedIDs.First();
			if( newPathID == m_CurrentPathID ) {
				return;
			}

			// keep the old path ID and selection for rollback before TryChangePath updates the current path
			string szOldPathID = m_CurrentPathID;
			List<string> szRollBackIDs = new List<string>( selectedIDs );
			if( !TryChangePath( newPathID ) ) {

				// failed: stay on the old path and roll back the selection
				ResetPathSelection( szRollBackIDs );
				return;
			}
			bool hasOldPath = !string.IsNullOrEmpty( szOldPathID );

			// cleanup old state
			if( hasOldPath ) {
				DeactivatePointSelection( szOldPathID );
			}

			// activate point selection for new path
			ActivatePointSelection();

			// create or refresh the segment renderer for the new path
			if( m_RCornerRenderer == null ) {
				m_RCornerRenderer = new RCornerRenderer( m_Viewer, m_DataManager );
			}
			if( hasOldPath ) {
				m_RCornerRenderer.Remove( new List<string>() { szOldPathID } );
			}
			m_RCornerRenderer.Show( new List<string>() { m_CurrentPathID }, true );

			UpdateDialogState();
		}

		bool TryChangePath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}

			// validate new path, contour only
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NotContourPath", NOT_CONTOUR_PATH ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			// init craft data for current path
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			// init contour cache for current path
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache pathCache ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			// update all path state only after validation succeeds
			m_CraftData = craftData;
			m_PathCache = pathCache;
			m_CurrentPathID = pathID;
			return true;
		}

		// change path failed: keep the old path but drop all selection
		void ResetPathSelection( List<string> rollBackIDs )
		{
			if( string.IsNullOrEmpty( m_CurrentPathID ) ) {
				return;
			}

			// clear point selection on the old path
			ResetPointSelection();
			UpdateDialogState();

			// clear path selection (tree + viewer highlight)
			m_PathIndexAction.ClearSelection();

			// restore the path selectability; keep the roll back IDs untouched
			DeactivatePointSelection( m_CurrentPathID );
			ActivatePointSelection();
		}

		#endregion

		#region Point Selection

		IndexSelectAction m_PointIndexSelectAction;
		int? m_FirstSelectedCADIndex = null;
		RCornerData m_SelectedRange = null;
		AIS_Shape m_SelectedVertexHighlight = null;
		RCornerRenderer m_RCornerRenderer = null;

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			m_FirstSelectedCADIndex = null;
			m_SelectedRange = null;
		}

		void ResetPointSelection()
		{
			m_FirstSelectedCADIndex = null;
			m_SelectedRange = null;
			HideSelectedVertexHighlight();
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexSelectAction = new IndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexSelectAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexSelectAction.Start();
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			ResetPointSelection();

			// cleanup point index action
			if( m_PointIndexSelectAction != null ) {
				m_PointIndexSelectAction.IndexChanged -= OnPointIndexChanged;
				m_PointIndexSelectAction.End();
				m_PointIndexSelectAction = null;
			}

			m_PathIndexAction.RestoreFromExclusion( szOldPathID );
		}

		void OnPointIndexChanged( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			// no valid path data yet; nothing to do
			if( m_CraftData == null ) {
				return;
			}
			m_Viewer.GetAISContext().ClearSelected( true );

			if( m_FirstSelectedCADIndex == null ) {
				// before click: no point is selected; select an existing range or start a new point
				SelectRangeOrSelectFirstPoint( nSelectIndex, selectedVertex );
			}
			else {
				// before click: the first point is selected; add a range or cancel the selected point
				AddRangeOrCancelFirstPoint( nSelectIndex );
			}
			UpdateDialogState();
			m_Viewer.UpdateView();
		}

		void SelectRangeOrSelectFirstPoint( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			// check if the clicked point is inside an existing range or on the boundary of a range
			m_PathCache.TryGetRCornerSelectionInfo(
				nSelectIndex,
				out RCornerData rangeContainingPoint,
				out RCornerData rangeWithPointAtBoundary );
			bool isOnExistingRCornerRange = rangeContainingPoint != null;
			bool isOnExistingRCornerBoundary = rangeWithPointAtBoundary != null;

			// middle of an existing range: pure select (for delete)
			if( isOnExistingRCornerRange && !isOnExistingRCornerBoundary ) {
				m_SelectedRange = rangeContainingPoint;
				m_FirstSelectedCADIndex = null;
				ShowSelectedVertexHighlight( selectedVertex );
				return;
			}

			// end point of a range (select it and use it as the first point), or a blank point (select it as the first point)
			m_SelectedRange = isOnExistingRCornerBoundary ? rangeWithPointAtBoundary : null;
			m_FirstSelectedCADIndex = nSelectIndex;
			ShowSelectedVertexHighlight( selectedVertex );
		}

		void AddRangeOrCancelFirstPoint( int nSelectIndex )
		{
			int firstIndex = m_FirstSelectedCADIndex.Value;
			int secondIndex = nSelectIndex;

			// click the same first point again: cancel the first point selection
			if( firstIndex == secondIndex ) {
				ResetPointSelection();
				return;
			}

			bool isClosedPath = IsClosedPath();
			RCornerData newRCornerRange = BuildRCornerRange( firstIndex, secondIndex, isClosedPath );
			if( !m_PathCache.TryBuildAddRCornerChange( newRCornerRange,
				out List<RCornerData> rangesToRemove, out RCornerData rangeToAdd )
				|| !m_CraftData.ApplyRCornerChange( rangesToRemove, rangeToAdd ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "RCornerAddFail", RCORNER_ADD_FAIL ), MyApp.NoticeType.Warning );
			}
			FinishAddPoint();
		}

		RCornerData BuildRCornerRange( int firstIndex, int secondIndex, bool isClosedPath )
		{
			bool firstIsClosedPathStartOrEndPoint = isClosedPath && IsClosedPathStartOrEndPoint( firstIndex );
			bool secondIsClosedPathStartOrEndPoint = isClosedPath && IsClosedPathStartOrEndPoint( secondIndex );

			// one of the points is the start or end point of a closed path
			if( firstIsClosedPathStartOrEndPoint && !secondIsClosedPathStartOrEndPoint ) {
				return BuildPathStartOrEndPointRange( otherIndex: secondIndex );
			}
			if( secondIsClosedPathStartOrEndPoint && !firstIsClosedPathStartOrEndPoint ) {
				return BuildPathStartOrEndPointRange( otherIndex: firstIndex );
			}

			// both are the closed path start/end point: whole path
			if( firstIsClosedPathStartOrEndPoint && secondIsClosedPathStartOrEndPoint ) {
				return new RCornerData( GetPathStartCADIndex(), SpecialPointIndex.CLOSED_POINT_INDEX );
			}

			// both normal: store real CAD indices; ordering is resolved in CAM space
			return new RCornerData( firstIndex, secondIndex );
		}

		// build a range that touches the start or end point of a closed path
		RCornerData BuildPathStartOrEndPointRange( int otherIndex )
		{
			RCornerData asStart = new RCornerData( GetPathStartCADIndex(), otherIndex );
			RCornerData asEnd = new RCornerData( otherIndex, SpecialPointIndex.CLOSED_POINT_INDEX );

			if( m_PathCache.CanAddRCorner( asStart ) ) {
				return asStart;
			}
			return asEnd;
		}

		// the CAD index of the current machining start point (start-point-change aware)
		int GetPathStartCADIndex()
		{
			if( m_PathCache == null || m_PathCache.MainPathPointList.Count == 0 ) {
				return 0;
			}
			return m_PathCache.MainPathPointList.First().InitPathIndex;
		}

		void FinishAddPoint()
		{
			ResetPointSelection();
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			RefreshSegmentRenderer();
		}

		bool IsClosedPath()
		{
			if( m_PathCache == null || m_PathCache.MainPathPointList.Count == 0 ) {
				return false;
			}
			return SpecialPointIndex.IsClosedPointIndex( m_PathCache.MainPathPointList.Last().InitPathIndex );
		}

		// the start or end point is the CAD point whose index equals the first main path point's init index on a closed path
		bool IsClosedPathStartOrEndPoint( int nCADIndex )
		{
			if( m_PathCache == null || m_PathCache.MainPathPointList.Count == 0 ) {
				return false;
			}
			return nCADIndex == m_PathCache.MainPathPointList.First().InitPathIndex;
		}

		// maintain highlight the selected vertex with a green ball
		void ShowSelectedVertexHighlight( TopoDS_Shape selectedVertex )
		{
			// clear previous highlight
			HideSelectedVertexHighlight();

			if( selectedVertex == null || selectedVertex.IsNull() ) {
				return;
			}

			// highlight the selected vertex with a green ball
			m_SelectedVertexHighlight = new AIS_Shape( selectedVertex );
			Prs3d_PointAspect pointAspect = new Prs3d_PointAspect(
				Aspect_TypeOfMarker.Aspect_TOM_BALL,
				new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ),
				3.0f
			);
			m_SelectedVertexHighlight.Attributes().SetPointAspect( pointAspect );

			m_Viewer.GetAISContext().Display( m_SelectedVertexHighlight, false );
		}

		void HideSelectedVertexHighlight()
		{
			if( m_SelectedVertexHighlight != null ) {
				m_Viewer.GetAISContext().Remove( m_SelectedVertexHighlight, false );
				m_SelectedVertexHighlight = null;
			}
		}

		void RefreshSegmentRenderer()
		{
			if( m_RCornerRenderer != null && !string.IsNullOrEmpty( m_CurrentPathID ) ) {
				m_RCornerRenderer.Show( new List<string>() { m_CurrentPathID }, true );
			}
		}

		#endregion

		#region Dialog Handling

		RCornerDlg m_Dlg = null;

		const string RCORNER_ADD_FAIL = "無法新增：與現有區段重疊";
		const string STATUS_SELECT_MODE = "點選端點新增，點區段中間選取";
		const string STATUS_ADD_SECOND = "請點選第二點";
		const string STATUS_SELECT_PATH = "請先選擇一條輪廓路徑";

		void OnDeleteRequest()
		{
			if( m_SelectedRange == null || m_CraftData == null ) {
				return;
			}
			if( m_CraftData.RemoveRCorner( m_SelectedRange ) ) {
				ResetPointSelection();
				PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
				RefreshSegmentRenderer();
			}
			UpdateDialogState();
		}

		void OnSwitchStartOrEndRequest()
		{
			if( m_SelectedRange == null || m_CraftData == null ) {
				return;
			}

			// build and apply the switch atomically; the old range is kept if validation fails
			if( !m_PathCache.TryBuildSwitchRCornerChange( m_SelectedRange, GetPathStartCADIndex(),
				out List<RCornerData> rangesToRemove, out RCornerData rangeToAdd )
				|| !m_CraftData.ApplyRCornerChange( rangesToRemove, rangeToAdd ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "RCornerAddFail", RCORNER_ADD_FAIL ), MyApp.NoticeType.Warning );
				return;
			}

			// back to state 0 (clear selection and highlight)
			ResetPointSelection();

			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			RefreshSegmentRenderer();
			UpdateDialogState();
		}

		void UpdateDialogState()
		{
			if( m_Dlg == null || m_Dlg.IsDisposed ) {
				return;
			}

			// status text
			if( string.IsNullOrEmpty( m_CurrentPathID ) ) {
				m_Dlg.SetStatus( m_LanguageManager.TryGetString( "StatusSelectPath", STATUS_SELECT_PATH ) );
			}
			else if( m_FirstSelectedCADIndex != null ) {
				m_Dlg.SetStatus( m_LanguageManager.TryGetString( "StatusAddSecond", STATUS_ADD_SECOND ) );
			}
			else {
				m_Dlg.SetStatus( m_LanguageManager.TryGetString( "StatusSelectMode", STATUS_SELECT_MODE ) );
			}

			// delete button: only with a selected range
			m_Dlg.SetDeleteEnabled( m_SelectedRange != null );

			// switch button: only when the selected range touches a path start/end point and is not the whole path
			m_Dlg.SetSwitchEnabled( m_SelectedRange != null && !IsWholePath( m_SelectedRange ) && RangeContainsPathStartOrEndPoint( m_SelectedRange ) );
		}

		bool IsWholePath( RCornerData range )
		{
			return range.StartIndex == GetPathStartCADIndex() && range.EndIndex == SpecialPointIndex.CLOSED_POINT_INDEX;
		}

		bool RangeContainsPathStartOrEndPoint( RCornerData range )
		{
			return range.StartIndex == GetPathStartCADIndex() || range.EndIndex == SpecialPointIndex.CLOSED_POINT_INDEX;
		}

		#endregion

		CraftData m_CraftData;
		ContourCache m_PathCache;

		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "RCornerAction" );
	}
}
