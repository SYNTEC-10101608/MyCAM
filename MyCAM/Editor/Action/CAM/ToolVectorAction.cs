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
			// checked in base constructor
			m_PathIDList = new List<string>() { pathID };
			if( !DataGettingHelper.GetCraftDataByID( pathID, out m_CraftData ) ) {
				throw new ArgumentException( "Cannot get CraftData by pathID: " + pathID );
			}
			if( !PathCacheProvider.TryGetToolVecCache( pathID, out m_ToolVecCache ) ) {
				throw new ArgumentException( "Cannot get ToolVecCache by pathID: " + pathID );
			}
			if( !DataGettingHelper.GetGeomDataByID( pathID, out m_GeomData ) ) {
				throw new ArgumentException( "Cannot get GeomData by pathID: " + pathID );
			}
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
			bool isModified = m_ToolVecCache.GetToolVecModify( nIndex, out double angleA_deg, out double angleB_deg );
			ToolVecParam toolVecParam = new ToolVecParam( isModified, angleA_deg, angleB_deg, m_CraftData.InterpolateType );

			// back up old data
			m_BackupToolVecParam = new ToolVecParam( isModified, angleA_deg, angleB_deg, m_CraftData.InterpolateType );
			ToolVectorDlg toolVecForm = new ToolVectorDlg( toolVecParam, false );
			toolVecForm.RaiseKeep += () => SetToolVecOfKeep( nIndex, toolVecForm );
			toolVecForm.RaiseZDir += () => SetToolVecOfZDir( nIndex, toolVecForm );
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
			m_CraftData.SetToolVecModify( VecIndex, toolVecParam.AngleA_deg, toolVecParam.AngleB_deg );
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

		bool CalABAngleToPreCtrlPntToolVec( int nSelectIndex, out Tuple<double, double> param )
		{
			// get this modify point cam point
			CADPoint toModifyCADPnt = m_ProcessCADPointList[ nSelectIndex ];
			CAMPoint toModifyCAMPnt = new CAMPoint( toModifyCADPnt );

			// get previous control point tool vector
			gp_Dir assignDir = GetPreCtrlPntToolVec( m_ProcessCADPointList, m_CraftData.ToolVecModifyMap, nSelectIndex, m_CraftData.IsToolVecReverse, m_CraftData.IsPathReverse, m_GeomData.IsClosed );
			ToolVecHelper.ECalAngleResult calResult = ToolVecHelper.GetABAngleToTargetVec( assignDir, toModifyCAMPnt, m_CraftData.IsToolVecReverse, out param );
			if( calResult == ToolVecHelper.ECalAngleResult.Done ) {
				return true;
			}
			if( calResult == ToolVecHelper.ECalAngleResult.TooLargeAngle ) {
				MyApp.Logger.ShowOnLogPanel( "目標向量與原始向量夾角過大", MyApp.NoticeType.Warning );
				MessageBox.Show( "目標向量與原始向量夾角過大", "計算失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning );
				return false;
			}
			return false;
		}

		bool CalABAngleToZDir( int nSelectIndex, out Tuple<double, double> param )
		{
			// get this modify point cam point
			CADPoint toModifyCADPnt = m_ProcessCADPointList[ nSelectIndex ];
			CAMPoint toModifyCAMPnt = new CAMPoint( toModifyCADPnt );
			gp_Dir assignDir = new gp_Dir( 0, 0, 1 );
			ToolVecHelper.ECalAngleResult calResult = ToolVecHelper.GetABAngleToTargetVec( assignDir, toModifyCAMPnt, m_CraftData.IsToolVecReverse, out param );
			if( calResult == ToolVecHelper.ECalAngleResult.Done ) {
				return true;
			}
			if( calResult == ToolVecHelper.ECalAngleResult.TooLargeAngle ) {
				MyApp.Logger.ShowOnLogPanel( "目標向量與原始向量夾角過大", MyApp.NoticeType.Warning );
				MessageBox.Show( "目標向量與原始向量夾角過大", "計算失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning );
				return false;
			}
			return false;
		}

		void SetToolVecOfKeep( int nSelectIndex, ToolVectorDlg toolVecForm )
		{
			bool GetParamSuccess = CalABAngleToPreCtrlPntToolVec( nSelectIndex, out Tuple<double, double> param );
			if( GetParamSuccess ) {
				toolVecForm.SetParamBack( param );
			}
		}

		void SetToolVecOfZDir( int nSelectIndex, ToolVectorDlg toolVecForm )
		{
			bool GetParamSuccess = CalABAngleToZDir( nSelectIndex, out Tuple<double, double> param );
			if( GetParamSuccess ) {
				toolVecForm.SetParamBack( param );
			}
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
			// fixdir can't remove point
			if( toolVecParam.InterpolateType == EToolVecInterpolateType.FixedDir ) {

				// this point originally is not modify point
				if( m_BackupToolVecParam.IsModified == false ) {

					// while dialog show up, user click other solution , will add this pnt modify
					m_CraftData.RemoveToolVecModify( VecIndex );
				}

				// this pnt originally is modify point
				else {

					// set original value back
					SetABAngle( VecIndex, m_BackupToolVecParam );
				}
			}
			else {

				// user remove pnt
				if( toolVecParam.IsModified == false ) {
					m_CraftData.RemoveToolVecModify( VecIndex );
				}

				// user add / adjust pnt
				else {
					SetABAngle( VecIndex, toolVecParam );
				}
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

		gp_Dir GetPreCtrlPntToolVec( List<CADPoint> oriCADPntList, IReadOnlyDictionary<int, Tuple<double, double>> toolVecModifyMap, int nTargetPntIdx, bool isToolVecReverse, bool isPathReverse, bool isClosePath )
		{
			List<int> ctrlPntIndexList = toolVecModifyMap.Keys.ToList();
			int preCtrlIndex = GetPreCtrlPntIndex( nTargetPntIdx, ctrlPntIndexList, isPathReverse, isClosePath );

			// do not have previous control point, return the target point tool vector
			if( preCtrlIndex == DEFAULT_SELECT_INDEX || preCtrlIndex == nTargetPntIdx ) {
				return oriCADPntList[ nTargetPntIdx ].NormalVec_1st;
			}
			Tuple<double, double> ctrlPntInfo = toolVecModifyMap[ preCtrlIndex ];
			CAMPoint preCtrlCAMPnt = new CAMPoint( oriCADPntList[ preCtrlIndex ] );
			gp_Vec targetVec = ToolVecHelper.GetVecFromABAngle( preCtrlCAMPnt,
				ctrlPntInfo.Item1 * Math.PI / 180.0,
				ctrlPntInfo.Item2 * Math.PI / 180.0,
				isToolVecReverse );
			return new gp_Dir( targetVec.XYZ() );
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
		CraftData m_CraftData = null;
		IToolVecCache m_ToolVecCache = null;
		List<string> m_PathIDList = null;
		const int DEFAULT_SELECT_INDEX = -1;
		IGeomData m_GeomData;
	}
}
