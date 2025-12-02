using MyCAM.CacheInfo;
using MyCAM.Data;
using OCC.AIS;
using OCC.BRepPrimAPI;
using OCC.gp;
using OCC.Graphic3d;
using OCC.Quantity;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor.Renderer
{
	/// <summary>
	/// Renderer for path orientation cones
	/// </summary>
	internal class OrientationRenderer : ICAMRenderer
	{
		readonly Viewer m_Viewer;
		readonly DataManager m_DataManager;
		readonly Dictionary<string, AIS_Shape> m_OrientationAISDict = new Dictionary<string, AIS_Shape>();
		readonly Dictionary<string, List<AIS_Shape>> m_LeadOrientationAISDict = new Dictionary<string, List<AIS_Shape>>();
		bool m_IsShow = true;

		public OrientationRenderer( Viewer viewer, DataManager dataManager )
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

			ShowPathOrientation( pathIDList );
			ShowLeadOrientation( pathIDList );
		}

		public void Remove()
		{
			Remove( m_DataManager.PathIDList );
		}

		public void Remove( List<string> pathIDList )
		{
			RemovePathOrientation( pathIDList );
			RemoveLeadOrientation( pathIDList );
		}

		public void SetShow( bool isShow )
		{
			m_IsShow = isShow;
		}

		public void UpdateView()
		{
			m_Viewer.UpdateView();
		}

		void ShowPathOrientation( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				gp_Pnt showPoint = contourCacheInfo.CAMPointList[ 0 ].Point;
				gp_Dir orientationDir = new gp_Dir( contourCacheInfo.CAMPointList[ 0 ].TangentVec.XYZ() );
				if( contourCacheInfo.IsPathReverse ) {
					orientationDir.Reverse();
				}
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISDict.Add( szPathID, orientationAIS );
			}

			// display the orientation
			foreach( string szPathID in pathIDList ) {
				if( m_OrientationAISDict.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Display( m_OrientationAISDict[ szPathID ], false );
					m_Viewer.GetAISContext().Deactivate( m_OrientationAISDict[ szPathID ] );
				}
			}
		}

		void RemovePathOrientation( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( m_OrientationAISDict.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Remove( m_OrientationAISDict[ szPathID ], false );
					m_OrientationAISDict.Remove( szPathID );
				}
			}
		}

		void ShowLeadOrientation( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( GetCacheInfoByID( szPathID, out ICacheInfo cacheInfo ) == false
					|| cacheInfo.PathType != PathType.Contour ) {
					continue;
				}
				ContourCacheInfo contourCacheInfo = (ContourCacheInfo)cacheInfo;
				LeadData leadData = contourCacheInfo.LeadData;
				if( leadData.LeadIn.Type != LeadLineType.None ) {
					List<AIS_Shape> orientationAISList = new List<AIS_Shape>();
					m_LeadOrientationAISDict.Add( szPathID, orientationAISList );

					if( contourCacheInfo.LeadInCAMPointList.Count == 0 ) {
						break;
					}
					gp_Pnt leadInStartPoint = contourCacheInfo.LeadInCAMPointList.First().Point;
					gp_Dir leadInOrientationDir = new gp_Dir( contourCacheInfo.LeadInCAMPointList.First().TangentVec.XYZ() );
					AIS_Shape orientationAIS = GetOrientationAIS( leadInStartPoint, leadInOrientationDir );
					orientationAISList.Add( orientationAIS );
				}

				// path with lead out
				if( leadData.LeadOut.Type != LeadLineType.None ) {
					List<AIS_Shape> orientationAISList;
					if( m_LeadOrientationAISDict.ContainsKey( szPathID ) ) {
						orientationAISList = m_LeadOrientationAISDict[ szPathID ];
					}
					else {
						orientationAISList = new List<AIS_Shape>();
						m_LeadOrientationAISDict.Add( szPathID, orientationAISList );
					}

					if( contourCacheInfo.LeadOutCAMPointList.Count == 0 ) {
						break;
					}
					gp_Pnt leadOutEndPoint = contourCacheInfo.LeadOutCAMPointList.Last().Point;
					gp_Dir leadOutOrientationDir = new gp_Dir( contourCacheInfo.LeadOutCAMPointList.Last().TangentVec.XYZ() );
					AIS_Shape orientationAIS = GetOrientationAIS( leadOutEndPoint, leadOutOrientationDir );
					orientationAISList.Add( orientationAIS );
				}
			}

			// display the orientation
			foreach( string szPathID in pathIDList ) {
				if( m_LeadOrientationAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Shape> orientationAISList = m_LeadOrientationAISDict[ szPathID ];
					foreach( AIS_Shape orientationAIS in orientationAISList ) {
						m_Viewer.GetAISContext().Display( orientationAIS, false );
						m_Viewer.GetAISContext().Deactivate( orientationAIS );
					}
				}
			}
		}

		void RemoveLeadOrientation( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( m_LeadOrientationAISDict.ContainsKey( szPathID ) ) {
					List<AIS_Shape> orientationAISList = m_LeadOrientationAISDict[ szPathID ];
					foreach( AIS_Shape orientationAIS in orientationAISList ) {
						m_Viewer.GetAISContext().Remove( orientationAIS, false );
					}
					m_LeadOrientationAISDict[ szPathID ].Clear();
					m_LeadOrientationAISDict.Remove( szPathID );
				}
			}
		}

		AIS_Shape GetOrientationAIS( gp_Pnt point, gp_Dir dir )
		{
			// draw a cone to indicate the orientation
			gp_Ax2 coneAx2 = new gp_Ax2( point, dir );
			BRepPrimAPI_MakeCone coneMaker = new BRepPrimAPI_MakeCone( coneAx2, 0.5, 0, 2 );
			AIS_Shape coneAIS = new AIS_Shape( coneMaker.Shape() );
			Graphic3d_MaterialAspect aspect = new Graphic3d_MaterialAspect( Graphic3d_NameOfMaterial.Graphic3d_NOM_STEEL );
			coneAIS.SetMaterial( aspect );
			coneAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_WHITE ) );
			coneAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_Shaded );
			coneAIS.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			return coneAIS;
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
