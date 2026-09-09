using MyCAM.Data;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.TopAbs;
using OCC.TopoDS;
using OCC.TopTools;
using OCCTool;
using OCCViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyCAM.Editor
{
	internal abstract class IndexSelectActionBase : KeyMouseActionBase
	{
		public Action<int, TopoDS_Shape> IndexChanged;

		public IndexSelectActionBase( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager )
		{
			if( string.IsNullOrEmpty( pathID ) ) {
				throw new ArgumentNullException( "IndexSelectAction constructing argument null" );
			}
			m_PathID = pathID;
			RebuildSelectionPoints();
		}

		// Rebuild selection points from the contourCache
		public void UpdateSelectionPoints()
		{
			m_Viewer.GetAISContext().ClearSelected( true );
			HideSelectPoint();

			RebuildSelectionPoints();
			ShowSelectPoint();
		}

		// Transform and rebuild selection points without reloading the contourCache
		public void TranfAndRebuildMap( gp_Trsf trsf, int oriHighLightIdx, out TopoDS_Shape oriHighLightVtx )
		{
			// rebuild transformed vertex map, and get the original vertex for selected index
			oriHighLightVtx = new TopoDS_Shape();
			if( m_PathPointList == null || m_PathPointList.Count == 0 )
				return;

			// translate original highlight index to offset index when OriginalIndexMap is available
			if( m_ToCADIndexMap != null ) {
				for( int i = 0; i < m_ToCADIndexMap.Count; i++ ) {
					if( m_ToCADIndexMap[ i ] == oriHighLightIdx ) {
						oriHighLightIdx = i;
						break;
					}
				}
			}
			List<gp_Pnt> newPntList = new List<gp_Pnt>( m_PathPointList.Count );
			foreach( gp_Pnt point in m_PathPointList ) {
				gp_Pnt copyPnt = new gp_Pnt( point.X(), point.Y(), point.Z() );
				copyPnt.Transform( trsf );
				newPntList.Add( copyPnt );
			}
			TopTools_DataMapOfShapeInteger tempVertexMap = new TopTools_DataMapOfShapeInteger();
			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();
			for( int i = 0; i < newPntList.Count; i++ ) {
				TopoDS_Vertex vtx = new BRepBuilderAPI_MakeVertex( newPntList[ i ] ).Vertex();
				polygonMaker.Add( vtx );
				if( i == oriHighLightIdx ) {
					oriHighLightVtx = vtx;
				}
				tempVertexMap.Bind( vtx, i );
			}
			if( !polygonMaker.IsDone() ) {
				return;
			}

			HideSelectPoint();
			TopoDS_Wire wire = polygonMaker.Wire();
			m_SelectedPointAIS = new AIS_Shape( wire );
			m_SelectedPointAIS.SetWidth( 1e-3 );
			ShowSelectPoint();
			m_VertexMap = tempVertexMap;
		}

		public override EditActionType ActionType
		{
			get { return EditActionType.SelectObject; }
		}

		public override void Start()
		{
			base.Start();

			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// show select point
			ShowSelectPoint();
		}

		public override void End()
		{
			// clear selection
			m_Viewer.GetAISContext().ClearSelected( true );

			// hide select point
			HideSelectPoint();
			base.End();
		}

		protected override void ViewerMouseClick( MouseEventArgs e )
		{
			if( e.Button != MouseButtons.Left ) {
				return;
			}
			int? index = GetSelectIndex( out TopoDS_Shape selectedShape );
			if( index == null ) {
				return;
			}
			IndexChanged?.Invoke( index.Value, selectedShape );
		}

		protected int? GetSelectIndex( out TopoDS_Shape selectedShape )
		{
			selectedShape = new TopoDS_Shape();
			m_Viewer.Select();
			m_Viewer.GetAISContext().InitSelected();
			if( !m_Viewer.GetAISContext().MoreSelected() ) {
				return null;
			}
			selectedShape = m_Viewer.GetAISContext().SelectedShape();
			if( selectedShape == null || selectedShape.IsNull() ) {
				return null;
			}
			if( selectedShape.ShapeType() != TopAbs_ShapeEnum.TopAbs_VERTEX ) {
				m_Viewer.GetAISContext().ClearSelected( false );
				return null;
			}
			TopoDS_Vertex selectedVertex = TopoDS.ToVertex( selectedShape );
			if( m_VertexMap.IsBound( selectedVertex ) == false ) {
				return null;
			}
			int offsetIndex = m_VertexMap.Find( selectedVertex );

			// translate offset index to ori CAD index when OriginalIndexMap is available
			if( m_ToCADIndexMap != null && offsetIndex >= 0 && offsetIndex < m_ToCADIndexMap.Count ) {
				int oriIndex = m_ToCADIndexMap[ offsetIndex ];
				if( SpecialPointIndex.IsGeneratedPointIndex( oriIndex ) ) {
					return null;
				}
				return oriIndex;
			}
			return offsetIndex;
		}

		protected void ShowSelectPoint()
		{
			if( m_SelectedPointAIS == null ) {
				return;
			}
			m_Viewer.GetAISContext().Display( m_SelectedPointAIS, true );
			m_Viewer.GetAISContext().Activate( m_SelectedPointAIS, (int)AISActiveMode.Vertex );
		}

		protected void HideSelectPoint()
		{
			if( m_SelectedPointAIS == null ) {
				return;
			}
			m_Viewer.GetAISContext().Remove( m_SelectedPointAIS, true );
		}

		void RebuildSelectionPoints()
		{
			m_PathPointList = GetPathPointList( m_PathID );
			m_ToCADIndexMap = GetIndexMap( m_PathID );
			m_VertexMap = new TopTools_DataMapOfShapeInteger();
			m_SelectedPointAIS = null;

			MakeSelectPoint();
		}

		void MakeSelectPoint()
		{
			// build wire from cad points
			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

			// add points to the polygon
			for( int i = 0; i < m_PathPointList.Count; i++ ) {
				BRepBuilderAPI_MakeVertex vertexMaker = new BRepBuilderAPI_MakeVertex( m_PathPointList[ i ] );
				polygonMaker.Add( vertexMaker.Vertex() );
				m_VertexMap.Bind( vertexMaker.Vertex(), i );
			}
			if( polygonMaker.IsDone() == false ) {
				return;
			}
			TopoDS_Wire wire = polygonMaker.Wire();
			m_SelectedPointAIS = new AIS_Shape( wire );

			// set invisible
			m_SelectedPointAIS.SetWidth( 1e-3 );
		}

		protected abstract List<gp_Pnt> GetPathPointList( string szPathID );

		protected abstract List<int> GetIndexMap( string szPathID );

		protected string m_PathID;
		protected List<gp_Pnt> m_PathPointList;
		List<int> m_ToCADIndexMap;

		// map point on view to index on CAMData
		protected TopTools_DataMapOfShapeInteger m_VertexMap;
		protected AIS_Shape m_SelectedPointAIS;
	}

	internal sealed class IndexSelectAction : IndexSelectActionBase
	{
		public IndexSelectAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager, pathID )
		{
		}

		protected override List<gp_Pnt> GetPathPointList( string szPathID )
		{
			// get path type
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}

			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
					return new List<gp_Pnt>();
				}
				return ( pathCache as ContourCache )?.TrsfCADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}

			if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( szPathID, out IStdPatternCache stdPatternCache ) ) {
					return new List<gp_Pnt>();
				}
				return stdPatternCache.KeyCADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}

			return new List<gp_Pnt>();
		}

		protected override List<int> GetIndexMap( string szPathID )
		{
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) || pathType != PathType.Contour ) {
				return null;
			}

			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}

			return ( pathCache as ContourCache )?.OriginalIndexMap ?? new List<int>();
		}
	}

	internal sealed class CAMIndexSelectAction : IndexSelectActionBase
	{
		public CAMIndexSelectAction( DataManager dataManager, Viewer viewer, TreeView treeView, ViewManager viewManager, string pathID )
			: base( dataManager, viewer, treeView, viewManager, pathID )
		{
		}

		protected override List<gp_Pnt> GetPathPointList( string szPathID )
		{
			// get path type
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}

			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
					return new List<gp_Pnt>();
				}

				return ( pathCache as ContourCache )?.MainPathPointList
					.Where( p => !SpecialPointIndex.IsGeneratedPointIndex( p.InitPathIndex ) )
					.Select( p => p.Point )
					.ToList() ?? new List<gp_Pnt>();
			}

			if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( szPathID, out IStdPatternCache stdPatternCache ) ) {
					return new List<gp_Pnt>();
				}
				return stdPatternCache.KeyCAMPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}

			return new List<gp_Pnt>();
		}

		protected override List<int> GetIndexMap( string szPathID )
		{
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) || pathType != PathType.Contour ) {
				return null;
			}

			if( !DataGettingHelper.GetPathCacheByID( szPathID, out IPathCache pathCache ) ) {
				return null;
			}

			return ( pathCache as ContourCache )?.MainPathPointList
					.Where( p => !SpecialPointIndex.IsGeneratedPointIndex( p.InitPathIndex ) )
					.Select( p => p.InitPathIndex )
					.ToList() ?? new List<int>();
		}
	}
}
