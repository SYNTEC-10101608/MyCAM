using MyCAM.App;
using MyCAM.Data;
using MyCAM.Editor.Dialog;
using MyCAM.PathCache;
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
			OverCutDlg overCutForm = new OverCutDlg( m_dOverCutBackupList[ 0 ] );
			overCutForm.CheckValueAccrodGeomRestriction += IsValidOverCut;
			PropertyChanged?.Invoke( m_PathIDList );

			// preview
			overCutForm.Preview += ( overCutLength ) =>
			{
				SetOverCutLength( overCutLength );
				PropertyChanged?.Invoke( m_PathIDList );
			};

			// confirm
			overCutForm.Confirm += ( overCutLength ) =>
			{
				SetOverCutLength( overCutLength );
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};

			// cancel
			overCutForm.Cancel += () =>
			{
				RestoreBackupOverCutLength();
				PropertyChanged?.Invoke( m_PathIDList );
				End();
			};
			overCutForm.Show( MyApp.MainForm );
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

		bool IsValidOverCut( double overcut )
		{
			bool isValid = true;
			foreach( string szID in m_PathIDList ) {
				PathCacheProvider.TryGetStdPatternOverCutMaxinumCache( szID, out IStdPatternOverCutMaxinumCache overCutCache );
				if( overCutCache != null ) {
					if( overcut > overCutCache.GetMaxinumOverCutLength() ) {
						isValid = false;
						break;
					}
				}
			}
			return isValid;
		}

		readonly List<double> m_dOverCutBackupList;
	}
}
