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
	/// Renderer for craft data (lead lines and overcut)
	/// </summary>
	internal class CraftRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Line>> m_LeadInAISDict = new Dictionary<string, List<AIS_Line>>();
		readonly Dictionary<string, List<AIS_Line>> m_LeadOutAISDict = new Dictionary<string, List<AIS_Line>>();
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
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}
			ShowLeadInLine( pathIDList );
			ShowLeadOutLine( pathIDList );
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
			RemoveLeadInLine( pathIDList );
			RemoveLeadOutLine( pathIDList );
			RemoveOverCut( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void ShowLeadInLine( List<string> pathIDList )
		{
			ShowLeadLine( pathIDList, m_LeadInAISDict, GetLeadInPointList );
		}

		void ShowLeadOutLine( List<string> pathIDList )
		{
			ShowLeadLine( pathIDList, m_LeadOutAISDict, GetLeadOutPointList );
		}

		void RemoveLeadInLine( List<string> pathIDList )
		{
			RemoveLines( m_LeadInAISDict, pathIDList );
		}

		void RemoveLeadOutLine( List<string> pathIDList )
		{
			RemoveLines( m_LeadOutAISDict, pathIDList );
		}

		void ShowLeadLine( List<string> pathIDList, Dictionary<string, List<AIS_Line>> lineDict, System.Func<string, IReadOnlyList<IProcessPoint>> getPointListFunc )
		{
			foreach( string szPathID in pathIDList ) {
				LeadData leadData = GetLeadData( szPathID );
				if( leadData == null || ( leadData.LeadIn.StraightLength == 0 && leadData.LeadIn.ArcLength == 0 ) ) {
					continue;
				}
				IReadOnlyList<IProcessPoint> pointList = getPointListFunc( szPathID );
				if( pointList == null || pointList.Count == 0 ) {
					continue;
				}
				List<AIS_Line> leadAISList = new List<AIS_Line>();
				lineDict.Add( szPathID, leadAISList );
				for( int i = 0; i < pointList.Count - 1; i++ ) {
					gp_Pnt currentCAMPoint = pointList[ i ].Point;
					gp_Pnt nextCAMPoint = pointList[ i + 1 ].Point;
					AIS_Line LeadAISLine = GetLineAIS( currentCAMPoint, nextCAMPoint, Quantity_NameOfColor.Quantity_NOC_DARKORANGE );
					leadAISList.Add( LeadAISLine );
				}
			}

			// display the lead line
			DisplayLines( lineDict, pathIDList );
		}

		void ShowOverCut( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				double overCutLength = GetOverCutLength( szPathID );
				IReadOnlyList<IProcessPoint> overCutPointList = GetOverCutPointList( szPathID );

				if( overCutPointList == null || overCutPointList.Count == 0 ) {
					continue;
				}

				List<AIS_Line> overcutAISList = new List<AIS_Line>();
				m_OverCutAISDict.Add( szPathID, overcutAISList );

				if( overCutLength > 0 ) {
					for( int i = 0; i < overCutPointList.Count - 1; i++ ) {
						AIS_Line overCutAISLine = GetLineAIS( overCutPointList[ i ].Point, overCutPointList[ i + 1 ].Point, Quantity_NameOfColor.Quantity_NOC_DARKORANGE );
						overcutAISList.Add( overCutAISLine );
					}
				}
			}

			// display the overcut line
			DisplayLines( m_OverCutAISDict, pathIDList );
		}

		void RemoveOverCut( List<string> pathIDList )
		{
			RemoveLines( m_OverCutAISDict, pathIDList );
		}

		void DisplayLines( Dictionary<string, List<AIS_Line>> lineDict, List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( lineDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> lineAISList = lineDict[ szPathID ];
					foreach( AIS_Line lineAIS in lineAISList ) {
						m_Viewer.GetAISContext().Display( lineAIS, false );
						m_Viewer.GetAISContext().Deactivate( lineAIS );
					}
				}
			}
		}

		void RemoveLines( Dictionary<string, List<AIS_Line>> lineDict, List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( lineDict.ContainsKey( szPathID ) ) {
					List<AIS_Line> lineAISList = lineDict[ szPathID ];
					foreach( AIS_Line lineAIS in lineAISList ) {
						m_Viewer.GetAISContext().Remove( lineAIS, false );
					}
					lineDict[ szPathID ].Clear();
					lineDict.Remove( szPathID );
				}
			}
		}

		AIS_Line GetLineAIS( gp_Pnt startPnt, gp_Pnt endPnt, Quantity_NameOfColor color )
		{
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( startPnt ), new Geom_CartesianPoint( endPnt ) );
			lineAIS.SetColor( new Quantity_Color( color ) );
			lineAIS.SetWidth( 2.5 );
			return lineAIS;
		}

		IReadOnlyList<IProcessPoint> GetLeadInPointList( string pathID )
		{
			if( !PathCacheProvider.TryGetLeadCache( pathID, out ILeadCache leadCache ) ) {
				return null;
			}
			return leadCache.LeadInCAMPointList;
		}

		IReadOnlyList<IProcessPoint> GetLeadOutPointList( string pathID )
		{
			if( !PathCacheProvider.TryGetLeadCache( pathID, out ILeadCache leadCache ) ) {
				return null;
			}
			return leadCache.LeadOutCAMPointList;
		}

		IReadOnlyList<IProcessPoint> GetOverCutPointList( string pathID )
		{
			if( !PathCacheProvider.TryGetOverCutCache( pathID, out IOverCutCache overCutCache ) ) {
				return null;
			}
			return overCutCache.OverCutCAMPointList;
		}

		LeadData GetLeadData( string pathID )
		{
			if( !PathCacheProvider.TryGetLeadCache( pathID, out ILeadCache leadCache ) ) {
				return null;
			}
			return leadCache.LeadData;
		}

		double GetOverCutLength( string pathID )
		{
			if( !PathCacheProvider.TryGetOverCutCache( pathID, out IOverCutCache overCutCache ) ) {
				return 0.0;
			}
			return overCutCache.OverCutLength;
		}
	}
}
