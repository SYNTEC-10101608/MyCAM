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
	internal class ToolVecRenderer : ICAMRenderer
	{
		readonly Viewer m_Viewer;
		readonly DataManager m_DataManager;
		readonly Dictionary<string, List<AIS_Line>> m_ToolVecAISDict = new Dictionary<string, List<AIS_Line>>();
		bool m_IsShow = true;

		public ToolVecRenderer( Viewer viewer, DataManager dataManager )
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

			// no need to show
			if( !m_IsShow ) {
				return;
			}

			// build tool vec
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( szPathID, out ICacheInfo cacheInfo ) == false || cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				List<AIS_Line> toolVecAISList = new List<AIS_Line>();
				m_ToolVecAISDict.Add( szPathID, toolVecAISList );

				for( int i = 0; i < contourCacheInfo.CAMPointList.Count; i++ ) {
					CAMPoint camPoint = contourCacheInfo.CAMPointList[ i ];
					AIS_Line toolVecAIS = GetVecAIS( camPoint.Point, camPoint.ToolVec );
					if( IsModifiedToolVecIndex( i, contourCacheInfo ) ) {
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
		}

		public void Remove()
		{
			Remove( m_DataManager.PathIDList );
		}

		public void Remove( List<string> pathIDList )
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

		public void SetShow( bool isShow )
		{
			m_IsShow = isShow;
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}

		bool IsModifiedToolVecIndex( int index, ContourCacheInfo cacheInfo )
		{
			return cacheInfo.GetToolVecModifyIndex().Contains( index );
		}

		AIS_Line GetVecAIS( gp_Pnt point, gp_Dir dir )
		{
			gp_Pnt endPoint = new gp_Pnt( point.XYZ() + dir.XYZ() * 10 );
			AIS_Line lineAIS = new AIS_Line( new Geom_CartesianPoint( point ), new Geom_CartesianPoint( endPoint ) );
			lineAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
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
