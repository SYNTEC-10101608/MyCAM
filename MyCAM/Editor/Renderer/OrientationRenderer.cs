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

		public override void SetPauseRefreshAndHide( bool isPause )
		{
			if( m_IsPauseRefreshAndHide == isPause ) {
				return;
			}
			base.SetPauseRefreshAndHide( isPause );
			if( isPause ) {
				// hide all managed AIS objects without destroying them
				HideAllShapes( m_OrientationAISDict );
				HideAllShapes( m_LeadInOrientationAISDict );
				HideAllShapes( m_LeadOutOrientationAISDict );
			}
			else {
				// re-display all managed AIS objects
				ReDisplayAllShapes( m_OrientationAISDict );
				ReDisplayAllShapes( m_LeadInOrientationAISDict );
				ReDisplayAllShapes( m_LeadOutOrientationAISDict );
			}
		}

		public override void Show( bool bUpdate = false )
		{
			Show( m_DataManager.PathIDList, bUpdate );
		}

		public void Show( List<string> pathIDList, bool bUpdate = false )
		{
			// paused, do not rebuild or display
			if( m_IsPauseRefreshAndHide ) {
				return;
			}
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

		public void Trans( gp_Trsf trsf, bool bUpdate = false )
		{
			foreach( var oriAIS in m_OrientationAISDict ) {
				if( oriAIS.Value == null ) {
					continue;
				}
				oriAIS.Value.SetLocalTransformation( trsf );
			}
			foreach( var leadinOriAIS in m_LeadInOrientationAISDict ) {
				if( leadinOriAIS.Value == null ) {
					continue;
				}
				leadinOriAIS.Value.SetLocalTransformation( trsf );
			}
			foreach( var leadoutOriAis in m_LeadOutOrientationAISDict ) {
				if( leadoutOriAis.Value == null ) {
					continue;
				}
				leadoutOriAis.Value.SetLocalTransformation( trsf );
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		public void Reset( bool bUpdate = false )
		{
			gp_Trsf trsf = new gp_Trsf();
			Trans( trsf, bUpdate );
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

				// set color
				orientationAIS.SetColor( new OCC.Quantity.Quantity_Color( OCC.Quantity.Quantity_NameOfColor.Quantity_NOC_YELLOW ) );
				orientationAIS.SetWidth( 4.0 );
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

		void HideAllShapes( Dictionary<string, AIS_Shape> shapeDict )
		{
			foreach( var kvp in shapeDict ) {
				if( kvp.Value != null ) {
					m_Viewer.GetAISContext().Erase( kvp.Value, false );
				}
			}
		}

		void ReDisplayAllShapes( Dictionary<string, AIS_Shape> shapeDict )
		{
			foreach( var kvp in shapeDict ) {
				if( kvp.Value != null ) {
					m_Viewer.GetAISContext().Display( kvp.Value, false );
					m_Viewer.GetAISContext().Deactivate( kvp.Value );
				}
			}
		}

		IOrientationPoint GetFirstCAMPoint( string pathID )
		{
			return CacheHelper.GetMainPathStartPoint( pathID );
		}

		IOrientationPoint GetLeadInFirstPoint( string pathID )
		{
			return CacheHelper.GetLeadInStartPoint( pathID );
		}

		IOrientationPoint GetLeadOutLastPoint( string pathID )
		{
			return CacheHelper.GetLeadOutEndPoint( pathID );
		}

		LeadData GetLeadData( string pathID )
		{
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return null;
			}
			return craftData.LeadData;
		}

		bool GetIsPathReverse( string pathID )
		{
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return false;
			}
			return craftData.IsPathReverse;
		}
	}
}
