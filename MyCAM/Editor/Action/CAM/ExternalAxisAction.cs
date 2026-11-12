using MyCAM.App;
using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.gp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor
{
	internal sealed class ExternalAxisEditData
	{
		public ExternalAxisEditData( double? ext1, double? ext2 )
		{
			new ExternalAxisPos( ext1 ?? 0, ext2 ?? 0 );
			Ext1 = ext1;
			Ext2 = ext2;
		}

		public double? Ext1 { get; }
		public double? Ext2 { get; }
	}

	internal sealed class ExternalAxisAction : EditActionBase
	{
		public ExternalAxisAction( DataManager dataManager, List<string> pathIDList, SelectPathAction pathIndexAction )
			: base( dataManager )
		{
			m_PathIndexAction = pathIndexAction ?? throw new ArgumentNullException( nameof( pathIndexAction ) );
			m_MachineData = dataManager.MachineData as FiveAxisMachineData;
			if( m_MachineData == null || m_MachineData.ExternalAxes.Type == ExternalAxisType.None ) {
				throw new ArgumentException( "External axis programming requires an enabled five-axis external axis configuration." );
			}
			m_MachineData.ExternalAxes.ValidateFor( m_MachineData.FiveAxisType );
			m_CurrentPathIDList = GetEditablePaths( pathIDList );
		}

		public Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool> RaiseTrans;
		public Action<bool> RaiseActionStart;
		public Action NeedOptimizedIK;
		public Action<List<string>> PropertyChanged;

		public override EditActionType ActionType => EditActionType.ExternalAxis;

		public override void Start()
		{
			base.Start();
			m_Dlg = new ExternalAxisDlg( m_MachineData.ExternalAxes.Type, GetPositions() );
			m_Dlg.PositionsChanged += OnPositionsChanged;
			m_Dlg.PreviewRequested += RefreshPreview;
			m_Dlg.Cancel += End;
			m_PathIndexAction.ExitSingleSelectMode();
			m_PathIndexAction.Start();
			m_IsSelectionStarted = true;
			m_PathIndexAction.SelectionChange += OnPathSelectionChanged;
			m_CurrentPathIDList = GetEditablePaths( m_PathIndexAction.GetSelectedIDs() );
			m_Dlg.SetPositions( GetPositions() );
			RaiseActionStart?.Invoke( true );
			m_Dlg.Show( MyApp.MainForm );
			RefreshPreview();
		}

		public override void End()
		{
			if( m_IsEnded ) {
				return;
			}
			m_IsEnded = true;
			if( m_IsSelectionStarted ) {
				m_PathIndexAction.SelectionChange -= OnPathSelectionChanged;
				m_PathIndexAction.End();
				m_IsSelectionStarted = false;
			}
			if( m_Dlg != null ) {
				m_Dlg.PositionsChanged -= OnPositionsChanged;
				m_Dlg.PreviewRequested -= RefreshPreview;
				m_Dlg.Cancel -= End;
				m_Dlg.Close();
				m_Dlg.Dispose();
				m_Dlg = null;
			}
			RaiseActionStart?.Invoke( false );
			PropertyChanged?.Invoke( m_CurrentPathIDList );
			base.End();
			NeedOptimizedIK?.Invoke();
		}

		List<string> GetEditablePaths( List<string> pathIDs )
		{
			return ( pathIDs ?? new List<string>() ).Where( id => m_DataManager.PathIDList.Contains( id )
				&& DataGettingHelper.GetCraftDataByID( id, out CraftData craft ) ).Distinct().ToList();
		}

		List<ExternalAxisPos> GetPositions()
		{
			return m_CurrentPathIDList.Select( id => {
				DataGettingHelper.GetCraftDataByID( id, out CraftData craft );
				return craft.ExternalAxisPos;
			} ).ToList();
		}

		void OnPathSelectionChanged()
		{
			m_CurrentPathIDList = GetEditablePaths( m_PathIndexAction.GetSelectedIDs() );
			if( m_CurrentPathIDList.Count > 0 ) {
				NeedOptimizedIK?.Invoke();
			}
			m_Dlg.SetPositions( GetPositions() );
			RefreshPreview();
		}

		void ApplyPositions( ExternalAxisEditData data )
		{
			if( data == null ) {
				throw new ArgumentNullException( nameof( data ) );
			}
			foreach( string pathID in m_CurrentPathIDList ) {
				if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craft ) ) {
					continue;
				}
				ExternalAxisPos current = craft.ExternalAxisPos;
				craft.ExternalAxisPos = new ExternalAxisPos( data.Ext1 ?? current.Ext1,
					m_MachineData.ExternalAxes.Type == ExternalAxisType.Dual ? data.Ext2 ?? current.Ext2 : current.Ext2 );
			}
		}

		void OnPositionsChanged( ExternalAxisEditData data )
		{
			ApplyPositions( data );
			RefreshPreview();
		}

		void RefreshPreview()
		{
			if( m_CurrentPathIDList.Count == 0 ) {
				PropertyChanged?.Invoke( m_CurrentPathIDList );
				return;
			}
			if( !BuildPreview( out Dictionary<MachineComponentType, List<gp_Trsf>> frames ) ) {
				PropertyChanged?.Invoke( m_CurrentPathIDList );
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotSimulatePosture", "[操作提醒]無法模擬此姿態" ), MyApp.NoticeType.Warning );
				return;
			}
			RaiseTrans?.Invoke( frames, true );
		}

		bool BuildPreview( out Dictionary<MachineComponentType, List<gp_Trsf>> frames )
		{
			frames = null;
			if( m_CurrentPathIDList.Count == 0 ) {
				return false;
			}
			CAMPoint point = CacheHelper.GetProcessStartPoint( m_CurrentPathIDList[ 0 ] );
			if( point == null ) {
				return false;
			}
			PostPoint orientation = new PostPoint { Master = point.ModMaster_rad, Slave = point.ModSlave_rad, ExternalAxisPos = point.ExternalAxisPos };
			return SimuPathBuilderFactory.Create( m_MachineData ).BuildSinglePointFrameTransform( m_MachineData, point.Point, orientation, out frames );
		}

		readonly FiveAxisMachineData m_MachineData;
		readonly SelectPathAction m_PathIndexAction;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExternalAxisDlg" );
		List<string> m_CurrentPathIDList;
		ExternalAxisDlg m_Dlg;
		bool m_IsSelectionStarted;
		bool m_IsEnded;
	}
}
