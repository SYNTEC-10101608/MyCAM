using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;

namespace MyCAM.Editor
{
	internal class ContourEditAction : EditActionBase
	{
		public ContourEditAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_Viewer = viewer;
			m_TreeView = treeView;
			m_ViewManager = viewManager;

			// for path index control
			m_PathIndexAction = pathIndexAction;
			m_CurrentPathID = pathID;
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ContourEdit;
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
			m_Dlg = new ContourEditDlg();
			m_Dlg.AddEditIndex += OnAddEditIndex;
			m_Dlg.RemoveEditIndex += OnRemoveEditIndex;
			m_Dlg.DisplacementChanged += OnDisplacementChanged;
			m_Dlg.SaveEditIndex += OnSaveEditIndex;
			m_Dlg.ResetDisplacement += OnResetDisplacement;
			m_Dlg.ToStartOrEnd += OnToStartOrEnd;
			m_Dlg.Cancel += End;
			m_Dlg.Show( MyApp.MainForm );

			// create coordinate icon renderer
			m_CoordIconRenderer = new CoordIconRenderer( m_Viewer, m_DataManager );
			m_CoordIconRenderer.SetAppearance( Quantity_NameOfColor.Quantity_NOC_RED, 10.0 );

			// start path index action
			m_PathIndexAction.AddLockedPaths( m_DataManager.PathIDList.Where(
				id => !DataGettingHelper.GetPathType( id, out PathType type ) || type != PathType.Contour ) );
			m_PathIndexAction.Start();
			m_PathIndexAction.SelectionChange += OnPathIndexChanged;

			// activate point selection for initial path
			ActivatePointSelection();
		}

		public override void End()
		{
			DeactivatePointSelection( m_CurrentPathID );

			// cleanup path index action
			m_PathIndexAction.SelectionChange -= OnPathIndexChanged;
			m_PathIndexAction.End();

			m_EditPointRenderer?.Remove( true );
			m_CoordIconRenderer?.Remove( true );

			// close dialog
			if( m_Dlg != null ) {
				m_Dlg.Cancel -= End;
				m_Dlg.Close();
				m_Dlg = null;
			}

			base.End();
		}

		#region Path Selection

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

			// init ContourCache data for current path
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache pathCache ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			m_CurrentPathID = pathID;
			m_CraftData = craftData;
			m_PathCache = pathCache;

			BuildOrignCachePoint();

