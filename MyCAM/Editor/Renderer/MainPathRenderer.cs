using MyCAM.Data;
using MyCAM.Editor.Renderer;
using MyCAM.PathCache;
using OCC.AIS;
using OCC.BRepBuilderAPI;
using OCC.gp;
using OCC.Quantity;
using OCC.TopAbs;
using OCC.TopoDS;
using OCCViewer;
using System.Collections.Generic;
using System.Linq;

namespace MyCAM.Editor
{
	internal class MainPathRenderer : CAMRendererBase
	{
		readonly Dictionary<string, AIS_Shape> m_MainPathAISDict = new Dictionary<string, AIS_Shape>();
		ViewManager m_ViewManager;

		public MainPathRenderer( Viewer viewer, ViewManager viewManager, DataManager dataManager )
			: base( viewer, dataManager )
		{
			m_ViewManager = viewManager;
		}

		public override void Show( bool bUpdate = false )
		{
			Remove();

			if( !m_IsShow ) {
				if( bUpdate ) {
					UpdateView();
				}
				return;
			}

			foreach( string pathID in m_DataManager.PathIDList ) {
				IReadOnlyList<gp_Pnt> pointList = GetMainPathPointList( pathID );
				if( pointList == null || pointList.Count < 2 ) {
					continue;
				}

				TopoDS_Wire pathWire = CreatePolylineWire( pointList );
				if( pathWire == null || pathWire.IsNull() ) {
					continue;
				}

				AIS_Shape pathAIS = new AIS_Shape( pathWire );
				pathAIS.SetColor( new Quantity_Color( Quantity_NameOfColor.Quantity_NOC_BLUE ) );
				pathAIS.SetWidth( 3.0 );

				// Register to DataManager for shape-ID mapping
				m_DataManager.RegisterShapeIDMapping( pathWire, pathID );

				// Local storage
				m_MainPathAISDict.Add( pathID, pathAIS );

				if( m_ViewManager.ViewObjectMap.ContainsKey( pathID ) ) {
					m_ViewManager.ViewObjectMap.Remove( pathID );
				}
				m_ViewManager.ViewObjectMap.Add( pathID, new ViewObject( pathAIS ) );
				m_Viewer.GetAISContext().Display( pathAIS, false );
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
			RemovePaths( pathIDList );
			if( bUpdate ) {
				UpdateView();
			}
		}

		void RemovePaths( List<string> pathIDList )
		{
			// Unregister from DataManager
			foreach( string pathID in pathIDList ) {
				TopoDS_Wire wire = GetWireFromPathID( pathID );
				if( wire != null && !wire.IsNull() ) {
					m_DataManager.UnregisterShapeIDMapping( wire );
				}
			}

			foreach( string pathID in pathIDList ) {
				if( m_MainPathAISDict.TryGetValue( pathID, out AIS_Shape pathAIS ) ) {
					m_Viewer.GetAISContext().Remove( pathAIS, false );
					m_MainPathAISDict.Remove( pathID );
				}
			}
		}

		TopoDS_Wire CreatePolylineWire( IReadOnlyList<gp_Pnt> pointList )
		{
			if( pointList == null || pointList.Count < 2 ) {
				return null;
			}

			BRepBuilderAPI_MakePolygon polygonMaker = new BRepBuilderAPI_MakePolygon();

			foreach( gp_Pnt point in pointList ) {
				polygonMaker.Add( point );
			}

			if( pointList.Count > 2 ) {
				gp_Pnt firstPoint = pointList[ 0 ];
				gp_Pnt lastPoint = pointList[ pointList.Count - 1 ];
				if( firstPoint.Distance( lastPoint ) < 1e-6 ) {
					polygonMaker.Close();
				}
			}

			if( !polygonMaker.IsDone() ) {
				return null;
			}

			return polygonMaker.Wire();
		}

		IReadOnlyList<gp_Pnt> GetMainPathPointList( string szPathID )
		{
			// get path type
			if( !DataGettingHelper.GetPathType( szPathID, out PathType pathType ) ) {
				return new List<gp_Pnt>();
			}
			if( pathType == PathType.Contour ) {
				if( !DataGettingHelper.GetGeomDataByID( szPathID, out IGeomData contourGeomData ) ) {
					return new List<gp_Pnt>();
				}
				return ( contourGeomData as ContourGeomData )?.CADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else if( DataGettingHelper.IsStdPattern( pathType ) ) {
				if( !DataGettingHelper.GetStdPatternCacheByID( szPathID, out StdPatternCacheBase stdPatternCache ) ) {
					return new List<gp_Pnt>();
				}
				return stdPatternCache.MainPathCADPointList.Select( p => p.Point ).ToList() ?? new List<gp_Pnt>();
			}
			else {
				return new List<gp_Pnt>();
			}
		}

		TopoDS_Wire GetWireFromPathID( string pathID )
		{
			if( !m_MainPathAISDict.TryGetValue( pathID, out AIS_Shape pathAIS ) ) {
				return null;
			}

			TopoDS_Shape shape = pathAIS.Shape();
			if( shape == null || shape.IsNull() || shape.ShapeType() != TopAbs_ShapeEnum.TopAbs_WIRE ) {
				return null;
			}

			return TopoDS.ToWire( shape );
		}
	}
}