using MyCAM.Data;
using MyCAM.Helper;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.gp;
using OCCViewer;
using System.Collections.Generic;

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
				AIS_Shape orientationAIS = DrawHelper.GetOrientationAIS( showPoint, orientationDir );
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
			ShowLeadOrientation( pathIDList, m_LeadInOrientationAISDict, GetLeadInFirstPoint );
		}

		void ShowLeadOutOrientation( List<string> pathIDList )
		{
			ShowLeadOrientation( pathIDList, m_LeadOutOrientationAISDict, GetLeadOutLastPoint );
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
			System.Func<string, IOrientationPoint> getPointFunc )
		{
			foreach( string szPathID in pathIDList ) {
				LeadData leadData = GetLeadData( szPathID );
				if( leadData == null || ( leadData.LeadIn.StraightLength == 0 && leadData.LeadIn.ArcLength == 0 ) ) {
					continue;
				}
				IOrientationPoint orientationPoint = getPointFunc( szPathID );
				if( orientationPoint == null ) {
					continue;
				}
				gp_Dir orientationDir = new gp_Dir( orientationPoint.TangentVec.XYZ() );
				AIS_Shape orientationAIS = DrawHelper.GetOrientationAIS( orientationPoint.Point, orientationDir );
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

		IOrientationPoint GetFirstCAMPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetOrientationCache( pathID, out IOrientationCache orientationCache ) ) {
				return null;
			}
			return orientationCache.MainPathStartPoint;
		}

		IOrientationPoint GetLeadInFirstPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetOrientationCache( pathID, out IOrientationCache orientationCache ) ) {
				return null;
			}
			return orientationCache.LeadInStartPoint;
		}

		IOrientationPoint GetLeadOutLastPoint( string pathID )
		{
			if( !PathCacheProvider.TryGetOrientationCache( pathID, out IOrientationCache orientationCache ) ) {
				return null;
			}
			return orientationCache.LeadOutEndPoint;
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