			// show edit point marks
			m_EditPointRenderer?.Remove();
			m_EditPointRenderer = new EditPointRenderer( m_Viewer, m_CurrentPathID );
			m_EditPointRenderer.Show( true );
			return true;
		}

		void BuildOrignCachePoint()
		{
			Dictionary<int, ContourEditData> editBackup = m_CraftData.ContourEditMap.ToDictionary( pair => pair.Key, pair => pair.Value.Clone() );
			m_CraftData.ClearContourEditPoint();

			m_OrignalTrsfCADPointList = m_PathCache.TrsfCADPointList.Select( p => p.Point ).ToList();

			foreach( var kvp in editBackup ) {
				ContourEditData data = kvp.Value;
				m_CraftData.SetContourEditPoint( kvp.Key, data.DX, data.DY, data.DZ );
			}
		}

		#endregion

		#region Point Selection

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

			List<CAMPoint> camPointList = m_PathCache.MainPathPointList;
			OnSelectedPointIndexChanged( camPointList.First().InitPathIndex );
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexAction = new CAMIndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexAction.Start();
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			m_nSelectIndex = NULL_SELECT_INDEX;
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
			if( nSelectIndex == m_nSelectIndex ) {
				return;
			}

			OnSelectedPointIndexChanged( nSelectIndex );
		}

		void OnSelectedPointIndexChanged( int nSelectIndex )
		{
			// should not happen, but just in case
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				m_Param = null;
				m_Dlg.ResetParam( null );
				return;
			}

			if( m_nSelectIndex == nSelectIndex ) {
				return;
			}

			m_nSelectIndex = nSelectIndex;

			bool? isPathStartPoint = IsStartOrEndPoint( m_nSelectIndex );

			if( TryBuildContourEditParameter( m_Param?.RefCoordinateType ?? RefCoordinateType.World, m_nSelectIndex, isPathStartPoint ) ) {
				m_Dlg.ResetParam( m_Param );
				UpdateCoordinate();
			}
		}

		bool TryBuildContourEditParameter( RefCoordinateType refCoordinateType, int cadIndex, bool? isPathStartPoint )
		{
			if( m_CraftData == null || cadIndex == NULL_SELECT_INDEX ) {
				return false;
			}

			bool isModified = false;
			ContourEditData data = new ContourEditData();

			if( isPathStartPoint != null ) {

				// startpoint always exists
				isModified = true;

				data = ( isPathStartPoint == true )
					? m_CraftData.StartPntContourEditData.StartPnt
					: m_CraftData.StartPntContourEditData.EndPnt;
			}
			else {
				isModified = m_CraftData.ContourEditMap.ContainsKey( cadIndex );

				if( isModified ) {
					data = m_CraftData.ContourEditMap[ cadIndex ];
				}
			}

			m_BaseWorldDX = data.DX;
			m_BaseWorldDY = data.DY;
			m_BaseWorldDZ = data.DZ;

			gp_Vec displayDisplacement = new gp_Vec( m_BaseWorldDX, m_BaseWorldDY, m_BaseWorldDZ );

			if( refCoordinateType == RefCoordinateType.Local
				&& TryConvertWorldToLocal( displayDisplacement, out gp_Vec localDisplacement ) ) {
				displayDisplacement = localDisplacement;
			}

			m_Param = new ContourEditParam( refCoordinateType, isModified, isPathStartPoint, displayDisplacement.X(), displayDisplacement.Y(), displayDisplacement.Z() );

			return true;
		}

		bool? IsStartOrEndPoint( int cadIndex )
		{
			List<CAMPoint> camPointList = m_PathCache.MainPathPointList;

			if( camPointList == null || camPointList.Count == 0 ) {
				return null;
			}

			if( cadIndex == camPointList.First().InitPathIndex ) {
				return true;
			}

			if( cadIndex == camPointList.Last().InitPathIndex ) {
				return false;
			}

			return null;
		}

		#endregion

		#region Dialog Handling

		void OnDisplacementChanged( double dx, double dy, double dz, RefCoordinateType refCoordType )
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}

			m_Param.RefCoordinateType = refCoordType;

			double worldDX = m_BaseWorldDX;
			double worldDY = m_BaseWorldDY;
			double worldDZ = m_BaseWorldDZ;

			// if this is control point, add the currenet displacement to total displacement and update the displacement to CraftData
			if( m_Param.IsModified ) {
				gp_Vec displacement = new gp_Vec( dx, dy, dz );
				if( refCoordType == RefCoordinateType.Local ) {
					if( !TryConvertLocalToWorld( displacement, out gp_Vec worldDisplacement ) ) {
						m_CoordIconRenderer?.Remove( true );
						return;
					}
					displacement = worldDisplacement;
				}

				worldDX += displacement.X();
				worldDY += displacement.Y();
				worldDZ += displacement.Z();

				if( m_Param.IsPathStartPoint.HasValue ) {
					m_CraftData.SetStartPntContourEdit( m_Param.IsPathStartPoint.Value, worldDX, worldDY, worldDZ );
				}
				else {
					m_CraftData.SetContourEditPoint( m_nSelectIndex, worldDX, worldDY, worldDZ );
				}

				UpdatePath();
			}

			// convert the world displacement to local if needed 
			gp_Vec displayDisplacement = new gp_Vec( worldDX, worldDY, worldDZ );
			if( refCoordType == RefCoordinateType.Local ) {
				if( !TryConvertWorldToLocal( displayDisplacement, out gp_Vec localDisplacement ) ) {
					return;
				}
				displayDisplacement = localDisplacement;
			}

			m_Param.TotalDX = displayDisplacement.X();
			m_Param.TotalDY = displayDisplacement.Y();
			m_Param.TotalDZ = displayDisplacement.Z();

			UpdateCoordinate();

			// only refresh total displacement
			m_Dlg.UpdateTotalDisplacement();
		}

		void OnToStartOrEnd( bool toStart )
		{
			if( m_Param == null ) {
				return;
			}

			List<CAMPoint> camPointList = m_PathCache.MainPathPointList;
			if( camPointList == null || camPointList.Count == 0 ) {
				return;
			}

			OnSelectedPointIndexChanged( toStart ? camPointList.First().InitPathIndex : camPointList.Last().InitPathIndex );
		}

		void OnSaveEditIndex()
		{
			if( m_Param == null || m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}

			if( TryBuildContourEditParameter( m_Param.RefCoordinateType, m_nSelectIndex, IsStartOrEndPoint( m_nSelectIndex ) ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void OnResetDisplacement()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}

			ResetDisplacement();

			if( m_Param.IsPathStartPoint == null ) {
				m_CraftData.SetContourEditPoint( m_nSelectIndex, 0, 0, 0 );
			}
			else {
				m_CraftData.SetStartPntContourEdit( m_Param.IsPathStartPoint.Value, 0, 0, 0 );
			}

			UpdatePath();
			UpdateCoordinate();

			// refresh dialog
			if( TryBuildContourEditParameter( m_Param.RefCoordinateType, m_nSelectIndex, IsStartOrEndPoint( m_nSelectIndex ) ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void OnAddEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}

			m_Param.IsModified = true;

			int trsfCADIndex = m_PathCache.OriginalIndexMap.IndexOf( m_nSelectIndex );
			if( !TryGetCAMPoint( m_nSelectIndex, out CAMPoint camPoint ) || trsfCADIndex == -1 ) {
				return;
			}

			// displacement of control point
			gp_Pnt currentPnt = camPoint.Point;
			gp_Pnt orignalPnt = m_OrignalTrsfCADPointList[ trsfCADIndex ];
			gp_Vec displacement = new gp_Vec( orignalPnt, currentPnt );

			// displacement of start/end before adding
			gp_Pnt orignalStartPnt = m_PathCache.MainPathPointList.First().Point;
			gp_Pnt orignalEndPnt = m_PathCache.MainPathPointList.Last().Point;

			m_CraftData.SetContourEditPoint( m_nSelectIndex, displacement.X(), displacement.Y(), displacement.Z() );

			// displacement of start/end after adding
			gp_Pnt currentStartPnt = m_PathCache.MainPathPointList.First().Point;
			gp_Pnt currentEndPnt = m_PathCache.MainPathPointList.Last().Point;

			// calibrate displacement of start/end  
			gp_Vec startPointDisplacement = new gp_Vec( currentStartPnt, orignalStartPnt );
			gp_Vec endPointDisplacement = new gp_Vec( currentEndPnt, orignalEndPnt );

			ContourEditData startPntContourEditData = m_CraftData.StartPntContourEditData.StartPnt;
			ContourEditData endPntContourEditData = m_CraftData.StartPntContourEditData.EndPnt;

			m_CraftData.SetStartPntContourEdit( true,
				startPntContourEditData.DX + startPointDisplacement.X(),
				startPntContourEditData.DY + startPointDisplacement.Y(),
				startPntContourEditData.DZ + startPointDisplacement.Z() );

			m_CraftData.SetStartPntContourEdit( false,
				endPntContourEditData.DX + endPointDisplacement.X(),
				endPntContourEditData.DY + endPointDisplacement.Y(),
				endPntContourEditData.DZ + endPointDisplacement.Z() );

			UpdatePath();
			UpdateCoordinate();

			// refresh dialog
			if( TryBuildContourEditParameter( m_Param.RefCoordinateType, m_nSelectIndex, IsStartOrEndPoint( m_nSelectIndex ) ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void OnRemoveEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}

			m_Param.IsModified = false;

			ResetDisplacement();

			m_CraftData.RemoveContourEditPoint( m_nSelectIndex );

			UpdatePath();
			UpdateCoordinate();

			// refresh dialog
			if( TryBuildContourEditParameter( m_Param?.RefCoordinateType ?? RefCoordinateType.World, m_nSelectIndex, IsStartOrEndPoint( m_nSelectIndex ) ) ) {
				m_Dlg.ResetParam( m_Param );
			}
		}

		void ResetDisplacement()
		{
			m_BaseWorldDX = 0;
			m_BaseWorldDY = 0;
			m_BaseWorldDZ = 0;

			// for dialog display
			if( m_Param != null ) {
				m_Param.TotalDX = 0;
				m_Param.TotalDY = 0;
				m_Param.TotalDZ = 0;
			}
		}

		#endregion

		#region UI Render

		bool TryGetCAMPointIndex( int selectIndex, out int camIndex )
		{
			camIndex = -1;
			List<CAMPoint> camPointList = m_PathCache.MainPathPointList;
			if( camPointList == null || camPointList.Count == 0 ) {
				return false;
			}

			// closed point always maps to the last CAM point
			if( SpecialPointIndex.IsClosedPointIndex( selectIndex ) ) {
				camIndex = camPointList.Count - 1;
				return true;
			}
			if( !m_PathCache.CADToCAMIndexMap.ContainsKey( selectIndex ) ) {
				return false;
			}
			int index = m_PathCache.CADToCAMIndexMap[ selectIndex ];
			if( index < 0 || index >= camPointList.Count ) {
				return false;
			}
			camIndex = index;
			return true;
		}

		bool TryGetCAMPoint( int selectIndex, out CAMPoint camPoint )
		{
			camPoint = null;
			if( !TryGetCAMPointIndex( selectIndex, out int camIndex ) ) {
				return false;
			}
			camPoint = m_PathCache.MainPathPointList[ camIndex ];
			return true;
		}

		bool TryGetLocalCoordinate( int selectIndex, out gp_Ax3 localCoordinate )
		{
			localCoordinate = null;
			if( !TryGetCAMPointIndex( selectIndex, out int camIndex ) ) {
				return false;
			}
			List<CAMPoint> camPointList = m_PathCache.MainPathPointList;
			CAMPoint camPoint = camPointList[ camIndex ];

			// Use the normal as the local Z-axis
			gp_Vec normalVec = new gp_Vec( camPoint.NormalVec_1st );
			if( normalVec.Magnitude() < GEOM_TOLERANCE ) {
				return false;
			}
			normalVec.Normalize();

			// Use the tangent as the local X-axis if possible
			// Else, use a neighbor point direction ( should not happen, but just in case)
			gp_Vec yVec = normalVec.Crossed( new gp_Vec( camPoint.TangentVec ) );
			gp_Dir xDirection;
			if( yVec.Magnitude() < GEOM_TOLERANCE ) {
				List<gp_Pnt> pointList = camPointList.Select( p => p.Point ).ToList();
				if( !TryGetNextPointXDirection( pointList, camIndex, normalVec, out xDirection ) ) {
					return false;
				}
			}
			else {
				yVec.Normalize();
				gp_Vec xVec = yVec.Crossed( normalVec );
				if( xVec.Magnitude() < GEOM_TOLERANCE ) {
					return false;
				}
				xVec.Normalize();
				xDirection = new gp_Dir( xVec );
			}

			localCoordinate = new gp_Ax3( camPoint.Point, new gp_Dir( normalVec ), xDirection );
			return true;
		}

		bool TryConvertLocalToWorld( gp_Vec locaclDisplacement, out gp_Vec worldDisplacement )
		{
			worldDisplacement = null;

			// Build LocalCoord at the selected point
			if( m_nSelectIndex == NULL_SELECT_INDEX || !TryGetLocalCoordinate( m_nSelectIndex, out gp_Ax3 localCoordinate ) ) {
				return false;
			}

			gp_Trsf trsf = new gp_Trsf();
			trsf.SetTransformation( localCoordinate );

			worldDisplacement = locaclDisplacement.Transformed( trsf.Inverted() );

			return true;
		}

		bool TryConvertWorldToLocal( gp_Vec worldDisplacement, out gp_Vec localDisplacement )
		{
			localDisplacement = null;

			// Build LocalCoord at the selected point
			if( m_nSelectIndex == NULL_SELECT_INDEX || !TryGetLocalCoordinate( m_nSelectIndex, out gp_Ax3 localCoordinate ) ) {
				return false;
			}

			gp_Trsf trsf = new gp_Trsf();
			trsf.SetTransformation( localCoordinate );

			localDisplacement = worldDisplacement.Transformed( trsf );

			return true;
		}

		// derive the local X-axis from a neighbor point direction
		bool TryGetNextPointXDirection( List<gp_Pnt> pointList, int index, gp_Vec normalVec, out gp_Dir xDirection )
		{
			xDirection = null;
			gp_Pnt currentPoint = pointList[ index ];
			int pointCount = pointList.Count;

			// search forward first and wrap around for closed contours
			for( int step = 1; step < pointCount; step++ ) {
				int nextIndex = ( index + step ) % pointCount;

				gp_Vec yVec = normalVec.Crossed( new gp_Vec( currentPoint, pointList[ nextIndex ] ) );
				if( yVec.Magnitude() >= GEOM_TOLERANCE ) {
					yVec.Normalize();
					gp_Vec xVec = yVec.Crossed( normalVec );
					xVec.Normalize();
					xDirection = new gp_Dir( xVec );
					return true;
				}
			}

			// search backward when no valid point in forward
			for( int previousIndex = index - 1; previousIndex >= 0; previousIndex-- ) {
				gp_Vec yVec = normalVec.Crossed( new gp_Vec( pointList[ previousIndex ], currentPoint ) );
				if( yVec.Magnitude() >= GEOM_TOLERANCE ) {
					yVec.Normalize();
					gp_Vec xVec = yVec.Crossed( normalVec );
					xVec.Normalize();
					xDirection = new gp_Dir( xVec );
					return true;
				}
			}
			return false;
		}

		void UpdatePath()
		{
			PropertyChanged?.Invoke( new List<string>() { m_CurrentPathID } );
			m_PointIndexAction.UpdateSelectionPoints();
			m_EditPointRenderer?.Show( true );
		}

		void UpdateCoordinate()
		{
			if( m_CoordIconRenderer == null ) {
				return;
			}

			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				m_CoordIconRenderer.Remove( true );
				return;
			}

			// Show the icon directly when using world coordinates
			if( m_Param.RefCoordinateType == RefCoordinateType.World ) {
				if( !TryGetCAMPoint( m_nSelectIndex, out CAMPoint camPoint ) ) {
					m_CoordIconRenderer.Remove( true );
					return;
				}
				m_CoordIconRenderer.Show( camPoint.Point, true );
				return;
			}

			// Build the LocalCoord
			if( !TryGetLocalCoordinate( m_nSelectIndex, out gp_Ax3 coordinate ) ) {
				m_CoordIconRenderer.Remove( true );
				return;
			}

			// Transform the Coord from World to Local
			gp_Trsf trsf = new gp_Trsf();
			trsf.SetDisplacement( new gp_Ax3(), coordinate );
			m_CoordIconRenderer.Show( false );
			m_CoordIconRenderer.Trans( trsf );
			m_Viewer.UpdateView();
		}

		#endregion

		// Path Selection
		string m_CurrentPathID = null;
		SelectPathAction m_PathIndexAction = null;

		CraftData m_CraftData = null;
		ContourCache m_PathCache = null;

		// cahce of TrsfCADPointList before contour edit
		IReadOnlyList<gp_Pnt> m_OrignalTrsfCADPointList = null;

		const string NOT_CONTOUR_PATH = "路徑 {0} 不是輪廓路徑，無法編輯";
		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";

		// Point Selection

		// point index control
		int m_nSelectIndex = NULL_SELECT_INDEX;
		CAMIndexSelectAction m_PointIndexAction = null;

		// null point index as -999, -1 is used for closed point index
		const int NULL_SELECT_INDEX = -999;

		// Dialog Handling
		ContourEditDlg m_Dlg = null;

		ContourEditParam m_Param = null;
		double m_BaseWorldDX = 0;
		double m_BaseWorldDY = 0;
		double m_BaseWorldDZ = 0;

		// UI Render
		const double GEOM_TOLERANCE = 1e-3;

		EditPointRenderer m_EditPointRenderer = null;
		CoordIconRenderer m_CoordIconRenderer = null;

		// composition references
		Viewer m_Viewer = null;
		TreeView m_TreeView = null;
		ViewManager m_ViewManager = null;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ContourEditAction" );
	}
}
