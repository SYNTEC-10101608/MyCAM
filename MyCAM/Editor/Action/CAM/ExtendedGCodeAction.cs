using MyCAM.App;
using MyCAM.Data;
using MyCAM.PathCache;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;
namespace MyCAM.Editor
{
	internal class ExtendedGCodeAction : EditActionBase
	{
		public ExtendedGCodeAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
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
				return EditActionType.ExtendedGCode;
			}
		}

		public Action<List<string>> PropertyChanged;

		public override void Start()
		{
			base.Start();

			// setup initial path
			if( !TryChangePath( m_CurrentPathID ) ) {
				End();
				return;
			}

			// create dialog
			m_Dlg = new ExtendedGCodeDlg();
			m_Dlg.Cancel += End;
			m_Dlg.AddRequest += OnAddRequest;
			m_Dlg.DeleteRequest += OnDeleteRequest;
			m_Dlg.PositionChanged += OnPositionChanged;
			m_Dlg.ContentChanged += OnContentChanged;
			m_Dlg.ToStartOrEnd += OnToStartOrEnd;
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// activate point selection for initial path
			ActivatePointSelection();
			m_PathIndexAction.EnterSingleSelectMode();
		}

		public override void End()
		{
			m_Dlg?.FlushContent();

			m_PathIndexAction.ExitSingleSelectMode();
			DeactivatePointSelection( m_CurrentPathID );

			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			// close dialog
			if( m_Dlg != null ) {
				m_Dlg.Cancel -= End;
				m_Dlg.Close();
				m_Dlg = null;
			}

			base.End();
		}

		#region Path Selection

		// path index control
		string m_CurrentPathID = null;
		SelectPathAction m_PathIndexAction = null;

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

			m_Dlg?.FlushContent();

			string szOldPathID = m_CurrentPathID;
			if( !TryChangePath( newPathID ) ) {
				return;
			}

			// cleanup old state
			DeactivatePointSelection( szOldPathID );

			// activate point selection for new path
			ActivatePointSelection();
		}

		bool TryChangePath( string pathID )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				return false;
			}

			// validate new path (contour only)
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NotContourPath", NOT_CONTOUR_PATH ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			// init data for current path
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			m_CraftData = craftData;

			// init ContourCache data for current path
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache pathCache ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}
			m_PathCache = pathCache;

			m_CurrentPathID = pathID;
			return true;
		}

		#endregion

		#region Point Selection

		// point index control
		int? m_nSelectCADIndex = null;
		IndexSelectAction m_PointIndexAction;

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			m_nSelectCADIndex = null;

			if( TryBuildExtendedGCodeParameter( ExtendedGCodeType.PathHead, null ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexAction = new IndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexAction.Start();
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			m_nSelectCADIndex = null;
			m_Param = null;

			DestroyPointIndexAction();
			m_PathIndexAction.RestoreFromExclusion( szOldPathID );
		}

		void DestroyPointIndexAction()
		{
			if( m_PointIndexAction != null ) {
				m_PointIndexAction.IndexChanged -= OnPointIndexChanged;
				m_PointIndexAction.End();
				m_PointIndexAction = null;
			}
		}

		void OnPointIndexChanged( int nSelectIndex, TopoDS_Shape selectedVertex )
		{
			if( nSelectIndex == m_nSelectCADIndex && m_Param.GCodeType == ExtendedGCodeType.ControlPoint ) {
				return;
			}
			OnSelectedPointIndexChanged( nSelectIndex );
		}

		void OnSelectedPointIndexChanged( int? nSelectIndex )
		{
			if( m_nSelectCADIndex == nSelectIndex && m_Param.GCodeType == ExtendedGCodeType.ControlPoint ) {
				return;
			}

			m_Dlg?.FlushContent();

			m_nSelectCADIndex = nSelectIndex;

			// should not happen, but just in case
			if( nSelectIndex == null ) {
				m_Param = null;
				m_Dlg?.ResetParam( null );
				return;
			}

			// refresh dialog
			if( TryBuildExtendedGCodeParameter( ExtendedGCodeType.ControlPoint, m_nSelectCADIndex ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		int? GetCurrentStoredIndex()
		{
			if( m_Param.GCodeType != ExtendedGCodeType.ControlPoint || m_nSelectCADIndex == null ) {
				return null;
			}
			return GetStoredIndex( m_nSelectCADIndex.Value, m_Param.IsPathStartPoint );
		}

		int GetStoredIndex( int cadIndex, bool? isPathStartPoint )
		{
			// end point and start point share the same CAD index,
			// use closed point index to distinguish the end point
			if( isPathStartPoint == false ) {
				return SpecialPointIndex.CLOSED_POINT_INDEX;
			}
			return cadIndex;
		}

		#endregion

		#region Dialog Handling

		ExtendedGCodeDlg m_Dlg = null;
		ExtendedGCodeParam m_Param = null;

		bool TryBuildExtendedGCodeParameter( ExtendedGCodeType gCodeType, int? cadIndex, bool? isPathStartPoint = null )
		{
			if( m_CraftData == null ) {
				return false;
			}

			if( gCodeType != ExtendedGCodeType.ControlPoint ) {
				m_Param = BuildExtendedGCodeParameter( gCodeType, null, null );
				return true;
			}

			// must have CAD index for control point
			if( cadIndex == null ) {
				return false;
			}

			// only closed path start point can toggle between start and end
			if( !IsClosedPathStartPoint( cadIndex.Value ) ) {
				isPathStartPoint = null;
			}
			else if( isPathStartPoint == null ) {
				isPathStartPoint = true;
			}

			int nStoredIndex = GetStoredIndex( cadIndex.Value, isPathStartPoint );
			m_Param = BuildExtendedGCodeParameter( gCodeType, nStoredIndex, isPathStartPoint );

			return true;
		}

		ExtendedGCodeParam BuildExtendedGCodeParameter( ExtendedGCodeType gCodeType, int? cadIndex, bool? isPathStartPoint )
		{
			Tuple<ExtendedGCodeType, int?> key = Tuple.Create( gCodeType, cadIndex );

			if( m_CraftData.ExtendedGCodeMap.TryGetValue( key, out string content ) ) {
				return new ExtendedGCodeParam( true, gCodeType, content, isPathStartPoint );
			}

			return new ExtendedGCodeParam( false, gCodeType, string.Empty, isPathStartPoint );
		}

		bool IsClosedPathStartPoint( int cadIndex )
		{
			if( m_PathCache.MainPathPointList.Count == 0 ) {
				return false;
			}

			return SpecialPointIndex.IsClosedPointIndex( m_PathCache.MainPathPointList.Last().InitPathIndex )
				&& ( cadIndex == m_PathCache.MainPathPointList.First().InitPathIndex );
		}

		void OnAddRequest()
		{
			if( m_CraftData == null || m_Param == null ) {
				return;
			}

			m_Param.IsModified = true;
			m_CraftData.SetExtendedGCode( m_Param.GCodeType, GetCurrentStoredIndex(), m_Param.Content );
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
		}

		void OnDeleteRequest()
		{
			if( m_CraftData == null || m_Param == null ) {
				return;
			}

			m_CraftData.RemoveExtendedGCode( m_Param.GCodeType, GetCurrentStoredIndex() );
			m_Param.IsModified = false;
			m_Param.Content = string.Empty;
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
		}

		void OnPositionChanged( ExtendedGCodeType gCodeType )
		{
			if( m_CraftData == null || m_Param == null ) {
				return;
			}

			bool isControlPoint = ( gCodeType == ExtendedGCodeType.ControlPoint );

			if( !isControlPoint ) {
				m_Viewer.GetAISContext().ClearSelected( true );
			}

			if( TryBuildExtendedGCodeParameter( gCodeType, isControlPoint ? m_nSelectCADIndex : null ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void OnContentChanged( string content )
		{
			if( m_CraftData == null || m_Param == null ) {
				return;
			}

			if( !m_Param.IsModified ) {
				return;
			}

			if( m_Param.Content == content ) {
				return;
			}

			m_Param.Content = content;
			m_CraftData.SetExtendedGCode( m_Param.GCodeType, GetCurrentStoredIndex(), m_Param.Content );
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
		}

		void OnToStartOrEnd()
		{
			if( m_Param == null || m_Param.IsPathStartPoint == null ) {
				return;
			}

			// should not happen, but just in case
			if( m_PathCache.MainPathPointList.Count <= 0 ) {
				return;
			}

			bool toStart = ( m_Param.IsPathStartPoint != true );
			if( TryBuildExtendedGCodeParameter( ExtendedGCodeType.ControlPoint, m_nSelectCADIndex, toStart ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		#endregion

		CraftData m_CraftData;
		ContourCache m_PathCache;

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExtendedGCodeAction" );
	}
}
