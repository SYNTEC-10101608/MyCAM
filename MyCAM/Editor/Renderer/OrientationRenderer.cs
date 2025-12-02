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
	internal class OrientationRenderer : CAMRendererBase
	{
		readonly Dictionary<string, AIS_Shape> m_OrientationAISDict = new Dictionary<string, AIS_Shape>();
		readonly Dictionary<string, List<AIS_Shape>> m_LeadOrientationAISDict = new Dictionary<string, List<AIS_Shape>>();

		public OrientationRenderer( Viewer viewer, DataManager dataManager )
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

			ShowPathOrientation( pathIDList );
			ShowLeadOrientation( pathIDList );

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
			RemovePathOrientation( pathIDList );
			RemoveLeadOrientation( pathIDList );

			if( bUpdate ) {
				UpdateView();
			}
		}

		void ShowPathOrientation( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( !GetContourCacheInfoByID( szPathID, out ContourCacheInfo contourCacheInfo ) ) {
					continue;
				}
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
				if( !GetContourCacheInfoByID( szPathID, out ContourCacheInfo contourCacheInfo ) ) {
					continue;
				}
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
	}
}
