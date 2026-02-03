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

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			// editing tool vector, do not allow other operation
			if( IsPausedSelectMode ) {
				return;
			}
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int nIndex = GetSelectIndex( out TopoDS_Shape selectedVertex );
			if( nIndex == DEFAULT_SELECT_INDEX ) {
				return;
			}

			// modify tool vector
			bool isModified = m_DataHandler.GetToolVecModify( nIndex, out double angleA_deg, out double angleB_deg, out double master_deg, out double slave_deg );
			ToolVecParam toolVecParam = new ToolVecParam( isModified, angleA_deg, angleB_deg, master_deg, slave_deg, m_CraftData.InterpolateType );

			// back up old data
			m_BackupToolVecParam = new ToolVecParam( isModified, angleA_deg, angleB_deg, master_deg, slave_deg, m_CraftData.InterpolateType );
			ToolVectorDlg toolVecForm = new ToolVectorDlg( toolVecParam, m_CraftData.IsPathReverse );
			toolVecForm.RaiseKeep += () => SetToolVecOfKeep( nIndex, toolVecForm );
			toolVecForm.RaiseZDir += () => SetToolVecOfZDir( nIndex, toolVecForm );
			toolVecForm.RaiseCalculateMSAngleFromABAngle = ( angleA, angleB ) => CalculateMSAngleFromABAngle( nIndex, angleA, angleB );
			toolVecForm.RaiseCalculateABAngleFromMSAngle = ( master, slave ) => CalculateABAngleFromMSAngle( nIndex, master, slave );
			toolVecForm.Preview += ( ToolVec ) => SetToolVecParamAndPeview( nIndex, ToolVec );
			toolVecForm.Confirm += ( ToolVec ) => ConfirmSetting( nIndex, ToolVec );
			toolVecForm.Cancel += () => CancelSetting( nIndex );

			// when editing a point lock the main form
			RaiseEditingToolVecDlg?.Invoke( EActionStatus.Start );

			// when editing a point, cannot select other points but still show selected point
			LockSelectedVertexHighLight( selectedVertex );
			toolVecForm.Show( MyApp.MainForm );
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
			Pause();
		}

		void UnlockSelectedVertexHighLight()
		{
			// remove selected vertex which show on viewer
			if( m_KeepedHighLightPoint != null ) {

				// true means update viewer
				m_Viewer.GetAISContext().Remove( m_KeepedHighLightPoint, true );
				m_KeepedHighLightPoint = null;
			}
			Resume();
		}

		void SetToolVecDone()
		{
			// unlock main form
			RaiseEditingToolVecDlg?.Invoke( EActionStatus.End );

			// unlock viewer
			UnlockSelectedVertexHighLight();
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void SetInterpolateType( ToolVecParam toolVecParam )
		{
			m_CraftData.InterpolateType = toolVecParam.InterpolateType;
		}

		void SetABAngle( int VecIndex, ToolVecParam toolVecParam )
		{
			m_CraftData.SetToolVecModify( VecIndex, toolVecParam.AngleA_deg, toolVecParam.AngleB_deg, toolVecParam.Master_deg, toolVecParam.Slave_deg );
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

		bool CalABAngleToKeep( int nSelectIndex, out Tuple<double, double> abAngle_deg )
		{
			// get this modify point cam point
			ISetToolVecPoint pointToModify = m_DataHandler.GetPointByCADIndex( nSelectIndex ).Clone();

			// get previous control point tool vector
			gp_Dir assignDir = GetPreCtrlPntToolVec( m_CraftData.ToolVecModifyMap, nSelectIndex, m_CraftData.IsPathReverse, m_DataHandler.IsClosed() );
			ToolVecHelper.ECalAngleResult calResult = ToolVecHelper.GetABAngleFromToolVec( assignDir, pointToModify, out abAngle_deg );
			if( calResult == ToolVecHelper.ECalAngleResult.Done ) {
				return true;
			}
			if( calResult == ToolVecHelper.ECalAngleResult.TooLargeAngle ) {
				MyApp.Logger.ShowOnLogPanel( "目標向量與原始向量夾角過大", MyApp.NoticeType.Warning, true );
				return false;
			}
			return false;
		}

		bool CalABAngleToZDir( int nSelectIndex, out Tuple<double, double> abAngle_deg )
		{
			// get this modify point cam point
			ISetToolVecPoint pointToModify = m_DataHandler.GetPointByCADIndex( nSelectIndex ).Clone();
			gp_Dir assignDir = new gp_Dir( 0, 0, 1 );
			ToolVecHelper.ECalAngleResult calResult = ToolVecHelper.GetABAngleFromToolVec( assignDir, pointToModify, out abAngle_deg );
			if( calResult == ToolVecHelper.ECalAngleResult.Done ) {
				return true;
			}
			if( calResult == ToolVecHelper.ECalAngleResult.TooLargeAngle ) {
				MyApp.Logger.ShowOnLogPanel( "目標向量與原始向量夾角過大", MyApp.NoticeType.Warning, true );
				return false;
			}
			return false;
		}

		void SetToolVecOfKeep( int nSelectIndex, ToolVectorDlg toolVecForm )
		{
			bool GetParamSuccess = CalABAngleToKeep( nSelectIndex, out Tuple<double, double> abAngles_deg );
			if( GetParamSuccess ) {

				// Calculate MS angles from AB angles
				Tuple<double, double> msAngles_deg = CalculateMSAngleFromABAngle( nSelectIndex, abAngles_deg.Item1, abAngles_deg.Item2 );
				toolVecForm.SetAngleFromTargetVec( abAngles_deg, msAngles_deg );
			}
		}

		void SetToolVecOfZDir( int nSelectIndex, ToolVectorDlg toolVecForm )
		{
			bool GetParamSuccess = CalABAngleToZDir( nSelectIndex, out Tuple<double, double> abAngles_deg );
			if( GetParamSuccess ) {

				// Calculate MS angles from AB angles
				Tuple<double, double> msAngles_deg = CalculateMSAngleFromABAngle( nSelectIndex, abAngles_deg.Item1, abAngles_deg.Item2 );
				toolVecForm.SetAngleFromTargetVec( abAngles_deg, msAngles_deg );
			}
		}

		Tuple<double, double> CalculateMSAngleFromABAngle( int nSelectIndex, double angleA_deg, double angleB_deg )
		{
			ISetToolVecPoint point = m_DataHandler.GetPointByCADIndex( nSelectIndex ).Clone();
			return ToolVecHelper.GetMSAngleFromABAngle( angleA_deg, angleB_deg, point );
		}

		Tuple<double, double> CalculateABAngleFromMSAngle( int nSelectIndex, double master_deg, double slave_deg )
		{
			ISetToolVecPoint point = m_DataHandler.GetPointByCADIndex( nSelectIndex ).Clone();
			return ToolVecHelper.GetABAngleFromMSAngle( master_deg, slave_deg, point );
		}

		void SetToolVecParamAndPeview( int VecIndex, ToolVecParam toolVecParam )
		{
			SetInterpolateType( toolVecParam );
			SetABAngle( VecIndex, toolVecParam );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		// remove or ok button clicked
		void ConfirmSetting( int VecIndex, ToolVecParam toolVecParam )
		{
			// user remove pnt
			if( toolVecParam.IsModified == false ) {
				m_CraftData.RemoveToolVecModify( VecIndex );
			}
			// user add / adjust pnt
			else {
				SetABAngle( VecIndex, toolVecParam );
			}

			SetInterpolateType( toolVecParam );
			SetToolVecDone();
		}

		void CancelSetting( int VecIndex )
		{
			// this point is not modify point when dialog show up
			if( m_BackupToolVecParam.IsModified == false ) {
				m_CraftData.RemoveToolVecModify( VecIndex );
			}
			else {
				SetABAngle( VecIndex, m_BackupToolVecParam );
			}
			SetInterpolateType( m_BackupToolVecParam );
			SetToolVecDone();
		}

		gp_Dir GetPreCtrlPntToolVec( IReadOnlyDictionary<int, ToolVecModifyData> toolVecModifyMap, int nTargetPntIdx, bool isPathReverse, bool isClosePath )
		{
			List<int> ctrlPntIndexList = toolVecModifyMap.Keys.ToList();
			int preCtrlIndex = GetPreCtrlPntIndex( nTargetPntIdx, ctrlPntIndexList, isPathReverse, isClosePath );
			ISetToolVecPoint preCtrlPoint = m_DataHandler.GetPointByCADIndex( preCtrlIndex ).Clone();
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

		ToolVecParam m_BackupToolVecParam;

		// to storage which vertex keep show high light point on viewer
		AIS_Shape m_KeepedHighLightPoint = null;
		ToolVecActionDataHandler m_DataHandler = null;
		CraftData m_CraftData;
		List<string> m_PathIDList = null;
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

		public EToolVecInterpolateType GetInterpolateType()
		{
			return m_CraftData.InterpolateType;
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
