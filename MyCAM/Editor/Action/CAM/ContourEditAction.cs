using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
			if( !SetupPath( m_CurrentPathID ) ) {
				End();
				return;
			}

			// create dialog
			m_Dlg = new ContourEditDlg();
			m_Dlg.AddEditIndex += OnAddEditIndex;
			m_Dlg.RemoveEditIndex += OnRemoveEditIndex;
			m_Dlg.DisplacementChanged += OnDisplacementChanged;
			m_Dlg.ResetDisplacement += OnResetDisplacement;
			m_Dlg.Cancel += End;
			m_Dlg.Show( MyApp.MainForm );

			// create coordinate icon renderer
			m_CoordIconRenderer = new CoordIconRenderer( m_Viewer, m_DataManager );
			m_CoordIconRenderer.SetAppearance( Quantity_NameOfColor.Quantity_NOC_RED, 10.0 );

			// start path index action
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

		bool SetupPath( string pathID )
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
			m_CurrentPathID = pathID;
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				MyApp.Logger.ShowOnLogPanel( string.Format( m_LanguageManager.TryGetString( "CannotGetPathCraftInfo", CANNOT_GET_PATH_CRAFT_INFO ), pathID ), MyApp.NoticeType.Warning );
				return false;
			}

			// show edit point marks
			m_EditPointRenderer?.Remove( true );
			m_EditPointRenderer = new EditPointRenderer( m_Viewer, m_CurrentPathID );
			m_EditPointRenderer.Show( true );
			return true;
		}

		void ActivatePointSelection()
		{
			m_PathIndexAction.ExcludeFromSelection( m_CurrentPathID );
			CreatePointIndexAction( m_CurrentPathID );

			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

			OnSelectedPointIndexChanged( 0 );
		}

		void DeactivatePointSelection( string szOldPathID )
		{
			m_nSelectIndex = NULL_SELECT_INDEX;
			m_Param = null;

			DestroyPointIndexAction();
			m_PathIndexAction.RestoreFromExclusion( szOldPathID );
		}

		void CreatePointIndexAction( string pathID )
		{
			m_PointIndexAction = new IndexSelectAction( m_DataManager, m_Viewer, m_TreeView, m_ViewManager, pathID );
			m_PointIndexAction.IndexChanged += OnPointIndexChanged;
			m_PointIndexAction.Start();
		}

		void DestroyPointIndexAction()
		{
			if( m_PointIndexAction != null ) {
				m_PointIndexAction.IndexChanged -= OnPointIndexChanged;
				m_PointIndexAction.End();
				m_PointIndexAction = null;
			}
		}

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
			if( !SetupPath( newPathID ) ) {
				return;
			}

			// cleanup old state
			DeactivatePointSelection( szOldPathID );

			// activate point selection for new path
			ActivatePointSelection();
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
			if( m_nSelectIndex == nSelectIndex ) {
				return;
			}
			m_nSelectIndex = nSelectIndex;

			// should not happen, but just in case
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				m_Param = null;
				m_Dlg.ResetParam( null );
				return;
			}

			// refresh dialog
			GetParamFormIndex( nSelectIndex, out m_BaseWorldDX, out m_BaseWorldDY, out m_BaseWorldDZ, out bool isModified );
			m_Param = new ContourEditParam( RefCoordinateType.Local, isModified );
			m_Dlg.ResetParam( m_Param );

			UpdateCoordinate();
		}

		void OnAddEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			if( m_Param != null ) {
				m_Param.IsModified = true;
			}

			// commit to data with current (zero) displacement
			m_BaseWorldDX = 0;
			m_BaseWorldDY = 0;
			m_BaseWorldDZ = 0;
			m_CraftData.SetContourEditPoint( m_nSelectIndex, m_BaseWorldDX, m_BaseWorldDY, m_BaseWorldDZ );
			UpdatePath();
			UpdateCoordinate();

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
		}

		void OnRemoveEditIndex()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			if( m_Param != null ) {
				m_Param.IsModified = false;
			}

			m_BaseWorldDX = 0;
			m_BaseWorldDY = 0;
			m_BaseWorldDZ = 0;
			m_CraftData.RemoveContourEditPoint( m_nSelectIndex );
			UpdatePath();
			UpdateCoordinate();

			// refresh dialog
			m_Dlg.ResetParam( m_Param );
		}

		void OnDisplacementChanged( double dx, double dy, double dz, RefCoordinateType refCoordType )
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}

			m_Param.RefCoordinateType = refCoordType;

			if( !m_Param.IsModified ) {
				UpdateCoordinate();
				return;
			}

			if( !TryConvertToWorldDisplacement( dx, dy, dz, out gp_Vec worldDisplacement ) ) {
				return;
			}

			m_CraftData.SetContourEditPoint( m_nSelectIndex, m_BaseWorldDX + worldDisplacement.x, m_BaseWorldDY + worldDisplacement.y, m_BaseWorldDZ + worldDisplacement.z );
			UpdatePath();
			UpdateCoordinate();
		}

		void OnResetDisplacement()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX || m_Param == null ) {
				return;
			}

			m_BaseWorldDX = 0;
			m_BaseWorldDY = 0;
			m_BaseWorldDZ = 0;
			m_CraftData.SetContourEditPoint( m_nSelectIndex, 0, 0, 0 );
			UpdatePath();
			UpdateCoordinate();
		}

		void GetParamFormIndex( int nSelectIndex, out double dx, out double dy, out double dz, out bool isModified )
		{
			dx = 0;
			dy = 0;
			dz = 0;
			isModified = false;
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}
			isModified = m_CraftData.ContourEditMap.ContainsKey( nSelectIndex );
			if( isModified ) {
				ContourEditData data = m_CraftData.ContourEditMap[ nSelectIndex ];
				dx = data.DX;
				dy = data.DY;
				dz = data.DZ;
			}
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

			// Get the selected point's contourCache
			if( m_nSelectIndex == NULL_SELECT_INDEX
				|| !DataGettingHelper.GetPathCacheByID( m_CurrentPathID, out IPathCache pathCache )
				|| !( pathCache is ContourCache contourCache ) ) {
				m_CoordIconRenderer.Remove( true );
				return;
			}

			// Convert the selected point index to its corresponding index in TrsfCADPointList (offsetIndex)
			List<int> originalIndexMap = contourCache.OriginalIndexMap;
			int offsetIndex = originalIndexMap != null ? originalIndexMap.IndexOf( m_nSelectIndex ) : m_nSelectIndex;
			List<CADPoint> cadPointList = contourCache.TrsfCADPointList;
			if( offsetIndex < 0 || offsetIndex >= cadPointList.Count ) {
				m_CoordIconRenderer.Remove( true );
				return;
			}

			// Show the icon directly when using world coordinates
			CADPoint cadPoint = cadPointList[ offsetIndex ];
			RefCoordinateType refCoordType = m_Param.RefCoordinateType;
			if( refCoordType == RefCoordinateType.World ) {
				m_CoordIconRenderer.Show( cadPoint.Point, true );
				return;
			}

			// Build the LocalCoord
			gp_Ax2 coordinate = refCoordType == RefCoordinateType.World
				? new gp_Ax2( cadPoint.Point, new gp_Dir( 0, 0, 1 ), new gp_Dir( 1, 0, 0 ) )
				: TryGetLocalCoordinate( cadPointList, offsetIndex, out gp_Ax2 localCoordinate ) ? localCoordinate : null;
			if( coordinate == null ) {
				m_CoordIconRenderer.Remove( true );
				return;
			}

			// Transform the Coord from World to Local
			gp_Ax3 coordinateSystem = new gp_Ax3( coordinate.Location(), coordinate.Direction(), coordinate.XDirection() );
			gp_Trsf transform = new gp_Trsf();
			transform.SetDisplacement( new gp_Ax3(), coordinateSystem );
			m_CoordIconRenderer.Show( false );
			m_CoordIconRenderer.Trans( transform );
			m_Viewer.UpdateView();
		}

		bool TryGetLocalCoordinate( List<CADPoint> cadPointList, int pointIndex, out gp_Ax2 localCoordinate )
		{
			localCoordinate = null;
			if( cadPointList == null || pointIndex < 0 || pointIndex >= cadPointList.Count ) {
				return false;
			}

			// Use the normal as the local Z-axis
			CADPoint cadPoint = cadPointList[ pointIndex ];
			gp_Vec normalVec = new gp_Vec( cadPoint.NormalVec_1st );
			if( normalVec.Magnitude() < GEOM_TOLERANCE ) {
				return false;
			}
			normalVec.Normalize();

			// Use the tangent as the local X-axis if possible
			// Else, use a neighbor point direction ( should not happen, but just in case)
			gp_Vec yVec = normalVec.Crossed( new gp_Vec( cadPoint.TangentVec ) );
			gp_Dir xDirection;
			if( yVec.Magnitude() < GEOM_TOLERANCE ) {
				if( !TryGetNextPointXDirection( cadPointList, pointIndex, normalVec, out xDirection ) ) {
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

			localCoordinate = new gp_Ax2( cadPoint.Point, new gp_Dir( normalVec ), xDirection );
			return true;
		}

		bool TryConvertToWorldDisplacement( double dx, double dy, double dz, out gp_Vec worldDisplacement )
		{
			// WorldCoord can be used directly
			if( m_Param.RefCoordinateType != RefCoordinateType.Local ) {
				worldDisplacement = new gp_Vec( dx, dy, dz );
				return true;
			}

			// Get the selected point's contourCache
			if( m_nSelectIndex == NULL_SELECT_INDEX
			|| !DataGettingHelper.GetPathCacheByID( m_CurrentPathID, out IPathCache pathCache )
			|| !( pathCache is ContourCache contourCache ) ) {
				m_CoordIconRenderer?.Remove( true );
				worldDisplacement = null;
				return false;
			}

			// Convert the selected point index to its corresponding index in TrsfCADPointList (offsetIndex)
			List<CADPoint> cadPointList = contourCache.TrsfCADPointList;
			List<int> originalIndexMap = contourCache.OriginalIndexMap;
			int offsetIndex = originalIndexMap != null ? originalIndexMap.IndexOf( m_nSelectIndex ) : m_nSelectIndex;

			// Build LocalCoord at the selected point
			if( !TryGetLocalCoordinate( cadPointList, offsetIndex, out gp_Ax2 localCoordinate ) ) {
				m_CoordIconRenderer?.Remove( true );
				worldDisplacement = null;
				return false;
			}

			gp_Dir xDirection = localCoordinate.XDirection();
			gp_Dir yDirection = localCoordinate.YDirection();
			gp_Dir zDirection = localCoordinate.Direction();
			worldDisplacement = new gp_Vec(
				dx * xDirection.X() + dy * yDirection.X() + dz * zDirection.X(),
				dx * xDirection.Y() + dy * yDirection.Y() + dz * zDirection.Y(),
				dx * xDirection.Z() + dy * yDirection.Z() + dz * zDirection.Z() );
			return true;
		}

		bool TryGetNextPointXDirection( List<CADPoint> cadPointList, int pointIndex, gp_Vec normalVec, out gp_Dir xDirection )
		{
			xDirection = null;
			CADPoint currentPoint = cadPointList[ pointIndex ];
			int pointCount = cadPointList.Count;

			// Search forward first and wrap around for closed contours
			for( int step = 1; step < pointCount; step++ ) {
				int nextIndex = pointIndex + step;
				if( nextIndex >= pointCount ) {
					nextIndex %= pointCount;
				}

				gp_Vec yVec = normalVec.Crossed( new gp_Vec( currentPoint.Point, cadPointList[ nextIndex ].Point ) );
				if( yVec.Magnitude() >= GEOM_TOLERANCE ) {
					yVec.Normalize();
					gp_Vec xVec = yVec.Crossed( normalVec );
					xVec.Normalize();
					xDirection = new gp_Dir( xVec );
					return true;
				}
			}

			// Search backward when no valid point in forward
			for( int previousIndex = pointIndex - 1; previousIndex >= 0; previousIndex-- ) {
				gp_Vec yVec = normalVec.Crossed( new gp_Vec( cadPointList[ previousIndex ].Point, currentPoint.Point ) );
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

		// path index control
		string m_CurrentPathID;
		CraftData m_CraftData;

		// point index control
		int m_nSelectIndex = NULL_SELECT_INDEX;

		ContourEditParam m_Param = null;
		double m_BaseWorldDX = 0;
		double m_BaseWorldDY = 0;
		double m_BaseWorldDZ = 0;

		// action utility
		EditPointRenderer m_EditPointRenderer = null;
		CoordIconRenderer m_CoordIconRenderer = null;
		ContourEditDlg m_Dlg = null;

		// null point index as -999, -1 is used for closed point index
		const int NULL_SELECT_INDEX = -999;

		const double GEOM_TOLERANCE = 1e-3;

		const string NOT_CONTOUR_PATH = "路徑 {0} 不是輪廓路徑，無法編輯";
		const string CANNOT_GET_PATH_CRAFT_INFO = "無法獲取路徑 {0} 的加工資訊";

		// composition references
		Viewer m_Viewer;
		TreeView m_TreeView;
		ViewManager m_ViewManager;
		SelectPathAction m_PathIndexAction;
		IndexSelectAction m_PointIndexAction;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ContourEditAction" );
	}
}
