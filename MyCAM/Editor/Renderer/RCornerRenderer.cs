using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopoDS;
using OCCTool;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor.Renderer
{
	internal class RCornerRenderer : CAMRendererBase
	{
		readonly Dictionary<string, List<AIS_Shape>> m_PathAISDict = new Dictionary<string, List<AIS_Shape>>();

		static readonly Quantity_NameOfColor RCORNER_COLOR = Quantity_NameOfColor.Quantity_NOC_ORANGE;
		const double PATH_WIDTH = 3.0;

		public RCornerRenderer( Viewer viewer, DataManager dataManager )
			: base( viewer, dataManager )
		{
		}

		public override void Show( bool bUpdate = false )
		{
			List<string> detailIDs = m_DataManager.GetEditablePathIDList( m_DataManager.PathIDList.ToList() );
			Show( detailIDs, bUpdate );
		}

		public void Show( IReadOnlyList<string> pathIDList, bool bUpdate = false )
		{
			if( m_IsPauseRefreshAndHide || m_IsPauseRefresh ) {
				return;
			}

			Remove( pathIDList );
			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			foreach( string pathID in pathIDList ) {
				BuildAndDisplayPath( pathID );
			}

			if( bUpdate ) {
				UpdateView();
			}
		}

		public override void Remove( bool bUpdate = false )
		{
			List<string> detailIDs = m_DataManager.GetEditablePathIDList( m_DataManager.PathIDList.ToList() );
			Remove( detailIDs, bUpdate );
		}

		public void Remove( IReadOnlyList<string> pathIDList, bool bUpdate = false )
		{
			foreach( string pathID in pathIDList ) {
				if( m_PathAISDict.TryGetValue( pathID, out List<AIS_Shape> aisList ) ) {
					foreach( AIS_Shape ais in aisList ) {
						m_Viewer.GetAISContext().Remove( ais, false );
					}
					m_PathAISDict.Remove( pathID );
				}
			}
			if( bUpdate ) {
				UpdateView();
			}
		}

		void BuildAndDisplayPath( string pathID )
		{
			if( !DataGettingHelper.GetPathType( pathID, out PathType pathType ) || pathType != PathType.Contour ) {
				return;
			}
			if( !DataGettingHelper.GetContourCacheByID( pathID, out ContourCache pathCache ) ) {
				return;
			}
			if( !DataGettingHelper.GetCraftDataByID( pathID, out CraftData craftData ) ) {
				return;
			}
			if( craftData.RCornerMap.Count == 0 ) {
				return;
			}

			List<CAMPoint> camPointList = pathCache.MainPathPointList;
			if( camPointList == null || camPointList.Count < 2 ) {
				return;
			}

			List<AIS_Shape> aisList = new List<AIS_Shape>();
			foreach( RCornerData range in craftData.RCornerMap.Values ) {
				if( !pathCache.TryGetRCornerCAMRange( range, out int startCam, out int endCam ) ) {
					continue;
				}

				if( startCam < 0 || endCam >= camPointList.Count || endCam <= startCam ) {
					continue;
				}

				List<gp_Pnt> segmentPoints = new List<gp_Pnt>();
				for( int i = startCam; i <= endCam; i++ ) {
					segmentPoints.Add( camPointList[ i ].Point );
				}

				TopoDS_Wire segmentWire = CreateSegmentWire( segmentPoints );
				if( segmentWire == null || segmentWire.IsNull() ) {
					continue;
				}

				AIS_Shape segmentAIS = new AIS_Shape( segmentWire );
				segmentAIS.SetColor( new Quantity_Color( RCORNER_COLOR ) );
				segmentAIS.SetWidth( PATH_WIDTH );
				segmentAIS.SetZLayer( (int)Graphic3d_ZLayerId.Graphic3d_ZLayerId_Top );
				aisList.Add( segmentAIS );
			}

			foreach( AIS_Shape ais in aisList ) {
				m_Viewer.GetAISContext().Display( ais, false );
				m_Viewer.GetAISContext().Deactivate( ais );
			}
			if( aisList.Count > 0 ) {
				m_PathAISDict[ pathID ] = aisList;
			}
		}

		// ToolVecAndPathVisibleHelper.CreatePolylineWire only handles more than 2 points,
		// so handle the 2-point (adjacent ends) case here directly.
		TopoDS_Wire CreateSegmentWire( List<gp_Pnt> segmentPoints )
		{
			if( segmentPoints == null || segmentPoints.Count < 2 ) {
				return null;
			}
			if( segmentPoints.Count == 2 ) {
				BRepBuilderAPI_MakePolygon maker = new BRepBuilderAPI_MakePolygon( segmentPoints[ 0 ], segmentPoints[ 1 ] );
				if( !maker.IsDone() ) {
					return null;
				}
				return maker.Wire();
			}
			return ToolVecAndPathVisibleHelper.CreatePolylineWire( segmentPoints );
		}
	}
}
