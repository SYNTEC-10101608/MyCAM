using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.Aspect;
using OCC.gp;
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
	internal class ToolVectorAction : IndexSelectAction
	{
		public ToolVectorAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager, pathID )
		{
			// get craft data by pathID
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				throw new ArgumentException( "Cannot get CraftData by pathID: " + pathID );
			}
			m_RotaryAxisConfig = CreateRotaryAxisConfig();
			m_DataHandler = new ToolVecActionDataHandler( pathID );
			m_PathIDList = new List<string>() { pathID };
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.ToolVec;
			}
		}

		public Action<List<string>> PropertyChanged;
		public Action<EActionStatus> RaiseEditingToolVecDlg;

		public override void Start()
		{
			base.Start();

			// init global param
			m_InterpolateType = m_CraftData.InterpolateType;

			// init index param
			m_nSelectIndex = NULL_SELECT_INDEX;
			m_SelectedPoint = null;
			m_ToolVecParam = null;

			// init dialog
			m_ToolVecDlg = new ToolVectorDlg( m_CraftData.InterpolateType, m_ToolVecParam, m_CraftData.IsPathReverse, m_RotaryAxisConfig );
			m_ToolVecDlg.SetKeep += () => OnSetKeep();
			m_ToolVecDlg.SetZdir += () => OnSetZDir();
			m_ToolVecDlg.SetRevert += () => OnSetRevert();
			m_ToolVecDlg.ABAngleChanged = ( angleA, angleB ) => OnABAngleChanged( angleA, angleB );
			m_ToolVecDlg.MSAngleChanged = ( master, slave ) => OnMSAngleChanged( master, slave );
			m_ToolVecDlg.TypeChanged += ( type ) => OnTypeChanged( type );
			m_ToolVecDlg.AddEditIndex += () => OnAddEditIndex();
			m_ToolVecDlg.RemoveEditIndex += () => OnRemoveEditIndex();
			m_ToolVecDlg.SwitchStartEnd += () => OnSwitchStartEnd();
			m_ToolVecDlg.EnableStartEndSwitch( false );
			m_ToolVecDlg.Cancel += End;
			m_ToolVecDlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			UnlockSelectedVertexHighLight();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			// editing tool vector, do not allow other operation
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			UnlockSelectedVertexHighLight();

			// update select index
			int? _nSelectIndex = GetSelectIndex( out TopoDS_Shape selectedVertex );
			int nSelectIndex = _nSelectIndex ?? NULL_SELECT_INDEX;

			// lock selected vertex high light
			LockSelectedVertexHighLight( selectedVertex );
			OnSelectedIndexChanged( nSelectIndex );
		}

		protected override void ViewerKeyDown( KeyEventArgs e )
		{
			// editing tool vector, do not allow other operation
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.KeyCode == Keys.Escape ) {
				End();
			}
		}

		void OnSelectedIndexChanged( int nSelectIndex )
		{
			m_nSelectIndex = nSelectIndex;

			// no select
			if( nSelectIndex == NULL_SELECT_INDEX ) {
				m_SelectedPoint = null;
				m_ToolVecParam = null;
				m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );
			}

			// with select
			else {
				m_SelectedPoint = m_DataHandler.GetPointByCADIndex( m_nSelectIndex );
				bool isModified = m_DataHandler.GetToolVecModify( m_nSelectIndex, out double angleA_deg, out double angleB_deg, out double master_deg, out double slave_deg );
				m_ToolVecParam = new ToolVecParam( angleA_deg, angleB_deg, master_deg, slave_deg, isModified );

				// update dialog
				m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );
				m_ToolVecDlg.EnableStartEndSwitch( ( m_nSelectIndex == m_DataHandler.GetStartPointCADIndex() || m_nSelectIndex == CLOSED_POINT_INDEX )
													&& m_DataHandler.IsClosed() );
			}
		}

		void LockSelectedVertexHighLight( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex == null || selectedVertex.IsNull() ) {
				return;
			}

			// show selected vertex on viewer, because pause select mode woud clear select
			DrawVertexOnViewer( selectedVertex );
		}

		void UnlockSelectedVertexHighLight()
		{
			// remove selected vertex which show on viewer
			if( m_KeepedHighLightPoint != null ) {

				// true means update viewer
				m_Viewer.GetAISContext().Remove( m_KeepedHighLightPoint, true );
				m_KeepedHighLightPoint = null;
			}
		}

		void DrawVertexOnViewer( TopoDS_Shape selectedVertex )
		{
			if( selectedVertex != null ) {
				m_KeepedHighLightPoint = new AIS_Shape( selectedVertex );

				// set pointAspect
				Prs3d_PointAspect pointAspect = new Prs3d_PointAspect(
					Aspect_TypeOfMarker.Aspect_TOM_BALL,
					new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_GREEN ),
					3.0f
				);

				// set high light attribute
				m_KeepedHighLightPoint.Attributes().SetPointAspect( pointAspect );

				// refresh viewer
				m_Viewer.GetAISContext().Display( m_KeepedHighLightPoint, true );
			}
		}

		// dialog event
		void OnSetRevert()
		{
			if( m_SelectedPoint == null ) {
				return;
			}

			// point exists, get original MS angles
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = m_SelectedPoint.InitMaster_rad * 180.0 / Math.PI;
				m_ToolVecParam.Slave_deg = m_SelectedPoint.InitSlave_rad * 180.0 / Math.PI;
				m_ToolVecParam.AngleA_deg = 0;
				m_ToolVecParam.AngleB_deg = 0;
			}
			SetToolVecParamAndPeview();
		}

		void OnSetKeep()
		{
			if( m_nSelectIndex == NULL_SELECT_INDEX ) {
				return;
			}

			// get previous control point
			List<int> ctrlPntIndexList = m_CraftData.ToolVecModifyMap.Keys.ToList();
			int preCtrlPntIndex = GetPreCtrlPntIndex( m_nSelectIndex, ctrlPntIndexList, m_CraftData.IsPathReverse, m_DataHandler.IsClosed() );
			ISetToolVecPoint preCtrlPoint = m_DataHandler.GetPointByCADIndex( preCtrlPntIndex );
			if( preCtrlPoint == null ) {
				return;
			}

			// Calculate AB angles from previous control point MS angles
			if( m_SelectedPoint == null ) {
				return;
			}
			Tuple<double, double> abAngles_deg =
				ToolVecHelper.GetABAngleFromMSAngle( preCtrlPoint.ModMaster_rad * 180.0 / Math.PI,
														preCtrlPoint.ModSlave_rad * 180.0 / Math.PI,
														m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 ) ) {
				return;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = preCtrlPoint.ModMaster_rad * 180.0 / Math.PI;
				m_ToolVecParam.Slave_deg = preCtrlPoint.ModSlave_rad * 180.0 / Math.PI;
				m_ToolVecParam.AngleA_deg = abAngles_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngles_deg.Item2;
			}
			SetToolVecParamAndPeview();
		}

		void OnSetZDir()
		{
			// calculate MS angles from Z direction
			if( m_SelectedPoint == null ) {
				return;
			}
			Tuple<double, double> msAngles_deg = ToolVecHelper.GetMSAngleFromToolVec( new gp_Dir( 0, 0, 1 ), m_SelectedPoint );

			// calculate AB angles from MS angles
			Tuple<double, double> abAngles_deg = ToolVecHelper.GetABAngleFromMSAngle( msAngles_deg.Item1, msAngles_deg.Item2, m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 ) ) {
				return;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = msAngles_deg.Item1;
				m_ToolVecParam.Slave_deg = msAngles_deg.Item2;
				m_ToolVecParam.AngleA_deg = abAngles_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngles_deg.Item2;
			}
			SetToolVecParamAndPeview();
		}

		void OnABAngleChanged( double angleA_deg, double angleB_deg )
		{
			// check angle range
			if( !CheckABAngleRange( angleA_deg, angleB_deg ) ) {
				return;
			}
			if( m_SelectedPoint == null ) {
				return;
			}
			Tuple<double, double> msAngle_deg = ToolVecHelper.GetMSAngleFromABAngle( angleA_deg, angleB_deg, m_SelectedPoint );

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = msAngle_deg.Item1;
				m_ToolVecParam.Slave_deg = msAngle_deg.Item2;
				m_ToolVecParam.AngleA_deg = angleA_deg;
				m_ToolVecParam.AngleB_deg = angleB_deg;
			}
			SetToolVecParamAndPeview();
		}

		void OnMSAngleChanged( double master_deg, double slave_deg )
		{
			if( m_SelectedPoint == null ) {
				return;
			}
			Tuple<double, double> abAngle_deg = ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngle_deg.Item1, abAngle_deg.Item2 ) ) {
				return;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.AngleA_deg = abAngle_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngle_deg.Item2;
				m_ToolVecParam.Master_deg = master_deg;
				m_ToolVecParam.Slave_deg = slave_deg;
			}
			SetToolVecParamAndPeview();
		}

		void OnTypeChanged( EToolVecInterpolateType type )
		{
			m_InterpolateType = type;
			SetToolVecParamAndPeview( true );
		}

		void OnAddEditIndex()
		{
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.IsModified = true;
				SetToolVecParamAndPeview( true );
			}
		}

		void OnRemoveEditIndex()
		{
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.IsModified = false;
				SetToolVecParamAndPeview( true );
			}
		}

		void OnSwitchStartEnd()
		{
			// at start index, switch to end index
			if( m_nSelectIndex == m_DataHandler.GetStartPointCADIndex() ) {
				OnSelectedIndexChanged( CLOSED_POINT_INDEX );
			}

			// at end index, switch to start index
			else if( m_nSelectIndex == CLOSED_POINT_INDEX ) {
				OnSelectedIndexChanged( m_DataHandler.GetStartPointCADIndex() );
			}
			else {
				// do nothing if not start or end index
			}
		}

		bool CheckABAngleRange( double angleA_deg, double angleB_deg )
		{
			if( angleA_deg < MIN_TiltAngle || angleA_deg > MAX_TiltAngle ||
				angleB_deg < MIN_TiltAngle || angleB_deg > MAX_TiltAngle ) {
				MyApp.Logger.ShowOnLogPanel( "傾角值過大，角度必須在 -60~+60 範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		int GetPreCtrlPntIndex( int targetIndex, List<int> ctrlPntIndexList, bool isReverse, bool isClosePath )
		{
			// keep the list in order
			ctrlPntIndexList.Sort();
			int result = NULL_SELECT_INDEX;

			// find the last index which small than targetIndex
			if( isReverse == false ) {
				foreach( int nIndex in ctrlPntIndexList ) {
					if( nIndex < targetIndex ) {
						result = nIndex;
					}
					else {
						break;
					}
				}

				// unclose path do not find pre ctrl pnt index
				if( isClosePath == false ) {
					return targetIndex;
				}

				// if not found, return the last value of the list (circular logic)
				if( result == NULL_SELECT_INDEX && ctrlPntIndexList.Count > 0 ) {
					result = ctrlPntIndexList.Last();
				}
			}
			else {
				// find the first index which larger than targetIndex
				foreach( int nIndex in ctrlPntIndexList ) {
					if( nIndex > targetIndex ) {
						result = nIndex;
						break;
					}
				}

				// unclose path do not find next ctrl pnt index
				if( isClosePath == false ) {
					return targetIndex;
				}

				// if not found, return the first value of the list (circular logic)
				if( result == NULL_SELECT_INDEX && ctrlPntIndexList.Count > 0 ) {
					result = ctrlPntIndexList.First();
				}
			}
			return result;
		}

		// update
		void SetToolVecParamAndPeview( bool bForceUpdate = false )
		{
			SetInterpolateType();
			SetIndexAngleParam();
			if( ( m_ToolVecParam != null && m_ToolVecParam.IsModified ) || bForceUpdate ) {
				PropertyChanged?.Invoke( m_PathIDList );
			}

			// update cache point
			if( m_SelectedPoint != null && m_nSelectIndex != NULL_SELECT_INDEX ) {
				m_SelectedPoint = m_DataHandler.GetPointByCADIndex( m_nSelectIndex );
			}
		}

		void SetInterpolateType()
		{
			m_CraftData.InterpolateType = m_InterpolateType;
		}

		void SetIndexAngleParam()
		{
			if( m_ToolVecParam == null ) {
				return;
			}

			// remove modify data
			if( !m_ToolVecParam.IsModified ) {
				m_CraftData.RemoveToolVecModify( m_nSelectIndex );
				return;
			}

			// set modify data
			m_CraftData.SetToolVecModify( m_nSelectIndex,
				m_ToolVecParam.AngleA_deg, m_ToolVecParam.AngleB_deg, m_ToolVecParam.Master_deg, m_ToolVecParam.Slave_deg );
		}

		// rotary axis config
		RotaryAxisConfig CreateRotaryAxisConfig()
		{
			if( !DataGettingHelper.GetMachineData( out MachineData machineData ) ) {
				throw new InvalidOperationException( "Cannot get machine data" );
			}
			RotaryAxisConfig config = new RotaryAxisConfig();
			config.MasterName = ConvertRotaryAxisName( machineData.MasterRotaryAxis );
			config.SlaveName = ConvertRotaryAxisName( machineData.SlaveRotaryAxis );
			if( machineData.FiveAxisType == FiveAxisType.Table || machineData.FiveAxisType == FiveAxisType.Mix ) {
				config.RotaryAxis = ETypeOfRotaryAxis.Slave;
			}
			else {
				config.RotaryAxis = ETypeOfRotaryAxis.Master;
			}
			return config;
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


		// index param
		int m_nSelectIndex = NULL_SELECT_INDEX;
		ToolVecParam m_ToolVecParam = null;
		ISetToolVecPoint m_SelectedPoint = null;

		// global param
		EToolVecInterpolateType m_InterpolateType = EToolVecInterpolateType.Normal;
		ToolVecActionDataHandler m_DataHandler = null;
		CraftData m_CraftData = null;
		RotaryAxisConfig m_RotaryAxisConfig = null;

		// action data
		List<string> m_PathIDList = null;
		AIS_Shape m_KeepedHighLightPoint = null;
		ToolVectorDlg m_ToolVecDlg = null;

		// angle limit
		public const double MAX_TiltAngle = 60.0;
		public const double MIN_TiltAngle = -60.0;

		// null select index as -999, -1 is used for closed point index
		const int NULL_SELECT_INDEX = -999;
		const int CLOSED_POINT_INDEX = -1;
	}

	class ToolVecActionDataHandler
	{
		public ToolVecActionDataHandler( string szPathID )
		{
			// check ID is a contour path
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType )
				|| pathType != PathType.Contour ) {
				throw new ArgumentException( "PathID is not a contour path: " + szPathID );
			}

			// get craft data by pathID
			if( !DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData ) ) {
				throw new ArgumentException( "Cannot get CraftData by pathID: " + szPathID );
			}
			m_CraftData = craftData;

			// get cache by pathID
			if( !DataGettingHelper.GetContourCacheByID( szPathID, out ContourCache pathCache ) ) {
				throw new ArgumentException( "Cannot get ContourCache by pathID: " + szPathID );
			}
			m_PathCache = pathCache;

			// get geom data by pathID
			if( !DataGettingHelper.GetGeomDataByID( szPathID, out IGeomData geomData ) ) {
				throw new ArgumentException( "Cannot get GeomData by pathID: " + szPathID );
			}
			m_GeomData = geomData as ContourGeomData;
		}

		public bool GetToolVecModify( int index, out double dRA_deg, out double dRB_deg, out double master_deg, out double slave_deg )
		{
			if( m_CraftData.ToolVecModifyMap.ContainsKey( index ) ) {
				dRA_deg = m_CraftData.ToolVecModifyMap[ index ].RA_deg;
				dRB_deg = m_CraftData.ToolVecModifyMap[ index ].RB_deg;
				master_deg = m_CraftData.ToolVecModifyMap[ index ].Master_deg;
				slave_deg = m_CraftData.ToolVecModifyMap[ index ].Slave_deg;
				return true;
			}
			else {
				dRA_deg = 0;
				dRB_deg = 0;

				// get CAM map index
				int camIndex = 0;
				if( index == CLOSED_POINT_INDEX ) {
					camIndex = m_PathCache.MainPathPointList.Count - 1;
				}
				else if( m_PathCache.CADToCAMIndexMap.ContainsKey( index ) ) {
					camIndex = m_PathCache.CADToCAMIndexMap[ index ];
				}
				else {
					master_deg = 0;
					slave_deg = 0;
					return false;
				}

				// get master and slave from cache
				if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
					master_deg = m_PathCache.MainPathPointList[ camIndex ].ModMaster_rad * 180.0 / Math.PI;
					slave_deg = m_PathCache.MainPathPointList[ camIndex ].ModSlave_rad * 180.0 / Math.PI;

					// get AB angles from master and slave
					Tuple<double, double> abAngles = ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, m_PathCache.MainPathPointList[ camIndex ] );
					dRA_deg = abAngles.Item1;
					dRB_deg = abAngles.Item2;
				}
				else {
					master_deg = 0;
					slave_deg = 0;
				}
				return false;
			}
		}

		public ISetToolVecPoint GetPointByCADIndex( int cadIndex )
		{
			if( m_PathCache.CADToCAMIndexMap.ContainsKey( cadIndex ) ) {
				int camIndex = m_PathCache.CADToCAMIndexMap[ cadIndex ];
				if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
					return m_PathCache.MainPathPointList[ camIndex ];
				}
			}
			else if( cadIndex == CLOSED_POINT_INDEX ) {
				return m_PathCache.MainPathPointList.Last();
			}
			return null;
		}

		public bool IsClosed()
		{
			return m_GeomData.IsClosed;
		}

		public int GetStartPointCADIndex()
		{
			if( m_PathCache.MainPathPointList.Count > 0 ) {
				return m_PathCache.MainPathPointList[ 0 ].InitPathIndex;
			}
			return NULL_SELECT_INDEX;
		}

		readonly CraftData m_CraftData;
		readonly ContourCache m_PathCache;
		readonly ContourGeomData m_GeomData;
		const int NULL_SELECT_INDEX = -999;
		const int CLOSED_POINT_INDEX = -1;
	}
}
