using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;
using System.Collections.Generic;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Renderer for tool vectors
	/// </summary>
	internal class ToolVecRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Line>> m_ToolVecAISDict = new Dictionary<string, List<AIS_Line>>();
		bool m_IsPauseRefresh = false;

		public ToolVecRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public void SetPauseRefresh( bool isPause )
		{
			if( m_IsPauseRefresh == isPause ) {
				return;
			}
			m_IsPauseRefresh = isPause;
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
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			BuildAndDisplay( pathIDList, null, bUpdate );
		}

		public override void Remove( bool bUpdate = false )
		{
			Remove( m_DataManager.PathIDList, bUpdate );
		}

		public void Remove( List<string> pathIDList, bool bUpdate = false )
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
			if( bUpdate ) {
				UpdateView();
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
		}

		void BuildAndDisplay( List<string> pathIDList, gp_Trsf trsf, bool bUpdate )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefresh ) {
				return;
			}
			Remove( pathIDList );

			// no need to show
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			// build tool vec using shared helper
			Dictionary<string, List<AIS_Line>> built = RendererHelper.BuildToolVecAISDict( pathIDList, trsf );
			foreach( var kvp in built ) {
				m_ToolVecAISDict.Add( kvp.Key, kvp.Value );
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

		AIS_Line GetVecAIS( gp_Pnt point, gp_Dir dir )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
			lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			lineAIS.SetWidth( 1 );
			return lineAIS;
		}

		IReadOnlyList<IProcessPoint> GetToolVecPointList( string pathID )
		{
			// get path type
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) ) {
				return null;
			}

			// for contour
			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache contourCache ) ) {
					return null;
				}
				return contourCache.MainPathPointList;
			}

			// for standard pattern
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( pathID, out IStdPatternCache stdPatternCache ) ) {
					return null;
				}
				return stdPatternCache.KeyCAMPointList;
			}

			// other type path do not support tool vec
			else {
				return null;
			}
		}
	}
}