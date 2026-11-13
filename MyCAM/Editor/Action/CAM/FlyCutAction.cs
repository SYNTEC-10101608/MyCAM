using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.Helper;
using MyCAM.Helper.CAM;
using MyCAM.PathCache;
using OCC.gp;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal class FlyCutAction : EditActionBase
	{
		public FlyCutAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, List<string> pathIDList, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIDList = pathIDList;
			m_PathIndexAction = pathIndexAction;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.FlyCut;
			}
		}

		public Action<List<string>> PropertyChanged;
		public Action SortCompleted;

		public override void Start()
		{
			base.Start();

			// SingleSpindle or SingleTable
			if( m_DataManager.MachineData is MixTypeMachineData mixData ) {
				if( mixData.MixTypeVariant == MixTypeVariant.MasterOnly
					|| mixData.MixTypeVariant == MixTypeVariant.SlaveOnly ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "FourAxisNotSupportFlycut", FOUR_AXIS_NOT_SUPPORT_FLYCUT ), MyApp.NoticeType.Warning );
					End();
					return;
				}
			}

			// setup initial path
			if( !TryChangePath( m_PathIDList ) ) {
				End();
				return;
			}
			TryGetPlaneCoordinate( out m_PlaneCoordinate );

			if( !DataGettingHelper.GetMachineData( out FiveAxisMachineData machineData ) || machineData == null ) {

				End();
				return;
			}
			m_AxisDir = machineData.TubeDir == RotaryAxis.None ? null : RotateAxisDef.RotaryAxisToDir( machineData.TubeDir );

			// create dialog
			m_Dlg = new FlyCutDlg();
			m_Dlg.Cancel += End;
			m_Dlg.AddFlyCut += OnAddFlyCut;
			m_Dlg.RemoveFlyCut += OnRemoveFlyCut;
			m_Dlg.PositionChanged += OnPositionChanged;
			m_Dlg.DirectionChanged += OnDirectionChanged;
			m_Dlg.GroupWidthChanged += OnGroupWidthChanged;
			m_Dlg.Show( MyApp.MainForm );

			// start path index action
			m_PathIndexAction.AddLockedPaths( CreateLockList() );
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			m_Param = new FlyCutParam( true, true, true, 0, false );
			UpdateDialogParam();
		}

		public override void End()
		{
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

		IEnumerable<string> CreateLockList()
		{
			// TODO: using std pattern recognize to exclude wrong shape with current flycut type

			List<string> lockList = new List<string>();
			foreach( string pathID in m_DataManager.PathIDList ) {

				// is std pattern path, lock it
				if( !DataGettingHelper.GetPathType( pathID, out PathType pathType )
					|| ( pathType != PathType.Contour && pathType != PathType.FlyCutGroup ) ) {
					lockList.Add( pathID );
				}
			}
			return lockList;
		}

		#region Path Selection

		void OnPathIndexChanged()
		{
			List<string> selectedIDs = m_PathIndexAction.GetSelectedIDs();

			if( !TryChangePath( selectedIDs ) ) {
				m_PathIDList.Clear();
				m_PathCacheList.Clear();
				m_CraftDataList.Clear();
				m_PlaneCoordinate = null;
			}
			else {
				TryGetPlaneCoordinate( out m_PlaneCoordinate );
			}
			UpdateDialogParam();
		}

		bool TryChangePath( List<string> pathIDList )
		{
			if( pathIDList == null ) {
				return false;
			}

			List<string> newPathIDList = new List<string>( pathIDList );
			List<CraftData> newCraftDataList = new List<CraftData>();
			List<ContourCache> newPathCacheList = new List<ContourCache>();


			foreach( string pathID in newPathIDList ) {

				string pathName = m_DataManager.GetPathName( pathID );
				if( DataGettingHelper.GetPathType( pathID, out PathType selectedType ) && selectedType == PathType.FlyCutGroup ) {
					continue;
				}

				// validate new path (contour only)
				if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "NotContourPath", NOT_CONTOUR_PATH ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}

				// init data for current path
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}
				newCraftDataList.Add( craftData );

				// init ContourCache data for current path
				if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache pathCache ) ) {
					MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathName ), MyApp.NoticeType.Warning );
					return false;
				}
				newPathCacheList.Add( pathCache );
			}

			m_PathIDList = newPathIDList;
			m_CraftDataList = newCraftDataList;
			m_PathCacheList = newPathCacheList;

			return true;
		}

		#endregion

		#region Dailog Handling

		void OnPositionChanged( bool isLeftOrRight, bool isTopOrBottom )
		{
			m_Param.IsLeftOrRight = isLeftOrRight;
			m_Param.IsTopOrBottom = isTopOrBottom;
		}

		void OnDirectionChanged( bool isVerticalOrHorizontal )
		{
			m_Param.IsVerticalOrHorizontal = isVerticalOrHorizontal;
			UpdateDialogParam();
		}

		void UpdateDialogParam()
		{
			if( m_Dlg == null || m_Param == null ) {
				return;
			}

			m_Param.IsGroupWidthAngle = m_PathIDList != null && m_PathIDList.Count >= 2
				&& m_PlaneCoordinate == null && m_Param.IsVerticalOrHorizontal;
			m_Dlg.ResetParam( m_Param );
		}

		void OnAddFlyCut()
		{
			if( m_PathIDList == null || m_PathIDList.Count < 2
				|| m_PathIDList.Any( id => !DataGettingHelper.GetPathType( id, out PathType type ) || type != PathType.Contour ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildFlyCut", CANNOT_BUILD_FLYCUT ), MyApp.NoticeType.Warning );
				return;
			}

			for( int i = 0; i < m_PathIDList.Count; ++i ) {
				if( !m_DataManager.PathIDList.Contains( m_PathIDList[ i ] ) ) {
					MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildFlyCut", CANNOT_BUILD_FLYCUT ), MyApp.NoticeType.Warning );
					return;
				}
			}

			BuildFlyCut();
		}

		void OnRemoveFlyCut()
		{
			bool hasRemovedFlyCut = false;
			foreach( string pathID in m_PathIDList ) {
				if( DataGettingHelper.GetPathType( pathID, out PathType type ) && type == PathType.FlyCutGroup ) {
					m_DataManager.RemovePath( pathID );
					hasRemovedFlyCut = true;
				}
			}

			if( hasRemovedFlyCut ) {
				SortCompleted?.Invoke();
			}
		}

		void OnGroupWidthChanged( double groupWidth )
		{
			if( m_Dlg == null || m_Param == null ) {
				return;
			}

			m_Param.GroupWidth = groupWidth;
		}

		#endregion

		void BuildFlyCut()
		{
			List<string> originalIDs = m_PathIDList.ToList();
			List<string> originalOrder = m_DataManager.PathIDList.ToList();

			TryGetPlaneCoordinate( out m_PlaneCoordinate );
			if( !TrySortPath( out List<string> sortedMembers, out Dictionary<string, int> groupIndexByPathID ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildFlyCut", CANNOT_BUILD_FLYCUT ), MyApp.NoticeType.Warning );
				return;
			}

			FlyCutGroupPathObject group = m_DataManager.AddFlyCutGroup( sortedMembers );
			if( group == null ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildFlyCut", CANNOT_BUILD_FLYCUT ), MyApp.NoticeType.Warning );
				return;
			}

			TryChangePath( sortedMembers );
			if( !TrySetFlyCutStartPoints( sortedMembers, groupIndexByPathID ) || !TrySetFlyCutDirections( sortedMembers, groupIndexByPathID ) ) {
				m_DataManager.RemovePath( group.UID );
				m_DataManager.SetSortedPathIDList( originalOrder );
				TryChangePath( originalIDs );
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotBuildFlyCut", CANNOT_BUILD_FLYCUT ), MyApp.NoticeType.Warning );
				return;
			}

			m_DataManager.PathAdded?.Invoke( m_DataManager.PathIDList.ToList() );
			SortCompleted?.Invoke();
		}

		bool TrySortPath( out List<string> sortedMembers, out Dictionary<string, int> groupIndexByPathID )
		{
			sortedMembers = null;
			groupIndexByPathID = null;

			if( m_Param == null || m_PathIDList == null || m_PathIDList.Count < 2 ) {
				return false;
			}

			sortedMembers = m_PlaneCoordinate != null
				? PathSortHelper.SortFlyCutOnPlane( m_PathIDList, m_PlaneCoordinate,
					m_Param.IsLeftOrRight, m_Param.IsTopOrBottom, m_Param.IsVerticalOrHorizontal,
					m_Param.GroupWidth, out groupIndexByPathID )
				: PathSortHelper.SortFlyCut( m_PathIDList, m_AxisDir, m_AxisLocation,
					m_Param.IsLeftOrRight, m_Param.IsTopOrBottom, m_Param.IsVerticalOrHorizontal,
					m_Param.GroupWidth, out groupIndexByPathID );

			return sortedMembers.Count == m_PathIDList.Count && groupIndexByPathID.Count == m_PathIDList.Count;
		}

		bool TrySetFlyCutStartPoints( List<string> sortedMembers, Dictionary<string, int> groupIndexByPathID )
		{
			if( sortedMembers == null || groupIndexByPathID == null
				|| ( m_PlaneCoordinate == null && m_AxisDir == null ) ) {
				return false;
			}

			gp_Vec axis = m_PlaneCoordinate != null ? null : new gp_Vec( m_AxisDir );
			gp_Dir planeSearchDirection = null;
			if( m_PlaneCoordinate != null ) {
				planeSearchDirection = m_Param.IsVerticalOrHorizontal ? m_PlaneCoordinate.XDirection() : m_PlaneCoordinate.YDirection().Reversed();
				if( m_Param.IsVerticalOrHorizontal ? !m_Param.IsLeftOrRight : !m_Param.IsTopOrBottom ) {
					planeSearchDirection = planeSearchDirection.Reversed();
				}
			}

			List<int> startIndices = new List<int>();
			for( int i = 0; i < m_PathIDList.Count; ++i ) {
				ContourCache cache = m_PathCacheList[ i ];
				if( cache == null || m_CraftDataList[ i ] == null
					|| !groupIndexByPathID.ContainsKey( m_PathIDList[ i ] ) ) {
					return false;
				}

				gp_Dir searchDirection = planeSearchDirection;
				if( m_PlaneCoordinate == null ) {
					gp_Ax1 reference = cache.ComputeRefCenterDir;
					if( reference == null || reference.Location() == null ) {
						return false;
					}

					gp_Vec axisToCenter = new gp_Vec( m_AxisLocation, reference.Location() );
					gp_Vec circumferential = axis.Crossed( axisToCenter );
					double magnitudeSquared = circumferential.SquareMagnitude();
					if( double.IsNaN( magnitudeSquared ) || double.IsInfinity( magnitudeSquared ) || magnitudeSquared <= FlyCutHelper.LENGTH_TOLERANCE_SQUARED ) {
						return false;
					}

					searchDirection = new gp_Dir( circumferential );
					if( m_Param.IsVerticalOrHorizontal ) {
						if( !m_Param.IsLeftOrRight ) {
							searchDirection = searchDirection.Reversed();
						}
					}
					else {
						gp_Vec toInside = axis.Crossed( circumferential );
						searchDirection = new gp_Dir( circumferential.Crossed( toInside ) );
						if( m_Param.IsTopOrBottom ) {
							searchDirection = searchDirection.Reversed();
						}
					}
				}

				List<CAMPoint> points = cache.MainPathPointList;
				if( ( m_PlaneCoordinate != null && !FlyCutHelper.TryGetPlaneClockwise( points, m_PlaneCoordinate, out _ ) )
					|| !StartPointHelper.TryChooseByDirection( points, searchDirection, START_POINT_CANDIDATE_BAND_MM, out int camIndex ) ) {
					return false;
				}
				startIndices.Add( points[ camIndex ].InitPathIndex );
			}
			for( int i = 0; i < m_PathIDList.Count; ++i ) {
				m_CraftDataList[ i ].SetStartPntIndex( startIndices[ i ] );
			}

			return true;
		}

		bool TrySetFlyCutDirections( List<string> sortedMembers, Dictionary<string, int> groupIndexByPathID )
		{
			if( sortedMembers == null || groupIndexByPathID == null
				|| ( m_PlaneCoordinate == null && m_AxisDir == null ) ) {
				return false;
			}

			bool firstClockwise = m_Param.IsVerticalOrHorizontal
				? m_Param.IsLeftOrRight == m_Param.IsTopOrBottom
				: m_Param.IsLeftOrRight != m_Param.IsTopOrBottom;

			List<CraftData> toReverse = new List<CraftData>();
			foreach( string pathID in sortedMembers ) {
				if( !groupIndexByPathID.TryGetValue( pathID, out int groupIndex )
					|| !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache cache )
					|| cache == null
					|| !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craft )
					|| craft == null ) {
					return false;
				}

				bool clockwise;
				bool hasDirection = m_PlaneCoordinate != null
					? FlyCutHelper.TryGetPlaneClockwise( cache.MainPathPointList, m_PlaneCoordinate, out clockwise )
					: FlyCutHelper.TryGetClockwise( cache.MainPathPointList, m_AxisLocation, m_AxisDir, out clockwise );
				if( !hasDirection ) {
					return false;
				}
				bool targetClockwise = groupIndex % 2 == 0 ? firstClockwise : !firstClockwise;
				if( clockwise != targetClockwise ) {
					toReverse.Add( craft );
				}
			}
			foreach( CraftData craft in toReverse ) {
				craft.IsPathReverse = !craft.IsPathReverse;
			}
			return true;
		}

		bool TryGetPlaneCoordinate( out gp_Ax3 planeCoordinate )
		{
			planeCoordinate = null;
			if( m_PathCacheList == null || m_PathCacheList.Count == 0 ) {
				return false;
			}

			List<gp_Pnt> points = new List<gp_Pnt>();
			foreach( ContourCache cache in m_PathCacheList ) {
				List<CAMPoint> pathPoints = cache?.MainPathPointList;
				if( pathPoints == null || pathPoints.Count == 0 ) {
					return false;
				}
				foreach( CAMPoint pathPoint in pathPoints ) {
					gp_Pnt point = pathPoint?.Point;
					if( point == null
						|| double.IsNaN( point.X() ) || double.IsInfinity( point.X() )
						|| double.IsNaN( point.Y() ) || double.IsInfinity( point.Y() )
						|| double.IsNaN( point.Z() ) || double.IsInfinity( point.Z() ) ) {
						return false;
					}
					points.Add( point );
				}
			}
			if( points.Count < 3 ) {
				return false;
			}

			gp_Pnt origin = points[ 0 ];
			gp_Pnt farthestPoint = null;
			double farthestDistanceSquared = FlyCutHelper.POSITION_TOLERANCE_SQUARED;
			foreach( gp_Pnt point in points ) {
				double distanceSquared = origin.SquareDistance( point );
				if( double.IsNaN( distanceSquared ) || double.IsInfinity( distanceSquared ) ) {
					return false;
				}
				if( distanceSquared > farthestDistanceSquared ) {
					farthestDistanceSquared = distanceSquared;
					farthestPoint = point;
				}
			}
			if( farthestPoint == null ) {
				return false;
			}

			gp_Vec baseline = new gp_Vec( origin, farthestPoint ).Multiplied( 1 / Math.Sqrt( farthestDistanceSquared ) );
			gp_Vec planeNormal = null;
			double farthestLineDistanceSquared = FlyCutHelper.POSITION_TOLERANCE_SQUARED;
			foreach( gp_Pnt point in points ) {
				gp_Vec cross = baseline.Crossed( new gp_Vec( origin, point ) );
				double distanceSquared = cross.SquareMagnitude();
				if( double.IsNaN( distanceSquared ) || double.IsInfinity( distanceSquared ) ) {
					return false;
				}
				if( distanceSquared > farthestLineDistanceSquared ) {
					farthestLineDistanceSquared = distanceSquared;
					planeNormal = cross;
				}
			}
			if( planeNormal == null ) {
				return false;
			}

			gp_Dir normalDirection = new gp_Dir( planeNormal );
			double absoluteX = Math.Abs( normalDirection.X() );
			double absoluteY = Math.Abs( normalDirection.Y() );
			double absoluteZ = Math.Abs( normalDirection.Z() );
			double dominantComponent = absoluteZ >= absoluteX && absoluteZ >= absoluteY
				? normalDirection.Z() : absoluteY >= absoluteX ? normalDirection.Y() : normalDirection.X();
			if( dominantComponent < 0 ) {
				normalDirection = normalDirection.Reversed();
			}
			gp_Vec normalVector = new gp_Vec( normalDirection );
			foreach( gp_Pnt point in points ) {
				double distance = new gp_Vec( origin, point ).Dot( normalVector );
				if( double.IsNaN( distance ) || double.IsInfinity( distance ) || Math.Abs( distance ) > FlyCutHelper.POSITION_TOLERANCE_MM ) {
					return false;
				}
			}

			gp_Vec horizontal = new gp_Vec( 1, 0, 0 );
			horizontal = horizontal.Subtracted( normalVector.Multiplied( horizontal.Dot( normalVector ) ) );
			if( horizontal.SquareMagnitude() <= FlyCutHelper.NUMERICAL_TOLERANCE_SQUARED ) {
				horizontal = new gp_Vec( 0, 1, 0 );
				horizontal = horizontal.Subtracted( normalVector.Multiplied( horizontal.Dot( normalVector ) ) );
			}
			planeCoordinate = new gp_Ax3( origin, normalDirection, new gp_Dir( horizontal ) );
			return true;
		}

		// Select existing points within this projection distance of the directional extreme
		const double START_POINT_CANDIDATE_BAND_MM = 1;

		// If exist, selected path is on a plane, use axis direction to sort
		gp_Ax3 m_PlaneCoordinate = null;

		gp_Dir m_AxisDir = null;
		// suppose ring shape is centered at origin
		gp_Pnt m_AxisLocation = new gp_Pnt( 0, 0, 0 );

		// Path Selection
		List<string> m_PathIDList = null;
		List<CraftData> m_CraftDataList = null;
		List<ContourCache> m_PathCacheList = null;

		SelectPathAction m_PathIndexAction = null;

		// Dialog Handling
		FlyCutDlg m_Dlg = null;
		FlyCutParam m_Param = null;

		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;

		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "FlyCutAction" );
		const string NOT_CONTOUR_PATH = "路徑 {0} 不是輪廓路徑，無法編輯";
		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";
		const string CANNOT_BUILD_FLYCUT = "無法產生飛切";
		const string FOUR_AXIS_NOT_SUPPORT_FLYCUT = "四軸機構不支援飛切";
	}
}
