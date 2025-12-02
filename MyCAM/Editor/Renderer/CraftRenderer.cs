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
	internal class CraftRenderer : ICAMRenderer
	{
		readonly Viewer m_Viewer;
		readonly DataManager m_DataManager;
		readonly Dictionary<string, List<AIS_Line>> m_LeadAISDict = new Dictionary<string, List<AIS_Line>>();
		readonly Dictionary<string, List<AIS_Line>> m_OverCutAISDict = new Dictionary<string, List<AIS_Line>>();
		bool m_IsShow = true;

		public CraftRenderer( Viewer viewer, DataManager dataManager )
		{
			m_Viewer = viewer;
			m_DataManager = dataManager;
		}

		public void Show()
		{
			Show( m_DataManager.PathIDList );
		}

		public void Show( List<string> pathIDList )
		{
			Remove( pathIDList );

			if( !m_IsShow ) {
				return;
			}

			ShowLeadLine( pathIDList );
			ShowOverCut( pathIDList );
		}

		public void Remove()
		{
			Remove( m_DataManager.PathIDList );
		}

		public void Remove( List<string> pathIDList )
		{
			RemoveLeadLine( pathIDList );
			RemoveOverCut( pathIDList );
		}

		public void SetShow( bool isShow )
		{
			m_IsShow = isShow;
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}

		void ShowLeadLine( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				LeadData leadData = contourCacheInfo.LeadData;
				if( leadData.LeadIn.Type != LeadLineType.None ) {
					List<AIS_Line> leadAISList = new List<AIS_Line>();
					m_LeadAISDict.Add( szPathID, leadAISList );
					for( int i = 0; i < contourCacheInfo.LeadInCAMPointList.Count - 1; i++ ) {
						CAMPoint currentCAMPoint = contourCacheInfo.LeadInCAMPointList[ i ];
						CAMPoint nextCAMPoint = contourCacheInfo.LeadInCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint.Point, nextCAMPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
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
						CAMPoint currentCAMPoint = contourCacheInfo.LeadOutCAMPointList[ i ];
						CAMPoint nextCAMPoint = contourCacheInfo.LeadOutCAMPointList[ i + 1 ];
						AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint.Point, nextCAMPoint.Point, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
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
				if( GetCacheInfoByID( szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
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

		bool GetCacheInfoByID( string szPathID, out ICacheInfo cacheInfo )
		{
			cacheInfo = null;
			if( string.IsNullOrEmpty( szPathID )
				|| m_DataManager.ObjectMap.ContainsKey( szPathID ) == false
				|| m_DataManager.ObjectMap[ szPathID ] == null ) {
				return false;
			}
			if( ( (PathObject)m_DataManager.ObjectMap[ szPathID ] ).PathType == PathType.Contour ) {
				cacheInfo = ( (ContourPathObject)m_DataManager.ObjectMap[ szPathID ] ).ContourCacheInfo;
			}
			return true;
		}
	}
}
