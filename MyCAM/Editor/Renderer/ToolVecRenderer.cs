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
	/// Renderer for tool vectors
	/// </summary>
	internal class ToolVecRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Line>> m_ToolVecAISDict = new Dictionary<string, List<AIS_Line>>();

		public ToolVecRenderer( Viewer viewer, DataManager dataManager )
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

			// no need to show
			if( !m_IsShow ) {
				return;
			}

			// build tool vec
			foreach( string szPathID in pathIDList ) {
				if( !GetContourCacheInfoByID( szPathID, out ContourCacheInfo contourCacheInfo ) ) {
					continue;
				}
				List<AIS_Line> toolVecAISList = new List<AIS_Line>();
				m_ToolVecAISDict.Add( szPathID, toolVecAISList );

				for( int i = 0; i < contourCacheInfo.CAMPointList.Count; i++ ) {
					IToolVecPoint point = contourCacheInfo.CAMPointList[ i ];
					AIS_Line toolVecAIS = GetVecAIS( point.Point, point.ToolVec );
					if( contourCacheInfo.IsToolVecModifyPoint( point ) ) {
						toolVecAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_RED ) );
						toolVecAIS.SetWidth( 4 );
					}
					toolVecAISList.Add( toolVecAIS );
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

		AIS_Line GetVecAIS( gp_Pnt point, gp_Dir dir )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
			lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
			lineAIS.SetWidth( 1 );
			return lineAIS;
		}
	}
}
