using System.Collections.Generic;
using System.Linq;
using MyCAM.Data;
using OCC.AIS;
using OCC.gp;
using OCCViewer;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Renderer for tool vectors
	/// </summary>
	internal class ToolVecRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Line>> m_ToolVecAISDict = new Dictionary<string, List<AIS_Line>>();

		public ToolVecRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void SetPauseRefreshAndHide( bool isPause )
		{
			if( m_IsPauseRefreshAndHide == isPause ) {
				return;
			}
			base.SetPauseRefreshAndHide( isPause );
			if( isPause ) {
				// hide all managed AIS objects without destroying them
				foreach( var kvp in m_ToolVecAISDict ) {
					foreach( AIS_Line toolVecAIS in kvp.Value ) {
						m_Viewer.GetAISContext().Erase( toolVecAIS, false );
					}
				}
			}
			else {
				// re-display all managed AIS objects
				foreach( var kvp in m_ToolVecAISDict ) {
					foreach( AIS_Line toolVecAIS in kvp.Value ) {
						m_Viewer.GetAISContext().Display( toolVecAIS, false );
						m_Viewer.GetAISContext().Deactivate( toolVecAIS );
					}
				}
			}
		}

		public override void Show( bool bUpdate = false )
		{
			if( m_IsPauseRefreshAndHide ) {
				return;
			}
			Remove();
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( IReadOnlyList<string> pathIDList, bool bUpdate = false )
		{
			BuildAndDisplay( pathIDList, null, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			RemoveToolVecs( m_ToolVecAISDict.Keys.ToList() );
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Remove( IReadOnlyList<string> pathIDList, bool bUpdate = false )
		{
			RemoveToolVecs( m_DataManager.GetEditablePathIDList( pathIDList.ToList() ).Union( pathIDList ).ToList() );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void RemoveToolVecs( IReadOnlyList<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( m_ToolVecAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> toolVecAISList = m_ToolVecAISDict[ szPathID ];
					foreach( AIS_Line toolVecAIS in toolVecAISList ) {
						m_Viewer.GetAISContext().Remove( toolVecAIS, false );
					}
					m_ToolVecAISDict[ szPathID ].Clear();
					m_ToolVecAISDict.Remove( szPathID );
				}
			}
		}

		public void ShowTrans( gp_Trsf trsf, bool bUpdate = false )
		{
			BuildAndDisplay( m_DataManager.PathIDList, trsf, bUpdate );
		}

		public void Reset( bool bUpdate = false )
		{
			gp_Trsf trsf = new gp_Trsf();
			ShowTrans( trsf, bUpdate );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void BuildAndDisplay( IReadOnlyList<string> pathIDList, gp_Trsf trsf, bool bUpdate )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefreshAndHide ) {
				return;
			}
			Remove( pathIDList );

			pathIDList = m_DataManager.GetEditablePathIDList( m_DataManager.GetProcessPathIDList( pathIDList.ToList() ) );

			// no need to show
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			// separate paths into normal and special tool vec
			List<string> normalPathIDList = new List<string>();
			List<string> specialPathIDList = new List<string>();

			foreach( string szPathID in pathIDList ) {
				DataGettingHelper.GetCraftDataByID( szPathID, out CraftData craftData );
				if( craftData != null && craftData.SpecialToolVecData != null && craftData.SpecialToolVecData.Mode != ToolVecMode.Interpolate ) {

					// none process path do not show tool vec
					if( craftData == null ) {
						m_ToolVecAISDict.Remove( szPathID );
						continue;
					}
					specialPathIDList.Add( szPathID );
				}
				else {

					// none process path do not show tool vec
					if( craftData == null ) {
						m_ToolVecAISDict.Remove( szPathID );
						continue;
					}
					normalPathIDList.Add( szPathID );
				}
			}

			// build normal tool vec using shared helper
			if( normalPathIDList.Count > 0 ) {
				Dictionary<string, List<AIS_Line>> normalBuilt = ToolVecAndPathVisibleHelper.BuildToolVecAISDict( normalPathIDList, trsf );
				foreach( var kvp in normalBuilt ) {
					m_ToolVecAISDict.Add( kvp.Key, kvp.Value );
				}
			}

			// build special tool vec using shared helper
			if( specialPathIDList.Count > 0 ) {
				Dictionary<string, List<AIS_Line>> specialBuilt = ToolVecAndPathVisibleHelper.BuildSpecialToolVecAISDict( specialPathIDList, trsf );
				foreach( var kvp in specialBuilt ) {
					m_ToolVecAISDict.Add( kvp.Key, kvp.Value );
				}
			}

			// display the tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_ToolVecAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> toolVecAISList = m_ToolVecAISDict[ szPathID ];
					foreach( AIS_Line toolVecAIS in toolVecAISList ) {
						m_Viewer.GetAISContext().Display( toolVecAIS, false );
						m_Viewer.GetAISContext().Deactivate( toolVecAIS );
					}
				}
			}
			if( bUpdate ) {
				UpdateView();
			}
		}
	}
}
