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
				LeadData leadData = GetLeadData( szPathID );
				if( leadData == null ) {
					continue;
				}

				// path with lead in
				if( leadData.LeadIn.Type != LeadLineType.None ) {
					List<IProcessPoint> leadInPointList = GetLeadInPointList( szPathID );
					if( leadInPointList != null && leadInPointList.Count > 0 ) {
						List<AIS_Line> leadAISList = new List<AIS_Line>();
						m_LeadAISDict.Add( szPathID, leadAISList );

						for( int i = 0; i < leadInPointList.Count - 1; i++ ) {
							gp_Pnt currentCAMPoint = leadInPointList[ i ].Point;
							gp_Pnt nextCAMPoint = leadInPointList[ i + 1 ].Point;
							AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint, nextCAMPoint, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
							leadAISList.Add( LeadAISLine );
						}
					}
				}

				// path with lead out
				if( leadData.LeadOut.Type != LeadLineType.None ) {
					List<IProcessPoint> leadOutPointList = GetLeadOutPointList( szPathID );
					if( leadOutPointList != null && leadOutPointList.Count > 0 ) {
						List<AIS_Line> leadAISList;
						if( m_LeadAISDict.ContainsKey( szPathID ) ) {
							leadAISList = m_LeadAISDict[ szPathID ];
						}
						else {
							leadAISList = new List<AIS_Line>();
							m_LeadAISDict.Add( szPathID, leadAISList );
						}

						for( int i = 0; i < leadOutPointList.Count - 1; i++ ) {
							gp_Pnt currentCAMPoint = leadOutPointList[ i ].Point;
							gp_Pnt nextCAMPoint = leadOutPointList[ i + 1 ].Point;
							AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint, nextCAMPoint, Quantity_NameOfColor.Quantity_NOC_GREENYELLOW );
							leadAISList.Add( LeadAISLine );
						}
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
				double overCutLength = GetOverCutLength( szPathID );
				List<IProcessPoint> overCutPointList = GetOverCutPointList( szPathID );

				if( overCutPointList == null || overCutPointList.Count == 0 ) {
					continue;
				}

				List<AIS_Line> overcutAISList = new List<AIS_Line>();
				m_OverCutAISDict.Add( szPathID, overcutAISList );

				if( overCutLength > 0 ) {
					for( int i = 0; i < overCutPointList.Count - 1; i++ ) {
						AIS_Line overCutAISLine = GetLineAIS( overCutPointList[ i ].Point, overCutPointList[ i + 1 ].Point, Quantity_NameOfColor.Quantity_NOC_DEEPPINK );
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

		List<IProcessPoint> GetLeadInPointList( string pathID )
		{
			if( !GetContourCacheInfoByID( pathID, out ContourCacheInfo contourCacheInfo ) ) {
				return null;
			}
			if( contourCacheInfo.LeadInCAMPointList == null ) {
				return null;
			}
			List<IProcessPoint> pointList = new List<IProcessPoint>();
			foreach( CAMPoint camPoint in contourCacheInfo.LeadInCAMPointList ) {
				pointList.Add( camPoint );
			}
			return pointList;
		}

		List<IProcessPoint> GetLeadOutPointList( string pathID )
		{
			if( !GetContourCacheInfoByID( pathID, out ContourCacheInfo contourCacheInfo ) ) {
				return null;
			}
			if( contourCacheInfo.LeadOutCAMPointList == null ) {
				return null;
			}
			List<IProcessPoint> pointList = new List<IProcessPoint>();
			foreach( CAMPoint camPoint in contourCacheInfo.LeadOutCAMPointList ) {
				pointList.Add( camPoint );
			}
			return pointList;
		}

		List<IProcessPoint> GetOverCutPointList( string pathID )
		{
			if( !GetContourCacheInfoByID( pathID, out ContourCacheInfo contourCacheInfo ) ) {
				return null;
			}
			if( contourCacheInfo.OverCutCAMPointList == null ) {
				return null;
			}
			List<IProcessPoint> pointList = new List<IProcessPoint>();
			foreach( CAMPoint camPoint in contourCacheInfo.OverCutCAMPointList ) {
				pointList.Add( camPoint );
			}
			return pointList;
		}

		LeadData GetLeadData( string pathID )
		{
			if( !GetContourCacheInfoByID( pathID, out ContourCacheInfo contourCacheInfo ) ) {
				return null;
			}
			return contourCacheInfo.LeadData;
		}

		double GetOverCutLength( string pathID )
		{
			if( !GetContourCacheInfoByID( pathID, out ContourCacheInfo contourCacheInfo ) ) {
				return 0.0;
			}
			return contourCacheInfo.OverCutLength;
		}
	}
}
