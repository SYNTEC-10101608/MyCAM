using MyCAM.CacheInfo;
using MyCAM.Data;
using OCC.AIS;
using OCC.Geom;
using OCC.gp;
using OCC.Quantity;
using OCCViewer;
using System.Collections.Generic;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Renderer for craft data (lead lines and overcut)
	/// </summary>
	internal class CraftRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Line>> m_LeadAISDict = new Dictionary<string, List<AIS_Line>>();
		readonly Dictionary<string, List<AIS_Line>> m_OverCutAISDict = new Dictionary<string, List<AIS_Line>>();

		public CraftRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			Remove( pathIDList );

			if( !m_IsShow ) {
				return;
			}

			ShowLeadLine( pathIDList );
			ShowOverCut( pathIDList );

			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			Remove( m_DataManager.PathIDList, bUpdate );
		}

		public void Remove( List<string> pathIDList, bool bUpdate = false )
		{
			RemoveLeadLine( pathIDList );
			RemoveOverCut( pathIDList );

			if( bUpdate ) {
				UpdateView();
			}
		}

		void ShowLeadLine( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( !GetContourCacheInfoByID( szPathID, out ContourCacheInfo contourCacheInfo ) ) {
					continue;
				}
				LeadData leadData = contourCacheInfo.LeadData;
				if( leadData.LeadIn.Type != LeadLineType.None ) {
					List<AIS_Line> leadAISList = new List<AIS_Line>();
					m_LeadAISDict.Add( szPathID, leadAISList );
					for( int i = 0; i < contourCacheInfo.LeadInCAMPointList.Count - 1; i++ ) {
						gp_Pnt currentCAMPoint = contourCacheInfo.LeadInCAMPointList[ i ].Point;
						gp_Pnt nextCAMPoint = contourCacheInfo.LeadInCAMPointList[ i + 1 ].Point;
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint, nextCAMPoint, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						leadAISList.Add( LeadAISLine );
					}
				}

				if( leadData.LeadOut.Type != LeadLineType.None ) {
					List<AIS_Line> leadAISList;
					if( m_LeadAISDict.ContainsKey( szPathID ) ) {
						leadAISList = m_LeadAISDict[ szPathID ];
					}
					else {
						leadAISList = new List<AIS_Line>();
						m_LeadAISDict.Add( szPathID, leadAISList );
					}
					for( int i = 0; i < contourCacheInfo.LeadOutCAMPointList.Count - 1; i++ ) {
						gp_Pnt currentCAMPoint = contourCacheInfo.LeadOutCAMPointList[ i ].Point;
						gp_Pnt nextCAMPoint = contourCacheInfo.LeadOutCAMPointList[ i + 1 ].Point;
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint, nextCAMPoint, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
						leadAISList.Add( LeadAISLine );
					}
				}
			}

			// display the lead line
			foreach( string szPathID in pathIDList ) {
				if( m_LeadAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> leadAISList = m_LeadAISDict[ szPathID ];
					foreach( AIS_Line leadAIS in leadAISList ) {
						m_Viewer.GetAISContext().Display( leadAIS, false );
						m_Viewer.GetAISContext().Deactivate( leadAIS );
					}
				}
			}
		}

		void RemoveLeadLine( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( m_LeadAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> leadAISList = m_LeadAISDict[ szPathID ];
					foreach( AIS_Line leadAIS in leadAISList ) {
						m_Viewer.GetAISContext().Remove( leadAIS, false );
					}
					m_LeadAISDict[ szPathID ].Clear();
					m_LeadAISDict.Remove( szPathID );
				}
			}
		}

		void ShowOverCut( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( !GetContourCacheInfoByID( szPathID, out ContourCacheInfo contourCacheInfo ) ) {
					continue;
				}
				List<AIS_Line> overcutAISList = new List<AIS_Line>();
				m_OverCutAISDict.Add( szPathID, overcutAISList );
				if( contourCacheInfo.OverCutLength > 0 ) {
					for( int i = 0; i < contourCacheInfo.OverCutCAMPointList.Count - 1; i++ ) {
						AIS_Line overCutAISLine = GetLineAIS( contourCacheInfo.OverCutCAMPointList[ i ].Point, contourCacheInfo.OverCutCAMPointList[ i + 1 ].Point, Quantity_NameOfColor.Quantity_NOC_DEEPPINK );
						overcutAISList.Add( overCutAISLine );
					}
				}
			}

			// display the tool vec
			foreach( string szPathID in pathIDList ) {
				if( m_OverCutAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> overcutAISList = m_OverCutAISDict[ szPathID ];
					foreach( AIS_Line overcutAIS in overcutAISList ) {
						m_Viewer.GetAISContext().Display( overcutAIS, false );
						m_Viewer.GetAISContext().Deactivate( overcutAIS );
					}
				}
			}
		}

		void RemoveOverCut( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( m_OverCutAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> overcutAISList = m_OverCutAISDict[ szPathID ];
					foreach( AIS_Line overcutAIS in overcutAISList ) {
						m_Viewer.GetAISContext().Remove( overcutAIS, false );
					}
					m_OverCutAISDict[ szPathID ].Clear();
					m_OverCutAISDict.Remove( szPathID );
				}
			}
		}

		AIS_Line GetLineAIS( gp_Pnt startPnt, gp_Pnt endPnt, Quantity_NameOfColor color )
		{
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( startPnt ), new Geom_CartesianPoint( endPnt ) );
			lineAIS.SetColor( new Quantity_Color( color ) );
			lineAIS.SetWidth( 1 );
			return lineAIS;
		}
	}
}
