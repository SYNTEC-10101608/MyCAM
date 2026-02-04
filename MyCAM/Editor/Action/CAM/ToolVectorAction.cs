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
			m_nSelectIndex = DEFAULT_SELECT_INDEX;
			m_SelectedPoint = null;
			m_ToolVecParam = null;

			// init dialog
			m_ToolVecDlg = new ToolVectorDlg( m_CraftData.InterpolateType, m_ToolVecParam, m_CraftData.IsPathReverse );
			m_ToolVecDlg.SetKeep += () => OnSetKeep();
			m_ToolVecDlg.SetZdir += () => OnSetZDir();
			m_ToolVecDlg.SetRevert += () => OnSetRevert();
			m_ToolVecDlg.ABAngleChanged = ( angleA, angleB ) => OnABAngleChanged( angleA, angleB );
			m_ToolVecDlg.MSAngleChanged = ( master, slave ) => OnMSAngleChanged( master, slave );
			m_ToolVecDlg.TypeChanged += ( type ) => OnTypeChanged( type );
			m_ToolVecDlg.AddEditIndex += () => OnAddEditIndex();
			m_ToolVecDlg.RemoveEditIndex += () => OnRemoveEditIndex();

			// TODO: lock the main form when editing
			RaiseEditingToolVecDlg?.Invoke( EActionStatus.Start );
			m_ToolVecDlg.Show( MyApp.MainForm );
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
			m_nSelectIndex = GetSelectIndex( out TopoDS_Shape selectedVertex );
			if( m_nSelectIndex == DEFAULT_SELECT_INDEX ) {
				return;
			}

			// update index param
			m_SelectedPoint = m_DataHandler.GetPointByCADIndex( m_nSelectIndex );
			bool isModified = m_DataHandler.GetToolVecModify( m_nSelectIndex, out double angleA_deg, out double angleB_deg, out double master_deg, out double slave_deg );
			m_ToolVecParam = new ToolVecParam( angleA_deg, angleB_deg, master_deg, slave_deg, isModified );

			// update dialog
			m_ToolVecDlg.ResetToolVecParam( m_ToolVecParam );

			// lock selected vertex high light
			LockSelectedVertexHighLight( selectedVertex );
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
		SolveTargetResult OnSetRevert()
		{
			SolveTargetResult result = new SolveTargetResult() { IsValid = false };
			if( m_SelectedPoint == null ) {
				return result;
			}

			// point exists, get original MS angles
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = m_SelectedPoint.InitMaster_rad * 180.0 / Math.PI;
				m_ToolVecParam.Slave_deg = m_SelectedPoint.InitSlave_rad * 180.0 / Math.PI;
				m_ToolVecParam.AngleA_deg = 0;
				m_ToolVecParam.AngleB_deg = 0;
			}
			SetToolVecParamAndPeview();

			// return result
			return new SolveTargetResult()
			{
				IsValid = true,
				AngleA_deg = 0,
				AngleB_deg = 0,
				Master_deg = m_SelectedPoint.InitMaster_rad * 180.0 / Math.PI,
				Slave_deg = m_SelectedPoint.InitSlave_rad * 180.0 / Math.PI
			};
		}

		SolveTargetResult OnSetKeep()
		{
			SolveTargetResult result = new SolveTargetResult() { IsValid = false };
			if( m_nSelectIndex == DEFAULT_SELECT_INDEX ) {
				return result;
			}
			bool getAngleOK = CalABAngleToKeep( m_nSelectIndex, out Tuple<double, double> abAngles_deg );
			bool checkRangeOK = CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 );
			if( !getAngleOK || !checkRangeOK ) {
				return result;
			}

			// Calculate MS angles from AB angles
			if( m_SelectedPoint == null ) {
				return result;
			}
			Tuple<double, double> msAngles_deg = ToolVecHelper.GetMSAngleFromABAngle( abAngles_deg.Item1, abAngles_deg.Item2, m_SelectedPoint );

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = msAngles_deg.Item1;
				m_ToolVecParam.Slave_deg = msAngles_deg.Item2;
				m_ToolVecParam.AngleA_deg = abAngles_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngles_deg.Item2;
			}
			SetToolVecParamAndPeview();

			// return result
			return new SolveTargetResult()
			{
				IsValid = true,
				AngleA_deg = abAngles_deg.Item1,
				AngleB_deg = abAngles_deg.Item2,
				Master_deg = msAngles_deg.Item1,
				Slave_deg = msAngles_deg.Item2
			};
		}

		SolveTargetResult OnSetZDir()
		{
			SolveTargetResult result = new SolveTargetResult() { IsValid = false };
			bool getAngleOK = CalABAngleToZDir( out Tuple<double, double> abAngles_deg );
			bool checkRangeOK = CheckABAngleRange( abAngles_deg.Item1, abAngles_deg.Item2 );
			if( !getAngleOK || !checkRangeOK ) {
				return result;
			}

			// Calculate MS angles from AB angles
			if( m_SelectedPoint == null ) {
				return result;
			}
			Tuple<double, double> msAngles_deg = ToolVecHelper.GetMSAngleFromABAngle( abAngles_deg.Item1, abAngles_deg.Item2, m_SelectedPoint );

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.Master_deg = msAngles_deg.Item1;
				m_ToolVecParam.Slave_deg = msAngles_deg.Item2;
				m_ToolVecParam.AngleA_deg = abAngles_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngles_deg.Item2;
			}
			SetToolVecParamAndPeview();

			// return result
			return new SolveTargetResult()
			{
				IsValid = true,
				AngleA_deg = abAngles_deg.Item1,
				AngleB_deg = abAngles_deg.Item2,
				Master_deg = msAngles_deg.Item1,
				Slave_deg = msAngles_deg.Item2
			};
		}

		SolveMSResult OnABAngleChanged( double angleA_deg, double angleB_deg )
		{
			SolveMSResult result = new SolveMSResult() { IsValid = false };

			// check angle range
			if( !CheckABAngleRange( angleA_deg, angleB_deg ) ) {
				return result;
			}
			if( m_SelectedPoint == null ) {
				return result;
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

			// return result
			return new SolveMSResult()
			{
				IsValid = true,
				Master_deg = msAngle_deg.Item1,
				Slave_deg = msAngle_deg.Item2
			};
		}

		SolveABResult OnMSAngleChanged( double master_deg, double slave_deg )
		{
			SolveABResult result = new SolveABResult() { IsValid = false };
			if( m_SelectedPoint == null ) {
				return result;
			}
			Tuple<double, double> abAngle_deg = ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, m_SelectedPoint );

			// check angle range
			if( !CheckABAngleRange( abAngle_deg.Item1, abAngle_deg.Item2 ) ) {
				return result;
			}

			// valid result, trigger update
			if( m_ToolVecParam != null ) {
				m_ToolVecParam.AngleA_deg = abAngle_deg.Item1;
				m_ToolVecParam.AngleB_deg = abAngle_deg.Item2;
				m_ToolVecParam.Master_deg = master_deg;
				m_ToolVecParam.Slave_deg = slave_deg;
			}
			SetToolVecParamAndPeview();

			return new SolveABResult()
			{
				IsValid = true,
				AngleA_deg = abAngle_deg.Item1,
				AngleB_deg = abAngle_deg.Item2
			};
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

		bool CheckABAngleRange( double angleA_deg, double angleB_deg )
		{
			if( angleA_deg < MIN_TiltAngle || angleA_deg > MAX_TiltAngle ||
				angleB_deg < MIN_TiltAngle || angleB_deg > MAX_TiltAngle ) {
				MyApp.Logger.ShowOnLogPanel( "角度必須在 -60~+60 範圍內", MyApp.NoticeType.Warning );
				return false;
			}
			return true;
		}

		bool CalABAngleToKeep( int nSelectIndex, out Tuple<double, double> abAngle_deg )
		{
			// get previous control point tool vector
			gp_Dir assignDir = GetPreCtrlPntToolVec( m_CraftData.ToolVecModifyMap, nSelectIndex, m_CraftData.IsPathReverse, m_DataHandler.IsClosed() );
			return CalculateABAngleToTarget( assignDir, out abAngle_deg );
		}

		bool CalABAngleToZDir( out Tuple<double, double> abAngle_deg )
		{
			gp_Dir assignDir = new gp_Dir( 0, 0, 1 );
			return CalculateABAngleToTarget( assignDir, out abAngle_deg );
		}

		bool CalculateABAngleToTarget( gp_Dir assignDir, out Tuple<double, double> abAngle_deg )
		{
			if( m_SelectedPoint == null ) {
				abAngle_deg = new Tuple<double, double>( 0, 0 );
				return false;
			}
			ToolVecHelper.ECalAngleResult calResult = ToolVecHelper.GetABAngleFromToolVec( assignDir, m_SelectedPoint, out abAngle_deg );
			if( calResult == ToolVecHelper.ECalAngleResult.Done ) {
				return true;
			}
			if( calResult == ToolVecHelper.ECalAngleResult.TooLargeAngle ) {
				MyApp.Logger.ShowOnLogPanel( "目標向量與原始向量夾角過大", MyApp.NoticeType.Warning, true );
				return false;
			}
			return false;
		}

		gp_Dir GetPreCtrlPntToolVec( IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, int nTargetPntIdx, bool isPathReverse, bool isClosePath )
		{
			List<int> ctrlPntIndexList = toolVecModifyMap.Keys.ToList();
			int preCtrlIndex = GetPreCtrlPntIndex( nTargetPntIdx, ctrlPntIndexList, isPathReverse, isClosePath );
			ISetToolVecPoint preCtrlPoint = m_DataHandler.GetPointByCADIndex( preCtrlIndex );
			return new gp_Dir( preCtrlPoint.ToolVec.XYZ() );
		}

		int GetPreCtrlPntIndex( int targetIndex, List<int> ctrlPntIndexList, bool isReverse, bool isClosePath )
		{
			// keep the list in order
			ctrlPntIndexList.Sort();
			int result = DEFAULT_SELECT_INDEX;

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
				if( result == DEFAULT_SELECT_INDEX && ctrlPntIndexList.Count > 0 ) {
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
				if( result == DEFAULT_SELECT_INDEX && ctrlPntIndexList.Count > 0 ) {
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
			if( m_ToolVecParam.IsModified || bForceUpdate ) {
				PropertyChanged?.Invoke( m_PathIDList );
			}
		}

		void SetInterpolateType()
		{
			m_CraftData.InterpolateType = m_InterpolateType;
		}

		void SetIndexAngleParam()
		{
			// remove modify data
			if( !m_ToolVecParam.IsModified ) {
				m_CraftData.RemoveToolVecModify( m_nSelectIndex );
				return;
			}

			// set modify data
			m_CraftData.SetToolVecModify( m_nSelectIndex,
				m_ToolVecParam.AngleA_deg, m_ToolVecParam.AngleB_deg, m_ToolVecParam.Master_deg, m_ToolVecParam.Slave_deg );
		}


		// index param
		int m_nSelectIndex = DEFAULT_SELECT_INDEX;
		ToolVecParam m_ToolVecParam = null;
		ISetToolVecPoint m_SelectedPoint = null;

		// global param
		EToolVecInterpolateType m_InterpolateType = EToolVecInterpolateType.Normal;
		ToolVecActionDataHandler m_DataHandler = null;
		CraftData m_CraftData = null;

		// action data
		List<string> m_PathIDList = null;
		AIS_Shape m_KeepedHighLightPoint = null;
		ToolVectorDlg m_ToolVecDlg = null;

		// angle limit
		public const double MAX_TiltAngle = 60.0;
		public const double MIN_TiltAngle = -60.0;
		const int DEFAULT_SELECT_INDEX = -1;
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
				if( m_PathCache.CADToCAMIndexMap.ContainsKey( index ) ) {
					int camIndex = m_PathCache.CADToCAMIndexMap[ index ];

					// get master and slave from InitIKResult and convert rad to deg
					if( camIndex >= 0 && camIndex < m_PathCache.MainPathPointList.Count ) {
						master_deg = m_PathCache.MainPathPointList[ camIndex ].InitMaster_rad * 180.0 / Math.PI;
						slave_deg = m_PathCache.MainPathPointList[ camIndex ].InitSlave_rad * 180.0 / Math.PI;
					}
					else {
						master_deg = 0;
						slave_deg = 0;
					}
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
			return null;
		}

		public bool IsClosed()
		{
			return m_GeomData.IsClosed;
		}

		readonly CraftData m_CraftData;
		readonly ContourCache m_PathCache;
		readonly ContourGeomData m_GeomData;
	}
}
