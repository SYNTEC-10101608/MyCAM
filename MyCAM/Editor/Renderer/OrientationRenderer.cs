using MyCAM.PathCache;
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
		readonly Dictionary<string, AIS_Shape> m_LeadInOrientationAISDict = new Dictionary<string, AIS_Shape>();
		readonly Dictionary<string, AIS_Shape> m_LeadOutOrientationAISDict = new Dictionary<string, AIS_Shape>();

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
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}
			ShowPathOrientation( pathIDList );
			ShowLeadInOrientation( pathIDList );
			ShowLeadOutOrientation( pathIDList );
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
			RemoveLeadInOrientation( pathIDList );
			RemoveLeadOutOrientation( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void ShowPathOrientation( List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				IOrientationPoint firstPoint = GetFirstCAMPoint( szPathID );
				if( firstPoint == null ) {
					continue;
				}
				gp_Pnt showPoint = firstPoint.Point;
				gp_Dir orientationDir = new gp_Dir( firstPoint.TangentVec.XYZ() );
				if( GetIsPathReverse( szPathID ) ) {
					orientationDir.Reverse();
				}
				AIS_Shape orientationAIS = GetOrientationAIS( showPoint, orientationDir );
				m_OrientationAISDict.Add( szPathID, orientationAIS );
			}

			// display the orientation
			DisplayOrientations( m_OrientationAISDict, pathIDList );
		}

		void RemovePathOrientation( List<string> pathIDList )
		{
			RemoveOrientations( m_OrientationAISDict, pathIDList );
		}

		void ShowLeadInOrientation( List<string> pathIDList )
		{
			ShowLeadOrientation( pathIDList, m_LeadInOrientationAISDict,
				( leadData ) => leadData.LeadIn.Type != LeadGeomType.None,
				GetLeadInFirstPoint );
		}

		void ShowLeadOutOrientation( List<string> pathIDList )
		{
			ShowLeadOrientation( pathIDList, m_LeadOutOrientationAISDict,
				( leadData ) => leadData.LeadOut.Type != LeadGeomType.None,
				GetLeadOutLastPoint );
		}

		void RemoveLeadInOrientation( List<string> pathIDList )
		{
			RemoveOrientations( m_LeadInOrientationAISDict, pathIDList );
		}

		void RemoveLeadOutOrientation( List<string> pathIDList )
		{
			RemoveOrientations( m_LeadOutOrientationAISDict, pathIDList );
		}

		void ShowLeadOrientation(
			List<string> pathIDList,
			Dictionary<string, AIS_Shape> orientationDict,
			System.Func<LeadData, bool> needShowFunc,
			System.Func<string, IOrientationPoint> getPointFunc )
		{
			foreach( string szPathID in pathIDList ) {
				LeadData leadData = GetLeadData( szPathID );
				if( leadData == null || !needShowFunc( leadData ) ) {
					continue;
				}
				IOrientationPoint orientationPoint = getPointFunc( szPathID );
				if( orientationPoint == null ) {
					continue;
				}
				gp_Dir orientationDir = new gp_Dir( orientationPoint.TangentVec.XYZ() );
				AIS_Shape orientationAIS = GetOrientationAIS( orientationPoint.Point, orientationDir );
				orientationDict.Add( szPathID, orientationAIS );
			}

			// display the orientation
			DisplayOrientations( orientationDict, pathIDList );
		}

		void DisplayOrientations( Dictionary<string, AIS_Shape> orientationDict, List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( orientationDict.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Display( orientationDict[ szPathID ], false );
					m_Viewer.GetAISContext().Deactivate( orientationDict[ szPathID ] );
				}
			}
		}

		void RemoveOrientations( Dictionary<string, AIS_Shape> orientationDict, List<string> pathIDList )
		{
			foreach( string szPathID in pathIDList ) {
				if( orientationDict.ContainsKey( szPathID ) ) {
					m_Viewer.GetAISContext().Remove( orientationDict[ szPathID ], false );
					orientationDict.Remove( szPathID );
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
			coneAIS.SetDisplayMode( (int)AIS_DisplayMode.AIS_WireFrame );
			coneAIS.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Topmost );
			return coneAIS;
		}

		IOrientationPoint GetFirstCAMPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetMainPathStartPointCache( pathID, out IMainPathStartPointCache mainPathStartPointCache ) ) {
				return null;
			}
			if( mainPathStartPointCache.GetMainPathStartCAMPoint() == null || !( mainPathStartPointCache.GetMainPathStartCAMPoint() is IOrientationPoint orientationPoint ) ) {
				return null;
			}
			return orientationPoint;
		}

		IOrientationPoint GetLeadInFirstPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetLeadCache( pathID, out ILeadCache leadCache ) ) {
				return null;
			}
			if( leadCache.LeadInCAMPointList == null || leadCache.LeadInCAMPointList.Count == 0 ) {
				return null;
			}
			return leadCache.LeadInCAMPointList.First();
		}

		IOrientationPoint GetLeadOutLastPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetLeadCache( pathID, out ILeadCache leadCache ) ) {
				return null;
			}
			if( leadCache.LeadOutCAMPointList == null || leadCache.LeadOutCAMPointList.Count == 0 ) {
				return null;
			}
			return leadCache.LeadOutCAMPointList.Last();
		}

		LeadData GetLeadData( string pathID )
		{
			if( !PathCacheProvider.TryGetLeadCache( pathID, out ILeadCache leadCache ) ) {
				return null;
			}
			return leadCache.LeadData;
		}

		bool GetIsPathReverse( string pathID )
		{
			if( !PathCacheProvider.TryGetPathReverseCache( pathID, out IPathReverseCache pathReverseCache ) ) {
				return false;
			}
			return pathReverseCache.IsPathReverse;
		}
	}
}
