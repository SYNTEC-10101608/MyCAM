using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using System;
using System.Collections.Generic;

namespace MyCAM.Editor
{
	internal class OverCutAction : EditCAMActionBase
	{
		public OverCutAction( DataManager dataManager, List<string> pathIDList )
		: base( dataManager, pathIDList )
		{
			// checked in base constructor
			m_dOverCutBackupList = new List<double>();
			foreach( var craftData in m_CraftDataList ) {
				if( craftData == null ) {
					throw new ArgumentNullException( "OverCutAction constructing argument craftData null item" );
				}
				m_dOverCutBackupList.Add( craftData.OverCutLength );
			}
		}

		public override EditActionType ActionType
		{
			get
			{
				return EditActionType.OverCut;
			}
		}

		public override void Start()
		{
			base.Start();

			// TODO: check all CAMData have same over cut length?
			m_Dlg = new OverCutDlg( m_dOverCutBackupList[ 0 ] );
			m_Dlg.CheckValueGeomRestriction += IsValidOverCut;
			m_Dlg.Preview = PreviewOverCutLength;
			m_Dlg.Confirm = ConfirmOverCutLength;
			m_Dlg.Cancel  = CancelOverCutLength;
			PropertyChanged?.Invoke( m_PathIDList );
			m_Dlg.Show( MyApp.MainForm );
		}

		public override void End()
		{
			if( !m_IsConfirmed ) {
				RestoreBackupOverCutLength();
				PropertyChanged?.Invoke( m_PathIDList );
			}
			if( m_Dlg != null ) {
				m_Dlg.Preview  = null;
				m_Dlg.Confirm  = null;
				m_Dlg.Cancel   = null;
				m_Dlg.Close();
				m_Dlg = null;
			}
			base.End();
		}

		void PreviewOverCutLength( double overCutLength )
		{
			SetOverCutLength( overCutLength );
			PropertyChanged?.Invoke( m_PathIDList );
		}

		void ConfirmOverCutLength( double overCutLength )
		{
			SetOverCutLength( overCutLength );
			PropertyChanged?.Invoke( m_PathIDList );
			m_IsConfirmed = true;
			End();
		}

		void CancelOverCutLength()
		{
			End();
		}

		void SetOverCutLength( double dOverCutLength )
		{
			foreach( var camData in m_CraftDataList ) {
				camData.OverCutLength = dOverCutLength;
			}
		}

		void RestoreBackupOverCutLength()
		{
			for( int i = 0; i < m_CraftDataList.Count; i++ ) {
				m_CraftDataList[ i ].OverCutLength = m_dOverCutBackupList[ i ];
			}
		}

		// TODO: fix it
		bool IsValidOverCut( double overcut )
		{
			bool isValid = true;
			//foreach( string szID in m_PathIDList ) {
			//	PathCacheProvider.TryGetStdPatternMaxOverCutCache( szID, out IStdPatternMaxOverCutCache overCutCache );
			//	if( overCutCache != null ) {
			//		if( overcut > overCutCache.GetMaxOverCutLength() ) {
			//			isValid = false;
			//			break;
			//		}
			//	}
			//}
			return isValid;
		}

		readonly List<double> m_dOverCutBackupList;
		bool m_IsConfirmed = false;
		OverCutDlg m_Dlg;
	}
}
