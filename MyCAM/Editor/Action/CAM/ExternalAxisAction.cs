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
		public ExternalAxisEditData( double? ext1, double? ext2, string previewPathID )
		{
			new ExternalAxisPos( ext1 ?? 0, ext2 ?? 0 );
			Ext1 = ext1;
			Ext2 = ext2;
			PreviewPathID = previewPathID;
		}

		public double? Ext1 { get; }
		public double? Ext2 { get; }
		public string PreviewPathID { get; }
	}

	internal sealed class ExternalAxisAction : EditCAMActionBase
	{
		public ExternalAxisAction( DataManager dataManager, List<string> pathIDList )
			: base( dataManager, new List<string>( pathIDList ) )
		{
			m_MachineData = dataManager.MachineData as FiveAxisMachineData;
			if( m_MachineData == null || m_MachineData.ExternalAxes.Type == ExternalAxisType.None ) {
				throw new ArgumentException( "External axis programming requires an enabled five-axis external axis configuration." );
			}
			m_MachineData.ExternalAxes.ValidateFor( m_MachineData.FiveAxisType );
			m_BackupPositions = m_CraftDataList.Select( craft => craft.ExternalAxisPos ).ToList();
		}

		public Action<Dictionary<MachineComponentType, List<gp_Trsf>>, bool> RaiseTrans;
		public Action NeedOptimizedIK;

		public override EditActionType ActionType => EditActionType.ExternalAxis;

		public override void Start()
		{
			base.Start();
			List<string> names = m_PathIDList.Select( id => m_DataManager.GetPathName( m_DataManager.PathIDList.IndexOf( id ) ) ).ToList();
			m_Dlg = new ExternalAxisDlg( m_MachineData.ExternalAxes.Type, m_PathIDList, names, m_BackupPositions );
			m_Dlg.Preview = PreviewPositions;
			m_Dlg.Confirm = ConfirmPositions;
			m_Dlg.Cancel = End;
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( m_IsEnded ) {
				return;
			}
			m_IsEnded = true;
			if( !m_IsConfirmed ) {
				for( int i = 0; i < m_CraftDataList.Count; i++ ) {
					m_CraftDataList[ i ].ExternalAxisPos = m_BackupPositions[ i ];
				}
			}
			if( m_Dlg != null ) {
				m_Dlg.Preview = null;
				m_Dlg.Confirm = null;
				m_Dlg.Cancel = null;
				m_Dlg.Close();
				m_Dlg.Dispose();
				m_Dlg = null;
			}
			PropertyChanged?.Invoke( m_PathIDList );
			base.End();
		}

		void ApplyPositions( ExternalAxisEditData data )
		{
			if( data == null || !m_PathIDList.Contains( data.PreviewPathID ) ) {
				throw new ArgumentException( "The preview path must be in the edited selection." );
			}
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				ExternalAxisPos original = m_BackupPositions[ i ];
				m_CraftDataList[ i ].ExternalAxisPos = new ExternalAxisPos( data.Ext1 ?? original.Ext1,
					m_MachineData.ExternalAxes.Type == ExternalAxisType.Dual ? data.Ext2 ?? original.Ext2 : original.Ext2 );
			}
		}

		void PreviewPositions( ExternalAxisEditData data )
		{
			ApplyPositions( data );
			PropertyChanged?.Invoke( m_PathIDList );
			if( !BuildPreview( data.PreviewPathID, out Dictionary<MachineComponentType, List<gp_Trsf>> frames ) ) {
				MyApp.Logger.ShowOnLogPanel( m_LanguageManager.TryGetString( "CannotSimulatePosture", "[操作提醒]無法模擬此姿態" ), MyApp.NoticeType.Warning );
				return;
			}
			RaiseTrans?.Invoke( frames, true );
		}

		bool BuildPreview( string pathID, out Dictionary<MachineComponentType, List<gp_Trsf>> frames )
		{
			frames = null;
			if( !m_PathIDList.Contains( pathID ) ) {
				return false;
			}
			CAMPoint point = CacheHelper.GetProcessStartPoint( pathID );
			if( point == null ) {
				return false;
			}
			PostPoint orientation = new PostPoint { Master = point.ModMaster_rad, Slave = point.ModSlave_rad, ExternalAxisPos = point.ExternalAxisPos };
			return SimuPathBuilderFactory.Create( m_MachineData ).BuildSinglePointFrameTransform( m_MachineData, point.Point, orientation, out frames );
		}

		void ConfirmPositions( ExternalAxisEditData data )
		{
			ApplyPositions( data );
			m_IsConfirmed = true;
			End();
			NeedOptimizedIK?.Invoke();
		}

		readonly FiveAxisMachineData m_MachineData;
		readonly List<ExternalAxisPos> m_BackupPositions;
		readonly LanguageManager.LanguageManager m_LanguageManager = new LanguageManager.LanguageManager( "ExternalAxisDlg" );
		ExternalAxisDlg m_Dlg;
		bool m_IsConfirmed;
		bool m_IsEnded;
	}
}
